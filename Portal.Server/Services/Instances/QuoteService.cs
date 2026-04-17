using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using Nextended.Core.Extensions;
using Portal.Data;
using Portal.Data.Models;
using Portal.Server.Helpers;
using Portal.Server.Options;
using Portal.Server.Services.Interfaces;
using Portal.Server.Services.Interfaces.UtilityServices;
using Portal.Shared;
using Portal.Shared.DataEnums;
using Portal.Shared.DTO;
using Portal.Shared.DTO.Address;
using Portal.Shared.DTO.Contact;
using Portal.Shared.DTO.Job;
using Portal.Shared.DTO.Quote;
using Portal.Shared.DTO.Quote.PartailQuote;
using Portal.Shared.DTO.Types;
using Portal.Shared.Helpers;
using Portal.Shared.ResponseModels;
using System.Security.Cryptography;
using System.Text;

namespace Portal.Server.Services.Instances;

public class QuoteService(
    PrsDbContext _dbContext,
    ILogger<QuoteService> _logger,
    IEmailService _emailService,
    IPdfGenerationService _pdfGenerationService,
    IJobService _jobService,
    IOptions<QuotingOptions> _IQuotingOptions) : IQuoteService
{
    private readonly QuotingOptions _quotingOptions = _IQuotingOptions.Value;

    /// <summary>Matches contact <c>tsvector</c>; queries use <c>websearch_to_tsquery</c>.</summary>
    private const string FullTextSearchConfig = "english";

    /// <inheritdoc/>
    public async Task<Result<QuoteDetailsDto>> GetQuoteDetails(int quoteId)
    {
        Result<QuoteDetailsDto> res = new();
        try
        {
            QuoteDetailsDto? quoteDetails = await _dbContext.Quotes
                .AsSplitQuery()
                .Where(q => q.Id == quoteId)
                .Select(q => new QuoteDetailsDto(
                    q.Id,
                    q.QuoteReference ?? "",
                    q.Description,
                    q.TotalPrice ?? 0,
                    q.QuoteItems.Select(qi => new QuoteItemDto()
                    {
                        Id = qi.Id,
                        ServiceTypeId = qi.ServiceId ?? 0,
                        ServiceName = qi.ServiceNameSnapshot ?? "",
                        Price = qi.Total,
                        Description = qi.Notes
                    }).ToArray(),
                    q.JobTypeId,
                    new QuotesStatusTypeDto((QuoteStatusEnum)q.Status.Id, q.Status.Name, q.Status.Colour, q.Status.IsActive),
                    q.Address == null ? null : new AddressDto(q.Address.Id, (StateEnum)q.Address.StateId!, q.Address.StateId ?? 3, q.Address.Suburb, q.Address.Street, q.Address.PostCode),
                    q.Contact == null ? null : new ListContactDto(q.Contact.Id, q.Contact.FullName, q.Contact.Email, q.Contact.Phone, null, null, (ContactTypeEnum)q.Contact.TypeId),
                    q.CreatedOn,
                    q.CreatedByUser.DisplayName,
                    q.ModifiedOn,
                    q.ModifiedByUser != null ? q.ModifiedByUser.DisplayName : null,
                    q.TargetDeliveryDate,
                    q.DateSentToClient,
                    q.DateAccepted,
                    q.JobId,
                    q.Job != null ? q.Job.JobNumber : null,
                    q.ViewByClientAt,
                    q.QuoteStatusHistories.OrderByDescending(qsh => qsh.DateChanged)
                        .Select(qsh => new QuoteHistoryDto(qsh.QuoteId,
                            new QuotesStatusTypeDto(
                                (QuoteStatusEnum)qsh.StatusIdNew, qsh.StatusIdNewNavigation.Name, qsh.StatusIdNewNavigation.Colour, true),
                                qsh.DateChanged,
                                qsh.ModifiedByUser.DisplayName))
                        .ToArray()
                ))
                .FirstOrDefaultAsync();

            if (quoteDetails is null)
                return res.SetError(ErrorType.NotFound, "Quote not found.");

            return res.SetValue(quoteDetails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get the quote details for Quote ID {QuoteId}.", quoteId);
            return res.SetError(ErrorType.InternalError, "Failed to get the quote details.");
        }
    }

    /// <inheritdoc/>
    public async Task<Result<QuotePartialDetailsDto>> GetQuoteDetailsUnauthenticated(
        HttpContext httpContext,
        string quoteToken)
    {
        Result<QuotePartialDetailsDto> res = new();
        try
        {
            string hashedToken = ComputeHash(quoteToken);
            QuoteToken? quoteTokenEntity = await _dbContext.QuoteTokens
                .Include(qt => qt.Quote)
                .FirstOrDefaultAsync(qt => qt.Token == hashedToken);

            if (quoteTokenEntity is null)
            {
                _logger.LogError("An invalid quote token was used to attempt to access quote details. Token hash: {TokenHash}", hashedToken);
                return res.SetError(ErrorType.Unauthorized, "Invalid token.");
            }

            if (quoteTokenEntity.ExpiresAt < DateTime.UtcNow || quoteTokenEntity.UsedAt is not null)
            {
                _logger.LogError("A quote token has expired or has already been used. QuoteToken ID: {QuoteTokenId}, Quote ID: {QuoteId}", quoteTokenEntity.Id, quoteTokenEntity.QuoteId);
                return res.SetError(ErrorType.Unauthorized, "Token has expired.");
            }
            quoteTokenEntity.Quote.ViewByClientAt = DateTime.UtcNow;

            if (quoteTokenEntity.Quote.StatusId is (int)QuoteStatusEnum.Sent)
            {
                // Add the new history and set as client review.
                QuoteStatusHistory quoteStatusHistory = new()
                {
                    QuoteId = quoteTokenEntity.QuoteId,
                    StatusIdNew = (int)QuoteStatusEnum.ClientReview,
                    StatusIdOld = quoteTokenEntity.Quote.StatusId,
                    ModifiedByUserId = httpContext.UserId(),
                    DateChanged = DateTime.UtcNow,
                    Quote = quoteTokenEntity.Quote
                };
                _dbContext.QuoteStatusHistories.Add(quoteStatusHistory);
                quoteTokenEntity.Quote.StatusId = (int)QuoteStatusEnum.ClientReview;
            }

            await _dbContext.SaveChangesAsync();

            QuotePartialDetailsDto quote = await _dbContext
                .Quotes
                .Where(x => x.Id == quoteTokenEntity.QuoteId)
                .Select(q =>
                    new QuotePartialDetailsDto(
                        (QuoteStatusEnum)q.StatusId,
                         quoteTokenEntity.ExpiresAt,
                        q.QuoteReference,
                        q.TotalPrice ?? 0,
                        new ListContactDto(q.Contact!.Id, q.Contact.FullName, q.Contact.Email, q.Contact.Phone, null, null, (ContactTypeEnum)q.Contact.TypeId),
                        new AddressDto(q.Address!.Id, (StateEnum)q.Address.StateId!, q.Address.StateId ?? 3, q.Address.Suburb, q.Address.Street, q.Address.PostCode),
                        q.QuoteItems
                            .Select(x =>
                                new QuoteLineItemPartialDto(x.ServiceNameSnapshot, x.Total, x.Service!.Description ?? ""))
                            .ToArray()
                         ))
                .FirstAsync();

            res.SetValue(quote);

            return res;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get the quote details for Quote Token {QuoteToken}.", quoteToken);
            return res.SetError(ErrorType.InternalError, "Failed to get the quote details.");
        }
    }

    /// <inheritdoc/>
    public async Task<Result<int>> CreateNewQuote(QuoteCreationDto data, HttpContext httpContext)
    {
        Result<int> res = new();
        IDbContextTransaction transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            int[] serviceIds = [.. data.QuoteItems.Select(qi => qi.ServiceTypeId)];
            List<ServiceType> serviceTypes = await _dbContext.ServiceTypes.ToListAsync();

            if (!serviceIds.All(x => serviceTypes.Any(st => st.Id == x)))
                return res.SetError(ErrorType.BadRequest, "One or more service types provided are invalid.");

            QuoteStatus[] quoteStatusTypes = await _dbContext.QuoteStatuses.Where(qs => qs.IsActive).ToArrayAsync();
            if (!quoteStatusTypes.Any(qs => qs.Id == data.QuoteStatusId))
                return res.SetError(ErrorType.BadRequest, "Invalid quote status provided.");

            // Find the contact to ensure it exists and is active
            if (await _dbContext.Contacts.FirstOrDefaultAsync(c => c.Id == data.ContactId && c.DeletedAt == null) is null)
                return res.SetError(ErrorType.BadRequest, "Invalid contact provided.");

            Address address = await DBHelpers.CreateOrUpdateAddress(_dbContext, data.Address.ToAddress(httpContext.UserId()), httpContext.UserId());

            // Find count of quotes created in the current year to generate the quote reference number. This ensures the quote reference number is sequential for each year.
            int count = await _dbContext.Quotes.CountAsync(q => q.CreatedOn.Year == DateTime.UtcNow.Year);
            int nextQuoteNumber = count + 1;
            string quoteReference = "Q" + DateTime.UtcNow.ToString("yy") + nextQuoteNumber.ToString("D4");
            // Assert that this is unique, if not, append a random 4 digit number to the end. This is a safeguard and should not happen in normal circumstances.
            if (await _dbContext.Quotes.AnyAsync(q => q.QuoteReference == quoteReference))
                quoteReference += new Random().Next(0, 9999).ToString("D4");

            Quote newQuote = new()
            {
                QuoteReference = quoteReference,
                Description = data.Description?.Trim(),
                StatusId = data.QuoteStatusId,
                TotalPrice = data.QuoteItems.Sum(qi => qi.Price),
                ContactId = data.ContactId,
                JobTypeId = data.QuoteTypeId,
                JobId = null,
                DateSentToClient = null,
                TargetDeliveryDate = data.TargetDeliveryDate,
                CreatedByUserId = httpContext.UserId(),
                Address = address,
            };

            await _dbContext.Quotes.AddAsync(newQuote);
            await _dbContext.SaveChangesAsync();

            QuoteItem[] quoteItems = [.. data.QuoteItems.Select(qi => new QuoteItem
            {
                QuoteId = newQuote.Id,
                Notes = qi.Description?.Trim(),
                Total = qi.Price,
                ServiceId = qi.ServiceTypeId,
                ServiceNameSnapshot = serviceTypes.First(st => st.Id == qi.ServiceTypeId).ServiceName
            })];

            await _dbContext.QuoteItems.AddRangeAsync(quoteItems);
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return res.SetValue(newQuote.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Failed to create the quote.");
            return res.SetError(ErrorType.InternalError, "Failed to create the quote.");
        }
    }

    /// <inheritdoc/>
    public async Task<Result<int>> UpdateQuote(QuoteUpdateDto data, HttpContext httpContext)
    {
        Result<int> res = new();
        IDbContextTransaction transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            Quote? quote = await _dbContext.Quotes
                .AsSplitQuery()
                .Include(x => x.Address)
                .Include(x => x.QuoteItems)
                    .ThenInclude(x => x.Service)
                .FirstOrDefaultAsync(q => q.Id == data.QuoteId);

            if (quote is null)
                return res.SetError(ErrorType.NotFound, "Quote not found.");

            if ((QuoteStatusEnum)quote.StatusId is (QuoteStatusEnum.Accepted or QuoteStatusEnum.ClientReview or QuoteStatusEnum.Sent))
                return res.SetError(ErrorType.BadRequest, "Cannot update a quote with the status of Accepted, Client Review or Sent");

            // Can only update the quote if it is new, draft or rejected. Once a quote is sent to the client, it cannot be updated.
            List<QuoteStatusEnum> validQuoteStatuses = [QuoteStatusEnum.Rejected, QuoteStatusEnum.New, QuoteStatusEnum.Draft];

            if (!validQuoteStatuses.Contains((QuoteStatusEnum)quote.StatusId))
                return res.SetError(ErrorType.BadRequest, "Only quotes with status of New, Draft or Rejected can be updated.");

            // Find the contact to ensure it exists and is active
            if (await _dbContext.Contacts.FirstOrDefaultAsync(c => c.Id == data.ContactId && c.DeletedAt == null) is null)
                return res.SetError(ErrorType.BadRequest, "Invalid contact provided.");

            Address updatedAddress = await DBHelpers.CreateOrUpdateAddress(_dbContext, data.Address.ToAddress(httpContext.UserId()), httpContext.UserId());
            quote.Address = updatedAddress;
            quote.AddressId = updatedAddress.Id;
            quote.ContactId = data.ContactId;
            quote.JobTypeId = (int)data.JobType;
            quote.TargetDeliveryDate = data.TargetDeliveryDate?.ToUniversalTime();

            if (quote.StatusId != (int)data.QuoteStatusId)
            {
                quote.StatusId = (int)data.QuoteStatusId;
                QuoteStatusHistory quoteStatusHistory = new()
                {
                    QuoteId = quote.Id,
                    StatusIdNew = (int)data.QuoteStatusId,
                    StatusIdOld = quote.StatusId,
                    ModifiedByUserId = httpContext.UserId(),
                    DateChanged = DateTime.UtcNow,
                    Quote = quote
                };
                _dbContext.QuoteStatusHistories.Add(quoteStatusHistory);
            }

            quote.Description = StringNormalizer.TrimAndTruncate(quote.Description, 4000);

            int[] serviceIds = [.. data.QuoteItems.Select(qi => qi.ServiceTypeId)];
            List<ServiceType> serviceTypes = await _dbContext.ServiceTypes.ToListAsync();

            if (!serviceIds.All(x => serviceTypes.Any(st => st.Id == x)))
                return res.SetError(ErrorType.BadRequest, "One or more service types provided are invalid.");

            Dictionary<int, string> serviceNameById = serviceTypes.Where(x => serviceIds.Contains(x.Id)).ToDictionary(st => st.Id, st => st.ServiceName);

            // Update Existing Quote Items and add new ones
            List<QuoteItem> existingQuoteItems = [.. quote.QuoteItems.Where(qi => data.QuoteItems.Any(qid => qid.Id == qi.Id))];

            foreach (QuoteItem item in existingQuoteItems)
            {
                QuoteItemDto quoteItemDto = data.QuoteItems.First(qid => qid.Id == item.Id);
                item.ServiceNameSnapshot = quoteItemDto.ServiceName;
                item.ServiceId = quoteItemDto.ServiceTypeId;
                item.QuoteId = quote.Id;
                item.Notes = quoteItemDto.Description;
                item.Total = quoteItemDto.Price;
            }

            List<QuoteItem> newQuoteItems = [.. data.QuoteItems
                .Where(qi => !existingQuoteItems.Any(eq => eq.Id == qi.Id))
                .Select(qi => new QuoteItem
                {
                    QuoteId = quote.Id,
                    Notes = qi.Description,
                    Total = qi.Price,
                    ServiceId = qi.ServiceTypeId,
                    ServiceNameSnapshot = serviceNameById[qi.ServiceTypeId]
                })];
            await _dbContext.QuoteItems.AddRangeAsync(newQuoteItems);

            List<QuoteItem> removedQuoteItems = [.. quote.QuoteItems.Where(qi => !data.QuoteItems.Any(qid => qid.Id == qi.Id))];
            _dbContext.QuoteItems.RemoveRange(removedQuoteItems);

            quote.TotalPrice = data.QuoteItems.Sum(qi => qi.Price);
            quote.ModifiedOn = DateTime.UtcNow;
            quote.ModifiedByUserId = httpContext.UserId();

            if (await _dbContext.QuoteTokens.AnyAsync(q => q.QuoteId == quote.Id))
                await _dbContext.QuoteTokens.Where(q => q.QuoteId == quote.Id).ExecuteDeleteAsync();

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return res.SetValue(quote.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Failed to update the quote.");
            return res.SetError(ErrorType.InternalError, "Failed to update the quote.");
        }
    }

    /// <inheritdoc/>
    public async Task<Result<PagedResponse<QuoteListDto>>> GetAllQuotes(QuoteFilterDto filter)
    {
        Result<PagedResponse<QuoteListDto>> res = new();
        try
        {
            IQueryable<Quote> query = _dbContext.Quotes.AsQueryable();

            if (filter.Deleted)
                query = query.Where(q => q.DeletedAt != null);
            else
                query = query.Where(q => q.DeletedAt == null);

            if (!string.IsNullOrEmpty(filter.ContactSearch))
            {
                string contactSearch = filter.ContactSearch.Trim();
                query = query.Where(q => q.Contact != null && q.Contact.SearchVector.Matches(EF.Functions.WebSearchToTsQuery(FullTextSearchConfig, contactSearch)));
            }

            bool isDescending = filter.Order is SortDirectionEnum.Desc;
            query = filter.OrderBy switch
            {
                nameof(QuoteListDto.Id) => isDescending ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id),
                nameof(QuoteListDto.Contact.fullName) => isDescending ? query.OrderByDescending(x => x.Contact!.FullName) : query.OrderBy(x => x.Contact!.FullName),
                $"{nameof(QuoteListDto.Address)}.{nameof(QuoteListDto.Address.Suburb)}" => isDescending ? query.OrderByDescending(x => x.Address!.Suburb) : query.OrderBy(x => x.Address!.Suburb),
                $"{nameof(QuoteListDto.Address)}.{nameof(QuoteListDto.Address.Street)}" => isDescending ? query.OrderByDescending(x => x.Address!.Street) : query.OrderBy(x => x.Address!.Street),
                $"{nameof(QuoteListDto.Address)}.{nameof(QuoteListDto.Address.PostCode)}" => isDescending ? query.OrderByDescending(x => x.Address!.PostCode) : query.OrderBy(x => x.Address!.PostCode),
                _ => query.OrderByDescending(x => x.Id)
            };

            int total = await query.CountAsync();
            int skipValue = (filter.Page - 1) * filter.PageSize;

            IQueryable<QuoteListDto> pagedQuery = query
                .Skip(skipValue)
                .Take(filter.PageSize)
                .Select(q => new QuoteListDto(
                    q.Id,
                    q.QuoteReference ?? "",
                    q.TotalPrice,
                    new QuotesStatusTypeDto((QuoteStatusEnum)q.Status.Id, q.Status.Name, q.Status.Colour, q.Status.IsActive),
                    q.Contact == null ? null : new ContactDto(q.Contact.Id, q.Contact.FullName),
                    q.Address == null ? null : new AddressDto(q.Address.Id, (StateEnum)q.Address.StateId!, q.Address.StateId ?? 3, q.Address.Suburb, q.Address.Street, q.Address.PostCode),
                    q.JobId,
                    q.Job != null ? q.Job.JobNumber : null
                ));

            List<QuoteListDto> quotes = await pagedQuery.ToListAsync();
            return res.SetValue(new PagedResponse<QuoteListDto>(quotes, filter.PageSize, filter.Page, total));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all quotes.");
            return res.SetError(ErrorType.InternalError, "Failed to get the quotes.");
        }
    }

    /// <inheritdoc/>
    public async Task<Result<bool>> DeleteQuote(int quoteId, HttpContext httpContext)
    {
        Result<bool> res = new();
        try
        {
            if (await _dbContext.Quotes.FindAsync(quoteId) is not Quote qt)
                return res.SetError(ErrorType.BadRequest, "Quote not found.");

            if (qt.DeletedAt != null)
                return res.SetError(ErrorType.BadRequest, "Quote already deleted.");

            qt.DeletedAt = DateTime.UtcNow;
            qt.ModifiedByUserId = httpContext.UserId();
            qt.ModifiedByUser = null;
            await _dbContext.SaveChangesAsync();

            return res.SetValue(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete the quote template");
            return res.SetError(ErrorType.InternalError, "Failed to delete the quote");
        }
    }

    /// <inheritdoc/>
    public async Task<Result<QuoteTemplateDto[]>> GetQuotingTemplates()
    {
        Result<QuoteTemplateDto[]> res = new();
        try
        {
            res.Value = await _dbContext.QuoteTemplates
                .Where(qt => qt.IsActive && qt.DeletedAt == null)
                .Select(qt => new QuoteTemplateDto
                (
                    qt.Id,
                    qt.Name,
                    qt.Description,
                    qt.IsActive,
                    qt.CreatedOn,
                    qt.ModifiedOn,
                    qt.ModifiedByUserId != null ? qt.ModifiedByUser!.DisplayName : null,
                    (JobTypeEnum)qt.JobTypeId!,
                    qt.QuoteTemplateItems.Select(qti => new QuoteTemplateItemDto
                    (
                        qti.Id,
                        qti.ServiceId ?? 0,
                        qti.ServiceNameSnapshot ?? "",
                        qti.Description,
                        qti.DefaultPrice
                    )).ToArray()
                ))
                .ToArrayAsync();

            return res;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get the quoting template");
            return res.SetError(ErrorType.InternalError, "Failed to get the quoting template.");
        }
    }

    /// <inheritdoc/>
    public async Task<Result<QuoteTemplateDto>> CreateQuotingTemplate(QuoteTemplateDto data, HttpContext httpContext)
    {
        Result<QuoteTemplateDto> res = new();
        try
        {
            QuoteTemplate qt = new()
            {
                CreatedByUserId = httpContext.UserId(),
                Description = StringNormalizer.TrimAndTruncate(data.Description, 4000),
                Name = StringNormalizer.TrimAndTruncateNotNull(data.Name, 150),
                DeletedAt = null,
                CreatedOn = DateTime.UtcNow,
                IsActive = true,
                JobTypeId = (int)data.JobType,
            };

            if (data.Description is not null && data.Description.Length > 4000)
                qt.Description = data.Description[..4000].Trim();

            await _dbContext.QuoteTemplates.AddAsync(qt);
            await _dbContext.SaveChangesAsync();

            int[] distinctServiceTypes = [.. data.QuoteTemplateItems.Select(qi => qi.ServiceTypeId).Distinct()];

            List<ServiceType> serviceTypesForPayload = await _dbContext.ServiceTypes
                .Where(st => distinctServiceTypes.Contains(st.Id))
                .ToListAsync();

            if (serviceTypesForPayload.Count != distinctServiceTypes.Length)
                return res.SetError(ErrorType.BadRequest, "One or more service types provided are invalid.");

            QuoteTemplateItem[] quoteTemplateItems = [.. data.QuoteTemplateItems.Select(qti => new QuoteTemplateItem
            {
                QuoteTemplateId = qt.Id,
                Description = qti.Description?.Trim(),
                DefaultPrice = qti.DefaultPrice,
                ServiceId = qti.ServiceTypeId,
                ServiceNameSnapshot = serviceTypesForPayload.First(st => st.Id == qti.ServiceTypeId).ServiceName,
            })];
            await _dbContext.QuoteTemplateItems.AddRangeAsync(quoteTemplateItems);
            await _dbContext.SaveChangesAsync();

            QuoteTemplateDto createdQuoteTemplate = new(
                qt.Id,
                qt.Name,
                qt.Description,
                qt.IsActive,
                qt.CreatedOn,
                qt.ModifiedOn,
                qt.ModifiedByUserId != null ? qt.ModifiedByUser!.DisplayName : null,
                (JobTypeEnum)qt.JobTypeId!,
                [.. quoteTemplateItems.Select(qti => new QuoteTemplateItemDto
                (
                    qti.Id,
                    qti.ServiceId ?? 0,
                    qti.ServiceNameSnapshot ?? "",
                    qti.Description,
                    qti.DefaultPrice
                ))]
            );

            return res.SetValue(createdQuoteTemplate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create the quote template");
            return res.SetError(ErrorType.InternalError, "Failed to create the quoting template");
        }
    }

    /// <inheritdoc/>
    public async Task<Result<QuoteTemplateDto>> UpdateQuotingTemplate(QuoteTemplateDto data, HttpContext httpContext)
    {
        Result<QuoteTemplateDto> res = new();
        try
        {
            QuoteTemplate? qt = await _dbContext.QuoteTemplates
                .Include(qt => qt.QuoteTemplateItems)
                .FirstOrDefaultAsync(qt => qt.Id == data.Id);

            if (qt is null)
                return res.SetError(ErrorType.BadRequest, "Quote template not found.");

            int[] distinctServiceTypes = [.. data.QuoteTemplateItems.Select(qi => qi.ServiceTypeId).Distinct()];

            List<ServiceType> serviceTypesForPayload = await _dbContext.ServiceTypes
                .Where(st => distinctServiceTypes.Contains(st.Id))
                .ToListAsync();

            if (serviceTypesForPayload.Count != distinctServiceTypes.Length)
                return res.SetError(ErrorType.BadRequest, "One or more service types provided are invalid.");

            Dictionary<int, string> serviceNameById = serviceTypesForPayload.ToDictionary(st => st.Id, st => st.ServiceName);

            qt.Name = data.Name;
            qt.JobTypeId = (int)data.JobType;
            qt.ModifiedByUserId = httpContext.UserId();
            qt.IsActive = data.IsActive;
            qt.ModifiedOn = DateTime.UtcNow;
            qt.Description = StringNormalizer.TrimAndTruncate(qt.Description, 4000);

            // Update Existing Quote Items and add new ones
            QuoteTemplateItem[] existingQuoteItems = [.. qt.QuoteTemplateItems.Where(qi => data.QuoteTemplateItems.Any(qid => qid.Id == qi.Id))];

            foreach (QuoteTemplateItem item in existingQuoteItems)
            {
                QuoteTemplateItemDto dto = data.QuoteTemplateItems.First(qid => qid.Id == item.Id);
                item.ServiceNameSnapshot = dto.ServiceName;
                item.ServiceId = dto.ServiceTypeId;
                item.Description = dto.Description?.Trim();
                item.DefaultPrice = dto.DefaultPrice;
            }

            // Add new items
            QuoteTemplateItem[] newQuoteItems = [.. data.QuoteTemplateItems
                .Where(qi => !existingQuoteItems.Any(eq => eq.Id == qi.Id))
                .Select(qi => new QuoteTemplateItem
                {
                    QuoteTemplateId = qt.Id,
                    Description = qi.Description?.Trim(),
                    ServiceId = qi.ServiceTypeId,
                    ServiceNameSnapshot = serviceNameById[qi.ServiceTypeId],
                    DefaultPrice = qi.DefaultPrice
                })];
            await _dbContext.QuoteTemplateItems.AddRangeAsync(newQuoteItems);

            // Remove deleted items
            QuoteTemplateItem[] removedQuoteItems = [.. qt.QuoteTemplateItems.Where(qi => !data.QuoteTemplateItems.Any(qid => qid.Id == qi.Id))];
            _dbContext.QuoteTemplateItems.RemoveRange(removedQuoteItems);

            await _dbContext.SaveChangesAsync();

            string? modifiedByDisplay = qt.ModifiedByUserId is int modifierId
                ? await _dbContext.AppUsers.AsNoTracking()
                    .Where(u => u.Id == modifierId)
                    .Select(u => u.DisplayName)
                    .FirstOrDefaultAsync()
                : null;

            QuoteTemplateDto createdQuoteTemplate = new(
              qt.Id,
              qt.Name,
              qt.Description,
              qt.IsActive,
              qt.CreatedOn,
              qt.ModifiedOn,
              modifiedByDisplay,
              (JobTypeEnum)qt.JobTypeId!,
              await _dbContext.QuoteTemplateItems
                .Where(qti => qti.QuoteTemplateId == qt.Id)
                .Select(qti => new QuoteTemplateItemDto
                (
                    qti.Id,
                    qti.ServiceId ?? 0,
                    qti.ServiceNameSnapshot ?? "",
                    qti.Description,
                    qti.DefaultPrice
                )).ToArrayAsync()
          );

            return res.SetValue(createdQuoteTemplate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create the quote template");
            return res.SetError(ErrorType.InternalError, "Failed to create the quoting template");
        }
    }

    /// <inheritdoc/>
    public async Task<Result<bool>> DeleteQuotingTemplate(int quoteTemplateId, HttpContext httpContext)
    {
        Result<bool> res = new();
        try
        {
            if (await _dbContext.QuoteTemplates.FindAsync(quoteTemplateId) is not QuoteTemplate qt)
                return res.SetError(ErrorType.BadRequest, "Quote template not found.");

            if (qt.DeletedAt != null)
                return res.SetError(ErrorType.BadRequest, "Quote template already deleted.");

            qt.DeletedAt = DateTime.UtcNow;
            qt.ModifiedByUserId = httpContext.UserId();
            qt.ModifiedByUser = null;
            await _dbContext.SaveChangesAsync();

            return res.SetValue(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete the quote template");
            return res.SetError(ErrorType.InternalError, "Failed to delete the quoting template");
        }
    }

    /// <inheritdoc/>
    public async Task<Result<int>> SendQuoteToClient(int quoteId, HttpContext httpContext)
    {
        Result<int> res = new();
        IDbContextTransaction transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            // First, update the quote details
            Quote? quote = await _dbContext.Quotes.Include(x => x.Contact).FirstOrDefaultAsync(q => q.Id == quoteId);
            if (quote is null)
            {
                await transaction.RollbackAsync();
                return res.SetError(ErrorType.BadRequest, "Quote not found.");
            }

            if (quote.Contact is null || !Regexes.IsValidEmail(quote.Contact.Email))
            {
                await transaction.RollbackAsync();
                return res.SetError(ErrorType.BadRequest, $"The supplied contact email is invalid. Or the contact is not set. Email: {quote.Contact?.Email}");
            }

            QuoteStatusHistory quoteStatusHistory = new()
            {
                QuoteId = quote.Id,
                StatusIdNew = (int)QuoteStatusEnum.Sent,
                StatusIdOld = quote.StatusId,
                ModifiedByUserId = httpContext.UserId(),
                DateChanged = DateTime.UtcNow,
                Quote = quote
            };
            _dbContext.QuoteStatusHistories.Add(quoteStatusHistory);

            quote.StatusId = (int)QuoteStatusEnum.Sent;
            quote.DateSentToClient = DateTime.UtcNow;
            quote.ModifiedByUserId = httpContext.UserId();
            quote.QuoteSentByUserId = httpContext.UserId();

            await _dbContext.SaveChangesAsync();

            // Then, generate the quote PDF and send the email to the client
            Result<QuoteDetailsDto> quoteDetailsResult = await GetQuoteDetails(quoteId);
            if (!quoteDetailsResult.IsSuccess || quoteDetailsResult.Value is null)
            {
                await transaction.RollbackAsync();
                return res.SetError(ErrorType.InternalError, "Failed to get the quote details for PDF generation.");
            }

            byte[] quoteAsPdf = await _pdfGenerationService.CreateQuotePdf(quoteDetailsResult.Value);
            (byte[], string) attachment = (quoteAsPdf, $"Quote_{quote.QuoteReference}.pdf");
            (string, string) token = ComputeTokenAndHashToken();

            DateTime now = DateTime.UtcNow;

            if (await _dbContext.QuoteTokens.AnyAsync(q => q.QuoteId == quoteId))
                await _dbContext.QuoteTokens.Where(q => q.QuoteId == quoteId).ExecuteDeleteAsync();

            // Create a new token
            QuoteToken quoteToken = new()
            {
                QuoteId = quote.Id,
                Token = token.Item2,
                CreatedOn = now,
                ExpiresAt = now.AddDays(_quotingOptions.DayToAcceptFeeProposal),
                UsedAt = null
            };

            await _dbContext.QuoteTokens.AddAsync(quoteToken);
            await _dbContext.SaveChangesAsync();

            bool emailResult = await _emailService.SendQuoteEmail(
                [quote.Contact!.Email],
                $"Fee proposal — {quote.QuoteReference}",
                quoteDetailsResult.Value,
                [attachment],
                token.Item1);

            if (!emailResult)
            {
                await transaction.RollbackAsync();
                return res.SetError(ErrorType.InternalError, "Failed to send the email for the quote. Please try again.");
            }

            await transaction.CommitAsync();
            return res.SetValue(quoteId);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Failed to send the quote to the client. Quote ID: {QuoteId}", quoteId);
            return res.SetError(ErrorType.InternalError, "Failed to send the quote to the client");
        }
    }

    public async Task<Result<QuotePartialDetailsDto>> SubmitQuoteResponse(
        string quoteToken,
        ClientQuoteSubmissionDto data,
        HttpContext httpContext)
    {
        Result<QuotePartialDetailsDto> res = new();
        await using IDbContextTransaction transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            if (quoteToken.IsNullOrEmpty())
                return res.SetError(ErrorType.BadRequest, "Invalid token provided.");

            if (data.Status is not (QuoteStatusEnum.Accepted or QuoteStatusEnum.Rejected))
                return res.SetError(ErrorType.BadRequest, "Invalid Response type provided");

            string hashedToken = ComputeHash(quoteToken);
            QuoteToken? quoteTokenEntity = await _dbContext.QuoteTokens
                .Include(qt => qt.Quote)
                .FirstOrDefaultAsync(qt => qt.Token == hashedToken);

            if (quoteTokenEntity is null)
            {
                _logger.LogError("An invalid quote token was used to attempt to access quote details. Token hash: {TokenHash}", hashedToken);
                return res.SetError(ErrorType.Unauthorized, "Invalid token.");
            }

            if ((QuoteStatusEnum)quoteTokenEntity.Quote.StatusId is (QuoteStatusEnum.Accepted or QuoteStatusEnum.Rejected))
                return res.SetError(ErrorType.BadRequest, "Quote has already been submitted. Please wait for a response from Peter Richards Surveying.");

            if (quoteTokenEntity.ExpiresAt < DateTime.UtcNow || quoteTokenEntity.UsedAt is not null)
            {
                _logger.LogError("A quote token has expired or has already been used. QuoteToken ID: {QuoteTokenId}, Quote ID: {QuoteId}", quoteTokenEntity.Id, quoteTokenEntity.QuoteId);
                return res.SetError(ErrorType.Unauthorized, "Token has expired.");
            }

            int previousStatusId = quoteTokenEntity.Quote.StatusId;

            if (data.Status is QuoteStatusEnum.Rejected)
            {
                if (string.IsNullOrEmpty(data.ReasonForRejection))
                    return res.SetError(ErrorType.BadRequest, "When rejecting a fee proposal please provide a reason.");

                quoteTokenEntity.Quote.QuoteRejectionReason = StringNormalizer.TrimAndTruncate(data.ReasonForRejection);
                quoteTokenEntity.Quote.StatusId = (int)QuoteStatusEnum.Rejected;
            }
            if (data.Status is QuoteStatusEnum.Accepted)
            {
                if (string.IsNullOrEmpty(data.SignedName))
                    return res.SetError(ErrorType.BadRequest, "To accept the quote you must sign.");

                if (!data.AddressIsCorrect || !data.ContactDetailsAreCorrect)
                    return res.SetError(ErrorType.BadRequest, "To accept the quote the address and contact details must be correct.");

                QuoteAcceptance quoteAcceptance = new()
                {
                    QuoteId = quoteTokenEntity.QuoteId,
                    Quote = quoteTokenEntity.Quote,
                    QuoteTokenId = quoteTokenEntity.Id,
                    AcceptedAt = DateTime.UtcNow,
                    SignatoryName = data.SignedName,
                    ClientIp = httpContext.Connection.RemoteIpAddress?.ToString(),
                    QuoteTotalSnapshot = quoteTokenEntity.Quote.TotalPrice ?? 0,
                    QuoteReferenceSnapshot = quoteTokenEntity.Quote.QuoteReference ?? string.Empty,
                    SignatureContentType = "image/png",
                };

                await _dbContext.QuoteAcceptances.AddAsync(quoteAcceptance);
                quoteTokenEntity.Quote.StatusId = (int)QuoteStatusEnum.Accepted;
            }

            QuoteStatusHistory quoteStatusHistory = new()
            {
                QuoteId = quoteTokenEntity.QuoteId,
                StatusIdNew = (int)data.Status,
                StatusIdOld = previousStatusId,
                ModifiedByUserId = httpContext.UserId(),
                DateChanged = DateTime.UtcNow,
            };
            _dbContext.QuoteStatusHistories.Add(quoteStatusHistory);
            await _dbContext.SaveChangesAsync();

            List<JobStatus> jobStatuses = await _dbContext.JobStatuses.Where(x => x.JobTypeId == quoteTokenEntity.Quote.JobTypeId).ToListAsync();
            int jobStatusStart = jobStatuses.MinBy(x => x.Sequence)!.Id;

            JobCreationDto job = new()
            {
                Address = new() { AddressId = quoteTokenEntity.Quote.AddressId ?? 0 },
                ContactId = quoteTokenEntity.Quote.ContactId ?? 0,
                JobType = [(JobTypeEnum)quoteTokenEntity.Quote.JobTypeId],
                Details = quoteTokenEntity.Quote.Description,
                StatusId = jobStatusStart,
                TargetDeliveryDate = quoteTokenEntity.Quote.TargetDeliveryDate,
                LatestClientUpdate = DateTime.UtcNow
            };

            Result<int> jobResult = await _jobService.CreateJob(httpContext, job, manageTransaction: false);
            if (!jobResult.IsSuccess)
                return res.SetError(ErrorType.InternalError, "An error occured while creating the job for the quote.");

            quoteTokenEntity.Quote.DateAccepted = DateTime.UtcNow;
            quoteTokenEntity.Quote.JobId = jobResult.Value;

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to submit quote response");
            return res.SetError(ErrorType.InternalError, "Failed to send the quote to the client");
        }

        return await GetQuoteDetailsUnauthenticated(httpContext, quoteToken);
    }

    /// <inheritdoc />
    public async Task<Result<QuotePdfDto>> GetQuotePdf(int quoteId)
    {
        Result<QuotePdfDto> res = new();
        try
        {
            Quote? quote = await _dbContext.Quotes.FirstOrDefaultAsync(q => q.Id == quoteId);
            if (quote is null)
                return res.SetError(ErrorType.BadRequest, "Quote not found.");

            Result<QuoteDetailsDto> quoteDetailsResult = await GetQuoteDetails(quoteId);
            if (!quoteDetailsResult.IsSuccess || quoteDetailsResult.Value is null)
                return res.SetError(ErrorType.InternalError, "Failed to get the quote details for PDF generation.");

            byte[] quoteAsPdf = await _pdfGenerationService.CreateQuotePdf(quoteDetailsResult.Value);
            QuotePdfDto quotePdfDto = new() { FileName = $"Quote_{quote.QuoteReference}.pdf", Data = quoteAsPdf };

            return res.SetValue(quotePdfDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed ato generate the quote PDF. QUote ID:{quoteId}", quoteId);
            return res.SetError(ErrorType.InternalError, "Failed to delete the quoting template");
        }
    }

    /// <summary>
    /// Generates a cryptographically secure random token (Base64url) and the SHA-256 hash of that exact string (standard Base64).
    /// </summary>
    /// <remarks>
    /// Base64url avoids <c>+</c>, <c>/</c>, and padding <c>=</c> from standard Base64, so the raw token is safe in URL paths and query
    /// strings without extra encoding. Call sites should still use <see cref="UriBuilder"/> or <c>QueryHelpers.AddQueryString</c> when
    /// composing URLs. 32 bytes (256 bits) of entropy before encoding.
    /// </remarks>
    private static (string RawToken, string TokenHash) ComputeTokenAndHashToken()
    {
        Span<byte> data = stackalloc byte[32];
        RandomNumberGenerator.Fill(data);
        string rawToken = WebEncoders.Base64UrlEncode(data);
        byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        string tokenHash = Convert.ToBase64String(hashBytes);
        return (rawToken, tokenHash);
    }

    private static string ComputeHash(string token) =>
        Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
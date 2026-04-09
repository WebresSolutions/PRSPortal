using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Portal.Data;
using Portal.Data.Models;
using Portal.Server.Helpers;
using Portal.Server.Services.Interfaces;
using Portal.Shared;
using Portal.Shared.DataEnums;
using Portal.Shared.DTO;
using Portal.Shared.DTO.Address;
using Portal.Shared.DTO.Contact;
using Portal.Shared.DTO.Quote;
using Portal.Shared.DTO.Types;
using Portal.Shared.ResponseModels;

namespace Portal.Server.Services.Instances;

public class QuoteService(PrsDbContext _dbContext, ILogger<QuoteService> _logger) : IQuoteService
{
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
                    q.JobId,
                    q.Job != null ? q.Job.JobNumber : null
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
    public async Task<Result<int>> CreateNewQuote(QuoteCreationDto data, HttpContext httpContext)
    {
        Result<int> res = new();
        IDbContextTransaction transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            int[] serviceIds = [.. data.QuoteItems.Select(qi => qi.ServiceTypeId)];
            List<ServiceType> serviceTypes = await _dbContext.ServiceTypes.Where(st => serviceIds.Contains(st.Id)).ToListAsync();

            if (serviceTypes.Count != serviceIds.Length)
                return res.SetError(ErrorType.BadRequest, "One or more service types provided are invalid.");

            QuoteStatus[] quoteStatusTypes = await _dbContext.QuoteStatuses.Where(qs => qs.IsActive).ToArrayAsync();
            if (!quoteStatusTypes.Any(qs => qs.Id == data.QuoteStatusId))
                return res.SetError(ErrorType.BadRequest, "Invalid quote status provided.");

            // Find the contact to ensure it exists and is active
            if (await _dbContext.Contacts.FirstOrDefaultAsync(c => c.Id == data.ContactId && c.DeletedAt == null) is null)
                return res.SetError(ErrorType.BadRequest, "Invalid contact provided.");

            Address address = await DBHelpers.CreateOrUpdateAddress(_dbContext, data.Address.ToAddress(httpContext.UserId()), httpContext.UserId());

            Quote newQuote = new()
            {
                QuoteReference = "Q" + DateTime.UtcNow.Ticks.ToString("D6"),
                Description = data.Description,
                StatusId = data.QuoteStatusId,
                TotalPrice = data.QuoteItems.Sum(qi => qi.Price),
                ContactId = data.ContactId,
                JobTypeId = data.QuoteTypeId,
                JobId = null,
                DateSentToClient = null,
                TargetDeliveryDate = data.TargetDeliveryDate,
                CreatedByUserId = httpContext.UserId(),
                Address = address
            };

            await _dbContext.Quotes.AddAsync(newQuote);
            await _dbContext.SaveChangesAsync();

            QuoteItem[] quoteItems = [.. data.QuoteItems.Select(qi => new QuoteItem
            {
                QuoteId = newQuote.Id,
                Notes = qi.Description,
                Total = qi.Price,
                ServiceId = qi.ServiceTypeId,
                ServiceNameSnapshot = serviceTypes.First(st => st.Id == qi.ServiceTypeId).ServiceName,

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
            quote.StatusId = (int)data.QuoteStatusId;
            quote.JobTypeId = (int)data.JobType;

            if (data.Description is not null && data.Description.Length > 4000)
                quote.Description = data.Description[..4000].Trim();
            else
                quote.Description = data.Description;

            int[] distinctServiceTypes = [.. data.QuoteItems.Select(qi => qi.ServiceTypeId).Distinct()];

            List<ServiceType> serviceTypesForPayload = await _dbContext.ServiceTypes
                .Where(st => distinctServiceTypes.Contains(st.Id))
                .ToListAsync();

            if (serviceTypesForPayload.Count != distinctServiceTypes.Length)
                return res.SetError(ErrorType.BadRequest, "One or more service types provided are invalid.");

            Dictionary<int, string> serviceNameById = serviceTypesForPayload.ToDictionary(st => st.Id, st => st.ServiceName);

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

            if (filter.ShowDeleted)
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
                Description = data.Description,
                Name = data.Name,
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
                Description = qti.Description,
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

            if (data.Description is not null && data.Description.Length > 4000)
                qt.Description = data.Description[..4000].Trim();
            else
                qt.Description = data.Description;

            // Update Existing Quote Items and add new ones
            QuoteTemplateItem[] existingQuoteItems = [.. qt.QuoteTemplateItems.Where(qi => data.QuoteTemplateItems.Any(qid => qid.Id == qi.Id))];

            foreach (QuoteTemplateItem item in existingQuoteItems)
            {
                QuoteTemplateItemDto dto = data.QuoteTemplateItems.First(qid => qid.Id == item.Id);
                item.ServiceNameSnapshot = dto.ServiceName;
                item.ServiceId = dto.ServiceTypeId;
                item.Description = dto.Description;
                item.DefaultPrice = dto.DefaultPrice;
            }

            // Add new items
            QuoteTemplateItem[] newQuoteItems = [.. data.QuoteTemplateItems
                .Where(qi => !existingQuoteItems.Any(eq => eq.Id == qi.Id))
                .Select(qi => new QuoteTemplateItem
                {
                    QuoteTemplateId = qt.Id,
                    Description = qi.Description,
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
}
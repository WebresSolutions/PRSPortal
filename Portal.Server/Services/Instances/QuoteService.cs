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
                        Total = qi.Total,
                        Notes = qi.Notes
                    }).ToArray(),
                    q.JobTypeId,
                    new QuotesStatusTypeDto((QuoteStatusEnum)q.Status.Id, q.Status.Name, "", q.Status.IsActive),
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
                TotalPrice = data.QuoteItems.Sum(qi => qi.Total),
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
                Notes = qi.Notes,
                Total = qi.Total,
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

            // Validate all service types exist (load once for new line snapshots)
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
                item.Notes = quoteItemDto.Notes;
                item.Total = quoteItemDto.Total;
            }

            List<QuoteItem> newQuoteItems = [.. data.QuoteItems
                .Where(qi => !existingQuoteItems.Any(eq => eq.Id == qi.Id))
                .Select(qi => new QuoteItem
                {
                    QuoteId = quote.Id,
                    Notes = qi.Notes,
                    Total = qi.Total,
                    ServiceId = qi.ServiceTypeId,
                    ServiceNameSnapshot = serviceNameById[qi.ServiceTypeId]
                })];
            await _dbContext.QuoteItems.AddRangeAsync(newQuoteItems);

            List<QuoteItem> removedQuoteItems = [.. quote.QuoteItems.Where(qi => !data.QuoteItems.Any(qid => qid.Id == qi.Id))];
            _dbContext.QuoteItems.RemoveRange(removedQuoteItems);

            quote.TotalPrice = data.QuoteItems.Sum(qi => qi.Total);
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

            if (filter.Deleted)
                query = query.Where(q => q.DeletedAt != null);

            if (!string.IsNullOrEmpty(filter.ContactSearch))
            {
                string pattern = PartialMatch(filter.ContactSearch);
                query = query.Where(q => q.Contact != null && q.Contact.SearchVector.Matches(EF.Functions.ToTsQuery(pattern)));
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
                    new QuotesStatusTypeDto((QuoteStatusEnum)q.Status.Id, q.Status.Name, "", q.Status.IsActive),
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


        static string PartialMatch(string filter) => string.Join(" & ", filter.Split(' ', StringSplitOptions.RemoveEmptyEntries)) + ":*";
    }

    /// <inheritdoc/>
    public async Task<Result<bool>> DeleteQuote(int quoteId, HttpContext httpContext)
    {
        try
        {
            return new Result<bool>();
        }
        catch (Exception)
        {

            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<Result<QuoteTemplateDto[]>> GetQuotingTemplates() => throw new NotImplementedException();

    /// <inheritdoc/>
    public async Task<Result<QuoteTemplateDto>> CreateQuotingTemplate(QuoteTemplateDto quoteTemplateDto) => throw new NotImplementedException();

    /// <inheritdoc/>
    public async Task<Result<QuoteTemplateDto>> UpdateQuotingTemplate(QuoteTemplateDto quoteTemplateDto) => throw new NotImplementedException();

    /// <inheritdoc/>
    public async Task<Result<bool>> DeleteQuotingTemplate(int quoteTemplateId) => throw new NotImplementedException();


}
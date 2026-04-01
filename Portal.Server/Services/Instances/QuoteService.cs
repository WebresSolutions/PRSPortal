using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Portal.Data;
using Portal.Data.Models;
using Portal.Server.Services.Interfaces;
using Portal.Shared;
using Portal.Shared.DTO.Address;
using Portal.Shared.DTO.Contact;
using Portal.Shared.DTO.Quote;
using Portal.Shared.DTO.Types;
using Portal.Shared.ResponseModels;

namespace Portal.Server.Services.Instances;

public class QuoteService(PrsDbContext _dbContext, ILogger<QuoteService> _logger) : IQuoteService
{
    /// <inheritdoc/>
    public async Task<Result<int>> CreateNewQuote(QuoteCreationDto data)
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

            Quote newQuote = new()
            {
                QuoteReference = "Q" + DateTime.UtcNow.Ticks.ToString("D6"),
                Description = data.Description,
                StatusId = data.QuoteStatusId,
                TotalPrice = data.QuoteItems.Sum(qi => qi.Total),
                ContactId = data.ContactId,
                JobId = null,
                DateSentToClient = null,
                TargetDeliveryDate = data.TargetDeliveryDate,
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
            _logger.LogError(ex, "Failed to create schedule slot.");
            return res.SetError(ErrorType.InternalError, "Failed to create the schedule slot.");
        }
    }

    /// <inheritdoc/>
    public async Task<Result<PagedResponse<QuoteListDto>>> GetAllQuotes(QuoteFilterDto filter)
    {
        Result<PagedResponse<QuoteListDto>> res = new();
        try
        {
            IQueryable<Data.Models.Quote> query = _dbContext.Quotes.AsQueryable();
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
                    new QuotesStatusTypeDto(q.Status.Id, q.Status.Name, "", q.Status.IsActive),
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
}
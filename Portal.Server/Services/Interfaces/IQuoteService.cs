using Portal.Shared.DTO.Quote;
using Portal.Shared.ResponseModels;

namespace Portal.Server.Services.Interfaces;

public interface IQuoteService
{
    /// <summary>
    /// Retrieves a paginated list of quotes based on the provided filter criteria. The filter allows for searching by job number, contact, and address, as well as sorting and pagination options.
    /// </summary>
    /// <param name="filter">The filter criteria for retrieving quotes.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the paginated list of quotes.</returns>
    Task<Result<PagedResponse<QuoteListDto>>> GetAllQuotes(QuoteFilterDto filter);
    /// <summary>
    /// Creates a new quote based on the provided quote creation data. This method will validate the input data and, if valid, create a new quote record in the system.
    /// </summary>
    /// <param name="quoteDto">The data for creating the new quote.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the ID of the newly created quote.</returns>
    Task<Result<int>> CreateNewQuote(QuoteCreationDto quoteDto);
}
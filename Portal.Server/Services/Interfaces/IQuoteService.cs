using Portal.Shared.DTO.Quote;
using Portal.Shared.ResponseModels;

namespace Portal.Server.Services.Interfaces;

/// <summary>
/// Interface for quote-related business logic operations.
/// </summary>
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
    Task<Result<int>> CreateNewQuote(QuoteCreationDto quoteDto, HttpContext httpContext);

    /// <summary>
    /// Gets the details of a specific quote by its unique identifier.
    /// </summary>
    /// <param name="quoteId">The unique identifier of the quote to retrieve details for.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the details of the quote.</returns>
    Task<Result<QuoteDetailsDto>> GetQuoteDetails(int quoteId);

    /// <summary>
    /// Updates a quote.
    /// </summary>
    /// <param name="data">The data for updating the quote.</param>
    /// <param name="httpContext">The HTTP context for the current user.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the ID of the updated quote.</returns>
    Task<Result<int>> UpdateQuote(QuoteUpdateDto data, HttpContext httpContext);

    /// <summary>
    /// Deletes a quote.
    /// </summary>
    /// <param name="quoteId">The unique identifier of the quote to delete.</param>
    /// <param name="httpContext">The HTTP context for the current user.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the ID of the deleted quote.</returns>
    Task<Result<bool>> DeleteQuote(int quoteId, HttpContext httpContext);

    /// <summary>
    /// Gets the quoting templates.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the quoting templates.</returns>
    Task<Result<QuoteTemplateDto[]>> GetQuotingTemplates();

    /// <summary>
    /// Creates a new quoting template.
    /// </summary>
    /// <param name="quoteTemplateDto">The data for creating the new quoting template.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the ID of the newly created quoting template.</returns>
    Task<Result<QuoteTemplateDto>> CreateQuotingTemplate(QuoteTemplateDto quoteTemplateDto);

    /// <summary>
    /// Updates a quoting template.
    /// </summary>
    /// <param name="quoteTemplateDto">The data for updating the quoting template.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the ID of the updated quoting template.</returns>
    Task<Result<QuoteTemplateDto>> UpdateQuotingTemplate(QuoteTemplateDto quoteTemplateDto);

    /// <summary>
    /// Deletes a quoting template.
    /// </summary>
    /// <param name="quopteTemplateId">The unique identifier of the quoting template to delete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the ID of the deleted quoting template.</returns>
    Task<Result<bool>> DeleteQuotingTemplate(int quopteTemplateId);
}
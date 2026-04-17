using Portal.Shared.DTO.Quote;
using Portal.Shared.DTO.Quote.PartailQuote;
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
    /// Gets the details of a specific quote by its unique token without requiring authentication. 
    /// This method is intended for scenarios where clients need to access quote details without logging in, such as when viewing a quote through a shared link.
    /// </summary>
    /// <param name="httpContext">The Http Context</param>
    /// <param name="quoteToken">The token for viewing a quote</param>
    /// <returns>The partial restricted quote details</returns>
    Task<Result<QuotePartialDetailsDto>> GetQuoteDetailsUnauthenticated(HttpContext httpContext, string quoteToken);

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
    /// <param name="httpContext">The HTTP context for the current user.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the ID of the newly created quoting template.</returns>
    Task<Result<QuoteTemplateDto>> CreateQuotingTemplate(QuoteTemplateDto quoteTemplateDto, HttpContext httpContext);

    /// <summary>
    /// Updates a quoting template.
    /// </summary>
    /// <param name="quoteTemplateDto">The data for updating the quoting template.</param>
    /// <param name="httpContext">The HTTP context for the current user.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the ID of the updated quoting template.</returns>
    Task<Result<QuoteTemplateDto>> UpdateQuotingTemplate(QuoteTemplateDto quoteTemplateDto, HttpContext httpContext);

    /// <summary>
    /// Deletes a quoting template.
    /// </summary>
    /// <param name="quopteTemplateId">The unique identifier of the quoting template to delete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the ID of the deleted quoting template.</returns>
    Task<Result<bool>> DeleteQuotingTemplate(int quopteTemplateId, HttpContext httpContext);

    /// <summary>
    /// Emails the quote to the client. This method will generate the quote document, attach it to an email, and send it to the client's email address. 
    /// It will also log the email sending activity for auditing purposes.
    /// </summary>
    /// <param name="quoteId">The quote ID </param>
    /// <param name="httpContext">The http context of the caller</param>
    /// <returns>The Id of the quote.</returns>
    Task<Result<int>> SendQuoteToClient(int quoteId, HttpContext httpContext);

    /// <summary>
    /// Generates a PDF document for the specified quote. This method will retrieve the quote details, 
    /// format them into a PDF document, and return the PDF data along with the file name. 
    /// The PDF can then be downloaded or emailed to clients as needed.
    /// </summary>
    /// <param name="quoteId">The unique identifier of the quote.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the PDF data and file name.</returns>
    Task<Result<QuotePdfDto>> GetQuotePdf(int quoteId);
    Task<Result<QuotePartialDetailsDto>> SubmitQuoteResponse(string quoteToken, ClientQuoteSubmissionDto data, HttpContext httpContext);
}
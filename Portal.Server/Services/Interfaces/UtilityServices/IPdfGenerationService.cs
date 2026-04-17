using Portal.Shared.DTO.Quote;

namespace Portal.Server.Services.Interfaces.UtilityServices;

/// <summary>
/// Generates PDF documents using <see href="https://github.com/jhaygood86/PeachPDF">PeachPDF</see> (HTML to PDF, pure .NET).
/// </summary>
public interface IPdfGenerationService
{
    /// <summary>
    /// Creates a PDF document for the given quote details. The PDF is generated in-memory and returned as a byte array.
    /// </summary>
    /// <param name="quoteDetailsDto">The details of the quote to include in the PDF.</param>
    /// <returns>A byte array representing the generated PDF document.</returns>
    Task<byte[]> CreateQuotePdf(QuoteDetailsDto quoteDetailsDto);
}

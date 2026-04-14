using Portal.Shared.DTO.Quote;

namespace Portal.Server.Services.Interfaces.UtilityServices;

/// <summary>
/// Generates PDF documents (PDFsharp, MIT). Suitable for Linux/Docker when fonts are installed on the image.
/// </summary>
public interface IPdfGenerationService
{
    /// <summary>
    /// Creates a PDF document for the given quote details. The PDF is generated in-memory and returned as a byte array.
    /// </summary>
    /// <param name="quoteDetailsDto">The details of the quote to include in the PDF.</param>
    /// <returns>A byte array representing the generated PDF document.</returns>
    byte[] CreateQuotePdf(QuoteDetailsDto quoteDetailsDto);
}

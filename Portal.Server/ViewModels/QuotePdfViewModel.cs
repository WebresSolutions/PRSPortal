using Portal.Shared.DTO.Quote;

namespace Portal.Server.ViewModels;

/// <summary>
/// Model for <c>Views/QuotePdf.cshtml</c> (quote fee proposal HTML for PDF generation).
/// </summary>
public sealed class QuotePdfViewModel
{
    public required QuoteDetailsDto Quote { get; init; }
    /// <summary>Embedded image source (typically a <c>data:image/…;base64,…</c> URI for PeachPDF).</summary>
    public string? LogoUri { get; init; }
    /// <summary>Embedded image source for the survey-area section.</summary>
    public string? SurveyImageUri { get; init; }
}

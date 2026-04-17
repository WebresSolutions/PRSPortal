using Portal.Shared.DTO.Quote;

namespace Portal.Server.ViewModels;

/// <summary>
/// Model for <c>Views/QuotePdf.cshtml</c> (quote fee proposal HTML for PDF generation).
/// </summary>
public sealed class QuotePdfViewModel
{
    public required QuoteDetailsDto Quote { get; init; }
    public string? LogoUri { get; init; }
    public string? SurveyImageUri { get; init; }
}

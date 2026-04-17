using System.Globalization;
using System.Net;
using System.Text;
using PeachPDF;
using PeachPDF.PdfSharpCore;
using PeachPDF.PdfSharpCore.Pdf;
using Portal.Server.Services.Interfaces.UtilityServices;
using Portal.Shared.DTO.Quote;

namespace Portal.Server.Services.Instances.UtilityServices;

/// <summary>
/// Quote PDFs via <see href="https://github.com/jhaygood86/PeachPDF">PeachPDF</see> (HTML → PDF, pure .NET).
/// </summary>
public sealed class PdfGenerationService(IWebHostEnvironment env) : IPdfGenerationService
{
    private static readonly string[] LogoRasterCandidates =
    [
        "PRS-Primary-Logo.png",
        "prs-primary-logo.png",
        "logo.png",
    ];

    private static readonly CultureInfo Au = CultureInfo.GetCultureInfo("en-AU");

    /// <inheritdoc />
    public async Task<byte[]> CreateQuotePdf(QuoteDetailsDto quote)
    {
        PdfGenerator generator = new();
        generator.AddFontFamilyMapping("Segoe UI", "sans-serif");

        string html = BuildQuoteHtml(quote);
        PdfGenerateConfig pdfConfig = new()
        {
            PageSize = PageSize.A4,
            PageOrientation = PageOrientation.Portrait,
        };
        pdfConfig.SetMargins(48);

        using PdfDocument document = await generator.GeneratePdf(html, pdfConfig, cssData: null);
        await using MemoryStream outStream = new();
        document.Save(outStream);
        return outStream.ToArray();
    }

    private string BuildQuoteHtml(QuoteDetailsDto quote)
    {
        string? logoPath = TryResolveLogoPath();
        string? logoUri = logoPath is not null ? ToFileUri(logoPath) : null;
        string? surveyPath = ResolveAssetPath(Path.Combine("images", "PRS-Primary-Logo.png"));
        string? surveyUri = surveyPath is not null ? ToFileUri(surveyPath) : null;

        StringBuilder sb = new();
        sb.AppendLine("<!DOCTYPE html><html><head><meta charset=\"utf-8\"/>");
        sb.Append("<title>").Append(E($"Quote {quote.QuoteReference}")).AppendLine("</title>");
        sb.AppendLine("""
            <style>
              * { box-sizing: border-box; }
              body { font-family: 'Segoe UI', 'Liberation Sans', Arial, sans-serif; font-size: 10pt; line-height: 1.35; color: #000; margin: 0; }
              table { border-collapse: collapse; }
              .muted { color: #555; font-size: 8.5pt; font-style: italic; }
              .small { font-size: 8.5pt; }
              .company { font-weight: bold; font-size: 11pt; }
              .line { font-size: 9pt; }
              .hline { border: 0; border-top: 1px solid #d3d3d3; margin: 12pt 0; }
              .logo { max-width: 120pt; max-height: 52pt; }
              .section { margin-bottom: 10pt; }
              .page-break { break-before: page; page-break-before: always; }
              h2 { font-size: 10pt; margin: 0 0 8pt 0; }
              .mini-head .line { font-size: 8.5pt; }
              .mini-head .company { font-size: 9pt; }
              .mini-head .muted { font-size: 7.5pt; }
              .fee-total { font-weight: bold; }
              .scope-item { margin-bottom: 12pt; }
              .scope-item h3 { font-size: 10pt; margin: 0 0 6pt 0; }
              .survey-img { max-width: 100%; height: auto; margin-top: 8pt; }
            </style>
            </head><body>
            """);

        AppendCoverPage(sb, quote, logoUri);
        AppendScopeSection(sb, quote);
        AppendExclusionsSection(sb);
        AppendSurveySection(sb, surveyUri);

        sb.AppendLine("</body></html>");
        return sb.ToString();
    }

    private static void AppendCoverPage(StringBuilder sb, QuoteDetailsDto quote, string? logoUri)
    {
        sb.AppendLine("<section class=\"section\">");
        sb.AppendLine("<table style=\"width:100%; margin-bottom:12pt;\"><tr>");
        sb.Append("<td style=\"vertical-align:top;width:1%;padding-right:16pt;\">");
        if (logoUri is not null)
            sb.Append("<img class=\"logo\" src=\"").Append(E(logoUri)).Append("\" alt=\"\"/>");
        sb.AppendLine("</td><td style=\"vertical-align:top;\">");
        sb.AppendLine("<div class=\"company\">PETER RICHARDS SURVEYING</div>");
        sb.AppendLine("<div class=\"line\">T (03) 9432 6944</div>");
        sb.AppendLine("<div class=\"line\">E mail@prs.au</div>");
        sb.AppendLine("<div class=\"line\">W PRS.AU</div>");
        sb.AppendLine("<div class=\"line\">ABN 11 070 127 552</div>");
        sb.AppendLine("<div class=\"line\">45/7 Dalton Road</div>");
        sb.AppendLine("<div class=\"line\">Thomastown VIC 3074</div>");
        sb.AppendLine("<div class=\"line\">Suite 1 Level1, 20-30</div>");
        sb.AppendLine("<div class=\"line\">Mollison Street</div>");
        sb.AppendLine("<div class=\"line\">Abbotsford VIC 3067</div>");
        sb.AppendLine("<p class=\"muted\">Liability limited by a scheme approved under Professional Standards Legislation</p>");
        sb.AppendLine("</td></tr></table>");
        sb.AppendLine("<hr class=\"hline\"/>");

        string issued = quote.CreatedAt.ToString("d MMMM yyyy", Au);
        sb.Append("<p class=\"line\">").Append(E($"{issued}     Quote Ref: {quote.QuoteReference}")).AppendLine("</p>");

        if (quote.Contact is not null)
        {
            sb.Append("<p class=\"line\">").Append("Attention: ").Append(E(quote.Contact.FullName)).AppendLine("</p>");
            if (!string.IsNullOrWhiteSpace(quote.Contact.Phone))
                sb.Append("<p class=\"line\">").Append("Phone: ").Append(E(quote.Contact.Phone)).AppendLine("</p>");
            sb.Append("<p class=\"line\">").Append("Email: ").Append(E(quote.Contact.Email)).AppendLine("</p>");
        }

        string first = FirstName(quote.Contact?.FullName);
        sb.Append("<p class=\"line\">Dear ").Append(E(first)).AppendLine(",</p>");

        string subject = quote.Address is not null ? quote.Address.ToDisplayString() : "YOUR PROPERTY";
        sb.Append("<p><strong>").Append("RE: ").Append(E(subject)).AppendLine("</strong></p>");

        sb.Append("<p>").Append(E(
            "Thank you for the opportunity to provide our fee proposal for the proposed survey services at the above property. Our proposed scope of works and fees are set out below.")).AppendLine("</p>");
        sb.Append("<p>").Append(E(
            "Please note: This fee proposal remains valid for 30 days from the date of issue. If acceptance is received after this time, our fees may be reviewed and revised prior to commencement.")).AppendLine("</p>");

        sb.AppendLine("<p><strong>Project Scope:</strong></p>");
        string scopeText = string.IsNullOrWhiteSpace(quote.Description)
            ? "Survey services as summarised in the fee summary and detailed scope of works following this letter."
            : quote.Description!;
        sb.Append("<p>").Append(E(scopeText)).AppendLine("</p>");

        sb.AppendLine("<p><strong>Fee Summary</strong></p>");
        sb.AppendLine("<table style=\"width:100%;\">");
        foreach (QuoteItemDto item in quote.QuoteItems)
        {
            sb.Append("<tr><td style=\"padding:3pt 0;width:62%;\">").Append(E(item.ServiceName))
                .Append("</td><td style=\"padding:3pt 0;text-align:right;width:38%;\">")
                .Append(E(FormatFeePlusGst(item.Price))).AppendLine("</td></tr>");
        }

        sb.Append("<tr class=\"fee-total\"><td style=\"padding:6pt 0 3pt 0;\">Total Fees (excluding disbursements)</td><td style=\"padding:6pt 0 3pt 0;text-align:right;\">")
            .Append(E(FormatFeePlusGst(quote.TotalPrice))).AppendLine("</td></tr>");
        sb.AppendLine("</table>");

        sb.Append("<p>").Append(E(
            "Disbursements: General disbursement costs for searching relevant survey information will be in addition to the cost above (these typically range in the vicinity of $50-$150+GST per property).")).AppendLine("</p>");
        sb.Append("<p>").Append(E(
            "Scheduling of Survey: A site booking date will be scheduled upon receiving the attached Acceptance Form. Currently, our lead time is approximately one to two weeks to undertake site works. Survey plans are generally available within 7-8* business days following completion of site works.")).AppendLine("</p>");
        sb.Append("<p class=\"small\">").Append(E(
            "Timeframes above are estimates only and may vary depending on site conditions, access, project complexity, or matters arising during the survey, drafting and review process. Where additional time is required to ensure an accurate and professionally prepared outcome, we will make every reasonable effort to communicate any delay as soon as practicable.")).AppendLine("</p>");

        sb.AppendLine("<p><strong>Acceptance:</strong></p>");
        sb.Append("<p>").Append(E(
            "To accept this fee proposal, please fill out the Acceptance Form located within.")).AppendLine("</p>");

        sb.AppendLine("<p>Yours Sincerely,</p>");
        sb.AppendLine("<p><strong>Brodie Richards</strong><br/>Managing Director<br/>Licensed Surveyor</p>");
        sb.AppendLine("<p class=\"muted\">Please continue for detailed scope of works, exclusions and additional information</p>");
        sb.AppendLine("</section>");
    }

    private static void AppendScopeSection(StringBuilder sb, QuoteDetailsDto quote)
    {
        sb.AppendLine("<section class=\"section page-break mini-head\">");
        AppendMiniLetterhead(sb);
        sb.AppendLine("<h2>Scope of Works:</h2>");
        foreach (QuoteItemDto item in quote.QuoteItems)
        {
            string detail = string.IsNullOrWhiteSpace(item.Description) ? "As agreed for this service." : item.Description!;
            sb.AppendLine("<div class=\"scope-item\">");
            sb.Append("<h3>").Append(E(item.ServiceName)).AppendLine("</h3>");
            sb.Append("<p>").Append(E(detail)).AppendLine("</p>");
            sb.AppendLine("</div>");
        }

        sb.AppendLine("</section>");
    }

    private static void AppendExclusionsSection(StringBuilder sb)
    {
        sb.AppendLine("<section class=\"section page-break mini-head\">");
        AppendMiniLetterhead(sb);
        sb.AppendLine("<h2>Exclusions:</h2>");
        const string exclusionsBody =
            "Unless specifically noted otherwise, our fee does not include:\n" +
            "• Title boundaries unless a Title Re-establishment Survey is completed;\n" +
            "• Additional boundary mark placement or reinstatement beyond the quoted scope;\n" +
            "• Physical locating of underground services by specialist methods;\n" +
            "• Return visits due to restricted or unavailable site access;\n" +
            "• Invert levels of drainage pits, pipes or underground drainage infrastructure unless specifically requested;\n" +
            "• Point cloud data, BIM/3D modelling, or other advanced digital deliverables;";
        foreach (string line in exclusionsBody.Split('\n'))
            sb.Append("<p>").Append(E(line)).AppendLine("</p>");

        sb.AppendLine("<h2>Additional Information</h2>");
        sb.Append("<p>").Append(E(
            "Payment Terms: Payment is due within 14 days of receiving the tax invoice and prior to the release of any plans.")).AppendLine("</p>");
        sb.AppendLine("</section>");
    }

    private void AppendSurveySection(StringBuilder sb, string? surveyImageUri)
    {
        sb.AppendLine("<section class=\"section page-break mini-head\">");
        AppendMiniLetterhead(sb);
        sb.AppendLine("<h2>Proposed Survey Area</h2>");
        if (surveyImageUri is not null)
            sb.Append("<p><img class=\"survey-img\" src=\"").Append(E(surveyImageUri)).AppendLine("\" alt=\"\"/></p>");
        sb.AppendLine("</section>");
    }

    private static void AppendMiniLetterhead(StringBuilder sb)
    {
        sb.AppendLine("<div class=\"company\">PETER RICHARDS SURVEYING</div>");
        sb.AppendLine("<div class=\"line\">T (03) 9432 6944</div>");
        sb.AppendLine("<div class=\"line\">E mail@prs.au</div>");
        sb.AppendLine("<div class=\"line\">W PRS.AU</div>");
        sb.AppendLine("<div class=\"line\">ABN 11 070 127 552</div>");
        sb.AppendLine("<div class=\"line\">45/7 Dalton Road</div>");
        sb.AppendLine("<div class=\"line\">Thomastown VIC 3074</div>");
        sb.AppendLine("<div class=\"line\">Suite 1 Level1, 20-30</div>");
        sb.AppendLine("<div class=\"line\">Mollison Street</div>");
        sb.AppendLine("<div class=\"line\">Abbotsford VIC 3067</div>");
        sb.AppendLine("<p class=\"muted\">Liability limited by a scheme approved under Professional Standards Legislation</p>");
    }

    private string? TryResolveLogoPath()
    {
        foreach (string name in LogoRasterCandidates)
        {
            string? path = ResolveAssetPath(Path.Combine("images", name));
            if (path is not null)
                return path;
        }

        return null;
    }

    private string? ResolveAssetPath(string relativeUnderAssetsFolder)
    {
        string rooted = Path.Combine(env.ContentRootPath, "Assets", relativeUnderAssetsFolder);
        if (File.Exists(rooted))
            return rooted;

        string fallback = Path.Combine(AppContext.BaseDirectory, "Assets", relativeUnderAssetsFolder);
        return File.Exists(fallback) ? fallback : null;
    }

    private static string ToFileUri(string path) =>
        new Uri(Path.GetFullPath(path)).AbsoluteUri;

    private static string FirstName(string? fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return "Sir/Madam";

        string[] parts = fullName.Trim().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 0 ? parts[0] : "Sir/Madam";
    }

    private static string FormatFeePlusGst(decimal amount) =>
        "$" + amount.ToString("#,##0", CultureInfo.InvariantCulture) + "+GST";

    private static string E(string? s) => WebUtility.HtmlEncode(s ?? string.Empty);
}

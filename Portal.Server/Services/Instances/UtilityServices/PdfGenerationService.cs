using PeachPDF;
using PeachPDF.PdfSharpCore;
using PeachPDF.PdfSharpCore.Pdf;
using Portal.Server.Services.Interfaces.UtilityServices;
using Portal.Server.ViewModels;
using Portal.Shared.DTO.Quote;

namespace Portal.Server.Services.Instances.UtilityServices;

/// <summary>
/// Quote PDFs: Razor view → HTML (<see cref="IRazorViewRenderer"/>) → PDF via
/// <see href="https://github.com/jhaygood86/PeachPDF">PeachPDF</see>.
/// </summary>
public sealed class PdfGenerationService(IWebHostEnvironment env, IRazorViewRenderer razorViewRenderer) : IPdfGenerationService
{
    private const string QuotePdfViewName = "QuotePdf";

    private static readonly string[] LogoRasterCandidates =
    [
        "PRS-Primary-Logo.png",
        "prs-primary-logo.png",
        "logo.png",
    ];

    /// <inheritdoc />
    public async Task<byte[]> CreateQuotePdf(QuoteDetailsDto quote)
    {
        string? logoPath = TryResolveLogoPath();
        string? logoUri = logoPath is not null ? ToImageDataUri(logoPath) : null;
        string? surveyPath = ResolveAssetPath(Path.Combine("images", "PRS-Primary-Logo.png"));
        string? surveyUri = surveyPath is not null ? ToImageDataUri(surveyPath) : null;

        QuotePdfViewModel model = new()
        {
            Quote = quote,
            LogoUri = logoUri,
            SurveyImageUri = surveyUri,
        };

        string html = await razorViewRenderer.Render(QuotePdfViewName, model);

        PdfGenerator generator = new();
        generator.AddFontFamilyMapping("Segoe UI", "sans-serif");

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

    /// <summary>
    /// PeachPDF resolves images from <c>data:</c> URIs reliably; <c>file://</c> often does not load in the HTML pipeline.
    /// </summary>
    private static string? ToImageDataUri(string path)
    {
        if (!File.Exists(path))
            return null;

        byte[] bytes = File.ReadAllBytes(path);
        string mime = Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            _ => "application/octet-stream",
        };

        return $"data:{mime};base64,{Convert.ToBase64String(bytes)}";
    }
}

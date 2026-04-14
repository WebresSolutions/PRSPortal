using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using Portal.Server.Services.Interfaces.UtilityServices;
using Portal.Shared.DTO.Quote;
using System.Globalization;

namespace Portal.Server.Services.Instances.UtilityServices;

public sealed class PdfGenerationService(IWebHostEnvironment env) : IPdfGenerationService
{
    private static readonly object FontInitLock = new();
    private static bool _fontResolverInitialized;

    private static readonly string[] LogoRasterCandidates =
    [
        "PRS-Primary-Logo.png",
        "prs-primary-logo.png",
        "logo.png",
    ];

    private static readonly CultureInfo Au = CultureInfo.GetCultureInfo("en-AU");

    /// <inheritdoc />
    public byte[] CreateQuotePdf(QuoteDetailsDto quote)
    {
        EnsureFontResolver();

        const double margin = 48;

        using MemoryStream workStream = new();
        using (PdfDocument document = new())
        {
            document.Info.Title = $"Quote {quote.QuoteReference}";
            document.Info.Creator = "PRS Portal";

            DrawAllPages(document, quote, margin);
            document.Save(workStream, false);
        }

        workStream.Position = 0;
        using PdfDocument stamped = PdfReader.Open(workStream, PdfDocumentOpenMode.Modify);
        for (int i = 0; i < stamped.PageCount; i++)
        {
            PdfPage page = stamped.Pages[i];
            using XGraphics gfx = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append);
            DrawPageFooter(gfx, page, i + 1, stamped.PageCount, margin);
        }

        using MemoryStream outStream = new();
        stamped.Save(outStream, false);
        return outStream.ToArray();
    }

    private void DrawAllPages(PdfDocument document, QuoteDetailsDto quote, double margin)
    {
        PdfPage page = document.AddPage();
        page.Size = PageSize.A4;
        double contentWidth = page.Width.Point - 2 * margin;
        using (XGraphics gfx = XGraphics.FromPdfPage(page))
        {
            double y = margin;
            y = DrawCompanyLetterhead(gfx, margin, contentWidth, y);
            DrawCoverLetterBody(gfx, quote, margin, contentWidth, y, page.Height.Point - margin);
        }

        DrawScopeOfWorksPages(document, quote, margin, contentWidth);
        DrawExclusionsAndInfoPage(document, margin, contentWidth);
        DrawSurveyAreaPage(document, margin, contentWidth);
    }

    private static void DrawPageFooter(XGraphics gfx, PdfPage page, int pageNumber, int totalPages, double margin)
    {
        XFont f = CreateFont(9, XFontStyleEx.Regular);
        string text = $"-- {pageNumber} of {totalPages} --";
        XSize sz = gfx.MeasureString(text, f);
        double w = page.Width.Point;
        double h = page.Height.Point;
        double x = (w - sz.Width) / 2;
        double y = h - margin + 8;
        gfx.DrawString(text, f, XBrushes.Gray, new XPoint(x, y));
    }

    private double DrawCompanyLetterhead(XGraphics gfx, double margin, double contentWidth, double y)
    {
        XFont companyFont = CreateFont(11, XFontStyleEx.Bold);
        XFont lineFont = CreateFont(9, XFontStyleEx.Regular);
        XFont smallFont = CreateFont(8, XFontStyleEx.Italic);

        const double logoMaxWidth = 120;
        const double logoMaxHeight = 52;
        double textStartX = margin;
        double headerTop = y;

        XImage? logo = TryOpenLogoRaster();
        try
        {
            if (logo is not null)
            {
                double scaleW = logoMaxWidth / (logo.PixelWidth * 72.0 / logo.HorizontalResolution);
                double scaleH = logoMaxHeight / (logo.PixelHeight * 72.0 / logo.VerticalResolution);
                double scale = Math.Min(1, Math.Min(scaleW, scaleH));
                double drawW = logo.PixelWidth * scale * 72.0 / logo.HorizontalResolution;
                double drawH = logo.PixelHeight * scale * 72.0 / logo.VerticalResolution;
                gfx.DrawImage(logo, margin, y, drawW, drawH);
                textStartX = margin + drawW + 16;
            }

            double ty = headerTop;
            gfx.DrawString("PETER RICHARDS SURVEYING", companyFont, XBrushes.Black, new XPoint(textStartX, ty));
            ty += 14;
            gfx.DrawString("T (03) 9432 6944", lineFont, XBrushes.Black, new XPoint(textStartX, ty));
            ty += 12;
            gfx.DrawString("E mail@prs.au", lineFont, XBrushes.Black, new XPoint(textStartX, ty));
            ty += 12;
            gfx.DrawString("W PRS.AU", lineFont, XBrushes.Black, new XPoint(textStartX, ty));
            ty += 12;
            gfx.DrawString("ABN 11 070 127 552", lineFont, XBrushes.Black, new XPoint(textStartX, ty));
            ty += 12;
            gfx.DrawString("45/7 Dalton Road", lineFont, XBrushes.Black, new XPoint(textStartX, ty));
            ty += 12;
            gfx.DrawString("Thomastown VIC 3074", lineFont, XBrushes.Black, new XPoint(textStartX, ty));
            ty += 12;
            gfx.DrawString("Suite 1 Level1, 20-30", lineFont, XBrushes.Black, new XPoint(textStartX, ty));
            ty += 12;
            gfx.DrawString("Mollison Street", lineFont, XBrushes.Black, new XPoint(textStartX, ty));
            ty += 12;
            gfx.DrawString("Abbotsford VIC 3067", lineFont, XBrushes.Black, new XPoint(textStartX, ty));
            ty += 14;
            XTextFormatter tf = new(gfx);
            XRect liabilityRect = new(textStartX, ty, margin + contentWidth - textStartX, 36);
            tf.DrawString("Liability limited by a scheme approved under Professional Standards Legislation",
                smallFont, XBrushes.Black, liabilityRect, XStringFormats.TopLeft);
            ty = Math.Max(headerTop + logoMaxHeight + 8, ty + tf.LayoutRectangle.Height + 12);

            gfx.DrawLine(XPens.LightGray, margin, ty, margin + contentWidth, ty);
            return ty + 16;
        }
        finally
        {
            logo?.Dispose();
        }
    }

    private XImage? TryOpenLogoRaster()
    {
        foreach (string name in LogoRasterCandidates)
        {
            string? path = ResolveAssetPath(Path.Combine("images", name));
            if (path is not null)
                return XImage.FromFile(path);
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

    private static void DrawCoverLetterBody(
        XGraphics gfx,
        QuoteDetailsDto quote,
        double margin,
        double contentWidth,
        double y,
        double pageBottom)
    {
        XFont body = CreateFont(10, XFontStyleEx.Regular);
        XFont bold = CreateFont(10, XFontStyleEx.Bold);
        XFont small = CreateFont(8.5, XFontStyleEx.Regular);

        string issued = quote.CreatedAt.ToString("d MMMM yyyy", Au);
        string refLine = $"{issued}     Quote Ref: {quote.QuoteReference}";
        gfx.DrawString(refLine, body, XBrushes.Black, new XRect(margin, y, contentWidth, 20), XStringFormats.TopLeft);
        y += 22;

        if (quote.Contact is not null)
        {
            gfx.DrawString($"Attention: {quote.Contact.FullName}", body, XBrushes.Black, new XPoint(margin, y));
            y += 14;
            if (!string.IsNullOrWhiteSpace(quote.Contact.Phone))
            {
                gfx.DrawString($"Phone: {quote.Contact.Phone}", body, XBrushes.Black, new XPoint(margin, y));
                y += 14;
            }

            gfx.DrawString($"Email: {quote.Contact.Email}", body, XBrushes.Black, new XPoint(margin, y));
            y += 18;
        }

        string first = FirstName(quote.Contact?.FullName);
        gfx.DrawString($"Dear {first},", body, XBrushes.Black, new XPoint(margin, y));
        y += 18;

        string subject = quote.Address is not null ? quote.Address.ToDisplayString() : "YOUR PROPERTY";
        gfx.DrawString($"RE: {subject}", bold, XBrushes.Black, new XRect(margin, y, contentWidth, 40), XStringFormats.TopLeft);
        y += 22;

        y = DrawWrappedParagraph(gfx,
            "Thank you for the opportunity to provide our fee proposal for the proposed survey services at the above property. Our proposed scope of works and fees are set out below.",
            body,
            margin,
            contentWidth,
            y,
            pageBottom);

        y = DrawWrappedParagraph(gfx,
            "Please note: This fee proposal remains valid for 30 days from the date of issue. If acceptance is received after this time, our fees may be reviewed and revised prior to commencement.",
            body,
            margin,
            contentWidth,
            y,
            pageBottom);

        gfx.DrawString("Project Scope:", bold, XBrushes.Black, new XPoint(margin, y));
        y += 16;
        string scopeText = string.IsNullOrWhiteSpace(quote.Description)
            ? "Survey services as summarised in the fee summary and detailed scope of works following this letter."
            : quote.Description!;
        y = DrawWrappedParagraph(gfx, scopeText, body, margin, contentWidth, y, pageBottom);

        y += 8;
        gfx.DrawString("Fee Summary", bold, XBrushes.Black, new XPoint(margin, y));
        y += 16;

        foreach (QuoteItemDto item in quote.QuoteItems)
        {
            string label = item.ServiceName;
            string fee = FormatFeePlusGst(item.Price);
            gfx.DrawString(label, body, XBrushes.Black, new XRect(margin, y, contentWidth * 0.62, 80), XStringFormats.TopLeft);
            gfx.DrawString(fee, body, XBrushes.Black, new XRect(margin + contentWidth * 0.62, y, contentWidth * 0.38, 80),
                XStringFormats.TopRight);
            y += 16;
        }

        gfx.DrawString("Total Fees (excluding disbursements)", bold, XBrushes.Black,
            new XRect(margin, y, contentWidth * 0.62, 24), XStringFormats.TopLeft);
        gfx.DrawString(FormatFeePlusGst(quote.TotalPrice), bold, XBrushes.Black,
            new XRect(margin + contentWidth * 0.62, y, contentWidth * 0.38, 24), XStringFormats.TopRight);
        y += 22;

        y = DrawWrappedParagraph(gfx,
            "Disbursements: General disbursement costs for searching relevant survey information will be in addition to the cost above (these typically range in the vicinity of $50-$150+GST per property).",
            body,
            margin,
            contentWidth,
            y,
            pageBottom);

        y = DrawWrappedParagraph(gfx,
            "Scheduling of Survey: A site booking date will be scheduled upon receiving the attached Acceptance Form. Currently, our lead time is approximately one to two weeks to undertake site works. Survey plans are generally available within 7-8* business days following completion of site works.",
            body,
            margin,
            contentWidth,
            y,
            pageBottom);

        y = DrawWrappedParagraph(gfx,
            "Timeframes above are estimates only and may vary depending on site conditions, access, project complexity, or matters arising during the survey, drafting and review process. Where additional time is required to ensure an accurate and professionally prepared outcome, we will make every reasonable effort to communicate any delay as soon as practicable.",
            small,
            margin,
            contentWidth,
            y,
            pageBottom);

        y += 6;
        gfx.DrawString("Acceptance:", bold, XBrushes.Black, new XPoint(margin, y));
        y += 14;
        gfx.DrawString("To accept this fee proposal, please fill out the Acceptance Form located within.", body, XBrushes.Black,
            new XRect(margin, y, contentWidth, 40), XStringFormats.TopLeft);
        y += 28;

        gfx.DrawString("Yours Sincerely,", body, XBrushes.Black, new XPoint(margin, y));
        y += 36;
        gfx.DrawString("Brodie Richards", bold, XBrushes.Black, new XPoint(margin, y));
        y += 14;
        gfx.DrawString("Managing Director", body, XBrushes.Black, new XPoint(margin, y));
        y += 12;
        gfx.DrawString("Licensed Surveyor", body, XBrushes.Black, new XPoint(margin, y));
        y += 22;

        XFont italic = CreateFont(9, XFontStyleEx.Italic);
        gfx.DrawString("Please continue for detailed scope of works, exclusions and additional information", italic,
            XBrushes.DarkGray, new XRect(margin, y, contentWidth, 40), XStringFormats.TopLeft);
    }

    private static void DrawScopeOfWorksPages(PdfDocument document, QuoteDetailsDto quote, double margin, double contentWidth)
    {
        PdfPage page = document.AddPage();
        page.Size = PageSize.A4;
        XGraphics gfx = XGraphics.FromPdfPage(page);
        double y = margin;
        double pageBottom = page.Height.Point - margin;
        XFont heading = CreateFont(10, XFontStyleEx.Bold);
        XFont body = CreateFont(10, XFontStyleEx.Regular);

        void NewScopePage(bool continued)
        {
            gfx.Dispose();
            page = document.AddPage();
            page.Size = PageSize.A4;
            gfx = XGraphics.FromPdfPage(page);
            y = margin;
            pageBottom = page.Height.Point - margin;
            y = DrawMiniLetterhead(gfx, margin, contentWidth, y);
            gfx.DrawString(continued ? "Scope of Works (continued):" : "Scope of Works:", heading, XBrushes.Black,
                new XPoint(margin, y));
            y += 18;
        }

        try
        {
            y = DrawMiniLetterhead(gfx, margin, contentWidth, y);
            gfx.DrawString("Scope of Works:", heading, XBrushes.Black, new XPoint(margin, y));
            y += 18;

            foreach (QuoteItemDto item in quote.QuoteItems)
            {
                string detail = string.IsNullOrWhiteSpace(item.Description) ? "As agreed for this service." : item.Description!;
                if (y + 40 > pageBottom)
                    NewScopePage(true);

                gfx.DrawString(item.ServiceName, heading, XBrushes.Black, new XPoint(margin, y));
                y += 14;
                y = DrawWrappedParagraph(gfx, detail, body, margin, contentWidth, y, pageBottom);
                if (y + 40 > pageBottom)
                    NewScopePage(true);
                else
                    y += 10;
            }
        }
        finally
        {
            gfx.Dispose();
        }
    }

    private static double DrawMiniLetterhead(XGraphics gfx, double margin, double contentWidth, double y)
    {
        XFont companyFont = CreateFont(9, XFontStyleEx.Bold);
        XFont lineFont = CreateFont(8.5, XFontStyleEx.Regular);
        XFont smallFont = CreateFont(7.5, XFontStyleEx.Italic);

        gfx.DrawString("PETER RICHARDS SURVEYING", companyFont, XBrushes.Black, new XPoint(margin, y));
        y += 13;
        gfx.DrawString("T (03) 9432 6944", lineFont, XBrushes.Black, new XPoint(margin, y));
        y += 11;
        gfx.DrawString("E mail@prs.au", lineFont, XBrushes.Black, new XPoint(margin, y));
        y += 11;
        gfx.DrawString("W PRS.AU", lineFont, XBrushes.Black, new XPoint(margin, y));
        y += 11;
        gfx.DrawString("ABN 11 070 127 552", lineFont, XBrushes.Black, new XPoint(margin, y));
        y += 11;
        gfx.DrawString("45/7 Dalton Road", lineFont, XBrushes.Black, new XPoint(margin, y));
        y += 11;
        gfx.DrawString("Thomastown VIC 3074", lineFont, XBrushes.Black, new XPoint(margin, y));
        y += 11;
        gfx.DrawString("Suite 1 Level1, 20-30", lineFont, XBrushes.Black, new XPoint(margin, y));
        y += 11;
        gfx.DrawString("Mollison Street", lineFont, XBrushes.Black, new XPoint(margin, y));
        y += 11;
        gfx.DrawString("Abbotsford VIC 3067", lineFont, XBrushes.Black, new XPoint(margin, y));
        y += 12;
        XTextFormatter tf = new(gfx);
        XRect liabilityRect = new(margin, y, contentWidth, 28);
        tf.DrawString("Liability limited by a scheme approved under Professional Standards Legislation",
            smallFont, XBrushes.Black, liabilityRect, XStringFormats.TopLeft);
        y += tf.LayoutRectangle.Height + 14;
        return y;
    }

    private static void DrawExclusionsAndInfoPage(PdfDocument document, double margin, double contentWidth)
    {
        PdfPage page = document.AddPage();
        page.Size = PageSize.A4;
        using XGraphics gfx = XGraphics.FromPdfPage(page);
        double y = margin;
        y = DrawMiniLetterhead(gfx, margin, contentWidth, y);

        XFont heading = CreateFont(10, XFontStyleEx.Bold);
        XFont body = CreateFont(10, XFontStyleEx.Regular);
        double pageBottom = page.Height.Point - margin;

        gfx.DrawString("Exclusions:", heading, XBrushes.Black, new XPoint(margin, y));
        y += 16;
        const string exclusionsBody =
            "Unless specifically noted otherwise, our fee does not include:\n" +
            "• Title boundaries unless a Title Re-establishment Survey is completed;\n" +
            "• Additional boundary mark placement or reinstatement beyond the quoted scope;\n" +
            "• Physical locating of underground services by specialist methods;\n" +
            "• Return visits due to restricted or unavailable site access;\n" +
            "• Invert levels of drainage pits, pipes or underground drainage infrastructure unless specifically requested;\n" +
            "• Point cloud data, BIM/3D modelling, or other advanced digital deliverables;";
        y = DrawWrappedParagraph(gfx, exclusionsBody, body, margin, contentWidth, y, pageBottom);

        y += 12;
        gfx.DrawString("Additional Information", heading, XBrushes.Black, new XPoint(margin, y));
        y += 16;
        y = DrawWrappedParagraph(gfx,
            "Payment Terms: Payment is due within 14 days of receiving the tax invoice and prior to the release of any plans.",
            body,
            margin,
            contentWidth,
            y,
            pageBottom);
    }

    private void DrawSurveyAreaPage(PdfDocument document, double margin, double contentWidth)
    {
        PdfPage page = document.AddPage();
        page.Size = PageSize.A4;
        using XGraphics gfx = XGraphics.FromPdfPage(page);
        double y = margin;
        y = DrawMiniLetterhead(gfx, margin, contentWidth, y);

        XFont heading = CreateFont(10, XFontStyleEx.Bold);
        gfx.DrawString("Proposed Survey Area", heading, XBrushes.Black, new XPoint(margin, y));
        y += 18;

        string? mapPath = ResolveAssetPath(Path.Combine("images", "PRS-Primary-Logo.png"));
        if (mapPath is null)
            return;

        using XImage map = XImage.FromFile(mapPath);
        double maxW = contentWidth;
        double maxH = page.Height.Point - margin - y;
        double scale = Math.Min(maxW / map.PixelWidth * 72.0 / map.HorizontalResolution,
            maxH / map.PixelHeight * 72.0 / map.VerticalResolution);
        double drawW = map.PixelWidth * scale * 72.0 / map.HorizontalResolution;
        double drawH = map.PixelHeight * scale * 72.0 / map.VerticalResolution;
        gfx.DrawImage(map, margin, y, drawW, drawH);
    }

    private static double DrawWrappedParagraph(
        XGraphics gfx,
        string text,
        XFont font,
        double margin,
        double contentWidth,
        double y,
        double pageBottom)
    {
        double remaining = pageBottom - y;
        if (remaining < 8)
            return y;

        XTextFormatter tf = new(gfx);
        XRect rect = new(margin, y, contentWidth, remaining);
        tf.DrawString(text, font, XBrushes.Black, rect, XStringFormats.TopLeft);
        double used = tf.LayoutRectangle.Height;
        if (used < 1)
            used = gfx.MeasureString("Ig", font).Height;
        return y + used + 6;
    }

    private static string FirstName(string? fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return "Sir/Madam";

        string[] parts = fullName.Trim().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 0 ? parts[0] : "Sir/Madam";
    }

    private static string FormatFeePlusGst(decimal amount) =>
        "$" + amount.ToString("#,##0", CultureInfo.InvariantCulture) + "+GST";

    private static void EnsureFontResolver()
    {
        if (_fontResolverInitialized)
            return;

        lock (FontInitLock)
        {
            if (_fontResolverInitialized)
                return;

            GlobalFontSettings.FontResolver = new ApplicationPdfFontResolver();
            _fontResolverInitialized = true;
        }
    }

    private static XFont CreateFont(double size, XFontStyleEx style) =>
        new("Liberation Sans", size, style);
}

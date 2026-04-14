using PdfSharp.Fonts;

namespace Portal.Server.Services.Instances.UtilityServices;

/// <summary>
/// Resolves sans-serif fonts from well-known paths (Liberation on Linux, Arial/Segoe fallbacks on Windows).
/// PDFsharp 6 may not find "Segoe UI" under some hosts (e.g. integration tests); file-based resolution stays reliable.
/// </summary>
internal sealed class ApplicationPdfFontResolver : IFontResolver
{
    private static readonly string[][] RegularPaths =
    [
        ["/usr/share/fonts/truetype/liberation/LiberationSans-Regular.ttf"],
        ["/usr/share/fonts/TTF/LiberationSans-Regular.ttf"],
        [@"C:\Windows\Fonts\arial.ttf"],
        [@"C:\Windows\Fonts\calibri.ttf"],
        [@"C:\Windows\Fonts\segoeui.ttf"]
    ];

    private static readonly string[][] BoldPaths =
    [
        ["/usr/share/fonts/truetype/liberation/LiberationSans-Bold.ttf"],
        ["/usr/share/fonts/TTF/LiberationSans-Bold.ttf"],
        [@"C:\Windows\Fonts\arialbd.ttf"],
        [@"C:\Windows\Fonts\calibrib.ttf"],
        [@"C:\Windows\Fonts\segoeuib.ttf"]
    ];

    private static readonly string[][] ItalicPaths =
    [
        ["/usr/share/fonts/truetype/liberation/LiberationSans-Italic.ttf"],
        ["/usr/share/fonts/TTF/LiberationSans-Italic.ttf"],
        [@"C:\Windows\Fonts\ariali.ttf"],
        [@"C:\Windows\Fonts\calibrii.ttf"],
        [@"C:\Windows\Fonts\segoeuii.ttf"]
    ];

    private static readonly string[][] BoldItalicPaths =
    [
        ["/usr/share/fonts/truetype/liberation/LiberationSans-BoldItalic.ttf"],
        ["/usr/share/fonts/TTF/LiberationSans-BoldItalic.ttf"],
        [@"C:\Windows\Fonts\arialbi.ttf"],
        [@"C:\Windows\Fonts\calibriz.ttf"],
        [@"C:\Windows\Fonts\segoeuiz.ttf"]
    ];

    public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        _ = familyName;
        // PDFsharp may request simulated styles; we map to nearest TTF.
        string key = isBold ? "LiberationSans#Bold" : "LiberationSans#Regular";
        if (isItalic && !isBold)
            key = "LiberationSans#Italic";
        if (isItalic && isBold)
            key = "LiberationSans#BoldItalic";

        return new FontResolverInfo(key);
    }

    public byte[] GetFont(string faceName) =>
        faceName switch
        {
            "LiberationSans#Regular" => ReadFirstExisting(RegularPaths),
            "LiberationSans#Bold" => ReadFirstExisting(BoldPaths),
            "LiberationSans#Italic" => ReadFirstExisting(ItalicPaths),
            "LiberationSans#BoldItalic" => ReadFirstExisting(BoldItalicPaths),
            _ => ReadFirstExisting(RegularPaths)
        };

    private static byte[] ReadFirstExisting(string[][] pathGroups)
    {
        foreach (string[] group in pathGroups)
        {
            foreach (string path in group)
            {
                if (File.Exists(path))
                    return File.ReadAllBytes(path);
            }
        }

        throw new InvalidOperationException(
            "No suitable TTF font found for PDF generation. On Linux install fonts-liberation; on Windows ensure %WINDIR%\\Fonts is available.");
    }
}

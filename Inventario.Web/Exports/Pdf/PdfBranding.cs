using Microsoft.AspNetCore.Hosting;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Inventario.Web.Exports.Pdf;

public static class PdfBranding
{
    private const string InventoryLogoFileName = "logo-inventario.png";

    public static string ResolveWebRootPath(IWebHostEnvironment environment)
    {
        return string.IsNullOrWhiteSpace(environment.WebRootPath)
            ? Path.Combine(environment.ContentRootPath, "wwwroot")
            : environment.WebRootPath;
    }

    public static void ComposeInventoryLogo(IContainer container, string webRootPath)
    {
        container
            .Border(1)
            .BorderColor(PdfTheme.GrisLinea)
            .Background(Colors.White)
            .Padding(6)
            .Element(inner =>
            {
                var logoPath = Path.Combine(webRootPath, "images", InventoryLogoFileName);
                if (File.Exists(logoPath))
                {
                    inner.AlignCenter().AlignMiddle().Image(File.ReadAllBytes(logoPath));
                    return;
                }

                inner.Column(column =>
                {
                    column.Item().AlignCenter().Text("IC").FontSize(20).SemiBold().FontColor(PdfTheme.AzulPrincipal);
                    column.Item().AlignCenter().Text("Inventario").FontSize(8).FontColor(PdfTheme.AzulPrincipal);
                });
            });
    }

    public static byte[]? TryResolveImageBytes(string webRootPath, string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return null;
        }

        foreach (var candidatePath in BuildCandidatePaths(relativePath))
        {
            var absoluteCandidatePath = Path.Combine(webRootPath, candidatePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(absoluteCandidatePath))
            {
                return File.ReadAllBytes(absoluteCandidatePath);
            }
        }

        return null;
    }

    public static void ComposeProductThumbnail(IContainer container, string webRootPath, string? relativePath)
    {
        var bytes = TryResolveImageBytes(webRootPath, relativePath);

        container
            .Border(1)
            .BorderColor(PdfTheme.GrisLinea)
            .Background(Colors.White)
            .Padding(3)
            .AlignCenter()
            .AlignMiddle()
            .Element(inner =>
            {
                if (bytes is not null)
                {
                    inner.Padding(1).Image(bytes);
                    return;
                }

                inner.Text("IMG").FontSize(8).FontColor(PdfTheme.TextoSecundario).SemiBold();
            });
    }

    private static IReadOnlyCollection<string> BuildCandidatePaths(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return Array.Empty<string>();
        }

        var extension = Path.GetExtension(relativePath);
        if (string.IsNullOrWhiteSpace(extension))
        {
            return Array.Empty<string>();
        }

        var candidates = new List<string>
        {
            relativePath[..^extension.Length] + "-thumb" + extension,
            relativePath
        };

        if (extension.Equals(".png", StringComparison.OrdinalIgnoreCase))
        {
            candidates.Insert(1, relativePath[..^extension.Length] + "-thumb.jpg");
            candidates.Insert(2, relativePath[..^extension.Length] + ".jpg");
        }

        return candidates;
    }
}

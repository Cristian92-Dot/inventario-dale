using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Inventario.Web.Exports.Pdf;

public static class PdfTheme
{
    public static string AzulPrincipal => "#1D4ED8";
    public static string AzulOscuro => "#0F172A";
    public static string AzulSuave => "#DBEAFE";
    public static string GrisLinea => "#CBD5E1";
    public static string Texto => "#0F172A";
    public static string TextoSecundario => "#475569";

    public static TextStyle Titulo => TextStyle.Default.FontSize(22).SemiBold().FontColor(AzulOscuro);
    public static TextStyle Subtitulo => TextStyle.Default.FontSize(11).FontColor(TextoSecundario);
    public static TextStyle Etiqueta => TextStyle.Default.FontSize(10).SemiBold().FontColor(TextoSecundario);
    public static TextStyle Valor => TextStyle.Default.FontSize(11).FontColor(Texto);
}

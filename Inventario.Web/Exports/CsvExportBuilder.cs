using System.Text;
using Inventario.Application.Features.Reports.Dtos;

namespace Inventario.Web.Exports;

public static class CsvExportBuilder
{
    public static byte[] BuildReport(IEnumerable<ReportExportRowDto> rows)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Nombre,ImagenPath,Precio,StockActual,StockMinimo,EstadoReposicion,Estado,FechaActualizacion");

        foreach (var row in rows)
        {
            builder.AppendLine(string.Join(',',
                Escape(row.Nombre),
                Escape(row.ImagenPath ?? string.Empty),
                row.Precio.ToString(System.Globalization.CultureInfo.InvariantCulture),
                row.StockActual,
                row.StockMinimo,
                Escape(row.EstadoReposicion),
                Escape(row.Estado),
                row.FechaActualizacion.ToString("yyyy-MM-dd HH:mm:ss")));
        }

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    private static string Escape(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}

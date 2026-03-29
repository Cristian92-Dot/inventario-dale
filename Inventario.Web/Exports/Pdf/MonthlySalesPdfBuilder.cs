using Inventario.Application.Features.Reports.Dtos;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Inventario.Web.Exports.Pdf;

public static class MonthlySalesPdfBuilder
{
    public static byte[] Build(MonthlySalesPdfDto model, string webRootPath)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(32);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(PdfTheme.Texto));

                page.Header().Column(header =>
                {
                    header.Item().Row(row =>
                    {
                        row.ConstantItem(105).Element(container => PdfBranding.ComposeInventoryLogo(container, webRootPath));
                        row.RelativeItem().PaddingLeft(16).Column(col =>
                        {
                            col.Item().Text(model.Empresa).Style(PdfTheme.Titulo);
                            col.Item().Text($"Reporte mensual de ventas - {model.Mes} {model.Anio}").FontSize(15).SemiBold().FontColor(PdfTheme.AzulPrincipal);
                            col.Item().Text($"Área solicitante: {model.AreaSolicitante}").Style(PdfTheme.Subtitulo);
                            col.Item().Text($"Responsable: {model.Responsable}").Style(PdfTheme.Subtitulo);
                        });
                    });
                });

                page.Content().PaddingVertical(18).Column(content =>
                {
                    content.Item().Row(row =>
                    {
                        void SummaryCard(IContainer container, string title, string value) => container.Background(PdfTheme.AzulSuave).Padding(10).Column(col =>
                        {
                            col.Item().Text(title).Style(PdfTheme.Etiqueta);
                            col.Item().Text(value).FontSize(14).SemiBold().FontColor(PdfTheme.AzulOscuro);
                        });

                        row.RelativeItem().Element(c => SummaryCard(c, "Total vendido", model.TotalVendido.ToString("C")));
                        row.RelativeItem().PaddingLeft(8).Element(c => SummaryCard(c, "Ventas registradas", model.TotalVentas.ToString()));
                        row.RelativeItem().PaddingLeft(8).Element(c => SummaryCard(c, "Promedio por venta", model.PromedioPorVenta.ToString("C")));
                    });

                    content.Item().PaddingTop(18).Text("Totales por día").FontSize(13).SemiBold().FontColor(PdfTheme.AzulOscuro);
                    content.Item().PaddingTop(8).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1.3f);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1.2f);
                        });

                        table.Header(header =>
                        {
                            static IContainer Cell(IContainer container) => container.Background(PdfTheme.AzulPrincipal).Padding(8);

                            header.Cell().Element(Cell).Text("Fecha").FontColor(Colors.White).SemiBold();
                            header.Cell().Element(Cell).AlignCenter().Text("Ventas").FontColor(Colors.White).SemiBold();
                            header.Cell().Element(Cell).AlignRight().Text("Total").FontColor(Colors.White).SemiBold();
                        });

                        foreach (var day in model.TotalesPorDia)
                        {
                            table.Cell().BorderBottom(1).BorderColor(PdfTheme.GrisLinea).Padding(8).Text(day.Fecha.ToString("dd/MM/yyyy"));
                            table.Cell().BorderBottom(1).BorderColor(PdfTheme.GrisLinea).Padding(8).AlignCenter().Text(day.Ventas.ToString());
                            table.Cell().BorderBottom(1).BorderColor(PdfTheme.GrisLinea).Padding(8).AlignRight().Text(day.Total.ToString("C"));
                        }
                    });

                    content.Item().PaddingTop(18).Text("Productos más vendidos").FontSize(13).SemiBold().FontColor(PdfTheme.AzulOscuro);
                    content.Item().PaddingTop(8).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(58);
                            columns.RelativeColumn(2.5f);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1.2f);
                        });

                        table.Header(header =>
                        {
                            static IContainer Cell(IContainer container) => container.Background("#E0EAFF").Padding(8);

                            header.Cell().Element(Cell).AlignCenter().Text("Imagen").SemiBold();
                            header.Cell().Element(Cell).Text("Producto").SemiBold();
                            header.Cell().Element(Cell).AlignCenter().Text("Cantidad").SemiBold();
                            header.Cell().Element(Cell).AlignRight().Text("Total vendido").SemiBold();
                        });

                        foreach (var item in model.ProductosMasVendidos)
                        {
                            table.Cell().BorderBottom(1).BorderColor(PdfTheme.GrisLinea).Padding(4).Element(cell => PdfBranding.ComposeProductThumbnail(cell, webRootPath, item.ImagenPath));
                            table.Cell().BorderBottom(1).BorderColor(PdfTheme.GrisLinea).Padding(8).Text(item.Nombre);
                            table.Cell().BorderBottom(1).BorderColor(PdfTheme.GrisLinea).Padding(8).AlignCenter().Text(item.CantidadVendida.ToString());
                            table.Cell().BorderBottom(1).BorderColor(PdfTheme.GrisLinea).Padding(8).AlignRight().Text(item.TotalVendido.ToString("C"));
                        }
                    });
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Reporte generado automáticamente | ").FontColor(PdfTheme.TextoSecundario);
                    text.Span(model.FechaGeneracion.ToString("dd/MM/yyyy HH:mm")).FontColor(PdfTheme.TextoSecundario);
                });
            });
        }).GeneratePdf();
    }
}

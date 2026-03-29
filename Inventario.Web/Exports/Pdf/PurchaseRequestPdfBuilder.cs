using Inventario.Application.Features.Reports.Dtos;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Inventario.Web.Exports.Pdf;

public static class PurchaseRequestPdfBuilder
{
    public static byte[] Build(PurchaseRequestPdfDto model, string webRootPath)
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
                            col.Item().Text(model.Titulo).FontSize(15).SemiBold().FontColor(PdfTheme.AzulPrincipal);
                            col.Item().Text($"Área solicitante: {model.AreaSolicitante}").Style(PdfTheme.Subtitulo);
                            col.Item().Text($"Responsable: {model.Responsable}").Style(PdfTheme.Subtitulo);
                        });
                    });

                    header.Item().PaddingTop(16).Row(row =>
                    {
                        row.RelativeItem().Background(PdfTheme.AzulSuave).Padding(10).Column(col =>
                        {
                            col.Item().Text("Fecha de emisión").Style(PdfTheme.Etiqueta);
                            col.Item().Text(model.FechaEmision.ToString("dd/MM/yyyy HH:mm")).Style(PdfTheme.Valor);
                        });
                        row.RelativeItem().PaddingLeft(10).Background(PdfTheme.AzulSuave).Padding(10).Column(col =>
                        {
                            col.Item().Text("Correlativo").Style(PdfTheme.Etiqueta);
                            col.Item().Text(model.Correlativo).Style(PdfTheme.Valor);
                        });
                    });
                });

                page.Content().PaddingVertical(18).Column(content =>
                {
                    content.Item().Text("Resumen ejecutivo").FontSize(13).SemiBold().FontColor(PdfTheme.AzulOscuro);
                    content.Item().PaddingTop(4).Text("Este documento consolida los productos que requieren reposición inmediata para sostener la continuidad operativa del inventario.").Style(PdfTheme.Subtitulo);

                    content.Item().PaddingTop(18).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(58);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1.2f);
                            columns.RelativeColumn(1.3f);
                        });

                        table.Header(header =>
                        {
                            static IContainer Cell(IContainer container) => container.Background(PdfTheme.AzulPrincipal).Padding(8);

                            header.Cell().Element(Cell).AlignCenter().Text("Imagen").FontColor(Colors.White).SemiBold();
                            header.Cell().Element(Cell).Text("Producto").FontColor(Colors.White).SemiBold();
                            header.Cell().Element(Cell).AlignCenter().Text("Stock actual").FontColor(Colors.White).SemiBold();
                            header.Cell().Element(Cell).AlignCenter().Text("Stock mínimo").FontColor(Colors.White).SemiBold();
                            header.Cell().Element(Cell).AlignCenter().Text("Cantidad a comprar").FontColor(Colors.White).SemiBold();
                            header.Cell().Element(Cell).AlignCenter().Text("Estado").FontColor(Colors.White).SemiBold();
                        });

                        foreach (var item in model.Items)
                        {
                            table.Cell().BorderBottom(1).BorderColor(PdfTheme.GrisLinea).Padding(4).Element(cell => PdfBranding.ComposeProductThumbnail(cell, webRootPath, item.ImagenPath));
                            table.Cell().BorderBottom(1).BorderColor(PdfTheme.GrisLinea).Padding(8).Text(item.Nombre);
                            table.Cell().BorderBottom(1).BorderColor(PdfTheme.GrisLinea).Padding(8).AlignCenter().Text(item.StockActual.ToString());
                            table.Cell().BorderBottom(1).BorderColor(PdfTheme.GrisLinea).Padding(8).AlignCenter().Text(item.StockMinimo.ToString());
                            table.Cell().BorderBottom(1).BorderColor(PdfTheme.GrisLinea).Padding(8).AlignCenter().Text(item.CantidadSugerida.ToString());
                            table.Cell().BorderBottom(1).BorderColor(PdfTheme.GrisLinea).Padding(8).AlignCenter().Text(item.Estado);
                        }
                    });

                    content.Item().PaddingTop(18).Row(row =>
                    {
                        row.RelativeItem().Border(1).BorderColor(PdfTheme.GrisLinea).Padding(12).Column(col =>
                        {
                            col.Item().Text("Observaciones").Style(PdfTheme.Etiqueta);
                            col.Item().Text("Documento de prueba generado para procesos de abastecimiento. Se recomienda validar proveedor sugerido, prioridad y fecha objetivo antes de emitir la orden final.").Style(PdfTheme.Subtitulo);
                        });
                        row.RelativeItem().PaddingLeft(12).Border(1).BorderColor(PdfTheme.GrisLinea).Padding(12).Column(col =>
                        {
                            col.Item().Text("Aprobación").Style(PdfTheme.Etiqueta);
                            col.Item().PaddingTop(24).BorderBottom(1).BorderColor(PdfTheme.GrisLinea);
                            col.Item().PaddingTop(8).Text("Firma / nombre responsable").Style(PdfTheme.Subtitulo);
                        });
                    });
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Documento generado por Inventario Central | ").FontColor(PdfTheme.TextoSecundario);
                    text.Span(model.FechaEmision.ToString("dd/MM/yyyy HH:mm")).FontColor(PdfTheme.TextoSecundario);
                });
            });
        }).GeneratePdf();
    }
}

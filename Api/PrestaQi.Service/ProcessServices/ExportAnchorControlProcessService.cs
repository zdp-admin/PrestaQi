using ClosedXML.Excel;
using InsiscoCore.Base.Service;
using PrestaQi.Service.Tools;
using InsiscoCore.Service;
using PrestaQi.Model;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Model.Dto.Output;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using iText.Kernel.Colors;
using iText.Kernel.Geom;
using System;
using iText.StyledXmlParser.Node;
using PrestaQi.Model.Enum;

namespace PrestaQi.Service.ProcessServices
{
    public class ExportAnchorControlProcessService : ProcessService<ExportAnchorControl>
    {
        IRetrieveService<Configuration> _ConfigurationRetrieveService;

        public ExportAnchorControlProcessService(
            IRetrieveService<Configuration> configurationRetrieveService
            )
        {
            this._ConfigurationRetrieveService = configurationRetrieveService;
        }

        public MemoryStream ExecuteProcess(ExportAnchorControl export)
        {
            var configuration = this._ConfigurationRetrieveService.Where(p => p.Configuration_Name == "COLUMNS_ANCHORCONTROL_EXPORT").FirstOrDefault();
            List<string> columns = configuration.Configuration_Value.Split('|').ToList();

            return export.Type == 1 ?
                GenerateExcel(export.AnchorControls, columns) : 
                GeneratePdf(export.AnchorControls, columns);
        }

        MemoryStream GenerateExcel(List<AnchorControl> data, List<string> columns)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                var workBook = new XLWorkbook();
                var workSheet = workBook.Worksheets.Add("Data");

                var range = workSheet.Range(1, 1, 1, columns.Count);
                range.Merge();
                range.Value = "Listado de Control de Fondeo";
                range.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                range.Style.Font.Bold = true;
                range.Style.Font.SetFontSize(20);

                columns.ForEach((item, index) =>
                {
                    workSheet.Cell(3, index + 1).Value = item;
                    workSheet.Cell(3, index + 1).Style.Font.Bold = true;
                });

                int row = 4;

                double total = 0;
                foreach (var item in data)
                {
                    workSheet.Cell(row, 1).Value = item.Investor_Id;
                    workSheet.Cell(row, 2).Value = item.Name_Complete;

                    if (item.MyInvestments.Count == 0)
                        row += 1;

                    item.MyInvestments.ForEach(p =>
                    {
                        workSheet.Cell(row, 3).Value = $"${p.Amount}";
                        workSheet.Cell(row, 4).Value = p.Start_Date.ToString("dd/MM/yyyy");
                        workSheet.Cell(row, 5).Value = p.End_Date.ToString("dd/MM/yyyy");
                        workSheet.Cell(row, 6).Value = $"{p.Interest_Rate}%";
                        workSheet.Cell(row, 7).Value = $"${p.Interest_Payable}";
                        workSheet.Cell(row, 8).Value = $"${p.Quantity_Interest_Arrears}";
                        workSheet.Cell(row, 9).Value = p.Pay_Day_Limit.ToString("dd/MM/yyyy");
                        workSheet.Cell(row, 10).Value = p.Enabled == true ? "Activo" : "No Activo";
                        workSheet.Cell(row, 11).Value = p.Principal_Payment;

                        if (DateTime.Now <= p.Pay_Day_Limit)
                            total += p.Interest_Payable;

                        row += 1;
                    });
                }

                var rangeTotalText = workSheet.Range(row + 1, 1, row + 1, 6);
                rangeTotalText.Merge();
                rangeTotalText.Value = $"Monto a pagar en fecha próxima: ";
                rangeTotalText.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                rangeTotalText.Style.Font.Bold = true;
                rangeTotalText.Style.Font.SetFontSize(14);

                workSheet.Cell(row + 1, 7).Value = $"${total}";
                workSheet.Cell(row + 1, 7).Style.Font.Bold = true;
                workSheet.Cell(row + 1, 7).Style.Font.SetFontSize(14);

                var rangeTotalExt = workSheet.Range(row + 1, 8, row + 1, columns.Count);
                rangeTotalExt.Merge();

                workBook.SaveAs(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);

                return memoryStream;
            }
        }

        MemoryStream GeneratePdf(List<AnchorControl> data, List<string> columns)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                PdfWriter pdfWriter = new PdfWriter(memoryStream);
                PdfDocument pdfDocument = new PdfDocument(pdfWriter);
                pdfDocument.SetDefaultPageSize(PageSize.LEGAL.Rotate());
                Document document = new Document(pdfDocument);
                Paragraph header = new Paragraph("Listado de Control de Fondeo")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(20);

                List<Cell> cells = new List<Cell>();

                Table table = new Table(columns.Count, true);

                columns.ForEach(p =>
                {
                    Cell cell = new Cell(1, 1)
                      .SetBackgroundColor(ColorConstants.GRAY)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .Add(new Paragraph(p));

                    cells.Add(cell);
                });

                double total = 0;
                foreach (var item in data)
                {
                    cells.Add(new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .Add(new Paragraph(item.Investor_Id.ToString())));

                    cells.Add(new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .Add(new Paragraph(item.Name_Complete)));


                    if (item.MyInvestments.Count > 0)
                    {
                        item.MyInvestments.ForEach((data, index) =>
                        {
                            if (DateTime.Now <= data.Pay_Day_Limit)
                                total += data.Interest_Payable;

                            if (index != 0)
                            {
                               Utilities.GenerateEmptyCell(2, cells);
                            }

                            cells.Add(new Cell(1, 1)
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetFontSize(8)
                                .Add(new Paragraph($"${data.Amount}")));

                            cells.Add(new Cell(1, 1)
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetFontSize(8)
                                .Add(new Paragraph(data.Start_Date.ToString("dd/MM/yyyy"))));


                            cells.Add(new Cell(1, 1)
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetFontSize(8)
                                .Add(new Paragraph(data.End_Date.ToString("dd/MM/yyyy"))));

                            cells.Add(new Cell(1, 1)
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetFontSize(8)
                                .Add(new Paragraph($"{data.Interest_Rate}%")));

                            cells.Add(new Cell(1, 1)
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetFontSize(8)
                                .Add(new Paragraph($"${data.Interest_Payable}")));

                            cells.Add(new Cell(1, 1)
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetFontSize(8)
                                .Add(new Paragraph($"${data.Quantity_Interest_Arrears}")));

                            cells.Add(new Cell(1, 1)
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetFontSize(8)
                                .Add(new Paragraph(data.Pay_Day_Limit.ToString("dd/MM/yyyy"))));

                            cells.Add(new Cell(1, 1)
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetFontSize(8)
                                .Add(new Paragraph(data.Enabled == true ? "Activo" : "No Activo")));

                            cells.Add(new Cell(1, 1)
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetFontSize(8)
                                .Add(new Paragraph(data.Principal_Payment?? string.Empty)));
                        });
                    }
                    else
                    {
                       Utilities.GenerateEmptyCell(9, cells);
                    }
                }


                cells.ForEach(p =>
                {
                    table.AddCell(p);
                });

                Paragraph totalText = new Paragraph($"Monto a pagar en fecha próxima ${total}")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(14);

                document.Add(header);
                document.Add(table);
                document.Add(totalText);
                document.Close();

                return memoryStream;
            }
        }

        
    }
}

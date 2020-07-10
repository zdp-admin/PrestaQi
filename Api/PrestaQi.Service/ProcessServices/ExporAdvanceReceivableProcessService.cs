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

namespace PrestaQi.Service.ProcessServices
{
    public class ExporAdvanceReceivableProcessService : ProcessService<ExportAdvanceReceivable>
    {
        IRetrieveService<Configuration> _ConfigurationRetrieveService;

        public ExporAdvanceReceivableProcessService(
            IRetrieveService<Configuration> configurationRetrieveService
            )
        {
            this._ConfigurationRetrieveService = configurationRetrieveService;
        }

        public MemoryStream ExecuteProcess(ExportAdvanceReceivable export)
        {
            var configuration = this._ConfigurationRetrieveService.Where(p => p.Configuration_Name == "COLUMNS_ADVANCERECEIVABLE_EXPORT").FirstOrDefault();
            List<string> columns = configuration.Configuration_Value.Split('|').ToList();

            return export.Type == 1 ?
                GenerateExcel(export.AdvanceReceivables, columns) : 
                GeneratePdf(export.AdvanceReceivables, columns);
        }

        MemoryStream GenerateExcel(List<AdvanceReceivable> data, List<string> columns)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                var workBook = new XLWorkbook();
                var workSheet = workBook.Worksheets.Add("Data");

                var range = workSheet.Range(1, 1, 1, columns.Count);
                range.Merge();
                range.Value = "Listado de Adelantos Por Cobrar";
                range.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                range.Style.Font.Bold = true;
                range.Style.Font.SetFontSize(20);

                columns.ForEach((item, index) =>
                {
                    workSheet.Cell(3, index + 1).Value = item;
                    workSheet.Cell(3, index + 1).Style.Font.Bold = true;
                });

                int row = 4;

                foreach (var item in data)
                {
                    workSheet.Cell(row, 1).Value = item.Company;
                    workSheet.Cell(row, 2).Value = item.Contract_Number;
                    var rangeCompany = workSheet.Range(row, 1, row, columns.Count);
                    rangeCompany.Style.Fill.BackgroundColor = XLColor.Gray;
                    rangeCompany.Style.Font.FontColor = XLColor.White;
                    
                    item.Accrediteds.ForEach(p =>
                    {
                        row += 1;

                        workSheet.Cell(row, 3).Value = p.Id;
                        workSheet.Cell(row, 4).Value = p.NameComplete;
                        workSheet.Cell(row, 5).Value = p.Amount.ToString("C");
                        workSheet.Cell(row, 6).Value = p.Date_Advance.ToString("dd/MM/yyyy");
                        workSheet.Cell(row, 7).Value = p.Requested_Day;
                        workSheet.Cell(row, 8).Value = $"{p.Interest_Rate}%";
                        workSheet.Cell(row, 9).Value = p.Comission;
                        workSheet.Cell(row, 10).Value = p.Payment.ToString("C");
                    });

                    row += 1;

                    var rangeTotal = workSheet.Range(row, 1, row, 9);
                    rangeTotal.Merge();
                    rangeTotal.Value = $"Total a Cobrar {item.Contract_Number}:";
                    rangeTotal.Style.Font.Bold = true;

                    workSheet.Cell(row, 10).Value = item.Amount.ToString("C");
                    workSheet.Cell(row, 10).Style.Font.Bold = true;

                    row += 1;
                }

              

                workBook.SaveAs(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);

                return memoryStream;
            }
        }

        MemoryStream GeneratePdf(List<AdvanceReceivable> data, List<string> columns)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                PdfWriter pdfWriter = new PdfWriter(memoryStream);
                PdfDocument pdfDocument = new PdfDocument(pdfWriter);
                pdfDocument.SetDefaultPageSize(PageSize.LEGAL.Rotate());
                Document document = new Document(pdfDocument);
                Paragraph header = new Paragraph("Listado de Adelantos Por Cobrar")
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
                      .SetBold()
                      .Add(new Paragraph(item.Company)));

                    cells.Add(new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .SetBold()
                      .Add(new Paragraph(item.Contract_Number)));

                    Utilities.GenerateEmptyCell(8, cells);

                    item.Accrediteds.ForEach(p =>
                    {
                        Utilities.GenerateEmptyCell(2, cells);

                        cells.Add(new Cell(1, 1)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetFontSize(8)
                            .Add(new Paragraph(p.Id)));

                        cells.Add(new Cell(1, 1)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetFontSize(8)
                            .Add(new Paragraph(p.NameComplete)));

                        cells.Add(new Cell(1, 1)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetFontSize(8)
                            .Add(new Paragraph(p.Amount.ToString("C"))));

                        cells.Add(new Cell(1, 1)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetFontSize(8)
                            .Add(new Paragraph(p.Date_Advance.ToString("dd/MM/yyyy"))));

                        cells.Add(new Cell(1, 1)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetFontSize(8)
                            .Add(new Paragraph(p.Requested_Day.ToString())));

                        cells.Add(new Cell(1, 1)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetFontSize(8)
                            .Add(new Paragraph($"{p.Interest_Rate}%")));

                        cells.Add(new Cell(1, 1)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetFontSize(8)
                            .Add(new Paragraph(p.Comission.ToString())));

                        cells.Add(new Cell(1, 1)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetFontSize(8)
                            .Add(new Paragraph(p.Payment.ToString("C"))));
                    });

                    cells.Add(new Cell(1, 9)
                        .SetTextAlignment(TextAlignment.LEFT)
                        .SetFontSize(8)
                        .SetBold()
                        .Add(new Paragraph($"Total a Cobrar {item.Company}:")));

                    cells.Add(new Cell(1, 1)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetFontSize(8)
                        .SetBold()
                        .Add(new Paragraph(item.Amount.ToString("C"))));
                }


                cells.ForEach(p =>
                {
                    table.AddCell(p);
                });


                document.Add(header);
                document.Add(table);
                document.Close();

                return memoryStream;
            }
        }
    }
}

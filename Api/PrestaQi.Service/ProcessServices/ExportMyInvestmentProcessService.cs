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
    public class ExportMyInvestmentProcessService : ProcessService<ExportMyInvestment>
    {
        IRetrieveService<Configuration> _ConfigurationRetrieveService;

        public ExportMyInvestmentProcessService(
            IRetrieveService<Configuration> configurationRetrieveService
            )
        {
            this._ConfigurationRetrieveService = configurationRetrieveService;
        }

        public MemoryStream ExecuteProcess(ExportMyInvestment export)
        {
            var configuration = this._ConfigurationRetrieveService.Where(p => p.Configuration_Name == "COLUMNS_MYINVESTMEN_EXPORT").FirstOrDefault();
            List<string> columns = configuration.Configuration_Value.Split('|').ToList();

            return export.Type == 1 ?
                GenerateExcel(export.MyInvestments, columns) : 
                GeneratePdf(export.MyInvestments, columns);
        }

        MemoryStream GenerateExcel(List<MyInvestment> data, List<string> columns)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                var workBook = new XLWorkbook();
                var workSheet = workBook.Worksheets.Add("Data");

                var range = workSheet.Range(1, 1, 1, columns.Count);
                range.Merge();
                range.Value = "Mis Inversiones";
                range.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                range.Style.Font.Bold = true;
                range.Style.Font.SetFontSize(20);

                columns.ForEach((item, index) =>
                {
                    workSheet.Cell(3, index + 1).Value = item;
                    workSheet.Cell(3, index + 1).Style.Font.Bold = true;
                });

                int row = 4;

                int cont = 1;
                foreach (var item in data)
                {
                    workSheet.Cell(row, 1).Value = $"Inversión {item.Capital_ID}";
                    workSheet.Cell(row, 2).Value = item.Amount.ToString("C");
                    workSheet.Cell(row, 3).Value = $"{item.Interest_Rate}%";
                    workSheet.Cell(row, 4).Value = item.Annual_Interest_Payment.ToString("C");
                    workSheet.Cell(row, 5).Value = item.Total.ToString("C");
                    workSheet.Cell(row, 6).Value = item.Start_Date.ToString("dd/MM/yyyy");
                    workSheet.Cell(row, 7).Value = item.End_Date.ToString("dd/MM/yyyy");
                    workSheet.Cell(row, 8).Value = item.Enabled;

                    cont += 1;
                    row += 1;
                }



                workBook.SaveAs(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);

                return memoryStream;
            }
        }

        MemoryStream GeneratePdf(List<MyInvestment> data, List<string> columns)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                PdfWriter pdfWriter = new PdfWriter(memoryStream);
                PdfDocument pdfDocument = new PdfDocument(pdfWriter);
                pdfDocument.SetDefaultPageSize(PageSize.LEGAL.Rotate());
                Document document = new Document(pdfDocument);
                Paragraph header = new Paragraph("Mis Inversiones")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(20);

                List<Cell> cells = new List<Cell>();

                Table table = new Table(columns.Count, false);

                columns.ForEach(p =>
                {
                    Cell cell = new Cell(1, 1)
                      .SetBackgroundColor(ColorConstants.GRAY)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .Add(new Paragraph(p));

                    cells.Add(cell);
                });

                int cont = 1;
                foreach (var item in data)
                {
                    cells.Add(new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .SetBold()
                      .Add(new Paragraph($"Inversión {cont}")));

                    cells.Add(new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .SetBold()
                      .Add(new Paragraph(item.Amount.ToString("C"))));

                    cells.Add(new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .SetBold()
                      .Add(new Paragraph($"{item.Interest_Rate}%")));

                    cells.Add(new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .SetBold()
                      .Add(new Paragraph(item.Annual_Interest_Payment.ToString("C"))));

                    cells.Add(new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .SetBold()
                      .Add(new Paragraph(item.Total.ToString("C"))));

                    cells.Add(new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .SetBold()
                      .Add(new Paragraph(item.Start_Date.ToString("dd/MM/yyyy"))));

                    cells.Add(new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .SetBold()
                      .Add(new Paragraph(item.End_Date.ToString("dd/MM/yyyy"))));

                    cells.Add(new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .SetBold()
                      .Add(new Paragraph(item.Enabled)));

                    cont += 1;
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

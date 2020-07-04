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
using DocumentFormat.OpenXml.Office.CustomUI;

namespace PrestaQi.Service.ProcessServices
{
    public class ExportCapitalDetailProcessService : ProcessService<ExportCapitalDetail>
    {
        IRetrieveService<Configuration> _ConfigurationRetrieveService;

        public ExportCapitalDetailProcessService(
            IRetrieveService<Configuration> configurationRetrieveService
            )
        {
            this._ConfigurationRetrieveService = configurationRetrieveService;
        }

        public MemoryStream ExecuteProcess(ExportCapitalDetail export)
        {
            var configuration = this._ConfigurationRetrieveService.Where(p => p.Configuration_Name == "COLUMN_CAPITALDETAIL_EXPORT1").FirstOrDefault();
            List<string> columns = configuration.Configuration_Value.Split('|').ToList();

            return export.Type == 1 ?
                GenerateExcel(export, columns) : 
                GeneratePdf(export, columns);
        }

        MemoryStream GenerateExcel(ExportCapitalDetail export, List<string> columns)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                var workBook = new XLWorkbook();
                var workSheet = workBook.Worksheets.Add("Data");

                var rangeTitle = workSheet.Range(1, 1, 1, columns.Count);
                rangeTitle.Merge();
                rangeTitle.Value = $"Estado de Cuenta";
                rangeTitle.Style.Font.Bold = true;
                rangeTitle.Style.Font.SetFontSize(20);

                var rangeName = workSheet.Range(3, 1, 3, columns.Count);
                rangeName.Merge();
                rangeName.Value = $"Inversionista: {export.Name}    Tasa de Interés: {export.Interest_Rate}%";
                rangeName.Style.Font.SetFontSize(12);
                rangeName.Style.Font.SetFontColor(XLColor.Gray);

                columns.ForEach((item, index) =>
                {
                    workSheet.Cell(5, index + 1).Value = item;
                    workSheet.Cell(5, index + 1).Style.Font.Bold = true;
                });

                int row = 6;

                foreach (var item in export.CapitalDetails)
                {
                    workSheet.Cell(row, 1).Value = item.Period;
                    workSheet.Cell(row, 2).Value = item.Start_Date.ToString("dd/MM/yyyy");
                    workSheet.Cell(row, 3).Value = item.Pay_Day_Limit.ToString("dd/MM/yyyy");
                    workSheet.Cell(row, 4).Value = $"{item.Outstanding_Balance:C}";
                    workSheet.Cell(row, 5).Value = $"{item.Principal_Payment:C}";
                    workSheet.Cell(row, 6).Value = $"{item.Interest_Payment:C}";
                    workSheet.Cell(row, 7).Value = $"{item.Vat:C}";
                    workSheet.Cell(row, 8).Value = $"{item.Vat_Retention:C}";
                    workSheet.Cell(row, 9).Value = $"{item.Isr_Retention:C}";
                    workSheet.Cell(row, 10).Value = $"{item.Payment:C}";
                    row += 1;
                }

                var rangeTota1 = workSheet.Range(row, 1, row, 4);
                rangeTota1.Merge();
                rangeTota1.Value = "Pago Total";
                rangeTota1.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                rangeTota1.Style.Font.Bold = true;

                workSheet.Cell(row, 5).Value = $"{export.CapitalDetails.Sum(p => p.Principal_Payment):C}";
                workSheet.Cell(row, 5).Style.Font.Bold = true;
                workSheet.Cell(row, 6).Value = $"{export.CapitalDetails.Sum(p => p.Interest_Payment):C}";
                workSheet.Cell(row, 6).Style.Font.Bold = true;
                workSheet.Cell(row, 7).Value = $"{export.CapitalDetails.Sum(p => p.Vat):C}";
                workSheet.Cell(row, 7).Style.Font.Bold = true;
                workSheet.Cell(row, 8).Value = $"{export.CapitalDetails.Sum(p => p.Vat_Retention):C}";
                workSheet.Cell(row, 8).Style.Font.Bold = true;
                workSheet.Cell(row, 9).Value = $"{export.CapitalDetails.Sum(p => p.Isr_Retention):C}";
                workSheet.Cell(row, 9).Style.Font.Bold = true;
                workSheet.Cell(row, 10).Value = $"{export.CapitalDetails.Sum(p => p.Payment):C}";
                workSheet.Cell(row, 10).Style.Font.Bold = true;

                workBook.SaveAs(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);

                return memoryStream;
            }
        }

        MemoryStream GeneratePdf(ExportCapitalDetail export, List<string> columns)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                PdfWriter pdfWriter = new PdfWriter(memoryStream);
                PdfDocument pdfDocument = new PdfDocument(pdfWriter);
                pdfDocument.SetDefaultPageSize(PageSize.LETTER.Rotate());
                Document document = new Document(pdfDocument);
                Paragraph header = new Paragraph("Estado de cuenta")
                    .SetTextAlignment(TextAlignment.LEFT)
                    .SetFontSize(20);

                /*Paragraph name = new Paragraph($"{accredited.First_Name} {accredited.Last_Name}")
                    .SetTextAlignment(TextAlignment.LEFT)
                    .SetFontSize(14);*/

                Paragraph company = new Paragraph($"Inversionista: {export.Name}     Tasa de interés: {export.Interest_Rate}%")
                    .SetTextAlignment(TextAlignment.LEFT)
                    .SetFontSize(12);

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

                foreach (var item in export.CapitalDetails)
                {
                    cells.Add(new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .Add(new Paragraph(item.Period.ToString())));

                    cells.Add(new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .Add(new Paragraph(item.Start_Date.ToString("dd/MM/yyyy"))));

                    cells.Add(new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .Add(new Paragraph(item.Pay_Day_Limit.ToString("dd/MM/yyyy"))));

                    cells.Add(new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .Add(new Paragraph($"{item.Outstanding_Balance:C}")));

                    cells.Add(new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .Add(new Paragraph($"{item.Principal_Payment:C}")));

                    cells.Add(new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .Add(new Paragraph($"{item.Interest_Payment:C}")));

                    cells.Add(new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .Add(new Paragraph($"{item.Vat:C}")));

                    cells.Add(new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .Add(new Paragraph($"{item.Vat_Retention:C}")));

                    cells.Add(new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .Add(new Paragraph($"{item.Isr_Retention:C}")));

                    cells.Add(new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .Add(new Paragraph($"{item.Payment:C}")));
                }

                Utilities.GenerateEmptyCell(2, cells);
                cells.Add(new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .SetBold()
                      .Add(new Paragraph("Pago Total:")));
                Utilities.GenerateEmptyCell(1, cells);
                cells.Add(new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .SetBold()
                      .Add(new Paragraph(export.CapitalDetails.Sum(p => p.Principal_Payment).ToString("C"))));
                cells.Add(new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .SetBold()
                      .Add(new Paragraph(export.CapitalDetails.Sum(p => p.Interest_Payment).ToString("C"))));
                cells.Add(new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .SetBold()
                      .Add(new Paragraph(export.CapitalDetails.Sum(p => p.Vat).ToString("C"))));
                cells.Add(new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .SetBold()
                      .Add(new Paragraph(export.CapitalDetails.Sum(p => p.Vat_Retention).ToString("C"))));
                cells.Add(new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .SetBold()
                      .Add(new Paragraph(export.CapitalDetails.Sum(p => p.Isr_Retention).ToString("C"))));
                cells.Add(new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .SetBold()
                      .Add(new Paragraph(export.CapitalDetails.Sum(p => p.Payment).ToString("C"))));

                cells.ForEach(p =>
                {
                    table.AddCell(p);
                });

                document.Add(header);
                //document.Add(name);
                document.Add(company);
                document.Add(table);

                document.Close();


                return memoryStream;
            }
        }
    }
}

using ClosedXML.Excel;
using InsiscoCore.Base.Service;
using PrestaQi.Service.Tools;
using InsiscoCore.Service;
using PrestaQi.Model;
using PrestaQi.Model.Dto.Input;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using iText.Kernel.Colors;
using iText.Kernel.Geom;
using PrestaQi.Model.Enum;

namespace PrestaQi.Service.ProcessServices
{
    public class ExportAdvanceProcessService : ProcessService<ExportAdvance>
    {
        IRetrieveService<Configuration> _ConfigurationRetrieveService;

        public ExportAdvanceProcessService(
            IRetrieveService<Configuration> configurationRetrieveService
            )
        {
            this._ConfigurationRetrieveService = configurationRetrieveService;
        }

        public MemoryStream ExecuteProcess(ExportAdvance export)
        {
            var configuration = this._ConfigurationRetrieveService.Where(p => p.Configuration_Name == "COLUMNS_ADVANCE_EXPORT").FirstOrDefault();
            List<string> columns = configuration.Configuration_Value.Split('|').ToList();

            return export.Type == 1 ?
                GenerateExcel(export.Accredited, columns) : 
                GeneratePdf(export.Accredited, columns);
        }

        MemoryStream GenerateExcel(AccreditedExportData accredited, List<string> columns)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                var workBook = new XLWorkbook();
                var workSheet = workBook.Worksheets.Add("Data");

                var rangeId = workSheet.Range(1, 1, 1, columns.Count);
                rangeId.Value = $"ID {accredited.Identify}";
                rangeId.Style.Font.Bold = true;
                rangeId.Style.Font.SetFontSize(20);

                var rangeName = workSheet.Range(3, 1, 3, columns.Count);
                rangeName.Merge();
                rangeName.Value = $"{accredited.First_Name} {accredited.Last_Name}";
                rangeName.Style.Font.Bold = true;
                rangeName.Style.Font.SetFontSize(14);

                var rangeCompany = workSheet.Range(4, 1, 4, columns.Count);
                rangeCompany.Merge();
                rangeCompany.Value = $"{accredited.Company_Name} | {accredited.Contract_Number}";
                rangeCompany.Style.Font.SetFontSize(12);
                rangeCompany.Style.Font.SetFontColor(XLColor.Gray);

                columns.ForEach((item, index) =>
                {
                    workSheet.Cell(6, index + 1).Value = item;
                    workSheet.Cell(6, index + 1).Style.Font.Bold = true;
                });

                int row = 7;

                foreach (var item in accredited.Advances)
                {
                    workSheet.Cell(row, 1).Value = $"{item.Amount:C}";
                    workSheet.Cell(row, 2).Value = item.Date_Advance.ToString("dd/MM/yyyy");
                    workSheet.Cell(row, 3).Value = item.Requested_Day;
                    workSheet.Cell(row, 4).Value = $"{accredited.Interest_Rate}%";
                    workSheet.Cell(row, 5).Value = item.Comission;
                    workSheet.Cell(row, 6).Value = $"{item.Total_Withhold:C}";
                    workSheet.Cell(row, 7).Value = item.Paid_Status == (int)PrestaQiEnum.AdvanceStatus.Pagado ? "No Activo" : "Activo";

                    row += 1;
                }

                workBook.SaveAs(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);

                return memoryStream;
            }
        }

        MemoryStream GeneratePdf(AccreditedExportData accredited, List<string> columns)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                PdfWriter pdfWriter = new PdfWriter(memoryStream);
                PdfDocument pdfDocument = new PdfDocument(pdfWriter);
                pdfDocument.SetDefaultPageSize(PageSize.LETTER);
                Document document = new Document(pdfDocument);
                Paragraph header = new Paragraph($"ID {accredited.Identify}")
                    .SetTextAlignment(TextAlignment.LEFT)
                    .SetFontSize(20);

                Paragraph name = new Paragraph($"{accredited.First_Name} {accredited.Last_Name}")
                    .SetTextAlignment(TextAlignment.LEFT)
                    .SetFontSize(14);
                
                Paragraph company = new Paragraph($"{accredited.Company_Name} | {accredited.Contract_Number}")
                    .SetTextAlignment(TextAlignment.LEFT)
                    .SetFontSize(12);

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

                foreach (var item in accredited.Advances)
                {
                    cells.Add(new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .Add(new Paragraph($"${item.Amount}")));

                    cells.Add(new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .Add(new Paragraph(item.Date_Advance.ToString("dd/MM/yyyy"))));

                    cells.Add(new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .Add(new Paragraph(item.Requested_Day.ToString())));

                    cells.Add(new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .Add(new Paragraph($"{accredited.Interest_Rate}%")));

                    cells.Add(new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .Add(new Paragraph(item.Comission.ToString())));

                    cells.Add(new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .Add(new Paragraph($"{item.Total_Withhold:C}")));

                    cells.Add(new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .Add(new Paragraph(item.Paid_Status == (int)PrestaQiEnum.AdvanceStatus.Pagado ? "No Activo" : "Activo")));

                    
                }

                cells.ForEach(p =>
                {
                    table.AddCell(p);
                });

                document.Add(header);
                document.Add(name);
                document.Add(company);
                document.Add(table);

                document.Close();


                return memoryStream;
            }
        }
    }
}

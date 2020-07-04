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

namespace PrestaQi.Service.ProcessServices
{
    public class ExportFileProcessService : ProcessService<ExportFile>
    {
        IRetrieveService<Configuration> _ConfigurationRetrieveService;
        IRetrieveService<Gender> _GenderRetrieveService;

        public ExportFileProcessService(
            IRetrieveService<Configuration> configurationRetrieveService,
            IRetrieveService<Gender> genderRetrieveService
            )
        {
            this._ConfigurationRetrieveService = configurationRetrieveService;
            this._GenderRetrieveService = genderRetrieveService;
        }

        public MemoryStream ExecuteProcess(ExportInvestor exportInvestor)
        {
            var configuration = this._ConfigurationRetrieveService.Where(p => p.Configuration_Name == "COLUMNS_INVESTOR_EXPORT").FirstOrDefault();
            List<string> columns = configuration.Configuration_Value.Split('|').ToList();

            return exportInvestor.Type == 1 ? 
                GenerateExcelInvestor(exportInvestor.InvestorDatas, columns) : 
                GeneratePdfInvestor(exportInvestor.InvestorDatas, columns);
        }

        MemoryStream GenerateExcelInvestor(List<InvestorData> investorDatas, List<string> columns)
        {
            
            using (MemoryStream memoryStream = new MemoryStream())
            {
                var workBook = new XLWorkbook();
                var workSheet = workBook.Worksheets.Add("Data");

                var range = workSheet.Range(1, 1, 1, columns.Count);
                range.Merge();
                range.Value = "Listado de Inversionistas";
                range.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                range.Style.Font.Bold = true;
                range.Style.Font.SetFontSize(20);

                columns.ForEach((item, index) =>
                {
                    workSheet.Cell(3, index + 1).Value = item;
                    workSheet.Cell(3, index + 1).Style.Font.Bold = true;
                });

                int row = 4;

                foreach (var item in investorDatas)
                {
                    workSheet.Cell(row, 1).Value = item.Id;
                    workSheet.Cell(row, 2).Value = item.NameComplete;
                    workSheet.Cell(row, 3).Value = item.Limit_Date.ToString("dd/MM/yyyy");
                    workSheet.Cell(row, 4).Value = $"${item.Commited_Amount}";
                    workSheet.Cell(row, 5).Value = $"${item.AmountExercised}";

                    if (item.CapitalDatas.Count == 0)
                        row += 1;

                    item.CapitalDatas.ForEach(p =>
                    {
                        workSheet.Cell(row, 6).Value = $"{p.Interest_Rate}%";
                        workSheet.Cell(row, 7).Value = $"${p.Amount}";
                        workSheet.Cell(row, 8).Value = p.Start_Date.ToString("dd/MM/yyyy");
                        workSheet.Cell(row, 9).Value = p.End_Date.ToString("dd/MM/yyyy");
                        workSheet.Cell(row, 10).Value = p.Capital_Status;
                        workSheet.Cell(row, 11).Value = p.File ?? string.Empty;
                        workSheet.Cell(row, 12).Value = p.Investment_Status;

                        row += 1;
                    });

                }

                workBook.SaveAs(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);

                return memoryStream;
            }
        }

        MemoryStream GeneratePdfInvestor(List<InvestorData> investorDatas, List<string> columns)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                PdfWriter pdfWriter = new PdfWriter(memoryStream);
                PdfDocument pdfDocument = new PdfDocument(pdfWriter);
                pdfDocument.SetDefaultPageSize(PageSize.LEGAL.Rotate());
                Document document = new Document(pdfDocument);
                Paragraph header = new Paragraph("Listado de Inversionistas")
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

                foreach (var item in investorDatas)
                {
                    cells.Add(new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .Add(new Paragraph(item.Id.ToString())));

                    cells.Add(new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .Add(new Paragraph(item.NameComplete)));


                    cells.Add(new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .Add(new Paragraph(item.Limit_Date.ToString("dd/MM/yyyy"))));

                    cells.Add(new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .Add(new Paragraph($"${item.Commited_Amount}")));

                    cells.Add(new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .Add(new Paragraph($"${item.AmountExercised}")));

                    if (item.CapitalDatas.Count > 0)
                    {
                        item.CapitalDatas.ForEach((data, index) =>
                        {
                            if (index != 0)
                            {
                                Utilities.GenerateEmptyCell(5, cells);
                               
                            }

                            cells.Add(new Cell(1, 1)
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetFontSize(8)
                                .Add(new Paragraph($"{data.Interest_Rate}%")));

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
                                .Add(new Paragraph(data.Capital_Status)));

                            cells.Add(new Cell(1, 1)
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetFontSize(8)
                                .Add(new Paragraph(data.File ?? string.Empty)));

                            cells.Add(new Cell(1, 1)
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetFontSize(8)
                                .Add(new Paragraph(data.Investment_Status)));
                        });
                    }
                    else
                    {
                        Utilities.GenerateEmptyCell(7, cells);
                    }
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

        public MemoryStream ExecuteProcess(ExportAccredited exportAccredited)
        {
            var configuration = this._ConfigurationRetrieveService.Where(p => p.Configuration_Name == "COLUMNS_ACCREDITED_EXPORT").FirstOrDefault();
            var genders = this._GenderRetrieveService.Where(p => p.Enabled == true).ToList();
            List<string> columns = configuration.Configuration_Value.Split('|').ToList();

            return exportAccredited.Type == 1 ?
                GenerateExcelAccredited(exportAccredited.Accrediteds, columns, genders) :
                GeneratePdfAccredited(exportAccredited.Accrediteds, columns, genders);
        }

        MemoryStream GenerateExcelAccredited(List<Accredited> accrediteds, List<string> columns, List<Gender> genders)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                var workBook = new XLWorkbook();
                var workSheet = workBook.Worksheets.Add("Data");

                var range = workSheet.Range(1, 1, 1, columns.Count);
                range.Merge();
                range.Value = "Listado de Acreditados";
                range.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                range.Style.Font.Bold = true;
                range.Style.Font.SetFontSize(20);

                columns.ForEach((item, index) =>
                {
                    workSheet.Cell(3, index + 1).Value = item;
                    workSheet.Cell(3, index + 1).Style.Font.Bold = true;
                });

                int row = 4;

                foreach (var item in accrediteds)
                {
                    workSheet.Cell(row, 1).Value = item.Identify;
                    workSheet.Cell(row, 2).Value = item.Company_Name;
                    workSheet.Cell(row, 3).Value = item.Contract_number;
                    workSheet.Cell(row, 4).Value = $"{item.First_Name} {item.Last_Name}";
                    workSheet.Cell(row, 5).Value = $"${item.Net_Monthly_Salary}";
                    workSheet.Cell(row, 6).Value = $"{item.Interest_Rate}%";
                    workSheet.Cell(row, 7).Value = item.Rfc;
                    workSheet.Cell(row, 8).Value = item.Institution_Name;
                    workSheet.Cell(row, 9).Value = item.Clabe;
                    workSheet.Cell(row, 10).Value = item.Account_Number;
                    workSheet.Cell(row, 11).Value = item.Birth_Date.ToString("dd/MM/yyyy");
                    workSheet.Cell(row, 12).Value = item.Age;
                    workSheet.Cell(row, 13).Value = item.Position;
                    workSheet.Cell(row, 14).Value = genders.Find(p => p.id == item.Gender_Id).Description;
                    workSheet.Cell(row, 15).Value = item.Seniority_Company;

                    row += 1;
                }

                workBook.SaveAs(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);

                return memoryStream;
            }
        }

        MemoryStream GeneratePdfAccredited(List<Accredited> accrediteds, List<string> columns, List<Gender> genders)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                PdfWriter pdfWriter = new PdfWriter(memoryStream);
                PdfDocument pdfDocument = new PdfDocument(pdfWriter);
                pdfDocument.SetDefaultPageSize(PageSize.LEGAL.Rotate());
                Document document = new Document(pdfDocument);
                Paragraph header = new Paragraph("Listado de Inversionistas")
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

                foreach (var item in accrediteds)
                {
                    cells.Add(new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .Add(new Paragraph(item.Identify.ToString())));

                    cells.Add(new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .Add(new Paragraph(item.Company_Name)));

                    cells.Add(new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .Add(new Paragraph(item.Contract_number)));

                    cells.Add(new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .Add(new Paragraph($"{item.First_Name } {item.Last_Name}")));

                    cells.Add(new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .Add(new Paragraph($"${item.Net_Monthly_Salary}")));

                    cells.Add(new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .Add(new Paragraph($"{item.Interest_Rate}%")));

                    cells.Add(new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .Add(new Paragraph(item.Rfc)));

                    cells.Add(new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .Add(new Paragraph(item.Institution_Name)));

                    cells.Add(new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .Add(new Paragraph(item.Clabe)));

                    cells.Add(new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .Add(new Paragraph(item.Account_Number)));

                    cells.Add(new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .Add(new Paragraph(item.Birth_Date.ToString("dd/MM/yyyy"))));

                    cells.Add(new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .Add(new Paragraph(item.Age.ToString())));

                    cells.Add(new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .Add(new Paragraph(item.Position)));

                    cells.Add(new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .Add(new Paragraph(genders.Find(p => p.id == item.Gender_Id).Description)));

                    cells.Add(new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetFontSize(8)
                      .Add(new Paragraph(item.Seniority_Company.ToString())));
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

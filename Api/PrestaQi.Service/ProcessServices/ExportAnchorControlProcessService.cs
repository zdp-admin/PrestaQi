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

                List<DateTime> dateTimes = new List<DateTime>();

                data.ForEach(investor =>
                {
                    investor.MyInvestments.ForEach(p =>
                    {
                        dateTimes.Add(p.Pay_Day_Limit);
                    });
                });

                var nextDate = dateTimes.Where(p => p.Date > DateTime.Now.Date).OrderBy(p => p).FirstOrDefault();
                
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

                double sumInterest = 0, vat = 0, vatRetention = 0, isrRetention = 0, netInterest = 0, principalPayment = 0;
                double sumMoratorium = 0, moratoriumVat = 0, moratoriumVatRetention = 0, moratoriumIsrRetention = 0, moratoriumNetInterest = 0;
                foreach (var item in data)
                {
                    workSheet.Cell(row, 1).Value = item.Investor_Id;
                    workSheet.Cell(row, 2).Value = item.Name_Complete;

                    if (item.MyInvestments.Count == 0)
                        row += 1;

                    item.MyInvestments.ForEach(p =>
                    {
                        workSheet.Cell(row, 3).Value = $"{p.Amount:C}";
                        workSheet.Cell(row, 4).Value = p.Start_Date.ToString("dd/MM/yyyy");
                        workSheet.Cell(row, 5).Value = p.End_Date.ToString("dd/MM/yyyy");
                        workSheet.Cell(row, 6).Value = $"{p.Interest_Rate}%";
                        workSheet.Cell(row, 7).Value = $"{p.Interest_Arrears}%";
                        workSheet.Cell(row, 8).Value = p.Period_Name;
                        workSheet.Cell(row, 9).Value = p.Interest_Payable.ToString("C");
                        workSheet.Cell(row, 10).Value = p.Quantity_Interest_Arrears.ToString("C");
                        workSheet.Cell(row, 11).Value = p.Promotional_Setting.ToString("C");
                        workSheet.Cell(row, 12).Value = p.Total_Interest.ToString("C");
                        workSheet.Cell(row, 13).Value = p.Vat.ToString("C");
                        workSheet.Cell(row, 14).Value = p.Vat_Retention.ToString("C");
                        workSheet.Cell(row, 15).Value = p.Isr_Retention.ToString("C");
                        workSheet.Cell(row, 16).Value = p.Net_Interest.ToString("C");
                        workSheet.Cell(row, 17).Value = p.Day_Overdue;
                        workSheet.Cell(row, 18).Value = p.Pay_Day_Limit.ToString("dd/MM/yyyy");
                        workSheet.Cell(row, 19).Value = p.Enabled;
                        workSheet.Cell(row, 20).Value = p.Principal_Payment;

                        row += 1;
                        if (p.Quantity_Interest_Arrears > 0)
                        {
                            sumMoratorium += p.Interest_Payable;
                            moratoriumVat = p.Vat;
                            moratoriumVatRetention = p.Vat_Retention;
                            moratoriumIsrRetention = p.Isr_Retention;
                            moratoriumNetInterest = p.Net_Interest;
                        }
                        
                        if (p.Enabled == "Activo")
                        {
                            sumInterest += p.Interest_Payable;
                            vat += p.Vat;
                            vatRetention += p.Vat_Retention;
                            isrRetention += p.Isr_Retention;
                            netInterest += p.Net_Interest;
                            double principal = 0;
                            double.TryParse(p.Principal_Payment, out principal);

                            principalPayment += principal;
                        }
                    });
                }

                row += 3;

                var rangeInterest = workSheet.Range(row, 4, row, 8);
                rangeInterest.Merge();
                rangeInterest.Value = "Subtotal a Pagaren la Fecha Próxima";
                workSheet.Cell(row, 9).Value = sumInterest.ToString("C");
                row += 1;
                var rangeVat = workSheet.Range(row, 4, row, 8);
                rangeVat.Merge();
                rangeVat.Value = "IVA";
                workSheet.Cell(row, 9).Value = vat.ToString("C");
                row += 1;
                var rangeVatRetention = workSheet.Range(row, 4, row, 8);
                rangeVatRetention.Merge();
                rangeVatRetention.Value = "Retención de IVA";
                workSheet.Cell(row, 9).Value = vatRetention.ToString("C");
                row += 1;
                var rangeIsrRetention = workSheet.Range(row, 4, row, 8);
                rangeIsrRetention.Merge();
                rangeIsrRetention.Value = "Retención de ISR";
                workSheet.Cell(row, 9).Value = isrRetention.ToString("C");
                row += 1;
                var rangeNetInterest = workSheet.Range(row, 4, row, 8);
                rangeNetInterest.Merge();
                rangeNetInterest.Value = "Intereses netos por Pagar";
                workSheet.Cell(row, 9).Value = netInterest.ToString("C");
                row += 1;
                var rangePrincipal = workSheet.Range(row, 4, row, 8);
                rangePrincipal.Merge();
                rangePrincipal.Value = "Principal por Pagar";
                workSheet.Cell(row, 9).Value = principalPayment.ToString("C");
                row += 1;
                var rangeTotalPayment = workSheet.Range(row, 4, row, 8);
                rangeTotalPayment.Merge();
                rangeTotalPayment.Value = "Total a Pagar del Periodo";
                workSheet.Cell(row, 9).Value = (netInterest + principalPayment).ToString("C");

                row += 2;

                var rangeMoratorium = workSheet.Range(row, 4, row, 8);
                rangeMoratorium.Merge();
                rangeMoratorium.Value = "Subtotal a Pagar en la Fecha Próxima";
                workSheet.Cell(row, 9).Value = sumMoratorium.ToString("C");
                row += 1;
                var rangeVatMoratorium = workSheet.Range(row, 4, row, 8);
                rangeVatMoratorium.Merge();
                rangeVatMoratorium.Value = "IVA";
                workSheet.Cell(row, 9).Value = moratoriumVat.ToString("C");
                row += 1;
                var rangeVatRetentionMoratorium = workSheet.Range(row, 4, row, 8);
                rangeVatRetentionMoratorium.Merge();
                rangeVatRetentionMoratorium.Value = "Retención de IVA";
                workSheet.Cell(row, 9).Value = moratoriumVatRetention.ToString("C");
                row += 1;
                var rangeIsrRetentionMoratorium = workSheet.Range(row, 4, row, 8);
                rangeIsrRetentionMoratorium.Merge();
                rangeIsrRetentionMoratorium.Value = "Retención de ISR";
                workSheet.Cell(row, 9).Value = moratoriumIsrRetention.ToString("C");
                row += 1;
                var rangeNetMoratorium = workSheet.Range(row, 4, row, 8);
                rangeNetMoratorium.Merge();
                rangeNetMoratorium.Value = "Intereses Netos por Pagar de Periodos Anteriores";
                workSheet.Cell(row, 9).Value = moratoriumNetInterest.ToString("C");

                row += 2;
                var rangeTotal = workSheet.Range(row, 4, row, 8);
                rangeTotal.Merge();
                rangeTotal.Value = "Deuda Total de Fondeo";
                workSheet.Cell(row, 9).Value = (netInterest + principalPayment + moratoriumNetInterest).ToString("C");

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

                double sumInterest = 0, vat = 0, vatRetention = 0, isrRetention = 0, netInterest = 0, principalPayment = 0;
                double sumMoratorium = 0, moratoriumVat = 0, moratoriumVatRetention = 0, moratoriumIsrRetention = 0, moratoriumNetInterest = 0;
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
                            if (data.Quantity_Interest_Arrears > 0)
                            {
                                sumMoratorium += data.Interest_Payable;
                                moratoriumVat = data.Vat;
                                moratoriumVatRetention = data.Vat_Retention;
                                moratoriumIsrRetention = data.Isr_Retention;
                                moratoriumNetInterest = data.Net_Interest;
                            }
                            
                            if (data.Enabled == "Activo")
                            {
                                sumInterest += data.Interest_Payable;
                                vat += data.Vat;
                                vatRetention += data.Vat_Retention;
                                isrRetention += data.Isr_Retention;
                                netInterest += data.Net_Interest;
                                double principal = 0;
                                double.TryParse(data.Principal_Payment, out principal);

                                principalPayment += principal;
                            }

                            if (index != 0)
                            {
                               Utilities.GenerateEmptyCell(2, cells);
                            }

                            cells.Add(new Cell(1, 1)
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetFontSize(8)
                                .Add(new Paragraph($"{data.Amount:C}")));

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
                                .Add(new Paragraph($"{data.Interest_Arrears}%")));

                            cells.Add(new Cell(1, 1)
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetFontSize(8)
                                .Add(new Paragraph(data.Period_Name)));

                            cells.Add(new Cell(1, 1)
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetFontSize(8)
                                .Add(new Paragraph(data.Interest_Payable.ToString("C"))));

                            cells.Add(new Cell(1, 1)
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetFontSize(8)
                                .Add(new Paragraph(data.Quantity_Interest_Arrears.ToString("C"))));

                            cells.Add(new Cell(1, 1)
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetFontSize(8)
                                .Add(new Paragraph(data.Promotional_Setting.ToString("C"))));

                            cells.Add(new Cell(1, 1)
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetFontSize(8)
                                .Add(new Paragraph(data.Total_Interest.ToString("C"))));

                            cells.Add(new Cell(1, 1)
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetFontSize(8)
                                .Add(new Paragraph(data.Vat.ToString("C"))));

                            cells.Add(new Cell(1, 1)
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetFontSize(8)
                                .Add(new Paragraph(data.Vat_Retention.ToString("C"))));

                            cells.Add(new Cell(1, 1)
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetFontSize(8)
                                .Add(new Paragraph(data.Isr_Retention.ToString("C"))));

                            cells.Add(new Cell(1, 1)
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetFontSize(8)
                                .Add(new Paragraph(data.Net_Interest.ToString("C"))));

                            cells.Add(new Cell(1, 1)
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetFontSize(8)
                                .Add(new Paragraph(data.Day_Overdue.ToString())));

                            cells.Add(new Cell(1, 1)
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetFontSize(8)
                                .Add(new Paragraph(data.Pay_Day_Limit.ToString("dd/MM/yyyy"))));

                            cells.Add(new Cell(1, 1)
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetFontSize(8)
                                .Add(new Paragraph(data.Enabled)));

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

                Table tableInterest = new Table(2, false);
                tableInterest.SetHorizontalAlignment(HorizontalAlignment.CENTER);
                List<Cell> cellsTotalInterest = new List<Cell>();
                Utilities.GenerateCell("Subtotal a Pagar en la Fecha Próxima", cellsTotalInterest, 8, TextAlignment.LEFT);
                Utilities.GenerateCell(sumInterest.ToString("C"), cellsTotalInterest, 8, TextAlignment.RIGHT);
                Utilities.GenerateCell("IVA", cellsTotalInterest, 8, TextAlignment.LEFT);
                Utilities.GenerateCell(vat.ToString("C"), cellsTotalInterest, 8, TextAlignment.RIGHT);
                Utilities.GenerateCell("Retención de IVA", cellsTotalInterest, 8, TextAlignment.LEFT);
                Utilities.GenerateCell(vatRetention.ToString("C"), cellsTotalInterest, 8, TextAlignment.RIGHT);
                Utilities.GenerateCell("Retención de ISR", cellsTotalInterest, 8, TextAlignment.LEFT);
                Utilities.GenerateCell(isrRetention.ToString("C"), cellsTotalInterest, 8, TextAlignment.RIGHT);
                Utilities.GenerateCell("Intereses Netos por Pagar", cellsTotalInterest, 8, TextAlignment.LEFT);
                Utilities.GenerateCell(netInterest.ToString("C"), cellsTotalInterest, 8, TextAlignment.RIGHT);
                Utilities.GenerateCell("Principal por Pagar", cellsTotalInterest, 8, TextAlignment.LEFT);
                Utilities.GenerateCell(principalPayment.ToString("C"), cellsTotalInterest, 8, TextAlignment.RIGHT);
                Utilities.GenerateCell("Total a Pagar del Periodo", cellsTotalInterest, 8, TextAlignment.LEFT);
                Utilities.GenerateCell((netInterest + principalPayment).ToString("C"), cellsTotalInterest, 8, TextAlignment.RIGHT);
                cellsTotalInterest.ForEach(p => { tableInterest.AddCell(p); });

                Table tableMoratorium = new Table(2, false);
                tableMoratorium.SetHorizontalAlignment(HorizontalAlignment.CENTER);
                List<Cell> cellsMoratorium = new List<Cell>();
                Utilities.GenerateCell("Subtotal a Pagar en la Fecha Próxima ", cellsMoratorium, 8, TextAlignment.LEFT);
                Utilities.GenerateCell(sumMoratorium.ToString("C"), cellsMoratorium, 8, TextAlignment.RIGHT);
                Utilities.GenerateCell("IVA", cellsMoratorium, 8, TextAlignment.LEFT);
                Utilities.GenerateCell(moratoriumVat.ToString("C"), cellsMoratorium, 8, TextAlignment.RIGHT);
                Utilities.GenerateCell("Retención de IVA", cellsMoratorium, 8, TextAlignment.LEFT);
                Utilities.GenerateCell(moratoriumVatRetention.ToString("C"), cellsMoratorium, 8, TextAlignment.RIGHT);
                Utilities.GenerateCell("Retención de ISR", cellsMoratorium, 8, TextAlignment.LEFT);
                Utilities.GenerateCell(moratoriumIsrRetention.ToString("C"), cellsMoratorium, 8, TextAlignment.RIGHT);
                Utilities.GenerateCell("Intereses Netos por Pagar de Periodos Anteriores", cellsMoratorium, 8, TextAlignment.LEFT);
                Utilities.GenerateCell(moratoriumNetInterest.ToString("C"), cellsMoratorium, 8, TextAlignment.RIGHT);
                cellsMoratorium.ForEach(p => { tableMoratorium.AddCell(p); });

                Table tableTotal = new Table(2, false);
                tableTotal.SetHorizontalAlignment(HorizontalAlignment.CENTER);
                List<Cell> cellsTotal = new List<Cell>();
                Utilities.GenerateCell("Dueda Total de Fondeo", cellsTotal, 8, TextAlignment.LEFT);
                Utilities.GenerateCell((netInterest + principalPayment + moratoriumNetInterest).ToString("C"), cellsTotal, 8, TextAlignment.RIGHT);
                cellsTotal.ForEach(p => { tableTotal.AddCell(p); });

                document.Add(header);
                document.Add(table);
                document.Add(new Paragraph(""));
                document.Add(tableInterest);
                document.Add(new Paragraph(""));
                document.Add(tableMoratorium);
                document.Add(new Paragraph(""));
                document.Add(tableTotal);
                document.Close();

                return memoryStream;
            }
        }

        
    }
}

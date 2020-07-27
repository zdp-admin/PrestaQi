using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using OpenXmlPowerTools;
using PrestaQi.Model;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Model.Dto.Output;
using PrestaQi.Model.Enum;
using PrestaQi.Service.Tools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PrestaQi.Service.ProcessServices
{
    public class CapitalProcessService : ProcessService<Capital>
    {
        IRetrieveService<Period> _PeriodRetrieveService;
        IRetrieveService<Capital> _CapitalRetrieveService;
        IRetrieveService<Configuration> _ConfigurationRetrieveService;
        IRetrieveService<Investor> _InvestorRetrieveService;
        IRetrieveService<CapitalDetail> _CapitalDetailRetrieveService;

        public CapitalProcessService(
            IRetrieveService<Period> periodRetrieveService,
            IRetrieveService<Capital> capitalRetrieveService,
            IRetrieveService<Configuration> configurationRetrieveService,
            IRetrieveService<Investor> investorRetrieveService,
            IRetrieveService<CapitalDetail> capitalDetailRetrieveService
            )
        {
            this._PeriodRetrieveService = periodRetrieveService;
            this._CapitalRetrieveService = capitalRetrieveService;
            this._ConfigurationRetrieveService = configurationRetrieveService;
            this._InvestorRetrieveService = investorRetrieveService;
            this._CapitalDetailRetrieveService = capitalDetailRetrieveService;
        }

        public MyInvestmentPagination ExecuteProcess(GetMyInvestment getInvestment)
        {
            var result = GenerateInvestments(getInvestment);
            return new MyInvestmentPagination() { TotalRecord = result.Item2, MyInvestments = result.Item1 };
        }

        public AnchorControlPagination ExecuteProcess(AnchorByFilter anchorByFilter)
        {
            List<Investor> investors = new List<Investor>();
            AnchorControlTotal anchorControlTotal = new AnchorControlTotal();
            double sumMoratorium = 0, moratoriumVat = 0, moratoriumVatRetention = 0, moratoriumIsrRetention = 0, moratoriumNetInterest = 0;
            double sumInterest = 0, vat = 0, vatRetention = 0, isrRetention = 0, netInterest = 0, principalPayment = 0;

            var listInvestorIds = this._CapitalRetrieveService.Where(p => p.Investment_Status == (int)PrestaQiEnum.InvestmentEnum.Activa).Select(p => p.investor_id).Distinct().ToList();

            if (anchorByFilter.Page == 0 || anchorByFilter.NumRecord == 0)
            {
                anchorByFilter.Page = 1;
                anchorByFilter.NumRecord = 20;
            }
            int totalRecord = 0;

            if (anchorByFilter.Start_Date != null && anchorByFilter.End_Date != null)
            {
                totalRecord = this._InvestorRetrieveService.Where(
                    p => p.Start_Date_Prestaqi.Date >= ((DateTime)anchorByFilter.Start_Date).Date &&
                    p.Start_Date_Prestaqi.Date <= ((DateTime)anchorByFilter.End_Date).Date && p.Deleted_At == null &&
                    listInvestorIds.Contains(p.id)
                    ).ToList().Count;

                investors = this._InvestorRetrieveService.Where(
                    p => p.Start_Date_Prestaqi.Date >= ((DateTime)anchorByFilter.Start_Date).Date &&
                    p.Start_Date_Prestaqi.Date <= ((DateTime)anchorByFilter.End_Date).Date
                    && p.Deleted_At == null && listInvestorIds.Contains(p.id))
                    .Skip((anchorByFilter.Page - 1) * anchorByFilter.NumRecord).Take(anchorByFilter.NumRecord).ToList();
            }
            else
            {
                if (anchorByFilter.Type == 0)
                {
                    if (string.IsNullOrEmpty(anchorByFilter.Filter))
                    {
                        totalRecord = this._InvestorRetrieveService.Where(p => p.Deleted_At == null && listInvestorIds.Contains(p.id)).ToList().Count;

                        investors = this._InvestorRetrieveService.Where(p => p.Deleted_At == null && listInvestorIds.Contains(p.id)).OrderBy(p => p.created_at)
                            .Skip((anchorByFilter.Page - 1) * anchorByFilter.NumRecord).Take(anchorByFilter.NumRecord)
                            .ToList();
                    }
                    else
                        investors = this._InvestorRetrieveService.Where(p => p.Deleted_At == null && listInvestorIds.Contains(p.id)).OrderBy(p => p.created_at).ToList();
                }
                else
                    investors = this._InvestorRetrieveService.Where(p => p.Deleted_At == null && listInvestorIds.Contains(p.id)).OrderBy(p => p.created_at).ToList();
            }

            List<AnchorControl> result = new List<AnchorControl>();

            if (investors.Count > 0)
            {
                result = investors.Select(p => new AnchorControl
                {
                    Investor_Id = p.id,
                    MyInvestments = GenerateInvestments(new GetMyInvestment() { Investor_Id = p.id }).Item1,
                    Name_Complete = $"{p.First_Name} {p.Last_Name}",
                }).ToList();

            }

            if (anchorByFilter.Type == 0 && anchorByFilter.Filter.Length > 0)
            {
                var stringProperties = typeof(AnchorControl).GetProperties().Where(prop =>
                    prop.PropertyType == anchorByFilter.Filter.GetType());

                totalRecord = result
                        .Where(p => stringProperties.Any(prop => prop.GetValue(p, null) != null && prop.GetValue(p, null).ToString().ToLower().Contains(anchorByFilter.Filter.ToLower())))
                        .ToList().Count;

                result = result
                        .Where(p => stringProperties.Any(prop => prop.GetValue(p, null) != null && prop.GetValue(p, null).ToString().ToLower().Contains(anchorByFilter.Filter.ToLower())))
                        .Skip((anchorByFilter.Page - 1) * anchorByFilter.NumRecord)
                        .Take(anchorByFilter.NumRecord).ToList();
            }

            result.ForEach(header =>
            {
                header.MyInvestments.ForEach(p =>
                {
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
            });

            anchorControlTotal.Moratorium_Interest_Total = sumMoratorium;
            anchorControlTotal.Moratorium_Vat = moratoriumVat;
            anchorControlTotal.Moratorium_Vat_Retention = moratoriumVatRetention;
            anchorControlTotal.Moratorium_Isr_Retention = moratoriumIsrRetention;
            anchorControlTotal.Moratorium_Net_Interest = moratoriumNetInterest;

            anchorControlTotal.Interest = sumInterest;
            anchorControlTotal.Vat = vat;
            anchorControlTotal.Vat_Retention = vatRetention;
            anchorControlTotal.Isr_Retention = isrRetention;
            anchorControlTotal.Net_Interest = netInterest;
            anchorControlTotal.Principal_Payment = principalPayment;
            anchorControlTotal.Total_Period = netInterest + principalPayment;

            anchorControlTotal.Total_Anchor_Period = (netInterest + principalPayment + moratoriumNetInterest);

            return new AnchorControlPagination() { AnchorControls = result, TotalRecord = totalRecord, Totals = anchorControlTotal };
        }

        (List<MyInvestment>, int) GenerateInvestments(GetMyInvestment getMyInvestment)
        {
            int totalRecord = 0;
            
            if (getMyInvestment.Source == 1 && (getMyInvestment.Page == 0 || getMyInvestment.NumRecord == 0))
            {
                getMyInvestment.Page = 1;
                getMyInvestment.NumRecord = 20;
            }

            var investor = this._InvestorRetrieveService.Find(getMyInvestment.Investor_Id);
            int vat = Convert.ToInt32(this._ConfigurationRetrieveService.Where(p => p.Configuration_Name == "VAT").FirstOrDefault().Configuration_Value);
            int isr = Convert.ToInt32(this._ConfigurationRetrieveService.Where(p => p.Configuration_Name == "ISR").FirstOrDefault().Configuration_Value);

            var periods = this._PeriodRetrieveService.Where(p => p.Enabled == true && p.User_Type == 1);

            List<Capital> listCapitalByInvestor = new List<Capital>();

            if (getMyInvestment.Source == 1)
                totalRecord = this._CapitalRetrieveService.Where(p => p.investor_id == getMyInvestment.Investor_Id).ToList().Count;
            else
                totalRecord = this._CapitalRetrieveService.Where(p => p.investor_id == getMyInvestment.Investor_Id && p.Investment_Status == (int)PrestaQiEnum.InvestmentEnum.Activa).ToList().Count;

            if (getMyInvestment.Source == 1)
            {
                listCapitalByInvestor = this._CapitalRetrieveService
                    .Where(p => p.investor_id == getMyInvestment.Investor_Id)
                    .OrderBy(p => p.id).Skip((getMyInvestment.Page - 1) * getMyInvestment.NumRecord).Take(getMyInvestment.NumRecord).ToList();
            }
            else
                listCapitalByInvestor = this._CapitalRetrieveService.Where(p => p.investor_id == getMyInvestment.Investor_Id && p.Investment_Status == (int)PrestaQiEnum.InvestmentEnum.Activa).OrderBy(p => p.id).ToList();
            
            List<MyInvestment> myInvestments = new List<MyInvestment>();

            if (listCapitalByInvestor.Count > 0)
            {
                myInvestments = listCapitalByInvestor.Select(p => new MyInvestment
                {
                    Capital_ID = p.id,
                    Amount = p.Amount,
                    End_Date = p.End_Date,
                    Interest_Rate = p.Interest_Rate,
                    Interest_Arrears = p.Default_Interest,
                    Start_Date = p.Start_Date,
                    Annual_Interest_Payment = Math.Round((p.Amount * ((double)p.Interest_Rate / 100)), 2),
                    Total = Math.Round(p.Amount + (p.Amount * ((double)p.Interest_Rate / 100)), 2),
                    Period_Id = p.period_id,
                    Period_Name = periods.FirstOrDefault(period => period.id == p.period_id).Description,
                    MyInvestmentDetails = this._CapitalDetailRetrieveService.Where(detail => detail.Capital_Id == p.id).OrderBy(p => p.Period).ToList(),
                    Capital_Status = p.Capital_Status,
                    Capital_Status_Name = ((PrestaQiEnum.CapitalEnum)p.Capital_Status).ToString(),
                    Investment_Status = p.Investment_Status,
                    Investment_Status_Name = ((PrestaQiEnum.InvestmentEnum)p.Investment_Status).ToString()
                }).ToList();

                myInvestments.ForEach(p =>
                {
                    var currenPeriod = p.MyInvestmentDetails.FindIndex(s => DateTime.Now.Date >= s.Start_Date &&   DateTime.Now.Date <= s.End_Date.Date);

                    if (currenPeriod >= 0)
                        p.MyInvestmentDetails[currenPeriod].IsPeriodActual = true;

                    p.MyInvestmentDetails.ForEach(detail =>
                    {
                        int periodValue = periods.FirstOrDefault(perdiod => perdiod.id == p.Period_Id).Period_Value;

                        if (!detail.IsPayment)
                        {
                            if (detail.Principal_Payment == 0)
                            {
                                detail.Interest_Payment = Math.Round((detail.Outstanding_Balance * ((double)p.Interest_Rate / 100)) / periodValue);
                            }

                            if (!detail.IsPeriodActual)
                            {
                                if (DateTime.Now.Date > detail.End_Date)
                                {
                                    detail.Default_Interest = Math.Round((detail.Outstanding_Balance * ((double)p.Interest_Arrears / 100)) / periodValue);
                                    p.Day_Overdue = (DateTime.Now.Date - detail.End_Date).Days;
                                }
                            }

                            detail.Vat = Math.Round((detail.Interest_Payment + detail.Default_Interest + detail.Promotional_Setting) * ((double)vat / 100), 2);

                            if (!investor.Is_Moral_Person)
                            {
                                detail.Vat_Retention = Math.Round((detail.Vat * 2) / 3, 2);
                                detail.Isr_Retention = Math.Round((detail.Interest_Payment + detail.Default_Interest + detail.Promotional_Setting) * ((double)isr / 100));
                            }

                            detail.Payment = (detail.Principal_Payment + detail.Interest_Payment + detail.Default_Interest + detail.Vat + detail.Promotional_Setting) - (detail.Vat_Retention + detail.Isr_Retention);
                        }
                    });

                    var detailShow = p.MyInvestmentDetails.FirstOrDefault(p => !p.IsPayment);
                    
                    if (detailShow != null)
                    {
                        p.Interest_Payable = detailShow.Interest_Payment;
                        p.Quantity_Interest_Arrears = detailShow.Default_Interest;
                        p.Total_Interest = p.Interest_Payable + p.Quantity_Interest_Arrears;
                        p.Vat = detailShow.Vat;
                        p.Vat_Retention = detailShow.Vat_Retention;
                        p.Isr_Retention = detailShow.Isr_Retention;
                        p.Net_Interest = detailShow.Payment;
                        p.Pay_Day_Limit = detailShow.Pay_Day_Limit;
                        p.Principal_Payment = detailShow.Principal_Payment > 0 ? detailShow.ToString() : "No Aplica";
                        p.Promotional_Setting = detailShow.Promotional_Setting;
                        p.Reason = detailShow.Reason;

                        DateTime dateTimeTemp = new DateTime(DateTime.Now.Year, DateTime.Now.AddMonths(2).Month, 1);
                        p.Enabled = p.Quantity_Interest_Arrears > 0 ? "Default" :
                            p.Pay_Day_Limit.Date < dateTimeTemp ? "Activo" : "No Activo";
                    }
                });
            }

            return (myInvestments, totalRecord);
        }

        public InvestmentDashboard ExecuteProcess(GetInvestment getInvestment)
        {
            int vat = Convert.ToInt32(this._ConfigurationRetrieveService.Where(p => p.Configuration_Name == "VAT").FirstOrDefault().Configuration_Value);
            int isr = Convert.ToInt32(this._ConfigurationRetrieveService.Where(p => p.Configuration_Name == "ISR").FirstOrDefault().Configuration_Value);
            var periods = this._PeriodRetrieveService.Where(p => p.Enabled == true && p.User_Type == 1);

            List<CapitalDetail> capitalDetails = new List<CapitalDetail>();

            if (getInvestment.Filter != "range-dates" && getInvestment.Filter != "specific-day")
            {
                var resultDate = Utilities.CalcuateDates(getInvestment.Filter);
                getInvestment.Start_Date = resultDate.Item1;
                getInvestment.End_Date = resultDate.Item2;
                getInvestment.Is_Specifid_Day = resultDate.Item3;
            }
            if (getInvestment.Start_Date.Date == getInvestment.End_Date.Date && getInvestment.Is_Specifid_Day == false)
                getInvestment.Is_Specifid_Day = true;

            if (getInvestment.Is_Specifid_Day)
            {
                capitalDetails = this._CapitalDetailRetrieveService.Where(p => p.Start_Date.Date == getInvestment.Start_Date.Date).ToList();
                getInvestment.End_Date = getInvestment.Start_Date;
            }
            else
            {
                capitalDetails = this._CapitalDetailRetrieveService.Where(p => p.Start_Date.Date >= getInvestment.Start_Date.Date &&
                p.Start_Date.Date <= getInvestment.End_Date.Date).ToList();
            }

            capitalDetails.ForEach(p =>
            {
                if (p.Interest_Payment == 0 && p.Principal_Payment == 0)
                {
                    var capital = this._CapitalRetrieveService.Find(p.Capital_Id);
                    var investor = this._InvestorRetrieveService.Find(capital.investor_id);
                    int periodValue = periods.FirstOrDefault(perdiod => perdiod.id == capital.period_id).Period_Value;
                    p.Interest_Payment = Math.Round((p.Outstanding_Balance * ((double)capital.Interest_Rate / 100)) / periodValue);
                    p.Vat = Math.Round((p.Interest_Payment + p.Default_Interest + p.Promotional_Setting) * ((double)vat / 100), 2);

                    if (!investor.Is_Moral_Person)
                    {
                        p.Vat_Retention = Math.Round((p.Vat * 2) / 3, 2);
                        p.Isr_Retention = Math.Round((p.Interest_Payment + p.Default_Interest + p.Promotional_Setting) * ((double)isr / 100));
                    }

                    p.Payment = (p.Principal_Payment + p.Interest_Payment + p.Default_Interest + p.Vat + p.Promotional_Setting) - (p.Vat_Retention + p.Isr_Retention);
                }
            });

            List<InvestmentDashboardDetail> details = new List<InvestmentDashboardDetail>();

            if (capitalDetails.Count > 0)
            {
                if (getInvestment.Filter != "range-dates")
                {
                    while (getInvestment.Start_Date.Date <= getInvestment.End_Date.Date)
                    {
                        InvestmentDashboardDetail investmentDashboardDetail = new InvestmentDashboardDetail()
                        {
                            Date = getInvestment.Start_Date,
                            Interest_Paid = capitalDetails.Where(p => p.Start_Date.Date == getInvestment.Start_Date.Date && p.Principal_Payment == 0).Sum(p => p.Payment),
                            Main_Return = capitalDetails.Where(p => p.Start_Date.Date == getInvestment.Start_Date.Date && p.Principal_Payment > 0).Sum(p => p.Payment)
                        };

                        details.Add(investmentDashboardDetail);
                        getInvestment.Start_Date = getInvestment.Start_Date.AddDays(1);
                    }
                }
                else
                {
                    var grupo = capitalDetails.GroupBy(p => p.Start_Date.Date).OrderBy(p => p.Key);

                    foreach (var group in grupo)
                    {
                        InvestmentDashboardDetail investmentDashboardDetail = new InvestmentDashboardDetail()
                        {
                            Date = group.Key,
                            Interest_Paid = group.Where(p => p.Principal_Payment == 0).Sum(p => p.Payment),
                            Main_Return = capitalDetails.Where(p => p.Principal_Payment > 0).Sum(p => p.Payment)
                        };

                        details.Add(investmentDashboardDetail);
                    }
                }
            }

            InvestmentDashboard investmentDashboard = new InvestmentDashboard()
            {
                InvestmentDashboardDetails = details,
                Interest_Paid = details.Sum(p => p.Interest_Paid),
                Main_Return = details.Sum(p => p.Main_Return),
                Average_Interest_Paid = capitalDetails.Count > 0 ? getInvestment.Is_Specifid_Day == true ?
                        Math.Round(details.Sum(p => p.Interest_Paid) / capitalDetails.Where(p => p.Interest_Payment > 0).Count(), 2) :
                        Math.Round(details.Sum(p => p.Interest_Paid) / details.Count) : 0,
                Average_Main_Return = capitalDetails.Count > 0 ? getInvestment.Is_Specifid_Day == true ?
                        Math.Round(details.Sum(p => p.Main_Return) / capitalDetails.Where(p => p.Principal_Payment > 0).Count(), 2) :
                        Math.Round(details.Sum(p => p.Main_Return) / details.Count) : 0
            };


            return investmentDashboard;
        }
    }
}

using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using PrestaQi.Model;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Model.Dto.Output;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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

        public List<MyInvestment> ExecuteProcess(int id)
        {
            return GenerateInvestments(id);
        }

        public List<AnchorControl> ExecuteProcess(AnchorByFilter anchorByFilter)
        {
            var investors = this._InvestorRetrieveService.Where(
                p => p.Start_Date_Prestaqi.Date >= anchorByFilter.Start_Date && 
                p.Start_Date_Prestaqi.Date <= anchorByFilter.End_Date.Date
                ).ToList();

            List<AnchorControl> result = new List<AnchorControl>();

            if (investors.Count > 0)
            {
                result = investors.Select(p => new AnchorControl
                {
                    Investor_Id = p.id,
                    MyInvestments = GenerateInvestments(p.id),
                    Name_Complete = $"{p.First_Name} {p.Last_Name}"
                }).ToList();
            }

            return result;
        }

        List<MyInvestment> GenerateInvestments(int investor_id)
        {
            var investor = this._InvestorRetrieveService.Find(investor_id);
            int vat = Convert.ToInt32(this._ConfigurationRetrieveService.Where(p => p.Configuration_Name == "VAT").FirstOrDefault().Configuration_Value);
            int isr = Convert.ToInt32(this._ConfigurationRetrieveService.Where(p => p.Configuration_Name == "ISR").FirstOrDefault().Configuration_Value);

            var periods = this._PeriodRetrieveService.Where(p => p.Enabled == true && p.User_Type == 1);

            var listCapitalByInvestor = this._CapitalRetrieveService.Where(p => p.investor_id == investor_id).ToList();
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
                    MyInvestmentDetails = this._CapitalDetailRetrieveService.Where(detail => detail.Capital_Id == p.id).OrderBy(p => p.Period).ToList(),
                    Enabled = p.End_Date.Date >= DateTime.Now.Date ? true : false,
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
                                if (DateTime.Now.Date > detail.Pay_Day_Limit)
                                {
                                    detail.Default_Interest = Math.Round((detail.Outstanding_Balance * ((double)p.Interest_Arrears / 100)) / periodValue);
                                    p.Day_Overdue = (DateTime.Now.Date - detail.Pay_Day_Limit.Date).Days;
                                }
                            }

                            detail.Vat = Math.Round((detail.Interest_Payment + detail.Default_Interest) * ((double)vat / 100), 2);

                            if (!investor.Is_Moral_Person)
                            {
                                detail.Vat_Retention = Math.Round((detail.Vat * 2) / 3, 2);
                                detail.Isr_Retention = Math.Round((detail.Interest_Payment + detail.Default_Interest) * ((double)isr / 100));
                            }

                            detail.Payment = (detail.Principal_Payment + detail.Interest_Payment + detail.Default_Interest + detail.Vat) - (detail.Vat_Retention + detail.Isr_Retention);
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
                    }
                });
            }

            return myInvestments;
        }
    }
}

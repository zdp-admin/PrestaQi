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

        public CapitalProcessService(
            IRetrieveService<Period> periodRetrieveService,
            IRetrieveService<Capital> capitalRetrieveService,
            IRetrieveService<Configuration> configurationRetrieveService,
            IRetrieveService<Investor> investorRetrieveService
            )
        {
            this._PeriodRetrieveService = periodRetrieveService;
            this._CapitalRetrieveService = capitalRetrieveService;
            this._ConfigurationRetrieveService = configurationRetrieveService;
            this._InvestorRetrieveService = investorRetrieveService;
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
                    MyInvestmentDetails = new List<MyInvestmentDetail>(),
                    Enabled = p.End_Date.Date >= DateTime.Now.Date ? true : false
                }).ToList();

                myInvestments.ForEach(p =>
                {
                    int totalMonth = p.End_Date.Subtract(p.Start_Date).Days / (365 / 12);
                    int periodValue = periods.FirstOrDefault(z => z.id == p.Period_Id).Period_Value;
                    DateTime startDate = p.Start_Date;

                    p.Interest_Payable = Math.Round((p.Amount * ((double)p.Interest_Rate / 100)) / periodValue, 2);
                    p.Total_Interest = p.Interest_Payable + p.Quantity_Interest_Arrears;
                    p.Vat = Math.Round(p.Total_Interest * ((double)vat / 100), 2);

                    if (!investor.Is_Moral_Person)
                    {
                        p.Vat_Retention = Math.Round((p.Vat * 2) / 3, 2);
                        p.Isr_Retention = Math.Round(p.Total_Interest * ((double)isr / 100), 2);
                    }

                    p.Net_Interest = p.Total_Interest + p.Vat - p.Vat_Retention - p.Isr_Retention;

                    for (int i = 1; i <= periodValue; i++)
                    {
                        var newDetail = new MyInvestmentDetail()
                        {
                            Period = i,
                            Initial_Date = startDate,
                            End_Date = startDate.AddMonths(periodValue),
                            Outstanding_Balance = p.Amount,
                            Principal_Payment = i == periodValue ? p.Amount : 0,
                            Interest_Payment = i != periodValue ? Math.Round((p.Amount / periodValue) * ((double)p.Interest_Rate / 100), 2) : 0,
                            Default_Interest = i == periodValue ?
                                Math.Round((p.Amount / periodValue) * ((double)p.Interest_Arrears / 100), 2) : 0
                        };

                        newDetail.Vat = Math.Round((newDetail.Interest_Payment + newDetail.Default_Interest) * ((double)vat / 100), 2);

                        if (!investor.Is_Moral_Person)
                        {
                            newDetail.Vat_Retention = Math.Round((newDetail.Vat * 2) / 3, 2);
                            newDetail.Isr_Retention = Math.Round((newDetail.Interest_Payment + newDetail.Default_Interest) * ((double)isr / 100), 2);
                        }

                        if (i == periodValue)
                            newDetail.Payment = p.Amount + newDetail.Interest_Payment + newDetail.Default_Interest + newDetail.Vat - newDetail.Vat_Retention - newDetail.Isr_Retention;
                        else
                            newDetail.Payment = newDetail.Interest_Payment + newDetail.Default_Interest + newDetail.Vat - newDetail.Vat_Retention - newDetail.Isr_Retention;

                        p.MyInvestmentDetails.Add(newDetail);

                        startDate = startDate.AddMonths(periodValue);
                    }
                });
            }

            return myInvestments;
        }
    }
}

using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using PrestaQi.Model;
using PrestaQi.Model.Dto.Output;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            var investor = this._InvestorRetrieveService.Find(id);
            int vat = Convert.ToInt32(this._ConfigurationRetrieveService.Where(p => p.Configuration_Name == "VAT").FirstOrDefault().Configuration_Value);
            int isr = Convert.ToInt32(this._ConfigurationRetrieveService.Where(p => p.Configuration_Name == "ISR").FirstOrDefault().Configuration_Value);
            
            var periods = this._PeriodRetrieveService.Where(p => p.Enabled == true);

            var listCapitalByInvestor = this._CapitalRetrieveService.Where(p => p.investor_id == id).ToList();
            List<MyInvestment> myInvestments = new List<MyInvestment>();

            if (listCapitalByInvestor.Count > 0)
            {
                myInvestments = listCapitalByInvestor.Select(p => new MyInvestment
                {
                    Capital_ID = p.id,
                    Amount = p.Amount,
                    End_Date = p.End_Date,
                    Interest_Rate = p.Interest_Rate,
                    Default_Interest = p.Default_Interest,
                    Start_Date = p.Start_Date,
                    Annual_Interest_Payment = Math.Round((p.Amount * ((double)p.Interest_Rate / 100)), 2),
                    Total = Math.Round(p.Amount + (p.Amount * ((double)p.Interest_Rate / 100)), 2),
                    Period_Id = p.period_id,
                    MyInvestmentDetails = new List<MyInvestmentDetail>()
                }).ToList();

                myInvestments.ForEach(p =>
                {
                    int totalMonth = p.End_Date.Subtract(p.Start_Date).Days / (365 / 12);
                    int periodValue = periods.FirstOrDefault(z => z.id == p.Period_Id).Period_Value;
                    DateTime startDate = p.Start_Date;
                    int totalPeriod = totalMonth / periodValue;

                    for (int i = 1; i <= totalPeriod; i++)
                    {
                        var newDetail = new MyInvestmentDetail()
                        {
                            Period = i,
                            Initial_Date = startDate,
                            End_Date = startDate.AddMonths(periodValue),
                            Outstanding_Balance = p.Amount,
                            Principal_Payment = i == totalPeriod ? p.Amount : 0,
                            Interest_Payment = i != totalPeriod ? Math.Round((p.Amount / totalPeriod) * ((double)p.Interest_Rate / 100), 2) : 0,
                            Default_Interest = i == totalPeriod ?
                                Math.Round((p.Amount / totalPeriod) * ((double)p.Default_Interest / 100), 2) : 0
                        };

                        newDetail.Vat = Math.Round((newDetail.Interest_Payment + newDetail.Default_Interest) * ((double)vat / 100), 2);

                        if (!investor.Is_Moral_Person)
                        {
                            newDetail.Vat_Retention = Math.Round((newDetail.Vat * 2) / 3, 2);
                            newDetail.Isr_Retention = Math.Round((newDetail.Interest_Payment + newDetail.Default_Interest) * ((double)isr / 100), 2);
                        }

                        if (i == totalPeriod)
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

using InsiscoCore.Base.Data;
using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using PrestaQi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrestaQi.Service.ProcessServices
{
    public class CapitalDetailProcessService : ProcessService<CapitalDetail>
    {
        IRetrieveService<Configuration> _ConfigurationRetrieveService;
        IRetrieveService<Investor> _InvestorRetrieveService;
        IRetrieveService<Capital> _CapitalRetrieveService;
        IRetrieveService<Period> _PeriodRetrieveService;
        IWriteRepository<CapitalDetail> _CapitalDetailWriteRepository;

        public CapitalDetailProcessService(
            IRetrieveService<Configuration> configurationRetrieveService,
            IRetrieveService<Investor> investorRetrieveService,
            IRetrieveService<Capital> capitalRetrieveService,
            IRetrieveService<Period> periodRetrieveService,
            IWriteRepository<CapitalDetail> capitalDetailWriteRepository
            )
        {
            this._InvestorRetrieveService = investorRetrieveService;
            this._ConfigurationRetrieveService = configurationRetrieveService;
            this._CapitalRetrieveService = capitalRetrieveService;
            this._PeriodRetrieveService = periodRetrieveService;
            this._CapitalDetailWriteRepository = capitalDetailWriteRepository;
        }

        public CapitalDetail ExecuteProcess(CapitalDetail detail)
        {
            var capital = this._CapitalRetrieveService.Find(detail.Capital_Id);
            var investor = this._InvestorRetrieveService.Find(capital.investor_id);
            int vat = Convert.ToInt32(this._ConfigurationRetrieveService.Where(p => p.Configuration_Name == "VAT").FirstOrDefault().Configuration_Value);
            int isr = Convert.ToInt32(this._ConfigurationRetrieveService.Where(p => p.Configuration_Name == "ISR").FirstOrDefault().Configuration_Value);
            var periods = this._PeriodRetrieveService.Where(p => p.Enabled == true && p.User_Type == 1);

            int periodValue = periods.FirstOrDefault(perdiod => perdiod.id == capital.period_id).Period_Value;

            detail.Interest_Payment = Math.Round((detail.Outstanding_Balance * ((double)capital.Interest_Rate / 100)) / periodValue);

            if (!detail.IsPeriodActual)
            {
                if (DateTime.Now.Date > detail.End_Date)
                {
                    detail.Default_Interest = Math.Round((detail.Outstanding_Balance * ((double)capital.Default_Interest / 100)) / periodValue);
                }
            }

            detail.Vat = Math.Round((detail.Interest_Payment + detail.Default_Interest + detail.Promotional_Setting) * ((double)vat / 100), 2);

            if (!investor.Is_Moral_Person)
            {
                detail.Vat_Retention = Math.Round((detail.Vat * 2) / 3, 2);
                detail.Isr_Retention = Math.Round((detail.Interest_Payment + detail.Default_Interest + detail.Promotional_Setting) * ((double)isr / 100));
            }

            detail.Payment = (detail.Principal_Payment + detail.Interest_Payment + detail.Default_Interest + detail.Vat + detail.Promotional_Setting) - (detail.Vat_Retention + detail.Isr_Retention);

            this._CapitalDetailWriteRepository.Update(detail);

            return detail;
        }
    }
}

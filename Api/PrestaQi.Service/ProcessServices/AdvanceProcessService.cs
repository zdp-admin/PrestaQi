using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using PrestaQi.Model;
using PrestaQi.Model.Configurations;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Model.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace PrestaQi.Service.ProcessServices
{
    public class AdvanceProcessService : ProcessService<Advance>
    {
        IRetrieveService<Accredited> _AcreditedRetrieveService;
        IRetrieveService<Advance> _AdvanceRetrieveService;
        IRetrieveService<Configuration> _ConfigutarionRetrieveService;
        IRetrieveService<Period> _PeriodRetrieveService;

        public AdvanceProcessService(
            IRetrieveService<Accredited> acreditedRetrieveService,
            IRetrieveService<Advance> advanceRetrieveService,
            IRetrieveService<Configuration> configurationRetrieveService,
            IRetrieveService<Period> periodRetrieveService
            )
        {
            this._AcreditedRetrieveService = acreditedRetrieveService;
            this._AdvanceRetrieveService = advanceRetrieveService;
            this._ConfigutarionRetrieveService = configurationRetrieveService;
            this._PeriodRetrieveService = periodRetrieveService;
        }

        public Advance ExecuteProcess(CalculateAmount calculateAmount)
        {
            var accredited = this._AcreditedRetrieveService.Find(calculateAmount.Accredited_Id);

            int initialCommission = Convert.ToInt32(this._ConfigutarionRetrieveService.Where(p => p.Configuration_Name == "INITIAL_COMMISSION").FirstOrDefault().Configuration_Value);
            double annualInterest = ((double)Convert.ToInt32(this._ConfigutarionRetrieveService.Where(p => p.Configuration_Name == "ANNUAL_INTEREST").FirstOrDefault().Configuration_Value) / 100);
            int finantialDay = Convert.ToInt32(this._ConfigutarionRetrieveService.Where(p => p.Configuration_Name == "FINANCIAL_DAYS").FirstOrDefault().Configuration_Value);

            var period = this._PeriodRetrieveService.Find(accredited.Period_Id);

            if (accredited == null)
                throw new SystemValidationException("Accredited not found");

            DateTime startDate = DateTime.Now;
            DateTime endDate = DateTime.Now;
            int day = 0;

            switch (accredited.Period_Id)
            {
                case (int)PrestaQiEnum.PerdioAccredited.Quincenal:
                    if (DateTime.Now.Day <= period.Period_Value)
                    {
                        startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                        endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 14);
                        day = DateTime.Now.Day;
                    }
                    else
                    {
                        startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 15);
                        endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, (DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month) - 1));
                        day = (DateTime.Now.Day - startDate.Day) + 1;
                    }
                    break;
                case (int)PrestaQiEnum.PerdioAccredited.Mensual:
                    startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, (DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month) - 1));
                    day = DateTime.Now.Day;
                    break;

            }

            var advances = this._AdvanceRetrieveService.Where(p => p.Accredited_Id == accredited.id &&
            p.Date_Advance.Date >= startDate.Date && p.Date_Advance <= endDate.Date).ToList();
            int commision = (initialCommission + day) - 1;
            double sumMaxAmount = Math.Round(advances.Count > 0 ? accredited.Net_Monthly_Salary - advances.Sum(p => p.Total_Withhold) : accredited.Net_Monthly_Salary);
            double total_Withhold = Math.Round(calculateAmount.Amount + (((calculateAmount.Amount * annualInterest) / finantialDay) * (period.Period_Value - day) + commision), 2);
            Advance advanceCalculated = new Advance()
            {
                Accredited_Id = accredited.id,
                Amount = calculateAmount.Amount,
                Requested_Day = day,
                Comission = commision,
                Total_Withhold = total_Withhold,
                Maximum_Amount = sumMaxAmount - total_Withhold
            };

            return advanceCalculated;
                
        }
    }
}

using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using PrestaQi.Model;
using PrestaQi.Model.Configurations;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Model.Dto.Output;
using PrestaQi.Model.Enum;
using PrestaQi.Service.Tools;
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
            double annualInterest = ((double)accredited.Interest_Rate / 100);
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

                    if (DateTime.Now.Day > 14)
                        day = 14;    
                    break;
                case (int)PrestaQiEnum.PerdioAccredited.Semanal:
                    startDate = DateTime.Now.StartOfWeek(DayOfWeek.Monday);
                    endDate = DateTime.Now.StartOfWeek(DayOfWeek.Saturday);
                    day = (DateTime.Now.Day - startDate.Day) + 1;
                    break;
            }

            var advances = this._AdvanceRetrieveService.Where(p => p.Accredited_Id == accredited.id &&
            p.Date_Advance.Date >= startDate.Date && p.Date_Advance <= endDate.Date).ToList();
            int commision = (initialCommission + day) - 1;
            double sumMaxAmount = Math.Round(advances.Count > 0 ? accredited.Net_Monthly_Salary - advances.Sum(p => p.Total_Withhold) : accredited.Net_Monthly_Salary);

            double total_Withhold = (accredited.Period_Id != (int)PrestaQiEnum.PerdioAccredited.Mensual) ? Math.Round(calculateAmount.Amount + (((calculateAmount.Amount * annualInterest) / finantialDay) * (period.Period_Value - day) + commision), 2) :
                Math.Round(calculateAmount.Amount + (((calculateAmount.Amount * annualInterest) / finantialDay) * (period.Period_Value - DateTime.Now.Day) + commision), 2);

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

        public CommisionAndInterestMaster ExecuteProcess(GetCommisionAndIntereset getCommisionAndIntereset)
        {
            List<Advance> advances = new List<Advance>();

            if (getCommisionAndIntereset.Is_Specifid_Day)
            {
                advances = this._AdvanceRetrieveService.Where(p => p.Date_Advance.Date == getCommisionAndIntereset.Start_Date).ToList();
                getCommisionAndIntereset.End_Date = getCommisionAndIntereset.Start_Date;
            }
            else
            {
                advances = this._AdvanceRetrieveService.Where(p => p.Date_Advance.Date >= getCommisionAndIntereset.Start_Date &&
                p.Date_Advance.Date <= getCommisionAndIntereset.End_Date).ToList();
            }

            var listDetial = advances.GroupBy(p => p.Date_Advance.Date).Select(item => new CommissionAndInterestByDay()
            {
                Date = item.Key,
                Commission = item.Sum(com => com.Comission),
                Interest = item.Sum(inte => inte.Total_Withhold - inte.Amount)
            }).ToList();

            CommisionAndInterestMaster commisionAndInterestMaster = new CommisionAndInterestMaster()
            {
                CommissionAndInterestByDays = listDetial,
                Total_Interest = listDetial.Sum(p => p.Interest),
                Total_Commission = listDetial.Sum(p => p.Commission),
                Average_Commission = getCommisionAndIntereset.Is_Specifid_Day ? listDetial.Sum(p => p.Commission) / advances.Count :
                    listDetial.Sum(p => p.Commission) / listDetial.Count, 
                Average_Interest = getCommisionAndIntereset.Is_Specifid_Day ? listDetial.Sum(p => p.Interest) / advances.Count :
                    listDetial.Sum(p => p.Interest) / listDetial.Count
            };

            return commisionAndInterestMaster;
        }

        public CreditAverage ExecuteProcess(GetCredits getCredits)
        {
            List<Advance> advances = new List<Advance>();

            if (getCredits.Is_Specifid_Day)
            {
                advances = this._AdvanceRetrieveService.Where(p => p.Date_Advance.Date == getCredits.Start_Date).ToList();
                getCredits.End_Date = getCredits.Start_Date;
            }
            else
            {
                advances = this._AdvanceRetrieveService.Where(p => p.Date_Advance.Date >= getCredits.Start_Date &&
                p.Date_Advance.Date <= getCredits.End_Date).ToList();
            }

            var detail = advances.GroupBy(p => p.Date_Advance.Date).Select(item => new CreditAvarageDetail()
            {
                Date = item.Key,
                Amount = item.Sum(p => p.Amount)
            }).ToList();

            CreditAverage creditAverage = new CreditAverage()
            {
                Total_Credit = advances.Count,
                CreditAvarageDetails = detail,
                Credit_Average = advances.Count / detail.Count,
                Amount_Average = getCredits.Is_Specifid_Day ? detail.Sum(p => p.Amount) / advances.Count :
                 detail.Sum(p => p.Amount) / detail.Count
            };

            return creditAverage;
        }
    }
}

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
using System.Linq;

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
            bool isMaxAmount = false;
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
                        startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 16);
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
                    endDate = startDate.AddDays(5);//DateTime.Now.(DayOfWeek.Saturday);
                    day = (int)DateTime.Now.DayOfWeek; 
                    break;
            }

            var advances = this._AdvanceRetrieveService.Where(p => p.Accredited_Id == accredited.id &&
            p.Date_Advance.Date >= startDate.Date && p.Date_Advance <= endDate.Date && p.Paid_Status == 0).ToList();
            int commision = (initialCommission + day) - 1;
            double sumMaxAmount = Math.Round(advances.Count > 0 ? accredited.Net_Monthly_Salary - advances.Sum(p => p.Total_Withhold) : accredited.Net_Monthly_Salary);

            if (calculateAmount.Amount == 0)
            {
                isMaxAmount = true;
                calculateAmount.Amount = sumMaxAmount;
            }

            double total_Withhold = (accredited.Period_Id != (int)PrestaQiEnum.PerdioAccredited.Mensual) ? Math.Round(calculateAmount.Amount + (((calculateAmount.Amount * annualInterest) / finantialDay) * (period.Period_Value - day) + commision), 2) :
                Math.Round(calculateAmount.Amount + (((calculateAmount.Amount * annualInterest) / finantialDay) * (period.Period_Value - DateTime.Now.Day) + commision), 2);

            Advance advanceCalculated = new Advance();
            if (!isMaxAmount) {
                advanceCalculated.Accredited_Id = accredited.id;
                advanceCalculated.Amount = calculateAmount.Amount;
                advanceCalculated.Requested_Day = day;
                advanceCalculated.Comission = commision;
                advanceCalculated.Total_Withhold = total_Withhold;
            }
            else
            {
                advanceCalculated.Maximum_Amount = Math.Round(sumMaxAmount - (total_Withhold - sumMaxAmount));
            }


            return advanceCalculated;
                
        }

        public CommisionAndInterestMaster ExecuteProcess(GetCommisionAndIntereset getCommisionAndIntereset)
        {
            List<Advance> advances = new List<Advance>();

            if (getCommisionAndIntereset.Filter != "range-dates" && getCommisionAndIntereset.Filter != "specific-day")
            {
                var resultDate = Utilities.CalcuateDates(getCommisionAndIntereset.Filter);
                getCommisionAndIntereset.Start_Date = resultDate.Item1;
                getCommisionAndIntereset.End_Date = resultDate.Item2;
                getCommisionAndIntereset.Is_Specifid_Day = resultDate.Item3;
            }

            if (getCommisionAndIntereset.Start_Date.Date == getCommisionAndIntereset.End_Date.Date && getCommisionAndIntereset.Is_Specifid_Day == false)
                getCommisionAndIntereset.Is_Specifid_Day = true;

            if (getCommisionAndIntereset.Is_Specifid_Day)
            {
                advances = this._AdvanceRetrieveService.Where(p => p.Date_Advance.Date == getCommisionAndIntereset.Start_Date.Date).ToList();
                getCommisionAndIntereset.End_Date = getCommisionAndIntereset.Start_Date;
            }
            else
            {
                advances = this._AdvanceRetrieveService.Where(p => p.Date_Advance.Date >= getCommisionAndIntereset.Start_Date.Date &&
                p.Date_Advance.Date <= getCommisionAndIntereset.End_Date.Date).ToList();
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
                Average_Commission = advances.Count > 0 ? getCommisionAndIntereset.Is_Specifid_Day ? listDetial.Sum(p => p.Commission) / advances.Count :
                    listDetial.Sum(p => p.Commission) / listDetial.Count : 0, 
                Average_Interest = advances.Count > 0 ? getCommisionAndIntereset.Is_Specifid_Day ? listDetial.Sum(p => p.Interest) / advances.Count :
                    listDetial.Sum(p => p.Interest) / listDetial.Count : 0
            };

            return commisionAndInterestMaster;
        }

        public CreditAverage ExecuteProcess(GetCredits getCredits)
        {
            List<Advance> advances = new List<Advance>();

            if (getCredits.Filter != "range-dates" && getCredits.Filter != "specific-day")
            {
                var resultDate = Utilities.CalcuateDates(getCredits.Filter);
                getCredits.Start_Date = resultDate.Item1;
                getCredits.End_Date = resultDate.Item2;
                getCredits.Is_Specifid_Day = resultDate.Item3;
            }
            if (getCredits.Start_Date.Date == getCredits.End_Date.Date && getCredits.Is_Specifid_Day == false)
                getCredits.Is_Specifid_Day = true;

            if (getCredits.Is_Specifid_Day)
            {
                advances = this._AdvanceRetrieveService.Where(p => p.Date_Advance.Date == getCredits.Start_Date.Date).ToList();
                getCredits.End_Date = getCredits.Start_Date;
            }
            else
            {
                advances = this._AdvanceRetrieveService.Where(p => p.Date_Advance.Date >= getCredits.Start_Date.Date &&
                p.Date_Advance.Date <= getCredits.End_Date.Date).ToList();
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
                Credit_Average = advances.Count > 0 ? advances.Count / detail.Count : 0,
                Amount_Average = advances.Count > 0 ? getCredits.Is_Specifid_Day ? detail.Sum(p => p.Amount) / advances.Count :
                 detail.Sum(p => p.Amount) / detail.Count : 0
            };

            return creditAverage;
        }

        
    }
}

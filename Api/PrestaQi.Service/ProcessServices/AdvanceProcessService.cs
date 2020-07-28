using DocumentFormat.OpenXml.Math;
using DocumentFormat.OpenXml.Spreadsheet;
using InsiscoCore.Base.Data;
using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using OpenXmlPowerTools;
using PrestaQi.Model;
using PrestaQi.Model.Configurations;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Model.Dto.Output;
using PrestaQi.Model.Enum;
using PrestaQi.Service.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PrestaQi.Service.ProcessServices
{
    public class AdvanceProcessService : ProcessService<Advance>
    {
        IRetrieveRepository<Accredited> _AcreditedRetrieveService;
        IRetrieveService<Advance> _AdvanceRetrieveService;
        IRetrieveService<Configuration> _ConfigutarionRetrieveService;
        IRetrieveService<Period> _PeriodRetrieveService;
        IRetrieveService<PeriodCommission> _PeriodCommissionRetrieve;
        IRetrieveService<PeriodCommissionDetail> _PeriodCommissionDetailRetrieve;


        public AdvanceProcessService(
            IRetrieveRepository<Accredited> acreditedRetrieveService,
            IRetrieveService<Advance> advanceRetrieveService,
            IRetrieveService<Configuration> configurationRetrieveService,
            IRetrieveService<Period> periodRetrieveService,
            IRetrieveService<PeriodCommission> periodCommissionRetrieve,
            IRetrieveService<PeriodCommissionDetail> periodCommissionDetailRetrieve
            )
        {
            this._AcreditedRetrieveService = acreditedRetrieveService;
            this._AdvanceRetrieveService = advanceRetrieveService;
            this._ConfigutarionRetrieveService = configurationRetrieveService;
            this._PeriodRetrieveService = periodRetrieveService;
            this._PeriodCommissionRetrieve = periodCommissionRetrieve;
            this._PeriodCommissionDetailRetrieve = periodCommissionDetailRetrieve;
    }

        public Advance ExecuteProcess(CalculateAmount calculateAmount)
        {
            Advance advanceCalculated = new Advance();
            bool isMaxAmount = false;
            var accredited = this._AcreditedRetrieveService.Find(calculateAmount.Accredited_Id);
            int commission = Convert.ToInt32(this._ConfigutarionRetrieveService.Where(p => p.Configuration_Name == "INITIAL_COMMISSION").FirstOrDefault().Configuration_Value);
            double annualInterest = ((double)accredited.Interest_Rate / 100);
            int finantialDay = Convert.ToInt32(this._ConfigutarionRetrieveService.Where(p => p.Configuration_Name == "FINANCIAL_DAYS").FirstOrDefault().Configuration_Value);
            double vat = Convert.ToDouble(this._ConfigutarionRetrieveService.Where(p => p.Configuration_Name == "VAT").FirstOrDefault().Configuration_Value) / 100;

            var period = this._PeriodRetrieveService.Find(accredited.Period_Id);

            if (accredited == null)
                throw new SystemValidationException("Accredited not found");

            DateTime startDate = DateTime.Now;
            DateTime endDate = DateTime.Now;
            DateTime limitDate = DateTime.Now;
            int day = DateTime.Now.Day;

            int endDay = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);

            var commissionPeriodId = this._PeriodCommissionRetrieve.Where(p => p.Period_Id == accredited.Period_Id && p.Type_Month == endDay).FirstOrDefault().id;
            var commissionPerioDetail = this._PeriodCommissionDetailRetrieve.Where(p => p.Period_Commission_Id == commissionPeriodId && p.Day_Month == DateTime.Now.Day).FirstOrDefault();
            commission = Convert.ToInt32(commissionPerioDetail.Commission);

            switch (accredited.Period_Id)
            {
                case (int)PrestaQiEnum.PerdioAccredited.Quincenal:
                    
                    if (commissionPerioDetail.Date_Payment == 15 && DateTime.Now.Day > 15)
                        limitDate = limitDate.AddMonths(1);

                    limitDate = new DateTime(limitDate.Year, limitDate.Month, commissionPerioDetail.Date_Payment);

                    if (calculateAmount.Amount == 0)
                    {
                        calculateAmount.Amount = Math.Round(accredited.Net_Monthly_Salary / 2);
                        isMaxAmount = true;
                    }

                    break;
                case (int)PrestaQiEnum.PerdioAccredited.Mensual:

                    if (DateTime.Now.Day >= (endDay - 2))
                    {
                        limitDate = limitDate.AddMonths(1);
                        limitDate = new DateTime(limitDate.Year, limitDate.Month, DateTime.DaysInMonth(limitDate.Year, limitDate.Month));
                    }
                    else
                        limitDate = new DateTime(limitDate.Year, limitDate.Month, endDay);

                    if (calculateAmount.Amount == 0)
                    {
                        isMaxAmount = true;
                        calculateAmount.Amount = Math.Round(accredited.Net_Monthly_Salary / 2);
                    }
                    break;
                case (int)PrestaQiEnum.PerdioAccredited.Semanal:
                    startDate = DateTime.Now.StartOfWeek(DayOfWeek.Monday);
                    endDate = startDate.AddDays(6);
                    limitDate = endDate;

                    if ((int)DateTime.Now.DayOfWeek <= 4)
                        commission = commission + ((int)DateTime.Now.DayOfWeek - 1);

                    if (DateTime.Now.Day >= endDate.AddDays(-2).Day)
                    {
                        DateTime today = DateTime.Today;
                        int daysUntilMonday = ((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7;
                        if (daysUntilMonday == 0)
                            daysUntilMonday = 7;

                        DateTime nextWeekMonday = today.AddDays(daysUntilMonday);
                        limitDate = nextWeekMonday.AddDays(6);
                    }

                    if (calculateAmount.Amount == 0)
                    {
                        isMaxAmount = true;
                        calculateAmount.Amount = Math.Round(accredited.Net_Monthly_Salary / 4);
                    }
                    break;
            }

            var advances = this._AdvanceRetrieveService.Where(p => p.Accredited_Id == accredited.id &&
            p.Date_Advance.Date >= startDate.Date && p.Date_Advance <= endDate.Date && p.Paid_Status == 0).ToList();

            if (isMaxAmount)
            {
                advanceCalculated.Maximum_Amount = Math.Round(advances.Count > 0 ? calculateAmount.Amount - advances.Sum(p => p.Total_Withhold) : calculateAmount.Amount);
                return advanceCalculated;
            }

            double intereset = (accredited.Period_Id == (int)PrestaQiEnum.PerdioAccredited.Mensual) || (accredited.Period_Id == (int)PrestaQiEnum.PerdioAccredited.Quincenal) ?
                Math.Round(calculateAmount.Amount * ((annualInterest / finantialDay) * commissionPerioDetail.Day_Payement), 2) :
                Math.Round(((calculateAmount.Amount * annualInterest / finantialDay) * (period.Period_Value - DateTime.Now.Day)), 2);

            double subtotal = intereset + commission;
            double vatTotal = Math.Round(subtotal * vat, 2);

            double total_Withhold = Math.Round(calculateAmount.Amount + subtotal + vatTotal, 2);

            advanceCalculated.Accredited_Id = accredited.id;
            advanceCalculated.Amount = calculateAmount.Amount;
            advanceCalculated.Requested_Day = day;
            advanceCalculated.Comission = commission;
            advanceCalculated.Total_Withhold = total_Withhold;
            advanceCalculated.Interest = intereset;
            advanceCalculated.Vat = vatTotal;
            advanceCalculated.Subtotal = subtotal;
            advanceCalculated.Limit_Date = limitDate;
            advanceCalculated.Date_Advance = DateTime.Now;

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

        public Advance ExecuteProcess(CalculatePromotional calculatePromotional)
        {
            try
            {
                int finantialDay = Convert.ToInt32(this._ConfigutarionRetrieveService.Where(p => p.Configuration_Name == "FINANCIAL_DAYS").FirstOrDefault().Configuration_Value);
                double vat = Convert.ToDouble(this._ConfigutarionRetrieveService.Where(p => p.Configuration_Name == "VAT").FirstOrDefault().Configuration_Value) / 100;
                var advance = this._AdvanceRetrieveService.Find(calculatePromotional.Advance_Id);
                var accredited = this._AcreditedRetrieveService.Find(advance.Accredited_Id);

                if (advance == null)
                    throw new SystemValidationException("No se encontró el adelando");

                advance.Promotional_Setting = calculatePromotional.Amount;
                advance.Day_Moratorium = DateTime.Now.Date > advance.Limit_Date.Date ?
                    (DateTime.Now.Date - advance.Limit_Date).Days : 0;
                advance.Interest_Moratorium = DateTimeOffset.Now.Date > advance.Limit_Date.Date ?
                Math.Round((advance.Amount * ((double)accredited.Moratoruim_Interest_Rate / 100) / finantialDay) * advance.Day_Moratorium, 2) :
                0;
                advance.Subtotal = advance.Interest + advance.Interest_Moratorium + advance.Comission + advance.Promotional_Setting;
                advance.Vat = Math.Round(advance.Subtotal * vat, 2);
                advance.Total_Withhold = Math.Round(advance.Amount + advance.Subtotal + advance.Vat, 2);

                return advance;
            }
            catch (Exception exception)
            {
                throw new SystemValidationException($"Error al calcular el Ajuste: {exception.Message}");
            }
        }
    }
}

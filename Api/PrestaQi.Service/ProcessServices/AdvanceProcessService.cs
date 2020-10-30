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
        IRetrieveService<AdvanceDetail> _AdvanceDetailRetrieveService;


        public AdvanceProcessService(
            IRetrieveRepository<Accredited> acreditedRetrieveService,
            IRetrieveService<Advance> advanceRetrieveService,
            IRetrieveService<Configuration> configurationRetrieveService,
            IRetrieveService<Period> periodRetrieveService,
            IRetrieveService<PeriodCommission> periodCommissionRetrieve,
            IRetrieveService<PeriodCommissionDetail> periodCommissionDetailRetrieve,
            IRetrieveService<AdvanceDetail> advanceDetailRetrieveService
            )
        {
            this._AcreditedRetrieveService = acreditedRetrieveService;
            this._AdvanceRetrieveService = advanceRetrieveService;
            this._ConfigutarionRetrieveService = configurationRetrieveService;
            this._PeriodRetrieveService = periodRetrieveService;
            this._PeriodCommissionRetrieve = periodCommissionRetrieve;
            this._PeriodCommissionDetailRetrieve = periodCommissionDetailRetrieve;
            this._AdvanceDetailRetrieveService = advanceDetailRetrieveService;
    }

        public List<Advance> ExecuteProcess(CalculateAmount calculateAmount)
        {
            var accredited = this._AcreditedRetrieveService.Find(calculateAmount.Accredited_Id);
            var result = CalculateAdvance(calculateAmount, accredited);

            double periodPercentage = Convert.ToDouble(this._ConfigutarionRetrieveService.Where(p => p.Configuration_Name == "PERIOD_SALARY_PERCENTAGE").FirstOrDefault().Configuration_Value) / 100;
            double maximumAmountDiscountByPeriod = Math.Round(accredited.Gross_Monthly_Salary * periodPercentage, 2);

            if (accredited.Type_Contract_Id == (int)PrestaQiEnum.AccreditedContractType.AssimilatedToSalary || result.Item2)
                return new List<Advance>() { result.Item1 };

            var advanceDetails = this._AdvanceDetailRetrieveService.Where(p => p.Accredited_Id == accredited.id &&
            p.Limit_Date.Date >= result.Item1.Limit_Date.Date && (p.Paid_Status == 0 || p.Paid_Status == 2)).OrderBy(p => p.id).ToList();


            if ((result.Item1.Total_Withhold + advanceDetails.Sum(p => p.Total_Withhold)) <= maximumAmountDiscountByPeriod)
                return new List<Advance>() { result.Item1 };

            var resultList = CalculateAdvanceList(calculateAmount, accredited, result.Item1, periodPercentage, maximumAmountDiscountByPeriod,
                advanceDetails);

            return resultList;
        }

        private (Advance, bool) CalculateAdvance(CalculateAmount calculateAmount, Accredited accredited)
        {
            Advance advanceCalculated = new Advance();
            bool isMaxAmount = false;
           
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
            PeriodCommissionDetail commissionPerioDetail = new PeriodCommissionDetail();

            int endDay = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);

            if (accredited.Period_Id != (int)PrestaQiEnum.PerdioAccredited.Semanal)
            {
                var commissionPeriodId = this._PeriodCommissionRetrieve.Where(p => p.Period_Id == accredited.Period_Id && p.Type_Month == endDay).FirstOrDefault().id;
                commissionPerioDetail = this._PeriodCommissionDetailRetrieve.Where(p => p.Period_Commission_Id == commissionPeriodId && p.Day_Month == DateTime.Now.Day).FirstOrDefault();
                commission = Convert.ToInt32(commissionPerioDetail.Commission);
                advanceCalculated.Day_For_Payment = commissionPerioDetail.Day_Payement;
            }

            switch (accredited.Period_Id)
            {
                case (int)PrestaQiEnum.PerdioAccredited.Quincenal:
                    if (commissionPerioDetail.Date_Payment == 15 && DateTime.Now.Day > 15)
                        limitDate = limitDate.AddMonths(1);

                    limitDate = new DateTime(limitDate.Year, limitDate.Month, commissionPerioDetail.Date_Payment);

                    if (calculateAmount.Amount == 0)
                    {
                        advanceCalculated.Maximum_Amount = Math.Round(accredited.Net_Monthly_Salary / 2);
                        isMaxAmount = true;
                    }

                    if (DateTime.Now.Day >= 15)
                    {
                        startDate = new DateTime(endDate.Year, endDate.Month, 15);
                        endDate = new DateTime(endDate.Year, endDate.Month, DateTime.DaysInMonth(endDate.Year, endDate.Month));
                    }

                    if (DateTime.Now.Day < 15)
                    {
                        startDate = new DateTime(startDate.Year, startDate.Month, 1);
                        endDate = new DateTime(endDate.Year, endDate.Month, 15);
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
                        advanceCalculated.Maximum_Amount = Math.Round(accredited.Net_Monthly_Salary / 2);
                    }

                    startDate = new DateTime(startDate.Year, startDate.Month, 1);
                    endDate = new DateTime(endDate.Year, endDate.Month, DateTime.DaysInMonth(endDate.Year, endDate.Month));

                    break;
                case (int)PrestaQiEnum.PerdioAccredited.Semanal:
                    var dayWeek = accredited.End_Day_Payment.DayOfWeek;

                    if (DateTime.Now.DayOfWeek == dayWeek)
                        startDate = DateTime.Now.StartOfWeek(dayWeek).AddDays(-6);
                    else
                        startDate = DateTime.Now.StartOfWeek(dayWeek).AddDays(1);

                    endDate = startDate.AddDays(6);
                    limitDate = endDate;

                    if (DateTime.Now.Date >= endDate.AddDays(-2).Date)
                    {
                        DateTime today = DateTime.Today;
                        int daysUntilMonday = ((int)dayWeek - (int)today.DayOfWeek + 7) % 7;
                        if (daysUntilMonday == 0)
                            daysUntilMonday = 1;

                        DateTime nextWeekMonday = ((int)dayWeek - (int)today.DayOfWeek + 7) % 7 == 0 ? today.AddDays(daysUntilMonday) : today.AddDays(daysUntilMonday).AddDays(1);
                        limitDate = nextWeekMonday.AddDays(6);
                    }
                    else
                        commission = commission + ((DateTime.Now.Date - startDate.Date).Days);

                    if (calculateAmount.Amount == 0)
                    {
                        isMaxAmount = true;
                        advanceCalculated.Maximum_Amount = Math.Round(accredited.Net_Monthly_Salary / 4);
                    }

                    advanceCalculated.Day_For_Payment = (limitDate.Date - DateTime.Now.Date).Days;
                    break;
            }

            var advances = this._AdvanceRetrieveService.Where(p => p.Accredited_Id == accredited.id &&
            p.Date_Advance.Date >= startDate.Date && p.Date_Advance <= endDate.Date && p.Paid_Status == 0).ToList();

            advanceCalculated.Maximum_Amount = Math.Round(advances.Count > 0 ? advanceCalculated.Maximum_Amount - advances.Sum(p => p.Total_Withhold) : advanceCalculated.Maximum_Amount);
            if (isMaxAmount)
                calculateAmount.Amount = advanceCalculated.Maximum_Amount;

            double intereset = (accredited.Period_Id == (int)PrestaQiEnum.PerdioAccredited.Mensual) || (accredited.Period_Id == (int)PrestaQiEnum.PerdioAccredited.Quincenal) ?
                Math.Round(calculateAmount.Amount * ((annualInterest / finantialDay) * commissionPerioDetail.Day_Payement), 2) :
                Math.Round(calculateAmount.Amount * ((annualInterest / finantialDay) * (limitDate.Date - DateTime.Now.Date).Days), 2);

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
            advanceCalculated.Interest_Rate = accredited.Interest_Rate;
            advanceCalculated.Maximum_Amount = isMaxAmount ? Math.Round(advanceCalculated.Maximum_Amount - subtotal - vatTotal) : 0;
            advanceCalculated.Maximum_Amount = advanceCalculated.Maximum_Amount < 0 ? 0 : advanceCalculated.Maximum_Amount;

            return (advanceCalculated, isMaxAmount);
        }

        private List<Advance> CalculateAdvanceList(CalculateAmount calculateAmount, Accredited accredited, Advance firtsAdvance,
            double periodPercentage, double maximumAmountDiscountByPeriod, List<AdvanceDetail> advanceDetails)
        {
            double maxdiscount = maximumAmountDiscountByPeriod;
            double totalDisponible = accredited.Gross_Monthly_Salary * periodPercentage;
            double totalForPay = advanceDetails.Sum(advance => advance.Total_Withhold);
            int numberOfPayments = Convert.ToInt32(Math.Round(totalForPay / maxdiscount, 2));
            double residuo = totalForPay % maxdiscount;
            double totalAdvances = advanceDetails.Sum(advance => advance.Amount);

            List<Advance> detailsAdvances = new List<Advance>();
            int index = 0;

            if (numberOfPayments > 1)
            {
                while (totalAdvances > 0 || index < numberOfPayments)
                {
                    DateTime nextDayForPayment;

                    if (index > advanceDetails.Count - 1)
                    {
                        if (detailsAdvances.Last().Date_Advance.Day == 15)
                        {
                            var lastDayInMonth = DateTime.DaysInMonth(detailsAdvances.Last().Date_Advance.Year, detailsAdvances.Last().Date_Advance.Month);
                            nextDayForPayment = new DateTime(detailsAdvances.Last().Date_Advance.Year, detailsAdvances.Last().Date_Advance.Month, lastDayInMonth);
                        } else
                        {
                            nextDayForPayment = detailsAdvances.Last().Date_Advance.AddDays(15);
                        }
                    } else
                    {
                        nextDayForPayment = advanceDetails[index].Limit_Date;
                    }

                    // verify days habiles
                    if (nextDayForPayment.DayOfWeek == DayOfWeek.Saturday)
                    {
                        nextDayForPayment = nextDayForPayment.AddDays(-1);
                    }

                    if (nextDayForPayment.DayOfWeek == DayOfWeek.Sunday)
                    {
                        nextDayForPayment = nextDayForPayment.AddDays(-2);
                    }

                    var count = detailsAdvances.Count();

                    if (count > 0 && detailsAdvances.FirstOrDefault(detail => detail.Date_Advance == nextDayForPayment) != null)
                    {
                        totalAdvances += detailsAdvances.First(detail => detail.Date_Advance == nextDayForPayment).Amount;

                        detailsAdvances.RemoveAt(detailsAdvances.FindIndex(detail => detail.Date_Advance == nextDayForPayment));
                    }

                    //advances
                    var deductionsForNextDayPayment = advanceDetails.Where(advance => advance.Limit_Date <= nextDayForPayment).Sum(advance => advance.Comission + advance.Interest + advance.Vat);
                    var totalForNextDayPayment = advanceDetails.Where(advance => advance.Limit_Date <= nextDayForPayment).Sum(advance => advance.Total_Withhold);
                    var amounForNextDayPayment = advanceDetails.Where(advance => advance.Limit_Date <= nextDayForPayment).Sum(advance => advance.Amount);

                    //detailsAdvance
                    var detailsDeductionsTotal = detailsAdvances.Sum(da => da.Interest + da.Vat);
                    var detailsTotalPayment = detailsAdvances.Sum(da => da.Total_Withhold);
                    var detailsAmountPayment = detailsAdvances.Sum(da => da.Amount);

                    var totalPrevAdvanceWithDeduccion = (amounForNextDayPayment + deductionsForNextDayPayment) + detailsDeductionsTotal;
                    double[] listTotalsToDiscount =
                    {
                        totalDisponible,
                        ((amounForNextDayPayment / 2) + (deductionsForNextDayPayment - detailsTotalPayment) + detailsAmountPayment),
                        (amounForNextDayPayment - detailsAmountPayment) + (deductionsForNextDayPayment + detailsDeductionsTotal) - detailsTotalPayment + detailsAmountPayment,
                        maxdiscount
                    };

                    var totalDiscountForPaymentDetails = 0.0;

                    if ((totalPrevAdvanceWithDeduccion - detailsTotalPayment) < maxdiscount) 
                    {
                        totalDiscountForPaymentDetails = totalPrevAdvanceWithDeduccion - detailsTotalPayment;
                    }
                    else
                    {
                        totalDiscountForPaymentDetails = listTotalsToDiscount.Min();
                    }

                    var discountForPrincipalPayment = deductionsForNextDayPayment + detailsDeductionsTotal;
                    var principalPayment = totalDiscountForPaymentDetails - (discountForPrincipalPayment - (detailsTotalPayment - detailsAmountPayment));
                    var finalBalance = (amounForNextDayPayment - detailsAmountPayment) - principalPayment;


                    var dayForPayment = 15;
                    var nextDate = DateTime.Now;

                    if (nextDayForPayment.Day >= 1 && nextDayForPayment.Day <= 15)
                    {
                        dayForPayment = DateTime.DaysInMonth(nextDayForPayment.Year, nextDayForPayment.Month);
                        nextDate = new DateTime(nextDayForPayment.Year, nextDayForPayment.Month, dayForPayment);
                    } else
                    {
                        nextDate = nextDayForPayment.AddMonths(1);
                        nextDate = new DateTime(nextDate.Year, nextDate.Month, 15);
                    }

                    //verify days habiles
                    if (nextDate.DayOfWeek == DayOfWeek.Saturday)
                    {
                        nextDate = nextDate.AddDays(-1);
                    }

                    if (nextDate.DayOfWeek == DayOfWeek.Sunday)
                    {
                        nextDate = nextDate.AddDays(-2);
                    }

                    dayForPayment = nextDate.Subtract(nextDayForPayment).Days;

                    var interest = finalBalance * dayForPayment * Math.Round(.6 / 360, 6);
                    var vat = interest * .16;

                    interest = interest < 0 ? 0 : Math.Round(interest, 2);
                    vat = vat < 0 ? 0 : Math.Round(vat, 2);

                    if (principalPayment > 0)
                    {
                        detailsAdvances.Add(new Advance()
                        {
                            Accredited_Id = accredited.id,
                            Amount = Math.Round(principalPayment, 2),
                            Requested_Day = dayForPayment,
                            Comission = 0,
                            Total_Withhold = Math.Round(totalDiscountForPaymentDetails, 2),
                            Day_For_Payment = dayForPayment,
                            Interest = interest,
                            Vat = vat,
                            Subtotal = interest + vat,
                            Limit_Date = nextDayForPayment.AddDays(dayForPayment),
                            Date_Advance = nextDayForPayment,
                            Interest_Rate = accredited.Interest_Rate,
                            Initial = Math.Round(amounForNextDayPayment - detailsAmountPayment, 2),
                            Final = Math.Round(finalBalance, MidpointRounding.AwayFromZero)
                        });

                        totalAdvances -= principalPayment;
                    } else
                    {
                        totalAdvances = 0;
                    }

                    index++;
                }
            }





























            Advance principalAdvance = firtsAdvance.CloneJson<Advance>();

            if (advanceDetails.Count > 0)
            {
                var details = advanceDetails.Where(p => p.Limit_Date.Date == firtsAdvance.Limit_Date.Date);

                firtsAdvance.Amount += details.Sum(p => p.Initial);
                firtsAdvance.Comission += details.Sum(p => p.Comission);
                firtsAdvance.Interest += details.Sum(p => p.Interest);
                firtsAdvance.Vat += details.Sum(p => p.Vat);
                firtsAdvance.Subtotal += details.Sum(p => p.Subtotal);
                calculateAmount.Amount = firtsAdvance.Amount;
                firtsAdvance.Total_Withhold = firtsAdvance.Amount + firtsAdvance.Subtotal + firtsAdvance.Vat;
            }

            double grossPercentage = Convert.ToDouble(this._ConfigutarionRetrieveService.Where(p => p.Configuration_Name == "GROSS_MONTHLY_SALARY_PERCENTAGE").FirstOrDefault().Configuration_Value) / 100;
            double netPercentage = Convert.ToDouble(this._ConfigutarionRetrieveService.Where(p => p.Configuration_Name == "NET_MONTHLY_SALARY_PERCENTAGE").FirstOrDefault().Configuration_Value) / 100;
            double maximumAmountDiscountByMonth = Math.Round((accredited.Gross_Monthly_Salary * grossPercentage) - accredited.Other_Obligations, 2);
            
            int numberPayments = Convert.ToInt32(Math.Round(firtsAdvance.Total_Withhold / maximumAmountDiscountByPeriod));

            if (numberPayments < 2 && firtsAdvance.Total_Withhold > maximumAmountDiscountByPeriod)
                numberPayments = 2;

            int numberPaymentsTotal = Convert.ToInt32(Math.Round(firtsAdvance.Total_Withhold / maximumAmountDiscountByPeriod) + 1); // 2

            double amountNumberPayment = firtsAdvance.Amount / numberPayments; // 800

            if (amountNumberPayment + firtsAdvance.Subtotal + firtsAdvance.Vat < maximumAmountDiscountByPeriod) // 874.62
            {
                firtsAdvance.Total_Withhold = Math.Round(amountNumberPayment + firtsAdvance.Subtotal + firtsAdvance.Vat, 2); // 874.62
                firtsAdvance.Amount = amountNumberPayment; // 800
            }
            else
            {
                firtsAdvance.Total_Withhold = maximumAmountDiscountByPeriod;
                firtsAdvance.Amount = Math.Round(firtsAdvance.Total_Withhold - firtsAdvance.Subtotal - firtsAdvance.Vat, 2);
            }

            firtsAdvance.Initial = calculateAmount.Amount; //1600
            firtsAdvance.Final = Math.Round(firtsAdvance.Initial - firtsAdvance.Amount, MidpointRounding.AwayFromZero); // 800


            List<Advance> advances = new List<Advance>();
            firtsAdvance.Date_Advance = DateTime.Now;

            advances.Add(firtsAdvance);

            int endDay = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);

            for (int i = 1; i < numberPaymentsTotal; i++)
            {

                if (i + 1 == numberPaymentsTotal)
                    amountNumberPayment = Math.Round(calculateAmount.Amount - advances.Sum(p => p.Amount), 2);

                Advance advanceAux = advances[i - 1];

                DateTime dateNextPayment = advanceAux.Limit_Date;
                int dayForPayment = 0;
                var nextCommission = 0;
                double vat = Convert.ToDouble(this._ConfigutarionRetrieveService.Where(p => p.Configuration_Name == "VAT").FirstOrDefault().Configuration_Value) / 100;
                double amount = 0;
                double total = 0;
                var commissionPeriodId = this._PeriodCommissionRetrieve.Where(p => p.Period_Id == accredited.Period_Id && p.Type_Month == endDay).FirstOrDefault().id;
                var commissionPerioDetail = this._PeriodCommissionDetailRetrieve.Where(p => p.Period_Commission_Id == commissionPeriodId && p.Day_Month == dateNextPayment.Day).FirstOrDefault();
                dayForPayment = commissionPerioDetail.Day_Payement;

                double interestAux = (advanceAux.Initial > advanceAux.Amount && advanceAux.Total_Withhold > 0) ? advanceAux.Final * dayForPayment * 0.0017 : 0;
                double interestAux2 = ((accredited.Net_Monthly_Salary / 2) > 0 && advanceAux.Total_Withhold == 0) ? advanceAux.Final * dayForPayment * 0.0017 : 0;
                var nextInterest = Math.Round(nextCommission * dayForPayment * 0.0017 + interestAux + interestAux2, 2);

                var nextVat = Math.Round((advanceAux.Comission + nextInterest) * vat, 2);

                if (amountNumberPayment + nextInterest + nextVat < maximumAmountDiscountByPeriod)
                {
                    total = Math.Round(amountNumberPayment + nextInterest + nextVat, 2);
                    amount = amountNumberPayment;
                }
                else
                {
                    total = maximumAmountDiscountByPeriod;
                    amount = Math.Round(total - nextInterest - nextVat, 2);
                }

                Advance advance = new Advance()
                {
                    Accredited_Id = accredited.id,
                    Amount = amount,
                    Requested_Day = dateNextPayment.Day,
                    Comission = nextCommission,
                    Total_Withhold = total,
                    Day_For_Payment = dayForPayment,
                    Interest = nextInterest,
                    Vat = nextVat,
                    Subtotal = nextInterest + nextVat,
                    Limit_Date = advanceAux.Limit_Date.AddDays(dayForPayment),
                    Date_Advance = advanceAux.Limit_Date,
                    Interest_Rate = accredited.Interest_Rate,
                    Initial = advanceAux.Final,
                    Final = Math.Round(advanceAux.Final - amount, MidpointRounding.AwayFromZero)
                };

                advances.Add(advance);
            }

            advances.Insert(0, principalAdvance);

            return advances;
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
                var accredited = this._AcreditedRetrieveService.Find(calculatePromotional.Accredited_Id);
                int finantialDay = Convert.ToInt32(this._ConfigutarionRetrieveService.Where(p => p.Configuration_Name == "FINANCIAL_DAYS").FirstOrDefault().Configuration_Value);
                double vat = Convert.ToDouble(this._ConfigutarionRetrieveService.Where(p => p.Configuration_Name == "VAT").FirstOrDefault().Configuration_Value) / 100;
                var advance = this._AdvanceRetrieveService.Find(calculatePromotional.Advance_Id);

                if (advance == null)
                    throw new SystemValidationException("No se encontró el adelando");

                advance.Promotional_Setting = calculatePromotional.Amount;
                advance.Day_Moratorium = DateTime.Now.Date > advance.Limit_Date.Date ?
                    (DateTime.Now.Date - advance.Limit_Date.Date).Days : 0;
                advance.Interest_Moratorium = DateTimeOffset.Now.Date > advance.Limit_Date.Date ?
                Math.Round(advance.Amount * (((double)accredited.Moratoruim_Interest_Rate / 100) / finantialDay) * advance.Day_Moratorium, 2) :
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

        public AdvanceDetail ExecuteProcess(CalculatePromotionalDetail calculatePromotional)
        {
            try
            {
                var accredited = this._AcreditedRetrieveService.Find(calculatePromotional.Accredited_Id);
                int finantialDay = Convert.ToInt32(this._ConfigutarionRetrieveService.Where(p => p.Configuration_Name == "FINANCIAL_DAYS").FirstOrDefault().Configuration_Value);
                double vat = Convert.ToDouble(this._ConfigutarionRetrieveService.Where(p => p.Configuration_Name == "VAT").FirstOrDefault().Configuration_Value) / 100;
                var advance = this._AdvanceDetailRetrieveService.Find(calculatePromotional.Advance_Id);

                if (advance == null)
                    throw new SystemValidationException("No se encontró el adelando");

                advance.Promotional_Setting = calculatePromotional.Amount;

                advance.Day_Moratorium = DateTime.Now.Date > advance.Limit_Date.Date ?
                    (DateTime.Now.Date - advance.Limit_Date.Date).Days : 0;
                advance.Interest_Moratorium = DateTimeOffset.Now.Date > advance.Limit_Date.Date ?
                Math.Round(advance.Amount * (((double)accredited.Moratoruim_Interest_Rate / 100) / finantialDay) * advance.Day_Moratorium, 2) :
                0;

                advance.Subtotal = advance.Interest + advance.Interest_Moratorium + advance.Comission;
                advance.Vat = Math.Round(advance.Subtotal * vat, 2);
                advance.Total_Withhold = Math.Round(advance.Amount + advance.Subtotal + advance.Vat + advance.Promotional_Setting, 2);

                return advance;
            }
            catch (Exception exception)
            {
                throw new SystemValidationException($"Error al calcular el Ajuste: {exception.Message}");
            }
        }
    }
}

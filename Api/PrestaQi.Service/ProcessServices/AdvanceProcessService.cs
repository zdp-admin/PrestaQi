﻿using InsiscoCore.Base.Data;
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
        IRetrieveRepository<Accredited> _AcreditedRetrieveService;
        IRetrieveService<Advance> _AdvanceRetrieveService;
        IRetrieveService<Configuration> _ConfigutarionRetrieveService;
        IRetrieveService<Period> _PeriodRetrieveService;
        IRetrieveService<PeriodCommission> _PeriodCommissionRetrieve;
        IRetrieveService<PeriodCommissionDetail> _PeriodCommissionDetailRetrieve;
        IRetrieveService<AdvanceDetail> _AdvanceDetailRetrieveService;
        IRetrieveService<DetailsAdvance> _DetailsAdvance;
        IRetrieveRepository<DetailsAdvance> _DetailsAdvanceRetrieveService;
        IRetrieveService<TypeContract> _TypeContractService;
        IRetrieveService<Institution> _InsitutionRetrieveService;
        IRetrieveService<DetailsByAdvance> _DetailsByAdvanceRetrieveService;
        IRetrieveRepository<PaidAdvance> _PaidAdvanceRepository;
        DateTime dateNow = new DateTime(2021, 9, 29, 10, 0, 0); //DateTime.Now;
        // solo cambiar la hora en el cuarto dia antes de la fecha de pago
        // liberar pago principal

        public AdvanceProcessService(
            IRetrieveRepository<Accredited> acreditedRetrieveService,
            IRetrieveService<Advance> advanceRetrieveService,
            IRetrieveService<Configuration> configurationRetrieveService,
            IRetrieveService<Period> periodRetrieveService,
            IRetrieveService<PeriodCommission> periodCommissionRetrieve,
            IRetrieveService<PeriodCommissionDetail> periodCommissionDetailRetrieve,
            IRetrieveService<AdvanceDetail> advanceDetailRetrieveService,
            IRetrieveService<DetailsAdvance> detailsAdvance,
            IRetrieveRepository<DetailsAdvance> detailsAdvanceRetrieveService,
            IRetrieveService<TypeContract> typeContractService,
            IRetrieveService<Institution> insitutionRetrieveService,
            IRetrieveService<DetailsByAdvance> detailsByAdvance,
            IRetrieveRepository<PaidAdvance> paidAdvanceRepository
            )
        {
            this._AcreditedRetrieveService = acreditedRetrieveService;
            this._AdvanceRetrieveService = advanceRetrieveService;
            this._ConfigutarionRetrieveService = configurationRetrieveService;
            this._PeriodRetrieveService = periodRetrieveService;
            this._PeriodCommissionRetrieve = periodCommissionRetrieve;
            this._PeriodCommissionDetailRetrieve = periodCommissionDetailRetrieve;
            this._AdvanceDetailRetrieveService = advanceDetailRetrieveService;
            this._DetailsAdvance = detailsAdvance;
            this._DetailsAdvanceRetrieveService = detailsAdvanceRetrieveService;
            this._TypeContractService = typeContractService;
            this._InsitutionRetrieveService = insitutionRetrieveService;
            this._DetailsByAdvanceRetrieveService = detailsByAdvance;
            this._PaidAdvanceRepository = paidAdvanceRepository;
        }

        public AdvanceAndDetails ExecuteProcess(CalculateAmount calculateAmount)
        {
            var accredited = this._AcreditedRetrieveService.Find(calculateAmount.Accredited_Id);
            accredited.Period_Name = this._PeriodRetrieveService.Where(periodo => periodo.id == accredited.Period_Id).First().Description;
            var result = CalculateAdvance(calculateAmount, accredited);

            double periodPercentage = Convert.ToDouble(this._ConfigutarionRetrieveService.Where(p => p.Configuration_Name == "GROSS_MONTHLY_SALARY_PERCENTAGE").FirstOrDefault().Configuration_Value) / 100;

            if (accredited.Period_Id == (int)PrestaQiEnum.PerdioAccredited.Quincenal) {
                periodPercentage = periodPercentage / 2;
            }

            if (accredited.Period_Id == (int)PrestaQiEnum.PerdioAccredited.Semanal) {
                periodPercentage = periodPercentage / 4;
            }

            double maximumAmountDiscountByPeriod = Math.Round(accredited.Gross_Monthly_Salary * periodPercentage, 2);

            if (accredited.Type_Contract_Id == (int)PrestaQiEnum.AccreditedContractType.AssimilatedToSalary || result.Item2)
                return new AdvanceAndDetails() { advance = result.Item1, details = new List<DetailsAdvance>() };

            var totalAdvance = this._AdvanceRetrieveService.Where(advance => advance.Accredited_Id == accredited.id && (advance.Paid_Status == 0 || advance.Paid_Status == 2)).OrderByDescending(advance => advance.id).ToList();


            if ((result.Item1.Total_Withhold + totalAdvance.Sum(advance => advance.Total_Withhold)) <= maximumAmountDiscountByPeriod)
                return new AdvanceAndDetails() { advance = result.Item1, details = new List<DetailsAdvance>() };

            var advances = new List<Advance>();
            var detailsAdvance = this._DetailsAdvance.Where(detail => detail.Accredited_Id == accredited.id && (detail.Paid_Status == 0 || detail.Paid_Status == 2)).OrderBy(detail => detail.id).ToList();

            var advanceOrderDesc = this._AdvanceRetrieveService.Where(advance => advance.Accredited_Id == accredited.id).OrderByDescending(a => a.id).ToList();

            foreach(Advance advance in advanceOrderDesc)
            {
                if (advances.Sum(a => a.Amount) < detailsAdvance.Sum(da => da.Principal_Payment))
                {
                    advances.Add(advance);
                } else
                {
                    break;
                }
            }

            if (advances.Count <= 0)
            {
                advances = totalAdvance;
            }

            int maxId = 1;

            if (advances.Count > 0)
            {
                maxId = advances.Max(advance => advance.id);
                result.Item1.id = maxId + 1;
            }

            advances.Add(result.Item1);

            var resultList = CalculateAdvanceList(advances, accredited, periodPercentage, maximumAmountDiscountByPeriod);

            result.Item1.id = 0;

            var sumAditionalVat = 0.0;

            /*if (resultList.Count > 0)
            {
                foreach(DetailsAdvance detail in resultList)
                {
                    if (detail.Interest > 0)
                    {
                        sumAditionalVat += detail.Interest / advances.Count;
                    }

                    if (detail.Vat > 0)
                    {
                        sumAditionalVat += detail.Vat / advances.Count;
                    }
                }

                result.Item1.Total_Withhold += Math.Round(sumAditionalVat, 2);
            }*/

            return new AdvanceAndDetails() { advance = result.Item1, details = resultList };
        }

        private (Advance, bool) CalculateAdvance(CalculateAmount calculateAmount, Accredited accredited)
        {
            Advance advanceCalculated = new Advance();
            bool isMaxAmount = false;
           
            int commission = Convert.ToInt32(this._ConfigutarionRetrieveService.Where(p => p.Configuration_Name == "INITIAL_COMMISSION").FirstOrDefault().Configuration_Value);
            double annualInterestRate = Convert.ToInt32(this._ConfigutarionRetrieveService.Where(c => c.Configuration_Name == "ANNUAL_INTEREST").FirstOrDefault().Configuration_Value);
            double annualInterest = ((double)annualInterestRate / 100); //((double)accredited.Interest_Rate / 100);
            int finantialDay = Convert.ToInt32(this._ConfigutarionRetrieveService.Where(p => p.Configuration_Name == "FINANCIAL_DAYS").FirstOrDefault().Configuration_Value);
            double vat = Convert.ToDouble(this._ConfigutarionRetrieveService.Where(p => p.Configuration_Name == "VAT").FirstOrDefault().Configuration_Value) / 100;
            
            var period = this._PeriodRetrieveService.Find(accredited.Period_Id);

            if (accredited == null)
                throw new SystemValidationException("Accredited not found");

            DateTime currentDate = dateNow;
            bool periodIsPaying = false;
            DateTime datePeriodInitial = currentDate;
            DateTime datePeriodFinish = currentDate;

            var currentPeriod = Utilities.getPeriodoByAccredited(accredited, currentDate);
            datePeriodInitial = currentPeriod.initial;
            datePeriodFinish = currentPeriod.finish;

            periodIsPaying = this._PaidAdvanceRepository
                .Where(paidAdvance => paidAdvance.Company_Id == accredited.Company_Id)
                .Where(paidAdvance => paidAdvance.created_at >= datePeriodInitial && paidAdvance.created_at <= datePeriodFinish).FirstOrDefault() != null;

            periodIsPaying = false;
            DateTime limitDate = datePeriodFinish;

            switch (accredited.Period_Id)
            {
                case (int)PrestaQiEnum.PerdioAccredited.Quincenal:

                    /*Check is available day*/
                    if (limitDate.DayOfWeek == DayOfWeek.Saturday)
                    {
                        limitDate = limitDate.AddDays(-1);
                    }

                    if (limitDate.DayOfWeek == DayOfWeek.Sunday)
                    {
                        limitDate = limitDate.AddDays(-2);
                    }

                    break;
                case (int)PrestaQiEnum.PerdioAccredited.Mensual:

                    /*Check is available day*/
                    if (limitDate.DayOfWeek == DayOfWeek.Saturday)
                    {
                        limitDate = limitDate.AddDays(-1);
                    }

                    if (limitDate.DayOfWeek == DayOfWeek.Sunday)
                    {
                        limitDate = limitDate.AddDays(-2);
                    }

                    break;
            }

            if (periodIsPaying || (limitDate - currentDate.Date).Days <= 3)
            {
                currentPeriod = Utilities.getPeriodoByAccredited(accredited, datePeriodFinish.AddDays(1));
                datePeriodInitial = currentPeriod.initial;
                datePeriodFinish = currentPeriod.finish;
            } else if ((limitDate - currentDate).Days == 4)
            {
                if (dateNow.Hour >= 12)
                {
                    currentPeriod = Utilities.getPeriodoByAccredited(accredited, datePeriodFinish.AddDays(1));
                    datePeriodInitial = currentPeriod.initial;
                    datePeriodFinish = currentPeriod.finish;
                }
            }

            switch (accredited.Period_Id)
            {
                case (int)PrestaQiEnum.PerdioAccredited.Quincenal:

                    /*Check is available day*/
                    if (limitDate.DayOfWeek == DayOfWeek.Saturday)
                    {
                        limitDate = limitDate.AddDays(-1);
                    }

                    if (limitDate.DayOfWeek == DayOfWeek.Sunday)
                    {
                        limitDate = limitDate.AddDays(-2);
                    }

                    break;
                case (int)PrestaQiEnum.PerdioAccredited.Mensual:

                    /*Check is available day*/
                    if (limitDate.DayOfWeek == DayOfWeek.Saturday)
                    {
                        limitDate = limitDate.AddDays(-1);
                    }

                    if (limitDate.DayOfWeek == DayOfWeek.Sunday)
                    {
                        limitDate = limitDate.AddDays(-2);
                    }

                    break;
            }

            DateTime startDate = currentDate;
            DateTime endDate = currentDate;
            
            int day = currentDate.Day;
            PeriodCommissionDetail commissionPerioDetail = new PeriodCommissionDetail();

            int endDay = DateTime.DaysInMonth(currentDate.Year, currentDate.Month);

            if (accredited.Period_Id != (int)PrestaQiEnum.PerdioAccredited.Semanal)
            {
                var commissionPeriodId = this._PeriodCommissionRetrieve.Where(p => p.Period_Id == accredited.Period_Id && p.Type_Month == endDay).FirstOrDefault().id;
                commissionPerioDetail = this._PeriodCommissionDetailRetrieve.Where(p => p.Period_Commission_Id == commissionPeriodId && p.Day_Month == currentDate.Day).FirstOrDefault();
                commission = Convert.ToInt32(commissionPerioDetail.Commission);
                advanceCalculated.Day_For_Payment = commissionPerioDetail.Day_Payement;
            }

            switch (accredited.Period_Id)
            {
                case (int)PrestaQiEnum.PerdioAccredited.Quincenal:

                    limitDate = datePeriodFinish;

                    /*Check is available day*/
                    if (limitDate.DayOfWeek == DayOfWeek.Saturday)
                    {
                        limitDate = limitDate.AddDays(-1);
                    }

                    if (limitDate.DayOfWeek == DayOfWeek.Sunday)
                    {
                        limitDate = limitDate.AddDays(-2);
                    }
                    /*Check is available day*/

                    if (calculateAmount.Amount == 0)
                    {
                        advanceCalculated.Maximum_Amount = Math.Round(accredited.Net_Monthly_Salary / 2);
                        isMaxAmount = true;
                    }

                    startDate = datePeriodInitial;
                    endDate = datePeriodFinish;

                    break;
                case (int)PrestaQiEnum.PerdioAccredited.Mensual:
                    
                    limitDate = datePeriodFinish;

                    /*Check is available day*/
                    if (limitDate.DayOfWeek == DayOfWeek.Saturday)
                    {
                        limitDate = limitDate.AddDays(-1);
                    }

                    if (limitDate.DayOfWeek == DayOfWeek.Sunday)
                    {
                        limitDate = limitDate.AddDays(-2);
                    }
                    /*Check is available day*/

                    if (calculateAmount.Amount == 0)
                    {
                        isMaxAmount = true;
                        advanceCalculated.Maximum_Amount = Math.Round(accredited.Net_Monthly_Salary);
                    }

                    startDate = datePeriodInitial;
                    endDate = datePeriodFinish;

                    break;
                case (int)PrestaQiEnum.PerdioAccredited.Semanal:

                    startDate = datePeriodInitial;
                    endDate = datePeriodFinish;
                    limitDate = endDate;

                    if (!(dateNow.Date >= endDate.AddDays(-2).Date))
                    {
                        commission = commission + ((dateNow.Date - startDate.Date).Days);
                    }

                    if (calculateAmount.Amount == 0)
                    {
                        isMaxAmount = true;
                        advanceCalculated.Maximum_Amount = Math.Round(accredited.Net_Monthly_Salary / 4);
                    }

                    advanceCalculated.Day_For_Payment = (limitDate.Date - currentDate.Date).Days;
                    break;
            }

            var advances = this._AdvanceRetrieveService.Where(p => p.Accredited_Id == accredited.id &&
            ((p.Date_Advance.Date >= startDate.Date && p.Date_Advance <= endDate.Date && p.Paid_Status == 0) || (p.Date_Advance < endDate && p.Paid_Status == 0))).ToList();

            var advancesIds = new List<int>();
            var detailsAdvancesPaided = 0d;

            if (advances.Count > 0)
            {
                advancesIds = advances.Select(a => a.id).ToList();

                var detailsAdvances = this._DetailsAdvance.Where(da => advancesIds.Contains(da.Advance_Id)).Where(da => da.Paid_Status == (int)PrestaQiEnum.AdvanceStatus.Pagado).ToList();

                if (detailsAdvances.Count > 0)
                {
                    detailsAdvancesPaided = detailsAdvances.Sum(da => da.Total_Payment + (da.Promotional_Setting ?? 0));
                }
            }

            advanceCalculated.Maximum_Amount = Math.Round(advances.Count > 0 ? advanceCalculated.Maximum_Amount - advances.Sum(p => p.Total_Withhold) : advanceCalculated.Maximum_Amount);
            if (advances.Count > 0)
            {
                advanceCalculated.Maximum_Amount += detailsAdvancesPaided;
            }

            if (isMaxAmount)
                calculateAmount.Amount = advanceCalculated.Maximum_Amount;

            double intereset = (accredited.Period_Id == (int)PrestaQiEnum.PerdioAccredited.Mensual) || (accredited.Period_Id == (int)PrestaQiEnum.PerdioAccredited.Quincenal) ?
                Math.Round(calculateAmount.Amount * ((annualInterest / finantialDay) * commissionPerioDetail.Day_Payement), 2) :
                Math.Round(calculateAmount.Amount * ((annualInterest / finantialDay) * (limitDate.Date - currentDate.Date).Days), 2);

            intereset = Math.Round(calculateAmount.Amount * ((annualInterest / finantialDay) * (limitDate.Date - currentDate.Date).Days), 2);

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
            advanceCalculated.Date_Advance = dateNow;
            advanceCalculated.Interest_Rate = accredited.Interest_Rate;
            advanceCalculated.Maximum_Amount = isMaxAmount ? Math.Round(advanceCalculated.Maximum_Amount - subtotal - vatTotal) : 0;
            advanceCalculated.Maximum_Amount = advanceCalculated.Maximum_Amount < 0 ? 0 : advanceCalculated.Maximum_Amount;
            advanceCalculated.created_at = dateNow;

            if (accredited.Net_Monthly_Salary == 0)
            {
                advanceCalculated.Maximum_Amount = 0;
                isMaxAmount = true;
            }

            return (advanceCalculated, isMaxAmount);
        }

        private List<DetailsAdvance> CalculateAdvanceList(List<Advance> advances, Accredited accredited,
            double periodPercentage, double maximumAmountDiscountByPeriod)
        {
            advances.Sort(delegate(Advance x, Advance y) { return x.created_at > y.created_at ? 1 : 0; });
            double gross_percentage = periodPercentage;

            double annualInterestRate = Convert.ToInt32(this._ConfigutarionRetrieveService.Where(c => c.Configuration_Name == "ANNUAL_INTEREST").FirstOrDefault().Configuration_Value);
            double annualInterest = ((double)annualInterestRate / 100);
            double vatConfig = Convert.ToInt32(this._ConfigutarionRetrieveService.Where(c => c.Configuration_Name == "VAT").FirstOrDefault().Configuration_Value);
            double FINANCIAL_DAYS = Convert.ToInt32(this._ConfigutarionRetrieveService.Where(c => c.Configuration_Name == "FINANCIAL_DAYS").FirstOrDefault().Configuration_Value);

            double discountPeriod = 1;
            switch (accredited.Period_Id)
            {
                case (int)PrestaQiEnum.PerdioAccredited.Semanal:
                    discountPeriod = .25;
                    break;
                case (int)PrestaQiEnum.PerdioAccredited.Quincenal:
                    discountPeriod = .5;
                    break;
                default:
                    discountPeriod = 1;
                    break;
            }

            var datesToPayment = advances.Select(a => a.Limit_Date).ToList();


            double maxdiscount = maximumAmountDiscountByPeriod;
            double totalDisponible = (accredited.Gross_Monthly_Salary * gross_percentage) - accredited.Other_Obligations;
            double totalForPay = advances.Sum(advance => advance.Total_Withhold);
            int numberOfPayments = Convert.ToInt32(Math.Round(totalForPay / maxdiscount, 2));
            double residuo = totalForPay % maxdiscount;
            double totalAdvances = advances.Sum(advance => advance.Amount);

            List<DetailsAdvance> detailsAdvances = new List<DetailsAdvance>();
            int index = 0;

            DateTime nextDayForPayment = datesToPayment.Min();
            DateTime dateRequest = advances[0].created_at;

            if (numberOfPayments > 1 || residuo > 0)
            {
                while (totalAdvances > 0 || index < numberOfPayments)
                {

                    if (index > 0)
                    {
                        var currentPeriodDetails = Utilities.getPeriodoByAccredited(accredited, nextDayForPayment.AddDays(4));
                        nextDayForPayment = currentPeriodDetails.finish;
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

                    if (count > 0 && detailsAdvances.FirstOrDefault(detail => detail.Date_Payment == nextDayForPayment) != null
                        && detailsAdvances.FirstOrDefault(detail => detail.Date_Payment == nextDayForPayment).Total_Payment < maxdiscount)
                    {
                        totalAdvances += detailsAdvances.First(detail => detail.Date_Payment == nextDayForPayment).Principal_Payment;

                        detailsAdvances.RemoveAt(detailsAdvances.FindIndex(detail => detail.Date_Payment == nextDayForPayment));
                    }

                    //advances
                    var deductionsForNextDayPayment = advances.Where(advance => advance.Limit_Date <= nextDayForPayment).Sum(advance => advance.Comission + advance.Interest + advance.Vat);
                    var totalForNextDayPayment = advances.Where(advance => advance.Limit_Date <= nextDayForPayment).Sum(advance => advance.Total_Withhold);
                    var amounForNextDayPayment = advances.Where(advance => advance.Limit_Date <= nextDayForPayment).Sum(advance => advance.Amount);

                    //detailsAdvance
                    var detailsDeductionsTotal = detailsAdvances.Sum(da => da.Interest + da.Vat);
                    var detailsTotalPayment = detailsAdvances.Sum(da => da.Total_Payment);
                    var detailsAmountPayment = detailsAdvances.Sum(da => da.Principal_Payment);

                    var totalPrevAdvanceWithDeduccion = (amounForNextDayPayment + deductionsForNextDayPayment) + detailsDeductionsTotal;
                    double[] listTotalsToDiscount =
                    {
                        totalDisponible,
                        ((amounForNextDayPayment * discountPeriod) + ((deductionsForNextDayPayment + detailsDeductionsTotal) - detailsTotalPayment) + detailsAmountPayment),
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


                    var dayForPayment = accredited.Period_End_Date ?? 15;
                    var nextDate = dateNow;

                    var currentPeriod = Utilities.getPeriodoByAccredited(accredited, nextDayForPayment.AddDays(4));
                    nextDate = currentPeriod.finish;

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

                    var interest = finalBalance * dayForPayment * Math.Round(annualInterest / FINANCIAL_DAYS, 6);
                    var vat = interest * (vatConfig / 100);

                    interest = interest < 0 ? 0 : Math.Round(interest, 2);
                    vat = vat < 0 ? 0 : Math.Round(vat, 2);

                    if (principalPayment > 0)
                    {
                        DetailsAdvance detailsAdvanceNew = new DetailsAdvance()
                        {
                            Accredited_Id = accredited.id,
                            Principal_Payment = Math.Round(principalPayment, 2),
                            Total_Payment = Math.Round(totalDiscountForPaymentDetails, 2),
                            Interest = interest < 0 ? 0 : Math.Round(interest, 2),
                            Vat = vat < 0 ? 0 : Math.Round(vat, 2),
                            Date_Payment = nextDayForPayment,
                            Initial_Balance = Math.Round(amounForNextDayPayment - detailsAmountPayment, 2),
                            Final_Balance = Math.Round(finalBalance, 2),
                            Days_For_Payments = dayForPayment
                        };

                        if ((detailsAdvanceNew.Principal_Payment + detailsAdvanceNew.Total_Payment + detailsAdvanceNew.Interest + detailsAdvanceNew.Vat) > 0 )
                        {
                            detailsAdvances.Add(detailsAdvanceNew);
                            totalAdvances -= principalPayment;
                            totalAdvances = Math.Round(totalAdvances, 2);
                        }
                    } else
                    {
                        totalAdvances = 0;
                    }

                    index++;
                }
            }

            return detailsAdvances;
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
                advance.Day_Moratorium = dateNow.Date > advance.Limit_Date.Date ?
                    (dateNow.Date - advance.Limit_Date.Date).Days : 0;
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

                advance.Day_Moratorium = dateNow.Date > advance.Limit_Date.Date ?
                    (dateNow.Date - advance.Limit_Date.Date).Days : 0;
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
    
        public MyAdvances ExecuteProcess(int id)
        {
            MyAdvances myAdvances = new MyAdvances();
            myAdvances.Befores = new List<Advance>();
            myAdvances.Currents = new List<Advance>();
            var date = dateNow;
            var nextDayForPay = 15;

            if (date.Day > 15)
            {
                nextDayForPay = DateTime.DaysInMonth(date.Year, date.Month);
            }

            var acredited = this._AcreditedRetrieveService.Where(acredit => acredit.id == id).First();
            TypeContract typeContract = this._TypeContractService.Where(type => type.id == acredited.Type_Contract_Id).First();
            Period period = this._PeriodRetrieveService.Where(period => period.id == acredited.Period_Id).First();
            Institution institution = this._InsitutionRetrieveService.Where(institution => institution.id == acredited.Institution_Id).First();
            var advances = this._AdvanceRetrieveService.Where(advance => advance.Accredited_Id == id).ToList();
            var datesPeriod = Utilities.getPeriodoByAccredited(acredited, dateNow);

            if (typeContract.Code == "sueldoysalario")
            {
                var forPayment = this._DetailsAdvanceRetrieveService.Where(d => advances.Where(a => a.id == d.Advance_Id).FirstOrDefault() != null).Where(detail => detail.Date_Payment == datesPeriod.finish).FirstOrDefault();

                if (forPayment != null)
                {
                    myAdvances.For_Payment = forPayment;
                    myAdvances.For_Payment.Bank_Name = institution.Description;
                    myAdvances.For_Payment.Account_Number = acredited.Account_Number;
                }
            }

            foreach(Advance advance in advances)
            {
                advance.details = this._DetailsByAdvanceRetrieveService.Where(da => da.Advance_Id == advance.id).OrderBy(da => da.Detail_Id).ToList();

                advance.details.ForEach(d =>
                {
                    d.Detail = this._DetailsAdvanceRetrieveService.Where(da => da.id == d.Detail_Id).FirstOrDefault();
                });

                if (advance.Date_Advance >= datesPeriod.initial && advance.Date_Advance <= datesPeriod.finish)
                {
                    myAdvances.Currents.Add(advance);
                }
                else
                {
                    myAdvances.Befores.Add(advance);
                }
            }

            return myAdvances;
        }
    }
}

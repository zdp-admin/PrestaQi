using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using OpenXmlPowerTools;
using PrestaQi.Model;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Model.Dto.Output;
using PrestaQi.Model.Enum;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PrestaQi.Service.ProcessServices
{
    public class AccreditedProcessService : ProcessService<Accredited>
    {
        IRetrieveService<Company> _CompanyRetrieveService;
        IRetrieveService<Accredited> _AccreditedRetrieveService;
        IRetrieveService<Advance> _AdvanceRetrieveService;
        IRetrieveService<PaidAdvance> _PaidAdvanceRetrieveService;
        IRetrieveService<Configuration> _ConfigurationRetrieveService;
        IRetrieveService<AdvanceDetail> _AdvanceDetailRetrieveService;
        IRetrieveService<DetailsAdvance> _DetailsAdvance;
        IRetrieveService<DetailsByAdvance> _DetailsByAdvance;

        public AccreditedProcessService(
            IRetrieveService<Company> companyRetrieveService,
            IRetrieveService<Accredited> accreditedRetrieveService,
            IRetrieveService<Advance> advanceRetrieveService,
            IRetrieveService<PaidAdvance> paidAdvanceRetrieveService,
            IRetrieveService<Configuration> configurationRetrieveService,
            IRetrieveService<AdvanceDetail> advanceDetailRetrieveService,
            IRetrieveService<DetailsAdvance> detailsAdvance,
            IRetrieveService<DetailsByAdvance> detailsByAvance
            )
        {
            this._CompanyRetrieveService = companyRetrieveService;
            this._AccreditedRetrieveService = accreditedRetrieveService;
            this._AdvanceRetrieveService = advanceRetrieveService;
            this._PaidAdvanceRetrieveService = paidAdvanceRetrieveService;
            this._ConfigurationRetrieveService = configurationRetrieveService;
            this._AdvanceDetailRetrieveService = advanceDetailRetrieveService;
            this._DetailsAdvance = detailsAdvance;
            this._DetailsByAdvance = detailsByAvance;
        }

        public List<AdvanceReceivable> ExecuteProcess(AdvancesReceivableByFilter filter)
        {
            List<Accredited> accrediteds = new List<Accredited>();
            List<Company> companies = new List<Company>();

            int finantialDay = Convert.ToInt32(this._ConfigurationRetrieveService.Where(p => p.Configuration_Name == "FINANCIAL_DAYS").FirstOrDefault().Configuration_Value);
            double vat = Convert.ToDouble(this._ConfigurationRetrieveService.Where(p => p.Configuration_Name == "VAT").FirstOrDefault().Configuration_Value) / 100;
            var advances = this._AdvanceRetrieveService.Where(p => p.Paid_Status == 0 || p.Paid_Status == 2).ToList();
            var accreditIds = advances.Select(p => p.Accredited_Id).Distinct();

            accrediteds = this._AccreditedRetrieveService.RetrieveResult<Func<Accredited, bool>, List<Accredited>>(p => accreditIds.Contains(p.id) && p.Deleted_At == null);

            if (!string.IsNullOrEmpty(filter.Filter))
            {
                accrediteds = accrediteds.Where(p => accreditIds.Contains(p.id) && p.Deleted_At == null &&
                (p.First_Name.ToLower().Contains(filter.Filter.ToLower()) || p.Last_Name.ToLower().Contains(filter.Filter.ToLower()) ||
                p.Contract_number.ToLower().Contains(filter.Filter.ToLower()) ||
                p.Company_Name.ToLower().Contains(filter.Filter.ToLower()))).ToList();
            }

            var companyIds = accrediteds.Select(p => p.Company_Id).Distinct();
            companies = this._CompanyRetrieveService.Where(p => companyIds.Contains(p.id)).ToList();

            var detail = accrediteds.Select(accredited => new AdvanceReceivableAccredited()
            {
                Accredited_Id = accredited.id,
                Company_Id = accredited.Company_Id,
                Advances = advances.Where(p => p.Accredited_Id == accredited.id).ToList(),
                Id = accredited.Identify,
                Interest_Rate = accredited.Interest_Rate,
                Moratoruim_Interest_Rate = accredited.Moratoruim_Interest_Rate,
                NameComplete = $"{accredited.First_Name} {accredited.Last_Name}",
                Is_Blocked = accredited.Is_Blocked,
                TypeContractId = (int)accredited.Type_Contract_Id,
                Period_Id = accredited.Period_Id
            }).ToList();

            detail.ForEach(accredited =>
            {
                if (accredited.TypeContractId == (int)PrestaQiEnum.AccreditedContractType.AssimilatedToSalary)
                {
                    accredited.Advances.ForEach(advance =>
                    {
                        advance.Day_Moratorium = DateTime.Now.Date > advance.Limit_Date.Date ?
                            (DateTime.Now.Date - advance.Limit_Date).Days : 0;

                        advance.Interest_Moratorium = DateTimeOffset.Now.Date > advance.Limit_Date.Date ?
                        Math.Round(advance.Amount * (((double)accredited.Moratoruim_Interest_Rate / 100) / finantialDay) * advance.Day_Moratorium, 2) :
                        0;
                        advance.Subtotal = advance.Interest + advance.Interest_Moratorium + advance.Comission + advance.Promotional_Setting;
                        advance.Vat = Math.Round(advance.Subtotal * vat, 2);
                        advance.Total_Withhold = Math.Round(advance.Amount + advance.Subtotal + advance.Vat, 2);
                    });

                    accredited.Payment = Math.Round(accredited.Advances.Sum(p => (p == null ? 0 : p.Total_Withhold)), 2);
                }
                else
                {
                    var totalPayment = 0.0;
                    var nextDate = DateTime.Now;

                    if (accredited.Period_Id == (int)PrestaQiEnum.PerdioAccredited.Semanal)
                    {
                        var day = nextDate.Day;

                        if (day > 0)
                        {
                            nextDate = nextDate.AddDays(6 - day);
                        }
                    }

                    if (accredited.Period_Id == (int)PrestaQiEnum.PerdioAccredited.Mensual)
                    {
                        var dayInMonth = DateTime.DaysInMonth(nextDate.Year, nextDate.Month);
                        nextDate = new DateTime(nextDate.Year, nextDate.Month, dayInMonth);
                    }

                    if (accredited.Period_Id == (int)PrestaQiEnum.PerdioAccredited.Quincenal)
                    {
                        if (nextDate.Day >= 1 && nextDate.Day <= 15)
                        {
                            nextDate = new DateTime(nextDate.Year, nextDate.Month, 15);
                        } else
                        {
                            var dayInMonth = DateTime.DaysInMonth(nextDate.Year, nextDate.Month);
                            nextDate = new DateTime(nextDate.Year, nextDate.Month, dayInMonth);
                        }
                    }

                    var detailsAdvanceAll = this._DetailsAdvance.Where(da => da.Accredited_Id == accredited.Accredited_Id).ToList();

                    accredited.Advances.ForEach(advance =>
                    {
                        advance.details = this._DetailsByAdvance.Where(da => da.Advance_Id == advance.id).OrderBy(da => da.Detail_Id).ToList();

                        if (advance.details.Count > 0)
                        {
                            advance.details.ForEach(d =>
                            {
                                d.Detail = detailsAdvanceAll.Where(da => da.id == d.Detail_Id).FirstOrDefault();
                                if (d.Detail != null)
                                {
                                    d.Detail.Total_Payment += double.Parse((d.Detail.Promotional_Setting ?? 0).ToString());
                                    if (d.Detail.Date_Payment <= nextDate && d.Detail.Paid_Status != (int)PrestaQiEnum.AdvanceStatus.Pagado)
                                    {
                                        totalPayment += d.Detail.Total_Payment;
                                    }
                                }
                            });
                        } else
                        {
                            advance.Day_Moratorium = DateTime.Now.Date > advance.Limit_Date.Date ?
                            (DateTime.Now.Date - advance.Limit_Date).Days : 0;

                            advance.Interest_Moratorium = DateTimeOffset.Now.Date > advance.Limit_Date.Date ?
                            Math.Round(advance.Amount * (((double)accredited.Moratoruim_Interest_Rate / 100) / finantialDay) * advance.Day_Moratorium, 2) :
                            0;
                            advance.Subtotal = advance.Interest + advance.Interest_Moratorium + advance.Comission + advance.Promotional_Setting;
                            advance.Vat = Math.Round(advance.Subtotal * vat, 2);
                            advance.Total_Withhold = Math.Round(advance.Amount + advance.Subtotal + advance.Vat, 2);
                            totalPayment += advance.Total_Withhold;
                        }
                    });

                    accredited.Payment = Math.Round(totalPayment, 2);
                }
            });

            var result = companies.Select(company => new AdvanceReceivable()
            {
                Company_Id = company.id,
                Company = company.Description,
                Accrediteds = detail.Where(p => p.Company_Id == company.id).ToList(),
                Contract_Number = accrediteds.FirstOrDefault(p => p.Company_Id == company.id).Contract_number,
                Amount = detail.Where(p => p.Company_Id == company.id).Sum(z => z.Payment) -
                    this._PaidAdvanceRetrieveService.Where(p => p.Company_Id == company.id &&
                    p.Is_Partial).Sum(z => z.Amount),
                Partial_Amount = this._PaidAdvanceRetrieveService.Where(p => p.Company_Id == company.id && 
                    p.Is_Partial).Sum(z => z.Amount),
                 
            }).ToList();

            return result;
        }
    }
}

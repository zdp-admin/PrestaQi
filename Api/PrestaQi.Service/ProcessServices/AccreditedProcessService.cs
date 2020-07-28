using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using OpenXmlPowerTools;
using PrestaQi.Model;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Model.Dto.Output;
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

        public AccreditedProcessService(
            IRetrieveService<Company> companyRetrieveService,
            IRetrieveService<Accredited> accreditedRetrieveService,
            IRetrieveService<Advance> advanceRetrieveService,
            IRetrieveService<PaidAdvance> paidAdvanceRetrieveService,
            IRetrieveService<Configuration> configurationRetrieveService
            )
        {
            this._CompanyRetrieveService = companyRetrieveService;
            this._AccreditedRetrieveService = accreditedRetrieveService;
            this._AdvanceRetrieveService = advanceRetrieveService;
            this._PaidAdvanceRetrieveService = paidAdvanceRetrieveService;
            this._ConfigurationRetrieveService = configurationRetrieveService;
        }

        public List<AdvanceReceivable> ExecuteProcess(AdvancesReceivableByFilter filter)
        {
            List<Accredited> accrediteds = new List<Accredited>();
            List<Company> companies = new List<Company>();

            int finantialDay = Convert.ToInt32(this._ConfigurationRetrieveService.Where(p => p.Configuration_Name == "FINANCIAL_DAYS").FirstOrDefault().Configuration_Value);
            double vat = Convert.ToDouble(this._ConfigurationRetrieveService.Where(p => p.Configuration_Name == "VAT").FirstOrDefault().Configuration_Value) / 100;
            var advances = this._AdvanceRetrieveService.Where(p => p.Paid_Status == 0 || p.Paid_Status == 2).ToList();
            var accreditIds = advances.Select(p => p.Accredited_Id).Distinct();

            accrediteds = this._AccreditedRetrieveService.Where(p => accreditIds.Contains(p.id) && p.Deleted_At == null).ToList();

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
                Is_Blocked = accredited.Is_Blocked
            }).ToList();

            detail.ForEach(accredited =>
            {
                accredited.Advances.ForEach(advance =>
                {
                    advance.Day_Moratorium = DateTime.Now.Date > advance.Limit_Date.Date ?
                        (DateTime.Now.Date - advance.Limit_Date).Days : 0;
                    advance.Interest_Moratorium = DateTimeOffset.Now.Date > advance.Limit_Date.Date ?
                    Math.Round((advance.Amount * ((double)accredited.Moratoruim_Interest_Rate / 100) / finantialDay) * advance.Day_Moratorium, 2) :
                    0;
                    advance.Subtotal = advance.Interest + advance.Interest_Moratorium + advance.Comission + advance.Promotional_Setting;
                    advance.Vat = Math.Round(advance.Subtotal * vat, 2);
                    advance.Total_Withhold = Math.Round(advance.Amount + advance.Subtotal + advance.Vat, 2);
                });

                accredited.Payment = Math.Round(accredited.Advances.Sum(p => p.Total_Withhold), 2);
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

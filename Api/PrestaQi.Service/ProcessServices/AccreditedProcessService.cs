using InsiscoCore.Base.Data;
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
    public class AccreditedProcessService : ProcessService<Accredited>
    {
        IRetrieveService<Company> _CompanyRetrieveService;
        IRetrieveService<Accredited> _AccreditedRetrieveService;
        IRetrieveService<Advance> _AdvanceRetrieveService;

        public AccreditedProcessService(
            IRetrieveService<Company> companyRetrieveService,
            IRetrieveService<Accredited> accreditedRetrieveService,
            IRetrieveService<Advance> advanceRetrieveService
            )
        {
            this._CompanyRetrieveService = companyRetrieveService;
            this._AccreditedRetrieveService = accreditedRetrieveService;
            this._AdvanceRetrieveService = advanceRetrieveService;
        }

        public List<AdvanceReceivable> ExecuteProcess(bool parameter)
        {
            var advances = this._AdvanceRetrieveService.Where(p => p.Paid_Status == 0 || p.Paid_Status == 2).ToList();
            var accreditIds = advances.Select(p => p.Accredited_Id).Distinct();
            var accrediteds = this._AccreditedRetrieveService.Where(p => accreditIds.Contains(p.id)).ToList();
            var companyIds = accrediteds.Select(p => p.Company_Id).Distinct();
            var companies = this._CompanyRetrieveService.Where(p => companyIds.Contains(p.id)).ToList();

           var detail = accrediteds.Select(accredited => new AdvanceReceivableAccredited()
            {
                Company_Id = accredited.Company_Id,
                Advances = advances.Where(p => p.Accredited_Id == accredited.id).ToList(),
                Id = accredited.Identify,
                Amount = advances.Where(p => p.Accredited_Id == accredited.id).FirstOrDefault().Amount,
                Comission = advances.Where(p => p.Accredited_Id == accredited.id).FirstOrDefault().Comission,
                Date_Advance = advances.Where(p => p.Accredited_Id == accredited.id).FirstOrDefault().Date_Advance,
                Interest_Rate = accredited.Interest_Rate,
                NameComplete = $"{accredited.First_Name} {accredited.Last_Name}",
                Payment = advances.Where(p => p.Accredited_Id == accredited.id).FirstOrDefault().Total_Withhold,
                Requested_Day = advances.Where(p => p.Accredited_Id == accredited.id).FirstOrDefault().Requested_Day
            }).ToList();

            var result = companies.Select(company => new AdvanceReceivable()
            {
                Company = company.Description,
                Accrediteds = detail.Where(p => p.Company_Id == company.id).ToList(),
                Contract_Number = accrediteds.FirstOrDefault(p => p.Company_Id == company.id).Contract_number,
                Amount = detail.Where(p => p.Company_Id == company.id).Sum(z => z.Payment)
            }).ToList();

            return result;
        }
    }
}

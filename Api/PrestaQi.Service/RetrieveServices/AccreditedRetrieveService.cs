using InsiscoCore.Base.Data;
using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using PrestaQi.Model;
using PrestaQi.Model.Configurations;
using PrestaQi.Model.Dto.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PrestaQi.Service.RetrieveServices
{
    public class AccreditedRetrieveService : RetrieveService<Accredited>
    {
        IRetrieveService<Period> _PeriodRetrieveService;
        IRetrieveService<Company> _CompanyRetrieveService;

        public AccreditedRetrieveService(
            IRetrieveRepository<Accredited> repository,
            IRetrieveService<Period> periodRetrieveService,
            IRetrieveService<Company> companyRetrieveService
            ) : base(repository)
        {
            this._PeriodRetrieveService = periodRetrieveService;
            this._CompanyRetrieveService = companyRetrieveService;
        }

        public override IEnumerable<Accredited> Where(Func<Accredited, bool> predicate)
        {
            var list = this._Repository.Where(predicate).ToList();
            var periods = this._PeriodRetrieveService.Where(p => p.User_Type == 2).ToList();
            var companies = this._CompanyRetrieveService.Where(p => true).ToList();

            list.ForEach(p =>
            {
                p.Period_Name = periods.FirstOrDefault(period => period.id == p.Period_Id).Description;
                p.Company_Name = companies.FirstOrDefault(company => company.id == p.Company_Id).Description;
            });

            return list;

        }
    }
}

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
    public class InvestorRetrieveService : RetrieveService<Investor>
    {
        IRetrieveService<Capital> _CapitalRetrieveService;
        public InvestorRetrieveService(
            IRetrieveRepository<Investor> repository,
            IRetrieveService<Capital> capitalRetrieveService
            ) : base(repository)
        {
            this._CapitalRetrieveService = capitalRetrieveService;
    }

        public override IEnumerable<Investor> Where(Func<Investor, bool> predicate)
        {
            var list = this._Repository.Where(predicate);
            return list;

        }
    }
}

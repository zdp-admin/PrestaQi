using InsiscoCore.Base.Data;
using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using PrestaQi.Model;
using PrestaQi.Model.Configurations;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Model.Dto.Output;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PrestaQi.Service.RetrieveServices
{
    public class InvestorRetrieveService : RetrieveService<Investor>
    {
        IRetrieveService<Period> _PeriodRetrieveService;
        IRetrieveService<Capital> _CapitalRetrieveService;

        public InvestorRetrieveService(
            IRetrieveRepository<Investor> repository,
            IRetrieveService<Capital> capitalRetrieveService,
            IRetrieveService<Period> periodRetrieveService
            ) : base(repository)
        {
            this._CapitalRetrieveService = capitalRetrieveService;
            this._PeriodRetrieveService = periodRetrieveService;
        }

        public override IEnumerable<Investor> Where(Func<Investor, bool> predicate)
        {
            var list = this._Repository.Where(predicate);

            foreach (var item in list)
            {
                if (item.Capitals != null)
                {
                    item.Capitals.ForEach(p =>
                    {
                        p.Period_Name = this._PeriodRetrieveService.Find(p.period_id).Description;
                    });
                }
            }
            return list;

        }

    }
}

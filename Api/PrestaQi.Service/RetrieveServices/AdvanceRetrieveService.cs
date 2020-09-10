using InsiscoCore.Base.Data;
using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using PrestaQi.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PrestaQi.Service.RetrieveServices
{
    public class AdvanceRetrieveService : RetrieveService<Advance>
    {
        IRetrieveService<AdvanceDetail> _AdvanceDetailRetrieveService;

        public AdvanceRetrieveService(IRetrieveRepository<Advance> repository,
            IRetrieveService<AdvanceDetail> advanceDetailRetrieveService) : base(repository)
        {
            _AdvanceDetailRetrieveService = advanceDetailRetrieveService;
        }

        public override IEnumerable<Advance> Where(Func<Advance, bool> predicate)
        {
            var result = base.Where(predicate).ToList();

            result.ForEach(p =>
            {
                p.AdvanceDetails = this._AdvanceDetailRetrieveService.Where(z => z.Advance_Id == p.id).ToList();
            });

            return result;
        }
    }
}

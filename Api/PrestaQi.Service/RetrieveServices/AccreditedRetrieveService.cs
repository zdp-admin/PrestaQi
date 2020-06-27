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

        public AccreditedRetrieveService(
            IRetrieveRepository<Accredited> repository,
            IRetrieveService<Period> periodRetrieveService
            ) : base(repository)
        {
            this._PeriodRetrieveService = periodRetrieveService;
        }

        public override IEnumerable<Accredited> Where(Func<Accredited, bool> predicate)
        {
            var list = this._Repository.Where(predicate);
            var periods = this._PeriodRetrieveService.Where(p => p.Enabled == true).ToList();

            foreach (var item in list)
            {
                item.Period_Name = periods.FirstOrDefault(p => p.id == item.Period_Id).Description;
            }
            return list;

        }
    }
}

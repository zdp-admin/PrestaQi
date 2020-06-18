
using JabilCore.Base.Data;
using JabilCore.Base.Service;
using JabilCore.Service;
using PrestaQi.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Service.WriteServices
{
    public class PeriodWriteService : WriteService<Period>
    {
        IRetrieveService<Period> _PeriodRetrieveService;

        public PeriodWriteService(
            IWriteRepository<Period> repository,
            IRetrieveService<Period> periodRetrieveService
            ) : base(repository)
        {
            this._PeriodRetrieveService = periodRetrieveService;
        }

        public override bool Create(Period entity)
        {
            try
            {
                entity.created_at = DateTime.Now;
                entity.updated_at = DateTime.Now;

                return base.Create(entity);
            }
            catch (Exception exception)
            {
                throw;
            }
           
        }

        public override bool Update(Period entity)
        {
            try
            {
                Period entityFound = this._PeriodRetrieveService.Find(entity.id);

                if (entityFound != null)
                {
                    entity.updated_at = DateTime.Now;
                    entity.created_at = entityFound.created_at;

                    return base.Update(entity);
                }

                throw new Exception("NO_FOUND");
            }
            catch (Exception exception)
            {
                throw;
            }
            
        }
    }
}

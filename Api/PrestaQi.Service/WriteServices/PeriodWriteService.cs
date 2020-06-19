
using JabilCore.Base.Data;
using JabilCore.Base.Service;
using JabilCore.Service;
using PrestaQi.Model;
using PrestaQi.Model.Configurations;
using System;

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
                throw new SystemValidationException("Error creating Period!");
            }

        }

        public override bool Update(Period entity)
        {
            Period entityFound = this._PeriodRetrieveService.Find(entity.id);

            if (entityFound == null)
                throw new SystemValidationException("Period not found!");

            entity.updated_at = DateTime.Now;
            entity.created_at = entityFound.created_at;

            try
            {
                return base.Update(entity);
            }
            catch (Exception) { throw new SystemValidationException("Error updating Period!"); }
        }
    }
}

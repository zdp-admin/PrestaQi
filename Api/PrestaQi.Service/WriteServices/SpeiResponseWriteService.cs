using InsiscoCore.Base.Data;
using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using PrestaQi.Model;
using PrestaQi.Model.Configurations;
using PrestaQi.Model.Dto;
using PrestaQi.Model.Dto.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PrestaQi.Service.WriteServices
{
    public class SpeiResponseWriteService : WriteService<SpeiResponse>
    {
        IRetrieveService<SpeiResponse> _SpeiResponseRetrieveService;
        
        
        public SpeiResponseWriteService(
            IWriteRepository<SpeiResponse> repository,
            IRetrieveService<SpeiResponse> speiResponseRetrieveService
            ) : base(repository)
        {
            this._SpeiResponseRetrieveService = speiResponseRetrieveService;
        }

        public override bool Create(SpeiResponse entity)
        {
            try
            {
                entity.created_at = DateTime.Now;
                entity.updated_at = DateTime.Now;

                return base.Create(entity);
            }
            catch (Exception exception)
            {
                throw new SystemValidationException($"Error creating Spei: {exception.Message}");
            }

        }

        public bool Update(StateChange stateChange)
        {
            try
            {
                var speiResponse = this._SpeiResponseRetrieveService.Where(p => p.tracking_id == stateChange.Id).FirstOrDefault();

                if (speiResponse == null)
                    throw new SystemValidationException($"Id: {stateChange.Id} Not found");

                speiResponse.State_Name = stateChange.Estado;
                if (stateChange.CausaDevolucion > 0)
                    speiResponse.Repayment_Id = stateChange.CausaDevolucion;

                return base.Update(speiResponse);
            }
            catch (Exception exception)
            {
                throw new SystemValidationException($"Error update Spei: {exception.Message}");
            }
        }

    }
}

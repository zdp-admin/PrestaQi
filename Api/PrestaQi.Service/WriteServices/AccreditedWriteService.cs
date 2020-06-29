using InsiscoCore.Base.Data;
using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using PrestaQi.Model;
using PrestaQi.Model.Configurations;
using PrestaQi.Model.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PrestaQi.Service.WriteServices
{
    public class AccreditedWriteService : WriteService<Accredited>
    {
        IRetrieveService<Accredited> _AccreditedRetrieveService;

        public AccreditedWriteService(
            IWriteRepository<Accredited> repository,
            IRetrieveService<Accredited> accreditedRetrieveService
            ) : base(repository)
        {
            this._AccreditedRetrieveService = accreditedRetrieveService;
        }

        public override bool Create(Accredited entity)
        {
            try
            {
                entity.Password = InsiscoCore.Utilities.Crypto.MD5.Encrypt(entity.Password);
                entity.created_at = DateTime.Now;
                entity.updated_at = DateTime.Now;

                return base.Create(entity);
            }
            catch (Exception exception)
            {
                throw new SystemValidationException("Error creating Accredited");
            }

        }

        public override bool Update(Accredited entity)
        {
            Accredited accredited = this._AccreditedRetrieveService.Find(entity.id);
            if (accredited == null)
                throw new SystemValidationException("Accredited not found");

            entity.updated_at = DateTime.Now;
            entity.created_at = entity.created_at;

            try
            {
                return base.Update(entity);
            }
            catch (Exception exception) { throw new SystemValidationException("Error updating Accredited!");  }
        }

        public override bool Create(IEnumerable<Accredited> entities)
        {
            try
            {
                entities.ToList().ForEach(p =>
                {
                    p.created_at = DateTime.Now;
                    p.updated_at = DateTime.Now;
                });

                return base.Create(entities);
            }
            catch (Exception exception)
            {
                throw new SystemValidationException("Error creating Accredited");
            }
            
        }
    }
}

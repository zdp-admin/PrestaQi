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
    public class SpeiResponseWriteService : WriteService<SpeiResponse>
    {
        public SpeiResponseWriteService(
            IWriteRepository<SpeiResponse> repository
            ) : base(repository)
        {
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

    }
}

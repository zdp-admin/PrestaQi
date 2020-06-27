using InsiscoCore.Base.Data;
using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using Microsoft.VisualBasic;
using PrestaQi.Model;
using PrestaQi.Model.Configurations;
using PrestaQi.Model.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PrestaQi.Service.WriteServices
{
    public class AdvanceWriteService : WriteService<Advance>
    {
        public AdvanceWriteService(
            IWriteRepository<Advance> repository
            ) : base(repository)
        {
        }

        public override bool Create(Advance entity)
        {
            try
            {
                entity.Date_Advance = DateTime.Now;
                entity.created_at = DateTime.Now;
                entity.updated_at = DateTime.Now;

                return base.Create(entity);
            }
            catch (Exception exception)
            {
                throw new SystemValidationException("Error creating Advance");
            }

        }

    }
}

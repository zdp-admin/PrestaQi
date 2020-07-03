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
    public class InstitutionWriteService : WriteService<Institution>
    {
        IRetrieveService<Institution> _InstitutionRetrieveService;

        public InstitutionWriteService(
            IWriteRepository<Institution> repository,
            IRetrieveService<Institution> institutionRetrieveService
            ) : base(repository)
        {
            this._InstitutionRetrieveService = institutionRetrieveService;
        }

        public override bool Create(Institution entity)
        {
            try
            {
                Institution institution = this._InstitutionRetrieveService.Where(p => p.Code == entity.Code).FirstOrDefault();
                if (institution != null)
                    throw new SystemValidationException("The Code is already registered");

                entity.created_at = DateTime.Now;
                entity.updated_at = DateTime.Now;

                return base.Create(entity);
            }
            catch (Exception exception)
            {
                throw new SystemValidationException($"Error creating Institution: {exception.Message}");
            }

        }

        public override bool Update(Institution entity)
        {
            Institution institution = this._InstitutionRetrieveService.Find(entity.id);
            if (institution == null)
                throw new SystemValidationException("Institution not found");

            entity.updated_at = DateTime.Now;
            entity.created_at = entity.created_at;

            try
            {
                return base.Update(entity);
            }
            catch (Exception exception) { throw new SystemValidationException($"Error updating Institution!: {exception.Message}");  }
        }

    }
}

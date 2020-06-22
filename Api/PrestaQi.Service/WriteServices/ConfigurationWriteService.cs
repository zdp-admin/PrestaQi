
using InsiscoCore.Base.Data;
using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using PrestaQi.Model;
using PrestaQi.Model.Configurations;
using System;

namespace PrestaQi.Service.WriteServices
{
    public class ConfigurationWriteService : WriteService<Configuration>
    {
        IRetrieveService<Configuration> _ConfigurationRetrieveService;

        public ConfigurationWriteService(
            IWriteRepository<Configuration> repository,
            IRetrieveService<Configuration> configurationRetrieveService
            ) : base(repository)
        {
            this._ConfigurationRetrieveService = configurationRetrieveService;
        }

        public override bool Create(Configuration entity)
        {
            try
            {
                entity.created_at = DateTime.Now;
                entity.updated_at = DateTime.Now;

                return base.Create(entity);
            }
            catch (Exception exception)
            {
                throw new SystemValidationException("Error creating Configuration!");
            }
           
        }

        public override bool Update(Configuration entity)
        {
            Configuration entityFound = this._ConfigurationRetrieveService.Find(entity.id);

            if (entityFound == null)
                throw new SystemValidationException("Configuration not found!");

            entity.updated_at = DateTime.Now;
            entity.created_at = entityFound.created_at;

            try
            {
                return base.Update(entity);
            }
            catch (Exception) { throw new SystemValidationException("Error updating Configuration!"); }
        }
    }
}

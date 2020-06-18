
using JabilCore.Base.Data;
using JabilCore.Base.Service;
using JabilCore.Service;
using PrestaQi.Model;
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
                throw;
            }
           
        }

        public override bool Update(Configuration entity)
        {
            try
            {
                Configuration entityFound = this._ConfigurationRetrieveService.Find(entity.id);

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

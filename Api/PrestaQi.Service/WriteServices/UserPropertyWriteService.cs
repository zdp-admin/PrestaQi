using JabilCore.Base.Data;
using JabilCore.Base.Service;
using JabilCore.Service;
using PrestaQi.Model;
using PrestaQi.Model.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PrestaQi.Service.WriteServices
{
    public class UserPropertyWriteService : WriteService<UserProperty>
    {
        IRetrieveService<UserProperty> _UserPropertyRetrieveService;

        public UserPropertyWriteService(
            IWriteRepository<UserProperty> repository,
            IRetrieveService<UserProperty> userPropertyRetrieveService
            ) : base(repository)
        {
            this._UserPropertyRetrieveService = userPropertyRetrieveService;
        }

        public override bool Create(UserProperty entity)
        {
            try
            {
                entity.created_at = DateTime.Now;
                entity.updated_at = DateTime.Now;

                return base.Create(entity);
            }
            catch (Exception exception)
            {
                throw new SystemValidationException("Error creating Property!");
            }
           
        }

        public override bool Update(UserProperty entity)
        {
            UserProperty entityFound = this._UserPropertyRetrieveService.Find(entity.id);
            if (entityFound == null)
                throw new SystemValidationException("Property not found");

            entityFound.updated_at = DateTime.Now;
            entityFound.Property_Name = entity.Property_Name;
            entityFound.Property_Value = entity.Property_Value;

            try
            {
                return base.Update(entity);
            }
            catch (Exception) { throw new SystemValidationException("Error updating Property!"); }
        }

        public override bool Create(IEnumerable<UserProperty> entities)
        {
            try
            {
                entities.ToList().ForEach(p => { p.created_at = DateTime.Now; p.updated_at = DateTime.Now; });
                return base.Create(entities);
            }
            catch (Exception exception)
            {
                throw new SystemValidationException("Error creating Periods!");
            }
        }

        public override bool Update(IEnumerable<UserProperty> entities)
        {
            List<UserProperty> userProperties = this._UserPropertyRetrieveService.Where(p => p.User_Id == entities.FirstOrDefault().User_Id).ToList();

            if (userProperties.Count == 0)
                throw new SystemValidationException("Properties not found");

            entities.ToList().ForEach(p =>
            {
                var entityFound = userProperties.Find(z => z.id == p.id);
                p.created_at = entityFound.created_at;
                p.updated_at = DateTime.Now;
            });

            try
            {
                return base.Update(entities);
            }
            catch (Exception) { throw new SystemValidationException("Error updating Configurations!"); }
        }
    }
}

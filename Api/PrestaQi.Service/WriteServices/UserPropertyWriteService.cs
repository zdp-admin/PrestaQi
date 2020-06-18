using JabilCore.Base.Data;
using JabilCore.Base.Service;
using JabilCore.Service;
using PrestaQi.Model;
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
                throw;
            }
           
        }

        public override bool Update(UserProperty entity)
        {
            try
            {
                UserProperty entityFound = this._UserPropertyRetrieveService.Find(entity.id);
                if (entityFound != null)
                {
                    entityFound.updated_at = DateTime.Now;
                    entityFound.Property_Name = entity.Property_Name;
                    entityFound.Property_Value = entity.Property_Value;

                    return base.Update(entityFound);
                }

                throw new Exception("NOT_FOUND");
            }
            catch (Exception exception)
            {
                throw;
            }
        }

        public override bool Create(IEnumerable<UserProperty> entities)
        {
            entities.ToList().ForEach(p => { p.created_at = DateTime.Now; p.updated_at = DateTime.Now; });
            return base.Create(entities);
        }

        public override bool Update(IEnumerable<UserProperty> entities)
        {
            List<UserProperty> userProperties = this._UserPropertyRetrieveService.Where(p => p.User_Id == entities.FirstOrDefault().User_Id).ToList();

            if (userProperties.Count > 0)
            {
                entities.ToList().ForEach(p =>
                {
                    var entityFound = userProperties.Find(z => z.id == p.id);
                    p.created_at = entityFound.created_at;
                    p.updated_at = DateTime.Now;
                });

                return base.Update(entities);
            }

            throw new Exception("NOT_FOUND");
        }
    }
}

using JabilCore.Base.Data;
using JabilCore.Base.Service;
using JabilCore.Service;
using PrestaQi.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Service.WriteServices
{
    public class UserTypeWriteService : WriteService<UserType>
    {
        IRetrieveService<UserType> _UserTypeRetrieveService;

        public UserTypeWriteService(
            IWriteRepository<UserType> repository,
            IRetrieveService<UserType> userTypeRetrieveService
            ) : base(repository)
        {
            this._UserTypeRetrieveService = userTypeRetrieveService;
        }

        public override bool Create(UserType entity)
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

        public override bool Update(UserType entity)
        {
            try
            {
                UserType entityFound = this._UserTypeRetrieveService.Find(entity.id);
                
                if (entityFound != null)
                {
                    entity.created_at = entityFound.created_at;
                    entity.updated_at = DateTime.Now;

                    return base.Update(entity);
                }

                throw new Exception("NOT_FOUND");
                
            }
            catch (Exception exception)
            {
                throw;
            }
            
        }
    }
}

using JabilCore.Base.Data;
using JabilCore.Base.Service;
using JabilCore.Service;
using PrestaQi.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Service.WriteServices
{
    public class UserWriteService : WriteService<User>
    {
        IRetrieveService<User> _UserRetrieveService;

        public UserWriteService(
            IWriteRepository<User> repository,
            IRetrieveService<User> userRetrieveService
            ) : base(repository)
        {
            this._UserRetrieveService = userRetrieveService;
        }

        public override bool Create(User entity)
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

        public override bool Update(User entity)
        {
            try
            {
                User user = this._UserRetrieveService.Find(entity.id);
                if (user != null)
                {
                    entity.updated_at = DateTime.Now;
                    entity.created_at = entity.created_at;

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

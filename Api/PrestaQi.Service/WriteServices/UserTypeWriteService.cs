using JabilCore.Base.Data;
using JabilCore.Base.Service;
using JabilCore.Service;
using PrestaQi.Model;
using PrestaQi.Model.Configurations;
using System;

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
                throw new SystemValidationException("Error creating User Type!");
            }
           
        }

        public override bool Update(UserType entity)
        {
            UserType entityFound = this._UserTypeRetrieveService.Find(entity.id);

            if (entityFound == null)
                throw new SystemValidationException("User Type not found!");

            entity.created_at = entityFound.created_at;
            entity.updated_at = DateTime.Now;

            try
            {
                return base.Update(entity);
            }
            catch (Exception) { throw new SystemValidationException("Error updating User Type!"); }
        }
    }
}

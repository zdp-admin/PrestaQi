using JabilCore.Base.Data;
using JabilCore.Base.Service;
using JabilCore.Service;
using PrestaQi.Model;
using PrestaQi.Model.Configurations;
using PrestaQi.Model.Dto;
using System;

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
                entity.Password = JabilCore.Utilities.Crypto.MD5.Encrypt(entity.Password);
                entity.created_at = DateTime.Now;
                entity.updated_at = DateTime.Now;

                return base.Create(entity);
            }
            catch (Exception exception)
            {
                throw new SystemValidationException("Error creating user");
            }
           
        }

        public override bool Update(User entity)
        {
            User user = this._UserRetrieveService.Find(entity.id);
            if (user == null)
                throw new SystemValidationException("User not found");

            entity.Password = user.Password;
            entity.updated_at = DateTime.Now;
            entity.created_at = entity.created_at;

            try
            {
                return base.Update(entity);
            }
            catch (Exception exception) { throw new SystemValidationException("Error updating password!");  }
        }

        public bool Update(ChangePassword changePassword)
        {
            var user = this._UserRetrieveService.Find(changePassword.User_Id);

            if (user == null)
                throw new SystemValidationException("User not found");

            user.updated_at = DateTime.Now;

            try
            {
                user.Password = JabilCore.Utilities.Crypto.MD5.Encrypt(changePassword.Password);

                return base.Update(user);
            }
            catch (Exception) { throw new SystemValidationException("Error change password!"); }
        }
    }
}

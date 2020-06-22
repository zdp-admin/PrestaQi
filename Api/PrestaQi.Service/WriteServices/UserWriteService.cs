using InsiscoCore.Base.Data;
using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using PrestaQi.Model;
using PrestaQi.Model.Configurations;
using PrestaQi.Model.Dto;
using System;
using System.Linq;

namespace PrestaQi.Service.WriteServices
{
    public class UserWriteService : WriteService<User>
    {
        IRetrieveService<User> _UserRetrieveService;
        IWriteService<UserProperty> _UserPropertyWriteService;

        public UserWriteService(
            IWriteRepository<User> repository,
            IRetrieveService<User> userRetrieveService,
            IWriteService<UserProperty> userPropertyWriteService
            ) : base(repository)
        {
            this._UserRetrieveService = userRetrieveService;
            this._UserPropertyWriteService = userPropertyWriteService;
        }

        public override bool Create(User entity)
        {
            try
            {
                entity.Password = InsiscoCore.Utilities.Crypto.MD5.Encrypt(entity.Password);
                entity.created_at = DateTime.Now;
                entity.updated_at = DateTime.Now;

                bool create = base.Create(entity);

                if (create)
                {
                    if (entity.User_Properties != null)
                    {
                        if (entity.User_Properties.Count > 0)
                        {
                            entity.User_Properties.ForEach(p => { p.User_Id = entity.id; });
                            this._UserPropertyWriteService.Create(entity.User_Properties);
                        }
                    }
                }

                return create;
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
                bool updated = base.Update(entity);

                if (updated)
                {
                    if (entity.User_Properties != null)
                    {
                        if (entity.User_Properties.Count > 0)
                        {
                            entity.User_Properties.ForEach(p => { p.User_Id = entity.id; });

                            var listNew = entity.User_Properties.Where(p => p.id == 0).ToList();
                            var listUpdate = entity.User_Properties.Where(p => p.id > 0).ToList();

                            this._UserPropertyWriteService.Create(listNew);
                            this._UserPropertyWriteService.Update(listUpdate);
                        }
                    }
                }

                return updated;
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
                user.Password = InsiscoCore.Utilities.Crypto.MD5.Encrypt(changePassword.Password);

                return base.Update(user);
            }
            catch (Exception) { throw new SystemValidationException("Error change password!"); }
        }
    }
}

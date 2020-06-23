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
    public class UserWriteService : WriteService<User>
    {
        IRetrieveService<User> _UserRetrieveService;
        IWriteService<UserModule> _UserModuleWriteService;
        IRetrieveService<UserModule> _UserModuloRetrieveService;

        public UserWriteService(
            IWriteRepository<User> repository,
            IRetrieveService<User> userRetrieveService,
            IWriteService<UserModule> userModuleWriteService,
            IRetrieveService<UserModule> userModuloRetrieveService
            ) : base(repository)
        {
            this._UserRetrieveService = userRetrieveService;
            this._UserModuleWriteService = userModuleWriteService;
            this._UserModuloRetrieveService = userModuloRetrieveService;
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
                    SaveModules(entity.Modules, entity.id);

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
                    SaveModules(entity.Modules, entity.id);

                return updated;
            }
            catch (Exception exception) { throw new SystemValidationException("Error updating User!");  }
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

        public override bool Create(IEnumerable<User> entities)
        {
            try
            {
                entities.ToList().ForEach(p =>
                {
                    p.created_at = DateTime.Now;
                    p.updated_at = DateTime.Now;
                });

                bool created = base.Create(entities);

                entities.ToList().ForEach(p =>
                {
                    SaveModules(p.Modules, p.id);
                });

                return created;
            }
            catch (Exception exception)
            {
                throw new SystemValidationException("Error creating users");
            }
            
        }

        void SaveModules(List<UserModule> userModules, int id)
        {
            if (userModules != null)
            {
                if (userModules.Count > 0)
                {
                    userModules.ForEach(p =>
                    {
                        p.user_id = id; p.created_at = DateTime.Now;
                        p.updated_at = DateTime.Now;
                    });
                    this._UserModuleWriteService.Create(userModules);
                }
            }
        }
    }
}

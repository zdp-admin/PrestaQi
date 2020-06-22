using InsiscoCore.Base.Data;
using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using PrestaQi.Model;
using PrestaQi.Model.Configurations;
using PrestaQi.Model.Dto;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PrestaQi.Service.WriteServices
{
    public class UserWriteService : WriteService<User>
    {
        IRetrieveService<User> _UserRetrieveService;
        IWriteService<UserProperty> _UserPropertyWriteService;
        IWriteService<UserModule> _UserModuleWriteService;
        IRetrieveService<UserModule> _UserModuloRetrieveService;

        public UserWriteService(
            IWriteRepository<User> repository,
            IRetrieveService<User> userRetrieveService,
            IWriteService<UserProperty> userPropertyWriteService,
            IWriteService<UserModule> userModuleWriteService,
            IRetrieveService<UserModule> userModuloRetrieveService
            ) : base(repository)
        {
            this._UserRetrieveService = userRetrieveService;
            this._UserPropertyWriteService = userPropertyWriteService;
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
                {
                    if (entity.User_Properties != null)
                    {
                        if (entity.User_Properties.Count > 0)
                        {
                            entity.User_Properties.ForEach(p => { p.User_Id = entity.id; });
                            this._UserPropertyWriteService.Create(entity.User_Properties);
                        }

                        if (entity.Modules != null)
                        {
                            if (entity.Modules.Count > 0)
                            {
                                entity.Modules.ForEach(p => { p.user_id = entity.id; p.created_at = DateTime.Now;
                                    p.updated_at = DateTime.Now;
                                });
                                this._UserModuleWriteService.Create(entity.Modules);
                            }
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

                            if (listNew.Count > 0) this._UserPropertyWriteService.Create(listNew);
                            if (listUpdate.Count > 0) this._UserPropertyWriteService.Update(listUpdate);
                        }

                        if (entity.Modules != null)
                        {
                            if (entity.Modules.Count > 0)
                            {
                                var listSaved = this._UserModuloRetrieveService.Where(p => p.user_id == entity.id);
                                if (listSaved.Count() > 0) this._UserModuleWriteService.Delete(listSaved);

                                entity.Modules.ForEach(p => { p.user_id = entity.id; p.created_at = DateTime.Now; p.updated_at = DateTime.Now; });
                                this._UserModuleWriteService.Create(entity.Modules);
                            }
                        }
                    }
                }

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
    }
}

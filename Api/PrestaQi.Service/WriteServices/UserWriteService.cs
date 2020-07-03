using InsiscoCore.Base.Data;
using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using Newtonsoft.Json;
using PrestaQi.Model;
using PrestaQi.Model.Configurations;
using PrestaQi.Model.Dto;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Service.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace PrestaQi.Service.WriteServices
{
    public class UserWriteService : WriteService<User>
    {
        IProcessService<User> _UserProcessService;
        IRetrieveService<User> _UserRetrieveService;
        IWriteService<UserModule> _UserModuleWriteService;
        IRetrieveService<Configuration> _ConfigurationRetrieveService;

        public UserWriteService(
            IWriteRepository<User> repository,
            IRetrieveService<User> userRetrieveService,
            IWriteService<UserModule> userModuleWriteService,
            IRetrieveService<Configuration> configurationRetrieveService,
            IProcessService<User> userProcessService
            ) : base(repository)
        {
            this._UserRetrieveService = userRetrieveService;
            this._UserModuleWriteService = userModuleWriteService;
            this._ConfigurationRetrieveService = configurationRetrieveService;
            this._UserProcessService = userProcessService;
        }

        public override bool Create(User entity)
        {
            this._UserProcessService.ExecuteProcess<string, bool>(entity.Mail);

            try
            {
                string password = Utilities.GetPasswordRandom();

                entity.Password = InsiscoCore.Utilities.Crypto.MD5.Encrypt(password);
                entity.created_at = DateTime.Now;
                entity.updated_at = DateTime.Now;

                bool create = base.Create(entity);

                if (create)
                {
                    SaveModules(entity.Modules, entity.id);

                    try
                    {
                        SendMail(entity.Mail, password);
                       
                    }
                    catch (Exception)
                    {
                    }
                }

                return create;
            }
            catch (Exception exception)
            {
                throw new SystemValidationException($"Error creating user: {exception.Message}");
            }

        }

        public override bool Update(User entity)
        {
            User user = this._UserRetrieveService.Find(entity.id);
            if (user == null)
                throw new SystemValidationException("User not found");

            if (string.IsNullOrEmpty(entity.Password))
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
            catch (Exception exception) { throw new SystemValidationException($"Error updating User: {exception.Message}");  }
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

                bool update = base.Update(user);

                if (update)
                {
                    try
                    {
                        this._UserProcessService.ExecuteProcess<SendMailChangePassword, bool>(new SendMailChangePassword() { Mails = new List<string>() { user.Mail } });
                    }
                    catch (Exception)
                    {

                    }
                    
                }

                return update;
            }
            catch (Exception ex) { throw new SystemValidationException($"Error change password! {ex.Message}"); }
        }

        public override bool Create(IEnumerable<User> entities)
        {
            try
            {
                Dictionary<string, string> mails = new Dictionary<string, string>();

                entities.ToList().ForEach(p =>
                {
                    string password = Utilities.GetPasswordRandom();
                    p.Password = InsiscoCore.Utilities.Crypto.MD5.Encrypt(password);
                    p.created_at = DateTime.Now;
                    p.updated_at = DateTime.Now;

                    mails.Add(p.Mail, password);
                });

                bool created = base.Create(entities);

                entities.ToList().ForEach(p =>
                {
                    SaveModules(p.Modules, p.id);

                    try
                    {
                        SendMail(p.Mail, mails[p.Mail]);
                    }
                    catch (Exception)
                    {
                    }
                });

                return created;
            }
            catch (Exception exception)
            {
                throw new SystemValidationException($"Error creating users: {exception.Message}");
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

        void SendMail(string mail, string password)
        {
            var configurations = this._ConfigurationRetrieveService.Where(p => p.Enabled == true).ToList();

            var mailConf = configurations.FirstOrDefault(p => p.Configuration_Name == "EMAIL_CONFIG");
            var messageConfig = configurations.FirstOrDefault(p => p.Configuration_Name == "USER_CREATE");

            var messageMail = JsonConvert.DeserializeObject<MessageMail>(messageConfig.Configuration_Value);
            string textHtml = new StreamReader(new MemoryStream(Utilities.GetFile(configurations, messageMail.Message))).ReadToEnd();
            textHtml = textHtml.Replace("{PASSWORD}", password);
            messageMail.Message = textHtml;

            Utilities.SendEmail(new List<string> { mail }, messageMail, mailConf);
        }
    }
}

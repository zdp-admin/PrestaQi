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
using System.Reflection.Metadata.Ecma335;

namespace PrestaQi.Service.WriteServices
{
    public class UserWriteService : WriteService<User>
    {
        IProcessService<User> _UserProcessService;
        IRetrieveService<User> _UserRetrieveService;
        IRetrieveService<UserModule> _UserModuleRetrieveService;
        IWriteService<UserModule> _UserModuleWriteService;
        IRetrieveService<Configuration> _ConfigurationRetrieveService;
        IRetrieveService<Contact> _ContactRetrieveService;

        public UserWriteService(
            IWriteRepository<User> repository,
            IRetrieveService<User> userRetrieveService,
            IWriteService<UserModule> userModuleWriteService,
            IRetrieveService<Configuration> configurationRetrieveService,
            IProcessService<User> userProcessService,
            IRetrieveService<UserModule> userModuleRetrieveService,
            IRetrieveService<Contact> contactRetrieveService
            ) : base(repository)
        {
            this._UserRetrieveService = userRetrieveService;
            this._UserModuleWriteService = userModuleWriteService;
            this._ConfigurationRetrieveService = configurationRetrieveService;
            this._UserProcessService = userProcessService;
            this._UserModuleRetrieveService = userModuleRetrieveService;
            this._ContactRetrieveService = contactRetrieveService;
        }

        public override bool Create(User entity)
        {
            this._UserProcessService.ExecuteProcess<string, bool>(entity.Mail);

            try
            {
                string password = Utilities.GetPasswordRandom();

                entity.Enabled = true;
                entity.First_Login = true;
                entity.Password = InsiscoCore.Utilities.Crypto.MD5.Encrypt(password);
                entity.created_at = DateTime.Now;
                entity.updated_at = DateTime.Now;

                bool create = base.Create(entity);

                if (create)
                {
                    SaveModules(entity.Modules, entity.id);

                    try
                    {
                        SendMail(entity.Mail, password, entity.First_Name);
                       
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

        public bool Update(ChangeStatusUser changeStatusUser)
        {
            try
            {
                var user = changeStatusUser.User as User;
                user.updated_at = DateTime.Now;
                return base.Update(user);
            }
            catch (Exception exception)
            {
                throw new SystemValidationException($"Error change status! {exception.Message}");
            }
        } 

        public bool Update(ChangePassword changePassword)
        {
            var contacts = this._ContactRetrieveService.Where(p => p.Enabled == true).ToList();
            var user = this._UserRetrieveService.Find(changePassword.User_Id);
            if (user == null)
                throw new SystemValidationException("User not found");

            user.updated_at = DateTime.Now;

            try
            {
                user.Password = InsiscoCore.Utilities.Crypto.MD5.Encrypt(changePassword.Password);
                user.First_Login = false;

                bool update = base.Update(user);

                if (update)
                {
                    try
                    {
                        this._UserProcessService.ExecuteProcess<SendMailChangePassword, bool>(new SendMailChangePassword()
                        {
                            Mails = new List<string>() { user.Mail },
                            Name = user.First_Name,
                            Contacts = contacts
                        });
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
                    p.Enabled = true;
                    p.First_Login = true;

                    mails.Add(p.Mail, password);
                });

                bool created = base.Create(entities);

                entities.ToList().ForEach(p =>
                {
                    SaveModules(p.Modules, p.id);

                    try
                    {
                        SendMail(p.Mail, mails[p.Mail], p.First_Name);
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
            var list = this._UserModuleRetrieveService.Where(p => p.user_id == id).ToList();
            if (list.Count > 0)
                this._UserModuleWriteService.Delete(list);

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

        void SendMail(string mail, string password, string name)
        {
            var contacts = this._ContactRetrieveService.Where(p => p.Enabled == true).ToList();
            var configurations = this._ConfigurationRetrieveService.Where(p => p.Enabled == true).ToList();

            var mailConf = configurations.FirstOrDefault(p => p.Configuration_Name == "EMAIL_CONFIG");
            var messageConfig = configurations.FirstOrDefault(p => p.Configuration_Name == "USER_CREATE");

            var messageMail = JsonConvert.DeserializeObject<MessageMail>(messageConfig.Configuration_Value);
            string textHtml = new StreamReader(new MemoryStream(Utilities.GetFile(configurations, messageMail.Message))).ReadToEnd();
            textHtml = textHtml.Replace("{NAME}", name);
            textHtml = textHtml.Replace("{MAIL}", mail);
            textHtml = textHtml.Replace("{PASSWORD}", password);
            textHtml = textHtml.Replace("{WHATSAPP}", contacts.Find(p => p.id == 1).Contact_Data);
            textHtml = textHtml.Replace("{MAIL_SOPORTE}", contacts.Find(p => p.id == 2).Contact_Data);
            textHtml = textHtml.Replace("{PHONE}", contacts.Find(p => p.id == 3).Contact_Data);
            messageMail.Message = textHtml;

            Utilities.SendEmail(new List<string> { mail }, messageMail, mailConf);
        }
    }
}

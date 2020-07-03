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
    public class AccreditedWriteService : WriteService<Accredited>
    {
        IRetrieveService<Accredited> _AccreditedRetrieveService;
        IRetrieveService<Configuration> _ConfigurationRetrieveService;
        IProcessService<User> _UserProcessService;

        public AccreditedWriteService(
            IWriteRepository<Accredited> repository,
            IRetrieveService<Accredited> accreditedRetrieveService,
            IRetrieveService<Configuration> configurationRetrieveService,
            IProcessService<User> userProcessService
            ) : base(repository)
        {
            this._AccreditedRetrieveService = accreditedRetrieveService;
            this._ConfigurationRetrieveService = configurationRetrieveService;
            this._UserProcessService = userProcessService;
        }

        public override bool Create(Accredited entity)
        {
            this._UserProcessService.ExecuteProcess<string, bool>(entity.Mail);

            try
            {
                string password = Utilities.GetPasswordRandom();
                entity.Password = InsiscoCore.Utilities.Crypto.MD5.Encrypt(password);
                entity.created_at = DateTime.Now;
                entity.updated_at = DateTime.Now;

                bool created = base.Create(entity);

                if (created)
                {
                    try
                    {
                        SendMail(entity.Mail, password);
                    }
                    catch (Exception exception)
                    { 
                    }
                    
                }

                return created;
            }
            catch (Exception exception)
            {
                throw new SystemValidationException($"Error creating Accredited: {exception.Message}");
            }

        }

        public override bool Update(Accredited entity)
        {
            Accredited accredited = this._AccreditedRetrieveService.Find(entity.id);
            if (accredited == null)
                throw new SystemValidationException("Accredited not found");

            if (string.IsNullOrEmpty(entity.Password))
                entity.Password = accredited.Password;

            entity.updated_at = DateTime.Now;
            entity.created_at = entity.created_at;

            try
            {
                return base.Update(entity);
            }
            catch (Exception exception) { throw new SystemValidationException($"Error updating Accredited: {exception.Message}");  }
        }

        public override bool Create(IEnumerable<Accredited> entities)
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
                throw new SystemValidationException($"Error creating Accredited: {exception.Message}");
            }
        }

        public bool Update(ChangePassword changePassword)
        {
            var user = this._AccreditedRetrieveService.Find(changePassword.User_Id);
            if (user == null)
                throw new SystemValidationException("Accredited not found");

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
            catch (Exception exception) { throw new SystemValidationException($"Error change password! {exception.Message}"); }
        }

        void SendMail(string mail, string password)
        {
            var configurations = this._ConfigurationRetrieveService.Where(p => p.Enabled == true).ToList();

            var mailConf = configurations.FirstOrDefault(p => p.Configuration_Name == "EMAIL_CONFIG");
            var messageConfig = configurations.FirstOrDefault(p => p.Configuration_Name == "ACCREDITED_CREATE");

            var messageMail = JsonConvert.DeserializeObject<MessageMail>(messageConfig.Configuration_Value);
            string textHtml = new StreamReader(new MemoryStream(Utilities.GetFile(configurations, messageMail.Message))).ReadToEnd();
            textHtml = textHtml.Replace("{PASSWORD}", password);
            messageMail.Message = textHtml;

            Utilities.SendEmail(new List<string> { mail }, messageMail, mailConf);
        }
    }
}

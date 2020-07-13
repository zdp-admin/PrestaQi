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

namespace PrestaQi.Service.WriteServices
{
    public class InvestorWriteService : WriteService<Investor>
    {
        IRetrieveService<Investor> _InvestorRetrieveService;
        IRetrieveService<Configuration> _ConfigurationRetrieveService;
        IProcessService<User> _UserProcessService;
        IRetrieveService<Contact> _ContactRetrieveService;

        public InvestorWriteService(
            IWriteRepository<Investor> repository,
            IRetrieveService<Investor> investorRetrieveService,
            IRetrieveService<Configuration> configurationRetrieveService,
            IProcessService<User> userProcessService,
            IRetrieveService<Contact> contactRetrieveService
            ) : base(repository)
        {
            this._InvestorRetrieveService = investorRetrieveService;
            this._ConfigurationRetrieveService = configurationRetrieveService;
            this._UserProcessService = userProcessService;
            this._ContactRetrieveService = contactRetrieveService;
        }

        public override bool Create(Investor entity)
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

                bool created = base.Create(entity);

                if (created)
                {
                    try
                    {
                        SendMail(entity.Mail, password, entity.First_Name);

                    }
                    catch (Exception)
                    {
                    }
                }

                return created;
            }
            catch (Exception exception)
            {
                throw new SystemValidationException($"Error creating Investor: {exception.Message}");
            }

        }

        public override bool Update(Investor entity)
        {
            Investor investor = this._InvestorRetrieveService.Find(entity.id);
            if (investor == null)
                throw new SystemValidationException("Investor not found");

            if (string.IsNullOrEmpty(entity.Password))
                entity.Password = investor.Password;

            entity.updated_at = DateTime.Now;
            entity.created_at = entity.created_at;

            try
            {
                return base.Update(entity);
            }
            catch (Exception exception) { throw new SystemValidationException($"Error updating Investor: {exception.Message}");  }
        }

        public override bool Create(IEnumerable<Investor> entities)
        {
            try
            {
                Dictionary<string, string> mails = new Dictionary<string, string>();

                entities.ToList().ForEach(p =>
                {
                    p.Enabled = true;
                    p.First_Login = true;
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
                throw new SystemValidationException($"Error creating Investors: {exception.Message}");
            }
        }

        public bool Update(ChangePassword changePassword)
        {
            var contacts = this._ContactRetrieveService.Where(p => p.Enabled == true).ToList();
            var user = this._InvestorRetrieveService.Find(changePassword.User_Id);
            if (user == null)
                throw new SystemValidationException("Investor not found");

            user.updated_at = DateTime.Now;

            try
            {
                user.Password = InsiscoCore.Utilities.Crypto.MD5.Encrypt(changePassword.Password);
                user.First_Login = false;

                bool update = base.Update(user);

                if (update)
                {
                    this._UserProcessService.ExecuteProcess<SendMailChangePassword, bool>(new SendMailChangePassword()
                    {
                        Mails = new List<string>() { user.Mail },
                        Name = user.First_Name,
                        Contacts = contacts
                    });
                }

                return update;
            }
            catch (Exception exception) { throw new SystemValidationException($"Error change password: {exception.Message}"); }
        }

        void SendMail(string mail, string password, string name)
        {
            var contacts = this._ContactRetrieveService.Where(p => p.Enabled == true).ToList();
            var configurations = this._ConfigurationRetrieveService.Where(p => p.Enabled == true).ToList();

            var mailConf = configurations.FirstOrDefault(p => p.Configuration_Name == "EMAIL_CONFIG");
            var messageConfig = configurations.FirstOrDefault(p => p.Configuration_Name == "INVESTOR_CREATE");

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

        public bool Update(ChangeStatusUser changeStatusUser)
        {
            try
            {
                var user = changeStatusUser.User as Investor;
                user.updated_at = DateTime.Now;
                return base.Update(user);
            }
            catch (Exception exception)
            {
                throw new SystemValidationException($"Error change status! {exception.Message}");
            }
        }
    }
}

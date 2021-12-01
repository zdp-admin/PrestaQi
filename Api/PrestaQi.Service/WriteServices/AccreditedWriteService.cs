using DocumentFormat.OpenXml.Wordprocessing;
using InsiscoCore.Base.Data;
using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using InsiscoCore.Utilities.Crypto;
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
    public class AccreditedWriteService : WriteService<Accredited>
    {
        IRetrieveService<Company> _CompanyRetrieveService;
        IWriteService<Company> _CompanyWriteService;
        IRetrieveService<Accredited> _AccreditedRetrieveService;
        IProcessService<User> _UserProcessService;
        IRetrieveService<Configuration> _ConfigurationRetrieveService;
        IRetrieveService<Contact> _ContactRetrieveService;

        public AccreditedWriteService(
            IWriteRepository<Accredited> repository,
            IRetrieveService<Accredited> accreditedRetrieveService,
            IRetrieveService<Configuration> configurationRetrieveService,
            IProcessService<User> userProcessService,
            IRetrieveService<Company> companyRetrieveService,
            IWriteService<Company> companyWriteService,
            IRetrieveService<Contact> contactRetrieveService
            ) : base(repository)
        {
            this._AccreditedRetrieveService = accreditedRetrieveService;
            this._ConfigurationRetrieveService = configurationRetrieveService;
            this._UserProcessService = userProcessService;
            this._CompanyWriteService = companyWriteService;
            this._CompanyRetrieveService = companyRetrieveService;
            this._ContactRetrieveService = contactRetrieveService;
        }

        public override bool Create(Accredited entity)
        {
            this._UserProcessService.ExecuteProcess<string, bool>(entity.Mail);

            try
            {
                var company = this._CompanyRetrieveService.Where(p => p.Description.ToLower().Trim() ==
                    entity.Company_Name.ToLower().Trim()).FirstOrDefault();

                if (company == null)
                {
                    company = new Company() { Description = entity.Company_Name };
                    this._CompanyWriteService.Create(company);
                }

                if (entity.Outsourcing_Name != "")
                {
                    var outsorcing = this._CompanyRetrieveService.Where(p => p.Description.ToLower().Trim() ==
                        entity.Outsourcing_Name.ToLower().Trim()).FirstOrDefault();

                    if (outsorcing == null)
                    {
                        outsorcing = new Company() { Description = entity.Outsourcing_Name };
                        this._CompanyWriteService.Create(outsorcing);
                    }

                    entity.Outsourcing_id = outsorcing.id;
                }

                entity.Enabled = true;
                entity.Company_Id = company.id;
                entity.First_Login = true;
                string password = Utilities.GetPasswordRandom();

                if (entity.Password == String.Empty)
                {
                    entity.Password = InsiscoCore.Utilities.Crypto.MD5.Encrypt(password);
                } else
                {
                    password = entity.Password;
                }

                entity.created_at = DateTime.Now;
                entity.updated_at = DateTime.Now;

                bool created = base.Create(entity);

                if (created)
                {
                    try
                    {
                        SendMail(entity.Mail, password, entity.First_Name);
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
                    if (p.Company_Id == 0)
                    {
                        var company = this._CompanyRetrieveService.Where(company => company.Description.ToLower().Trim() ==
                            p.Company_Name.ToLower().Trim()).FirstOrDefault();

                        if (company == null)
                        {
                            company = new Company() { Description = p.Company_Name };
                            this._CompanyWriteService.Create(company);
                            p.Company_Id = company.id;
                        }
                    }

                    string password = Utilities.GetPasswordRandom();
                    p.Password = InsiscoCore.Utilities.Crypto.MD5.Encrypt(password);
                    p.created_at = DateTime.Now;
                    p.updated_at = DateTime.Now;
                    p.First_Login = true;
                    p.Enabled = true;
                    mails.Add(p.Mail, password);
                });

                bool created = base.Create(entities);

                return created;
            }
            catch (Exception exception)
            {
                throw new SystemValidationException($"Error creating Accredited: {exception.Message}");
            }
        }

        public bool Update(ChangePassword changePassword)
        {
            var contacts = this._ContactRetrieveService.Where(p => p.Enabled == true).ToList();

            var user = this._AccreditedRetrieveService.Find(changePassword.User_Id);
            if (user == null)
                throw new SystemValidationException("Accredited not found");

            user.updated_at = DateTime.Now;

            try
            {
                user.Password = InsiscoCore.Utilities.Crypto.MD5.Encrypt(changePassword.Password);
                user.First_Login = false;

                bool update = this._Repository.Update(user);//base.Update(user);

                if (update)
                {
                    try
                    {
                        this._UserProcessService.ExecuteProcess<SendMailChangePassword, bool>(new SendMailChangePassword() { Mails = new List<string>() { user.Mail },
                        Name = user.First_Name, Contacts = contacts
                        });
                    }
                    catch (Exception)
                    {

                    }
                    
                }

                return update;
            }
            catch (Exception exception) { throw new SystemValidationException($"Error change password! {exception.Message}"); }
        }

        void SendMail(string mail, string password, string name)
        {
            var contacts = this._ContactRetrieveService.Where(p => p.Enabled == true).ToList();

            var configurations = this._ConfigurationRetrieveService.Where(p => p.Enabled == true).ToList();

            var mailConf = configurations.FirstOrDefault(p => p.Configuration_Name == "EMAIL_CONFIG");
            var messageConfig = configurations.FirstOrDefault(p => p.Configuration_Name == "ACCREDITED_CREATE");

            var messageMail = JsonConvert.DeserializeObject<MessageMail>(messageConfig.Configuration_Value);
            string textHtml = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), messageMail.Message));
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
                var user = changeStatusUser.User as Accredited;
                user.updated_at = DateTime.Now;
                return base.Update(user);
            }
            catch (Exception exception)
            {
                throw new SystemValidationException($"Error change status! {exception.Message}");
            }
        }

        public bool Update(BlockedAccredited blockedAccredited)
        {
            try
            {
                var accrediteds = this._AccreditedRetrieveService.Where(p => p.Company_Id == blockedAccredited.Company_Id).ToList();

                if (accrediteds.Count == 0)
                    throw new SystemValidationException($"No se encontraron acreditados para la empresa seleccionada");

                accrediteds.ForEach(p => { p.Is_Blocked = blockedAccredited.Is_Blocked; });

                return this._Repository.Update(accrediteds);
            }
            catch (Exception exception)
            {
                throw new SystemValidationException("Error al bloquear/desbloquear los servicios: " + exception.Message);
            }
        }
    
        public bool Update(ChangeEmail changeEmail)
        {
            var accredited = this._AccreditedRetrieveService.Find(changeEmail.accreditedId);
            if (accredited != null)
            {
                accredited.Mail = changeEmail.email;

                return this._Repository.Update(accredited);
            }

            throw new SystemValidationException($"Usuario no encontrado");
        }
    }
}

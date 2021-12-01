using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using PrestaQi.Model;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Service.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PrestaQi.Service.ProcessServices
{
    public class RequestRegisterProcessService : ProcessService<RequestRegister>
    {
        IRetrieveService<Configuration> _ConfigurationRetrieveService;
        IRetrieveService<RequestRegister> _RequestRegisterRetrieveService;
        IRetrieveService<Accredited> _AccreditedRetrieveService;
        IWriteService<Accredited> _AccreditedWriteService;
        IRetrieveService<Contact> _ContactRetrieveService;

        public RequestRegisterProcessService(
            IRetrieveService<Configuration> configRetrieveService,
            IRetrieveService<RequestRegister> requestRegisterRetrieveService,
            IRetrieveService<Accredited> accreditedRetrieveService,
            IWriteService<Accredited> accreditedWriteService,
            IRetrieveService<Contact> contactRetrieveService
        ) {
            this._ConfigurationRetrieveService = configRetrieveService;
            this._RequestRegisterRetrieveService = requestRegisterRetrieveService;
            this._AccreditedRetrieveService = accreditedRetrieveService;
            this._AccreditedWriteService = accreditedWriteService;
            this._ContactRetrieveService = contactRetrieveService;
        }

        public bool ExecuteProcess(DeleteRegister notification)
        {
            var configurations = this._ConfigurationRetrieveService.Where(c => c.Enabled == true).ToList();
            var mailConf = configurations.FirstOrDefault(c => c.Configuration_Name == "EMAIL_CONFIG");
            var users = this._RequestRegisterRetrieveService.Where(a => notification.ids.Contains(a.id)).ToList();
            var contacts = this._ContactRetrieveService.Where(p => p.Enabled == true).ToList();

            foreach (var user in users)
            {
                var accredited = this._AccreditedRetrieveService.Where(a => a.Curp?.ToLower() == user.Curp.ToLower()).ToList().FirstOrDefault();

                if (accredited != null)
                {
                    var pass = Utilities.GetPasswordRandom();
                    accredited.Password = InsiscoCore.Utilities.Crypto.MD5.Encrypt(pass);
                    accredited.Mail = user.Email;
                    this._AccreditedWriteService.Update(accredited);

                    string textHtml = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Templates/UserLocalization.html"));
                    textHtml = textHtml.Replace("{NAME}", accredited.First_Name);
                    textHtml = textHtml.Replace("{MAIL}", user.Email);
                    textHtml = textHtml.Replace("{PASSWORD}", pass);
                    textHtml = textHtml.Replace("{WHATSAPP}", contacts.Find(p => p.id == 1).Contact_Data);
                    textHtml = textHtml.Replace("{MAIL_SOPORTE}", contacts.Find(p => p.id == 2).Contact_Data);
                    textHtml = textHtml.Replace("{PHONE}", contacts.Find(p => p.id == 3).Contact_Data);

                    var message = new MessageMail()
                    {
                        Message = textHtml,
                        Subject = "Continua con tu registro"
                    };
                    var emails = new List<string>();
                    emails.Add(user.Email);

                    Utilities.SendEmail(emails, message, mailConf);
                }
            }

            return true;
        }
    }
}

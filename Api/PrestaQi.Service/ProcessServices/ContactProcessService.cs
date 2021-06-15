using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using PrestaQi.Model;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Service.Tools;
using System.Collections.Generic;
using System.Linq;

namespace PrestaQi.Service.ProcessServices
{
    public class ContactProcessService: ProcessService<FormContact>
    {
        IRetrieveService<Configuration> _ConfigurationRetrieveService;

        public ContactProcessService(IRetrieveService<Configuration> configurationRetrieveService)
        {
            this._ConfigurationRetrieveService = configurationRetrieveService;
        }

        public bool ExecuteProcess(FormContact contact)
        {
            var configurations = this._ConfigurationRetrieveService.Where(p => p.Enabled == true).ToList();
            var mailConf = configurations.FirstOrDefault(p => p.Configuration_Name == "EMAIL_CONFIG");

            var messageMail = new MessageMail();
            messageMail.Subject = "CONTACTO";
            messageMail.Message += $"<p>Nombre: {contact.name}</p>";
            messageMail.Message += $"<p>Email: {contact.email}</p>";
            messageMail.Message += $"<p>Soy un: {contact.type}</p>";
            messageMail.Message += $"<p>Empresa: {contact.company}</p>";
            messageMail.Message += $"<p>Mensaje: {contact.message}</p>";
            messageMail.Message += $"<p>Teléfono: {contact.telephone}</p>";

            Utilities.SendEmail(new List<string> { "soporte@prestaqi.com" }, messageMail, mailConf);

            return true;
        }
    }
}

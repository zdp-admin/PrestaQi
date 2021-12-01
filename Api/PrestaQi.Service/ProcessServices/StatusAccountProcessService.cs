using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using Microsoft.AspNetCore.Hosting;
using PrestaQi.Model;
using PrestaQi.Model.Configurations;
using System;
using System.IO;
using System.Linq;

namespace PrestaQi.Service.ProcessServices
{
    public class StatusAccountProcessService : ProcessService<StatusAccount>
    {
        private string pathProyect;

        IWriteService<StatusAccount> _statusAccountWriteService;
        IWriteService<Accredited> _accreditedWriteService;
        IRetrieveService<Accredited> _accreditedRetrieveService;
        IRetrieveService<Institution> _institutionRetrieveService;

        public StatusAccountProcessService(
            IHostingEnvironment hostingEvironment,
            IWriteService<StatusAccount> statusAccountWriteService,
            IWriteService<Accredited> accreditedWriteService,
            IRetrieveService<Accredited> accreditedRetrieveService,
            IRetrieveService<Institution> institutionRetrieveService
        )
        {
            this.pathProyect = hostingEvironment.ContentRootPath;
            this._statusAccountWriteService = statusAccountWriteService;
            this._accreditedWriteService = accreditedWriteService;
            this._accreditedRetrieveService = accreditedRetrieveService;
            this._institutionRetrieveService = institutionRetrieveService;
        }

        public bool ExecuteProcess(StatusAccount statusAccount)
        {
            if (statusAccount.File is null)
            {
                throw new SystemValidationException("El archivo es necesario");
            }

            if (!Directory.Exists($"{this.pathProyect}/statusaccount"))
            {
                Directory.CreateDirectory($"{this.pathProyect}/statusaccount");
            }

            var extension = statusAccount.NameFile.Split(".").Last();
            var newname = $"{Guid.NewGuid()}.{extension}";

            var currentDirectory = $"{this.pathProyect}/statusaccount/{newname}";
            FileStream fileStream = File.Create(currentDirectory);
            fileStream.Write(statusAccount.File, 0, statusAccount.File.Length);
            fileStream.Close();

            statusAccount.PathFile = $"statusaccount/{newname}";
            statusAccount.created_at = DateTime.Now;
            statusAccount.updated_at = DateTime.Now;

            this._statusAccountWriteService.Create(statusAccount);

            var accredited = this._accreditedRetrieveService.Find(statusAccount.AccreditedId);
            accredited.Clabe = statusAccount.KeyAccount.Replace(" ", "").Replace("-", "");
            accredited.Account_Number = statusAccount.NumberAccount.Replace("-", "").Replace(" ", "");

            var institution = this._institutionRetrieveService.Where(comp => comp.Description.ToLower().Contains(statusAccount.NameBank.ToLower())).FirstOrDefault();
            if (institution != null)
            {
                accredited.Institution_Id = institution.id;
            }

            this._accreditedWriteService.Update(accredited);

            return true;
        }
    }
}

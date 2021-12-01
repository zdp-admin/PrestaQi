using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using Microsoft.AspNetCore.Hosting;
using PrestaQi.Model;
using PrestaQi.Model.Configurations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PrestaQi.Service.ProcessServices
{
    public class PaySheetUserProcessService : ProcessService<PaySheetUser>
    {
        private string pathProyect;

        IWriteService<PaySheetUser> _paySheetWriteService;

        public PaySheetUserProcessService(
            IHostingEnvironment hostingEnviroment,
            IWriteService<PaySheetUser> paySheetUserWriteService
        )
        {
            this._paySheetWriteService = paySheetUserWriteService;
            this.pathProyect = hostingEnviroment.ContentRootPath;
        }

        public bool ExecuteProcess(PaySheetUser paySheetUser)
        {
            if(paySheetUser.PaySheets is null || paySheetUser.PaySheets.Count <= 0)
            {
                throw new SystemValidationException("El archivo es necesario");
            }

            if (!Directory.Exists($"{this.pathProyect}/paysheet"))
            {
                Directory.CreateDirectory($"{this.pathProyect}/paysheet");
            }

            double total = 0;

            foreach(var paysheet in paySheetUser.PaySheets)
            {
                var extension = paysheet.NameFile.Split(".").Last();
                var newname = $"{Guid.NewGuid()}.{extension}";

                var currentDirectory = $"{this.pathProyect}/paysheet/{newname}";
                FileStream fileStream = File.Create(currentDirectory);
                fileStream.Write(paysheet.File, 0, paysheet.File.Length);
                fileStream.Close();

                paysheet.PathFile = $"paysheet/{newname}";
                paysheet.created_at = DateTime.Now;
                paysheet.updated_at = DateTime.Now;
                paysheet.AccreditedId = paySheetUser.AccreditedId;
                total += paysheet.Neto;

                this._paySheetWriteService.Create(paysheet);
            }

            return true;
        }
    }
}

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
        IRetrieveService<Accredited> _accreditedRetrevieService;
        IWriteService<Accredited> _accreditedWriteService;

        public PaySheetUserProcessService(
            IHostingEnvironment hostingEnviroment,
            IWriteService<PaySheetUser> paySheetUserWriteService,
            IRetrieveService<Accredited> accreditedRetrieveService,
            IWriteService<Accredited> accreditedWriteService
        )
        {
            this._paySheetWriteService = paySheetUserWriteService;
            this.pathProyect = hostingEnviroment.ContentRootPath;
            this._accreditedRetrevieService = accreditedRetrieveService;
            this._accreditedWriteService = accreditedWriteService;
        }

        public bool ExecuteProcess(PaySheetUser paySheetUser)
        {
            if(paySheetUser.File is null)
            {
                throw new SystemValidationException("El archivo es necesario");
            }

            if (!Directory.Exists($"{this.pathProyect}/paysheet"))
            {
                Directory.CreateDirectory($"{this.pathProyect}/paysheet");
            }

            var extension = paySheetUser.NameFile.Split(".").Last();
            var newname = $"{Guid.NewGuid()}.{extension}";

            var currentDirectory = $"{this.pathProyect}/paysheet/{newname}";
            FileStream fileStream = File.Create(currentDirectory);
            fileStream.Write(paySheetUser.File, 0, paySheetUser.File.Length);
            fileStream.Close();

            paySheetUser.PathFile = $"paysheet/{newname}";
            paySheetUser.created_at = DateTime.Now;
            paySheetUser.updated_at = DateTime.Now;

            this._paySheetWriteService.Create(paySheetUser);

            var user = this._accreditedRetrevieService.Where(accredited => accredited.id == paySheetUser.AccreditedId).FirstOrDefault();

            if (user != null)
            {
                var diffDays = paySheetUser.DateFinish - paySheetUser.DateInitial;
                var dayInPeriodo = diffDays.Days;

                if (dayInPeriodo >= 26)
                {
                    user.Gross_Monthly_Salary = paySheetUser.Neto;
                    user.Net_Monthly_Salary = paySheetUser.Total;
                } else if (dayInPeriodo >= 13)
                {
                    user.Gross_Monthly_Salary = paySheetUser.Neto * 2;
                    user.Net_Monthly_Salary = paySheetUser.Total * 2;
                } else
                {
                    user.Gross_Monthly_Salary = paySheetUser.Neto * 4;
                    user.Net_Monthly_Salary = paySheetUser.Total * 4;
                }

                this._accreditedWriteService.Update(user);
            }

            return true;
        }
    }
}

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
    public class IneAccountProcessService : ProcessService<IneAccount>
    {
        private string pathProyect;

        IWriteService<IneAccount> _ineAccountWriteService;

        public IneAccountProcessService(
            IHostingEnvironment hostingEvironment,
            IWriteService<IneAccount> ineAccountWriteService
        )
        {
            this.pathProyect = hostingEvironment.ContentRootPath;
            this._ineAccountWriteService = ineAccountWriteService;
        }

        public bool ExecuteProcess(IneAccount ineAccount)
        {
            if (ineAccount.File is null || ineAccount.FileBack is null)
            {
                throw new SystemValidationException("El archivo es necesario");
            }

            if (!Directory.Exists($"{this.pathProyect}/ineaccount"))
            {
                Directory.CreateDirectory($"{this.pathProyect}/ineaccount");
            }

            var extension = ineAccount.NameFile.Split(".").Last();
            var extensionBack = ineAccount.NameFileBack.Split(".").Last();

            var nameine = $"{Guid.NewGuid()}.{extension}";
            var nameineback = $"{Guid.NewGuid()}.{extensionBack}";

            var currentDirectory = $"{this.pathProyect}/ineaccount/{nameine}";
            FileStream fileStream = File.Create(currentDirectory);
            fileStream.Write(ineAccount.File, 0, ineAccount.File.Length);
            fileStream.Close();

            var currentDirectoryBack = $"{this.pathProyect}/ineaccount/{nameineback}";
            FileStream fileStreamBack = File.Create(currentDirectoryBack);
            fileStreamBack.Write(ineAccount.FileBack, 0, ineAccount.FileBack.Length);
            fileStreamBack.Close();

            ineAccount.PathFileFront = $"ineaccount/{nameine}";
            ineAccount.PathFileBack = $"ineaccount/{nameineback}";
            ineAccount.created_at = DateTime.Now;
            ineAccount.updated_at = DateTime.Now;

            this._ineAccountWriteService.Create(ineAccount);

            return true;
        }
    }
}

using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using PrestaQi.Model;
using PrestaQi.Model.Configurations;
using PrestaQi.Model.Dto.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PrestaQi.Service.ProcessServices
{
    public class SelfieUserProcessService : ProcessService<SelfieUser>
    {
        private string pathProyect;

        IWriteService<SelfieUser> _selfieWriteService;
        public SelfieUserProcessService(
            IHostingEnvironment hostingEnvironment,
            IWriteService<SelfieUser> selfieWriteService
        ) {
            this.pathProyect = hostingEnvironment.ContentRootPath;
            this._selfieWriteService = selfieWriteService;
        }
        
        public bool ExecuteProcess(UploadSelfieBinaria file)
        {
            if (file.File is null)
            {
                throw new SystemValidationException("El archivo es necesario");
            }

            if (!Directory.Exists($"{this.pathProyect}/selfies"))
            {
                Directory.CreateDirectory($"{this.pathProyect}/selfies");
            }

            var extension = file.NameFile.Split(".").Last();
            var newname = $"{Guid.NewGuid()}.{extension}";

            var currentDirectory = $"{this.pathProyect}/selfies/{newname}";
            FileStream fileStream = File.Create(currentDirectory);
            fileStream.Write(file.File, 0, file.File.Length);
            fileStream.Close();

            SelfieUser selfieUser = new SelfieUser();
            selfieUser.AccreditedId = file.AccreditedId;
            selfieUser.created_at = DateTime.Now;
            selfieUser.FaceId = file.FaceId;
            selfieUser.Meta = file.metadata;
            selfieUser.PathFile = $"selfies/{newname}";

            this._selfieWriteService.Create(selfieUser);

            return true;
        }
    }
}

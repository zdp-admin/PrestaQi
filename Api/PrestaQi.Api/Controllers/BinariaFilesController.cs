using InsiscoCore.Base.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PrestaQi.Api.Configuration;
using PrestaQi.Model;
using PrestaQi.Model.Configurations;
using PrestaQi.Model.Dto.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PrestaQi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BinariaFilesController : CustomController
    {

        IProcessService<SelfieUser> _selfieProcessService;
        IProcessService<PaySheetUser> _paySheetProcessService;
        IProcessService<StatusAccount> _statusAccountProcessService;
        IProcessService<IneAccount> _ineAccountProcessService;

        public BinariaFilesController(
            IProcessService<SelfieUser> selfieProcessService,
            IProcessService<PaySheetUser> paySheetProcessService,
            IProcessService<StatusAccount> statusAccountProcessService,
            IProcessService<IneAccount> ineAccountProcessService
        ) : base() {
            this._selfieProcessService = selfieProcessService;
            this._paySheetProcessService = paySheetProcessService;
            this._statusAccountProcessService = statusAccountProcessService;
            this._ineAccountProcessService = ineAccountProcessService;
        }

        [HttpPost, Route("Selfie")]
        public IActionResult UploadSelfie([FromForm] UploadSelfieBinaria file)
        {
            if (Request.Form.Files.Count <= 0)
            {
                throw new SystemValidationException("El archivo es requerido");
            }

            var requestFile = Request.Form.Files[0];

            file.NameFile = requestFile.FileName;

            using (var ms = new MemoryStream())
            {
                requestFile.CopyTo(ms);
                file.File = ms.ToArray();
            }

            var result =  this._selfieProcessService.ExecuteProcess<UploadSelfieBinaria, bool>(file);

            return Ok(result);
        }

        [HttpPost, Route("Paysheet")]
        public IActionResult UploadPaysheet([FromForm] PaySheetUser paySheetUser)
        {
            if (Request.Form.Files.Count <= 0)
            {
                throw new SystemValidationException("El archivo es requerido");
            }

            var requestFile = Request.Form.Files[0];
            paySheetUser.NameFile = requestFile.FileName;

            using(var ms = new MemoryStream())
            {
                requestFile.CopyTo(ms);
                paySheetUser.File = ms.ToArray();
            }

            var result = this._paySheetProcessService.ExecuteProcess<PaySheetUser, bool>(paySheetUser);

            return Ok(result);
        }

        [HttpPost, Route("StatusAccount")]
        public IActionResult UploadStatusAccount([FromForm] StatusAccount statusAccount)
        {
            if (Request.Form.Files.Count <= 0)
            {
                throw new SystemValidationException("El archivo es requerido");
            }

            var requestFile = Request.Form.Files[0];
            statusAccount.NameFile = requestFile.FileName;

            using (var ms = new MemoryStream())
            {
                requestFile.CopyTo(ms);
                statusAccount.File = ms.ToArray();
            }

            var result = this._statusAccountProcessService.ExecuteProcess<StatusAccount, bool>(statusAccount);

            return Ok(result);
        }

        [HttpPost, Route("IneAccount")]
        public IActionResult UploadIneAccount([FromForm] IneAccount ineAccount)
        {
            if (Request.Form.Files.Count <= 1)
            {
                throw new SystemValidationException("El archivo es requerido");
            }

            foreach(var file in Request.Form.Files)
            {
                if (file.Name == "File")
                {
                    ineAccount.NameFile = file.FileName;

                    using (var ms = new MemoryStream())
                    {
                        file.CopyTo(ms);
                        ineAccount.File = ms.ToArray();
                    }
                } else
                {
                    ineAccount.NameFileBack = file.FileName;

                    using (var ms = new MemoryStream())
                    {
                        file.CopyTo(ms);
                        ineAccount.FileBack = ms.ToArray();
                    }
                }
            }

            var result = this._ineAccountProcessService.ExecuteProcess<IneAccount, bool>(ineAccount);

            return Ok(result);
        }
    }
}

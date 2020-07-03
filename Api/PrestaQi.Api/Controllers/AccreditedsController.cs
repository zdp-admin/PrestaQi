using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using InsiscoCore.Base.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using PrestaQi.Api.Configuration;
using PrestaQi.Model;
using PrestaQi.Model.Dto;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Model.Dto.Output;

namespace PrestaQi.Api.Controllers
{
    [Route("api/[controller]"), Authorize]
    [ApiController]
    public class AccreditedsController : CustomController
    {
        IWriteService<Accredited> _AccreditedWriteService;
        IRetrieveService<Accredited> _AccreditedRetrieveService;
        IProcessService<Accredited> _AccreditedProcessService;
        IProcessService<ExportFile> _ExportProcessService;

        public AccreditedsController(
            IWriteService<Accredited> accreditedWriteService, 
            IRetrieveService<Accredited> accreditedRetrieveService,
            IProcessService<Accredited> accreditedProcessService,
             IProcessService<ExportFile> exportProcessService)
        {
            this._AccreditedWriteService = accreditedWriteService;
            this._AccreditedRetrieveService = accreditedRetrieveService;
            this._AccreditedProcessService = accreditedProcessService;
            this._ExportProcessService = exportProcessService;
        }

       [HttpPost, Route("GetList")]
        public IActionResult GetList(AccreditedByPagination accreditedByPagination)
        {
            var result = this._AccreditedRetrieveService.RetrieveResult<AccreditedByPagination, List<Accredited>>(accreditedByPagination);

            if (accreditedByPagination.Type == 0)
                return Ok(result);
            else
            {
                var msFile = this._ExportProcessService.ExecuteProcess<ExportAccredited, MemoryStream>(new ExportAccredited()
                {
                    Type = accreditedByPagination.Type,
                    Accrediteds = result
                });

                return this.File(
                    fileContents: msFile.ToArray(),
                    contentType: accreditedByPagination.Type == 1 ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" :
                        "application/pdf",
                    fileDownloadName: accreditedByPagination.Type == 1 ? "Accrediteds" + DateTime.Now.ToString("yyyyMMdd") + ".xlsx" :
                        "Accrediteds" + DateTime.Now.ToString("yyyyMMdd") + ".pdf"
                );
            }

        }

        [HttpPost]
        public IActionResult Post(Accredited accredited)
        {
            return Ok(this._AccreditedWriteService.Create(accredited), "Accredited created!");
        }

        [HttpPut]
        public IActionResult Put(Accredited accredited)
        {
            return Ok(this._AccreditedWriteService.Update(accredited), "Accredited updated!");
        }

        [HttpPost, Route("CreateAccrediteds")]
        public IActionResult CreateAccrediteds(List<Accredited> accrediteds)
        {
            return Ok(this._AccreditedWriteService.Create(accrediteds));
        }

        [HttpGet, Route("ListAdvancesReceivable")]
        public IActionResult ListAdvancesReceivable()
        {
            return Ok(this._AccreditedProcessService.ExecuteProcess<bool, List<AdvanceReceivable>>(true));
        }
    }
}
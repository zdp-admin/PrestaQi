using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using InsiscoCore.Base.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrestaQi.Api.Configuration;
using PrestaQi.Model;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Model.Dto.Output;
using PrestaQi.Service.Tools;

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
        IProcessService<ExportAdvance> _ExportAdvanceProcessService;
        IProcessService<ExportAdvanceReceivable> _ExportAdvanceReceivableProcessService;

        public AccreditedsController(
            IWriteService<Accredited> accreditedWriteService, 
            IRetrieveService<Accredited> accreditedRetrieveService,
            IProcessService<Accredited> accreditedProcessService,
            IProcessService<ExportFile> exportProcessService,
            IProcessService<ExportAdvance> exportAdvanceProcessService,
            IProcessService<ExportAdvanceReceivable> exportAdvanceReceivableProcessService)
        {
            this._AccreditedWriteService = accreditedWriteService;
            this._AccreditedRetrieveService = accreditedRetrieveService;
            this._AccreditedProcessService = accreditedProcessService;
            this._ExportProcessService = exportProcessService;
            this._ExportAdvanceProcessService = exportAdvanceProcessService;
            this._ExportAdvanceReceivableProcessService = exportAdvanceReceivableProcessService;
        }

        [HttpPost, Route("GetList")]
        public IActionResult GetList(AccreditedByPagination accreditedByPagination)
        {
            var result = this._AccreditedRetrieveService.RetrieveResult<AccreditedByPagination, AccreditedPagination>(accreditedByPagination);
            return Ok(result);
        }

        [HttpGet, Route("GetFile")]
        public IActionResult GetFile([FromQuery] int type)
        {
            var result = this._AccreditedRetrieveService.RetrieveResult<AccreditedByPagination, AccreditedPagination>(new AccreditedByPagination()
            {
                Type = type
            });

            var msFile = this._ExportProcessService.ExecuteProcess<ExportAccredited, MemoryStream>(new ExportAccredited()
            {
                Type = type,
                Accrediteds = result.Accrediteds
            });

            return this.File(
                fileContents: msFile.ToArray(),
                contentType: type == 1 ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" :
                    "application/pdf",
                fileDownloadName: type == 1 ? "Accrediteds" + DateTime.Now.ToString("yyyyMMdd") + ".xlsx" :
                    "Accrediteds" + DateTime.Now.ToString("yyyyMMdd") + ".pdf"
            );
        }

        [HttpPost]
        public IActionResult Post(Accredited accredited)
        {
            return Ok(this._AccreditedWriteService.Create(accredited), "Accredited created!");
        }

        [HttpPut]
        public IActionResult Put(Accredited accredited)
        {
            var accreditedFind = this._AccreditedRetrieveService.RetrieveResult<Func<Accredited, bool>, List<Accredited>>(a => a.id == accredited.id).FirstOrDefault();

            if (accreditedFind is null)
            {
                return BadRequest();
            }

            accredited.External = accreditedFind.External;
            accredited.Is_Blocked = accreditedFind.Is_Blocked;
            accredited.First_Login = accreditedFind.First_Login;
            accredited.CompleteUpload = accreditedFind.CompleteUpload;
            accredited.ApprovedDocuments = accreditedFind.ApprovedDocuments;

            return Ok(this._AccreditedWriteService.Update(accredited), "Accredited updated!");
        }

        [HttpPost, Route("CreateAccrediteds")]
        public IActionResult CreateAccrediteds(List<Accredited> accrediteds)
        {
            return Ok(this._AccreditedWriteService.Create(accrediteds));
        }

        [HttpGet, Route("ListAdvancesReceivable")]
        public IActionResult ListAdvancesReceivable([FromQuery] AdvancesReceivableByFilter filter)
        {
            var list = this._AccreditedProcessService.ExecuteProcess<AdvancesReceivableByFilter, List<AdvanceReceivable>>(filter).ToList();

            return Ok(list);
        }

        [HttpGet, Route("GetFileAdvances")]
        public IActionResult GetFileAdvances([FromQuery] int type)
        {
            var list = this._AccreditedProcessService.ExecuteProcess<AdvancesReceivableByFilter, List<AdvanceReceivable>>(new AdvancesReceivableByFilter() { }).ToList();

            var msFile = this._ExportAdvanceReceivableProcessService.ExecuteProcess<ExportAdvanceReceivable, MemoryStream>(new ExportAdvanceReceivable()
            {
                Type = type,
                AdvanceReceivables = list
            });

            return this.File(
                fileContents: msFile.ToArray(),
                contentType: type == 1 ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" :
                    "application/pdf",
                fileDownloadName: type == 1 ? "AdvanceReceivable" + DateTime.Now.ToString("yyyyMMdd") + ".xlsx" :
                    "AdvanceReceivable" + DateTime.Now.ToString("yyyyMMdd") + ".pdf"
            );
        }

        [HttpPost, Route("ExportAdvanceByAccredited")]
        public IActionResult ExportAdvanceByAccredited(ExportAdvance exportAdvance)
        {
            var file = this._ExportAdvanceProcessService.ExecuteProcess<ExportAdvance, MemoryStream>(exportAdvance);

            return this.File(
                    fileContents: file.ToArray(),
                    contentType: exportAdvance.Type == 1 ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" :
                        "application/pdf",
                    fileDownloadName: exportAdvance.Type == 1 ? "Advances" + DateTime.Now.ToString("yyyyMMdd") + ".xlsx" :
                        "Advances" + DateTime.Now.ToString("yyyyMMdd") + ".pdf"
                );
        }

        [HttpPut, Route("BlockedService")]
        public IActionResult BlockedService(BlockedAccredited blockedAccredited)
        {
            return Ok(this._AccreditedWriteService.Update<BlockedAccredited, bool>(blockedAccredited));
        }

        [HttpPost, Route("GetExternal")]
        public IActionResult GetExternal(AccreditedExternalByPagination filter)
        {
            var result = this._AccreditedRetrieveService.RetrieveResult<AccreditedExternalByPagination, AccreditedPagination>(filter);
            return Ok(result);
        }

        [HttpGet, Route("GetCurrentPeriod")]
        public IActionResult GetCurrentPeriod()
        {
            int accreditedId = int.Parse(HttpContext.User.FindFirst("UserId").Value);
            var accredited = this._AccreditedRetrieveService.Find(accreditedId);
            return Ok(Utilities.getPeriodoByAccredited(accredited, DateTime.Now));
        }

        [HttpPut, Route("CompleteUploadFiles")]
        public IActionResult CompleteUploadFiles([FromQuery] int id)
        {
            return Ok(this._AccreditedProcessService.ExecuteProcess<int, bool>(id));
        }

        [HttpPut, Route("ApproveDocuments/{id}")]
        public IActionResult ApproveDocuments(int id)
        {
            var accredited = this._AccreditedRetrieveService.RetrieveResult< Func<Accredited, bool>, List<Accredited>> (accredited => accredited.id == id).FirstOrDefault();

            if (accredited is null)
            {
                return BadRequest();
            }

            return Ok(this._AccreditedProcessService.ExecuteProcess<Accredited, Accredited>(accredited));
        }

        [HttpPost, Route("NotificationDocument")]
        public IActionResult NotificationDocument(NotificationDocument notificationDocument)
        {
            return Ok(this._AccreditedProcessService.ExecuteProcess<NotificationDocument, bool>(notificationDocument));
        }
    }
}
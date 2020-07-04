using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using InsiscoCore.Base.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrestaQi.Api.Configuration;
using PrestaQi.Model;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Model.Dto.Output;

namespace PrestaQi.Api.Controllers
{
    [Route("api/[controller]"), Authorize]
    [ApiController]
    public class CapitalsController : CustomController
    {
        IWriteService<Capital> _CapitalWriteService;
        IRetrieveService<Capital> _CapitalRetrieveService;
        IProcessService<Capital> _CapitalProcessService;
        IProcessService<ExportAnchorControl> _ExportAnchorProcessService;

        public CapitalsController(
            IWriteService<Capital> capitalWriteService, 
            IRetrieveService<Capital> capitalRetrieveService,
            IProcessService<Capital> capitalProcessService,
            IProcessService<ExportAnchorControl> exportAnchorProcessService)
        {
            this._CapitalWriteService = capitalWriteService;
            this._CapitalRetrieveService = capitalRetrieveService;
            this._CapitalProcessService = capitalProcessService;
            this._ExportAnchorProcessService = exportAnchorProcessService;
        }

        [HttpGet, Route("[action]/{id}")]
        public IActionResult GetByInvestor(int id, 
            [FromQuery(Name = "startDate")] DateTime startDate,
            [FromQuery(Name = "endDate")] DateTime endDate)
        {
            return Ok(this._CapitalRetrieveService.Where(p =>
            {
                return p.investor_id == id && p.created_at.Date >= startDate && p.created_at.Date <= endDate;
            }).ToList());
        }

        [HttpPost]
        public IActionResult Post(Capital userCapital)
        {
            userCapital.Created_By = int.Parse(HttpContext.User.FindFirst("UserId").Value);
            return Ok(this._CapitalWriteService.Create(userCapital), "Record created!");
        }

        [HttpGet, Route("GetMyInvestments/{id}")]
        public IActionResult GetMyInvestments(int id)
        {
            return Ok(this._CapitalProcessService.ExecuteProcess<int, List<MyInvestment>>(id));
        }

        [HttpPost, Route("[action]")]
        public IActionResult GetAnchorControl(AnchorByFilter anchorByFilter)
        {
            var list = this._CapitalProcessService.ExecuteProcess<AnchorByFilter, List<AnchorControl>>(anchorByFilter);

            if (anchorByFilter.Type == 0)
                return Ok(list);
            else
            {
                var msFile = this._ExportAnchorProcessService.ExecuteProcess<ExportAnchorControl, MemoryStream>(new ExportAnchorControl()
                {
                    Type = anchorByFilter.Type,
                    AnchorControls = list
                });

                return this.File(
                    fileContents: msFile.ToArray(),
                    contentType: anchorByFilter.Type == 1 ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" :
                        "application/pdf",
                    fileDownloadName: anchorByFilter.Type == 1 ? "AnchorControl" + DateTime.Now.ToString("yyyyMMdd") + ".xlsx" :
                        "AnchorControl" + DateTime.Now.ToString("yyyyMMdd") + ".pdf"
                );
            }

        }

        [HttpPost, Route("ChangeStatus")]
        public IActionResult ChangeStatus(CapitalChangeStatus capitalChangeStatus)
        {
            return Ok(this._CapitalWriteService.Update<CapitalChangeStatus, bool>(capitalChangeStatus));
        }
    }
}
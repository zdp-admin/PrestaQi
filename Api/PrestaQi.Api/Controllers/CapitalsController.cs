using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Composition;
using System.IO;
using System.Linq;
using InsiscoCore.Base.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
        IProcessService<ExportCapitalDetail> _ExportCapitalDetailProcessService;
        IProcessService<ExportMyInvestment> _ExportMyInvestmentProcessService;

        public CapitalsController(
            IWriteService<Capital> capitalWriteService, 
            IRetrieveService<Capital> capitalRetrieveService,
            IProcessService<Capital> capitalProcessService,
            IProcessService<ExportAnchorControl> exportAnchorProcessService,
            IProcessService<ExportCapitalDetail> exportCapitalDetailProcessService,
            IProcessService<ExportMyInvestment> exportMyInvestmentProcessService)
        {
            this._CapitalWriteService = capitalWriteService;
            this._CapitalRetrieveService = capitalRetrieveService;
            this._CapitalProcessService = capitalProcessService;
            this._ExportAnchorProcessService = exportAnchorProcessService;
            this._ExportCapitalDetailProcessService = exportCapitalDetailProcessService;
            this._ExportMyInvestmentProcessService = exportMyInvestmentProcessService;
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

        [HttpPost, Route("GetMyInvestments")]
        public IActionResult GetMyInvestments(GetMyInvestment getMyInvestment)
        {
            getMyInvestment.Investor_Id = int.Parse(HttpContext.User.FindFirst("UserId").Value);
            getMyInvestment.Source = 1;
            var result = this._CapitalProcessService.ExecuteProcess<GetMyInvestment, MyInvestmentPagination>(getMyInvestment);

            if (getMyInvestment.Type == 0)
                return Ok(result);
            else
            {
                var msFile = this._ExportMyInvestmentProcessService.ExecuteProcess<ExportMyInvestment, MemoryStream>(new ExportMyInvestment()
                {
                    Type = getMyInvestment.Type,
                    MyInvestments = result.MyInvestments
                });

                return this.File(
                    fileContents: msFile.ToArray(),
                    contentType: getMyInvestment.Type == 1 ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" :
                        "application/pdf",
                    fileDownloadName: getMyInvestment.Type == 1 ? "MyInvestment" + DateTime.Now.ToString("yyyyMMdd") + ".xlsx" :
                        "MyInvestment" + DateTime.Now.ToString("yyyyMMdd") + ".pdf"
                );
            }
        }

        [HttpPost, Route("GetAnchorControl")]
        public IActionResult GetAnchorControl(AnchorByFilter anchorByFilter)
        {
            var result = this._CapitalProcessService.ExecuteProcess<AnchorByFilter, AnchorControlPagination>(anchorByFilter);

            if (anchorByFilter.Type == 0)
                return Ok(result);
            else
            {
                var msFile = this._ExportAnchorProcessService.ExecuteProcess<ExportAnchorControl, MemoryStream>(new ExportAnchorControl()
                {
                    Type = anchorByFilter.Type,
                    AnchorControls = result.AnchorControls
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

        [HttpPost, Route("ExportDetailCapital")]
        public IActionResult ExportCapitalDetail(ExportCapitalDetail exportCapitalDetail)
        {
            var msFile = this._ExportCapitalDetailProcessService.ExecuteProcess<ExportCapitalDetail, MemoryStream>(exportCapitalDetail);

            return this.File(
                    fileContents: msFile.ToArray(),
                    contentType: exportCapitalDetail.Type == 1 ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" :
                        "application/pdf",
                    fileDownloadName: exportCapitalDetail.Type == 1 ? "AccountStatus" + DateTime.Now.ToString("yyyyMMdd") + ".xlsx" :
                        "AccountStatus" + DateTime.Now.ToString("yyyyMMdd") + ".pdf"
                );
        }

        [HttpPost, Route("ChangeStatus")]
        public IActionResult ChangeStatus(CapitalChangeStatus capitalChangeStatus)
        {
            return Ok(this._CapitalWriteService.Update<CapitalChangeStatus, bool>(capitalChangeStatus));
        }
    }
}
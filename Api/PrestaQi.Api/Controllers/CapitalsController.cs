using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Composition;
using System.IO;
using System.Linq;
using System.Security.Claims;
using InsiscoCore.Base.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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
        IConfiguration _Configuration;
        private NotificationsMessageHandler _NotificationsMessageHandler { get; set; }


        public CapitalsController(
            IWriteService<Capital> capitalWriteService, 
            IRetrieveService<Capital> capitalRetrieveService,
            IProcessService<Capital> capitalProcessService,
            IProcessService<ExportAnchorControl> exportAnchorProcessService,
            IProcessService<ExportCapitalDetail> exportCapitalDetailProcessService,
            IProcessService<ExportMyInvestment> exportMyInvestmentProcessService,
            NotificationsMessageHandler notificationsMessageHandler,
            IConfiguration configuration)
        {
            this._CapitalWriteService = capitalWriteService;
            this._CapitalRetrieveService = capitalRetrieveService;
            this._CapitalProcessService = capitalProcessService;
            this._ExportAnchorProcessService = exportAnchorProcessService;
            this._ExportCapitalDetailProcessService = exportCapitalDetailProcessService;
            this._ExportMyInvestmentProcessService = exportMyInvestmentProcessService;

            this._NotificationsMessageHandler = notificationsMessageHandler;
            this._Configuration = configuration;
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

            var result = this._CapitalWriteService.Create<Capital, CreateCapital>(userCapital);

            if (result.Success)
            {
                var socketAdmin = this._NotificationsMessageHandler._ConnectionManager.GetSocketById(HttpContext.User.Claims.FirstOrDefault(p => p.Type == ClaimTypes.Email).Value);
                var socketClient = this._NotificationsMessageHandler._ConnectionManager.GetSocketById(result.Mail);

                if (socketAdmin != null)
                {
                    var notificationAdmin = _Configuration.GetSection("Notification").GetSection("SendCapitalCall").Get<SendNotification>();
                    notificationAdmin.Message = string.Format(notificationAdmin.Message, result.Investor);

                    _ = this._NotificationsMessageHandler.SendMessageAsync(socketAdmin, notificationAdmin);
                }

                if (socketClient != null)
                    _ = this._NotificationsMessageHandler.SendMessageAsync(socketClient, _Configuration.GetSection("Notification").GetSection("NewCapitalCall").Get<SendNotification>());

               
            }

            return Ok(result.Success);
        }

        [HttpPost, Route("GetMyInvestments")]
        public IActionResult GetMyInvestments(GetMyInvestment getMyInvestment)
        {
            getMyInvestment.Investor_Id = int.Parse(HttpContext.User.FindFirst("UserId").Value);
            getMyInvestment.Source = 1;

            var result = this._CapitalProcessService.ExecuteProcess<GetMyInvestment, MyInvestmentPagination>(getMyInvestment);

            return Ok(result);
        }

        [HttpPost, Route("GetAnchorControl")]
        public IActionResult GetAnchorControl(AnchorByFilter anchorByFilter)
        {
            var result = this._CapitalProcessService.ExecuteProcess<AnchorByFilter, AnchorControlPagination>(anchorByFilter);
            return Ok(result);
        }

        [HttpGet, Route("GetFile")]
        public IActionResult GetFile([FromQuery] int type)
        {
            var result = this._CapitalProcessService.ExecuteProcess<AnchorByFilter, AnchorControlPagination>(new AnchorByFilter()
            {
                Type = type
            });

            var msFile = this._ExportAnchorProcessService.ExecuteProcess<ExportAnchorControl, MemoryStream>(new ExportAnchorControl()
            {
                Type = type,
                AnchorControls = result.AnchorControls
            });

            return this.File(
                fileContents: msFile.ToArray(),
                contentType: type == 1 ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" :
                    "application/pdf",
                fileDownloadName: type == 1 ? "AnchorControl" + DateTime.Now.ToString("yyyyMMdd") + ".xlsx" :
                    "AnchorControl" + DateTime.Now.ToString("yyyyMMdd") + ".pdf"
            );
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

        [HttpGet, Route("ExportMyInvestment")]
        public IActionResult ExportMyInvestment([FromQuery] int type)
        {
            var result = this._CapitalProcessService.ExecuteProcess<GetMyInvestment, MyInvestmentPagination>(new GetMyInvestment()
            {
                Investor_Id = int.Parse(HttpContext.User.FindFirst("UserId").Value),
                Source = 1,
                Type = type
            });

            var msFile = this._ExportMyInvestmentProcessService.ExecuteProcess<ExportMyInvestment, MemoryStream>(new ExportMyInvestment()
            {
                Type = type,
                MyInvestments = result.MyInvestments
            });

            return this.File(
                fileContents: msFile.ToArray(),
                contentType: type == 1 ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" :
                    "application/pdf",
                fileDownloadName: type == 1 ? "MyInvestment" + DateTime.Now.ToString("yyyyMMdd") + ".xlsx" :
                    "MyInvestment" + DateTime.Now.ToString("yyyyMMdd") + ".pdf"
            );

        }

        [HttpPost, Route("ChangeStatus")]
        public IActionResult ChangeStatus(CapitalChangeStatus capitalChangeStatus)
        {
            return Ok(this._CapitalWriteService.Update<CapitalChangeStatus, bool>(capitalChangeStatus));
        }
    }
}
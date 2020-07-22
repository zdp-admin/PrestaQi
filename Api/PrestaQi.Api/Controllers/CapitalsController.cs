using System;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.ServiceModel;
using InsiscoCore.Base.Service;
using iText.Forms.Xfdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PrestaQi.Api.Configuration;
using PrestaQi.Model;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Model.Dto.Output;
using PrestaQi.Model.Enum;

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
        IWriteService<Model.Notification> _NotificationWriteService;
        IRetrieveService<User> _UserRetrieveService;
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
            IWriteService<Model.Notification> notificationWriteService,
            IRetrieveService<User> userRetrieveService,
        IConfiguration configuration)
        {
            this._CapitalWriteService = capitalWriteService;
            this._CapitalRetrieveService = capitalRetrieveService;
            this._CapitalProcessService = capitalProcessService;
            this._ExportAnchorProcessService = exportAnchorProcessService;
            this._ExportCapitalDetailProcessService = exportCapitalDetailProcessService;
            this._ExportMyInvestmentProcessService = exportMyInvestmentProcessService;
            this._NotificationWriteService = notificationWriteService;
            this._NotificationsMessageHandler = notificationsMessageHandler;
            this._Configuration = configuration;
            this._UserRetrieveService = userRetrieveService;
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
                var notificationAdmin = _Configuration.GetSection("Notification").GetSection("SendCapitalCall").Get<Model.Notification>();
                notificationAdmin.Message = string.Format(notificationAdmin.Message, result.Investor);
                var notificationClient = _Configuration.GetSection("Notification").GetSection("NewCapitalCall").Get<Model.Notification>();

                dynamic dynamicNoti = new ExpandoObject();
                dynamicNoti.Capital_Id = result.Capital_Id;
                dynamicNoti.Amount = result.Amount;

                notificationAdmin.User_Id = userCapital.Created_By;
                notificationAdmin.User_Type = (int)PrestaQiEnum.UserType.Administrador;
                notificationAdmin.Icon = PrestaQiEnum.NotificationIconType.done.ToString();
                notificationAdmin.NotificationType = PrestaQiEnum.NotificationType.CapitalCall;
                
                this._NotificationWriteService.Create(notificationAdmin);

                notificationClient.User_Id = userCapital.investor_id;
                notificationClient.User_Type = (int)PrestaQiEnum.UserType.Inversionista;
                notificationClient.Icon = PrestaQiEnum.NotificationIconType.info.ToString();
                notificationClient.NotificationType = PrestaQiEnum.NotificationType.CapitalCall;
                notificationClient.Data = dynamicNoti;

                this._NotificationWriteService.Create(notificationClient);
                _ = this._NotificationsMessageHandler.SendMessageToAllAsync(notificationAdmin);
                _ = this._NotificationsMessageHandler.SendMessageToAllAsync(notificationClient);
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
            var result = this._CapitalWriteService.Update<CapitalChangeStatus, CapitalChangeStatusResponse>(capitalChangeStatus);

            if (result.Success)
            {
                if (capitalChangeStatus.Status == (int)PrestaQiEnum.CapitalEnum.Enviado)
                {
                    var notificationAdmin = _Configuration.GetSection("Notification").GetSection("ChangeStatusCapital").Get<Model.Notification>();
                    notificationAdmin.Message = string.Format(notificationAdmin.Message, result.CapitalId.ToString(_Configuration["Format:FolioCapital"]),
                        ((PrestaQiEnum.CapitalEnum)capitalChangeStatus.Status).ToString());

                    dynamic dynamicNoti = new ExpandoObject();
                    dynamicNoti.Capital_Id = result.CapitalId;
                    notificationAdmin.NotificationType = PrestaQiEnum.NotificationType.ChangeStatusCapital;
                    notificationAdmin.Data = dynamicNoti;
                    notificationAdmin.User_Type = (int)PrestaQiEnum.UserType.Administrador;
                    notificationAdmin.Icon = PrestaQiEnum.NotificationIconType.info.ToString();

                    var admintratorList = this._UserRetrieveService.Where(p => p.Enabled == true && p.Deleted_At == null).Select(p => p.id).ToList();

                    foreach (var item in admintratorList)
                    {
                        notificationAdmin.User_Id = item;
                        this._NotificationWriteService.Create(notificationAdmin);
                        _ = this._NotificationsMessageHandler.SendMessageToAllAsync(notificationAdmin);
                        notificationAdmin.id = 0;
                    }


                }

            }

            return Ok(result);
        }
    }
}
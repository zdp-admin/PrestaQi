using DocumentFormat.OpenXml.Office2010.ExcelAc;
using InsiscoCore.Base.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PrestaQi.Api.Configuration;
using PrestaQi.Api.Notification;
using PrestaQi.Model;
using PrestaQi.Model.Configurations;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Model.Enum;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PrestaQi.Api.Controllers
{
    [Route("api/[controller]"), Authorize]
    [ApiController]
    public class AdvanceDetailController : CustomController
    {
        IWriteService<AdvanceDetail> _AdvanceDetailWriteService;
        IRetrieveService<AdvanceDetail> _AdvanceDetailRetrieveService;
        IRetrieveService<Advance> _AdvanceRetrieveService;

        private NotificationsMessageHandler _NotificationsMessageHandler { get; set; }
        IWriteService<Model.Notification> _NotificationWriteService;

        public IConfiguration Configuration { get; }

        public AdvanceDetailController(
            IWriteService<AdvanceDetail> advanceDetailWriteService,
            IRetrieveService<AdvanceDetail> advanceDetailRetrieveService,
            IWriteService<Model.Notification> notificationWriteService,
            IConfiguration configuration
            )
        {
            this._AdvanceDetailWriteService = advanceDetailWriteService;
            this._AdvanceDetailRetrieveService = advanceDetailRetrieveService;
            this._NotificationWriteService = notificationWriteService;
            Configuration = configuration;
        }

        

        [HttpPost, Route("SetPaidAdvance")]
        public IActionResult SetPaidAdvance(SetPayAdvance setPayAdvance)
        {
            var result = this._AdvanceDetailWriteService.Update<SetPayAdvance, bool>(setPayAdvance);

            if (result)
                SendNotifiationSetPaidAdvance(setPayAdvance.AdvanceIds);

            return Ok(result);
        }
        
        void SendNotifiationSetPaidAdvance(List<int> detailIds)
        {
            var advanceIds = this._AdvanceDetailRetrieveService.Where(p => detailIds.Contains(p.id)).Select(p => p.Advance_Id);
            var accreditedIds = this._AdvanceRetrieveService.Where(p => advanceIds.Contains(p.id)).Select(p => p.Accredited_Id).ToList();
            
            var notification = Configuration.GetSection("Notification").GetSection("SetPaymentAdvance").Get<Model.Notification>();

            notification.NotificationType = PrestaQiEnum.NotificationType.SetPaymentAdvance;
            notification.User_Type = (int)PrestaQiEnum.UserType.Acreditado;
            notification.Icon = PrestaQiEnum.NotificationIconType.info.ToString();

            foreach (var item in accreditedIds)
            {
                notification.User_Id = item;
                this._NotificationWriteService.Create(notification);
                _ = this._NotificationsMessageHandler.SendMessageToAllAsync(notification);
                notification.id = 0;
            }
        }
    }
}   
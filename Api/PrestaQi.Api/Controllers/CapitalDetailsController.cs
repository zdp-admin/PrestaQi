using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Wordprocessing;
using InsiscoCore.Base.Service;
using iText.Forms.Xfdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PrestaQi.Api.Configuration;
using PrestaQi.Model;
using PrestaQi.Model.Dto.Output;
using PrestaQi.Model.Enum;
using System.Dynamic;

namespace PrestaQi.Api.Controllers
{
    [Route("api/[controller]"), Authorize]
    [ApiController]
    public class CapitalDetailsController : CustomController
    {
        IWriteService<CapitalDetail> _CapitalDetailWriteService;
        IWriteService<Model.Notification> _NotificationWriteService;
        private NotificationsMessageHandler _NotificationsMessageHandler { get; set; }
        IConfiguration _Configuration;

        public CapitalDetailsController(
            IWriteService<CapitalDetail> capitalDetailWriteService,
            IConfiguration configuration,
            NotificationsMessageHandler notificationsMessageHandler,
            IWriteService<Model.Notification> notificationWriteService
            )
        {
            this._CapitalDetailWriteService = capitalDetailWriteService;
            this._NotificationWriteService = notificationWriteService;
            this._Configuration = configuration;
            this._NotificationsMessageHandler = notificationsMessageHandler;
        }

        [HttpPut, Route("SetPaymentPeriod")]
        public IActionResult SetPaymentPeriod(CapitalDetail capitalDetail)
        {
            var result = this._CapitalDetailWriteService.Update<CapitalDetail, SetPaymentPeriod>(capitalDetail);

            if (result.Success)
            {
                var socket = this._NotificationsMessageHandler._ConnectionManager.GetSocketById(result.Mail);

                var notification = !result.PaymentTotal ?
                         _Configuration.GetSection("Notification").GetSection("SetPaymentPeriod").Get<Model.Notification>() :
                         _Configuration.GetSection("Notification").GetSection("SetPaymentPeriodTotal").Get<Model.Notification>();

                notification.User_Id = result.UserId;
                notification.Icon = PrestaQiEnum.NotificationIconType.info.ToString();
                notification.User_Type = (int)PrestaQiEnum.UserType.Inversionista;
                notification.NotificationType = PrestaQiEnum.NotificationType.SetPaymentPeriod;
                notification.Data = new ExpandoObject();
                notification.Data.Period_Id = result.Period_Id;
                notification.Data.Capital_Id = result.Capital_Id;
                notification.Data.Payment = result.Payment;
                notification.Data.Investor_Id = result.UserId;

                this._NotificationWriteService.Create(notification);

                if (socket != null)
                    _ = this._NotificationsMessageHandler.SendMessageAsync(socket, notification);

            }

            return Ok(result.Success);
        }
    }
}
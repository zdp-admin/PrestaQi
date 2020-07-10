using InsiscoCore.Base.Service;
using iText.Forms.Xfdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PrestaQi.Api.Configuration;
using PrestaQi.Model;
using PrestaQi.Model.Dto.Output;
using PrestaQi.Model.Enum;

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
                         _Configuration.GetSection("Notification").GetSection("SetPaymentPeriod").Get<SendNotification>() :
                         _Configuration.GetSection("Notification").GetSection("SetPaymentPeriodTotal").Get<SendNotification>();

                if (socket != null)
                {
                    _ = this._NotificationsMessageHandler.SendMessageAsync(socket, notification);
                }

                this._NotificationWriteService.Create(new Model.Notification()
                {
                    Message = notification.Message,
                    Title = notification.Title,
                    User_Id = result.UserId,
                    User_Type = (int)PrestaQiEnum.UserType.Inversionista
                });

            }

            return Ok(result.Success);
        }
    }
}
using InsiscoCore.Base.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PrestaQi.Api.Configuration;
using PrestaQi.Model;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Model.Dto.Output;
using PrestaQi.Model.Enum;

namespace PrestaQi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceController : CustomController
    {
        IWriteService<SpeiResponse> _SpeiResponseWriteService;
        IWriteService<Model.Notification> _NotificationWriteService;
        IConfiguration _Configuration;
        private NotificationsMessageHandler _NotificationsMessageHandler { get; set; }

        public ServiceController(
            IWriteService<SpeiResponse> speiResponseWriteService,
            IConfiguration configuration,
            NotificationsMessageHandler notificationsMessageHandler,
            IWriteService<Model.Notification> notificationWriteService
            )
        {
            this._SpeiResponseWriteService = speiResponseWriteService;
            this._Configuration = configuration;
            this._NotificationsMessageHandler = notificationsMessageHandler;
            this._NotificationWriteService = notificationWriteService;
        }

        [HttpPost, Route("stp/status")]
        public IActionResult GetStatusAccredited(StateChange stateChange)
        {
            var result = this._SpeiResponseWriteService.Update<StateChange, SpeiTransactionResult>(stateChange);
            var socket = this._NotificationsMessageHandler._ConnectionManager.GetSocketById(result.Mail);

            if (result.Success)
            {
                var notification = new SendNotification();

                if (stateChange.CausaDevolucion < 0)
                    notification = _Configuration.GetSection("Notification").GetSection("AdvanceSuccess").Get<SendNotification>();
                if (stateChange.CausaDevolucion > 0)
                {
                    notification = _Configuration.GetSection("Notification").GetSection("AdvanceFail").Get<SendNotification>();
                    notification.Message = string.Format(notification.Message, result.Message);
                }

                var notificationObj = new Model.Notification()
                {
                    Message = notification.Message,
                    Title = notification.Title,
                    User_Id = result.UserId,
                    User_Type = (int)PrestaQiEnum.UserType.Acreditado,
                    NotificationType = PrestaQiEnum.NotificationType.AdvanceStatus
                };

                this._NotificationWriteService.Create(notificationObj);

                if (socket != null)
                {
                    notification.Id = notificationObj.id;
                    _ = this._NotificationsMessageHandler.SendMessageAsync(socket, notification);
                }
            }

            return Ok(result.Success);
        }

    }
}
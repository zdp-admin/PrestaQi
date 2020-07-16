using InsiscoCore.Base.Service;
using iText.Forms.Xfdf;
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
            
            if (result.Success)
            {
                var notification = new Model.Notification();

                if (stateChange.CausaDevolucion == 0)
                {
                    notification = _Configuration.GetSection("Notification").GetSection("AdvanceSuccess").Get<Model.Notification>();
                    notification.Icon = PrestaQiEnum.NotificationIconType.done.ToString();
                }
                if (stateChange.CausaDevolucion > 0)
                {
                    notification = _Configuration.GetSection("Notification").GetSection("AdvanceFail").Get<Model.Notification>();
                    notification.Message = string.Format(notification.Message, result.Message);
                    notification.Icon = PrestaQiEnum.NotificationIconType.error.ToString();
                }

                notification.User_Id = result.UserId;
                notification.User_Type = (int)PrestaQiEnum.UserType.Acreditado;
                notification.NotificationType = PrestaQiEnum.NotificationType.AdvanceStatus;
            
                this._NotificationWriteService.Create(notification);

                _ = this._NotificationsMessageHandler.SendMessageToAllAsync(notification);
            }

            return Ok(result.Success);
        }

    }
}
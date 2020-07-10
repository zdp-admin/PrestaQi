using System.Collections.Generic;
using System.Linq;
using InsiscoCore.Base.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.Extensions.Configuration;
using PrestaQi.Api.Configuration;
using PrestaQi.Model;
using PrestaQi.Model.Dto;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Model.Dto.Output;

namespace PrestaQi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceController : CustomController
    {
        IWriteService<SpeiResponse> _SpeiResponseWriteService;
        IConfiguration _Configuration;
        private NotificationsMessageHandler _NotificationsMessageHandler { get; set; }

        public ServiceController(
            IWriteService<SpeiResponse> speiResponseWriteService,
            IConfiguration configuration,
            NotificationsMessageHandler notificationsMessageHandler
            )
        {
            this._SpeiResponseWriteService = speiResponseWriteService;
            this._Configuration = configuration;
            this._NotificationsMessageHandler = notificationsMessageHandler;
        }

        [HttpPost, Route("stp/status")]
        public IActionResult GetStatusAccredited(StateChange stateChange)
        {
            var result = this._SpeiResponseWriteService.Update<StateChange, SpeiTransactionResult>(stateChange);

            var socket = this._NotificationsMessageHandler._ConnectionManager.GetSocketById(result.Mail);

            if (socket != null)
            {
                if (result.Success && stateChange.CausaDevolucion < 0)
                {
                    _ = this._NotificationsMessageHandler.SendMessageAsync(socket, _Configuration.GetSection("Notification").GetSection("AdvanceSuccess").Get<SendNotification>());
                }

                if (result.Success && stateChange.CausaDevolucion > 0)
                {
                    var notification = _Configuration.GetSection("Notification").GetSection("AdvanceFail").Get<SendNotification>();
                    notification.Message = string.Format(notification.Message, result.Message);

                    _ = this._NotificationsMessageHandler.SendMessageAsync(socket, notification);
                }
            }

            return Ok(result.Success);
        }

    }
}
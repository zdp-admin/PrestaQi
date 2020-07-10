using InsiscoCore.Base.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PrestaQi.Api.Configuration;
using PrestaQi.Model;
using PrestaQi.Model.Dto.Output;

namespace PrestaQi.Api.Controllers
{
    [Route("api/[controller]"), Authorize]
    [ApiController]
    public class CapitalDetailsController : CustomController
    {
        IWriteService<CapitalDetail> _CapitalDetailWriteService;
        private NotificationsMessageHandler _NotificationsMessageHandler { get; set; }
        IConfiguration _Configuration;

        public CapitalDetailsController(
            IWriteService<CapitalDetail> capitalDetailWriteService,
            IConfiguration configuration,
            NotificationsMessageHandler notificationsMessageHandler
            )
        {
            this._CapitalDetailWriteService = capitalDetailWriteService;
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

                if (socket != null)
                {
                    _ = this._NotificationsMessageHandler.SendMessageAsync(socket, 
                        !result.PaymentTotal ?
                         _Configuration.GetSection("Notification").GetSection("SetPaymentPeriod").Get<SendNotification>() :
                         _Configuration.GetSection("Notification").GetSection("SetPaymentPeriodTotal").Get<SendNotification>()
                        );
                }
            }

            return Ok(result.Success);
        }
    }
}
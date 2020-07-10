using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using InsiscoCore.Base.Service;
using Microsoft.AspNetCore.Authorization;
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
    public class CapitalDetailsController : CustomController
    {
        IWriteService<CapitalDetail> _CapitalDetailWriteService;
        IConfiguration _Configuration;
        private NotificationsMessageHandler _NotificationsMessageHandler { get; set; }

        public CapitalDetailsController(
            IWriteService<CapitalDetail> capitalDetailWriteService,
            NotificationsMessageHandler notificationsMessageHandler,
            IConfiguration configuration
            )
        {
            this._CapitalDetailWriteService = capitalDetailWriteService;
            this._NotificationsMessageHandler = notificationsMessageHandler;
            this._Configuration = configuration;
        }

        [HttpPut, Route("SetPaymentPeriod")]
        public IActionResult SetPaymentPeriod(CapitalDetail capitalDetail)
        {
            var result = this._CapitalDetailWriteService.Update(capitalDetail);

            if (result)
                _ = this._NotificationsMessageHandler.SendMessageToAllAsync(_Configuration["Notification:SetPaymentPeriod"]);

            return Ok(result);
        }
    }
}
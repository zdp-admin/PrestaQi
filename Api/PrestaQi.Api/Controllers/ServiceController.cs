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

namespace PrestaQi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceController : CustomController
    {
        IWriteService<SpeiResponse> _SpeiResponseWriteService;
        IConfiguration _Configuration;
        IRetrieveService<Repayment> _Repayment;
        private NotificationsMessageHandler _NotificationsMessageHandler { get; set; }

        public ServiceController(
            IWriteService<SpeiResponse> speiResponseWriteService,
            NotificationsMessageHandler notificationsMessageHandler,
            IConfiguration configuration,
            IRetrieveService<Repayment> repayment
            )
        {
            this._SpeiResponseWriteService = speiResponseWriteService;
            this._NotificationsMessageHandler = notificationsMessageHandler;
            this._Configuration = configuration;
            this._Repayment = repayment;
        }

        [HttpPost, Route("stp/status")]
        public IActionResult GetStatusAccredited(StateChange stateChange)
        {
            var result = this._SpeiResponseWriteService.Update<StateChange, bool>(stateChange);

            if (result && stateChange.CausaDevolucion < 0)
                _ = this._NotificationsMessageHandler.SendMessageToAllAsync(_Configuration["Notification:AdvanceSuccess"]);
            if (result && stateChange.CausaDevolucion > 0)
            {
                string message = this._Repayment.Find(stateChange.CausaDevolucion).Description;
                _ = this._NotificationsMessageHandler.SendMessageToAllAsync(string.Format(_Configuration["Notification:AdvanceFail"], message));
            }

            return Ok(result);
        }

    }
}
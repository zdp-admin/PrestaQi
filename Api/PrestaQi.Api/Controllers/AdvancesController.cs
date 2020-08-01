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
    public class AdvancesController : CustomController
    {
        IWriteService<Advance> _AdvanceWriteService;
        IRetrieveService<Advance> _AdvanceRetrieveService;
        IProcessService<Advance> _AdvanceProcessService;
        IProcessService<PaidAdvance> _PaidAdvanceProcessService;
        IProcessService<ExportAdvance> _ExportAdvanceProcessService;
        private NotificationsMessageHandler _NotificationsMessageHandler { get; set; }
        IWriteService<Model.Notification> _NotificationWriteService;

        public IConfiguration Configuration { get; }

        public AdvancesController(
            IWriteService<Advance> advanceWriteService,
            IRetrieveService<Advance> advanceRetrieveService,
            IProcessService<Advance> advanceProcessService,
            IProcessService<PaidAdvance> paidAdvanceProcessService,
            IWriteService<Model.Notification> notificationWriteService,
            NotificationsMessageHandler notificationsMessageHandler,
            IConfiguration configuration,
            IProcessService<ExportAdvance> exportAdvanceProcessService
            )
        {
            this._AdvanceWriteService = advanceWriteService;
            this._AdvanceRetrieveService = advanceRetrieveService;
            this._AdvanceProcessService = advanceProcessService;
            this._NotificationsMessageHandler = notificationsMessageHandler;
            this._PaidAdvanceProcessService = paidAdvanceProcessService;
            this._NotificationWriteService = notificationWriteService;
            this._ExportAdvanceProcessService = exportAdvanceProcessService;
            Configuration = configuration;
        }

        [HttpPost, Route("CalculateAdvance")]
        public IActionResult CalculateAdvance(CalculateAmount calculateAmount)
        {
            calculateAmount.Accredited_Id = int.Parse(HttpContext.User.FindFirst("UserId").Value);
            return Ok(this._AdvanceProcessService.ExecuteProcess<CalculateAmount, Advance>(calculateAmount));
        }

        [HttpPost]
        public IActionResult Post(CalculateAmount calculateAmount)
        {
            calculateAmount.Accredited_Id = int.Parse(HttpContext.User.FindFirst("UserId").Value);

            var limitCredit = this._AdvanceProcessService.ExecuteProcess<CalculateAmount, Advance>(new CalculateAmount()
            {
                Accredited_Id = calculateAmount.Accredited_Id,
                Amount = 0
            });

            if (calculateAmount.Amount >= limitCredit.Maximum_Amount)
                throw new SystemValidationException($"No se puede realizar el préstamos, ya que la cantidad {calculateAmount.Amount:C} " +
                    $"excede el monto máximo {limitCredit.Maximum_Amount:C}");
            else
                 return Ok(this._AdvanceWriteService.Create<CalculateAmount, bool>(calculateAmount), "Generator Advance");
        }

        [HttpGet, Route("GetByAccredited/{id}")]
        public IActionResult GetByAccredited(int id)
        {
            return Ok(this._AdvanceRetrieveService.Where(p => p.Accredited_Id == id));
        }

        [HttpPost, Route("SetPaidAdvance")]
        public IActionResult SetPaidAdvance(SetPayAdvance setPayAdvance)
        {
            var result = this._PaidAdvanceProcessService.ExecuteProcess<SetPayAdvance, bool>(setPayAdvance);

            if (result)
               SendNotifiationSetPaidAdvance(setPayAdvance.AdvanceIds);

            return Ok(result);
        }
        
        [HttpPut, Route("CalculatePromotional")]
        public IActionResult CalucaltePromotional(CalculatePromotional calculatePromotional)
        {
            var advance = this._AdvanceProcessService.ExecuteProcess<CalculatePromotional, Advance>(calculatePromotional);
            this._AdvanceWriteService.Update(advance);
            return Ok(advance);
        }

        [HttpGet, Route("ExportMyAdvances/{id}")]
        public IActionResult ExportMyAdvances(int id)
        {
            var file = this._ExportAdvanceProcessService.ExecuteProcess<ExportMyAdvance, MemoryStream>(new ExportMyAdvance()
            {
                 Accredited_Id = id,
                  Advances = this._AdvanceRetrieveService.Where(p => p.Accredited_Id == id).ToList()
            });

            return this.File(
                    fileContents: file.ToArray(),
                    contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileDownloadName: "MyAdvances" + DateTime.Now.ToString("yyyyMMdd") + ".xlsx"
                );
        }

        void SendNotifiationSetPaidAdvance(List<int> advanceIds)
        {
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
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using InsiscoCore.Base.Data;
using InsiscoCore.Base.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PrestaQi.Api.Configuration;
using PrestaQi.Api.Notification;
using PrestaQi.Model;
using PrestaQi.Model.Configurations;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Model.Dto.Output;
using PrestaQi.Model.Enum;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PrestaQi.Api.Controllers
{
    [Route("api/[controller]"), Authorize]
    [ApiController]
    public class AdvancesController : CustomController
    {
        IRetrieveRepository<Accredited> _AcreditedRetrieveService;
        IWriteService<Advance> _AdvanceWriteService;
        IRetrieveService<Advance> _AdvanceRetrieveService;
        IProcessService<Advance> _AdvanceProcessService;
        IProcessService<PaidAdvance> _PaidAdvanceProcessService;
        IProcessService<ExportAdvance> _ExportAdvanceProcessService;
        private NotificationsMessageHandler _NotificationsMessageHandler { get; set; }
        IWriteService<Model.Notification> _NotificationWriteService;
        IWriteService<AdvanceDetail> _AdvanceDetailWriteService;
        IRetrieveService<DetailsAdvance> _DetailsAdvanceRetreviewService;
        IWriteService<DetailsAdvance> _DetailsAdvanceWriteService;
        IRetrieveService<DetailsByAdvance> _DetailsByAdvanceRetrieve;

        public IConfiguration Configuration { get; }

        public AdvancesController(
            IWriteService<Advance> advanceWriteService,
            IRetrieveService<Advance> advanceRetrieveService,
            IProcessService<Advance> advanceProcessService,
            IProcessService<PaidAdvance> paidAdvanceProcessService,
            IWriteService<Model.Notification> notificationWriteService,
            NotificationsMessageHandler notificationsMessageHandler,
            IConfiguration configuration,
            IProcessService<ExportAdvance> exportAdvanceProcessService,
            IWriteService<AdvanceDetail> advanceDetailWriteService,
            IRetrieveService<DetailsAdvance> detailsAdvance,
            IWriteService<DetailsAdvance> detailsAdvanceWrite,
            IRetrieveService<DetailsByAdvance> detailsByAdvanceRetrieve,
            IRetrieveRepository<Accredited> acreditedRetrieveService
            )
        {
            this._AdvanceWriteService = advanceWriteService;
            this._AdvanceRetrieveService = advanceRetrieveService;
            this._AdvanceProcessService = advanceProcessService;
            this._NotificationsMessageHandler = notificationsMessageHandler;
            this._PaidAdvanceProcessService = paidAdvanceProcessService;
            this._NotificationWriteService = notificationWriteService;
            this._ExportAdvanceProcessService = exportAdvanceProcessService;
            this._AdvanceDetailWriteService = advanceDetailWriteService;
            this._DetailsAdvanceRetreviewService = detailsAdvance;
            this._DetailsAdvanceWriteService = detailsAdvanceWrite;
            this._DetailsByAdvanceRetrieve = detailsByAdvanceRetrieve;
            this._AcreditedRetrieveService = acreditedRetrieveService;
            Configuration = configuration;
        }

        [HttpPost, Route("CalculateAdvance")]
        public IActionResult CalculateAdvance(CalculateAmount calculateAmount)
        {
            
            if (calculateAmount.Accredited_Id <= 0)
            {
                calculateAmount.Accredited_Id = int.Parse(HttpContext.User.FindFirst("UserId").Value);
            }

            return Ok(this._AdvanceProcessService.ExecuteProcess<CalculateAmount, AdvanceAndDetails>(calculateAmount));
        }

        [HttpPost]
        public IActionResult Post([FromForm] CalculateAmount calculateAmount)
        {
            if (calculateAmount.Accredited_Id <= 0)
            {
                calculateAmount.Accredited_Id = int.Parse(HttpContext.User.FindFirst("UserId").Value);
            }

            var limitCredit = this._AdvanceProcessService.ExecuteProcess<CalculateAmount, AdvanceAndDetails>(new CalculateAmount()
            {
                Accredited_Id = calculateAmount.Accredited_Id,
                Amount = 0
            });

            if (calculateAmount.PaySheetsJson != null)
            {
                calculateAmount.PaySheets = JsonSerializer.Deserialize<List<PaySheetUser>>(calculateAmount.PaySheetsJson, new JsonSerializerOptions
                {
                    NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString
                });
            }

            foreach(var file in Request.Form.Files)
            {
                var index = calculateAmount.PaySheets.FindIndex(sheet => sheet.UUID == file.Name);

                if (index >= 0)
                {
                    calculateAmount.PaySheets[index].NameFile = file.FileName;
                    using (var ms = new MemoryStream())
                    {
                        file.CopyTo(ms);
                        calculateAmount.PaySheets[index].File = ms.ToArray();
                    }
                }
            }

            if (calculateAmount.Amount > limitCredit.advance.Maximum_Amount)
                throw new SystemValidationException($"No se puede realizar el préstamos, ya que la cantidad {calculateAmount.Amount} " +
                    $"excede el monto máximo {limitCredit.advance.Maximum_Amount}");
            else
                 return Ok(this._AdvanceWriteService.Create<CalculateAmount, Advance>(calculateAmount), "Generator Advance");
        }

        [HttpGet, Route("GetByAccredited/{id}")]
        public IActionResult GetByAccredited(int id)
        {
            return Ok(this._AdvanceProcessService.ExecuteProcess<int, MyAdvances>(id));
        }

        [HttpPost, Route("SetPaidAdvance")]
        public IActionResult SetPaidAdvance(SetPayAdvance setPayAdvance)
        {
            var result = this._PaidAdvanceProcessService.ExecuteProcess<SetPayAdvance, bool>(setPayAdvance);

            if (result && Configuration["environment"] == "prod")
               SendNotifiationSetPaidAdvance(setPayAdvance.AdvanceIds);

            return Ok(result);
        }
        
        [HttpPut, Route("CalculatePromotional")]
        public IActionResult CalucaltePromotional(CalculatePromotional calculatePromotional)
        {
            var detailAdvanceAll = this._DetailsAdvanceRetreviewService.Where(da => da.Accredited_Id == calculatePromotional.Accredited_Id).ToList();
            var accredited = this._AcreditedRetrieveService.Where(a => a.id == calculatePromotional.Accredited_Id).First();
            if (accredited.Type_Contract_Id == (int)PrestaQiEnum.AccreditedContractType.WagesAndSalaries)
            {
                if (!calculatePromotional.Is_Details)
                {
                    var advance = this._AdvanceProcessService.ExecuteProcess<CalculatePromotional, Advance>(calculatePromotional);
                    this._AdvanceWriteService.Update(advance);

                    advance.details = this._DetailsByAdvanceRetrieve.Where(da => da.Advance_Id == advance.id).OrderBy(da => da.Detail_Id).ToList();

                    advance.details.ForEach(d =>
                    {
                        d.Detail = detailAdvanceAll.Where(da => da.id == d.Detail_Id).FirstOrDefault();
                    });

                    return Ok(advance);
                }
                else
                {
                    DetailsAdvance detailsAdvance = this._DetailsAdvanceRetreviewService.Where(d => d.id == calculatePromotional.Advance_Id).First();
                    detailsAdvance.Promotional_Setting = calculatePromotional.Amount;
                    this._DetailsAdvanceWriteService.Update(detailsAdvance);

                    var advance = this._AdvanceRetrieveService.Where(a => a.id == detailsAdvance.Advance_Id).FirstOrDefault();

                    advance.details = this._DetailsByAdvanceRetrieve.Where(da => da.Advance_Id == advance.id).OrderBy(da => da.Detail_Id).ToList();

                    advance.details.ForEach(d =>
                    {
                        d.Detail = detailAdvanceAll.Where(da => da.id == d.Detail_Id).FirstOrDefault();
                        d.Detail.Total_Payment += double.Parse(d.Detail.Promotional_Setting.ToString());
                    });

                    return Ok(advance);
                }
            } else
            {
                var advance = this._AdvanceProcessService.ExecuteProcess<CalculatePromotional, Advance>(calculatePromotional);
                this._AdvanceWriteService.Update(advance);

                return Ok(advance);
            }
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

        void SendNotifiationSetPaidAdvance(List<PaidAdvanceType> advanceIds)
        {
            var accreditedIds = advanceIds.Select(p => p.Accredited_Id).Distinct().ToList();   
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
using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using InsiscoCore.Base.Service;
using iText.Forms.Xfdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrestaQi.Api.Configuration;
using PrestaQi.Model;

namespace PrestaQi.Api.Controllers
{
    [Route("api/[controller]"), Authorize]
    [ApiController]
    public class NotificationsController : CustomController
    {
        IWriteService<Model.Notification> _NotificationWriteService;
        IRetrieveService<Model.Notification> _NotificationRetrieveService;

        public NotificationsController(
            IWriteService<Model.Notification> notificationWriteService, 
            IRetrieveService<Model.Notification> notificationRetrieveService)
        {
            this._NotificationWriteService = notificationWriteService;
            this._NotificationRetrieveService = notificationRetrieveService;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var result = this._NotificationRetrieveService.Where(p => p.User_Type == int.Parse(HttpContext.User.FindFirst("Type").Value) &&
            p.User_Id == int.Parse(HttpContext.User.FindFirst("UserId").Value) && !p.Is_Read).ToList();
            return Ok(result);
        }

        [HttpPut, Route("DisableNotification")]
        public IActionResult DisableNotification(List<int> notificationsId)
        {
            return Ok(this._NotificationWriteService.Update<List<int>, bool>(notificationsId));
        }

    }
}
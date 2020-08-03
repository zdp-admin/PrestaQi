using DocumentFormat.OpenXml.Drawing.Charts;
using InsiscoCore.Base.Service;
using iText.Forms.Xfdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PrestaQi.Api.Configuration;
using PrestaQi.Model;
using PrestaQi.Model.Dto;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Model.Dto.Output;
using PrestaQi.Model.Enum;
using PrestaQi.Service.Tools;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace PrestaQi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdministrativeController : CustomController
    {
        IRetrieveService<User> _UserRetrieveService;
        IWriteService<User> _UserWriteService;
        IWriteService<Investor> _InvestorWriteService;
        IWriteService<Accredited> _AccreditedWriteService;
        IProcessService<User> _UserProcessService;
        IRetrieveService<Contact> _ContactRetrieveService;
        IRetrieveService<Model.Configuration> _ConfigurationRetrieveService;
        IConfiguration _Configuration;
        private NotificationsMessageHandler _NotificationsMessageHandler { get; set; }
        IWriteService<Model.Notification> _NotificationWriteService;


        public AdministrativeController(
            IRetrieveService<User> _UserRetrieveService,
            IWriteService<Investor> investorWriteService,
            IWriteService<Accredited> accreditedWriteService,
            IProcessService<User> userProcessService,
            IWriteService<User> userWriteService,
            IRetrieveService<Contact> contactRetrieveService,
            IWriteService<Model.Notification> notificationWriteService,
            IConfiguration configuration,
            NotificationsMessageHandler notificationsMessageHandler,
            IRetrieveService<Model.Configuration> configurationRetrieveService
            ) : base()
        {
            this._InvestorWriteService = investorWriteService;
            this._AccreditedWriteService = accreditedWriteService;
            this._UserProcessService = userProcessService;
            this._UserWriteService = userWriteService;
            this._UserRetrieveService = _UserRetrieveService;
            this._ContactRetrieveService = contactRetrieveService;
            this._NotificationWriteService = notificationWriteService;
            this._Configuration = configuration;
            this._NotificationsMessageHandler = notificationsMessageHandler;
            this._ConfigurationRetrieveService = configurationRetrieveService;
        }

        [HttpPut, Route("[action]"), Authorize]
        public IActionResult ChangePassword(ChangePassword changePassword)
        {
            changePassword.Type = int.Parse(HttpContext.User.FindFirst("Type").Value);
            changePassword.User_Id = int.Parse(HttpContext.User.FindFirst("UserId").Value);

            bool changed = false;

            if (changePassword.Type == 1)
                changed = this._UserWriteService.Update<ChangePassword, bool>(changePassword);
            if (changePassword.Type == 2)
                changed = this._InvestorWriteService.Update<ChangePassword, bool>(changePassword);
            if (changePassword.Type == 3)
                changed = this._AccreditedWriteService.Update<ChangePassword, bool>(changePassword);


            if (changed)
            {
                var notification = _Configuration.GetSection("Notification").GetSection("ChangePassword").Get<Model.Notification>();

                notification.User_Id = changePassword.User_Id;
                notification.User_Type = changePassword.Type;
                notification.NotificationType = PrestaQiEnum.NotificationType.ChangePassword;
                notification.Icon = PrestaQiEnum.NotificationIconType.info.ToString();
                bool create = this._NotificationWriteService.Create(notification);
                
                _ = this._NotificationsMessageHandler.SendMessageToAllAsync(notification);
                
            }

            return Ok(changed, "Password Changed!");
        }

        [HttpPut, Route("RecoveryPassword"), AllowAnonymous]
        public IActionResult RecoveryPassword(RecoveryPassword recoveryPassword)
        {
            var contacts = this._ContactRetrieveService.Where(p => p.Enabled == true).ToList();

            var result = this._UserProcessService.ExecuteProcess<RecoveryPassword, RecoveryPasswordData>(recoveryPassword);
            string name = string.Empty;
            bool recovery = false;

            if (result.UserType == Model.Enum.PrestaQiEnum.UserType.Administrador)
            {
                recovery = this._UserWriteService.Update(result.Data as User);
                name = (result.Data as User).First_Name;
            }
            if (result.UserType == Model.Enum.PrestaQiEnum.UserType.Inversionista)
            {
                recovery = this._InvestorWriteService.Update(result.Data as Investor);
                name = (result.Data as Investor).First_Name;
            }
            if (result.UserType == Model.Enum.PrestaQiEnum.UserType.Acreditado)
            {
                recovery = this._AccreditedWriteService.Update(result.Data as Accredited);
                name = (result.Data as Accredited).First_Name;
            }

            if (recovery)
            {
                return Ok(this._UserProcessService.ExecuteProcess<SendMailRecoveryPassword, bool>(
                    new SendMailRecoveryPassword()
                        {
                            Mail = result.Mail,
                            Password = result.Password,
                            Name = name,
                            Contacts = contacts
                    }
                    ));
            }

            return Ok(recovery);

        }

        [HttpPost, Route("SaveFileUsers"), Authorize]
        public IActionResult SaveFileUsers(FileUser fileUser)
        {
            var response = this._UserProcessService.ExecuteProcess<FileUser, ResponseFile>(fileUser);

            if (fileUser.Type == (int)PrestaQiEnum.UserType.Administrador)
            {
                if (((List<User>)response.Entities).Count > 0)
                    this._UserWriteService.Create(response.Entities as List<User>);
            }
            if (fileUser.Type == (int)PrestaQiEnum.UserType.Inversionista)
            {
                if (((List<Investor>)response.Entities).Count > 0)
                    this._InvestorWriteService.Create(response.Entities as List<Investor>);
            }
            if (fileUser.Type == (int)PrestaQiEnum.UserType.Acreditado)
            {
                if (((List<Accredited>)response.Entities).Count > 0)
                    this._AccreditedWriteService.Create(response.Entities as List<Accredited>);
            }

            return Ok(true, response.Message.Length > 0 ? response.Message.ToString() : string.Empty);
        }

        [HttpPut, Route("ChangeStatusUser"), Authorize]
        public IActionResult ChangeStatusUser(DisableUser disableUser)
        {
            return Ok(ChangeStatus(disableUser).Item1);
        }

        [HttpPut, Route("DeleteAccount"), Authorize]
        public IActionResult DeleteAccount(DisableUser disableUser)
        {
            disableUser.Value = false;
            var result = ChangeStatus(disableUser);

            if (result.Item1)
            {
                SendNotificationChangeStatus(result.Item2);
            }

            return Ok(result.Item1);
        }

        (bool, string) ChangeStatus(DisableUser disableUser)
        {
            string name = string.Empty;
            bool success = false;

            var user = this._UserRetrieveService.RetrieveResult<DisableUser, UserLogin>(disableUser);

            if (disableUser.Type == (int)PrestaQiEnum.UserType.Administrador)
            {
                success = this._UserWriteService.Update<ChangeStatusUser, bool>(new Model.Dto.Input.ChangeStatusUser() { User = user.User });
                name = $"{((User)user.User).First_Name} {((User)user.User).Last_Name}";
            }
            if (disableUser.Type == (int)PrestaQiEnum.UserType.Inversionista)
            {
                success = this._InvestorWriteService.Update<ChangeStatusUser, bool>(new Model.Dto.Input.ChangeStatusUser() { User = user.User });
                name = $"{((Investor)user.User).First_Name} {((Investor)user.User).Last_Name}";
            }
            if (disableUser.Type == (int)PrestaQiEnum.UserType.Acreditado)
            {
                success = this._AccreditedWriteService.Update<ChangeStatusUser, bool>(new Model.Dto.Input.ChangeStatusUser() { User = user.User });
                name = $"{((Accredited)user.User).First_Name} {((Accredited)user.User).Last_Name}";
            }

            return (success, name);
        }

        void SendNotificationChangeStatus(string name)
        {
            var notificationAdmin = _Configuration.GetSection("Notification").GetSection("DeleteUser").Get<Model.Notification>();
            notificationAdmin.Message = string.Format(notificationAdmin.Message, name);

            notificationAdmin.NotificationType = PrestaQiEnum.NotificationType.DeleteUser;
            notificationAdmin.User_Type = (int)PrestaQiEnum.UserType.Administrador;
            notificationAdmin.Icon = PrestaQiEnum.NotificationIconType.info.ToString();

            var admintratorList = _UserRetrieveService.Where(p => p.Enabled == true && p.Deleted_At == null).ToList();

            foreach (var item in admintratorList)
            {
                notificationAdmin.User_Id = item.id;
                _NotificationWriteService.Create(notificationAdmin);
                _ = _NotificationsMessageHandler.SendMessageToAllAsync(notificationAdmin);
                notificationAdmin.id = 0;
            }

            var configurations = _ConfigurationRetrieveService.Where(p => p.Enabled == true).ToList();
            var mailConf = configurations.FirstOrDefault(p => p.Configuration_Name == "EMAIL_CONFIG");

            Utilities.SendEmail(admintratorList.Select(p => p.Mail).ToList(), new MessageMail()
            {
                Message = notificationAdmin.Message,
                Subject = notificationAdmin.Title
            }, mailConf);
        }
    }
}

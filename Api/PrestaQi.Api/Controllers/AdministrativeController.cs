using InsiscoCore.Base.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrestaQi.Api.Configuration;
using PrestaQi.Model;
using PrestaQi.Model.Dto;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Model.Dto.Output;
using PrestaQi.Model.Enum;
using System.Collections.Generic;

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

        public AdministrativeController(
            IRetrieveService<User> _UserRetrieveService,
            IWriteService<Investor> investorWriteService,
            IWriteService<Accredited> accreditedWriteService,
            IProcessService<User> userProcessService,
            IWriteService<User> userWriteService
            )
        {
            this._InvestorWriteService = investorWriteService;
            this._AccreditedWriteService = accreditedWriteService;
            this._UserProcessService = userProcessService;
            this._UserWriteService = userWriteService;
            this._UserRetrieveService = _UserRetrieveService;
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

            return Ok(changed, "Password Changed!");
        }

        [HttpPut, Route("RecoveryPassword"), AllowAnonymous]
        public IActionResult RecoveryPassword(RecoveryPassword recoveryPassword)
        {
            var result = this._UserProcessService.ExecuteProcess<RecoveryPassword, RecoveryPasswordData>(recoveryPassword);

            bool recovery = false;

            if (result.UserType == Model.Enum.PrestaQiEnum.UserType.Administrador)
                recovery = this._UserWriteService.Update(result.Data as User);
            if (result.UserType == Model.Enum.PrestaQiEnum.UserType.Inversionista)
                recovery = this._InvestorWriteService.Update(result.Data as Investor);
            if (result.UserType == Model.Enum.PrestaQiEnum.UserType.Acreditado)
                recovery = this._AccreditedWriteService.Update(result.Data as Accredited);

            if (recovery)
            {
                return Ok(this._UserProcessService.ExecuteProcess<SendMailRecoveryPassword, bool>(
                    new SendMailRecoveryPassword()
                        {
                            Mail = result.Mail,
                            Password = result.Password
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
            var user = this._UserRetrieveService.RetrieveResult<DisableUser, UserLogin>(disableUser);

            bool success = false;

            if (disableUser.Type == 1)
                success = this._UserWriteService.Update<ChangeStatusUser, bool>(new Model.Dto.Input.ChangeStatusUser() {  User = user.User});
            if (disableUser.Type == 2)
                success = this._InvestorWriteService.Update<ChangeStatusUser, bool>(new Model.Dto.Input.ChangeStatusUser() { User = user.User });
            if (disableUser.Type == 3)
                success = this._AccreditedWriteService.Update<ChangeStatusUser, bool>(new Model.Dto.Input.ChangeStatusUser() { User = user.User });

            return Ok(success);
        }
    }
}

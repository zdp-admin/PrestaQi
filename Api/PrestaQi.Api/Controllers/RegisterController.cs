using InsiscoCore.Base.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrestaQi.Api.Configuration;
using PrestaQi.Model;
using PrestaQi.Model.Dto.Input;
using System;
using System.Linq;

namespace PrestaQi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegisterController : CustomController
    {
        IRetrieveService<RequestRegister> _RequestRetrieveService;
        IWriteService<RequestRegister> _AccreditedWriteService;
        IWriteService<EmailNotFound> _EmailNotFoundWriteService;
        IProcessService<RequestRegister> _RequestRegisterProcessService;

        public RegisterController(
            IWriteService<RequestRegister> accreditedWriteService,
            IWriteService<EmailNotFound> emailNotFoundWriteService,
            IRetrieveService<RequestRegister> requestRetrieveService,
            IProcessService<RequestRegister> requestRegisterProcessService
        )
        {
            this._AccreditedWriteService = accreditedWriteService;
            this._EmailNotFoundWriteService = emailNotFoundWriteService;
            this._RequestRetrieveService = requestRetrieveService;
            this._RequestRegisterProcessService = requestRegisterProcessService;
        }

        [HttpGet, Authorize]
        public IActionResult GetResult()
        {
            return Ok(this._RequestRetrieveService.Where(r => r.id > 0 && r.Deleted_At is null));
        }

        [HttpPost, AllowAnonymous]
        public IActionResult Register(RequestRegister register)
        {
            register.created_at = DateTime.Now;
            register.updated_at = DateTime.Now;

            return Ok(this._AccreditedWriteService.Create(register));
        }

        [HttpPost, Route("EmailNotFound"), AllowAnonymous]
        public IActionResult EmailNotFound(EmailNotFound emailNotFound)
        {
            emailNotFound.created_at = DateTime.Now;
            emailNotFound.updated_at = DateTime.Now;

            return Ok(this._EmailNotFoundWriteService.Create(emailNotFound));
        }

        [HttpPost, Route("Delete"), Authorize]
        public IActionResult Delete(DeleteRegister delete)
        {
            var registers = this._RequestRetrieveService.Where(r => delete.ids.Contains(r.id)).ToList();
            foreach(var item in registers)
            {
                item.Deleted_At = DateTime.Now;
            }

            return Ok(this._AccreditedWriteService.Update(registers));
        }

        [HttpPost, Route("SendNotification"), Authorize]
        public IActionResult SendNotification(DeleteRegister send)
        {
            return Ok(this._RequestRegisterProcessService.ExecuteProcess<DeleteRegister, bool>(send));
        }
    }
}

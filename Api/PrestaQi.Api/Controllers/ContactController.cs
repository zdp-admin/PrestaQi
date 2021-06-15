using InsiscoCore.Base.Service;
using Microsoft.AspNetCore.Mvc;
using PrestaQi.Api.Configuration;
using PrestaQi.Service.ProcessServices;
using PrestaQi.Model.Dto.Input;

namespace PrestaQi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController : CustomController
    {
        IProcessService<FormContact> _contactProcessService;

        public ContactController(IProcessService<FormContact> processService)
        {
            this._contactProcessService = processService;
        }

        [HttpPost]
        public IActionResult Post(FormContact form)
        {
            return Ok(this._contactProcessService.ExecuteProcess<FormContact, bool>(form));
        }
    }
}

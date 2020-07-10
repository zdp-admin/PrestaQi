using InsiscoCore.Base.Service;
using Microsoft.AspNetCore.Mvc;
using PrestaQi.Api.Configuration;
using PrestaQi.Model;

namespace PrestaQi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DevicesController : CustomController
    {
        IWriteService<Device> _DeviceWriteService;

        public DevicesController(
            IWriteService<Device> deviceWriteService
            )
        {
            this._DeviceWriteService = deviceWriteService;
        }

        [HttpPost]
        public IActionResult Post(Device device)
        {
            return Ok(this._DeviceWriteService.Create(device));
        }

    }
}
using System.Collections.Generic;
using System.Linq;
using InsiscoCore.Base.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using PrestaQi.Api.Configuration;
using PrestaQi.Model;
using PrestaQi.Model.Dto;
using PrestaQi.Model.Dto.Output;

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
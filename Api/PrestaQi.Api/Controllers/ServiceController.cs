using System.Collections.Generic;
using System.Linq;
using InsiscoCore.Base.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
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

        public ServiceController()
        {

        }

        [HttpPost, Route("stp/status")]
        public IActionResult GetStatusAccredited(StateChange stateChange)
        {
            return Ok(stateChange, "Mensaje");
        }

    }
}
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
    [Route("api/[controller]"), Authorize]
    [ApiController]
    public class ModulesController : CustomController
    {
        IRetrieveService<Module> _AccreditedRetrieveService;

        public ModulesController(
            IRetrieveService<Module> accreditedRetrieveService)
        {
            this._AccreditedRetrieveService = accreditedRetrieveService;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(this._AccreditedRetrieveService.Where(p => p.Enabled == true));
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using InsiscoCore.Base.Service;
using Microsoft.AspNetCore.Mvc;
using PrestaQi.Api.Configuration;

namespace PrestaQi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigurationsController : CustomController
    {
        IWriteService<PrestaQi.Model.Configuration> _ConfigurationWriteService;
        IRetrieveService<PrestaQi.Model.Configuration> _ConfigurationRetrieveService;

        public ConfigurationsController(
            IWriteService<PrestaQi.Model.Configuration> configurationWriteService, 
            IRetrieveService<PrestaQi.Model.Configuration> configurationRetrieveService)
        {
            this._ConfigurationWriteService = configurationWriteService;
            this._ConfigurationRetrieveService = configurationRetrieveService;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(this._ConfigurationRetrieveService.Where(p => true).ToList());
        }

        [HttpPost]
        public IActionResult Post(PrestaQi.Model.Configuration configuration)
        {
            return Ok(this._ConfigurationWriteService.Create(configuration), "Configuration Created!");
        }

        [HttpPut]
        public IActionResult Put(PrestaQi.Model.Configuration configuration)
        {
            return Ok(this._ConfigurationWriteService.Update(configuration), "Configuration Updated!");
        }

    }
}
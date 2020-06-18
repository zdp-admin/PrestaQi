using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JabilCore.Base.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PrestaQi.Model;

namespace PrestaQi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigurationsController : ControllerBase
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
        public List<PrestaQi.Model.Configuration> Get()
        {
            return this._ConfigurationRetrieveService.Where(p => true).ToList();
        }

        [HttpPost]
        public bool Post(PrestaQi.Model.Configuration configuration)
        {
            return this._ConfigurationWriteService.Create(configuration);
        }

        [HttpPut]
        public bool Put(PrestaQi.Model.Configuration configuration)
        {
            return this._ConfigurationWriteService.Update(configuration);
        }

    }
}
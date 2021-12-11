using System;
using System.Collections.Generic;
using System.Linq;
using InsiscoCore.Base.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PrestaQi.Api.Configuration;
using PrestaQi.Model;
using PrestaQi.Model.Dto.Input;

namespace PrestaQi.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : CustomController
    {
        IProcessService<DetailsAdvance> _DetailAdvanceProcess;

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(
            ILogger<WeatherForecastController> logger,
            IProcessService<DetailsAdvance> detailAdvanceProcess
        )
        {
            _logger = logger;
            this._DetailAdvanceProcess = detailAdvanceProcess;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpPost]
        public IActionResult Post(CustomInsertDetails custom)
        {
            return Ok(this._DetailAdvanceProcess.ExecuteProcess<CustomInsertDetails, bool>(custom));
        }
    }
}

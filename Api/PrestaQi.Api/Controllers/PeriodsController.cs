using System.Collections.Generic;
using System.Linq;
using InsiscoCore.Base.Service;
using Microsoft.AspNetCore.Mvc;
using PrestaQi.Api.Configuration;
using PrestaQi.Model;

namespace PrestaQi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PeriodsController : CustomController
    {
        IWriteService<Period> _PeriodWriteService;
        IRetrieveService<Period> _PeriodRetrieveService;

        public PeriodsController(
            IWriteService<Period> periodWriteService, 
            IRetrieveService<Period> periodRetrieveService)
        {
            this._PeriodWriteService = periodWriteService;
            this._PeriodRetrieveService = periodRetrieveService;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(this._PeriodRetrieveService.Where(p => true).ToList());
        }

        [HttpGet, Route("[action]")]
        public IActionResult GetActive()
        {
            return Ok(this._PeriodRetrieveService.Where(p => p.Enabled == true).ToList());
        }

    }
}
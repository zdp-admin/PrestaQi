using System.Linq;
using InsiscoCore.Base.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrestaQi.Api.Configuration;
using PrestaQi.Model;

namespace PrestaQi.Api.Controllers
{
    [Route("api/[controller]"), Authorize]
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

        [HttpGet, Route("GetByType")]
        public IActionResult GetByType([FromQuery] int type)
        {
            return Ok(this._PeriodRetrieveService.Where(p => p.Enabled == true && p.User_Type == type).OrderBy(p => p.id).ToList());
        }

    }
}
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
    public class PeriodsController : ControllerBase
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
        public List<Period> Get()
        {
            return this._PeriodRetrieveService.Where(p => true).ToList();
        }

        [HttpPost]
        public bool Post(Period period)
        {
            return this._PeriodWriteService.Create(period);
        }

        [HttpPut]
        public bool Put(Period period)
        {
            return this._PeriodWriteService.Update(period);
        }

    }
}
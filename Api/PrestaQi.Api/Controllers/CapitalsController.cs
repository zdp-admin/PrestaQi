using System;
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
    public class CapitalsController : CustomController
    {
        IWriteService<Capital> _CapitalWriteService;
        IRetrieveService<Capital> _CapitalRetrieveService;

        public CapitalsController(
            IWriteService<Capital> CapitalWriteService, 
            IRetrieveService<Capital> CapitalRetrieveService)
        {
            this._CapitalWriteService = CapitalWriteService;
            this._CapitalRetrieveService = CapitalRetrieveService;
        }

        [HttpGet, Route("[action]/{id}")]
        public IActionResult GetByInvestor(int id, 
            [FromQuery(Name = "startDate")] DateTime startDate,
            [FromQuery(Name = "endDate")] DateTime endDate)
        {
            return Ok(this._CapitalRetrieveService.Where(p =>
            {
                return p.investor_id == id && p.created_at.Date >= startDate && p.created_at.Date <= endDate;
            }).ToList());
        }

        [HttpPost]
        public IActionResult Post(Capital userCapital)
        {
            return Ok(this._CapitalWriteService.Create(userCapital), "Record created!");
        }

    }
}
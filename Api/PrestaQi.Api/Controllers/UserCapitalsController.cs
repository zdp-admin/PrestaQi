using System;
using System.Collections.Generic;
using System.Linq;
using JabilCore.Base.Service;
using Microsoft.AspNetCore.Mvc;
using PrestaQi.Api.Configuration;
using PrestaQi.Model;

namespace PrestaQi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserCapitalsController : CustomController
    {
        IWriteService<UserCapital> _UserCapitalWriteService;
        IRetrieveService<UserCapital> _UserCapitalRetrieveService;

        public UserCapitalsController(
            IWriteService<UserCapital> userCapitalWriteService, 
            IRetrieveService<UserCapital> userCapitalRetrieveService)
        {
            this._UserCapitalWriteService = userCapitalWriteService;
            this._UserCapitalRetrieveService = userCapitalRetrieveService;
        }

        [HttpGet, Route("[action]/{id}")]
        public IActionResult GetByUser(int id, 
            [FromQuery(Name = "startDate")] DateTime startDate,
            [FromQuery(Name = "endDate")] DateTime endDate)
        {
            return Ok(this._UserCapitalRetrieveService.Where(p =>
            {
                return p.User_Capital_Id == id && p.created_at.Date >= startDate && p.created_at.Date <= endDate;
            }).ToList());
        }

        [HttpPost]
        public IActionResult Post(UserCapital userCapital)
        {
            return Ok(this._UserCapitalWriteService.Create(userCapital), "Record created!");
        }

    }
}
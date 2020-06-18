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
    public class UserCapitalsController : ControllerBase
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
        public List<UserCapital> GetByUser(int id, 
            [FromQuery(Name = "startDate")] DateTime startDate,
            [FromQuery(Name = "endDate")] DateTime endDate)
        {
            return this._UserCapitalRetrieveService.Where(p =>
            {
                return p.User_Capital_Id == id && p.created_at.Date >= startDate && p.created_at.Date <= endDate;
            }).ToList();
        }

        [HttpPost]
        public bool Post(UserCapital userCapital)
        {
            return this._UserCapitalWriteService.Create(userCapital);
        }

    }
}
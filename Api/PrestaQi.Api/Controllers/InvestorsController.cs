using System.Collections.Generic;
using System.Linq;
using InsiscoCore.Base.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using PrestaQi.Api.Configuration;
using PrestaQi.Model;
using PrestaQi.Model.Dto;

namespace PrestaQi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvestorsController : CustomController
    {
        IWriteService<Investor> _InvestorWriteService;
        IRetrieveService<Investor> _InvestorRetrieveService;

        public InvestorsController(
            IWriteService<Investor> investorWriteService, 
            IRetrieveService<Investor> investorRetrieveService)
        {
            this._InvestorWriteService = investorWriteService;
            this._InvestorRetrieveService = investorRetrieveService;
        }

        [HttpGet, Route("[action]")]
        public IActionResult GetList([FromQuery] bool onlyActive)
        {
            return Ok(
                onlyActive == true ? this._InvestorRetrieveService.Where(p => p.Enabled == true) :
                this._InvestorRetrieveService.Where(p => true)
                );
        }

        [HttpPost]
        public IActionResult Post(Investor investor)
        {
            return Ok(this._InvestorWriteService.Create(investor), "Investor created!");
        }

        [HttpPut]
        public IActionResult Put(Investor investor)
        {
            return Ok(this._InvestorWriteService.Update(investor), "Investor updated!");
        }

        [HttpPost, Route("[action]")]
        public IActionResult CreateInvestors(List<Investor> investors)
        {
            return Ok(this._InvestorWriteService.Create(investors));
        }

    }
}
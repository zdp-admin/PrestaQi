using System.Collections.Generic;
using System.Linq;
using InsiscoCore.Base.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using PrestaQi.Api.Configuration;
using PrestaQi.Model;
using PrestaQi.Model.Dto;

namespace PrestaQi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RepaymentsController : CustomController
    {
        IRetrieveService<Repayment> _RepaymentRetrieveService;

        public RepaymentsController(
            IRetrieveService<Repayment> repaymentRetrieveService)
        {
            this._RepaymentRetrieveService = repaymentRetrieveService;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(this._RepaymentRetrieveService.Where(p => true).OrderBy(p => p.Description));
        }


    }
}
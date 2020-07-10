using System.Linq;
using InsiscoCore.Base.Service;
using Microsoft.AspNetCore.Mvc;
using PrestaQi.Api.Configuration;
using PrestaQi.Model;

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
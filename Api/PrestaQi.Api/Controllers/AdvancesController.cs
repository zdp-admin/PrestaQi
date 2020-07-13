using InsiscoCore.Base.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PrestaQi.Api.Configuration;
using PrestaQi.Model;
using PrestaQi.Model.Dto.Input;

namespace PrestaQi.Api.Controllers
{
    [Route("api/[controller]"), Authorize]
    [ApiController]
    public class AdvancesController : CustomController
    {
        IWriteService<Advance> _AdvanceWriteService;
        IRetrieveService<Advance> _AdvanceRetrieveService;
        IProcessService<Advance> _AdvanceProcessService;
        IProcessService<PaidAdvance> _PaidAdvanceProcessService;
        public IConfiguration Configuration { get; }

        public AdvancesController(
            IWriteService<Advance> advanceWriteService, 
            IRetrieveService<Advance> advanceRetrieveService,
            IProcessService<Advance> advanceProcessService,
            IProcessService<PaidAdvance> paidAdvanceProcessService,
            IConfiguration configuration
            )
        {
            this._AdvanceWriteService = advanceWriteService;
            this._AdvanceRetrieveService = advanceRetrieveService;
            this._AdvanceProcessService = advanceProcessService;
            this._PaidAdvanceProcessService = paidAdvanceProcessService;
            Configuration = configuration;
        }

        [HttpPost, Route("CalculateAdvance")]
        public IActionResult CalculateAdvance(CalculateAmount calculateAmount)
        {
            calculateAmount.Accredited_Id = int.Parse(HttpContext.User.FindFirst("UserId").Value);
            return Ok(this._AdvanceProcessService.ExecuteProcess<CalculateAmount, Advance>(calculateAmount));
        }

        [HttpPost]
        public IActionResult Post(CalculateAmount calculateAmount)
        {
            calculateAmount.Accredited_Id = int.Parse(HttpContext.User.FindFirst("UserId").Value);
            return Ok(this._AdvanceWriteService.Create<CalculateAmount, bool>(calculateAmount), "Generator Advance");
        }

        [HttpGet, Route("GetByAccredited/{id}")]
        public IActionResult GetByAccredited(int id)
        {
            return Ok(this._AdvanceRetrieveService.Where(p => p.Accredited_Id == id));
        }

        [HttpPost, Route("SetPaidAdvance")]
        public IActionResult SetPaidAdvance(SetPayAdvance setPayAdvance)
        {
            return Ok(this._PaidAdvanceProcessService.ExecuteProcess<SetPayAdvance, bool>(setPayAdvance));
        }
    }
}   
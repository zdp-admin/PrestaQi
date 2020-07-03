using System.Collections.Generic;
using System.Linq;
using InsiscoCore.Base.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.CodeAnalysis.Operations;
using PrestaQi.Api.Configuration;
using PrestaQi.Model;
using PrestaQi.Model.Dto;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Model.Dto.Output;

namespace PrestaQi.Api.Controllers
{
    [Route("api/[controller]"), Authorize]
    [ApiController]
    public class DashboardController : CustomController
    {
        IProcessService<Advance> _AdvanceProcessService;
        IProcessService<Liquidity> _LiquidityProcessService;
        IProcessService<Capital> _CapitalProcessService;

        public DashboardController(
            IProcessService<Advance> advanceProcessService,
            IProcessService<Liquidity> liquidityProcessService,
            IProcessService<Capital> capitalProcessService
            )
        {
            this._AdvanceProcessService = advanceProcessService;
            this._LiquidityProcessService = liquidityProcessService;
            this._CapitalProcessService = capitalProcessService;
    }

        [HttpPost, Route("GetCommissionAndInterest")]
        public IActionResult GetCommissionAndInterest(GetCommisionAndIntereset getCommisionAndIntereset)
        {
            return Ok(this._AdvanceProcessService.ExecuteProcess<GetCommisionAndIntereset, CommisionAndInterestMaster>(getCommisionAndIntereset));
        }

        [HttpPost, Route("GetCredits")]
        public IActionResult GetCredits(GetCredits getCredits)
        {
            return Ok(this._AdvanceProcessService.ExecuteProcess<GetCredits, CreditAverage>(getCredits));
        }

        [HttpPost, Route("GetLiquidity")]
        public IActionResult GetLiquidity(GetLiquidity liquidity)
        {
            return Ok(this._LiquidityProcessService.ExecuteProcess<GetLiquidity, Liquidity>(liquidity));
        }

        [HttpPost, Route("GetInvestment")]
        public IActionResult GetInvestment(GetInvestment getInvestment)
        {
            return Ok(this._CapitalProcessService.ExecuteProcess<GetInvestment, InvestmentDashboard>(getInvestment));
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using InsiscoCore.Base.Service;
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
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : CustomController
    {
        IProcessService<Advance> _AdvanceProcessService;

        public DashboardController(
            IProcessService<Advance> advanceProcessService
            )
        {
            _AdvanceProcessService = advanceProcessService;
        }

        [HttpPost, Route("GetCommissionAndInterest")]
        public IActionResult GetCommissionAndInterest(GetCommisionAndIntereset getCommisionAndIntereset)
        {
            return Ok(this._AdvanceProcessService.ExecuteProcess<GetCommisionAndIntereset, CommisionAndInterestMaster>(getCommisionAndIntereset));
        }

    }
}
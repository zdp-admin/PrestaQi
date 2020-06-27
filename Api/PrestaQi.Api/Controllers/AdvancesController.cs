using System.Collections.Generic;
using System.Linq;
using InsiscoCore.Base.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using PrestaQi.Api.Configuration;
using PrestaQi.Model;
using PrestaQi.Model.Dto;
using PrestaQi.Model.Dto.Input;

namespace PrestaQi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdvancesController : CustomController
    {
        IWriteService<Advance> _AdvanceWriteService;
        IRetrieveService<Advance> _AdvanceRetrieveService;
        IProcessService<Advance> _AdvanceProcessService;

        public AdvancesController(
            IWriteService<Advance> advanceWriteService, 
            IRetrieveService<Advance> advanceRetrieveService,
            IProcessService<Advance> advanceProcessService)
        {
            this._AdvanceWriteService = advanceWriteService;
            this._AdvanceRetrieveService = advanceRetrieveService;
            this._AdvanceProcessService = advanceProcessService;
        }

        [HttpPost, Route("CalculateAdvance")]
        public IActionResult CalculateAdvance(CalculateAmount calculateAmount)
        {
            return Ok(this._AdvanceProcessService.ExecuteProcess<CalculateAmount, Advance>(calculateAmount));
        }

        [HttpPost]
        public IActionResult Post(Advance advance)
        {
            return Ok(this._AdvanceWriteService.Create(advance), "Generator Advance");
        }

        [HttpGet, Route("GetByAccredited/id")]
        public IActionResult GetByAccreedited(int id)
        {
            return Ok(this._AdvanceRetrieveService.Where(p => p.Accredited_Id == id));
        }
    }
}
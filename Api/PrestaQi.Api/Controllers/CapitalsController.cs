using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using InsiscoCore.Base.Service;
using Microsoft.AspNetCore.Mvc;
using PrestaQi.Api.Configuration;
using PrestaQi.Model;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Model.Dto.Output;

namespace PrestaQi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CapitalsController : CustomController
    {
        IWriteService<Capital> _CapitalWriteService;
        IRetrieveService<Capital> _CapitalRetrieveService;
        IProcessService<Capital> _CapitalProcessService;

        public CapitalsController(
            IWriteService<Capital> capitalWriteService, 
            IRetrieveService<Capital> capitalRetrieveService,
            IProcessService<Capital> capitalProcessService)
        {
            this._CapitalWriteService = capitalWriteService;
            this._CapitalRetrieveService = capitalRetrieveService;
            this._CapitalProcessService = capitalProcessService;
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

        [HttpGet, Route("GetMyInvestments/{id}")]
        public IActionResult GetMyInvestments(int id)
        {
            return Ok(this._CapitalProcessService.ExecuteProcess<int, List<MyInvestment>>(id));
        }

        [HttpPost, Route("[action]")]
        public IActionResult GetAnchorControl(AnchorByFilter anchorByFilter)
        {
            return Ok(this._CapitalProcessService.ExecuteProcess<AnchorByFilter, List<AnchorControl>>(anchorByFilter));
        }

        [HttpPost, Route("ChangeStatus")]
        public IActionResult ChangeStatus(CapitalChangeStatus capitalChangeStatus)
        {
            return Ok(this._CapitalWriteService.Update<CapitalChangeStatus, bool>(capitalChangeStatus));
        }
    }
}
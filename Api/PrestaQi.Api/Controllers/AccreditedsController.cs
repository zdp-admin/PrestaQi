using System.Collections.Generic;
using System.Linq;
using InsiscoCore.Base.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using PrestaQi.Api.Configuration;
using PrestaQi.Model;
using PrestaQi.Model.Dto;
using PrestaQi.Model.Dto.Output;

namespace PrestaQi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccreditedsController : CustomController
    {
        IWriteService<Accredited> _AccreditedWriteService;
        IRetrieveService<Accredited> _AccreditedRetrieveService;
        IProcessService<Accredited> _AccreditedProcessService;

        public AccreditedsController(
            IWriteService<Accredited> accreditedWriteService, 
            IRetrieveService<Accredited> accreditedRetrieveService,
            IProcessService<Accredited> accreditedProcessService)
        {
            this._AccreditedWriteService = accreditedWriteService;
            this._AccreditedRetrieveService = accreditedRetrieveService;
            this._AccreditedProcessService = accreditedProcessService;
        }

        [HttpGet, Route("[action]")]
        public IActionResult GetList([FromQuery] bool onlyActive)
        {
            return Ok(
                onlyActive == true ? this._AccreditedRetrieveService.Where(p => p.Enabled == true) :
                this._AccreditedRetrieveService.Where(p => true)
                );
        }

        [HttpPost]
        public IActionResult Post(Accredited accredited)
        {
            return Ok(this._AccreditedWriteService.Create(accredited), "Accredited created!");
        }

        [HttpPut]
        public IActionResult Put(Accredited accredited)
        {
            return Ok(this._AccreditedWriteService.Update(accredited), "Accredited updated!");
        }

        [HttpPost, Route("[action]")]
        public IActionResult CreateAccrediteds(List<Accredited> accrediteds)
        {
            return Ok(this._AccreditedWriteService.Create(accrediteds));
        }

        [HttpGet, Route("ListAdvancesReceivable")]
        public IActionResult ListAdvancesReceivable()
        {
            return Ok(this._AccreditedProcessService.ExecuteProcess<bool, List<AdvanceReceivable>>(true));
        }
    }
}
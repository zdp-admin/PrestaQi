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
    [Route("api/[controller]"), Authorize]
    [ApiController]
    public class InstitutionsController : CustomController
    {
        IWriteService<Institution> _InstitutionWriteService;
        IRetrieveService<Institution> _InstitutionRetrieveService;

        public InstitutionsController(
            IWriteService<Institution> institutionWriteService, 
            IRetrieveService<Institution> institutionRetrieveService)
        {
            this._InstitutionWriteService = institutionWriteService;
            this._InstitutionRetrieveService = institutionRetrieveService;
        }

        [HttpGet, Route("[action]")]
        public IActionResult GetList([FromQuery] bool onlyActive)
        {
            return Ok(
                onlyActive == true ? this._InstitutionRetrieveService.Where(p => p.Enabled == true).OrderBy(p => p.Description) :
                this._InstitutionRetrieveService.Where(p => true).OrderBy(p => p.Description)
                );
        }

        [HttpPost]
        public IActionResult Post(Institution institution)
        {
            return Ok(this._InstitutionWriteService.Create(institution), "Institution created!");
        }

        [HttpPut]
        public IActionResult Put(Institution institution)
        {
            return Ok(this._InstitutionWriteService.Update(institution), "Institution updated!");
        }

    }
}
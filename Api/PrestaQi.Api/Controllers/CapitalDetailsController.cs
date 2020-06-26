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
    public class CapitalDetailsController : CustomController
    {
        IWriteService<CapitalDetail> _CapitalDetailWriteService;

        public CapitalDetailsController(
            IWriteService<CapitalDetail> capitalDetailWriteService)
        {
            this._CapitalDetailWriteService = capitalDetailWriteService;
        }

        [HttpPut, Route("SetPaymentPeriod")]
        public IActionResult SetPaymentPeriod(CapitalDetail capitalDetail)
        {
            return Ok(this._CapitalDetailWriteService.Update(capitalDetail));
        }
    }
}
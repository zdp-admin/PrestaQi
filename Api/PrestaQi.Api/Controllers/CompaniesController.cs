﻿using System.Collections.Generic;
using System.Linq;
using InsiscoCore.Base.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using PrestaQi.Api.Configuration;
using PrestaQi.Model;
using PrestaQi.Model.Dto;

namespace PrestaQi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompaniesController : CustomController
    {
        IWriteService<Company> _CompanyWriteService;
        IRetrieveService<Company> _CompanyRetrieveService;

        public CompaniesController(
            IWriteService<Company> companyWriteService, 
            IRetrieveService<Company> companyRetrieveService)
        {
            this._CompanyWriteService = companyWriteService;
            this._CompanyRetrieveService = companyRetrieveService;
        }

        [HttpGet, Route("[action]")]
        public IActionResult GetList([FromQuery] bool onlyActive)
        {
            return Ok(
                onlyActive == true ? this._CompanyRetrieveService.Where(p => p.Enabled == true) :
                this._CompanyRetrieveService.Where(p => true)
                );
        }

        [HttpPost]
        public IActionResult Post(Company company)
        {
            return Ok(this._CompanyWriteService.Create(company), "Company created!");
        }

        [HttpPut]
        public IActionResult Put(Company company)
        {
            return Ok(this._CompanyWriteService.Update(company), "Company updated!");
        }

    }
}
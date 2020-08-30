using System;
using System.Collections.Generic;
using System.IO;
using InsiscoCore.Base.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrestaQi.Api.Configuration;
using PrestaQi.Model;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Model.Dto.Output;

namespace PrestaQi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvestorsController : CustomController
    {
        IWriteService<Investor> _InvestorWriteService;
        IRetrieveService<Investor> _InvestorRetrieveService;
        IProcessService<ExportFile> _ExportProcessService;

        public InvestorsController(
            IWriteService<Investor> investorWriteService, 
            IRetrieveService<Investor> investorRetrieveService,
            IProcessService<ExportFile> exportProcessService)
        {
            this._InvestorWriteService = investorWriteService;
            this._InvestorRetrieveService = investorRetrieveService;
            this._ExportProcessService = exportProcessService;
        }

        [HttpGet, Route("[action]")]
        public IActionResult GetList([FromQuery] bool onlyActive)
        {
            return Ok(
                onlyActive == true ? this._InvestorRetrieveService.Where(p => p.Enabled == true) :
                this._InvestorRetrieveService.Where(p => true)
                );
        }

        [HttpPost, Route("GetInvestors")]
        public IActionResult GetInvestors(InvestorByFilter investorByDate)
        {
            var result = this._InvestorRetrieveService.RetrieveResult<InvestorByFilter, InvestorPagination>(investorByDate);
            return Ok(result);
        }

        [HttpGet, Route("GetFile")]
        public IActionResult GetFile([FromQuery] int type)
        {
            var result = this._InvestorRetrieveService.RetrieveResult<InvestorByFilter, InvestorPagination>(new InvestorByFilter()
            {
                Type  = type
            });

            var msFile = this._ExportProcessService.ExecuteProcess<ExportInvestor, MemoryStream>(new ExportInvestor()
            {
                Type = type,
                InvestorDatas = result.InvestorDatas
            });

            return this.File(
                fileContents: msFile.ToArray(),
                contentType: type == 1 ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" :
                    "application/pdf",
                fileDownloadName: type == 1 ? "Investors_" + DateTime.Now.ToString("yyyyMMdd") + ".xlsx" :
                    "Investors_" + DateTime.Now.ToString("yyyyMMdd") + ".pdf"
            );
        }

        [HttpPost]
        public IActionResult Post(Investor investor)
        {
            return Ok(this._InvestorWriteService.Create(investor), "Investor created!");
        }

        [HttpPut]
        public IActionResult Put(Investor investor)
        {
            return Ok(this._InvestorWriteService.Update(investor), "Investor updated!");
        }

        [HttpPost, Route("CreateInvestors")]
        public IActionResult CreateInvestors(List<Investor> investors)
        {
            return Ok(this._InvestorWriteService.Create(investors));
        }

    }
}
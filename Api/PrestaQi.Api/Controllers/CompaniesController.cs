using InsiscoCore.Base.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrestaQi.Api.Configuration;
using PrestaQi.Model;

namespace PrestaQi.Api.Controllers
{
    [Route("api/[controller]"), Authorize]
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

        [HttpGet, Route("[action]"), AllowAnonymous]
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
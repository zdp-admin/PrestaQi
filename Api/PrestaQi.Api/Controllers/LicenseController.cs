using InsiscoCore.Base.Service;
using Microsoft.AspNetCore.Mvc;
using PrestaQi.Api.Configuration;
using PrestaQi.Model;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Model.Dto.Output;
using PrestaQi.Model.Enum;
using System.Collections.Generic;
using System.Linq;

namespace PrestaQi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LicenseController : CustomController
    {
        IWriteService<License> _licenseWriteService;
        IRetrieveService<License> _licenseRetrieveService;
        IRetrieveService<Accredited> _AccreditedRetrieveService;
        IRetrieveService<LicensePriceRange> _pricesRetrieveService;
        IProcessService<License> _licenseProcessService;

        public LicenseController(
            IWriteService<License> licenseWriteService,
            IRetrieveService<License> licenseRetrieveService,
            IRetrieveService<Accredited> accreditedRetrieveService,
            IRetrieveService<LicensePriceRange> pricesRetrieveService,
            IProcessService<License> licenseProcessService
            )
        {
            this._licenseWriteService = licenseWriteService;
            this._licenseRetrieveService = licenseRetrieveService;
            this._AccreditedRetrieveService = accreditedRetrieveService;
            this._pricesRetrieveService = pricesRetrieveService;
            this._licenseProcessService = licenseProcessService;
    }

        [HttpGet]
        public IActionResult Get([FromQuery] LicenseByFilter licenseByFilter)
        {
            ResponseWithPagination result = this._licenseRetrieveService.RetrieveResult<LicenseByFilter, ResponseWithPagination>(licenseByFilter);

            int idUser = int.Parse(HttpContext.User.FindFirst("UserId").Value);
            int type = int.Parse(HttpContext.User.FindFirst("Type").Value);


            if (type == (int) PrestaQiEnum.UserType.License)
            {
                result.data = (result.data as ICollection<License>).Where(license => license.id == idUser).ToList();
            }


            return Ok(result);
        }

        [HttpGet]
        [Route("All")]
        public IActionResult GetAll()
        {
            var list = this._licenseRetrieveService.Where(license => license.Enabled).ToList();
            
            foreach (var license in list)
            {
                license.Prices = this._pricesRetrieveService.Where(price => price.LicenseId == license.id).ToList();
            }

            return Ok(list);
        }

        [HttpGet]
        [Route("Accredited")]
        public IActionResult GetAccredited([FromQuery] LicenseByFilter licenseByFilter)
        {
            var result = this._AccreditedRetrieveService.RetrieveResult<LicenseByFilter, AccreditedPagination>(licenseByFilter);
            return Ok(result);
        }

        [HttpGet]
        [Route("FundingControl")]
        public IActionResult GetFundingControl([FromQuery] LicenseBalance filter)
        {
            return Ok(this._licenseProcessService.ExecuteProcess<LicenseBalance, AccountBalanceOutput>(filter));
        }

        [HttpPost]
        public IActionResult Post(License license)
        {
            return Ok(this._licenseWriteService.Create(license), "Licencia creada");
        }

        [HttpPut]
        public IActionResult Put(License license)
        {
            return Ok(this._licenseWriteService.Update(license), "Licencia actualizada");
        }
    }
}

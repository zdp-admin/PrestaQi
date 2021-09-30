using InsiscoCore.Base.Service;
using Microsoft.AspNetCore.Mvc;
using PrestaQi.Api.Configuration;
using PrestaQi.Model;
using System.Linq;

namespace PrestaQi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LicensePriceRangeController : CustomController
    {
        IRetrieveService<LicensePriceRange> _retrieveService;

        public LicensePriceRangeController(
            IRetrieveService<LicensePriceRange> retrieveService
        )
        {
            this._retrieveService = retrieveService;
        }

        [HttpGet]
        public IActionResult Get([FromQuery] int? LicenseId)
        {
            return Ok(this._retrieveService.Where((price) => price.LicenseId == LicenseId || price.LicenseId == null).ToList());
        }
    }
}

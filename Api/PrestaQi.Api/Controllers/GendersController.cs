using System.Linq;
using InsiscoCore.Base.Service;
using Microsoft.AspNetCore.Mvc;
using PrestaQi.Api.Configuration;
using PrestaQi.Model;

namespace PrestaQi.Api.Controllers
{
    [Route("api/[controller]")/*, Authorize*/]
    [ApiController]
    public class GendersController : CustomController
    {
        IRetrieveService<Gender> _GenderRetrieveService;

        public GendersController(
            IRetrieveService<Gender> genderRetrieveService)
        {
            this._GenderRetrieveService = genderRetrieveService;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(this._GenderRetrieveService.Where(p => true));
        }

        [HttpGet, Route("[action]")]
        public IActionResult GetActive()
        {
            return Ok(this._GenderRetrieveService.Where(p => p.Enabled == true).ToList());
        }

    }
}
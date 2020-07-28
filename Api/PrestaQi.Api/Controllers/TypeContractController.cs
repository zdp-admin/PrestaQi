using PrestaQi.Api.Configuration;
using PrestaQi.Model;
using InsiscoCore.Base.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PrestaQi.Api.Controllers
{
    [Route("api/[controller]"), Authorize]
    public class TypeContractController : CustomController
    {
        IRetrieveService<TypeContract> _TypeContractService;

        public TypeContractController(IRetrieveService<TypeContract> typeContractService)
        {
            this._TypeContractService = typeContractService;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(this._TypeContractService.Where(p => true));
        }
    }
}

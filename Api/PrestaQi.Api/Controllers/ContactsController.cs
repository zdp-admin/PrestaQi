using System.Linq;
using JabilCore.Base.Service;
using Microsoft.AspNetCore.Mvc;
using PrestaQi.Api.Configuration;
using PrestaQi.Model;

namespace PrestaQi.Api.Controllers
{
    [Route("api/[controller]")/*, Authorize*/]
    [ApiController]
    public class ContactsController : CustomController
    {
        IWriteService<Contact> _ContactWriteService;
        IRetrieveService<Contact> _ContactRetrieveService;

        public ContactsController(
            IWriteService<Contact> contactWriteService, 
            IRetrieveService<Contact> contactRetrieveService)
        {
            this._ContactWriteService = contactWriteService;
            this._ContactRetrieveService = contactRetrieveService;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(this._ContactRetrieveService.Where(p => true).ToList());
        }

        [HttpPost]
        public IActionResult Post(Contact period)
        {
            return Ok(this._ContactWriteService.Create(period), "Contact created!");
        }

        [HttpPut]
        public IActionResult Put(Contact period)
        {
            return Ok(this._ContactWriteService.Update(period), "Contact updated!");
        }

    }
}
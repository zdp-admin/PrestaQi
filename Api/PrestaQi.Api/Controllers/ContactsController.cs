using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JabilCore.Base.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PrestaQi.Model;

namespace PrestaQi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactsController : ControllerBase
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
        public List<Contact> Get()
        {
            return this._ContactRetrieveService.Where(p => true).ToList();
        }

        [HttpPost]
        public bool Post(Contact period)
        {
            return this._ContactWriteService.Create(period);
        }

        [HttpPut]
        public bool Put(Contact period)
        {
            return this._ContactWriteService.Update(period);
        }

    }
}
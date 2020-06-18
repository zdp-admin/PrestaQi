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
    public class UserTypesController : ControllerBase
    {
        IWriteService<UserType> _UserTypeWriteService;
        IRetrieveService<UserType> _UserTypeRetrieveService;

        public UserTypesController(
            IWriteService<UserType> userTypeWriteService,
            IRetrieveService<UserType> userTypeRetrieveService)
        {
            this._UserTypeWriteService = userTypeWriteService;
            this._UserTypeRetrieveService = userTypeRetrieveService;
        }

        [HttpGet]
        public List<UserType> Get()
        {
            return this._UserTypeRetrieveService.Where(p => true).ToList();
        }

        [HttpPost]
        public bool Post(UserType userType)
        {
            return this._UserTypeWriteService.Create(userType);
        }

        [HttpPut]
        public bool Put(UserType userType)
        {
            return this._UserTypeWriteService.Update(userType);
        }

    }
}
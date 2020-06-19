using System.Collections.Generic;
using System.Linq;
using JabilCore.Base.Service;
using Microsoft.AspNetCore.Mvc;
using PrestaQi.Api.Configuration;
using PrestaQi.Model;

namespace PrestaQi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserTypesController : CustomController
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
        public IActionResult Get()
        {
            return Ok(this._UserTypeRetrieveService.Where(p => true).ToList());
        }

        [HttpPost]
        public IActionResult Post(UserType userType)
        {
            return Ok(this._UserTypeWriteService.Create(userType), "User Type created!");
        }

        [HttpPut]
        public IActionResult Put(UserType userType)
        {
            return Ok(this._UserTypeWriteService.Update(userType), "User Type updated");
        }

    }
}
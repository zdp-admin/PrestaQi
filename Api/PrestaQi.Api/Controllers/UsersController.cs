using System.Collections.Generic;
using System.Linq;
using InsiscoCore.Base.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using PrestaQi.Api.Configuration;
using PrestaQi.Model;
using PrestaQi.Model.Dto;

namespace PrestaQi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : CustomController
    {
        IWriteService<User> _UserWriteService;
        IRetrieveService<User> _UserRetrieveService;

        public UsersController(
            IWriteService<User> userWriteService, 
            IRetrieveService<User> userRetrieveService)
        {
            this._UserWriteService = userWriteService;
            this._UserRetrieveService = userRetrieveService;
        }

        [HttpGet, Route("[action]")]
        public IActionResult GetList([FromQuery] bool onlyActive)
        {
            return Ok(
                onlyActive == true ? this._UserRetrieveService.Where(p => p.Enabled == true) :
                this._UserRetrieveService.Where(p => true)
                );
        }

        [HttpPost]
        public IActionResult Post(User user)
        {
            return Ok(this._UserWriteService.Create(user), "User created!");
        }

        [HttpPut]
        public IActionResult Put(User user)
        {
            return Ok(this._UserWriteService.Update(user), "User updated!");
        }

        [HttpPut, Route("[action]")]
        public IActionResult ChangePassword(ChangePassword changePassword)
        {
            return Ok(this._UserWriteService.Update<ChangePassword, bool>(changePassword), "Password changed!");
        }

        [HttpPost, Route("[action]")]
        public IActionResult CreateUsers(List<User> users)
        {
            return Ok(this._UserWriteService.Create(users));
        }

    }
}
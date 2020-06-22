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

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(this._UserRetrieveService.Where(p => true).ToList());
        }

        [HttpGet, Route("[action]/{id}")]
        public IActionResult GetByType(int id, [FromQuery] bool onlyActive)
        {
            return Ok(
                onlyActive == true ? this._UserRetrieveService.Where(p => p.User_Type_Id == id && p.Enabled == true) :
                this._UserRetrieveService.Where(p => p.User_Type_Id == id)
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

    }
}
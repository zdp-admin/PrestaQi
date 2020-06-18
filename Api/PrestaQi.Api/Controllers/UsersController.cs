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
    public class UsersController : ControllerBase
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
        public List<User> Get()
        {
            return this._UserRetrieveService.Where(p => true).ToList();
        }

        [HttpPost]
        public bool Post(User user)
        {
            return this._UserWriteService.Create(user);
        }

        [HttpPut]
        public bool Put(User user)
        {
            return this._UserWriteService.Update(user);
        }

    }
}
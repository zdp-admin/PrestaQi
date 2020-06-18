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
    public class UserPropertiesController : ControllerBase
    {
        IWriteService<UserProperty> _UserPropertyWriteService;
        IRetrieveService<UserProperty> _UserPropertyRetrieveService;

        public UserPropertiesController(
            IWriteService<UserProperty> userPropertyWriteService, 
            IRetrieveService<UserProperty> userPropertyRetrieveService)
        {
            this._UserPropertyWriteService = userPropertyWriteService;
            this._UserPropertyRetrieveService = userPropertyRetrieveService;
        }

        [HttpGet, Route("[action]/{id}")]
        public List<UserProperty> GetByUser(int id)
        {
            return this._UserPropertyRetrieveService.Where(p => p.User_Id == id).ToList();
        }

        [HttpPost]
        public bool Post(UserProperty userProperty)
        {
            return this._UserPropertyWriteService.Create(userProperty);
        }

        [HttpPut]
        public bool Put(UserProperty userProperty)
        {
            return this._UserPropertyWriteService.Update(userProperty);
        }

        [HttpPost, Route("[action]")]
        public bool CreateList(List<UserProperty> userProperties)
        {
            return this._UserPropertyWriteService.Create(userProperties);
        }

        [HttpPut, Route("[action]")]
        public bool UpdateList(List<UserProperty> userProperties)
        {
            return this._UserPropertyWriteService.Update(userProperties);
        }

    }
}
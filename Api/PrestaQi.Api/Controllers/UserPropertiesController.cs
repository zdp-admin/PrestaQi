using System.Collections.Generic;
using System.Linq;
using InsiscoCore.Base.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using PrestaQi.Api.Configuration;
using PrestaQi.Model;

namespace PrestaQi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserPropertiesController : CustomController
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
        public IActionResult GetByUser(int id)
        {
           return Ok(this._UserPropertyRetrieveService.Where(p => p.User_Id == id).ToList());
        }

        [HttpPost]
        public IActionResult Post(UserProperty userProperty)
        {
            return Ok(this._UserPropertyWriteService.Create(userProperty), "Property created!");
        }

        [HttpPut]
        public IActionResult Put(UserProperty userProperty)
        {
            return Ok(this._UserPropertyWriteService.Update(userProperty), "Property updated");
        }

        [HttpPost, Route("[action]")]
        public IActionResult CreateList(List<UserProperty> userProperties)
        {
            return Ok(this._UserPropertyWriteService.Create(userProperties), "Properties created!");
        }

        [HttpPut, Route("[action]")]
        public IActionResult UpdateList(List<UserProperty> userProperties)
        {
            return Ok(this._UserPropertyWriteService.Update(userProperties), "Properties updated!");
        }

    }
}
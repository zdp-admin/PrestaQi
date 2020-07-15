using InsiscoCore.Base.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoreLinq;
using PrestaQi.Api.Configuration;
using PrestaQi.Model;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Model.Dto.Output;
using PrestaQi.Model.Enum;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;

namespace PrestaQi.Api.Controllers
{
    [Route("api/[controller]"), Authorize]
    [ApiController]
    public class UsersController : CustomController
    {
        IWriteService<User> _UserWriteService;
        IRetrieveService<User> _UserRetrieveService;
        IProcessService<DocumentUser> _DocumentUserWriteService;

        public UsersController(
            IWriteService<User> userWriteService, 
            IRetrieveService<User> userRetrieveService,
            IProcessService<DocumentUser> documentUserWriteService
            )
        {
            this._UserWriteService = userWriteService;
            this._UserRetrieveService = userRetrieveService;
            this._DocumentUserWriteService = documentUserWriteService;
        }

        [HttpGet, Route("[action]")]
        public IActionResult GetList([FromQuery] bool onlyActive)
        {
            return Ok(
                onlyActive == true ? this._UserRetrieveService.Where(p => p.Deleted_At == null &&
                p.Enabled == true &&
                p.id != Convert.ToInt32(HttpContext.User.FindFirst("UserId").Value)).OrderBy(p => p.First_Name) :
                this._UserRetrieveService.Where(p => p.Deleted_At == null &&
                p.id != Convert.ToInt32(HttpContext.User.FindFirst("UserId").Value)).OrderBy(p => p.First_Name)
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

        [HttpGet, Route("GetUser")]
        public IActionResult GetUser()
        { 
            int userId = Convert.ToInt32(HttpContext.User.FindFirst("UserId").Value);
            int type = Convert.ToInt32(HttpContext.User.FindFirst("Type").Value);

            var data = this._UserRetrieveService.RetrieveResult<UserByType, UserLogin>(new UserByType()
            {
                UserType = ((PrestaQiEnum.UserType)type),
                User_Id = userId
            });

            return Ok(new { User = data.User, Type = type, TypeName = ((PrestaQiEnum.UserType)type).ToString() });
        }

        [HttpGet, Route("GetContract"), AllowAnonymous]
        public IActionResult GetContract([FromQuery] string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token);
            var tokenS = handler.ReadToken(token) as JwtSecurityToken;

            var userId = Convert.ToInt32(tokenS.Claims.FirstOrDefault(p => p.Type == "UserId").Value);
            var type = Convert.ToInt32(tokenS.Claims.FirstOrDefault(p => p.Type == "Type").Value);

            var data = this._UserRetrieveService.RetrieveResult<UserByType, UserLogin>(new UserByType()
            {
                UserType = ((PrestaQiEnum.UserType)type),
                User_Id = userId
            });

            string html = string.Empty;

            if (type == (int)PrestaQiEnum.UserType.Inversionista)
                html = this._DocumentUserWriteService.ExecuteProcess<Investor, string>(data.User as Investor);

            if (type == (int)PrestaQiEnum.UserType.Acreditado)
                html = this._DocumentUserWriteService.ExecuteProcess<Accredited, string>(data.User as Accredited);

            return new ContentResult
            {
                ContentType = "text/html",
                StatusCode = (int)HttpStatusCode.OK,
                Content = html
            };
        }
    }
}
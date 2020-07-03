using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using InsiscoCore.Base.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PrestaQi.Api.Configuration;
using PrestaQi.Model;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Model.Dto.Output;
using PrestaQi.Model.Enum;

namespace PrestaQi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : CustomController
    {
        IRetrieveService<User> _UserRetrieveService;
        IConfiguration _Configuration;

        public LoginController(
            IConfiguration configuration,
            IRetrieveService<User> userRetrieveService)
        {
            this._UserRetrieveService = userRetrieveService;
            this._Configuration = configuration;
        }

        [HttpPost, AllowAnonymous]
        public IActionResult Login(PrestaQi.Model.Dto.Input.Login login)
        {
            IActionResult response = Unauthorized();

            var user = this._UserRetrieveService.RetrieveResult<Login, UserLogin>(login);

            if (user != null)
            {
                var tokenString = GenerateJSONWebToken(user);
                response = Ok(new { Token = tokenString, User = user.User, Type = user.Type, TypeName = ((PrestaQiEnum.UserType)user.Type).ToString() });
            }

            return response;
        }

        private string GenerateJSONWebToken(UserLogin user)
        {
            string nameComplete = string.Empty;
            string mail = string.Empty;
            int id = 0;

            switch (user.Type)
            {
                case 1:
                    nameComplete = $"{((User)user.User).First_Name} {((User)user.User).Last_Name}";
                    mail = ((User)user.User).Mail;
                    id = ((User)user.User).id;
                    break;
                case 2:
                    nameComplete = $"{((Investor)user.User).First_Name} {((Investor)user.User).Last_Name}";
                    mail = ((Investor)user.User).Mail;
                    id = ((Investor)user.User).id;
                    break;
                case 3:
                    nameComplete = $"{((Accredited)user.User).First_Name} {((Accredited)user.User).Last_Name}";
                    mail = ((Accredited)user.User).Mail;
                    id = ((Accredited)user.User).id;
                    break;
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_Configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.GivenName, nameComplete),
                new Claim(JwtRegisteredClaimNames.Email, mail),
                new Claim("Type", user.Type.ToString()),
                new Claim("UserId", id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(_Configuration["Jwt:Issuer"],
              _Configuration["Jwt:Issuer"],
              claims,
              expires: DateTime.Now.AddMinutes(Convert.ToInt32(_Configuration["Jwt:Duration"])),
              signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
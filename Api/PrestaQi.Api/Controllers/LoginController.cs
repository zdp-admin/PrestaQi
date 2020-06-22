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

            var user = this._UserRetrieveService.Find<Login, User>(login);

            if (user != null)
            {
                var tokenString = GenerateJSONWebToken(user);
                response = Ok(tokenString);
            }

            return response;
        }

        private string GenerateJSONWebToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_Configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.GivenName, $"{user.First_Name} {user.Last_Name}"),
                new Claim(JwtRegisteredClaimNames.Email, user.Mail),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(_Configuration["Jwt:Issuer"],
              _Configuration["Jwt:Issuer"],
              claims,
              expires: DateTime.Now.AddMinutes(120),
              signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
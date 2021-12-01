using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using InsiscoCore.Base.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
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
        IWriteService<Accredited> _AccreditedWriteService;
        IProcessService<Accredited> _AccreditedProcess;

        public LoginController(
            IConfiguration configuration,
            IRetrieveService<User> userRetrieveService,
            IWriteService<Accredited> accreditedWriteService,
            IProcessService<Accredited> accreditedProcessService
            )
        {
            this._UserRetrieveService = userRetrieveService;
            this._Configuration = configuration;
            this._AccreditedWriteService = accreditedWriteService;
            this._AccreditedProcess = accreditedProcessService;
        }

        [HttpPost, AllowAnonymous]
        public IActionResult Login(Login login)
        {
            IActionResult response = Unauthorized();
            if (Request.Headers.ContainsKey("LicenseName"))
            {
                Request.Headers.TryGetValue("LicenseName", out var LicenseName);
                login.LicenceName = LicenseName;
            }

            var user = this._UserRetrieveService.RetrieveResult<Login, UserLogin>(login);
            string contract = string.Empty;

            if (user != null)
            {
                if (user.Type == 3)
                {
                    if (((Accredited)user.User).id <= 0)
                    {
                        this._AccreditedWriteService.Create((Accredited)user.User);
                    }
                }

                var tokenString = GenerateJSONWebToken(user);

                    return Ok(new
                    {
                        Token = tokenString,
                        User = user.User,
                        Type = user.Type,
                        TypeName = ((PrestaQiEnum.UserType)user.Type).ToString()
                    });
                
            }

            return Success("User not found");
        }

        [HttpPost, Route("VerifyEmployeeNumber")]
        public IActionResult VerifyEmployeeNumber(LoginVerifyNumber login)
        {
            if (Request.Headers.ContainsKey("LicenseName"))
            {
                Request.Headers.TryGetValue("LicenseName", out var LicenseName);
                login.LicenceName = LicenseName;

                var user = this._UserRetrieveService.RetrieveResult<LoginVerifyNumber, VerifyEmployeeNumber>(login);

                return Ok(user);
            } else
            {
                return NotFound("La licencia no es valida");
            }
        }

        [HttpPost, AllowAnonymous]
        [Route("LoginDocumentation")]
        public IActionResult LoginDocumentation(Login login)
        {
            if (!(login.Mail == "gte.desarrollo@singh.com.mx" && login.Password == "Singh2021*"))
            {
                return NotFound("User not found");
            }

            var user = new User()
            {
                First_Name = "gte",
                Last_Name = "desarrollo",
                Mail = "gte.desarrollo@singh.com.mx",
                id = 1,
                Modules = new System.Collections.Generic.List<UserModule>()
            };

            var tokenString = GenerateJSONWebToken(new UserLogin()
            {
                Type = 1,
                User = user
            });

            return Ok(new
            {
                Token = tokenString,
                User = user,
                Type = 1,
                TypeName = "Administrador"
            });
        }

        [HttpPut, AllowAnonymous]
        [Route("ChangeEmail")]
        public IActionResult ChangeEmail(ChangeEmail changeEmail)
        {
            return Ok(this._AccreditedWriteService.Update<ChangeEmail, bool>(changeEmail));
        }

        [HttpPut, AllowAnonymous]
        [Route("UpdateEmail")]
        public IActionResult UpdateEmail(UpdateEmail updateEmail)
        {
            return Ok(this._AccreditedProcess.ExecuteProcess<UpdateEmail, bool>(updateEmail));
        }

        private string GenerateJSONWebToken(UserLogin user)
        {
            string nameComplete = string.Empty;
            string mail = string.Empty;
            int id = 0;
            int? licenseId = 0;

            switch (user.Type)
            {
                case 1:
                    nameComplete = $"{((User)user.User).First_Name.TrimStart().TrimEnd()} {((User)user.User).Last_Name.TrimStart().TrimEnd()}";
                    mail = ((User)user.User).Mail;
                    id = ((User)user.User).id;
                    break;
                case 2:
                    nameComplete = $"{((Investor)user.User).First_Name.TrimStart().TrimEnd()} {((Investor)user.User).Last_Name.TrimStart().TrimEnd()}";
                    mail = ((Investor)user.User).Mail;
                    id = ((Investor)user.User).id;
                    break;
                case 3:
                    nameComplete = $"{((Accredited)user.User).First_Name.TrimStart().TrimEnd()} {((Accredited)user.User).Last_Name.TrimStart().TrimEnd()}";
                    mail = ((Accredited)user.User).Mail;
                    id = ((Accredited)user.User).id;
                    licenseId = ((Accredited)user.User).License_Id;
                    break;
                case 4:
                    nameComplete = ((License)user.User).NamePersonCharge;
                    mail = ((License)user.User).Mail;
                    id = ((License)user.User).id;
                    break;
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_Configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);


            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.GivenName, nameComplete),
                new Claim(JwtRegisteredClaimNames.Email, mail),
                new Claim("Type", user.Type.ToString()),
                new Claim("TypeName", ((PrestaQiEnum.UserType)user.Type).ToString()),
                new Claim("UserId", id.ToString()),
                new Claim("LicenseId", licenseId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            }.ToList();

            if (user.Type == (int)PrestaQiEnum.UserType.Administrador)
            {
                claims.Add(new Claim("Roles", JsonConvert.SerializeObject(((User)user.User).Modules)));
            }

            var token = new JwtSecurityToken(_Configuration["Jwt:Issuer"],
              _Configuration["Jwt:Issuer"],
              claims,
              expires: DateTime.Now.AddMinutes(Convert.ToInt32(_Configuration["Jwt:Duration"])),
              signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
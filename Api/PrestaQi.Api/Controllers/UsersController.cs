using DocumentFormat.OpenXml.Wordprocessing;
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
        IRetrieveService<AccreditedContractMutuo> _AccreditedContractMutuo;
        IWriteService<AccreditedContractMutuo> _AccreditedContractMutuoWrite;
        IProcessService<Advance> _AdvanceProcessService;
        IRetrieveService<AcreditedCartaMandato> _AcreditedCartaMandato;
        IWriteService<AcreditedCartaMandato> _AcreditedCartaMandatoWrite;

        public UsersController(
            IWriteService<User> userWriteService, 
            IRetrieveService<User> userRetrieveService,
            IProcessService<DocumentUser> documentUserWriteService,
            IRetrieveService<AccreditedContractMutuo> accreditedContractMutuo,
            IWriteService<AccreditedContractMutuo> accreditedContractMutuoWrite,
            IProcessService<Advance> advanceProcessService,
            IRetrieveService<AcreditedCartaMandato> acreditedCartaMandato,
            IWriteService<AcreditedCartaMandato> acreditedCartaMandatoWrite
            )
        {
            this._UserWriteService = userWriteService;
            this._UserRetrieveService = userRetrieveService;
            this._DocumentUserWriteService = documentUserWriteService;
            this._AccreditedContractMutuo = accreditedContractMutuo;
            this._AccreditedContractMutuoWrite = accreditedContractMutuoWrite;
            this._AdvanceProcessService = advanceProcessService;
            this._AcreditedCartaMandato = acreditedCartaMandato;
            this._AcreditedCartaMandatoWrite = acreditedCartaMandatoWrite;
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
        public IActionResult GetContract([FromQuery] string token, [FromQuery] int capitalId)
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
                html = this._DocumentUserWriteService.ExecuteProcess<DocumentInvestor, string>(new DocumentInvestor()
                {
                    Investor = data.User as Investor,
                    CapitalId = capitalId
                });

            if (type == (int)PrestaQiEnum.UserType.Acreditado)
                html = this._DocumentUserWriteService.ExecuteProcess<Accredited, string>(data.User as Accredited);

            return new ContentResult
            {
                ContentType = "text/html",
                StatusCode = (int)HttpStatusCode.OK,
                Content = html
            };
        }

        [HttpGet, Route("GetCartaAvisoGeneral"), AllowAnonymous]
        public IActionResult GetCartaAvisoGeneral([FromQuery] string token)
        {
            string html = string.Empty;

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

            var accredited = data.User as Accredited;

            var contractMutuo = this._AccreditedContractMutuo.Where(c => c.Accredited_Id == accredited.id).FirstOrDefault();

            if (contractMutuo is null)
            {
                this._AccreditedContractMutuoWrite.Create(new AccreditedContractMutuo { Accredited_Id = accredited.id, Path_Contract = "", created_at = DateTime.Now, updated_at = DateTime.Now });
                contractMutuo = this._AccreditedContractMutuo.Where(c => c.Accredited_Id == accredited.id).FirstOrDefault();
            }

            if (type == (int)PrestaQiEnum.UserType.Acreditado)
                html = this._DocumentUserWriteService.ExecuteProcess<CartaAvisoGeneral, string>(new CartaAvisoGeneral { accredited = accredited, contractMutuo = contractMutuo });

            return new ContentResult
            {
                ContentType = "text/html",
                StatusCode = (int)HttpStatusCode.OK,
                Content = html
            };
        }

        [HttpGet, Route("GetCartaMandato"), AllowAnonymous]
        public IActionResult GetCartaMandato([FromQuery] string token, 
                                             [FromQuery] double amount, 
                                             [FromQuery] int days, 
                                             [FromQuery] int commision, 
                                             [FromQuery] double totalAmount)
        {
            string html = string.Empty;

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

            var advance = new Advance();
            advance.Amount = amount;
            advance.Day_For_Payment = days;
            advance.Comission = commision;
            advance.Total_Withhold = totalAmount;

            var accredited = data.User as Accredited;

            var cartaMandato = this._AcreditedCartaMandato.Where(c => c.Accredited_Id == accredited.id).FirstOrDefault();

            if (cartaMandato is null)
            {
                this._AcreditedCartaMandatoWrite.Create(new AcreditedCartaMandato { Accredited_Id = accredited.id, Path_Contract = "", created_at = DateTime.Now, updated_at = DateTime.Now });
                cartaMandato = this._AcreditedCartaMandato.Where(c => c.Accredited_Id == accredited.id).FirstOrDefault();
            }

            var contractMutuo = this._AccreditedContractMutuo.Where(c => c.Accredited_Id == accredited.id).FirstOrDefault();

            if (contractMutuo is null)
            {
                this._AccreditedContractMutuoWrite.Create(new AccreditedContractMutuo { Accredited_Id = accredited.id, Path_Contract = "", created_at = DateTime.Now, updated_at = DateTime.Now });
                contractMutuo = this._AccreditedContractMutuo.Where(c => c.Accredited_Id == accredited.id).FirstOrDefault();
            }

            if (type == (int)PrestaQiEnum.UserType.Acreditado)
                html = this._DocumentUserWriteService.ExecuteProcess<CartaMandato, string>(new CartaMandato { accredited = accredited, advance = advance, acreditedCartaMandato = cartaMandato, contractMutuo = contractMutuo });

            return new ContentResult
            {
                ContentType = "text/html",
                StatusCode = (int)HttpStatusCode.OK,
                Content = html
            };
        }

        [HttpGet, Route("GetContratoMutuo"), AllowAnonymous]
        public IActionResult GetContratoMutuo([FromQuery] string token)
        {
            string html = string.Empty;

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

            var accredited = data.User as Accredited;
            var calculateAmount = new CalculateAmount { Accredited_Id = accredited.id };
            var advance = this._AdvanceProcessService.ExecuteProcess<CalculateAmount, Advance>(calculateAmount);

            var contractMutuo = this._AccreditedContractMutuo.Where(c => c.Accredited_Id == accredited.id).FirstOrDefault();

            if (contractMutuo is null)
            {
                this._AccreditedContractMutuoWrite.Create(new AccreditedContractMutuo { Accredited_Id = accredited.id, Path_Contract = "", created_at = DateTime.Now, updated_at = DateTime.Now});
                contractMutuo = this._AccreditedContractMutuo.Where(c => c.Accredited_Id == accredited.id).FirstOrDefault();
            }

            if (type == (int)PrestaQiEnum.UserType.Acreditado)
                html = this._DocumentUserWriteService.ExecuteProcess<ContratoMutuo, string>(new ContratoMutuo { accredited = accredited, contractMutuo = contractMutuo, advance = advance });

            return new ContentResult
            {
                ContentType = "text/html",
                StatusCode = (int)HttpStatusCode.OK,
                Content = html
            };
        }

        [HttpGet, Route("GetTransferenciaDatosPersonales"), AllowAnonymous]
        public IActionResult GetTransferenciaDatosPersonales([FromQuery] string token)
        {
            string html = string.Empty;

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

            if (type == (int)PrestaQiEnum.UserType.Acreditado)
                html = this._DocumentUserWriteService.ExecuteProcess<TransferenciaDatosPersonales, string>(new TransferenciaDatosPersonales { accredited = data.User as Accredited });

            return new ContentResult
            {
                ContentType = "text/html",
                StatusCode = (int)HttpStatusCode.OK,
                Content = html
            };
        }

        [HttpGet, Route("GetTerminosCondiciones"), AllowAnonymous]
        public IActionResult GetTerminosCondiciones([FromQuery] string token)
        {
            string html = this._DocumentUserWriteService.ExecuteProcess<TerminosCondiciones, string>(new TerminosCondiciones() { });

            return new ContentResult
            {
                ContentType = "text/html",
                StatusCode = (int)HttpStatusCode.OK,
                Content = html
            };
        }

        [HttpGet, Route("GetAvisoPrivacidad"), AllowAnonymous]
        public IActionResult GetAvisoPrivacidad([FromQuery] string token)
        {
            string html = this._DocumentUserWriteService.ExecuteProcess<AvisoPrivacidad, string>(new AvisoPrivacidad() { });

            return new ContentResult
            {
                ContentType = "text/html",
                StatusCode = (int)HttpStatusCode.OK,
                Content = html
            };
        }
    }
}
using InsiscoCore.Base.Data;
using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using PrestaQi.Model;
using PrestaQi.Model.Configurations;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Model.Dto.Output;
using PrestaQi.Service.Tools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PrestaQi.Service.RetrieveServices
{
    public class UserRetrieveService : RetrieveService<User>
    {
        IRetrieveService<Investor> _InvestorRetrieveService;
        IRetrieveService<Accredited> _AccreditedRetrieveService;
        IRetrieveService<UserModule> _UserModuleRetrieveService;

        public UserRetrieveService(
            IRetrieveRepository<User> repository,
            IRetrieveService<Investor> investorRetrieveService,
            IRetrieveService<Accredited> accreditedRetrieveService,
            IRetrieveService<UserModule> userModuleRetrieveService
            ) : base(repository)
        {
            this._InvestorRetrieveService = investorRetrieveService;
            this._AccreditedRetrieveService = accreditedRetrieveService;
            this._UserModuleRetrieveService = userModuleRetrieveService;
        }

        public override IEnumerable<User> Where(Func<User, bool> predicate)
        {
            var users = this._Repository.Where(predicate).ToList();

            users.ForEach(p =>
            {
                p.Modules = this._UserModuleRetrieveService.Where(m => m.user_id == p.id).ToList();
            });

            return users;
        }

        public UserLogin RetrieveResult(Login login)
        {
            User user = this._Repository.Where(p => p.Mail == login.Mail).FirstOrDefault();
            Investor investor = this._InvestorRetrieveService.Where(p => p.Mail == login.Mail).FirstOrDefault();
            Accredited accredited = this._AccreditedRetrieveService.Where(p => p.Mail == login.Mail).FirstOrDefault();

            if (user != null)
            {
                if (user.Password != InsiscoCore.Utilities.Crypto.MD5.Encrypt(login.Password))
                    throw new SystemValidationException("Invalid Password!");

                user.Modules = this._UserModuleRetrieveService.Where(p => p.user_id == user.id).ToList();

                return new UserLogin() { Type = 1, User = user };
            }

            if (investor != null)
            {
                if (investor.Password != InsiscoCore.Utilities.Crypto.MD5.Encrypt(login.Password))
                    throw new SystemValidationException("Invalid Password!");

                return new UserLogin() { Type = 2, User = investor };
            }

            if (accredited != null)
            {
                if (accredited.Password != InsiscoCore.Utilities.Crypto.MD5.Encrypt(login.Password))
                    throw new SystemValidationException("Invalid Password!");

                accredited.Advances = null;
                return new UserLogin() { Type = 3, User = accredited };
            }

            throw new SystemValidationException("User not found!");
        }

        public UserLogin RetrieveResult(UserByType userByType)
        {
            if (userByType.UserType == Model.Enum.PrestaQiEnum.UserType.Administrador)
            {
                var user = this._Repository.Find(userByType.User_Id);
                user.Modules = this._UserModuleRetrieveService.Where(p => p.user_id == user.id).ToList();
                return new UserLogin() { User = user };
            }

            if (userByType.UserType == Model.Enum.PrestaQiEnum.UserType.Inversionista)
            {
                var user = this._InvestorRetrieveService.Find(userByType.User_Id);
                return new UserLogin() { User = user };
            }

            if (userByType.UserType == Model.Enum.PrestaQiEnum.UserType.Acreditado)
            {
                var user = this._AccreditedRetrieveService.Find(userByType.User_Id);
                user.Advances = null;
                return new UserLogin() { User = user };
            }

            throw new SystemValidationException("Type not found!");
        }
      
    }
}

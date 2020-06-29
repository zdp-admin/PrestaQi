using InsiscoCore.Base.Data;
using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using PrestaQi.Model;
using PrestaQi.Model.Configurations;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Model.Dto.Output;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PrestaQi.Service.RetrieveServices
{
    public class UserRetrieveService : RetrieveService<User>
    {
        IRetrieveService<Investor> _InvestorRetrieveService;
        IRetrieveService<Accredited> _AccreditedRetrieveService;

        public UserRetrieveService(
            IRetrieveRepository<User> repository,
            IRetrieveService<Investor> investorRetrieveService,
            IRetrieveService<Accredited> accreditedRetrieveService
            ) : base(repository)
        {
            this._InvestorRetrieveService = investorRetrieveService;
            this._AccreditedRetrieveService = accreditedRetrieveService;
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
    }
}

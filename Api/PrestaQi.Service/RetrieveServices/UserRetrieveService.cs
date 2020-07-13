using InsiscoCore.Base.Data;
using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using PrestaQi.Model;
using PrestaQi.Model.Configurations;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Model.Dto.Output;
using PrestaQi.Model.Enum;
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
                p.Type = (int)PrestaQiEnum.UserType.Administrador;
                p.TypeName = PrestaQiEnum.UserType.Administrador.ToString();
            });

            return users;
        }

        public UserLogin RetrieveResult(Login login)
        {
            User user = this._Repository.Where(p => p.Mail == login.Mail && p.Deleted_At == null).FirstOrDefault();
            Investor investor = this._InvestorRetrieveService.Where(p => p.Mail == login.Mail && p.Deleted_At == null).FirstOrDefault();
            Accredited accredited = this._AccreditedRetrieveService.Where(p => p.Mail == login.Mail && p.Deleted_At == null).FirstOrDefault();

            if (user != null)
            {
                if (!user.Enabled)
                    throw new SystemValidationException("Inactive User!");

                if (user.Password != InsiscoCore.Utilities.Crypto.MD5.Encrypt(login.Password))
                    throw new SystemValidationException("Invalid Password!");

                user.Modules = this._UserModuleRetrieveService.Where(p => p.user_id == user.id).ToList();

                return new UserLogin() { Type = 1, User = user };
            }

            if (investor != null)
            {
                if (!investor.Enabled)
                    throw new SystemValidationException("Inactive User!");

                if (investor.Password != InsiscoCore.Utilities.Crypto.MD5.Encrypt(login.Password))
                    throw new SystemValidationException("Invalid Password!");

                return new UserLogin() { Type = 2, User = investor };
            }

            if (accredited != null)
            {
                if (!accredited.Enabled)
                    throw new SystemValidationException("Inactive User!");

                if (accredited.Password != InsiscoCore.Utilities.Crypto.MD5.Encrypt(login.Password))
                    throw new SystemValidationException("Invalid Password!");

                accredited.Advances = null;

                return new UserLogin() { Type = 3, User = accredited };
            }

            throw new SystemValidationException("User not found!");
        }

        public UserLogin RetrieveResult(DisableUser disableUser)
        { 
            if (disableUser.Type == 1)
            {
                User user = this._Repository.Where(p => p.id == disableUser.UserId).FirstOrDefault();
                if (disableUser.IsDelete)
                {
                    user.Enabled = false;
                    user.Deleted_At = DateTime.Now;
                }
                else
                    user.Enabled = disableUser.Value;
                
                return new UserLogin() { User = user };
            }

            if (disableUser.Type == 2)
            {
                Investor investor = this._InvestorRetrieveService.Where(p => p.id == disableUser.UserId).FirstOrDefault();
                if (disableUser.IsDelete)
                {
                    investor.Enabled = false;
                    investor.Deleted_At = DateTime.Now;
                }
                else
                    investor.Enabled = disableUser.Value;

                return new UserLogin() { User = investor };
            }

            if (disableUser.Type == 3)
            {
                Accredited accredited = this._AccreditedRetrieveService.Where(p => p.id == disableUser.UserId).FirstOrDefault();
                if (disableUser.IsDelete)
                {
                    accredited.Enabled = false;
                    accredited.Deleted_At = DateTime.Now;
                }
                else
                    accredited.Enabled = disableUser.Value;

                return new UserLogin() { User = accredited };
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

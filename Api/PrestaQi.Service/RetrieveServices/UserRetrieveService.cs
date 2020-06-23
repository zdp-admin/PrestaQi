using InsiscoCore.Base.Data;
using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using PrestaQi.Model;
using PrestaQi.Model.Configurations;
using PrestaQi.Model.Dto.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PrestaQi.Service.RetrieveServices
{
    public class UserRetrieveService : RetrieveService<User>
    {
        public UserRetrieveService(
            IRetrieveRepository<User> repository
            ) : base(repository)
        {
        }

        public User RetrieveResult(Login login)
        {
            var user = this._Repository.Where(p => p.Mail == login.Mail).FirstOrDefault();

            if (user == null)
                throw new SystemValidationException("User not found!");

            if (user.Password != InsiscoCore.Utilities.Crypto.MD5.Encrypt(login.Password))
                throw new SystemValidationException("Invalid Password!");

            return user;
        }


    }
}

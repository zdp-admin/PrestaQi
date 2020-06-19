using JabilCore.Base.Data;
using JabilCore.Service;
using PrestaQi.Model;
using PrestaQi.Model.Dto.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

namespace PrestaQi.Service.RetrieveServices
{
    public class UserRetrieveService : RetrieveService<User>
    {
        public UserRetrieveService(
            IRetrieveRepository<User> repository
            ) : base(repository)
        {
        }

        public User Find(Login login)
        {
            var user = this._Repository.Where(p => p.Mail == login.Mail).FirstOrDefault();

            if (user == null)
                throw new Exception("User not found!");

            if (user.Password != JabilCore.Utilities.Crypto.MD5.Encrypt(login.Password))
                throw new Exception("Invalid Password!");

            return user;
        }
    }
}

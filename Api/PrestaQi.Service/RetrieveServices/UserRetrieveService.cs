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
        IRetrieveService<UserProperty> _UserPropertyRetrieveService;

        public UserRetrieveService(
            IRetrieveRepository<User> repository,
            IRetrieveService<UserProperty> userPropertyRetrieveService
            ) : base(repository)
        {
            this._UserPropertyRetrieveService = userPropertyRetrieveService;
        }

        public User Find(Login login)
        {
            var user = this._Repository.Where(p => p.Mail == login.Mail).FirstOrDefault();

            if (user == null)
                throw new SystemValidationException("User not found!");

            if (user.Password != InsiscoCore.Utilities.Crypto.MD5.Encrypt(login.Password))
                throw new SystemValidationException("Invalid Password!");

            return user;
        }

        public override IEnumerable<User> Where(Func<User, bool> predicate)
        {
            var listResult = this._Repository.Where(predicate).ToList();

            var properties = this._UserPropertyRetrieveService.Where(p => true).ToList();

            foreach (var item in listResult)
            {
                item.User_Properties = properties.Where(p => p.User_Id == item.id).ToList();
            }

            return listResult;
        }

    }
}

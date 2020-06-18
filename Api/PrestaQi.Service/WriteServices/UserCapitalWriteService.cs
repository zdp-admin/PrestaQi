using JabilCore.Base.Data;
using JabilCore.Base.Service;
using JabilCore.Service;
using PrestaQi.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Service.WriteServices
{
    public class UserCapitalWriteService : WriteService<UserCapital>
    {
        IRetrieveService<UserCapital> _UserCapitalRetrieveService;

        public UserCapitalWriteService(
            IWriteRepository<UserCapital> repository,
            IRetrieveService<UserCapital> userCapitalRetrieveService
            ) : base(repository)
        {
            this._UserCapitalRetrieveService = userCapitalRetrieveService;
        }

        public override bool Create(UserCapital entity)
        {
            try
            {
                entity.created_at = DateTime.Now;
                entity.updated_at = DateTime.Now;

                return base.Create(entity);
            }
            catch (Exception exception)
            {
                throw;
            }
           
        }

    }
}

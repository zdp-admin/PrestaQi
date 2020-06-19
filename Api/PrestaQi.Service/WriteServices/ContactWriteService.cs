
using JabilCore.Base.Data;
using JabilCore.Base.Service;
using JabilCore.Service;
using Microsoft.AspNetCore.Authorization;
using PrestaQi.Model;
using System;

namespace PrestaQi.Service.WriteServices
{
    public class ContactWriteService : WriteService<Contact>
    {
        IRetrieveService<Contact> _ContactRetrieveService;

        public ContactWriteService(
            IWriteRepository<Contact> repository,
            IRetrieveService<Contact> contactRetrieveService
            ) : base(repository)
        {
            this._ContactRetrieveService = contactRetrieveService;
        }

        public override bool Create(Contact entity)
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

        public override bool Update(Contact entity)
        {
            try
            {
                Contact entityFound = this._ContactRetrieveService.Find(entity.id);

                if (entityFound != null)
                {
                    entity.updated_at = DateTime.Now;
                    entity.created_at = entityFound.created_at;

                    return base.Update(entity);
                }

                throw new Exception("NO_FOUND");
            }
            catch (Exception exception)
            {
                throw;
            }
            
        }
    }
}


using InsiscoCore.Base.Data;
using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using PrestaQi.Model;
using PrestaQi.Model.Configurations;
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
                throw new SystemValidationException("Error creating Contact!");
            }
        }

        public override bool Update(Contact entity)
        {
            Contact entityFound = this._ContactRetrieveService.Find(entity.id);

            if (entityFound == null)
                throw new SystemValidationException("Contact not found!");

            entity.updated_at = DateTime.Now;
            entity.created_at = entityFound.created_at;

            try
            {
                return base.Update(entity);
            }
            catch (Exception) { throw new SystemValidationException("Error updating Contact!"); }
        }
    }
}

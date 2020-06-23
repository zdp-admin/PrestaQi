using InsiscoCore.Base.Data;
using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using PrestaQi.Model;
using PrestaQi.Model.Configurations;
using System;

namespace PrestaQi.Service.WriteServices
{
    public class CapitalWriteService : WriteService<Capital>
    {
        IRetrieveService<Capital> _UserCapitalRetrieveService;

        public CapitalWriteService(
            IWriteRepository<Capital> repository,
            IRetrieveService<Capital> userCapitalRetrieveService
            ) : base(repository)
        {
            this._UserCapitalRetrieveService = userCapitalRetrieveService;
        }

        public override bool Create(Capital entity)
        {
            try
            {
                entity.created_at = DateTime.Now;
                entity.updated_at = DateTime.Now;

                return base.Create(entity);
            }
            catch (Exception exception)
            {
                throw new SystemValidationException("Error creating Capital!");
            }
           
        }

    }
}

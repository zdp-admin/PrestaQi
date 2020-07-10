using InsiscoCore.Base.Data;
using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using PrestaQi.Model;
using PrestaQi.Model.Configurations;
using System;

namespace PrestaQi.Service.WriteServices
{
    public class CompanyWriteService : WriteService<Company>
    {
        IRetrieveService<Company> _CompanyRetrieveService;
        
        public CompanyWriteService(
            IWriteRepository<Company> repository,
            IRetrieveService<Company> companyRetrieveService
            ) : base(repository)
        {
            this._CompanyRetrieveService = companyRetrieveService;
        }

        public override bool Create(Company entity)
        {
            try
            {
                entity.Enabled = true;
                entity.created_at = DateTime.Now;
                entity.updated_at = DateTime.Now;

                return base.Create(entity);
            }
            catch (Exception exception)
            {
                throw new SystemValidationException($"Error creating Company: {exception.Message}");
            }

        }

        public override bool Update(Company entity)
        {
            Company company = this._CompanyRetrieveService.Find(entity.id);
            if (company == null)
                throw new SystemValidationException("Company not found");

            entity.updated_at = DateTime.Now;
            entity.created_at = entity.created_at;

            try
            {
                return base.Update(entity);
            }
            catch (Exception exception) { throw new SystemValidationException($"Error updating Company: {exception.Message}");  }
        }

    }
}

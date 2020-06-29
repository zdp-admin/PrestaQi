using InsiscoCore.Base.Data;
using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using PrestaQi.Model;
using PrestaQi.Model.Configurations;
using PrestaQi.Model.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PrestaQi.Service.WriteServices
{
    public class InvestorWriteService : WriteService<Investor>
    {
        IRetrieveService<Investor> _InvestorRetrieveService;

        public InvestorWriteService(
            IWriteRepository<Investor> repository,
            IRetrieveService<Investor> investorRetrieveService
            ) : base(repository)
        {
            this._InvestorRetrieveService = investorRetrieveService;
        }

        public override bool Create(Investor entity)
        {
            try
            {
                entity.Password = InsiscoCore.Utilities.Crypto.MD5.Encrypt(entity.Password);
                entity.created_at = DateTime.Now;
                entity.updated_at = DateTime.Now;

                return base.Create(entity);
            }
            catch (Exception exception)
            {
                throw new SystemValidationException("Error creating Investor");
            }

        }

        public override bool Update(Investor entity)
        {
            Investor investor = this._InvestorRetrieveService.Find(entity.id);
            if (investor == null)
                throw new SystemValidationException("Investor not found");

            entity.updated_at = DateTime.Now;
            entity.created_at = entity.created_at;

            try
            {
                return base.Update(entity);
            }
            catch (Exception exception) { throw new SystemValidationException("Error updating Investor!");  }
        }

        public override bool Create(IEnumerable<Investor> entities)
        {
            try
            {
                entities.ToList().ForEach(p =>
                {
                    p.created_at = DateTime.Now;
                    p.updated_at = DateTime.Now;
                });

                return base.Create(entities);
            }
            catch (Exception exception)
            {
                throw new SystemValidationException("Error creating Investors");
            }
            
        }
    }
}

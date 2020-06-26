using InsiscoCore.Base.Data;
using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using PrestaQi.Model;
using PrestaQi.Model.Configurations;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Model.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Service.WriteServices
{
    public class CapitalDetailWriteService : WriteService<CapitalDetail>
    {
        IRetrieveService<CapitalDetail> _CapitalDetailRetrieveService;

        public CapitalDetailWriteService(
            IWriteRepository<CapitalDetail> repository,
            IRetrieveService<CapitalDetail> capitalDetailRetrieveService
            ) : base(repository)
        {
            this._CapitalDetailRetrieveService = capitalDetailRetrieveService;
    }

        public override bool Update(CapitalDetail entity)
        {
            var entityFound = this._CapitalDetailRetrieveService.Find(entity.id);

            if (entityFound == null)
                throw new SystemValidationException("Record not found");

            entity.IsPayment = true;
            entity.updated_at = DateTime.Now;
            entity.created_at = entityFound.created_at;

            return base.Update(entity);
        }

    }
}

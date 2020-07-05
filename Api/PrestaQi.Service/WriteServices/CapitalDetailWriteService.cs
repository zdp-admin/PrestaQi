using InsiscoCore.Base.Data;
using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using PrestaQi.Model;
using PrestaQi.Model.Configurations;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Model.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrestaQi.Service.WriteServices
{
    public class CapitalDetailWriteService : WriteService<CapitalDetail>
    {
        IRetrieveService<CapitalDetail> _CapitalDetailRetrieveService;
        IRetrieveRepository<Capital> _CapitalRetrieveRepository;
        IWriteRepository<Capital> _CapitaWriteRepository;

        public CapitalDetailWriteService(
            IWriteRepository<CapitalDetail> repository,
            IRetrieveService<CapitalDetail> capitalDetailRetrieveService,
            IRetrieveRepository<Capital> capitalRetrieveRepository,
            IWriteRepository<Capital> capitaWriteRepository
            ) : base(repository)
        {
            this._CapitalDetailRetrieveService = capitalDetailRetrieveService;
            this._CapitalRetrieveRepository = capitalRetrieveRepository;
            this._CapitaWriteRepository = capitaWriteRepository;
        }

        public override bool Update(CapitalDetail entity)
        {
            var entityFound = this._CapitalDetailRetrieveService.Find(entity.id);

            if (entityFound == null)
                throw new SystemValidationException("Record not found");

            entity.IsPayment = true;
            entity.updated_at = DateTime.Now;
            entity.created_at = entityFound.created_at;

            bool update = base.Update(entity);

            if (update && this._CapitalDetailRetrieveService.Where(p => p.IsPayment == false).Count() == 0)
            {
                var capital = this._CapitalRetrieveRepository.Find(entity.Capital_Id);
                capital.Enabled = false;
                capital.Investment_Status = (int)PrestaQiEnum.InvestmentEnum.NoActive;

            }
            return update;
        }

    }
}

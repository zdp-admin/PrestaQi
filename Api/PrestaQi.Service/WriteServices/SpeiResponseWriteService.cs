using ClosedXML;
using InsiscoCore.Base.Data;
using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using Newtonsoft.Json.Schema;
using PrestaQi.Model;
using PrestaQi.Model.Configurations;
using PrestaQi.Model.Dto;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Model.Dto.Output;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PrestaQi.Service.WriteServices
{
    public class SpeiResponseWriteService : WriteService<SpeiResponse>
    {
        IRetrieveService<SpeiResponse> _SpeiResponseRetrieveService;
        IRetrieveService<Advance> _AdvanceRetrieveService;
        IRetrieveService<Accredited> _AccreditedRetrieveService;
        IRetrieveService<Repayment> _RepaymentRetrieveService;
        
        public SpeiResponseWriteService(
            IWriteRepository<SpeiResponse> repository,
            IRetrieveService<SpeiResponse> speiResponseRetrieveService,
            IRetrieveService<Advance> advanceRetrieveService,
            IRetrieveService<Accredited> accreditedRetrieveService,
            IRetrieveService<Repayment> repaymentRetrieveService
            ) : base(repository)
        {
            this._SpeiResponseRetrieveService = speiResponseRetrieveService;
            this._AdvanceRetrieveService = advanceRetrieveService;
            this._AccreditedRetrieveService = accreditedRetrieveService;
            this._RepaymentRetrieveService = repaymentRetrieveService;
        }

        public override bool Create(SpeiResponse entity)
        {
            try
            {
                entity.created_at = DateTime.Now;
                entity.updated_at = DateTime.Now;

                return base.Create(entity);
            }
            catch (Exception exception)
            {
                throw new SystemValidationException($"Error creating Spei: {exception.Message}");
            }

        }

        public SpeiTransactionResult Update(StateChange stateChange)
        {
            try
            {
                SpeiTransactionResult speiTransactionResult = new SpeiTransactionResult();

                var speiResponse = this._SpeiResponseRetrieveService.Where(p => p.tracking_id == stateChange.Id).FirstOrDefault();

                if (speiResponse == null)
                    throw new SystemValidationException($"Id: {stateChange.Id} Not found");

                var advance = this._AdvanceRetrieveService.Find(speiResponse.advance_id);
                var accredited = this._AccreditedRetrieveService.Find(advance.Accredited_Id);

                speiResponse.State_Name = stateChange.Estado;

                if (stateChange.CausaDevolucion > 0)
                {
                    speiResponse.Repayment_Id = stateChange.CausaDevolucion;

                    var repayment = this._RepaymentRetrieveService.Find(speiResponse.Repayment_Id);
                    speiTransactionResult.Message = repayment.Description;
                }

                speiTransactionResult.Mail = accredited.Mail;
                speiTransactionResult.Accredited = $"{accredited.First_Name} {accredited.Last_Name}";
                speiTransactionResult.Success = base.Update(speiResponse);

                return speiTransactionResult;
            }
            catch (Exception exception)
            {
                throw new SystemValidationException($"Error update Spei: {exception.Message}");
            }
        }

    }
}

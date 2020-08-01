using InsiscoCore.Base.Data;
using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using PrestaQi.Model;
using PrestaQi.Model.Configurations;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Model.Dto.Output;
using PrestaQi.Model.Spei;
using System;
using System.Linq;

namespace PrestaQi.Service.WriteServices
{
    public class SpeiResponseWriteService : WriteService<SpeiResponse>
    {
        IRetrieveService<SpeiResponse> _SpeiResponseRetrieveService;
        IRetrieveService<Advance> _AdvanceRetrieveService;
        IRetrieveService<Accredited> _AccreditedRetrieveService;
        IRetrieveService<Repayment> _RepaymentRetrieveService;
        IProcessService<ordenPagoWS> _OrdenPagoProcessService;
        IWriteRepository<Advance> _AdvanceWriteRepository;

        public SpeiResponseWriteService(
            IWriteRepository<SpeiResponse> repository,
            IRetrieveService<SpeiResponse> speiResponseRetrieveService,
            IRetrieveService<Advance> advanceRetrieveService,
            IRetrieveService<Accredited> accreditedRetrieveService,
            IRetrieveService<Repayment> repaymentRetrieveService,
            IProcessService<ordenPagoWS> ordenPagoProcessService,
            IWriteRepository<Advance> advanceWriteRepository
            ) : base(repository)
        {
            this._SpeiResponseRetrieveService = speiResponseRetrieveService;
            this._AdvanceRetrieveService = advanceRetrieveService;
            this._AccreditedRetrieveService = accreditedRetrieveService;
            this._RepaymentRetrieveService = repaymentRetrieveService;
            this._OrdenPagoProcessService = ordenPagoProcessService;
            this._AdvanceWriteRepository = advanceWriteRepository;
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
                speiTransactionResult.UserId = accredited.id;
                speiTransactionResult.Accredited = $"{accredited.First_Name} {accredited.Last_Name}";
                speiTransactionResult.Success = base.Update(speiResponse);

                if (speiTransactionResult.Success && stateChange.CausaDevolucion == 0)
                {
                    advance.Enabled = true;
                    this._AdvanceWriteRepository.Update(advance);

                    this._OrdenPagoProcessService.ExecuteProcess<SendSpeiMail, bool>(new SendSpeiMail()
                    {
                        Amount = advance.Amount,
                        Accredited_Id = accredited.id,
                        Accredited = accredited,
                        Advance = advance
                    });
                }

                if (speiTransactionResult.Success && stateChange.CausaDevolucion > 0)
                {
                    advance.Enabled = false;
                    this._AdvanceWriteRepository.Update(advance);
                }

                return speiTransactionResult;
            }
            catch (Exception exception)
            {
                throw new SystemValidationException($"Error update Spei: {exception.Message}");
            }
        }

    }
}

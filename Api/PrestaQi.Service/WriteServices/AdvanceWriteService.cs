using InsiscoCore.Base.Data;
using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using PrestaQi.Model;
using PrestaQi.Model.Configurations;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Model.Dto.Output;
using PrestaQi.Model.Enum;
using PrestaQi.Model.Spei;
using System;

namespace PrestaQi.Service.WriteServices
{
    public class AdvanceWriteService : WriteService<Advance>
    {
        IProcessService<Advance> _AdvacenProcessService;
        IProcessService<ordenPagoWS> _OrdenPagoProcessService;
        IWriteService<SpeiResponse> _SpeiWriteService;
        IRetrieveService<Accredited> _AccreditedRetrieveService;

        public AdvanceWriteService(
            IWriteRepository<Advance> repository,
            IProcessService<Advance> advanceProcessService,
            IProcessService<ordenPagoWS> ordenPagoProcessService,
            IWriteService<SpeiResponse> speiWriteService,
            IRetrieveService<Accredited> accreditedRetrieveService
            ) : base(repository)
        {
            this._AdvacenProcessService = advanceProcessService;
            this._OrdenPagoProcessService = ordenPagoProcessService;
            this._SpeiWriteService = speiWriteService;
            this._AccreditedRetrieveService = accreditedRetrieveService;
        }

        public bool Create(CalculateAmount calculateAmount)
        {
            Advance advance = this._AdvacenProcessService.ExecuteProcess<CalculateAmount, Advance>(calculateAmount);
            Accredited accredited = this._AccreditedRetrieveService.Find(advance.Accredited_Id);
            advance.Date_Advance = DateTime.Now;
            advance.created_at = DateTime.Now;
            advance.updated_at = DateTime.Now;
            advance.Enabled = true;
            advance.Paid_Status = (int)PrestaQiEnum.AdvanceStatus.NoPagado;

            var spei = this._OrdenPagoProcessService.ExecuteProcess<OrderPayment, ResponseSpei>(new OrderPayment()
            {
                Accredited_Id = calculateAmount.Accredited_Id,
                Advance = advance
            });

            try
            {
                if (spei.resultado.id > 0)
                {
                    bool created = base.Create(advance);

                    if (created)
                    {
                        this._SpeiWriteService.Create(new SpeiResponse()
                        {
                            created_at = DateTime.Now,
                            updated_at = DateTime.Now,
                            advance_id = advance.id,
                            tracking_id = spei.resultado.id,
                            tracking_key = spei.resultado.claveRastreo
                        });

                        this._OrdenPagoProcessService.ExecuteProcess<SendSpeiMail, bool>(new SendSpeiMail()
                        {
                            Amount = advance.Amount,
                            Accredited_Id = calculateAmount.Accredited_Id,
                            Accredited = accredited,
                            Advance = advance
                        });
                    }

                    return created;
                }
                else
                    throw new SystemValidationException(spei.resultado.descripcionError);

            }
            catch (Exception exception)
            {
                throw new SystemValidationException($"Error creating Advance: {exception.Message}");
            }

        }

    }
}

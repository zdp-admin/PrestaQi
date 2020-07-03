using InsiscoCore.Base.Data;
using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using Microsoft.VisualBasic;
using PrestaQi.Model;
using PrestaQi.Model.Configurations;
using PrestaQi.Model.Dto;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Model.Dto.Output;
using PrestaQi.Model.Spei;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PrestaQi.Service.WriteServices
{
    public class AdvanceWriteService : WriteService<Advance>
    {
        IProcessService<Advance> _AdvacenProcessService;
        IProcessService<ordenPagoWS> _OrdenPagoProcessService;
        IWriteService<SpeiResponse> _SpeiWriteService;

        public AdvanceWriteService(
            IWriteRepository<Advance> repository,
            IProcessService<Advance> advanceProcessService,
            IProcessService<ordenPagoWS> ordenPagoProcessService,
            IWriteService<SpeiResponse> speiWriteService
            ) : base(repository)
        {
            this._AdvacenProcessService = advanceProcessService;
            this._OrdenPagoProcessService = ordenPagoProcessService;
            this._SpeiWriteService = speiWriteService;
        }

        public bool Create(CalculateAmount calculateAmount)
        {
           
                Advance advance = this._AdvacenProcessService.ExecuteProcess<CalculateAmount, Advance>(calculateAmount);
                advance.Date_Advance = DateTime.Now;
                advance.created_at = DateTime.Now;
                advance.updated_at = DateTime.Now;
                advance.Enabled = true;

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
                            Tracking_Key = spei.resultado.claveRastreo,
                            Accredited_Id = calculateAmount.Accredited_Id
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

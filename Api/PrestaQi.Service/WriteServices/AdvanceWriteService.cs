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
using System.Collections.Generic;
using System.Linq;

namespace PrestaQi.Service.WriteServices
{
    public class AdvanceWriteService : WriteService<Advance>
    {
        IProcessService<Advance> _AdvacenProcessService;
        IProcessService<ordenPagoWS> _OrdenPagoProcessService;
        IWriteService<SpeiResponse> _SpeiWriteService;
        IRetrieveService<Accredited> _AccreditedRetrieveService;
        IWriteService<AdvanceDetail> _AdvanceDetailWriteService;

        public AdvanceWriteService(
            IWriteRepository<Advance> repository,
            IProcessService<Advance> advanceProcessService,
            IProcessService<ordenPagoWS> ordenPagoProcessService,
            IWriteService<SpeiResponse> speiWriteService,
            IRetrieveService<Accredited> accreditedRetrieveService,
            IWriteService<AdvanceDetail> advanceDetailWriteService
            ) : base(repository)
        {
            this._AdvacenProcessService = advanceProcessService;
            this._OrdenPagoProcessService = ordenPagoProcessService;
            this._SpeiWriteService = speiWriteService;
            this._AccreditedRetrieveService = accreditedRetrieveService;
            this._AdvanceDetailWriteService = advanceDetailWriteService;
        }

        public bool Create(CalculateAmount calculateAmount)
        {
            List<Advance> advances = this._AdvacenProcessService.ExecuteProcess<CalculateAmount, List<Advance>>(calculateAmount);
            Accredited accredited = this._AccreditedRetrieveService.Find(calculateAmount.Accredited_Id);
            Advance advance = null;

            advances.ForEach(advance =>
            {
                advance.created_at = DateTime.Now;
                advance.updated_at = DateTime.Now;
                advance.Enabled = false;
                advance.Paid_Status = (int)PrestaQiEnum.AdvanceStatus.NoPagado;
            });

            if (advances.Count == 1)
                advance = advances.FirstOrDefault();
            else
            {
                advance = new Advance()
                {
                    Accredited_Id = advances.FirstOrDefault().Accredited_Id,
                    Amount = Math.Round(advances.Sum(p => p.Amount), 2),
                    Comission = advances.FirstOrDefault().Comission,
                    created_at = DateTime.Now,
                    Date_Advance = DateTime.Now,
                    Day_For_Payment = advances.FirstOrDefault().Day_For_Payment,
                    Day_Moratorium = advances.FirstOrDefault().Day_Moratorium,
                    Enabled = false,
                    Interest = advances.Sum(p => p.Interest),
                    Interest_Moratorium = Math.Round(advances.Sum(p => p.Interest_Moratorium), 2),
                    Interest_Rate = advances.FirstOrDefault().Interest_Rate,
                    Limit_Date = advances.FirstOrDefault().Limit_Date,
                    Paid_Status = (int)PrestaQiEnum.AdvanceStatus.NoPagado,
                    Promotional_Setting = advances.FirstOrDefault().Promotional_Setting,
                    Requested_Day = advances.FirstOrDefault().Requested_Day,
                    Subtotal = Math.Round(advances.Sum(p => p.Subtotal), 2),
                    Total_Withhold = Math.Round(advances.Sum(p => p.Total_Withhold), 2),
                    updated_at = DateTime.Now,
                    Vat = advances.Sum(p => p.Vat)
                };
            }
          
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
                        if (advances.Count > 1)
                            SaveDetail(advance.id, advances);

                        this._SpeiWriteService.Create(new SpeiResponse()
                        {
                            created_at = DateTime.Now,
                            updated_at = DateTime.Now,
                            advance_id = advance.id,
                            tracking_id = spei.resultado.id,
                            tracking_key = spei.resultado.claveRastreo
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

        private void SaveDetail(int advanceId, List<Advance> advances)
        {
            List<AdvanceDetail> advanceDetails = new List<AdvanceDetail>();

            advances.ForEach(p =>
            {
                AdvanceDetail advanceDetail = new AdvanceDetail()
                {
                    Advance_Id = advanceId,
                    Amount = p.Amount,
                    Comission = p.Comission,
                    id = p.id,
                    created_at = p.created_at,
                    Date_Advance = p.Date_Advance,
                    Day_For_Payment = p.Day_For_Payment,
                    Enabled = p.Enabled,
                    Interest = p.Interest,
                    Limit_Date = p.Limit_Date,
                    Paid_Status = p.Paid_Status,
                    Requested_Day = p.Requested_Day,
                    Subtotal = p.Subtotal,
                    Total_Withhold = p.Total_Withhold,
                    updated_at = p.updated_at,
                    Vat = p.Vat
                };

                advanceDetails.Add(advanceDetail);
            });

            this._AdvanceDetailWriteService.Create(advanceDetails);
        }

    }
}

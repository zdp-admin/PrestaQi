using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using PrestaQi.Model;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Model.Enum;
using System;
using System.Linq;

namespace PrestaQi.Service.ProcessServices
{
    public class PaidAdvanceProcessService : ProcessService<PaidAdvance>
    {
        IRetrieveService<Advance> _AdvanceRetrieveService;
        IWriteService<Advance> _AdvanceWriteService;
        IWriteService<PaidAdvance> _PaidAdvanceWriteService;
        IRetrieveService<PaidAdvance> _PaidAdvanceRetrieveService;

        public PaidAdvanceProcessService(
            IRetrieveService<Advance> advanceRetrieveService,
            IWriteService<Advance> advanceWriteService,
            IWriteService<PaidAdvance> paidAdvanceWriteService,
            IRetrieveService<PaidAdvance> paidAdvanceRetrieveService
            )
        {
            this._AdvanceRetrieveService = advanceRetrieveService;
            this._AdvanceWriteService = advanceWriteService;
            this._PaidAdvanceWriteService = paidAdvanceWriteService;
            this._PaidAdvanceRetrieveService = paidAdvanceRetrieveService;

        }

        public bool ExecuteProcess(SetPayAdvance data)
        {
            PaidAdvance paidAdvance = new PaidAdvance()
            {
                Amount = data.Amount,
                Company_Id = data.Company_Id,
                created_at = DateTime.Now,
                Is_Partial = data.IsPartial,
                updated_at = DateTime.Now
            };

            bool createPay = this._PaidAdvanceWriteService.Create(paidAdvance);

            if (createPay)
            {
                var advances = this._AdvanceRetrieveService.Where(p => data.AdvanceIds.Contains(p.id)).ToList();
                advances.ForEach(p =>
                {
                    p.updated_at = DateTime.Now;
                    p.Enabled = data.IsPartial ? true : false;
                    p.Paid_Status = data.IsPartial ? (int)PrestaQiEnum.AdvanceStatus.PagadoParcial :
                        (int)PrestaQiEnum.AdvanceStatus.Pagado;
                });
                this._AdvanceWriteService.Update(advances);

                if (!data.IsPartial)
                {
                    var payAdvances = this._PaidAdvanceRetrieveService.Where(p => p.Company_Id == data.Company_Id &&
                        p.created_at.Date <= DateTime.Now.Date && p.Is_Partial).ToList();

                    if (payAdvances.Count > 0)
                    {
                        payAdvances.ForEach(p =>
                        {
                            p.updated_at = DateTime.Now;
                            p.Is_Partial = false;
                        });
                        this._PaidAdvanceWriteService.Update(payAdvances);
                    }
                }

            }

            return true;
        }
    }
}

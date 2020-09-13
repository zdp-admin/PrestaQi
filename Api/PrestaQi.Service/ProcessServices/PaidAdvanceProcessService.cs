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
        IRetrieveService<AdvanceDetail> _AdvanceDetailRetrieveService;
        IWriteService<AdvanceDetail> _AdvanceDetailWriteService;
        IWriteService<PaidAdvance> _PaidAdvanceWriteService;
        IRetrieveService<PaidAdvance> _PaidAdvanceRetrieveService;


        public PaidAdvanceProcessService(
            IRetrieveService<Advance> advanceRetrieveService,
            IWriteService<Advance> advanceWriteService,
            IWriteService<PaidAdvance> paidAdvanceWriteService,
            IRetrieveService<PaidAdvance> paidAdvanceRetrieveService,
            IRetrieveService<AdvanceDetail> advanceDetailRetrieveService,
            IWriteService<AdvanceDetail> advanceDetailWriteService
            )
        {
            this._AdvanceRetrieveService = advanceRetrieveService;
            this._AdvanceWriteService = advanceWriteService;
            this._PaidAdvanceWriteService = paidAdvanceWriteService;
            this._PaidAdvanceRetrieveService = paidAdvanceRetrieveService;
            this._AdvanceDetailRetrieveService = advanceDetailRetrieveService;
            this._AdvanceDetailWriteService = advanceDetailWriteService;
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
                var advanceIds = data.AdvanceIds.Where(p => p.Contract_Type_Id == 2).Select(p => p.Advance_Id);
                var advances = this._AdvanceRetrieveService.Where(p => advanceIds.Contains(p.id)).ToList();
                advances.ForEach(p =>
                {
                    p.updated_at = DateTime.Now;
                    p.Enabled = data.IsPartial ? true : false;
                    p.Paid_Status = data.IsPartial ? (int)PrestaQiEnum.AdvanceStatus.PagadoParcial :
                        (int)PrestaQiEnum.AdvanceStatus.Pagado;
                });
                this._AdvanceWriteService.Update(advances);

                var advanceDetailIds = data.AdvanceIds.Where(p => p.Contract_Type_Id == 1).Select(p => p.Advance_Id);
                var advanceDetails = this._AdvanceDetailRetrieveService.Where(p => advanceIds.Contains(p.id)).ToList();
                advanceDetails.ForEach(p =>
                {
                    p.updated_at = DateTime.Now;
                    p.Enabled = data.IsPartial ? true : false;
                    p.Paid_Status = data.IsPartial ? (int)PrestaQiEnum.AdvanceStatus.PagadoParcial :
                        (int)PrestaQiEnum.AdvanceStatus.Pagado;
                });
                this._AdvanceDetailWriteService.Update(advanceDetails);

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

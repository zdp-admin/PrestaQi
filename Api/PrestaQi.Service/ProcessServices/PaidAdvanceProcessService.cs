using InsiscoCore.Base.Data;
using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using PrestaQi.Model;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Model.Enum;
using System;
using System.Linq;
using PrestaQi.Service.Tools;
using System.Collections.Generic;

namespace PrestaQi.Service.ProcessServices
{
    public class PaidAdvanceProcessService : ProcessService<PaidAdvance>
    {
        IRetrieveService<Advance> _AdvanceRetrieveService;
        IWriteService<Advance> _AdvanceWriteService;
        IWriteService<PaidAdvance> _PaidAdvanceWriteService;
        IRetrieveService<PaidAdvance> _PaidAdvanceRetrieveService;
        IRetrieveRepository<Accredited> _AcreditedRetrieveService;

        IRetrieveService<DetailsByAdvance> _DetailsByAdvance;
        IRetrieveService<DetailsAdvance> _DetailsAdvance;
        IWriteService<DetailsByAdvance> _DetailsByAdvanceWriteService;
        IWriteService<DetailsAdvance> _DetailsAdvanceWriteService;

        public PaidAdvanceProcessService(
            IRetrieveRepository<Accredited> acreditedRetrieveService,
            IRetrieveService<Advance> advanceRetrieveService,
            IWriteService<Advance> advanceWriteService,
            IWriteService<PaidAdvance> paidAdvanceWriteService,
            IRetrieveService<PaidAdvance> paidAdvanceRetrieveService,
            IRetrieveService<DetailsByAdvance> detailsByAdvance,
            IRetrieveService<DetailsAdvance> detailsAdvance,
            IWriteService<DetailsByAdvance> detailsByAdvanceWriteService,
            IWriteService<DetailsAdvance> detailsAdvanceWriteService
        ) {
            this._AcreditedRetrieveService = acreditedRetrieveService;
            this._AdvanceRetrieveService = advanceRetrieveService;
            this._AdvanceWriteService = advanceWriteService;
            this._PaidAdvanceWriteService = paidAdvanceWriteService;
            this._PaidAdvanceRetrieveService = paidAdvanceRetrieveService;
            this._DetailsByAdvance = detailsByAdvance;
            this._DetailsAdvance = detailsAdvance;
            this._DetailsByAdvanceWriteService = detailsByAdvanceWriteService;
            this._DetailsAdvanceWriteService = detailsAdvanceWriteService;
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

            var advancesUpdate = new List<Advance>();
            var advances = this._AdvanceRetrieveService.Where(a => a.Paid_Status == 0).ToList();
            var advanceIds = advances.Select(a => a.id).ToList();

            var details = this._DetailsAdvance.Where(d => d.Date_Payment.Date < DateTime.Now.Date).Where(d => advanceIds.Contains(d.Advance_Id)).ToList();
            var detailsForPay = this._DetailsAdvance.Where(d => d.Paid_Status == 0).Where(d => advanceIds.Contains(d.Advance_Id)).ToList();

            foreach (Advance advance in advances)
            {
                advance.DetailsAdvances = detailsForPay.Where(d => d.Advance_Id == advance.id).Where(d => d.Date_Payment.Date < DateTime.Now.Date).ToList();

                if (advance.DetailsAdvances.Count <= 0 && advance.Limit_Date.Date < DateTime.Now.Date)
                {
                    advance.Paid_Status = 1;
                    advancesUpdate.Add(advance);
                }
            }

            this._AdvanceWriteService.Update(advancesUpdate);

            foreach (DetailsAdvance detailsAdvance in details)
            {
                detailsAdvance.Paid_Status = 1;
            }

            this._DetailsAdvanceWriteService.Update(details);

            return true;
        }

        private DateTime nextDatePayment(int accreditedId)
        {
            var date = DateTime.Now;
            var now = DateTime.Now;

            var accredited = this._AcreditedRetrieveService.Find(accreditedId);

            if (accredited is null)
            {
                return date;
            }

            var periods = Utilities.getPeriodoByAccredited(accredited, new DateTime(2021, 05, 28));

            return periods.finish;
        }
    }
}

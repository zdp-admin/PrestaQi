using InsiscoCore.Base.Data;
using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using PrestaQi.Model;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Model.Enum;
using System;
using System.Linq;
using PrestaQi.Service.Tools;

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

            bool createPay = true; // this._PaidAdvanceWriteService.Create(paidAdvance);

            if (createPay)
            {
                var advanceIds = data.AdvanceIds.Where(p => p.Contract_Type_Id == 1).Select(p => p.Advance_Id);
                var advances = this._AdvanceRetrieveService.Where(p => advanceIds.Contains(p.id)).ToList();
                var detailsAdvanceAll = this._DetailsAdvance.Where(da => advanceIds.Contains(da.Advance_Id)).ToList();
                var detailsByAdvances = this._DetailsByAdvance.Where(da => advanceIds.Contains(da.Advance_Id)).ToList();

                advances.ForEach(p =>
                {
                    var nextDayForPayment = nextDatePayment(p.Accredited_Id);

                    p.updated_at = DateTime.Now;

                    p.details = detailsByAdvances.Where(da => da.Advance_Id == p.id).ToList();

                    if (p.details.Count <= 0)
                    {
                        if (p.Limit_Date <= nextDayForPayment)
                        {
                            p.Enabled = data.IsPartial ? true : false;
                            p.Paid_Status = data.IsPartial ? (int)PrestaQiEnum.AdvanceStatus.PagadoParcial :
                                (int)PrestaQiEnum.AdvanceStatus.Pagado;
                        }
                    } else
                    {
                        p.details.ForEach(d =>
                        {
                            d.Detail = detailsAdvanceAll.Where(da => da.id == d.Detail_Id && da.Date_Payment <= nextDayForPayment).FirstOrDefault();

                            if (d.Detail != null)
                            {
                                d.Detail.Paid_Status = data.IsPartial ? (int)PrestaQiEnum.AdvanceStatus.PagadoParcial :
                                        (int)PrestaQiEnum.AdvanceStatus.Pagado;

                                this._DetailsAdvanceWriteService.Update(d.Detail);
                            }
                        });

                        bool allpaided = p.details.All(d => d.Detail is null ? false : d.Detail.Paid_Status != 0);

                        if (allpaided)
                        {
                            p.Enabled = data.IsPartial ? true : false;
                            p.Paid_Status = data.IsPartial ? (int)PrestaQiEnum.AdvanceStatus.PagadoParcial :
                                (int)PrestaQiEnum.AdvanceStatus.Pagado;
                        }
                    }
                });

                this._AdvanceWriteService.Update(advances);

                
                advanceIds = data.AdvanceIds.Where(p => p.Contract_Type_Id == 2).Select(p => p.Advance_Id);
                advances = this._AdvanceRetrieveService.Where(p => advanceIds.Contains(p.id)).ToList();

                advances.ForEach(p =>
                {
                    var nextDayForPayment = nextDatePayment(p.Accredited_Id);

                    if (p.Limit_Date <= nextDayForPayment)
                    {
                        p.updated_at = DateTime.Now;
                        p.Enabled = data.IsPartial ? true : false;
                        p.Paid_Status = data.IsPartial ? (int)PrestaQiEnum.AdvanceStatus.PagadoParcial :
                            (int)PrestaQiEnum.AdvanceStatus.Pagado;
                    }

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

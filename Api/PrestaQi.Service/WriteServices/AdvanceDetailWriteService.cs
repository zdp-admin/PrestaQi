using InsiscoCore.Base.Data;
using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using PrestaQi.Model;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Model.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrestaQi.Service.WriteServices
{
    public class AdvanceDetailWriteService : WriteService<AdvanceDetail>
    {
        IRetrieveService<AdvanceDetail> _AdvanceDetailRetrieveService;

        public AdvanceDetailWriteService(IWriteRepository<AdvanceDetail> repository,
             IRetrieveService<AdvanceDetail> advanceDetailRetrieveService) : base(repository)
        {
            this._AdvanceDetailRetrieveService = advanceDetailRetrieveService;
        }

        public bool Update(SetPayAdvance setPayAdvance)
        {
            var advanceDetail = this._AdvanceDetailRetrieveService.Where(p => setPayAdvance.AdvanceIds.Contains(p.id)).ToList();

            advanceDetail.ForEach(p =>
            {
                p.updated_at = DateTime.Now;
                p.Enabled = setPayAdvance.IsPartial ? true : false;
                p.Paid_Status = setPayAdvance.IsPartial ? (int)PrestaQiEnum.AdvanceStatus.PagadoParcial :
                    (int)PrestaQiEnum.AdvanceStatus.Pagado;
            });

            return base.Update(advanceDetail);
        }
    }
}

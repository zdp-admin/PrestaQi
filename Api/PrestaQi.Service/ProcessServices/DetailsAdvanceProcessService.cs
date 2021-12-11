using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using PrestaQi.Model;
using PrestaQi.Model.Dto.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrestaQi.Service.ProcessServices
{
    public class DetailsAdvanceProcessService : ProcessService<DetailsAdvance>
    {
        IWriteService<DetailsAdvance> _DetailAdvanceWriteService;
        IWriteService<DetailsByAdvance> _DetailByAdvanceWriteService;
        IRetrieveService<DetailsAdvance> _DetailAdvanceRetrieveService;
        IRetrieveService<DetailsByAdvance> _DetailByAdvanceRetrieveService;

        public DetailsAdvanceProcessService(
            IWriteService<DetailsByAdvance> detailsByAdvanceWrite,
            IWriteService<DetailsAdvance> detailsAdvanceWrite,
            IRetrieveService<DetailsAdvance> detailAdvanceRetrieveService,
            IRetrieveService<DetailsByAdvance> detailByAdvanceRetrieveService
        ) {
            this._DetailAdvanceWriteService = detailsAdvanceWrite;
            this._DetailByAdvanceWriteService = detailsByAdvanceWrite;
            this._DetailAdvanceRetrieveService = detailAdvanceRetrieveService;
            this._DetailByAdvanceRetrieveService = detailByAdvanceRetrieveService;
        }

        public bool ExecuteProcess(CustomInsertDetails details)
        {
            var deleteAdvanceIds = details.Details.Select(d => d.Advance_Id).ToList();

            var forDeteleAdvance = this._DetailAdvanceRetrieveService.Where(d => deleteAdvanceIds.Contains(d.Advance_Id)).ToList();

            var idsForsDetele = forDeteleAdvance.Select(d => d.id).ToList();
            var forDeteleteByAdvances = this._DetailByAdvanceRetrieveService.Where(d => idsForsDetele.Contains(d.Detail_Id)).ToList();

            this._DetailByAdvanceWriteService.Delete(forDeteleteByAdvances);

            this._DetailAdvanceWriteService.Delete(forDeteleAdvance);

            foreach (DetailsAdvance detail in details.Details)
            {
                this._DetailAdvanceWriteService.Create(detail);

                this._DetailByAdvanceWriteService.Create(new DetailsByAdvance() {
                    Advance_Id = detail.Advance_Id,
                    amount = detail.Total_Payment,
                    created_at = DateTime.Now,
                    Detail_Id = detail.id,
                    updated_at = DateTime.Now
                });
            }

            return true;
        }
    }
}

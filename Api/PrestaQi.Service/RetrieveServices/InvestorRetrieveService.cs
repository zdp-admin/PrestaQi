using InsiscoCore.Base.Data;
using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using PrestaQi.Model;
using PrestaQi.Model.Configurations;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Model.Dto.Output;
using PrestaQi.Model.Enum;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace PrestaQi.Service.RetrieveServices
{
    public class InvestorRetrieveService : RetrieveService<Investor>
    {
        IRetrieveService<Period> _PeriodRetrieveService;
        IRetrieveService<Capital> _CapitalRetrieveService;
        IRetrieveService<Institution> _InstitutionRetrieveService;

        public InvestorRetrieveService(
            IRetrieveRepository<Investor> repository,
            IRetrieveService<Capital> capitalRetrieveService,
            IRetrieveService<Period> periodRetrieveService,
            IRetrieveService<Institution> institutionRetrieveService
            ) : base(repository)
        {
            this._CapitalRetrieveService = capitalRetrieveService;
            this._PeriodRetrieveService = periodRetrieveService;
            this._InstitutionRetrieveService = institutionRetrieveService;
        }

        public override IEnumerable<Investor> Where(Func<Investor, bool> predicate)
        {
            var list = this._Repository.Where(predicate).ToList();
            var institutions = this._InstitutionRetrieveService.Where(p => true).ToList();
            var periods = this._PeriodRetrieveService.Where(p => true).ToList();

            list.ForEach(item =>
            {
                item.Institution_Name = institutions.FirstOrDefault(p => p.id == item.Institution_Id).Description;

                if (item.Capitals != null)
                {
                    item.Capitals.ForEach(p =>
                    {
                        p.Period_Name = periods.FirstOrDefault(period => period.id == p.period_id).Description;
                        p.Enabled = p.End_Date.Date <= DateTime.Now.Date ? true : false;
                    });
                }
            });

            return list;

        }

        public List<InvestorData> RetrieveResult(InvestorByDate investorByDate)
        {
            if (investorByDate.Page == 0 || investorByDate.NumRecord == 0)
            {
                investorByDate.Page = 1;
                investorByDate.NumRecord = 20;
            }

            List<InvestorData> investorDatas = new List<InvestorData>();
            var periods = this._PeriodRetrieveService.Where(p => p.Enabled == true).ToList();
            var institutions = this._InstitutionRetrieveService.Where(p => true).ToList();

            var list = this._Repository.Where(p => p.Start_Date_Prestaqi.Date >= investorByDate.Start_Date.Date &&
                p.Start_Date_Prestaqi.Date <= investorByDate.End_Date).Skip(investorByDate.Page).Take(investorByDate.NumRecord).ToList();

            if (list.Count > 0)
            {
                investorDatas = list.Select(p => new InvestorData()
                {
                    Id = p.id,
                    Commited_Amount = p.Total_Amount_Agreed,
                    NameComplete = $"{p.First_Name} {p.Last_Name}",
                    Limit_Date = p.Limit_Date
                }).ToList();


                investorDatas.ForEach(p =>
                {
                    var capitals = this._CapitalRetrieveService.Where(c => c.investor_id == p.Id).OrderBy(p => p.Start_Date).ToList();

                    p.AmountExercised = capitals.Count > 0 ? capitals.Sum(p => p.Amount) : 0;
                    p.CapitalDatas = capitals.Select(c => new CapitalData()
                    {
                        Amount = c.Amount,
                        Capital_Id = c.id,
                        End_Date = c.End_Date,
                        Start_Date = c.Start_Date,
                        File = c.Files,
                        Interest_Rate = c.Interest_Rate,
                        Period = periods.Find(item => item.id == c.period_id).Description,
                        Capital_Status = ((PrestaQiEnum.CapitalEnum)c.Capital_Status).ToString(),
                        Investment_Status = "Prueba"

                    }).ToList();
                });
            }

            return investorDatas;
        }
    }
}

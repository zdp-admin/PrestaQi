using InsiscoCore.Base.Data;
using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using PrestaQi.Model;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Model.Dto.Output;
using PrestaQi.Model.Enum;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PrestaQi.Service.RetrieveServices
{
    public class AccreditedRetrieveService : RetrieveService<Accredited>
    {
        IRetrieveService<Period> _PeriodRetrieveService;
        IRetrieveService<Company> _CompanyRetrieveService;
        IRetrieveService<Advance> _AdvanceRetrieveService;
        IRetrieveService<Institution> _InsitutionRetrieveService;

        public AccreditedRetrieveService(
            IRetrieveRepository<Accredited> repository,
            IRetrieveService<Period> periodRetrieveService,
            IRetrieveService<Company> companyRetrieveService,
            IRetrieveService<Advance> advanceRetrieveService,
            IRetrieveService<Institution> insitutionRetrieveService
            ) : base(repository)
        {
            this._PeriodRetrieveService = periodRetrieveService;
            this._CompanyRetrieveService = companyRetrieveService;
            this._AdvanceRetrieveService = advanceRetrieveService;
            this._InsitutionRetrieveService = insitutionRetrieveService;
        }

        public override IEnumerable<Accredited> Where(Func<Accredited, bool> predicate)
        {
            var list = this._Repository.Where(predicate).ToList();
            return GetList(list);
        }

        public AccreditedPagination RetrieveResult(AccreditedByPagination accreditedByPagination)
        {
            int totalRecord = 0;
            if (accreditedByPagination.Page == 0 || accreditedByPagination.NumRecord == 0)
            {
                accreditedByPagination.Page = 1;
                accreditedByPagination.NumRecord = 20;
            }

            List<Accredited> accrediteds = new List<Accredited>();

            if (accreditedByPagination.Type == 0)
            {
                if (string.IsNullOrEmpty(accreditedByPagination.Filter))
                {
                    totalRecord = this._Repository.Where(p => p.Deleted_At == null).OrderBy(p => p.id).ToList().Count;
                    accrediteds = this._Repository.Where(p => p.Deleted_At == null).OrderBy(p => p.id).Skip((accreditedByPagination.Page - 1) * accreditedByPagination.NumRecord).Take(accreditedByPagination.NumRecord).ToList();
                }
                else
                    accrediteds = this._Repository.Where(p => p.Deleted_At == null).OrderBy(p => p.id).ToList();
            }
            else
            {
                accrediteds = this._Repository.Where(p => p.Deleted_At == null).OrderBy(p => p.id).ToList();
            }

            accrediteds = GetList(accrediteds).ToList();
            
            if (accreditedByPagination.Type == 0 && accreditedByPagination.Filter.Length > 0)
            {
                var stringProperties = typeof(Accredited).GetProperties().Where(prop =>
                    prop.PropertyType == accreditedByPagination.Filter.GetType());

                totalRecord = accrediteds
                        .Where(p => stringProperties.Any(prop => prop.GetValue(p, null) != null && prop.GetValue(p, null).ToString().ToLower().Contains(accreditedByPagination.Filter.ToLower())))
                        .OrderBy(p => p.id).ToList().Count;

                accrediteds = accrediteds
                        .Where(p => stringProperties.Any(prop => prop.GetValue(p, null) != null && prop.GetValue(p, null).ToString().ToLower().Contains(accreditedByPagination.Filter.ToLower())))
                        .OrderBy(p => p.id)
                        .Skip((accreditedByPagination.Page - 1) * accreditedByPagination.NumRecord)
                        .Take(accreditedByPagination.NumRecord).ToList();
            }

            return new AccreditedPagination() { Accrediteds = accrediteds, TotalRecord = totalRecord };
        }  

        IEnumerable<Accredited> GetList(List<Accredited> list)
        {
            
            var periods = this._PeriodRetrieveService.Where(p => p.User_Type == 2).ToList();
            var companies = this._CompanyRetrieveService.Where(p => true).ToList();
            var institutions = this._InsitutionRetrieveService.Where(p => true).ToList();

            list.ForEach(p =>
            {
                p.Institution_Name = institutions.FirstOrDefault(institution => institution.id == p.Institution_Id).Description;
                p.Period_Name = periods.FirstOrDefault(period => period.id == p.Period_Id).Description;
                p.Company_Name = companies.FirstOrDefault(company => company.id == p.Company_Id).Description;
                p.Advances = this._AdvanceRetrieveService.Where(advace => advace.Accredited_Id == p.id && (advace.Paid_Status == 0 || advace.Paid_Status == 2)).ToList();
                p.Type = (int)PrestaQiEnum.UserType.Acreditado;
                p.TypeName = PrestaQiEnum.UserType.Acreditado.ToString();
            });

            return list;
        }

        public override Accredited Find(object id)
        {
            var accredited = base.Find(id);
            accredited = GetList(new List<Accredited>() { accredited }).FirstOrDefault();

            return accredited;
        }
    }
}

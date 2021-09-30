﻿using InsiscoCore.Base.Data;
using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using OpenXmlPowerTools;
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
        IRetrieveService<TypeContract> _TypeContractService;
        IProcessService<Advance> _AdvanceProcessService;
        IRetrieveService<Configuration> _ConfigurationRetrieveService;
        IRetrieveService<AdvanceDetail> _AdvanceDetailRetrieveService;
        IRetrieveService<DetailsAdvance> _DetailsAdvance;
        IRetrieveService<DetailsByAdvance> _DetailsByAdvance;
        IRetrieveService<SelfieUser> _SelfieUser;
        IRetrieveService<PaySheetUser> _PaySheetUser;
        IRetrieveService<StatusAccount> _StatusAccount;
        IRetrieveService<IneAccount> _IneAccount;

        public AccreditedRetrieveService(
            IRetrieveRepository<Accredited> repository,
            IRetrieveService<Period> periodRetrieveService,
            IRetrieveService<Company> companyRetrieveService,
            IRetrieveService<Advance> advanceRetrieveService,
            IRetrieveService<Institution> insitutionRetrieveService,
            IRetrieveService<TypeContract> typeContractService,
            IProcessService<Advance> advanceProcessService,
            IRetrieveService<Configuration> configurationRetrieveService,
            IRetrieveService<DetailsAdvance> detailsAdvance,
            IRetrieveService<AdvanceDetail> advanceDetailRetrieveService,
            IRetrieveService<DetailsByAdvance> detailsByAvance,
            IRetrieveService<SelfieUser> selfieUser,
            IRetrieveService<PaySheetUser> paySheetUser,
            IRetrieveService<StatusAccount> statusAccount,
            IRetrieveService<IneAccount> ineAccount
            ) : base(repository)
        {
            this._PeriodRetrieveService = periodRetrieveService;
            this._CompanyRetrieveService = companyRetrieveService;
            this._AdvanceRetrieveService = advanceRetrieveService;
            this._InsitutionRetrieveService = insitutionRetrieveService;
            this._TypeContractService = typeContractService;
            this._AdvanceProcessService = advanceProcessService;
            this._ConfigurationRetrieveService = configurationRetrieveService;
            this._AdvanceDetailRetrieveService = advanceDetailRetrieveService;
            this._DetailsAdvance = detailsAdvance;
            this._DetailsByAdvance = detailsByAvance;
            this._SelfieUser = selfieUser;
            this._PaySheetUser = paySheetUser;
            this._StatusAccount = statusAccount;
            this._IneAccount = ineAccount;
        }

        public override IEnumerable<Accredited> Where(Func<Accredited, bool> predicate)
        {
            var list = this._Repository.Where(predicate).ToList();
            return GetList(list);
        }

        public AccreditedPagination RetrieveResult(LicenseByFilter licenseByFilter)
        {
            int totalRecord = 0;
            if (licenseByFilter.Page == 0 || licenseByFilter.NumRecord == 0)
            {
                licenseByFilter.Page = 1;
                licenseByFilter.NumRecord = 20;
            }

            List<Accredited> accrediteds = new List<Accredited>();

            accrediteds = this._Repository.Where(p => p.Deleted_At == null && p.License_Id == licenseByFilter.Id).OrderBy(p => p.id).ToList();

            accrediteds = GetList(accrediteds).ToList();

            if (licenseByFilter.Filter != null && licenseByFilter.Filter.Length > 0)
            {
                var stringProperties = typeof(Accredited).GetProperties().Where(prop =>
                    prop.PropertyType == licenseByFilter.Filter.GetType());

                totalRecord = accrediteds
                        .Where(p => stringProperties.Any(prop => prop.GetValue(p, null) != null && prop.GetValue(p, null).ToString().ToLower().Contains(licenseByFilter.Filter.ToLower())))
                        .OrderBy(p => p.id).ToList().Count;

                accrediteds = accrediteds
                        .Where(p => stringProperties.Any(prop => prop.GetValue(p, null) != null && prop.GetValue(p, null).ToString().ToLower().Contains(licenseByFilter.Filter.ToLower())))
                        .OrderBy(p => p.id)
                        .Skip((licenseByFilter.Page - 1) * licenseByFilter.NumRecord)
                        .Take(licenseByFilter.NumRecord).ToList();
            }

            accrediteds.ForEach(a =>
            {
                var detailsAdvanceAll = this._DetailsAdvance.Where(da => da.Accredited_Id == a.id).ToList();
                a.Advances.ForEach(advance =>
                {
                    advance.details = this._DetailsByAdvance.Where(da => da.Advance_Id == advance.id).OrderBy(da => da.Detail_Id).ToList();
                    if (advance.details.Count > 0)
                    {
                        advance.details.ForEach(d =>
                        {
                            d.Detail = detailsAdvanceAll.Where(da => da.id == d.Detail_Id).FirstOrDefault();
                        });
                    }
                });
            });

            return new AccreditedPagination() { Accrediteds = accrediteds, TotalRecord = totalRecord };
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

            accrediteds.ForEach(a =>
            {
                var detailsAdvanceAll = this._DetailsAdvance.Where(da => da.Accredited_Id == a.id).ToList();
                a.Advances.ForEach(advance =>
                {
                    advance.details = this._DetailsByAdvance.Where(da => da.Advance_Id == advance.id).OrderBy(da => da.Detail_Id).ToList();
                    if (advance.details.Count > 0)
                    {
                        advance.details.ForEach(d =>
                        {
                            d.Detail = detailsAdvanceAll.Where(da => da.id == d.Detail_Id).FirstOrDefault();
                        });
                    }
                });
            });

            return new AccreditedPagination() { Accrediteds = accrediteds, TotalRecord = totalRecord };
        }  

        public AccreditedPagination RetrieveResult(AccreditedExternalByPagination accreditedByPagination)
        {
            int totalRecord = 0;
            if (accreditedByPagination.Page == 0 || accreditedByPagination.NumRecord == 0)
            {
                accreditedByPagination.Page = 1;
                accreditedByPagination.NumRecord = 20;
            }

            List<Accredited> accrediteds = new List<Accredited>();

            if (accreditedByPagination.Filter.Length > 0)
            {
                var stringProperties = typeof(Accredited).GetProperties().Where(prop =>
                    prop.PropertyType == accreditedByPagination.Filter.GetType());

                totalRecord = accrediteds
                        .Where(p => !p.ApprovedDocuments && p.External && stringProperties.Any(prop => prop.GetValue(p, null) != null && prop.GetValue(p, null).ToString().ToLower().Contains(accreditedByPagination.Filter.ToLower())))
                        .OrderBy(p => p.id).ToList().Count;

                accrediteds = accrediteds
                        .Where(p => !p.ApprovedDocuments && p.External && stringProperties.Any(prop => prop.GetValue(p, null) != null && prop.GetValue(p, null).ToString().ToLower().Contains(accreditedByPagination.Filter.ToLower())))
                        .OrderBy(p => p.id)
                        .Skip((accreditedByPagination.Page - 1) * accreditedByPagination.NumRecord)
                        .Take(accreditedByPagination.NumRecord).ToList();
            } else
            {
                totalRecord = this._Repository.Where(p => p.External && p.Deleted_At == null && !p.ApprovedDocuments).OrderBy(p => p.id).ToList().Count;
                accrediteds = this._Repository.Where(p => p.External && p.Deleted_At == null && !p.ApprovedDocuments).OrderBy(p => p.id).Skip((accreditedByPagination.Page - 1) * accreditedByPagination.NumRecord).Take(accreditedByPagination.NumRecord).ToList();
            }

            var s = this._PaySheetUser.Where((paysheet) => paysheet.AccreditedId == 210).FirstOrDefault();

            accrediteds.ForEach(a =>
            {
                a.Selfie = this._SelfieUser.Where((selfie) => selfie.AccreditedId == a.id).OrderBy(selfie => selfie.created_at).FirstOrDefault();
                a.PaySheet = this._PaySheetUser.Where((paysheet) => paysheet.AccreditedId == a.id).OrderBy(selfie => selfie.created_at).FirstOrDefault();
                a.StatusAccount = this._StatusAccount.Where((statusAccount) => statusAccount.AccreditedId == a.id).OrderBy(selfie => selfie.created_at).FirstOrDefault();
                a.IneAccount = this._IneAccount.Where((ine) => ine.AccreditedId == a.id).OrderBy(selfie => selfie.created_at).FirstOrDefault();
            });

            return new AccreditedPagination() { Accrediteds = accrediteds, TotalRecord = totalRecord };

        }

        IEnumerable<Accredited> GetList(List<Accredited> list)
        {
            var configurations = this._ConfigurationRetrieveService.Where(p => true).ToList();
            var periods = this._PeriodRetrieveService.Where(p => p.User_Type == 2).ToList();
            var companies = this._CompanyRetrieveService.Where(p => true).ToList();
            var institutions = this._InsitutionRetrieveService.Where(p => true).ToList();
            var typeContracts = this._TypeContractService.Where(p => true).ToList();

            double gross_percentage = Convert.ToDouble(configurations.Find(p => p.Configuration_Name == "GROSS_MONTHLY_SALARY_PERCENTAGE").Configuration_Value) / 100;
            double net_percentage = Convert.ToDouble(configurations.Find(p => p.Configuration_Name == "NET_MONTHLY_SALARY_PERCENTAGE").Configuration_Value) / 100;
            
            list.ForEach(p =>
            {
                p.Institution_Name = institutions.FirstOrDefault(institution => institution.id == p.Institution_Id).Description;
                p.Period_Name = periods.FirstOrDefault(period => period.id == p.Period_Id).Description;
                p.Company_Name = companies.FirstOrDefault(company => company.id == p.Company_Id).Description;
                if (p.Outsourcing_id != null)
                {
                    p.Outsourcing_Name = companies.FirstOrDefault(company => company.id == p.Outsourcing_id).Description;
                }
                p.Advances = this._AdvanceRetrieveService.Where(advace => advace.Accredited_Id == p.id && (advace.Paid_Status == 0 || advace.Paid_Status == 2)).ToList();
                p.Type = (int)PrestaQiEnum.UserType.Acreditado;
                p.TypeName = PrestaQiEnum.UserType.Acreditado.ToString();
                p.TypeContract = typeContracts.FirstOrDefault(tc => tc.id == p.Type_Contract_Id);
                p.Credit_Limit = this._AdvanceProcessService.ExecuteProcess<CalculateAmount, AdvanceAndDetails>(new CalculateAmount()
                {
                    Accredited_Id = p.id
                }).advance.Maximum_Amount;

                if (p.Type_Contract_Id == (int)PrestaQiEnum.AccreditedContractType.WagesAndSalaries)
                {                    
                    p.AdvanceDetails = this._AdvanceDetailRetrieveService.Where(detail => detail.Accredited_Id == p.id && (detail.Paid_Status == 0 || detail.Paid_Status == 2)).ToList();
                    p.Advance_Autorhized_Amount = Math.Round(p.Gross_Monthly_Salary * gross_percentage, 2);
                    p.Advance_Via_Payroll = Math.Round(p.Net_Monthly_Salary * net_percentage, 2);
                    p.Authorized_Advance_After_Obligations = Math.Round(p.Advance_Autorhized_Amount - p.Other_Obligations, 2);
                    p.Payroll_Advance_Authorized_After_Obligations = Math.Round(p.Advance_Via_Payroll - p.Other_Obligations, 2);
                }
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

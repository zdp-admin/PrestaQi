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

namespace PrestaQi.Service.ProcessServices
{
    public class AccreditedProcessService : ProcessService<Accredited>
    {
        IRetrieveService<Company> _CompanyRetrieveService;
        IRetrieveService<Accredited> _AccreditedRetrieveService;
        IRetrieveService<Advance> _AdvanceRetrieveService;
        IRetrieveService<PaidAdvance> _PaidAdvanceRetrieveService;
        IRetrieveService<Configuration> _ConfigurationRetrieveService;
        IRetrieveService<DetailsAdvance> _DetailsAdvance;
        IRetrieveService<DetailsByAdvance> _DetailsByAdvance;
        IWriteService<Accredited> _accreditedWriteService;
        IWriteService<IneAccount> _IneAccountWriteService;
        IWriteService<SelfieUser> _SelfieUserWriteService;
        IWriteService<StatusAccount> _StatusAccountWriteService;
        IWriteService<PaySheetUser> _PaySheetUserWriteService;
        IWriteService<Notification> _NotificationWriteService;
        IRetrieveService<IneAccount> _IneAccountRetrieveService;
        IRetrieveService<SelfieUser> _SelfieUserRetrieveService;
        IRetrieveService<StatusAccount> _StatusAccountRetrieveService;
        IRetrieveService<PaySheetUser> _PaySheetUserRetrieveService;
        IProcessService<Notification> _NotificationProcessService;


        public AccreditedProcessService(
            IRetrieveService<Company> companyRetrieveService,
            IRetrieveService<Accredited> accreditedRetrieveService,
            IRetrieveService<Advance> advanceRetrieveService,
            IRetrieveService<PaidAdvance> paidAdvanceRetrieveService,
            IRetrieveService<Configuration> configurationRetrieveService,
            IRetrieveService<DetailsAdvance> detailsAdvance,
            IRetrieveService<DetailsByAdvance> detailsByAvance,
            IWriteService<Accredited> accreditedWriteService,
            IWriteService<IneAccount> ineAccountWriteService,
            IWriteService<SelfieUser> selfieUserWriteService,
            IWriteService<StatusAccount> statusAccountWriteService,
            IWriteService<PaySheetUser> paySheetUserWriteService,
            IRetrieveService<IneAccount> ineAccountRetrieveService,
            IRetrieveService<SelfieUser> selfieUserRetrieveService,
            IRetrieveService<StatusAccount> statusAccountRetrieveService,
            IRetrieveService<PaySheetUser> paySheetUserRetrieveService,
            IProcessService<Notification> notificationProcessService,
            IWriteService<Notification> notificationWriteService
            )
        {
            this._CompanyRetrieveService = companyRetrieveService;
            this._AccreditedRetrieveService = accreditedRetrieveService;
            this._AdvanceRetrieveService = advanceRetrieveService;
            this._PaidAdvanceRetrieveService = paidAdvanceRetrieveService;
            this._ConfigurationRetrieveService = configurationRetrieveService;
            this._DetailsAdvance = detailsAdvance;
            this._DetailsByAdvance = detailsByAvance;
            this._accreditedWriteService = accreditedWriteService;
            this._IneAccountWriteService = ineAccountWriteService;
            this._SelfieUserWriteService = selfieUserWriteService;
            this._StatusAccountWriteService = statusAccountWriteService;
            this._PaySheetUserWriteService = paySheetUserWriteService;
            this._IneAccountRetrieveService = ineAccountRetrieveService;
            this._SelfieUserRetrieveService = selfieUserRetrieveService;
            this._StatusAccountRetrieveService = statusAccountRetrieveService;
            this._PaySheetUserRetrieveService = paySheetUserRetrieveService;
            this._NotificationProcessService = notificationProcessService;
            this._NotificationWriteService = notificationWriteService;
        }

        public List<AdvanceReceivable> ExecuteProcess(AdvancesReceivableByFilter filter)
        {
            List<Accredited> accrediteds = new List<Accredited>();
            List<Company> companies = new List<Company>();

            int finantialDay = Convert.ToInt32(this._ConfigurationRetrieveService.Where(p => p.Configuration_Name == "FINANCIAL_DAYS").FirstOrDefault().Configuration_Value);
            double vat = Convert.ToDouble(this._ConfigurationRetrieveService.Where(p => p.Configuration_Name == "VAT").FirstOrDefault().Configuration_Value) / 100;
            var advances = this._AdvanceRetrieveService.Where(p => p.Paid_Status == 0 || p.Paid_Status == 2).ToList();
            var accreditIds = advances.Select(p => p.Accredited_Id).Distinct();

            if (filter.LicenseId > 0)
            {
                accrediteds = this._AccreditedRetrieveService.Where(p => accreditIds.Contains(p.id) && p.Deleted_At == null && p.License_Id == filter.LicenseId).ToList();
            } else
            {
                accrediteds = this._AccreditedRetrieveService.Where(p => accreditIds.Contains(p.id) && p.Deleted_At == null && p.License_Id is null).ToList();
            }


            if (!string.IsNullOrEmpty(filter.Filter))
            {
                accrediteds = accrediteds.Where(p => accreditIds.Contains(p.id) && p.Deleted_At == null &&
                (p.First_Name.ToLower().Contains(filter.Filter.ToLower()) || p.Last_Name.ToLower().Contains(filter.Filter.ToLower()) ||
                p.Contract_number.ToLower().Contains(filter.Filter.ToLower()) ||
                p.Company_Name.ToLower().Contains(filter.Filter.ToLower()))).ToList();
            }

            var companyIds = accrediteds.Select(p => p.Company_Id).Distinct();
            companies = this._CompanyRetrieveService.Where(p => companyIds.Contains(p.id)).ToList();

            var detail = accrediteds.Select(accredited => new AdvanceReceivableAccredited()
            {
                Accredited_Id = accredited.id,
                Company_Id = accredited.Company_Id,
                Advances = advances.Where(p => p.Accredited_Id == accredited.id).ToList(),
                Id = accredited.Identify,
                Interest_Rate = accredited.Interest_Rate,
                Moratoruim_Interest_Rate = accredited.Moratoruim_Interest_Rate,
                NameComplete = $"{accredited.First_Name} {accredited.Last_Name}",
                Is_Blocked = accredited.Is_Blocked,
                TypeContractId = (int)accredited.Type_Contract_Id,
                Period_Id = accredited.Period_Id
            }).ToList();

            detail.ForEach(accredited =>
            {
                if (accredited.TypeContractId == (int)PrestaQiEnum.AccreditedContractType.AssimilatedToSalary)
                {
                    accredited.Advances.ForEach(advance =>
                    {
                        advance.Day_Moratorium = DateTime.Now.Date > advance.Limit_Date.Date ?
                            (DateTime.Now.Date - advance.Limit_Date).Days : 0;

                        advance.Interest_Moratorium = DateTimeOffset.Now.Date > advance.Limit_Date.Date ?
                        Math.Round(advance.Amount * (((double)accredited.Moratoruim_Interest_Rate / 100) / finantialDay) * advance.Day_Moratorium, 2) :
                        0;
                        advance.Subtotal = advance.Interest + advance.Interest_Moratorium + advance.Comission + advance.Promotional_Setting;
                        advance.Vat = Math.Round(advance.Subtotal * vat, 2);
                        advance.Total_Withhold = Math.Round(advance.Amount + advance.Subtotal + advance.Vat, 2);
                    });

                    accredited.Payment = Math.Round(accredited.Advances.Sum(p => (p == null ? 0 : p.Total_Withhold)), 2);
                }
                else
                {
                    var totalPayment = 0.0;
                    var nextDate = DateTime.Now;

                    if (accredited.Period_Id == (int)PrestaQiEnum.PerdioAccredited.Semanal)
                    {
                        var day = nextDate.Day;

                        if (day > 0)
                        {
                            nextDate = nextDate.AddDays(6 - day);
                        }
                    }

                    if (accredited.Period_Id == (int)PrestaQiEnum.PerdioAccredited.Mensual)
                    {
                        var dayInMonth = DateTime.DaysInMonth(nextDate.Year, nextDate.Month);
                        nextDate = new DateTime(nextDate.Year, nextDate.Month, dayInMonth);
                    }

                    if (accredited.Period_Id == (int)PrestaQiEnum.PerdioAccredited.Quincenal)
                    {
                        if (nextDate.Day >= 1 && nextDate.Day <= 15)
                        {
                            nextDate = new DateTime(nextDate.Year, nextDate.Month, 15);
                        } else
                        {
                            var dayInMonth = DateTime.DaysInMonth(nextDate.Year, nextDate.Month);
                            nextDate = new DateTime(nextDate.Year, nextDate.Month, dayInMonth);
                        }
                    }

                    var detailsAdvanceAll = this._DetailsAdvance.Where(da => da.Accredited_Id == accredited.Accredited_Id).ToList();

                    accredited.Advances.ForEach(advance =>
                    {
                        advance.details = this._DetailsByAdvance.Where(da => da.Advance_Id == advance.id).OrderBy(da => da.Detail_Id).ToList();

                        if (advance.details.Count > 0)
                        {
                            advance.details.ForEach(d =>
                            {
                                d.Detail = detailsAdvanceAll.Where(da => da.id == d.Detail_Id).FirstOrDefault();
                                if (d.Detail != null)
                                {
                                    d.Detail.Total_Payment += double.Parse((d.Detail.Promotional_Setting ?? 0).ToString());
                                    if (d.Detail.Date_Payment <= nextDate && d.Detail.Paid_Status != (int)PrestaQiEnum.AdvanceStatus.Pagado)
                                    {
                                        totalPayment += d.Detail.Total_Payment;
                                    }
                                }
                            });
                        } else
                        {
                            advance.Day_Moratorium = DateTime.Now.Date > advance.Limit_Date.Date ?
                            (DateTime.Now.Date - advance.Limit_Date).Days : 0;

                            advance.Interest_Moratorium = DateTimeOffset.Now.Date > advance.Limit_Date.Date ?
                            Math.Round(advance.Amount * (((double)accredited.Moratoruim_Interest_Rate / 100) / finantialDay) * advance.Day_Moratorium, 2) :
                            0;
                            advance.Subtotal = advance.Interest + advance.Interest_Moratorium + advance.Comission + advance.Promotional_Setting;
                            advance.Vat = Math.Round(advance.Subtotal * vat, 2);
                            advance.Total_Withhold = Math.Round(advance.Amount + advance.Subtotal + advance.Vat, 2);
                            totalPayment += advance.Total_Withhold;
                        }
                    });

                    accredited.Payment = Math.Round(totalPayment, 2);
                }
            });

            var result = companies.Select(company => new AdvanceReceivable()
            {
                Company_Id = company.id,
                Company = company.Description,
                Accrediteds = detail.Where(p => p.Company_Id == company.id).ToList(),
                Contract_Number = accrediteds.FirstOrDefault(p => p.Company_Id == company.id).Contract_number,
                Amount = detail.Where(p => p.Company_Id == company.id).Sum(z => z.Payment) -
                    this._PaidAdvanceRetrieveService.Where(p => p.Company_Id == company.id &&
                    p.Is_Partial).Sum(z => z.Amount),
                Partial_Amount = this._PaidAdvanceRetrieveService.Where(p => p.Company_Id == company.id && 
                    p.Is_Partial).Sum(z => z.Amount),
                 
            }).ToList();

            return result;
        }
    
        public bool ExecuteProcess(int id)
        {
            Accredited user = this._AccreditedRetrieveService.Where(accredited => accredited.id == id).FirstOrDefault();

            user.Is_Blocked = true;
            user.First_Login = false;
            this._accreditedWriteService.Update(user);

            return true;
        }

        public Accredited ExecuteProcess(Accredited accredited)
        {
            Accredited user = this._AccreditedRetrieveService.Where(accreditedfilter => accreditedfilter.id == accredited.id).FirstOrDefault();

            user.ApprovedDocuments = true;
            user.Is_Blocked = false;
            user.First_Login = false;
            this._accreditedWriteService.Update(user);

            Notification notification = new Notification();
            notification.User_Id = accredited.id;
            notification.User_Type = (int)PrestaQiEnum.UserType.Acreditado;
            notification.Title = "Actualizacion Documentación";
            notification.Message = $"Sus documentos fueron aprovados";
            notification.Icon = "done";
            notification.created_at = DateTime.Now;
            this._NotificationWriteService.Create(notification);

            this._NotificationProcessService.ExecuteProcess<Notification, bool>(notification);

            return user;
        }

        public bool ExecuteProcess(NotificationDocument notificationDocument)
        {
            switch (notificationDocument.Type)
            {
                case "INE":
                    var ine = this._IneAccountRetrieveService.Find(notificationDocument.IdDocument);
                    ine.Approved = notificationDocument.Approve;
                    ine.CommnetApproved = notificationDocument.Message;
                    this._IneAccountWriteService.Update(ine);
                    break;
                case "SELFIE":
                    var selfie = this._SelfieUserRetrieveService.Find(notificationDocument.IdDocument);
                    selfie.Approved = notificationDocument.Approve;
                    selfie.CommnetApproved = notificationDocument.Message;
                    this._SelfieUserWriteService.Update(selfie);
                    break;
                case "ESTADO DE CUENTA":
                    var statusAccount = this._StatusAccountRetrieveService.Find(notificationDocument.IdDocument);
                    statusAccount.Approved = notificationDocument.Approve;
                    statusAccount.CommnetApproved = notificationDocument.Message;
                    this._StatusAccountWriteService.Update(statusAccount);
                    break;
                case "RECIBO DE NOMINA":
                    var paySheet = this._PaySheetUserRetrieveService.Find(notificationDocument.IdDocument);
                    paySheet.Approved = notificationDocument.Approve;
                    paySheet.CommnetApproved = notificationDocument.Message;
                    this._PaySheetUserWriteService.Update(paySheet);
                    break;
            }

            if (!notificationDocument.Approve)
            {
                Notification notification = new Notification();
                notification.User_Id = notificationDocument.AccreditedId;
                notification.User_Type = (int) PrestaQiEnum.UserType.Acreditado;
                notification.Title = "Actualizacion Documentación";
                notification.Message = $"Su {notificationDocument.Type} Fue Rechazado, Motivo: {notificationDocument.Message}";
                notification.Icon = "warning";
                notification.created_at = DateTime.Now;
                this._NotificationWriteService.Create(notification);

                this._NotificationProcessService.ExecuteProcess<Notification, bool>(notification);

                var accredited = this._AccreditedRetrieveService.Where(accredited => accredited.id == notificationDocument.AccreditedId).FirstOrDefault();
                if (accredited != null)
                {
                    accredited.Is_Blocked = false;
                    this._accreditedWriteService.Update(accredited);
                }
            }

            return true;
        }
    }
}

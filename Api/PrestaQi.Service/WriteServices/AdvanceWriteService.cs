using InsiscoCore.Base.Data;
using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using Microsoft.Extensions.Configuration;
using PrestaQi.Model;
using PrestaQi.Model.Configurations;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Model.Dto.Output;
using PrestaQi.Model.Enum;
using PrestaQi.Model.Spei;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PrestaQi.Service.WriteServices
{
    public class AdvanceWriteService : WriteService<Advance>
    {
        IProcessService<Advance> _AdvacenProcessService;
        IProcessService<ordenPagoWS> _OrdenPagoProcessService;
        IWriteService<SpeiResponse> _SpeiWriteService;
        IRetrieveService<Accredited> _AccreditedRetrieveService;
        IWriteService<AdvanceDetail> _AdvanceDetailWriteService;
        IRetrieveService<AdvanceDetail> _AdvanceDetailRetrieveService;
        IRetrieveService<Advance> _AdvanceRepository;
        IRetrieveService<Configuration> _ConfigutarionRetrieveService;
        IRetrieveService<PeriodCommission> _PeriodCommissionRetrieve;
        IRetrieveService<PeriodCommissionDetail> _PeriodCommissionDetailRetrieve;
        IWriteService<DetailsAdvance> _DetailsAdvanceWriteService;
        IRetrieveService<DetailsAdvance> _DetailsAdvanceRetrieve;
        IRetrieveService<DetailsByAdvance> _DetailsByAdvanceRetrieve;
        IWriteService<DetailsByAdvance> _DetailsByAdvanceWriteService;
        IProcessService<PaySheetUser> _paySheetProcessService;
        IWriteService<Accredited> _accreditedWriteService;
        public IConfiguration Configuration { get; }

        public AdvanceWriteService(
            IWriteRepository<Advance> repository,
            IProcessService<Advance> advanceProcessService,
            IProcessService<ordenPagoWS> ordenPagoProcessService,
            IWriteService<SpeiResponse> speiWriteService,
            IRetrieveService<Accredited> accreditedRetrieveService,
            IWriteService<AdvanceDetail> advanceDetailWriteService,
            IRetrieveService<AdvanceDetail> advanceDetailRetrieveService,
            IRetrieveService<Advance> advanceRepository,
            IRetrieveService<Configuration> configurationRetrieveService,
            IRetrieveService<PeriodCommission> periodCommissionRetrieve,
            IRetrieveService<PeriodCommissionDetail> periodCommissionDetailRetrieve,
            IWriteService<DetailsAdvance> detailsAdvanceWriteService,
            IRetrieveService<DetailsAdvance> detailsAdvanceRetrieve,
            IRetrieveService<DetailsByAdvance> detailsByAdvanceRetrieve,
            IWriteService<DetailsByAdvance> detailByAdvanceWriteService,
            IProcessService<PaySheetUser> paySheetProcessService,
            IWriteService<Accredited> accreditedWriteService,
            IConfiguration configuration
            ) : base(repository)
        {
            this._AdvacenProcessService = advanceProcessService;
            this._OrdenPagoProcessService = ordenPagoProcessService;
            this._SpeiWriteService = speiWriteService;
            this._AccreditedRetrieveService = accreditedRetrieveService;
            this._AdvanceDetailWriteService = advanceDetailWriteService;
            this._AdvanceDetailRetrieveService = advanceDetailRetrieveService;
            this._AdvanceRepository = advanceRepository;
            this._ConfigutarionRetrieveService = configurationRetrieveService;
            this._PeriodCommissionRetrieve = periodCommissionRetrieve;
            this._PeriodCommissionDetailRetrieve = periodCommissionDetailRetrieve;
            this._DetailsAdvanceWriteService = detailsAdvanceWriteService;
            this._DetailsAdvanceRetrieve = detailsAdvanceRetrieve;
            this._DetailsByAdvanceRetrieve = detailsByAdvanceRetrieve;
            this._DetailsByAdvanceWriteService = detailByAdvanceWriteService;
            this._paySheetProcessService = paySheetProcessService;
            this._accreditedWriteService = accreditedWriteService;
            Configuration = configuration;
        }

        public Advance Create(CalculateAmount calculateAmount)
        {
            Accredited accredited = this._AccreditedRetrieveService.Find(calculateAmount.Accredited_Id);
            AdvanceAndDetails response = this._AdvacenProcessService.ExecuteProcess<CalculateAmount, AdvanceAndDetails>(calculateAmount);
            
            response.details.ForEach(advance =>
            {
                advance.created_at = DateTime.Now;
                advance.updated_at = DateTime.Now;
                advance.Paid_Status = (int)PrestaQiEnum.AdvanceStatus.NoPagado;
            });

            ResponseSpei spei;

            if (Configuration["environment"] == "prod")
            {
                spei = this._OrdenPagoProcessService.ExecuteProcess<OrderPayment, ResponseSpei>(new OrderPayment()
                {
                    Accredited_Id = calculateAmount.Accredited_Id,
                    Advance = response.advance
                });
            } else
            {
                spei = new ResponseSpei() { resultado = new resultado() { id = 4500, claveRastreo = "TESTFF12333", descripcionError = "" } };
            }

            try
            {
                
                if (spei.resultado.id > 0)
                {
                    response.advance.created_at = DateTime.Now;
                    response.advance.updated_at = DateTime.Now;

                    bool created =  this._Repository.Create(response.advance);

                    if (created)
                    {
                        if (response.details.Count > 1)
                        {
                            var listDetails = accredited.License_Id > 0 ? SaveDetailSimple(response.advance, accredited.id, response.details) : SaveAdvanceDetails(response.advance, accredited.id, response.details);
                            var detailsByAdvance = this.RelationDetailsToAdvance(listDetails, accredited).Where(detail => detail.Advance_Id == response.advance.id).ToList();

                            foreach (DetailsByAdvance detail in detailsByAdvance)
                            {
                                detail.Detail = this._DetailsAdvanceRetrieve.Find(detail.Detail_Id);
                            }

                            response.advance.details = detailsByAdvance;
                        }

                        this._SpeiWriteService.Create(new SpeiResponse()
                        {
                            created_at = DateTime.Now,
                            updated_at = DateTime.Now,
                            advance_id = response.advance.id,
                            tracking_id = spei.resultado.id,
                            tracking_key = spei.resultado.claveRastreo
                        });

                        if (Configuration["environment"] == "prod")
                        {
                            this._OrdenPagoProcessService.ExecuteProcess<SendSpeiMail, bool>(new SendSpeiMail()
                            {
                                Amount = calculateAmount.Amount,
                                Accredited_Id = response.advance.Accredited_Id,
                                Advance = response.advance
                            });
                        }

                        PaySheetUser paySheetUser = new PaySheetUser();
                        paySheetUser.AccreditedId = response.advance.Accredited_Id;
                        paySheetUser.PaySheets = new List<PaySheetUser>();

                        foreach (var paysheet in calculateAmount.PaySheets)
                        {
                            paysheet.AccreditedId = response.advance.Accredited_Id;
                            paysheet.AdvanceId = response.advance.id;

                            paySheetUser.PaySheets.Add(paysheet);
                        }

                        this._paySheetProcessService.ExecuteProcess<PaySheetUser, bool>(paySheetUser);
                    }

                    return response.advance;
                }
                else
                    throw new SystemValidationException(spei.resultado.descripcionError);

            }
            catch (Exception exception)
            {
                throw new SystemValidationException($"Error creating Advance: {exception.Message}");
            }

        }

        private List<DetailsAdvance> SaveAdvanceDetails(Advance advance, int accreditedId, List<DetailsAdvance> details)
        {

            var advancesDetails = this._DetailsAdvanceRetrieve.Where(detail => detail.Accredited_Id == accreditedId &&
                detail.Date_Payment >= advance.Date_Advance && (detail.Paid_Status == 0 || detail.Paid_Status == 2)).OrderBy(detail => detail.id).ToList();
            
            foreach(DetailsAdvance detail in advancesDetails)
            {
                var list = this._DetailsByAdvanceRetrieve.Where(da => da.Detail_Id == detail.id).ToList();
                if (list.Count > 0)
                {
                    this._DetailsByAdvanceWriteService.Delete(list);
                }
            }

            this._DetailsAdvanceWriteService.Delete(advancesDetails);

            details.ForEach(detail =>
            {
                detail.Accredited_Id = accreditedId;
                detail.Advance_Id = advance.id;

                this._DetailsAdvanceWriteService.Create(detail);
            });

            return details;
        }

        private List<DetailsAdvance> SaveDetailSimple(Advance advance, int accreditedId, List<DetailsAdvance> details)
        {
            details.ForEach(detail =>
            {
                detail.Accredited_Id = accreditedId;
                detail.Advance_Id = advance.id;

                this._DetailsAdvanceWriteService.Create(detail);
            });

            return details;
        }

        public (double, double) CalculateInterestAndVat(CalculateAmount calculateAmount, Accredited accredited, int dayForPayment)
        {
            Advance advanceCalculated = new Advance();
            DateTime startDate = DateTime.Now;
            DateTime endDate = DateTime.Now;
            DateTime limitDate = DateTime.Now;
            int day = DateTime.Now.Day;
            PeriodCommissionDetail commissionPerioDetail = new PeriodCommissionDetail();
            double annualInterest = ((double)accredited.Interest_Rate / 100);
            int finantialDay = Convert.ToInt32(this._ConfigutarionRetrieveService.Where(p => p.Configuration_Name == "FINANCIAL_DAYS").FirstOrDefault().Configuration_Value);
            double vat = Convert.ToDouble(this._ConfigutarionRetrieveService.Where(p => p.Configuration_Name == "VAT").FirstOrDefault().Configuration_Value) / 100;

            int endDay = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);

            if (accredited.Period_Id != (int)PrestaQiEnum.PerdioAccredited.Semanal)
            {
                var commissionPeriodId = this._PeriodCommissionRetrieve.Where(p => p.Period_Id == accredited.Period_Id && p.Type_Month == endDay).FirstOrDefault().id;
                commissionPerioDetail = this._PeriodCommissionDetailRetrieve.Where(p => p.Period_Commission_Id == commissionPeriodId && p.Day_Month == dayForPayment).FirstOrDefault();
                advanceCalculated.Day_For_Payment = commissionPerioDetail.Day_Payement;
            }

            double intereset = (accredited.Period_Id == (int)PrestaQiEnum.PerdioAccredited.Mensual) || (accredited.Period_Id == (int)PrestaQiEnum.PerdioAccredited.Quincenal) ?
                Math.Round(calculateAmount.Amount * ((annualInterest / finantialDay) * commissionPerioDetail.Day_Payement), 2) :
                Math.Round(calculateAmount.Amount * ((annualInterest / finantialDay) * (limitDate.Date - DateTime.Now.Date).Days), 2);

            return (intereset, intereset * vat);
        }

        private List<DetailsByAdvance> RelationDetailsToAdvance(List<DetailsAdvance> detailsAdvances, Accredited accredited)
        {
            var advances = new List<Advance>();
            detailsAdvances = detailsAdvances.OrderByDescending(da => da.Date_Payment).ToList();
            var cloneDetailAdvance = new List<DetailsAdvance>(detailsAdvances);

            var advanceOrderDesc = this._AdvanceRepository.Where(advance => advance.Accredited_Id == accredited.id).OrderByDescending(a => a.id).ToList();

            List<DetailsByAdvance> detailsByAdvance = new List<DetailsByAdvance>();

            foreach (DetailsAdvance detail in cloneDetailAdvance)
            {
                detailsByAdvance.Add(new DetailsByAdvance()
                {
                    Advance_Id = detail.Advance_Id,
                    Detail_Id = detail.id,
                    amount = detail.Total_Payment,
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now
                });
            }

            this._DetailsByAdvanceWriteService.Create(detailsByAdvance);

            return detailsByAdvance;
        }
    }
}

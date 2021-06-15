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
                            var listDetails = SaveAdvanceDetails(response.advance, accredited.id, response.details);
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

        private void SaveDetail(int advanceId, int accreditedId, List<Advance> advances)
        {
            DateTime limitDate = advances.FirstOrDefault().Limit_Date;
            double amount = advances.FirstOrDefault().Amount;

            advances.RemoveAt(0);
            List<AdvanceDetail> advanceDetails = new List<AdvanceDetail>();

            var advancesDetails = this._AdvanceDetailRetrieveService.Where(p => true).ToList();
            var advancesList = this._AdvanceRepository.Where(p => p.Accredited_Id == accreditedId).ToList();

            var details = (from a in advancesDetails
                           join b in advancesList on a.Advance_Id equals b.id
                           where a.Limit_Date.Date >= limitDate.Date && (a.Paid_Status == 0 || a.Paid_Status == 2)
                           select a).OrderBy(p => p.Advance_Id).ToList();
            
            if (details.Count > 0) 
                this._AdvanceDetailWriteService.Delete(details);

            Accredited accredited = this._AccreditedRetrieveService.Find(accreditedId);

            advances.ForEach(p =>
            {
                var recalcInteres = this.CalculateInterestAndVat(
                    new CalculateAmount() { Amount = p.Amount, Accredited_Id = accreditedId }, accredited, p.Limit_Date.Day);
                AdvanceDetail advanceDetail = new AdvanceDetail()
                {
                    Accredited_Id = accreditedId,
                    Advance_Id = advanceId,
                    Amount = p.Amount,
                    Comission = p.Comission,
                    id = p.id,
                    created_at = p.created_at,
                    Date_Advance = p.Date_Advance,
                    Day_For_Payment = p.Day_For_Payment,
                    Enabled = p.Enabled,
                    Interest = recalcInteres.Item1 , //p.Interest,
                    Limit_Date = p.Limit_Date,
                    Paid_Status = p.Paid_Status,
                    Requested_Day = p.Requested_Day,
                    Subtotal = p.Subtotal,
                    Total_Withhold = p.Total_Withhold,
                    updated_at = p.updated_at,
                    Vat = recalcInteres.Item2, //p.Vat,
                    Initial = p.Initial,
                    Final = p.Final
                };

                advanceDetails.Add(advanceDetail);
            });

            this._AdvanceDetailWriteService.Create(advanceDetails);

            var detail = this._AdvanceDetailRetrieveService.Where(p => p.Accredited_Id == accreditedId &&
            p.Limit_Date.Date == DateTime.Now.Date && (p.Paid_Status == 0 || p.Paid_Status == 2)).OrderBy(p => p.id).FirstOrDefault();

            if (detail != null)
            {
                detail.Initial = detail.Initial + amount;
                detail.Final = Math.Round(detail.Initial - detail.Amount, MidpointRounding.AwayFromZero);

                this._AdvanceDetailWriteService.Update(detail);
            }

            
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

            int index = 0;
            foreach (DetailsAdvance detail in cloneDetailAdvance)
            {
                while(detail.Principal_Payment > 0 && advanceOrderDesc.Count > index)
                {
                    if (detail.Principal_Payment > Math.Round(advanceOrderDesc[index].Amount, 2))
                    {
                        detailsByAdvance.Add(new DetailsByAdvance()
                        {
                            Advance_Id = advanceOrderDesc[index].id,
                            Detail_Id = detail.id,
                            amount = Math.Round(advanceOrderDesc[index].Amount, 2),
                            created_at = DateTime.Now,
                            updated_at = DateTime.Now
                        });

                        detail.Principal_Payment -= Math.Round(advanceOrderDesc[index].Amount, 2);
                        index++;
                    } else
                    {
                        detailsByAdvance.Add(new DetailsByAdvance()
                        {
                            Advance_Id = advanceOrderDesc[index].id,
                            Detail_Id = detail.id,
                            amount = detail.Principal_Payment,
                            created_at = DateTime.Now,
                            updated_at = DateTime.Now
                        });

                        advanceOrderDesc[index].Amount -= detail.Principal_Payment;
                        detail.Principal_Payment = 0;
                    }
                }
            }

            this._DetailsByAdvanceWriteService.Create(detailsByAdvance);

            return detailsByAdvance;
        }
    }
}

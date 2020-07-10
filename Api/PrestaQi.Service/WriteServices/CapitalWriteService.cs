using InsiscoCore.Base.Data;
using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using Newtonsoft.Json;
using PrestaQi.Model;
using PrestaQi.Model.Configurations;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Model.Dto.Output;
using PrestaQi.Model.Enum;
using PrestaQi.Service.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PrestaQi.Service.WriteServices
{
    public class CapitalWriteService : WriteService<Capital>
    {
        IRetrieveService<Capital> _UserCapitalRetrieveService;
        IRetrieveService<Period> _PeriodRetrieveService;
        IWriteService<CapitalDetail> _CapitalDetailWriteService;
        IRetrieveService<User> _UserRetrieveService;
        IRetrieveService<Configuration> _ConfigurationRetrieveService;
        IRetrieveService<Contact> _ContactRetrieveService;
        IRetrieveService<Investor> _InvestorRetrieveSercice;

        public CapitalWriteService(
            IWriteRepository<Capital> repository,
            IRetrieveService<Capital> userCapitalRetrieveService,
            IRetrieveService<Period> periodRetrieveService,
            IWriteService<CapitalDetail> capitalDetailWriteService,
            IRetrieveService<User> userRetrieveService,
            IRetrieveService<Contact> contactRetrieveService,
            IRetrieveService<Configuration> configurationRetrieveService,
            IRetrieveService<Investor> investorRetrieveSercice
            ) : base(repository)
        {
            this._UserCapitalRetrieveService = userCapitalRetrieveService;
            this._PeriodRetrieveService = periodRetrieveService;
            this._CapitalDetailWriteService = capitalDetailWriteService;
            this._UserRetrieveService = userRetrieveService;
            this._ConfigurationRetrieveService = configurationRetrieveService;
            this._InvestorRetrieveSercice = investorRetrieveSercice;
            this._ContactRetrieveService = contactRetrieveService;
        }

        public CreateCapital Create(Capital entity)
        {
            CreateCapital createCapital = new CreateCapital();

            var user = this._UserRetrieveService.Find(entity.Created_By);
            var investor = this._InvestorRetrieveSercice.Find(entity.investor_id);

            if (user.Password != InsiscoCore.Utilities.Crypto.MD5.Encrypt(entity.Password))
                throw new SystemValidationException("Incorrect Password!");

            entity.Start_Date = DateTime.Now;
            entity.End_Date = DateTime.Now;
            entity.Capital_Status = (int)PrestaQiEnum.CapitalEnum.Solicitado;
            entity.Investment_Status = (int)PrestaQiEnum.InvestmentEnum.NoActive;
            entity.created_at = DateTime.Now;
            entity.updated_at = DateTime.Now;

            try
            {
                createCapital.Success = base.Create(entity);

                if (createCapital.Success)
                {
                    createCapital.Investor = $"{investor.First_Name} {investor.Last_Name}";
                    createCapital.Mail = investor.Mail;

                    try
                    {
                        SendMail(entity.investor_id);
                    }
                    catch (Exception)
                    {

                    }
                }

                return createCapital;
            }
            catch (Exception exception)
            {
                throw new SystemValidationException($"Error creating Capital: {exception.Message}");
            }
        }

        public CapitalChangeStatusResponse Update(CapitalChangeStatus capitalChangeStatus)
        {
            CapitalChangeStatusResponse capitalChangeStatusResponse = new CapitalChangeStatusResponse();

            var capital = this._UserCapitalRetrieveService.Find(capitalChangeStatus.Capital_Id);

            if (capital == null)
                throw new SystemValidationException("Capital not found");

            capitalChangeStatusResponse.CapitalId = capital.id;

            List<CapitalDetail> capitalDetails = new List<CapitalDetail>();
            var period = this._PeriodRetrieveService.Find(capital.period_id);

            capital.Capital_Status = capitalChangeStatus.Status;

            if (capital.Capital_Status == (int)PrestaQiEnum.CapitalEnum.Terminado)
            {
                int monthNum = (12 * capital.Investment_Horizon) / period.Period_Value;

                capital.Start_Date = DateTime.Now.AddMonths(1);
                capital.End_Date = capital.Start_Date.AddYears(capital.Investment_Horizon);
                capital.Investment_Status = (int)PrestaQiEnum.InvestmentEnum.Active;
                capital.Enabled = true;

                DateTime startDateTemp = capital.Start_Date;

                for (int i = 1; i <= period.Period_Value; i++)
                {
                    capitalDetails.Add(new CapitalDetail()
                    {
                        Capital_Id = capital.id,
                        Start_Date = startDateTemp,
                        End_Date = startDateTemp.AddMonths(monthNum),
                        Period = i,
                        Outstanding_Balance = capital.Amount,
                        created_at = DateTime.Now,
                        updated_at = DateTime.Now,
                        Pay_Day_Limit = startDateTemp.AddMonths(monthNum),//startDateTemp.AddMonths(monthNum).AddDays(capital.Bussiness_Day),
                        Principal_Payment = i == period.Period_Value ? capital.Amount : 0
                    });

                    startDateTemp = startDateTemp.AddMonths(monthNum);
                }
            }

            capitalChangeStatusResponse.Success = base.Update(capital);

            if (capitalChangeStatusResponse.Success && capitalDetails.Count > 0)
                this._CapitalDetailWriteService.Create(capitalDetails);

            return capitalChangeStatusResponse;
        }

        void SendMail(int investorId)
        {
            var investor = this._InvestorRetrieveSercice.Find(investorId);
            var contacts = this._ContactRetrieveService.Where(p => p.Enabled == true).ToList();

            var configurations = this._ConfigurationRetrieveService.Where(p => p.Enabled == true).ToList();

            var mailConf = configurations.FirstOrDefault(p => p.Configuration_Name == "EMAIL_CONFIG");
            var messageConfig = configurations.FirstOrDefault(p => p.Configuration_Name == "CAPITAL_CREATE");

            var messageMail = JsonConvert.DeserializeObject<MessageMail>(messageConfig.Configuration_Value);
            string textHtml = new StreamReader(new MemoryStream(Utilities.GetFile(configurations, messageMail.Message))).ReadToEnd();
            textHtml = textHtml.Replace("{NAME}", investor.First_Name);
            textHtml = textHtml.Replace("{WHATSAPP}", contacts.Find(p => p.id == 1).Contact_Data);
            textHtml = textHtml.Replace("{MAIL_SOPORTE}", contacts.Find(p => p.id == 2).Contact_Data);
            textHtml = textHtml.Replace("{PHONE}", contacts.Find(p => p.id == 3).Contact_Data);
            messageMail.Message = textHtml;

            Utilities.SendEmail(new List<string> { investor.Mail }, messageMail, mailConf);
        }
    }
}

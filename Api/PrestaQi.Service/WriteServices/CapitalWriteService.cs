using InsiscoCore.Base.Data;
using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using PrestaQi.Model;
using PrestaQi.Model.Configurations;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Model.Enum;
using PrestaQi.Service.Tools;
using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Service.WriteServices
{
    public class CapitalWriteService : WriteService<Capital>
    {
        IRetrieveService<Capital> _UserCapitalRetrieveService;
        IRetrieveService<Period> _PeriodRetrieveService;
        IWriteService<CapitalDetail> _CapitalDetailWriteService;
        IRetrieveService<User> _UserRetrieveService;


        public CapitalWriteService(
            IWriteRepository<Capital> repository,
            IRetrieveService<Capital> userCapitalRetrieveService,
            IRetrieveService<Period> periodRetrieveService,
            IWriteService<CapitalDetail> capitalDetailWriteService,
            IRetrieveService<User> userRetrieveService
            ) : base(repository)
        {
            this._UserCapitalRetrieveService = userCapitalRetrieveService;
            this._PeriodRetrieveService = periodRetrieveService;
            this._CapitalDetailWriteService = capitalDetailWriteService;
            this._UserRetrieveService = userRetrieveService;
        }

        public override bool Create(Capital entity)
        {
            var user = this._UserRetrieveService.Find(entity.Created_By);

            if (user.Password != InsiscoCore.Utilities.Crypto.MD5.Encrypt(entity.Password))
                throw new SystemValidationException("Incorrect Password!");

            entity.Start_Date = DateTime.Now;
            entity.End_Date = DateTime.Now;
            entity.Capital_Status = (int)PrestaQiEnum.CapitalEnum.Requested;
            entity.Investment_Status = (int)PrestaQiEnum.InvestmentEnum.NoActive;
            entity.created_at = DateTime.Now;
            entity.updated_at = DateTime.Now;

            try
            {
                return base.Create(entity);
            }
            catch (Exception exception)
            {
                throw new SystemValidationException($"Error creating Capital: {exception.Message}");
            }
        }

        public bool Update(CapitalChangeStatus capitalChangeStatus)
        {
            var capital = this._UserCapitalRetrieveService.Find(capitalChangeStatus.Capital_Id);

            if (capital == null)
                throw new SystemValidationException("Capital not found");
            
            List<CapitalDetail> capitalDetails = new List<CapitalDetail>();
            var period = this._PeriodRetrieveService.Find(capital.period_id);

            capital.Capital_Status = capitalChangeStatus.Status;

            if (capital.Capital_Status == (int)PrestaQiEnum.CapitalEnum.Finished)
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

            bool update = base.Update(capital);

            if (update && capitalDetails.Count > 0)
                this._CapitalDetailWriteService.Create(capitalDetails);

            return true;
        }

    }
}

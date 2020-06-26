using InsiscoCore.Base.Data;
using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using PrestaQi.Model;
using PrestaQi.Model.Configurations;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Model.Enum;
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


        public CapitalWriteService(
            IWriteRepository<Capital> repository,
            IRetrieveService<Capital> userCapitalRetrieveService,
            IRetrieveService<Period> periodRetrieveService,
            IWriteService<CapitalDetail> capitalDetailWriteService
            ) : base(repository)
        {
            this._UserCapitalRetrieveService = userCapitalRetrieveService;
            this._PeriodRetrieveService = periodRetrieveService;
            this._CapitalDetailWriteService = capitalDetailWriteService;
        }

        public override bool Create(Capital entity)
        {
            try
            {
                entity.Start_Date = DateTime.Now;
                entity.End_Date = DateTime.Now;
                entity.Capital_Status = (int)PrestaQiEnum.CapitalEnum.Requested;
                entity.created_at = DateTime.Now;
                entity.updated_at = DateTime.Now;

                return base.Create(entity);
            }
            catch (Exception exception)
            {
                throw new SystemValidationException("Error creating Capital!");
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
                        Pay_Day_Limit = startDateTemp.AddMonths(monthNum).AddDays(capital.Bussiness_Day)
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

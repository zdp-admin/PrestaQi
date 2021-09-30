using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PrestaQi.Model;
using PrestaQi.Model.Configurations;
using PrestaQi.Model.Dto;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Model.Dto.Output;
using PrestaQi.Model.Dto.Output.Stp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace PrestaQi.Service.ProcessServices
{
    public class LicenseProcessService : ProcessService<License>
    {
        IRetrieveService<Accredited> _accrediteRetrieveService;
        IRetrieveService<Advance> _advanceRetrieveService;
        IRetrieveService<DetailsAdvance> _detailsAdvanceRetrieveService;
        IRetrieveService<License> _licenseRetrieveService;
        IRetrieveService<Configuration> _ConfigurationRetrieveService;
        IWriteService<License> _licenseWriteService;
        IRetrieveService<LicensePriceRange> _licensePricesRetrieveService;
        IRetrieveService<LicenseDeposits> _licenseDepositRetrieveService;
        IWriteService<LicenseDeposits> _licenseDepositWriteService;
        public IConfiguration Configuration { get; }

        public LicenseProcessService(
            IRetrieveService<License> licenseRetrieveService,
            IRetrieveService<Configuration> configurationRetrieveService,
            IWriteService<License> licenseWriteService,
            IRetrieveService<LicensePriceRange> licensePriceRetrieveService,
            IConfiguration configuration,
            IRetrieveService<LicenseDeposits> licenseDepositRetrieveService,
            IWriteService<LicenseDeposits> licenseDepositWriteService,
            IRetrieveService<Advance> advanceRetrieveService,
            IRetrieveService<Accredited> accrediteRetrieveService,
            IRetrieveService<DetailsAdvance> detailsAdvanceRetrieveService
        ) 
        {
            this._licenseRetrieveService = licenseRetrieveService;
            this._ConfigurationRetrieveService = configurationRetrieveService;
            this.Configuration = configuration;
            this._licenseWriteService = licenseWriteService;
            this._licensePricesRetrieveService = licensePriceRetrieveService;
            this._licenseDepositRetrieveService = licenseDepositRetrieveService;
            this._licenseDepositWriteService = licenseDepositWriteService;
            this._advanceRetrieveService = advanceRetrieveService;
            this._accrediteRetrieveService = accrediteRetrieveService;
            this._detailsAdvanceRetrieveService = detailsAdvanceRetrieveService;
        }

        public AccountBalanceOutput ExecuteProcess(LicenseBalance filter)
        {
            int year = filter.Year <= 0 ? DateTime.Now.Year : filter.Year;
            int month = filter.Month <= 0 ? DateTime.Now.Month : filter.Month;
            double accountsReceivable = 0;

            AccountBalanceOutput accountBalance = new AccountBalanceOutput();

            License license = this._licenseRetrieveService.Find(filter.LicenseId);

            if (license == null)
            {
                throw new SystemValidationException($"No se Encontro la license con id: {filter.LicenseId}");
            }

            var accreditedIds = this._accrediteRetrieveService.Where(accredite => accredite.License_Id == license.id).Select(accredite => accredite.id).ToList();

            accountBalance.OriginatorAccount = license.OriginatorAccount;
            accountBalance.saldo = (float) license.Balance;
            accountBalance.deposits = this._licenseDepositRetrieveService.Where(depost => depost.LicenseId == license.id && depost.DateTransaction.Year == year && depost.DateTransaction.Month == month).ToList();
            accountBalance.advances = this._advanceRetrieveService.Where(advance => accreditedIds.Contains(advance.Accredited_Id) && advance.created_at.Year == year && advance.created_at.Month == month).ToList();

            accountBalance.advances.ForEach(advance =>
            {
                var details = this._detailsAdvanceRetrieveService.Where(detail => detail.Advance_Id == advance.id && (detail.Paid_Status == 0 || detail.Paid_Status == 2)).ToList();

                if (details.Count > 0)
                {
                    accountsReceivable += details.Aggregate(0d, (accum, current) => accum + current.Total_Payment);
                } else if (advance.Paid_Status == 0 || advance.Paid_Status == 2)
                {
                    accountsReceivable += advance.Total_Withhold;
                }
            });

            accountBalance.AccountsReceivable = accountsReceivable;

            return accountBalance;
        }
    }
}

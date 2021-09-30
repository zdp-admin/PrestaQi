using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Model.Dto.Output
{
    public class AccountBalanceOutput
    {
        public float cargosPendientes { get; set; }
        public float saldo { get; set; }
        public List<LicenseDeposits> deposits { get; set; }
        public List<Advance> advances { get; set; }
        public string OriginatorAccount { get; set; }
        public double AccountsReceivable { get; set; }
    }
}
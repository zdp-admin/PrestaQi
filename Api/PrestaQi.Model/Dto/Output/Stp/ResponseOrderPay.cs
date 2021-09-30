using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Model.Dto.Output.Stp
{
    public class ResponseOrderPay
    {
        public Result resultado { get; set; }
    }

    public class Result
    {
        public int id { get; set; }
        public List<LicenseDeposits> lst { get; set; }
    }
}

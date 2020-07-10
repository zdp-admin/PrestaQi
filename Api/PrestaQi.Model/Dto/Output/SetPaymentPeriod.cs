using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Model.Dto.Output
{
    public class SetPaymentPeriod
    {
        public bool Success { get; set; }
        public string Mail { get; set; }
        public bool PaymentTotal { get; set; }
    }
}

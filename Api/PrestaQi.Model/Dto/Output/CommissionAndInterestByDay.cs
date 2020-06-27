using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Model.Dto.Output
{
    public class CommissionAndInterestByDay
    {
        public DateTime Date { get; set; }
        public double Commission { get; set; }
        public double Interest { get; set; }
    }
}

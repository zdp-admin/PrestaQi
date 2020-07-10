using System;

namespace PrestaQi.Model.Dto.Output
{
    public class CommissionAndInterestByDay
    {
        public DateTime Date { get; set; }
        public double Commission { get; set; }
        public double Interest { get; set; }
    }
}

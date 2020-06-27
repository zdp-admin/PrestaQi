using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Model.Dto.Output
{
    public class CommisionAndInterestMaster
    {
        public double Total_Interest { get; set; }
        public double Average_Interest { get; set; }
        public double Total_Commission { get; set; }
        public double Average_Commission { get; set; }
        public List<CommissionAndInterestByDay> CommissionAndInterestByDays { get; set; }
    }
}

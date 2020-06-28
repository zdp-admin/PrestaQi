using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Model.Dto.Output
{
    public class InvestmentDashboard
    {
        public double Interest_Paid { get; set; }
        public double Average_Interest_Paid { get; set; }

        public double Main_Return { get; set; }
        public double Average_Main_Return { get; set; }

        public List<InvestmentDashboardDetail> InvestmentDashboardDetails { get; set; }
    }
}

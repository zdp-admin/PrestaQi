using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Model.Dto.Output
{
    public class InvestmentDashboardDetail
    {
        public DateTime Date { get; set; }
        public double Interest_Paid { get; set; }
        public double Main_Return { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;

namespace PrestaQi.Model.Dto.Output
{
    public class MyInvestment
    {
        public int Capital_ID { get; set; }
        public double Amount { get; set; }
        public int Interest_Rate { get; set; }
        public double Annual_Interest_Payment { get; set; }
        public double Total { get; set; }
        public DateTime Start_Date { get; set; }
        public DateTime End_Date { get; set; }
        public int Period_Id { get; set; }
        public int Default_Interest { get; set; }

        public List<MyInvestmentDetail> MyInvestmentDetails { get; set; }

    }
}

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
        public int Interest_Arrears { get; set; }
        public bool Enabled { get; set; }
        public double Interest_Payable { get; set; }
        public double Quantity_Interest_Arrears { get; set; }
        public double Total_Interest { get; set; }
        public double Vat { get; set; }
        public double Vat_Retention { get; set; }
        public double Isr_Retention { get; set; }
        public double Net_Interest { get; set; }
        public DateTime Pay_Day_Limit { get; set; }
        public int Day_Overdue { get; set; }
        public List<CapitalDetail> MyInvestmentDetails { get; set; }
    }
}

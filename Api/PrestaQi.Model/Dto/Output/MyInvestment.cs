using System;
using System.Collections.Generic;
using System.Security.Principal;

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
        public string Period_Name { get; set; }
        public int Interest_Arrears { get; set; }
        public string Enabled { get; set; }
        public int Capital_Status { get; set; }
        public string Capital_Status_Name { get; set; }
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
        public string Principal_Payment { get; set; }
        public int Investment_Status { get; set; }
        public string Investment_Status_Name { get; set; }
        public double Promotional_Setting { get; set; }
        public string Reason { get; set; }
    }
}

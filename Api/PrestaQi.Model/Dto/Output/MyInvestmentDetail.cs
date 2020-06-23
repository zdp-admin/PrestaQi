using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;

namespace PrestaQi.Model.Dto.Output
{
    public class MyInvestmentDetail
    {
        public int Period { get; set; }
        public DateTime Initial_Date { get; set; }
        public DateTime End_Date { get; set; }
        public double Outstanding_Balance { get; set; }
        public double Principal_Payment { get; set; }
        public double Interest_Payment { get; set; }
        public double Default_Interest { get; set; }
        public double Promotional_Setting { get; set; }
        public double Vat { get; set; }
        public double Vat_Retention { get; set; }
        public double Isr_Retention { get; set; }
        public double Payment { get; set; }

    }
}

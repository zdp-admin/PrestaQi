using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Model.Dto.Output
{
    public class AnchorControlTotal
    {
        public double Moratorium_Interest_Total { get; set; }
        public double Moratorium_Vat { get; set; }
        public double Moratorium_Vat_Retention { get; set; }
        public double Moratorium_Isr_Retention { get; set; }
        public double Moratorium_Net_Interest { get; set; }

        public double Interest { get; set; }
        public double Vat { get; set; }
        public double Vat_Retention { get; set; }
        public double Isr_Retention { get; set; }
        public double Net_Interest { get; set; }
        public double Principal_Payment { get; set; }
        public double Total_Period { get; set; }

        public double Total_Anchor_Period { get; set; }

    }
}

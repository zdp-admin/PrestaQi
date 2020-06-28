using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;

namespace PrestaQi.Model.Dto.Output
{
    public class LiquidityDetail
    {
        public DateTime Date { get; set; }
        public double Amount_Capital { get; set; }
        public double Amount_Call_Capital { get; set; }
        public double Amount_Advance { get; set; }
    }
}

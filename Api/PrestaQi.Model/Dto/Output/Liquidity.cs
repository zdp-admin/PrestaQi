using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Model.Dto.Output
{
    public class Liquidity
    {
        public double Commited_Capital { get; set; }
        public double Average_Capital { get; set; }

        public double Call_Capital { get; set; }
        public double Average_Call_Capital { get; set; }

        public double Total_Advances { get; set; }
        public double Average_Advances { get; set; }

        public List<LiquidityDetail> LiquidityDetails { get; set; }
    }
}

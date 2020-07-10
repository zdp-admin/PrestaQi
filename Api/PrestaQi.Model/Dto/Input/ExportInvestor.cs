using System.Collections.Generic;

namespace PrestaQi.Model.Dto.Input
{
    public class ExportInvestor
    {
        public int Type { get; set; }
        public List<Investor> InvestorDatas { get; set; }
    }
}

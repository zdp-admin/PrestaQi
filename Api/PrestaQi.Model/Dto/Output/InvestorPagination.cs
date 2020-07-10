using System.Collections.Generic;

namespace PrestaQi.Model.Dto.Output
{
    public class InvestorPagination
    {
        public List<Investor> InvestorDatas { get; set; }
        public int TotalRecord { get; set; }
    }
}

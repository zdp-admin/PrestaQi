using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Model.Dto.Output
{
    public class InvestorPagination
    {
        public List<Investor> InvestorDatas { get; set; }
        public int TotalRecord { get; set; }
    }
}

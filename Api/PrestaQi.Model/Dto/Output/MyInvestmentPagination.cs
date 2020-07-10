using System.Collections.Generic;

namespace PrestaQi.Model.Dto.Output
{
    public class MyInvestmentPagination
    {
        public List<MyInvestment> MyInvestments { get; set; }
        public int TotalRecord { get; set; }
    }
}

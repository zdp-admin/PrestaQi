using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Model.Dto.Output
{
    public class AdvanceReceivablePagination
    {
        public List<AdvanceReceivable> AdvanceReceivables { get; set; }
        public int TotalRecord { get; set; }
    }
}

using System.Collections.Generic;

namespace PrestaQi.Model.Dto.Output
{
    public class AdvanceReceivablePagination
    {
        public List<AdvanceReceivable> AdvanceReceivables { get; set; }
        public int TotalRecord { get; set; }
    }
}

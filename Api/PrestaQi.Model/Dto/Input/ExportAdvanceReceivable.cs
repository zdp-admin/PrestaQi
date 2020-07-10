using PrestaQi.Model.Dto.Output;
using System.Collections.Generic;

namespace PrestaQi.Model.Dto.Input
{
    public class ExportAdvanceReceivable
    {
        public int Type { get; set; }
        public List<AdvanceReceivable> AdvanceReceivables { get; set; }
    }
}

using System.Collections.Generic;

namespace PrestaQi.Model.Dto.Input
{
    public class SetPayAdvance
    {
        public int Company_Id { get; set; }
        public double Amount { get; set; }
        public bool IsPartial { get; set; }
        public List<int> AdvanceIds { get; set; }
    }
}

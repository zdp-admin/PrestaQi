using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrestaQi.Model
{
    public partial class Advance
    {
        [NotMapped]
        public double Maximum_Amount { get; set; }
        [NotMapped]
        public double Initial { get; set; }
        [NotMapped]
        public double Final { get; set; }
        [NotMapped]
        public List<AdvanceDetail>  AdvanceDetails { get; set; }
    }
}

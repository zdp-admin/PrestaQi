using PrestaQi.Model.Dto.Output;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrestaQi.Model
{
    public partial class Investor
    {
        [NotMapped]
        public string Institution_Name { get; set; }
        [NotMapped]
        public string NameComplete { get; set; }
        [NotMapped]
        public double AmountExercised { get; set; }
        [NotMapped]
        public List<CapitalData> CapitalDatas { get; set; }
        [NotMapped]
        public int Type { get; set; }
        [NotMapped]
        public string TypeName { get; set; }
    }
}

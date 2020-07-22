using InsiscoCore.Utilities.Crypto;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrestaQi.Model
{
    public partial class Accredited
    {
        [NotMapped]
        public string Period_Name { get; set; }
        [NotMapped]
        public string Company_Name { get; set; }
        [NotMapped]
        public List<Advance> Advances { get; set; }
        [NotMapped]
        public string Institution_Name { get; set; }
        [NotMapped]
        public int Type { get; set; }
        [NotMapped]
        public string TypeName { get; set; }
        [NotMapped]
        public double Credit_Limit { get; set; }

    }
}

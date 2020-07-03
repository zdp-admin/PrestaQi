using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Principal;
using System.Text;

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
    }
}

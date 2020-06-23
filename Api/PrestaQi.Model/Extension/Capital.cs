using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PrestaQi.Model
{
    public partial class Capital
    {
        [NotMapped]
        public string Period_Name { get; set; }
    }
}

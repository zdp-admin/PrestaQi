using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PrestaQi.Model
{
    public partial class Advance
    {
        [NotMapped]
        public double Maximum_Amount { get; set; }
    }
}

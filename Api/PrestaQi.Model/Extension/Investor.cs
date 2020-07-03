using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Principal;
using System.Text;

namespace PrestaQi.Model
{
    public partial class Investor
    {
        [NotMapped]
        public string Institution_Name { get; set; }
    }
}

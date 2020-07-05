using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Principal;
using System.Text;

namespace PrestaQi.Model
{
    public partial class User
    {
        [NotMapped]
        public int Type { get; set; }
        [NotMapped]
        public string TypeName { get; set; }
    }
}

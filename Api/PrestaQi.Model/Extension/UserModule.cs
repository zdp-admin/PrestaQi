using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PrestaQi.Model
{
    public partial class UserModule
    {
        [NotMapped]
        public string Module { get; set; }
    }
}

using PrestaQi.Model.General;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PrestaQi.Model
{
    [Table("usermodules")]
    public class UserModule : Entity<int>
    {
        public int user_id { get; set; }
        public int module_id { get; set; }
    }
}

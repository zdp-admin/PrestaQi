using PrestaQi.Model.General;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PrestaQi.Model
{
    [Table("usertypes")]
    public class UserType : Entity<int>
    {
        [Column("description")]
        public string Description { get; set; }
    }
}

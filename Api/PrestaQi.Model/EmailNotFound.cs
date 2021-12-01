using PrestaQi.Model.General;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PrestaQi.Model
{
    [Table("email_not_found")]
    public class EmailNotFound: Entity<int>
    {
        [Column("curp")]
        public string Curp { get; set; }

        [Column("email")]
        public string Email { get; set; }
    }
}

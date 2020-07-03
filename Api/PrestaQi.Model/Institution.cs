using PrestaQi.Model.General;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PrestaQi.Model
{
    [Table("institutions")]
    public class Institution : Entity<int>
    {
        [Column("code")]
        public int Code { get; set; }
        [Column("description")]
        public string Description { get; set; }
        [Column("enabled")]
        public bool Enabled { get; set; }
    }
}

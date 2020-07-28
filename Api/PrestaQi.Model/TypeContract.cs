using PrestaQi.Model.General;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace PrestaQi.Model
{
    [Table("typecontract")]
    public class TypeContract : Entity<int>
    {
        [Column("description")]
        public string Description { get; set; }
        [Column("code")]
        public string Code { get; set; }
    }
}

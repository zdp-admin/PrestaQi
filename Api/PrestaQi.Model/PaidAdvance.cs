using PrestaQi.Model.General;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PrestaQi.Model
{
    [Table("paidadvances")]
    public class PaidAdvance : Entity<int>
    {
        [Column("amount")]
        public double Amount { get; set; }
        [Column("company_id")]
        public int Company_Id { get; set; }
        [Column("is_partial")]
        public bool Is_Partial { get; set; }

    }
}

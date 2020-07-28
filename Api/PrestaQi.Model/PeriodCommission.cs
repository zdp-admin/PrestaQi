using PrestaQi.Model.General;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PrestaQi.Model
{
    [Table("periodcommission")]
    public class PeriodCommission : Entity<int>
    {
        [Column("period_id")]
        public int Period_Id { get; set; }
        [Column("type_month")]
        public int Type_Month { get; set; }
    }
}

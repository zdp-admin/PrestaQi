using PrestaQi.Model.General;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime;
using System.Text;

namespace PrestaQi.Model
{
    [Table("periodcommissiondetail")]
    public class PeriodCommissionDetail : Entity<int>
    {
        [Column("period_commission_id")]
        public int Period_Commission_Id { get; set; }
        [Column("day_month")]
        public int Day_Month { get; set; }
        [Column("commission")]
        public double Commission { get; set; }
        [Column("day_payement")]
        public int Day_Payement { get; set; }
        [Column("interest")]
        public double Interest { get; set; }
        [Column("date_payment")]
        public int Date_Payment { get; set; }
    }
}

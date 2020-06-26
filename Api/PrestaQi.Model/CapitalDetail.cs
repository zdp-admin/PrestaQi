using PrestaQi.Model.General;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PrestaQi.Model
{
    [Table("capitaldetails")]
    public partial class CapitalDetail : Entity<int>
    {
        [Column("period")]
        public int Period { get; set; }
        [Column("capital_id")]
        public int Capital_Id { get; set; }
        [Column("start_date")]
        public DateTime Start_Date { get; set; }
        [Column("end_date")]
        public DateTime End_Date { get; set; }
        [Column("outstanding_balance")]
        public double Outstanding_Balance { get; set; }
        [Column("principal_payment")]
        public double Principal_Payment { get; set; }
        [Column("interest_payment")]
        public double Interest_Payment { get; set; }
        [Column("default_interest")]
        public double Default_Interest { get; set; }
        [Column("promotional_settings")]
        public double Promotional_Settings { get; set; }
        [Column("vat")]
        public double Vat { get; set; }
        [Column("vat_retention")]
        public double Vat_Retention { get; set; }
        [Column("isr_retention")]
        public double Isr_Retention { get; set; }
        [Column("payment")]
        public double Payment { get; set; }
        [Column("ispayment")]
        public bool IsPayment { get; set; }
        [Column("pay_day_limit")]
        public DateTime Pay_Day_Limit { get; set; }
    }
}

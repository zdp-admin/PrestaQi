using PrestaQi.Model.General;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrestaQi.Model
{
    [Table("details_advance")]
    public partial class DetailsAdvance : Entity<int>
    {
        [Column("advance_id")]
        public int Advance_Id { get; set; }
        [Column("principal_payment")]
        public double Principal_Payment { get; set; }
        [Column("total_payment")]
        public double Total_Payment { get; set; }
        [Column("interest")]
        public double Interest { get; set; }
        [Column("vat")]
        public double Vat { get; set; }
        [Column("date_payment")]
        public DateTime Date_Payment { get; set; }
        [Column("initial_balance")]
        public double Initial_Balance { get; set; }
        [Column("final_balance")]
        public double Final_Balance { get; set; }
        [Column("days_for_payments")]
        public int Days_For_Payments { get; set; }
        [Column("paid_status")]
        public int Paid_Status { get; set; }
        [Column("accredited_id")]
        public int Accredited_Id { get; set; }
        [Column("promotional_setting")]
        public double? Promotional_Setting { get; set; }
        [NotMapped]
        public string Bank_Name { get; set; }
        [NotMapped]
        public string Account_Number { get; set; }
    }
}

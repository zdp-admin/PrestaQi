using PrestaQi.Model.General;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PrestaQi.Model
{
    [Table("speiresponses")]
    public class SpeiResponse : Entity<int>
    {
        [Column("advance_id")]
        public int advance_id { get; set; }
        [Column("tracking_id")]
        public int tracking_id { get; set; }
        [Column("tracking_key")]
        public string tracking_key { get; set; }
        [Column("state_name")]
        public string State_Name { get; set; }
        [Column("repayment_id")]
        public int? Repayment_Id { get; set; }
    }
}

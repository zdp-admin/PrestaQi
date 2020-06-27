using PrestaQi.Model.General;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PrestaQi.Model
{
    [Table("advances")]
    public partial class Advance : Entity<int>
    {
        [Column("accredited_id")]
        public int Accredited_Id { get; set; }
        [Column("amount")]
        public double Amount { get; set; }
        [Column("date_advance")]
        public DateTime Date_Advance { get; set; }
        [Column("requested_day")]
        public int Requested_Day { get; set; }
        [Column("total_withhold")]
        public double Total_Withhold { get; set; }
        [Column("comission")]
        public int Comission { get; set; }

    }
}

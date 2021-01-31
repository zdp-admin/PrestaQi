using PrestaQi.Model.General;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PrestaQi.Model
{
    [Table("details_by_advance")]
    public class DetailsByAdvance : Entity<int>
    {
        [Column("advance_id")]
        public int Advance_Id { get; set; }
        [Column("details_id")]
        public int Detail_Id { get; set; }
        [Column("amount")]
        public double amount { get; set; }
        [NotMapped]
        public DetailsAdvance Detail { get; set; }
    }
}

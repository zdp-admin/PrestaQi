using PrestaQi.Model.General;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PrestaQi.Model
{
    [Table("license_price_range")]
    public class LicensePriceRange : Entity<int>
    {
        [Column("initial_quantity")]
        public int? InitialQuantity { get; set; }
        [Column("final_quantity")]
        public int? FinalQuantity { get; set; }
        [Column("cost")]
        public double Cost { get; set; }
        [Column("license_id")]
        public int? LicenseId { get; set; }
    }
}

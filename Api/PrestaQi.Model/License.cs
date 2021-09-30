using PrestaQi.Model.General;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrestaQi.Model
{
    [Table("license")]
    public class License : Entity<int>
    {
        [Column("name")]
        public string Name { get; set; }
        [Column("name_person_charge")]
        public string NamePersonCharge { get; set; }
        [Column("contract_number")]
        public string ContractNumber { get; set; }
        [Column("license_number")]
        public string LicenseNumber { get; set; }
        [Column("date_payment")]
        public DateTime DatePayment { get; set; }
        [Column("cost_center")]
        public string CostCenter { get; set; }
        [Column("email")]
        public string Mail { get; set; }
        [Column("password")]
        public string Password { get; set; }
        [Column("enabled")]
        public bool Enabled { get; set; }
        [Column("first_login")]
        public bool FirstLogin { get; set; }
        [Column("model_prestaqi")]
        public bool ModelPrestaqi { get; set; }
        [Column("originator_account")]
        public string OriginatorAccount { get; set; }
        [Column("balance")]
        public double Balance { get; set; }
        [NotMapped]
        public List<LicensePriceRange> Prices { get; set; }
        [NotMapped]
        public List<LicenseDeposits> Deposits { get; set; }
    }
}
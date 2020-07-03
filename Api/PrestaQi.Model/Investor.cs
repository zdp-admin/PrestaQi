using PrestaQi.Model.General;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Principal;

namespace PrestaQi.Model
{
    [Table("investors")]
    public partial class Investor : Entity<int>
    {
        [Column("first_name")]
        public string First_Name { get; set; }
        [Column("last_name")]
        public string Last_Name { get; set; }
        [Column("total_amount_agreed")]
        public double Total_Amount_Agreed { get; set; }
        [Column("start_date_prestaqi")]
        public DateTime Start_Date_Prestaqi { get; set; }
        [Column("limit_date")]
        public DateTime Limit_Date { get; set; }
        [Column("rfc")]
        public string Rfc { get; set; }
        [Column("institution_id")]
        public int Institution_Id { get; set; }
        [Column("clabe")]
        public string Clabe { get; set; }
        [Column("account_number")]
        public string Account_Number { get; set; }
        [Column("enabled")]
        public bool Enabled { get; set; }

        [Column("is_moral_person")]
        public bool Is_Moral_Person { get; set; }
        [Column("mail")]
        public string Mail { get; set; }
        [Column("password")]
        public string Password { get; set; }
        public List<Capital> Capitals { get; set; }
    }
}

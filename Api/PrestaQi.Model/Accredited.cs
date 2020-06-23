using PrestaQi.Model.General;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PrestaQi.Model
{
    [Table("accrediteds")]
    public class Accredited : Entity<int>
    {
        [Column("first_name")]
        public string First_Name { get; set; }
        [Column("last_name")]
        public string Last_Name { get; set; }
        [Column("company")]
        public string Company { get; set; }
        [Column("identify")]
        public string Identify { get; set; }
        [Column("contract_number")]
        public string Contract_number { get; set; }
        [Column("position")]
        public string Position { get; set; }
        [Column("net_monthly_salary")]
        public double Net_Monthly_Salary { get; set; }
        [Column("rfc")]
        public string Rfc { get; set; }
        [Column("interest_rate")]
        public int Interest_Rate { get; set; }
        [Column("seniority_company")]
        public int Seniority_Company { get; set; }
        [Column("birth_date")]
        public DateTime Birth_Date { get; set; }
        [Column("age")]
        public int Age { get; set; }
        [Column("gender_id")]
        public int Gender_Id { get; set; }
        [Column("bank")]
        public string Bank { get; set; }
        [Column("clabe")]
        public string Clabe { get; set; }
        [Column("account_number")]
        public string Account_Number { get; set; }
        [Column("enabled")]
        public bool Enabled { get; set; }
    }
}

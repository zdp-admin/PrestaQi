using PrestaQi.Model.General;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PrestaQi.Model
{
    [Table("accrediteds")]
    public partial class Accredited : Entity<int>
    {
        [Column("first_name")]
        public string First_Name { get; set; }
        [Column("last_name")]
        public string Last_Name { get; set; }
        [Column("company_id")]
        public int Company_Id { get; set; }
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
        [Column("institution_id")]
        public int Institution_Id { get; set; }
        [Column("clabe")]
        public string Clabe { get; set; }
        [Column("account_number")]
        public string Account_Number { get; set; }
        [Column("enabled")]
        public bool Enabled { get; set; }

        [Column("moratoruim_interest_rate")]
        public int Moratoruim_Interest_Rate { get; set; }
        [Column("period_id")]
        public int Period_Id { get; set; }
        [Column("mail")]
        public string Mail { get; set; }
        [Column("mail_mandate_latter")]
        public string Mail_Mandate_Latter { get; set; }
        [Column("password")]
        public string Password { get; set; }
    }
}

using InsiscoCore.Utilities.Crypto;
using PrestaQi.Model.General;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata.Ecma335;
using System.Security.Principal;

namespace PrestaQi.Model
{
    [Table("accrediteds")]
    public partial class Accredited : Entity<int>
    {
        #region "Fields"
        string _first_name;
        string _last_name;
        string _identify;
        string _contract_number;
        string _position;
        string _rfc;
        string _seniority;
        string _mail;
        string _mail_mandatory;
        string _clabe;
        string _account;
        string _address;
        string _colony;
        string _municipality;
        string _zip_code;
        string _state;
        #endregion

        #region "Properties"
        [Column("first_name")]
        [Encrypted(nameof(_first_name))]
        public string First_Name { get => _first_name.Decrypt(); set => _first_name = value.Encrypt(); }
        [Column("last_name")]
        [Encrypted(nameof(_last_name))]
        public string Last_Name { get => _last_name.Decrypt(); set => _last_name = value.Encrypt(); }
        [Column("company_id")]
        public int Company_Id { get; set; }
        [Column("identify")]
        [Encrypted(nameof(_identify))]
        public string Identify { get => _identify.Decrypt(); set => _identify = value.Encrypt(); }
        [Column("contract_number")]
        [Encrypted(nameof(_contract_number))]
        public string Contract_number { get => _contract_number.Decrypt(); set => _contract_number = value.Encrypt(); }
        [Column("position")]
        [Encrypted(nameof(_position))]
        public string Position { get => _position.Decrypt(); set => _position = value.Encrypt(); }
        [Column("net_monthly_salary")]
        public double Net_Monthly_Salary { get; set; }
        [Column("gross_monthly_salary")]
        public double Gross_Monthly_Salary { get; set; }
        [Column("other_obligations")]
        public double Other_Obligations { get; set; }
        [Column("rfc")]
        [Encrypted(nameof(_rfc))]
        public string Rfc { get => _rfc.Decrypt(); set => _rfc = value.Encrypt(); }
        [Column("interest_rate")]
        public int Interest_Rate { get; set; }
        [Column("seniority_company")]
        [Encrypted(nameof(_seniority))]
        public string Seniority_Company { get => _seniority.Decrypt(); set => _seniority = value.Encrypt(); }
        [Column("birth_date")]
        public DateTime Birth_Date { get; set; }
        [Column("age")]
        public int Age { get; set; }
        [Column("gender_id")]
        public int Gender_Id { get; set; }
        [Column("institution_id")]
        public int Institution_Id { get; set; }
        [Column("clabe")]
        [Encrypted(nameof(_clabe))]
        public string Clabe { get => _clabe.Decrypt(); set => _clabe = value.Encrypt(); }
        [Column("account_number")]
        [Encrypted(nameof(_account))]
        public string Account_Number { get => _account.Decrypt(); set => _account = value.Encrypt(); }
        [Column("enabled")]
        public bool Enabled { get; set; }
        [Column("moratoruim_interest_rate")]
        public int Moratoruim_Interest_Rate { get; set; }
        [Column("period_id")]
        public int Period_Id { get; set; }
        [Column("mail")]
        [Encrypted(nameof(_mail))]
        public string Mail { get => _mail.Decrypt(); set => _mail = value.Encrypt(); }
        [Column("mail_mandate_latter")]
        [Encrypted(nameof(_mail_mandatory))]
        public string Mail_Mandate_Latter { get => _mail_mandatory.Decrypt(); set => _mail_mandatory = value.Encrypt(); }
        [Column("password")]
        public string Password { get; set; }
        [Column("deleted_at")]
        public DateTime? Deleted_At { get; set; }
        [Column("first_login")]
        public bool First_Login { get; set; }
        [Column("type_contract_id")]
        public int? Type_Contract_Id { get; set; }
        [Column("address")]
        [Encrypted(nameof(_address))]
        public string Address { get => _address.Decrypt(); set => _address = value.Encrypt(); }
        [Column("colony")]
        [Encrypted(nameof(_colony))]
        public string Colony { get => _colony.Decrypt(); set => _colony = value.Encrypt(); }
        [Column("municipality")]
        [Encrypted(nameof(_municipality))]
        public string Municipality { get => _municipality.Decrypt(); set => _municipality = value.Encrypt(); }
        [Column("zip_code")]
        [Encrypted(nameof(_zip_code))]
        public string Zip_Code { get => _zip_code.Decrypt(); set => _zip_code = value.Encrypt(); }
        [Column("state")]
        [Encrypted(nameof(_state))]
        public string State { get => _state.Decrypt(); set => _state = value.Encrypt(); }
        [Column("is_blocked")]
        public bool Is_Blocked { get; set; }
        [Column("end_day_payment")]
        public DateTime End_Day_Payment { get; set; }
        [Column("outsourcing_id")]
        public int? Outsourcing_id { get; set; }
        [Column("period_start_date")]
        public int? Period_Start_Date { get; set; }
        [Column("period_end_date")]
        public int? Period_End_Date { get; set; }
        #endregion
    }
}

using InsiscoCore.Utilities.Crypto;
using PrestaQi.Model.General;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrestaQi.Model
{
    [Table("investors")]
    public partial class Investor : Entity<int>
    {
        #region "Fields"
        string _first_name;
        string _last_name;
        string _rfc;
        string _clabe;
        string _account;
        string _mail;
        string _contract;
        #endregion

        [Column("first_name")]
        [Encrypted(nameof(_first_name))]
        public string First_Name { get => _first_name.Decrypt(); set => _first_name = value.Encrypt(); }
        [Column("last_name")]
        [Encrypted(nameof(_last_name))]
        public string Last_Name { get => _last_name.Decrypt(); set => _last_name = value.Encrypt(); }
        [Column("total_amount_agreed")]
        public double Total_Amount_Agreed { get; set; }
        [Column("start_date_prestaqi")]
        public DateTime Start_Date_Prestaqi { get; set; }
        [Column("limit_date")]
        public DateTime Limit_Date { get; set; }
        [Column("rfc")]
        [Encrypted(nameof(_rfc))]
        public string Rfc { get => _rfc.Decrypt(); set => _rfc = value.Encrypt(); }
        [Column("institution_id")]
        public int Institution_Id { get; set; }
        [Column("clabe")]
        [Encrypted(nameof(_clabe))]
        public string Clabe { get => _clabe.Decrypt(); set => _clabe = value.Encrypt(); }
        [Column("account_number")]
        [Encrypted(nameof(_clabe))]
        public string Account_Number { get => _account.Decrypt(); set => _account = value.Encrypt(); }
        [Column("enabled")]
        public bool Enabled { get; set; }

        [Column("is_moral_person")]
        public bool Is_Moral_Person { get; set; }
        [Column("mail")]
        [Encrypted(nameof(_mail))]
        public string Mail { get => _mail.Decrypt(); set => _mail = value.Encrypt(); }
        [Column("password")]
        public string Password { get; set; }
        [Column("deleted_at")]
        public DateTime? Deleted_At { get; set; }
        [Column("first_login")]
        public bool First_Login { get; set; }
        [Column("contract_number")]
        [Encrypted(nameof(_contract))]
        public string Contract_number { get => _contract.Decrypt(); set => _contract = value.Encrypt(); }
        public List<Capital> Capitals { get; set; }
    }
}

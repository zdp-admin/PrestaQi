using InsiscoCore.Utilities.Crypto;
using PrestaQi.Model.General;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrestaQi.Model
{
    [Table("users")]
    public partial class User : Entity<int>
    {
        #region "Fields"
        string _first_name;
        string _last_name;
        string _employee_number;
        string _mail;
        string _phone;
        #endregion

        [Column("first_name")]
        [Encrypted(nameof(_first_name))]
        public string First_Name { get => _first_name.Decrypt(); set => _first_name = value.Encrypt(); }
        [Column("last_name")]
        [Encrypted(nameof(_last_name))]
        public string Last_Name { get => _last_name.Decrypt(); set => _last_name = value.Encrypt(); }
        [Column("password")]
        public string Password { get; set; }
        [Column("employee_number")]
        [Encrypted(nameof(_employee_number))]
        public string Employee_Number { get => _employee_number.Decrypt(); set => _employee_number = value.Encrypt(); }
        [Column("mail")]
        [Encrypted(nameof(_mail))]
        public string Mail { get => _mail.Decrypt(); set => _mail = value.Encrypt(); }
        [Column("phone")]
        [Encrypted(nameof(_phone))]
        public string Phone { get => _phone.Decrypt(); set => _phone = value.Encrypt(); }
        [Column("enabled")]
        public bool Enabled { get; set; }
        [Column("deleted_at")]
        public DateTime? Deleted_At { get; set; }
        [Column("first_login")]
        public bool First_Login { get; set; }
        [NotMapped]
        public List<UserModule> Modules { get; set; }
    }
}

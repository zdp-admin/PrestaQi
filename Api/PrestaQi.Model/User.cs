using PrestaQi.Model.General;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrestaQi.Model
{
    [Table("users")]
    public partial class User : Entity<int>
    {
        [Column("first_name")]
        public string First_Name { get; set; }
        [Column("last_name")]
        public string Last_Name { get; set; }
        [Column("password")]
        public string Password { get; set; }
        [Column("employee_number")]
        public string Employee_Number { get; set; }
        [Column("mail")]
        public string Mail { get; set; }
        [Column("phone")]
        public string Phone { get; set; }
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

using PrestaQi.Model.General;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrestaQi.Model
{
    [Table("users")]
    public class User : Entity<int>
    {
        [Column("first_name"), Display(Name = "first_name")]
        public string First_Name { get; set; }
        [Column("last_name")]
        public string Last_Name { get; set; }
        [Column("password")]
        public string Password { get; set; }
        [Column("rfc")]
        public string RFC { get; set; }
        [Column("birth_date")]
        public DateTime Birth_Date { get; set; }
        [Column("age")]
        public int Age { get; set; }
        [Column("phone")]
        public string Phone { get; set; }
        [Column("mail")]
        public string Mail { get; set; }
        [Column("enabled")]
        public bool Enabled { get; set; }
        [Column("user_type_id")]
        public int User_Type_Id { get; set; }
    }
}

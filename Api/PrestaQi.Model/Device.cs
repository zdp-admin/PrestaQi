using PrestaQi.Model.General;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PrestaQi.Model
{
    [Table("devices")]
    public class Device : Entity<int>
    {
        [Column("device_id")]
        public string Device_Id { get; set; }
        [Column("user_id")]
        public int User_Id { get; set; }
        [Column("user_type")]
        public int User_Type { get; set; }
        [Column("enabled")]
        public bool Enabled { get; set; }
    }
}

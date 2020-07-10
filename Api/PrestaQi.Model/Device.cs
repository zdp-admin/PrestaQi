using PrestaQi.Model.General;
using System.ComponentModel.DataAnnotations.Schema;

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

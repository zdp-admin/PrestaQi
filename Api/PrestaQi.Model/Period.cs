using PrestaQi.Model.General;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrestaQi.Model
{
    [Table("periods")]
    public class Period : Entity<int>
    {
        [Column("description")]
        public string Description { get; set; }
        [Column("enabled")]
        public bool Enabled { get; set; }
        [Column("period_value")]
        public int Period_Value { get; set; }
        [Column("user_type")]
        public int User_Type { get; set; }
    }
}

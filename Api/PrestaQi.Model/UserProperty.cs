using PrestaQi.Model.General;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrestaQi.Model
{
    [Table("userproperties")]
    public class UserProperty : Entity<int>
    {
        [Column("user_id")]
        public int User_Id { get; set; }
        [Column("property_name")]
        public string Property_Name { get; set; }
        [Column("property_value")]
        public string Property_Value { get; set; }
    }
}

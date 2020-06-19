using PrestaQi.Model.General;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrestaQi.Model
{
    [Table("usertypes")]
    public class UserType : Entity<int>
    {
        [Column("description")]
        public string Description { get; set; }
    }
}

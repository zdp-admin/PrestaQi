using PrestaQi.Model.General;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrestaQi.Model
{
    [Table("contacts")]
    public class Contact : Entity<int>
    {
        [Column("contact_name")]
        public string Contact_Name { get; set; }
        [Column("contact_data")]
        public string Contact_Data { get; set; }
        [Column("logo")]
        public string Logo { get; set; }
        [Column("enabled")]
        public bool Enabled { get; set; }
    }
}

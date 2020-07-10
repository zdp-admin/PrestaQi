using System.ComponentModel.DataAnnotations.Schema;

namespace PrestaQi.Model
{
    public partial class Capital
    {
        [NotMapped]
        public string Period_Name { get; set; }
        [NotMapped]
        public bool Enabled { get; set; }
        [NotMapped]
        public string Password { get; set; }
    }
}

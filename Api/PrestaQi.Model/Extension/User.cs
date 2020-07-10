using System.ComponentModel.DataAnnotations.Schema;

namespace PrestaQi.Model
{
    public partial class User
    {
        [NotMapped]
        public int Type { get; set; }
        [NotMapped]
        public string TypeName { get; set; }
    }
}

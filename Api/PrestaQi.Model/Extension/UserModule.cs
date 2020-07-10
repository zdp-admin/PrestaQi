using System.ComponentModel.DataAnnotations.Schema;

namespace PrestaQi.Model
{
    public partial class UserModule
    {
        [NotMapped]
        public string Module { get; set; }
    }
}

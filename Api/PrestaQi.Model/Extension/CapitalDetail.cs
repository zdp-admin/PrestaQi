using System.ComponentModel.DataAnnotations.Schema;

namespace PrestaQi.Model
{
    public partial class CapitalDetail
    {
        [NotMapped]
        public bool IsPeriodActual { get; set; }
    }
}

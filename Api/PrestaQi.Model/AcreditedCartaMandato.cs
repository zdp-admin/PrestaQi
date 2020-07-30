using PrestaQi.Model.General;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrestaQi.Model
{
    [Table("acreditedcartamandato")]
    public class AcreditedCartaMandato: Entity<int>
    {
        [Column("accredited_id")]
        public int Accredited_Id { get; set; }
        [Column("path_contract")]
        public string Path_Contract { get; set; }
    }
}

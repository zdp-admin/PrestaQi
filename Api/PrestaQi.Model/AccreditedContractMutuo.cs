using PrestaQi.Model.General;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrestaQi.Model
{
    [Table("acreditedcontractmutuo")]
    public class AccreditedContractMutuo: Entity<int>
    {
        [Column("accredited_id")]
        public int Accredited_Id { get; set; }
        [Column("number_contract")]
        public int Number_Contract { get; set; }
        [Column("path_contract")]
        public string Path_Contract { get; set; }
    }
}

using PrestaQi.Model.General;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrestaQi.Model
{
    [Table("request_register")]
    public class RequestRegister: Entity<int>
    {
        [Column("name")]
        public string Name { get; set; }

        [Column("lastname")]
        public string Lastname { get; set; }

        [Column("m_lastname")]
        public string MLastname { get; set; }
        
        [Column("employee_number")]
        public string EmployeeNumber { get; set; }
        
        [Column("curp")]
        public string Curp { get; set; }

        [Column("company_id")]
        public int CompanyId { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("phone")]
        public string Phone { get; set; }
        [Column("deleted_at")]
        public DateTime? Deleted_At { get; set; }
    }
}

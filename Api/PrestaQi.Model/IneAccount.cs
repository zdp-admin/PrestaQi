using PrestaQi.Model.General;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PrestaQi.Model
{
    [Table("ine_account")]
    public class IneAccount : Entity<int>
    {
        [Column("name")]
        public string Name { get; set; }
        [Column("last_name")]
        public string LastName { get; set; }
        [Column("curp")]
        public string Curp { get; set; }
        [Column("birth_date")]
        public string BirthDate { get; set; }
        [Column("address")]
        public string Address { get; set; }
        [Column("clave_elector")]
        public string ClaveElector { get; set; }
        [Column("path_file_front")]
        public string PathFileFront { get; set; }
        [Column("clave_elector_back")]
        public string ClaveElectorBack { get; set; }
        [Column("path_file_back")]
        public string PathFileBack { get; set; }
        [Column("meta")]
        public string Meta { get; set; }
        [Column("accredited_id")]
        public int AccreditedId { get; set; }
        [Column("approved")]
        public bool? Approved { get; set; }
        [Column("comment_approved")]
        public string CommnetApproved { get; set; }
        [NotMapped]
        public byte[] File { get; set; }
        [NotMapped]
        public string NameFile { get; set; }
        [NotMapped]
        public byte[] FileBack { get; set; }
        [NotMapped]
        public string NameFileBack { get; set; }
    }
}

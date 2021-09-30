using PrestaQi.Model.General;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PrestaQi.Model
{
    [Table("status_account")]
    public class StatusAccount : Entity<int>
    {
        [Column("key_account")]
        public string KeyAccount { get; set; }
        [Column("address")]
        public string Address { get; set; }
        [Column("name")]
        public string Name { get; set; }
        [Column("periodo")]
        public string Periodo { get; set; }
        [Column("rfc")]
        public string Rfc { get; set; }
        [Column("name_bank")]
        public string NameBank { get; set; }
        [Column("business_name_bank")]
        public string BusinessNameBank { get; set; }
        [Column("number_account")]
        public string NumberAccount { get; set; }
        [Column("path_file")]
        public string PathFile { get; set; }
        [Column("meta")]
        public string Meta { get; set; }
        [Column("accredited_id")]
        public int? AccreditedId { get; set; }
        [Column("approved")]
        public bool? Approved { get; set; }
        [Column("comment_approved")]
        public string CommnetApproved { get; set; }
        [NotMapped]
        public string NameFile { get; set; }
        [NotMapped]
        public byte[] File { get; set; }
    }
}

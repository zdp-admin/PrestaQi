using PrestaQi.Model.General;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PrestaQi.Model
{
    [Table("paysheet_user")]
    public class PaySheetUser : Entity<int>
    {
        [Column("name")]
        public string Name { get; set; }
        [Column("rfc")]
        public string Rfc { get; set; }
        [Column("curp")]
        public string Curp { get; set; }
        [Column("nss")]
        public string Nss { get; set; }
        [Column("date_initial")]
        public DateTime DateInitial { get; set; }
        [Column("date_finish")]
        public DateTime DateFinish { get; set; }
        [Column("neto")]
        public double Neto { get; set; }
        [Column("total")]
        public double Total { get; set; }
        [Column("accredited_id")]
        public int AccreditedId { get; set; }
        [Column("path_file")]
        public string PathFile { get; set; }
        [Column("meta")]
        public string Meta { get; set; }
        [Column("advance_id")]
        public int? AdvanceId { get; set; }
        [Column("approved")]
        public bool? Approved { get; set; }
        [Column("comment_approved")]
        public string CommnetApproved { get; set; }
        [NotMapped]
        public byte[] File { get; set; }
        [NotMapped]
        public string NameFile { get; set; }
        [NotMapped]
        public string UUID { get; set; }
        [NotMapped]
        public List<PaySheetUser> PaySheets { get; set; }
        [NotMapped]
        public string PaySheetsJson { get; set; }
    }
}

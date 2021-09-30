using PrestaQi.Model.General;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PrestaQi.Model
{
    [Table("selfie_user")]
    public class SelfieUser : Entity<int>
    {
        [Column("face_id")]
        public string FaceId { get; set; }
        [Column("meta")]
        public string Meta { get; set; }
        [Column("accredited_id")]
        public int AccreditedId { get; set; }
        [Column("path_file")]
        public string PathFile { get; set; }
        [Column("approved")]
        public bool? Approved { get; set; }
        [Column("comment_approved")]
        public string CommnetApproved { get; set; }
    }
}

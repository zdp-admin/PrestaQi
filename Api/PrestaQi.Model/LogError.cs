using PrestaQi.Model.General;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PrestaQi.Model
{
    [Table("logerrors")]
    public class LogError : Entity<long>
    {
        [Column("File_Name")]
        public string file_name { get; set; }
        [Column("Class_Name")]
        public string class_name { get; set; }
        [Column("Method_Name")]
        public string method_name { get; set; }
        [Column("Code_Line")]
        public int code_line { get; set; }
        [Column("Message_Error")]
        public string message_error { get; set; }
        [Column("Inner_Exception_Message")]
        public string Inner_Exception_Message { get; set; }
    }
}

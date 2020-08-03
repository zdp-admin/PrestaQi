using PrestaQi.Model.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PrestaQi.Model
{
    public partial class Notification
    {
        [NotMapped]
        public dynamic Data { get; set; }
        
    }
}

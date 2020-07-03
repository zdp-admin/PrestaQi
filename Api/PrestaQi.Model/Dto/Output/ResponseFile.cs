using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;

namespace PrestaQi.Model.Dto.Output
{
    public class ResponseFile
    {
        public object Entities { get; set; }
        public StringBuilder Message { get; set; }
    }
}

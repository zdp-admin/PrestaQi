using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Model.Dto.Output
{
    public class ResponseWithPagination
    {
        public ICollection data { get; set; }
        public int TotalRecord { get; set; }
    }
}

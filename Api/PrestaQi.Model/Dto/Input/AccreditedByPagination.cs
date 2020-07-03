using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Model.Dto.Input
{
    public class AccreditedByPagination
    {
        public bool OnlyActive { get; set; }
        public int Page { get; set; }
        public int NumRecord { get; set; }
        public int Type { get; set; }
    }
}

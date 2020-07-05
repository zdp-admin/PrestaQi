using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Model.Dto.Output
{
    public class AccreditedPagination
    {
        public List<Accredited> Accrediteds { get; set; }
        public int TotalRecord { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Runtime;
using System.Text;

namespace PrestaQi.Model.Dto.Input
{
    public class ExportMyAdvance
    {
        public int Accredited_Id { get; set; }
        public List<Advance> Advances { get; set; }
    }
}

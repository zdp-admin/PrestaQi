using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Model.Dto.Input
{
    public class ExportAccredited
    {
        public int Type { get; set; }
        public List<Accredited> Accrediteds { get; set; }
    }
}

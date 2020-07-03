using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Model.Dto.Input
{
    public class ExportAdvance
    {
        public int Type { get; set; }
        public AccreditedExportData Accredited { get; set; }
    }
}

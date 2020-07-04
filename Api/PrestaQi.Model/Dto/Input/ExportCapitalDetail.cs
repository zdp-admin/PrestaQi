using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Model.Dto.Input
{
    public class ExportCapitalDetail
    {
        public int Type { get; set; }
        public string Name { get; set; }
        public int Interest_Rate { get; set; }
        public List<CapitalDetail> CapitalDetails { get; set; }
        public int Source { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Model.Dto.Input
{
    public class AccreditedExportData
    {
        public string Identify { get; set; }
        public string First_Name { get; set; }
        public string Last_Name { get; set; }
        public string Company_Name { get; set; }
        public string Contract_Number { get; set; }
        public int Interest_Rate { get; set; }
        public List<Advance> Advances { get; set; }
    }
}

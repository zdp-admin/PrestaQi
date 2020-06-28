using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Model.Dto.Output
{
    public class AdvanceReceivable
    {
        public string Company { get; set; }
        public string Contract_Number { get; set; }
        public double Amount { get; set; }
        public List<AdvanceReceivableAccredited> Accrediteds { get; set; }
    }
}

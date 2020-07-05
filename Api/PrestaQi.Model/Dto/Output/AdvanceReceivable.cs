using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Model.Dto.Output
{
    public class AdvanceReceivable
    {
        public int Company_Id { get; set; }
        public string Company { get; set; }
        public string Contract_Number { get; set; }
        public double Amount { get; set; }
        public double Partial_Amount { get; set; }
        public List<AdvanceReceivableAccredited> Accrediteds { get; set; }
    }
}

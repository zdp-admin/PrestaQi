using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Model.Dto.Output
{
    public class AnchorControl
    {
        public int Investor_Id { get; set; }
        public string Name_Complete { get; set; }
        public List<MyInvestment> MyInvestments { get; set; }
    }
}

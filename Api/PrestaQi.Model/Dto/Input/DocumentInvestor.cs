using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Model.Dto.Input
{
    public class DocumentInvestor
    {
        public int CapitalId { get; set; }
        public Investor Investor { get; set; }
    }
}

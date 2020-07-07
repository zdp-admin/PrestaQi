using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Model.Dto.Input
{
    public class InvestorByFilter
    {
        public int Page { get; set; }
        public int NumRecord { get; set; }
        public int Type { get; set; }
        public string Filter { get; set; }
    }
}

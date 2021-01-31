using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Model.Dto.Output
{
    public class MyAdvances
    {
        public DetailsAdvance For_Payment { get; set; }
        public List<Advance> Currents { get; set; }
        public List<Advance> Befores { get; set; }
    }
}

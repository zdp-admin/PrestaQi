using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Model.Dto.Output
{
    public class AdvanceAndDetails
    {
        public Advance advance { get; set; }
        public List<DetailsAdvance> details { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Model.Dto.Input
{
    public class CustomInsertDetails
    {
        public int AdvanceId { get; set; }
        public List<DetailsAdvance> Details {get; set;}
    }
}

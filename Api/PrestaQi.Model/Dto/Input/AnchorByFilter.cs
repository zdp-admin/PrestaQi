using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Model.Dto.Input
{
    public class AnchorByFilter
    {
        public DateTime? Start_Date { get; set; }
        public DateTime? End_Date { get; set; }
        public int Page { get; set; }
        public int NumRecord { get; set; }
        public int Type { get; set; }
        public string Filter { get; set; }
    }
}

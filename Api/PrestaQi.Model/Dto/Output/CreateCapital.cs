using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Model.Dto.Output
{
    public class CreateCapital
    {
        public string Investor { get; set; }
        public bool Success { get; set; }
        public string Mail { get; set; }
    }
}

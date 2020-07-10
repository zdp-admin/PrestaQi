using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Model.Dto.Output
{
    public class SpeiTransactionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Accredited { get; set; }
    }
}

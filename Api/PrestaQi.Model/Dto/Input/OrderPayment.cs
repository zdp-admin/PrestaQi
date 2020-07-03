using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace PrestaQi.Model.Dto.Input
{
    public class OrderPayment
    {
        public int Accredited_Id { get; set; }
        public Advance Advance { get; set; }
    }
}

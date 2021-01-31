using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Model.Dto.Input
{
    public class CalculatePromotional
    {
        public int Advance_Id { get; set; }
        public int Accredited_Id { get; set; }
        public double Amount { get; set; }
        public bool Is_Details { get; set; }
        public string password { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Model.Dto.Input
{
    public class SendSpeiMail
    {
        public int Accredited_Id { get; set; }
        public double Amount { get; set; }
        public string Tracking_Key { get; set; }
    }
}

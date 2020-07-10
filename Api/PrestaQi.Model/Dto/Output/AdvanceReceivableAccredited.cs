using System;
using System.Collections.Generic;

namespace PrestaQi.Model.Dto.Output
{
    public class AdvanceReceivableAccredited
    {
        public int Accredited_Id { get; set; }
        public int Company_Id { get; set; }
        public string Id { get; set; }
        public string NameComplete { get; set; }
        public double Amount { get; set; }
        public DateTime Date_Advance { get; set; }
        public int Requested_Day { get; set; }
        public int Comission { get; set; }
        public int Interest_Rate { get; set; }
        public double Payment { get; set; }
        public List<Advance> Advances { get; set; }
    }
}

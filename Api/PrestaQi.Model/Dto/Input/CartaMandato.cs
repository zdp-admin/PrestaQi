using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Model.Dto.Input
{
    public class CartaMandato
    {
        public Accredited accredited { get; set; }
        public Advance advance { get; set; }
        public AcreditedCartaMandato acreditedCartaMandato { get; set; }
        public AccreditedContractMutuo contractMutuo { get; set; }
        public bool CheckedHide { get; set; }
        public String ?dates { get; set; }
        public double totalWeek { get; set; }
    }
}

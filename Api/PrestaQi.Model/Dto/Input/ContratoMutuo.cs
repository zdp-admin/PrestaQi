using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Model.Dto.Input
{
    public class ContratoMutuo
    {
        public Accredited accredited { get; set; }
        public AccreditedContractMutuo contractMutuo { get; set; }
        public Advance advance { get; set; }
    }
}

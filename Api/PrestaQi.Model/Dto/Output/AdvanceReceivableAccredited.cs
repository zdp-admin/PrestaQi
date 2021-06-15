using System;
using System.Collections.Generic;
using System.Security.Principal;

namespace PrestaQi.Model.Dto.Output
{
    public class AdvanceReceivableAccredited
    {
        public int Accredited_Id { get; set; }
        public int Company_Id { get; set; }
        public string Id { get; set; }
        public string NameComplete { get; set; }
        public int Interest_Rate { get; set; }
        public int Moratoruim_Interest_Rate { get; set; }
        public double Payment { get; set; }
        public bool Is_Blocked { get; set; }
        public List<Advance> Advances { get; set; }
        public List<AdvanceDetail> AdvanceDetails { get; set; }
        public List<DetailsAdvance> DetailsAdvances { get; set; }
        public int TypeContractId { get; set; }
        public int Period_Id { get; set; }
    }
}

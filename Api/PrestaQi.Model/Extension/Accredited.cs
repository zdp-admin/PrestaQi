using InsiscoCore.Utilities.Crypto;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrestaQi.Model
{
    public partial class Accredited
    {
        [NotMapped]
        public string Period_Name { get; set; }
        [NotMapped]
        public string Company_Name { get; set; }
        [NotMapped]
        public List<Advance> Advances { get; set; }
        [NotMapped]
        public List<AdvanceDetail> AdvanceDetails { get; set; }
        [NotMapped]
        public string Institution_Name { get; set; }
        [NotMapped]
        public int Type { get; set; }
        [NotMapped]
        public string TypeName { get; set; }
        [NotMapped]
        public TypeContract TypeContract { get; set; }
        [NotMapped]
        public double Credit_Limit { get; set; }
        [NotMapped]
        public double Advance_Autorhized_Amount { get; set; }
        [NotMapped]
        public double Advance_Via_Payroll { get; set; }
        [NotMapped]
        public double Authorized_Advance_After_Obligations { get; set; }
        [NotMapped]
        public double Payroll_Advance_Authorized_After_Obligations { get; set; }
        [NotMapped]
        public string Outsourcing_Name { get; set; }
        [NotMapped]
        public SelfieUser Selfie { get; set; }
        [NotMapped]
        public PaySheetUser PaySheet { get; set; }
        [NotMapped]
        public StatusAccount StatusAccount { get; set; }
        [NotMapped]
        public List<PaySheetUser> PaySheetInitial { get; set; }
        [NotMapped]
        public IneAccount IneAccount { get; set; }

    }
}

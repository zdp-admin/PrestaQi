using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Model.Dto.Output
{
    public class InvestorData
    {
        public int Id { get; set; }
        public string NameComplete { get; set; }
        public DateTime Limit_Date { get; set; }
        public double Commited_Amount { get; set; }
        public double AmountExercised { get; set; }
        public List<CapitalData> CapitalDatas { get; set; }
    }
}

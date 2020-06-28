using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Model.Dto.Input
{
    public class GetInvestment
    {
        public DateTime Start_Date { get; set; }
        public DateTime End_Date { get; set; }
        public bool Is_Specifid_Day { get; set; }
    }
}

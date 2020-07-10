using System;

namespace PrestaQi.Model.Dto.Input
{
    public class GetLiquidity
    {
        public DateTime Start_Date { get; set; }
        public DateTime End_Date { get; set; }
        public bool Is_Specifid_Day { get; set; }
        public string Filter { get; set; }
    }
}

using System.Collections.Generic;

namespace PrestaQi.Model.Dto.Input
{
    public class CalculateAmount
    {
        public int Accredited_Id { get; set; }
        public double Amount { get; set; }
        public List<PaySheetUser> PaySheets { get; set; }
        public string PaySheetsJson { get; set; }

    }
}

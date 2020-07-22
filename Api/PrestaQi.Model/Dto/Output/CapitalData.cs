using System;

namespace PrestaQi.Model.Dto.Output
{
    public class CapitalData
    {
        public int Capital_Id { get; set; }
        public int Interest_Rate { get; set; }
        public double Amount { get; set; }
        public DateTime Start_Date { get; set; }
        public DateTime End_Date { get; set; }
        public string Period { get; set; }
        public string Capital_Status { get; set; }
        public string File { get; set; }
        public byte[] File_Byte { get; set; }
        public string Investment_Status { get; set; }
        public int Day_Capital_Call { get; set; }
    }
}

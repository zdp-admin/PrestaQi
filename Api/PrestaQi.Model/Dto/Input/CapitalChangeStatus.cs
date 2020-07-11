namespace PrestaQi.Model.Dto.Input
{
    public class CapitalChangeStatus
    {
        public int Capital_Id { get; set; }
        public int Status { get; set; }
        public string File_Name { get; set; }
        public byte[] File_Byte { get; set; }
    }
}

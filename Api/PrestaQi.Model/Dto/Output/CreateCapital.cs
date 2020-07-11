namespace PrestaQi.Model.Dto.Output
{
    public class CreateCapital
    {
        public string Investor { get; set; }
        public bool Success { get; set; }
        public string Mail { get; set; }
        public int Capital_Id { get; set; }
        public double Amount { get; set; }
    }
}

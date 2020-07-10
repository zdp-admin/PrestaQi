namespace PrestaQi.Model.Dto.Input
{
    public class GetMyInvestment
    {
        public int Investor_Id { get; set; }
        public int Type { get; set; }
        public int Page { get; set; }
        public int NumRecord { get; set; }
        public int Source { get; set; }
    }
}

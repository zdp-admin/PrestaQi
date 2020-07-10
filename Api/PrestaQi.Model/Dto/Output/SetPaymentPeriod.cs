namespace PrestaQi.Model.Dto.Output
{
    public class SetPaymentPeriod
    {
        public bool Success { get; set; }
        public string Mail { get; set; }
        public bool PaymentTotal { get; set; }
        public int UserId { get; set; }
    }
}

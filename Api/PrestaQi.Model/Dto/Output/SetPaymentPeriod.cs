using System.Security.Principal;

namespace PrestaQi.Model.Dto.Output
{
    public class SetPaymentPeriod
    {
        public bool Success { get; set; }
        public string Mail { get; set; }
        public bool PaymentTotal { get; set; }
        public int UserId { get; set; }
        public int Capital_Id { get; set; }
        public int Period_Id { get; set; }
        public double Payment { get; set; }
    }
}

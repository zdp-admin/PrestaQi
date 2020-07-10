namespace PrestaQi.Model.Dto.Output
{
    public class SpeiTransactionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Accredited { get; set; }
        public string Mail { get; set; }
        public int UserId { get; set; }
    }
}

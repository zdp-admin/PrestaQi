namespace PrestaQi.Model.Dto.Input
{
    public class DisableUser
    {
        public int UserId { get; set; }
        public int Type { get; set; }
        public bool IsDelete { get; set; }
        public bool Value { get; set; }
    }
}

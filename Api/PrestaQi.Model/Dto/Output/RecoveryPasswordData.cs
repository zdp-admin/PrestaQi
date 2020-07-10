using PrestaQi.Model.Enum;

namespace PrestaQi.Model.Dto.Output
{
    public class RecoveryPasswordData
    {
        public PrestaQiEnum.UserType UserType { get; set; }
        public object Data { get; set; }
        public string Password { get; set; }
        public string Mail { get; set; }
    }
}

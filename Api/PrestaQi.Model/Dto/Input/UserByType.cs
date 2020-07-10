using PrestaQi.Model.Enum;

namespace PrestaQi.Model.Dto.Input
{
    public class UserByType
    {
        public int User_Id { get; set; }
        public PrestaQiEnum.UserType UserType { get; set; }
    }
}

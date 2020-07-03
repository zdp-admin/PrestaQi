using PrestaQi.Model.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Model.Dto.Input
{
    public class UserByType
    {
        public int User_Id { get; set; }
        public PrestaQiEnum.UserType UserType { get; set; }
    }
}

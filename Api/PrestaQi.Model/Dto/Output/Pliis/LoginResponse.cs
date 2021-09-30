using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Model.Dto.Output.Pliis
{
    public class LoginResponse
    {
        public string token { get; set; }
        public string bearer { get; set; }
        public string nick { get; set; }
        public DataUser dataUser { get; set; }
    }
}

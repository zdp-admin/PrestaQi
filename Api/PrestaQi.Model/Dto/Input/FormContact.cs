using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Model.Dto.Input
{
    public class FormContact
    {
        public string name { get; set; }
        public string email { get; set; }
        public string type { get; set; }
        public string company { get; set; }
        public string message { get; set; }
        public string telephone { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;

namespace PrestaQi.Model.Dto.Input
{
    public class SendMailRecoveryPassword
    {
        public string Name { get; set; }
        public string Mail { get; set; }
        public string Password { get; set; }
        public List<Contact> Contacts { get; set; }
    }
}

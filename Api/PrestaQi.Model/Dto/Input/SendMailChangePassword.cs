using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;

namespace PrestaQi.Model.Dto.Input
{
    public class SendMailChangePassword
    {
        public List<string> Mails { get; set; }
        public string Name { get; set; }
        public List<Contact> Contacts { get; set; }
    }
}

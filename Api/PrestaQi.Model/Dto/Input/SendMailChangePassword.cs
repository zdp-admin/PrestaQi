using System.Collections.Generic;

namespace PrestaQi.Model.Dto.Input
{
    public class SendMailChangePassword
    {
        public List<string> Mails { get; set; }
        public string Name { get; set; }
        public List<Contact> Contacts { get; set; }
    }
}

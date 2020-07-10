using System.Collections.Generic;

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

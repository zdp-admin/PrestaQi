using InsiscoCore.Service;
using PrestaQi.Model;

namespace PrestaQi.Service.ProcessServices
{
    public class ContactProcessService: ProcessService<Contact>
    {
        public bool ExecuteProcess(Contact contact)
        {

            return true;
        }
    }
}

using InsiscoCore.Base.Data;
using InsiscoCore.Service;
using PrestaQi.Model;

namespace PrestaQi.Service.RetrieveServices
{
    public class CapitalRetrieveService : RetrieveService<Capital>
    {
        public CapitalRetrieveService(
            IRetrieveRepository<Capital> repository
            ) : base(repository)
        {
        }
    }
}

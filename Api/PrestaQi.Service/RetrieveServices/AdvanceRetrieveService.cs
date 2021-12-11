using InsiscoCore.Base.Data;
using InsiscoCore.Service;
using PrestaQi.Model;

namespace PrestaQi.Service.RetrieveServices
{
    public class AdvanceRetrieveService : RetrieveService<Advance>
    {
        public AdvanceRetrieveService(IRetrieveRepository<Advance> repository) : base(repository) { }
    }
}

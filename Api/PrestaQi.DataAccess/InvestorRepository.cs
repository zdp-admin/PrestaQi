using InsiscoCore.EFRepository;
using Microsoft.EntityFrameworkCore;
using PrestaQi.Model;

namespace PrestaQi.DataAccess
{
    public class InvestorRepository : Repository<Investor>
    {
        
        public InvestorRepository(DbContext dbContext) : base(dbContext)
        {
        }

        /*public override IEnumerable<Investor> Where(Func<Investor, bool> predicate)
        {
            var result = this._dbContext.Set<Investor>()
                .Include(p => p.Capitals)
                .Where(predicate).ToList();
            return result;
        }*/
    }
}

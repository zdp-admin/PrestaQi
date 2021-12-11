using InsiscoCore.EFRepository;
using Microsoft.EntityFrameworkCore;
using PrestaQi.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PrestaQi.DataAccess
{
    public class InvestorRepository : Repository<Advance>
    {
        
        public InvestorRepository(DbContext dbContext) : base(dbContext)
        {
            
        }

        /*public override IEnumerable<Advance> Where(Func<Advance, bool> predicate)
        {
            var result = this._dbContext.Set<Advance>()
                .Include(p => p.DetailsAdvances)
                .Where(predicate).ToList();

            return result;
        }*/
    }
}

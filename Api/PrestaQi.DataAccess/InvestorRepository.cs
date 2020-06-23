using InsiscoCore.EFRepository;
using Microsoft.EntityFrameworkCore;
using PrestaQi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrestaQi.DataAccess
{
    public class InvestorRepository : Repository<Investor>
    {
        public InvestorRepository(DbContext dbContext) : base(dbContext)
        {
        }

        public override IEnumerable<Investor> Where(Func<Investor, bool> predicate)
        {
            var prueba = this._dbContext.Set<Investor>().Include(p => p.Capitals).Where(predicate).ToList();
            return prueba;
        }
    }
}

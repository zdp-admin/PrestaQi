using InsiscoCore.Base.Data;
using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using PrestaQi.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PrestaQi.Service.RetrieveServices
{
    public class UserModuleRetrieveService : RetrieveService<UserModule>
    {
        IRetrieveService<Module> _ModuleRetrieveService;

        public UserModuleRetrieveService(
            IRetrieveRepository<UserModule> repository,
            IRetrieveService<Module> moduleRetrieveService
            ) : base(repository)
        {
            this._ModuleRetrieveService = moduleRetrieveService;
        }

        public override IEnumerable<UserModule> Where(Func<UserModule, bool> predicate)
        {
            var modules = this._ModuleRetrieveService.Where(p => p.Enabled == true).ToList();

            var list = base.Where(predicate).ToList();

            list.ForEach(p =>
            {
                p.Module = modules.Find(m => m.id == p.module_id).Description;
            });

            return list;
        }
    }
}

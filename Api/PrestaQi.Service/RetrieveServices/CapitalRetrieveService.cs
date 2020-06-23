using InsiscoCore.Base.Data;
using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using PrestaQi.Model;
using PrestaQi.Model.Configurations;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Model.Dto.Output;
using System;
using System.Collections.Generic;
using System.Linq;

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

using InsiscoCore.Base.Data;
using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using Microsoft.VisualBasic;
using PrestaQi.Model;
using PrestaQi.Model.Configurations;
using PrestaQi.Model.Dto;
using PrestaQi.Model.Dto.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PrestaQi.Service.WriteServices
{
    public class AdvanceWriteService : WriteService<Advance>
    {
        IProcessService<Advance> _AdvacenProcessService; 

        public AdvanceWriteService(
            IWriteRepository<Advance> repository,
            IProcessService<Advance> advanceProcessService
            ) : base(repository)
        {
            this._AdvacenProcessService = advanceProcessService;
        }

        public bool Create(CalculateAmount calculateAmount)
        {
            try
            {
                Advance advance = this._AdvacenProcessService.ExecuteProcess<CalculateAmount, Advance>(calculateAmount);
                advance.Date_Advance = DateTime.Now;
                advance.created_at = DateTime.Now;
                advance.updated_at = DateTime.Now;
                advance.Enabled = true;
            
                return base.Create(advance);
            }
            catch (Exception exception)
            {
                throw new SystemValidationException("Error creating Advance");
            }

        }

    }
}

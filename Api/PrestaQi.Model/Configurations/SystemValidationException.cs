using PrestaQi.Model.Dto.Output;
using PrestaQi.Model.General;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace PrestaQi.Model.Configurations
{
    public class SystemValidationException : Exception
    {
        public SystemValidationException(string message) : base(message)
        {
            
        }
    }
}

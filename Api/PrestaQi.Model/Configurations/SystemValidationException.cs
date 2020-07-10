using System;

namespace PrestaQi.Model.Configurations
{
    public class SystemValidationException : Exception
    {
        public SystemValidationException(string message) : base(message)
        {
            
        }
    }
}

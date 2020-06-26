using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Model.Enum
{
    public class PrestaQiEnum
    {
        public enum CapitalEnum
        {
            Requested = 1,
            Sent = 2,
            Finished
        }

        public enum InvestmentEnum
        {
            Active = 1,
            NoActive = 2
        }
    }
}

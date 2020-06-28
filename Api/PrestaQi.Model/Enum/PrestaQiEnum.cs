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

        public enum PerdioAccredited
        {
            Semanal = 7,
            Quincenal = 9,
            Mensual = 10
        }

        public enum AdvanceStatus
        {
            NoPagado = 0,
            Pagado = 1,
            PagadoParial = 2
        }
    }
}

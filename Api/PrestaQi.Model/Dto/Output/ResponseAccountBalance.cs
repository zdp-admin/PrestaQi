using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Model.Dto.Output
{
    public class ResponseAccountBalance
    {
        public ResultAccountBalance resultado { get; set; }
    }

    public class ResultAccountBalance
    {
        public int id { get; set; }
        public AccountBalanceOutput saldoCuenta { get; set; }
    }
}

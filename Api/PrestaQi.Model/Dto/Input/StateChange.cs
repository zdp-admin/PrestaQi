using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Model.Dto.Input
{
    public class StateChange
    {
        public int Id { get; set; }
        public string Empresa { get; set; }
        public string FolioOrigen { get; set; }
        public string Estado { get; set; }
        public int CausaDevolucion { get; set; }
    }
}

using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Model.Dto.Output
{
    public class ResponseFile
    {
        public object Entities { get; set; }
        public StringBuilder Message { get; set; }
        public List<Accredited> ForDelete { get; set; }
        public List<Accredited> ForUnDelete { get; set; }
    }
}

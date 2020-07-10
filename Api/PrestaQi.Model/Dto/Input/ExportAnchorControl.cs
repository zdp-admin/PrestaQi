using PrestaQi.Model.Dto.Output;
using System.Collections.Generic;

namespace PrestaQi.Model.Dto.Input
{
    public class ExportAnchorControl
    {
        public int Type { get; set; }
        public List<AnchorControl> AnchorControls { get; set; }
    }
}

using PrestaQi.Model.Dto.Output;
using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Model.Dto.Input
{
    public class ExportAnchorControl
    {
        public int Type { get; set; }
        public List<AnchorControl> AnchorControls { get; set; }
    }
}

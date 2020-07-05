using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Model.Dto.Output
{
    public class AnchorControlPagination
    {
        public List<AnchorControl> AnchorControls { get; set; }
        public int TotalRecord { get; set; }
    }
}

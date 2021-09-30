using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Model.Dto.Input
{
    public class LicenseByFilter
    {
        public int Page { get; set; }
        public int NumRecord { get; set; }
        public string Filter { get; set; }
        public int Id { get; set; }
    }
}
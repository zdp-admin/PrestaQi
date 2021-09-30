using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Model.Dto.Input
{
    public class MovementByFilter
    {
        public int Page { get; set; }
        public int NumRecord { get; set; }
        public string Filter { get; set; }
        public int LicenseId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
    }
}
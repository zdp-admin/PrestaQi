﻿using PrestaQi.Model.Dto.Output;
using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Model.Dto.Input
{
    public class ExportMyInvestment
    {
        public int Type { get; set; }
        public List<MyInvestment> MyInvestments { get; set; }
    }
}

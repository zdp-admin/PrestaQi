using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PrestaQi.Model.Dto.Input
{
    public class FileMail
    {
        public string FileName { get; set; }
        public Stream File { get; set; }
    }
}

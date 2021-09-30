using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Model.Dto.Input
{
    public class UploadSelfieBinaria
    {
        public string FaceId { get; set; }
        public byte[] File { get; set; }
        public string NameFile { get; set; }
        public int AccreditedId { get; set; }
        public string metadata { get; set; }
    }
}

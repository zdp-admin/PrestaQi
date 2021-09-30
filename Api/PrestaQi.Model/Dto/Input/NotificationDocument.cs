using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Model.Dto.Input
{
    public class NotificationDocument
    {
        public string Type { get; set; }
        public bool Approve { get; set; }
        public string Message { get; set; }
        public int IdDocument { get; set; }
        public int AccreditedId { get; set; }
    }
}

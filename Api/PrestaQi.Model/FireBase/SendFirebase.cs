using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Model.FireBase
{
    public class SendFirebase
    {
        public FireNotification notification { get; set; }
        public string priority { get; set; }
        public dynamic data { get; set; }
        public List<string> registration_ids { get; set; }
    }
}

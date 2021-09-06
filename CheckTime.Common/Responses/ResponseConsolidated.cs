using System;
using System.Collections.Generic;
using System.Text;

namespace CheckTime.Common.Responses
{
    public class ResponseConsolidated
    {
        public bool Completed { get; set; }
        public string Message { get; set; }
        public object Result { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckTime.Common.Responses
{
    public class ResponseCheckTime
    {
        public int IdClient { get; set; }
        public DateTime RegisteredTime { get; set; }
        public string Message { get; set; }
    }
}

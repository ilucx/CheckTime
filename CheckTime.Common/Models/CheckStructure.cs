using System;
using System.Collections.Generic;
using System.Text;

namespace CheckTime.Common.Models
{
    class CheckStructure
    {
        public int IdClient { get; set; }
        public DateTime RegisterTime { get; set; }
        public int Type { get; set; }
        public bool Consolidated { get; set; }
    }
}

using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckTime.Functions.Entities
{
    public class CheckConsolidateEntity : TableEntity
    {
        public int IdClient { get; set; }
        public DateTime DateClient { get; set; }
        public double MinWorked { get; set; }
    }
}

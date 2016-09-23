using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace APISamples.Driver.Models
{
    public class EGeckoValidationEvent
    {
        public string TemplateName { get; set; }
        public string IsValidating { get; set; }
        public Dictionary<string, string> BarcodesScanned { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}

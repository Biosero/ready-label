using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace APISamples.Driver.Models
{
    public enum PrinterState { Ready, Busy, Errored }

    public class PrinterStatus
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public PrinterState State { get; set; }
        public List<string> Errors { get; set; }
    }
}

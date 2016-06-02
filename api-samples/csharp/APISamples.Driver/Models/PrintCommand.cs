using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace APISamples.Driver.Models
{
    public class PrintCommand
    {
        public string LabelFile { get; set; }
        public Dictionary<string, string> Data { get; set; }
    }
}

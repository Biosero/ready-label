using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace APISamples.Driver.Models
{
    public class LabelTemplate
    {
        public string FilePath { get; set; }
        public string TemplateName { get; set; }
        public bool IsOpen { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double XOffset { get; set; }
        public double YOffset { get; set; }
        public int TotalElements { get; set; }
    }
}

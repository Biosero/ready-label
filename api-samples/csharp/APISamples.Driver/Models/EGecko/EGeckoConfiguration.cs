using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace APISamples.Driver.Models
{
    public class EGeckoConfiguration
    {
        public bool IsValidating { get; set; }
        public double PickupHeight { get; set; }
        public double ApplyHeight { get; set; }
        public double ApplyDepth { get; set; }
        public bool ApplyNorth { get; set; }
        public bool ApplySouth { get; set; }
        public bool ApplyEast { get; set; }
        public bool ApplyWest { get; set; }
    }
}

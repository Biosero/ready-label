using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace APISamples.Driver.Models
{
    public class EGeckoPrinterStatus : PrinterStatus
    {
        public EGeckoValidationEvent LastValidationEvent { get; set; }
    }
}

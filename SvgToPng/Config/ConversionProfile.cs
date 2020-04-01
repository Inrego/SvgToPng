using System;
using System.Collections.Generic;
using System.Text;

namespace SvgToPng.Config
{
    public class ConversionProfile
    {
        public ColorConversion[] ColorConversions { get; set; }
        public Output[] Output { get; set; }
    }
}

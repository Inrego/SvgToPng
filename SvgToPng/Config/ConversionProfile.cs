using System;
using System.Collections.Generic;
using System.Text;

namespace SvgToPng.Config
{
    public class ConversionProfile
    {
        public ColorConversion[] ColorConversions { get; set; }
        public Output[] Output { get; set; }
        public string OutputDirectory { get; set; }
        public string[] Input { get; set; }
        public bool Overwrite { get; set; }
    }
}

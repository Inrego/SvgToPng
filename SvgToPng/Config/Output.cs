using System;
using System.Collections.Generic;
using System.Text;

namespace SvgToPng.Config
{
    public class Output
    {
        public string Path { get; set; }
        public ColorConversion[] ColorConversions { get; set; }
        public float? Width { get; set; }
        public float? Height { get; set; }
    }
}

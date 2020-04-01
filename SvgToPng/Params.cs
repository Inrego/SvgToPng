using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SvgToPng
{
    public class Params
    {
        public string InputFile { get; set; }
        public DirectoryInfo OutputDirectory { get; set; }
        public FileInfo ProfileConfig { get; set; }
    }
}

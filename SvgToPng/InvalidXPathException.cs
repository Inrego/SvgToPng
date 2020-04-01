using System;
using System.Collections.Generic;
using System.Text;

namespace SvgToPng
{
    public class InvalidXPathException : Exception
    {
        public string XPath { get; }
        public InvalidXPathException(string xpath)
        {
            XPath = xpath;
        }
    }
}

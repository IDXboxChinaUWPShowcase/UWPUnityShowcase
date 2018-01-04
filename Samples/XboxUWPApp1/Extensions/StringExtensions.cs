using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XboxUWPApp1.Extensions
{
    public static class StringExtensions
    {
        public static string WriteMessageLine(this string s, string str)
        {
            s += str + "\n";
            return s;
        }
    }
}

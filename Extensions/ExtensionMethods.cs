using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServerCSharp.Extensions
{
    public static class ExtensionMethods
    {
        public static string RightOf(this string src, char c)
        {
            return src.Substring(src.IndexOf(c)+1);
        }
        public static string RightOf(this string src, string substring)
        {
            string ret = src;
            int idx = src.IndexOf(substring);
            if (idx != -1)
            {
                ret = src.Substring(idx + substring.Length);
            }
            return ret;
        }
        public static string RightOfRightmostOf(this string src, char c)
        {
            string ret = src;
            int idx = src.LastIndexOf(c);
            if (idx != -1)
            {
                ret = src.Substring(idx+1);
            }
            return ret;

        }
        public static string LeftOfRightmostOf(this string src, string substring)
        {
            string ret = src ;
            int idx = src.LastIndexOf(substring);
            if (idx != -1)
            {
                ret = src.Substring (0, idx);
            }
            return ret;
        }
    }
}

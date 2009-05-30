using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MS3DProcessor
{
    public static class ExtensionMethods
    {
        public static String ReadFixedLengthString(this BinaryReader bin, int length)
        {
            char[] ch = bin.ReadChars(length);
            String ret = new String(ch);
            if (ret.IndexOf('\0') >= 0)
                ret = ret.Substring(0, ret.IndexOf('\0'));
            return ret;
        }
    }
}

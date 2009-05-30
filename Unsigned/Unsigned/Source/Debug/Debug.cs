using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unsigned
{
    public static class Debug
    {
        public static void Error(String message)
        {
            Console.WriteLine(message);
        }

        public static void Warning(String message)
        {
            Console.WriteLine(message);
        }

        public static void Error(String message, Exception e)
        {
            Console.WriteLine(message);
            Console.WriteLine(e.StackTrace);
            throw e;
        }

        public static void Warning(String message, Exception e)
        {
            Console.WriteLine(message);
        }
    }
}

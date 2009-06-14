using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Unsigned
{
    public static class Debug
    {
        public static void Error(String message)
        {
            Console.WriteLine(message);
            Logger.WriteLine("Error: " + message);
        }

        public static void Warning(String message)
        {
            Console.WriteLine(message);
            Logger.WriteLine("Warning: " + message);
        }

        public static void Error(String message, Exception e)
        {
            Console.WriteLine(message);
            Console.WriteLine(e.StackTrace);
            Logger.WriteLine("Error: " + message);
            Logger.WriteLine(e.StackTrace);
            throw e;
        }

        public static void Warning(String message, Exception e)
        {
            Console.WriteLine(message);
            Logger.WriteLine("Warning: " + message);
            Logger.WriteLine(e.StackTrace);
        }
    }

    public static class Logger
    {
        public static String LogFilename = "Log.txt";
        private static List<String> lines;

        public static void WriteLine(String line)
        {
            if (lines == null)
                lines = new List<String>();
            lines.Add(line);
        }

        public static void Save()
        {
            if (lines != null)
            {
                StreamWriter sw = new StreamWriter(LogFilename,true);

                sw.WriteLine("Error Log (" + DateTime.Now.Month + "." + DateTime.Now.Day + "." + DateTime.Now.Year + " " + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second);

                sw.WriteLine();

                for (int i = 0; i < lines.Count; i++)
                    sw.WriteLine(lines[i]);

                sw.WriteLine();
                sw.WriteLine();

                sw.Close();
            }
        }
    }
}

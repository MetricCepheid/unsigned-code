using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Unsigned
{
    public class Localizer
    {
        public enum Language
        {
            ENGLISH=0,
            FRENCH,
            SPANISH,
            GERMAN,

            Length,
        };
        public static String[] LangCodes = { "EN", "FR", "ES", "DE", };
        private static String[][] localized;

        public static void Load()
        {
            localized = new String[(int)Language.Length][];
            for (Language i = 0; i < Language.Length; i++)
            {
                StreamReader fin = null;
                fin = new StreamReader(File.OpenRead("Content\\text\\Unsigned_" + LangCodes[(int)i] + ".txt"));

                try
                {
                    List<String> strs = new List<String>();
                    while (!fin.EndOfStream)
                        strs.Add(fin.ReadLine());
                    localized[(int)i] = strs.ToArray();
                }
                finally
                {
                    fin.Close();
                }
            }
        }

        public static String Get(String index)
        {
            for (int i = 0; i < localized[0].Length; i++)
            {
                if (localized[(int)Language.ENGLISH][i].ToLower().Equals(index.ToLower()))
                    return localized[(int)Configuration.CurrentLanguage][i];
            }
#if DEBUG
            return "INVALID";
#else
            return index;
#endif
        }
    }
}

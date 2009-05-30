using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SongDataIO
{
    public class InstrumentMaster
    {
        private static InstrumentMaster _singleton = null;
        public static InstrumentMaster Singleton
        {
            get
            {
                if (_singleton == null)
                    _singleton = new InstrumentMaster();
                return _singleton;
            }
        }

        public bool IsLoaded { get; private set; }

        private List<Instrument> instruments;

        private InstrumentMaster()
        {
            instruments = new List<Instrument>();
            IsLoaded = false;
        }

        public void Load(String directory)
        {
            String[] files = Directory.GetFiles(directory, "*.txt");
            for (int i = 0; i < files.Length; i++)
            {
                Instrument instr = new Instrument();
                StreamReader bin = new StreamReader(File.OpenRead(files[i]));
                while (!bin.EndOfStream)
                {
                    String line = bin.ReadLine();
                    if (line.Contains("="))
                    {
                        String left = line.Substring(0, line.IndexOf('=')).Trim();
                        String right = line.Substring(line.IndexOf('=') + 1).Trim();
                        //if(left.ToLower().Equals("boardbump"))
                        //handle commas
                        instr.SetValue(left, right);
                    }
                }
                bin.Close();

                instruments.Add(instr);
            }
            IsLoaded = true;
        }

        public Instrument GetInstrument(int index)
        {
            return instruments[index];
        }

        public int GetNumInstruments()
        {
            return instruments.Count;
        }

        public Instrument GetInstrument(String codename)
        {
            for (int i = 0; i < instruments.Count; i++)
                if (instruments[i].CodeName.Equals(codename.ToUpper()))
                    return instruments[i];
            throw new IndexOutOfRangeException();
        }
    }
}

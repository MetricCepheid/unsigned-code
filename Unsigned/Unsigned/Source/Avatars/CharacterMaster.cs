using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using SongDataIO;

namespace Unsigned
{
    /// <summary>
    /// Represents a set of data to construct the
    /// appearance for a rocker
    /// </summary>
    public class CharacterIdol
    {
        public String Name, Filename;
        public String InstrumentBrand;
        public int InstrumentIndex;

        internal static CharacterIdol RandomIdol(Instrument instr)
        {
            CharacterIdol randIdol = new CharacterIdol();
            randIdol.InstrumentBrand = "[none]";
            randIdol.InstrumentIndex = -1;
            if (instr.CodeName == "LGT")
            {
                randIdol.InstrumentBrand = "Gobsin";
                randIdol.InstrumentIndex = 0;
            }
            if (instr.CodeName == "BAS")
            {
                randIdol.InstrumentBrand = "Itanex";
                randIdol.InstrumentIndex = 0;
            }
            if (instr.CodeName == "SET")
            {
                randIdol.InstrumentBrand = "Yahama";
                randIdol.InstrumentIndex = 0;
            }
            if (instr.CodeName == "LVX")
            {
                randIdol.InstrumentBrand = "FVProductions";
                randIdol.InstrumentIndex = 0;
            }
            randIdol.Name = "GenericRocker";
            randIdol.Filename = null;
            return randIdol;
        }
    }

    public class CharacterMaster
    {
        private static CharacterMaster _singleton = null;
        public static CharacterMaster Singleton
        {
            get
            {
                if (_singleton == null)
                    _singleton = new CharacterMaster();
                return _singleton;
            }
        }

        private List<CharacterIdol> idols;

        private CharacterMaster()
        {
            if (Directory.Exists("characters"))
            {
                String[] files = Directory.GetFiles("characters\\", "*.unc");
                List<CharacterIdol> list = new List<CharacterIdol>();
                for (int i = 0; i < files.Length; i++)
                {
                    CharacterIdol idol = new CharacterIdol();
                    BinaryReader bin = new BinaryReader(File.OpenRead(files[i]));
                    if (bin.ReadChar() == 'U' && bin.ReadChar() == 'N' && bin.ReadChar() == 'C')
                    {
                        idol.Filename = files[i];
                        idol.Name = bin.ReadString();
                        list.Add(idol);
                    }
                    bin.Close();
                }
                idols = list;
            }
            else
                idols = new List<CharacterIdol>();
        }

        public CharacterIdol GetCharacter(int index)
        {
            return idols[index];
        }

        public int GetNumCharacters()
        {
            return idols.Count;
        }

        public CharacterIdol GetCharacter(String index)
        {
            for (int i = 0; i < idols.Count; i++)
                if (idols[i].Name.Equals(index))
                    return idols[i];
            throw new IndexOutOfRangeException();
        }
    }
}
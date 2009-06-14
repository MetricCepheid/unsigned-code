using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using FVProductions.Utility;
using SongDataIO;

namespace Unsigned
{
    /// <summary>
    /// Represents a set of data to construct the
    /// appearance for a rocker
    /// </summary>
    public class CharacterIdol
    {
        public String Name;
        public String InstrumentBrand;
        public int InstrumentIndex;
        private IdolColor _sbgc, _sfgc, _nc;
        public IdolColor ShirtBGColor { get { return _sbgc; } set { _sbgc = (IdolColor)Math.Max(0, Math.Min(IdolColors.Length - 1, (int)value)); } }
        public IdolColor ShirtFGColor { get { return _sfgc; } set { _sfgc = (IdolColor)Math.Max(0, Math.Min(IdolColors.Length - 1, (int)value)); } }
        public IdolColor NecklaceColor { get { return _nc; } set { _nc = (IdolColor)Math.Max(0, Math.Min(IdolColors.Length - 1, (int)value)); } }
        private int _dsti, _pti, _hti, _nli, _pnti, _sfgti, _shti, _skti;
        public int DrumstickTexIndex { get { return _dsti; } set { _dsti = Math.Max(0, Math.Min(DRUMSTICK_NUM_TEXS - 1, value)); } }
        public int PickTexIndex { get { return _pti; } set { _pti = Math.Max(0, Math.Min(PICK_NUM_TEXS - 1, value)); } }
        public int HairTexIndex { get { return _hti; } set { _hti = Math.Max(0, Math.Min(HAIR_NUM_TEXS - 1, value)); } }
        public int NecklaceTexIndex { get { return _nli; } set { _nli = Math.Max(0, Math.Min(NECKLACE_NUM_TEXS - 1, value)); } }
        public int PantsTexIndex { get { return _pnti; } set { _pnti = Math.Max(0, Math.Min(PANTS_NUM_TEXS - 1, value)); } }
        public int ShirtFGTexIndex { get { return _sfgti; } set { _sfgti = Math.Max(0, Math.Min(SHIRT_FG_NUM_TEXS - 1, value)); } }
        public int ShoeTexIndex { get { return _shti; } set { _shti = Math.Max(0, Math.Min(SHOE_NUM_TEXS - 1, value)); } }
        public int SkinTexIndex { get { return _skti; } set { _skti = Math.Max(0, Math.Min(SKIN_NUM_TEXS - 1, value)); } }

        public const int DRUMSTICK_NUM_TEXS = 1;
        public const int PICK_NUM_TEXS = 1;
        public const int HAIR_NUM_TEXS = 5;
        public const int NECKLACE_NUM_TEXS = 1;
        public const int PANTS_NUM_TEXS = 2;
        public const int SHIRT_FG_NUM_TEXS = 1;
        public const int SHOE_NUM_TEXS = 1;
        public const int SKIN_NUM_TEXS = 7;

        public static Color[] IdolColors = 
        { 
            Color.White, 
            Color.Black, 
            Color.Red,
            Color.Orange,
            Color.Yellow,
            Color.GreenYellow,
            Color.Lime,
            Color.Aqua,
            Color.Blue,
            Color.Purple,
        };
        public enum IdolColor
        {
            White,
            Black,
            Red,
            Orange,
            Yellow,
            YellowGreen,
            Green,
            Aqua,
            Blue,
            Purple,
        }

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
            randIdol.Name = "Unnamed";
            randIdol.ShirtBGColor = (IdolColor)Global.Random.Next(IdolColors.Length);
            randIdol.ShirtFGColor = (IdolColor)Global.Random.Next(IdolColors.Length);
            randIdol.NecklaceColor = (IdolColor)Global.Random.Next(IdolColors.Length);
            randIdol.DrumstickTexIndex = Global.Random.Next(DRUMSTICK_NUM_TEXS);
            randIdol.HairTexIndex = Global.Random.Next(HAIR_NUM_TEXS);
            randIdol.NecklaceTexIndex = Global.Random.Next(NECKLACE_NUM_TEXS);
            randIdol.PantsTexIndex = Global.Random.Next(PANTS_NUM_TEXS);
            randIdol.PickTexIndex = Global.Random.Next(PICK_NUM_TEXS);
            randIdol.ShirtFGTexIndex = Global.Random.Next(SHIRT_FG_NUM_TEXS);
            randIdol.ShoeTexIndex = Global.Random.Next(SHOE_NUM_TEXS);
            randIdol.SkinTexIndex = Global.Random.Next(SKIN_NUM_TEXS);
            return randIdol;
        }

        public void Save()
        {
            BinaryWriter bw = new BinaryWriter(File.OpenWrite("Configuration\\Idols\\" + Name + ".idl"));
            bw.Write((byte)0);//version
            bw.Write(Name);
            bw.Write(InstrumentBrand);
            bw.Write(InstrumentIndex);
            bw.Write((int)ShirtBGColor);
            bw.Write((int)ShirtFGColor);
            bw.Write((int)NecklaceColor);
            bw.Write(DrumstickTexIndex);
            bw.Write(HairTexIndex);
            bw.Write(NecklaceTexIndex);
            bw.Write(PantsTexIndex);
            bw.Write(PickTexIndex);
            bw.Write(ShirtFGTexIndex);
            bw.Write(ShoeTexIndex);
            bw.Write(SkinTexIndex);
            bw.Close();
        }

        public static CharacterIdol Load(String Filename)
        {
            CharacterIdol idol = new CharacterIdol();
            BinaryReader br = new BinaryReader(File.OpenRead(Filename));

            if (br.ReadByte() == 0)
            {
                idol.Name = br.ReadString();
                idol.InstrumentBrand = br.ReadString();
                idol.InstrumentIndex = br.ReadInt32();
                idol.ShirtBGColor = (IdolColor)br.ReadInt32();
                idol.ShirtFGColor = (IdolColor)br.ReadInt32();
                idol.NecklaceColor = (IdolColor)br.ReadInt32();
                idol.DrumstickTexIndex = br.ReadInt32();
                idol.HairTexIndex = br.ReadInt32();
                idol.NecklaceTexIndex = br.ReadInt32();
                idol.PantsTexIndex = br.ReadInt32();
                idol.PickTexIndex = br.ReadInt32();
                idol.ShirtFGTexIndex = br.ReadInt32();
                idol.ShoeTexIndex = br.ReadInt32();
                idol.SkinTexIndex = br.ReadInt32();
            }

            br.Close();

            return idol;
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
            if (Directory.Exists("Configuration\\Idols\\"))
            {
                String[] files = Directory.GetFiles("Configuration\\Idols\\", "*.idl");
                List<CharacterIdol> list = new List<CharacterIdol>();
                for (int i = 0; i < files.Length; i++)
                {
                    list.Add(CharacterIdol.Load(files[i]));
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

        internal void AddCharacter(CharacterIdol idol)
        {
            idols.Add(idol);
            Save();
        }

        public void Save()
        {
            if (!Directory.Exists("Configuration\\Idols\\"))
                Directory.CreateDirectory("Configuration\\Idols");
            for (int i = 0; i < idols.Count; i++)
                idols[i].Save();
        }
    }
}
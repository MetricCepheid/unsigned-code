using System;
using System.Collections.Generic;
using System.Text;

namespace VocalEditor
{
    public struct Bar
    {
        public uint time;
        public int numBeats;
    }
    public struct Harmony
    {
        public uint start, end;
        public byte instruments;
    }
    public class VocalWord : IComparable
    {
        public uint time;
        public uint len;
        public short startNote, endNote;
        public String value;
        public bool connected;

        public int CompareTo(Object other)
        {
            return time.CompareTo(((VocalWord)other).time);
        }
    }
    public class VocalPhrase : IComparable
    {
        public static byte REGULAR = 0, BLANK = 1, RHYTHM = 2;
        public uint time;
        public byte typ;
        public bool overdrive;
        public List<VocalWord> words;
        public byte rhythmType;

        public int CompareTo(Object other)
        {
            return time.CompareTo(((VocalPhrase)other).time);
        }
        public VocalPhrase()
        {
            words = new List<VocalWord>();
        }
    }

    public class Song
    {
        public static int SONGDATA_VERSION=17;
        public bool valid;
        public String songName, artistName, genre, formattedTime;
        public int year;
        public String[] quotes = new string[8];
        public String[] charters = new string[6];
        public byte[] diff = new byte[4];
        public Bar[] bars;
        public int trailingLen;
        public bool hasBRE;
        public uint breStart, breEnd;
        public Harmony[] harmonies;
        public List<VocalPhrase> notes;

        public Song(String filename)
        {
            valid = true;
            LoadUNS(filename);
        }

        private void LoadUNS(String fn)
        {
            if (!System.IO.File.Exists(fn))
            { valid = false; System.Windows.Forms.MessageBox.Show("File DNE\n" + fn); return; }
            if (!fn.ToLower().EndsWith(".uns"))
                return;
            System.IO.BinaryReader bin = null;
            try
            {
                bin = new System.IO.BinaryReader(System.IO.File.OpenRead(fn));

                bin.ReadChars(3);//UNS
                uint[] offsets = new uint[6];
                for(int i=0;i<6;i++)
                    offsets[i] = bin.ReadUInt32();
                byte version = bin.ReadByte();
                if (version != SONGDATA_VERSION)
                { valid = false; bin.Close();  System.Windows.Forms.MessageBox.Show("File INV\n" + fn); return; }
                songName = bin.ReadString();
                artistName = bin.ReadString();
                year = bin.ReadInt32();
                genre = bin.ReadString();
                formattedTime = bin.ReadString();
                for (int i = 0; i < 8; i++)
                    quotes[i] = bin.ReadString();
                for (int i = 0; i < 6; i++)
                    charters[i] = bin.ReadString();
                for (int i = 0; i < 4; i++)
                    diff[i] = bin.ReadByte();
                bars = new Bar[bin.ReadInt32()];
                for (int i = 0; i < bars.Length; i++)
                {
                    bars[i].time = bin.ReadUInt32();
                    bars[i].numBeats = bin.ReadInt32();
                }
                trailingLen = bin.ReadInt32();
                hasBRE = bin.ReadBoolean();
                breStart = bin.ReadUInt32();
                breEnd = bin.ReadUInt32();
                harmonies = new Harmony[bin.ReadUInt32()];
                for (int i = 0; i < harmonies.Length; i++)
                {
                    harmonies[i].start = bin.ReadUInt32();
                    harmonies[i].end = bin.ReadUInt32();
                    harmonies[i].instruments = bin.ReadByte();
                }

                //go to vocals
                bin.ReadBytes((int)(offsets[4] - offsets[1]));

                int numN = bin.ReadInt32();
                notes = new List<VocalPhrase>();
                for (int i = 0; i < numN; i++)
                {
                    VocalPhrase vp = new VocalPhrase();
                    vp.time = bin.ReadUInt32();
                    vp.typ = bin.ReadByte();
                    vp.overdrive = bin.ReadBoolean();
                    int numW = bin.ReadInt32();
                    if (vp.typ == VocalPhrase.RHYTHM)
                        vp.rhythmType = bin.ReadByte();
                    for (int k = 0; k < numW; k++)
                    {
                        VocalWord w = new VocalWord();
                        w.time = bin.ReadUInt32();
                        if (vp.typ == VocalPhrase.REGULAR)
                        {
                            w.len = bin.ReadUInt32();
                            w.startNote = bin.ReadInt16();
                            w.endNote = bin.ReadInt16();
                            w.value = bin.ReadString();
                        }
                        vp.words.Add(w);
                    }
                    vp.words.Sort();
                    notes.Add(vp);
                }
                notes.Sort();
            }
            finally
            {
                bin.Close();
            }
        }

        private void Save(String fn)
        {

        }
    }
}

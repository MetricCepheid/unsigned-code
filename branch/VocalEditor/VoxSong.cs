using System;
using System.Collections.Generic;
using System.Text;

namespace VocalEditor
{
    public class VoxSong
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
            public uint end
            {
                get { return time + len; }
            }

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
            public byte rhythmType;

            public int CompareTo(Object other)
            {
                return time.CompareTo(((VocalPhrase)other).time);
            }
            public VocalPhrase()
            {
            }
        }

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
        public List<VocalWord> words;

        /*public VoxSong(String filename)
        {
            valid = true;
            LoadUNS(filename);
        }*/
        private VoxSong()
        {

        }

        public static VoxSong FromSongData(SongData songdata)
        {
            VoxSong song = new VoxSong();

            return song;
        }

        private void Save(String fn)
        {

        }
    }
}

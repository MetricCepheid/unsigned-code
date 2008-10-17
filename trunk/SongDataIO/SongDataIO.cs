using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SongDataIO
{
    /// <summary>
    /// Contains all data in a songdata file(s)
    /// </summary>
    public class SongData
    {
        public struct Color
        {
            public byte R, G, B, A;

            public Color(byte r, byte g, byte b)
            {
                R = r;
                G = g;
                B = b;
                A = 255;
            }

            public Color(byte r, byte g, byte b, byte a)
            {
                R = r;
                G = g;
                B = b;
                A = a;
            }
        }

        public class NoteSet : IComparable
        {
            public uint time, length;
            public ulong type, endtype;
            public String text;
            public uint end
            {
                get { return time + length; }
            }

            public VIS_STATE[] visible;//0=visible,1=greyedout,2=invisible,3=invisibleButAvailable(HOPO)
            public bool burning;//for held Notes

            public uint late;

            public enum VIS_STATE { VISIBLE = 0, GREYED_OUT = 1, INVISIBLE = 2, HOPOED = 3, OVERDONE = 4/*drums*/, };

            private bool strummed;
            private ulong pressed;
            private bool good;

            public static bool IsValidFrettage(ulong note, ulong pressed, Instrument instr)
            {
                ulong NOT_HOPO_VALUE = ~(((ulong)1) << instr.NumTracks);
                note &= NOT_HOPO_VALUE;
                int numNotes = 0;
                for (int i = 0; i < instr.NumTracks; i++)
                    if ((note & (((ulong)1) << ((int)i))) != 0)
                        numNotes++;
                if (numNotes > 1 && pressed == note)
                    return true;
                else if (numNotes > 1)
                    return false;
                if ((note & pressed) != note)
                    return false;
                for (int i = 4; i >= 0; i--)
                {
                    if ((note & (((ulong)1) << i)) != 0)
                        return true;
                    if ((pressed & (((ulong)1) << i)) != 0)
                        return false;
                }
                return false;
            }

            public void Strum()
            {
                strummed = true;
            }

            public void AddPressedGuitar(Instrument instr, ulong pressed)
            {
                if (IsValidFrettage(type, pressed, instr))
                    good = true;
            }

            public ulong AddPressedDrums(Instrument instr, ulong pressed)
            {
                ulong ret = 0;
                if (pressed == 0)
                    return 0;
                for (int i = 0; i < instr.NumTracks; i++)
                {
                    if ((pressed & (((ulong)1) << i)) != 0 && (this.pressed & (((ulong)1) << i)) == 0)
                    { ret |= (byte)(1 << i); visible[i] = VIS_STATE.INVISIBLE; }
                }
                this.pressed |= pressed;
                ret &= this.type;
                return ret;
            }

            public bool IsGood(Instrument instr, bool HOPOable)
            {
                if (!instr.NeedsStrum)
                    if (pressed != 0)
                        return (pressed) == (type);
                    else
                        return (pressed) == (type);
                if ((instr.CanHOPO && HOPOable && (type & (((ulong)1) << instr.NumTracks)) != 0) || strummed)
                    return good;
                return false;
            }

            public int CompareTo(object other)
            {
                return time.CompareTo(((NoteSet)other).time);
            }

            public void Kill()
            {
                for (int i = 0; i < visible.Length; i++)
                    visible[i] = VIS_STATE.INVISIBLE;
            }

            public bool IsHOPO(Instrument instrument)
            {
                if (!instrument.CanHOPO)
                    return false;
                return (type & (((ulong)1) << instrument.NumTracks)) != 0;
            }

            public bool HasStrummed()
            {
                return strummed;
            }
        }

        public struct Phrase
        {
            public uint time;
            public SongData.TYPE type;
            public bool rockpower;
            public SongData.RTYPE rType;
            public NoteSet[] notes;
        }

        public struct Fill
        {
            public uint time, len;
            public bool hitGreen;
            public float amount;
            public uint end
            {
                get { return time + len; }
            }

            public Fill(uint time, uint len)
            {
                this.time = time;
                this.len = len;
                hitGreen = false;
                amount = 0f;
            }
        }

        public struct RockPowerPhrase
        {
            public uint time, len;
            public bool okay;//defaulted to true, falsed when note missed
            public uint end
            {
                get { return time + len; }
            }
            public RockPowerPhrase(uint time, uint len)
            {
                this.time = time;
                this.len = len;
                okay = true;
            }
        }

        public struct Solo
        {
            public uint time, len;
            public bool okay;//defaulted to true, falsed when note missed
            public uint end
            {
                get { return time + len; }
            }
            public Solo(uint time, uint len)
            {
                this.time = time;
                this.len = len;
                okay = true;
            }
        }

        public class SpecialEffect
        {
            public uint time;
            public String type;
            public uint length;

            public uint begin { get { return time; } }
            public uint end { get { return time+length; } }
        }

        public class NormalLightingSpecialEffect : SpecialEffect
        {
            public Color color;
        }

        public struct EffectsTrack
        {
            public uint[] cameraSwitches;
            public SpecialEffect[] effects;
        }
        
        public struct Barline
        {
            public uint time;
            public uint numBeats;
            public Barline(uint time, uint numBeats)
            {
                this.time = time;
                this.numBeats = numBeats;
            }
        }

        public struct BigRockEnding
        {
            public bool enabled;
            public uint start, end;
        }

        public struct Harmony
        {
            public uint start, end;
            public int instruments;
            public Harmony(uint start, uint end, int instruments)
            {
                this.start = start;
                this.end = end;
                this.instruments = instruments;
            }
        }

        public class FullBandChunk
        {
            public byte version;
            public String filename, name, artist;
            public uint year;
            public String genre;
            public TimeSpan length;
            public String[] quotes;
            public String[] charters;
            public byte[] difficulties;
            public Barline[] barlines;
            public uint trailingBeatLen;
            public BigRockEnding bre;
            public Harmony[] harmonies;

            public String[] SongDisplayInfo;

            public void GenerateSongDisplayInfo()
            {
                SongDisplayInfo = new string[3 + charters.Length];
                SongDisplayInfo[0] = name;
                SongDisplayInfo[1] = artist;
                SongDisplayInfo[2] = "Charter" + (charters.Length > 1 ? "s:" : ":");
                for (int i = 0; i < charters.Length; i++)
                    SongDisplayInfo[i + 3] = charters[i];
            }
        }

        public class DifficultySet
        {
            public int diff;
            public SongData.Phrase[] phrases;
            public uint[] starScoreLevels;
        }

        public class SongDataInstrument
        {
            public String instrumentType;
            public SongData.RockPowerPhrase[] rpPhrases;
            public SongData.Solo[] solos;
            public SongData.Fill[] fills;
            public DifficultySet[] diffSets;
        }

        public enum TYPE { REGULAR = 0, BLANK = 1, RHYTHM = 2 };
        public enum RTYPE { TAMBOURINE = 0, COWBELL = 1, CLAP = 2 };

        public FullBandChunk info;
        public SongDataInstrument[] instruments;
        public EffectsTrack effects;

        public SongData()
        {
            info = new FullBandChunk();
            info.quotes = new string[8];
            effects = new EffectsTrack();
        }
    }

    /// <summary>
    /// Loads songdata files into a SongData struct
    /// </summary>
    public class SongLoader
    {
        public static TimeSpan LengthStringToTimeSpan(String songLength)
        {
            String z = songLength;
            uint iSongLength = 0;
            iSongLength += 3600u*UInt32.Parse(z.Substring(0, z.IndexOf(':')));
            z = z.Substring(z.IndexOf(':') + 1);
            iSongLength += 60u*UInt32.Parse(z.Substring(0, z.IndexOf(':')));
            iSongLength += UInt32.Parse(z.Substring(z.IndexOf(':') + 1));
            return TimeSpan.FromSeconds(iSongLength);
        }

        private static String[] GetCharters(List<String> charters)
        {
            int numCharters = charters.Count; ;
            int[] numC = new int[numCharters];
            string[] charters2 = new string[numCharters];
            charters2[0] = charters[0];
            numC[0]++;
            int nC2 = 1;
            for (int i = 1; i < numCharters; i++)
            {
                int k;
                for (k = 0; k < nC2; k++)
                    if (charters[i].Equals(charters2[k]))
                    {
                        numC[k]++;
                        break;
                    }
                if (k >= nC2)
                {
                    charters2[k] = charters[i];
                    numC[k]++;
                    nC2++;
                }
            }
            if (nC2 > 2)
            {
                for (int i = 2; i < nC2; i++)
                {
                    for (int k = i - 1; k >= 1; k--)
                    {
                        if (numC[k] > numC[k + 1])
                        {
                            int t = numC[k];
                            numC[k] = numC[k + 1];
                            numC[k + 1] = t;
                            string s = charters2[k];
                            charters2[k] = charters2[k + 1];
                            charters2[k + 1] = s;
                        }
                        else
                            break;
                    }
                }
            }
            String[] ret = new String[nC2];
            for (int i = 0; i < nC2; i++)
                ret[i] = charters2[i];
            return ret;
        }

        public static SongData LoadSong12(String fn)
        {
            SongData ret = new SongData();

            String dir = "";

            if (fn.Contains("\\") || fn.Substring(0,fn.Length-1).ToLower().EndsWith(".gb"))
            {
                dir = fn.Substring(0, fn.LastIndexOf('\\') + 1);
                fn = fn.Substring(fn.LastIndexOf('\\') + 1);
                fn = fn.Substring(0, fn.LastIndexOf('.'));
            }
            ret.info.filename = fn;
            if (!File.Exists(dir + fn + ".gba"))
            {
#if WINDOWS
                System.Windows.Forms.MessageBox.Show("songdata not found");
#endif
                return null;
            }
            BinaryReader reader = new BinaryReader(File.OpenRead(dir + fn + ".gba"));
            ret.info.version = reader.ReadByte();
            if (ret.info.version < 12)
            {
#if WINDOWS
                System.Windows.Forms.MessageBox.Show("SongData Version too old");
#endif
                return null;
            }
            ret.info.name = reader.ReadString();
            ret.info.artist = reader.ReadString();
            ret.info.year = reader.ReadUInt32();
            ret.info.genre = reader.ReadString();
            String lengthString = reader.ReadString();
            ret.info.length = LengthStringToTimeSpan(lengthString);
            ret.info.quotes = new string[8];
            for (int i = 0; i < ret.info.quotes.Length; i++)
                ret.info.quotes[i] = reader.ReadString();

            int numCharters = 6;
            List<String> chtemp = new List<String>();
            for (int i = 0; i < numCharters; i++)
                chtemp.Add(reader.ReadString());
            ret.info.charters = GetCharters(chtemp);

            ret.info.difficulties = new byte[4];
            for (int i = 0; i < 4; i++)
                ret.info.difficulties[i] = reader.ReadByte();
            ret.info.barlines = new SongData.Barline[reader.ReadInt32()];
            for (int c = 0; c < ret.info.barlines.Length; c++)
            {
                ret.info.barlines[c] = new SongData.Barline(reader.ReadUInt32(), reader.ReadUInt32());
            }
            ret.info.trailingBeatLen = reader.ReadUInt32();

            reader.Close();

            ret.info.harmonies = new SongData.Harmony[0];

            ret.instruments = new SongData.SongDataInstrument[4];

            reader = new BinaryReader(File.OpenRead(dir + fn + ".gbg"));

            reader.ReadByte();//version
            {
                SongData.SongDataInstrument guitar = new SongData.SongDataInstrument();
                guitar.instrumentType = "LGT";

                guitar.rpPhrases = new SongData.RockPowerPhrase[reader.ReadInt32()];
                for (int i = 0; i < guitar.rpPhrases.Length; i++)
                {
                    guitar.rpPhrases[i] = new SongData.RockPowerPhrase();
                    guitar.rpPhrases[i].time = reader.ReadUInt32();
                    guitar.rpPhrases[i].len = reader.ReadUInt32();
                }
                guitar.solos = new SongData.Solo[0];
                guitar.diffSets = new SongData.DifficultySet[4];
                for (int i = 0; i < 4; i++)
                {
                    int k = reader.ReadInt32();
                    guitar.diffSets[k] = new SongData.DifficultySet();
                    guitar.diffSets[k].diff = k;
                    guitar.diffSets[k].phrases = new SongData.Phrase[1];
                    guitar.diffSets[k].phrases[0] = new SongData.Phrase();
                    guitar.diffSets[k].phrases[0].notes = new SongData.NoteSet[reader.ReadInt32()];
                    for (int j = 0; j < guitar.diffSets[k].phrases[0].notes.Length; j++)
                    {
                        guitar.diffSets[k].phrases[0].notes[j] = new SongData.NoteSet();
                        guitar.diffSets[k].phrases[0].notes[j].type = reader.ReadByte();
                        guitar.diffSets[k].phrases[0].notes[j].time = reader.ReadUInt32();
                        guitar.diffSets[k].phrases[0].notes[j].length = reader.ReadUInt32();
                    }
                    guitar.diffSets[k].starScoreLevels = new uint[6];
                    for (int j = 0; j < 5; j++)
                        guitar.diffSets[k].starScoreLevels[j] = reader.ReadUInt32();
                }
                reader.Close();

                ret.instruments[0] = guitar;
            }

            reader = new BinaryReader(File.OpenRead(dir + fn + ".gbb"));

            reader.ReadByte();//version

            SongData.SongDataInstrument bass = new SongData.SongDataInstrument();
            bass.instrumentType = "BAS";

            bass.rpPhrases = new SongData.RockPowerPhrase[reader.ReadInt32()];
            for (int i = 0; i < bass.rpPhrases.Length; i++)
            {
                bass.rpPhrases[i] = new SongData.RockPowerPhrase(reader.ReadUInt32(), reader.ReadUInt32());
            }
            bass.diffSets = new SongData.DifficultySet[4];
            for (int i = 0; i < 4; i++)
            {
                int k = reader.ReadInt32();
                bass.diffSets[k] = new SongData.DifficultySet();
                bass.diffSets[k].phrases = new SongData.Phrase[1];
                bass.diffSets[k].phrases[0].notes = new SongData.NoteSet[reader.ReadInt32()];
                SongData.NoteSet[] arr = bass.diffSets[k].phrases[0].notes;
                for (int j = 0; j < arr.Length; j++)
                {
                    arr[j] = new SongData.NoteSet();
                    arr[j].type = reader.ReadByte();
                    arr[j].time = reader.ReadUInt32();
                    arr[j].length = reader.ReadUInt32();
                }
                bass.diffSets[k].starScoreLevels = new uint[6];
                for (int j = 0; j < 5; j++)
                    bass.diffSets[k].starScoreLevels[j] = reader.ReadUInt32();
            }
            reader.Close();

            ret.instruments[3] = bass;

            reader = new BinaryReader(File.OpenRead(dir + fn + ".gbd"));

            SongData.SongDataInstrument drums = new SongData.SongDataInstrument();
            drums.instrumentType = "SET";

            reader.ReadByte();//version
            drums.rpPhrases = new SongData.RockPowerPhrase[reader.ReadInt32()];
            for (int i = 0; i < drums.rpPhrases.Length; i++)
            {
                drums.rpPhrases[i] = new SongData.RockPowerPhrase(reader.ReadUInt32(), reader.ReadUInt32());
            }
            drums.fills = new SongData.Fill[reader.ReadInt32()];
            for (int i = 0; i < drums.fills.Length; i++)
            {
                drums.fills[i] = new SongData.Fill(reader.ReadUInt32(), reader.ReadUInt32());
            }
            drums.diffSets = new SongData.DifficultySet[4];
            for (int i = 0; i < 4; i++)
            {
                int k = reader.ReadInt32();
                drums.diffSets[k] = new SongData.DifficultySet();
                drums.diffSets[k].phrases = new SongData.Phrase[1];
                drums.diffSets[k].phrases[0] = new SongData.Phrase();
                drums.diffSets[k].phrases[0].notes = new SongData.NoteSet[reader.ReadInt32()];
                SongData.NoteSet[] arr = drums.diffSets[k].phrases[0].notes;
                for (int j = 0; j < arr.Length; j++)
                {
                    arr[j] = new SongData.NoteSet();
                    arr[j].type = reader.ReadByte();
                    arr[j].time = reader.ReadUInt32();
                }
                drums.diffSets[k].starScoreLevels = new uint[6];
                for (int j = 0; j < 5; j++)
                    drums.diffSets[k].starScoreLevels[j] = reader.ReadUInt32();
            }
            reader.Close();

            ret.instruments[2] = drums;

            reader = new BinaryReader(File.OpenRead(dir + fn + ".gbv"));

            SongData.SongDataInstrument vocals = new SongData.SongDataInstrument();

            reader.ReadByte();//version

            vocals.instrumentType = "LVX";
            vocals.rpPhrases = new SongData.RockPowerPhrase[0];
            vocals.fills = new SongData.Fill[0];
            vocals.diffSets = new SongData.DifficultySet[1];
            vocals.diffSets[0] = new SongData.DifficultySet();
            vocals.diffSets[0].phrases = new SongData.Phrase[reader.ReadUInt32()];
            for (int i = 0; i < vocals.diffSets[0].phrases.Length; i++)
            {
                vocals.diffSets[0].phrases[i] = new SongData.Phrase();
                vocals.diffSets[0].phrases[i].time = reader.ReadUInt32();
                vocals.diffSets[0].phrases[i].type = (SongData.TYPE)reader.ReadByte();
                vocals.diffSets[0].phrases[i].notes = new SongData.NoteSet[reader.ReadInt32()];
                if (vocals.diffSets[0].phrases[i].type == SongData.TYPE.REGULAR)
                {
                    for (int k = 0; k < vocals.diffSets[0].phrases[i].notes.Length; k++)
                    {
                        vocals.diffSets[0].phrases[i].notes[k] = new SongData.NoteSet();
                        vocals.diffSets[0].phrases[i].notes[k].time = reader.ReadUInt32();
                        vocals.diffSets[0].phrases[i].notes[k].length = reader.ReadUInt32() - vocals.diffSets[0].phrases[i].notes[k].time;
                        vocals.diffSets[0].phrases[i].notes[k].type = (ulong)reader.ReadInt16();
                        vocals.diffSets[0].phrases[i].notes[k].endtype = vocals.diffSets[0].phrases[i].notes[k].type;
                        vocals.diffSets[0].phrases[i].notes[k].text = reader.ReadString();
                    }
                }
                else if (vocals.diffSets[0].phrases[i].type == SongData.TYPE.RHYTHM)
                {
                    vocals.diffSets[0].phrases[i].rType = (SongData.RTYPE)reader.ReadByte();
                    for (int k = 0; k < vocals.diffSets[0].phrases[i].notes.Length; k++)
                    {
                        vocals.diffSets[0].phrases[i].notes[k] = new SongData.NoteSet();
                        vocals.diffSets[0].phrases[i].notes[k].time = reader.ReadUInt32();
                    }
                }
                else if (vocals.diffSets[0].phrases[i].type == SongData.TYPE.BLANK)
                    vocals.diffSets[0].phrases[i].notes = new SongData.NoteSet[0];
            }
            vocals.diffSets[0].starScoreLevels = new uint[6];
            for (int j = 0; j < 5; j++)
                vocals.diffSets[0].starScoreLevels[j] = reader.ReadUInt32();
            reader.Close();

            ret.instruments[1] = vocals;

            reader = new BinaryReader(File.OpenRead(dir + fn + ".gbe"));

            reader.ReadByte();//version
            ret.effects.cameraSwitches = new uint[reader.ReadInt32()];
            for (int i = 0; i < ret.effects.cameraSwitches.Length; i++)
                ret.effects.cameraSwitches[i] = reader.ReadUInt32();

            reader.Close();

            return ret;
        }

        public static SongData LoadSong17(String fn)
        {

            SongData ret = new SongData();
            String dir = "";
            if (fn.Contains("\\") || fn.ToLower().EndsWith("uns"))
            {
                dir = fn.Substring(0, fn.LastIndexOf('\\') + 1);
                fn = fn.Substring(fn.LastIndexOf('\\') + 1);
                fn = fn.Substring(0, fn.LastIndexOf('.'));
            }
            ret.info.filename = fn;

            if (!File.Exists(dir + fn + ".uns"))
            {
#if WINDOWS
                System.Windows.Forms.MessageBox.Show("songdata not found");
#endif
                return null;
            }

            BinaryReader reader = new BinaryReader(File.OpenRead("songdata\\" + fn + ".uns"));

            reader.ReadBytes(3);//UNS

            int offsetToGBA = reader.ReadInt32();
            int offsetToGBG = reader.ReadInt32();
            int offsetToGBB = reader.ReadInt32();
            int offsetToGBD = reader.ReadInt32();
            int offsetToGBV = reader.ReadInt32();
            int offsetToGBE = reader.ReadInt32();

            ret.info.version = reader.ReadByte();
            if (ret.info.version != 17)
            {
#if WINDOWS
                System.Windows.Forms.MessageBox.Show("SongData Version too old");
#endif
                return null;
            }

            ret.info.name = reader.ReadString();
            ret.info.artist = reader.ReadString();
            ret.info.year = reader.ReadUInt32();
            ret.info.genre = reader.ReadString();
            ret.info.length = LengthStringToTimeSpan(reader.ReadString());
            ret.info.quotes = new string[8];
            for (int i = 0; i < 8; i++)
                ret.info.quotes[i] = reader.ReadString();

            List<String> charters = new List<String>();
            for (int i = 0; i < 6; i++)
            {
                String str = reader.ReadString();
                if (charters.Contains(str))
                    charters.Add(str);
            }
            ret.info.charters = charters.ToArray();

            reader.ReadUInt32();//irrelevant diffs

            ret.info.barlines = new SongData.Barline[reader.ReadInt32()];
            for (int i = 0; i < ret.info.barlines.Length; i++)
                ret.info.barlines[i] = new SongData.Barline(reader.ReadUInt32(), reader.ReadUInt32());

            ret.info.trailingBeatLen = reader.ReadUInt32();

            ret.info.bre.enabled = reader.ReadBoolean();
            ret.info.bre.start = reader.ReadUInt32();
            ret.info.bre.end = reader.ReadUInt32();

            ret.info.harmonies = new SongData.Harmony[reader.ReadUInt32()];
            for (int i = 0; i < ret.info.harmonies.Length; i++)
                ret.info.harmonies[i] = new SongData.Harmony(reader.ReadUInt32(), reader.ReadUInt32(), (int)reader.ReadByte());

            ret.instruments = new SongData.SongDataInstrument[4];

            //guitar
            ret.instruments[0] = new SongData.SongDataInstrument();
            ret.instruments[0].instrumentType = "LGT";
            ret.instruments[0].rpPhrases = new SongData.RockPowerPhrase[reader.ReadUInt32()];
            for (int i = 0; i < ret.instruments[0].rpPhrases.Length; i++)
                ret.instruments[0].rpPhrases[i] = new SongData.RockPowerPhrase(reader.ReadUInt32(), reader.ReadUInt32());
            ret.instruments[0].solos = new SongData.Solo[reader.ReadUInt32()];
            for (int i = 0; i < ret.instruments[0].solos.Length; i++)
                ret.instruments[0].solos[i] = new SongData.Solo(reader.ReadUInt32(), reader.ReadUInt32());
            ret.instruments[0].diffSets = new SongData.DifficultySet[4];
            for (int ir = 0; ir < 4; ir++)
            {
                uint k = reader.ReadUInt32();
                ret.instruments[0].diffSets[k].phrases = new SongData.Phrase[1];
                ret.instruments[0].diffSets[k].phrases[0] = new SongData.Phrase();
                ret.instruments[0].diffSets[k].phrases[0].notes = new SongData.NoteSet[reader.ReadUInt32()];
                for(int i=0;i<ret.instruments[0].diffSets[k].phrases[0].notes.Length;i++)
                {
                    SongData.NoteSet note = new SongData.NoteSet();
                    note.type = (ulong)reader.ReadByte();
                    note.time = reader.ReadUInt32();
                    note.length = reader.ReadUInt32();
                    ret.instruments[0].diffSets[k].phrases[0].notes[i] = note;
                }
                ret.instruments[0].diffSets[k].starScoreLevels = new uint[6];
                for (int i = 0; i < 5; i++)
                    ret.instruments[0].diffSets[k].starScoreLevels[i] = reader.ReadUInt32();
            }


            //bass
            ret.instruments[3] = new SongData.SongDataInstrument();
            ret.instruments[3].instrumentType = "BAS";
            ret.instruments[3].rpPhrases = new SongData.RockPowerPhrase[reader.ReadUInt32()];
            for (int i = 0; i < ret.instruments[3].rpPhrases.Length; i++)
                ret.instruments[3].rpPhrases[i] = new SongData.RockPowerPhrase(reader.ReadUInt32(), reader.ReadUInt32());
            ret.instruments[3].diffSets = new SongData.DifficultySet[4];
            for (int ir = 0; ir < 4; ir++)
            {
                uint k = reader.ReadUInt32();
                ret.instruments[3].diffSets[k].phrases = new SongData.Phrase[1];
                ret.instruments[3].diffSets[k].phrases[0] = new SongData.Phrase();
                ret.instruments[3].diffSets[k].phrases[0].notes = new SongData.NoteSet[reader.ReadUInt32()];
                for (int i = 0; i < ret.instruments[3].diffSets[k].phrases[0].notes.Length; i++)
                {
                    SongData.NoteSet note = new SongData.NoteSet();
                    note.type = (ulong)reader.ReadByte();
                    note.time = reader.ReadUInt32();
                    note.length = reader.ReadUInt32();
                    ret.instruments[3].diffSets[k].phrases[0].notes[i] = note;
                }
                ret.instruments[3].diffSets[k].starScoreLevels = new uint[6];
                for (int i = 0; i < 5; i++)
                    ret.instruments[3].diffSets[k].starScoreLevels[i] = reader.ReadUInt32();
            }


            //drums
            ret.instruments[2] = new SongData.SongDataInstrument();
            ret.instruments[2].instrumentType = "SET";
            ret.instruments[2].rpPhrases = new SongData.RockPowerPhrase[reader.ReadUInt32()];
            for (int i = 0; i < ret.instruments[2].rpPhrases.Length; i++)
                ret.instruments[2].rpPhrases[i] = new SongData.RockPowerPhrase(reader.ReadUInt32(), reader.ReadUInt32());
            ret.instruments[2].fills = new SongData.Fill[reader.ReadUInt32()];
            for (int i = 0; i < ret.instruments[2].fills.Length; i++)
                ret.instruments[2].fills[i] = new SongData.Fill(reader.ReadUInt32(), reader.ReadUInt32());
            ret.instruments[2].diffSets = new SongData.DifficultySet[4];
            for (int ir = 0; ir < 4; ir++)
            {
                uint k = reader.ReadUInt32();
                ret.instruments[2].diffSets[k].phrases = new SongData.Phrase[1];
                ret.instruments[2].diffSets[k].phrases[0] = new SongData.Phrase();
                ret.instruments[2].diffSets[k].phrases[0].notes = new SongData.NoteSet[reader.ReadUInt32()];
                for (int i = 0; i < ret.instruments[2].diffSets[k].phrases[0].notes.Length; i++)
                {
                    SongData.NoteSet note = new SongData.NoteSet();
                    note.type = (ulong)reader.ReadByte();
                    note.time = reader.ReadUInt32();
                    ret.instruments[2].diffSets[k].phrases[0].notes[i] = note;
                }
                ret.instruments[2].diffSets[k].starScoreLevels = new uint[6];
                for (int i = 0; i < 5; i++)
                    ret.instruments[2].diffSets[k].starScoreLevels[i] = reader.ReadUInt32();
            }


            //vocals
            ret.instruments[1] = new SongData.SongDataInstrument();
            ret.instruments[1].instrumentType = "LVX";
            ret.instruments[1].diffSets = new SongData.DifficultySet[4];
            ret.instruments[1].diffSets[3].phrases = new SongData.Phrase[reader.ReadUInt32()];
            for (int j = 0; j < ret.instruments[1].diffSets[3].phrases.Length; j++)
            {
                ret.instruments[1].diffSets[3].phrases[j] = new SongData.Phrase();
                ret.instruments[1].diffSets[3].phrases[j].type = (SongData.TYPE)reader.ReadByte();
                ret.instruments[1].diffSets[3].phrases[j].rockpower = reader.ReadBoolean();
                ret.instruments[1].diffSets[3].phrases[j].notes = new SongData.NoteSet[reader.ReadUInt32()];
                if (ret.instruments[1].diffSets[3].phrases[j].type == SongData.TYPE.REGULAR)
                {
                    for (int i = 0; i < ret.instruments[1].diffSets[3].phrases[i].notes.Length; i++)
                    {
                        SongData.NoteSet note = new SongData.NoteSet();
                        note.time = reader.ReadUInt32();
                        note.length = reader.ReadUInt32()-note.time;
                        note.type = reader.ReadUInt16();
                        note.endtype = reader.ReadUInt16();
                        note.text = reader.ReadString();
                        ret.instruments[1].diffSets[3].phrases[j].notes[i] = note;
                    }
                }
                else if (ret.instruments[1].diffSets[3].phrases[j].type == SongData.TYPE.RHYTHM)
                {
                    ret.instruments[1].diffSets[3].phrases[j].rType = (SongData.RTYPE)reader.ReadByte();
                    for (int i = 0; i < ret.instruments[1].diffSets[3].phrases[i].notes.Length; i++)
                    {
                        SongData.NoteSet note = new SongData.NoteSet();
                        note.time = reader.ReadUInt32();
                        ret.instruments[1].diffSets[3].phrases[j].notes[i] = note;
                    }
                }
            }
            for (int ir = 0; ir < 4; ir++)
            {
                ret.instruments[1].diffSets[ir].starScoreLevels = new uint[6];
                for (int i = 0; i < 5; i++)
                    ret.instruments[1].diffSets[ir].starScoreLevels[i] = reader.ReadUInt32();
            }


            //effects
            ret.effects.effects = new SongData.SpecialEffect[0];
            ret.effects.cameraSwitches = new uint[reader.ReadUInt32()];
            for (int i = 0; i < ret.effects.cameraSwitches.Length; i++)
                ret.effects.cameraSwitches[i] = reader.ReadUInt32();

            reader.Close();

            return ret;

        }

        public static SongData LoadSong20(String filename)
        {
            SongData ret = new SongData();
            String fn = filename;
            String dir = "";
            if (fn.Contains("\\") || fn.ToLower().EndsWith("uns"))
            {
                dir = fn.Substring(0, fn.LastIndexOf('\\') + 1);
                fn = fn.Substring(fn.LastIndexOf('\\') + 1);
                fn = fn.Substring(0, fn.LastIndexOf('.'));
            }
            ret.info.filename = fn;

            if (!File.Exists(dir + fn + ".uns"))
            {
#if WINDOWS
                System.Windows.Forms.MessageBox.Show("songdata not found");
#endif
                return null;
            }

            BinaryReader reader = new BinaryReader(File.OpenRead("songdata\\" + fn + ".uns"));

            reader.ReadBytes(3);//UNS

            int offsetToGBA = reader.ReadInt32();
            int offsetToGBG = reader.ReadInt32();
            int offsetToGBB = reader.ReadInt32();
            int offsetToGBD = reader.ReadInt32();
            int offsetToGBV = reader.ReadInt32();
            int offsetToGBE = reader.ReadInt32();

            ret.info.version = reader.ReadByte();
            if (ret.info.version != 20)
            {
                return LoadSong17(filename);
            }

            return ret;
        }

        public static SongData LoadSong(String filename)
        {
            if (filename.ToLower().EndsWith(".uns"))
                return LoadSong20(filename);
            else if (filename.Substring(0,filename.Length-1).ToLower().EndsWith(".gb"))
                return LoadSong12(filename);
            return null;
        }

        public static void SaveSong(SongData songdata, String filename)
        {
            BinaryWriter writer = null;
            try
            {
                writer = new BinaryWriter(File.OpenWrite(filename));

                writer.Write('U');
                writer.Write('N');
                writer.Write('S');

                for (int i = 0; i < 6 * 4; i++) writer.Write('\0');

                writer.Write((byte)20);

                writer.Write((uint)songdata.instruments.Length);

                writer.Write(songdata.info.name);
                writer.Write(songdata.info.artist);
                writer.Write((uint)songdata.info.year);
                writer.Write(songdata.info.genre);
                writer.Write((byte)songdata.info.length.Hours);
                writer.Write((byte)songdata.info.length.Minutes);
                writer.Write((byte)songdata.info.length.Seconds);

                for (int i = 0; i < 8; i++)
                    writer.Write(songdata.info.quotes[i] == null ? "" : songdata.info.quotes[i]);

                writer.Write((uint)songdata.info.charters.Length);
                for (int i = 0; i < songdata.info.charters.Length; i++)
                    writer.Write(songdata.info.charters[i]);

                writer.Write((uint)songdata.info.barlines.Length);
                for (int i = 0; i < songdata.info.barlines.Length; i++)
                {
                    writer.Write((uint)songdata.info.barlines[i].time);
                    writer.Write((byte)songdata.info.barlines[i].numBeats);
                }

                writer.Write((uint)songdata.info.trailingBeatLen);

                writer.Write((bool)songdata.info.bre.enabled);
                writer.Write((uint)songdata.info.bre.start);
                writer.Write((uint)songdata.info.bre.end);

                writer.Write((uint)songdata.info.harmonies.Length);
                for (int i = 0; i < songdata.info.harmonies.Length; i++)
                {
                    writer.Write((uint)songdata.info.harmonies[i].start);
                    writer.Write((uint)songdata.info.harmonies[i].end);
                    writer.Write((ulong)songdata.info.harmonies[i].instruments);
                }

                for (int instr = 0; instr < songdata.instruments.Length; instr++)
                {
                    SongData.SongDataInstrument instrument = songdata.instruments[instr];

                    for (int i = 0; i < 3; i++)
                        writer.Write((char)instrument.instrumentType.ToCharArray()[i]);

                    writer.Write(instrument.rpPhrases.Length);
                    for (int i = 0; i < instrument.rpPhrases.Length; i++)
                    {
                        writer.Write((uint)instrument.rpPhrases[i].time);
                        writer.Write((uint)instrument.rpPhrases[i].len);
                    }

                    Instrument theType = InstrumentMaster.GetSingleton().GetInstrument(instrument.instrumentType);
                    if (theType == null)
                    {
                        throw new InvalidOperationException("Error! Cannot find " + instrument.instrumentType + " instrument file!\nSaving Failed!\nPlease place the instrument file in the correct folder,\nreload instruments, and try to save again.");
                    }

                    if (theType.HasSolos)
                    {
                        writer.Write((uint)instrument.solos.Length);
                        for (int i = 0; i < instrument.solos.Length; i++)
                        {
                            writer.Write((uint)instrument.solos[i].time);
                            writer.Write((uint)instrument.solos[i].len);
                        }
                    }

                    if ((theType.RPEnableType & Instrument.RockPowerEnableTypes.FILL) != 0)
                    {
                        writer.Write((uint)instrument.fills.Length);
                        for (int i = 0; i < instrument.fills.Length; i++)
                        {
                            writer.Write((uint)instrument.fills[i].time);
                            writer.Write((uint)instrument.fills[i].len);
                        }
                    }

                    writer.Write((uint)instrument.diffSets.Length);

                    for (int dsi = 0; dsi < instrument.diffSets.Length; dsi++)
                    {
                        SongData.DifficultySet set = instrument.diffSets[dsi];

                        writer.Write((byte)set.diff);

                        writer.Write((uint)set.phrases.Length);

                        for (int pi = 0; pi < set.phrases.Length; pi++)
                        {
                            if (theType.TypesOfPhrases != Instrument.PhraseType.NONE)
                            {
                                writer.Write((uint)set.phrases[pi].time);
                                writer.Write((byte)set.phrases[pi].type);
                                writer.Write((bool)set.phrases[pi].rockpower);
                                if ((theType.TypesOfPhrases & Instrument.PhraseType.RHYTHM) != 0)
                                    writer.Write((byte)set.phrases[pi].rType);
                            }

                            writer.Write((uint)set.phrases[pi].notes.Length);

                            for (int ni = 0; ni < set.phrases[pi].notes.Length; ni++)
                            {
                                writer.Write((ulong)set.phrases[pi].notes[ni].type);
                                if (theType.PitchShifts)
                                    writer.Write((ulong)set.phrases[pi].notes[ni].endtype);
                                writer.Write((uint)set.phrases[pi].notes[ni].time);
                                if (theType.ContainsHeldNotes)
                                    writer.Write((uint)set.phrases[pi].notes[ni].length);
                                if (theType.ContainsText)
                                    writer.Write(set.phrases[pi].notes[ni].text);
                            }
                        }

                        for (int i = 0; i < 6; i++)
                            writer.Write((uint)set.starScoreLevels[i]);
                    }
                }

                writer.Write((uint)songdata.effects.cameraSwitches.Length);
                for (int i = 0; i < songdata.effects.cameraSwitches.Length; i++)
                    writer.Write((uint)songdata.effects.cameraSwitches[i]);

                writer.Write((uint)songdata.effects.effects.Length);
                for (int ei = 0; ei < songdata.effects.effects.Length; ei++)
                {
                    SongData.SpecialEffect effect = songdata.effects.effects[ei];

                    writer.Write((uint)effect.time);
                    writer.Write((char)effect.type.ToCharArray()[0]);
                    writer.Write((char)effect.type.ToCharArray()[1]);
                    writer.Write((uint)effect.length);

                    if (effect is SongData.NormalLightingSpecialEffect)
                    {
                        writer.Write((byte)((effect as SongData.NormalLightingSpecialEffect).color.R));
                        writer.Write((byte)((effect as SongData.NormalLightingSpecialEffect).color.G));
                        writer.Write((byte)((effect as SongData.NormalLightingSpecialEffect).color.B));
                        writer.Write((byte)((effect as SongData.NormalLightingSpecialEffect).color.A));
                    }
                }
            }
            finally
            {
                if(writer!=null)
                    writer.Close();
            }
        }
    }
}

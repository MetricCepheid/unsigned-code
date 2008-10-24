using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Unsigned
{
    /// <summary>
    /// Contains all data in a songdata file(s)
    /// </summary>
    public class SongData
    {
        public class Effect
        {
            public uint time;
            public String type;
            public uint length;

            public uint begin { get { return time; } }
            public uint end { get { return time+length; } }
        }

        public class NormalLightingEffect : Effect
        {
            public Color color;
        }

        public struct EffectsTrack
        {
            public uint[] cameraSwitches;
            public Effect[] effects;
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
            public Phrase[] phrases;
            public uint[] starScoreLevels;
        }

        public class SongDataInstrument
        {
            public String instrumentType;
            public RockPowerPhrase[] rpPhrases;
            public Solo[] solos;
            public Fill[] fills;
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
    class SongLoader
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
            if (fn.Contains("\\") || fn.ToLower().EndsWith("gba"))
            {
                fn = fn.Substring(fn.LastIndexOf('\\') + 1);
                fn = fn.Substring(0, fn.LastIndexOf('.'));
            }
            ret.info.filename = fn;

            if (!File.Exists("songdata\\" + fn + ".gba"))
            {
#if WINDOWS
                System.Windows.Forms.MessageBox.Show("songdata not found");
#endif
                return null;
            }
            //            BinaryWriter writer = new BinaryWriter(File.OpenWrite("songdata\\" + fn + ".uns"));
            BinaryReader reader = new BinaryReader(File.OpenRead("songdata\\" + fn + ".gba"));
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

            ret.instruments = new SongData.SongDataInstrument[4];

            reader = new BinaryReader(File.OpenRead("songdata\\" + fn + ".gbg"));

            reader.ReadByte();//version
            {
                SongData.SongDataInstrument guitar = new SongData.SongDataInstrument();
                guitar.instrumentType = "LGT";

                guitar.rpPhrases = new RockPowerPhrase[reader.ReadInt32()];
                for (int i = 0; i < guitar.rpPhrases.Length; i++)
                {
                    guitar.rpPhrases[i] = new RockPowerPhrase();
                    guitar.rpPhrases[i].time = reader.ReadUInt32();
                    guitar.rpPhrases[i].len = reader.ReadUInt32();
                }
                guitar.diffSets = new SongData.DifficultySet[4];
                for (int i = 0; i < 4; i++)
                {
                    int k = reader.ReadInt32();
                    guitar.diffSets[k] = new SongData.DifficultySet();
                    guitar.diffSets[k].diff = k;
                    guitar.diffSets[k].phrases = new Phrase[1];
                    guitar.diffSets[k].phrases[0] = new Phrase();
                    guitar.diffSets[k].phrases[0].notes = new NoteSet[reader.ReadInt32()];
                    for (int j = 0; j < guitar.diffSets[k].phrases[0].notes.Length; j++)
                    {
                        guitar.diffSets[k].phrases[0].notes[j] = new NoteSet();
                        guitar.diffSets[k].phrases[0].notes[j].visible = new NoteSet.VIS_STATE[5];
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

            reader = new BinaryReader(File.OpenRead("songdata\\" + fn + ".gbb"));

            reader.ReadByte();//version

            SongData.SongDataInstrument bass = new SongData.SongDataInstrument();
            bass.instrumentType = "BAS";

            bass.rpPhrases = new RockPowerPhrase[reader.ReadInt32()];
            for (int i = 0; i < bass.rpPhrases.Length; i++)
            {
                bass.rpPhrases[i] = new RockPowerPhrase(reader.ReadUInt32(), reader.ReadUInt32());
            }
            bass.diffSets = new SongData.DifficultySet[4];
            for (int i = 0; i < 4; i++)
            {
                int k = reader.ReadInt32();
                bass.diffSets[k] = new SongData.DifficultySet();
                bass.diffSets[k].phrases = new Phrase[1];
                bass.diffSets[k].phrases[0].notes = new NoteSet[reader.ReadInt32()];
                NoteSet[] arr = bass.diffSets[k].phrases[0].notes;
                for (int j = 0; j < arr.Length; j++)
                {
                    arr[j] = new NoteSet();
                    arr[j].visible = new NoteSet.VIS_STATE[5];
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

            reader = new BinaryReader(File.OpenRead("songdata\\" + fn + ".gbd"));

            SongData.SongDataInstrument drums = new SongData.SongDataInstrument();
            drums.instrumentType = "SET";

            reader.ReadByte();//version
            drums.rpPhrases = new RockPowerPhrase[reader.ReadInt32()];
            for (int i = 0; i < drums.rpPhrases.Length; i++)
            {
                drums.rpPhrases[i] = new RockPowerPhrase(reader.ReadUInt32(), reader.ReadUInt32());
            }
            drums.fills = new Fill[reader.ReadInt32()];
            for (int i = 0; i < drums.fills.Length; i++)
            {
                drums.fills[i] = new Fill(reader.ReadUInt32(), reader.ReadUInt32());
                drums.fills[i].len = drums.fills[i].len - drums.fills[i].time;
            }
            drums.diffSets = new SongData.DifficultySet[4];
            for (int i = 0; i < 4; i++)
            {
                int k = reader.ReadInt32();
                drums.diffSets[k] = new SongData.DifficultySet();
                drums.diffSets[k].phrases = new Phrase[1];
                drums.diffSets[k].phrases[0] = new Phrase();
                drums.diffSets[k].phrases[0].notes = new NoteSet[reader.ReadInt32()];
                NoteSet[] arr = drums.diffSets[k].phrases[0].notes;
                for (int j = 0; j < arr.Length; j++)
                {
                    arr[j] = new NoteSet();
                    arr[j].visible = new NoteSet.VIS_STATE[5];
                    arr[j].type = reader.ReadByte();
                    arr[j].time = reader.ReadUInt32();
                }
                drums.diffSets[k].starScoreLevels = new uint[6];
                for (int j = 0; j < 5; j++)
                    drums.diffSets[k].starScoreLevels[j] = reader.ReadUInt32();
            }
            reader.Close();

            ret.instruments[2] = drums;

            reader = new BinaryReader(File.OpenRead("songdata\\" + fn + ".gbv"));

            SongData.SongDataInstrument vocals = new SongData.SongDataInstrument();

            reader.ReadByte();//version

            vocals.instrumentType = "LVX";
            vocals.diffSets = new SongData.DifficultySet[4];
            vocals.diffSets[3] = new SongData.DifficultySet();
            vocals.diffSets[3].phrases = new Phrase[reader.ReadUInt32()];
            for (int i = 0; i < vocals.diffSets[3].phrases.Length; i++)
            {
                vocals.diffSets[3].phrases[i] = new Phrase();
                vocals.diffSets[3].phrases[i].time = reader.ReadUInt32();
                vocals.diffSets[3].phrases[i].type = (SongData.TYPE)reader.ReadByte();
                vocals.diffSets[3].phrases[i].notes = new NoteSet[reader.ReadInt32()];
                if (vocals.diffSets[3].phrases[i].type == SongData.TYPE.REGULAR)
                {
                    for (int k = 0; k < vocals.diffSets[3].phrases[i].notes.Length; k++)
                    {
                        vocals.diffSets[3].phrases[i].notes[k] = new NoteSet();
                        vocals.diffSets[3].phrases[i].notes[k].time = reader.ReadUInt32();
                        vocals.diffSets[3].phrases[i].notes[k].length = reader.ReadUInt32();
                        vocals.diffSets[3].phrases[i].notes[k].visible = new NoteSet.VIS_STATE[(vocals.diffSets[3].phrases[i].notes[k].length / 10) + 1];
                        vocals.diffSets[3].phrases[i].notes[k].type = (ulong)reader.ReadInt16();
                        vocals.diffSets[3].phrases[i].notes[k].endtype = vocals.diffSets[3].phrases[i].notes[k].type;
                        vocals.diffSets[3].phrases[i].notes[k].text = reader.ReadString();
                    }
                }
                else if (vocals.diffSets[3].phrases[i].type == SongData.TYPE.RHYTHM)
                {
                    vocals.diffSets[3].phrases[i].rType = (SongData.RTYPE)reader.ReadByte();
                    for (int k = 0; k < vocals.diffSets[3].phrases[i].notes.Length; k++)
                    {
                        vocals.diffSets[3].phrases[i].notes[k] = new NoteSet();
                        vocals.diffSets[3].phrases[i].notes[k].time = reader.ReadUInt32();
                        vocals.diffSets[3].phrases[i].notes[k].visible = new NoteSet.VIS_STATE[1];
                    }
                }
                else if (vocals.diffSets[3].phrases[i].type == SongData.TYPE.BLANK)
                    vocals.diffSets[3].phrases[i].notes = new NoteSet[0];
            }
            vocals.diffSets[3].starScoreLevels = new uint[6];
            for (int j = 0; j < 5; j++)
                vocals.diffSets[3].starScoreLevels[j] = reader.ReadUInt32();
            reader.Close();

            ret.instruments[1] = vocals;

            reader = new BinaryReader(File.OpenRead("songdata\\" + fn + ".gbe"));

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

            if (fn.Contains("\\") || fn.ToLower().EndsWith("uns"))
            {
                fn = fn.Substring(fn.LastIndexOf('\\') + 1);
                fn = fn.Substring(0, fn.LastIndexOf('.'));
            }
            ret.info.filename = fn;

            if (!File.Exists("songdata\\" + fn + ".uns"))
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
            ret.instruments[0].rpPhrases = new RockPowerPhrase[reader.ReadUInt32()];
            for (int i = 0; i < ret.instruments[0].rpPhrases.Length; i++)
                ret.instruments[0].rpPhrases[i] = new RockPowerPhrase(reader.ReadUInt32(), reader.ReadUInt32());
            ret.instruments[0].solos = new Solo[reader.ReadUInt32()];
            for (int i = 0; i < ret.instruments[0].solos.Length; i++)
                ret.instruments[0].solos[i] = new Solo(reader.ReadUInt32(), reader.ReadUInt32());
            ret.instruments[0].diffSets = new SongData.DifficultySet[4];
            for (int ir = 0; ir < 4; ir++)
            {
                uint k = reader.ReadUInt32();
                ret.instruments[0].diffSets[k].phrases = new Phrase[1];
                ret.instruments[0].diffSets[k].phrases[0] = new Phrase();
                ret.instruments[0].diffSets[k].phrases[0].notes = new NoteSet[reader.ReadUInt32()];
                for(int i=0;i<ret.instruments[0].diffSets[k].phrases[0].notes.Length;i++)
                {
                    NoteSet note = new NoteSet();
                    note.visible = new NoteSet.VIS_STATE[5];
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
            ret.instruments[3].rpPhrases = new RockPowerPhrase[reader.ReadUInt32()];
            for (int i = 0; i < ret.instruments[3].rpPhrases.Length; i++)
                ret.instruments[3].rpPhrases[i] = new RockPowerPhrase(reader.ReadUInt32(), reader.ReadUInt32());
            ret.instruments[3].diffSets = new SongData.DifficultySet[4];
            for (int ir = 0; ir < 4; ir++)
            {
                uint k = reader.ReadUInt32();
                ret.instruments[3].diffSets[k].phrases = new Phrase[1];
                ret.instruments[3].diffSets[k].phrases[0] = new Phrase();
                ret.instruments[3].diffSets[k].phrases[0].notes = new NoteSet[reader.ReadUInt32()];
                for (int i = 0; i < ret.instruments[3].diffSets[k].phrases[0].notes.Length; i++)
                {
                    NoteSet note = new NoteSet();
                    note.visible = new NoteSet.VIS_STATE[5];
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
            ret.instruments[2].rpPhrases = new RockPowerPhrase[reader.ReadUInt32()];
            for (int i = 0; i < ret.instruments[2].rpPhrases.Length; i++)
                ret.instruments[2].rpPhrases[i] = new RockPowerPhrase(reader.ReadUInt32(), reader.ReadUInt32());
            ret.instruments[2].fills = new Fill[reader.ReadUInt32()];
            for (int i = 0; i < ret.instruments[2].fills.Length; i++)
                ret.instruments[2].fills[i] = new Fill(reader.ReadUInt32(), reader.ReadUInt32());
            ret.instruments[2].diffSets = new SongData.DifficultySet[4];
            for (int ir = 0; ir < 4; ir++)
            {
                uint k = reader.ReadUInt32();
                ret.instruments[2].diffSets[k].phrases = new Phrase[1];
                ret.instruments[2].diffSets[k].phrases[0] = new Phrase();
                ret.instruments[2].diffSets[k].phrases[0].notes = new NoteSet[reader.ReadUInt32()];
                for (int i = 0; i < ret.instruments[2].diffSets[k].phrases[0].notes.Length; i++)
                {
                    NoteSet note = new NoteSet();
                    note.visible = new NoteSet.VIS_STATE[5];
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
            ret.instruments[1].diffSets[3].phrases = new Phrase[reader.ReadUInt32()];
            for (int j = 0; j < ret.instruments[1].diffSets[3].phrases.Length; j++)
            {
                ret.instruments[1].diffSets[3].phrases[j] = new Phrase();
                ret.instruments[1].diffSets[3].phrases[j].type = (SongData.TYPE)reader.ReadByte();
                ret.instruments[1].diffSets[3].phrases[j].rockpower = reader.ReadBoolean();
                ret.instruments[1].diffSets[3].phrases[j].notes = new NoteSet[reader.ReadUInt32()];
                if (ret.instruments[1].diffSets[3].phrases[j].type == SongData.TYPE.REGULAR)
                {
                    for (int i = 0; i < ret.instruments[1].diffSets[3].phrases[i].notes.Length; i++)
                    {
                        NoteSet note = new NoteSet();
                        note.time = reader.ReadUInt32();
                        note.length = reader.ReadUInt32();
                        note.visible = new NoteSet.VIS_STATE[(note.length / 10) + 1];
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
                        NoteSet note = new NoteSet();
                        note.visible = new NoteSet.VIS_STATE[1];
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
            ret.effects.effects = new SongData.Effect[0];
            ret.effects.cameraSwitches = new uint[reader.ReadUInt32()];
            for (int i = 0; i < ret.effects.cameraSwitches.Length; i++)
                ret.effects.cameraSwitches[i] = reader.ReadUInt32();

            reader.Close();

            return ret;

        }

        public static SongData LoadSong(string filename)
        {
            if (filename.ToLower().EndsWith(".uns"))
                return LoadSong17(filename);
            else if (filename.ToLower().EndsWith(".gba"))
                return LoadSong12(filename);
            return null;
        }
    }
}

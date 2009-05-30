using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SongDataIO
{
    /// <summary>
    /// Contains all data in a songdata file(s)
    /// </summary>
    public class SongData
    {
        /// <summary>
        /// Defines a Color in non-XNA environments (such as VocalsEditor)
        /// </summary>
        public struct Color
        {
            /// <summary>
            /// Red, Green, Blue, Alpha... duh
            /// </summary>
            public byte R, G, B, A;

            /// <summary>
            /// Generates an opaque Color
            /// </summary>
            /// <param name="r">Red Channel</param>
            /// <param name="g">Green Channel</param>
            /// <param name="b">Blue Channel</param>
            public Color(byte r, byte g, byte b)
            {
                R = r;
                G = g;
                B = b;
                A = 255;
            }

            /// <summary>
            /// Generates a semi-transparent Color
            /// </summary>
            /// <param name="r">Red Channel</param>
            /// <param name="g">Green Channel</param>
            /// <param name="b">Blue Channel</param>
            /// <param name="a">Alpha Channel (255=opaque,0=invisible)</param>
            public Color(byte r, byte g, byte b, byte a)
            {
                R = r;
                G = g;
                B = b;
                A = a;
            }
        }

        /// <summary>
        /// Represents a single note or chord
        /// </summary>
        public class NoteSet : IComparable
        {
            public uint time, length;
            public ulong type, endtype;
            public String text;

            public int CompareTo(object other)
            {
                return time.CompareTo(((NoteSet)other).time);
            }
        }

        /// <summary>
        /// Represents a section of notes
        /// </summary>
        public struct Phrase
        {
            public uint time;
            public SongData.TYPE type;
            public bool rockpower;
            public SongData.RTYPE rType;
            public NoteSet[] notes;
        }

        /// <summary>
        /// Represents a "fill" (think drum fills) that
        /// can be used for some instruments to activate
        /// rock power
        /// </summary>
        public struct Fill
        {
            public uint time, len;
            public uint end
            {
                get { return time + len; }
            }

            public Fill(uint time, uint len)
            {
                this.time = time;
                this.len = len;
            }
        }

        /// <summary>
        /// Represents a section of time in which
        /// all encompassed notes are white and, if
        /// hit, will give the user 25% rock power
        /// </summary>
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

        /// <summary>
        /// Represents a "solo" section where the user
        /// can recieve a score bonus for high levels
        /// of hit percentage.
        /// </summary>
        public struct Solo
        {
            public uint time, len;
            public uint end
            {
                get { return time + len; }
            }
            public Solo(uint time, uint len)
            {
                this.time = time;
                this.len = len;
            }
        }

        /// <summary>
        /// Represents a special lighting effect.
        /// </summary>
        public abstract class SpecialEffect
        {
            public uint time;
            public String type;
            public uint length;

            public uint begin { get { return time; } }
            public uint end { get { return time + length; } }
        }

        /// <summary>
        /// Represents a return to normalcy in regards to lighting
        /// </summary>
        public class NormalLightingSpecialEffect : SpecialEffect
        {
            public Color color;
            public NormalLightingSpecialEffect(uint time, uint length, Color color)
            {
                this.time = time;
                this.length = length;
                this.color = color;
                this.type = "ln";
            }
        }

        /// <summary>
        /// Holds all lighting effects in the track
        /// </summary>
        public struct EffectsTrack
        {
            /// <summary>
            /// Array of time values for when the camera
            /// should switch to a new track.
            /// Stored in Milliseconds
            /// </summary>
            public uint[] cameraSwitches;
            /// <summary>
            /// An array of lighting effects sorted by time
            /// </summary>
            public SpecialEffect[] effects;
        }

        /// <summary>
        /// represents a bar line for visual purposes
        /// </summary>
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

        /// <summary>
        /// represents a big rock ending section
        /// in which all players play freestyle for
        /// bonus points at the end of a song
        /// </summary>
        public struct BigRockEnding
        {
            public bool enabled;
            public uint start, end;
            public uint len { get { return end - start; } }

            public BigRockEnding(bool enabled, uint start, uint end)
            {
                this.enabled = enabled;
                this.start = start;
                this.end = end;
            }
        }

        /// <summary>
        /// Represents a section of the song where if all
        /// specified players hit all notes encompassed by
        /// the section, they get a bonus
        /// </summary>
        public struct Harmony
        {
            public uint start, end;
            // a series of instrument code names
            // length is always a multiple of 3
            public String instruments;
            public Harmony(uint start, uint end, String instruments)
            {
                this.start = start;
                this.end = end;
                this.instruments = instruments;
            }

            public bool ContainsInstrument(Instrument i)
            {
                for (int k = 0; k < instruments.Length; k += 3)
                    if (instruments.Substring(k, 3) == i.CodeName)
                        return true;
                return false;
            }

            public void RemoveInstrument(Instrument i)
            {
                for (int k = 0; k < instruments.Length; k += 3)
                    if (instruments.Substring(k, 3) == i.CodeName)
                    {
                        String pre = instruments.Substring(0, k);
                        String post = instruments.Substring(k + 3);
                        instruments = pre + post;
                    }
            }

            public void AddInstrument(Instrument i)
            {
                if (!ContainsInstrument(i))
                    instruments += i.CodeName;
            }
        }

        /// <summary>
        /// Represents information not specific to a 
        /// single instrument
        /// </summary>
        public class FullBandChunk
        {
            /// <summary>
            /// SongData version, for compatibility purposes
            /// </summary>
            public byte version;
            /// <summary>
            /// Name of the file
            /// </summary>
            public String filename;
            /// <summary>
            /// Title of the Song
            /// </summary>
            public String name;
            /// <summary>
            /// Name of the Composer/Performer of the Song
            /// </summary>
            public String artist;
            /// <summary>
            /// Year song was released
            /// </summary>
            public uint year;
            /// <summary>
            /// Genre of the song
            /// Only used for sorting and pre-song header
            /// </summary>
            public String genre;
            /// <summary>
            /// When the song should end in success
            /// </summary>
            public TimeSpan length;
            /// <summary>
            /// Quotes to display during loading
            /// Always length 8, but not all strings
            /// need to be valid
            /// </summary>
            public String[] quotes;
            /// <summary>
            /// A list of people involved in charting
            /// this song
            /// </summary>
            public String[] charters;
            /// <summary>
            /// The barlines for visual purposes
            /// </summary>
            public Barline[] barlines;
            /// <summary>
            /// After we run out of barlines, how long
            /// should measures be while the song trails off
            /// In milliseconds
            /// </summary>
            public uint trailingBeatLen;
            /// <summary>
            /// Notice there is only one BRE
            /// </summary>
            public BigRockEnding bre;
            /// <summary>
            /// Array of harmony sections.
            /// Sorted by time
            /// </summary>
            public Harmony[] harmonies;

            /// <summary>
            /// what is displayed as the song revs up
            /// </summary>
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

        /// <summary>
        /// Contains all the information for a specific
        /// instruments at a specific difficulty
        /// </summary>
        public class DifficultySet
        {
            public int diff;
            public SongData.Phrase[] phrases;
            public uint[] starScoreLevels;

            public DifficultySet()
            {
                diff = -1;
                phrases = null;
                starScoreLevels = new uint[6];
            }
        }

        /// <summary>
        /// holds all information for each instrument
        /// including which instrument it maps to
        /// </summary>
        public class SongDataInstrument
        {
            /// <summary>
            /// The 3-character instrument code name
            /// </summary>
            public String instrumentType;
            /// <summary>
            /// How hard this instrument is
            /// on a scale of 1-100
            /// </summary>
            public byte difficulty;
            /// <summary>
            /// All rock power phrases
            /// </summary>
            public SongData.RockPowerPhrase[] rpPhrases;
            /// <summary>
            /// All solo sections
            /// should be length 0 for non solo types
            /// </summary>
            public SongData.Solo[] solos;
            /// <summary>
            /// All fills
            /// should be length 0 for non fill RPEnable types
            /// </summary>
            public SongData.Fill[] fills;
            /// <summary>
            /// the actual note data
            /// should be length 4
            /// </summary>
            public DifficultySet[] diffSets;

            public SongDataInstrument()
            {
                instrumentType = "NUL";
                difficulty = 0;
                rpPhrases = null;
                solos = null;
                fills = null;
                diffSets = new DifficultySet[SongDataLoader.NumDifficulties];
            }
        }

        public enum TYPE { REGULAR = 0, BLANK = 1, RHYTHM = 2 };
        public enum RTYPE { TAMBOURINE = 0, COWBELL = 1, CLAP = 2 };

        public FullBandChunk info;
        public SongDataInstrument[] instruments;
        public EffectsTrack effects;

        public SongData()
        {
            info = new FullBandChunk();
            //make sure quotes is length 8
            info.quotes = new string[8];
            info.charters = new String[0];
            effects = new EffectsTrack();
            effects.effects = new SpecialEffect[0];
            effects.cameraSwitches = new uint[0];
        }
    }
}

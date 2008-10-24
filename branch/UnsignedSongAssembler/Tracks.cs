using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace Unsigned
{
    public class Barline
    {
        public uint time;
        public uint numBeats;

        public Barline(uint time, uint numBeats)
        {
            this.time = time;
            this.numBeats = numBeats;
        }
    }

    public class HarmonyPart
    {
        public uint start, end;
        public int instruments;

        public HarmonyPart(uint start, uint end, int instruments)
        {
            this.start = start;
            this.end = end;
            this.instruments = instruments;
        }
    }

    public class Fill
    {
        public uint time;
        public uint len;
        public Fill(uint time, uint len)
        {
            this.time = time;
            this.len = len;
        }
    }

    public class RockPowerPhrase
    {
        public uint time;
        public uint len;

        public RockPowerPhrase(uint time, uint len)
        {
            this.time = time;
            this.len = len;
        }
    }

    public class Solo
    {
        public uint time;
        public uint len;
        public Solo(uint time, uint len)
        {
            this.time = time;
            this.len = len;
        }
    }

    public class SongNote
    {
        public long value;
        public long endValue;
        public uint time;
        public uint len;
        public String text;
        public SongNote(uint time, long value)
        {
            this.time = time;
            this.value = value;
        }
    }

    public class Phrase
    {
        public uint time;
        public byte type, rtype;
        public bool rockpower;

        public List<SongNote> SongNotes;

        public Phrase()
        {
            time = 0;
            type = 0;
            rtype = 0;
            rockpower = false;
            SongNotes = new List<SongNote>();
        }
    }

    public class DifficultySet
    {
        public int diff;
        public List<Phrase> phrases;
        public int[] starScores;

        public DifficultySet()
        {
            phrases = new List<Phrase>();
            starScores = new int[6];
        }
    }

    [DefaultPropertyAttribute("Filename")]
    public class TrackFile
    {
        private String fileName;
        public TrackFileInfo info;
        public List<Track> tracks;
        public EffectsTrack effects;

        [CategoryAttribute("File"), DescriptionAttribute("The Filename of the File")]
        public String Filename
        {
            get { return fileName; }
            set { fileName = value; }
        }

        public TrackFile()
        {
            fileName = "DefaultProject";
            info = new TrackFileInfo();
            tracks = new List<Track>();
            effects = new EffectsTrack();
        }
    }

    [DefaultPropertyAttribute("NumberOfNotes")]
    public class Track
    {
        public String InstrumentType;

        public List<RockPowerPhrase> rockPowerPhrases;

        public List<Solo> solos;

        public List<Fill> fills;

        public DifficultySet[] diffSets;

        public override string ToString()
        {
            return InstrumentType;
        }

        public Track()
        {
            InstrumentType = "INV";
            rockPowerPhrases = new List<RockPowerPhrase>();
            solos = new List<Solo>();
            fills = new List<Fill>();
            diffSets = new DifficultySet[4];
            for (int i = 0; i < 4; i++)
                diffSets[i] = new DifficultySet();
        }

        [CategoryAttribute("Number Of Notes"), 
         DescriptionAttribute("Number of notes in the Expert Track")]
        public int ExpertNotes
        {
            get
            {
                int ret = 0;
                for(int i=0;i<diffSets[3].phrases.Count;i++)
                    ret+=diffSets[3].phrases[i].SongNotes.Count;
                return ret;
            }
        }

        [CategoryAttribute("Number Of Notes"),
         DescriptionAttribute("Number of notes in the Hard Track")]
        public int HardNotes
        {
            get
            {
                int ret = 0;
                for (int i = 0; i < diffSets[2].phrases.Count; i++)
                    ret += diffSets[2].phrases[i].SongNotes.Count;
                return ret;
            }
        }

        [CategoryAttribute("Number Of Notes"),
         DescriptionAttribute("Number of notes in the Medium Track")]
        public int MediumNotes
        {
            get
            {
                int ret = 0;
                for (int i = 0; i < diffSets[1].phrases.Count; i++)
                    ret += diffSets[1].phrases[i].SongNotes.Count;
                return ret;
            }
        }

        [CategoryAttribute("Number Of Notes"),
         DescriptionAttribute("Number of notes in the Easy Track")]
        public int EasyNotes
        {
            get
            {
                int ret = 0;
                for (int i = 0; i < diffSets[0].phrases.Count; i++)
                    ret += diffSets[0].phrases[i].SongNotes.Count;
                return ret;
            }
        }
    }

    [DefaultPropertyAttribute("Name")]
    public class TrackFileInfo
    {
        String name, artist;
        int year;
        String genre;
        TimeSpan length;
        String[] quotes;
        public List<String> charters;
        public List<Barline> barlines;
        public uint trailingBeatLen;
        bool hasBRE;
        TimeSpan bREStart, bREEnd;
        public List<HarmonyPart> harmonies;

        public TrackFileInfo()
        {
            name = "Default";
            artist = "NoOne";
            year = 2008;
            genre = "Rock";
            length = new TimeSpan(0,0,0);
            quotes = new string[8];
            for (int i = 0; i < 8; i++)
                quotes[i] = "";
            charters = new List<String>();
            barlines = new List<Barline>();
            trailingBeatLen = 0;
            hasBRE = false;
            bREStart = new TimeSpan(0, 0, 0);
            bREEnd = new TimeSpan(0, 0, 0);
            harmonies = new List<HarmonyPart>();
        }

        [CategoryAttribute("Song"), DescriptionAttribute("Name of the Song")]
        public String Name
        {
            get { return name; }
            set { name = value; }
        }

        [CategoryAttribute("Song"), DescriptionAttribute("Song's Artist")]
        public String Artist
        {
            get { return artist; }
            set { artist = value; }
        }

        [CategoryAttribute("Song"), DescriptionAttribute("Year Song was released")]
        public int Year
        {
            get { return year; }
            set { year = Math.Min(2010,Math.Max(1900,value)); }
        }

        [CategoryAttribute("Song"), DescriptionAttribute("Song's Subgenre of Rock")]
        public String Genre
        {
            get { return genre; }
            set { genre = value; }
        }

        [CategoryAttribute("Song"), DescriptionAttribute("Song's Length")]
        public TimeSpan Length
        {
            get { return length; }
            set { length = value; }
        }

        [CategoryAttribute("Chart"), DescriptionAttribute("Quotes about song to be displayed during loading")]
        public String[] Quotes
        {
            get { return quotes; }
            set { quotes = value; }
        }

        [CategoryAttribute("Big Rock Ending"), DescriptionAttribute("Whether or not the song supports a Big Rock Ending")]
        public bool HasBRE
        {
            get { return hasBRE; }
            set { hasBRE = value; }
        }

        [CategoryAttribute("Big Rock Ending"), DescriptionAttribute("The start time of the Big Rock Ending")]
        public TimeSpan BREStart
        {
            get { return bREStart; }
            set { bREStart = value; }
        }

        [CategoryAttribute("Big Rock Ending"), DescriptionAttribute("The end time of the Big Rock Ending")]
        public TimeSpan BREEnd
        {
            get { return bREEnd; }
            set { bREEnd = value; }
        }
    }

    public class EffectsTrack
    {
        public List<uint> cameraSwitches;
        public List<Effect> effects;
    }

    public class Effect
    {
        public int time;
        public String type;
        public int length;
    }

    public class NormalLightingSpecialEffect : Effect
    {
        public int color;
    }
}

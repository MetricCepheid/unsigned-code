using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Graphics;

namespace chart2unsigned
{
    public struct Pair
    {
        public int time;
        public int value;

        public Pair(int t, int v)
        {
            time = t;
            value = v;
        }
    }

    public struct Note : IComparable
    {
        public byte value;
        public uint time, len;
        public Note(byte v, uint t, uint l)
        {
            value = v;
            time = t;
            len = l;
        }
        public int CompareTo(object o)
        {
            return time.CompareTo(((Note)o).time);
        }
    }

    public struct SPPH
    {
        public byte value;
        public uint time, len;
        public int start1, start2;
        public int end1, end2;
        public SPPH(byte v, uint t, uint l)
        {
            value = v;
            time = t;
            len = l;
            start1 = 0;
            start2 = 0;
            end1 = 0;
            end2 = 0;
        }
    }
    
    public struct Event
    {
        public string value;
        public uint time;
        public Event(string v, uint t)
        {
            value = v;
            time = t;
        }
    }

    public class BarLine
    {
        public uint time;
        public int beats;
        public uint sBeat;
        public BarLine(uint t, int b, uint s)
        {
            time = t;
            beats = b;
            sBeat = s;
        }
    }

    public class VocalWord
    {
        public uint time, length;
        public ushort note;
        public string value;
    }

    public class VocalPhrase
    {
        public List<VocalWord> words;
        public uint time;
    }

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {

            if (args.Length < 1)
            {
                Error("Must pass a file to convert!");
                return;
            }

            string thisPath = System.Reflection.Assembly.GetEntryAssembly().Location;
            thisPath = thisPath.Substring(0, thisPath.LastIndexOf('\\') + 1);

            for (int i = 0; i < args.Length; i++)
                Console.WriteLine(args[i]);

            byte VERSION = 8;

            Console.WriteLine("Initial setup complete");
            
            //SETUP

            string cSync = "", cEffects = "", cGuitar = "", 
                   cBass = "", cDrums = "", cVocals = "";

            string name="", artist="", charter="";
            int outputLevel = 1;
            string cName;
            bool drumLayout=false;//true = bycolor
            float offset = 0;
            int resolution = 0;
            string player2="";
            float difficulty=0;
            float pStart=0, pEnd=0;
            string genre="";
            List<Pair> bpmsigs = new List<Pair>();
            List<Pair> timesigs = new List<Pair>();
            List<Note>[][] notes = new List<Note>[4][];
            List<SPPH>[][] SPs = new List<SPPH>[4][];
            List<SPPH> DFs = new List<SPPH>();
            List<uint> cameraSwitches = new List<uint>();
            List<Event> globalevents = new List<Event>();
            int[][][] starLevels = new int[4][][];
            for (int i = 0; i < 4; i++)
            {
                starLevels[i] = new int[4][];
                for (int k = 0; k < 4; k++)
                    starLevels[i][k] = new int[6];
            }
            for (int i = 0; i < 4; i++)
            {
                notes[i] = new List<Note>[4];
                for (int k = 0; k < 4; k++)
                    notes[i][k] = new List<Note>();
            }
            for (int i = 0; i < 4; i++)
            {
                SPs[i] = new List<SPPH>[4];
                for (int k = 0; k < 4; k++)
                    SPs[i][k] = new List<SPPH>();
            }
            List<Event>[][] events = new List<Event>[4][];
            for (int i = 0; i < 4; i++)
            {
                events[i] = new List<Event>[4];
                for (int k = 0; k < 4; k++)
                    events[i][k] = new List<Event>();
            }
            System.IO.StreamReader fin;

            Console.WriteLine("Full setup complete");

            Console.Out.WriteLine("Preparing to open config file");
            try
            {
                fin = new System.IO.StreamReader(thisPath + "config.cfg");
            }
            catch (Exception e)
            {
                Error("Error while trying to open config\n"+
                    "This probably means there was a problem deciphering the Path\n"+
                    "Crash Information (post these on forums):\n"+
                    e.Message);
                return;
            }

            try
            {
                while (!fin.EndOfStream)
                {
                    String str = fin.ReadLine().Trim();
                    if (str.Length > 6 && str.Substring(0, 6).Equals("output"))
                    {
                        try
                        {
                            outputLevel = Int32.Parse(str.Substring(str.IndexOf('=') + 1).Trim());
                            outputLevel = Math.Max(1, Math.Min(3, outputLevel));
                        }
                        catch (Exception)
                        {
                            Error("Error! The output in config.cfg must be a number!");
                        }
                    }
                    if (str.Length > 6 && str.Substring(0, 6).Equals("syncer"))
                    {
                        cSync = str.Substring(str.IndexOf('=') + 1).Trim();
                    }
                    else if (str.Length > 8 && str.Substring(0, 8).Equals("effecter"))
                    {
                        cEffects = str.Substring(str.IndexOf('=') + 1).Trim();
                    }
                    else if (str.Length > 8 && str.Substring(0, 8).Equals("guitarer"))
                    {
                        cGuitar = str.Substring(str.IndexOf('=') + 1).Trim();
                    }
                    else if (str.Length > 6 && str.Substring(0, 6).Equals("basser"))
                    {
                        cBass= str.Substring(str.IndexOf('=') + 1).Trim();
                    }
                    else if (str.Length > 7 && str.Substring(0, 7).Equals("drummer"))
                    {
                        cDrums = str.Substring(str.IndexOf('=') + 1).Trim();
                    }
                    else if (str.Length > 7 && str.Substring(0, 7).Equals("vocaler"))
                    {
                        cVocals = str.Substring(str.IndexOf('=') + 1).Trim();
                    }
                    else if (str.Length > 6 && str.Substring(0, 6).Equals("layout"))
                    {
                        String sy = str.Substring(str.IndexOf('=') + 1).Trim();
                        if (sy.Equals("color"))
                            drumLayout = true;
                        else
                            drumLayout = false;
                    }
                }
            }
            catch (System.IO.IOException e)
            {
                Error("IO Error\n"+
                    "Please Dump output at forums\n"+e.Message);
                return;
            }
            catch (System.OutOfMemoryException)
            {
                Error("You ran out of memory!!!\n"+
                    "Try closing some programs, or go buy some memory. It\'s cheap");
                return;
            }
            catch (Exception e)
            {
                Error("Unknown Error\n"+
                    "Please Dump output at forums\n"+
                    "Error info: " + e.Message);
                return;
            }
            

            fin.Close();
            Console.WriteLine("Config successfully read");

            //INPUT

            Console.WriteLine("Trying to open file specified");
            try
            {
                fin = new System.IO.StreamReader(args[0]);
            }
            catch (Exception e)
            {
                Error("Error while trying to open file\n"+
                    "This probably means file not found\n"+
                    "Crash Information (post these on forums):\n"+
                    e.Message);
                return;
            }
            cName = args[0].Substring(args[0].LastIndexOf('\\')+1);
            cName = cName.Substring(0,cName.IndexOf('.'));

            Console.WriteLine("File Opened Successfully");
            if(outputLevel>=2)
                Console.Out.WriteLine("File Details:");
            int inputLineCount = 0;
#if !DEBUG
            try
            {
#endif
                while (!fin.EndOfStream)
                {
                    inputLineCount++;
                    String track = fin.ReadLine();

                    if (track.Equals("[Song]"))
                    {
                        inputLineCount++;
                        fin.ReadLine();//{
                        inputLineCount++;
                        string line = fin.ReadLine().Trim();
                        while (!line.Equals("}"))
                        {
                            if (line.StartsWith("Name"))
                            {
                                name = line.Substring(line.IndexOf("\"") + 1);
                                name = name.Substring(0, name.IndexOf("\""));
                                if(outputLevel>=2)
                                Console.Out.WriteLine("Name: " + name);
                            }
                            else if (line.StartsWith("Artist"))
                            {
                                artist = line.Substring(line.IndexOf("\"") + 1);
                                artist = artist.Substring(0, artist.IndexOf("\""));
                                if(outputLevel>=2)
                                Console.Out.WriteLine("Artist: " + artist);
                            }
                            else if (line.StartsWith("Charter"))
                            {
                                charter = line.Substring(line.IndexOf("\"") + 1);
                                charter = charter.Substring(0, charter.IndexOf("\""));
                                if(outputLevel>=2)
                                Console.Out.WriteLine("Charter: " + charter);
                            }
                            else if (line.StartsWith("Offset"))
                            {
                                String temp = line.Substring(line.IndexOf("=") + 1).Trim();
                                offset = float.Parse(temp, System.Globalization.CultureInfo.InvariantCulture);
                                if(outputLevel>=2)
                                Console.Out.WriteLine("Offset: " + offset);
                            }
                            else if (line.StartsWith("Resolution"))
                            {
                                resolution = Int32.Parse(line.Substring(line.IndexOf("=") + 1).Trim());
                            }
                            else if (line.StartsWith("Player2"))
                            {
                                player2 = line.Substring(line.IndexOf("=") + 1).Trim();
                                if(outputLevel>=2)
                                Console.Out.WriteLine("Player2 Type: " + player2);
                            }
                            else if (line.StartsWith("Difficulty"))
                            {
                                difficulty = float.Parse(line.Substring(line.IndexOf("=") + 1).Trim(), System.Globalization.CultureInfo.InvariantCulture);
                                if(outputLevel>=2)
                                Console.Out.WriteLine("Difficulty: " + difficulty);
                            }
                            else if (line.StartsWith("PreviewStart"))
                            {
                                pStart = float.Parse(line.Substring(line.IndexOf("=") + 1).Trim(), System.Globalization.CultureInfo.InvariantCulture);
                            }
                            else if (line.StartsWith("PreviewEnd"))
                            {
                                pEnd = float.Parse(line.Substring(line.IndexOf("=") + 1).Trim(), System.Globalization.CultureInfo.InvariantCulture);
                            }
                            else if (line.StartsWith("Genre"))
                            {
                                genre = line.Substring(line.IndexOf("\"") + 1);
                                genre = genre.Substring(0, genre.IndexOf("\""));
                                if(outputLevel>=2)
                                Console.Out.WriteLine("Genre: " + genre);
                            }
                            inputLineCount++;
                            line = fin.ReadLine().Trim();
                        }
                    }
                    else if (track.Equals("[SyncTrack]"))
                    {
                        inputLineCount++;
                        fin.ReadLine();//{
                        inputLineCount++;
                        string line = fin.ReadLine().Trim();
                        if(outputLevel>=2)
                        Console.WriteLine("Syncing Information loading");
                        while (!line.Equals("}"))
                        {
                            int time = Int32.Parse(line.Substring(0, line.IndexOf('=')).Trim());
                            line = line.Substring(line.IndexOf('=') + 1).Trim();
                            string what = line.Substring(0, line.IndexOf(' ')).Trim();
                            line = line.Substring(line.IndexOf(' ') + 1).Trim();
                            int value = Int32.Parse(line);
                            if (what.Equals("B"))
                            {
                                bpmsigs.Add(new Pair(time, value));
                                if(outputLevel>=3)
                                Console.Write("(B:" + time + ")");
                            }
                            else if (what.Equals("TS"))
                            {
                                timesigs.Add(new Pair(time, value));
                                if(outputLevel>=3)
                                Console.Write("(TS:" + time + ")");
                            }
                            inputLineCount++;
                            line = fin.ReadLine().Trim();
                        }
                    }
                    else if (track.Equals("[Events]"))
                    {
                        inputLineCount++;
                        fin.ReadLine();//{
                        inputLineCount++;
                        string line = fin.ReadLine().Trim();
                        if(outputLevel>=2)
                        Console.WriteLine("Event info loading");
                        while (!line.Equals("}"))
                        {
                            Event e = new Event();
                            e.time = UInt32.Parse(line.Substring(0, line.IndexOf('=')).Trim());
                            e.value = line.Substring(line.IndexOf('E') + 1).Trim();
                            e.value = e.value.Substring(1, e.value.Length - 2);
                            globalevents.Add(e);
                            inputLineCount++;
                            line = fin.ReadLine().Trim();
                        }
                    }
                    else if (track.Contains("Single") || track.Contains("Double") || track.Contains("Drums") || track.Contains("Vocals"))
                    {
                        if (outputLevel >= 2)
                            Console.WriteLine("Track info Loading");
                        int diff;
                        int inst;
                        if (track.Contains("Single"))
                            inst = 0;
                        else if (track.Contains("Double"))
                            inst = 3;
                        else if (track.Contains("Drums"))
                            inst = 2;
                        else 
                            inst = 1;
                        if (track.Contains("Easy"))
                            diff = 0;
                        else if (track.Contains("Medium"))
                            diff = 1;
                        else if (track.Contains("Hard"))
                            diff = 2;
                        else
                            diff = 3;

                        inputLineCount++;
                        fin.ReadLine();//{
                        inputLineCount++;
                        string line = fin.ReadLine().Trim();
                        while (!line.Equals("}"))
                        {
                            uint time = UInt32.Parse(line.Substring(0, line.IndexOf('=')).Trim());
                            line = line.Substring(line.IndexOf('=') + 1).Trim();
                            string what = line.Substring(0, line.IndexOf(' ')).Trim();
                            line = line.Substring(line.IndexOf(' ') + 1).Trim();

                            if (what.Equals("N"))
                            {
                                int value = Int32.Parse(line.Substring(0, line.IndexOf(' ')).Trim());
                                line = line.Substring(line.IndexOf(' ') + 1).Trim();
                                uint len = UInt32.Parse(line);
                                byte valueB = (byte)(1 << value);
                                notes[inst][diff].Add(new Note(valueB, time, len));
                                if (outputLevel >= 3)
                                    Console.WriteLine("(N:" + time + ")");
                            }
                            else if (what.Equals("E"))
                            {
                                string ev = line;
                                events[inst][diff].Add(new Event(ev, time));
                                if (outputLevel >= 3)
                                    Console.WriteLine("(E:" + time + ")");
                            }
                            else if (what.Equals("S"))
                            {
                                int value = Int32.Parse(line.Substring(0, line.IndexOf(' ')).Trim());
                                line = line.Substring(line.IndexOf(' ') + 1).Trim();
                                uint len = UInt32.Parse(line);
                                if (value == 2)
                                    SPs[inst][diff].Add(new SPPH((byte)value, time, len));
                                else if (inst == 2 && diff == 3 && value == 3)
                                    DFs.Add(new SPPH((byte)value, time, len));
                                if (outputLevel >= 3)
                                    Console.WriteLine("(S"+value+":" + time + ")");
                            }
                            inputLineCount++;
                            line = fin.ReadLine().Trim();
                        }
                    }
                    else
                    {
                        inputLineCount++;
                        string line = fin.ReadLine().Trim();
                        while (!line.Equals("}"))
                        {
                            inputLineCount++;
                            line = fin.ReadLine().Trim();
                        }
                    }
                }
#if !DEBUG
            }
            catch (System.IO.IOException e)
            {
                Error("IO Error at line " + inputLineCount+"\n"+
                    "Please Dump output at forums");
                return;
            }
            catch (System.OutOfMemoryException e)
            {
                Error("You ran out of memory!!!\n"+
                    "Try closing some programs, or go buy some memory. It\'s cheap");
                return;
            }
            catch (Exception e)
            {
                Error("Unknown Error at line " + inputLineCount+"\n"+
                    "Please Dump output at forums\n"+
                    "Error info: " + e.Message);
                return;
            }
#endif
            fin.Close();
            Console.Out.WriteLine("File successfully Loaded");


            try
            {
                if (cSync.Equals("ask"))
                {
                    Console.Write("Who synced this chart? ");
                    cSync = Console.ReadLine().Trim();
                }
                if (cEffects.Equals("ask"))
                {
                    Console.Write("Who wrote the effects for this chart? ");
                    cEffects = Console.ReadLine().Trim();
                }
                if (cGuitar.Equals("ask"))
                {
                    Console.Write("Who wrote the guitar part of this chart? ");
                    cGuitar = Console.ReadLine().Trim();
                }
                if (cBass.Equals("ask"))
                {
                    Console.Write("Who wrote the bass part of this chart? ");
                    cBass = Console.ReadLine().Trim();
                }
                if (cDrums.Equals("ask"))
                {
                    Console.Write("Who wrote the drum part of this chart? ");
                    cDrums = Console.ReadLine().Trim();
                }
                if (cVocals.Equals("ask"))
                {
                    Console.Write("Who wrote the vocal part of this chart? ");
                    cVocals = Console.ReadLine().Trim();
                }
            }
            catch (Exception e)
            {
                Error("Unknown Error occurred\n" + e.Message);
                return;
            }
            int sLength = 0;
            string timeS = "";
            try
            {
                Console.WriteLine("Song Length:");
                Console.WriteLine("Format hh:mm:ss (leading digits optional)");
                String timeTemp = Console.ReadLine();
                if (!timeTemp.Contains(":"))
                    sLength = Int32.Parse(timeTemp) * 1;
                else if (timeTemp.IndexOf(':') == timeTemp.LastIndexOf(':'))
                    sLength = (Int32.Parse(timeTemp.Substring(0, timeTemp.IndexOf(':'))) * 60) +
                              (Int32.Parse(timeTemp.Substring(timeTemp.IndexOf(':') + 1)) * 1);
                else
                    sLength = (Int32.Parse(timeTemp.Substring(0, timeTemp.IndexOf(':'))) * 360) +
                              (Int32.Parse(timeTemp.Substring(timeTemp.IndexOf(':') + 1, timeTemp.LastIndexOf(':') - (timeTemp.IndexOf(':') + 1))) * 60) +
                              (Int32.Parse(timeTemp.Substring(timeTemp.LastIndexOf(':') + 1)) * 1);

                
                if (((int)(sLength / 3600)) <= 0)
                    timeS += "00" + ":";
                else if (((int)(sLength / 3600)) < 10)
                    timeS += "0" + ((int)(sLength / 3600)) + ":";
                else
                    timeS += ((int)(sLength / 3600)) + ":";
                if (((int)((sLength / 60) % 60)) <= 0)
                    timeS += "00" + ":";
                else if (((int)((sLength / 60) % 60)) < 10)
                    timeS += "0" + ((int)((sLength / 60) % 60)) + ":";
                else
                    timeS += ((int)((sLength / 60) % 60)) + ":";
                if (((int)(sLength % 60)) <= 0)
                    timeS += "00";
                else if (((int)(sLength % 60)) < 10)
                    timeS += "0" + ((int)(sLength % 60));
                else
                    timeS += ((int)(sLength % 60));

                sLength *= 1000;
            }
            catch (Exception e)
            {
                Error("Unknown Error occurred\n" + e.Message);
                return;
            }

            if (outputLevel >= 2)
                Console.WriteLine("Length parsed well");
            uint lastBeat = 0;
            for (int i = 0; i < globalevents.Count; i++)
                if (globalevents[i].value.Equals("end"))
                    lastBeat = globalevents[i].time;

            if (outputLevel >= 3)
                Console.WriteLine("Last Beat Found");

            
            List<BarLine> barlines = new List<BarLine>();
            List<uint> beatTimesL = new List<uint>();
            int cBPM=0, cTS = 0;
            float currentTime = 0;
            uint currentBeat = 0;
            int currentBPM = 0;
            try
            {
                for (int i = 0; i < lastBeat / 192 + 8; i++)
                {
                    float add = 0;
                    for (uint k = currentBeat; k < currentBeat + 192; k++)
                    {
                        if (cBPM < bpmsigs.Count && bpmsigs[cBPM].time == k)
                        { currentBPM = bpmsigs[cBPM].value; cBPM++; }

                        add += (60000f / currentBPM * 1000) / 192f;
                    }
                    beatTimesL.Add((uint)currentTime);
                    if (outputLevel >= 3)
                        Console.Write("(Beat:" + ((int)currentTime) + ")");
                    currentBeat += 192;
                    currentTime += add;
                }
                while (currentTime < sLength)
                {
                    beatTimesL.Add((uint)currentTime);
                    if (outputLevel >= 3)
                        Console.Write("(Beat:" + ((int)currentTime) + ")");
                    currentTime += (60000f / currentBPM * 1000) / 192f;
                }
                beatTimesL.Add((uint)currentTime);
            }
            catch (Exception e)
            {
                Error("Unknown Error occurred\n" + e.Message);
                return;
            }
            uint[] beatTimes = beatTimesL.ToArray();
            if (outputLevel >= 2)
                Console.WriteLine("Beat times parsed");
            int currentTS = 0;
            currentBeat = 0;
            int countBeat = 0;
            try
            {
                for (int i = 0; i < beatTimes.Length; i++)
                {
                    if (cTS < timesigs.Count && timesigs[cTS].time <= currentBeat)
                    { currentTS = timesigs[cTS].value; cTS++; }
                    if (countBeat == 0)
                        barlines.Add(new BarLine(beatTimes[currentBeat / 192], currentTS, currentBeat / 192));
                    countBeat++;
                    if (countBeat >= currentTS)
                        countBeat = 0;
                    currentBeat += 192;
                }
                if (outputLevel >= 2)
                    Console.WriteLine("Barlines parsed");
                for (int m = 0; m < notes.Length; m++)
                    for (int n = 0; n < notes[m].Length; n++)
                    {
                        for (int i = 0; i < notes[m][n].Count - 1; )
                            if (notes[m][n][i].time == notes[m][n][i + 1].time)
                            {
                                Note r = notes[m][n][i];
                                r.value |= notes[m][n][i + 1].value;
                                notes[m][n][i] = r;
                                notes[m][n].RemoveAt(i + 1);
                            }
                            else
                                i++;
                    }
            }
            catch (Exception e)
            {
                Error("Unknown Error occurred\n" + e.Message);
                return;
            }
            
            if (outputLevel >= 2)
                Console.WriteLine("Chords Parsed");

            Note[][][] newnotes = new Note[notes.Length][][];
            try
            {
                for (int m = 0; m < notes.Length; m++)
                {
                    newnotes[m] = new Note[notes[m].Length][];
                    for (int n = 0; n < notes[m].Length; n++)
                    {
                        newnotes[m][n] = notes[m][n].ToArray();
                        if (outputLevel >= 3)
                            Console.WriteLine("Notes Arrayed");
                        Array.Sort<Note>(newnotes[m][n]);
                        if(m==2 && !drumLayout)
                            for (int i = 0; i < newnotes[m][n].Length; i++)
                            {
                                byte newVal = 0;
                                if ((newnotes[m][n][i].value & 1) != 0)
                                    newVal |= 16;
                                if ((newnotes[m][n][i].value & 2) != 0)
                                    newVal |= 2;
                                if ((newnotes[m][n][i].value & 4) != 0)
                                    newVal |= 4;
                                if ((newnotes[m][n][i].value & 8) != 0)
                                    newVal |= 8;
                                if ((newnotes[m][n][i].value & 16) != 0)
                                    newVal |= 1;
                                newnotes[m][n][i].value = newVal;
                                if (outputLevel >= 3)
                                    Console.WriteLine("Drum Layout Converted");
                            }
                        
                        for (int i = 1; i < newnotes[m][n].Length; i++)
                            if (newnotes[m][n][i].time - newnotes[m][n][i - 1].time < 90)
                                if(!IsChord(newnotes[m][n][i].value) && (newnotes[m][n][i-1].value&0x1F&newnotes[m][n][i].value)==0)
                                    newnotes[m][n][i].value |= (1 << 5);
                        if (outputLevel >= 3)
                            Console.WriteLine("HOPOs Configured");
                        for (int i = 0; i < newnotes[m][n].Length; i++)
                        {
                            float when = newnotes[m][n][i].time / 192f;
                            if (when == (int)when)
                                newnotes[m][n][i].time = beatTimes[(int)when];
                            else
                                newnotes[m][n][i].time = (uint)((beatTimes[(int)when+1]*(when-(int)when))+(beatTimes[(int)when]*(1-(when-(int)when))));
                            when += newnotes[m][n][i].len / 192f;
                            if (when == (int)when)
                                newnotes[m][n][i].len = beatTimes[(int)when]-newnotes[m][n][i].time;
                            else
                                newnotes[m][n][i].len = (uint)((beatTimes[(int)when+1]*(when-(int)when))+(beatTimes[(int)when]*(1-(when-(int)when))))-newnotes[m][n][i].time;
                        }
                        if (outputLevel >= 3)
                            Console.WriteLine("Times Converted");
                        for (int i = 0; i < newnotes[m][n].Length - 1; i++)
                            if (newnotes[m][n][i].len > 0)
                                if (newnotes[m][n][i].time + newnotes[m][n][i].len > newnotes[m][n][i + 1].time - 50)
                                    newnotes[m][n][i].len -= 50;
                        if (outputLevel >= 3)
                            Console.WriteLine("Fixed for Held Continuity Glitch");
                        
                    }
                }
            }
            catch (Exception e)
            {
                Error("Unknown Error occurred\n" + e.Message);
                return;
            }
            if (outputLevel >= 2)
                Console.WriteLine("Notes Parsed");
            SPPH[][][] newSPs = new SPPH[SPs.Length][][];
            try
            {
                for (int m = 0; m < SPs.Length; m++)
                {
                    newSPs[m] = new SPPH[SPs[m].Length][];
                    for (int n = 0; n < SPs[m].Length; n++)
                    {
                        newSPs[m][n] = SPs[m][n].ToArray();
                        for (int i = 0; i < newSPs[m][n].Length; i++)
                        {
                            float when = newSPs[m][n][i].time / 192f;
                            if (when == (int)when)
                                newSPs[m][n][i].time = beatTimes[(int)when];
                            else
                                newSPs[m][n][i].time = (uint)((beatTimes[(int)when]*(when-(int)when))+(beatTimes[(int)when+1]*(1-(when-(int)when))));
                            when += newSPs[m][n][i].len / 192f;
                            if (when == (int)when)
                                newSPs[m][n][i].len = beatTimes[(int)when];
                            else
                                newSPs[m][n][i].len = (uint)((beatTimes[(int)when]*(when-(int)when))+(beatTimes[(int)when+1]*(1-(when-(int)when))));
                        }
                        if (outputLevel >= 3)
                            Console.Out.Write("Overdrive Phrases calculated");
                    }
                }
            }
            catch (Exception e)
            {
                Error("Unknown Error occurred\n" + e.Message);
                return;
            }
            if (outputLevel >= 2)
                Console.WriteLine("Overdrive Phrases parsed");

            uint start=0;
            for (int i = 0; i < events[2][3].Count; i++)
            {
                if (events[2][3][i].value.Equals("drumfill_on"))
                    start = events[2][3][i].time;
                if (events[2][3][i].value.Equals("drumfill_off"))
                    DFs.Add(new SPPH(0, (uint)start, (uint)(events[2][3][i].time - start)));
            }

            for (int i = 0; i < DFs.Count-1; i++)
            {
                for (int k = i + 1; k < DFs.Count; k++)
                {
                    if (DFs[i].time == DFs[k].time)
                        DFs.Remove(DFs[k]);
                }
            }

            SPPH[] newDFs = DFs.ToArray(); ;
            try
            {
                for (int i = 0; i < newDFs.Length; i++)
                {
                    float when = newDFs[i].time / 192f;
                    if (when == (int)when)
                        newDFs[i].time = beatTimes[(int)when];
                    else
                        newDFs[i].time = (uint)((beatTimes[(int)when]*(when-(int)when))+(beatTimes[(int)when+1]*(1-(when-(int)when))));
                    when += newDFs[i].len / 192f;
                    if (when == (int)when)
                        newDFs[i].len = beatTimes[(int)when];
                    else
                        newDFs[i].len = (uint)((beatTimes[(int)when]*(when-(int)when))+(beatTimes[(int)when+1]*(1-(when-(int)when))));
                }
                if (outputLevel >= 3)
                    Console.Out.Write("Drum Fills calculated");
            }
            catch (Exception e)
            {
                Error("Unknown Error occurred\n" + e.Message);
                return;
            }
            if (outputLevel >= 2)
                Console.WriteLine("Drum Fills parsed");


            for(int m=0;m<4;m++)
                for (int n = 0; n < 4; n++)
                {
                    int maxscore = 0;
                    for (int i = 0; i < newnotes[m][n].Length; i++)
                    {
                        maxscore += 100 * CountNotes(newnotes[m][n][i].value);

                        maxscore += (int)(newnotes[m][n][i].len * CountNotes(newnotes[m][n][i].value) * 0.1f);

                    }
                    switch (m)
                    {
                        case 0:
                            starLevels[m][n][0] = (int)(0.21f * maxscore);
                            starLevels[m][n][1] = (int)(0.46f * maxscore);
                            starLevels[m][n][2] = (int)(0.77f * maxscore);
                            starLevels[m][n][3] = (int)(1.85f * maxscore);
                            starLevels[m][n][4] = (int)(3.08f * maxscore);
                            starLevels[m][n][5] = (int)(4.68f * maxscore);
                            break;
                        case 2:
                            starLevels[m][n][0] = (int)(0.21f * maxscore);
                            starLevels[m][n][1] = (int)(0.46f * maxscore);
                            starLevels[m][n][2] = (int)(0.77f * maxscore);
                            starLevels[m][n][3] = (int)(1.85f * maxscore);
                            starLevels[m][n][4] = (int)(3.08f * maxscore);
                            starLevels[m][n][5] = (int)(4.44f * maxscore);
                            break;
                        case 3:
                            starLevels[m][n][0] = (int)(0.32f * maxscore);
                            starLevels[m][n][1] = (int)(0.69f * maxscore);
                            starLevels[m][n][2] = (int)(1.16f * maxscore);
                            starLevels[m][n][3] = (int)(2.78f * maxscore);
                            starLevels[m][n][4] = (int)(4.62f * maxscore);
                            starLevels[m][n][5] = (int)(7.02f * maxscore);
                            break;
                    }
                    if (outputLevel >= 3)
                    Console.WriteLine("ScoreStars calculated");
                }
            if (outputLevel >= 2)
                Console.WriteLine("ScoreStars Parsed");

            for (int i = 0; i < globalevents.Count; i++)
            {
                if (globalevents[i].value.Equals("camera_switch"))
                    cameraSwitches.Add(globalevents[i].time);
            }
            if (outputLevel >= 2)
            Console.Out.Write("Camera Switches parsed");
            uint[] newcameraSwitches = cameraSwitches.ToArray(); ;
            try
            {
                for (int i = 0; i < newcameraSwitches.Length; i++)
                {
                    float when = newcameraSwitches[i] / 192f;
                    if (when == (int)when)
                        newcameraSwitches[i] = beatTimes[(int)when];
                    else
                        newcameraSwitches[i] = (uint)((beatTimes[(int)when]*(when-(int)when))+(beatTimes[(int)when+1]*(1-(when-(int)when))));
                }
                if (outputLevel >= 3)
                    Console.Out.Write("Camera Switches calculated");
            }
            catch (Exception e)
            {
                Error("Unknown Error occurred\n" + e.Message);
                return;
            }

            List<Event> vocalEvents = new List<Event>();
            for (int i = 0; i < events[1][3].Count; i++)
                if (events[1][3][i].value.StartsWith("V"))
                    vocalEvents.Add(events[1][3][i]);
            if (outputLevel >= 2)
                    Console.Out.Write("Vocal Events parsed");

            ushort[] notesVal = { 1, 3, 4, 6, 8, 9, 11 };
            string notesStr = "ABCDEFG";

            List<VocalPhrase> vocalPhrases = new List<VocalPhrase>();
            VocalPhrase tempPhrase = new VocalPhrase();
            for (int i = 0; i < notes[1][3].Count; i++)
            {
                if ((notes[1][3][i].value & (1 << 4))!=0)
                {
                    if(tempPhrase.words!=null)
                        vocalPhrases.Add(tempPhrase);
                    tempPhrase = new VocalPhrase();
                    tempPhrase.words = new List<VocalWord>();
                    tempPhrase.time = notes[1][3][i].time;
                }
                if ((notes[1][3][i].value & 1)!=0)
                {
                    VocalWord tW = new VocalWord();
                    tW.time = notes[1][3][i].time;
                    tW.length = notes[1][3][i].len;
                    ushort note = 0;
                    Event ev = new Event();
                    for (int k = 0; k < vocalEvents.Count; k++)
                    {
                        if (vocalEvents[k].time == notes[1][3][i].time)
                        {
                            ev = vocalEvents[k];
                            break;
                        }
                    }
                    if (ev.value.Equals(""))
                        continue;
                    tW.value = ev.value.Substring(ev.value.IndexOf('-')+1);
                    char[] chars = ev.value.Substring(0, 4).ToCharArray();
                    if (chars[1] == 'H')
                        note += 24;
                    else if (chars[1] == 'M')
                        note += 12;
                    note += notesVal[notesStr.IndexOf(chars[2])];
                    if (chars[3] == 'S')
                        note++;
                    else if (chars[3] == 'F')
                        note--;
                    tW.note = note;
                    tempPhrase.words.Add(tW);
                }
            }
            try
            {
                for (int i = 0; i < vocalPhrases.Count; i++)
                {
                    {
                        float when = vocalPhrases[i].time / 192f;
                        if (when == (int)when)
                            vocalPhrases[i].time = beatTimes[(int)when];
                        else
                            vocalPhrases[i].time = (uint)((beatTimes[(int)when+1] * (when - (int)when)) + (beatTimes[(int)when] * (1 - (when - (int)when))));
                    }
                    for (int k = 0; k < vocalPhrases[i].words.Count; k++)
                    {
                        float when = vocalPhrases[i].words[k].time / 192f;
                        if (when == (int)when)
                            vocalPhrases[i].words[k].time = beatTimes[(int)when];
                        else
                            vocalPhrases[i].words[k].time = (uint)((beatTimes[(int)when+1]*(when-(int)when))+(beatTimes[(int)when]*(1-(when-(int)when))));
                        when += vocalPhrases[i].words[k].length / 192f;
                        if (when == (int)when)
                            vocalPhrases[i].words[k].length = beatTimes[(int)when];
                        else
                            vocalPhrases[i].words[k].length = (uint)((beatTimes[(int)when+1]*(when-(int)when))+(beatTimes[(int)when]*(1-(when-(int)when))));
                    }
                }
                if (outputLevel >= 3)
                    Console.Out.Write("Camera Switches calculated");
            }
            catch (Exception e)
            {
                Error("Unknown Error occurred\n" + e.Message);
                return;
            }
            if (outputLevel >= 2)
                    Console.Out.Write("Vocal Phrases parsed");
            

            Console.WriteLine("Finished Processing");

            //OUTPUT
            string[] strDiff = { "EASY", "MEDIUM", "HARD", "EXPERT", };
            System.IO.BinaryWriter fout;
            try
            {
                fout = new System.IO.BinaryWriter(System.IO.File.OpenWrite(args[0].Substring(0, args[0].LastIndexOf("\\") + 1) + cName + ".gba"));
            }
            catch(Exception)
            {
                Error("Problem opening GBA for writing");
                return;
            }
            Console.Out.WriteLine("Writing...");
            fout.Write(VERSION);
            fout.Write(name);
            fout.Write(artist);
            fout.Write(timeS);
            fout.Write(cSync);
            fout.Write(cEffects);
            fout.Write(cGuitar);
            fout.Write(cVocals);
            fout.Write(cDrums);
            fout.Write(cBass);
            fout.Write(barlines.Count);
            for (int i = 0; i < barlines.Count; i++)
            { fout.Write(barlines[i].time); fout.Write(barlines[i].beats); }
            fout.Write(beatTimes[beatTimes.Length-1]-beatTimes[beatTimes.Length-2]);
            fout.Close();
            Console.Out.WriteLine("GBA written");
            try
            {
                fout = new System.IO.BinaryWriter(System.IO.File.OpenWrite(args[0].Substring(0, args[0].LastIndexOf("\\") + 1) + cName + ".gbg"));
            }
            catch(Exception)
            {
                Error("Problem opening GBG for writing");
            }
            fout.Write(VERSION);
            fout.Write(newSPs[0][3].Length);
            for (int i = 0; i < newSPs[0][3].Length; i++)
            {
                fout.Write(newSPs[0][3][i].time);
                fout.Write(newSPs[0][3][i].len-newSPs[0][3][i].time);
            }
            for (int i = 3; i >= 0; i--)
            {
                fout.Write(i);
                fout.Write(newnotes[0][i].Length);
                for (int k = 0; k < newnotes[0][i].Length; k++)
                {
                    fout.Write(newnotes[0][i][k].value);
                    fout.Write(newnotes[0][i][k].time);
                    fout.Write(newnotes[0][i][k].len);
                }
                for (int k = 0; k < 5; k++)
                    fout.Write(starLevels[0][i][k]);
            }
            fout.Close();
            Console.Out.WriteLine("GBG Written");
            try
            {
                fout = new System.IO.BinaryWriter(System.IO.File.OpenWrite(args[0].Substring(0, args[0].LastIndexOf("\\") + 1) + cName + ".gbb"));
            }
            catch(Exception)
            {
                Error("Problem opening GBB for writing");
            }
            fout.Write(VERSION);
            fout.Write(newSPs[3][3].Length);
            for (int i = 0; i < newSPs[3][3].Length; i++)
            {
                fout.Write(newSPs[3][3][i].time);
                fout.Write(newSPs[3][3][i].len-newSPs[3][3][i].time);
            }
            for (int i = 3; i >= 0; i--)
            {
                fout.Write(i);
                fout.Write(newnotes[3][i].Length);
                for (int k = 0; k < newnotes[3][i].Length; k++)
                {
                    fout.Write(newnotes[3][i][k].value);
                    fout.Write(newnotes[3][i][k].time);
                    fout.Write(newnotes[3][i][k].len);
                }
                for (int k = 0; k < 5; k++)
                    fout.Write(starLevels[3][i][k]);
            }
            fout.Close();
            Console.Out.WriteLine("GBB Written");
            try
            {
                fout = new System.IO.BinaryWriter(System.IO.File.OpenWrite(args[0].Substring(0, args[0].LastIndexOf("\\") + 1) + cName + ".gbd"));
            }
            catch(Exception)
            {
                Error("Problem opening GBD for writing");
            }
            fout.Write(VERSION);
            fout.Write(newSPs[2][3].Length);
            for (int i = 0; i < newSPs[2][3].Length; i++)
            {
                fout.Write(newSPs[2][3][i].time);
                fout.Write(newSPs[2][3][i].len-newSPs[2][3][i].time);
            }
            fout.Write(newDFs.Length);
            for (int i = 0; i < newDFs.Length; i++)
            {
                fout.Write(newDFs[i].time);
                fout.Write(newDFs[i].len);
            }
            for (int i = 3; i >= 0; i--)
            {
                fout.Write(i);
                fout.Write(newnotes[2][i].Length);
                for (int k = 0; k < newnotes[2][i].Length; k++)
                {
                    fout.Write(newnotes[2][i][k].value);
                    fout.Write(newnotes[2][i][k].time);
                }
                for (int k = 0; k < 5; k++)
                    fout.Write(starLevels[2][i][k]);
            }
            fout.Close();
            Console.Out.WriteLine("GBD Written");
            try
            {
                fout = new System.IO.BinaryWriter(System.IO.File.OpenWrite(args[0].Substring(0, args[0].LastIndexOf("\\") + 1) + cName + ".gbv"));
            }
            catch(Exception)
            {
                Error("Problem opening GBV for writing");
            }
            fout.Write(VERSION);
            fout.Write(vocalPhrases.Count);
            for (int i = 0; i < vocalPhrases.Count; i++)
            {
                fout.Write(vocalPhrases[i].time);
                fout.Write(vocalPhrases[i].words.Count);
                for (int k = 0; k < vocalPhrases[i].words.Count; k++)
                {
                    fout.Write(vocalPhrases[i].words[k].time);
                    fout.Write(vocalPhrases[i].words[k].length);
                    fout.Write(vocalPhrases[i].words[k].note);
                    fout.Write(vocalPhrases[i].words[k].value);
                }
            }
            for (int k = 0; k < 5; k++)
                fout.Write(starLevels[1][3][k]);
            fout.Close();
            Console.Out.WriteLine("GBV Written");
            try
            {
                fout = new System.IO.BinaryWriter(System.IO.File.OpenWrite(args[0].Substring(0, args[0].LastIndexOf("\\") + 1) + cName + ".gbe"));
            }
            catch(Exception)
            {
                Error("Problem opening GBE for writing");
            }
            fout.Write(2+newcameraSwitches.Length);
            fout.Write(0);
            for (int i = 0; i < newcameraSwitches.Length; i++)
                fout.Write(newcameraSwitches[i]);
            fout.Write(sLength);
            fout.Write(1);
            fout.Write(0);
            fout.Write('n');
            fout.Write('r');
            fout.Write(sLength);
            fout.Write(100);
            fout.Close();
            Console.Out.WriteLine("GBE Written");
            Console.Out.WriteLine("COMPLETE!");
        }

        private static void Error(String a)
        {
            Console.BackgroundColor = ConsoleColor.Red;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(a);
            Console.Beep(100, 200);
            Console.Beep(200, 400);
            Console.Beep(150, 400);
            Console.Beep(200, 150);
            Console.Beep(100, 150);
            Console.Beep(200, 150);
            Console.Beep(100, 150);
            Console.Write("Closing...");
            for (int i = 5; i >= 0; i--)
            {
                Console.Write(i + "...");
                Console.Beep(37+i, 1000);
            }
            return;
        }

        private static bool IsChord(byte p)
        {
            int count=0;
            for (int i = 0; i < 5; i++)
                if ((p & (1 << i)) != 0)
                    count++;
            return count > 1;
        }

        private static int CountNotes(byte p)
        {
            int count=0;
            for (int i = 0; i < 5; i++)
                if ((p & (1 << i)) != 0)
                    count++;
            return count;
        }
    }
}
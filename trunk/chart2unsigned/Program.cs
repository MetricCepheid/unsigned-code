using System;
using System.Collections.Generic;
using System.Windows.Forms;

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

    public struct Note
    {
        public byte value;
        public int time, len;
        public Note(byte v, int t, int l)
        {
            value = v;
            time = t;
            len = l;
        }
    }

    public struct SPPH
    {
        public byte value;
        public int time, len;
        public int start1, start2;
        public int end1, end2;
        public SPPH(byte v, int t, int l)
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
        string value;
        int time;
        public Event(string v, int t)
        {
            value = v;
            time = t;
        }
    }

    public class BarLine
    {
        public int time;
        public int beats;
        public int sBeat;
        public BarLine(int t, int b, int s)
        {
            time = t;
            beats = b;
            sBeat = s;
        }
    }

    public class AdvBarLine : BarLine
    {
        public int[] eigthtimes;
        public AdvBarLine(int t, int b, int s) : base(t,b,s)
        {
            time = t;
            beats = b;
            sBeat = s;
            eigthtimes = new int[beats * 2];
        }
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
                throw new ArgumentException("Must pass a file to convert");

            
            //SETUP

            string name="", artist="", charter="";
            string cName;
            int offset=0, resolution=0;
            string player2="";
            float difficulty=0;
            float pStart=0, pEnd=0;
            string genre="";
            List<Pair> bpmsigs = new List<Pair>();
            List<Pair> timesigs = new List<Pair>();
            List<Note>[][] notes = new List<Note>[2][];
            List<SPPH>[][] SPs = new List<SPPH>[2][];
            for (int i = 0; i < 2; i++)
            {
                notes[i] = new List<Note>[4];
                for (int k = 0; k < 4; k++)
                    notes[i][k] = new List<Note>();
            }
            for (int i = 0; i < 2; i++)
            {
                SPs[i] = new List<SPPH>[4];
                for (int k = 0; k < 4; k++)
                    SPs[i][k] = new List<SPPH>();
            }
            List<Event>[][] events = new List<Event>[2][];
            for (int i = 0; i < 2; i++)
            {
                events[i] = new List<Event>[4];
                for (int k = 0; k < 4; k++)
                    events[i][k] = new List<Event>();
            }

            int dotcount = 0;
            int dcmax = 500;

            //INPUT

            System.IO.StreamReader fin = new System.IO.StreamReader(args[0]);
            cName = args[0].Substring(args[0].LastIndexOf('\\')+1);
            cName = cName.Substring(0,cName.IndexOf('.'));

            Console.Out.WriteLine("File Details:");

            while (!fin.EndOfStream)
            {
                String track = fin.ReadLine();
                if (track.Equals("[Song]"))
                {
                    fin.ReadLine();//{
                    string line = fin.ReadLine().Trim();
                    while (!line.Equals("}"))
                    {
                        if (line.StartsWith("Name"))
                        {
                            name = line.Substring(line.IndexOf("\"") + 1);
                            name = name.Substring(0, name.IndexOf("\""));
                            Console.Out.WriteLine("Name: " + name);
                        }
                        else if (line.StartsWith("Artist"))
                        {
                            artist = line.Substring(line.IndexOf("\"") + 1);
                            artist = artist.Substring(0, artist.IndexOf("\""));
                            Console.Out.WriteLine("Artist: " + artist);
                        }
                        else if (line.StartsWith("Charter"))
                        {
                            charter = line.Substring(line.IndexOf("\"") + 1);
                            charter = charter.Substring(0, charter.IndexOf("\""));
                        }
                        else if (line.StartsWith("Offset"))
                        {
                            offset = Int32.Parse(line.Substring(line.IndexOf("=") + 1).Trim());
                            Console.Out.WriteLine("Offset: " + offset);
                        }
                        else if (line.StartsWith("Resolution"))
                        {
                            resolution = Int32.Parse(line.Substring(line.IndexOf("=") + 1).Trim());
                        }
                        else if (line.StartsWith("Player2"))
                        {
                            player2 = line.Substring(line.IndexOf("=") + 1).Trim();
                            Console.Out.WriteLine("Player2 Type: " + player2);
                        }
                        else if (line.StartsWith("Difficulty"))
                        {
                            difficulty = (float)Double.Parse(line.Substring(line.IndexOf("=") + 1).Trim());
                            Console.Out.WriteLine("Difficulty: " + difficulty);
                        }
                        else if (line.StartsWith("PreviewStart"))
                        {
                            pStart = (float)Double.Parse(line.Substring(line.IndexOf("=") + 1).Trim());
                        }
                        else if (line.StartsWith("PreviewEnd"))
                        {
                            pEnd = (float)Double.Parse(line.Substring(line.IndexOf("=") + 1).Trim());
                        }
                        else if (line.StartsWith("Genre"))
                        {
                            genre = line.Substring(line.IndexOf("\"") + 1);
                            genre = genre.Substring(0, genre.IndexOf("\""));
                            Console.Out.WriteLine("Genre: "+genre);
                        }
                        line = fin.ReadLine().Trim();
                    }
                    Console.Out.Write("Loading.");
                }
                else if (track.Equals("[SyncTrack]"))
                {
                    fin.ReadLine();//{
                    string line = fin.ReadLine().Trim();
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
                            dotcount++;
                            if (dotcount >= dcmax)
                            {
                                Console.Out.Write(".");
                                dotcount = 0;
                            }
                        }
                        else if (what.Equals("TS"))
                        {
                            timesigs.Add(new Pair(time, value));
                            dotcount++;
                            if (dotcount >= dcmax)
                            {
                                Console.Out.Write(".");
                                dotcount = 0;
                            }
                        }
                        line = fin.ReadLine().Trim();
                    }
                }
                else if (track.Equals("[Events]"))
                {
                    fin.ReadLine();//{
                    string line = fin.ReadLine().Trim();
                    while (!line.Equals("}"))
                    {
                        line = fin.ReadLine().Trim();
                    }
                    dotcount++;
                    if (dotcount >= dcmax)
                    {
                        Console.Out.Write(".");
                        dotcount = 0;
                    }
                }
                else if (track.Contains("Single") || track.Contains("Double"))
                {
                    int diff;
                    int inst;
                    if (track.Contains("Single"))
                        inst = 0;
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

                    fin.ReadLine();//{
                    string line = fin.ReadLine().Trim();
                    while (!line.Equals("}"))
                    {
                        int time = Int32.Parse(line.Substring(0, line.IndexOf('=')).Trim());
                        line = line.Substring(line.IndexOf('=') + 1).Trim();
                        string what = line.Substring(0, line.IndexOf(' ')).Trim();
                        line = line.Substring(line.IndexOf(' ') + 1).Trim();

                        if (what.Equals("N"))
                        {
                            int value = Int32.Parse(line.Substring(0, line.IndexOf(' ')).Trim());
                            line = line.Substring(line.IndexOf(' ') + 1).Trim();
                            int len = Int32.Parse(line);
                            byte valueB = (byte)(1 << value);
                            notes[inst][diff].Add(new Note(valueB, time, len));
                            dotcount++;
                            if (dotcount >= dcmax)
                            {
                                Console.Out.Write(".");
                                dotcount = 0;
                            }
                        }
                        else if (what.Equals("E"))
                        {
                            string ev = line;
                            events[inst][diff].Add(new Event(ev, time));
                            dotcount++;
                            if (dotcount >= dcmax)
                            {
                                Console.Out.Write(".");
                                dotcount = 0;
                            }
                        }
                        else if (what.Equals("S"))
                        {
                            int value = Int32.Parse(line.Substring(0, line.IndexOf(' ')).Trim());
                            line = line.Substring(line.IndexOf(' ') + 1).Trim();
                            int len = Int32.Parse(line);
                            SPs[inst][diff].Add(new SPPH((byte)value, time, len));
                        }

                        line = fin.ReadLine().Trim();
                    }
                }
                else
                {
                    string line = fin.ReadLine().Trim();
                    while (!line.Equals("}"))
                        line = fin.ReadLine().Trim();
                }
            }
            Console.Out.WriteLine(".Done");

            fin.Close();

            Console.Write("Song Length in seconds:");
            int sLength = Int32.Parse(Console.ReadLine())*1000;

            //PROCESS
            Console.Out.Write("Processing.");

            int lastBeat = 0;
            for (int m = 0; m < notes.Length; m++)
                for (int n = 0; n < notes[m].Length; n++)
                    if(notes[m][n].Count>0)
                        if (notes[m][n][notes[m][n].Count - 1].time + notes[m][n][notes[m][n].Count - 1].len > lastBeat)
                            lastBeat = notes[m][n][notes[m][n].Count - 1].time + notes[m][n][notes[m][n].Count - 1].len;
            if (bpmsigs[bpmsigs.Count - 1].time > lastBeat)
                lastBeat = bpmsigs[bpmsigs.Count - 1].time;
            if (timesigs[timesigs.Count - 1].time > lastBeat)
                lastBeat = timesigs[timesigs.Count - 1].time;
            

            string timeS=((int)(sLength/360))+":"+((int)((sLength/60)%60))+":"+((int)(sLength%60));
            List<BarLine> barlines = new List<BarLine>();
            int[] beatTimes = new int[lastBeat / 192 + 8];
            int cBPM=0, cTS = 0;
            float currentTime = 0;
            int currentBeat = 0;
            int currentBPM = 0;
            for (int i = 0; i < beatTimes.Length; i++)
            {
                float add = 0;
                for (int k = currentBeat; k < currentBeat + 192; k++)
                {
                    if (cBPM<bpmsigs.Count && bpmsigs[cBPM].time == k)
                    { currentBPM = bpmsigs[cBPM].value; cBPM++; }

                    add+=(60000f / currentBPM * 1000) / 192f;
                }
                beatTimes[currentBeat / 192] = (int)currentTime;
                currentBeat += 192;
                currentTime += add;
            }
            int currentTS = 0;
            currentBeat = 0;
            int countBeat = 0;
            for (int i = 0; i < beatTimes.Length; i++)
            {
                if (cTS<timesigs.Count && timesigs[cTS].time <= currentBeat)
                { currentTS = timesigs[cTS].value; cTS++; }
                if (countBeat == 0)
                    barlines.Add(new BarLine(beatTimes[currentBeat / 192], currentTS, currentBeat / 192));
                countBeat++;
                if (countBeat >= currentTS)
                    countBeat = 0;
                currentBeat += 192;
            }
            /*int currentBPM = 0;
            int currentTS = 0;
            int currentBeats = 0, currentTime = offset;
            {
                int k = 0;
                int i;
                for (i = 0; i < bpmsigs.Count - 1; i++)
                {
                    if (timesigs[k].time <= bpmsigs[i].time)
                    {
                        if(timesigs[k].time*4/768!=currentBeats)
                            throw new InvalidProgramException("does not support timesig not synced with measure");
                        currentTS = timesigs[k].value;
                        //if (((bpmsigs[i].time / (float)(768 / 4)) - currentBeats) % currentTS != 0)
                        //{
                            if (((bpmsigs[i].time / (float)(768 / 4)) - currentBeats) > currentTS)
                            {
                                int numMAddp = (int)(((bpmsigs[i].time / (float)(768 / 4)) - currentBeats)/currentTS);
                                for (int p = 0; p < numMAddp; p++)
                                {
                                    barlines.Add(new BarLine(currentTime + (int)(p * (60000f / currentBPM * currentTS * 1000)), currentTS, currentBeats + (p * currentTS)));
                                    Console.Out.Write(".");
                                }
                                currentBeats += numMAddp * currentTS;
                                currentTime += (int)(60000f / currentBPM * currentTS * 1000) * numMAddp;
                            }

                            int firstMeasureLen = (int)((((bpmsigs[i].time/192f)-currentBeats)/currentTS) * (60000f / currentBPM * currentTS * 1000));
                            currentBPM = bpmsigs[i].value;
                            firstMeasureLen += (int)((1-(((bpmsigs[i].time/192f)-currentBeats)/currentTS)) * (60000f / currentBPM * currentTS * 1000));
                            if (firstMeasureLen > 0)
                            {
                                barlines.Add(new BarLine(currentTime, currentTS, currentBeats));
                                currentTime += firstMeasureLen;
                                currentBeats += currentTS;
                            }
                            k++;

                            int numMAdd = ((Math.Min(bpmsigs[i + 1].time, timesigs[k].time) / (192)) - currentBeats) / currentTS;
                            if (numMAdd > 0)
                            {
                                for (int p = 0; p < numMAdd; p++)
                                {
                                    barlines.Add(new BarLine(currentTime + (int)(p * (60000f / currentBPM * currentTS * 1000)), currentTS, currentBeats + (p * currentTS)));
                                    Console.Out.Write(".");
                                }
                                currentBeats += numMAdd * currentTS;
                                currentTime += (int)(60000f / currentBPM * currentTS * 1000) * numMAdd;
                            }
                    }
                    else
                    {
                        if (((bpmsigs[i].time / (float)(768 / 4)) - currentBeats) > currentTS)
                        {
                            int numMAddp = (int)(((bpmsigs[i].time / (float)(768 / 4)) - currentBeats)/currentTS);
                            for (int p = 0; p < numMAddp; p++)
                            {
                                barlines.Add(new BarLine(currentTime + (int)(p * (60000f / currentBPM * currentTS * 1000)), currentTS, currentBeats + (p * currentTS)));
                                Console.Out.Write(".");
                            }
                            currentBeats += numMAddp * currentTS;
                            currentTime += (int)(60000f / currentBPM * currentTS * 1000) * numMAddp;
                        }
                        int firstMeasureLen = (int)((((bpmsigs[i].time/192f)-currentBeats)/currentTS) * (60000f / currentBPM * currentTS * 1000));
                        currentBPM = bpmsigs[i].value;
                        firstMeasureLen += (int)((1-(((bpmsigs[i].time/192f)-currentBeats)/currentTS)) * (60000f / currentBPM * currentTS * 1000));
                        if (firstMeasureLen > 0)
                        {
                            barlines.Add(new BarLine(currentTime, currentTS, currentBeats));
                            currentTime += firstMeasureLen;
                            currentBeats += currentTS;
                        }

                        int numMAdd = ((Math.Min(bpmsigs[i + 1].time,timesigs[k].time) / (768 / 4))-currentBeats)/currentTS;
                        if(numMAdd>0)
                        {
                            for (int p = 0; p < numMAdd; p++)
                            {
                                barlines.Add(new BarLine(currentTime + (int)(p * (60000f / currentBPM * currentTS * 1000)), currentTS, currentBeats + (p * currentTS)));
                                Console.Out.Write(".");
                            }
                            currentBeats += numMAdd * currentTS;
                            currentTime += (int)(60000f / currentBPM * currentTS * 1000) * numMAdd;
                        }
                    }
                }
                {
                    if (timesigs[k].time <= bpmsigs[i].time)
                    {
                        currentTS = timesigs[k].value;
                        k++;
                    }
                    currentBPM = bpmsigs[i].value;
                    int mLen = (int)(60000f / currentBPM * currentTS * 1000);
                    int numMAdd = (int)((sLength - currentTime) / mLen)+2;
                    for (int p = 0; p < numMAdd; p++)
                    {
                        barlines.Add(new BarLine(currentTime + (int)(p * (60000f / currentBPM * currentTS * 1000)), currentTS,currentBeats+(p*currentTS)));
                        Console.Out.Write(".");
                    }
                    currentBeats += numMAdd * currentTS;
                    currentTime += (int)(60000f / currentBPM * currentTS * 1000 * numMAdd);
                }
            }*/

            //int remainingMLen = (int)(60000f / currentBPM * currentTS * 1000);
            for (int m = 0; m < notes.Length; m++)
                for (int n = 0; n < notes[m].Length; n++)
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
            //int rest = (int)(60000f / currentBPM * currentTS * 1000);
            Note[][][] newnotes = new Note[notes.Length][][];
            for (int m = 0; m < notes.Length; m++)
            {
                newnotes[m] = new Note[notes[m].Length][];
                for (int n = 0; n < notes[m].Length; n++)
                {
                    newnotes[m][n] = notes[m][n].ToArray();
                    int k = 0;
                    for (int i = 1; i < newnotes[m][n].Length; i++)
                        if (newnotes[m][n][i].time - newnotes[m][n][i - 1].time <= 96)
                            if(!IsChord(newnotes[m][n][i].value) && (newnotes[m][n][i].value&0x1F)!=(newnotes[m][n][i-1].value&0x1F))
                                newnotes[m][n][i].value |= (1 << 5);
                    for (int i = 0; i < newnotes[m][n].Length; i++)
                    {
                        float when = newnotes[m][n][i].time / 192f;
                        if (when == (int)when)
                            newnotes[m][n][i].time = beatTimes[(int)when];
                        else
                            newnotes[m][n][i].time = (int)((beatTimes[(int)when]*(when-(int)when))+(beatTimes[(int)when+1]*(1-(when-(int)when))));
                        when += newnotes[m][n][i].len / 192f;
                        if (when == (int)when)
                            newnotes[m][n][i].len = beatTimes[(int)when]-newnotes[m][n][i].time;
                        else
                            newnotes[m][n][i].len = (int)((beatTimes[(int)when]*(when-(int)when))+(beatTimes[(int)when+1]*(1-(when-(int)when))))-newnotes[m][n][i].time;
                    }
                    for (int i = 0; i < newnotes[m][n].Length - 1; i++)
                        if (newnotes[m][n][i].len > 0)
                            if (newnotes[m][n][i].time + newnotes[m][n][i].len > newnotes[m][n][i + 1].time - 50)
                                newnotes[m][n][i].len -= 50;
                    Console.Out.Write(".");
                }
            }
            SPPH[][][] newSPs = new SPPH[SPs.Length][][];
            for (int m = 0; m < SPs.Length; m++)
            {
                newSPs[m] = new SPPH[SPs[m].Length][];
                for (int n = 0; n < SPs[m].Length; n++)
                {
                    newSPs[m][n] = SPs[m][n].ToArray();
                    int k = 0;
                    for (int i = 0; i < newSPs[m][n].Length; i++)
                    {
                        
                        while (k<barlines.Count && SPs[m][n][i].time / (768 / 4) >= barlines[k].sBeat + barlines[k].beats)
                            k++;
                        int endtime = SPs[m][n][i].len + SPs[m][n][i].time;
                        if (k < barlines.Count-1)
                        {
                            int t = barlines[k].time;
                            int d = (int)((SPs[m][n][i].time / (768f / 4)) - barlines[k].sBeat);
                            newSPs[m][n][i].start1 = k;
                            newSPs[m][n][i].start2 = d;
                        }
                        while (k < barlines.Count && endtime / (768 / 4) >= barlines[k].sBeat + barlines[k].beats)
                            k++;
                        if (k < barlines.Count - 1)
                        {
                            int t = barlines[k].time;
                            int d = (int)((endtime / (768f / 4)) - barlines[k].sBeat);
                            newSPs[m][n][i].end1 = k;
                            newSPs[m][n][i].end2 = d;
                        }
                    }
                    Console.Out.Write(".");
                }
            }
            Console.Out.WriteLine(".Done");

            //OUTPUT
            string[] strDiff = { "EASY", "MEDIUM", "HARD", "EXPERT", };

            System.IO.StreamWriter fout = new System.IO.StreamWriter(args[0].Substring(0, args[0].LastIndexOf("\\") + 1) + cName + ".gba");
            Console.Out.WriteLine("Writing...");
            fout.WriteLine(name);
            fout.WriteLine(artist);
            fout.WriteLine(timeS);
            fout.WriteLine("Bars:" + barlines.Count);
            for (int i = 0; i < barlines.Count; i++)
                fout.WriteLine(barlines[i].time + ":" + barlines[i].beats);
            fout.WriteLine(beatTimes[beatTimes.Length-1]-beatTimes[beatTimes.Length-2]);
            fout.WriteLine("Cues:1");
            fout.WriteLine("0:"+cName);
            fout.WriteLine("" + sLength);
            fout.Close();
            Console.Out.WriteLine("GBA written");
            fout = new System.IO.StreamWriter(args[0].Substring(0, args[0].LastIndexOf("\\") + 1) + cName + ".gbg");
            fout.WriteLine("SPPH:"+newSPs[0][3].Length);
            for (int i = 0; i < newSPs[0][3].Length; i++)
            {
                fout.WriteLine("" + newSPs[0][3][i].start1 + ":" + newSPs[0][3][i].start2);
                fout.WriteLine("" + newSPs[0][3][i].end1 + ":" + newSPs[0][3][i].end2);
            }
            for (int i = 3; i >= 0; i--)
            {
                fout.WriteLine(strDiff[i] + ":" + newnotes[0][i].Length);
                for (int k = 0; k < newnotes[0][i].Length; k++)
                {
                    for (int l = 0; l < 6; l++)
                        if ((newnotes[0][i][k].value & (1 << l)) != 0)
                            fout.Write("O");
                        else
                            fout.Write("X");
                    fout.Write(":");
                    fout.Write(newnotes[0][i][k].time);
                    fout.Write(";");
                    fout.WriteLine(newnotes[0][i][k].len);
                }
                fout.WriteLine("0");
                fout.WriteLine("0");
                fout.WriteLine("0");
                fout.WriteLine("0");
                fout.WriteLine("0");
            }
            fout.Close();
            Console.Out.WriteLine("GBG Written");
            fout = new System.IO.StreamWriter(args[0].Substring(0, args[0].LastIndexOf("\\") + 1) + cName + ".gbe");
            fout.WriteLine("TRANSITIONS:2");
            fout.WriteLine("0");
            fout.WriteLine(""+sLength);
            fout.WriteLine("EFFECTS:1");
            fout.WriteLine("00000:nr;" + sLength + "!100");
            fout.Close();
            Console.Out.WriteLine("GBE Written");
            Console.Out.WriteLine("...Done");
        }

        private static bool IsChord(byte p)
        {
            int count=0;
            for (int i = 0; i < 5; i++)
                if ((p & (1 << i)) != 0)
                    count++;
            return count > 1;
        }
    }
}
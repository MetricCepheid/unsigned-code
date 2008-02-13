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

    public struct BarLine
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
            for (int i = 0; i < 2; i++)
            {
                notes[i] = new List<Note>[4];
                for (int k = 0; k < 4; k++)
                    notes[i][k] = new List<Note>();
            }
            List<Event>[][] events = new List<Event>[2][];
            for (int i = 0; i < 2; i++)
            {
                events[i] = new List<Event>[4];
                for (int k = 0; k < 4; k++)
                    events[i][k] = new List<Event>();
            }

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
                            Console.Out.Write(".");
                        }
                        else if (what.Equals("TS"))
                        {
                            timesigs.Add(new Pair(time, value));
                            Console.Out.Write(".");
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
                    Console.Out.Write(".");
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
                            Console.Out.Write(".");
                        }
                        else if (what.Equals("E"))
                        {
                            string ev = line;
                            events[inst][diff].Add(new Event(ev, time));
                            Console.Out.Write(".");
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

            //PROCESS
            Console.Out.Write("Processing.");

            string timeS="00:00:00";
            List<BarLine> barlines = new List<BarLine>();
            int currentBPM = 0;
            int currentTS = 0;
            int currentBeats = 0, currentTime = offset;
            {
                int k = 0;
                for (int i = 0; i < bpmsigs.Count - 1; i++)
                {
                    if (timesigs[k].time <= bpmsigs[k].time)
                    {
                        currentTS = timesigs[k].value;
                        k++;
                    }
                    currentBPM = bpmsigs[i].value;
                    int numMAdd = (bpmsigs[i + 1].time - bpmsigs[i].time) / (768 / 4 * currentTS);
                    for (int p = 0; p < numMAdd; p++)
                    {
                        barlines.Add(new BarLine(currentTime + (int)(p * (60000f / currentBPM * currentTS * 1000)), currentTS,currentBeats+(p*currentTS)));
                        Console.Out.Write(".");
                    }
                    currentBeats += numMAdd * currentTS;
                    currentTime += (int)(60000f / currentBPM * currentTS * 1000) * numMAdd;
                }
            }
            int remainingMLen = (int)(60000f / currentBPM * currentTS * 1000);
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
            int rest = (int)(60000f / currentBPM * currentTS * 1000);
            Note[][][] newnotes = new Note[notes.Length][][];
            for (int m = 0; m < notes.Length; m++)
            {
                newnotes[m] = new Note[notes[m].Length][];
                for (int n = 0; n < notes[m].Length; n++)
                {
                    newnotes[m][n] = notes[m][n].ToArray();
                    int k = 0;
                    for (int i = 0; i < newnotes[m][n].Length; i++)
                    {
                        
                        while (k<barlines.Count && newnotes[m][n][i].time / (768 / 4) >= barlines[k].sBeat + barlines[k].beats)
                            k++;
                        if (k < barlines.Count-1)
                        {
                            int t = barlines[k].time;
                            float d = ((newnotes[m][n][i].time / (768f / 4)) - barlines[k].sBeat) / barlines[k].beats;
                            t += (int)((barlines[k + 1].time - barlines[k].time) * d);
                            newnotes[m][n][i].time = t;
                        }
                        else
                        {
                            int t = barlines[barlines.Count-1].time;
                            float d = ((newnotes[m][n][i].time / (768f / 4)) - barlines[barlines.Count-1].sBeat) / barlines[barlines.Count-1].beats;
                            t += (int)((remainingMLen) * d);
                            newnotes[m][n][i].time = t;
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
            fout.WriteLine(rest);
            fout.WriteLine("Cues:1");
            fout.WriteLine("0:"+cName);
            fout.Close();
            Console.Out.WriteLine("GBA written");
            fout = new System.IO.StreamWriter(args[0].Substring(0, args[0].LastIndexOf("\\") + 1) + cName + ".gbg");
            fout.WriteLine("SPPH:0");
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
            Console.Out.WriteLine("...Done");
        }
    }
}
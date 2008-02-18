using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace GarageBand
{
    class Song
    {
        private int bpm;
        private float mps;
        private String FileName;
        private Vector2[] Bars;
        private int endLength;
        private int DefaultBPM = 4;
        private String SongName, ArtistName;
        private int TimeH, TimeM, TimeS;
        private int currentBar = 0;

        private String[] cues;
        private int[] cuetimes;//last value is song length
        private int cueindex;

        public Song(int bpm, float mps, String FileName)
        {
            this.bpm = bpm;
            this.mps = mps;
            this.FileName = FileName;
            LoadSong(FileName);
            cueindex = 0;
        }

        public int GetBPM()
        {
            return bpm;
        }

        public float GetMPS()
        {
            return mps;
        }

        private void LoadSong(String fn)
        {
            if (!System.IO.File.Exists("songdata\\"+fn+".gba"))
                return;
            System.IO.StreamReader reader = new System.IO.StreamReader("songdata\\" + fn + ".gba");
            String z;
            SongName = reader.ReadLine();
            ArtistName = reader.ReadLine();
            z = reader.ReadLine();
            TimeH = Int32.Parse(z.Substring(0, z.IndexOf(':')));
            z = z.Substring(z.IndexOf(':') + 1);
            TimeM = Int32.Parse(z.Substring(0, z.IndexOf(':')));
            TimeS = Int32.Parse(z.Substring(z.IndexOf(':')+1));
            while (!reader.EndOfStream)
            {
                do { z = reader.ReadLine(); }
                while (z.Length >= 2 && z.Substring(0, 2).Equals("//"));
                if (z.Substring(0, 4).Equals("Bars"))
                {
                    Bars = new Vector2[Int32.Parse(z.Substring(5))];

                    String a;
                    for (int c = 0; c < Bars.Length; c++)
                    {
                        do { a = reader.ReadLine(); }
                        while (a.Length >= 2 && a.Substring(0, 2).Equals("//"));
                        Bars[c] = new Vector2(Int32.Parse(a.Substring(0, a.IndexOf(':'))), Int32.Parse(a.Substring(a.IndexOf(':') + 1)));
                    }
                    do { a = reader.ReadLine(); }
                    while (a.Length >= 2 && a.Substring(0, 2).Equals("//"));
                    endLength = Int32.Parse(a);
                }
                else if (z.Substring(0, 4).Equals("Cues"))
                {
                    cues = new String[Int32.Parse(z.Substring(5))];
                    cuetimes = new int[Int32.Parse(z.Substring(5))+1];

                    String a;
                    for (int c = 0; c < cues.Length; c++)
                    {
                        do { a = reader.ReadLine(); }
                        while (a.Length >= 2 && a.Substring(0, 2).Equals("//"));
                        cuetimes[c] = Int32.Parse(a.Substring(0, a.IndexOf(':')));
                        cues[c] = a.Substring(a.IndexOf(':')+1);
                    }
                    do { a = reader.ReadLine(); }
                    while (a.Length >= 2 && a.Substring(0, 2).Equals("//"));
                    cuetimes[cuetimes.Length-1] = Int32.Parse(a);
                }
            }
        }

        public Vector2[] GetZVals(int currenttime)
        {
            currenttime /= (int)(Game1.TicksPerSecond / 1000);
            int k;
            for (k = 0; k < Bars.Length; k++)
                if (Bars[k].X > currenttime)
                    break;
            k -= 3;
            if (k < 0)
                k = 0;
            Vector2[] ret = new Vector2[12];
            for (int i = 0; i < 12; i++)
            {
                if (i + k >= Bars.Length)
                    ret[i] = new Vector2(((i + k) - (Bars.Length - 1)) * endLength + Bars[Bars.Length - 1].X, DefaultBPM);
                else
                    ret[i] = Bars[i + k];
            }
            for (int i = 0; i < 12; i++)
                ret[i].X = ((ret[i].X)-currenttime)/1000f;
            return ret;
        }

        public void Update(int currenttime, SoundBank asb)
        {
            currenttime /= (int)(Game1.TicksPerSecond / 1000);
            while(true)
            {
                if (currenttime > cuetimes[cueindex] + 2000)
                    cueindex++;
                else
                    break;
            }
            if (cueindex < cues.Length && currenttime > cuetimes[cueindex])
            {
                asb.PlayCue(cues[cueindex]);
                cueindex++;
            }
            if (cueindex<cues.Length && currenttime-2000 > cuetimes[cueindex])
            {
                if (!asb.GetCue(cues[cueindex]).IsPreparing && !asb.GetCue(cues[cueindex]).IsPrepared)
                {
                    asb.GetCue(cues[cueindex]).Play();
                    asb.GetCue(cues[cueindex]).Stop(AudioStopOptions.Immediate);
                }
            }
            if (currentBar<Bars.Length-1 && currenttime >= Bars[currentBar + 1].X)
            {
                currentBar++;
            }
        }

        public bool IsOver(long currenttime)
        {
            if (currenttime/(Game1.TicksPerSecond/1000) > cuetimes[cuetimes.Length - 1])
                return true;
            return false;
        }

        public int[] GetCamTimes()
        {
            return cuetimes;
        }

        public String GetFilename()
        {
            return FileName;
        }

        public Vector2[] GetAllBars()
        {
            return Bars;
        }

        internal float GetMeasureProgress(int currenttime)
        {
            if(currentBar<Bars.Length)
                return (currenttime - Bars[currentBar].X) / (Bars[currentBar + 1].X - Bars[currentBar].X);
            return 0f;
        }

        internal int GetBPMeasure(int currenttime)
        {
            if(currentBar<Bars.Length)
                return (int)Bars[currentBar].Y;
            return 1;
        }
    }
}

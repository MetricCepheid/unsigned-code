using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using IrrKlang;

namespace Unsigned
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
        bool playing = false;
        String[] charters;

#if ! XBOX

        
        //OggPlayManager manager;
        ISoundEngine sEngine;
        ISoundSource song;
        ISound sound;
#else

        

#endif

        public Song(int bpm, float mps, String FileName, IntPtr game)
        {
            this.bpm = bpm;
            this.mps = mps;
            this.FileName = FileName;
            LoadSong(FileName, game);
        }

        public int GetBPM()
        {
            return bpm;
        }

        public float GetMPS()
        {
            return mps;
        }

        private void LoadSong(String fn, IntPtr game)
        {
            if (!System.IO.File.Exists("songdata\\"+fn+".gba"))
                return;
            System.IO.BinaryReader reader = new System.IO.BinaryReader(System.IO.File.OpenRead("songdata\\" + fn + ".gba"));
            byte version = reader.ReadByte();
            SongName = reader.ReadString();
            ArtistName = reader.ReadString();
            String z = reader.ReadString();
            TimeH = Int32.Parse(z.Substring(0, z.IndexOf(':')));
            z = z.Substring(z.IndexOf(':') + 1);
            TimeM = Int32.Parse(z.Substring(0, z.IndexOf(':')));
            TimeS = Int32.Parse(z.Substring(z.IndexOf(':')+1));
            charters = new String[6];
            for (int i = 0; i < 6; i++)
                charters[i] = reader.ReadString();
            Bars = new Vector2[reader.ReadInt32()];

            String a;
            for (int c = 0; c < Bars.Length; c++)
            {
                Bars[c] = new Vector2(reader.ReadInt32(), reader.ReadInt32());
            }
            endLength = reader.ReadInt32();
#if ! XBOX

            sEngine = new ISoundEngine();
            song = sEngine.AddSoundSourceFromFile("audio\\" + FileName + ".ogg", StreamMode.Streaming, true);
            sound = sEngine.Play2D(song, false, true, true);
            /*manager = new OggPlayManager(System.Windows.Forms.Form.FromHandle(game));
            manager.PlayOggFile("audio\\" + FileName + ".ogg", 0);
            manager.StopOggFile(0);*/
#endif
        }

        public Vector2[] GetZVals(long currenttime)
        {
            currenttime /= (long)(Game1.TicksPerSecond / 1000);
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

        public void Update(long currenttime)
        {
            currenttime /= (long)(Game1.TicksPerSecond / 1000);
            if (currenttime >= 0 && !playing)
            {
#if ! XBOX

                sound.Paused = false; ;
                //manager.PlayOggFile("audio\\" + FileName + ".ogg", 0);
                playing = true;
#else

                asb.PlayCue(cues[cueindex]);

#endif
            }
            if (currentBar<Bars.Length-1 && currenttime >= Bars[currentBar + 1].X)
            {
                currentBar++;
            }
        }

        public bool IsOver(long currenttime)
        {
            if (currenttime/Game1.TicksPerSecond > ((TimeH*360)+(TimeM*60)+(TimeS)))
                return true;
            return false;
        }

        public String GetFilename()
        {
            return FileName;
        }

        public Vector2[] GetAllBars()
        {
            return Bars;
        }

        internal float GetMeasureProgress(long currenttime)
        {
            if(currentBar<Bars.Length)
                return (currenttime - Bars[currentBar].X) / (Bars[currentBar + 1].X - Bars[currentBar].X);
            return 0f;
        }

        internal int GetBPMeasure(long currenttime)
        {
            if(currentBar<Bars.Length)
                return (int)Bars[currentBar].Y;
            return 1;
        }
    }
}

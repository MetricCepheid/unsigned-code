using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
#if WINDOWS
using IrrKlang;
#endif

namespace Unsigned
{
    class Song
    {
        private String FileName;
        private Vector2[] Bars;
        private int endLength;
        private int DefaultBPM = 4;
        private String SongName, ArtistName;
        private int TimeH, TimeM, TimeS;
        private int currentBar = 0;
        bool playing = false;
        String[] charters;
        public Vector2[] zVals = new Vector2[12];
        public int[] diffs = new int[4];
        public string[] songInfo;
        public string[] quotes = new string[8];

        public float percentBeat;


#if WINDOWS

        
        //OggPlayManager manager;
        ISoundEngine sEngine;
        ISoundSource song;
        ISound sound;
#else

        AudioEngine engine;
        SoundBank sB;
        WaveBank wB;
        Cue cue;

#endif

#if WINDOWS
        public Song(String FileName, IntPtr game)
#else
        public Song(String FileName, AudioEngine eng, SoundBank sb, WaveBank wb)
#endif
        {
            this.FileName = FileName;
#if !WINDOWS
            engine = eng;
            sB = sb;
            wB = wb;
#endif

            LoadSong(FileName, game);
        }

        private bool LoadSong(String fn, IntPtr game)
        {

            if (!System.IO.File.Exists("songdata\\" + fn + ".gba"))
            {
#if WINDOWS
                System.Windows.Forms.MessageBox.Show("songdata not found");
#endif
                return false;
            }
            System.IO.BinaryReader reader = new System.IO.BinaryReader(System.IO.File.OpenRead("songdata\\" + fn + ".gba"));
            byte version = reader.ReadByte();
            SongName = reader.ReadString();
            ArtistName = reader.ReadString();
            int year = reader.ReadInt32();
            String genre = reader.ReadString();
            String z = reader.ReadString();
            TimeH = Int32.Parse(z.Substring(0, z.IndexOf(':')));
            z = z.Substring(z.IndexOf(':') + 1);
            TimeM = Int32.Parse(z.Substring(0, z.IndexOf(':')));
            TimeS = Int32.Parse(z.Substring(z.IndexOf(':')+1));

            for (int i = 0; i < quotes.Length; i++)
                quotes[i] = reader.ReadString();

            int numCharters = 6;
            charters = new String[numCharters];
            for (int i = 0; i < numCharters; i++)
                charters[i] = reader.ReadString();
            int[] numC = new int[numCharters];
            string[] charters2 = new string[numCharters];
            charters2[0] = charters[0];
            numC[0]++;
            int nC2 = 1;
            for (int i = 1; i < numCharters; i++)
            {
                int k;
                for(k=0;k<nC2;k++)
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

            songInfo = new string[3 + nC2];
            songInfo[0] = SongName;
            songInfo[1] = ArtistName;
            songInfo[2] = "Charter" + (nC2 > 1 ? "s:" : ":");
            for (int i = 0; i < nC2; i++)
                songInfo[i + 3] = charters2[i];

            for(int i=0;i<4;i++)
                diffs[i] = reader.ReadByte();
            int rks = reader.ReadInt32();
            Bars = new Vector2[rks];
            for (int c = 0; c < Bars.Length; c++)
            {
                Bars[c] = new Vector2(reader.ReadInt32(), reader.ReadInt32());
            }
            endLength = reader.ReadInt32();
#if WINDOWS

            sEngine = new ISoundEngine();
            song = sEngine.AddSoundSourceFromFile("audio\\" + FileName + ".ogg", StreamMode.NoStreaming, true);
            sound = sEngine.Play2D(song, false, true, true);
            if (song==null || sound==null)
            {
                System.Windows.Forms.MessageBox.Show("audio not found");
                return false;
            }
            sound.Volume = 0.75f;
            /*manager = new OggPlayManager(System.Windows.Forms.Form.FromHandle(game));
            manager.PlayOggFile("audio\\" + FileName + ".ogg", 0);
            manager.StopOggFile(0);*/
#else
            cue = sB.GetCue(fn);
            cue.Play();
            cue.Pause();
#endif
            return true;
        }

        public void GetZVals(long currenttime)
        {
            int k;
            for (k = 0; k < Bars.Length; k++)
                if (Bars[k].X > (currenttime/(float)(Game1.TicksPerSecond/1000)))
                    break;
            k -= 3;
            if (k < 0)
                k = 0;
            for (int i = 0; i < 12; i++)
            {
                if (i + k >= Bars.Length)
                    zVals[i] = new Vector2(((i + k) - (Bars.Length - 1)) * endLength + Bars[Bars.Length - 1].X, DefaultBPM);
                else
                    zVals[i] = Bars[i + k];
            }
            for (int i = 0; i < 12; i++)
                zVals[i].X = ((zVals[i].X)-(currenttime/(float)(Game1.TicksPerSecond/1000)))/1000f;
        }

        public void play()
        {
#if WINDOWS
            sound.Paused = false; 
                //manager.PlayOggFile("audio\\" + FileName + ".ogg", 0);
                playing = true;
#else
            cue.Resume();
#endif
        }

        public long getTime()
        {
#if WINDOWS
            return sound.PlayPosition;  
#else
            return 0;
#endif
        }

        public void Update(long currenttime)
        {
            if (currenttime >= 0 && !playing)
            {
                
#if WINDOWS

                
#else

                //asb.PlayCue(cues[cueindex]);
                

#endif
            }
            if (currentBar<Bars.Length-1 && currenttime >= Bars[currentBar + 1].X)
            {
                currentBar++;
            }
            if (currentBar >= Bars.Length)
                percentBeat = 0;
            else
            percentBeat = ((currenttime - Bars[currentBar].X) / (Bars[currentBar + 1].X - Bars[currentBar].X))%(1/Bars[currentBar].Y);
        }

        public bool IsOver(long currenttime)
        {
            if ((long)currenttime/1000 > ((TimeH*3600)+(TimeM*60)+(TimeS)))
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

        internal void pause()
        {
#if WINDOWS
            sound.Paused = true;
#else
            cue.Pause();
#endif
        }

        internal void resume(long p)
        {
#if WINDOWS
            sound.PlayPosition = (uint)p;
            sound.Paused = false;
#else
            cue.Resume();
#endif
        }

        public float PercentSong()
        {
#if WINDOWS
            return sound.PlayPosition / (float)sound.PlayLength;
#else
            return 0.0f;
#endif
        }

        public int GetBeatLength()
        {
            return (int)(Bars[currentBar].X / Bars[currentBar].Y);
        }
    }
}

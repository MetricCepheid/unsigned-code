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
    class SongAudioMaster
    {
        private static SongAudioMaster SINGLETON_SongAudioMaster = null;

        private String FileName;
        bool playing = false;
        private String[] songInfo;

        public String[] SongDisplayInfo
        {
            get { return songInfo; }
        }

#if WINDOWS
        private IntPtr WINDOW;
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
        private SongAudioMaster(IntPtr game)
        {
            WINDOW = game;
        }
#else
        private SongAudioMaster(AudioEngine eng, SoundBank sb, WaveBank wb)
        {
            engine = eng;
            sB = sb;
            wB = wb;
        }
#endif

        public bool InitSong(SongData songdata)
        {
            if (songdata == null)
                return false;

#if WINDOWS

            sEngine = new ISoundEngine();
            String fn = songdata.info.filename;
            if (fn.Contains("\\") || fn.ToLower().EndsWith("gba") || fn.ToLower().EndsWith("uns"))
            {
                fn = fn.Substring(fn.LastIndexOf('\\') + 1);
                fn = fn.Substring(0, fn.LastIndexOf('.'));
            }
            song = sEngine.AddSoundSourceFromFile("audio\\" + fn + ".ogg", StreamMode.NoStreaming, true);
            sound = sEngine.Play2D(song, false, true, true);
            if (song == null || sound == null)
            {
                System.Windows.Forms.MessageBox.Show("audio not found");
                return false;
            }
            sound.Volume = 0.75f;
#else
            cue = sB.GetCue(fn);
            cue.Play();
            cue.Pause();
#endif
            return true;
        }

        /* 
         * legacy.... still needed?
         * 
        public void GetZVals(long currenttime)
        {
            int k;
            for (k = 0; k < Bars.Length; k++)
                if (Bars[k].X > (currenttime))
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
                zVals[i].X = ((zVals[i].X)-(currenttime))/1000f;
        }
         */

        public void Play()
        {
#if WINDOWS
            sound.Paused = false; 
            playing = true;
#else
            cue.Resume();
#endif
        }

        public double GetTime()
        {
#if WINDOWS
            return sound.PlayPosition/1000.0;  
#else
            return 0;
#endif
        }

        /*
         * legacy again... but percent beat code :P
         * 
        public void Update(long currenttime)
        {
            //percentBeat = ((currenttime - Bars[currentBar].X) / (Bars[currentBar + 1].X - Bars[currentBar].X))%(1/Bars[currentBar].Y);
        }
         */

        /*
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
         */

        internal void Pause()
        {
#if WINDOWS
            sound.Paused = true;
#else
            cue.Pause();
#endif
        }

        internal void Resume()
        {
#if WINDOWS
            sound.PlayPosition = (uint)(RhythmMaster.GetSingleton().GetCurrentTime()*1000);
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

        /*
        public int GetBeatLength()
        {
            return (int)(Bars[currentBar].X / Bars[currentBar].Y);
        }
         */

        public static void CreateSingleton(IntPtr game)
        {
            SINGLETON_SongAudioMaster = new SongAudioMaster(game);
        }

        public static SongAudioMaster GetSingleton()
        {
            return SINGLETON_SongAudioMaster;
        }

        public static void DestroySingleton()
        {
            SINGLETON_SongAudioMaster = null;
        }

        public TimeSpan GetSongLength()
        {
            return new TimeSpan(0, 0, 0, 0, (int)sound.PlayLength);
        }
    }
}

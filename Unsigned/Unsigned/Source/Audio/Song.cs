using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using SongDataIO;
#if WINDOWS
using IrrKlang;
#endif

namespace Unsigned
{
    public class Song
    {
        public bool IsPlaying { get { return !sound.Paused && !sound.Finished; } }

        private IntPtr WINDOW;
        ISoundEngine sEngine;
        ISoundSource song;
        ISound sound;

        private bool started = false;

        public Song(IntPtr game)
        {
            WINDOW = game;
        }

        public bool InitSong(SongData songdata)
        {
            if (songdata == null)
                return false;

            started = false;

            sEngine = new ISoundEngine();
            String fn = songdata.info.filename;
            if (fn.Contains("\\") || fn.ToLower().EndsWith("gba") || fn.ToLower().EndsWith("uns"))
            {
                fn = fn.Substring(fn.LastIndexOf('\\') + 1);
                fn = fn.Substring(0, fn.LastIndexOf('.'));
            }
            song = sEngine.AddSoundSourceFromFile("audio\\" + fn + ".ogg", StreamMode.Streaming, true);
            sound = sEngine.Play2D(song, false, true, true);
            if (song == null || sound == null)
            {
                System.Windows.Forms.MessageBox.Show("Audio file \"" + System.IO.Directory.GetCurrentDirectory() + "\\audio\\" + fn + ".ogg\" not found");
                return false;
            }
            sound.Volume = 0.75f;

            return true;
        }

        public void Play()
        {
            sound.Paused = false;
            started = true;
        }

        public double GetTime()
        {
            return sound.PlayPosition/1000.0;
        }

        internal void Pause()
        {
            sound.Paused = true;
        }

        internal void Resume(SongTime songTime)
        {
            if (started)
            {
                sound.PlayPosition = (uint)(songTime.TotalSongTime.TotalMilliseconds);
                sound.Paused = false;
            }
        }

        public float PercentSong()
        {
            return sound.PlayPosition / (float)sound.PlayLength;
        }

        public TimeSpan GetSongLength()
        {
            return new TimeSpan(0, 0, 0, 0, (int)sound.PlayLength);
        }

        internal void Restart()
        {
            sound.Paused = true;
            sound.PlayPosition = 0;
            started = false;
        }

        internal void SetTime(float p)
        {
            sound.PlayPosition = (uint)(p * 1000);
        }

        internal void Stop()
        {
            sound.Stop();
        }
    }
}

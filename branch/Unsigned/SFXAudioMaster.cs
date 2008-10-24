using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Audio;

namespace Unsigned
{
    class SFXAudioMaster
    {
        private static SFXAudioMaster SINGLETON_SFXAudioMaster = null;

        private AudioEngine engine;
        private SoundBank sB;
        private WaveBank wB;


        private SFXAudioMaster()
        {
            engine = new AudioEngine("audio\\Win\\Unsigned.xgs");
            sB = new SoundBank(engine, "audio\\Win\\Sound Bank.xsb");
            wB = new WaveBank(engine, "audio\\Win\\Wave Bank.xwb");
        }


        public void Play(String sound)
        {
            sB.PlayCue(sound);
        }

        public static void CreateSingleton(IntPtr game)
        {
            SINGLETON_SFXAudioMaster = new SFXAudioMaster();
        }

        public static SFXAudioMaster GetSingleton()
        {
            return SINGLETON_SFXAudioMaster;
        }

        public static void DestroySingleton()
        {
            SINGLETON_SFXAudioMaster = null;
        }
    }
}

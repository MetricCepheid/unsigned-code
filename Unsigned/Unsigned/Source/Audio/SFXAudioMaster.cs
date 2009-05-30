using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Audio;

namespace Unsigned
{
    class SFXAudioMaster
    {
        private static SFXAudioMaster _singleton = null;
        public static SFXAudioMaster Singleton
        {
            get
            {
                if (_singleton == null)
                    _singleton = new SFXAudioMaster();
                return _singleton;
            }
        }

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
    }
}

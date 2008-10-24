using System;
using System.Collections.Generic;
using System.Text;
using DS = Microsoft.DirectX.DirectSound;

namespace XNAP
{
    public class XNAPDevice
    {
        internal DS.Device DSDevice;
        private Dictionary<String, XNAPSound> initialSounds;
        private Dictionary<String, XNAPSubSound> playableSounds;

        private static int numInstances = 0;
        private static bool ignoreMI = false;
        public static bool IgnoreMultipleInstances
        {
            get { return ignoreMI; }
            set { ignoreMI = value; }
        }

        public XNAPDevice(IntPtr Owner)
        {
            numInstances++;
            if (numInstances > 1 && !ignoreMI)
                throw new MultipleDeviceException("Multiple Instantiations of XNAPDevice are not permitted.\n"+
                                                  "To establish multiple devices, please turn off XNAPDevice.IgnoreMultipleInstances");
            DSDevice = new DS.Device();
            DSDevice.SetCooperativeLevel(Owner, Microsoft.DirectX.DirectSound.CooperativeLevel.Normal);
            initialSounds = new Dictionary<string, XNAPSound>();
            playableSounds = new Dictionary<string, XNAPSubSound>();
        }

        public void AddSound(XNAPSound sound)
        {
            initialSounds.Add(sound.Key, sound);
        }

        /// <summary>
        /// Plays a sound once
        /// </summary>
        /// <param name="soundName">The Key of the sound to be played</param>
        public void Play(String soundName)
        {
            XNAPSound sound = initialSounds[soundName];
            XNAPSubSound snd = new XNAPSubSound(this, sound);
            snd.Play();
            playableSounds.Add(snd.Key, snd);
        }
    }
}

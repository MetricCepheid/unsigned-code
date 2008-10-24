using System;
using System.Collections.Generic;
using System.Text;
using DS = Microsoft.DirectX.DirectSound;

namespace XNAP
{
    internal class XNAPSubSound
    {
        private DS.SecondaryBuffer buffer;

        private String key;
        public String Key
        {
            get { return key; }
        }

        private double position;
        internal float Position
        {
            get { return (float)position; }
        }

        private double length;
        internal float Length
        {
            get { return (float)length; }
        }

        private bool looping;

        /// <summary>
        /// Creates a new XNAPSound
        /// Defaults to Simple Sound Effect configuration
        /// </summary>
        /// <param name="device">The XNAPDevice currently in use</param>
        /// <param name="FileName">Name of the File to Load</param>
        internal XNAPSubSound(XNAPDevice device, XNAPSound sound)
        {
            this.buffer = sound.buffer.Clone(device.DSDevice);
            this.position = 0.0;
            
            this.key = sound.Key;
        }

        /// <summary>
        /// Plays this sound once
        /// </summary>
        internal void Play()
        {
            buffer.Play(0, DS.BufferPlayFlags.Default);
            looping = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            position += gameTime.ElapsedGameTime.TotalSeconds;
            
        }

        /// <summary>
        /// Plays this sound until a Stop() is called
        /// </summary>
        internal void Loop()
        {
            buffer.Play( 0, DS.BufferPlayFlags.Default);
            looping = true;
        }
    }

    public class XNAPSound
    {
        internal DS.SecondaryBuffer buffer;
        private String key;
        public String Key
        {
            get { return key; }
        }

        public enum SoundType
        {
            SoundEffect = 0,
            Music,
            SimpleSoundEffect,
            EnhancedSoundEffect,
            EnhancedMusic,
            MusicThread,
            EnhancedMusicThread
        };

        /// <summary>
        /// Creates a new XNAPSound
        /// Defaults to Simple Sound Effect configuration
        /// </summary>
        /// <param name="device">The XNAPDevice currently in use</param>
        /// <param name="FileName">Name of the File to Load</param>
        public XNAPSound(XNAPDevice device, String FileName)
        {
            DS.BufferDescription description = new DS.BufferDescription();
            description.ControlEffects = false;
            buffer = new DS.SecondaryBuffer(FileName, description, device.DSDevice);

            key = FileName;
            if(FileName.LastIndexOf('\\')>=0)
                key = key.Substring(key.LastIndexOf('\\')+1);
            if (FileName.LastIndexOf('/') >= 0)
                key = key.Substring(key.LastIndexOf('/')+1);
            key = key.Substring(0,key.LastIndexOf('.'));
        }

        /// <summary>
        /// Creates a new XNAPSound
        /// Defaults to Simple Sound Effect configuration
        /// </summary>
        /// <param name="device">The XNAPDevice currently in use</param>
        /// <param name="FileName">Name of the File to Load</param>
        /// <param name="key">Program-Friendly name of the Sound</param>
        public XNAPSound(XNAPDevice device, String FileName, String key)
        {
            DS.BufferDescription description = new DS.BufferDescription();
            description.ControlEffects = false;
            buffer = new DS.SecondaryBuffer(FileName, description, device.DSDevice);

            this.key = key;
        }
    }
}

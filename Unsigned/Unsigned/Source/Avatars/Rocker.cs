using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using SongDataIO;
using FVProductions.Utility;

namespace Unsigned
{
    class Rocker
    {
        public Vector3 Position { get; set; }
        public float Yaw { get; set; }
        private FVModel model;
        private Texture2D tex;
        private int characterIndex;
        private Instrument instrument;
                                     
        public Rocker(int characterindex)
        {
            this.characterIndex = characterindex;
            Yaw = 0;
        }

        protected void LoadModel(ContentManager Content)
        {
        }

        public void Draw(GameTime gameTime)
        {
        }

        public CharacterIdol GetCharacter()
        {
            return CharacterMaster.Singleton.GetCharacter(characterIndex);
        }

        public Instrument GetInstrument()
        {
            return instrument;
        }
    }
}

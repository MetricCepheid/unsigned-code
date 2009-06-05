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
        public static SongData SongData;
        public Vector3 Position { get; set; }
        public float Yaw { get; set; }
        private Dictionary<String,FVModel> models;
        private Texture2D tex;
        private Instrument instrument;
        private Texture2D texInstr;
        private FVModel mdlInstr;
        private CharacterIdol idol;
                                     
        public Rocker(CharacterIdol idol, Instrument instr)
        {
            instrument = instr;
            Yaw = 0;
            this.idol = idol;
        }

        public void Load(ContentManager Content, String brand, int index)
        {

        }

        public void Draw(GameTime gameTime)
        {
        }

        public CharacterIdol GetCharacter()
        {
            return idol;
        }

        public Instrument GetInstrument()
        {
            return instrument;
        }
    }
}

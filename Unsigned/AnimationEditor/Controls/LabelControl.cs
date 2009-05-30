using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AnimationEditor
{
    public class LabelControl : XNAControl
    {
        public String Text { get; private set; }
        public float Scale { get; set; }

        public LabelControl(String name, String text)
        {
            Name = name;
            Text = text;
            Scale = 1.0f;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(Global.DefaultFont, Text, new Vector2(Bounds.X, Bounds.Y), Color.Black, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
            base.Draw(spriteBatch);
        }
    }
}

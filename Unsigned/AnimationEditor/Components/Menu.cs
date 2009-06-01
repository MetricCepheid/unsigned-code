using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using FVProductions.Utility;

namespace AnimationEditor
{
    public class Menu : XNAComponent
    {
        private SpriteBatch spriteBatch;

        public Menu(String name)
        {
            Name = name;
        }

        public override void Load(ContentManager Content)
        {
            spriteBatch = new SpriteBatch(Global.Graphics.GraphicsDevice);
        }

        public override void Update(GameTime gameTime)
        {

        }

        public override void InnerDraw()
        {
            spriteBatch.Begin();
            spriteBatch.Draw(Global.TexWhite, Bounds, Color.Gray);
            spriteBatch.End();
        }

        public override RightClickMenu GetRightClickMenu(Point p)
        {
            return null;
        }
    }
}

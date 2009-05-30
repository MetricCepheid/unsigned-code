using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AnimationEditor
{
    public class RightClickMenu
    {
        public struct RightClickMenuOption
        {
            public String Name;
            public VoidDelegate VoidDelegate;

            public RightClickMenuOption(String name, VoidDelegate voidDelegate)
            {
                Name = name;
                VoidDelegate = voidDelegate;
            }
        }

        private Point Position;
        private List<RightClickMenuOption> Options;
        private int selected;

        public bool ShouldDestroy { get; private set; }

        public RightClickMenu(Point pos)
        {
            Position = pos;
            Options = new List<RightClickMenuOption>();
            selected = -1;
            ShouldDestroy = false;
        }

        public void AddOption(RightClickMenuOption option)
        {
            Options.Add(option);
        }

        public void Update(GameTime gameTime)
        {
            Point mousePt = new Point(Mouse.GetState().X, Mouse.GetState().Y);
            Vector2 size = new Vector2(0, 0);
            for (int i = 0; i < Options.Count; i++)
            {
                Vector2 sz = Global.DefaultFont.MeasureString(Options[i].Name);
                if (sz.X+4 > size.X)
                    size.X = sz.X+4;
                size.Y += sz.Y;
            }
            size += new Vector2(4, 4);
            float y = Position.Y+2;
            float x = Position.X+2;
            bool leftPressed = Mouse.GetState().LeftButton == ButtonState.Pressed;
            selected = -1;
            for (int i = 0; i < Options.Count; i++)
            {
                Vector2 sz = Global.DefaultFont.MeasureString(Options[i].Name);
                Rectangle rect = new Rectangle((int)x, (int)y, (int)sz.X, (int)sz.Y);
                if (rect.Contains(mousePt))
                {
                    selected = i;
                    if (leftPressed)
                    {
                        Options[i].VoidDelegate.Invoke();
                        ShouldDestroy = true;
                    }
                }
                y += sz.Y;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Vector2 size = new Vector2(0,0);
            for(int i=0;i<Options.Count;i++)
            {
                Vector2 sz = Global.DefaultFont.MeasureString(Options[i].Name);
                if(sz.X+4 > size.X)
                    size.X = sz.X+4;
                size.Y += sz.Y;
            }
            size += new Vector2(4, 4);
            Rectangle menuRect = new Rectangle(Position.X, Position.Y, (int)size.X, (int)size.Y);
            spriteBatch.Draw(Global.TexWhite, menuRect, Color.Gray);
            float y = Position.Y+2;
            float x = Position.X+2;
            for (int i = 0; i < Options.Count; i++)
            {
                Vector2 sz = Global.DefaultFont.MeasureString(Options[i].Name);
                Rectangle rect = new Rectangle((int)x, (int)y, (int)sz.X+4, (int)sz.Y);
                if (selected == i)
                    spriteBatch.Draw(Global.TexWhite, rect, new Color(1f, 1f, 1f, 0.2f));
                spriteBatch.DrawString(Global.DefaultFont, Options[i].Name, new Vector2(x+2, y+2), Color.White);
                y += sz.Y;
            }
        }
    }
}

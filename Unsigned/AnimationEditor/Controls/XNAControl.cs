using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace AnimationEditor
{
    public abstract class XNAControl
    {
        private static float _atmol = 1;
        public static float AltTextMouseOverLength { get { return _atmol; } set { _atmol = value; } }

        private Rectangle _bounds;
        public Rectangle Bounds
        {
            get { return _bounds; }
            set
            {
                _bounds = value;
                OnResize();
            }
        }

        public String Name { get; protected set; }

        private bool _enabled;
        public bool Enabled 
        { 
            get { return _enabled; } 
            set 
            { 
                _enabled = value; 
                if(EnabledChanged!=null) 
                    EnabledChanged.Invoke(this, new EventArgs()); 
            } 
        }

        public String AltText { get; set; }

        private float altTextCounter;
        private float altTextAlpha;

        public EventHandler EnabledChanged;

        public XNAControl()
        {
        }

        public virtual void Load(ContentManager Content) { }

        public virtual void Update(GameTime gameTime, Point mousePoint)
        {
            if (Bounds.Contains(mousePoint))
                altTextCounter += (float)gameTime.ElapsedGameTime.TotalSeconds;
            else
                altTextCounter = 0;
            if (altTextCounter >= AltTextMouseOverLength)
                altTextAlpha = Math.Min(altTextAlpha + (float)gameTime.ElapsedGameTime.TotalSeconds * 10, 1);
            else
                altTextAlpha = Math.Max(altTextAlpha - (float)gameTime.ElapsedGameTime.TotalSeconds * 10, 0);
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (altTextAlpha > 0)
            {
                String txt = "";
                if(AltText != null)
                    txt = AltText;
                if (txt.Length > 0)
                {
                    Vector2 measStr = Global.DefaultFont.MeasureString(txt) * 0.5f;
                    spriteBatch.Draw(Global.TexWhite, new Rectangle(Bounds.Center.X, Bounds.Center.Y, (int)measStr.X + 4, (int)measStr.Y + 4), new Color(Color.Yellow, altTextAlpha));
                    spriteBatch.DrawString(Global.DefaultFont, txt, new Vector2(Bounds.Center.X + 2, Bounds.Center.Y + 2), Color.Black, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);
                }
            }
        }

        public virtual RightClickMenu GetRightClickMenu(Point p)
        {
            return null;
        }

        public virtual void OnResize()
        {

        }
    }
}

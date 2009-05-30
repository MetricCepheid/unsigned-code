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
    public abstract class XNAComponent
    {
        private Rectangle _bounds;
        public Rectangle Bounds
        {
            get { return _bounds; }
            set
            {
                _bounds = value;
                if (rt != null)
                    rt.Dispose();
                rt = new RenderTarget2D(Global.Graphics.GraphicsDevice, _bounds.Width, _bounds.Height, 1, SurfaceFormat.Color);
                OnResize();
            }
        }
        private RenderTarget2D rt;
        private Texture2D retTex;

        public String Name { get; protected set; }

        public void Draw()
        {
            retTex = null;
            Global.Graphics.GraphicsDevice.SetRenderTarget(0, rt);
            Global.Graphics.GraphicsDevice.Clear(Color.Black);
            InnerDraw();
        }

        public abstract void InnerDraw();

        public abstract void Load(ContentManager Content);

        public abstract void Update(GameTime gameTime);

        public Texture2D GetTexture()
        {
            if (retTex == null)
                retTex = rt.GetTexture();
            return retTex;
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

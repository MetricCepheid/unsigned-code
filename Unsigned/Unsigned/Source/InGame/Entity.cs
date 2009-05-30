using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using FVProductions.Utility;

namespace Unsigned
{
    public abstract class Entity
    {
        protected Entity parent;
        protected Vector3 relLoc;
        public abstract void Update(SongTime gameTime);
        public abstract void Draw(FVShader engine, Vector3 CamPos);
        public virtual Matrix GetTransform()
        {
            if(parent!=null)
                return Matrix.CreateTranslation(relLoc)*parent.GetTransform();
            return Matrix.CreateTranslation(relLoc);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Unsigned
{
    public abstract class BaseState
    {
        public abstract void Update(GameTime gameTime);
        public abstract void Render(GameTime gameTime);
        public abstract void Load(Microsoft.Xna.Framework.Content.ContentManager content);
        public abstract void Unload();
    }
}

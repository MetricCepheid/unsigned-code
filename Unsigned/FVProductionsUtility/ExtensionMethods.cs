using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using System.IO;

namespace FVProductions.Utility
{
    public static class ExtensionMethods
    {
        public static void EnableAlphaBlending(this GraphicsDevice graphicsDevice)
        {
            graphicsDevice.RenderState.AlphaBlendEnable = true;
            graphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
            graphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
        }

        public static void EnableAdditiveBlending(this GraphicsDevice graphicsDevice)
        {
            graphicsDevice.RenderState.AlphaBlendEnable = true;
            graphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
            graphicsDevice.RenderState.DestinationBlend = Blend.One;
        }

        public static void DisableAlphaBlending(this GraphicsDevice graphicsDevice)
        {
            graphicsDevice.RenderState.AlphaBlendEnable = false;
        }
    }
}

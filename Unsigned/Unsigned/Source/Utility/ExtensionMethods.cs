using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FVProductions.Utility;

namespace Unsigned
{
    public static class ExtensionMethods
    {
        public static void ApplyScreenConfiguration(this GraphicsDeviceManager graphics)
        {
            if (Configuration.WideScreen)
            {
                graphics.PreferredBackBufferWidth = Configuration.ResolutionWidthOptions[Configuration.ResIndex];
                graphics.PreferredBackBufferHeight = Configuration.ResolutionHeightWideOptions[Configuration.ResIndex];
            }
            else
            {
                graphics.PreferredBackBufferWidth = Configuration.ResolutionWidthOptions[Configuration.ResIndex];
                graphics.PreferredBackBufferHeight = Configuration.ResolutionHeightFullOptions[Configuration.ResIndex];
            }
            graphics.IsFullScreen = Configuration.FullScreen;
            graphics.ApplyChanges();
        }

        private static VertexTangentBinormal[] square = null;
        public static void DrawSquare(this GraphicsDevice graphicsDevice)
        {
            if (square == null)
            {
                square = new VertexTangentBinormal[6];
                square[0] = new VertexTangentBinormal(new Vector3(-1f, 0f, 1f), new Vector2(0f, 0f), new Vector3(0f, 1f, 0f), new Vector3(1, 0, 0), new Vector3(0, 0, 1));
                square[1] = new VertexTangentBinormal(new Vector3(-1f, 0f, -1f), new Vector2(0f, 1f), new Vector3(0f, 1f, 0f), new Vector3(1, 0, 0), new Vector3(0, 0, 1));
                square[2] = new VertexTangentBinormal(new Vector3(1f, 0f, 1f), new Vector2(1f, 0f), new Vector3(0f, 1f, 0f), new Vector3(1, 0, 0), new Vector3(0, 0, 1));
                square[3] = new VertexTangentBinormal(new Vector3(1f, 0f, 1f), new Vector2(1f, 0f), new Vector3(0f, 1f, 0f), new Vector3(1, 0, 0), new Vector3(0, 0, 1));
                square[4] = new VertexTangentBinormal(new Vector3(-1f, 0f, -1f), new Vector2(0f, 1f), new Vector3(0f, 1f, 0f), new Vector3(1, 0, 0), new Vector3(0, 0, 1));
                square[5] = new VertexTangentBinormal(new Vector3(1f, 0f, -1f), new Vector2(1f, 1f), new Vector3(0f, 1f, 0f), new Vector3(1, 0, 0), new Vector3(0, 0, 1));
            }
            graphicsDevice.VertexDeclaration = VertexTangentBinormal.VertexDeclaration;
            graphicsDevice.DrawUserPrimitives<VertexTangentBinormal>(PrimitiveType.TriangleList, square, 0, 2);
        }
    }
}

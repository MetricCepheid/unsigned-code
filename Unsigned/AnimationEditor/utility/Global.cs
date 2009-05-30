using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace AnimationEditor
{
    static class Global
    {
        public static Texture2D TexWhite, TexDefaultBM;
        //public const byte D_EASY = 3, D_MEDIUM = 6, D_HARD = 12, D_EXPERT = 24;
        //public static String[] DifficultyStr = { "Easy", "Medium", "Hard", "Expert" };
        //public static Color[] FretColors = { Color.Green, Color.Red, Color.Yellow, Color.Blue, Color.Orange, };
        public static SpriteFont DefaultFont;
        //public static long TicksPerSecond = 10000000;
        public static Random Random;

        public static GraphicsDeviceManager Graphics;
        public static IServiceProvider Services;

        public static int ScreenWidth { get { return Graphics.GraphicsDevice.Viewport.Width; } }
        public static int ScreenHeight { get { return Graphics.GraphicsDevice.Viewport.Height; } }
        public static Rectangle SafeArea
        {
            get
            {
                //return Graphics.GraphicsDevice.Viewport.TitleSafeArea;
                return new Rectangle(ScreenWidth / 10, ScreenHeight / 10, ScreenWidth * 8 / 10, ScreenHeight * 8 / 10);
            }
        }
    }
}

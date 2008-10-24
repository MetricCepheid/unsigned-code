using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Unsigned
{
    static class GameSettings
    {
        private static bool fScreen = false;
        public static bool fullScreen
        {
            get { return fScreen; }
            set { fScreen = value; RenderMaster.GetSingleton().graphics.IsFullScreen = value; }
        }
        public static bool render3D = true;
        public static int renderLevel = 10;
        public static bool HALF_RENDER = false;
        public static GameUIMaster.GUIStyle guiStyle = GameUIMaster.GUIStyle.UN;
        public static bool ShowFPS;
        public static int waveDetail;
        public static bool WideScreen
        {
            get { return RenderMaster.GetSingleton().graphics.GraphicsDevice.DisplayMode.AspectRatio < 1.3f; }
        }
        public static int windowheight
        {
            get { return resY[resIndex]; }
        }
        public static int windowwidth
        {
            get { return resX[resIndex]; }
        }
        public static int[] resX = { 640, 800, 960, 1024, 1152, 1280, 1440, 1600, 1920, 2048};
        private static int[] rYF = { 480, 600, 720,  768,  864,  960, 1080, 1200, 1440, 1536};
        private static int[] rYW = { 360, 480, 540,  576,  720,  720,  900, 1024, 1200, 1152};
        public static int[] resY
        {
            get { return WideScreen ? rYW : rYF; }
        }
        private static int mResIndex;
        public static int resIndex
        {
            get { return mResIndex; }
            set { RenderMaster.GetSingleton().graphics.PreferredBackBufferWidth = resX[value]; RenderMaster.GetSingleton().graphics.PreferredBackBufferHeight = resY[value]; mResIndex = value; }
        }
        public static Rectangle Resolution
        {
            get { return new Rectangle(0, 0, resX[resIndex], resY[resIndex]); }
        }
        public static bool NormalMapping=false, Specular=false, Lighting=true;
    }
}

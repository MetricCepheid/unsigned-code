using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Unsigned
{
    static class GameSettings
    {
        public static bool fullScreen = false;
        public static bool render3D = true;
        public static int renderLevel = 10;
        public static bool HALF_RENDER = false;
        public static GameUIMaster.GUIStyle cGUIStyle = GameUIMaster.GUIStyle.UN;
        public static bool ShowFPS;
        public static int waveDetail;
        public static int windowheight, windowwidth;
        public static int[] resX = { 640, 800, 1024, };
        public static int[] resY = { 480, 600, 768, };
        public static int resIndex;
    }
}

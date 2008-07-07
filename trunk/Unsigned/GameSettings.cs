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
    }

    static class RenderData
    {
        public static Effect engine, ppEngine, fader;
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Unsigned
{
    class GameUIMaster
    {
        private static GameUIMaster SINGLETON_GameUIMaster = null;

        private float UIHScale, UIVScale;

        private Vector2 rockstarLoc, rockstarScale;
        private float rockstarDir;
        private Texture2D texRockstarRed, texRockstarRing, texRockstarCover;
        private Texture2D[] texRockstarRingHiLi;
        private Texture2D texScoreBoard;

        public enum GUIStyle { RB = 0, GH = 2, UN = 1 };

        private Texture2D texRockMeterOutline;
        private Texture2D texRockMeterGuitarLogo, texRockMeterBassLogo,
                          texRockMeterDrumLogo, texRockMeterSingerLogo;
        private Texture2D texRockMeterLogoStem;
        private Vector2 rockMeterLoc, rockMeterScale;
        private Texture2D rmUNbg, rmUNfg, rmUNstar, rmUNstaro;

        Texture2D lastframe;

        private Matrix matView;
        private Matrix matProj;
        private RenderTarget2D screenTarget, screenTargetPre, screenTargetFinal;
        private RenderTarget2D[] boardsTarget;

        private GameUIMaster()
        {

        }

        ~GameUIMaster()
        {

        }



        public static void CreateSingleton()
        {
            if (SINGLETON_GameUIMaster == null)
                SINGLETON_GameUIMaster = new GameUIMaster();
            else
                throw new InvalidOperationException("Singleton has already been initialized");
        }

        public static void DestroySingleton()
        {
            if (SINGLETON_GameUIMaster != null)
                SINGLETON_GameUIMaster = null;
            else
                throw new InvalidOperationException("Singleton has already been destroyed");
        }

        public static GameUIMaster GetSingleton()
        {
            return SINGLETON_GameUIMaster;
        }
    }
}

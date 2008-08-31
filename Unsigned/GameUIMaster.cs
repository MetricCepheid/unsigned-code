using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Unsigned
{
    class GameUIMaster
    {
        private static GameUIMaster SINGLETON_GameUIMaster = null;

        private float UIHScale, UIVScale;

        private Vector2 rockstarLoc, rockstarScale;
        private float rockstarDir;
        private Texture2D texRockstarRed, texRockstarRing, texRockstarCover;
        private Texture2D[] texRockstarRingHiLi, texMult;
        private Texture2D texScoreBoard;

        public enum GUIStyle { RB = 0, GH = 2, UN = 1 };

        private Texture2D texRockMeterOutline;
        private Texture2D texRockMeterGuitarLogo, texRockMeterBassLogo,
                          texRockMeterDrumLogo, texRockMeterSingerLogo;
        private Texture2D texRockMeterLogoStem;
        private Vector2 rockMeterLoc, rockMeterScale;
        private Texture2D rmUNbg, rmUNfg, rmUNstar, rmUNstaro;

        private static Vector2 pauseMenuPos, pauseMenuVel, pauseWingRot;
        private static float pauseRot;
        private static int pauseSelected;
        private static int pauseSelectOwner;
        private static String[] pauseTextDisp;
        private static Texture2D texPauseBorder, texPauseWings, texPausePick;

        public Texture2D texButtonGreen, texButtonRed, texButtonYellow;

        private static Texture2D lastframe;

        private RenderTarget2D[] boardsTarget;

        private GameUIMaster()
        {
            int wwidth = GameSettings.resX[GameSettings.resIndex];
            int wheight = GameSettings.resY[GameSettings.resIndex];
            rockstarLoc = new Vector2(wwidth * 0.8f, wheight * (/*(vocalist) ? 0.25f :*/ 0.1f));
            rockstarScale = new Vector2(wwidth * 0.2f, wheight * 0.1f);
            rockMeterLoc = new Vector2(wwidth * 0.02f, wheight * 0.20f);
            rockMeterScale = new Vector2(wwidth * 0.02f, wheight * 0.5f);
            rockstarDir = 0;
        }

        ~GameUIMaster()
        {

        }

        public void Load(ContentManager content)
        {
            texRockstarRed = content.Load<Texture2D>("graphics\\red");
            texRockstarRing = content.Load<Texture2D>("graphics\\ring");
            texRockstarCover = content.Load<Texture2D>("graphics\\starcover");
            texRockMeterOutline = content.Load<Texture2D>("graphics\\rockmeter");
            texRockMeterLogoStem = content.Load<Texture2D>("graphics\\logo_stem");
            texRockMeterGuitarLogo = content.Load<Texture2D>("graphics\\guitar_logo");
            texRockMeterBassLogo = content.Load<Texture2D>("graphics\\bass_logo");
            texRockMeterDrumLogo = content.Load<Texture2D>("graphics\\drums_logo");
            texRockMeterSingerLogo = content.Load<Texture2D>("graphics\\vocal_logo");
            texScoreBoard = content.Load<Texture2D>("graphics\\scoreboard");
            rmUNbg = content.Load<Texture2D>("graphics\\roundmeterbg");
            rmUNfg = content.Load<Texture2D>("graphics\\roundmeterfg");
            rmUNstar = content.Load<Texture2D>("graphics\\scorestar");
            rmUNstaro = content.Load<Texture2D>("graphics\\scorestaro");
            texRockstarRingHiLi = new Texture2D[11];
            for (int i = 0; i <= 9; i++)
                texRockstarRingHiLi[i] = content.Load<Texture2D>("graphics\\border0" + i);
            texRockstarRingHiLi[10] = content.Load<Texture2D>("graphics\\border10");
            texPauseBorder = content.Load<Texture2D>("graphics\\pauseborder");
            texPauseWings = content.Load<Texture2D>("graphics\\pausewing");
            texPausePick = content.Load<Texture2D>("graphics\\pickofselect");
            pauseMenuPos = new Vector2(GameSettings.windowwidth / 2, GameSettings.windowheight / 2);
            pauseMenuVel = new Vector2(10, 0);
            pauseWingRot.Y = 30;
            texMult = new Texture2D[8];
            for (int i = 0; i < Global.multToIndex.Length; i++)
                if (Global.multToIndex[i] >= 0)
                    texMult[Global.multToIndex[i]] = content.Load<Texture2D>("graphics\\X" + i);
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

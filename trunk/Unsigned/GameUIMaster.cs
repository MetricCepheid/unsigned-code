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
        private Texture2D[] texRockstarRingHiLi;
        private Texture2D texScoreBoard;

        private byte lastStar; //for ching after star gain

        public enum GUIStyle { RB = 0, GH = 2, UN = 1 };

        private Texture2D texRockMeterOutline;
        private Texture2D texRockMeterGuitarLogo, texRockMeterBassLogo,
                          texRockMeterDrumLogo, texRockMeterSingerLogo;
        private Texture2D texRockMeterLogoStem;
        private Vector2 rockMeterLoc, rockMeterScale;
        private Texture2D rmUNbg, rmUNfg, rmUNstar, rmUNstaro;

        public Texture2D texButtonGreen, texButtonRed, texButtonYellow;

        private Rectangle GUIArea;

        private Color rmColor;

        

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
            lastStar = 0;
        }

        private void DrawRockMeter()
        {
            double currentTime = RhythmMaster.GetSingleton().GetCurrentTime();
            float failTime = RhythmMaster.GetSingleton().GetFailTime();
            SpriteBatch spritebatch = RenderMaster.GetSingleton().spritebatch;
            float rmFill = RhythmMaster.GetSingleton().GetRockMeterFill();

            if (GameSettings.guiStyle == GUIStyle.UN)
            {
                rmColor = new Color(rmFill < 0.66 ? (byte)255 : (byte)0,
                                    rmFill > 0.33 ? (byte)255 : (byte)0,
                                    0);
                int height = (int)(GameSettings.windowheight * 0.2f);
#if WINDOWS
                spritebatch.Draw(rmUNbg, new Rectangle(0, (GameSettings.windowheight / 2) - (int)(height * 0.75f), (int)(height * 0.5f), (int)(height * 1.5f)), Color.White);
                spritebatch.Draw(rmUNfg, new Rectangle(0, (GameSettings.windowheight / 2), height / 2, height), null, rmColor, MathHelper.Pi - (rmFill * MathHelper.Pi), new Vector2(0, rmUNfg.Height / 2), SpriteEffects.None, 0);
                spritebatch.Draw(rmUNbg, new Rectangle(GameSettings.windowwidth, (GameSettings.windowheight / 2), (int)(height * 0.5f), (int)(height * 1.5f)), null, Color.White, MathHelper.Pi, new Vector2(0, rmUNbg.Height / 2), SpriteEffects.None, 0);
                spritebatch.Draw(rmUNfg, new Rectangle(GameSettings.windowwidth, (GameSettings.windowheight / 2), height / 2, height), null, Color.Wheat, (RhythmMaster.GetSingleton().PercentSong() * MathHelper.Pi), new Vector2(0, rmUNfg.Height / 2), SpriteEffects.None, 0);
#else
                spritebatch.Draw(rmUNbg, new Rectangle((int)(GameSettings.windowwidth*0), (GameSettings.windowheight / 2) - (int)(height * 0.75f), (int)(height * 0.5f), (int)(height*1.5f)), Color.White);
                spritebatch.Draw(rmUNfg, new Rectangle((int)(GameSettings.windowwidth * 0), (GameSettings.windowheight / 2), height / 2, height), null, rmColor, MathHelper.Pi - (rmFill * MathHelper.Pi), new Vector2(0, rmUNfg.Height / 2), SpriteEffects.None, 0);
                spritebatch.Draw(rmUNbg, new Rectangle((int)(GameSettings.windowwidth*1f), (GameSettings.windowheight / 2), (int)(height * 0.5f), (int)(height*1.5f)), null, Color.White, MathHelper.Pi, new Vector2(0,rmUNbg.Height/2),SpriteEffects.None,0);
                spritebatch.Draw(rmUNfg, new Rectangle((int)(GameSettings.windowwidth*1f), (GameSettings.windowheight / 2), height / 2, height), null, Color.Wheat, (song.PercentSong()*MathHelper.Pi),new Vector2(0,rmUNfg.Height/2),SpriteEffects.None,0);
#endif
            }
            else if (GameSettings.guiStyle == GUIStyle.RB)
            {
                rmColor = Color.Black;

                rmColor = new Color(rmFill < 0.66 ? (byte)255 : (byte)128,
                                    rmFill > 0.33 ? (byte)255 : (byte)128,
                                    127);
                float rex;
                if (currentTime > -2.5)
                    rex = rockMeterLoc.X;
                else if (currentTime > -3)
                    rex = -(rockMeterScale.X * 3) + ((((float)currentTime + 3) * 2) * ((rockMeterLoc.X * 3) + rockMeterScale.X));
                else
                    rex = -rockMeterScale.X * 3;
                if (failTime > 0)
                {
                    if (RhythmMaster.GetSingleton().GetPercentBeat() > 0.5)
                    {
                        spritebatch.Draw(Global.texWhite, new Rectangle((int)((rockMeterScale.X * 0.2f) + rex), (int)((rockMeterScale.Y * 0.025f) + rockMeterLoc.Y + (rockMeterScale.Y * (1 - (failTime / 3)) * 0.95f)), (int)(rockMeterScale.X * 0.65f), (int)(rockMeterScale.Y * (failTime / 3) * 0.95f)), new Color((byte)((RhythmMaster.GetSingleton().GetPercentBeat() - 0.5) * 255 + 128), 0, 0));
                    }
                    else
                    {
                        spritebatch.Draw(Global.texWhite, new Rectangle((int)((rockMeterScale.X * 0.2f) + rex), (int)((rockMeterScale.Y * 0.025f) + rockMeterLoc.Y + (rockMeterScale.Y * (1 - (failTime / 3)) * 0.95f)), (int)(rockMeterScale.X * 0.65f), (int)(rockMeterScale.Y * (failTime / 3) * 0.95f)), new Color((byte)((0.5 - RhythmMaster.GetSingleton().GetPercentBeat()) * 255 + 128), 0, 0));
                    }
                }
                else
                {
                    if (rmFill <= 0.33)
                        spritebatch.Draw(Global.texWhite, new Rectangle((int)((rockMeterScale.X * 0.2f) + rex), (int)((rockMeterScale.Y * 0.025f) + rockMeterLoc.Y + (rockMeterScale.Y * (1 - rmFill) * 0.95f)), (int)(rockMeterScale.X * 0.65f), (int)(rockMeterScale.Y * rmFill * 0.95f)), new Color(new Vector4(1f, 0, 0f, 0.8f)));
                    else if (rmFill <= 0.67)
                        spritebatch.Draw(Global.texWhite, new Rectangle((int)((rockMeterScale.X * 0.2f) + rex), (int)((rockMeterScale.Y * 0.025f) + rockMeterLoc.Y + (rockMeterScale.Y * (1 - rmFill) * 0.95f)), (int)(rockMeterScale.X * 0.65f), (int)(rockMeterScale.Y * rmFill * 0.95f)), new Color(new Vector4(1f, 1f, 0f, 0.8f)));
                    else
                        spritebatch.Draw(Global.texWhite, new Rectangle((int)((rockMeterScale.X * 0.2f) + rex), (int)((rockMeterScale.Y * 0.025f) + rockMeterLoc.Y + (rockMeterScale.Y * (1 - rmFill) * 0.95f)), (int)(rockMeterScale.X * 0.65f), (int)(rockMeterScale.Y * rmFill * 0.95f)), new Color(new Vector4(0f, 1f, 0f, 0.8f)));
                }
                spritebatch.Draw(texRockMeterOutline, new Rectangle((int)rex, (int)rockMeterLoc.Y, (int)rockMeterScale.X, (int)rockMeterScale.Y), rmColor);

                byte[] logoslots = new byte[11];

                for (int i = 0; i < logoslots.Length-1; i++)
                    logoslots[i] = (byte)0;
                for (int i = 0; i < 4; i++)
                    if (RhythmMaster.GetSingleton().GetRockMeterFill(i)>=0)
                        logoslots[(int)Math.Min(Math.Round(Math.Max(0, RhythmMaster.GetSingleton().GetRockMeterFill(i)) / (float)(logoslots.Length - 1)), logoslots.Length - 1)] |= (byte)(1<<i);

                for (int i = 0; i < logoslots.Length; i++)
                {
                    int logoscale = 4;
                    if (logoslots[i] != 0)
                    {
                        int numhere = 0;
                        for (int k = 0; k < 4; k++)
                            if ((logoslots[i] & (1<<k)) != 0)
                                numhere++;
                        rmFill = (((logoslots.Length-1) - i) / (float)(logoslots.Length-1));
                        rmColor = new Color((1 - rmFill) < 0.66 ? (byte)255 : (byte)128,
                                            (1 - rmFill) > 0.33 ? (byte)255 : (byte)128,
                                            127);
                        spritebatch.Draw(texRockMeterLogoStem, new Vector2(rex + (rockMeterScale.X / 2), rockMeterLoc.Y + (rockMeterScale.Y * (((logoslots.Length-1) - i) / (float)(logoslots.Length-1)) * 0.92f) + (rockMeterScale.Y * 0.04f)), new Rectangle(0, 0, 256, 256), rmColor, 0, new Vector2(0, 128), rockMeterScale.X / 800f * 3, new SpriteEffects(), 0);
                        int tnum = numhere;
                        for (int k = 3; k >= 0; k--)
                        {

                            if ((logoslots[i] & (i<<k)) != 0)
                            {
                                rmFill = RhythmMaster.GetSingleton().GetRockMeterFill(k) / 100f;
                                rmColor = new Color(rmFill < 0.66 ? (byte)255 : (byte)128,
                                    rmFill > 0.33 ? (byte)255 : (byte)128,
                                    127);
                                switch (k)
                                {
                                    case 0:
                                        spritebatch.Draw(texRockMeterGuitarLogo, new Vector2(rex + (rockMeterScale.X / 2) + (tnum * (210 * (rockMeterScale.X / 800f * logoscale))) - (128 * (rockMeterScale.X / 800f * logoscale)), rockMeterLoc.Y + (rockMeterScale.Y * (((logoslots.Length - 1) - i) / (float)(logoslots.Length - 1)) * 0.92f) + (rockMeterScale.Y * 0.04f)), Global.rect256, rmColor, 0, new Vector2(0, 128), rockMeterScale.X / 800f * logoscale, SpriteEffects.None, 0);
                                        break;
                                    case 1:
                                        spritebatch.Draw(texRockMeterSingerLogo, new Vector2(rex + (rockMeterScale.X / 2) + (tnum * (210 * (rockMeterScale.X / 800f * logoscale))) - (128 * (rockMeterScale.X / 800f * logoscale)), rockMeterLoc.Y + (rockMeterScale.Y * (((logoslots.Length - 1) - i) / (float)(logoslots.Length - 1)) * 0.92f) + (rockMeterScale.Y * 0.04f)), Global.rect256, rmColor, 0, new Vector2(0, 128), rockMeterScale.X / 800f * logoscale, SpriteEffects.None, 0);
                                        break;
                                    case 2:
                                        spritebatch.Draw(texRockMeterDrumLogo, new Vector2(rex + (rockMeterScale.X / 2) + (tnum * (210 * (rockMeterScale.X / 800f * logoscale))) - (128 * (rockMeterScale.X / 800f * logoscale)), rockMeterLoc.Y + (rockMeterScale.Y * (((logoslots.Length - 1) - i) / (float)(logoslots.Length - 1)) * 0.92f) + (rockMeterScale.Y * 0.04f)), Global.rect256, rmColor, 0, new Vector2(0, 128), rockMeterScale.X / 800f * logoscale, SpriteEffects.None, 0);
                                        break;
                                    case 3:
                                        spritebatch.Draw(texRockMeterBassLogo, new Vector2(rex + (rockMeterScale.X / 2) + (tnum * (210 * (rockMeterScale.X / 800f * logoscale))) - (128 * (rockMeterScale.X / 800f * logoscale)), rockMeterLoc.Y + (rockMeterScale.Y * (((logoslots.Length - 1) - i) / (float)(logoslots.Length - 1)) * 0.92f) + (rockMeterScale.Y * 0.04f)), Global.rect256, rmColor, 0, new Vector2(0, 128), rockMeterScale.X / 800f * logoscale, SpriteEffects.None, 0);
                                        break;
                                }
                                tnum--;
                            }
                        }
                    }
                }
            }
        }

        private void DrawScoreStars()
        {
            SpriteBatch spritebatch = RenderMaster.GetSingleton().spritebatch;
            float totalRSA = RhythmMaster.GetSingleton().GetRockstarAmount();
            double currentTime = RhythmMaster.GetSingleton().GetCurrentTime();


            if (GameSettings.guiStyle == GUIStyle.UN)
            {
                float height = (GameSettings.windowheight * 0.15f);
#if WINDOWS
                spritebatch.Draw(rmUNstar, new Vector2(0, GameSettings.windowheight / 2), null, totalRSA < 5 ? Global.FretColors[(int)totalRSA] : Color.White, rockstarDir, new Vector2(rmUNstar.Width / 2, rmUNstar.Height / 2), totalRSA >= 5 ? (height / rmUNstaro.Height) : (totalRSA % 1) * (height / rmUNstaro.Height), SpriteEffects.None, 0);
                spritebatch.Draw(rmUNstaro, new Vector2(0, GameSettings.windowheight / 2), null, Color.White, rockstarDir, new Vector2(rmUNstar.Width / 2, rmUNstar.Height / 2), height / rmUNstaro.Height, SpriteEffects.None, 0);
                {
                    spritebatch.Draw(rmUNstar, new Vector2(GameSettings.windowwidth, GameSettings.windowheight / 2), null, totalRSA < 5 ? Global.FretColors[(int)totalRSA] : Color.White, rockstarDir, new Vector2(rmUNstar.Width / 2, rmUNstar.Height / 2), totalRSA >= 5 ? (height / rmUNstaro.Height) : (totalRSA % 1) * (height / rmUNstaro.Height), SpriteEffects.None, 0);
                    spritebatch.Draw(rmUNstaro, new Vector2(GameSettings.windowwidth, GameSettings.windowheight / 2), null, Color.White, rockstarDir, new Vector2(rmUNstar.Width / 2, rmUNstar.Height / 2), height / rmUNstaro.Height, SpriteEffects.None, 0);
                }
#else
                spritebatch.Draw(rmUNstar, new Vector2(GameSettings.windowwidth*0f, GameSettings.windowheight / 2), null, totalRSA<5?FretColors[(int)totalRSA]:Color.White, rockstarDir, new Vector2(rmUNstar.Width / 2, rmUNstar.Height / 2),(totalRSA%1)*(height/rmUNstaro.Height), SpriteEffects.None, 0);
                spritebatch.Draw(rmUNstaro, new Vector2(GameSettings.windowwidth * 0f, GameSettings.windowheight / 2), null, Color.White, rockstarDir, new Vector2(rmUNstar.Width / 2, rmUNstar.Height / 2), height / rmUNstaro.Height, SpriteEffects.None, 0);
                //if (ct <= 1)
                {
                    spritebatch.Draw(rmUNstar, new Vector2(GameSettings.windowwidth*1f, GameSettings.windowheight / 2), null, FretColors[(int)totalRSA], rockstarDir, new Vector2(rmUNstar.Width / 2, rmUNstar.Height / 2),(totalRSA%1)*(height/rmUNstaro.Height), SpriteEffects.None, 0);
                    spritebatch.Draw(rmUNstaro, new Vector2(GameSettings.windowwidth*1f, GameSettings.windowheight / 2), null, Color.White, rockstarDir, new Vector2(rmUNstar.Width / 2, rmUNstar.Height / 2), height/rmUNstaro.Height, SpriteEffects.None, 0);
                }
#endif
                String scr = RhythmMaster.GetSingleton().GetScore().ToString();
                char[] scrarr = scr.ToCharArray();
                String scr2 = "";
                int count = 0;
                for (int l = scr.Length - 1; l >= 0; l--)
                {
                    scr2 = scrarr[l] + scr2;
                    if (count >= 2)
                    {
                        count = 0;
                        scr2 = "," + scr2;
                    }
                    else
                        count++;
                }
                if (scr2.StartsWith(","))
                    scr2 = scr2.Substring(1);
                spritebatch.DrawString(Global.DefaultFont, scr2, new Vector2((GameSettings.windowwidth / 2) - (Global.DefaultFont.MeasureString(scr2).X / 2), GUIArea.Y), Color.White);
            }
            else if (GameSettings.guiStyle == GUIStyle.RB)
            {
                float xers;
                if (currentTime > -2.5)
                    xers = rockstarLoc.X;
                else if (currentTime> -3)
                    xers = (GameSettings.Resolution.Width - ((((float)currentTime + 3f) * 2) * (GameSettings.Resolution.Width - rockstarLoc.X)));
                else
                    xers = GameSettings.Resolution.Width;
                for (int i = 0; i < 5; i++)
                {
                    float scale = (totalRSA > i + 1) ? 1 : totalRSA - i;
                    if (scale < 0)
                        scale = 0;
                    scale *= 0.9f;

                    spritebatch.Draw(texRockstarRed, new Vector2((int)(xers + (rockstarScale.X / 5 * (i + 0.5f))), (int)(rockstarLoc.Y + (rockstarScale.X / 5 * 0.5f))), new Rectangle(0, 0, 256, 256), new Color(new Vector3(0.1f, 0.1f, 0.1f)), rockstarDir, new Vector2(128, 128), ((rockstarScale.X / 5) / 256), new SpriteEffects(), 0);
                    if (totalRSA >= i + 1)
                        spritebatch.Draw(texRockstarRed, new Vector2((int)(xers + (rockstarScale.X / 5 * (i + 0.5f))), (int)(rockstarLoc.Y + (rockstarScale.X / 5 * 0.5f))), new Rectangle(0, 0, 256, 256), Color.White, rockstarDir, new Vector2(128, 128), ((rockstarScale.X / 5) / 256) * scale, new SpriteEffects(), 0);
                    else
                        spritebatch.Draw(texRockstarRed, new Vector2((int)(xers + (rockstarScale.X / 5 * (i + 0.5f))), (int)(rockstarLoc.Y + (rockstarScale.X / 5 * 0.5f))), new Rectangle(0, 0, 256, 256), Color.Gray, rockstarDir, new Vector2(128, 128), ((rockstarScale.X / 5) / 256) * scale, new SpriteEffects(), 0);
                    spritebatch.Draw(texRockstarCover, new Vector2((int)(xers + (rockstarScale.X / 5 * (i + 0.5f))), (int)(rockstarLoc.Y + (rockstarScale.X / 5 * 0.5f))), new Rectangle(0, 0, 256, 256), Color.White, rockstarDir, new Vector2(128, 128), (rockstarScale.X / 5) / 256, new SpriteEffects(), 0);
                    spritebatch.Draw(texRockstarRing, new Vector2((int)(xers + (rockstarScale.X / 5 * (i + 0.5f))), (int)(rockstarLoc.Y + (rockstarScale.X / 5 * 0.5f))), new Rectangle(0, 0, 256, 256), Color.White, rockstarDir, new Vector2(128, 128), (rockstarScale.X / 5) / 256, new SpriteEffects(), 0);
                    if (totalRSA >= i + 0.25)
                        spritebatch.Draw(texRockstarRingHiLi[10], new Vector2((int)(xers + (rockstarScale.X / 5 * (i + 0.5f))), (int)(rockstarLoc.Y + (rockstarScale.X / 5 * 0.5f))), new Rectangle(0, 0, 128, 128), Color.White, 0, new Vector2(128, 0), ((rockstarScale.X / 5) / 280), new SpriteEffects(), 0);
                    else if (totalRSA > i)
                        spritebatch.Draw(texRockstarRingHiLi[(int)Math.Floor((totalRSA - i) / 0.25f * 11)], new Vector2((int)(xers + (rockstarScale.X / 5 * (i + 0.5f))), (int)(rockstarLoc.Y + (rockstarScale.X / 5 * 0.5f))), new Rectangle(0, 0, 128, 128), Color.White, 0, new Vector2(128, 0), ((rockstarScale.X / 5) / 280), new SpriteEffects(), 0);
                    if (totalRSA >= i + 0.5)
                        spritebatch.Draw(texRockstarRingHiLi[10], new Vector2((int)(xers + (rockstarScale.X / 5 * (i + 0.5f))), (int)(rockstarLoc.Y + (rockstarScale.X / 5 * 0.5f))), new Rectangle(0, 0, 128, 128), Color.White, (float)(Math.PI) / 2, new Vector2(128, 0), ((rockstarScale.X / 5) / 280), new SpriteEffects(), 0);
                    else if (totalRSA > i + 0.25)
                        spritebatch.Draw(texRockstarRingHiLi[(int)Math.Floor((totalRSA - (i + 0.25f)) / 0.25f * 11)], new Vector2((int)(xers + (rockstarScale.X / 5 * (i + 0.5f))), (int)(rockstarLoc.Y + (rockstarScale.X / 5 * 0.5f))), new Rectangle(0, 0, 128, 128), Color.White, (float)(Math.PI) / 2, new Vector2(128, 0), ((rockstarScale.X / 5) / 280), new SpriteEffects(), 0);
                    if (totalRSA >= i + 0.75)
                        spritebatch.Draw(texRockstarRingHiLi[10], new Vector2((int)(xers + (rockstarScale.X / 5 * (i + 0.5f))), (int)(rockstarLoc.Y + (rockstarScale.X / 5 * 0.5f))), new Rectangle(0, 0, 128, 128), Color.White, (float)(Math.PI), new Vector2(128, 0), ((rockstarScale.X / 5) / 280), new SpriteEffects(), 0);
                    else if (totalRSA > i + 0.5)
                        spritebatch.Draw(texRockstarRingHiLi[(int)Math.Floor((totalRSA - (i + 0.5f)) / 0.25f * 11)], new Vector2((int)(xers + (rockstarScale.X / 5 * (i + 0.5f))), (int)(rockstarLoc.Y + (rockstarScale.X / 5 * 0.5f))), new Rectangle(0, 0, 128, 128), Color.White, (float)(Math.PI), new Vector2(128, 0), ((rockstarScale.X / 5) / 280), new SpriteEffects(), 0);
                    if (totalRSA >= i + 1)
                        spritebatch.Draw(texRockstarRingHiLi[10], new Vector2((int)(xers + (rockstarScale.X / 5 * (i + 0.5f))), (int)(rockstarLoc.Y + (rockstarScale.X / 5 * 0.5f))), new Rectangle(0, 0, 128, 128), Color.White, (float)(Math.PI) * 1.5f, new Vector2(128, 0), ((rockstarScale.X / 5) / 280), new SpriteEffects(), 0);
                    else if (totalRSA > i + 0.75f)
                        spritebatch.Draw(texRockstarRingHiLi[(int)Math.Floor((totalRSA - (i + 0.75f)) / 0.25f * 11)], new Vector2((int)(xers + (rockstarScale.X / 5 * (i + 0.5f))), (int)(rockstarLoc.Y + (rockstarScale.X / 5 * 0.5f))), new Rectangle(0, 0, 128, 128), Color.White, (float)(Math.PI) * 1.5f, new Vector2(128, 0), ((rockstarScale.X / 5) / 280), new SpriteEffects(), 0);
                }
                spritebatch.Draw(texScoreBoard, new Rectangle((int)xers, (int)rockstarLoc.Y - (int)(rockstarScale.Y / 2), (int)rockstarScale.X, (int)(rockstarScale.Y / 2)), Color.White);
                String scr = RhythmMaster.GetSingleton().GetScore().ToString();
                char[] scrarr = scr.ToCharArray();
                String scr2 = "";
                int count = 0;
                for (int l = scr.Length - 1; l >= 0; l--)
                {
                    scr2 = scrarr[l] + scr2;
                    if (count >= 2)
                    {
                        count = 0;
                        scr2 = "," + scr2;
                    }
                    else
                        count++;
                }
                spritebatch.DrawString(Global.DefaultFont, scr2, new Vector2(xers + (rockstarScale.X * 0.95f) - Global.DefaultFont.MeasureString(scr2).X, rockstarLoc.Y - (rockstarScale.Y / 2)), Color.White);
            }
        }

        public void Update(GameTime gameTime)
        {
            if (rockstarDir > Math.PI * 2)
                rockstarDir = 0;
            rockstarDir += 1f / gameTime.ElapsedGameTime.Milliseconds;

            if (RhythmMaster.GetSingleton().GetRockstarAmount() > lastStar && lastStar <= 5)
            {
                lastStar++;
                SFXAudioMaster.GetSingleton().Play("starching");
            }
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

        internal void Render()
        {
            DrawRockMeter();
            DrawScoreStars();
        }
    }
}

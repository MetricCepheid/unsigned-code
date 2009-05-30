using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using SongDataIO;
using FVProductions.Utility;

namespace Unsigned
{
    class GameUIMaster
    {
        private SpriteBatch spriteBatch;

        private float rockstarOrientation;

        private Texture2D texRockstarRed, texRockstarRing, texRockstarCover,
                          texRockstarCoverGold;
        private Texture2D[] texRockstarRingHiLi;
        private Texture2D texScoreBoard, texBREScoreBG;
        private Texture2D texRockMeterOutline, texRockMeterFiller;
        private Dictionary<String,Texture2D> texRockMeterInstrumentLogos;
        private Texture2D texRockMeterLogoStem;
        private Texture2D rmUNbg, rmUNfg, rmUNstar, rmUNstaro, rmUNstarbg;

        private bool breStarted;
        private float breScoreAlpha;

        private Board[] boards;
        private SongData songData;

        private bool draw_Failing;
        private float draw_SongTime, draw_RockMeterValue, draw_NumStars,
                      draw_BeatTime;
        private String draw_Score;

        public enum GUIStyle { RB = 0, GH = 2, UN = 1 };

        public GameUIMaster(Board[] boards, SongData songData)
        {
            rockstarOrientation = 0;

            this.boards = boards;
            this.songData = songData;

            draw_Score = "";
        }

        public void Load(ContentManager Content)
        {
            spriteBatch = new SpriteBatch(Global.Graphics.GraphicsDevice);

            texRockstarRed = Content.Load<Texture2D>("textures\\ui\\red");
            texRockstarRing = Content.Load<Texture2D>("textures\\ui\\ring");
            texRockstarCover = Content.Load<Texture2D>("textures\\ui\\starcover");
            texRockstarCoverGold = Content.Load<Texture2D>("textures\\ui\\starcovergold");
            texRockMeterOutline = Content.Load<Texture2D>("textures\\ui\\rockmeter");
            texRockMeterFiller = Content.Load<Texture2D>("textures\\ui\\rockmeterfiller");
            texRockMeterLogoStem = Content.Load<Texture2D>("textures\\ui\\stem");
            {
                texRockMeterInstrumentLogos = new Dictionary<String, Texture2D>();
                String[] files = System.IO.Directory.GetFiles("Content\\textures\\ui\\");
                for (int i = 0; i < files.Length; i++)
                {
                    String name = files[i];
                    if (!name.Contains("logo_"))
                        continue;
                    name = name.Substring(name.IndexOf("logo_"));
                    name = name.Substring(0, name.LastIndexOf('.'));
                    String code = name.Substring(5);
                    texRockMeterInstrumentLogos.Add(code, Content.Load<Texture2D>("textures\\ui\\" + name));
                }
            }
            texScoreBoard = Content.Load<Texture2D>("textures\\ui\\scoreboard");
            rmUNbg = Content.Load<Texture2D>("textures\\ui\\roundmeterbg");
            rmUNfg = Content.Load<Texture2D>("textures\\ui\\roundmeterfg");
            rmUNstar = Content.Load<Texture2D>("textures\\ui\\scorestar");
            rmUNstarbg = Content.Load<Texture2D>("textures\\ui\\scorestarbg");
            rmUNstaro = Content.Load<Texture2D>("textures\\ui\\scorestaro");
            texRockstarRingHiLi = new Texture2D[11];
            for (int i = 0; i < 11; i++)
                texRockstarRingHiLi[i] = Content.Load<Texture2D>("textures\\ui\\border" + (i < 10 ? "0" : "") + i);
            texBREScoreBG = Content.Load<Texture2D>("textures\\Game\\solopercentbg");
        }

        public void Update(SongTime songTime)
        {
            rockstarOrientation = GetBeatTime(songTime) * (1 / 5f) * MathHelper.TwoPi;
            int score = 0;
            draw_SongTime = (float)songTime.TotalSongTime.TotalSeconds;
            draw_Failing = false;
            float rmv = 0;
            draw_NumStars = 0;
            for (int i = 0; i < boards.Length; i++)
            {
                score += boards[i].Score;
                if (boards[i].IsFailing)
                    draw_Failing = true;
                rmv += Math.Max(0, Math.Min(1, boards[i].RockMeterLevel));
                draw_NumStars += boards[i].GetNumStars();
            }
            draw_NumStars /= boards.Length;
            rmv /= boards.Length;
            draw_RockMeterValue += (rmv - draw_RockMeterValue) * (float)(songTime.ElapsedGameTime.TotalSeconds*20);
            draw_Score = "";
            if (score <= 0)
                draw_Score = "0";
            else
            {
                int count = 0;
                for (int i = 1; true; i *= 10)
                {
                    if (i > score)
                        break;
                    int digit = (score / i) % 10;
                    draw_Score = digit + draw_Score;
                    count++;
                    if (count == 3)
                    {
                        count = 0;
                        draw_Score = ',' + draw_Score;
                    }
                }
                if (draw_Score.StartsWith(","))
                    draw_Score = draw_Score.Substring(1);
            }

            if (songData.info.bre.enabled && songTime.TotalSongTime.TotalSeconds >= songData.info.bre.start / 1000f)
                breStarted = true;
            if (breStarted && breScoreAlpha < 1)
            {
                breScoreAlpha += (float)songTime.ElapsedGameTime.TotalSeconds;
                if (breScoreAlpha > 1)
                    breScoreAlpha = 1;
            }
        }

        private void DrawRockMeter()
        {
            if (Configuration.GUIStyle == GUIStyle.UN)
            {
                Color rockMeterColor = new Color(draw_RockMeterValue < 0.66 ? (byte)255 : (byte)0,
                                                 draw_RockMeterValue > 0.33 ? (byte)255 : (byte)0,
                                                 0);
                int height = (int)(Global.SafeArea.Height * 0.2f);
                float percentSong = draw_SongTime/(float)songData.info.length.TotalSeconds;

                Color exStarCol = draw_NumStars >= 6 ? Color.Yellow : Color.White;

                if (draw_NumStars > 3)
                {
                    spriteBatch.Draw(rmUNstarbg, new Vector2(Global.SafeArea.X, (Global.ScreenHeight / 2) - (height * 0.9f)), null, exStarCol, rockstarOrientation, new Vector2(rmUNstarbg.Width / 2, rmUNstarbg.Height / 2), (height * 0.5f) / (float)rmUNstarbg.Height, SpriteEffects.None, 0);
                    spriteBatch.Draw(rmUNstaro, new Vector2(Global.SafeArea.X, (Global.ScreenHeight / 2) - (height * 0.9f)), null, Color.Orange, rockstarOrientation, new Vector2(rmUNstarbg.Width / 2, rmUNstarbg.Height / 2), (height * 0.5f) / (float)rmUNstarbg.Height, SpriteEffects.None, 0);
                    spriteBatch.Draw(texRockstarRing, new Vector2(Global.SafeArea.X, (Global.ScreenHeight / 2) - (height * 0.9f)), null, Color.Orange, 0, new Vector2(texRockstarRing.Width / 2, texRockstarRing.Height / 2), (height * 0.5f) / (float)texRockstarRing.Height, SpriteEffects.None, 0);
                    spriteBatch.Draw(rmUNstarbg, new Vector2(Global.SafeArea.Right, (Global.ScreenHeight / 2) + (height * 0.9f)), null, exStarCol, rockstarOrientation, new Vector2(rmUNstarbg.Width / 2, rmUNstarbg.Height / 2), (height * 0.5f) / (float)rmUNstarbg.Height, SpriteEffects.None, 0);
                    spriteBatch.Draw(rmUNstaro, new Vector2(Global.SafeArea.Right, (Global.ScreenHeight / 2) + (height * 0.9f)), null, Color.Orange, rockstarOrientation, new Vector2(rmUNstarbg.Width / 2, rmUNstarbg.Height / 2), (height * 0.5f) / (float)rmUNstarbg.Height, SpriteEffects.None, 0);
                    spriteBatch.Draw(texRockstarRing, new Vector2(Global.SafeArea.Right, (Global.ScreenHeight / 2) + (height * 0.9f)), null, Color.Orange, 0, new Vector2(texRockstarRing.Width / 2, texRockstarRing.Height / 2), (height * 0.5f) / (float)texRockstarRing.Height, SpriteEffects.None, 0);
                }
                if (draw_NumStars > 4)
                {
                    spriteBatch.Draw(rmUNstarbg, new Vector2(Global.SafeArea.X, (Global.ScreenHeight / 2) + (height * 0.9f)), null, exStarCol, rockstarOrientation, new Vector2(rmUNstarbg.Width / 2, rmUNstarbg.Height / 2), (height * 0.5f) / (float)rmUNstarbg.Height, SpriteEffects.None, 0);
                    spriteBatch.Draw(rmUNstaro, new Vector2(Global.SafeArea.X, (Global.ScreenHeight / 2) + (height * 0.9f)), null, Color.Orange, rockstarOrientation, new Vector2(rmUNstarbg.Width / 2, rmUNstarbg.Height / 2), (height * 0.5f) / (float)rmUNstarbg.Height, SpriteEffects.None, 0);
                    spriteBatch.Draw(texRockstarRing, new Vector2(Global.SafeArea.X, (Global.ScreenHeight / 2) + (height * 0.9f)), null, Color.Orange, 0, new Vector2(texRockstarRing.Width / 2, texRockstarRing.Height / 2), (height * 0.5f) / (float)texRockstarRing.Height, SpriteEffects.None, 0);
                    spriteBatch.Draw(rmUNstarbg, new Vector2(Global.SafeArea.Right, (Global.ScreenHeight / 2) - (height * 0.9f)), null, exStarCol, rockstarOrientation, new Vector2(rmUNstarbg.Width / 2, rmUNstarbg.Height / 2), (height * 0.5f) / (float)rmUNstarbg.Height, SpriteEffects.None, 0);
                    spriteBatch.Draw(rmUNstaro, new Vector2(Global.SafeArea.Right, (Global.ScreenHeight / 2) - (height * 0.9f)), null, Color.Orange, rockstarOrientation, new Vector2(rmUNstarbg.Width / 2, rmUNstarbg.Height / 2), (height * 0.5f) / (float)rmUNstarbg.Height, SpriteEffects.None, 0);
                    spriteBatch.Draw(texRockstarRing, new Vector2(Global.SafeArea.Right, (Global.ScreenHeight / 2) - (height * 0.9f)), null, Color.Orange, 0, new Vector2(texRockstarRing.Width / 2, texRockstarRing.Height / 2), (height * 0.5f) / (float)texRockstarRing.Height, SpriteEffects.None, 0);
                }
                if (draw_NumStars > 1)
                {
                    spriteBatch.Draw(rmUNstarbg, new Vector2(Global.SafeArea.X, (Global.ScreenHeight / 2) - (height * 0.6f)), null, exStarCol, rockstarOrientation, new Vector2(rmUNstarbg.Width / 2, rmUNstarbg.Height / 2), (height * 0.5f) / (float)rmUNstarbg.Height, SpriteEffects.None, 0);
                    spriteBatch.Draw(rmUNstaro, new Vector2(Global.SafeArea.X, (Global.ScreenHeight / 2) - (height * 0.6f)), null, Color.Orange, rockstarOrientation, new Vector2(rmUNstarbg.Width / 2, rmUNstarbg.Height / 2), (height * 0.5f) / (float)rmUNstarbg.Height, SpriteEffects.None, 0);
                    spriteBatch.Draw(texRockstarRing, new Vector2(Global.SafeArea.X, (Global.ScreenHeight / 2) - (height * 0.6f)), null, Color.Orange, 0, new Vector2(texRockstarRing.Width / 2, texRockstarRing.Height / 2), (height * 0.5f) / (float)texRockstarRing.Height, SpriteEffects.None, 0);
                    spriteBatch.Draw(rmUNstarbg, new Vector2(Global.SafeArea.Right, (Global.ScreenHeight / 2) + (height * 0.6f)), null, exStarCol, rockstarOrientation, new Vector2(rmUNstarbg.Width / 2, rmUNstarbg.Height / 2), (height * 0.5f) / (float)rmUNstarbg.Height, SpriteEffects.None, 0);
                    spriteBatch.Draw(rmUNstaro, new Vector2(Global.SafeArea.Right, (Global.ScreenHeight / 2) + (height * 0.6f)), null, Color.Orange, rockstarOrientation, new Vector2(rmUNstarbg.Width / 2, rmUNstarbg.Height / 2), (height * 0.5f) / (float)rmUNstarbg.Height, SpriteEffects.None, 0);
                    spriteBatch.Draw(texRockstarRing, new Vector2(Global.SafeArea.Right, (Global.ScreenHeight / 2) + (height * 0.6f)), null, Color.Orange, 0, new Vector2(texRockstarRing.Width / 2, texRockstarRing.Height / 2), (height * 0.5f) / (float)texRockstarRing.Height, SpriteEffects.None, 0);
                }
                if (draw_NumStars > 2)
                {
                    spriteBatch.Draw(rmUNstarbg, new Vector2(Global.SafeArea.X, (Global.ScreenHeight / 2) + (height * 0.6f)), null, exStarCol, rockstarOrientation, new Vector2(rmUNstarbg.Width / 2, rmUNstarbg.Height / 2), (height * 0.5f) / (float)rmUNstarbg.Height, SpriteEffects.None, 0);
                    spriteBatch.Draw(rmUNstaro, new Vector2(Global.SafeArea.X, (Global.ScreenHeight / 2) + (height * 0.6f)), null, Color.Orange, rockstarOrientation, new Vector2(rmUNstarbg.Width / 2, rmUNstarbg.Height / 2), (height * 0.5f) / (float)rmUNstarbg.Height, SpriteEffects.None, 0);
                    spriteBatch.Draw(texRockstarRing, new Vector2(Global.SafeArea.X, (Global.ScreenHeight / 2) + (height * 0.6f)), null, Color.Orange, 0, new Vector2(texRockstarRing.Width / 2, texRockstarRing.Height / 2), (height * 0.5f) / (float)texRockstarRing.Height, SpriteEffects.None, 0);
                    spriteBatch.Draw(rmUNstarbg, new Vector2(Global.SafeArea.Right, (Global.ScreenHeight / 2) - (height * 0.6f)), null, exStarCol, rockstarOrientation, new Vector2(rmUNstarbg.Width / 2, rmUNstarbg.Height / 2), (height * 0.5f) / (float)rmUNstarbg.Height, SpriteEffects.None, 0);
                    spriteBatch.Draw(rmUNstaro, new Vector2(Global.SafeArea.Right, (Global.ScreenHeight / 2) - (height * 0.6f)), null, Color.Orange, rockstarOrientation, new Vector2(rmUNstarbg.Width / 2, rmUNstarbg.Height / 2), (height * 0.5f) / (float)rmUNstarbg.Height, SpriteEffects.None, 0);
                    spriteBatch.Draw(texRockstarRing, new Vector2(Global.SafeArea.Right, (Global.ScreenHeight / 2) - (height * 0.6f)), null, Color.Orange, 0, new Vector2(texRockstarRing.Width / 2, texRockstarRing.Height / 2), (height * 0.5f) / (float)texRockstarRing.Height, SpriteEffects.None, 0);
                }

                spriteBatch.Draw(rmUNbg, new Rectangle(Global.SafeArea.X, (Global.ScreenHeight / 2) - (int)(height * 0.75f), (int)(height * 0.5f), (int)(height * 1.5f)), Color.Orange);
                spriteBatch.Draw(rmUNfg, new Rectangle(Global.SafeArea.X, (Global.ScreenHeight / 2), height / 2, height), null, rockMeterColor, MathHelper.Pi - (draw_RockMeterValue * MathHelper.Pi), new Vector2(0, rmUNfg.Height / 2), SpriteEffects.None, 0);
                spriteBatch.Draw(rmUNbg, new Rectangle(Global.SafeArea.Right, (Global.ScreenHeight / 2), (int)(height * 0.5f), (int)(height * 1.5f)), null, Color.Orange, MathHelper.Pi, new Vector2(0, rmUNbg.Height / 2), SpriteEffects.None, 0);
                spriteBatch.Draw(rmUNfg, new Rectangle(Global.SafeArea.Right, (Global.ScreenHeight / 2), height / 2, height), null, Color.Orange, (percentSong * MathHelper.Pi), new Vector2(0, rmUNfg.Height / 2), SpriteEffects.None, 0);
                
            }
            else if (Configuration.GUIStyle == GUIStyle.RB)
            {
                Color rockMeterColor = new Color(draw_RockMeterValue < 0.66 ? (byte)255 : (byte)0,
                                                 draw_RockMeterValue > 0.33 ? (byte)255 : (byte)0,
                                                 0);
                Rectangle rockMeterRect;
                {
                    float width = Global.SafeArea.Width * 0.05f;
                    float height = Global.SafeArea.Height * 0.8f;
                    float rex;
                    if (draw_SongTime > -2.5)
                        rex = Global.SafeArea.Left;
                    else if (draw_SongTime > -3)
                        rex = -width + ((((float)draw_SongTime + 3) * 2) * (Global.SafeArea.X + width));
                    else
                        rex = -width;
                    rockMeterRect = new Rectangle((int)rex, (Global.ScreenHeight / 2) - (int)(height / 2), (int)width, (int)height);
                }
                Rectangle rockMeterInnerRect = new Rectangle(rockMeterRect.X,
                                                             (int)(rockMeterRect.Y+(rockMeterRect.Height*0.023f)),
                                                             rockMeterRect.Width,
                                                             (int)(rockMeterRect.Height-(rockMeterRect.Height*0.046f)));
                int newY = rockMeterInnerRect.Y + (int)((1 - draw_RockMeterValue) * rockMeterInnerRect.Height);
                rockMeterInnerRect = new Rectangle(rockMeterInnerRect.X, newY, rockMeterInnerRect.Width, rockMeterInnerRect.Bottom-newY);
                Rectangle rockMeterInnerSource = new Rectangle(0, (int)((1 - draw_RockMeterValue) * texRockMeterFiller.Height), texRockMeterFiller.Width, (int)(texRockMeterFiller.Height * draw_RockMeterValue));
                if (draw_Failing)
                {
                    if (draw_BeatTime > 0.5f)
                    {
                        spriteBatch.Draw(texRockMeterFiller, rockMeterInnerRect, rockMeterInnerSource, new Color((byte)((draw_BeatTime - 0.5) * 255 + 128), 0, 0));
                    }
                    else
                    {
                        spriteBatch.Draw(texRockMeterFiller, rockMeterInnerRect, rockMeterInnerSource, new Color((byte)((0.5 - draw_BeatTime) * 255 + 128), 0, 0));
                    }
                }
                else
                {
                    Color col;
                    if (draw_RockMeterValue <= 0.33)
                        col = new Color(1f, 0, 0f, 0.8f);
                    else if (draw_RockMeterValue <= 0.67)
                        col = new Color(1f, 1f, 0f, 0.8f);
                    else
                        col = new Color(0f, 1f, 0f, 0.8f);

                    spriteBatch.Draw(texRockMeterFiller, rockMeterInnerRect, rockMeterInnerSource, col);
                }
                spriteBatch.Draw(texRockMeterOutline, rockMeterRect, Color.Orange);

                for (int i = 0; i < boards.Length; i++)
                {
                    float rmv = Math.Max(0,Math.Min(1,boards[i].RockMeterLevel));
                    int width = (int)(Global.SafeArea.Width*0.04f)/2*2;
                    int top = (int)(rockMeterRect.Y+(rockMeterRect.Height*0.025f));
                    int height = (int)(rockMeterRect.Height-(rockMeterRect.Height*0.05f));
                    int y = (int)(top + (height * (1 - rmv)));
                    Rectangle rect = new Rectangle(rockMeterRect.X+(int)(rockMeterRect.Width*0.84f), y-(width/2), width, width);
                    if(texRockMeterInstrumentLogos.ContainsKey(boards[i].InstrumentCode))
                    {
                        spriteBatch.Draw(texRockMeterLogoStem, rect, Color.Orange);
                        rect = new Rectangle(rect.X+(rect.Width/2),rect.Y,rect.Width,rect.Height);
                        spriteBatch.Draw(texRockMeterInstrumentLogos[boards[i].InstrumentCode], rect, Color.Orange);
                    }
                }
            }
        }

        private void DrawScoreStars()
        {
            if (Configuration.GUIStyle == GUIStyle.UN)
            {
                float height = (Global.SafeArea.Height * 0.15f);
                Color exStarCol = draw_NumStars >= 6 ? Color.Yellow : Color.White;
                spriteBatch.Draw(rmUNstar, new Vector2(Global.SafeArea.X, Global.ScreenHeight / 2), null, exStarCol, rockstarOrientation, new Vector2(rmUNstar.Width / 2, rmUNstar.Height / 2), draw_NumStars >= 5 ? (height / rmUNstaro.Height) : (draw_NumStars % 1) * (height / rmUNstaro.Height), SpriteEffects.None, 0);
                spriteBatch.Draw(rmUNstaro, new Vector2(Global.SafeArea.X, Global.ScreenHeight / 2), null, Color.Orange, rockstarOrientation, new Vector2(rmUNstar.Width / 2, rmUNstar.Height / 2), height / rmUNstaro.Height, SpriteEffects.None, 0);
                {
                    spriteBatch.Draw(rmUNstar, new Vector2(Global.SafeArea.Right, Global.ScreenHeight / 2), null, exStarCol, rockstarOrientation, new Vector2(rmUNstar.Width / 2, rmUNstar.Height / 2), draw_NumStars >= 5 ? (height / rmUNstaro.Height) : (draw_NumStars % 1) * (height / rmUNstaro.Height), SpriteEffects.None, 0);
                    spriteBatch.Draw(rmUNstaro, new Vector2(Global.SafeArea.Right, Global.ScreenHeight / 2), null, Color.Orange, rockstarOrientation, new Vector2(rmUNstar.Width / 2, rmUNstar.Height / 2), height / rmUNstaro.Height, SpriteEffects.None, 0);
                }
                spriteBatch.DrawString(Global.DefaultFont, draw_Score, new Vector2((Global.ScreenWidth / 2) - (Global.DefaultFont.MeasureString(draw_Score).X / 2), Global.SafeArea.Y), Color.Orange);
            }
            else if (Configuration.GUIStyle == GUIStyle.RB)
            {
                Rectangle scoreRect;
                {
                    float width = Global.SafeArea.Width * 0.3f;
                    float height = Global.SafeArea.Height * 0.075f;
                    float xers;
                    if (draw_SongTime > -2.5)
                        xers = Global.SafeArea.Right-width;
                    else if (draw_SongTime > -3)
                        xers = (Global.SafeArea.Right - ((((float)draw_SongTime + 3f) * 2) * (Global.ScreenWidth - (Global.SafeArea.Width - width))));
                    else
                        xers = Global.ScreenWidth;
                    scoreRect = new Rectangle((int)xers, Global.SafeArea.Y, (int)width, (int)height);
                }
                for (int i = 0; i < 5; i++)
                {
                    float scale = (draw_NumStars > i + 1) ? 1 : draw_NumStars - i;
                    if (scale < 0)
                        scale = 0;
                    scale *= 0.9f;
                    Rectangle thisStarRect = new Rectangle(scoreRect.X + (scoreRect.Width / 5 * i), scoreRect.Bottom + (int)(scoreRect.Height * 0.05f), scoreRect.Width / 5, scoreRect.Width / 5);
                    Vector2 thisStarCenter = new Vector2(thisStarRect.X + (thisStarRect.Width / 2), thisStarRect.Y + (thisStarRect.Height / 2));
                    if(draw_NumStars>=6)
                        spriteBatch.Draw(texRockstarRed, thisStarCenter, null, Color.Yellow, rockstarOrientation, new Vector2(texRockstarRed.Width / 2, texRockstarRed.Height / 2), (thisStarRect.Width / (float)texRockstarRed.Width) * scale, SpriteEffects.None, 0);
                    else if (draw_NumStars >= i + 1)
                        spriteBatch.Draw(texRockstarRed, thisStarCenter, null, Color.Orange, rockstarOrientation, new Vector2(texRockstarRed.Width / 2, texRockstarRed.Height / 2), (thisStarRect.Width / (float)texRockstarRed.Width) * scale, SpriteEffects.None, 0);
                    else
                        spriteBatch.Draw(texRockstarRed, thisStarCenter, null, Color.DarkOrange, rockstarOrientation, new Vector2(texRockstarRed.Width / 2, texRockstarRed.Height / 2), (thisStarRect.Width / (float)texRockstarRed.Width), SpriteEffects.None, 0);
                    spriteBatch.Draw(draw_NumStars>=6 ? texRockstarCoverGold : texRockstarCover, thisStarCenter, null, Color.White, rockstarOrientation, new Vector2(texRockstarCover.Width / 2, texRockstarCover.Height / 2), (thisStarRect.Width / (float)texRockstarCover.Width), SpriteEffects.None, 0);
                    spriteBatch.Draw(texRockstarRing, thisStarCenter, null, Color.White, 0, new Vector2(texRockstarRing.Width / 2, texRockstarRing.Height / 2), (thisStarRect.Width / (float)texRockstarRing.Width), SpriteEffects.None, 0);
                    for (int k = 0; k < 36; k++)
                    {
                        if (draw_NumStars > i + (k / 36f))
                            spriteBatch.Draw(texRockstarRingHiLi[0], thisStarCenter, null, Color.Orange, -(k / 36f) * MathHelper.TwoPi, new Vector2(texRockstarRingHiLi[0].Width / 2, texRockstarRingHiLi[0].Height / 2), (thisStarRect.Width / (float)texRockstarRingHiLi[0].Width), SpriteEffects.None, 0);
                        else
                            break;
                    }
                }
                spriteBatch.Draw(texScoreBoard, scoreRect, Color.Orange);

                {
                    Vector2 measStr = Global.DefaultFont.MeasureString(draw_Score);
                    float scale = (scoreRect.Height * 0.95f) / measStr.Y;
                    spriteBatch.DrawString(Global.DefaultFont, draw_Score, new Vector2(scoreRect.Right - (scoreRect.Width*0.05f), scoreRect.Y+(scoreRect.Height/2)), Color.White, 0, new Vector2(measStr.X,measStr.Y/2), scale, SpriteEffects.None, 0);
                }
            }
        }

        internal void Render()
        {
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.None);
            DrawRockMeter();
            DrawScoreStars();

            if (breScoreAlpha > 0)
            {
                float scale = -(((breScoreAlpha * 1.2f) - 1) * ((breScoreAlpha * 1.2f) - 1)) + 1;
                spriteBatch.Draw(texBREScoreBG, new Vector2(Global.ScreenWidth / 2, Global.ScreenHeight * 0.3f), null, new Color(Global.UnsignedOrange, scale), 0, new Vector2(texBREScoreBG.Width / 2, 0), new Vector2(Global.ScreenHeight / 1200f * scale, Global.ScreenHeight / 1800f * scale), SpriteEffects.None, 0);
                int totalBREScore = 0;
                for (int i = 0; i < boards.Length; i++)
                    totalBREScore += boards[i].BREScore;
                {
                    Vector2 meas = Global.DefaultFont.MeasureString("" + totalBREScore);
                    spriteBatch.DrawString(Global.DefaultFont, "" + totalBREScore, new Vector2(Global.ScreenWidth / 2, Global.ScreenHeight * 0.31f), new Color(1f, 1f, 1f, scale), 0, new Vector2(meas.X / 2, 0), Global.ScreenHeight / 600f * scale, SpriteEffects.None, 0);
                }
                {
                    Vector2 meas = Global.DefaultFont.MeasureString("Ending Score");
                    spriteBatch.DrawString(Global.DefaultFont, "Ending Score", new Vector2(Global.ScreenWidth / 2, Global.ScreenHeight * 0.3f), new Color(1f, 1f, 1f, scale), 0, new Vector2(meas.X / 2, meas.Y), Global.ScreenHeight / 600f * scale, SpriteEffects.None, 0);
                }
            }

            spriteBatch.End();
        }

        private float GetBeatTime(SongTime songTime)
        {
            float currentTime = (float)songTime.TotalSongTime.TotalSeconds;
            for (int i = 0; i < songData.info.barlines.Length - 1; i++)
            {
                if (currentTime * 1000 < songData.info.barlines[i].time)
                    continue;
                if (currentTime * 1000 > songData.info.barlines[i + 1].time)
                    continue;
                float start = songData.info.barlines[i].time / 1000f;
                float end = songData.info.barlines[i + 1].time / 1000f;
                float val = (currentTime - start) / (end - start);
                val *= songData.info.barlines[i].numBeats;
                val %= 1.0f;
                return val;
            }
            return 0;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UnsignedPeripheralPlugins;
using SongDataIO;
using FVProductions.Utility;

namespace Unsigned
{
    class ResultsScreen : BaseState
    {
        private ContentManager Content;

        private SpriteBatch spriteBatch;

        private bool failed;
        private Results[] totalresults;
        private Results addedResults;
        private float dialogscroll;
        private Texture2D failbg, resultsScroller;
        private Texture2D coolbg1, coolbg2, texStar;

        private SessionInfo sesInfo;

        public ResultsScreen(SessionInfo sesInfo, bool failed)
        {
            this.sesInfo = sesInfo;
            this.failed = failed;
        }

        public override void Load()
        {
            Content = new ContentManager(Global.Services);
            Content.RootDirectory = "Content";

            spriteBatch = new SpriteBatch(Global.Graphics.GraphicsDevice);

            coolbg1 = Content.Load<Texture2D>("textures\\Results\\coolbg1");
            coolbg2 = Content.Load<Texture2D>("textures\\Results\\coolbg2");
            resultsScroller = Content.Load<Texture2D>("textures\\Results\\resultscroller");
            failbg = Content.Load<Texture2D>("textures\\Results\\faildialog");
            texStar = Content.Load<Texture2D>("textures\\UI\\scorestarbg");
        }

        public override void Unload()
        {
            Content.Unload();
        }

        public override void Update(GameTime gameTime)
        {
            if (!failed)
            {
                try
                {
                    dialogscroll += (float)gameTime.ElapsedGameTime.TotalSeconds / 10f;
                    while (dialogscroll > 1)
                        dialogscroll -= 1;
                    bool green = false;//what should red be used for?
                    Peripheral[] peripherals =  PeripheralManager.Singleton.GetPeripherals();
                    for (int i = 0; i < peripherals.Length; i++)
                        if (peripherals[i].IsConnected())
                        {
                            if (peripherals[i].WasPressed(PeripheralButton.CONFIRM))
                                green = true;
                        }
                    if (green)
                    { 
                        String songCodeName = sesInfo.songFileName.Substring(sesInfo.songFileName.LastIndexOf('\\')+1);
                        songCodeName = songCodeName.Substring(0,songCodeName.IndexOf('.'));
                        String[] instrCodeNames = new String[totalresults.Length];
                        String[] instrOwners = new String[totalresults.Length];
                        int[] streaks = new int[totalresults.Length];
                        for(int i=0;i<instrCodeNames.Length;i++)
                        {
                            instrCodeNames[i] = totalresults[i].instr.CodeName;
                            instrOwners[i] = totalresults[i].OwnerName;
                            streaks[i] = totalresults[i].streak;
                        }
                        SaveDataManager.Singleton.AddEntry(songCodeName, instrCodeNames, instrOwners, addedResults.score, (int)addedResults.numStars, streaks);
                        UnsignedGame.Singleton.SwitchState(new SongSelectScreen(sesInfo));
                    }
                }
                catch(Exception e)
                {
                    Debug.Error("Problem in ResultsScreen.Update[1]",e);
                    UnsignedGame.Singleton.Exit();
                    return;
                }
            }
            else
            {

                try
                {
                    dialogscroll += (float)gameTime.ElapsedGameTime.TotalSeconds / 10f;
                    while (dialogscroll > 1)
                        dialogscroll -= 1;

                    bool green = false, red = false;//what should red be used for?
                    Peripheral[] peripherals = PeripheralManager.Singleton.GetPeripherals();
                    for (int i = 0; i < peripherals.Length; i++)
                        if (peripherals[i].IsConnected())
                        {
                            if (peripherals[i].WasPressed(PeripheralButton.CONFIRM))
                                green = true;
                            if (peripherals[i].WasPressed(PeripheralButton.BACK))
                                red = true;
                        }
                    if (red || green)
                    { 
                        UnsignedGame.Singleton.SwitchState(new MainMenuScreen()); 
                    }

                }
                catch(Exception e)
                {
                    Debug.Error("Problem in ResultsScreen.Update[2]",e);
                    UnsignedGame.Singleton.Exit();
                    return;
                }
            }
        }

        public override void Render(GameTime gameTime)
        {
            if (!failed)
            {

                Global.Graphics.GraphicsDevice.Clear(Color.Black);
                spriteBatch.Begin();
                spriteBatch.Draw(coolbg1, new Rectangle((int)(0.1 * Global.ScreenWidth), (int)(0.1 * Global.ScreenHeight), (int)(0.8 * Global.ScreenWidth), (int)(0.8 * Global.ScreenHeight)), new Color(Global.UnsignedOrange, 30));
                spriteBatch.Draw(failbg, new Rectangle(0, 0, Global.ScreenWidth, Global.ScreenHeight), Global.UnsignedOrange);
                {//stars
                    int startx = (int)((Global.ScreenWidth * 0.25f) - (Global.ScreenWidth * 0.025f * (int)addedResults.numStars));
                    for (int i = 0; i < (int)addedResults.numStars; i++)
                        spriteBatch.Draw(texStar, new Rectangle(startx + (int)(i * Global.ScreenWidth * 0.05f), (int)(Global.ScreenHeight * 0.25f), (int)(Global.ScreenWidth * 0.05f), (int)(Global.ScreenWidth * 0.05f)), Color.White);
                }

                spriteBatch.DrawString(Global.DefaultFont, "" + addedResults.score, new Vector2(Global.ScreenWidth * 0.75f, Global.ScreenHeight * 0.3f), Color.White, 0, Global.DefaultFont.MeasureString("" + addedResults.score) * 0.5f, Global.ScreenHeight / 800f, SpriteEffects.None, 0);

                int numRS = 4;
                for (int i = -1; i < numRS; i++)
                    spriteBatch.Draw(resultsScroller, new Rectangle((int)((dialogscroll * (Global.ScreenWidth / (float)numRS)) + (i * (Global.ScreenWidth / (float)numRS))), (int)((128f / 768) * Global.ScreenHeight), (int)(Global.ScreenWidth / (float)numRS + 1), (int)((64f / 768) * Global.ScreenHeight)), Global.UnsignedYellow);
                for (int i = 0; i < numRS + 1; i++)
                    spriteBatch.Draw(resultsScroller, new Rectangle((int)((-dialogscroll * (Global.ScreenWidth / (float)numRS)) + (i * (Global.ScreenWidth / (float)numRS))), Global.ScreenHeight - (int)((192f / 768) * Global.ScreenHeight), (int)(Global.ScreenWidth / (float)numRS + 1), (int)((64f / 768) * Global.ScreenHeight)), Global.UnsignedYellow);
                spriteBatch.DrawString(Global.DefaultFont, "Song Passed", new Vector2((Global.ScreenWidth / 2), Global.ScreenHeight * 0.3f), Global.UnsignedYellow, 0, Global.DefaultFont.MeasureString("Song Passed") * 0.5f, Global.ScreenHeight/600f, SpriteEffects.None,0);

                if (totalresults != null && totalresults.Length != 0)
                    for (int i = 0; i < totalresults.Length; i++)
                    {
                        //calculate percent
                        float percent = totalresults[i].hitNotes / (float)totalresults[i].totalNotes;
                        int pc = (int)(Math.Round(percent * 100) + 0.5f);
                        //fix rounding errors
                        if (pc > 100)
                            pc = 100;
                        if (pc < 0)
                            pc = 0;
                        if (totalresults[i].hitNotes < totalresults[i].totalNotes && pc >= 100)
                            pc = 99;
                        //draw
                        String str1 = totalresults[i].instr.FullName,
                               str2 = "" + pc + "%",
                               str3 = "Rock Power Phrases: " + totalresults[i].hitSPPH + "/" + totalresults[i].totalSPPH,
                               str4 = "" + totalresults[i].streak + " Note Streak";
                        spriteBatch.DrawString(Global.DefaultFont,
                                               str1,
                                               new Vector2((Global.ScreenWidth * 0.2f), (Global.ScreenHeight * (0.35f + (0.07f * i)))), Color.White, 0, Vector2.Zero, Global.ScreenHeight / 800f, SpriteEffects.None, 0);
                        spriteBatch.DrawString(Global.DefaultFont,
                                               str2,
                                               new Vector2((Global.ScreenWidth * 0.5f), (Global.ScreenHeight * (0.35f + (0.07f * i)))), Color.White, 0, new Vector2(Global.DefaultFont.MeasureString(str2).X, 0), Global.ScreenHeight / 800f, SpriteEffects.None, 0);
                        spriteBatch.DrawString(Global.DefaultFont,
                                                str3,
                                               new Vector2((Global.ScreenWidth * 0.8f), (Global.ScreenHeight * (0.35f + 0.03f + (0.07f * i)))), Color.White, 0, new Vector2(Global.DefaultFont.MeasureString(str3).X, 0), Global.ScreenHeight / 900f, SpriteEffects.None, 0);
                        spriteBatch.DrawString(Global.DefaultFont,
                                                str4,
                                               new Vector2((Global.ScreenWidth * 0.8f), (Global.ScreenHeight * (0.35f + (0.07f * i)))), Color.White, 0, new Vector2(Global.DefaultFont.MeasureString(str4).X, 0), Global.ScreenHeight / 800f, SpriteEffects.None, 0);
                    }
                spriteBatch.End();
            }
            else
            {
                Global.Graphics.GraphicsDevice.Clear(Color.Black);
                spriteBatch.Begin();
                spriteBatch.Draw(coolbg2, new Rectangle((int)(0.1 * Global.ScreenWidth), (int)(0.1 * Global.ScreenHeight), (int)(0.8 * Global.ScreenWidth), (int)(0.8 * Global.ScreenHeight)), new Color(30, 30, 30));
                spriteBatch.Draw(failbg, new Rectangle(0, 0, Global.ScreenWidth, Global.ScreenHeight), Color.White);
                int numRS = 4;
                for (int i = -1; i < numRS; i++)
                    spriteBatch.Draw(resultsScroller, new Rectangle((int)((dialogscroll * (Global.ScreenWidth / (float)numRS)) + (i * (Global.ScreenWidth / (float)numRS))), (int)((128f / 768) * Global.ScreenHeight), (int)(Global.ScreenWidth / (float)numRS + 1), (int)((64f / 768) * Global.ScreenHeight)), Color.White);
                for (int i = 0; i < numRS + 1; i++)
                    spriteBatch.Draw(resultsScroller, new Rectangle((int)((-dialogscroll * (Global.ScreenWidth / (float)numRS)) + (i * (Global.ScreenWidth / (float)numRS))), Global.ScreenHeight - (int)((192f / 768) * Global.ScreenHeight), (int)(Global.ScreenWidth / (float)numRS + 1), (int)((64f / 768) * Global.ScreenHeight)), Color.White);
                if (totalresults.Length > 0)
                {
                    String percent = "" + (int)(addedResults.percentSong * 100 + 0.5f) + "%";
                    spriteBatch.DrawString(Global.DefaultFont, "Failed", new Vector2((Global.ScreenWidth / 2) - (Global.DefaultFont.MeasureString("Failed").X / 2), Global.ScreenHeight * 0.3f), Color.Red);
                    spriteBatch.DrawString(Global.DefaultFont, percent, new Vector2((Global.ScreenWidth / 2) - (Global.DefaultFont.MeasureString(percent).X / 2), Global.ScreenHeight * 0.4f), Color.Red);
                }
                /*
                spriteBatch.Draw(GameUIMaster.Singleton.texButtonGreen, new Rectangle((int)(0.01f * Global.ScreenWidth), (int)(0.90f * Global.ScreenHeight), (int)(0.09f * Global.ScreenHeight), (int)(0.09f * Global.ScreenHeight)), Color.White);
                spriteBatch.DrawString(Global.DefaultFont, "Retry", new Vector2((0.01f * Global.ScreenWidth) + (0.10f * Global.ScreenHeight), (0.90f * Global.ScreenHeight) + (0.09f * Global.ScreenHeight) - (Global.DefaultFont.MeasureString("Retry").Y)), Color.White);
                spriteBatch.Draw(GameUIMaster.Singleton.texButtonRed, new Rectangle((int)(0.99f * Global.ScreenWidth) - (int)(0.09f * Global.ScreenHeight), (int)(0.90f * Global.ScreenHeight), (int)(0.09f * Global.ScreenHeight), (int)(0.09f * Global.ScreenHeight)), Color.White);
                spriteBatch.DrawString(Global.DefaultFont, "Main Menu", new Vector2((0.99f * Global.ScreenWidth) - (0.10f * Global.ScreenHeight) - Global.DefaultFont.MeasureString("Main Menu").X, (0.90f * Global.ScreenHeight) + (0.09f * Global.ScreenHeight) - (Global.DefaultFont.MeasureString("Main Menu").Y)), Color.White);
                */
                spriteBatch.End();
            }
        }

        public void SetResults(Board[] boards)
        {
            if (totalresults==null || totalresults.Length == 0)
            {
                int count = 0;
                for (int i = 0; i < boards.Length; i++)
                    if (boards[i]!=null)
                        count++;
                totalresults = new Results[count];
                int r = 0;
                for (int i = 0; i < boards.Length; i++)
                    if(boards[i]!=null)
                    {
                        totalresults[r] = boards[i].GetResults();
                        r++;
                    }
                //song.pause();
                addedResults = new Results();
                for (int i = 0; i < count; i++)
                {
                    addedResults.hitNotes += totalresults[i].hitNotes;
                    addedResults.hitSPPH += totalresults[i].hitSPPH;
                    addedResults.missedNotes += totalresults[i].missedNotes;
                    addedResults.missedSPPH += totalresults[i].missedSPPH;
                    addedResults.totalNotes += totalresults[i].totalNotes;
                    addedResults.totalSPPH += totalresults[i].totalSPPH;
                    addedResults.numStars += totalresults[i].numStars;
                    addedResults.score += totalresults[i].score;
                }
                addedResults.numStars /= totalresults.Length;
            }
        }

        internal void SetPercentSong(float ps)
        {
            addedResults.percentSong = ps;
        }
    }
}

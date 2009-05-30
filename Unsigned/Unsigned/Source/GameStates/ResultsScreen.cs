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
        private Texture2D coolbg1, coolbg2;

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
                int numRS = 4;
                for (int i = -1; i < numRS; i++)
                    spriteBatch.Draw(resultsScroller, new Rectangle((int)((dialogscroll * (Global.ScreenWidth / (float)numRS)) + (i * (Global.ScreenWidth / (float)numRS))), (int)((128f / 768) * Global.ScreenHeight), (int)(Global.ScreenWidth / (float)numRS + 1), (int)((64f / 768) * Global.ScreenHeight)), Global.UnsignedYellow);
                for (int i = 0; i < numRS + 1; i++)
                    spriteBatch.Draw(resultsScroller, new Rectangle((int)((-dialogscroll * (Global.ScreenWidth / (float)numRS)) + (i * (Global.ScreenWidth / (float)numRS))), Global.ScreenHeight - (int)((192f / 768) * Global.ScreenHeight), (int)(Global.ScreenWidth / (float)numRS + 1), (int)((64f / 768) * Global.ScreenHeight)), Global.UnsignedYellow);
                spriteBatch.DrawString(Global.DefaultFont, "Song Passed", new Vector2((Global.ScreenWidth / 2) - (Global.DefaultFont.MeasureString("Song Passed").X / 2), Global.ScreenHeight * 0.3f), Global.UnsignedYellow);
                if (totalresults!=null && totalresults.Length != 0)
                    for (int i = 0; i < totalresults.Length; i++)
                    {
                        //calculate percent
                        float percent = totalresults[i].hitNotes/(float)totalresults[i].totalNotes;
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
                               str2 = "" + pc +"%",
                               str3 = "Rock Power Phrases: " + totalresults[i].hitSPPH + "/" + totalresults[i].totalSPPH,
                               str4 = "Streak: " + totalresults[i].streak;
                        spriteBatch.DrawString(Global.DefaultFont,
                                               str1,
                                               new Vector2((Global.ScreenWidth *0.2f), (Global.ScreenHeight * (0.4f + (0.1f * i)))), Color.White, 0, Vector2.Zero, Global.ScreenHeight/800f, SpriteEffects.None, 0);
                        spriteBatch.DrawString(Global.DefaultFont,
                                               str2,
                                               new Vector2((Global.ScreenWidth * 0.8f), (Global.ScreenHeight * (0.4f + (0.1f * i)))), Color.White, 0, new Vector2(Global.DefaultFont.MeasureString(str2).X,0), Global.ScreenHeight / 800f, SpriteEffects.None, 0);
                        spriteBatch.DrawString(Global.DefaultFont,
                                                str3,
                                               new Vector2((Global.ScreenWidth * 0.8f), (Global.ScreenHeight * (0.4f + 0.04f + (0.1f * i)))), Color.White, 0, new Vector2(Global.DefaultFont.MeasureString(str3).X,0),Global.ScreenHeight/800f, SpriteEffects.None, 0);
                        spriteBatch.DrawString(Global.DefaultFont,
                                                str4,
                                               new Vector2((Global.ScreenWidth * 0.8f), (Global.ScreenHeight * (0.4f + 0.08f + (0.1f * i)))), Color.White, 0, new Vector2(Global.DefaultFont.MeasureString(str4).X, 0), Global.ScreenHeight / 800f, SpriteEffects.None, 0);

                    }
                /*
                spriteBatch.Draw(GameUIMaster.Singleton.texButtonGreen, new Rectangle((int)(0.01f * Global.ScreenWidth), (int)(0.90f * Global.ScreenHeight), (int)(0.09f * Global.ScreenHeight), (int)(0.09f * Global.ScreenHeight)), Color.White);
                spriteBatch.DrawString(Global.DefaultFont, "Continue", new Vector2((0.01f * Global.ScreenWidth) + (0.10f * Global.ScreenHeight), (0.90f * Global.ScreenHeight) + (0.09f * Global.ScreenHeight) - (Global.DefaultFont.MeasureString("Continue").Y)), Color.White);
                */
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
                    String percent = "" + (int)(totalresults[4].percentSong * 100 + 0.5f) + "%";
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
                }
            }
        }

        public void SetResults(Results[] results)
        {
            if (totalresults == null || totalresults.Length == 0)
            {
                totalresults = results;
                //song.pause();
                addedResults = new Results();
                for (int i = 0; i < results.Length; i++)
                {
                    addedResults.hitNotes += totalresults[i].hitNotes;
                    addedResults.hitSPPH += totalresults[i].hitSPPH;
                    addedResults.missedNotes += totalresults[i].missedNotes;
                    addedResults.missedSPPH += totalresults[i].missedSPPH;
                    addedResults.totalNotes += totalresults[i].totalNotes;
                    addedResults.totalSPPH += totalresults[i].totalSPPH;
                }
            }
        }
    }
}

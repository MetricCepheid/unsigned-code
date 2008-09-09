using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UnsignedPeripheralPlugins;

namespace Unsigned
{
    struct Results
    {
        public int hitNotes, missedNotes;
        public int totalNotes, totalSPPH;//temp
        public int hitSPPH, missedSPPH;
        public float percentSong;
        public Instrument instr;
    }

    class ResultsScreen : BaseState
    {
        private bool failed;
        private Results[] totalresults;
        private Results addedResults;
        private float dialogscroll;
        private Texture2D failbg, resultsScroller;
        private Texture2D coolbg1, coolbg2;

        public ResultsScreen()
        {

        }

        public override void Load(ContentManager content)
        {
            coolbg1 = content.Load<Texture2D>("graphics\\coolbg1");
            coolbg2 = content.Load<Texture2D>("graphics\\coolbg2");
            resultsScroller = content.Load<Texture2D>("graphics\\resultscroller");
            failbg = content.Load<Texture2D>("graphics\\faildialog");
            
        }

        public override void Unload()
        {
            
        }

        public override void Update(GameTime gameTime)
        {
            if (!failed)
            {
#if !DEBUG
                try
                {
#endif
                

                bool green = false, red = false;//what should red be used for?
                Peripheral[] peripherals =  PeripheralManager.GetSingleton().GetPeripherals();
                for (int i = 0; i < peripherals.Length; i++)
                    if (peripherals[i].IsConnected())
                    {
                        if (peripherals[i].WasPressed(PeripheralButton.CONFIRM))
                            green = true;
                        if (peripherals[i].WasPressed(PeripheralButton.BACK))
                            red = true;
                    }
                if (green)
                { 
                    totalresults = null;
                    UnsignedGame.GetSingleton().PushState(new MainMenuScreen());
                }

#if !DEBUG
                }
                catch(Exception e)
                {
#if WINDOWS
                    System.Windows.Forms.MessageBox.Show("Problem in Update/R/Pt0\n"+e.Message+"\n"+e.StackTrace);
#endif
                    Exit();
                    return;
                }
#endif
            }
            else
            {

#if !DEBUG
                try
                {
#endif

                dialogscroll += gameTime.ElapsedGameTime.Milliseconds / 10000f;
                while (dialogscroll > 1)
                    dialogscroll -= 1;

                bool green = false, red = false;//what should red be used for?
                Peripheral[] peripherals = PeripheralManager.GetSingleton().GetPeripherals();
                for (int i = 0; i < peripherals.Length; i++)
                    if (peripherals[i].IsConnected())
                    {
                        if (peripherals[i].WasPressed(PeripheralButton.CONFIRM))
                            green = true;
                        if (peripherals[i].WasPressed(PeripheralButton.BACK))
                            red = true;
                    }
                if (red)
                { totalresults = new Results[0]; UnsignedGame.GetSingleton().PushState(new MainMenuScreen()); }
                if (green)
                { totalresults = new Results[0]; UnsignedGame.GetSingleton().RestartSong(); }

#if !DEBUG
                }
                catch(Exception e)
                {
#if WINDOWS
                    System.Windows.Forms.MessageBox.Show("Problem in Update/F/Pt0\n"+e.Message+"\n"+e.StackTrace);
#endif
                    Exit();
                    return;
                }
#endif
            }
        }

        public override void Render(GameTime gameTime)
        {
            RenderMaster rm = RenderMaster.GetSingleton();
            if (!failed)
            {

                rm.graphics.GraphicsDevice.Clear(Color.Black);
                rm.spritebatch.Begin();
                rm.spritebatch.Draw(coolbg1, new Rectangle((int)(0.1 * GameSettings.windowwidth), (int)(0.1 * GameSettings.windowheight), (int)(0.8 * GameSettings.windowwidth), (int)(0.8 * GameSettings.windowheight)), new Color(30, 30, 30));
                rm.spritebatch.Draw(failbg, new Rectangle(0, 0, GameSettings.windowwidth, GameSettings.windowheight), Color.White);
                int numRS = 4;
                for (int i = -1; i < numRS; i++)
                    rm.spritebatch.Draw(resultsScroller, new Rectangle((int)((dialogscroll * (GameSettings.windowwidth / (float)numRS)) + (i * (GameSettings.windowwidth / (float)numRS))), (int)((128f / 768) * GameSettings.windowheight), (int)(GameSettings.windowwidth / (float)numRS + 1), (int)((64f / 768) * GameSettings.windowheight)), Color.White);
                for (int i = 0; i < numRS + 1; i++)
                    rm.spritebatch.Draw(resultsScroller, new Rectangle((int)((-dialogscroll * (GameSettings.windowwidth / (float)numRS)) + (i * (GameSettings.windowwidth / (float)numRS))), GameSettings.windowheight - (int)((192f / 768) * GameSettings.windowheight), (int)(GameSettings.windowwidth / (float)numRS + 1), (int)((64f / 768) * GameSettings.windowheight)), Color.White);
                rm.spritebatch.DrawString(Global.DefaultFont, "Song Passed", new Vector2((GameSettings.windowwidth / 2) - (Global.DefaultFont.MeasureString("Song Passed").X / 2), GameSettings.windowheight * 0.3f), Color.Green);
                if (totalresults!=null && totalresults.Length != 0)
                    for (int i = 0; i < totalresults.Length; i++)
                    {
                            String str1 = totalresults[i].instr.FullName + " - Notes: " + totalresults[i].hitNotes + "/" + totalresults[i].totalNotes,
                                   str2 = totalresults[i].instr.FullName + " - Rock Power Phrases: " + totalresults[i].hitSPPH + "/" + totalresults[i].totalSPPH;
                            rm.spritebatch.DrawString(Global.DefaultFont,
                                                   str1,
                                                   new Vector2((GameSettings.windowwidth / 2) - (Global.DefaultFont.MeasureString(str1).X / 2), (GameSettings.windowheight * 0.4f) + (40 * i)), Color.Wheat);
                            rm.spritebatch.DrawString(Global.DefaultFont,
                                                    str2,
                                                   new Vector2((GameSettings.windowwidth / 2) - (Global.DefaultFont.MeasureString(str2).X / 2), (GameSettings.windowheight * 0.4f) + 20 + (40 * i)), Color.Wheat);

                    }
                rm.spritebatch.Draw(GameUIMaster.GetSingleton().texButtonGreen, new Rectangle((int)(0.01f * GameSettings.windowwidth), (int)(0.90f * GameSettings.windowheight), (int)(0.09f * GameSettings.windowheight), (int)(0.09f * GameSettings.windowheight)), Color.White);
                rm.spritebatch.DrawString(Global.DefaultFont, "Continue", new Vector2((0.01f * GameSettings.windowwidth) + (0.10f * GameSettings.windowheight), (0.90f * GameSettings.windowheight) + (0.09f * GameSettings.windowheight) - (Global.DefaultFont.MeasureString("Continue").Y)), Color.White);
                if (Global.DemoMode)
                {
                    rm.spritebatch.DrawString(Global.BigFont, "Demo Mode", new Vector2((GameSettings.windowwidth / 2) - (Global.BigFont.MeasureString("Demo Mode").X / 2), GameSettings.windowheight * 0.15f), new Color(255, 0, 0, 64));
                    rm.spritebatch.DrawString(Global.BigFont, "Demo Mode", new Vector2((GameSettings.windowwidth / 2) - (Global.BigFont.MeasureString("Demo Mode").X / 2), GameSettings.windowheight * 0.4f), new Color(255, 0, 0, 64));
                    rm.spritebatch.DrawString(Global.BigFont, "Demo Mode", new Vector2((GameSettings.windowwidth / 2) - (Global.BigFont.MeasureString("Demo Mode").X / 2), GameSettings.windowheight * 0.65f), new Color(255, 0, 0, 64));
                }
                rm.spritebatch.End();
            }
            else
            {
                rm.graphics.GraphicsDevice.Clear(Color.Black);
                rm.spritebatch.Begin();
                rm.spritebatch.Draw(coolbg2, new Rectangle((int)(0.1 * GameSettings.windowwidth), (int)(0.1 * GameSettings.windowheight), (int)(0.8 * GameSettings.windowwidth), (int)(0.8 * GameSettings.windowheight)), new Color(30, 30, 30));
                rm.spritebatch.Draw(failbg, new Rectangle(0, 0, GameSettings.windowwidth, GameSettings.windowheight), Color.White);
                int numRS = 4;
                for (int i = -1; i < numRS; i++)
                    rm.spritebatch.Draw(resultsScroller, new Rectangle((int)((dialogscroll * (GameSettings.windowwidth / (float)numRS)) + (i * (GameSettings.windowwidth / (float)numRS))), (int)((128f / 768) * GameSettings.windowheight), (int)(GameSettings.windowwidth / (float)numRS + 1), (int)((64f / 768) * GameSettings.windowheight)), Color.White);
                for (int i = 0; i < numRS + 1; i++)
                    rm.spritebatch.Draw(resultsScroller, new Rectangle((int)((-dialogscroll * (GameSettings.windowwidth / (float)numRS)) + (i * (GameSettings.windowwidth / (float)numRS))), GameSettings.windowheight - (int)((192f / 768) * GameSettings.windowheight), (int)(GameSettings.windowwidth / (float)numRS + 1), (int)((64f / 768) * GameSettings.windowheight)), Color.White);
                if (totalresults.Length > 0)
                {
                    String percent = "" + (int)(totalresults[4].percentSong * 100 + 0.5f) + "%";
                    rm.spritebatch.DrawString(Global.DefaultFont, "Failed", new Vector2((GameSettings.windowwidth / 2) - (Global.DefaultFont.MeasureString("Failed").X / 2), GameSettings.windowheight * 0.3f), Color.Red);
                    rm.spritebatch.DrawString(Global.DefaultFont, percent, new Vector2((GameSettings.windowwidth / 2) - (Global.DefaultFont.MeasureString(percent).X / 2), GameSettings.windowheight * 0.4f), Color.Red);
                }
                rm.spritebatch.Draw(GameUIMaster.GetSingleton().texButtonGreen, new Rectangle((int)(0.01f * GameSettings.windowwidth), (int)(0.90f * GameSettings.windowheight), (int)(0.09f * GameSettings.windowheight), (int)(0.09f * GameSettings.windowheight)), Color.White);
                rm.spritebatch.DrawString(Global.DefaultFont, "Retry", new Vector2((0.01f * GameSettings.windowwidth) + (0.10f * GameSettings.windowheight), (0.90f * GameSettings.windowheight) + (0.09f * GameSettings.windowheight) - (Global.DefaultFont.MeasureString("Retry").Y)), Color.White);
                rm.spritebatch.Draw(GameUIMaster.GetSingleton().texButtonRed, new Rectangle((int)(0.99f * GameSettings.windowwidth) - (int)(0.09f * GameSettings.windowheight), (int)(0.90f * GameSettings.windowheight), (int)(0.09f * GameSettings.windowheight), (int)(0.09f * GameSettings.windowheight)), Color.White);
                rm.spritebatch.DrawString(Global.DefaultFont, "Main Menu", new Vector2((0.99f * GameSettings.windowwidth) - (0.10f * GameSettings.windowheight) - Global.DefaultFont.MeasureString("Main Menu").X, (0.90f * GameSettings.windowheight) + (0.09f * GameSettings.windowheight) - (Global.DefaultFont.MeasureString("Main Menu").Y)), Color.White);
                rm.spritebatch.End();
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
    }
}

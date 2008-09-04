using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Unsigned
{
    class RhythmMaster
    {
        private static RhythmMaster SINGLETON_RhythmMaster = null;

        private static float[] rockMeterLevel;

        private int[] selectedInstruments;
        private Instrument[] instrumentTypes;
        private Board[] boards;
        private float failTime;
        private double CurrentTime, lastChange;

        private int started = 0;

        private RhythmMaster()
        {
            selectedInstruments = new int[4];
            selectedInstruments[0] = -1;
            selectedInstruments[1] = -1;
            selectedInstruments[2] = -1;
            selectedInstruments[3] = -1;
            rockMeterLevel = new float[4];
            rockMeterLevel[0] = 80;
            rockMeterLevel[1] = 80;
            rockMeterLevel[2] = 80;
            rockMeterLevel[3] = 80;
            instrumentTypes = new Instrument[4];
            instrumentTypes[0].CodeName = "LGT";
            instrumentTypes[0].FullName = "Lead Guitar";
            instrumentTypes[0].Dimensions = Instrument.BoardDimensions.THREE_DIMENSIONAL;
            instrumentTypes[0].NumTracks = 5;
            instrumentTypes[0].RPEnableType = Instrument.RockPowerEnableTypes.SELECT;
            instrumentTypes[0].ContainsHeldNotes = true;
            instrumentTypes[0].CanWhammy = true;
            instrumentTypes[0].CanHOPO = true;
            instrumentTypes[0].HasSolos = true;
            instrumentTypes[0].NeedsStrum = true;
            instrumentTypes[0].MaxMultiplier = 4;
            instrumentTypes[0].BumpNotes = 0;
            instrumentTypes[1].CodeName = "LVX";
            instrumentTypes[1].FullName = "Lead Vocals";
            instrumentTypes[1].Dimensions = Instrument.BoardDimensions.TWO_DIMENSIONAL;
            instrumentTypes[1].NumTracks = 12 * 3;// 3 octaves
            instrumentTypes[1].RPEnableType = Instrument.RockPowerEnableTypes.FILL;
            instrumentTypes[1].ContainsHeldNotes = true;
            instrumentTypes[1].CanWhammy = false;
            instrumentTypes[1].CanHOPO = false;
            instrumentTypes[1].HasSolos = false;
            instrumentTypes[1].NeedsStrum = false;
            instrumentTypes[1].MaxMultiplier = 4;
            instrumentTypes[1].BumpNotes = 0;
            instrumentTypes[2].CodeName = "SET";
            instrumentTypes[2].FullName = "Drum Set";
            instrumentTypes[2].Dimensions = Instrument.BoardDimensions.THREE_DIMENSIONAL;
            instrumentTypes[2].NumTracks = 4;
            instrumentTypes[2].RPEnableType = Instrument.RockPowerEnableTypes.FILL;
            instrumentTypes[2].ContainsHeldNotes = false;
            instrumentTypes[2].CanWhammy = false;
            instrumentTypes[2].CanHOPO = false;
            instrumentTypes[2].HasSolos = false;
            instrumentTypes[2].NeedsStrum = false;
            instrumentTypes[2].MaxMultiplier = 4;
            instrumentTypes[2].BumpNotes = ((ulong)1) << 4;
            instrumentTypes[3].CodeName = "BAS";
            instrumentTypes[3].FullName = "Bass Guitar";
            instrumentTypes[3].Dimensions = Instrument.BoardDimensions.THREE_DIMENSIONAL;
            instrumentTypes[3].NumTracks = 5;
            instrumentTypes[3].RPEnableType = Instrument.RockPowerEnableTypes.SELECT;
            instrumentTypes[3].ContainsHeldNotes = true;
            instrumentTypes[3].CanWhammy = true;
            instrumentTypes[3].CanHOPO = true;
            instrumentTypes[3].HasSolos = false;
            instrumentTypes[3].NeedsStrum = true;
            instrumentTypes[3].MaxMultiplier = 6;
            instrumentTypes[3].BumpNotes = 0;

        }

        public Instrument GetInstrumentType(int index)
        {
            if (IsInstrumentAvailable(index))
                return instrumentTypes[index];
            else
                return null;
        }

        public static void CreateSingleton()
        {
            if (SINGLETON_RhythmMaster == null)
                SINGLETON_RhythmMaster = new RhythmMaster();
            else
                throw new InvalidOperationException("Singleton has already been initialized");
        }

        public static void DestroySingleton()
        {
            if (SINGLETON_RhythmMaster != null)
                SINGLETON_RhythmMaster = null;
            else
                throw new InvalidOperationException("Singleton has already been destroyed");
        }

        public static RhythmMaster GetSingleton()
        {
            return SINGLETON_RhythmMaster;
        }

        public void Update(GameTime gameTime)
        {
            //start song TEMPORARY CODE? probably not...
            if (started == 0)
            {
                CurrentTime = 5;
                started = 1;
                return;
            }
            else if (started == 1)
            {
                if (CurrentTime - gameTime.ElapsedGameTime.TotalSeconds < 0)
                {
                    CurrentTime = 0;
                    SongAudioMaster.GetSingleton().Play();
                    started = 2;
                }
                else
                    CurrentTime -= gameTime.ElapsedGameTime.TotalSeconds;
                return;
            }
            double songt = SongAudioMaster.GetSingleton().GetTime();
            if (lastChange != songt && Math.Abs(CurrentTime - songt) > 0.005 && Math.Abs(CurrentTime - songt) < 5.000)
            {
                CurrentTime = songt;
                lastChange = songt;
            }
            else if (lastChange != songt)
                lastChange = songt;
            CurrentTime += gameTime.ElapsedGameTime.TotalSeconds;

            bool allFail = true;
            int anyFail = 0;
            for (int i = 0; i < 4; i++)
            {
                if (!boards[i].IsFailing)
                    allFail = false;
                if (boards[i].IsFailing)
                    anyFail++;
            }
            if (allFail)
                UnsignedGame.GetSingleton().PushState(new ResultsScreen());// TODO: FAILURE
            if (anyFail > 0)
                failTime -= (gameTime.ElapsedGameTime.Milliseconds / 20000f) * anyFail;
            else
                failTime = 10;
            if (failTime <= 0)
                UnsignedGame.GetSingleton().PushState(new ResultsScreen());// TODO: FAILURE
        }

        public void Render()
        {
            Matrix fling;
            if (started == 1)
            {
                if (RhythmMaster.GetSingleton().GetCurrentTime() < 3)
                    fling = Matrix.CreateRotationX(Board.rotate);
                else if (RhythmMaster.GetSingleton().GetCurrentTime() < 4)
                    fling = Matrix.CreateRotationX(((-((float)RhythmMaster.GetSingleton().GetCurrentTime() - 4f)) * Board.rotate * 4) - (Board.rotate * 3));
                else
                    fling = Matrix.CreateRotationX((float)Math.PI / 2);
            }
            else
                fling = Matrix.CreateRotationX(Board.rotate);

            for (int i = 0; i < boards.Length; i++)
            {
                
            }
        }

        /// <summary>
        /// Returns whether or not a player is using this instrument slot (0-3)
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool IsInstrumentAvailable(int index)
        {
            return selectedInstruments[index] >= 0;
        }

        public float GetRockstarAmount()
        {
            float total = 0, count = 0;
            for (int i = 0; i < 4; i++)
                if (selectedInstruments[i]>=0)
                {
                    int adddiff;
                    if (boards[i].GetDifficulty() == Global.D_EASY)
                        adddiff = 1;
                    else if (boards[i].GetDifficulty() == Global.D_MEDIUM)
                        adddiff = 2;
                    else if (boards[i].GetDifficulty() == Global.D_HARD)
                        adddiff = 3;
                    else
                        adddiff = 4;
                    total += boards[i].GetStars() * adddiff;
                    count += adddiff;
                }
            return total / count;
        }

        private void StarPowerAction(int i)
        {
            if (boards[i].IsFailing)
                return;
            if (boards[i].GetSPAmount() < 0.5)
                return;
            if (failTime > 0)
            {
                for (int k = 0; k < boards.Length; k++)
                    if (selectedInstruments[i]>=0 && boards[k].IsFailing)
                    {
                        boards[i].Save();
                        boards[i].EatHalfSP();
                        return;
                    }
            }
            boards[i].ActivateStarPower();
        }

        public float GetRockMeterFill()
        {
            int ct = 0;
            float add = 0;
            for (int i = 0; i < 4; i++)
                if (selectedInstruments[i]>=0)
                {
                    if (rockMeterLevel[i] > 100)
                        rockMeterLevel[i] = 100;
                    //if (TEST_SONG)
                    //    rockMeterLevel[i] = 99f;
                    ct++;
                    add += rockMeterLevel[i];
                }
            return (add / ct) / 100f;
        }

        public float GetRockMeterFill(int which)
        {
            if (selectedInstruments[which] < 0)
                return -1000;
            if (rockMeterLevel[which] > 100)
                rockMeterLevel[which] = 100;
            return rockMeterLevel[which];
        }

        public int GetScore()
        {
            int total = 0;
            for (int i = 0; i < 4; i++)
                if (boards[i] != null)
                {
                    total += boards[i].GetScore();
                }
            return total;
        }

        internal void Restart()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        internal float PercentSong()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        internal double GetCurrentTime()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        internal float GetFailTime()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        internal double GetPercentBeat()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        internal SongData GetSongData()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        internal void Burn(ulong note, Board board)
        {
            for (int i = 0; i < 4; i++)
                if (boards[i] == board)
                    ParticleMaster.GetSingleton().AddSparks(note, i, board, 1);
        }

        internal void AddSparks(ulong note, Board board)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        internal float GetMeasureProgress()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        internal int GetBPMeasure()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        internal void DrawBoardRenders(SpriteBatch spritebatch)
        {
            for (int i = 0; i < 4; i++)
                if(boards[i]!=null)
                    spritebatch.Draw(boards[i].GetRender(), new Rectangle(boards[i].GetXOffset(), 0, GameSettings.windowwidth, GameSettings.windowheight), Color.White);
        }

        private void ProcessInput(GameTime gameTime, long currenttime)
        {
            byte er = 0;
            for (int i = 0; i < 4; i++)
                if (boards[i] != null)
                {
                    boards[i].Update(gameTime);

                    /*if (i == 0 || i == 3)
                    {
                        byte pressed = 0;
                        bool up = false, down = false;
                        if (contInput[i] < 4)
                        {
                            if (controllers[contInput[i]].Buttons.A == ButtonState.Pressed)
                                pressed |= bits[boards[i].IsLefty() ? 4 : 0];
                            if (controllers[contInput[i]].Buttons.B == ButtonState.Pressed)
                                pressed |= bits[boards[i].IsLefty() ? 3 : 1];
                            if (controllers[contInput[i]].Buttons.Y == ButtonState.Pressed)
                                pressed |= bits[2];
                            if (controllers[contInput[i]].Buttons.X == ButtonState.Pressed)
                                pressed |= bits[boards[i].IsLefty() ? 1 : 3];
                            if (controllers[contInput[i]].Buttons.LeftShoulder == ButtonState.Pressed)
                                pressed |= bits[boards[i].IsLefty() ? 0 : 4];
                            if (controllers[contInput[i]].DPad.Down == ButtonState.Pressed)
                                down = true;
                            if (controllers[contInput[i]].DPad.Up == ButtonState.Pressed)
                                up = true;
                            if (controllers[contInput[i]].ThumbSticks.Right.Y > 0.95f || controllers[contInput[i]].Buttons.Back == ButtonState.Pressed)
                                StarPowerAction(i);
                            if (controllers[contInput[i]].Buttons.Start == ButtonState.Pressed)
                            { TogglePause(); pauseSelectOwner = i; }
                            boards[i].Whammy(controllers[contInput[i]].ThumbSticks.Right.X, currenttime, gameTime, song.GetBeatLength());
                        }
                        else if (contInput[i] == 4)
                        {
                            KeyboardState kbst = Keyboard.GetState();
                            if (kbst.IsKeyDown(Keys.G))
                                pressed |= bits[boards[i].IsLefty() ? 0 : 4];
                            if (kbst.IsKeyDown(Keys.F))
                                pressed |= bits[boards[i].IsLefty() ? 1 : 3];
                            if (kbst.IsKeyDown(Keys.D))
                                pressed |= bits[2];
                            if (kbst.IsKeyDown(Keys.S))
                                pressed |= bits[boards[i].IsLefty() ? 3 : 1];
                            if (kbst.IsKeyDown(Keys.A))
                                pressed |= bits[boards[i].IsLefty() ? 4 : 0];
                            if (kbst.IsKeyDown(Keys.Down))
                                down = true;
                            if (kbst.IsKeyDown(Keys.Up))
                                up = true;
                            if (kbst.IsKeyDown(Keys.RightShift) || kbst.IsKeyDown(Keys.NumPad0) || kbst.IsKeyDown(Keys.Insert) || kbst.IsKeyDown(Keys.D0))
                                StarPowerAction(i);
                            if (kbst.IsKeyDown(Keys.Escape) || kbst.IsKeyDown(Keys.Back))
                            { TogglePause(); pauseSelectOwner = i; }
                            boards[i].Whammy(kbst.IsKeyDown(Keys.Left) ? 1.0f : -1.0f, currenttime, gameTime, song.GetBeatLength());
                        }
                        if ((pressed & 1) == 0)
                        {
                            if (i == 0)
                                guitarGreen = false;
                            else //if i==3
                                bassGreen = false;
                        }
                        if (!IsPaused)
                        {
                            if (i == 0)
                            {
                                if (down && guitarStrum != 1)
                                { boards[i].Strum(0, currenttime, this, i); guitarStrum = 1; }
                                else if (up && guitarStrum != 2)
                                { boards[i].Strum(0, currenttime, this, i); guitarStrum = 2; }
                                else if (!up && !down)
                                    guitarStrum = 0;
                            }
                            else
                            {
                                if (down && bassStrum != 1)
                                { boards[i].Strum(0, currenttime, this, i); bassStrum = 1; }
                                else if (up && bassStrum != 2)
                                { boards[i].Strum(0, currenttime, this, i); bassStrum = 2; }
                                else if (!up && !down)
                                    bassStrum = 0;
                            }
                            er = boards[i].Update(gameTime, currenttime, this, i, pressed);
                        }
                        else if (i == pauseSelectOwner)
                        {
                            if (i == 0)
                            {
                                if (down && guitarStrum != 1)
                                { pauseSelected++; guitarStrum = 1; }
                                else if (up && guitarStrum != 2)
                                { pauseSelected--; guitarStrum = 2; }
                                else if (!up && !down)
                                    guitarStrum = 0;
                            }
                            else
                            {
                                if (down && bassStrum != 1)
                                { pauseSelected++; bassStrum = 1; }
                                else if (up && bassStrum != 2)
                                { pauseSelected--; bassStrum = 2; }
                                else if (!up && !down)
                                    bassStrum = 0;
                            }
                            if (pauseSelected < 0)
                                pauseSelected = 0;
                            else if (pauseSelected >= pauseTextDisp.Length)
                                pauseSelected = pauseTextDisp.Length - 1;
                            if ((pressed & 1) != 0 && !guitarGreen && i == 0)
                            { ApplyPauseOption(); guitarGreen = true; }
                            if ((pressed & 1) != 0 && !bassGreen && i == 3)
                            { ApplyPauseOption(); bassGreen = true; }
                        }
                    }
                    else if (i == 2)
                    {
                        byte pressed = 0;
                        if (contInput[2] < 4)
                        {
                            if (controllers[contInput[i]].Buttons.A == ButtonState.Pressed)
                                pressed |= 1;
                            if (controllers[contInput[i]].Buttons.B == ButtonState.Pressed)
                                pressed |= 2;
                            if (controllers[contInput[i]].Buttons.Y == ButtonState.Pressed)
                                pressed |= 4;
                            if (controllers[contInput[i]].Buttons.X == ButtonState.Pressed)
                                pressed |= 8;
                            if (controllers[contInput[i]].Buttons.LeftShoulder == ButtonState.Pressed)
                                pressed |= 16;
                            if (controllers[contInput[i]].Buttons.Start == ButtonState.Pressed)
                            { TogglePause(); pauseSelectOwner = i; }
                        }
                        else if (contInput[2] == 4)
                        {
                            KeyboardState kbst = Keyboard.GetState();
                            if (kbst.IsKeyDown(Keys.F))
                                pressed |= 1;
                            if (kbst.IsKeyDown(Keys.A))
                                pressed |= 2;
                            if (kbst.IsKeyDown(Keys.S))
                                pressed |= 4;
                            if (kbst.IsKeyDown(Keys.D))
                                pressed |= 8;
                            if (kbst.IsKeyDown(Keys.Space))
                                pressed |= 16;
                            if (kbst.IsKeyDown(Keys.Escape) || kbst.IsKeyDown(Keys.Back))
                            { TogglePause(); pauseSelectOwner = i; }
                        }
                        if (pressed != 0)
                        {
                            byte e = boards[2].Bang(pressed, currenttime, this);
                            if (e > 0)
                            {
                                if ((e & bits[7]) != 0)
                                {
                                    AddSparks(e, 2);
                                    if ((e & bits[0]) != 0)
                                        audioSoundBank.PlayCue("crash");
                                    if ((e & bits[1]) != 0)
                                        audioSoundBank.PlayCue("snare");
                                    if ((e & bits[2]) != 0)
                                        audioSoundBank.PlayCue("tom1");
                                    if ((e & bits[3]) != 0)
                                        audioSoundBank.PlayCue("tom2");
                                    if ((e & bits[4]) != 0)
                                        audioSoundBank.PlayCue("bass");
                                }
                                else
                                    AddShards(e, 2);
                            }
                        }
                        if ((pressed & 2) == 0)
                            drumsGreen = false;

                        if (!IsPaused)
                            er = boards[i].Update(gameTime, currenttime, this, i, pressed);
                        else if (i == pauseSelectOwner)
                        {
                            bool up = (pressed & 4) != 0, down = (pressed & 8) != 0;
                            if (down && drumsStrum != 1)
                            { pauseSelected++; drumsStrum = 1; }
                            else if (up && drumsStrum != 2)
                            { pauseSelected--; drumsStrum = 2; }
                            else if (!up && !down)
                                drumsStrum = 0;
                            if (pauseSelected < 0)
                                pauseSelected = 0;
                            else if (pauseSelected >= pauseTextDisp.Length)
                                pauseSelected = pauseTextDisp.Length - 1;

                            if ((pressed & 1) != 0 && !drumsGreen)
                            { ApplyPauseOption(); drumsGreen = true; }
                        }
                    }
                    else if (!IsPaused)
                        er = boards[i].Update(gameTime, currenttime, this, i, 0);
                    if (er > 0)
                    {
                        if ((er & bits[7]) != 0)
                            AddSparks(er, i);
                        else
                            AddShards(er, i);
                    }*/
                }
        }

        internal void DrawSongInfo()
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}

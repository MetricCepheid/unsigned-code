using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using UnsignedPeripheralPlugins;
using SongDataIO;

namespace Unsigned
{
    class PlayerConfigNugget
    {
        public Peripheral[] peripherals;
        public int[] instruments;
        public int[] characterIndices;
        public byte[] difficulties;

        public String songFileName;

        public PlayerConfigNugget()
        {
            peripherals = new Peripheral[4];
            instruments = new int[4];
            characterIndices = new int[4];
            difficulties = new byte[4];
            songFileName = "";
        }
    }

    class RhythmMaster
    {
        private static RhythmMaster SINGLETON_RhythmMaster = null;

        private Board[] boards;
        private float failTime;
        private double CurrentTime, lastChange;

        private SongData songData;

        private int started = 0;

        private RhythmMaster()
        {
        }

        public void Initialize(PlayerConfigNugget info, SongData song, ContentManager content)
        {
            boards = new Board[4];
            Board.Load(content);
            songData = song;

            int num3D = 0;
            for(int i=0;i<4;i++)
                if(info.peripherals[i]!=null)
                    if(InstrumentMaster.GetSingleton().GetInstrument(info.instruments[i]).Dimensions==Instrument.BoardDimensions.THREE_DIMENSIONAL)
                        num3D++;
            int count3D = 0;
            for(int i=0;i<4;i++)
                if(info.peripherals[i]!=null)
                {
                    boards[i] = new Board(InstrumentMaster.GetSingleton().GetInstrument(info.instruments[i]),
                        InstrumentMaster.GetSingleton().GetInstrument(info.instruments[i]).Dimensions == Instrument.BoardDimensions.THREE_DIMENSIONAL ? (int)(((((count3D * 2) + 1) / (num3D * 2.0f))-0.5f) * GameSettings.windowwidth) : 0,
                        song, info.difficulties[i]);
                    boards[i].LoadInstance(content);
                    boards[i].Peripheral = info.peripherals[i];
                    if (InstrumentMaster.GetSingleton().GetInstrument(info.instruments[i]).Dimensions == Instrument.BoardDimensions.THREE_DIMENSIONAL)
                        count3D++;
                }
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

            for (int i = 0; i < 4; i++)
            {
                if (boards[i] != null)
                {
                    if ((boards[i].GetBoardType().RPEnableType & Instrument.RockPowerEnableTypes.SELECT) != 0 && boards[i].Peripheral.IsPressed(PeripheralButton.SELECT))
                        StarPowerAction(i);
                    boards[i].Update(gameTime);
                }
            }

            bool allFail = true;
            int anyFail = 0;
            for (int i = 0; i < 4; i++)
            {
                if (boards[i] == null)
                    continue;
                if (!boards[i].IsFailing)
                    allFail = false;
                else
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
                if (CurrentTime < 3)
                    fling = Matrix.CreateRotationX(Board.Rotate);
                else if (CurrentTime < 4)
                    fling = Matrix.CreateRotationX(((-((float)CurrentTime - 4f)) * Board.Rotate * 4) - (Board.Rotate * 3));
                else
                    fling = Matrix.CreateRotationX((float)Math.PI / 2);
            }
            else
                fling = Matrix.CreateRotationX(Board.Rotate);

            for (int i = 0; i < boards.Length; i++)
            if(boards[i]!=null)
            {
                boards[i].Draw(fling);
            }
        }

        /// <summary>
        /// Returns whether or not a player is using this instrument slot (0-3)
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool IsInstrumentAvailable(int index)
        {
            return boards[index]!=null;
        }

        public float GetRockstarAmount()
        {
            float total = 0, count = 0;
            for (int i = 0; i < 4; i++)
                if (boards[i]!=null)
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
                    if (boards[k]!=null && boards[k].IsFailing)
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
                if (boards[i]!=null)
                {
                    float rockMeterLevel = boards[i].GetRockMeterLevel();
                    //if (TEST_SONG)
                    //    rockMeterLevel[i] = 99f;
                    ct++;
                    add += rockMeterLevel;
                }
            return (add / ct) / 100f;
        }

        public float GetRockMeterFill(int which)
        {
            if (boards[which]==null)
                return -1000;
            return boards[which].GetRockMeterLevel();
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
            CurrentTime = 0;
            for (int i = 0; i < 4; i++)
                if (boards[i] != null)
                {
                    boards[i].Reset();
                }
            SongAudioMaster.GetSingleton().Restart();
        }

        internal float PercentSong()
        {
            return (float)(CurrentTime / SongAudioMaster.GetSingleton().GetSongLength().TotalSeconds);
        }

        internal double GetCurrentTime()
        {
            return CurrentTime;
        }

        internal float GetFailTime()
        {
            return failTime;
        }

        internal double GetPercentBeat()
        {
            for(int i=0;i<songData.info.barlines.Length-1;i++)
                if (songData.info.barlines[i].time <= CurrentTime * 1000 && songData.info.barlines[i+1].time > CurrentTime * 1000)
                {
                    double percentBar = ((CurrentTime*1000)-songData.info.barlines[i].time)/(songData.info.barlines[i+1].time-songData.info.barlines[i].time);
                    percentBar *= songData.info.barlines[i].numBeats;
                    percentBar = percentBar - (int)percentBar;
                    return percentBar;
                }
            return 0;
        }

        internal SongData GetSongData()
        {
            return songData;
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
            int num3d = 0;
            for (int i = 0; i < 4; i++)
                if(boards[i] != null)
                    if (boards[i].GetBoardType().Dimensions == Instrument.BoardDimensions.THREE_DIMENSIONAL)
                        num3d++;
            int width = GameSettings.windowwidth;
            if (num3d == 2)
                width = (int)(width * 0.75f);
            if (num3d == 3)
                width = (int)(width * 0.5f);
            for (int i = 0; i < 4; i++)
                if (boards[i] != null)
                    spritebatch.Draw(boards[i].GetRender(), new Rectangle((GameSettings.windowwidth / 2) - (width / 2) + boards[i].GetXOffset(), 0, width, GameSettings.windowheight), Color.White);
        }

        private void ProcessInput(GameTime gameTime, long currenttime)
        {
            for (int i = 0; i < 4; i++)
                if (boards[i] != null)
                {
                    if ((boards[i].GetBoardType().RPEnableType & Instrument.RockPowerEnableTypes.SELECT) != 0 && boards[i].Peripheral.IsPressed(PeripheralButton.SELECT))
                        StarPowerAction(i);

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
            
        }

        internal float GetBPM()
        {
            return 120;
        }
    }
}

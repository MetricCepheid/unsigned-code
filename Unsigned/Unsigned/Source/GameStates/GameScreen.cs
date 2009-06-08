using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using UnsignedPeripheralPlugins;
using SongDataIO;
using FVProductions.Utility;

namespace Unsigned
{
    class GameState : BaseState
    {
        private ContentManager Content;

        private SpriteBatch spriteBatch;

        private Board[] boards;

        private PostProcessor postProcessor;

        private PauseScreen pauseMenu;

        private Song song;

        private static Venue venue;

        private SongData songData;

        private SessionInfo sesInfo;

        private SongTime songTime;

        private GameUIMaster ui;

        public GameState(SessionInfo info)
        {
            for (int i = 0; i < info.peripherals.Length; i++)
                if (info.peripherals[i] != null)
                    info.peripherals[i].SetMode(InstrumentMaster.Singleton.GetInstrument(info.instruments[i]).CodeName);
            this.sesInfo = info;
            songTime = new SongTime(new TimeSpan(0), new TimeSpan(-40000000));
        }

        public override void Load()
        {
            Content = new ContentManager(Global.Services);
            Content.RootDirectory = "Content";

            spriteBatch = new SpriteBatch(Global.Graphics.GraphicsDevice);

            ParticleMaster.Load(Content);

            songData = SongDataLoader.LoadSongData(sesInfo.songFileName);
            song = new Song(UnsignedGame.Singleton.Window.Handle);
            song.InitSong(songData);
            postProcessor = new PostProcessor();
            postProcessor.Load(Content);
            Board.Load(Content);

            List<Board> boardsList = new List<Board>();

            int num3D = 0;
            for (int i = 0; i < 4; i++)
                if (sesInfo.peripherals[i] != null)
                    if (InstrumentMaster.Singleton.GetInstrument(sesInfo.instruments[i]).Dimensions == Instrument.BoardDimensions.THREE_DIMENSIONAL)
                        num3D++;
            int count3D = 0;
            for (int i = 0; i < 4; i++)
                if (sesInfo.peripherals[i] != null)
                {
                    boardsList.Add(new Board(InstrumentMaster.Singleton.GetInstrument(sesInfo.instruments[i]),
                        InstrumentMaster.Singleton.GetInstrument(sesInfo.instruments[i]).Dimensions == Instrument.BoardDimensions.THREE_DIMENSIONAL ? (int)(((((count3D * 2) + 1) / (num3D * 2.0f)) - 0.5f) * Global.ScreenWidth) : 0,
                        songData, sesInfo.difficulties[i]));
                    boardsList[i].LoadInstance(Content);
                    boardsList[i].Peripheral = sesInfo.peripherals[i];
                    if (InstrumentMaster.Singleton.GetInstrument(sesInfo.instruments[i]).Dimensions == Instrument.BoardDimensions.THREE_DIMENSIONAL)
                        count3D++;
                }
            boards = boardsList.ToArray();
            venue = new Venue("tikibar.gbw", songData, boards, sesInfo);

            ui = new GameUIMaster(boards, songData);
            ui.Load(Content);

            PauseScreen.Load(Content);
        }

        public override void Update(GameTime gameTime)
        {
#if !DEBUG
            try
            {
#endif
#if !DEBUG
            }
            catch(Exception e)
            {
                Debug.Error("Problem in GameMaster.Update[1]",e);
                UnsignedGame.Singleton.Exit();
                return;
            }
            try
            {
#endif
            if (pauseMenu == null)
            {
#if DEBUG
                TimeSpan elapsed = new TimeSpan(Keyboard.GetState().IsKeyDown(Keys.P)?gameTime.ElapsedGameTime.Ticks*20:gameTime.ElapsedGameTime.Ticks);
#else
                TimeSpan elapsed = new TimeSpan(gameTime.ElapsedGameTime.Ticks);
#endif
                songTime = new SongTime(elapsed, songTime.TotalSongTime + elapsed);
                if (!song.IsPlaying && songTime.TotalSongTime.TotalSeconds >= 0)
                    song.Play();
                if (song.IsPlaying)
                {
                    if (Math.Abs(song.GetTime() - songTime.TotalSongTime.TotalSeconds) > 0.1f)
                    {
#if DEBUG
                        if (Keyboard.GetState().IsKeyDown(Keys.P))
                            song.SetTime((float)songTime.TotalSongTime.TotalSeconds);
                        else
                            songTime = new SongTime(elapsed, new TimeSpan((long)(song.GetTime() * 10000000)));
#else
                        songTime = new SongTime(elapsed, new TimeSpan((long)(song.GetTime() * 10000000)));
#endif
                    }
                }
                double currenttime = songTime.TotalSongTime.TotalSeconds;

                Board.UpdateRPMultiplier(boards);

                if (currenttime >= song.GetSongLength().TotalSeconds)
                {
                    ResultsScreen r = new ResultsScreen(sesInfo, false);
                    r.SetResults(boards);
                    UnsignedGame.Singleton.SwitchState(r);
                    return;
                }

                if (Configuration.RenderVenues)
                    venue.Update(songTime);

                for (int i = 0; i < boards.Length; i++)
                    boards[i].Update(songTime);

                ui.Update(songTime);

                Peripheral[] peripherals = PeripheralManager.Singleton.GetPeripherals();
                for (int i = 0; i < peripherals.Length; i++)
                    if (peripherals[i].WasPressed(PeripheralButton.START))
                    {
                        EnablePause(peripherals[i]);
                    }
            }
            else
            {
                pauseMenu.Update(gameTime);
            }
#if !DEBUG
            }
            catch(Exception e)
            {
                Debug.Error("Problem in GameMaster.Update[2]",e);
                UnsignedGame.Singleton.Exit();
                return;
            }
#endif
        }

        public override void Render(GameTime gameTime)
        {
            RenderTarget2D oldRT = (RenderTarget2D)Global.Graphics.GraphicsDevice.GetRenderTarget(0);
#if !DEBUG
            try
            {
#endif
                for (int i = 0; i < boards.Length; i++)
                    boards[i].Draw(songTime);

                Global.Graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;
                Global.Graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
                Global.Graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
                Global.Graphics.ApplyChanges();

                Global.Graphics.GraphicsDevice.SetRenderTarget(0, oldRT);
                
                Global.Graphics.GraphicsDevice.Clear(Color.Black);
                //postProcessor.SetRenderingTarget();

                if(Configuration.RenderVenues)
                    venue.Render(songTime);
#if !DEBUG
            }
            catch(Exception e)
            {
                Debug.Error("Problem in GameState.Draw[1]", e);
                UnsignedGame.Singleton.Exit();
                return;
            }
            try
            {
#endif
                // TODO: render boards
                //RhythmMaster.Singleton.Render();
#if !DEBUG
            }
            catch(Exception e)
            {
                Debug.Error("Problem in GameState.Draw[2]", e);
                UnsignedGame.Singleton.Exit();
                return;
            }
            try
            {
#endif
                //postProcessor.ApplyPostprocess(songTime);
#if !DEBUG
            }
            catch(Exception e)
            {
                Debug.Error("Problem in GameState.Draw[3]", e);
                UnsignedGame.Singleton.Exit();
                return;
            }
            try
            {
#endif
                // TODO: well, all this commented stuff!
                //RhythmMaster.Singleton.DrawBoardRenders(spriteBatch);
                spriteBatch.Begin();
                int num3d = 0;
                for (int i = 0; i < boards.Length; i++)
                    if (boards[i].GetBoardType().Dimensions == Instrument.BoardDimensions.THREE_DIMENSIONAL)
                        num3d++;
                int width = Global.ScreenWidth;
                if (num3d == 2)
                    width = (int)(width * 0.75f);
                if (num3d == 3)
                    width = (int)(width * 0.5f);
                for (int i = 0; i < boards.Length; i++)
                    spriteBatch.Draw(boards[i].GetRender(), new Rectangle((Global.ScreenWidth / 2) - (width / 2) + boards[i].GetXOffset(), 0, width, Global.ScreenHeight), Color.White);
                spriteBatch.End();

                ui.Render();

                if (pauseMenu != null)
                    pauseMenu.Render(gameTime);
#if !DEBUG
            }
            catch(Exception e)
            {
                Debug.Error("Problem in GameState.Draw[4]",e);
                UnsignedGame.Singleton.Exit();
                return;
            }
#endif
        }

        public override void Unload()
        {
            Content.Unload();
            songData = null;
            venue = null;
        }

        public void EnablePause(Peripheral p)
        {
            if (pauseMenu == null)
            {
                song.Pause();
                pauseMenu = new PauseScreen();
                pauseMenu.SetOwner(p);
                pauseMenu.ClosePauseScreen = new VoidDelegate(DisablePause);
                pauseMenu.RestartSong = new VoidDelegate(RestartSong);
                pauseMenu.ExitSong = new VoidDelegate(ExitSong);
            }
        }

        public void DisablePause()
        {
            if(pauseMenu != null)
            {
                song.Resume(songTime);
                pauseMenu = null;
            }
        }

        public void RestartSong()
        {
            for (int i = 0; i < boards.Length; i++)
                boards[i].Reset();
            songTime = new SongTime(new TimeSpan(0), new TimeSpan(-40000000));
        }

        public void ExitSong()
        {
            UnsignedGame.Singleton.SwitchState(new SongSelectScreen(sesInfo));
        }
    }
}

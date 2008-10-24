using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using UnsignedPeripheralPlugins;
using SongDataIO;

namespace Unsigned
{
    class GameState : BaseState
    {

        private ContentManager content;

        SpecialEffectsSettings currentSettings;

        private static Venue venue;

        private SongData songData;

        private PlayerConfigNugget nugget;

        public GameState(PlayerConfigNugget nugget)
        {
            for (int i = 0; i < nugget.peripherals.Length; i++)
                if(nugget.peripherals[i]!=null)
                    nugget.peripherals[i].SetMode(InstrumentMaster.GetSingleton().GetInstrument(nugget.instruments[i]).CodeName);
            this.nugget = nugget;
        }

        public override void Update(GameTime gameTime)
        {

#if !DEBUG
                try
                {
#endif
                RhythmMaster.GetSingleton().Update(gameTime);

#if !DEBUG
            }
            catch(Exception e)
            {
#if WINDOWS
                System.Windows.Forms.MessageBox.Show("Problem in Update/IG/Pt0\n"+e.Message+"\n"+e.StackTrace);
#endif
                UnsignedGame.GetSingleton().Exit();
                return;
            }
                    try
                    {
#endif

                double currenttime = RhythmMaster.GetSingleton().GetCurrentTime();

                if (currenttime >= SongAudioMaster.GetSingleton().GetSongLength().TotalSeconds)
                    UnsignedGame.GetSingleton().PushState(new ResultsScreen());

                if (GameSettings.renderLevel > 0)
                    venue.Update(gameTime);
                Matrix matView = venue.GetViewMatrix();

                //ProcessInput(gameTime, currenttime);

                GameUIMaster.GetSingleton().Update(gameTime);

#if !DEBUG
            }
            catch(Exception e)
            {
#if WINDOWS
                System.Windows.Forms.MessageBox.Show("Problem in Update/IG/Pt1\n"+e.Message+"\n"+e.StackTrace);
#endif
                UnsignedGame.GetSingleton().Exit();
                return;
            }
#endif
                Peripheral[] peripherals = PeripheralManager.GetSingleton().GetPeripherals();
                for (int i = 0; i < peripherals.Length; i++)
                    if (peripherals[i].WasPressed(PeripheralButton.START))
                    {
                        TogglePause();
                        if (UnsignedGame.GetSingleton().IsPaused())
                        {
                            (UnsignedGame.GetSingleton().PeekState() as PauseScreen).SetOwner(peripherals[i]);
                            (UnsignedGame.GetSingleton().PeekState() as PauseScreen).SetGameState(this);
                        }
                    }
        }

        public override void Render(Microsoft.Xna.Framework.GameTime gameTime)
        {
            RenderMaster rm = RenderMaster.GetSingleton();
            GraphicsDeviceManager graphics = rm.graphics;
            FVShader effect = rm.engine;
            SpriteBatch spritebatch = rm.spritebatch;
            //long currenttime = (long)(CurrentTime / (long)(TicksPerSecond / 1000));
#if !DEBUG

                    try
                    {
#endif
            rm.graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;
            rm.graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
            rm.graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
            //graphics.PreferMultiSampling = true;
            rm.graphics.ApplyChanges();
            Version SM = rm.graphics.GraphicsDevice.GraphicsDeviceCapabilities.PixelShaderVersion;
            if (SM.Major < 1 || (SM.Major==1 && SM.Minor<1))
            {
#if !XBOX
                System.Windows.Forms.MessageBox.Show("Error. Must have minimum of Shader Model 1.1");
#endif
                UnsignedGame.GetSingleton().Exit();
            }
            
            {
                rm.graphics.GraphicsDevice.Clear(Color.Black);
                if (currentSettings.currentFES == SpecialEffectsSettings.FRAME_EFFECT_STYLE.CREST)
                {
                    if (currentSettings.countFES < 1)
                        rm.graphics.GraphicsDevice.SetRenderTarget(0, rm.screenTarget);
                    else
                        rm.graphics.GraphicsDevice.SetRenderTarget(0, rm.screenTargetPre);
                }
                else
                    rm.graphics.GraphicsDevice.SetRenderTarget(0, rm.screenTarget);

                effect.NormalMapTexture =Global.texDefaultBM;
                effect.AmbientMaterial = new Color(24, 24, 24);
                effect.DiffuseMaterial = new Color(128, 128, 128);
                effect.SpecularMaterial = Color.White;

                Matrix matProj = venue.GetProjMatrix(GameSettings.windowwidth / (float)GameSettings.windowheight);
                Matrix matView = venue.GetViewMatrix();
                rm.Projection = matProj;
                rm.View = matView;

                effect.LightingEnabled = GameSettings.Lighting;
                effect.SpecularEnabled = GameSettings.Specular;
                effect.NormalMapEnabled = GameSettings.NormalMapping;

                effect.CommitChanges();

                graphics.GraphicsDevice.Clear(Color.Black);
                if (GameSettings.renderLevel>1 && GameSettings.render3D)
                {
                    effect.Begin();
                    foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Begin();
                        venue.Render(gameTime, matProj, GBVertexFormat.VertexDeclaration);
                        pass.End();
                    }
                    effect.End();
                }
                graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;
            }
#if !DEBUG
    }
    catch(Exception e)
    {
#if WINDOWS
        System.Windows.Forms.MessageBox.Show("Problem in Draw/IG/Pt2\n"+e.Message+"\n"+e.StackTrace);
#endif
        UnsignedGame.GetSingleton().Exit();
        return;
    }

    try
    {
#endif
            RhythmMaster.GetSingleton().Render();
#if !DEBUG
    }
    catch(Exception e)
    {
#if WINDOWS
        System.Windows.Forms.MessageBox.Show("Problem in Draw/IG/Pt3\n"+e.Message+"\n"+e.StackTrace);
#endif
        UnsignedGame.GetSingleton().Exit();
        return;
    }

    try
    {
#endif
            if (GameSettings.renderLevel > 0)
            {
                if (currentSettings.currentFES == SpecialEffectsSettings.FRAME_EFFECT_STYLE.CREST)
                {
                    rm.graphics.GraphicsDevice.SetRenderTarget(0, rm.screenTargetFinal);
                    rm.graphics.GraphicsDevice.Clear(Color.Black);

                    spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.None);
                    if (currentSettings.countFES < 1)
                    {
                        spritebatch.Draw(rm.lastframe, new Rectangle(0, 0, GameSettings.windowwidth, GameSettings.windowheight), Color.White);
                        spritebatch.Draw(rm.screenTarget.GetTexture(), new Rectangle(0, 0, GameSettings.windowwidth, GameSettings.windowheight), new Color(new Vector4(1, 1, 1, ((currentSettings.countFES)))));
                    }
                    else
                    {
                        rm.lastframe = rm.screenTargetPre.GetTexture();
                        spritebatch.Draw(rm.lastframe, new Rectangle(0, 0, GameSettings.windowwidth, GameSettings.windowheight), Color.White);
                        currentSettings.countFES--;
                    }
                    //spritebatch.Draw(lastframe, new Rectangle(0, 0, GameSettings.windowwidth, GameSettings.windowheight), Color.White);
                    spritebatch.End();
                    currentSettings.countFES += gameTime.ElapsedGameTime.Milliseconds / 500f;

                    rm.graphics.GraphicsDevice.SetRenderTarget(0, null);
                    rm.graphics.GraphicsDevice.Clear(Color.Black);


                    rm.ppEngine.Parameters["gradientTex"].SetValue(Global.gradient);

                    spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
                    rm.ppEngine.CurrentTechnique = rm.ppEngine.Techniques["Gamma"];

                    //ppEngine.Begin();
                    //ppEngine.CurrentTechnique.Passes[0].Begin();
                    rm.ppEngine.Parameters["dotGrainOn"].SetValue(true);
                    rm.ppEngine.Parameters["ValueShift"].SetValue(0.5f);
                    rm.ppEngine.Parameters["grainStrength"].SetValue(.25f);
                    float time = (float)(DateTime.Now.Ticks / 1000 % 90) + 10;
                    rm.ppEngine.Parameters["time"].SetValue(time);
                    rm.ppEngine.CommitChanges();
                    spritebatch.Draw(rm.screenTargetFinal.GetTexture(), new Rectangle(0, 0, GameSettings.windowwidth, GameSettings.windowheight), Color.White);
                }
                else
                {
                    rm.graphics.GraphicsDevice.SetRenderTarget(0, null);
                    rm.graphics.GraphicsDevice.Clear(Color.Black);
                    spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
                    spritebatch.Draw(rm.screenTarget.GetTexture(), new Rectangle(0, 0, GameSettings.windowwidth, GameSettings.windowheight), Color.White);
                }
            }
            else
            {
                rm.graphics.GraphicsDevice.SetRenderTarget(0, null);
                rm.graphics.GraphicsDevice.Clear(Color.Black);
                spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
            }
#if !DEBUG
                    }
                    catch(Exception e)
                    {
#if WINDOWS
                        System.Windows.Forms.MessageBox.Show("Problem in Draw/IG/Pt4\n"+e.Message+"\n"+e.StackTrace);
#endif
                        UnsignedGame.GetSingleton().Exit();
                        return;
                    }
                    //ppEngine.CurrentTechnique.Passes[0].End();
                    //ppEngine.End();
                    //spritebatch.End();
                    try
                    {
#endif
            RhythmMaster.GetSingleton().DrawBoardRenders(spritebatch);

            GameUIMaster.GetSingleton().Render();


            RhythmMaster.GetSingleton().DrawSongInfo();
            
            
            /*if (DemoMode)
            {
                spritebatch.DrawString(BigFont, Localizer.Get("Demo Mode"), new Vector2((GameSettings.windowwidth / 2) - (BigFont.MeasureString(Localizer.Get("Demo Mode")).X / 2), GameSettings.windowheight * 0.15f), new Color(255, 0, 0, 64));
                spritebatch.DrawString(BigFont, Localizer.Get("Demo Mode"), new Vector2((GameSettings.windowwidth / 2) - (BigFont.MeasureString(Localizer.Get("Demo Mode")).X / 2), GameSettings.windowheight * 0.4f), new Color(255, 0, 0, 64));
                spritebatch.DrawString(BigFont, Localizer.Get("Demo Mode"), new Vector2((GameSettings.windowwidth / 2) - (BigFont.MeasureString(Localizer.Get("Demo Mode")).X / 2), GameSettings.windowheight * 0.65f), new Color(255, 0, 0, 64));
            }*/


            //spritebatch.DrawString(DefaultFont, "" + ((currenttime / 1000) / 3600) + ":" + ((currenttime / 1000) / 60 % 3600) + ":" + (currenttime / 1000 % 60), new Vector2(0, 0), Color.Wheat);
#if !DEBUG
                    }
                    catch(Exception e)
                    {
#if WINDOWS
                        System.Windows.Forms.MessageBox.Show("Problem in Draw/IG/Pt5\n"+e.Message+"\n"+e.StackTrace);
#endif
                        UnsignedGame.GetSingleton().Exit();
                        return;
                    }
#endif
            spritebatch.End();
        }

        public override void Load()
        {
            content = new ContentManager(UnsignedGame.GetSingleton().Services);
            songData = SongLoader.LoadSong(nugget.songFileName);
            venue = new Venue("tikibar.gbw", songData, content, nugget);
            RhythmMaster.CreateSingleton();
            RhythmMaster.GetSingleton().Initialize(nugget, songData,content);
            ParticleMaster.CreateSingleton();
            ParticleMaster.GetSingleton().Load(content);
            SongAudioMaster.CreateSingleton(UnsignedGame.GetSingleton().Window.Handle);
            SongAudioMaster.GetSingleton().InitSong(songData);
        }

        public override void Unload()
        {
            content.Unload();
            songData = null;
            venue = null;
            RhythmMaster.DestroySingleton();
            ParticleMaster.DestroySingleton();
            SongAudioMaster.DestroySingleton();
        }

        public void TogglePause()
        {
            if (!UnsignedGame.GetSingleton().IsPaused())
            {
                SongAudioMaster.GetSingleton().Pause();
                UnsignedGame.GetSingleton().PushState(new PauseScreen());
            }
            else
            {
                SongAudioMaster.GetSingleton().Resume();
                UnsignedGame.GetSingleton().PopState();
            }
        }
    }
}

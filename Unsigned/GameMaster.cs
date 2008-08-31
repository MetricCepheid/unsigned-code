using System;
using System.Collections.Generic;
using System.Text;

namespace Unsigned
{
    class GameState : BaseState
    {
        private static GameState SINGLETON_GameState = null;

        private bool IsPaused = false;
        private int pausetimer;

        SpecialEffectsSettings currentSettings;

        private GameState()
        {
            
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            if (GameLoaded)
            {
                if (!IsPaused)
                {

#if !DEBUG
                    try
                    {
#endif
                    //start song TEMPORARY CODE? probably not...
                    if (started == 0)
                    {
                        CurrentTime = 5 * TicksPerSecond;
                        started = 1;
                        return;
                    }
                    else if (started == 1)
                    {
                        if ((long)CurrentTime - (long)(gameTime.ElapsedGameTime.TotalSeconds * TicksPerSecond) < 0)
                        {
                            CurrentTime = 0;
                            song.play();
                            started = 2;
                        }
                        else
                            CurrentTime -= (long)(gameTime.ElapsedGameTime.TotalSeconds * TicksPerSecond);

                        song.GetZVals(started < 2 ? -(long)CurrentTime : (long)CurrentTime);
                        return;
                    }
                    long songt = song.getTime();
                    if (lastChange != songt && Math.Abs((float)(CurrentTime / (long)(TicksPerSecond / 1000)) - songt) > 50 && Math.Abs((float)(CurrentTime / (long)(TicksPerSecond / 1000)) - songt) < 5000)
                    {
                        CurrentTime = songt * (long)(TicksPerSecond / 1000);
                        lastChange = songt;
                    }
                    else if (lastChange != songt)
                        lastChange = songt;
                    CurrentTime += (gameTime.ElapsedGameTime.TotalSeconds * TicksPerSecond);
                    UpdateGibs(gameTime);

                    for (int i = 0; i < spcircles.Length; i++)
                    {
                        spcircles[i].rotation += spcircles[i].rotDir ? gameTime.ElapsedGameTime.Milliseconds / 1000f : -gameTime.ElapsedGameTime.Milliseconds / 1000f;
                        spcircles[i].alpha -= gameTime.ElapsedGameTime.Milliseconds / 2000f;
                        if (spcircles[i].alpha <= 0)
                        {
                            spcircles[i].alpha = (float)r.NextDouble();
                            spcircles[i].rotation = (float)r.NextDouble();
                            spcircles[i].pos = new Vector2((float)r.NextDouble(), (float)r.NextDouble());
                            spcircles[i].rotDir = r.Next() % 2 == 0;
                        }
                    }


#if !DEBUG
                }
                catch(Exception e)
                {
#if WINDOWS
                    System.Windows.Forms.MessageBox.Show("Problem in Update/IG/Pt0\n"+e.Message+"\n"+e.StackTrace);
#endif
                    Exit();
                    return;
                }
                        try
                        {
#endif

                    long currenttime = (long)((CurrentTime) / (TicksPerSecond / 1000));

                    if (song.IsOver(currenttime))
                        screen = S_RESULTS;
                    if (renderLevel > 0)
                        venue.Update(gameTime, currenttime, engine, song);
                    matView = venue.GetViewMatrix();

                    song.Update(currenttime);

                    ProcessInput(gameTime, currenttime);

                    if (rockstarDir > Math.PI * 2)
                        rockstarDir = 0;
                    rockstarDir += 1f / gameTime.ElapsedGameTime.Milliseconds;
                    if (GetRockstarAmount() > lastStar && lastStar <= 5)
                    {
                        lastStar++;
                        audioSoundBank.PlayCue("starching");
                    }
                    bool allFail = true;
                    int anyFail = 0;
                    for (int i = 0; i < 4; i++)
                    {
                        if (failStatus[i] == FS_GOOD && rockMeterLevel[i] <= 0)
                        {
                            failStatus[i] = FS_FAILING;
                            if (failTime < -1)
                                failTime = 1;
                        }
                        if (failStatus[i] == FS_GOOD)
                            allFail = false;
                        if (failStatus[i] == FS_FAILING)
                            anyFail++;
                    }
                    /*if (allFail)
                        screen = S_FAIL;
                    if (anyFail > 0)
                        failTime -= (gameTime.ElapsedGameTime.Milliseconds / 20000f) * anyFail;
                    else
                        failTime = -2;
                    if (failTime > -1 && failTime <= 0)
                        screen = S_FAIL;*/

                    if (instruments[0] && contInput[0] < 4)
                        boards[0].Whammy(controllers[contInput[0]].ThumbSticks.Right.X, currenttime, gameTime, song.GetBeatLength());
                    else if (instruments[0] && contInput[0] == 4)
                        boards[0].Whammy(Keyboard.GetState().IsKeyDown(Keys.Left) ? 1 : -1, currenttime, gameTime, song.GetBeatLength());
                    if (instruments[3] && contInput[3] < 4)
                        boards[3].Whammy(controllers[contInput[3]].ThumbSticks.Right.X, currenttime, gameTime, song.GetBeatLength());
                    else if (instruments[3] && contInput[3] == 4)
                        boards[3].Whammy(Keyboard.GetState().IsKeyDown(Keys.Left) ? 1 : -1, currenttime, gameTime, song.GetBeatLength());

                    song.GetZVals(started < 2 ? -(long)currenttime : (long)currenttime);
                    for (int i = 0; i < 4; i++)
                        if (instruments[i] && i != 1)
                        {
                            boards[i].GetNotes((long)CurrentTime, (long)(Board.eFade * Global.TicksPerSecond));
                            if (1 != 2)
                                boards[i].getWaves((long)(started < 2 ? -(CurrentTime / (TicksPerSecond / 1000)) : (CurrentTime / (TicksPerSecond / 1000))));
                        }
                    for (int i = 0; i < 4; i++)
                    {
                        if (rockMeterLevel[i] < 0)
                        {

                            rockMeterLevel[i] = 0;
                        }
                        else if (rockMeterLevel[i] > 100)
                            rockMeterLevel[i] = 100;
                    }

#if !DEBUG
                }
                catch(Exception e)
                {
#if WINDOWS
                    System.Windows.Forms.MessageBox.Show("Problem in Update/IG/Pt1\n"+e.Message+"\n"+e.StackTrace);
#endif
                    Exit();
                    return;
                }
#endif
                }
                else
                {
                    long currenttime = (long)(CurrentTime / (long)(TicksPerSecond / 1000));
                    ProcessInput(gameTime, currenttime);
                    pauseMenuPos += pauseMenuVel * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (pauseMenuPos.X > (GameSettings.windowwidth * 0.6f))
                        pauseMenuVel.X = -Math.Abs(pauseMenuVel.X);
                    if (pauseMenuPos.X < (GameSettings.windowwidth * 0.4f))
                        pauseMenuVel.X = Math.Abs(pauseMenuVel.X);
                    pauseMenuVel.Y = pauseWingRot.Y;
                    pauseWingRot.X += (float)gameTime.ElapsedGameTime.TotalSeconds * pauseWingRot.Y * 0.1f;
                    if (pauseWingRot.Y < 0)
                        pauseRot += (pauseMenuVel.X * (float)gameTime.ElapsedGameTime.TotalSeconds * 0.01f) * (Math.Sign(pauseMenuVel.X) == Math.Sign(pauseRot) ? 1 : 3);
                    if (pauseWingRot.Y > 0 && pauseWingRot.X > MathHelper.Pi / 4)
                        pauseWingRot.Y = -160;
                    else if (pauseWingRot.Y < 0 && pauseWingRot.X < -MathHelper.Pi / 2)
                        pauseWingRot.Y = 30;
                }
            }
        }

        public override void Render(Microsoft.Xna.Framework.GameTime gameTime)
        {
            long currenttime = (long)(CurrentTime / (long)(TicksPerSecond / 1000));
#if !DEBUG

                    try
                    {
#endif
           RenderMaster.GetSingleton().graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;
           RenderMaster.GetSingleton().graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
           RenderMaster.GetSingleton().graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
            vd = new VertexDeclaration(graphics.GraphicsDevice, GBVertexFormat.Elements);
            //graphics.PreferMultiSampling = true;
           RenderMaster.GetSingleton().graphics.ApplyChanges();
            Version SM =RenderMaster.GetSingleton().graphics.GraphicsDevice.GraphicsDeviceCapabilities.PixelShaderVersion;
            if (SM.Major >= 3)
                engine.CurrentTechnique = engine.Techniques["maintechnique"];
            else if (SM.Major >= 2)
                engine.CurrentTechnique = engine.Techniques["maintechniquet"];
            else
            {
#if !XBOX
                System.Windows.Forms.MessageBox.Show("Error. Must have minimum of Shader Model 2.0");
#endif
                Exit();
            }
            if (renderLevel > 0)
            {
               RenderMaster.GetSingleton().graphics.GraphicsDevice.Clear(Color.Black);
                if (currentFES == FRAME_EFFECT_STYLE.CREST)
                {
                    if (countFES < 1)
                       RenderMaster.GetSingleton().graphics.GraphicsDevice.SetRenderTarget(0, screenTarget);
                    else
                       RenderMaster.GetSingleton().graphics.GraphicsDevice.SetRenderTarget(0, screenTargetPre);
                }
                else
                   RenderMaster.GetSingleton().graphics.GraphicsDevice.SetRenderTarget(0, screenTarget);

                engine.Parameters["bumpTexture"].SetValue(texDefaultBM);
                engine.Parameters["ambientColor"].SetValue(new Vector4(0.1f, 0.1f, 0.1f, 1.0f));
                engine.Parameters["diffuseColor"].SetValue(new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
                engine.Parameters["specularColor"].SetValue(new Vector4(1f, 1f, 1f, 1.0f));

                matProj = venue.GetProjMatrix(GameSettings.windowwidth / (float)GameSettings.windowheight);
               RenderMaster.GetSingleton().graphics.GraphicsDevice.Clear(Color.Black);
                engine.Begin();
                foreach (EffectPass pass in engine.CurrentTechnique.Passes)
                {
                    pass.Begin();
                    engine.Parameters["BumpMappingEnabled"].SetValue(false);
                    engine.Parameters["SpecularEnabled"].SetValue(false);

                    engine.Parameters["fullbright"].SetValue(false);
                    matView = venue.GetViewMatrix();
                    //render the background graphics
                    engine.Parameters["view"].SetValue(matView);
                    engine.Parameters["proj"].SetValue(matProj);
                    engine.Parameters["viewInverse"].SetValue(Matrix.Invert(matView));
                    venue.Render(graphics, engine, matProj, vd, gameTime);
                    pass.End();
                }
                engine.End();
            }
#if !DEBUG
                    }
                    catch(Exception e)
                    {
#if WINDOWS
                        System.Windows.Forms.MessageBox.Show("Problem in Draw/IG/Pt2\n"+e.Message+"\n"+e.StackTrace);
#endif
                        Exit();
                        return;
                    }

                    try
                    {
#endif

            for (int i = 0; i < boards.Length; i++)
            {
                if (!instruments[i])
                    continue;

                if (i != 1)
                    DrawBoardTarget(i, song.zVals, boards[i].IsSPActivated());

               RenderMaster.GetSingleton().graphics.GraphicsDevice.SetRenderTarget(0, boardsTarget[i]);
               RenderMaster.GetSingleton().graphics.GraphicsDevice.Clear(new Color(new Vector4(0, 0, 0, 0)));

                if (i == 1)
                {
                    spritebatch.Begin();
                    boards[i].Draw(spritebatch, (currenttime));
                    spritebatch.End();
                    continue;
                }
               RenderMaster.GetSingleton().graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;
               RenderMaster.GetSingleton().graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
               RenderMaster.GetSingleton().graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
               RenderMaster.GetSingleton().graphics.ApplyChanges();

                matProj = Matrix.CreatePerspectiveFieldOfView((float)Math.PI / 4.0f,
                          boardsTarget[i].Width / (float)boardsTarget[i].Height,
                          1f, 10.0f);
                engine.CurrentTechnique = engine.Techniques["boardTechnique"];
                engine.Begin();
                foreach (EffectPass pass in engine.CurrentTechnique.Passes)
                {
                    pass.Begin();

                    engine.Parameters["view"].SetValue(Matrix.Identity);
                    engine.Parameters["viewInverse"].SetValue(Matrix.Identity);
                    engine.Parameters["proj"].SetValue(matProj);

                    //get board measure world lengths


                    //determine fling (song start board comes up)
                    Matrix fling;
                    if (started == 1)
                    {
                        if (currenttime / 1000f < 3)
                            fling = Matrix.CreateRotationX(Board.rotate);
                        else if (currenttime / 1000f < 4)
                            fling = Matrix.CreateRotationX(((-((currenttime / 1000f) - 4f)) * Board.rotate * 4) - (Board.rotate * 3));
                        else
                            fling = Matrix.CreateRotationX((float)Math.PI / 2);
                    }
                    else
                        fling = Matrix.CreateRotationX(Board.rotate);
#if DEBUG_CAM_CONTROL
                                    fling *= Matrix.CreateRotationX(-0.4f);
#endif

                    engine.Parameters["fullbright"].SetValue(true);
                    Matrix matTransl = Matrix.CreateTranslation(0f, Board.height + (boards[i].GetBoardBump() * Board.BOARD_BUMP_COEF), 0f);

                    //draw each boards
                    DrawBoard(i, fling, false, (long)CurrentTime, matTransl);
                    if (failStatus[i] == FS_GOOD)
                        DrawNotes(i, fling, (long)(started < 2 ? -CurrentTime : CurrentTime));
                    DrawBoardDetail(i, fling, matTransl);
                    if (failStatus[i] == FS_GOOD)
                    {
                        DrawWaves(i, fling, matTransl);
                        //draw the non-world gibs (glass shards sparks)
                        DrawGibs(i);
                        DrawFlashes(i, fling);
                    }
                    pass.End();
                }
                engine.End();
                //spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.None);

                //spritebatch.End();
            }
#if !DEBUG
                    }
                    catch(Exception e)
                    {
#if WINDOWS
                        System.Windows.Forms.MessageBox.Show("Problem in Draw/IG/Pt3\n"+e.Message+"\n"+e.StackTrace);
#endif
                        Exit();
                        return;
                    }

                    try
                    {
#endif
            if (renderLevel > 0)
            {
                if (currentFES == FRAME_EFFECT_STYLE.CREST)
                {
                   RenderMaster.GetSingleton().graphics.GraphicsDevice.SetRenderTarget(0, screenTargetFinal);
                   RenderMaster.GetSingleton().graphics.GraphicsDevice.Clear(Color.Black);

                    spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.None);
                    if (countFES < 1)
                    {
                        spritebatch.Draw(lastframe, new Rectangle(0, 0, GameSettings.windowwidth, GameSettings.windowheight), Color.White);
                        spritebatch.Draw(screenTarget.GetTexture(), new Rectangle(0, 0, GameSettings.windowwidth, GameSettings.windowheight), new Color(new Vector4(1, 1, 1, ((countFES)))));
                    }
                    else
                    {
                        lastframe = screenTargetPre.GetTexture();
                        spritebatch.Draw(lastframe, new Rectangle(0, 0, GameSettings.windowwidth, GameSettings.windowheight), Color.White);
                        countFES--;
                    }
                    //spritebatch.Draw(lastframe, new Rectangle(0, 0, GameSettings.windowwidth, GameSettings.windowheight), Color.White);
                    spritebatch.End();
                    countFES += gameTime.ElapsedGameTime.Milliseconds / 500f;

                   RenderMaster.GetSingleton().graphics.GraphicsDevice.SetRenderTarget(0, null);
                   RenderMaster.GetSingleton().graphics.GraphicsDevice.Clear(Color.Black);


                    ppEngine.Parameters["gradientTex"].SetValue(gradient);

                    spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
                    ppEngine.CurrentTechnique = ppEngine.Techniques["Gamma"];

                    //ppEngine.Begin();
                    //ppEngine.CurrentTechnique.Passes[0].Begin();
                    ppEngine.Parameters["dotGrainOn"].SetValue(true);
                    ppEngine.Parameters["ValueShift"].SetValue(0.5f);
                    ppEngine.Parameters["grainStrength"].SetValue(.25f);
                    float time = (float)(DateTime.Now.Ticks / 1000 % 90) + 10;
                    ppEngine.Parameters["time"].SetValue(time);
                    ppEngine.CommitChanges();
                    spritebatch.Draw(screenTargetFinal.GetTexture(), new Rectangle(0, 0, GameSettings.windowwidth, GameSettings.windowheight), Color.White);
                }
                else
                {
                   RenderMaster.GetSingleton().graphics.GraphicsDevice.SetRenderTarget(0, null);
                   RenderMaster.GetSingleton().graphics.GraphicsDevice.Clear(Color.Black);
                    spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
                    spritebatch.Draw(screenTarget.GetTexture(), new Rectangle(0, 0, GameSettings.windowwidth, GameSettings.windowheight), Color.White);
                }
            }
            else
            {
               RenderMaster.GetSingleton().graphics.GraphicsDevice.SetRenderTarget(0, null);
                spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
               RenderMaster.GetSingleton().graphics.GraphicsDevice.Clear(Color.Black);
            }
#if !DEBUG
                    }
                    catch(Exception e)
                    {
#if WINDOWS
                        System.Windows.Forms.MessageBox.Show("Problem in Draw/IG/Pt4\n"+e.Message+"\n"+e.StackTrace);
#endif
                        Exit();
                        return;
                    }
                    //ppEngine.CurrentTechnique.Passes[0].End();
                    //ppEngine.End();
                    //spritebatch.End();
                    try
                    {
#endif
            for (int i = 0; i < 4; i++)
                if (instruments[i])
                    spritebatch.Draw(boardsTarget[i].GetTexture(), new Rectangle(boards[i].xOffset, 0, GameSettings.windowwidth, GameSettings.windowheight), Color.White);
            //draw score/stars
            DrawRockMeter(currenttime);
            DrawScoreStars(currenttime);
            //draw rock meter

            //spritebatch.Draw(boards[0].SPMRTex, new Rectangle(0, 0, 256, 128), Color.White);

            //draw development info
            {
                float fps = 0;
                lastframes[frameIndex % lastframes.Length] = (float)gameTime.ElapsedGameTime.TotalSeconds;

                frameIndex++;
                for (int i = 0; i < lastframes.Length; i++)
                    fps += lastframes[i];
                fps /= lastframes.Length;
                fps = 1 / fps;
                if (ShowFPS)
                    spritebatch.DrawString(DefaultFont, "" + (int)fps, new Vector2(GameSettings.windowwidth - 40, GameSettings.windowheight - 40), Color.Red);
                //spritebatch.Draw(boards[0].texBoard, new Rectangle(0, 0, 300, 600), Color.White);
                //spritebatch.Draw(boards[0].texBoard, new Rectangle(0, 10, 100, 200), Color.White);
                //spritebatch.DrawString(DefaultFont, "" + venue.camindex, new Vector2(0,24), Color.Red);
                //spritebatch.DrawString(DefaultFont, "" + (boards[2].lastPressed & bits[0]) + (boards[2].lastPressed & bits[1]) + (boards[2].lastPressed & bits[2]) + (boards[2].lastPressed & bits[3]) + (boards[2].lastPressed & bits[4]), new Vector2(0, 48), Color.Red);
                //spritebatch.DrawString(DefaultFont, "" + boards[2].multiplier, new Vector2(0, 48), Color.Red);
                //spritebatch.DrawString(DefaultFont, "" + controllers[contInput[0]].ThumbSticks.Right.Y, new Vector2(0, 48), Color.Red);

            }

            if (started < 2)
            {
                float alpha;
                if (CurrentTime > 4 * TicksPerSecond)
                    alpha = 1 - (((float)CurrentTime - (4 * TicksPerSecond)) / (float)TicksPerSecond);
                else if (CurrentTime < TicksPerSecond)
                    alpha = (float)CurrentTime / (float)TicksPerSecond;
                else
                    alpha = 1;
                Color aColor = new Color(new Vector4(1, 1, 1, alpha));
                for (int i = 0; i < 2; i++)
                    spritebatch.DrawString(DefaultFont, song.songInfo[i], new Vector2((GameSettings.windowwidth / 2) - (DefaultFont.MeasureString(song.songInfo[i]).X / 2), 180 + (40 * i)), aColor);
                for (int i = 2; i < song.songInfo.Length; i++)
                    spritebatch.DrawString(DefaultFont, song.songInfo[i], new Vector2((GameSettings.windowwidth / 2) - (DefaultFont.MeasureString(song.songInfo[i]).X / 2), 220 + (40 * i)), aColor);
            }
            if (IsPaused)
            {
                float scale = 1.5f;

                Vector2 origin = new Vector2(texPauseBorder.Width / 2, texPauseBorder.Height / 2);
                Vector3 wingPosR = new Vector3(455, 79, 0);
                Vector3 wingPosL = new Vector3(25, 59, 0);
                wingPosR -= new Vector3(origin, 0);
                wingPosL -= new Vector3(origin, 0);
                wingPosR *= scale;
                wingPosL *= scale;
                wingPosR = Vector3.Transform(wingPosR, Matrix.CreateRotationZ(pauseRot));
                wingPosL = Vector3.Transform(wingPosL, Matrix.CreateRotationZ(pauseRot));

                Vector3[] textPos = new Vector3[pauseTextDisp.Length];

                for (int i = 0; i < textPos.Length; i++)
                {
                    textPos[i] = new Vector3(texPauseBorder.Width / 2, ((350 - 68) * ((i + 1f) / (textPos.Length + 1f))) + 68, 0);
                    textPos[i] -= new Vector3(origin, 0);
                    textPos[i] *= scale;
                    textPos[i] = Vector3.Transform(textPos[i], Matrix.CreateRotationZ(pauseRot));
                }

                Vector3 pickPosL, pickPosR;

                pickPosL = new Vector3(texPauseBorder.Width * 0.25f, ((350 - 68) * ((pauseSelected + 1f) / (textPos.Length + 1f))) + 68, 0);
                pickPosL -= new Vector3(origin, 0);
                pickPosL *= scale;
                pickPosL = Vector3.Transform(pickPosL, Matrix.CreateRotationZ(pauseRot));

                pickPosR = new Vector3(texPauseBorder.Width * 0.75f, ((350 - 68) * ((pauseSelected + 1f) / (textPos.Length + 1f))) + 68, 0);
                pickPosR -= new Vector3(origin, 0);
                pickPosR *= scale;
                pickPosR = Vector3.Transform(pickPosR, Matrix.CreateRotationZ(pauseRot));

                spritebatch.Draw(texPauseBorder, pauseMenuPos, null, Color.White, pauseRot, origin, scale, SpriteEffects.None, 0);
                spritebatch.Draw(texPauseWings, new Vector2(wingPosR.X, wingPosR.Y) + pauseMenuPos, null, Color.White, -pauseWingRot.X / 2, new Vector2(32, 69), scale, SpriteEffects.None, 0);
                spritebatch.Draw(texPauseWings, new Vector2(wingPosL.X, wingPosL.Y) + pauseMenuPos, null, Color.White, pauseWingRot.X / 2, new Vector2(texPauseWings.Width - 32, 69), scale, SpriteEffects.FlipHorizontally, 0);
                for (int i = 0; i < textPos.Length; i++)
                    spritebatch.DrawString(DefaultFont, pauseTextDisp[i], new Vector2(textPos[i].X, textPos[i].Y) + pauseMenuPos, pauseSelected == i ? Color.Red : new Color(100, 128, 100), pauseRot, DefaultFont.MeasureString(pauseTextDisp[i]) * 0.5f, scale * 2, SpriteEffects.None, 0);
                spritebatch.Draw(texPausePick, new Vector2(pickPosL.X, pickPosL.Y) + pauseMenuPos, null, Color.White, pauseRot, new Vector2(texPausePick.Width, texPausePick.Height / 2), scale / 3, SpriteEffects.None, 0);
                spritebatch.Draw(texPausePick, new Vector2(pickPosR.X, pickPosR.Y) + pauseMenuPos, null, Color.White, pauseRot + MathHelper.Pi, new Vector2(texPausePick.Width, texPausePick.Height / 2), scale / 3, SpriteEffects.None, 0);
            }
            /*if (DemoMode)
            {
                spritebatch.DrawString(BigFont, "Demo Mode", new Vector2((GameSettings.windowwidth / 2) - (BigFont.MeasureString("Demo Mode").X / 2), GameSettings.windowheight * 0.15f), new Color(255, 0, 0, 64));
                spritebatch.DrawString(BigFont, "Demo Mode", new Vector2((GameSettings.windowwidth / 2) - (BigFont.MeasureString("Demo Mode").X / 2), GameSettings.windowheight * 0.4f), new Color(255, 0, 0, 64));
                spritebatch.DrawString(BigFont, "Demo Mode", new Vector2((GameSettings.windowwidth / 2) - (BigFont.MeasureString("Demo Mode").X / 2), GameSettings.windowheight * 0.65f), new Color(255, 0, 0, 64));
            }*/


            //spritebatch.DrawString(DefaultFont, "" + ((currenttime / 1000) / 3600) + ":" + ((currenttime / 1000) / 60 % 3600) + ":" + (currenttime / 1000 % 60), new Vector2(0, 0), Color.Wheat);
#if !DEBUG
                    }
                    catch(Exception e)
                    {
#if WINDOWS
                        System.Windows.Forms.MessageBox.Show("Problem in Draw/IG/Pt5\n"+e.Message+"\n"+e.StackTrace);
#endif
                        Exit();
                        return;
                    }
#endif
            spritebatch.End();
        }

        public override void Load(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            
        }

        public override void Unload(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            
        }

        public static void CreateSingleton()
        {
            if (SINGLETON_GameMaster == null)
                SINGLETON_GameMaster = new GameMaster();
            else
                throw new InvalidOperationException("Singleton has already been initialized");
        }

        public static void DestroySingleton()
        {
            if (SINGLETON_GameMaster != null)
                SINGLETON_GameMaster = null;
            else
                throw new InvalidOperationException("Singleton has already been destroyed");
        }

        public static GameState GetSingleton()
        {
            return SINGLETON_GameMaster;
        }
    }
}

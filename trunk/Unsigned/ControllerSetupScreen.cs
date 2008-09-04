using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Unsigned
{
    class ControllerSetupScreen : BaseState
    {
        private enum ConfirmState
        {
            EMPTY_SLOT=0,
            CHOOSING_NAME,
            CHOOSING_INSTRUMENT,
            FULLY_CONFIRMED
        };

        private RenderTarget2D[] rtNote;
        private Texture2D[] texNote;
        private Texture2D[] nPadTex;
        private Texture2D concrTex, concrBM;
        private Texture2D flameTex, hairr, hairl;
        private Model nPadMdl;

        private int[] selectedCharacter;
        private ConfirmState[] confirmStates;

        private float idleTime;
        private float[] arrowTimer, arrowRot;
        private Vector3[][] flames;
        private bool notesNeedRefreshing;

        public ControllerSetupScreen()
        {
            texNote = new Texture2D[4];
            nPadTex = new Texture2D[4];
            selectedCharacter = new int[4];
            arrowRot = new float[4];
            arrowTimer = new float[4];
            flames = new Vector3[4][];
        }

        public override void Load(ContentManager content)
        {
            nPadMdl = content.Load<Model>("meshes\\paper1");
            nPadTex = new Texture2D[4];
            nPadTex[0] = content.Load<Texture2D>("graphics\\paper1");
            nPadTex[1] = content.Load<Texture2D>("graphics\\paper2");
            nPadTex[2] = content.Load<Texture2D>("graphics\\paper3");
            nPadTex[3] = content.Load<Texture2D>("graphics\\paper4");
            flames = new Vector3[4][];
            flames[0] = new Vector3[100];
            flames[1] = new Vector3[100];
            flames[2] = new Vector3[100];
            flames[3] = new Vector3[100];
        }

        public override void Unload()
        {
            
        }

        public override void Update(GameTime gameTime)
        {
            RenderMaster rm = RenderMaster.GetSingleton();
            if (rtNote == null || notesNeedRefreshing)
            {
                if (rtNote == null)
                {
                    rtNote = new RenderTarget2D[4];
                    rtNote[0] = new RenderTarget2D(rm.graphics.GraphicsDevice, 256, 256, 1, SurfaceFormat.Color);
                    rtNote[1] = new RenderTarget2D(rm.graphics.GraphicsDevice, 256, 256, 1, SurfaceFormat.Color);
                    rtNote[2] = new RenderTarget2D(rm.graphics.GraphicsDevice, 256, 256, 1, SurfaceFormat.Color);
                    rtNote[3] = new RenderTarget2D(rm.graphics.GraphicsDevice, 256, 256, 1, SurfaceFormat.Color);
                }

                for (int i = 0; i < 4; i++)
                {
                   RenderMaster.GetSingleton().graphics.GraphicsDevice.SetRenderTarget(0, rtNote[i]);
                    rm.spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
                    rm.spritebatch.Draw(nPadTex[i], new Rectangle(0, 0, 256, 256), Color.White);
                    rm.spritebatch.DrawString(rm.fontHandwritten, CharacterMaster.GetSingleton().GetCharacter(selectedCharacter[i]).name, new Vector2(70, 20), Color.Black);
                    rm.spritebatch.End();
                   RenderMaster.GetSingleton().graphics.GraphicsDevice.SetRenderTarget(0, null);
                    texNote[i] = rtNote[i].GetTexture();
                }
            }
#if !DEBUG
            try
            {
#endif
                /*for (int i = 0; i < 8; i++)
                {
                    if (arrowTimer[i] <= 0)
                    {
                        if (i % 2 == 0)
                            if (selectedCharacter[i / 2] <= 0)
                            {
                                arrowRot[i] = 0;
                                arrowTimer[i] = 5000;
                                continue;
                            }
                        if (i % 2 == 1)
                            if (selectedCharacter[i / 2] >= charNames[(i / 2) < 3 ? (i / 2) : 0].Length - 1)
                            {
                                arrowRot[i] = 0;
                                arrowTimer[i] = 5000;
                                continue;
                            }
                        if (arrowRot[i] < Math.PI * 2)
                        {
                            arrowRot[i] += gameTime.ElapsedGameTime.Milliseconds / 100f;
                            if (arrowRot[i] > Math.PI * 2)
                            {
                                arrowRot[i] = 0;
                                arrowTimer[i] = 1000 + (float)(r.NextDouble() * 3000);
                            }
                        }
                    }
                    else
                        arrowTimer[i] -= gameTime.ElapsedGameTime.Milliseconds;
                }*/

#if !DEBUG
                }
                catch(Exception e)
                {
#if WINDOWS
                    System.Windows.Forms.MessageBox.Show("Problem in Update/CCS/Pt1\n"+e.Message+"\n"+e.StackTrace);
#endif
                    Exit();
                    return;
                }
                    try
                    {
#endif


                /* if ((kblev > 0.5 && !finalized[kblev == 1 ? 0 : (int)(kblev + 0.5)]) || kblev <= 0.5)
                     {
                         if (Math.Abs(kbld) < 0.01 && kblev > 0 && Keyboard.GetState().IsKeyDown(Keys.Left))
                             kbld = -1;
                         if (Math.Abs(kbld) < 0.01 && kblev < 3 && Keyboard.GetState().IsKeyDown(Keys.Right))
                             kbld = 1;
                     }
                     else if ((kblev > 0.5 && !chosen[kblev == 1 ? 0 : (int)(kblev + 0.5)]) || kblev <= 0.5)
                     {
                         if (charNameSelected[kblev == 1 ? 0 : (int)(kblev + 0.5)] < charNames[kblev == 1 ? 0 : kblev < 3 ? (int)(kblev + 0.5) : 0].Length - 1 && Keyboard.GetState().IsKeyDown(Keys.Right))
                             charNameSelected[kblev == 1 ? 0 : (int)(kblev + 0.5)]++;
                         if (charNameSelected[kblev == 1 ? 0 : (int)(kblev + 0.5)] > 0 && Keyboard.GetState().IsKeyDown(Keys.Left))
                             charNameSelected[kblev == 1 ? 0 : (int)(kblev + 0.5)]--;
                     }
                     kblev += kbld * (gameTime.ElapsedGameTime.Milliseconds / 500f);
                     if (kbld > 0)
                         if (kblev >= kblevp + 1)
                         {
                             kblev = kblevp + 1;
                             kblevp = kblev;
                             kbld = 0;
                         }
                     if (kbld < 0)
                         if (kblev <= kblevp - 1)
                         {
                             kblev = kblevp - 1;
                             kblevp = kblev;
                             kbld = 0;
                         }
                     Vector3[] nfo = { new Vector3(-140, 180, -120), new Vector3(-140, 190, -110), new Vector3(-96, 128, -110), new Vector3(32, 128, -110), new Vector3(96, 128, -110) };
                     if (kblev == (int)kblev)
                         kbinfo = new Vector4(nfo[(int)kblev + (kblev == 0 ? 0 : 1)], kblev <= 0 ? (float)Math.PI / 2 : 0);
                     else if (kblev < 1)
                     {
                         if (kblev < 0.2)
                             kbinfo = new Vector4((nfo[0] * ((0.2f - kblev) / 0.2f)) + (nfo[1] * (kblev % 1 / 0.2f)), (1 - kblev) * (float)(Math.PI / 2));
                         else
                             kbinfo = new Vector4((nfo[1] * ((1f - kblev) / 0.8f)) + (nfo[2] * ((kblev - 0.2f) / 0.8f)), (1 - kblev) * (float)(Math.PI / 2));
                     }
                     else
                     {
                         if (kbld > 0)
                             kbinfo = new Vector4((nfo[(int)kblevp + 1] * (1 - (kblev % 1))) + (nfo[(int)(kblevp + kbld) + 1] * (kblev % 1)), 0);
                         else
                             kbinfo = new Vector4((nfo[(int)kblevp + 1] * (kblev % 1)) + (nfo[(int)(kblevp + kbld) + 1] * (1 - (kblev % 1))), 0);
                     }*/
                /*for (int k = 0; k < 4; k++)
                    for (int i = 0; i < flames[k].Length; i++)
                        if (flames[k][i].Z > 0)
                        {
                            flames[k][i].Z -= gameTime.ElapsedGameTime.Milliseconds / 1000f;
                            flames[k][i].Y += (k + 1) * 1.5f * gameTime.ElapsedGameTime.Milliseconds / 100f;
                            flames[k][i].X += (float)(Global.random.NextDouble() - 0.5) * gameTime.ElapsedGameTime.Milliseconds / 50f;
                        }*/
#if !DEBUG
                }
                catch(Exception e)
                {
                    
#if WINDOWS
                    System.Windows.Forms.MessageBox.Show("Problem in Update/CCS/Pt2\n"+e.Message+"\n"+e.StackTrace);
#endif
                    Exit();
                    return;
                }
                    try
                    {
#endif
                /*if (Keyboard.GetState().IsKeyDown(Keys.Space) || Keyboard.GetState().IsKeyDown(Keys.Enter))
                        if ((int)kblev == kblev && kblev > 0)
                            if (wait <= 0)
                            {
                                if(kblev > 0.5 && finalized[kblev == 1 ? 0 : (int)(kblev + 0.5)] && chosen[kblev == 1 ? 0 : (int)(kblev + 0.5)])
                                {
                                    screen = S_INGAME;
                                    contInput = new byte[4];
                                    contInput[kblev == 1 ? 0 : (int)(kblev + 0.5)] = 4;
                                    InitForSong(chosen[0], chosen[1], chosen[2], chosen[3], diff, "garage");
                                }
                                else if (!finalized[kblev == 1 ? 0 : (int)(kblev + 0.5)])
                                {
                                    finalized[kblev == 1 ? 0 : (int)kblev] = true;
                                    wait = 30;
                                }
                                else if ((kblev > 0.5 && finalized[kblev == 1 ? 0 : (int)(kblev + 0.5)]) || kblev <= 0.5)
                                {
                                    chosen[kblev == 1 ? 0 : (int)kblev] = true;
                                    leader = kblev == 1 ? 0 : (int)kblev;
                                    wait = 30;
                                }
                            }
                    if (Keyboard.GetState().IsKeyDown(Keys.Back) || Keyboard.GetState().IsKeyDown(Keys.Escape))
                        if ((int)kblev == kblev && kblev > 0)
                            if (wait <= 0)
                            {
                                if (chosen[kblev == 1 ? 0 : (int)(kblev + 0.5)])
                                {
                                    chosen[kblev == 1 ? 0 : (int)kblev] = false;
                                    wait = 30;
                                }
                                else if (finalized[kblev == 1 ? 0 : (int)(kblev + 0.5)])
                                {
                                    finalized[kblev == 1 ? 0 : (int)kblev] = false;
                                }
                            }

                    if (wait > 0)
                        wait--;*/
                /*for (int i = 0; i < 5; i++)
                {
                    if (contguis[i].nextLoc > 0)
                        finals[contguis[i].nextLoc - 1] = true;
                    if (contguis[i].GetLeaderVal() * ((contguis[i].status == 2) ? 1 : 0) > contguis[leader].GetLeaderVal() * ((contguis[leader].status == 2) ? 1 : 0))
                        leader = i;
                }
                for (int i = 0; i < 5; i++)
                {
                    ContGUIData.RETURN_VALUE ret = contguis[i].Update(gameTime, finals);
                    if (ret != ContGUIData.RETURN_VALUE.NOTHING)
                    {
                        if (ret == ContGUIData.RETURN_VALUE.NEXT_SCREEN)
                        {
                            int cgcount = 0;
                            for (int p = 0; p < contguis.Length; p++)
                                if (contguis[p].loc > 0.5)
                                    cgcount++;
                            if (cgcount > 0)
                                if (leader == i)
                                {
                                    screen = S_CHOOSESONG;
                                    contInput = new byte[4];
                                    rockerNames = new String[4];
                                    for (int k = 0; k < 4; k++)
                                    { contInput[k] = 255; instruments[k] = false; rockerNames[k] = null; }
                                    for (int k = 0; k < 5; k++)
                                        if (contguis[k].status == 2)
                                        {
                                            rockerNames[(int)contguis[k].loc - 1] = charNameSelected[(int)contguis[k].loc - 1] > 0 ? charNames[(int)contguis[k].loc - 1 > 2 ? 0 : (int)contguis[k].loc - 1][charNameSelected[(int)contguis[k].loc - 1]] : "Default";
                                            instruments[(int)contguis[k].loc - 1] = true;
                                            contInput[(int)contguis[k].loc - 1] = (byte)((int)contguis[k].index >= 0 ? (int)contguis[k].index : 4);
                                        }
                                    menu_ticker = 200;
                                }
                            idleTime = 0;
                        }
                        else
                        {
                            idleTime = 0;
                            if (ret == ContGUIData.RETURN_VALUE.INCREMENT_NAME && charNameSelected[(int)contguis[i].loc - 1] < charNames[((int)contguis[i].loc - 1) <= 2 ? ((int)contguis[i].loc - 1) : 0].Length - 1)
                            {
                                if (contguis[i].loc == 1 && charNameSelected[0] + 1 == charNameSelected[3] && charNameSelected[0] + 2 < charNames[0].Length)
                                    charNameSelected[(int)contguis[i].loc - 1]++;
                                else if (contguis[i].loc == 4 && charNameSelected[3] + 1 == charNameSelected[0] && charNameSelected[3] + 2 < charNames[0].Length)
                                    charNameSelected[(int)contguis[i].loc - 1]++;
                                else if ((contguis[i].loc == 1 && charNameSelected[0] + 1 == charNameSelected[3]))
                                    charNameSelected[(int)contguis[i].loc - 1]--;
                                else if (contguis[i].loc == 4 && charNameSelected[3] + 1 == charNameSelected[0])
                                    charNameSelected[(int)contguis[i].loc - 1]--;
                                charNameSelected[(int)contguis[i].loc - 1]++;
                            }
                            if (ret == ContGUIData.RETURN_VALUE.DECREMENT_NAME && charNameSelected[(int)contguis[i].loc - 1] > -1)
                            {
                                if (contguis[i].loc == 1 && charNameSelected[0] - 1 == charNameSelected[3] && charNameSelected[0] - 2 >= -1)
                                    charNameSelected[(int)contguis[i].loc - 1]--;
                                else if (contguis[i].loc == 4 && charNameSelected[3] - 1 == charNameSelected[0] && charNameSelected[3] - 2 >= -1)
                                    charNameSelected[(int)contguis[i].loc - 1]--;
                                else if (contguis[i].loc == 1 && charNameSelected[0] - 1 == charNameSelected[3] && charNameSelected[0] - 1 != -1)
                                    charNameSelected[(int)contguis[i].loc - 1]++;
                                else if (contguis[i].loc == 4 && charNameSelected[3] - 1 == charNameSelected[0] && charNameSelected[3] - 1 != -1)
                                    charNameSelected[(int)contguis[i].loc - 1]++;
                                charNameSelected[(int)contguis[i].loc - 1]--;
                            }
                            ort = (RenderTarget2D)graphics.GraphicsDevice.GetRenderTarget(0);
                           RenderMaster.GetSingleton().graphics.GraphicsDevice.SetRenderTarget(0, rtNote[(int)contguis[i].loc - 1]);
                            spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
                            spritebatch.Draw(nPadTex[(int)contguis[i].loc - 1], new Rectangle(0, 0, 256, 256), Color.White);
                            spritebatch.DrawString(sfManager, musicianNames[(int)contguis[i].loc - 1], new Vector2(70, 20), Color.Black);
                            if (contguis[i].status == 1)
                                spritebatch.DrawString((int)contguis[i].loc - 1 == 0 || (int)contguis[i].loc - 1 == 3 ? sfGuitarist : (int)contguis[i].loc - 1 == 1 ? sfSinger : sfDrummer, (charNameSelected[(int)contguis[i].loc - 1]) >= 0 ? charNames[((int)contguis[i].loc - 1 < 3) ? (int)contguis[i].loc - 1 : 0][charNameSelected[(int)contguis[i].loc - 1]] : "New Rocker", new Vector2(100, 80), Color.Black, (float)Math.PI / 4 - 0.07f, new Vector2(0, 0), 1.4f, SpriteEffects.None, 0);
                            spritebatch.End();
                           RenderMaster.GetSingleton().graphics.GraphicsDevice.SetRenderTarget(0, ort);
                            texNote[(int)contguis[i].loc - 1] = rtNote[(int)contguis[i].loc - 1].GetTexture();
                        }
                    }
                }

                for (int i = 0; i < 5; i++)
                    if (contguis[i].index >= 0)
                        if ((((int)contguis[i].index == 4 && Keyboard.GetState().IsKeyDown(Keys.Back)) || ((int)contguis[i].index != 4 && controllers[(int)contguis[i].index].IsButtonDown(Buttons.B))) && contguis[i].status == 0)
                        { screen = S_MAINMENU; menu_ticker = 200; }
                */
#if !DEBUG
                }
                catch(Exception e)
                {
#if WINDOWS
                    System.Windows.Forms.MessageBox.Show("Problem in Update/CCS/Pt3\n"+e.Message+"\n"+e.StackTrace);
#endif
                    Exit();
                    return;
                }
#endif
                /*for (int i = 0; i < 4; i++)
                    contInput[i] = 100;

                for (byte i = 0; i < 4; i++)
                {
                    if (contCapabilities[i].GamePadType == GamePadType.Guitar)
                    {
                        if (contInput[0] >=100)
                            contInput[0] = i;
                        else if (contInput[3] >=100)
                            contInput[3] = i;
                    }
                    else if (contCapabilities[i].GamePadType == GamePadType.DrumKit)
                    {
                        if (contInput[2] >= 100)
                            contInput[2] = i;
                    }
                    else if (contCapabilities[i].GamePadType == GamePadType.GamePad)
                    {
                        if (contInput[1] >= 100)
                            contInput[1] = i;
                    }
                }

                bool noone = true;
                for (int i = 0; i < 4; i++)
                    if (contInput[i] < 100)
                        noone = false;
                if (noone)
                    contInput[2] = 4;*/
        }

        public override void Render(GameTime gameTime)
        {
            UnsignedGame game = UnsignedGame.GetSingleton();
            RenderMaster rm = RenderMaster.GetSingleton();
            Effect effect = rm.engine;

            float vmul = 2f, hmul = 2f;
            bool[] plo = new bool[16];
            Vector3[] plp = new Vector3[16];
            float[] pln = new float[16];
            float[] plf = new float[16];
            Vector3[] pld = new Vector3[16];
            Vector3[] pls = new Vector3[16];
#if !DEBUG
                    try
                    {
#endif
            rm.graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
            rm.graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
            //graphics.PreferMultiSampling = true;
            rm.graphics.ApplyChanges();

            VertexDeclaration vd = new VertexDeclaration(rm.graphics.GraphicsDevice, GBVertexFormat.Elements);
            rm.graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
            //graphics.GraphicsDevice.

            effect.Parameters["bumpTexture"].SetValue(Global.texDefaultBM);
            effect.Parameters["ambientColor"].SetValue(new Vector4(0.1f, 0.1f, 0.1f, 1.0f));
            effect.Parameters["diffuseColor"].SetValue(new Vector4(0.8f, 0.8f, 0.8f, 1.0f));
            effect.Parameters["specularColor"].SetValue(new Vector4(1f, 1f, 1f, 1.0f));
            effect.Parameters["dLDiffuseColor"].SetValue(new Vector4(0, 0, 0, 0));
            effect.Parameters["dLSpecularColor"].SetValue(new Vector4(0, 0, 0, 0));

            Random r = new Random();


            /*for (int i = 0; i < contguis.Length; i++)
            {
                if (contguis[i].status >= 2)
                {
                    plo[linum] = true;
                    plp[linum] = new Vector3(-192 + (contguis[i].loc * 80), 192, -100);
                    pln[linum] = r.Next(64);
                    plf[linum] = r.Next(64) + 65;
                    pld[linum] = new Vector3(.9f + (float)(r.NextDouble() / 10), .5f + (float)(r.NextDouble() / 10), .2f + (float)(r.NextDouble() / 10));
                    pls[linum] = new Vector3(0.2f, 0.1f, 0.0f);
                    linum++;
                }
            }*/
            effect.Parameters["pLightOn"].SetValue(plo);
            effect.Parameters["pLightPos"].SetValue(plp);
            effect.Parameters["pLightNear"].SetValue(pln);
            effect.Parameters["pLightFar"].SetValue(plf);
            effect.Parameters["pLightDiffuse"].SetValue(pld);
            effect.Parameters["pLightSpecular"].SetValue(pls);
            effect.Parameters["dLDiffuseColor"].SetValue(new Vector4(0.2f, 0.2f, 0.2f, 1.0f));
            effect.Parameters["dLSpecularColor"].SetValue(new Vector4(0.0f, 0.0f, 0.0f, 1.0f));
            effect.Parameters["dLightDir"].SetValue(new Vector3(0, 1, 1));

            Version SM =RenderMaster.GetSingleton().graphics.GraphicsDevice.GraphicsDeviceCapabilities.PixelShaderVersion;
            if (SM.Major >= 3)
                effect.CurrentTechnique = effect.Techniques["menutechnique"];
            else if (SM.Major >= 2)
                effect.CurrentTechnique = effect.Techniques["menutechniquet"];
            else
            {
#if WINDOWS
                System.Windows.Forms.MessageBox.Show("Whoops! Your graphics card only supports Shader Model "+SM.Major+"."+SM.Minor+"\nYou need at least 2.0 to run Unsigned");
#endif
                game.Exit();
            }
            effect.CommitChanges();
#if !DEBUG
                    }
                    catch(Exception e)
                    {
#if WINDOWS
                        System.Windows.Forms.MessageBox.Show("Problem in Draw/CCS/Pt1\n"+e.Message+"\n"+e.StackTrace);
#endif
                        Exit();
                        return;
                    }
                    try
                    {
#endif
            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();
                //SetProjMatrix(Window.ClientBounds.Width, Window.ClientBounds.Height);

                //effect.Parameters["fullbright"].SetValue(false);

                Matrix matView;
                if (idleTime < 29.5)
                    matView = Matrix.CreateLookAt(new Vector3(-8, 128, 150), new Vector3(-8, 128, 0), new Vector3(0, 1, 0));
                else
                    matView = Matrix.CreateLookAt(new Vector3(-8, 128, 150), new Vector3(-24 + ((idleTime * hmul) % 1 < 0.5 ? (idleTime * hmul) % 0.5f * 32 : (1 - ((idleTime * hmul) % .5f * 2)) * 16), 128 - ((idleTime * vmul) % 1 < 0.5 ? (idleTime * vmul) % 0.5f * 32 : (1 - ((idleTime * vmul) % .5f * 2)) * 16), 0), new Vector3(0, 1, 0));
                rm.SetViewMatrix(matView);

                Matrix matRot, matScale, matTranslate;
                {
                    matTranslate = Matrix.CreateTranslation(-32, 120, -128);
                    matRot = Matrix.CreateRotationX((float)Math.PI / 2);
                    matScale = Matrix.CreateScale(300, 1, 192);

                    effect.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                    effect.Parameters["wRot"].SetValue(matRot);
                    effect.Parameters["diffuseTexture"].SetValue(concrTex);
                    effect.Parameters["bumpTexture"].SetValue(concrBM);
                    effect.Parameters["shininess"].SetValue(0.25f);
                    effect.Parameters["SpecularEnabled"].SetValue(false);
                    effect.Parameters["vertexAlpha"].SetValue(true);
                    effect.Parameters["BumpMappingEnabled"].SetValue(true);
                    effect.CommitChanges();

                    rm.graphics.GraphicsDevice.VertexDeclaration = vd;
                    rm.graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                    rm.graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                    rm.graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                    rm.graphics.GraphicsDevice.Vertices[0].SetSource(Global.square, 0, GBVertexFormat.SizeInBytes);
                    rm.graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
                    rm.graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                }

                for (int i = 0; i < 4; i++)
                {
                    matTranslate = Matrix.CreateTranslation(-96 + (64 * i), 192, -126);
                    matRot = Matrix.Identity;
                    matScale = Matrix.CreateScale(16, 16, 16);

                    effect.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                    effect.Parameters["wRot"].SetValue(matRot);
                    effect.Parameters["diffuseTexture"].SetValue(texNote[i]);
                    effect.Parameters["diffuseColor"].SetValue(new Vector4(0.8f, 0.8f, 0.8f, 1.0f));
                    effect.Parameters["bumpTexture"].SetValue(Global.texDefaultBM);
                    effect.Parameters["shininess"].SetValue(0.25f);
                    effect.Parameters["SpecularEnabled"].SetValue(false);
                    effect.Parameters["vertexAlpha"].SetValue(false);
                    effect.Parameters["BumpMappingEnabled"].SetValue(false);
                    effect.CommitChanges();

                    foreach (ModelMesh mesh in nPadMdl.Meshes)
                    {
                        foreach (ModelMeshPart meshpart in mesh.MeshParts)
                        {
                           rm.graphics.GraphicsDevice.VertexDeclaration = meshpart.VertexDeclaration;
                           rm.graphics.GraphicsDevice.Vertices[0].SetSource(mesh.VertexBuffer, meshpart.StreamOffset, meshpart.VertexStride);
                           rm.graphics.GraphicsDevice.Indices = mesh.IndexBuffer;
                           rm.graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, meshpart.BaseVertex, 0, meshpart.NumVertices, meshpart.StartIndex, meshpart.PrimitiveCount);
                        }
                    }
                }

                /*for (int i = 0; i < 5; i++)
                {
                    if (contguis[i].status == 1)
                    {
                        matTranslate = Matrix.CreateTranslation(-96 + (64 * (contguis[i].loc - 1)) - 24, 180, -120);
                        matRot = Matrix.CreateRotationZ(arrowRot[(int)(contguis[i].loc - 1) * 2]) * Matrix.CreateRotationY(-(float)Math.PI / 2) * Matrix.CreateRotationZ(-MathHelper.PiOver2);
                        matScale = Matrix.CreateScale(4, 2, 4);

                        engine.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                        engine.Parameters["wRot"].SetValue(matRot);
                        engine.Parameters["diffuseTexture"].SetValue(arrowTex);
                        engine.Parameters["bumpTexture"].SetValue(texDefaultBM);
                        engine.Parameters["diffuseColor"].SetValue(charNameSelected[(int)(contguis[i].loc - 1)] > -1 ? new Vector4(0f, 1f, 0f, 1f) : new Vector4(1f, 0f, 0f, 1f));
                        engine.Parameters["specularColor"].SetValue(charNameSelected[(int)(contguis[i].loc - 1)] > -1 ? new Vector4(0f, 1f, 0f, 1f) : new Vector4(1f, 0f, 0f, 1f));
                        engine.Parameters["SpecularEnabled"].SetValue(true);
                        engine.Parameters["shininess"].SetValue(4f);
                        engine.Parameters["vertexAlpha"].SetValue(false);
                        engine.Parameters["BumpMappingEnabled"].SetValue(false);
                        engine.CommitChanges();
                        foreach (ModelMesh mesh in arrowMdl.Meshes)
                        {
                            foreach (ModelMeshPart meshpart in mesh.MeshParts)
                            {
                               RenderMaster.GetSingleton().graphics.GraphicsDevice.VertexDeclaration = meshpart.VertexDeclaration;
                               RenderMaster.GetSingleton().graphics.GraphicsDevice.Vertices[0].SetSource(mesh.VertexBuffer, meshpart.StreamOffset, meshpart.VertexStride);
                               RenderMaster.GetSingleton().graphics.GraphicsDevice.Indices = mesh.IndexBuffer;
                               RenderMaster.GetSingleton().graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, meshpart.BaseVertex, 0, meshpart.NumVertices, meshpart.StartIndex, meshpart.PrimitiveCount);
                            }
                        }

                        matTranslate = Matrix.CreateTranslation(-96 + (64 * (int)(contguis[i].loc - 1)) + 24, 180, -120);
                        matRot = Matrix.CreateRotationZ(arrowRot[(int)(contguis[i].loc - 1) * 2 + 1]) * Matrix.CreateRotationY((float)Math.PI / 2) * Matrix.CreateRotationZ(-MathHelper.PiOver2);
                        matScale = Matrix.CreateScale(4, 2, 4);

                        engine.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                        engine.Parameters["wRot"].SetValue(matRot);
                        engine.Parameters["diffuseColor"].SetValue(charNameSelected[(int)(contguis[i].loc - 1)] < charNames[(int)(contguis[i].loc - 1) < 3 ? (int)(contguis[i].loc - 1) : 0].Length - 1 ? new Vector4(0f, 1f, 0f, 1f) : new Vector4(1f, 0f, 0f, 1f));
                        engine.CommitChanges();
                        foreach (ModelMesh mesh in arrowMdl.Meshes)
                        {
                            foreach (ModelMeshPart meshpart in mesh.MeshParts)
                            {
                               RenderMaster.GetSingleton().graphics.GraphicsDevice.VertexDeclaration = meshpart.VertexDeclaration;
                               RenderMaster.GetSingleton().graphics.GraphicsDevice.Vertices[0].SetSource(mesh.VertexBuffer, meshpart.StreamOffset, meshpart.VertexStride);
                               RenderMaster.GetSingleton().graphics.GraphicsDevice.Indices = mesh.IndexBuffer;
                               RenderMaster.GetSingleton().graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, meshpart.BaseVertex, 0, meshpart.NumVertices, meshpart.StartIndex, meshpart.PrimitiveCount);
                            }
                        }
                    }

                }*/

                /*{
                    matTranslate = Matrix.CreateTranslation(-140, 200, -120);
                    matRot = Matrix.CreateRotationX((float)Math.PI / 4);
                    matScale = Matrix.CreateScale(2);

                    effect.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                    effect.Parameters["wRot"].SetValue(matRot);
                    effect.Parameters["diffuseColor"].SetValue(new Vector4(0.8f, 0.8f, 0.8f, 1f));
                    effect.Parameters["diffuseTexture"].SetValue(rustyTex);
                    effect.CommitChanges();
                    foreach (ModelMesh mesh in nailMdl.Meshes)
                    {
                        foreach (ModelMeshPart meshpart in mesh.MeshParts)
                        {
                           RenderMaster.GetSingleton().graphics.GraphicsDevice.VertexDeclaration = meshpart.VertexDeclaration;
                           RenderMaster.GetSingleton().graphics.GraphicsDevice.Vertices[0].SetSource(mesh.VertexBuffer, meshpart.StreamOffset, meshpart.VertexStride);
                           RenderMaster.GetSingleton().graphics.GraphicsDevice.Indices = mesh.IndexBuffer;
                           RenderMaster.GetSingleton().graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, meshpart.BaseVertex, 0, meshpart.NumVertices, meshpart.StartIndex, meshpart.PrimitiveCount);
                        }
                    }
                }*/
                /*{
                    matTranslate = Matrix.CreateTranslation(100, 85, -80);
                    matRot = Matrix.CreateRotationY(-(float)Math.PI / 4);
                    matScale = Matrix.CreateScale(5);

                    engine.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                    engine.Parameters["wRot"].SetValue(matRot);
                    engine.Parameters["diffuseColor"].SetValue(new Vector4(0.2f, 0.2f, 0.2f, 1f));
                    engine.Parameters["diffuseTexture"].SetValue(texWhite);
                    engine.CommitChanges();
                    foreach (ModelMesh mesh in stand.Meshes)
                    {
                        foreach (ModelMeshPart meshpart in mesh.MeshParts)
                        {
                           RenderMaster.GetSingleton().graphics.GraphicsDevice.VertexDeclaration = meshpart.VertexDeclaration;
                           RenderMaster.GetSingleton().graphics.GraphicsDevice.Vertices[0].SetSource(mesh.VertexBuffer, meshpart.StreamOffset, meshpart.VertexStride);
                           RenderMaster.GetSingleton().graphics.GraphicsDevice.Indices = mesh.IndexBuffer;
                           RenderMaster.GetSingleton().graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, meshpart.BaseVertex, 0, meshpart.NumVertices, meshpart.StartIndex, meshpart.PrimitiveCount);
                        }
                    }
                }
                {
                    matTranslate = Matrix.CreateTranslation(105, 92, -85);
                    matRot = Matrix.CreateRotationY((float)Math.PI / 2) * Matrix.CreateRotationX((float)Math.PI / 2 - 0.2f) * Matrix.CreateRotationY(-(float)Math.PI / 4);
                    matScale = Matrix.CreateScale(5);

                    engine.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                    engine.Parameters["wRot"].SetValue(matRot);
                    engine.Parameters["diffuseColor"].SetValue(new Vector4(0.8f, 0.8f, 0.8f, 1f));
                    engine.Parameters["diffuseTexture"].SetValue(stratTex);
                    engine.CommitChanges();
                    foreach (ModelMesh mesh in strat.Meshes)
                    {
                        foreach (ModelMeshPart meshpart in mesh.MeshParts)
                        {
                           RenderMaster.GetSingleton().graphics.GraphicsDevice.VertexDeclaration = meshpart.VertexDeclaration;
                           RenderMaster.GetSingleton().graphics.GraphicsDevice.Vertices[0].SetSource(mesh.VertexBuffer, meshpart.StreamOffset, meshpart.VertexStride);
                           RenderMaster.GetSingleton().graphics.GraphicsDevice.Indices = mesh.IndexBuffer;
                           RenderMaster.GetSingleton().graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, meshpart.BaseVertex, 0, meshpart.NumVertices, meshpart.StartIndex, meshpart.PrimitiveCount);
                        }
                    }
                }*/
                for (int k = 0; k < 4; k++)
                    for (int i = 0; i < flames[k].Length; i++)
                        if (flames[k][i].Z > 0)
                        {
                            matTranslate = Matrix.CreateTranslation(new Vector3(flames[k][i].X, flames[k][i].Y, -117 + (k * 0.5f)));
                            matRot = Matrix.CreateRotationX((float)Math.PI / 2);
                            matScale = Matrix.CreateScale(8, 1, Math.Max(24 * (1 - flames[k][i].Z), 4));

                            effect.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                            effect.Parameters["wRot"].SetValue(matRot);
                            effect.Parameters["diffuseColor"].SetValue(new Vector4(0.8f, 0.8f, 0.8f, 1.0f));
                            effect.Parameters["specularColor"].SetValue(new Vector4(0f, 0f, 0f, 1f));
                            float alpha = 0;
                            if (flames[k][i].Z > 3 / 4f)
                                alpha = 1 - ((flames[k][i].Z - 3 / 4f) * 4);
                            else
                                alpha = flames[k][i].Z;
                            effect.Parameters["wAlpha"].SetValue(alpha);
                            effect.Parameters["diffuseTexture"].SetValue(flameTex);
                            effect.Parameters["fullbright"].SetValue(true);
                            effect.Parameters["SpecularEnabled"].SetValue(false);
                            effect.Parameters["vertexAlpha"].SetValue(true);
                            effect.Parameters["BumpMappingEnabled"].SetValue(false);
                            effect.CommitChanges();

                            rm.graphics.GraphicsDevice.VertexDeclaration = vd;
                            rm.graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                            rm.graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                            rm.graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                            rm.graphics.GraphicsDevice.Vertices[0].SetSource(Global.square, 0, GBVertexFormat.SizeInBytes);
                            rm.graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
                            rm.graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                        }

                effect.Parameters["fullbright"].SetValue(true);
                /*{//keyboard gui
                    {
                        matTranslate = Matrix.CreateTranslation(new Vector3(contguis[0].info.X, contguis[0].info.Y, contguis[0].info.Z));
                        matRot = Matrix.CreateRotationX((float)Math.PI / 2) * Matrix.CreateRotationZ(contguis[0].info.W);
                        matScale = Matrix.CreateScale(32, 32, 32);

                        effect.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                        effect.Parameters["wRot"].SetValue(matRot);
                        effect.Parameters["diffuseTexture"].SetValue(contguis[0].loc <= 0.99 ? ContGUIData.KB_ICO_BLUR : contguis[0].loc <= 1.99 ? ContGUIData.KB_ICO_GUITAR : contguis[0].loc <= 2.99 ? ContGUIData.KB_ICO_VOCAL : contguis[0].loc <= 3.99 ? ContGUIData.KB_ICO_DRUM : ContGUIData.KB_ICO_GUITAR);
                        effect.Parameters["bumpTexture"].SetValue(texDefaultBM);
                        effect.Parameters["shininess"].SetValue(0.25f);
                        effect.Parameters["wAlpha"].SetValue(1);
                        effect.Parameters["SpecularEnabled"].SetValue(false);
                        effect.Parameters["vertexAlpha"].SetValue(true);
                        effect.Parameters["BumpMappingEnabled"].SetValue(true);
                        effect.CommitChanges();

                        rm.graphics.GraphicsDevice.VertexDeclaration = vd;
                        rm.graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                        rm.graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                        rm.graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                        rm.graphics.GraphicsDevice.Vertices[0].SetSource(square, 0, GBVertexFormat.SizeInBytes);
                        rm.graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
                        rm.graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                    }
                    if (contguis[0].loc != (int)contguis[0].loc)
                    {
                        matTranslate = Matrix.CreateTranslation(new Vector3(contguis[0].info.X, contguis[0].info.Y, contguis[0].info.Z));
                        matRot = Matrix.CreateRotationX((float)Math.PI / 2) * Matrix.CreateRotationZ(contguis[0].info.W);
                        matScale = Matrix.CreateScale(32, 32, 32);

                        engine.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                        engine.Parameters["wRot"].SetValue(matRot);
                        engine.Parameters["diffuseTexture"].SetValue(contguis[0].loc <= 1 ? ContGUIData.KB_ICO_GUITAR : contguis[0].loc <= 2 ? ContGUIData.KB_ICO_VOCAL : contguis[0].loc <= 3 ? ContGUIData.KB_ICO_DRUM : ContGUIData.KB_ICO_GUITAR);
                        engine.Parameters["bumpTexture"].SetValue(texDefaultBM);
                        engine.Parameters["shininess"].SetValue(0);
                        engine.Parameters["wAlpha"].SetValue(contguis[0].loc - (int)contguis[0].loc);
                        engine.Parameters["SpecularEnabled"].SetValue(false);
                        engine.Parameters["vertexAlpha"].SetValue(true);
                        engine.Parameters["BumpMappingEnabled"].SetValue(true);
                        engine.CommitChanges();

                       RenderMaster.GetSingleton().graphics.GraphicsDevice.VertexDeclaration = vd;
                       RenderMaster.GetSingleton().graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                       RenderMaster.GetSingleton().graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                       RenderMaster.GetSingleton().graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                       RenderMaster.GetSingleton().graphics.GraphicsDevice.Vertices[0].SetSource(square, 0, GBVertexFormat.SizeInBytes);
                       RenderMaster.GetSingleton().graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
                       RenderMaster.GetSingleton().graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                    }
                }*/

                effect.Parameters["wAlpha"].SetValue(1);
                for (int j = 1; j <= 4; j++)
                /*{//instrument gui
                    {
                        matTranslate = Matrix.CreateTranslation(new Vector3(contguis[j].info.X, contguis[j].info.Y, contguis[j].info.Z - contguis[j].loc));
                        matRot = Matrix.CreateRotationX((float)Math.PI / 2) * Matrix.CreateRotationZ(contguis[j].info.W);
                        matScale = Matrix.CreateScale(32, 32, 32);

                        engine.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                        engine.Parameters["wRot"].SetValue(matRot);
                        if (contguis[j].type == ContGUIData.CONT_TYPE.DRUMSET)
                            engine.Parameters["diffuseTexture"].SetValue(contguis[j].loc <= 0.99 ? ContGUIData.DRUMS_ICO_BLUR : ContGUIData.DRUMS_ICO);
                        else if (contguis[j].type == ContGUIData.CONT_TYPE.STRATOCASTER)
                            engine.Parameters["diffuseTexture"].SetValue(contguis[j].loc <= 0.99 ? ContGUIData.GUITAR_ICO_BLUR : ContGUIData.GUITAR_ICO);
                        else if (contguis[j].type == ContGUIData.CONT_TYPE.XPLORER)
                            engine.Parameters["diffuseTexture"].SetValue(contguis[j].loc <= 0.99 ? ContGUIData.GUITARX_ICO_BLUR : ContGUIData.GUITARX_ICO);
                        else if (contguis[j].type == ContGUIData.CONT_TYPE.MICROPHONE)
                            engine.Parameters["diffuseTexture"].SetValue(contguis[j].loc <= 0.99 ? ContGUIData.MICROPHONE_ICO_BLUR : ContGUIData.MICROPHONE_ICO);
                        engine.Parameters["bumpTexture"].SetValue(texDefaultBM);
                        engine.Parameters["shininess"].SetValue(0.25f);
                        engine.Parameters["wAlpha"].SetValue(1);
                        engine.Parameters["SpecularEnabled"].SetValue(false);
                        engine.Parameters["vertexAlpha"].SetValue(true);
                        engine.Parameters["BumpMappingEnabled"].SetValue(true);
                        engine.CommitChanges();

                       RenderMaster.GetSingleton().graphics.GraphicsDevice.VertexDeclaration = vd;
                       RenderMaster.GetSingleton().graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                       RenderMaster.GetSingleton().graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                       RenderMaster.GetSingleton().graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                       RenderMaster.GetSingleton().graphics.GraphicsDevice.Vertices[0].SetSource(square, 0, GBVertexFormat.SizeInBytes);
                       RenderMaster.GetSingleton().graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
                       RenderMaster.GetSingleton().graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                    }
                    if (contguis[j].loc > 0 && contguis[j].loc < 1)
                    {
                        matTranslate = Matrix.CreateTranslation(new Vector3(contguis[j].info.X, contguis[j].info.Y, contguis[j].info.Z - contguis[j].loc));
                        matRot = Matrix.CreateRotationX((float)Math.PI / 2) * Matrix.CreateRotationZ(contguis[j].info.W);
                        matScale = Matrix.CreateScale(32, 32, 32);

                        engine.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                        engine.Parameters["wRot"].SetValue(matRot);
                        if (contguis[j].type == ContGUIData.CONT_TYPE.DRUMSET)
                            engine.Parameters["diffuseTexture"].SetValue(ContGUIData.DRUMS_ICO);
                        else if (contguis[j].type == ContGUIData.CONT_TYPE.STRATOCASTER)
                            engine.Parameters["diffuseTexture"].SetValue(ContGUIData.GUITAR_ICO);
                        else if (contguis[j].type == ContGUIData.CONT_TYPE.XPLORER)
                            engine.Parameters["diffuseTexture"].SetValue(ContGUIData.GUITARX_ICO);
                        else if (contguis[j].type == ContGUIData.CONT_TYPE.MICROPHONE)
                            engine.Parameters["diffuseTexture"].SetValue(ContGUIData.MICROPHONE_ICO);
                        engine.Parameters["bumpTexture"].SetValue(texDefaultBM);
                        engine.Parameters["shininess"].SetValue(0);
                        engine.Parameters["wAlpha"].SetValue(contguis[j].loc);
                        engine.Parameters["SpecularEnabled"].SetValue(false);
                        engine.Parameters["vertexAlpha"].SetValue(true);
                        engine.Parameters["BumpMappingEnabled"].SetValue(true);
                        engine.CommitChanges();

                       RenderMaster.GetSingleton().graphics.GraphicsDevice.VertexDeclaration = vd;
                       RenderMaster.GetSingleton().graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                       RenderMaster.GetSingleton().graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                       RenderMaster.GetSingleton().graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                       RenderMaster.GetSingleton().graphics.GraphicsDevice.Vertices[0].SetSource(square, 0, GBVertexFormat.SizeInBytes);
                       RenderMaster.GetSingleton().graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
                       RenderMaster.GetSingleton().graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                    }
                }*/
                effect.Parameters["fullbright"].SetValue(false);
                effect.Parameters["wAlpha"].SetValue(1.0f);

                pass.End();
            }
            effect.End();
#if !DEBUG
                    }
                    catch (Exception e)
                    {
#if WINDOWS
                        System.Windows.Forms.MessageBox.Show("Problem in Draw/CCS/Pt2\n"+e.Message+"\n"+e.StackTrace);
#endif
                        Exit();
                        return;
                    }
                    try
                    {
#endif
            rm.spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);

            //if (leader >= 0 && contguis[leader].status == 2)
                //spritebatch.Draw(texContinue, new Rectangle((int)(Window.ClientBounds.Width * (contguis[leader].loc) / 6), Window.ClientBounds.Height - (Window.ClientBounds.Height / 5), Window.ClientBounds.Width / 6, Window.ClientBounds.Height / 6), Color.Red);

            if (idleTime > 30)
            {
                rm.spritebatch.Draw(hairl, new Rectangle(-20, 0, (int)game.Window.ClientBounds.Height - (int)((idleTime * hmul) % 1 < 0.5 ? (idleTime * hmul) % 0.5f * (game.Window.ClientBounds.Height * 2) : (1 - ((idleTime * hmul) % .5f * 2)) * game.Window.ClientBounds.Height), (int)game.Window.ClientBounds.Height), Color.White);
                rm.spritebatch.Draw(hairr, 
                    new Rectangle(20 + (int)game.Window.ClientBounds.Width - (int)((idleTime * hmul) % 1 < 0.5 ? (idleTime * hmul) % 0.5f * (game.Window.ClientBounds.Height * 2) : (1 - ((idleTime * hmul) % .5f * 2)) * game.Window.ClientBounds.Height), 
                                  0, 
                                  (int)((idleTime * hmul) % 1 < 0.5 ? (idleTime * hmul) % 0.5f * (game.Window.ClientBounds.Height * 2) : (1 - ((idleTime * hmul) % .5f * 2)) * game.Window.ClientBounds.Height), 
                                  (int)game.Window.ClientBounds.Height), 
                    Color.White);
            }

            /*rm.spritebatch.Draw(GameUIMaster.GetSingleton().texButtonGreen, new Rectangle((int)(0.1f * GameSettings.windowwidth), (int)(0.80f * GameSettings.windowheight), (int)(0.09f * GameSettings.windowheight), (int)(0.09f * GameSettings.windowheight)), Color.White);
            if (leader >= 0 && contguis[leader].status == 2)
                rm.spritebatch.DrawString(Global.DefaultFont, "Continue", new Vector2((0.10f * GameSettings.windowwidth) + (int)(0.1f * GameSettings.windowheight), (0.90f * GameSettings.windowheight) - (DefaultFont.MeasureString("Continue").Y)), Color.White);
            else
                rm.spritebatch.DrawString(Global.DefaultFont, "Select", new Vector2((0.10f * GameSettings.windowwidth) + (int)(0.1f * GameSettings.windowheight), (0.90f * GameSettings.windowheight) - (DefaultFont.MeasureString("Select").Y)), Color.White);*/
            /*rm.spritebatch.Draw(tbgRed, new Rectangle((int)(0.80f * GameSettings.windowwidth), (int)(0.80f * GameSettings.windowheight), (int)(0.09f * GameSettings.windowheight), (int)(0.09f * GameSettings.windowheight)), Color.White);
            rm.spritebatch.DrawString(DefaultFont, "Back", new Vector2((0.80f * GameSettings.windowwidth) - DefaultFont.MeasureString("Back").X, (0.90f * GameSettings.windowheight) - (DefaultFont.MeasureString("Back").Y)), Color.White);*/
            //spritebatch.DrawString(DefaultFont, "" + contguis[0].type+","+GamePad.GetState(PlayerIndex.One).IsConnected + ","+ GamePad.GetCapabilities(PlayerIndex.One).GamePadType, new Vector2(100, 100), Color.Red);

            //spritebatch.DrawString(DefaultFont, "" + contguis[0].loc + "::" + contguis[0].info, new Vector2(10, 10), Color.White);
            if (Global.DemoMode)
            {
                rm.spritebatch.DrawString(Global.BigFont, "Demo Mode", new Vector2((GameSettings.windowwidth / 2) - (Global.BigFont.MeasureString("Demo Mode").X / 2), GameSettings.windowheight * 0.15f), new Color(255, 0, 0, 64));
                rm.spritebatch.DrawString(Global.BigFont, "Demo Mode", new Vector2((GameSettings.windowwidth / 2) - (Global.BigFont.MeasureString("Demo Mode").X / 2), GameSettings.windowheight * 0.4f), new Color(255, 0, 0, 64));
                rm.spritebatch.DrawString(Global.BigFont, "Demo Mode", new Vector2((GameSettings.windowwidth / 2) - (Global.BigFont.MeasureString("Demo Mode").X / 2), GameSettings.windowheight * 0.65f), new Color(255, 0, 0, 64));
            }
            rm.spritebatch.End();
#if !DEBUG
                    }
                    catch (Exception e)
                    {
#if WINDOWS
                        System.Windows.Forms.MessageBox.Show("Problem in Draw/CCS/Pt3\n"+e.Message+"\n"+e.StackTrace);
#endif
                        Exit();
                        return;
                    }
#endif
        }
    }
}

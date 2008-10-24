using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UnsignedPeripheralPlugins;

namespace Unsigned
{
    class MainMenuScreen : BaseState
    {
        private ContentManager content;
        private float waveM1, waveM2;
        private int mmenu_select = 0, menu_ticker = 0;
        private float mmLogoTime;
        private SpriteFont sfMenu;
        private int menuSnakeRotOffset;
        private Model mSnake;
        private Texture2D texSnake, texSnakeBM, texSnakeSkinBM, texSnakeSkin;
        private RenderTarget2D rtSnake, rtSnakeBM;
        private String[][] mMenuStr;
        private Color[][] mMenuCol;
        private Vector2 menuShiftPos;
        private Texture2D waves, wave;
        private Texture2D songchoosetop;
        private Texture2D texHeader;

        public MainMenuScreen()
        {
            mMenuStr = new string[1][];
            mMenuStr[0] = new string[3];
            mMenuStr[0][0] = "QUICKPLAY";
            mMenuStr[0][1] = "OPTIONS";
            mMenuStr[0][2] = "EXIT";
            mMenuCol = new Color[1][];
            mMenuCol[0] = new Color[3];
            mMenuCol[0][0] = Color.White;
            mMenuCol[0][1] = Color.White;
            mMenuCol[0][2] = Color.White;
        }

        public override void Load()
        {
            content = new ContentManager(UnsignedGame.GetSingleton().Services);
            texSnakeSkin = content.Load<Texture2D>("graphics\\snake");
            texSnakeSkinBM = content.Load<Texture2D>("graphics\\snakeBM");
            mSnake = content.Load<Model>("meshes\\snake");
            rtSnake = new RenderTarget2D(RenderMaster.GetSingleton().graphics.GraphicsDevice, 512, 128, 1, SurfaceFormat.Color);
            rtSnakeBM = new RenderTarget2D(RenderMaster.GetSingleton().graphics.GraphicsDevice, 512, 128, 1, SurfaceFormat.Color);
            waves = content.Load<Texture2D>("graphics\\waves");
            wave = content.Load<Texture2D>("graphics\\wave");
            songchoosetop = content.Load<Texture2D>("graphics\\songscreentop");
            texHeader = content.Load<Texture2D>("graphics\\header");
        }

        public override void Unload()
        {
            content.Unload();
        }

        public override void Update(GameTime gameTime)
        {
#if !DEBUG
                    try
                    {
#endif
            waveM1 += gameTime.ElapsedGameTime.Milliseconds / 50f;
            waveM2 -= gameTime.ElapsedGameTime.Milliseconds / 25f;
            if (waveM1 >= GameSettings.windowwidth)
                waveM1 -= GameSettings.windowwidth;
            if (waveM2 <= 0)
                waveM2 += GameSettings.windowwidth;
            if (menu_ticker <= 0)
            {
                if (mmLogoTime > 2)
                {
                    int collective = 0;
                    bool green = false, red = false;
                    PeripheralManager.GetSingleton().ReloadDLLs();
                    PeripheralManager.GetSingleton().CheckConnections();
                    PeripheralManager.GetSingleton().QueryAll();
                    Peripheral[] controllers = PeripheralManager.GetSingleton().GetPeripherals();
                    for (int i = 0; i < controllers.Length; i++)
                    {
                        if (controllers[i].WasPressed(PeripheralButton.DOWN))
                            collective--;
                        if (controllers[i].WasPressed(PeripheralButton.UP))
                            collective++;
                        if (controllers[i].WasPressed(PeripheralButton.CONFIRM))
                            green = true;
                        if (controllers[i].WasPressed(PeripheralButton.BACK))
                            red = true;
                    }
                    if (mmenu_select % 10 == 0)
                    {
                        mmenu_select -= 10 * collective;
                        if (collective != 0)
                        {
                            menuShiftPos.Y = -1 * collective;
                            menuSnakeRotOffset += collective;
                        }
                        while (mmenu_select < 10)
                        { mmenu_select += 10; menuShiftPos.Y = 0; menuSnakeRotOffset -= collective; }
                        while (mmenu_select >= 40)
                        { mmenu_select -= 10; menuShiftPos.Y = 0; menuSnakeRotOffset -= collective; }

                        if (green && mmenu_select == 10)
                            UnsignedGame.SINGLETON.PushState(new ControllerSetupScreen());
                        if (green && mmenu_select == 20)
                            UnsignedGame.SINGLETON.PushState(new OptionsScreen());
                        if (green && mmenu_select == 30)
                            UnsignedGame.SINGLETON.Exit();
                    }
                    if (collective != 0 || green || red)
                        menu_ticker = 200;
                }
                else
                {
                    mmLogoTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    while (mmenu_select < 10)
                    { mmenu_select += 10; menuShiftPos.Y = 0; }
                    while (mmenu_select >= 50)
                    { mmenu_select -= 10; menuShiftPos.Y = 0; }
                }
            }
            else
                menu_ticker -= gameTime.ElapsedGameTime.Milliseconds;
            menuShiftPos.Y *= 0.9f;
            menuShiftPos.X *= 0.9f;

#if !DEBUG
                }
                catch(Exception e)
                {
#if WINDOWS
                    System.Windows.Forms.MessageBox.Show("Problem in Update/MM/Pt1\n"+e.Message+"\n"+e.StackTrace);
#endif
                    UnsignedGame.GetSingleton().Exit();
                    return;
                }
#endif
        }
        public override void Render(Microsoft.Xna.Framework.GameTime gameTime)
        {
            RenderMaster rm = RenderMaster.GetSingleton();
            FVShader effect = rm.menuEngine;
            rm.graphics.GraphicsDevice.Clear(Color.Black);

#if !DEBUG
                    try
                    {
#endif
            Color txt = Color.Black;

            rm.graphics.GraphicsDevice.SetRenderTarget(0, rtSnake);
            rm.graphics.GraphicsDevice.Clear(Color.Red);
            rm.spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);
            //29.589 69.727
            float xscale = 128, yscale = 48;
            float rotxscale = 128, rotyscale = 48;
            for (int i = -1; i < 5; i++)
                for (int k = -1; k < 4; k++)
                {
                    rm.spritebatch.Draw(texSnakeSkin, new Rectangle((int)(((menuShiftPos.X * 2) * rotxscale) + (i * xscale)), (int)((menuShiftPos.Y * rotyscale) + (k * yscale)), (int)(xscale), (int)(yscale)), Color.White);
                }
            if (Math.Abs(menuShiftPos.X) <= 0.004)
            {
                if (mmenu_select % 10 == 0)
                {
                    for (int i = 0; i < mMenuStr[0].Length; i++)
                    {
                        Vector2 sz = Global.DefaultFont.MeasureString("" + Localizer.Get(mMenuStr[0][i]).ToUpper());
                        rm.spritebatch.DrawString(Global.DefaultFont, "" + Localizer.Get(mMenuStr[0][i]).ToUpper(), new Vector2((rtSnake.Width / 2) - (sz.X / 2), (rtSnake.Height / 2) - (sz.Y / 2) + (yscale * (i + menuShiftPos.Y - (mmenu_select / 10) + 1))), mMenuCol[0][i]);
                    }
                }
                else
                {
                    for (int i = 0; i < mMenuStr[mmenu_select / 10].Length; i++)
                    {
                        Vector2 sz = Global.DefaultFont.MeasureString("" + Localizer.Get(mMenuStr[mmenu_select / 10][i]).ToUpper());
                        rm.spritebatch.DrawString(Global.DefaultFont, "" + Localizer.Get(mMenuStr[mmenu_select / 10][i]).ToUpper(), new Vector2((rtSnake.Width / 2) - (sz.X / 2), (rtSnake.Height / 2) - (sz.Y / 2) + (yscale * (i + menuShiftPos.Y - (mmenu_select % 10) + 1))), mMenuCol[mmenu_select / 10][i]);
                    }
                }
            }
            else if (menuShiftPos.X > 0)
            {
                for (int i = 0; i < mMenuStr[0].Length; i++)
                {
                    Vector2 sz = Global.DefaultFont.MeasureString("" + Localizer.Get(mMenuStr[0][i]).ToUpper());
                    rm.spritebatch.DrawString(Global.DefaultFont, "" + Localizer.Get(mMenuStr[0][i]).ToUpper(), new Vector2((rtSnake.Width / 2) - (sz.X / 2) + (((menuShiftPos.X * 2) - 2) * xscale), (rtSnake.Height / 2) - (sz.Y / 2) + (yscale * (i + menuShiftPos.Y - (mmenu_select / 10) + 1))), new Color(mMenuCol[0][i].R, mMenuCol[0][i].G, mMenuCol[0][i].B, (byte)(255 * menuShiftPos.X)));
                }
                for (int i = 0; i < mMenuStr[mmenu_select / 10].Length; i++)
                {
                    Vector2 sz = Global.DefaultFont.MeasureString("" + Localizer.Get(mMenuStr[mmenu_select / 10][i]).ToUpper());
                    rm.spritebatch.DrawString(Global.DefaultFont, "" + Localizer.Get(mMenuStr[mmenu_select / 10][i]).ToUpper(), new Vector2((rtSnake.Width / 2) - (sz.X / 2) + ((menuShiftPos.X * 2) * xscale), (rtSnake.Height / 2) - (sz.Y / 2) + (yscale * (i + menuShiftPos.Y - (mmenu_select % 10) + 1))), new Color(mMenuCol[mmenu_select / 10][i].R, mMenuCol[mmenu_select / 10][i].G, mMenuCol[mmenu_select / 10][i].B, (byte)(255 * (1 - menuShiftPos.X))));
                }
                
            }
            else if (menuShiftPos.X < 0)
            {
                for (int i = 0; i < mMenuStr[0].Length; i++)
                {
                    Vector2 sz = Global.DefaultFont.MeasureString("" + Localizer.Get(mMenuStr[0][i]).ToUpper());
                    rm.spritebatch.DrawString(Global.DefaultFont, "" + Localizer.Get(mMenuStr[0][i]).ToUpper(), new Vector2((rtSnake.Width / 2) - (sz.X / 2) + (((menuShiftPos.X * 2)) * xscale), (rtSnake.Height / 2) - (sz.Y / 2) + (yscale * (i + menuShiftPos.Y - (mmenu_select / 10) + 1))), new Color(mMenuCol[0][i].R, mMenuCol[0][i].G, mMenuCol[0][i].B, (byte)(255 * menuShiftPos.X)));
                }
                for (int i = 0; i < mMenuStr[mmenu_select / 10].Length; i++)
                {
                    Vector2 sz = Global.DefaultFont.MeasureString("" + Localizer.Get(mMenuStr[mmenu_select / 10][i]).ToUpper());
                    rm.spritebatch.DrawString(Global.DefaultFont, "" + Localizer.Get(mMenuStr[mmenu_select / 10][i]).ToUpper(), new Vector2((rtSnake.Width / 2) - (sz.X / 2) + (((menuShiftPos.X * 2) + 2) * xscale), (rtSnake.Height / 2) - (sz.Y / 2) + (yscale * (i + menuShiftPos.Y - (mmenu_select % 10) + 1))), new Color(mMenuCol[mmenu_select / 10][i].R, mMenuCol[mmenu_select / 10][i].G, mMenuCol[mmenu_select / 10][i].B, (byte)(255 * (1 - menuShiftPos.X))));
                }
            }
            rm.spritebatch.End();
            rm.graphics.GraphicsDevice.SetRenderTarget(0, rtSnakeBM);
            texSnake = rtSnake.GetTexture();
            rm.graphics.GraphicsDevice.Clear(new Color(128,128,255));
            rm.spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);
            //29.589 69.727
            for (int i = -1; i < 5; i++)
                for (int k = -1; k < 4; k++)
                {
                    rm.spritebatch.Draw(texSnakeSkinBM, new Rectangle((int)(((menuShiftPos.X * 2) * rotxscale) + (i * xscale)), (int)((menuShiftPos.Y * rotyscale) + (k * yscale)), (int)(xscale), (int)(yscale)), Color.White);
                }
            if (Math.Abs(menuShiftPos.X) <= 0.004)
            {
                if (mmenu_select % 10 == 0)
                {
                    for (int i = 0; i < mMenuStr[0].Length; i++)
                    {
                        Vector2 sz = Global.DefaultFont.MeasureString("" + Localizer.Get(mMenuStr[0][i]).ToUpper());
                        rm.spritebatch.DrawString(Global.DefaultFont, "" + Localizer.Get(mMenuStr[0][i]).ToUpper(), new Vector2((rtSnake.Width / 2) - (sz.X / 2), (rtSnake.Height / 2) - (sz.Y / 2) + (yscale * (i + menuShiftPos.Y - (mmenu_select / 10) + 1))), new Color(128, 128, 255));
                    }
                }
                else
                {
                    for (int i = 0; i < mMenuStr[mmenu_select / 10].Length; i++)
                    {
                        Vector2 sz = Global.DefaultFont.MeasureString("" + Localizer.Get(mMenuStr[mmenu_select / 10][i]).ToUpper());
                        rm.spritebatch.DrawString(Global.DefaultFont, "" + Localizer.Get(mMenuStr[mmenu_select / 10][i]).ToUpper(), new Vector2((rtSnake.Width / 2) - (sz.X / 2), (rtSnake.Height / 2) - (sz.Y / 2) + (yscale * (i + menuShiftPos.Y - (mmenu_select % 10) + 1))), new Color(128, 128, 255));
                    }
                }
            }
            else if (menuShiftPos.X > 0)
            {
                for (int i = 0; i < mMenuStr[0].Length; i++)
                {
                    Vector2 sz = Global.DefaultFont.MeasureString("" + Localizer.Get(mMenuStr[0][i]).ToUpper());
                    rm.spritebatch.DrawString(Global.DefaultFont, "" + Localizer.Get(mMenuStr[0][i]).ToUpper(), new Vector2((rtSnake.Width / 2) - (sz.X / 2) + (((menuShiftPos.X * 2) - 2) * xscale), (rtSnake.Height / 2) - (sz.Y / 2) + (yscale * (i + menuShiftPos.Y - (mmenu_select / 10) + 1))), new Color(128, 128, 255, (byte)(255 * menuShiftPos.X)));
                }
                for (int i = 0; i < mMenuStr[mmenu_select / 10].Length; i++)
                {
                    Vector2 sz = Global.DefaultFont.MeasureString("" + Localizer.Get(mMenuStr[mmenu_select / 10][i]).ToUpper());
                    rm.spritebatch.DrawString(Global.DefaultFont, "" + Localizer.Get(mMenuStr[mmenu_select / 10][i]).ToUpper(), new Vector2((rtSnake.Width / 2) - (sz.X / 2) + ((menuShiftPos.X * 2) * xscale), (rtSnake.Height / 2) - (sz.Y / 2) + (yscale * (i + menuShiftPos.Y - (mmenu_select % 10) + 1))), new Color(128, 128, 255, (byte)(255 * (1 - menuShiftPos.X))));
                }

            }
            else if (menuShiftPos.X < 0)
            {
                for (int i = 0; i < mMenuStr[0].Length; i++)
                {
                    Vector2 sz = Global.DefaultFont.MeasureString("" + Localizer.Get(mMenuStr[0][i]).ToUpper());
                    rm.spritebatch.DrawString(Global.DefaultFont, "" + Localizer.Get(mMenuStr[0][i]).ToUpper(), new Vector2((rtSnake.Width / 2) - (sz.X / 2) + (((menuShiftPos.X * 2)) * xscale), (rtSnake.Height / 2) - (sz.Y / 2) + (yscale * (i + menuShiftPos.Y - (mmenu_select / 10) + 1))), new Color(128, 128, 255, (byte)(255 * menuShiftPos.X)));
                }
                for (int i = 0; i < mMenuStr[mmenu_select / 10].Length; i++)
                {
                    Vector2 sz = Global.DefaultFont.MeasureString("" + Localizer.Get(mMenuStr[mmenu_select / 10][i]).ToUpper());
                    rm.spritebatch.DrawString(Global.DefaultFont, "" + Localizer.Get(mMenuStr[mmenu_select / 10][i]).ToUpper(), new Vector2((rtSnake.Width / 2) - (sz.X / 2) + (((menuShiftPos.X * 2) + 2) * xscale), (rtSnake.Height / 2) - (sz.Y / 2) + (yscale * (i + menuShiftPos.Y - (mmenu_select % 10) + 1))), new Color(128, 128, 255, (byte)(255 * (1 - menuShiftPos.X))));
                }
            }
            rm.spritebatch.End();
            rm.graphics.GraphicsDevice.SetRenderTarget(0, null);
            texSnakeBM = rtSnakeBM.GetTexture();
#if !DEBUG
                    }
                    catch(Exception e)
                    {
#if WINDOWS
                        System.Windows.Forms.MessageBox.Show("Problem in Draw/MM/Pt0\n"+e.Message+"\n"+e.StackTrace);
#endif
                        UnsignedGame.GetSingleton().Exit();
                        return;
                    }
                    try
                    {
#endif
            rm.graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
            rm.graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
            //graphics.PreferMultiSampling = true;
            rm.graphics.ApplyChanges();

            rm.graphics.GraphicsDevice.Clear(new Color(0, 0, 30, 255));
            //graphics.GraphicsDevice.
            effect.AmbientMaterial = new Color(24, 24, 24);
            effect.DiffuseMaterial = new Color(200, 200, 200);
            effect.SpecularMaterial = Color.White;
            rm.graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;
            rm.graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
            rm.graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;

            effect.LightingEnabled = GameSettings.Lighting;
            effect.SpecularEnabled = GameSettings.Specular;
            effect.NormalMapEnabled = GameSettings.NormalMapping;

            Random r = new Random();

            effect.DirectionalLight = new DirectionalLight(true,new Vector3(0f, 0f, 1f),new Color(200, 200, 200),Color.White);

            Version SM = rm.graphics.GraphicsDevice.GraphicsDeviceCapabilities.PixelShaderVersion;
            if(SM.Major<1 || (SM.Major==1 && SM.Minor<1))
            {
#if WINDOWS
                System.Windows.Forms.MessageBox.Show("Whoops! Your graphics card only supports Shader Model " + SM.Major + "." + SM.Minor + "\nYou need at least 1.1 to run Unsigned");
#endif
                UnsignedGame.GetSingleton().Exit();
            }
            effect.CommitChanges();
#if !DEBUG
                    }
                    catch(Exception e)
                    {
#if WINDOWS
                        System.Windows.Forms.MessageBox.Show("Problem in Draw/MM/Pt1\n"+e.Message+"\n"+e.StackTrace);
#endif
                        UnsignedGame.GetSingleton().Exit();
                        return;
                    }

                    try
                    {
#endif
            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();
                Matrix matProj = Matrix.CreatePerspectiveFieldOfView((float)Math.PI / 4.0f,
                  GameSettings.windowwidth / (float)GameSettings.windowheight,
                  10f, 80.0f);

                Matrix matView = Matrix.CreateLookAt(new Vector3(0, 0, 32), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
                //render the background graphics
                rm.View = matView;
                effect.Projection = matProj;

                Matrix matRot, matScale, matTranslate;
                {//basebottom
                    matTranslate = Matrix.CreateTranslation(0, 0, 0);
                    matRot = Matrix.Identity;// Matrix.CreateRotationX((float)Math.PI);
                    matScale = Matrix.CreateScale(2, 2, 2);

                    effect.World = matScale * matRot * matTranslate;
                    effect.TextureEnabled = true;
                    effect.SpecularMaterial = new Color(150,255,128);
                    effect.DiffuseTexture = texSnake;
                    effect.NormalMapTexture = texSnakeBM;
                    effect.Shininess = 1f;
                    effect.CommitChanges();

                    rm.graphics.GraphicsDevice.VertexDeclaration = GBVertexFormat.VertexDeclaration;
                    foreach (ModelMesh mesh in mSnake.Meshes)
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
                pass.End();
            }
            effect.End();



            rm.spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
            rm.spritebatch.Draw(waves, new Rectangle((int)waveM1, GameSettings.windowheight * 6 / 8, GameSettings.windowwidth, GameSettings.windowheight * 2 / 8), Color.White);
            rm.spritebatch.Draw(waves, new Rectangle(-GameSettings.windowwidth + (int)waveM1, GameSettings.windowheight * 6 / 8, GameSettings.windowwidth, GameSettings.windowheight * 2 / 8), Color.White);
            rm.spritebatch.Draw(waves, new Rectangle((int)waveM2, GameSettings.windowheight * 7 / 8, GameSettings.windowwidth, GameSettings.windowheight * 2 / 8), Color.White);
            rm.spritebatch.Draw(waves, new Rectangle(-GameSettings.windowwidth + (int)waveM2, GameSettings.windowheight * 7 / 8, GameSettings.windowwidth, GameSettings.windowheight * 2 / 8), Color.White);
            rm.spritebatch.Draw(songchoosetop, new Rectangle(0, 0, GameSettings.windowwidth, (int)(GameSettings.windowwidth * 0.25f)), Color.White);
            rm.spritebatch.Draw(songchoosetop, new Rectangle(0, (int)(GameSettings.windowheight-(GameSettings.windowwidth*0.25f)), GameSettings.windowwidth, (int)(GameSettings.windowwidth * 0.25f)), null, Color.White, 0, new Vector2(0, 0), SpriteEffects.FlipVertically, 0);
            rm.spritebatch.Draw(GameUIMaster.GetSingleton().texButtonGreen, new Rectangle((int)(0.1f * GameSettings.windowwidth), (int)(0.90f * GameSettings.windowheight), (int)(0.09f * GameSettings.windowheight), (int)(0.09f * GameSettings.windowheight)), Color.White);
            rm.spritebatch.DrawString(Global.DefaultFont, Localizer.Get("Select"), new Vector2((0.1f * GameSettings.windowwidth) + (0.10f * GameSettings.windowheight), (0.90f * GameSettings.windowheight) + (((0.09f * GameSettings.windowheight) - (Global.DefaultFont.MeasureString(Localizer.Get("Select")).Y)) / 2)), Color.Black);
            if (Global.DemoMode)
            {
                rm.spritebatch.DrawString(Global.BigFont, Localizer.Get(Localizer.Get("Demo Mode")), new Vector2((GameSettings.windowwidth / 2) - (Global.BigFont.MeasureString(Localizer.Get(Localizer.Get("Demo Mode"))).X / 2), GameSettings.windowheight * 0.15f), new Color(255, 0, 0, 64));
                rm.spritebatch.DrawString(Global.BigFont, Localizer.Get(Localizer.Get("Demo Mode")), new Vector2((GameSettings.windowwidth / 2) - (Global.BigFont.MeasureString(Localizer.Get(Localizer.Get("Demo Mode"))).X / 2), GameSettings.windowheight * 0.4f), new Color(255, 0, 0, 64));
                rm.spritebatch.DrawString(Global.BigFont, Localizer.Get(Localizer.Get("Demo Mode")), new Vector2((GameSettings.windowwidth / 2) - (Global.BigFont.MeasureString(Localizer.Get(Localizer.Get("Demo Mode"))).X / 2), GameSettings.windowheight * 0.65f), new Color(255, 0, 0, 64));
            }
            float txtScale = (float)(GameSettings.windowwidth-20) / Global.DefaultFont.MeasureString(Localizer.Get("Menus in Red are not yet implemented")).X;
            //rm.spritebatch.DrawString(Global.DefaultFont, Localizer.Get("Menus in Red are not yet implemented"), new Vector2(10, GameSettings.windowheight * 0.7f), Color.White, 0, new Vector2(0, 0), new Vector2(txtScale, 1),SpriteEffects.None,0);

            if (mmLogoTime < 1)
                rm.spritebatch.Draw(Global.texWhite, new Rectangle(0, 0, GameSettings.windowwidth, GameSettings.windowheight), Color.Black);
            else if (mmLogoTime < 2)
                rm.spritebatch.Draw(Global.texWhite, new Rectangle(0, 0, GameSettings.windowwidth, GameSettings.windowheight), new Color(0, 0, 0, (byte)(255 * (1 - (mmLogoTime - 1)))));
            if (mmLogoTime < 1)
                rm.spritebatch.Draw(texHeader, (new Rectangle((int)(GameSettings.windowwidth * -0.2f * (mmLogoTime)) + (int)((1 - mmLogoTime) * GameSettings.windowwidth * 0.4f), (int)(GameSettings.windowheight * .1f * (mmLogoTime)) + (int)((1 - mmLogoTime) * GameSettings.windowheight * -0.3f), (int)(GameSettings.windowwidth * 1.4f * (mmLogoTime)) + (int)((1 - mmLogoTime) * GameSettings.windowwidth * 0.2f), (int)(GameSettings.windowheight * 0.4f * (mmLogoTime)))), Color.White);
            else if (mmLogoTime < 2)
                rm.spritebatch.Draw(texHeader, (new Rectangle((int)((mmLogoTime - 1) * GameSettings.windowwidth * 0.1f) + (int)((1 - (mmLogoTime - 1)) * -GameSettings.windowwidth * 0.2f), (int)((mmLogoTime - 1) * GameSettings.windowheight * 0.2f) + (int)((1 - (mmLogoTime - 1)) * GameSettings.windowheight * 0.1f), (int)((mmLogoTime - 1) * GameSettings.windowwidth * 0.8f) + (int)((1 - (mmLogoTime - 1)) * GameSettings.windowwidth * 1.4f), (int)((mmLogoTime - 1) * GameSettings.windowheight * 0.2f) + (int)((1 - (mmLogoTime - 1)) * GameSettings.windowheight * .4f))), Color.White);
            else
                rm.spritebatch.Draw(texHeader, new Rectangle((int)(GameSettings.windowwidth * 0.1f), (int)(GameSettings.windowheight * 0.2f), (int)(GameSettings.windowwidth * 0.8f), (int)(GameSettings.windowheight * .2f)), Color.White);

            rm.spritebatch.End();
#if !DEBUG
                    }
                    catch(Exception e)
                    {
#if WINDOWS
                        System.Windows.Forms.MessageBox.Show("Problem in Draw/MM/Pt2\n"+e.Message+"\n"+e.StackTrace);
#endif
                        UnsignedGame.GetSingleton().Exit();
                        return;
                    }
#endif
            
        }
    }
}

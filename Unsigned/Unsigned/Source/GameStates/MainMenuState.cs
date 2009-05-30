using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UnsignedPeripheralPlugins;
using FVProductions.Utility;

namespace Unsigned
{
    class MainMenuScreen : BaseState
    {
        private ContentManager Content;

        private SpriteBatch spriteBatch;

        private FVShader effect;

        private float waveM1, waveM2;
        private int mmenu_select = 0, menu_ticker = 0;
        private float mmLogoTime;
        private int menuSnakeRotOffset;
        private FVModel mSnake;
        private Texture2D texSnake, texSnakeBM, texSnakeSpecular, texSnakeSkinBM, texSnakeSkin;
        private RenderTarget2D rtSnake, rtSnakeBM, rtSnakeSpecular;
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
            mMenuCol[0][0] = Color.Black;
            mMenuCol[0][1] = Color.Black;
            mMenuCol[0][2] = Color.Black;
        }

        public override void Load()
        {
            Content = new ContentManager(Global.Services);
            Content.RootDirectory = "Content";

            spriteBatch = new SpriteBatch(Global.Graphics.GraphicsDevice);

            effect = new FVShader(Global.Graphics.GraphicsDevice, Content.Load<Effect>("shaders\\UnsignedEngineShader"), "maintechnique");

            texSnakeSkin = Content.Load<Texture2D>("textures\\MainMenu\\snake");
            texSnakeSkinBM = Content.Load<Texture2D>("textures\\MainMenu\\snakeBM");
            mSnake = ModelLoader.LoadModel("meshes\\MainMenu\\snake");
            rtSnake = new RenderTarget2D(Global.Graphics.GraphicsDevice, 512, 128, 1, SurfaceFormat.Color);
            rtSnakeBM = new RenderTarget2D(Global.Graphics.GraphicsDevice, 512, 128, 1, SurfaceFormat.Color);
            rtSnakeSpecular = new RenderTarget2D(Global.Graphics.GraphicsDevice, 512, 128, 1, SurfaceFormat.Color);
            waves = Content.Load<Texture2D>("textures\\MainMenu\\waves");
            wave = Content.Load<Texture2D>("textures\\MainMenu\\wave");
            songchoosetop = Content.Load<Texture2D>("textures\\MainMenu\\barbedtop");
            texHeader = Content.Load<Texture2D>("textures\\MainMenu\\header");
        }

        public override void Unload()
        {
            Content.Unload();
        }

        public override void Update(GameTime gameTime)
        {
            try
            {
                waveM1 += gameTime.ElapsedGameTime.Milliseconds / 50f;
                waveM2 -= gameTime.ElapsedGameTime.Milliseconds / 25f;
                if (waveM1 >= Global.ScreenWidth)
                    waveM1 -= Global.ScreenWidth;
                if (waveM2 <= 0)
                    waveM2 += Global.ScreenWidth;
                if (menu_ticker <= 0)
                {
                    if (mmLogoTime > 2)
                    {
                        int collective = 0;
                        bool green = false, red = false;
                        PeripheralManager.Singleton.ReloadDLLs();
                        PeripheralManager.Singleton.CheckConnections();
                        PeripheralManager.Singleton.QueryAll();
                        Peripheral[] controllers = PeripheralManager.Singleton.GetPeripherals();
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
                                UnsignedGame.Singleton.SwitchState(new ControllerSetupScreen());
                            if (green && mmenu_select == 20)
                                UnsignedGame.Singleton.SwitchState(new OptionsScreen());
                            if (green && mmenu_select == 30)
                                UnsignedGame.Singleton.Exit();
                        }
                        if (collective != 0 || green || red)
                            menu_ticker = 200;
                    }
                    else
                    {
#if DEBUG
                        mmLogoTime += (float)gameTime.ElapsedGameTime.TotalSeconds*100;
#else
                        mmLogoTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
#endif
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

            }
            catch(Exception e)
            {
                Debug.Error("Problem in MainMenuState.Update", e);
                UnsignedGame.Singleton.Exit();
                return;
            }
        }

        public override void Render(GameTime gameTime)
        {
            Global.Graphics.GraphicsDevice.Clear(Color.Black);

            try
            {
                Color txt = Color.Black;

                Global.Graphics.GraphicsDevice.SetRenderTarget(0, rtSnake);
                Global.Graphics.GraphicsDevice.Clear(Color.Red);
                spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
                //29.589 69.727
                float xscale = 128, yscale = 48;
                float rotxscale = 128, rotyscale = 48;
                for (int i = -1; i < 5; i++)
                    for (int k = -1; k < 4; k++)
                    {
                        spriteBatch.Draw(texSnakeSkin, new Rectangle((int)(((menuShiftPos.X * 2) * rotxscale) + (i * xscale)), (int)((menuShiftPos.Y * rotyscale) + (k * yscale)), (int)(xscale), (int)(yscale)), Color.White);
                    }
                if (Math.Abs(menuShiftPos.X) <= 0.004)
                {
                    if (mmenu_select % 10 == 0)
                    {
                        for (int i = 0; i < mMenuStr[0].Length; i++)
                        {
                            Vector2 sz = Global.DefaultFont.MeasureString("" + Localizer.Get(mMenuStr[0][i]).ToUpper());
                            spriteBatch.DrawString(Global.DefaultFont, "" + Localizer.Get(mMenuStr[0][i]).ToUpper(), new Vector2((rtSnake.Width / 2) - (sz.X / 2), (rtSnake.Height / 2) - (sz.Y / 2) + (yscale * (i + menuShiftPos.Y - (mmenu_select / 10) + 1))), mMenuCol[0][i]);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < mMenuStr[mmenu_select / 10].Length; i++)
                        {
                            Vector2 sz = Global.DefaultFont.MeasureString("" + Localizer.Get(mMenuStr[mmenu_select / 10][i]).ToUpper());
                            spriteBatch.DrawString(Global.DefaultFont, "" + Localizer.Get(mMenuStr[mmenu_select / 10][i]).ToUpper(), new Vector2((rtSnake.Width / 2) - (sz.X / 2), (rtSnake.Height / 2) - (sz.Y / 2) + (yscale * (i + menuShiftPos.Y - (mmenu_select % 10) + 1))), mMenuCol[mmenu_select / 10][i]);
                        }
                    }
                }
                else if (menuShiftPos.X > 0)
                {
                    for (int i = 0; i < mMenuStr[0].Length; i++)
                    {
                        Vector2 sz = Global.DefaultFont.MeasureString("" + Localizer.Get(mMenuStr[0][i]).ToUpper());
                        spriteBatch.DrawString(Global.DefaultFont, "" + Localizer.Get(mMenuStr[0][i]).ToUpper(), new Vector2((rtSnake.Width / 2) - (sz.X / 2) + (((menuShiftPos.X * 2) - 2) * xscale), (rtSnake.Height / 2) - (sz.Y / 2) + (yscale * (i + menuShiftPos.Y - (mmenu_select / 10) + 1))), new Color(mMenuCol[0][i].R, mMenuCol[0][i].G, mMenuCol[0][i].B, (byte)(255 * menuShiftPos.X)));
                    }
                    for (int i = 0; i < mMenuStr[mmenu_select / 10].Length; i++)
                    {
                        Vector2 sz = Global.DefaultFont.MeasureString("" + Localizer.Get(mMenuStr[mmenu_select / 10][i]).ToUpper());
                        spriteBatch.DrawString(Global.DefaultFont, "" + Localizer.Get(mMenuStr[mmenu_select / 10][i]).ToUpper(), new Vector2((rtSnake.Width / 2) - (sz.X / 2) + ((menuShiftPos.X * 2) * xscale), (rtSnake.Height / 2) - (sz.Y / 2) + (yscale * (i + menuShiftPos.Y - (mmenu_select % 10) + 1))), new Color(mMenuCol[mmenu_select / 10][i].R, mMenuCol[mmenu_select / 10][i].G, mMenuCol[mmenu_select / 10][i].B, (byte)(255 * (1 - menuShiftPos.X))));
                    }
                    
                }
                else if (menuShiftPos.X < 0)
                {
                    for (int i = 0; i < mMenuStr[0].Length; i++)
                    {
                        Vector2 sz = Global.DefaultFont.MeasureString("" + Localizer.Get(mMenuStr[0][i]).ToUpper());
                        spriteBatch.DrawString(Global.DefaultFont, "" + Localizer.Get(mMenuStr[0][i]).ToUpper(), new Vector2((rtSnake.Width / 2) - (sz.X / 2) + (((menuShiftPos.X * 2)) * xscale), (rtSnake.Height / 2) - (sz.Y / 2) + (yscale * (i + menuShiftPos.Y - (mmenu_select / 10) + 1))), new Color(mMenuCol[0][i].R, mMenuCol[0][i].G, mMenuCol[0][i].B, (byte)(255 * menuShiftPos.X)));
                    }
                    for (int i = 0; i < mMenuStr[mmenu_select / 10].Length; i++)
                    {
                        Vector2 sz = Global.DefaultFont.MeasureString("" + Localizer.Get(mMenuStr[mmenu_select / 10][i]).ToUpper());
                        spriteBatch.DrawString(Global.DefaultFont, "" + Localizer.Get(mMenuStr[mmenu_select / 10][i]).ToUpper(), new Vector2((rtSnake.Width / 2) - (sz.X / 2) + (((menuShiftPos.X * 2) + 2) * xscale), (rtSnake.Height / 2) - (sz.Y / 2) + (yscale * (i + menuShiftPos.Y - (mmenu_select % 10) + 1))), new Color(mMenuCol[mmenu_select / 10][i].R, mMenuCol[mmenu_select / 10][i].G, mMenuCol[mmenu_select / 10][i].B, (byte)(255 * (1 - menuShiftPos.X))));
                    }
                }
                spriteBatch.End();
                Global.Graphics.GraphicsDevice.SetRenderTarget(0, rtSnakeBM);
                texSnake = rtSnake.GetTexture();
                Global.Graphics.GraphicsDevice.Clear(new Color(128,128,255));
                spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
                //29.589 69.727
                for (int i = -1; i < 5; i++)
                    for (int k = -1; k < 4; k++)
                    {
                        spriteBatch.Draw(texSnakeSkinBM, new Rectangle((int)(((menuShiftPos.X * 2) * rotxscale) + (i * xscale)), (int)((menuShiftPos.Y * rotyscale) + (k * yscale)), (int)(xscale), (int)(yscale)), Color.White);
                    }
                if (Math.Abs(menuShiftPos.X) <= 0.004)
                {
                    if (mmenu_select % 10 == 0)
                    {
                        for (int i = 0; i < mMenuStr[0].Length; i++)
                        {
                            Vector2 sz = Global.DefaultFont.MeasureString("" + Localizer.Get(mMenuStr[0][i]).ToUpper());
                            spriteBatch.DrawString(Global.DefaultFont, "" + Localizer.Get(mMenuStr[0][i]).ToUpper(), new Vector2((rtSnake.Width / 2) - (sz.X / 2), (rtSnake.Height / 2) - (sz.Y / 2) + (yscale * (i + menuShiftPos.Y - (mmenu_select / 10) + 1))), new Color(128, 128, 255));
                        }
                    }
                    else
                    {
                        for (int i = 0; i < mMenuStr[mmenu_select / 10].Length; i++)
                        {
                            Vector2 sz = Global.DefaultFont.MeasureString("" + Localizer.Get(mMenuStr[mmenu_select / 10][i]).ToUpper());
                            spriteBatch.DrawString(Global.DefaultFont, "" + Localizer.Get(mMenuStr[mmenu_select / 10][i]).ToUpper(), new Vector2((rtSnake.Width / 2) - (sz.X / 2), (rtSnake.Height / 2) - (sz.Y / 2) + (yscale * (i + menuShiftPos.Y - (mmenu_select % 10) + 1))), new Color(128, 128, 255));
                        }
                    }
                }
                else if (menuShiftPos.X > 0)
                {
                    for (int i = 0; i < mMenuStr[0].Length; i++)
                    {
                        Vector2 sz = Global.DefaultFont.MeasureString("" + Localizer.Get(mMenuStr[0][i]).ToUpper());
                        spriteBatch.DrawString(Global.DefaultFont, "" + Localizer.Get(mMenuStr[0][i]).ToUpper(), new Vector2((rtSnake.Width / 2) - (sz.X / 2) + (((menuShiftPos.X * 2) - 2) * xscale), (rtSnake.Height / 2) - (sz.Y / 2) + (yscale * (i + menuShiftPos.Y - (mmenu_select / 10) + 1))), new Color(128, 128, 255, (byte)(255 * menuShiftPos.X)));
                    }
                    for (int i = 0; i < mMenuStr[mmenu_select / 10].Length; i++)
                    {
                        Vector2 sz = Global.DefaultFont.MeasureString("" + Localizer.Get(mMenuStr[mmenu_select / 10][i]).ToUpper());
                        spriteBatch.DrawString(Global.DefaultFont, "" + Localizer.Get(mMenuStr[mmenu_select / 10][i]).ToUpper(), new Vector2((rtSnake.Width / 2) - (sz.X / 2) + ((menuShiftPos.X * 2) * xscale), (rtSnake.Height / 2) - (sz.Y / 2) + (yscale * (i + menuShiftPos.Y - (mmenu_select % 10) + 1))), new Color(128, 128, 255, (byte)(255 * (1 - menuShiftPos.X))));
                    }

                }
                else if (menuShiftPos.X < 0)
                {
                    for (int i = 0; i < mMenuStr[0].Length; i++)
                    {
                        Vector2 sz = Global.DefaultFont.MeasureString("" + Localizer.Get(mMenuStr[0][i]).ToUpper());
                        spriteBatch.DrawString(Global.DefaultFont, "" + Localizer.Get(mMenuStr[0][i]).ToUpper(), new Vector2((rtSnake.Width / 2) - (sz.X / 2) + (((menuShiftPos.X * 2)) * xscale), (rtSnake.Height / 2) - (sz.Y / 2) + (yscale * (i + menuShiftPos.Y - (mmenu_select / 10) + 1))), new Color(128, 128, 255, (byte)(255 * menuShiftPos.X)));
                    }
                    for (int i = 0; i < mMenuStr[mmenu_select / 10].Length; i++)
                    {
                        Vector2 sz = Global.DefaultFont.MeasureString("" + Localizer.Get(mMenuStr[mmenu_select / 10][i]).ToUpper());
                        spriteBatch.DrawString(Global.DefaultFont, "" + Localizer.Get(mMenuStr[mmenu_select / 10][i]).ToUpper(), new Vector2((rtSnake.Width / 2) - (sz.X / 2) + (((menuShiftPos.X * 2) + 2) * xscale), (rtSnake.Height / 2) - (sz.Y / 2) + (yscale * (i + menuShiftPos.Y - (mmenu_select % 10) + 1))), new Color(128, 128, 255, (byte)(255 * (1 - menuShiftPos.X))));
                    }
                }
                spriteBatch.End();
                Global.Graphics.GraphicsDevice.SetRenderTarget(0, rtSnakeSpecular);
                texSnakeBM = rtSnakeBM.GetTexture();
                Global.Graphics.GraphicsDevice.Clear(new Color(255, 255, 255));
                spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
                if (Math.Abs(menuShiftPos.X) <= 0.004)
                {
                    if (mmenu_select % 10 == 0)
                    {
                        for (int i = 0; i < mMenuStr[0].Length; i++)
                        {
                            Vector2 sz = Global.DefaultFont.MeasureString("" + Localizer.Get(mMenuStr[0][i]).ToUpper());
                            spriteBatch.DrawString(Global.DefaultFont, "" + Localizer.Get(mMenuStr[0][i]).ToUpper(), new Vector2((rtSnake.Width / 2) - (sz.X / 2), (rtSnake.Height / 2) - (sz.Y / 2) + (yscale * (i + menuShiftPos.Y - (mmenu_select / 10) + 1))), Color.Black);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < mMenuStr[mmenu_select / 10].Length; i++)
                        {
                            Vector2 sz = Global.DefaultFont.MeasureString("" + Localizer.Get(mMenuStr[mmenu_select / 10][i]).ToUpper());
                            spriteBatch.DrawString(Global.DefaultFont, "" + Localizer.Get(mMenuStr[mmenu_select / 10][i]).ToUpper(), new Vector2((rtSnake.Width / 2) - (sz.X / 2), (rtSnake.Height / 2) - (sz.Y / 2) + (yscale * (i + menuShiftPos.Y - (mmenu_select % 10) + 1))), Color.Black);
                        }
                    }
                }
                else if (menuShiftPos.X > 0)
                {
                    for (int i = 0; i < mMenuStr[0].Length; i++)
                    {
                        Vector2 sz = Global.DefaultFont.MeasureString("" + Localizer.Get(mMenuStr[0][i]).ToUpper());
                        spriteBatch.DrawString(Global.DefaultFont, "" + Localizer.Get(mMenuStr[0][i]).ToUpper(), new Vector2((rtSnake.Width / 2) - (sz.X / 2) + (((menuShiftPos.X * 2) - 2) * xscale), (rtSnake.Height / 2) - (sz.Y / 2) + (yscale * (i + menuShiftPos.Y - (mmenu_select / 10) + 1))), new Color(0, 0, 0, (byte)(255 * menuShiftPos.X)));
                    }
                    for (int i = 0; i < mMenuStr[mmenu_select / 10].Length; i++)
                    {
                        Vector2 sz = Global.DefaultFont.MeasureString("" + Localizer.Get(mMenuStr[mmenu_select / 10][i]).ToUpper());
                        spriteBatch.DrawString(Global.DefaultFont, "" + Localizer.Get(mMenuStr[mmenu_select / 10][i]).ToUpper(), new Vector2((rtSnake.Width / 2) - (sz.X / 2) + ((menuShiftPos.X * 2) * xscale), (rtSnake.Height / 2) - (sz.Y / 2) + (yscale * (i + menuShiftPos.Y - (mmenu_select % 10) + 1))), new Color(0, 0, 0, (byte)(255 * (1 - menuShiftPos.X))));
                    }

                }
                else if (menuShiftPos.X < 0)
                {
                    for (int i = 0; i < mMenuStr[0].Length; i++)
                    {
                        Vector2 sz = Global.DefaultFont.MeasureString("" + Localizer.Get(mMenuStr[0][i]).ToUpper());
                        spriteBatch.DrawString(Global.DefaultFont, "" + Localizer.Get(mMenuStr[0][i]).ToUpper(), new Vector2((rtSnake.Width / 2) - (sz.X / 2) + (((menuShiftPos.X * 2)) * xscale), (rtSnake.Height / 2) - (sz.Y / 2) + (yscale * (i + menuShiftPos.Y - (mmenu_select / 10) + 1))), new Color(0, 0, 0, (byte)(255 * menuShiftPos.X)));
                    }
                    for (int i = 0; i < mMenuStr[mmenu_select / 10].Length; i++)
                    {
                        Vector2 sz = Global.DefaultFont.MeasureString("" + Localizer.Get(mMenuStr[mmenu_select / 10][i]).ToUpper());
                        spriteBatch.DrawString(Global.DefaultFont, "" + Localizer.Get(mMenuStr[mmenu_select / 10][i]).ToUpper(), new Vector2((rtSnake.Width / 2) - (sz.X / 2) + (((menuShiftPos.X * 2) + 2) * xscale), (rtSnake.Height / 2) - (sz.Y / 2) + (yscale * (i + menuShiftPos.Y - (mmenu_select % 10) + 1))), new Color(0, 0, 0, (byte)(255 * (1 - menuShiftPos.X))));
                    }
                }
                spriteBatch.End();
                Global.Graphics.GraphicsDevice.SetRenderTarget(0, null);
                texSnakeSpecular = rtSnakeSpecular.GetTexture();
            }
            catch(Exception e)
            {
                Debug.Error("Problem in MainMenuState.Draw[1]", e);
                UnsignedGame.Singleton.Exit();
                return;
            }
            try
            {
                Global.Graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
                Global.Graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
                Global.Graphics.ApplyChanges();

                Global.Graphics.GraphicsDevice.Clear(new Color(0, 0, 30, 255));
                effect.AmbientMaterial = new Color(24, 24, 24);
                effect.DiffuseMaterial = new Color(200, 200, 200);
                effect.SpecularMaterial = Color.White;
                Global.Graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;
                Global.Graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
                Global.Graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;

                effect.LightingEnabled = Configuration.Lighting;
                effect.SpecularEnabled = Configuration.Specular;
                effect.NormalMapEnabled = Configuration.NormalMapping;

                Random r = new Random();

                effect.DirectionalLight = new DirectionalLight(true,new Vector3(0f, 0f, 1f),new Color(200, 200, 200),Color.White);
                effect.TextureEnabled = true;
                effect.CommitChanges();

                effect.Begin();
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Begin();

                    effect.View = Matrix.CreateLookAt(new Vector3(0, 0, 32), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
                    effect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, Global.ScreenWidth / (float)Global.ScreenHeight, 1f, 1000f);

                    Matrix matRot, matScale, matTranslate;
                    {//basebottom
                        matTranslate = Matrix.CreateTranslation(0, 0, 0);
                        matRot = Matrix.Identity;
                        matScale = Matrix.CreateScale(2, 2, 2);

                        effect.World = matScale * matRot * matTranslate;
                        effect.SpecularMaterial = new Color(202,255,191);
                        effect.SpecularMapTexture = texSnakeSpecular;
                        effect.DiffuseTexture = texSnake;
                        effect.NormalMapTexture = texSnakeBM;
                        effect.Shininess = 10f;
                        effect.CommitChanges();

                        mSnake.Draw();
                    }
                    pass.End();
                }
                effect.End();
                effect.SpecularMapTexture = Global.TexWhite;



                spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.None);
                spriteBatch.Draw(waves, new Rectangle((int)waveM1, Global.ScreenHeight * 6 / 8, Global.ScreenWidth, Global.ScreenHeight * 2 / 8), Color.White);
                spriteBatch.Draw(waves, new Rectangle(-Global.ScreenWidth + (int)waveM1, Global.ScreenHeight * 6 / 8, Global.ScreenWidth, Global.ScreenHeight * 2 / 8), Color.White);
                spriteBatch.Draw(waves, new Rectangle((int)waveM2, Global.ScreenHeight * 7 / 8, Global.ScreenWidth, Global.ScreenHeight * 2 / 8), Color.White);
                spriteBatch.Draw(waves, new Rectangle(-Global.ScreenWidth + (int)waveM2, Global.ScreenHeight * 7 / 8, Global.ScreenWidth, Global.ScreenHeight * 2 / 8), Color.White);
                spriteBatch.Draw(songchoosetop, new Rectangle(0, 0, Global.ScreenWidth, (int)(Global.ScreenWidth * 0.25f)), Color.White);
                spriteBatch.Draw(songchoosetop, new Rectangle(0, (int)(Global.ScreenHeight-(Global.ScreenWidth*0.25f)), Global.ScreenWidth, (int)(Global.ScreenWidth * 0.25f)), null, Color.White, 0, new Vector2(0, 0), SpriteEffects.FlipVertically, 0);
                /*
                spriteBatch.Draw(GameUIMaster.Singleton.texButtonGreen, new Rectangle((int)(0.1f * Global.ScreenWidth), (int)(0.90f * Global.ScreenHeight), (int)(0.09f * Global.ScreenHeight), (int)(0.09f * Global.ScreenHeight)), Color.White);
                spriteBatch.DrawString(Global.DefaultFont, Localizer.Get("Select"), new Vector2((0.1f * Global.ScreenWidth) + (0.10f * Global.ScreenHeight), (0.90f * Global.ScreenHeight) + (((0.09f * Global.ScreenHeight) - (Global.DefaultFont.MeasureString(Localizer.Get("Select")).Y)) / 2)), Color.Black);
                */

                if (mmLogoTime < 1)
                    spriteBatch.Draw(Global.TexWhite, new Rectangle(0, 0, Global.ScreenWidth, Global.ScreenHeight), Color.Black);
                else if (mmLogoTime < 2)
                    spriteBatch.Draw(Global.TexWhite, new Rectangle(0, 0, Global.ScreenWidth, Global.ScreenHeight), new Color(0, 0, 0, (byte)(255 * (1 - (mmLogoTime - 1)))));
                if (mmLogoTime < 1)
                    spriteBatch.Draw(texHeader, (new Rectangle((int)(Global.ScreenWidth * -0.2f * (mmLogoTime)) + (int)((1 - mmLogoTime) * Global.ScreenWidth * 0.4f), (int)(Global.ScreenHeight * .1f * (mmLogoTime)) + (int)((1 - mmLogoTime) * Global.ScreenHeight * -0.3f), (int)(Global.ScreenWidth * 1.4f * (mmLogoTime)) + (int)((1 - mmLogoTime) * Global.ScreenWidth * 0.2f), (int)(Global.ScreenHeight * 0.4f * (mmLogoTime)))), Color.White);
                else if (mmLogoTime < 2)
                    spriteBatch.Draw(texHeader, (new Rectangle((int)((mmLogoTime - 1) * Global.ScreenWidth * 0.1f) + (int)((1 - (mmLogoTime - 1)) * -Global.ScreenWidth * 0.2f), (int)((mmLogoTime - 1) * Global.ScreenHeight * 0.2f) + (int)((1 - (mmLogoTime - 1)) * Global.ScreenHeight * 0.1f), (int)((mmLogoTime - 1) * Global.ScreenWidth * 0.8f) + (int)((1 - (mmLogoTime - 1)) * Global.ScreenWidth * 1.4f), (int)((mmLogoTime - 1) * Global.ScreenHeight * 0.2f) + (int)((1 - (mmLogoTime - 1)) * Global.ScreenHeight * .4f))), Color.White);
                else
                    spriteBatch.Draw(texHeader, new Rectangle((int)(Global.ScreenWidth * 0.1f), (int)(Global.ScreenHeight * 0.2f), (int)(Global.ScreenWidth * 0.8f), (int)(Global.ScreenHeight * .2f)), Color.White);

                spriteBatch.End();
            }
            catch(Exception e)
            {
                Debug.Error("Problem in MainMenuState.Draw[2]", e);
                UnsignedGame.Singleton.Exit();
                return;
            }
            
        }
    }
}

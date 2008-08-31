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
        private float waveM1, waveM2;
        private int mmenu_select = 0, menu_ticker = 0;
        private float mmLogoTime;
        private SpriteFont sfMenu;
        private int menuSnakeRotOffset;
        private Model mSnake;
        private Texture2D texSnake, texSnakeSkin;
        private RenderTarget2D rtSnake;
        private String[][] mMenuStr;
        private Color[][] mMenuCol;
        private Vector2 menuShiftPos;
        private Texture2D waves, wave;
        private Texture2D songchoosetop;
        private Texture2D texHeader;

        public MainMenuScreen()
        {
            mMenuStr = new string[4][];
            mMenuStr[0] = new string[4];
            mMenuStr[0][0] = "SINGLE PLAYER";
            mMenuStr[0][1] = "MULTIPLAYER";
            mMenuStr[0][2] = "OPTIONS";
            mMenuStr[0][3] = "EXIT";
            mMenuStr[1] = new string[2];
            mMenuStr[1][0] = "CAREER";
            mMenuStr[1][1] = "QUICKPLAY";
            mMenuStr[2] = new string[4];
            mMenuStr[2][0] = "QUICKPLAY";
            mMenuStr[2][1] = "BAND CAREER";
            mMenuStr[2][2] = "ROCK-OFF";
            mMenuStr[2][3] = "SCORE KRIEG";
            mMenuStr[3] = new string[3];
            mMenuStr[3][0] = "AUDIO OPTIONS";
            mMenuStr[3][1] = "VIDEO OPTIONS";
            mMenuStr[3][2] = "DATA OPTIONS";
            mMenuCol = new Color[4][];
            mMenuCol[0] = new Color[4];
            mMenuCol[0][0] = Color.Red;
            mMenuCol[0][1] = Color.White;
            mMenuCol[0][2] = Color.White;
            mMenuCol[0][3] = Color.White;
            mMenuCol[1] = new Color[2];
            mMenuCol[1][0] = Color.Red;
            mMenuCol[1][1] = Color.White;
            mMenuCol[2] = new Color[4];
            mMenuCol[2][0] = Color.White;
            mMenuCol[2][1] = Color.Red;
            mMenuCol[2][2] = Color.Red;
            mMenuCol[2][3] = Color.Red;
            mMenuCol[3] = new Color[3];
            mMenuCol[3][0] = Color.Red;
            mMenuCol[3][1] = Color.Red;
            mMenuCol[3][2] = Color.Red;
        }

        public override void Load(ContentManager content)
        {

        }

        public override void Unload(ContentManager content)
        {

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
                    Peripheral[] controllers = PeripheralManager.GetSingleton().GetPeripherals();
                    for (int i = 0; i < controllers.Length; i++)
                    {
                        if (controllers[i].WasPressed(PeripheralButton.DOWN))
                            collective--;
                        if (controllers[i].WasPressed(PeripheralButton.UP))
                            collective++;
                        if (controllers[i].WasPressed(PeripheralButton.GREEN))
                            green = true;
                        if (controllers[i].WasPressed(PeripheralButton.RED))
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
                        while (mmenu_select >= 50)
                        { mmenu_select -= 10; menuShiftPos.Y = 0; menuSnakeRotOffset -= collective; }

                        if (green && mmenu_select == 20)
                        {
                            mmenu_select++;
                            menuShiftPos.X = 1;
                        }
                        if (green && mmenu_select == 30)
                            UnsignedGame.SINGLETON.PushState(new OptionsScreen());
                        if (green && mmenu_select == 40)
                            UnsignedGame.SINGLETON.Exit();
                    }
                    else
                    {
                        if (mmenu_select > 20 && mmenu_select < 30)
                        {
                            mmenu_select -= collective;
                            if (collective != 0)
                            {
                                menuShiftPos.Y = -1 * collective;
                                menuSnakeRotOffset += collective;
                            }
                            while (mmenu_select < 21)
                            { mmenu_select += 1; menuShiftPos.Y = 0; menuSnakeRotOffset -= collective; }
                            while (mmenu_select > 24)
                            { mmenu_select -= 1; menuShiftPos.Y = 0; menuSnakeRotOffset -= collective; }

                            if (green && mmenu_select == 21)
                                UnsignedGame.SINGLETON.PushState(new ControllerSetupScreen());
                        }


                        if (red)
                        {
                            mmenu_select = mmenu_select / 10 * 10;
                            menuShiftPos.X = -1;
                        }
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
                    Exit();
                    return;
                }
#endif
        }
        public override void Render(Microsoft.Xna.Framework.GameTime gameTime)
        {
            RenderMaster rm = RenderMaster.GetSingleton();
            Effect effect = rm.engine;
            rm.graphics.GraphicsDevice.Clear(Color.Black);

            int linum = 0;
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
            /*if (menuShiftPos.X > 0.004)
            {
                spritebatch.Draw(texSnakeSkin, new Rectangle((int)(rtSnake.Width*menuShiftPos.X), (int)(rtSnake.Height * (.29589/3) * (menuShiftPos.Y+1+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (.29589))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.2f);
                spritebatch.Draw(texSnakeSkin, new Rectangle((int)(rtSnake.Width*menuShiftPos.X), (int)(rtSnake.Height * .29589)+(int)(rtSnake.Height * ((.69727 - .29589)/3) * (menuShiftPos.Y+1+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (.69727 - .29589))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.2f);
                spritebatch.Draw(texSnakeSkin, new Rectangle((int)(rtSnake.Width*menuShiftPos.X), (int)(rtSnake.Height * .69727)+(int)(rtSnake.Height * ((1 - .69727)/3) * (menuShiftPos.Y+1+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (1 - .69727))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.2f);

                spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * (.29589/3) * (menuShiftPos.Y+1+(menuSnakeRotOffset%3))), (int)(rtSnake.Width*menuShiftPos.X), (int)(rtSnake.Height * (.29589))),new Rectangle((int)(texSnakeSkin.Width*(1-menuShiftPos.X)),0,texSnakeSkin.Width-(int)(texSnakeSkin.Width*(1-menuShiftPos.X)),texSnakeSkin.Height), Color.White,0,new Vector2(0,0),SpriteEffects.None,0.2f);
                spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * .29589)+(int)(rtSnake.Height * ((.69727 - .29589)/3) * (menuShiftPos.Y+1+(menuSnakeRotOffset%3))), (int)(rtSnake.Width*menuShiftPos.X), (int)(rtSnake.Height * (.69727 - .29589))),new Rectangle((int)(texSnakeSkin.Width*(1-menuShiftPos.X)),0,texSnakeSkin.Width-(int)(texSnakeSkin.Width*(1-menuShiftPos.X)),texSnakeSkin.Height), Color.White,0,new Vector2(0,0),SpriteEffects.None,0.2f);
                spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * .69727)+(int)(rtSnake.Height * ((1 - .69727)/3) * (menuShiftPos.Y+1+(menuSnakeRotOffset%3))), (int)(rtSnake.Width*menuShiftPos.X), (int)(rtSnake.Height * (1 - .69727))),new Rectangle((int)(texSnakeSkin.Width*(1-menuShiftPos.X)),0,texSnakeSkin.Width-(int)(texSnakeSkin.Width*(1-menuShiftPos.X)),texSnakeSkin.Height), Color.White,0,new Vector2(0,0),SpriteEffects.None,0.2f);
            }
            else if (menuShiftPos.X < -0.004)
            {
                menuShiftPos.X = -menuShiftPos.X;
                spritebatch.Draw(texSnakeSkin, new Rectangle((int)(rtSnake.Width*(1-menuShiftPos.X)), (int)(rtSnake.Height * (.29589/3) * (menuShiftPos.Y+2+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (.29589))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.3f);
                spritebatch.Draw(texSnakeSkin, new Rectangle((int)(rtSnake.Width*(1-menuShiftPos.X)), (int)(rtSnake.Height * .29589)+(int)(rtSnake.Height * ((.69727 - .29589)/3) * (menuShiftPos.Y+2+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (.69727 - .29589))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.3f);
                spritebatch.Draw(texSnakeSkin, new Rectangle((int)(rtSnake.Width*(1-menuShiftPos.X)), (int)(rtSnake.Height * .69727)+(int)(rtSnake.Height * ((1 - .69727)/3) * (menuShiftPos.Y+2+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (1 - .69727))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.3f);

                spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * (.29589/3) * (menuShiftPos.Y+2+(menuSnakeRotOffset%3))), (int)(rtSnake.Width*(1-menuShiftPos.X)), (int)(rtSnake.Height * (.29589))),new Rectangle((int)(texSnakeSkin.Width*(menuShiftPos.X)),0,texSnakeSkin.Width-(int)(texSnakeSkin.Width*(menuShiftPos.X)),texSnakeSkin.Height), Color.White,0,new Vector2(0,0),SpriteEffects.None,0.3f);
                spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * .29589)+(int)(rtSnake.Height * ((.69727 - .29589)/3) * (menuShiftPos.Y+2+(menuSnakeRotOffset%3))), (int)(rtSnake.Width*(1-menuShiftPos.X)), (int)(rtSnake.Height * (.69727 - .29589))),new Rectangle((int)(texSnakeSkin.Width*(menuShiftPos.X)),0,texSnakeSkin.Width-(int)(texSnakeSkin.Width*(menuShiftPos.X)),texSnakeSkin.Height), Color.White,0,new Vector2(0,0),SpriteEffects.None,0.3f);
                spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * .69727)+(int)(rtSnake.Height * ((1 - .69727)/3) * (menuShiftPos.Y+2+(menuSnakeRotOffset%3))), (int)(rtSnake.Width*(1-menuShiftPos.X)), (int)(rtSnake.Height * (1 - .69727))),new Rectangle((int)(texSnakeSkin.Width*(menuShiftPos.X)),0,texSnakeSkin.Width-(int)(texSnakeSkin.Width*(menuShiftPos.X)),texSnakeSkin.Height), Color.White,0,new Vector2(0,0),SpriteEffects.None,0.3f);

                spritebatch.Draw(texSnakeSkin, new Rectangle((int)(rtSnake.Width*(1-menuShiftPos.X)), (int)(rtSnake.Height * (.29589/3) * (menuShiftPos.Y+1+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (.29589))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.2f);
                spritebatch.Draw(texSnakeSkin, new Rectangle((int)(rtSnake.Width*(1-menuShiftPos.X)), (int)(rtSnake.Height * .29589)+(int)(rtSnake.Height * ((.69727 - .29589)/3) * (menuShiftPos.Y+1+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (.69727 - .29589))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.2f);
                spritebatch.Draw(texSnakeSkin, new Rectangle((int)(rtSnake.Width*(1-menuShiftPos.X)), (int)(rtSnake.Height * .69727)+(int)(rtSnake.Height * ((1 - .69727)/3) * (menuShiftPos.Y+1+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (1 - .69727))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.2f);

                spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * (.29589/3) * (menuShiftPos.Y+1+(menuSnakeRotOffset%3))), (int)(rtSnake.Width*(1-menuShiftPos.X)), (int)(rtSnake.Height * (.29589))),new Rectangle((int)(texSnakeSkin.Width*(menuShiftPos.X)),0,texSnakeSkin.Width-(int)(texSnakeSkin.Width*(menuShiftPos.X)),texSnakeSkin.Height), Color.White,0,new Vector2(0,0),SpriteEffects.None,0.2f);
                spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * .29589)+(int)(rtSnake.Height * ((.69727 - .29589)/3) * (menuShiftPos.Y+1+(menuSnakeRotOffset%3))), (int)(rtSnake.Width*(1-menuShiftPos.X)), (int)(rtSnake.Height * (.69727 - .29589))),new Rectangle((int)(texSnakeSkin.Width*(menuShiftPos.X)),0,texSnakeSkin.Width-(int)(texSnakeSkin.Width*(menuShiftPos.X)),texSnakeSkin.Height), Color.White,0,new Vector2(0,0),SpriteEffects.None,0.2f);
                spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * .69727)+(int)(rtSnake.Height * ((1 - .69727)/3) * (menuShiftPos.Y+1+(menuSnakeRotOffset%3))), (int)(rtSnake.Width*(1-menuShiftPos.X)), (int)(rtSnake.Height * (1 - .69727))),new Rectangle((int)(texSnakeSkin.Width*(menuShiftPos.X)),0,texSnakeSkin.Width-(int)(texSnakeSkin.Width*(menuShiftPos.X)),texSnakeSkin.Height), Color.White,0,new Vector2(0,0),SpriteEffects.None,0.2f);
                menuShiftPos.X = -menuShiftPos.X;
            }
            else if (menuShiftPos.Y >= 0)
            {
                spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * (.29589/3) * (menuShiftPos.Y-1+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (.29589))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.2f);
                spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * (.29589/3) * (menuShiftPos.Y+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (.29589))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.1f);
                spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * (.29589/3) * (menuShiftPos.Y+1+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (.29589))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.2f);
                spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * (.29589/3) * (menuShiftPos.Y+2+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (.29589))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.3f);

                spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * .29589)+(int)(rtSnake.Height * ((.69727 - .29589)/3) * (menuShiftPos.Y-1+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (.69727 - .29589))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.2f);
                spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * .29589)+(int)(rtSnake.Height * ((.69727 - .29589)/3) * (menuShiftPos.Y+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (.69727 - .29589))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.1f);
                spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * .29589)+(int)(rtSnake.Height * ((.69727 - .29589)/3) * (menuShiftPos.Y+1+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (.69727 - .29589))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.2f);
                spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * .29589)+(int)(rtSnake.Height * ((.69727 - .29589)/3) * (menuShiftPos.Y+2+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (.69727 - .29589))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.3f);
                        
                spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * .69727)+(int)(rtSnake.Height * ((1 - .69727)/3) * (menuShiftPos.Y-1+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (1 - .69727))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.2f);
                spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * .69727)+(int)(rtSnake.Height * ((1 - .69727)/3) * (menuShiftPos.Y+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (1 - .69727))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.1f);
                spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * .69727)+(int)(rtSnake.Height * ((1 - .69727)/3) * (menuShiftPos.Y+1+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (1 - .69727))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.2f);
                spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * .69727)+(int)(rtSnake.Height * ((1 - .69727)/3) * (menuShiftPos.Y+2+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (1 - .69727))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.3f);
            }
            else
            {
                spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * (.29589/3) * (menuShiftPos.Y-2+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (.29589))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.3f);
                spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * (.29589/3) * (menuShiftPos.Y-1+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (.29589))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.2f);
                spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * (.29589/3) * (menuShiftPos.Y+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (.29589))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.1f);
                spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * (.29589/3) * (menuShiftPos.Y+1+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (.29589))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.2f);
                spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * (.29589/3) * (menuShiftPos.Y+2+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (.29589))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.3f);

                spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * .29589)+(int)(rtSnake.Height * ((.69727 - .29589)/3) * (menuShiftPos.Y-2+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (.69727 - .29589))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.3f);
                spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * .29589)+(int)(rtSnake.Height * ((.69727 - .29589)/3) * (menuShiftPos.Y-1+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (.69727 - .29589))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.2f);
                spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * .29589)+(int)(rtSnake.Height * ((.69727 - .29589)/3) * (menuShiftPos.Y+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (.69727 - .29589))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.1f);
                spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * .29589)+(int)(rtSnake.Height * ((.69727 - .29589)/3) * (menuShiftPos.Y+1+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (.69727 - .29589))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.2f);
                spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * .29589)+(int)(rtSnake.Height * ((.69727 - .29589)/3) * (menuShiftPos.Y+2+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (.69727 - .29589))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.3f);
                        
                spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * .69727)+(int)(rtSnake.Height * ((1 - .69727)/3) * (menuShiftPos.Y-2+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (1 - .69727))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.3f);
                spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * .69727)+(int)(rtSnake.Height * ((1 - .69727)/3) * (menuShiftPos.Y-1+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (1 - .69727))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.2f);
                spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * .69727)+(int)(rtSnake.Height * ((1 - .69727)/3) * (menuShiftPos.Y+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (1 - .69727))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.1f);
                spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * .69727)+(int)(rtSnake.Height * ((1 - .69727)/3) * (menuShiftPos.Y+1+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (1 - .69727))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.2f);
                spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * .69727)+(int)(rtSnake.Height * ((1 - .69727)/3) * (menuShiftPos.Y+2+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (1 - .69727))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.3f);
                spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * .69727)+(int)(rtSnake.Height * ((1 - .69727)/3) * (menuShiftPos.Y+3+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (1 - .69727))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.4f);
            }*/
            if (Math.Abs(menuShiftPos.X) <= 0.004)
            {
                if (mmenu_select % 10 == 0)
                {
                    for (int i = 0; i < mMenuStr[0].Length; i++)
                    {
                        Vector2 sz = Global.DefaultFont.MeasureString("" + mMenuStr[0][i]);
                        rm.spritebatch.DrawString(Global.DefaultFont, "" + mMenuStr[0][i], new Vector2((rtSnake.Width / 2) - (sz.X / 2), (rtSnake.Height / 2) - (sz.Y / 2) + (yscale * (i + menuShiftPos.Y - (mmenu_select / 10) + 1))), mMenuCol[0][i]);
                    }
                    /*if (menuShiftPos.Y >= 0)
                    {
                        Vector2 meas = sfMenu.MeasureString(mMenuStr[0][Math.Max(0, mmenu_select / 10 - 1)]);
                        spritebatch.DrawString(sfMenu, mMenuStr[0][Math.Max(0, mmenu_select / 10 - 1)], new Vector2(rtSnake.Width / 2 - (meas.X / 2), ((1 - menuShiftPos.Y) * (rtSnake.Height * (((.69727f - .29589f) / 2) + .29589f) - (meas.Y / 2))) + (menuShiftPos.Y * (rtSnake.Height * (.69727f) - (meas.Y)))), mMenuCol[0][Math.Max(0, mmenu_select / 10 - 1)]);
                        if (mmenu_select / 10 - 2 > 0)
                        {
                            meas = sfMenu.MeasureString(mMenuStr[0][Math.Max(0, mmenu_select / 10 - 3)]);
                            spritebatch.DrawString(sfMenu, mMenuStr[0][Math.Max(0, mmenu_select / 10 - 3)], new Vector2(rtSnake.Width / 2 - (meas.X / 2), (rtSnake.Height * (.29589f))), mMenuCol[0][Math.Max(0, mmenu_select / 10 - 3)], 0, new Vector2(0, 0), new Vector2(1, menuShiftPos.Y), SpriteEffects.None, 0);
                        }
                        if (mmenu_select / 10 - 1 > 0)
                        {
                            meas = sfMenu.MeasureString(mMenuStr[0][Math.Max(0, mmenu_select / 10 - 2)]);
                            spritebatch.DrawString(sfMenu, mMenuStr[0][Math.Max(0, mmenu_select / 10 - 2)], new Vector2(rtSnake.Width / 2 - (meas.X / 2), ((1 - menuShiftPos.Y) * (rtSnake.Height * (.29589f))) + (menuShiftPos.Y * (rtSnake.Height * (((.69727f - .29589f) / 2) + .29589f) - (meas.Y / 2)))), mMenuCol[0][Math.Max(0, mmenu_select / 10 - 2)]);
                        }
                        if (mmenu_select / 10 - 1 < mMenuStr[0].Length - 1)
                        {
                            meas = sfMenu.MeasureString(mMenuStr[0][Math.Max(0, mmenu_select / 10)]);
                            spritebatch.DrawString(sfMenu, mMenuStr[0][Math.Max(0, mmenu_select / 10)], new Vector2(rtSnake.Width / 2 - (meas.X / 2), (rtSnake.Height * (.69727f))), mMenuCol[0][Math.Max(0, mmenu_select / 10)], 0, new Vector2(0, meas.Y), new Vector2(1, 1 - menuShiftPos.Y), SpriteEffects.None, 0);
                        }
                    }
                    else
                    {
                        menuShiftPos.Y = -menuShiftPos.Y;
                        Vector2 meas = sfMenu.MeasureString(mMenuStr[0][Math.Max(0, mmenu_select / 10 - 1)]);
                        spritebatch.DrawString(sfMenu, mMenuStr[0][Math.Max(0, mmenu_select / 10 - 1)], new Vector2(rtSnake.Width / 2 - (meas.X / 2), ((1 - menuShiftPos.Y) * (rtSnake.Height * (((.69727f - .29589f) / 2) + .29589f) - (meas.Y / 2))) + (menuShiftPos.Y * (rtSnake.Height * (.29589f)))), mMenuCol[0][Math.Max(0, mmenu_select / 10 - 1)]);
                        if (mmenu_select / 10 - 1 > 0)
                        {
                            meas = sfMenu.MeasureString(mMenuStr[0][Math.Max(0, mmenu_select / 10 - 2)]);
                            spritebatch.DrawString(sfMenu, mMenuStr[0][Math.Max(0, mmenu_select / 10 - 2)], new Vector2(rtSnake.Width / 2 - (meas.X / 2), (rtSnake.Height * (.29589f))), mMenuCol[0][Math.Max(0, mmenu_select / 10 - 2)], 0, new Vector2(0, 0), new Vector2(1, 1 - menuShiftPos.Y), SpriteEffects.None, 0);
                        }
                        if (mmenu_select / 10 - 1 < mMenuStr[0].Length - 1)
                        {
                            meas = sfMenu.MeasureString(mMenuStr[0][Math.Max(0, mmenu_select / 10)]);
                            spritebatch.DrawString(sfMenu, mMenuStr[0][Math.Max(0, mmenu_select / 10)], new Vector2(rtSnake.Width / 2 - (meas.X / 2), ((1 - menuShiftPos.Y) * (rtSnake.Height * (.69727f) - (meas.Y))) + (menuShiftPos.Y * (rtSnake.Height * (((.69727f - .29589f) / 2) + .29589f) - (meas.Y / 2)))), mMenuCol[0][Math.Max(0, mmenu_select / 10)]);
                        }
                        if (mmenu_select / 10 - 1 < mMenuStr[0].Length - 2)
                        {
                            meas = sfMenu.MeasureString(mMenuStr[0][Math.Max(0, mmenu_select / 10 + 1)]);
                            spritebatch.DrawString(sfMenu, mMenuStr[0][Math.Max(0, mmenu_select / 10 + 1)], new Vector2(rtSnake.Width / 2 - (meas.X / 2), (rtSnake.Height * (.69727f))), mMenuCol[0][Math.Max(0, mmenu_select / 10 + 1)], 0, new Vector2(0, meas.Y), new Vector2(1, menuShiftPos.Y), SpriteEffects.None, 0);
                        }
                        menuShiftPos.Y = -menuShiftPos.Y;
                    }*/
                }
                else
                {
                    for (int i = 0; i < mMenuStr[mmenu_select / 10].Length; i++)
                    {
                        Vector2 sz = Global.DefaultFont.MeasureString("" + mMenuStr[mmenu_select / 10][i]);
                        rm.spritebatch.DrawString(Global.DefaultFont, "" + mMenuStr[mmenu_select / 10][i], new Vector2((rtSnake.Width / 2) - (sz.X / 2), (rtSnake.Height / 2) - (sz.Y / 2) + (yscale * (i + menuShiftPos.Y - (mmenu_select % 10) + 1))), mMenuCol[mmenu_select / 10][i]);
                    }
                    /*if (menuShiftPos.Y >= 0)
                    {
                        Vector2 meas = sfMenu.MeasureString(mMenuStr[mmenu_select / 10][Math.Max(0, mmenu_select % 10 - 1)]);
                        spritebatch.DrawString(sfMenu, mMenuStr[mmenu_select / 10][Math.Max(0, mmenu_select % 10 - 1)], new Vector2(rtSnake.Width / 2 - (meas.X / 2), ((1 - menuShiftPos.Y) * (rtSnake.Height * (((.69727f - .29589f) / 2) + .29589f) - (meas.Y / 2))) + (menuShiftPos.Y * (rtSnake.Height * (.69727f) - (meas.Y)))), mMenuCol[mmenu_select / 10][Math.Max(0, mmenu_select % 10 - 1)]);
                        if (mmenu_select % 10 - 2 > 0)
                        {
                            meas = sfMenu.MeasureString(mMenuStr[mmenu_select / 10][Math.Max(0, mmenu_select % 10 - 3)]);
                            spritebatch.DrawString(sfMenu, mMenuStr[mmenu_select / 10][Math.Max(0, mmenu_select / 10 - 3)], new Vector2(rtSnake.Width / 2 - (meas.X / 2), (rtSnake.Height * (.29589f))), mMenuCol[mmenu_select / 10][Math.Max(0, mmenu_select / 10 - 3)], 0, new Vector2(0, 0), new Vector2(1, menuShiftPos.Y), SpriteEffects.None, 0);
                        }
                        if (mmenu_select % 10 - 1 > 0)
                        {
                            meas = sfMenu.MeasureString(mMenuStr[mmenu_select / 10][Math.Max(0, mmenu_select % 10 - 2)]);
                            spritebatch.DrawString(sfMenu, mMenuStr[mmenu_select / 10][Math.Max(0, mmenu_select % 10 - 2)], new Vector2(rtSnake.Width / 2 - (meas.X / 2), ((1 - menuShiftPos.Y) * (rtSnake.Height * (.29589f))) + (menuShiftPos.Y * (rtSnake.Height * (((.69727f - .29589f) / 2) + .29589f) - (meas.Y / 2)))), mMenuCol[mmenu_select / 10][Math.Max(0, mmenu_select % 10 - 2)]);
                        }
                        if (mmenu_select % 10 - 1 < mMenuStr[mmenu_select / 10].Length - 1)
                        {
                            meas = sfMenu.MeasureString(mMenuStr[mmenu_select / 10][Math.Max(0, mmenu_select % 10)]);
                            spritebatch.DrawString(sfMenu, mMenuStr[mmenu_select / 10][Math.Max(0, mmenu_select % 10)], new Vector2(rtSnake.Width / 2 - (meas.X / 2), (rtSnake.Height * (.69727f))), mMenuCol[mmenu_select / 10][Math.Max(0, mmenu_select % 10)], 0, new Vector2(0, meas.Y), new Vector2(1, 1 - menuShiftPos.Y), SpriteEffects.None, 0);
                        }
                    }
                    else
                    {
                        menuShiftPos.Y = -menuShiftPos.Y;
                        Vector2 meas = sfMenu.MeasureString(mMenuStr[mmenu_select / 10][Math.Max(0, mmenu_select % 10 - 1)]);
                        spritebatch.DrawString(sfMenu, mMenuStr[mmenu_select / 10][Math.Max(0, mmenu_select % 10 - 1)], new Vector2(rtSnake.Width / 2 - (meas.X / 2), ((1 - menuShiftPos.Y) * (rtSnake.Height * (((.69727f - .29589f) / 2) + .29589f) - (meas.Y / 2))) + (menuShiftPos.Y * (rtSnake.Height * (.29589f)))), mMenuCol[mmenu_select / 10][Math.Max(0, mmenu_select % 10 - 1)]);
                        if (mmenu_select % 10 - 1 > 0)
                        {
                            meas = sfMenu.MeasureString(mMenuStr[mmenu_select / 10][Math.Max(0, mmenu_select % 10 - 2)]);
                            spritebatch.DrawString(sfMenu, mMenuStr[mmenu_select / 10][Math.Max(0, mmenu_select % 10 - 2)], new Vector2(rtSnake.Width / 2 - (meas.X / 2), (rtSnake.Height * (.29589f))), mMenuCol[mmenu_select / 10][Math.Max(0, mmenu_select % 10 - 2)], 0, new Vector2(0, 0), new Vector2(1, 1 - menuShiftPos.Y), SpriteEffects.None, 0);
                        }
                        if (mmenu_select % 10 - 1 < mMenuStr[mmenu_select / 10].Length - 1)
                        {
                            meas = sfMenu.MeasureString(mMenuStr[mmenu_select / 10][Math.Max(0, mmenu_select % 10)]);
                            spritebatch.DrawString(sfMenu, mMenuStr[mmenu_select / 10][Math.Max(0, mmenu_select % 10)], new Vector2(rtSnake.Width / 2 - (meas.X / 2), ((1 - menuShiftPos.Y) * (rtSnake.Height * (.69727f) - (meas.Y))) + (menuShiftPos.Y * (rtSnake.Height * (((.69727f - .29589f) / 2) + .29589f) - (meas.Y / 2)))), mMenuCol[mmenu_select / 10][Math.Max(0, mmenu_select % 10)]);
                        }
                        if (mmenu_select % 10 - 1 < mMenuStr[mmenu_select / 10].Length - 2)
                        {
                            meas = sfMenu.MeasureString(mMenuStr[mmenu_select / 10][Math.Max(0, mmenu_select % 10 + 1)]);
                            spritebatch.DrawString(sfMenu, mMenuStr[mmenu_select / 10][Math.Max(0, mmenu_select % 10 + 1)], new Vector2(rtSnake.Width / 2 - (meas.X / 2), (rtSnake.Height * (.69727f))), mMenuCol[mmenu_select / 10][Math.Max(0, mmenu_select % 10 + 1)], 0, new Vector2(0, meas.Y), new Vector2(1, menuShiftPos.Y), SpriteEffects.None, 0);
                        }
                        menuShiftPos.Y = -menuShiftPos.Y;
                    }*/
                }
            }
            else if (menuShiftPos.X > 0)
            {
                for (int i = 0; i < mMenuStr[0].Length; i++)
                {
                    Vector2 sz = Global.DefaultFont.MeasureString("" + mMenuStr[0][i]);
                    rm.spritebatch.DrawString(Global.DefaultFont, "" + mMenuStr[0][i], new Vector2((rtSnake.Width / 2) - (sz.X / 2) + (((menuShiftPos.X * 2) - 2) * xscale), (rtSnake.Height / 2) - (sz.Y / 2) + (yscale * (i + menuShiftPos.Y - (mmenu_select / 10) + 1))), new Color(mMenuCol[0][i].R, mMenuCol[0][i].G, mMenuCol[0][i].B, (byte)(255 * menuShiftPos.X)));
                }
                for (int i = 0; i < mMenuStr[mmenu_select / 10].Length; i++)
                {
                    Vector2 sz = Global.DefaultFont.MeasureString("" + mMenuStr[mmenu_select / 10][i]);
                    rm.spritebatch.DrawString(Global.DefaultFont, "" + mMenuStr[mmenu_select / 10][i], new Vector2((rtSnake.Width / 2) - (sz.X / 2) + ((menuShiftPos.X * 2) * xscale), (rtSnake.Height / 2) - (sz.Y / 2) + (yscale * (i + menuShiftPos.Y - (mmenu_select % 10) + 1))), new Color(mMenuCol[mmenu_select / 10][i].R, mMenuCol[mmenu_select / 10][i].G, mMenuCol[mmenu_select / 10][i].B, (byte)(255 * (1 - menuShiftPos.X))));
                }
                /*Vector2 meas = sfMenu.MeasureString(mMenuStr[0][Math.Max(0, mmenu_select / 10 - 1)]);
                spritebatch.DrawString(sfMenu, mMenuStr[0][Math.Max(0, mmenu_select / 10 - 1)], new Vector2((rtSnake.Width / 2 - (meas.X / 2))-(rtSnake.Width*(1-menuShiftPos.X)), ((1 - menuShiftPos.Y) * (rtSnake.Height * (((.69727f - .29589f) / 2) + .29589f) - (meas.Y / 2)))), new Color(mMenuCol[0][Math.Max(0, mmenu_select / 10 - 1)].R,mMenuCol[0][Math.Max(0, mmenu_select / 10 - 1)].G,mMenuCol[0][Math.Max(0, mmenu_select / 10 - 1)].B,(byte)(255*menuShiftPos.X)));
                spritebatch.DrawString(sfMenu, mMenuStr[0][Math.Max(0, mmenu_select / 10 - 1)], new Vector2((rtSnake.Width / 2 - (meas.X / 2))-(rtSnake.Width*(1-menuShiftPos.X))+rtSnake.Width, ((1 - menuShiftPos.Y) * (rtSnake.Height * (.29589f / 2)))), new Color(mMenuCol[0][Math.Max(0, mmenu_select / 10 - 1)].R,mMenuCol[0][Math.Max(0, mmenu_select / 10 - 1)].G,mMenuCol[0][Math.Max(0, mmenu_select / 10 - 1)].B,(byte)(255*menuShiftPos.X)), 0, new Vector2(0,meas.Y / 2),new Vector2(1,.29589f/(.69727f - .29589f)),SpriteEffects.None,0);
                if (mmenu_select / 10 - 1 > 0)
                {
                    meas = sfMenu.MeasureString(mMenuStr[0][Math.Max(0, mmenu_select / 10 - 2)]);
                    spritebatch.DrawString(sfMenu, mMenuStr[0][Math.Max(0, mmenu_select / 10 - 2)], new Vector2((rtSnake.Width / 2 - (meas.X / 2))-(rtSnake.Width*(1-menuShiftPos.X)), rtSnake.Height*.29589f), new Color(mMenuCol[0][Math.Max(0, mmenu_select / 10 - 2)].R,mMenuCol[0][Math.Max(0, mmenu_select / 10 - 2)].G,mMenuCol[0][Math.Max(0, mmenu_select / 10 - 2)].B,(byte)(255*menuShiftPos.X)));
                    spritebatch.DrawString(sfMenu, mMenuStr[0][Math.Max(0, mmenu_select / 10 - 2)], new Vector2((rtSnake.Width / 2 - (meas.X / 2))-(rtSnake.Width*(1-menuShiftPos.X))+rtSnake.Width, ((meas.Y/2)*(.29589f/(.69727f - .29589f)))),new Color(mMenuCol[0][Math.Max(0, mmenu_select / 10 - 2)].R,mMenuCol[0][Math.Max(0, mmenu_select / 10 - 2)].G,mMenuCol[0][Math.Max(0, mmenu_select / 10 - 2)].B,(byte)(255*menuShiftPos.X)),0,new Vector2(0,meas.Y / 2),new Vector2(1,.29589f/(.69727f - .29589f)),SpriteEffects.None,0);
                }
                if (mmenu_select / 10 - 1 < mMenuStr[0].Length - 1)
                {
                    meas = sfMenu.MeasureString(mMenuStr[0][Math.Max(0, mmenu_select / 10)]);
                    spritebatch.DrawString(sfMenu, mMenuStr[0][Math.Max(0, mmenu_select / 10)], new Vector2((rtSnake.Width / 2 - (meas.X / 2))-(rtSnake.Width*(1-menuShiftPos.X)), (rtSnake.Height * (.69727f))-((meas.Y/2)*(.29589f/(.69727f - .29589f)))), new Color(mMenuCol[0][Math.Max(0, mmenu_select / 10)].R,mMenuCol[0][Math.Max(0, mmenu_select / 10)].G,mMenuCol[0][Math.Max(0, mmenu_select / 10)].B,(byte)(255*menuShiftPos.X)), 0, new Vector2(0, meas.Y/2), new Vector2(1, 1), SpriteEffects.None, 0);
                    spritebatch.DrawString(sfMenu, mMenuStr[0][Math.Max(0, mmenu_select / 10)], new Vector2((rtSnake.Width / 2 - (meas.X / 2))-(rtSnake.Width*(1-menuShiftPos.X))+rtSnake.Width, (rtSnake.Height * (.29589f))-((meas.Y/2)*(.29589f/(.69727f - .29589f)))), new Color(mMenuCol[0][Math.Max(0, mmenu_select / 10)].R,mMenuCol[0][Math.Max(0, mmenu_select / 10)].G,mMenuCol[0][Math.Max(0, mmenu_select / 10)].B,(byte)(255*menuShiftPos.X)), 0, new Vector2(0, meas.Y/2), new Vector2(1, .29589f/(.69727f - .29589f)), SpriteEffects.None, 0);
                }

                meas = sfMenu.MeasureString(mMenuStr[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 1)]);
                spritebatch.DrawString(sfMenu, mMenuStr[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 1)], new Vector2((rtSnake.Width / 2 - (meas.X / 2))-(rtSnake.Width*(1-menuShiftPos.X))+rtSnake.Width, ((rtSnake.Height * (((.69727f - .29589f) / 2) + .29589f) - (meas.Y / 2)))), new Color(mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 1)].R,mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 1)].G,mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 1)].B,(byte)(255*(1-menuShiftPos.X))));
                spritebatch.DrawString(sfMenu, mMenuStr[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 1)], new Vector2((rtSnake.Width / 2 - (meas.X / 2))-(rtSnake.Width*(1-menuShiftPos.X)), ((rtSnake.Height * ((1-.69727f) / 2 + .69727f)))), new Color(mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 1)].R,mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 1)].G,mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 1)].B,(byte)(255*(1-menuShiftPos.X))), 0, new Vector2(0,meas.Y / 2),new Vector2(1,.29589f/(.69727f - .29589f)),SpriteEffects.None,0);
                if (mmenu_select % 10 - 1 > 0)
                {
                    meas = sfMenu.MeasureString(mMenuStr[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 2)]);
                    spritebatch.DrawString(sfMenu, mMenuStr[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 2)], new Vector2((rtSnake.Width / 2 - (meas.X / 2))-(rtSnake.Width*(1-menuShiftPos.X))+rtSnake.Width, rtSnake.Height*.29589f), new Color(mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 2)].R,mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 2)].G,mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 2)].B,(byte)(255*(1-menuShiftPos.X))));
                    spritebatch.DrawString(sfMenu, mMenuStr[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 2)], new Vector2((rtSnake.Width / 2 - (meas.X / 2))-(rtSnake.Width*(1-menuShiftPos.X)), ((meas.Y/2)*(.29589f/(.69727f - .29589f) + .69727f))),new Color(mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 2)].R,mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 2)].G,mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 2)].B,(byte)(255*(1-menuShiftPos.X))),0,new Vector2(0,meas.Y / 2),new Vector2(1,.29589f/(.69727f - .29589f)),SpriteEffects.None,0);
                }
                if (mmenu_select % 10 - 1 < mMenuStr[mmenu_select/10].Length - 1)
                {
                    meas = sfMenu.MeasureString(mMenuStr[mmenu_select/10][Math.Max(0, mmenu_select / 10)]);
                    spritebatch.DrawString(sfMenu, mMenuStr[mmenu_select/10][Math.Max(0, mmenu_select % 10)], new Vector2((rtSnake.Width / 2 - (meas.X / 2))-(rtSnake.Width*(1-menuShiftPos.X))+rtSnake.Width, (rtSnake.Height * (.69727f))-((meas.Y/2)*(.29589f/(.69727f - .29589f)))), new Color(mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10)].R,mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10)].G,mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10)].B,(byte)(255*(1-menuShiftPos.X))), 0, new Vector2(0, meas.Y/2), new Vector2(1, 1), SpriteEffects.None, 0);
                    spritebatch.DrawString(sfMenu, mMenuStr[mmenu_select/10][Math.Max(0, mmenu_select % 10)], new Vector2((rtSnake.Width / 2 - (meas.X / 2))-(rtSnake.Width*(1-menuShiftPos.X)), (rtSnake.Height * (.29589f))-((meas.Y/2)*(.29589f/(.69727f - .29589f) + .69727f))), new Color(mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10)].R,mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10)].G,mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10)].B,(byte)(255*(1-menuShiftPos.X))), 0, new Vector2(0, meas.Y/2), new Vector2(1, .29589f/(.69727f - .29589f)), SpriteEffects.None, 0);
                }*/
            }
            else if (menuShiftPos.X < 0)
            {
                for (int i = 0; i < mMenuStr[0].Length; i++)
                {
                    Vector2 sz = Global.DefaultFont.MeasureString("" + mMenuStr[0][i]);
                    rm.spritebatch.DrawString(Global.DefaultFont, "" + mMenuStr[0][i], new Vector2((rtSnake.Width / 2) - (sz.X / 2) + (((menuShiftPos.X * 2)) * xscale), (rtSnake.Height / 2) - (sz.Y / 2) + (yscale * (i + menuShiftPos.Y - (mmenu_select / 10) + 1))), new Color(mMenuCol[0][i].R, mMenuCol[0][i].G, mMenuCol[0][i].B, (byte)(255 * menuShiftPos.X)));
                }
                for (int i = 0; i < mMenuStr[mmenu_select / 10].Length; i++)
                {
                    Vector2 sz = Global.DefaultFont.MeasureString("" + mMenuStr[mmenu_select / 10][i]);
                    rm.spritebatch.DrawString(Global.DefaultFont, "" + mMenuStr[mmenu_select / 10][i], new Vector2((rtSnake.Width / 2) - (sz.X / 2) + (((menuShiftPos.X * 2) + 2) * xscale), (rtSnake.Height / 2) - (sz.Y / 2) + (yscale * (i + menuShiftPos.Y - (mmenu_select % 10) + 1))), new Color(mMenuCol[mmenu_select / 10][i].R, mMenuCol[mmenu_select / 10][i].G, mMenuCol[mmenu_select / 10][i].B, (byte)(255 * (1 - menuShiftPos.X))));
                }
                /*menuShiftPos.X = (1 + menuShiftPos.X);
                Vector2 meas = sfMenu.MeasureString(mMenuStr[0][Math.Max(0, mmenu_select / 10 - 1)]);
                spritebatch.DrawString(sfMenu, mMenuStr[0][Math.Max(0, mmenu_select / 10 - 1)], new Vector2((rtSnake.Width / 2 - (meas.X / 2))-(rtSnake.Width*(1-menuShiftPos.X)), ((1 - menuShiftPos.Y) * (rtSnake.Height * (((.69727f - .29589f) / 2) + .29589f) - (meas.Y / 2)))), new Color(mMenuCol[0][Math.Max(0, mmenu_select / 10 - 1)].R,mMenuCol[0][Math.Max(0, mmenu_select / 10 - 1)].G,mMenuCol[0][Math.Max(0, mmenu_select / 10 - 1)].B,(byte)(255*menuShiftPos.X)));
                spritebatch.DrawString(sfMenu, mMenuStr[0][Math.Max(0, mmenu_select / 10 - 1)], new Vector2((rtSnake.Width / 2 - (meas.X / 2))-(rtSnake.Width*(1-menuShiftPos.X))+rtSnake.Width, ((1 - menuShiftPos.Y) * (rtSnake.Height * (.29589f / 2)))), new Color(mMenuCol[0][Math.Max(0, mmenu_select / 10 - 1)].R,mMenuCol[0][Math.Max(0, mmenu_select / 10 - 1)].G,mMenuCol[0][Math.Max(0, mmenu_select / 10 - 1)].B,(byte)(255*menuShiftPos.X)), 0, new Vector2(0,meas.Y / 2),new Vector2(1,.29589f/(.69727f - .29589f)),SpriteEffects.None,0);
                if (mmenu_select / 10 - 1 > 0)
                {
                    meas = sfMenu.MeasureString(mMenuStr[0][Math.Max(0, mmenu_select / 10 - 2)]);
                    spritebatch.DrawString(sfMenu, mMenuStr[0][Math.Max(0, mmenu_select / 10 - 2)], new Vector2((rtSnake.Width / 2 - (meas.X / 2))-(rtSnake.Width*(1-menuShiftPos.X)), rtSnake.Height*.29589f), new Color(mMenuCol[0][Math.Max(0, mmenu_select / 10 - 2)].R,mMenuCol[0][Math.Max(0, mmenu_select / 10 - 2)].G,mMenuCol[0][Math.Max(0, mmenu_select / 10 - 2)].B,(byte)(255*menuShiftPos.X)));
                    spritebatch.DrawString(sfMenu, mMenuStr[0][Math.Max(0, mmenu_select / 10 - 2)], new Vector2((rtSnake.Width / 2 - (meas.X / 2))-(rtSnake.Width*(1-menuShiftPos.X))+rtSnake.Width, ((meas.Y/2)*(.29589f/(.69727f - .29589f)))),new Color(mMenuCol[0][Math.Max(0, mmenu_select / 10 - 2)].R,mMenuCol[0][Math.Max(0, mmenu_select / 10 - 2)].G,mMenuCol[0][Math.Max(0, mmenu_select / 10 - 2)].B,(byte)(255*menuShiftPos.X)),0,new Vector2(0,meas.Y / 2),new Vector2(1,.29589f/(.69727f - .29589f)),SpriteEffects.None,0);
                }
                if (mmenu_select / 10 - 1 < mMenuStr[0].Length - 1)
                {
                    meas = sfMenu.MeasureString(mMenuStr[0][Math.Max(0, mmenu_select / 10)]);
                    spritebatch.DrawString(sfMenu, mMenuStr[0][Math.Max(0, mmenu_select / 10)], new Vector2((rtSnake.Width / 2 - (meas.X / 2))-(rtSnake.Width*(1-menuShiftPos.X)), (rtSnake.Height * (.69727f))-((meas.Y/2)*(.29589f/(.69727f - .29589f)))), new Color(mMenuCol[0][Math.Max(0, mmenu_select / 10)].R,mMenuCol[0][Math.Max(0, mmenu_select / 10)].G,mMenuCol[0][Math.Max(0, mmenu_select / 10)].B,(byte)(255*menuShiftPos.X)), 0, new Vector2(0, meas.Y/2), new Vector2(1, 1), SpriteEffects.None, 0);
                    spritebatch.DrawString(sfMenu, mMenuStr[0][Math.Max(0, mmenu_select / 10)], new Vector2((rtSnake.Width / 2 - (meas.X / 2))-(rtSnake.Width*(1-menuShiftPos.X))+rtSnake.Width, (rtSnake.Height * (.29589f))-((meas.Y/2)*(.29589f/(.69727f - .29589f)))), new Color(mMenuCol[0][Math.Max(0, mmenu_select / 10)].R,mMenuCol[0][Math.Max(0, mmenu_select / 10)].G,mMenuCol[0][Math.Max(0, mmenu_select / 10)].B,(byte)(255*menuShiftPos.X)), 0, new Vector2(0, meas.Y/2), new Vector2(1, .29589f/(.69727f - .29589f)), SpriteEffects.None, 0);
                }

                meas = sfMenu.MeasureString(mMenuStr[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 1)]);
                spritebatch.DrawString(sfMenu, mMenuStr[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 1)], new Vector2((rtSnake.Width / 2 - (meas.X / 2))-(rtSnake.Width*(1-menuShiftPos.X))+rtSnake.Width, ((rtSnake.Height * (((.69727f - .29589f) / 2) + .29589f) - (meas.Y / 2)))), new Color(mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 1)].R,mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 1)].G,mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 1)].B,(byte)(255*(1-menuShiftPos.X))));
                spritebatch.DrawString(sfMenu, mMenuStr[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 1)], new Vector2((rtSnake.Width / 2 - (meas.X / 2))-(rtSnake.Width*(1-menuShiftPos.X)), ((rtSnake.Height * ((1-.69727f) / 2 + .69727f)))), new Color(mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 1)].R,mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 1)].G,mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 1)].B,(byte)(255*(1-menuShiftPos.X))), 0, new Vector2(0,meas.Y / 2),new Vector2(1,.29589f/(.69727f - .29589f)),SpriteEffects.None,0);
                if (mmenu_select % 10 - 1 > 0)
                {
                    meas = sfMenu.MeasureString(mMenuStr[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 2)]);
                    spritebatch.DrawString(sfMenu, mMenuStr[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 2)], new Vector2((rtSnake.Width / 2 - (meas.X / 2))-(rtSnake.Width*(1-menuShiftPos.X))+rtSnake.Width, rtSnake.Height*.29589f), new Color(mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 2)].R,mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 2)].G,mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 2)].B,(byte)(255*(1-menuShiftPos.X))));
                    spritebatch.DrawString(sfMenu, mMenuStr[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 2)], new Vector2((rtSnake.Width / 2 - (meas.X / 2))-(rtSnake.Width*(1-menuShiftPos.X)), ((meas.Y/2)*(.29589f/(.69727f - .29589f) + .69727f))),new Color(mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 2)].R,mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 2)].G,mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 2)].B,(byte)(255*(1-menuShiftPos.X))),0,new Vector2(0,meas.Y / 2),new Vector2(1,.29589f/(.69727f - .29589f)),SpriteEffects.None,0);
                }
                if (mmenu_select % 10 - 1 < mMenuStr[mmenu_select/10].Length - 1)
                {
                    meas = sfMenu.MeasureString(mMenuStr[mmenu_select/10][Math.Max(0, mmenu_select / 10)]);
                    spritebatch.DrawString(sfMenu, mMenuStr[mmenu_select/10][Math.Max(0, mmenu_select % 10)], new Vector2((rtSnake.Width / 2 - (meas.X / 2))-(rtSnake.Width*(1-menuShiftPos.X))+rtSnake.Width, (rtSnake.Height * (.69727f))-((meas.Y/2)*(.29589f/(.69727f - .29589f)))), new Color(mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10)].R,mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10)].G,mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10)].B,(byte)(255*(1-menuShiftPos.X))), 0, new Vector2(0, meas.Y/2), new Vector2(1, 1), SpriteEffects.None, 0);
                    spritebatch.DrawString(sfMenu, mMenuStr[mmenu_select/10][Math.Max(0, mmenu_select % 10)], new Vector2((rtSnake.Width / 2 - (meas.X / 2))-(rtSnake.Width*(1-menuShiftPos.X)), (rtSnake.Height * (.29589f))-((meas.Y/2)*(.29589f/(.69727f - .29589f) + .69727f))), new Color(mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10)].R,mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10)].G,mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10)].B,(byte)(255*(1-menuShiftPos.X))), 0, new Vector2(0, meas.Y/2), new Vector2(1, .29589f/(.69727f - .29589f)), SpriteEffects.None, 0);
                }
                menuShiftPos.X = -(1 - menuShiftPos.X);*/
            }
            rm.spritebatch.End();
            rm.graphics.GraphicsDevice.SetRenderTarget(0, null);
            texSnake = rtSnake.GetTexture();
#if !DEBUG
                    }
                    catch(Exception e)
                    {
#if WINDOWS
                        System.Windows.Forms.MessageBox.Show("Problem in Draw/MM/Pt0\n"+e.Message+"\n"+e.StackTrace);
#endif
                        Exit();
                        return;
                    }
                    try
                    {
#endif
            rm.graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
            rm.graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
            //graphics.PreferMultiSampling = true;
            rm.graphics.ApplyChanges();

            VertexDeclaration vd = new VertexDeclaration(rm.graphics.GraphicsDevice, GBVertexFormat.Elements);
            rm.graphics.GraphicsDevice.Clear(new Color(0, 0, 30, 255));
            //graphics.GraphicsDevice.
            effect.Parameters["ambientColor"].SetValue(new Vector4(0.1f, 0.1f, 0.1f, 1.0f));
            effect.Parameters["diffuseColor"].SetValue(new Vector4(0.8f, 0.8f, 0.8f, 1.0f));
            effect.Parameters["specularColor"].SetValue(new Vector4(1f, 1f, 1f, 1.0f));
            rm.graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;
            rm.graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
            rm.graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;

            Random r = new Random();

            effect.Parameters["pLightOn"].SetValue(plo);
            effect.Parameters["pLightPos"].SetValue(plp);
            effect.Parameters["pLightNear"].SetValue(pln);
            effect.Parameters["pLightFar"].SetValue(plf);
            effect.Parameters["pLightDiffuse"].SetValue(pld);
            effect.Parameters["pLightSpecular"].SetValue(pls);
            effect.Parameters["dLDiffuseColor"].SetValue(new Vector4(0.8f, 0.8f, 0.8f, 1.0f));
            effect.Parameters["dLSpecularColor"].SetValue(new Vector4(0.0f, 0.0f, 0.0f, 1.0f));
            effect.Parameters["dLightDir"].SetValue(Vector3.Normalize(new Vector3(0f, 0f, 1f)));

            Version SM = rm.graphics.GraphicsDevice.GraphicsDeviceCapabilities.PixelShaderVersion;
            if (SM.Major >= 3)
                effect.CurrentTechnique = effect.Techniques["menutechnique"];
            else if (SM.Major >= 2)
                effect.CurrentTechnique = effect.Techniques["menutechniquet"];
            else
            {
#if WINDOWS
                System.Windows.Forms.MessageBox.Show("Whoops! Your graphics card only supports Shader Model " + SM.Major + "." + SM.Minor + "\nYou need at least 2.0 to run Unsigned");
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
                Matrix matProj = Matrix.CreatePerspectiveFieldOfView((float)Math.PI / 4.0f,
                  GameSettings.windowwidth / (float)GameSettings.windowheight,
                  10f, 80.0f);
                effect.Parameters["fullbright"].SetValue(false);

                Matrix matView = Matrix.CreateLookAt(new Vector3(0, 0, 32), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
                //render the background graphics
                rm.SetViewMatrix(matView);
                effect.Parameters["proj"].SetValue(matProj);

                Matrix matRot, matScale, matTranslate;
                {//basebottom
                    matTranslate = Matrix.CreateTranslation(0, 0, 0);
                    matRot = Matrix.Identity;// Matrix.CreateRotationX((float)Math.PI);
                    matScale = Matrix.CreateScale(2, 2, 2);

                    effect.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                    effect.Parameters["wRot"].SetValue(matRot);
                    effect.Parameters["diffuseTexture"].SetValue(texSnake);
                    effect.Parameters["shininess"].SetValue(0.25f);
                    effect.Parameters["SpecularEnabled"].SetValue(true);
                    effect.Parameters["vertexAlpha"].SetValue(false);
                    effect.Parameters["BumpMappingEnabled"].SetValue(false);
                    effect.CommitChanges();

                    rm.graphics.GraphicsDevice.VertexDeclaration = vd;
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
            rm.spritebatch.Draw(songchoosetop, new Rectangle(0, (int)(-songchoosetop.Height * 0.26f), GameSettings.windowwidth, songchoosetop.Height), Color.White);
            rm.spritebatch.Draw(songchoosetop, new Rectangle(0, (int)(GameSettings.windowheight - songchoosetop.Height * .74f), GameSettings.windowwidth, songchoosetop.Height), null, Color.White, 0, new Vector2(0, 0), SpriteEffects.FlipVertically, 0);
            rm.spritebatch.Draw(GameUIMaster.GetSingleton().texButtonGreen, new Rectangle((int)(0.1f * GameSettings.windowwidth), (int)(0.80f * GameSettings.windowheight), (int)(0.09f * GameSettings.windowheight), (int)(0.09f * GameSettings.windowheight)), Color.White);
            rm.spritebatch.DrawString(Global.DefaultFont, "Select", new Vector2((0.1f * GameSettings.windowwidth) + (0.10f * GameSettings.windowheight), (0.90f * GameSettings.windowheight) - (Global.DefaultFont.MeasureString("Select").Y)), Color.White);
            if (Global.DemoMode)
            {
                rm.spritebatch.DrawString(Global.BigFont, "Demo Mode", new Vector2((GameSettings.windowwidth / 2) - (Global.BigFont.MeasureString("Demo Mode").X / 2), GameSettings.windowheight * 0.15f), new Color(255, 0, 0, 64));
                rm.spritebatch.DrawString(Global.BigFont, "Demo Mode", new Vector2((GameSettings.windowwidth / 2) - (Global.BigFont.MeasureString("Demo Mode").X / 2), GameSettings.windowheight * 0.4f), new Color(255, 0, 0, 64));
                rm.spritebatch.DrawString(Global.BigFont, "Demo Mode", new Vector2((GameSettings.windowwidth / 2) - (Global.BigFont.MeasureString("Demo Mode").X / 2), GameSettings.windowheight * 0.65f), new Color(255, 0, 0, 64));
            }
            //spritebatch.Draw(texHeader, new Rectangle((int)(GameSettings.windowwidth*0.1f), (int)(GameSettings.windowheight*0.2f), (int)(GameSettings.windowwidth*0.8f), (int)(GameSettings.windowheight*.2f)), Color.White);
            //spritebatch.DrawString(DefaultFont, "Multiplayer->Quickplay allows for Single Player play", new Vector2((GameSettings.windowwidth / 2) - (DefaultFont.MeasureString("Multiplayer->Quickplay allows for Single Player play").X / 2), GameSettings.windowheight * 0.8f), Color.White);
            rm.spritebatch.DrawString(Global.DefaultFont, "Menus in Red are not yet implemented.", new Vector2((GameSettings.windowwidth / 2) - (Global.DefaultFont.MeasureString("Menus in Red are not yet implemented.").X / 2), GameSettings.windowheight * 0.75f), Color.White);

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
                        Exit();
                        return;
                    }
#endif
            /*spritebatch.Begin();
                    if(mmenu_select>=10 && mmenu_select <20)
                        spritebatch.DrawString(DefaultFont,"SINGLE PLAYER",new Vector2(100,100),Color.Red);
                    else
                        spritebatch.DrawString(DefaultFont,"SINGLE PLAYER",new Vector2(100,100),Color.DarkRed);
                    if(mmenu_select>=20 && mmenu_select <30)
                        spritebatch.DrawString(DefaultFont,"MULTIPLAYER",new Vector2(100,150),Color.Yellow);
                    else
                        spritebatch.DrawString(DefaultFont,"MULTIPLAYER",new Vector2(100,150),Color.Gray);
                    if (mmenu_select > 20 && mmenu_select < 30)
                    {
                        if(mmenu_select==21)
                            spritebatch.DrawString(DefaultFont,"QUICKPLAY",new Vector2(400,110),Color.Yellow);
                        else
                            spritebatch.DrawString(DefaultFont,"QUICKPLAY",new Vector2(400,110),Color.Gray);
                        if(mmenu_select==22)
                            spritebatch.DrawString(DefaultFont,"BAND WORLD TOUR",new Vector2(400,160),Color.Red);
                        else
                            spritebatch.DrawString(DefaultFont,"BAND WORLD TOUR",new Vector2(400,160),Color.DarkRed);
                        if(mmenu_select==23)
                            spritebatch.DrawString(DefaultFont,"TUG OF WAR",new Vector2(400,210),Color.Red);
                        else
                            spritebatch.DrawString(DefaultFont,"TUG OF WAR",new Vector2(400,210),Color.DarkRed);
                        if(mmenu_select==24)
                            spritebatch.DrawString(DefaultFont,"SCORE BATTLE",new Vector2(400,260),Color.Red);
                        else
                            spritebatch.DrawString(DefaultFont,"SCORE BATTLE",new Vector2(400,260),Color.DarkRed);
                    }
                    if(mmenu_select>=30 && mmenu_select <40)
                        spritebatch.DrawString(DefaultFont,"OPTIONS",new Vector2(100,200),Color.Red);
                    else
                        spritebatch.DrawString(DefaultFont,"OPTIONS",new Vector2(100,200),Color.DarkRed);
                    if(mmenu_select>=40 && mmenu_select <50)
                        spritebatch.DrawString(DefaultFont,"EXIT",new Vector2(100,250),Color.Yellow);
                    else
                        spritebatch.DrawString(DefaultFont,"EXIT",new Vector2(100,250),Color.Gray);

                    
                    spritebatch.End();*/
        }
    }
}

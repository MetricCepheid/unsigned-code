using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UnsignedPeripheralPlugins;
using FVProductions.Utility;
using SongDataIO;

namespace Unsigned
{
    class IdolCreationState : BaseState
    {
        private ContentManager Content;

        private SpriteBatch spriteBatch;

        private FVShader effect;

        private Texture2D songchoosetop;
        private RenderTarget2D SongListRT;
        private Texture2D concrTex, concrBM;

        private Peripheral Peripheral;
        private BaseState previousState;

        private CharacterIdol idol;
        private Rocker VisualRocker;

        private int selectedOption;
        private bool hasSelectedOption;

        private RoundTextChooser rtChooser;

        private static String[] ShirtDesigns = { "I ♥ Metal", };
        private static String[] Pants = { "Blue Jeans", "Black Jeans", };
        private static String[] Necklaces = { "Beaded", };

        /// <summary>
        /// Create a new Idol
        /// </summary>
        public IdolCreationState(BaseState prevState, Peripheral p)
        {
            previousState = prevState;
            Peripheral = p;
            idol = CharacterIdol.RandomIdol(InstrumentMaster.Singleton.GetInstrument("LGT"));
            VisualRocker = new Rocker(idol, InstrumentMaster.Singleton.GetInstrument("LGT"));
            selectedOption = 0;
        }

        /// <summary>
        /// Edit an existing Idol
        /// </summary>
        /// <param name="avatarName"></param>
        public IdolCreationState(BaseState prevState, Peripheral p, String avatarName)
        {
            previousState = prevState;
            Peripheral = p;
        }

        public override void Load()
        {
            Content = new ContentManager(Global.Services);
            Content.RootDirectory = "Content";

            spriteBatch = new SpriteBatch(Global.Graphics.GraphicsDevice);

            SongListRT = new RenderTarget2D(Global.Graphics.GraphicsDevice, Global.ScreenHeight < 512 ? 256 : 512, Global.ScreenHeight < 512 ? 256 : 512, 1, SurfaceFormat.Color);
            songchoosetop = Content.Load<Texture2D>("textures\\SongSelect\\songscreentop");
            concrTex = Content.Load<Texture2D>("textures\\SongSelect\\concr");
            concrBM = Content.Load<Texture2D>("textures\\SongSelect\\concrBM");

            effect = new FVShader(Global.Graphics.GraphicsDevice, Content.Load<Effect>("shaders\\UnsignedEngineShader"), "maintechnique");

            rtChooser = new RoundTextChooser(Peripheral);

            VisualRocker.Load(Content);
        }

        public override void Unload()
        {
            Content.Unload();
        }

        public override void Update(GameTime gameTime)
        {
#if !DEBUG
            try
            {
#endif

            Peripheral[] peripherals = PeripheralManager.Singleton.GetPeripherals();

            int collective = 0, lr = 0;
            bool green = false, red = false, startPressed = false;

            Peripheral[] perphs = PeripheralManager.Singleton.GetPeripherals();

            for (int i = 0; i < perphs.Length; i++)
                if (perphs[i].IsConnected())
                {
                    if (perphs[i].WasPressed(PeripheralButton.DOWN))
                        collective--;
                    if (perphs[i].WasPressed(PeripheralButton.UP))
                        collective++;
                    if (perphs[i].WasPressed(PeripheralButton.CONFIRM))
                        green = true;
                    if (perphs[i].WasPressed(PeripheralButton.BACK))
                        red = true;
                    if (perphs[i].WasPressed(PeripheralButton.LEFT))
                        lr--;
                    if (perphs[i].WasPressed(PeripheralButton.RIGHT))
                        lr++;
                    if (perphs[i].WasPressed(PeripheralButton.START))
                        startPressed = true;
                }

            char? ch = rtChooser.Update(gameTime);
            if (hasSelectedOption && selectedOption == 0)
            {
                if (ch.HasValue)
                {
                    if (ch.Value == (char)0x08)
                    {
                        if (idol.Name.Length > 0)
                            idol.Name = idol.Name.Substring(0, idol.Name.Length - 1);
                    }
                    else
                        idol.Name = idol.Name + ch.Value;
                }
            }

            if (startPressed || ((Peripheral is KeyboardPeripheral) && Peripheral.WasPressed(PeripheralButton.CONFIRM)))
            {
                if (hasSelectedOption && selectedOption == 0)
                {
                    rtChooser.TurnOff();
                    hasSelectedOption = false;
                    return;
                }
            }

            if (red)
            {
                if (hasSelectedOption)
                {
                    if(selectedOption != 0)
                        hasSelectedOption = false;
                }
                else
                {
                    if (previousState == null)
                        UnsignedGame.Singleton.SwitchState(new MainMenuScreen());
                    else
                        UnsignedGame.Singleton.SwitchState(previousState);
                }
            }
            if (green)
            {
                if (!hasSelectedOption)
                {
                    if (selectedOption == 0)
                    {
                        hasSelectedOption = true;
                        rtChooser.TurnOn();
                    }
                    else if (selectedOption == 9)
                    {
                        CharacterMaster.Singleton.AddCharacter(idol);
                        if (previousState == null)
                            UnsignedGame.Singleton.SwitchState(new MainMenuScreen());
                        else
                            UnsignedGame.Singleton.SwitchState(previousState);
                    }
                    else if (selectedOption == 10)
                    {
                        
                    }
                    else
                    {
                        hasSelectedOption = true;
                    }
                }
                else
                {
                    if (selectedOption != 0)
                    {
                        hasSelectedOption = false;
                    }
                }
            }
            
            if (collective != 0)
            {
                if (!hasSelectedOption)
                {
                    selectedOption -= collective;
                    if (selectedOption < 0)
                        selectedOption = 0;
                    if (selectedOption > 10)
                        selectedOption = 10;
                }
                else
                {
                    switch (selectedOption)
                    {
                        case 1:
                            {
                                int oldCol = idol.SkinTexIndex;
                                idol.SkinTexIndex -= collective;
                                if (idol.SkinTexIndex != oldCol)
                                    VisualRocker.Load(Content);
                                break;
                            }
                        case 2:
                            {
                                int oldCol = idol.HairTexIndex;
                                idol.HairTexIndex -= collective;
                                if (idol.HairTexIndex != oldCol)
                                    VisualRocker.Load(Content);
                                break;
                            }
                        case 3:
                            {
                                CharacterIdol.IdolColor oldCol = idol.ShirtBGColor;
                                idol.ShirtBGColor -= collective;
                                if (idol.ShirtBGColor != oldCol)
                                    VisualRocker.Load(Content);
                                break;
                            }
                        case 4:
                            {
                                int oldCol = idol.ShirtFGTexIndex;
                                idol.ShirtFGTexIndex -= collective;
                                if (idol.ShirtFGTexIndex != oldCol)
                                    VisualRocker.Load(Content);
                                break;
                            }
                        case 5:
                            {
                                CharacterIdol.IdolColor oldCol = idol.ShirtFGColor;
                                idol.ShirtFGColor -= collective;
                                if (idol.ShirtFGColor != oldCol)
                                    VisualRocker.Load(Content);
                                break;
                            }
                        case 6:
                            {
                                int oldCol = idol.PantsTexIndex;
                                idol.PantsTexIndex -= collective;
                                if (idol.PantsTexIndex != oldCol)
                                    VisualRocker.Load(Content);
                                break;
                            }
                        case 7:
                            {
                                int oldCol = idol.NecklaceTexIndex;
                                idol.NecklaceTexIndex -= collective;
                                if (idol.NecklaceTexIndex != oldCol)
                                    VisualRocker.Load(Content);
                                break;
                            }
                        case 8:
                            {
                                CharacterIdol.IdolColor oldCol = idol.NecklaceColor;
                                idol.NecklaceColor -= collective;
                                if (idol.NecklaceColor != oldCol)
                                    VisualRocker.Load(Content);
                                break;
                            }
                    }
                }
            }
#if !DEBUG
            }
            catch(Exception e)
            {
                Debug.Error("Problem in SongSelectScreen.Update[1]",e);
                UnsignedGame.Singleton.Exit();
                return;
            }
            try
            {
#endif
            
#if !DEBUG
            }
            catch(Exception e)
            {
                Debug.Error("Problem in SongSelectScreen.Update[2]",e);
                UnsignedGame.Singleton.Exit();
                return;
            }
#endif
        }

        public override void Render(GameTime gameTime)
        {
#if !DEBUG
            try
            {
#endif
            Global.Graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
            Global.Graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
            Global.Graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;
            Global.Graphics.ApplyChanges();

            Global.Graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            effect.NormalMapTexture = Global.TexDefaultBM;
            effect.SpecularMapTexture = Global.TexWhite;
            effect.AmbientMaterial = new Color(24, 24, 24);
            effect.DiffuseMaterial = new Color(255, 255, 255);
            effect.SpecularMaterial = new Color(255, 255, 255);
            effect.Shininess = 24f;

            effect.TextureEnabled = true;
            effect.LightingEnabled = Configuration.Lighting;
            effect.SpecularEnabled = Configuration.Specular;
            effect.NormalMapEnabled = Configuration.NormalMapping;

            effect.DirectionalLight = new DirectionalLight(true, new Vector3(0, 1, 2), Color.White, Color.White);

            effect.CommitChanges();

            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();

                effect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, Global.ScreenWidth / (float)Global.ScreenHeight, 1f, 1000f);
                effect.View = Matrix.CreateLookAt(new Vector3(0, 128, 100), new Vector3(0, 128, 0), new Vector3(0, 1, 0));

                Matrix matRot, matScale, matTranslate;

                {
                    matTranslate = Matrix.CreateTranslation(-32, 120, -128);
                    matRot = Matrix.CreateRotationX((float)Math.PI / 2);
                    matScale = Matrix.CreateScale(200, 1, 128);

                    effect.World = matScale * matRot * matTranslate;
                    effect.DiffuseTexture = concrTex;
                    effect.NormalMapTexture = concrBM;
                    effect.CommitChanges();

                    Global.Graphics.GraphicsDevice.DrawSquare();
                }

                {
                    VisualRocker.Position = new Vector3(20, 80, 0);
                    VisualRocker.Yaw = 0;
                    effect.NormalMapTexture = Global.TexDefaultBM;
                    effect.SpecularMapTexture = Global.TexWhite;
                    effect.SpecularMaterial = new Color(0.1f, 0.1f, 0.1f);
                    VisualRocker.Draw(new SongTime(gameTime.ElapsedGameTime, gameTime.TotalGameTime), effect);
                }

                pass.End();
            }
            effect.End();
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.None);

            spriteBatch.Draw(Global.TexWhite, new Rectangle(0, 0, (int)(Global.ScreenWidth * 0.4f), Global.ScreenHeight), Color.Black);

            Color selCol = (hasSelectedOption ? Global.UnsignedOrange : Global.UnsignedYellow);
            Color unselCol = Color.White;
            spriteBatch.DrawString(Global.DefaultFont, "Name: " + idol.Name, new Rectangle((int)(Global.ScreenWidth * 0.1f), (int)(Global.ScreenHeight * 0.2f), (int)(Global.ScreenWidth * 0.28f), 1000), selectedOption == 0 ? selCol : unselCol, 0, Vector2.Zero, Global.ScreenHeight / 1000f, SpriteEffects.None, 0);
            spriteBatch.DrawString(Global.DefaultFont, "Skin Color: " + idol.SkinTexIndex, new Rectangle((int)(Global.ScreenWidth * 0.1f), (int)(Global.ScreenHeight * 0.23f), (int)(Global.ScreenWidth * 0.28f), 1000), selectedOption == 1 ? selCol : unselCol, 0, Vector2.Zero, Global.ScreenHeight / 1000f, SpriteEffects.None, 0);
            spriteBatch.DrawString(Global.DefaultFont, "Hair Color: " + idol.HairTexIndex, new Rectangle((int)(Global.ScreenWidth * 0.1f), (int)(Global.ScreenHeight * 0.26f), (int)(Global.ScreenWidth * 0.28f), 1000), selectedOption == 2 ? selCol : unselCol, 0, Vector2.Zero, Global.ScreenHeight / 1000f, SpriteEffects.None, 0);
            spriteBatch.DrawString(Global.DefaultFont, "Shirt Color: " + idol.ShirtBGColor, new Rectangle((int)(Global.ScreenWidth * 0.1f), (int)(Global.ScreenHeight * 0.29f), (int)(Global.ScreenWidth * 0.28f), 1000), selectedOption == 3 ? selCol : unselCol, 0, Vector2.Zero, Global.ScreenHeight / 1000f, SpriteEffects.None, 0);
            spriteBatch.DrawString(Global.DefaultFont, "Shirt Design: " + ShirtDesigns[idol.ShirtFGTexIndex], new Rectangle((int)(Global.ScreenWidth * 0.1f), (int)(Global.ScreenHeight * 0.32f), (int)(Global.ScreenWidth * 0.28f), 1000), selectedOption == 4 ? selCol : unselCol, 0, Vector2.Zero, Global.ScreenHeight / 1000f, SpriteEffects.None, 0);
            spriteBatch.DrawString(Global.DefaultFont, "Shirt Design Color: " + idol.ShirtFGColor, new Rectangle((int)(Global.ScreenWidth * 0.1f), (int)(Global.ScreenHeight * 0.35f), (int)(Global.ScreenWidth * 0.28f), 1000), selectedOption == 5 ? selCol : unselCol, 0, Vector2.Zero, Global.ScreenHeight / 1000f, SpriteEffects.None, 0);
            spriteBatch.DrawString(Global.DefaultFont, "Pants: " + Pants[idol.PantsTexIndex], new Rectangle((int)(Global.ScreenWidth * 0.1f), (int)(Global.ScreenHeight * 0.38f), (int)(Global.ScreenWidth * 0.28f), 1000), selectedOption == 6 ? selCol : unselCol, 0, Vector2.Zero, Global.ScreenHeight / 1000f, SpriteEffects.None, 0);
            spriteBatch.DrawString(Global.DefaultFont, "Necklace Type: " + Necklaces[idol.NecklaceTexIndex], new Rectangle((int)(Global.ScreenWidth * 0.1f), (int)(Global.ScreenHeight * 0.41f), (int)(Global.ScreenWidth * 0.28f), 1000), selectedOption == 7 ? selCol : unselCol, 0, Vector2.Zero, Global.ScreenHeight / 1000f, SpriteEffects.None, 0);
            spriteBatch.DrawString(Global.DefaultFont, "Necklace Color: " + idol.NecklaceColor, new Rectangle((int)(Global.ScreenWidth * 0.1f), (int)(Global.ScreenHeight * 0.44f), (int)(Global.ScreenWidth * 0.28f), 1000), selectedOption == 8 ? selCol : unselCol, 0, Vector2.Zero, Global.ScreenHeight / 1000f, SpriteEffects.None, 0);

            spriteBatch.DrawString(Global.DefaultFont, "Create", new Rectangle((int)(Global.ScreenWidth * 0.1f), (int)(Global.ScreenHeight * 0.60f), (int)(Global.ScreenWidth * 0.28f), 1000), selectedOption == 9 ? selCol : unselCol, 0, Vector2.Zero, Global.ScreenHeight / 1000f, SpriteEffects.None, 0);
            spriteBatch.DrawString(Global.DefaultFont, "Cancel", new Rectangle((int)(Global.ScreenWidth * 0.1f), (int)(Global.ScreenHeight * 0.63f), (int)(Global.ScreenWidth * 0.28f), 1000), selectedOption == 10 ? selCol : unselCol, 0, Vector2.Zero, Global.ScreenHeight / 1000f, SpriteEffects.None, 0);

            rtChooser.Draw(spriteBatch);

            spriteBatch.Draw(songchoosetop, new Rectangle(0, -(int)(Global.ScreenHeight*0.1f), Global.ScreenWidth, (int)((Global.ScreenHeight / 768f) * 256)), Color.White);
            spriteBatch.End();
#if !DEBUG
            }
            catch(Exception e)
            {
                Debug.Error("Problem in SongSelectSceen.Draw[1]", e);
                UnsignedGame.Singleton.Exit();
                return;
            }
#endif
        }
    }
}

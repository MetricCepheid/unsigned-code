using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FVProductions.Utility;

namespace Unsigned
{
    class FVLogoScreen : BaseState
    {
        private ContentManager Content;

        private SpriteBatch spriteBatch;

        private FVShader effect;

        private float logoTime;
        Texture2D texBarrel, texGoo1, texGoo2, texPresser;
        Texture2D texGooBM, texPresserBM;
        FVModel mBarrel, mGoo1, mGoo2, mPresser;
        

        public FVLogoScreen()
        {
            
        }

        public override void Load()
        {
            Content = new ContentManager(Global.Services);
            Content.RootDirectory = "Content";

            spriteBatch = new SpriteBatch(Global.Graphics.GraphicsDevice);

            texBarrel = Content.Load<Texture2D>("textures\\FVLogo\\barrel");
            texGoo1 = Content.Load<Texture2D>("textures\\FVLogo\\goo1");
            texGoo2 = Content.Load<Texture2D>("textures\\FVLogo\\goo2");
            texPresser = Content.Load<Texture2D>("textures\\FVLogo\\presser");

            texGooBM = Content.Load<Texture2D>("textures\\FVLogo\\goobm");
            texPresserBM = Content.Load<Texture2D>("textures\\FVLogo\\presserbm");

            mBarrel = ModelLoader.LoadModel("meshes\\FVLogo\\barrel");
            mGoo1 = ModelLoader.LoadModel("meshes\\FVLogo\\goo1");
            mGoo2 = ModelLoader.LoadModel("meshes\\FVLogo\\goo2");
            mPresser = ModelLoader.LoadModel("meshes\\FVLogo\\presser");

            effect = new FVShader(Global.Graphics.GraphicsDevice, Content.Load<Effect>("shaders\\UnsignedEngineShader"), "maintechnique");

        }

        public override void Unload()
        {
            Content.Unload();
        }

        public override void Update(GameTime gameTime)
        {
            if (logoTime >= 35)
                UnsignedGame.Singleton.SwitchState(new MainMenuScreen());
            if (logoTime < 35)
#if DEBUG
                logoTime += (float)gameTime.ElapsedGameTime.TotalSeconds * 100;
#else
                logoTime += (float)gameTime.ElapsedGameTime.TotalSeconds * 8;
#endif
        }

        public override void Render(GameTime gameTime)
        {
            Global.Graphics.GraphicsDevice.Clear(Color.Black);

            try
            {
                Global.Graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
                Global.Graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
                //graphics.PreferMultiSampling = true;
                Global.Graphics.ApplyChanges();

                Global.Graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;
                Global.Graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
                Global.Graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;

                effect.AmbientMaterial = new Color(24, 24, 24);
                effect.DiffuseMaterial = Color.White;
                effect.SpecularMaterial = Color.White;
                effect.DirectionalLight = new DirectionalLight(true, new Vector3(-1, 3, -1), new Color(200, 200, 200), Color.White);
                effect.NormalMapTexture = Global.TexDefaultBM;
                effect.LightingEnabled = Configuration.Lighting;
                effect.SpecularEnabled = Configuration.Specular;
                effect.NormalMapEnabled = Configuration.NormalMapping;
                effect.SpecularMaterial = Color.White;
                effect.Shininess = 12.0f;
                effect.TextureEnabled = true;
                effect.CommitChanges();
            }
            catch(Exception e)
            {
                Debug.Error("Problem in FVLogoScreen.Draw[1]", e);
                UnsignedGame.Singleton.Exit();
                return;
            }

            try
            {
                effect.Begin();
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Begin();
                    Matrix matProj = Matrix.CreatePerspectiveFieldOfView((float)Math.PI / 4.0f,
                      Global.ScreenWidth / (float)Global.ScreenHeight,
                      1f, 40.0f);
                    Vector3 camPos = new Vector3(20, 8, 0);
                    if (logoTime < 20)
                        camPos.X = 20;
                    else if (logoTime < 30)
                        camPos.X = ((((logoTime - 20) / 10f)) * 12) + ((1 - ((logoTime - 20) / 10f)) * 20);
                    else
                        camPos.X = 12;
                    if (logoTime < 20)
                        camPos.Y = 8;
                    else if (logoTime < 30)
                        camPos.Y = ((((logoTime - 20) / 10f)) * 12) + ((1 - ((logoTime - 20) / 10f)) * 8);
                    else
                        camPos.Y = 12;

                    camPos = Vector3.Transform(camPos, Matrix.CreateRotationY(logoTime < 30 ? (float)(Math.PI * 3 / 4f) + (float)((logoTime / 30f) * (Math.PI * 3 / 4f)) : (float)(Math.PI * 1.5f)));

                    Vector3 target = new Vector3(0, 0, 0);

                    if (logoTime < 20)
                        camPos.X -= 0;
                    else if (logoTime < 30)
                    { target.X -= ((((logoTime - 20) / 10f)) * 1); camPos.X -= ((((logoTime - 20) / 10f)) * 1); }
                    else
                    { target.X -= 1; camPos.X -= 1; }

                    Matrix matView = Matrix.CreateLookAt(camPos, target, new Vector3(0, 1, 0));
                    //render the background graphics
                    effect.View = matView;
                    effect.Projection = matProj;

                    Matrix matRot, matScale, matTranslate;
                    {//goo1
                        matTranslate = Matrix.CreateTranslation(0, 0, 0);
                        matRot = Matrix.Identity;// Matrix.CreateRotationX((float)Math.PI);
                        matScale = Matrix.CreateScale(1, 1, 1);

                        effect.World = matScale * matRot * matTranslate;
                        if (logoTime < 17)
                            effect.DiffuseTexture = texGoo1;
                        else
                            effect.DiffuseTexture = texGoo2;

                        effect.NormalMapTexture = texGooBM;
                        effect.SpecularMaterial = new Color(255,200,0);

                        effect.CommitChanges();

                        if (logoTime < 17)
                            mGoo1.Draw();
                        else
                            mGoo2.Draw();
                    }
                    {//barrel
                        matTranslate = Matrix.CreateTranslation(-10.2f, 0, -6.8f);
                        matRot = Matrix.CreateRotationY((float)Math.PI * 1.25f);
                        matScale = Matrix.CreateScale(1, 1, 1);

                        effect.World = matScale * matRot * matTranslate;
                        effect.DiffuseTexture = texBarrel;
                        effect.NormalMapTexture = texPresserBM;
                        effect.SpecularMaterial = new Color(0.3f, 0.3f, 0.3f);
                        effect.CommitChanges();

                        mBarrel.Draw();
                    }
                    {//presser
                        float y = 30;
                        if (logoTime > 5 && logoTime < 15)
                            y = (10 - (logoTime - 5)) * 3;
                        else if (logoTime >= 15 && logoTime < 20)
                            y = 0;
                        else if (logoTime >= 20 && logoTime < 30)
                            y = (logoTime - 20) * 3;
                        matTranslate = Matrix.CreateTranslation(0, y, 0);
                        matRot = Matrix.Identity;//Matrix.CreateRotationY((float)Math.PI * 1.25f);
                        matScale = Matrix.CreateScale(1, 1, 1);

                        effect.World = matScale * matRot * matTranslate;
                        effect.DiffuseTexture = texPresser;
                        effect.NormalMapTexture = texPresserBM;
                        effect.SpecularMaterial = new Color(0.3f,0.3f,0.3f);
                        effect.CommitChanges();

                        mPresser.Draw();
                    }
                    pass.End();
                }
                effect.End();

                spriteBatch.Begin();
                if (logoTime < 5)
                    spriteBatch.Draw(Global.TexWhite, new Rectangle(0, 0, Global.ScreenWidth, Global.ScreenHeight), new Color(0, 0, 0, (byte)(255 * (1 - ((logoTime) / 5f)))));
                if (logoTime > 30 && logoTime < 35)
                    spriteBatch.Draw(Global.TexWhite, new Rectangle(0, 0, Global.ScreenWidth, Global.ScreenHeight), new Color(0, 0, 0, (byte)(255 * (((logoTime - 30) / 5f)))));
                else if (logoTime >= 35)
                    spriteBatch.Draw(Global.TexWhite, new Rectangle(0, 0, Global.ScreenWidth, Global.ScreenHeight), Color.Black);
                spriteBatch.End();
            }
            catch(Exception e)
            {
                Debug.Error("Problem in FVLogoScreen.Draw[2]", e);
                UnsignedGame.Singleton.Exit();
                return;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FVProductions.Utility;

namespace Unsigned
{
    class CurtainLoadingScreen : BaseState
    {
        private ContentManager Content;

        private FVShader effect;

        private FVModel mCurtain;
        private Texture2D texCurtainLeft, texCurtainRight;

        public CurtainLoadingScreen()
        {

        }

        public override void Load()
        {
            Content = new ContentManager(Global.Services);
            Content.RootDirectory = "Content";

            effect = new FVShader(Global.Graphics.GraphicsDevice, Content.Load<Effect>("shaders\\UnsignedEngineShader"), "maintechnique");
        }

        public override void Unload()
        {
            Content.Unload();
        }

        public override void Update(GameTime gameTime)
        {
        }

        public override void Render(GameTime gameTime)
        {
            Global.Graphics.GraphicsDevice.Clear(Color.Black);

            Global.Graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
            Global.Graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
            //graphics.PreferMultiSampling = true;
            Global.Graphics.ApplyChanges();

            Global.Graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;
            Global.Graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
            Global.Graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;

            effect.DiffuseMaterial = Color.White;
            effect.DirectionalLight = new DirectionalLight(true, new Vector3(-1, -3, -1), new Color(200, 200, 200), Color.White);
            effect.SpecularMaterial = Color.Black;
            effect.Shininess = 12.0f;
            effect.TextureEnabled = true;

            effect.LightingEnabled = Configuration.Lighting;
            effect.SpecularEnabled = Configuration.Specular;
            effect.NormalMapEnabled = Configuration.NormalMapping;

            effect.CommitChanges();
            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();

                Matrix matProj = Matrix.CreatePerspectiveFieldOfView((float)Math.PI / 4.0f,
                      Global.ScreenWidth / (float)Global.ScreenHeight,
                      1f, 40.0f);

                Matrix matView = Matrix.CreateLookAt(new Vector3(0, 0, 7), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
                //render the background graphics
                effect.View = matView;
                effect.Projection = matProj;

                Matrix matRot, matScale, matTranslate;
                matTranslate = Matrix.CreateTranslation(-2, 0, 0);
                matRot = Matrix.Identity;// Matrix.CreateRotationX((float)Math.PI);
                matScale = Matrix.CreateScale(1, 2, 1);

                effect.World = matScale * matRot * matTranslate;
                effect.DiffuseTexture = texCurtainLeft;
                effect.CommitChanges();

                mCurtain.Draw();

                matTranslate = Matrix.CreateTranslation(2, 0, 0);
                matRot = Matrix.Identity;// Matrix.CreateRotationX((float)Math.PI);
                matScale = Matrix.CreateScale(1, 2, 1);

                effect.World = matScale * matRot * matTranslate;
                effect.DiffuseTexture = texCurtainRight;
                effect.CommitChanges();

                mCurtain.Draw();
                pass.End();
            }
            effect.End();
        }
    }
}

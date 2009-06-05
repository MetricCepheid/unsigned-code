using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using FVProductions.Utility;
using System.IO;

namespace AnimationEditor
{
    public class AnimationEditorViewer : XNAComponent
    {
        private float yaw, pitch;

        private Point lastMousePos;
        private int lastMouseWheelPos;

        private AnimationVertex[] headVerts, torsoVerts, legsVerts;

        private Effect effect;
        private SpriteBatch spriteBatch;

        public AnimationMatrixInfo animInfo;

        private Matrix Projection, View;
        private Vector3 EyePos;
        private float Scale;

        public VoidDelegate SelectedVertChanged;

        private Dictionary<String, FVModel> models;

        public AnimationEditorViewer(String name)
        {
            Name = name;
            yaw = 0;
            pitch = 0;
            EyePos = new Vector3(0, 0, 12);
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, Bounds.Width / (float)Bounds.Height, 0.1f, 100f);
            View = Matrix.CreateLookAt(EyePos, new Vector3(0, 0, 0), Vector3.Up);
            Scale = 1f;
            lastMouseWheelPos = Mouse.GetState().ScrollWheelValue;
        }

        public override void Load(ContentManager Content)
        {
            effect = Content.Load<Effect>("NormalMappedShader");
            spriteBatch = new SpriteBatch(Global.Graphics.GraphicsDevice);
            models = new Dictionary<String, FVModel>();
            String[] files = Directory.GetFiles("Content\\bodyparts\\");
            for (int i = 0; i < files.Length; i++)
            {
                String str = files[i].Substring(files[i].LastIndexOf('\\')+1);
                str = str.Substring(0, str.LastIndexOf('.'));
                models.Add(str.ToUpper(), FVProductions.Utility.ModelLoader.LoadModel("bodyparts\\" + str));
            }
            animInfo = new AnimationMatrixInfo();
            animInfo.Load(File.OpenRead("DefaultHierarchy.txt"));
        }

        public override void Update(GameTime gameTime)
        {
            if (Mouse.GetState().MiddleButton == ButtonState.Pressed && Bounds.Contains(lastMousePos))
            {
                float dx = Mouse.GetState().X - lastMousePos.X;
                float dy = Mouse.GetState().Y - lastMousePos.Y;
                yaw += dx * 0.01f;
                pitch += dy * 0.01f;
            }

            int mbch = Mouse.GetState().ScrollWheelValue - lastMouseWheelPos;
            lastMouseWheelPos = Mouse.GetState().ScrollWheelValue;
            float scaleChange = 1.0f;
            while (mbch > 0)
            {
                scaleChange *= 1.0001f;
                mbch--;
            }
            while (mbch < 0)
            {
                scaleChange *= 0.9999f;
                mbch++;
            }
            Scale *= scaleChange;

            lastMousePos = new Point(Mouse.GetState().X, Mouse.GetState().Y);

            while (pitch > MathHelper.Pi)
                pitch -= MathHelper.TwoPi;
            while (pitch < -MathHelper.Pi)
                pitch += MathHelper.TwoPi;
            while (yaw > MathHelper.Pi)
                yaw -= MathHelper.TwoPi;
            while (yaw < -MathHelper.Pi)
                yaw += MathHelper.TwoPi;
        }

        public override void InnerDraw()
        {
            Global.Graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;

            effect.Parameters["LightDirection"].SetValue(Vector3.Normalize(new Vector3(0.1f, 1, 1)));
            effect.Parameters["LightDiffuse"].SetValue(new Vector4(1, 1, 1, 1));
            effect.Parameters["LightSpecular"].SetValue(new Vector4(1, 1, 1, 1));
            effect.Parameters["Ambient"].SetValue(new Vector4(0.1f, 0.1f, 0.1f, 1));
            effect.Parameters["Diffuse"].SetValue(new Vector4(1f, 1f, 1f, 1f));
            effect.Parameters["Specular"].SetValue(new Vector4(1f, 1f, 1f, 1));
            effect.Parameters["SpecularPower"].SetValue(24f);
            effect.Parameters["Projection"].SetValue(Projection);
            effect.Parameters["View"].SetValue(View);
            effect.Parameters["EyePos"].SetValue(EyePos);
            Matrix World = Matrix.CreateRotationY(yaw) * Matrix.CreateRotationX(pitch) * Matrix.CreateScale(Scale);
            effect.Parameters["World"].SetValue(World);
            effect.Parameters["specularTex"].SetValue(Global.TexWhite);
            effect.Parameters["diffuseTex"].SetValue(Global.TexWhite);
            effect.Parameters["normalTex"].SetValue(Global.TexDefaultBM);
            effect.CurrentTechnique = effect.Techniques["maintechnique"];

            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();

                Joint j = animInfo.RootJoint;

                DrawJoint(j, Matrix.CreateRotationY(yaw) * Matrix.CreateRotationX(pitch));

                pass.End();
            }
            effect.End();

            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.None);

            spriteBatch.End();
        }

        private void DrawJoint(Joint j, Matrix world)
        {
            world = Matrix.CreateTranslation(j.preOffset) * world * Matrix.CreateTranslation(j.postOffset);
            if (models.ContainsKey(j.Name.ToUpper()))
            {
                effect.Parameters["World"].SetValue(world);
                models[j.Name.ToUpper()].Draw();
            }
            for (int i = 0; i < j.Children.Count; i++)
                DrawJoint(j.Children[i], world);
        }

        public void ResetView()
        {
            yaw = 0;
            pitch = 0;
        }

        public override void OnResize()
        {
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, Bounds.Width / (float)Bounds.Height, 0.1f, 100f);
            base.OnResize();
        }
    }
}

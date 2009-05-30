using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace AnimationEditor
{
    public class EditorViewer : XNAComponent
    {
        private class AnimationOffsetData
        {
            public class AnimationOffset
            {
                public int index;
                public Vector3 offset1, offset2;

                public AnimationOffset(int index)
                {
                    this.index = index;
                    offset1 = Vector3.Zero;
                    offset2 = Vector3.Zero;
                }
            }

            public String Name { get; private set; }
            public List<AnimationOffset> offsets;

            public AnimationOffsetData(String name)
            {
                Name = name;
                offsets = new List<AnimationOffset>();
            }
        }

        private float yaw, pitch;

        private Point lastMousePos;
        private int lastMouseWheelPos;

        private AnimatedModel _headModel = null;
        public AnimatedModel HeadModel
        {
            get { return _headModel; }
            set
            {
                _headModel = value;
                List<VertexPositionColor> verts = new List<VertexPositionColor>();
                for (int i = 0; i < _headModel.Verts.Length; i++)
                {
                    verts.Add(new VertexPositionColor(_headModel.Verts[i].Position, Color.Lime));
                }
                lineVerts = verts.ToArray();
            }
        }
        private AnimatedModel _torsoModel = null;
        public AnimatedModel TorsoModel
        {
            get { return _torsoModel; }
            set
            {
                _torsoModel = value;
            }
        }
        private AnimatedModel _legsModel = null;
        public AnimatedModel LegsModel
        {
            get { return _legsModel; }
            set
            {
                _legsModel = value;
            }
        }
        private AnimatedModel _eyeModel = null;
        public AnimatedModel EyeModel
        {
            get { return _eyeModel; }
            set { _eyeModel = value; }
        }
        private Texture2D texEye, texHead, texHeadNM, texBody;

        private AnimationVertex[] headVerts, torsoVerts, legsVerts;

        private bool linesDirty;
        public VertexPositionColor[] lineVerts;
        public int[] lineIndices;

        private bool[] hiddenVerts;

        private Effect effect;
        private SpriteBatch spriteBatch;

        private Vector3 rightEyePos, leftEyePos;
        private Vector3 eyeTarget;

        private bool showDetails;

        public AnimationInfo animInfo;

        private Matrix Projection, View;
        private Vector3 EyePos;
        private float Scale;

        private int selectedVert = -1;

        private List<AnimationOffsetData> animationOffsets;

        public VoidDelegate SelectedVertChanged;

        public EditorViewer(String name)
        {
            Name = name;
            yaw = 0;
            pitch = 0;
            rightEyePos = new Vector3(-0.3331f, 0.3465f, 0.6603f);
            leftEyePos = new Vector3( 0.3331f, 0.3465f, 0.6603f);
            showDetails = true;
            EyePos = new Vector3(0, 0, 12);
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, Bounds.Width / (float)Bounds.Height, 0.1f, 100f);
            View = Matrix.CreateLookAt(EyePos, new Vector3(0, 0, 0), Vector3.Up);
            animationOffsets = new List<AnimationOffsetData>();
            Scale = 1f;
            lastMouseWheelPos = Mouse.GetState().ScrollWheelValue;
        }

        public override void Load(ContentManager Content)
        {
            effect = Content.Load<Effect>("NormalMappedShader");
            spriteBatch = new SpriteBatch(Global.Graphics.GraphicsDevice);

            texEye = Content.Load<Texture2D>("eyetex");
            texHead = Content.Load<Texture2D>("headtex");
            texHeadNM = Content.Load<Texture2D>("headtexbm");

            HeadModel = ModelLoader.LoadModel("head");
            TorsoModel = ModelLoader.LoadModel("torso");
            LegsModel = ModelLoader.LoadModel("legs");
            EyeModel = ModelLoader.LoadModel("eye");

            headVerts = new AnimationVertex[HeadModel.Verts.Length];
            torsoVerts = new AnimationVertex[TorsoModel.Verts.Length];
            for (int i = 0; i < headVerts.Length; i++)
                headVerts[i] = HeadModel.Verts[i];
            hiddenVerts = new bool[HeadModel.Verts.Length];
        }

        public override void Update(GameTime gameTime)
        {
            // add personal animationOffsets if an animInfo is added
            for (int i = 0; i < animInfo.Count; i++)
            {
                bool exists = false;
                for (int k = 0; k < animationOffsets.Count; k++)
                    if (animInfo[i].Name == animationOffsets[k].Name)
                        exists = true;
                if (!exists)
                {
                    animationOffsets.Add(new AnimationOffsetData(animInfo[i].Name));
                }
            }
            // update based on new data
            if (animInfo.Dirty)
            {
                animInfo.Dirty = false;
                linesDirty = true;
                for (int i = 0; i < headVerts.Length; i++)
                    headVerts[i] = HeadModel.Verts[i];
                for (int i = 0; i < torsoVerts.Length; i++)
                    torsoVerts[i] = TorsoModel.Verts[i];
                for (int i = 0; i < animationOffsets.Count; i++)
                {
                    float val = animInfo[animationOffsets[i].Name].Value;
                    for (int k = 0; k < animationOffsets[i].offsets.Count; k++)
                    {
                        Vector3 offset = ((1-val)*animationOffsets[i].offsets[k].offset1)+((val)*animationOffsets[i].offsets[k].offset2);
                        AnimationVertex vert = headVerts[animationOffsets[i].offsets[k].index];
                        headVerts[animationOffsets[i].offsets[k].index] = new AnimationVertex(vert.Position + offset, vert.Normal, vert.TexCoords, vert.Binormal, vert.Tangent);
                    }
                }
            }
            if (lineIndices == null)
            {
                List<int> indices1 = new List<int>();
                List<int> indices2 = new List<int>();
                for (int i = 0; i < HeadModel.Indices.Length; i += 3)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        int ind1 = HeadModel.Indices[i + k];
                        int ind2 = HeadModel.Indices[i + ((k + 1) % 3)];
                        bool found = false;
                        for (int j = 0; j < indices1.Count; j++)
                        {
                            if ((indices1[j] == ind1 && indices2[j] == ind2) || (indices1[j] == ind2 && indices2[j] == ind1))
                            {
                                found = true;
                            }
                        }
                        if (!found)
                        {
                            indices1.Add(ind1);
                            indices2.Add(ind2);
                        }
                    }
                }
                List<int> lineInd = new List<int>();
                for (int i = 0; i < indices1.Count; i++)
                {
                    lineInd.Add(indices1[i]);
                    lineInd.Add(indices2[i]);
                }
                lineIndices = lineInd.ToArray();
            }
            if (Mouse.GetState().MiddleButton == ButtonState.Pressed && Bounds.Contains(lastMousePos))
            {
                float dx = Mouse.GetState().X - lastMousePos.X;
                float dy = Mouse.GetState().Y - lastMousePos.Y;
                yaw += dx * 0.01f;
                pitch += dy * 0.01f;
            }
            if (Mouse.GetState().LeftButton == ButtonState.Pressed && Bounds.Contains(lastMousePos))
            {
                Viewport v = new Viewport();
                v.Width = Bounds.Width;
                v.Height = Bounds.Height;
                Matrix World = Matrix.CreateRotationY(yaw) * Matrix.CreateRotationX(pitch);
                Vector3 mousePos = new Vector3(Mouse.GetState().X - Bounds.X, Mouse.GetState().Y - Bounds.Y, 0);
                int bestIndex = -1;
                float bestValue = 0;
                for (int i = 0; i < headVerts.Length; i++)
                    if(!hiddenVerts[i])
                    {
                        Vector3 pos = v.Project(headVerts[i].Position, Projection, View, World);
                        pos.Z = 0;
                        float value = Vector3.DistanceSquared(pos, mousePos);
                        if (Vector3.Transform(headVerts[i].Normal, World).Z >= 0)
                        {
                            if (bestIndex < 0)
                            {
                                bestIndex = i;
                                bestValue = value;
                            }
                            else
                            {
                                if (value < bestValue)
                                {
                                    bestIndex = i;
                                    bestValue = value;
                                }
                            }
                        }
                    }
                selectedVert = bestIndex;
                if (SelectedVertChanged != null)
                    SelectedVertChanged.Invoke();
            }

            if (selectedVert >= 0)
                if(Keyboard.GetState().IsKeyDown(Keys.LeftControl) && Keyboard.GetState().IsKeyDown(Keys.H))
                {
                    hiddenVerts[selectedVert] = true;
                    selectedVert = -1;
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
            eyeTarget = Vector3.Transform(new Vector3(2,0,0),Matrix.CreateRotationZ((float)gameTime.TotalGameTime.TotalSeconds));
            eyeTarget.Y *= 0.75f;
            eyeTarget += new Vector3(0, 0.3465f, 10);

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
            effect.Parameters["Specular"].SetValue(new Vector4(0.1f, 0.1f, 0.1f, 1));
            effect.Parameters["SpecularPower"].SetValue(24f);
            effect.Parameters["Projection"].SetValue(Projection);
            effect.Parameters["View"].SetValue(View);
            effect.Parameters["EyePos"].SetValue(EyePos);
            Matrix World = Matrix.CreateRotationY(yaw) * Matrix.CreateRotationX(pitch) * Matrix.CreateScale(Scale);
            effect.Parameters["World"].SetValue(World);
            effect.Parameters["specularTex"].SetValue(Global.TexWhite);
            effect.Parameters["normalTex"].SetValue(Global.TexDefaultBM);
            effect.CurrentTechnique = effect.Techniques["maintechnique"];

            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();

                if (HeadModel != null)
                {
                    if (showDetails)
                    {
                        effect.Parameters["diffuseTex"].SetValue(texHead);
                        effect.Parameters["normalTex"].SetValue(texHeadNM);
                        effect.Parameters["Diffuse"].SetValue(new Vector4(1f, 1f, 1f, 1f));
                        effect.Parameters["Specular"].SetValue(new Vector4(0.1f, 0.1f, 0.1f, 1));
                    }
                    else
                    {
                        effect.Parameters["diffuseTex"].SetValue(Global.TexWhite);
                        effect.Parameters["normalTex"].SetValue(Global.TexDefaultBM);
                        effect.Parameters["Diffuse"].SetValue(new Vector4(0.1f, 0.1f, 0.1f, 1f));
                        effect.Parameters["Specular"].SetValue(new Vector4(0.0f, 0.0f, 0.0f, 1));
                    }
                    effect.CommitChanges();
                    Global.Graphics.GraphicsDevice.VertexDeclaration = AnimationVertex.VertexDeclaration;
                    //Global.Graphics.GraphicsDevice.DrawUserIndexedPrimitives<AnimationVertex>(PrimitiveType.TriangleList, headVerts, 0, headVerts.Length, HeadModel.Indices, 0, HeadModel.Indices.Length / 3);

                    if (showDetails)
                    {
                        effect.Parameters["diffuseTex"].SetValue(Global.TexWhite);
                        effect.Parameters["normalTex"].SetValue(Global.TexDefaultBM);
                        effect.Parameters["Diffuse"].SetValue(new Vector4(1f, 1f, 1f, 1f));
                        effect.Parameters["Specular"].SetValue(new Vector4(1f, 1f, 1f, 1));
                    }
                    else
                    {
                        effect.Parameters["diffuseTex"].SetValue(Global.TexWhite);
                        effect.Parameters["normalTex"].SetValue(Global.TexDefaultBM);
                        effect.Parameters["Diffuse"].SetValue(new Vector4(0.1f, 0.1f, 0.1f, 1f));
                        effect.Parameters["Specular"].SetValue(new Vector4(0.0f, 0.0f, 0.0f, 1));
                    }
                    effect.CommitChanges();
                    Global.Graphics.GraphicsDevice.DrawUserIndexedPrimitives<AnimationVertex>(PrimitiveType.TriangleList, torsoVerts, 0, torsoVerts.Length, TorsoModel.Indices, 0, TorsoModel.Indices.Length / 3);

                    {
                        float eyaw = Math.Max(-0.5f, Math.Min(0.5f, -yaw))+MathHelper.PiOver2;//(float)Math.Atan2(eyeTarget.Z - rightEyePos.Z, eyeTarget.X - rightEyePos.X);
                        float epitch = Math.Max(-0.3f, Math.Min(0.3f, -pitch));// (float)Math.Atan2(eyeTarget.Y - rightEyePos.Y, Vector2.Distance(new Vector2(eyeTarget.X, eyeTarget.Z), new Vector2(rightEyePos.X, rightEyePos.Z)));
                        effect.Parameters["World"].SetValue(Matrix.CreateScale(0.15f) * Matrix.CreateRotationZ(epitch) * Matrix.CreateRotationY(eyaw) * Matrix.CreateTranslation(rightEyePos) * World);
                        effect.Parameters["specularTex"].SetValue(Global.TexWhite);
                        if (showDetails)
                        {
                            effect.Parameters["Diffuse"].SetValue(Vector4.One);
                            effect.Parameters["Specular"].SetValue(Vector4.One);
                            effect.Parameters["diffuseTex"].SetValue(texEye);
                        }
                        else
                        {
                            effect.Parameters["Diffuse"].SetValue(new Vector4(0.1f,0.1f,0.1f,1));
                            effect.Parameters["Specular"].SetValue(new Vector4(0f,0f,0f,1f));
                            effect.Parameters["diffuseTex"].SetValue(Global.TexWhite);
                        }
                        effect.Parameters["normalTex"].SetValue(Global.TexDefaultBM);
                        effect.CommitChanges();
                        EyeModel.Draw();
                    }

                    {
                        float eyaw = Math.Max(-0.5f, Math.Min(0.5f, -yaw)) + MathHelper.PiOver2; //(float)Math.Atan2(eyeTarget.Z - leftEyePos.Z, eyeTarget.X - leftEyePos.X);
                        float epitch = Math.Max(-0.3f, Math.Min(0.3f, -pitch));// (float)Math.Atan2(eyeTarget.Y - leftEyePos.Y, Vector2.Distance(new Vector2(eyeTarget.X, eyeTarget.Z), new Vector2(leftEyePos.X, leftEyePos.Z)));
                        effect.Parameters["World"].SetValue(Matrix.CreateScale(0.15f) * Matrix.CreateRotationZ(epitch) * Matrix.CreateRotationY(eyaw) * Matrix.CreateTranslation(leftEyePos) * World);
                        effect.CommitChanges();
                        EyeModel.Draw();
                    }

                    effect.Parameters["World"].SetValue(World);
                    effect.Parameters["Ambient"].SetValue(Color.Lime.ToVector4());
                    effect.Parameters["Diffuse"].SetValue(Color.Black.ToVector4());
                    effect.Parameters["Specular"].SetValue(Color.Black.ToVector4());
                    effect.Parameters["diffuseTex"].SetValue(Global.TexWhite);
                    effect.Parameters["normalTex"].SetValue(Global.TexDefaultBM);
                    Global.Graphics.GraphicsDevice.RenderState.DepthBias = -0.1f;
                    effect.CommitChanges();
                    if (lineIndices != null && !showDetails)
                    {
                        Global.Graphics.GraphicsDevice.VertexDeclaration = new VertexDeclaration(Global.Graphics.GraphicsDevice, VertexPositionColor.VertexElements);
                        Global.Graphics.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(PrimitiveType.LineList, lineVerts, 0, lineVerts.Length, lineIndices, 0, lineIndices.Length / 2);
                    } 
                    Global.Graphics.GraphicsDevice.RenderState.DepthBias = 0f;
                }

                pass.End();
            }
            effect.End();

            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.None);

            if(selectedVert>=0)
            {
                Viewport v = new Viewport();
                v.Width = Bounds.Width;
                v.Height = Bounds.Height;
                Vector3 pos = v.Project(headVerts[selectedVert].Position, Projection, View, World);
                spriteBatch.Draw(Global.TexWhite, new Rectangle((int)pos.X - 2, (int)pos.Y - 2, 4, 4), Color.Red);
            }

            spriteBatch.End();
        }

        public override RightClickMenu GetRightClickMenu(Point p)
        {
            RightClickMenu menu = new RightClickMenu(p);
            menu.AddOption(new RightClickMenu.RightClickMenuOption("Reset View", ResetView));
            menu.AddOption(new RightClickMenu.RightClickMenuOption("Toggle Details", ToggleShowDetails));
            return menu;
        }

        public void ResetView()
        {
            yaw = 0;
            pitch = 0;
        }

        public void ToggleShowDetails()
        {
            showDetails = !showDetails;
        }

        public override void OnResize()
        {
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, Bounds.Width / (float)Bounds.Height, 0.1f, 100f);
            base.OnResize();
        }

        private AnimationOffsetData GetAnimOffsetFromName(String name)
        {
            for (int i = 0; i < animationOffsets.Count; i++)
                if (animationOffsets[i].Name == name)
                    return animationOffsets[i];
            return null;
        }

        public bool IsVertexEditingEnabled()
        {
            if (selectedVert >= 0)
            {
                if(animInfo.ModifyIndex>=0)
                {
                    AnimationOffsetData aod = GetAnimOffsetFromName(animInfo[animInfo.ModifyIndex].Name);
                    if (aod != null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public Vector3 GetVertexEditingValue()
        {
            if (selectedVert >= 0)
            {
                if (animInfo.ModifyIndex >= 0)
                {
                    AnimationOffsetData aod = GetAnimOffsetFromName(animInfo[animInfo.ModifyIndex].Name);
                    if (aod != null)
                    {
                        for (int i = 0; i < aod.offsets.Count; i++)
                        {
                            if (aod.offsets[i].index == selectedVert)
                            {
                                if (animInfo[animInfo.ModifyIndex].Value < 0.5f)
                                    return aod.offsets[i].offset1;
                                else
                                    return aod.offsets[i].offset2;
                            }
                        }
                        aod.offsets.Add(new AnimationOffsetData.AnimationOffset(selectedVert));
                        if (animInfo[animInfo.ModifyIndex].Value < 0.5f)
                            return aod.offsets[aod.offsets.Count - 1].offset1;
                        else
                            return aod.offsets[aod.offsets.Count - 1].offset2;
                    }
                }
            }
            return Vector3.Zero;
        }

        public void SetVertexEditingValue(Vector3 pos)
        {
            if (selectedVert >= 0)
            {
                if (animInfo.ModifyIndex >= 0)
                {
                    AnimationOffsetData aod = GetAnimOffsetFromName(animInfo[animInfo.ModifyIndex].Name);
                    if (aod != null)
                    {
                        for (int i = 0; i < aod.offsets.Count; i++)
                        {
                            if (aod.offsets[i].index == selectedVert)
                            {
                                if (animInfo[animInfo.ModifyIndex].Value < 0.5f)
                                {
                                    aod.offsets[i].offset1 = pos;
                                    animInfo.Dirty = true;
                                    return;
                                }
                                else
                                {
                                    aod.offsets[i].offset2 = pos;
                                    animInfo.Dirty = true;
                                    return;
                                }
                            }
                        }
                        aod.offsets.Add(new AnimationOffsetData.AnimationOffset(selectedVert));
                        if (animInfo[animInfo.ModifyIndex].Value < 0.5f)
                        {
                            aod.offsets[aod.offsets.Count-1].offset1 = pos;
                            return;
                        }
                        else
                        {
                            aod.offsets[aod.offsets.Count-1].offset2 = pos;
                            return;
                        }
                    }
                }
            }
        }
    }
}

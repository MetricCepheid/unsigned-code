using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Unsigned
{
    class FVLogoScreen : BaseState
    {
        private float logoTime;
        Texture2D texBarrel, texGoo1, texGoo2, texPresser;
        Model mBarrel, mGoo1, mGoo2, mPresser;
        

        public FVLogoScreen()
        {

        }

        public override void Load(ContentManager content)
        {
            texBarrel = content.Load<Texture2D>("graphics\\barrel");
            texGoo1 = content.Load<Texture2D>("graphics\\goo1");
            texGoo2 = content.Load<Texture2D>("graphics\\goo2");
            texPresser = content.Load<Texture2D>("graphics\\presser");

            mBarrel = content.Load<Model>("meshes\\barrel");
            mGoo1 = content.Load<Model>("meshes\\goo1");
            mGoo2 = content.Load<Model>("meshes\\goo2");
            mPresser = content.Load<Model>("meshes\\presser");
        }

        public override void Unload()
        {

        }

        public override void Update(GameTime gameTime)
        {
            if (logoTime >= 35)
                UnsignedGame.GetSingleton().PushState(new MainMenuScreen());
            if (logoTime < 35)
                logoTime += (float)gameTime.ElapsedGameTime.TotalSeconds * 8;
        }

        public override void Render(GameTime gameTime)
        {
            RenderMaster rm = RenderMaster.GetSingleton();
            BasicEffect bEffect = rm.bEffect;

            rm.graphics.GraphicsDevice.Clear(Color.Black);


#if !DEBUG

                    try
                    {
#endif


            rm.graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
            rm.graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
            //graphics.PreferMultiSampling = true;
            rm.graphics.ApplyChanges();

            VertexDeclaration vd = new VertexDeclaration(rm.graphics.GraphicsDevice, GBVertexFormat.Elements);
            rm.graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;
            rm.graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
            rm.graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;

            bEffect.DiffuseColor = new Vector3(1f, 1f, 1f);
            bEffect.DirectionalLight0.DiffuseColor = new Vector3(0.8f, 0.8f, 0.8f);
            bEffect.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(-1, -3, -1));
            bEffect.DirectionalLight0.Enabled = true;
            bEffect.DirectionalLight0.SpecularColor = new Vector3(1.0f, 1.0f, 1.0f);
            bEffect.LightingEnabled = true;
            bEffect.SpecularColor = new Vector3(1.0f, 1.0f, 1.0f);
            bEffect.SpecularPower = 12.0f;
            bEffect.TextureEnabled = true;
            bEffect.CommitChanges();
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
            bEffect.Begin();
            foreach (EffectPass pass in bEffect.CurrentTechnique.Passes)
            {
                pass.Begin();
                Matrix matProj = Matrix.CreatePerspectiveFieldOfView((float)Math.PI / 4.0f,
                  GameSettings.windowwidth / (float)GameSettings.windowheight,
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
                bEffect.View = matView;
                bEffect.Projection = matProj;

                Matrix matRot, matScale, matTranslate;
                {//goo1
                    matTranslate = Matrix.CreateTranslation(0, 0, 0);
                    matRot = Matrix.Identity;// Matrix.CreateRotationX((float)Math.PI);
                    matScale = Matrix.CreateScale(1, 1, 1);

                    bEffect.World = matScale * matRot * matTranslate;
                    if (logoTime < 17)
                        bEffect.Texture = texGoo1;
                    else
                        bEffect.Texture = texGoo2;
                    bEffect.CommitChanges();

                    rm.graphics.GraphicsDevice.VertexDeclaration = vd;
                    if (logoTime < 17)
                        foreach (ModelMesh mesh in mGoo1.Meshes)
                        {
                            foreach (ModelMeshPart meshpart in mesh.MeshParts)
                            {
                                rm.graphics.GraphicsDevice.VertexDeclaration = meshpart.VertexDeclaration;
                                rm.graphics.GraphicsDevice.Vertices[0].SetSource(mesh.VertexBuffer, meshpart.StreamOffset, meshpart.VertexStride);
                                rm.graphics.GraphicsDevice.Indices = mesh.IndexBuffer;
                                rm.graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, meshpart.BaseVertex, 0, meshpart.NumVertices, meshpart.StartIndex, meshpart.PrimitiveCount);
                            }
                        }
                    else
                        foreach (ModelMesh mesh in mGoo2.Meshes)
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
                {//barrel
                    matTranslate = Matrix.CreateTranslation(-10.2f, 0, -6.8f);
                    matRot = Matrix.CreateRotationY((float)Math.PI * 1.25f);
                    matScale = Matrix.CreateScale(1, 1, 1);

                    bEffect.World = matScale * matRot * matTranslate;
                    bEffect.Texture = texBarrel;
                    bEffect.CommitChanges();

                    rm.graphics.GraphicsDevice.VertexDeclaration = vd;
                    foreach (ModelMesh mesh in mBarrel.Meshes)
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

                    bEffect.World = matScale * matRot * matTranslate;
                    bEffect.Texture = texPresser;
                    bEffect.CommitChanges();

                    rm.graphics.GraphicsDevice.VertexDeclaration = vd;
                    foreach (ModelMesh mesh in mPresser.Meshes)
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
            bEffect.End();

            rm.spritebatch.Begin();
            if (logoTime < 5)
                rm.spritebatch.Draw(Global.texWhite, new Rectangle(0, 0, GameSettings.windowwidth, GameSettings.windowheight), new Color(0, 0, 0, (byte)(255 * (1 - ((logoTime) / 5f)))));
            if (logoTime > 30 && logoTime < 35)
                rm.spritebatch.Draw(Global.texWhite, new Rectangle(0, 0, GameSettings.windowwidth, GameSettings.windowheight), new Color(0, 0, 0, (byte)(255 * (((logoTime - 30) / 5f)))));
            else if (logoTime >= 35)
                rm.spritebatch.Draw(Global.texWhite, new Rectangle(0, 0, GameSettings.windowwidth, GameSettings.windowheight), Color.Black);
            rm.spritebatch.End();
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

#endif
        }
    }
}

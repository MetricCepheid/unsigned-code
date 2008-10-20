using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Unsigned
{
    class CurtainLoadingScreen : BaseState
    {
        private ContentManager content;
        private Model mCurtain;
        private Texture2D texCurtainLeft, texCurtainRight;

        public CurtainLoadingScreen()
        {

        }

        public override void Load()
        {
            content = new ContentManager(UnsignedGame.GetSingleton().Services);
        }

        public override void Unload()
        {
            content.Unload();
        }

        public override void Update(GameTime gameTime)
        {
        }

        public override void Render(GameTime gameTime)
        {
            RenderMaster rm = RenderMaster.GetSingleton();
            FVShader bEffect = rm.engine;

            rm.graphics.GraphicsDevice.Clear(Color.Black);

            rm.graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
            rm.graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
            //graphics.PreferMultiSampling = true;
            rm.graphics.ApplyChanges();

            rm.graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;
            rm.graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
            rm.graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;

            bEffect.DiffuseMaterial = Color.White;
            bEffect.DirectionalLight = new DirectionalLight(true, new Vector3(-1, -3, -1), new Color(200, 200, 200), Color.White);
            bEffect.SpecularMaterial = Color.Black;
            bEffect.Shininess = 12.0f;
            bEffect.TextureEnabled = true;

            bEffect.LightingEnabled = GameSettings.Lighting;
            bEffect.SpecularEnabled = GameSettings.Specular;
            bEffect.NormalMapEnabled = GameSettings.NormalMapping;

            bEffect.CommitChanges();
            bEffect.Begin();
            foreach (EffectPass pass in bEffect.CurrentTechnique.Passes)
            {
                pass.Begin();

                Matrix matProj = Matrix.CreatePerspectiveFieldOfView((float)Math.PI / 4.0f,
                      GameSettings.windowwidth / (float)GameSettings.windowheight,
                      1f, 40.0f);

                Matrix matView = Matrix.CreateLookAt(new Vector3(0, 0, 7), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
                //render the background graphics
                bEffect.View = matView;
                bEffect.Projection = matProj;

                Matrix matRot, matScale, matTranslate;
                matTranslate = Matrix.CreateTranslation(-2, 0, 0);
                matRot = Matrix.Identity;// Matrix.CreateRotationX((float)Math.PI);
                matScale = Matrix.CreateScale(1, 2, 1);

                bEffect.World = matScale * matRot * matTranslate;
                bEffect.DiffuseTexture = texCurtainLeft;
                bEffect.CommitChanges();

                foreach (ModelMesh mesh in mCurtain.Meshes)
                {
                    foreach (ModelMeshPart meshpart in mesh.MeshParts)
                    {
                        rm.graphics.GraphicsDevice.VertexDeclaration = meshpart.VertexDeclaration;
                        rm.graphics.GraphicsDevice.Vertices[0].SetSource(mesh.VertexBuffer, meshpart.StreamOffset, meshpart.VertexStride);
                        rm.graphics.GraphicsDevice.Indices = mesh.IndexBuffer;
                        rm.graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, meshpart.BaseVertex, 0, meshpart.NumVertices, meshpart.StartIndex, meshpart.PrimitiveCount);
                    }
                }

                matTranslate = Matrix.CreateTranslation(2, 0, 0);
                matRot = Matrix.Identity;// Matrix.CreateRotationX((float)Math.PI);
                matScale = Matrix.CreateScale(1, 2, 1);

                bEffect.World = matScale * matRot * matTranslate;
                bEffect.DiffuseTexture = texCurtainRight;
                bEffect.CommitChanges();

                foreach (ModelMesh mesh in mCurtain.Meshes)
                {
                    foreach (ModelMeshPart meshpart in mesh.MeshParts)
                    {
                        rm.graphics.GraphicsDevice.VertexDeclaration = meshpart.VertexDeclaration;
                        rm.graphics.GraphicsDevice.Vertices[0].SetSource(mesh.VertexBuffer, meshpart.StreamOffset, meshpart.VertexStride);
                        rm.graphics.GraphicsDevice.Indices = mesh.IndexBuffer;
                        rm.graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, meshpart.BaseVertex, 0, meshpart.NumVertices, meshpart.StartIndex, meshpart.PrimitiveCount);
                    }
                }
                pass.End();
            }
            bEffect.End();
        }
    }
}

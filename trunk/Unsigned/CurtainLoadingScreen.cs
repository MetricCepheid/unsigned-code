using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace Unsigned
{
    class CurtainLoadingScreen : BaseState
    {

        public CurtainLoadingScreen()
        {

        }

        public override void Load(ContentManager content)
        {

        }

        public override void Unload(ContentManager content)
        {

        }

        public override void Update(GameTime gameTime)
        {
        }

        public override void Render(GameTime gameTime)
        {
           RenderMaster.GetSingleton().graphics.GraphicsDevice.Clear(Color.Black);

           RenderMaster.GetSingleton().graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
           RenderMaster.GetSingleton().graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
            //graphics.PreferMultiSampling = true;
           RenderMaster.GetSingleton().graphics.ApplyChanges();

            vd = new VertexDeclaration(graphics.GraphicsDevice, GBVertexFormat.Elements);
           RenderMaster.GetSingleton().graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;
           RenderMaster.GetSingleton().graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
           RenderMaster.GetSingleton().graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;

            bEngine.DiffuseColor = new Vector3(1f, 1f, 1f);
            bEngine.DirectionalLight0.DiffuseColor = new Vector3(0.8f, 0.8f, 0.8f);
            bEngine.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(-1, -3, -1));
            bEngine.DirectionalLight0.Enabled = true;
            bEngine.DirectionalLight0.SpecularColor = new Vector3(1.0f, 1.0f, 1.0f);
            bEngine.LightingEnabled = true;
            bEngine.SpecularColor = new Vector3(0, 0, 0);
            bEngine.SpecularPower = 12.0f;
            bEngine.TextureEnabled = true;
            bEngine.CommitChanges();
            bEngine.Begin();
            foreach (EffectPass pass in bEngine.CurrentTechnique.Passes)
            {
                pass.Begin();

                matProj = Matrix.CreatePerspectiveFieldOfView((float)Math.PI / 4.0f,
                      GameSettings.windowwidth / (float)GameSettings.windowheight,
                      1f, 40.0f);

                matView = Matrix.CreateLookAt(new Vector3(0, 0, 7), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
                //render the background graphics
                bEngine.View = matView;
                bEngine.Projection = matProj;

                Matrix matRot, matScale, matTranslate;
                matTranslate = Matrix.CreateTranslation(-2, 0, 0);
                matRot = Matrix.Identity;// Matrix.CreateRotationX((float)Math.PI);
                matScale = Matrix.CreateScale(1, 2, 1);

                bEngine.World = matScale * matRot * matTranslate;
                bEngine.Texture = texCurtainLeft;
                bEngine.CommitChanges();

                foreach (ModelMesh mesh in mCurtain.Meshes)
                {
                    foreach (ModelMeshPart meshpart in mesh.MeshParts)
                    {
                       RenderMaster.GetSingleton().graphics.GraphicsDevice.VertexDeclaration = meshpart.VertexDeclaration;
                       RenderMaster.GetSingleton().graphics.GraphicsDevice.Vertices[0].SetSource(mesh.VertexBuffer, meshpart.StreamOffset, meshpart.VertexStride);
                       RenderMaster.GetSingleton().graphics.GraphicsDevice.Indices = mesh.IndexBuffer;
                       RenderMaster.GetSingleton().graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, meshpart.BaseVertex, 0, meshpart.NumVertices, meshpart.StartIndex, meshpart.PrimitiveCount);
                    }
                }

                matTranslate = Matrix.CreateTranslation(2, 0, 0);
                matRot = Matrix.Identity;// Matrix.CreateRotationX((float)Math.PI);
                matScale = Matrix.CreateScale(1, 2, 1);

                bEngine.World = matScale * matRot * matTranslate;
                bEngine.Texture = texCurtainRight;
                bEngine.CommitChanges();

                foreach (ModelMesh mesh in mCurtain.Meshes)
                {
                    foreach (ModelMeshPart meshpart in mesh.MeshParts)
                    {
                       RenderMaster.GetSingleton().graphics.GraphicsDevice.VertexDeclaration = meshpart.VertexDeclaration;
                       RenderMaster.GetSingleton().graphics.GraphicsDevice.Vertices[0].SetSource(mesh.VertexBuffer, meshpart.StreamOffset, meshpart.VertexStride);
                       RenderMaster.GetSingleton().graphics.GraphicsDevice.Indices = mesh.IndexBuffer;
                       RenderMaster.GetSingleton().graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, meshpart.BaseVertex, 0, meshpart.NumVertices, meshpart.StartIndex, meshpart.PrimitiveCount);
                    }
                }
                pass.End();
            }
            bEngine.End();
        }
    }
}

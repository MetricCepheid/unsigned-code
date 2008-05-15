using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace Unsigned
{
    class Rocker : DrawableGameComponent
    {
        public float Rot;
        private Model model;
        private ContentManager content;
        private String FileName;
        private Texture2D tex;
        private Vector3 position;
                                     
        public Rocker(String filename, Game1 game, ContentManager cont, Effect e) : base(game)
        {
            content = cont;
            FileName = filename;
            LoadModel(e);
            Rot = 0;
        }

        protected void LoadModel(Effect e)
        {
            model = content.Load<Model>("charmodels\\dude");


            foreach (ModelMesh mesh in model.Meshes)
                foreach (ModelMeshPart mPart in mesh.MeshParts)
                    mPart.Effect = e;

            //tex = content.Load<Texture2D>("graphics\\rocker");
        }

        public void Draw(GameTime gameTime, GraphicsDeviceManager graphics)
        {
            //Matrix[] bones = animationPlayer.GetSkinTransforms();
            
            
            model.Meshes[0].Effects[0].Parameters["diffuseTexture"].SetValue(tex);
            model.Meshes[0].Effects[0].Parameters["diffuseColor"].SetValue(new Vector4(1, 1, 1, 1));
            model.Meshes[0].Effects[0].Parameters["vertexAlpha"].SetValue(false);
            //model.Meshes[0].Effects[0].Parameters["false"].SetValue(true);
            model.Meshes[0].Effects[0].Parameters["BumpMappingEnabled"].SetValue(false);
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart meshpart in mesh.MeshParts)
                {
                    meshpart.Effect.Parameters["world"].SetValue(Matrix.CreateScale(Venue.SCALE)*Matrix.CreateRotationY(Rot)*Matrix.CreateTranslation(position));
                    //meshpart.Effect.Parameters["Bones"].SetValue(bones);
                    meshpart.Effect.CommitChanges();
                    graphics.GraphicsDevice.VertexDeclaration = meshpart.VertexDeclaration;
                    graphics.GraphicsDevice.Vertices[0].SetSource(mesh.VertexBuffer, meshpart.StreamOffset, meshpart.VertexStride);
                    graphics.GraphicsDevice.Indices = mesh.IndexBuffer;
                    graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, meshpart.BaseVertex, 0, meshpart.NumVertices, meshpart.StartIndex, meshpart.PrimitiveCount);
                }
            }
            //model.Meshes[0].Effects[0].Parameters["skinned"].SetValue(false);
            model.Meshes[0].Effects[0].Parameters["vertexAlpha"].SetValue(true);

            base.Draw(gameTime);
        }

        public String GetName()
        {
            return FileName;
        }

        public Vector3 GetPosition()
        {
            return position;
        }

        public void SetPosition(Vector3 inn)
        {
            position = inn;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using SongDataIO;

namespace Unsigned
{
    class Rocker
    {
        public float Rot;
        private Model model;
        private Texture2D tex;
        private Vector3 position;
        private int characterIndex;
        private Instrument instrument;
                                     
        public Rocker(int characterindex, ContentManager content)
        {
            this.characterIndex = characterindex;
            LoadModel(content);
            Rot = 0;
        }

        protected void LoadModel(ContentManager content)
        {
            model = content.Load<Model>("charmodels\\char");


            foreach (ModelMesh mesh in model.Meshes)
                foreach (ModelMeshPart mPart in mesh.MeshParts)
                    mPart.Effect = RenderMaster.GetSingleton().engine.InnerEffect;

            //tex = content.Load<Texture2D>("graphics\\rocker");
        }

        public void Draw(GameTime gameTime)
        {
            GraphicsDeviceManager graphics = RenderMaster.GetSingleton().graphics;
            FVShader engine = RenderMaster.GetSingleton().engine;

            //Matrix[] bones = animationPlayer.GetSkinTransforms();
            
            
            engine.DiffuseTexture = tex;
            engine.DiffuseMaterial = Color.White;
            engine.NormalMapTexture = Global.texDefaultBM;
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart meshpart in mesh.MeshParts)
                {
                    engine.World = Matrix.CreateScale(Venue.SCALE) * Matrix.CreateRotationY(Rot) * Matrix.CreateTranslation(position);
                    //meshpart.Effect.Parameters["Bones"].SetValue(bones);
                    engine.CommitChanges();
                    graphics.GraphicsDevice.VertexDeclaration = meshpart.VertexDeclaration;
                    graphics.GraphicsDevice.Vertices[0].SetSource(mesh.VertexBuffer, meshpart.StreamOffset, meshpart.VertexStride);
                    graphics.GraphicsDevice.Indices = mesh.IndexBuffer;
                    graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, meshpart.BaseVertex, 0, meshpart.NumVertices, meshpart.StartIndex, meshpart.PrimitiveCount);
                }
            }
        }

        public Vector3 GetPosition()
        {
            return position;
        }

        public void SetPosition(Vector3 inn)
        {
            position = inn;
        }

        public CharacterIdol GetCharacter()
        {
            return CharacterMaster.GetSingleton().GetCharacter(characterIndex);
        }

        public Instrument GetInstrument()
        {
            return instrument;
        }
    }
}

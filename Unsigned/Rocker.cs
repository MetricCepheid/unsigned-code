using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using SkinnedModel;

namespace GarageBand
{
    class Rocker : DrawableGameComponent
    {
        public float Rot;
        private float[] anmRots;
        private Model model;
        private Matrix[] boneTransforms;
        private ContentManager content;
        private String FileName;
        private Texture2D tex;
        AnimationPlayer animationPlayer;
        private Vector3 position;

        private enum RockerBoneData
        {
            BASE = 0, PELVIS = 1, LOWER_SPINE = 2, STERNUM = 3, COLLARBONE = 4, NECK = 5,
            LEFT_HIP = 6, LEFT_KNEE = 7, LEFT_ANKLE = 8, LEFT_FOOT = 9,
            RIGHT_HIP = 10, RIGHT_KNEE = 11, RIGHT_ANKLE = 12, RIGHT_FOOT = 13,
            LEFT_SHOULDER = 14, LEFT_ELBOW = 15, LEFT_WRIST = 16,
            LEFT_PINKY_ROOT = 17, LEFT_PINKY_MID = 18, LEFT_PINKY_TIP = 19,
            LEFT_RING_ROOT = 20, LEFT_RING_MID = 21, LEFT_RING_TIP = 22,
            LEFT_MIDDLE_ROOT = 23, LEFT_MIDDLE_UPPER = 24, LEFT_MIDDLE_LOWER = 25, LEFT_MIDDLE_TIP = 26,
            LEFT_POINTER_ROOT = 27, LEFT_POINTER_UPPER = 28, LEFT_POINTER_LOWER = 29, LEFT_POINTER_TIP = 30,
            LEFT_THUMB_ROOT = 31, LEFT_THUMB_MIDDLE = 32, LEFT_THUMB_TIP = 33,
            RIGHT_SHOULDER = 34, RIGHT_ELBOW = 35, RIGHT_WRIST = 36,
            RIGHT_PINKY_ROOT = 37, RIGHT_PINKY_MID = 38, RIGHT_PINKY_TIP = 39,
            RIGHT_RING_ROOT = 40, RIGHT_RING_MID = 41, RIGHT_RING_TIP = 42,
            RIGHT_MIDDLE_ROOT = 43, RIGHT_MIDDLE_UPPER = 44, RIGHT_MIDDLE_LOWER = 45, RIGHT_MIDDLE_TIP = 46,
            RIGHT_POINTER_ROOT = 47, RIGHT_POINTER_UPPER = 48, RIGHT_POINTER_LOWER = 49, RIGHT_POINTER_TIP = 50,
            RIGHT_THUMB_ROOT = 51, RIGHT_THUMB_MIDDLE = 52, RIGHT_THUMB_TIP = 53
        };
                                     
        public Rocker(String filename, Game1 game, ContentManager cont, Effect e) : base(game)
        {
            anmRots = new float[66];
            content = cont;
            FileName = filename;
            LoadModel(e);
            Rot = 0;
        }

        protected void LoadModel(Effect e)
        {
            model = content.Load<Model>("meshes\\char");

            SkinningData skinningData = model.Tag as SkinningData;

            if (skinningData == null)
                throw new InvalidOperationException
                    ("This model does not contain a SkinningData tag.");

            animationPlayer = new AnimationPlayer(skinningData);

            foreach (ModelMesh mesh in model.Meshes)
                foreach (ModelMeshPart mPart in mesh.MeshParts)
                    mPart.Effect = e;

            tex = content.Load<Texture2D>("graphics\\rocker");

            AnimationClip clip = skinningData.AnimationClips["Animation"];

            animationPlayer.StartClip(clip);
        }

        public void Draw(GameTime gameTime, GraphicsDeviceManager graphics)
        {
            animationPlayer.Update(gameTime.ElapsedGameTime, true, Matrix.Identity);
            Matrix[] bones = animationPlayer.GetSkinTransforms();
            
            
            model.Meshes[0].Effects[0].Parameters["diffuseTexture"].SetValue(tex);
            model.Meshes[0].Effects[0].Parameters["diffuseColor"].SetValue(new Vector4(1, 1, 1, 1));
            model.Meshes[0].Effects[0].Parameters["vertexAlpha"].SetValue(false);
            model.Meshes[0].Effects[0].Parameters["skinned"].SetValue(true);
            model.Meshes[0].Effects[0].Parameters["BumpMappingEnabled"].SetValue(false);
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart meshpart in mesh.MeshParts)
                {
                    meshpart.Effect.Parameters["world"].SetValue(Matrix.CreateScale(Venue.SCALE)*Matrix.CreateRotationY(Rot)*Matrix.CreateTranslation(position));
                    meshpart.Effect.Parameters["Bones"].SetValue(bones);
                    meshpart.Effect.CommitChanges();
                    graphics.GraphicsDevice.VertexDeclaration = meshpart.VertexDeclaration;
                    graphics.GraphicsDevice.Vertices[0].SetSource(mesh.VertexBuffer, meshpart.StreamOffset, meshpart.VertexStride);
                    graphics.GraphicsDevice.Indices = mesh.IndexBuffer;
                    graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, meshpart.BaseVertex, 0, meshpart.NumVertices, meshpart.StartIndex, meshpart.PrimitiveCount);
                }
            }
            model.Meshes[0].Effects[0].Parameters["skinned"].SetValue(false);
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
    }
}

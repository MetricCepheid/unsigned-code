using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using FVProductions.Utility;

namespace Unsigned
{
    public class Fan
    {
        public enum Animation
        {
            Idle=1,
            Pump=0,
        }

        private static FVModelArrays[][][] models;
        private static VertexTangentBinormal[][] currentVerts;
        private static Texture2D[] textures;

        public static Animation CurrentAnimation;

        private int modelIndex, texIndex;

        private Vector3 Position;
        private float yaw;

        public Fan(Vector3 pos)
        {
            Position = pos;
            modelIndex = Global.Random.Next(3);
            texIndex = Global.Random.Next(5);
            CurrentAnimation = Animation.Pump;
        }

        public static void Load(ContentManager Content)
        {
            models = new FVModelArrays[2][][];
            models[0] = new FVModelArrays[3][];
            models[0][0] = new FVModelArrays[2];
            models[0][0][0] = ModelLoader.LoadModelArrays("meshes\\venues\\fan_pump_0");
            models[0][0][1] = ModelLoader.LoadModelArrays("meshes\\venues\\fan_pump_1");
            models[0][1] = new FVModelArrays[2];
            models[0][1][0] = ModelLoader.LoadModelArrays("meshes\\venues\\fan_pump_2");
            models[0][1][1] = ModelLoader.LoadModelArrays("meshes\\venues\\fan_pump_3");
            models[0][2] = new FVModelArrays[3];
            models[0][2][0] = ModelLoader.LoadModelArrays("meshes\\venues\\fan_headbang_0");
            models[0][2][1] = ModelLoader.LoadModelArrays("meshes\\venues\\fan_headbang_1");
            models[0][2][2] = ModelLoader.LoadModelArrays("meshes\\venues\\fan_headbang_2");
            models[1] = new FVModelArrays[1][];
            models[1][0] = new FVModelArrays[3];
            models[1][0][0] = ModelLoader.LoadModelArrays("meshes\\venues\\fan_idle_0");
            models[1][0][1] = ModelLoader.LoadModelArrays("meshes\\venues\\fan_idle_1");
            models[1][0][2] = ModelLoader.LoadModelArrays("meshes\\venues\\fan_idle_2");

            textures = new Texture2D[5];
            for(int i=0;i<textures.Length;i++)
                textures[i] = Content.Load<Texture2D>("textures\\venues\\fanTex"+i);
            currentVerts = new VertexTangentBinormal[3][];
            for (int k = 0; k < currentVerts.Length; k++)
            {
                currentVerts[k] = new VertexTangentBinormal[models[0][0][0].VB.Length];
                for (int i = 0; i < currentVerts[k].Length; i++)
                    currentVerts[k][i] = models[0][0][0].VB[i];
            }
        }

        private static float lastAnimPos;
        private static int flip = 1;
        public static void Update(float absAnimPos)
        {
            if (CurrentAnimation == Animation.Idle)
            {
                float animPos = (absAnimPos/4) % 1;
                if (lastAnimPos > animPos)
                    flip = -flip;
                float lerp = (float)((Math.Sin((animPos - 0.25f) * MathHelper.TwoPi) + 1f) / 2f);
                for (int i = 0; i < currentVerts[0].Length; i++)
                {
                    float lerp2 = (1f - (((animPos * 2f) - 1f) * ((animPos * 2f) - 1f))) * flip;
                    if (lerp2 < 0)
                        currentVerts[0][i] = ((-lerp2) * models[1][0][1].VB[i]) + ((1 + lerp2) * models[1][0][0].VB[i]);
                    else
                        currentVerts[0][i] = ((lerp2) * models[1][0][2].VB[i]) + ((1 - lerp2) * models[1][0][0].VB[i]);
                }
                for (int i = 0; i < currentVerts[1].Length; i++)
                {
                    float lerp2 = (1f - (((animPos * 2f) - 1f) * ((animPos * 2f) - 1f))) * flip;
                    if (lerp2 < 0)
                        currentVerts[1][i] = ((-lerp2) * models[1][0][1].VB[i]) + ((1 + lerp2) * models[1][0][0].VB[i]);
                    else
                        currentVerts[1][i] = ((lerp2) * models[1][0][2].VB[i]) + ((1 - lerp2) * models[1][0][0].VB[i]);
                }
                for (int i = 0; i < currentVerts[2].Length; i++)
                {
                    float lerp2 = (1f - (((animPos * 2f) - 1f) * ((animPos * 2f) - 1f))) * -flip;
                    if (lerp2 < 0)
                        currentVerts[2][i] = ((-lerp2) * models[1][0][1].VB[i]) + ((1 + lerp2) * models[1][0][0].VB[i]);
                    else
                        currentVerts[2][i] = ((lerp2) * models[1][0][2].VB[i]) + ((1 - lerp2) * models[1][0][0].VB[i]);
                }
                lastAnimPos = animPos;
            }
            else if (CurrentAnimation == Animation.Pump)
            {
                float animPos = absAnimPos % 1;
                if (lastAnimPos > animPos)
                    flip = -flip;
                float lerp = (float)((Math.Sin((animPos - 0.25f) * MathHelper.TwoPi) + 1f) / 2f);
                for (int i = 0; i < currentVerts[0].Length; i++)
                {
                    currentVerts[0][i] = ((lerp) * models[0][0][0].VB[i]) + ((1 - lerp) * models[0][0][1].VB[i]);
                }
                for (int i = 0; i < currentVerts[1].Length; i++)
                {
                    currentVerts[1][i] = ((lerp) * models[0][1][0].VB[i]) + ((1 - lerp) * models[0][1][1].VB[i]);
                }
                for (int i = 0; i < currentVerts[2].Length; i++)
                {
                    float lerp2 = (1f - (((animPos * 2f) - 1f) * ((animPos * 2f) - 1f))) * flip;
                    if (lerp2 < 0)
                        currentVerts[2][i] = ((-lerp2) * models[0][2][0].VB[i]) + ((1 + lerp2) * models[0][2][2].VB[i]);
                    else
                        currentVerts[2][i] = ((lerp2) * models[0][2][1].VB[i]) + ((1 - lerp2) * models[0][2][2].VB[i]);
                }
                lastAnimPos = animPos;
            }
        }

        public void Update(SongTime songTime, Vector3 vocalistPos)
        {
            yaw = (float)Math.Atan2(vocalistPos.X - Position.X, vocalistPos.Z - Position.Z);
        }

        public Matrix GetWorldMatrix()
        {
            return Matrix.CreateScale(10) * Matrix.CreateRotationY(yaw) * Matrix.CreateTranslation(Position);
        }

        public void Draw(FVShader effect)
        {
            effect.World = GetWorldMatrix();
            effect.DiffuseTexture = textures[texIndex];
            effect.NormalMapTexture = Global.TexDefaultBM;
            effect.SpecularMapTexture = Global.TexWhite;
            effect.SpecularMaterial = new Color(effect.DiffuseMaterial.ToVector3()*0.1f);
            effect.CommitChanges();
            Global.Graphics.GraphicsDevice.DrawUserIndexedPrimitives<VertexTangentBinormal>(
                PrimitiveType.TriangleList, currentVerts[modelIndex], 0, models[0][0][0].VB.Length, models[0][0][0].IB,
                0, models[0][0][0].NumVertices / 3);
        }
    }
}

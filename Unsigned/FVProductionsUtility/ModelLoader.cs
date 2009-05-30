using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace FVProductions.Utility
{
    public static class ModelLoader
    {
        public static TextureCube GenerateCubeMap(String skyname)
        {
            ContentManager tContent = new ContentManager(Global.Services);
            tContent.RootDirectory = "Content\\";

            Texture2D tempTex = tContent.Load<Texture2D>(skyname + "_pos_x");
            TextureCube skybox = new TextureCube(Global.Graphics.GraphicsDevice, tempTex.Width, 1, TextureUsage.Linear, SurfaceFormat.Color);
            Color[] arr = new Color[tempTex.Width * tempTex.Width];

            tempTex.GetData<Color>(arr);
            skybox.SetData<Color>(CubeMapFace.PositiveX, arr);
            tempTex = tContent.Load<Texture2D>(skyname + "_pos_y");
            tempTex.GetData<Color>(arr);
            skybox.SetData<Color>(CubeMapFace.PositiveY, arr);
            tempTex = tContent.Load<Texture2D>(skyname + "_neg_x");
            tempTex.GetData<Color>(arr);
            skybox.SetData<Color>(CubeMapFace.NegativeX, arr);
            tempTex = tContent.Load<Texture2D>(skyname + "_neg_z");
            tempTex.GetData<Color>(arr);
            skybox.SetData<Color>(CubeMapFace.NegativeZ, arr);
            tempTex = tContent.Load<Texture2D>(skyname + "_neg_y");
            tempTex.GetData<Color>(arr);
            skybox.SetData<Color>(CubeMapFace.NegativeY, arr);
            tempTex = tContent.Load<Texture2D>(skyname + "_pos_z");
            tempTex.GetData<Color>(arr);
            skybox.SetData<Color>(CubeMapFace.PositiveZ, arr);
            tContent.Unload();
            tContent.Dispose();

            return skybox;
        }

        public static TextureCube GenerateCubeMap(Texture2D tempTex)
        {
            TextureCube skybox = new TextureCube(Global.Graphics.GraphicsDevice, tempTex.Width, 1, TextureUsage.Linear, SurfaceFormat.Color);
            Color[] arr = new Color[tempTex.Width * tempTex.Width];

            tempTex.GetData<Color>(arr);
            skybox.SetData<Color>(CubeMapFace.PositiveX, arr);
            skybox.SetData<Color>(CubeMapFace.PositiveY, arr);
            skybox.SetData<Color>(CubeMapFace.NegativeX, arr);
            skybox.SetData<Color>(CubeMapFace.NegativeZ, arr);
            skybox.SetData<Color>(CubeMapFace.NegativeY, arr);
            skybox.SetData<Color>(CubeMapFace.PositiveZ, arr);

            return skybox;
        }

        public static VertexTangentBinormal[] ConvertVerts(VertexPositionNormalTexture[] verts, int[] indices)
        {
            VertexTangentBinormal[] v2 = new VertexTangentBinormal[verts.Length];
            for (int i = 0; i < v2.Length; i++)
            {
                v2[i] = new VertexTangentBinormal();
                v2[i].position = verts[i].Position;
                v2[i].texCoords = verts[i].TextureCoordinate;
                v2[i].normal = verts[i].Normal;
            }

            CalculateTangentBinormal(v2, indices);
            return v2;
        }

        private static void CalculateTangentBinormal(VertexTangentBinormal[] verts, int[] indices)
        {
            Vector3[] tan1 = new Vector3[verts.Length];

            //for (long a = 0; a < verts.Length; a++)
            //    verts[a].normal = Vector3.Zero;

            for (long a = 0; a < indices.Length / 3; a++)
            {
                int i1 = indices[(a * 3) + 0];
                int i2 = indices[(a * 3) + 1];
                int i3 = indices[(a * 3) + 2];

                Vector3 v1 = verts[i1].position;
                Vector3 v2 = verts[i2].position;
                Vector3 v3 = verts[i3].position;

                Vector2 w1 = verts[i1].texCoords;
                Vector2 w2 = verts[i2].texCoords;
                Vector2 w3 = verts[i3].texCoords;

                //verts[i1].normal += Vector3.Normalize(Vector3.Cross(v2-v1, v3-v1));
                //verts[i2].normal += Vector3.Normalize(Vector3.Cross(v2-v1, v3-v1));
                //verts[i3].normal += Vector3.Normalize(Vector3.Cross(v2-v1, v3-v1));

                float x1 = v2.X - v1.X;
                float x2 = v3.X - v1.X;
                float y1 = v2.Y - v1.Y;
                float y2 = v3.Y - v1.Y;
                float z1 = v2.Z - v1.Z;
                float z2 = v3.Z - v1.Z;

                float s1 = w2.X - w1.X;
                float s2 = w3.X - w1.X;
                float t1 = w2.Y - w1.Y;
                float t2 = w3.Y - w1.Y;

                float r = 1.0f / (s1 * t2 - s2 * t1);
                Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
                Vector3 tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

                tan1[i1] += sdir;
                tan1[i2] += sdir;
                tan1[i3] += sdir;
            }

            for (long a = 0; a < verts.Length; a++)
            {
                //verts[a].normal = Vector3.Normalize(verts[a].normal);
                Vector3 n = verts[a].normal;
                Vector3 t = Vector3.Normalize(tan1[a]);

                // Gram-Schmidt orthogonalize
                verts[a].tangent = Vector3.Normalize(t - n * Vector3.Dot(n, t));
                verts[a].binormal = Vector3.Normalize(Vector3.Cross(verts[a].normal, verts[a].tangent));
                // Calculate handedness
                //tangent[a].w = (Dot(Cross(n, t), tan2[a]) < 0.0F) ? -1.0F : 1.0F;
            }
        }

        public static FVModel LoadModel(string modelName)
        {
            ContentManager tContent = new ContentManager(Global.Services);
            tContent.RootDirectory = "Content\\";

            FVModel mdl = new FVModel();

            {

                Model m = tContent.Load<Model>(modelName);

                int numVerts = 0, numIndices = 0;
                foreach (ModelMesh mesh in m.Meshes)
                {
                    numVerts += mesh.VertexBuffer.SizeInBytes / VertexPositionNormalTexture.SizeInBytes;
                    numIndices += mesh.IndexBuffer.SizeInBytes / (mesh.IndexBuffer.IndexElementSize == IndexElementSize.SixteenBits ? 2 : 4);
                }

                VertexPositionNormalTexture[] verts = new VertexPositionNormalTexture[numVerts];
                int[] indices = new int[numIndices];
                int currentVerts = 0, currentIndices = 0;
                foreach (ModelMesh mesh in m.Meshes)
                {
                    int nV = mesh.VertexBuffer.SizeInBytes / VertexPositionNormalTexture.SizeInBytes;
                    int nI = mesh.IndexBuffer.SizeInBytes / (mesh.IndexBuffer.IndexElementSize == IndexElementSize.SixteenBits ? 2 : 4);
                    VertexPositionNormalTexture[] oldVerts = new VertexPositionNormalTexture[nV];
                    int[] oldIndices = new int[nI];
                    mesh.VertexBuffer.GetData<VertexPositionNormalTexture>(oldVerts);
                    if (mesh.IndexBuffer.IndexElementSize == IndexElementSize.ThirtyTwoBits)
                        mesh.IndexBuffer.GetData<int>(oldIndices);
                    else
                    {
                        short[] oI = new short[nI];
                        mesh.IndexBuffer.GetData<short>(oI);
                        for (int i = 0; i < nI; i++)
                            oldIndices[i] = oI[i];
                    }
                    for (int i = 0; i < nV; i++)
                        verts[i + currentVerts] = new VertexPositionNormalTexture(oldVerts[i].Position, oldVerts[i].Normal, oldVerts[i].TextureCoordinate);
                    for (int i = 0; i < nI; i++)
                        indices[i + currentIndices] = oldIndices[i] + currentVerts;
                    currentVerts += nV;
                    currentIndices += nI;
                }

                VertexTangentBinormal[] moreVerts = ConvertVerts(verts, indices);

                mdl.VB = new VertexBuffer(Global.Graphics.GraphicsDevice, VertexTangentBinormal.SizeInBytes * numVerts, BufferUsage.WriteOnly);
                mdl.VB.SetData<VertexTangentBinormal>(moreVerts);
                mdl.IB = new IndexBuffer(Global.Graphics.GraphicsDevice, numIndices * 4, BufferUsage.WriteOnly, IndexElementSize.ThirtyTwoBits);
                mdl.IB.SetData<int>(indices);
            }

            tContent.Unload();
            tContent.Dispose();

            return mdl;
        }
    }
}
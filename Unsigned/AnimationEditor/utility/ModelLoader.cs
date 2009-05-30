using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace AnimationEditor
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

        public static AnimationVertex[] ConvertVerts(VertexPositionNormalTexture[] verts, int[] indices)
        {
            AnimationVertex[] v2 = new AnimationVertex[verts.Length];
            for (int i = 0; i < v2.Length; i++)
            {
                v2[i] = new AnimationVertex();
                v2[i].Position = verts[i].Position;
                v2[i].TexCoords = verts[i].TextureCoordinate;
                v2[i].Normal = verts[i].Normal;
            }

            CalculateTangentBinormal(v2, indices);
            return v2;
        }

        private static void CalculateTangentBinormal(AnimationVertex[] verts, int[] indices)
        {
            Vector3[] tan1 = new Vector3[verts.Length];

            //for (long a = 0; a < verts.Length; a++)
            //    verts[a].normal = Vector3.Zero;

            for (long a = 0; a < indices.Length / 3; a++)
            {
                int i1 = indices[(a * 3) + 0];
                int i2 = indices[(a * 3) + 1];
                int i3 = indices[(a * 3) + 2];

                Vector3 v1 = verts[i1].Position;
                Vector3 v2 = verts[i2].Position;
                Vector3 v3 = verts[i3].Position;

                Vector2 w1 = verts[i1].TexCoords;
                Vector2 w2 = verts[i2].TexCoords;
                Vector2 w3 = verts[i3].TexCoords;

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
                Vector3 n = verts[a].Normal;
                Vector3 t = Vector3.Normalize(tan1[a]);

                // Gram-Schmidt orthogonalize
                verts[a].Tangent = Vector3.Normalize(t - n * Vector3.Dot(n, t));
                verts[a].Binormal = Vector3.Normalize(Vector3.Cross(verts[a].Normal, verts[a].Tangent));
                // Calculate handedness
                //tangent[a].w = (Dot(Cross(n, t), tan2[a]) < 0.0F) ? -1.0F : 1.0F;
            }
        }

        public static AnimatedModel LoadModel(string modelName)
        {
            ContentManager tContent = new ContentManager(Global.Services);
            tContent.RootDirectory = "Content\\";

            AnimatedModel mdl = new AnimatedModel();

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

                List<VertexPositionNormalTexture> uniqueVerts = new List<VertexPositionNormalTexture>();
                int[] newIndices = new int[indices.Length];
                for (int i = 0; i < verts.Length; i++)
                {
                    int found = -1;
                    for (int k = 0; k < uniqueVerts.Count; k++)
                    {
                        if (Vector3.DistanceSquared(uniqueVerts[k].Position, verts[i].Position) < 0.001f)
                            if (Vector3.DistanceSquared(uniqueVerts[k].Normal, verts[i].Normal) < 0.001f)
                                if (Vector2.DistanceSquared(uniqueVerts[k].TextureCoordinate, verts[i].TextureCoordinate) < 0.001f)
                                    found = k;
                    }
                    if (found >= 0)
                    {
                        for (int k = 0; k < newIndices.Length; k++)
                            if (indices[k] == i)
                                newIndices[k] = found;
                    }
                    else
                    {
                        uniqueVerts.Add(verts[i]);
                        for (int k = 0; k < newIndices.Length; k++)
                            if (indices[k] == i)
                                newIndices[k] = uniqueVerts.Count - 1;
                    }
                }

                AnimationVertex[] moreVerts = ConvertVerts(uniqueVerts.ToArray(), newIndices);

                mdl.Verts = moreVerts;
                mdl.Indices = newIndices;
            }

            tContent.Unload();
            tContent.Dispose();

            return mdl;
        }
    }
}

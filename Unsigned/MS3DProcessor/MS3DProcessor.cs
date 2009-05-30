using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

// TODO: replace these with the processor input and output types.
using TInput = MS3DProcessor.MS3DSourceData;
using TOutput = MS3DProcessor.FVModelContent;

namespace MS3DProcessor
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to apply custom processing to content data, converting an object of
    /// type TInput to TOutput. The input and output types may be the same if
    /// the processor wishes to alter data without changing its type.
    ///
    /// This should be part of a Content Pipeline Extension Library project.
    /// </summary>
    [ContentProcessor(DisplayName = "FVModel - FV Productions")]
    public class MS3DProcessor : ContentProcessor<TInput, TOutput>
    {
        public override TOutput Process(TInput input, ContentProcessorContext context)
        {
            FVModelContent mdl = new FVModelContent();
            List<VertexPositionNormalTexture> verts = new List<VertexPositionNormalTexture>();
            List<int> indices = new List<int>();
            for (int i = 0; i < input.Triangles.Length; i++)
            {
                for(int k=0;k<3;k++)
                {
                    VertexPositionNormalTexture v = new VertexPositionNormalTexture(input.Vertices[input.Triangles[i].vertexIndices[k]].vertex,
                                                                                    input.Triangles[i].vertexNormals[k], input.Triangles[i].textureCoords[k]);
                    bool found = false;
                    for (int j = 0; j < verts.Count; j++)
                    {
                        if(Vector3.DistanceSquared(verts[j].Position, v.Position)<0.0001f)
                            if(Vector2.DistanceSquared(verts[j].TextureCoordinate,v.TextureCoordinate)<0.0001f)
                                if (Vector3.DistanceSquared(verts[j].Normal, v.Normal) < 0.00001f)
                                {
                                    indices.Add(j);
                                    found = true;
                                    break;
                                }
                    }
                    if (!found)
                    {
                        verts.Add(v);
                        indices.Add(verts.Count - 1);
                    }
                }
            }
            mdl.Indices = indices.ToArray();
            mdl.Verts = ConvertVerts(verts.ToArray(), indices.ToArray());
            return mdl;
        }

        private static FVModelContent.Vertex[] ConvertVerts(VertexPositionNormalTexture[] verts, int[] indices)
        {
            FVModelContent.Vertex[] v2 = new FVModelContent.Vertex[verts.Length];
            for (int i = 0; i < v2.Length; i++)
            {
                v2[i] = new FVModelContent.Vertex();
                v2[i].Position = verts[i].Position;
                v2[i].TexCoords = verts[i].TextureCoordinate;
                v2[i].Normal = verts[i].Normal;
            }

            CalculateTangentBinormal(v2, indices);
            return v2;
        }

        private static void CalculateTangentBinormal(FVModelContent.Vertex[] verts, int[] indices)
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
    }
}
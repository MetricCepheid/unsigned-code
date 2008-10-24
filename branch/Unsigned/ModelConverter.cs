using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Unsigned
{
    public static class ModelConverter
    {
        public static VertexBuffer Convert(VertexBuffer inBuf, VertexDeclaration inDec)
        {
            VertexBuffer vb;
            Convert(out vb, inBuf, inDec);
            return vb;
        }

        public static void Convert(out VertexBuffer outBuf, VertexBuffer inBuf, VertexDeclaration inDec)
        {
            int stride = 0;
            int numVertices = 0;
            VertexElement[] elements = inDec.GetVertexElements();
            for(int i=0;i<elements.Length;i++)
                stride += GetSize(elements[i]);
            numVertices = inBuf.SizeInBytes / stride;
            GBVertexFormat[] outArr = new GBVertexFormat[numVertices];
            float[] inArr = new float[inBuf.SizeInBytes/4];
            inBuf.GetData<float>(inArr);

            for (int i = 0; i < numVertices; i++)
            {
                GBVertexFormat vert = new GBVertexFormat();
                int offset = i * (stride/4);
                for (int j = 0; j < elements.Length; j++)
                {
                    switch (elements[j].VertexElementUsage)
                    {
                        case VertexElementUsage.Position:
                            if (elements[j].VertexElementFormat == VertexElementFormat.Vector3)
                            {
                                vert.Position.X = inArr[offset];
                                offset++;
                                vert.Position.Y = inArr[offset];
                                offset++;
                                vert.Position.Z = inArr[offset];
                                offset++;
                            }
                            else
                                throw new InvalidOperationException();
                            break;
                        case VertexElementUsage.Normal:
                            if (elements[j].VertexElementFormat == VertexElementFormat.Vector3)
                            {
                                vert.Normal.X = inArr[offset];
                                offset++;
                                vert.Normal.Y = inArr[offset];
                                offset++;
                                vert.Normal.Z = inArr[offset];
                                offset++;
                            }
                            else
                                throw new InvalidOperationException();
                            break;
                        case VertexElementUsage.TextureCoordinate:
                            if (elements[j].VertexElementFormat == VertexElementFormat.Vector2)
                            {
                                vert.TexCoord.X = inArr[offset];
                                offset++;
                                vert.TexCoord.Y = inArr[offset];
                                offset++;
                            }
                            else
                                throw new InvalidOperationException();
                            break;
                    }
                }
                vert.Alpha = 1.0f;
                Matrix matRot = (vert.Normal.Z > vert.Normal.X) ? Matrix.CreateRotationX(MathHelper.PiOver2) : Matrix.CreateRotationZ(MathHelper.PiOver2);
                vert.Tangent = Vector3.Transform(vert.Normal, matRot);
                outArr[i] = vert;
            }

            outBuf = new VertexBuffer(RenderMaster.GetSingleton().graphics.GraphicsDevice, numVertices * GBVertexFormat.SizeInBytes, BufferUsage.WriteOnly);
            outBuf.SetData<GBVertexFormat>(outArr);
            
        }

        private static int GetSize(VertexElement e)
        {
            switch (e.VertexElementFormat)
            {
                case VertexElementFormat.Byte4: return 4;
                case VertexElementFormat.Color: return 4;
                case VertexElementFormat.Single: return 4;
                case VertexElementFormat.Vector2: return 8;
                case VertexElementFormat.Vector3: return 12;
                case VertexElementFormat.Vector4: return 16;
                default: throw new InvalidOperationException();
            }
        }
    }

    

}

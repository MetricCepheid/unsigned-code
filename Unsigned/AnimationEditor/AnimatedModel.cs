using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FVProductions.Utility;

namespace AnimationEditor
{
    public struct AnimationVertex
    {
        public Vector3 Position, Normal;
        public Vector2 TexCoords;
        public Vector3 Binormal, Tangent;

        public static VertexElement[] Elements = { 
                                                     new VertexElement(0,0,VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 0),
                                                     new VertexElement(0,12,VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Normal, 0),
                                                     new VertexElement(0,24,VertexElementFormat.Vector2, VertexElementMethod.Default, VertexElementUsage.TextureCoordinate, 0),
                                                     new VertexElement(0,32,VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Binormal, 0),
                                                     new VertexElement(0,44,VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Tangent, 0),
                                                 };

        public AnimationVertex(Vector3 pos, Vector3 nrm, Vector2 txCrd, Vector3 bin, Vector3 tan)
        {
            Position = pos;
            Normal = nrm;
            TexCoords = txCrd;
            Binormal = bin;
            Tangent = tan;
        }

        private static VertexDeclaration _vd = null;
        public static VertexDeclaration VertexDeclaration
        {
            get
            {
                if (_vd == null)
                    _vd = new VertexDeclaration(Global.Graphics.GraphicsDevice, Elements);
                return _vd;
            }
        }
        public const int SizeInBytes = 32+24;
    }

    public class AnimatedModel
    {
        public AnimationVertex[] Verts;
        public int[] Indices;

        public void Draw()
        {
            Global.Graphics.GraphicsDevice.VertexDeclaration = AnimationVertex.VertexDeclaration;
            Global.Graphics.GraphicsDevice.DrawUserIndexedPrimitives<AnimationVertex>(PrimitiveType.TriangleList, Verts, 0, Verts.Length, Indices, 0, Indices.Length / 3);
        }
    }
}

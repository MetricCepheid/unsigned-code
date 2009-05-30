using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace MS3DProcessor
{
    public class FVModelContent
    {
        public struct Vertex
        {
            public Vector3 Position;
            public Vector3 Normal;
            public Vector3 Binormal, Tangent;
            public Vector2 TexCoords;
        }

        public Vertex[] Verts;
        public int[] Indices;
    }
}

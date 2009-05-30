using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FVProductions.Utility
{
    public struct VertexTangentBinormal
    {
        public Vector3 position;
        public Vector2 texCoords;
        public Vector3 normal, binormal, tangent;

        public static VertexElement[] Elements = { new VertexElement(0,0,VertexElementFormat.Vector3,VertexElementMethod.Default,VertexElementUsage.Position,0),
                                                   new VertexElement(0,3*4,VertexElementFormat.Vector2,VertexElementMethod.Default,VertexElementUsage.TextureCoordinate,0),
                                                   new VertexElement(0,5*4,VertexElementFormat.Vector3,VertexElementMethod.Default,VertexElementUsage.Normal,0),
                                                   new VertexElement(0,8*4,VertexElementFormat.Vector3,VertexElementMethod.Default,VertexElementUsage.Binormal,0),
                                                   new VertexElement(0,11*4,VertexElementFormat.Vector3,VertexElementMethod.Default,VertexElementUsage.Tangent,0) };

        public VertexTangentBinormal(Vector3 pos, Vector2 texCrd, Vector3 nrml, Vector3 bnrml, Vector3 tan)
        {
            position = pos;
            texCoords = texCrd;
            normal = nrml;
            binormal = bnrml;
            tangent = tan;
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
        public static int SizeInBytes = (3 + 2 + 3 + 3 + 3) * sizeof(float);
    }
}

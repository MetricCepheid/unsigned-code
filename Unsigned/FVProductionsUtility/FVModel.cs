using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace FVProductions.Utility
{
    public class FVModel
    {
        public VertexBuffer VB;
        public IndexBuffer IB;
        public int NumVertices
        {
            get { return IB.SizeInBytes / (IB.IndexElementSize == IndexElementSize.ThirtyTwoBits ? 4 : 2); }
        }

        public void Draw()
        {
            Global.Graphics.GraphicsDevice.VertexDeclaration = VertexTangentBinormal.VertexDeclaration;
            Global.Graphics.GraphicsDevice.Vertices[0].SetSource(VB, 0, VertexTangentBinormal.SizeInBytes);
            Global.Graphics.GraphicsDevice.Indices = IB;
            Global.Graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, NumVertices, 0, NumVertices / 3);
        }
    }

    public class FVModelArrays
    {
        public VertexTangentBinormal[] VB;
        public int[] IB;
        public int NumVertices
        {
            get { return IB.Length; }
        }

        public void Draw()
        {
            Global.Graphics.GraphicsDevice.VertexDeclaration = VertexTangentBinormal.VertexDeclaration;
            Global.Graphics.GraphicsDevice.DrawUserIndexedPrimitives<VertexTangentBinormal>(PrimitiveType.TriangleList, VB, 0, VB.Length, IB, 0, IB.Length / 3);
        }
    }
}

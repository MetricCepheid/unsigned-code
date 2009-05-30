using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using FVProductions.Utility;

// TODO: replace this with the type you want to write out.
using TWrite = MS3DProcessor.FVModelContent;

namespace MS3DProcessor
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to write the specified data type into binary .xnb format.
    ///
    /// This should be part of a Content Pipeline Extension Library project.
    /// </summary>
    [ContentTypeWriter]
    public class FVModelWriter : ContentTypeWriter<TWrite>
    {
        protected override void Write(ContentWriter output, TWrite value)
        {
            for (int i = 0; i < value.Verts.Length; i++)
            {
                output.Write(value.Verts[i].Position.X);
                output.Write(value.Verts[i].Position.Y);
                output.Write(value.Verts[i].Position.Z);
                output.Write(value.Verts[i].Normal.X);
                output.Write(value.Verts[i].Normal.Y);
                output.Write(value.Verts[i].Normal.Z);
                output.Write(value.Verts[i].TexCoords.X);
                output.Write(value.Verts[i].TexCoords.Y);
                output.Write(value.Verts[i].Tangent.X);
                output.Write(value.Verts[i].Tangent.Y);
                output.Write(value.Verts[i].Tangent.Z);
                output.Write(value.Verts[i].Binormal.X);
                output.Write(value.Verts[i].Binormal.Y);
                output.Write(value.Verts[i].Binormal.Z);
            }
            for (int i = 0; i < value.Indices.Length; i++)
            {
                output.Write(value.Indices[i]);
            }
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return typeof(FVModel).AssemblyQualifiedName;
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "MS3DProcessor.FVModelReader, MS3DProcessor, Version=1.0.0.0, Culture=neutral";
        }
    }
}

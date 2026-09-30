using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using System.IO;

// TODO: replace this with the type you want to import.
using TImport = MS3DProcessor.MS3DSourceData;

namespace MS3DProcessor
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to import a file from disk into the specified type, TImport.
    /// 
    /// This should be part of a Content Pipeline Extension Library project.
    /// 
    /// TODO: change the ContentImporter attribute to specify the correct file
    /// extension, display name, and default processor for this importer.
    /// </summary>
    [ContentImporter(".ms3d", DisplayName = "MS3D - FV Productions", DefaultProcessor = "MS3DProcessor")]
    public class MS3DImporter : ContentImporter<TImport>
    {
        public override TImport Import(string filename, ContentImporterContext context)
        {
            BinaryReader bin = new BinaryReader(File.OpenRead(filename));
            MS3DSourceData ret = new MS3DSourceData();
            ret.FileHeader.id = bin.ReadChars(10);
            ret.FileHeader.version = bin.ReadInt32();
            ret.Vertices = new MS3DSourceData.ms3d_vertex_t[bin.ReadUInt16()];
            for (int i = 0; i < ret.Vertices.Length; i++)
            {
                ret.Vertices[i] = new MS3DSourceData.ms3d_vertex_t();
                ret.Vertices[i].flags = bin.ReadByte();
                ret.Vertices[i].vertex = new Vector3(bin.ReadSingle(), bin.ReadSingle(), bin.ReadSingle());
                ret.Vertices[i].boneId = bin.ReadSByte();
                ret.Vertices[i].referenceCount = bin.ReadByte();
            }
            ret.Triangles = new MS3DSourceData.ms3d_triangle_t[bin.ReadUInt16()];
            for (int i = 0; i < ret.Triangles.Length; i++)
            {
                ret.Triangles[i] = new MS3DSourceData.ms3d_triangle_t();
                ret.Triangles[i].flags = bin.ReadUInt16();
                ret.Triangles[i].vertexIndices[0] = bin.ReadUInt16();
                ret.Triangles[i].vertexIndices[1] = bin.ReadUInt16();
                ret.Triangles[i].vertexIndices[2] = bin.ReadUInt16();
                ret.Triangles[i].vertexNormals[0] = new Vector3(bin.ReadSingle(), bin.ReadSingle(), bin.ReadSingle());
                ret.Triangles[i].vertexNormals[1] = new Vector3(bin.ReadSingle(), bin.ReadSingle(), bin.ReadSingle());
                ret.Triangles[i].vertexNormals[2] = new Vector3(bin.ReadSingle(), bin.ReadSingle(), bin.ReadSingle());
                ret.Triangles[i].textureCoords[0].X = bin.ReadSingle();
                ret.Triangles[i].textureCoords[1].X = bin.ReadSingle();
                ret.Triangles[i].textureCoords[2].X = bin.ReadSingle();
                ret.Triangles[i].textureCoords[0].Y = bin.ReadSingle();
                ret.Triangles[i].textureCoords[1].Y = bin.ReadSingle();
                ret.Triangles[i].textureCoords[2].Y = bin.ReadSingle();
                ret.Triangles[i].smoothingGroup = bin.ReadByte();
                ret.Triangles[i].groupIndex = bin.ReadByte();
            }
            ret.Groups = new MS3DSourceData.ms3d_group_t[bin.ReadUInt16()];
            for (int i = 0; i < ret.Groups.Length; i++)
            {
                ret.Groups[i] = new MS3DSourceData.ms3d_group_t();
                ret.Groups[i].flags = bin.ReadByte();
                ret.Groups[i].name = bin.ReadFixedLengthString(32);
                ret.Groups[i].triangleIndices = new ushort[bin.ReadUInt16()];
                for (int k = 0; k < ret.Groups[i].triangleIndices.Length; k++)
                    ret.Groups[i].triangleIndices[k] = bin.ReadUInt16();
                ret.Groups[i].materialIndex = bin.ReadSByte();
            }
            ret.Materials = new MS3DSourceData.ms3d_material_t[bin.ReadUInt16()];
            for (int i = 0; i < ret.Materials.Length; i++)
            {
                ret.Materials[i].name = bin.ReadFixedLengthString(32);
                ret.Materials[i].ambient = new Vector4(bin.ReadSingle(), bin.ReadSingle(), bin.ReadSingle(), bin.ReadSingle());
                ret.Materials[i].diffuse = new Vector4(bin.ReadSingle(), bin.ReadSingle(), bin.ReadSingle(), bin.ReadSingle());
                ret.Materials[i].specular = new Vector4(bin.ReadSingle(), bin.ReadSingle(), bin.ReadSingle(), bin.ReadSingle());
                ret.Materials[i].emissive = new Vector4(bin.ReadSingle(), bin.ReadSingle(), bin.ReadSingle(), bin.ReadSingle());
                ret.Materials[i].shininess = bin.ReadSingle();
                ret.Materials[i].transparency = bin.ReadSingle();
                ret.Materials[i].mode = bin.ReadSByte();
                ret.Materials[i].texture = bin.ReadFixedLengthString(128);
                ret.Materials[i].alphamap = bin.ReadFixedLengthString(128);
            }
            ret.fAnimationFPS = bin.ReadSingle();
            ret.fCurrentTime = bin.ReadSingle();
            ret.iTotalFrames = bin.ReadInt32();
            ret.Joints = new MS3DSourceData.ms3d_joint_t[bin.ReadUInt16()];
            for (int i = 0; i < ret.Joints.Length; i++)
            {
                ret.Joints[i] = new MS3DSourceData.ms3d_joint_t();
                ret.Joints[i].flags = bin.ReadByte();
                ret.Joints[i].name = bin.ReadFixedLengthString(32);
                ret.Joints[i].parentName = bin.ReadFixedLengthString(32);
                ret.Joints[i].rotation = new Vector3(bin.ReadSingle(), bin.ReadSingle(), bin.ReadSingle());
                ret.Joints[i].position = new Vector3(bin.ReadSingle(), bin.ReadSingle(), bin.ReadSingle());
                ret.Joints[i].keyFramesRot = new MS3DSourceData.ms3d_keyframe_rot_t[bin.ReadUInt16()];
                ret.Joints[i].keyFramesTrans = new MS3DSourceData.ms3d_keyframe_pos_t[bin.ReadUInt16()];
                for (int k = 0; k < ret.Joints[i].keyFramesRot.Length; k++)
                {
                    ret.Joints[i].keyFramesRot[k] = new MS3DSourceData.ms3d_keyframe_rot_t();
                    ret.Joints[i].keyFramesRot[k].time = bin.ReadSingle();
                    ret.Joints[i].keyFramesRot[k].rotation = new Vector3(bin.ReadSingle(), bin.ReadSingle(), bin.ReadSingle());
                }
                for (int k = 0; k < ret.Joints[i].keyFramesTrans.Length; k++)
                {
                    ret.Joints[i].keyFramesTrans[k] = new MS3DSourceData.ms3d_keyframe_pos_t();
                    ret.Joints[i].keyFramesTrans[k].time = bin.ReadSingle();
                    ret.Joints[i].keyFramesTrans[k].position = new Vector3(bin.ReadSingle(), bin.ReadSingle(), bin.ReadSingle());
                }
            }
            return ret;
        }
    }
}

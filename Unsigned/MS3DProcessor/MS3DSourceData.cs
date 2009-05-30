using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace MS3DProcessor
{
    public class MS3DSourceData
    {
        public const int MAX_VERTICES  = 8192;
        public const int MAX_TRIANGLES = 16384;
        public const int MAX_GROUPS    = 128;
        public const int MAX_MATERIALS = 128;
        public const int MAX_JOINTS    = 128;
        public const int MAX_KEYFRAMES = 216;
 
        public const int SELECTED      = 1;
        public const int HIDDEN        = 2;
        public const int SELECTED2     = 4;
        public const int DIRTY         = 8;

        public class ms3d_header_t 
        {
            private const String ID_PROPER_VALUE = "MS3D000000";
            private const int VERSION_PROPER_VALUE = 3;
            public char[] id;
            public int version;

            public bool IsValid
            {
                get
                {
                    bool anyWrong = false;
                    for (int i = 0; i < id.Length; i++)
                        if (id[i] != ID_PROPER_VALUE[i])
                            anyWrong = true;
                    if (anyWrong || version != VERSION_PROPER_VALUE)
                        return false;
                    return true;
                }
            }

            public ms3d_header_t()
            {
                id = new char[10];
            }
        }

        public class ms3d_vertex_t
        {
            public byte flags;
            public Vector3 vertex;
            public sbyte boneId;
            public byte referenceCount;
        }

        public class ms3d_triangle_t 
        {
            public ushort flags;
            public ushort[] vertexIndices;
            public Vector3[] vertexNormals;
            public Vector2[] textureCoords;
            public byte smoothingGroup;
            public byte groupIndex;

            public ms3d_triangle_t()
            {
                vertexIndices = new ushort[3];
                vertexNormals = new Vector3[3];
                textureCoords = new Vector2[3];
            }
        }

        public class ms3d_group_t 
        {
            public byte flags;
            public String name;
            public ushort[] triangleIndices;
            public sbyte materialIndex;
        }

        public class ms3d_material_t 
        {
            public String name;
            public Vector4 ambient;
            public Vector4 diffuse;
            public Vector4 specular;
            public Vector4 emissive;
            public float shininess;
            public float transparency;
            public sbyte mode;
            public String texture;
            public String alphamap;
        }

        public class ms3d_keyframe_rot_t
        {
            public float time;
            public Vector3 rotation;
        }

        public class ms3d_keyframe_pos_t 
        {
            public float time;
            public Vector3 position;
        }

        public class ms3d_joint_t
        {
            public byte flags;
            public String name;
            public String parentName;
            public Vector3 rotation;
            public Vector3 position;

            public ms3d_keyframe_rot_t[] keyFramesRot;
            public ms3d_keyframe_pos_t[] keyFramesTrans;
        }

        public ms3d_header_t FileHeader;
        public ms3d_vertex_t[] Vertices;
        public ms3d_triangle_t[] Triangles;
        public ms3d_group_t[] Groups;
        public ms3d_material_t[] Materials;

        public float fAnimationFPS;
        public float fCurrentTime;
        public int iTotalFrames;

        public ms3d_joint_t[] Joints;

        public MS3DSourceData()
        {
            FileHeader = new ms3d_header_t();
        }
    }
}

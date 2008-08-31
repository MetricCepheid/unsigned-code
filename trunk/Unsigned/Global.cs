using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Unsigned
{
    public struct GBVertexFormat
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TexCoord;
        public Vector3 Tangent;
        public float Alpha;


        public GBVertexFormat(Vector3 Position, Vector3 Normal, Vector2 TexCoord)
        {
            this.Position = Position;
            this.TexCoord = TexCoord;
            this.Normal = Normal;
            this.Tangent = Vector3.Transform(Normal, Matrix.CreateRotationZ((float)Math.PI / 2));
            Alpha = 1.0f;
        }
        public GBVertexFormat(Vector3 Position, Vector3 Normal, Vector2 TexCoord, Vector3 Tangent)
        {
            this.Position = Position;
            this.TexCoord = TexCoord;
            this.Normal = Normal;
            this.Tangent = Tangent;
            Alpha = 1.0f;
        }

        public static VertexElement[] Elements =
             {
                 new VertexElement(0, 0, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 0),
                 new VertexElement(0, sizeof(float)*3, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Normal, 0),
                 new VertexElement(0, sizeof(float)*6, VertexElementFormat.Vector2, VertexElementMethod.Default, VertexElementUsage.TextureCoordinate, 0),
                 new VertexElement(0, sizeof(float)*8, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Tangent, 0),
                 new VertexElement(0, sizeof(float)*11, VertexElementFormat.Single,VertexElementMethod.Default,VertexElementUsage.Fog,0),
             };
        public static int SizeInBytes = sizeof(float) * (3 + 2 + 3 + 3 + 1);
    }

    static class Global
    {
        public static Texture2D gradient, texWhite, texDefaultBM;
        public static byte[] bits = { 1, 1 << 1, 1 << 2, 1 << 3, 1 << 4, 1 << 5, 1 << 6, 1 << 7 };
        public const byte D_EASY = 3, D_MEDIUM = 6, D_HARD = 12, D_EXPERT = 24;
        public static String[] DifficultyStr = { "Easy", "Medium", "Hard", "Expert" };
        public static Color[] FretColors = { new Color(0, 255, 0), new Color(255, 0, 0), new Color(255, 255, 0), new Color(0, 0, 255), new Color(255, 128, 0), };
        public static Color[] FadedFretColors = { new Color(175, 207, 175), new Color(207, 175, 175), new Color(207, 207, 175), new Color(175, 175, 207), new Color(207, 191, 175), };
        public static Vector4[] FretColorsV4 = { new Vector4(0, 1, 0, 1), new Vector4(1, 0, 0, 1), new Vector4(1, 1, 0, 1), new Vector4(0, 0, 1, 1), new Vector4(1, 0.5f, 0, 1) };
        public static SpriteFont DefaultFont, BigFont, SmallFont;
        public static long TicksPerSecond = 10000000;
        public static VertexBuffer square;
        public static VertexDeclaration vd;
        public static int[] multToIndex = { -1, -1, 0, 1, 2, 3, 4, -1, 5, -1, 6, -1, 7 };
        public static bool DemoMode;
        public const byte M_GAME = 1, M_FREESTYLE = 2;
        public static byte mode = M_GAME;
        public static Random random;

        public static void Write(String output)
        {
#if DEBUG
            Console.Write(output);
#endif
        }

        public static void WriteLine(String output)
        {
#if DEBUG
            Console.WriteLine(output);
#endif
        }
    }
}

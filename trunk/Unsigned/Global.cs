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
        public const byte D_EASY = 3, D_MEDIUM = 6, D_HARD = 12, D_EXPERT = 24;
        public static String[] DifficultyStr = { "Easy", "Medium", "Hard", "Expert" };
        public static Color[] FretColors = { Color.Green, Color.Red, Color.Yellow, Color.Blue, Color.Orange, };
        public static SpriteFont DefaultFont, BigFont, SmallFont;
        public static long TicksPerSecond = 10000000;
        public static VertexBuffer square;
        public static VertexDeclaration vd;
        public static int[] multToIndex = { -1, -1, 0, 1, 2, 3, 4, -1, 5, -1, 6, -1, 7 };
        public static bool DemoMode;
        public const byte M_GAME = 1, M_FREESTYLE = 2;
        public static byte mode = M_GAME;
        public static Random random;
        public static Rectangle rect256 = new Rectangle(0, 0, 256, 256);

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

        public static int AddBits(ulong ind)
        {
            int ret = 0;
            for (ulong i = 1; i < sizeof(ulong)*8; i <<= 1)
                if ((ind & i) > 0)
                    ret++;
            return ret;
        }

        public static Color FadedFretColors(int i)
        {
            return new Color((byte)(((FretColors[i].R / 255f) * (207 - 175)) + 175),
                             (byte)(((FretColors[i].G / 255f) * (207 - 175)) + 175),
                             (byte)(((FretColors[i].B / 255f) * (207 - 175)) + 175),
                             255);
        }
    }
}

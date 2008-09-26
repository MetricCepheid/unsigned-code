using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Unsigned
{
    struct ShatterGlass
    {
        public Vector3 loc, rot, dir;
        public int col, frame;
        public float scale;
    }

    struct ShatterSpark
    {
        public Vector3 loc, dir;
        public int col;
        public float scale;
    }

    class ParticleMaster
    {
        private static ParticleMaster SINGLETON_ParticleMaster;

        private Dictionary<Board,ShatterGlass[]> glass;
        private Dictionary<Board,ShatterSpark[]> sparks;

        private Texture2D[] texShard;
        private Texture2D texSpark;

        private ParticleMaster()
        {
            glass = new Dictionary<Board, ShatterGlass[]>();
            sparks = new Dictionary<Board, ShatterSpark[]>();
        }

        private void CheckGenerateArray(Board b)
        {
            if (!glass.ContainsKey(b))
            {
                glass.Add(b,new ShatterGlass[100]);
                sparks.Add(b, new ShatterSpark[100]);
            }
        }

        public void Load(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            texShard = new Texture2D[8];
            for (int k = 0; k < 8; k++)
                texShard[k] = content.Load<Texture2D>("graphics\\glassshard0" + (k + 1));
            texSpark = content.Load<Texture2D>("graphics\\spark");
        }
        
        /// <summary>
        /// Adds new shards on the triggers
        /// </summary>
        /// <param name="note">the bitwise notes to hit</param>
        /// <param name="lane">the player lane index</param>
        /// <param name="board">the board object</param>
        /// <param name="density">number per lane to add, default=16</param>
        public void AddShards(ulong note, Board board, float density)
        {
            CheckGenerateArray(board);

            if (density >= 1)
                density = (float)((int)density);
            else
            {
                if (Global.random.NextDouble() < density)
                    density = 1;
                else
                    return;
            }
            density += 0.05f;

            int lefty = board.IsLefty?-1:1;
            int count = 0;
            int noteage = 0;
            for (noteage = 0; noteage < board.GetBoardType().NumTracks; noteage++)
                if ((note & (((ulong)1)<<noteage)) > 0)
                    break;
            Random r = Global.random;

            ShatterGlass[] glassArr = glass[board];

            for (int i = 0; i < glassArr.Length; i++)
            {
                if (glassArr[i].scale <= 0)
                {
                    glassArr[i].scale = 0.5f;
                    glassArr[i].dir = new Vector3(((float)(r.NextDouble()) * 2) - 1, ((float)(r.NextDouble()) * 1.5f) - 1, (float)(r.NextDouble() * 15)) * 0.2f;
                    glassArr[i].rot = new Vector3((float)(r.NextDouble() * Math.PI * 2), (float)(r.NextDouble() * Math.PI * 2), (float)(r.NextDouble() * Math.PI * 2));
                    glassArr[i].col = noteage;
                    glassArr[i].frame = r.Next(5);
                    Vector3 rval = Vector3.Transform(new Vector3(0, 0, -Board.zeroZ), Matrix.CreateRotationX(Board.rotate));
                    
                    glassArr[i].loc.X = (((noteage * 2 / (float)board.GetBoardType().NumTracks) - 1) + (1.0f / board.GetBoardType().NumTracks)) * Board.width * lefty;
                    glassArr[i].loc.Y = Board.height + rval.Y;
                    glassArr[i].loc.Z = rval.Z;
                    glassArr[i].loc.X += (float)(r.NextDouble() - 0.5) * (Board.width / (float)board.GetBoardType().NumTracks);
                    glassArr[i].loc.Y += (float)(r.NextDouble() - 0.5) * 0.3f;
                    count++;
                    if (count >= density)
                    {
                        count = 0;
                        noteage++;
                        for (; noteage < board.GetBoardType().NumTracks; noteage++)
                            if ((note & (((ulong)1)<<noteage)) > 0)
                                break;
                        if (noteage >= board.GetBoardType().NumTracks)
                            return;
                    }
                }
            }
        }

        /// <summary>
        /// Adds new sparks on the triggers
        /// </summary>
        /// <param name="note">the bitwise notes to shoot sparks from</param>
        /// <param name="lane">the player lane index</param>
        /// <param name="board">the board object</param>
        /// <param name="density">number per lane to add, default=16</param>
        public void AddSparks(ulong note, Board board, float density)
        {
            CheckGenerateArray(board);

            if (density >= 1)
                density = (float)((int)density);
            else
            {
                if (Global.random.NextDouble() < density)
                    density = 1;
                else
                    return;
            }
            density += 0.05f;

            int lefty = board.IsLefty ? -1 : 1;
            int count = 0;
            int noteage = 0;
            for (noteage = 0; noteage < board.GetBoardType().NumTracks; noteage++)
                if ((note & (((ulong)1) << noteage)) > 0)
                    break;
            Random r = Global.random;

            ShatterSpark[] sparkArr = sparks[board];

            for (int i = 0; i < sparkArr.Length; i++)
            {
                if (sparkArr[i].scale <= 0)
                {
                    sparkArr[i].scale = 0.25f * (float)r.NextDouble();
                    sparkArr[i].dir = new Vector3(((float)(r.NextDouble()) * 2) - 1, ((float)(r.NextDouble()) * 10f) + 15f, (float)(r.NextDouble() * 15)) * 0.2f;
                    sparkArr[i].col = noteage;
                    Vector3 rval = Vector3.Transform(new Vector3(0, 0, -Board.zeroZ), Matrix.CreateRotationX(Board.rotate));

                    sparkArr[i].loc.X = (((noteage * 2 / (float)board.GetBoardType().NumTracks) - 1) + (1.0f / board.GetBoardType().NumTracks)) * Board.width * lefty;
                    sparkArr[i].loc.Y = Board.height + rval.Y;
                    sparkArr[i].loc.Z = rval.Z;
                    sparkArr[i].loc.X += (float)(r.NextDouble() - 0.5) * (Board.width / (float)board.GetBoardType().NumTracks);
                    sparkArr[i].loc.Y += (float)(r.NextDouble() - 0.5) * 0.3f;
                    count++;
                    if (count >= density)
                    {
                        count = 0;
                        noteage++;
                        for (; noteage < board.GetBoardType().NumTracks; noteage++)
                            if ((note & (((ulong)1) << noteage)) > 0)
                                break;
                        if (noteage >= board.GetBoardType().NumTracks)
                            return;
                    }
                }
            }
        }

        /*public void AddSparks(byte note, int board)
        {
            int sparksToAdd = 0;
            for (int i = 0; i < 5; i++)
                if ((note & (1 << i)) != 0)
                    sparksToAdd += 16;
            int lefty = 1;
            if (boards[board].IsLefty())
                lefty = -1;
            int count = 0;
            int noteage = 0;
            for (noteage = 0; noteage < 5; noteage++)
                if ((note & bits[noteage]) > 0)
                    break;
            Random r = new Random((int)(DateTime.Now.Ticks / 1000));
            int k = board;
            for (int i = 0; i < sparks[k].Length; i++)
            {
                if (noteage >= 4 && boards[board].GetBoardType() == PERCUSSIONIST)
                    break;
                if (sparks[k][i].scale <= 0)
                {
                    sparks[k][i].scale = 0.25f + (float)r.NextDouble();
                    sparks[k][i].dir = new Vector3(((float)(r.NextDouble()) * 2) - 1, ((float)(r.NextDouble()) * 10f) + 15f, (float)(r.NextDouble() * 15)) * 0.2f;
                    sparks[k][i].col = noteage;
                    Vector3 rval = Vector3.Transform(new Vector3(0, 0, -Board.zeroZ), Matrix.CreateRotationX(Board.rotate));
                    if (boards[board].GetBoardType() != PERCUSSIONIST)
                        sparks[k][i].loc.X = (-.8f + (noteage * 0.4f)) * Board.width * lefty;
                    else
                        sparks[k][i].loc.X = (-.75f + (Board.drumsToGuitar[noteage] * 0.5f)) * Board.width;
                    sparks[k][i].loc.Y = Board.height + rval.Y;
                    sparks[k][i].loc.Z = rval.Z;
                    sparks[k][i].loc.X += (float)(r.NextDouble() - 0.5) * (Board.width / (boards[board].GetBoardType() == PERCUSSIONIST ? 4 : 5));
                    sparks[k][i].loc.Y += (float)(r.NextDouble() - 0.5) * 0.3f;
                    count++;
                    if (count < sparksToAdd)
                    {
                        while (true)
                        {
                            noteage++;
                            if (noteage >= 5)
                                noteage = 0;
                            if ((note & bits[noteage]) > 0)
                                break;
                        }
                    }
                    else return;
                }
            }
        }
        public void AddShortSparks(byte note, int board)
        {
            int sparksToAdd = 0;
            for (int i = 0; i < 5; i++)
                if ((note & (1 << i)) != 0)
                    sparksToAdd += 8;
            int lefty = 1;
            if (boards[board].IsLefty())
                lefty = -1;
            int count = 0;
            int noteage = 0;
            for (noteage = 0; noteage < 5; noteage++)
                if ((note & bits[noteage]) > 0)
                    break;
            Random r = new Random((int)(DateTime.Now.Ticks / 1000));
            int k = board;
            for (int i = 0; i < sparks[k].Length; i++)
            {
                if (noteage >= 4 && boards[board].GetBoardType() == PERCUSSIONIST)
                    break;
                if (sparks[k][i].scale <= 0)
                {
                    sparks[k][i].scale = 0.25f + (float)r.NextDouble();
                    sparks[k][i].dir = new Vector3(((float)(r.NextDouble()) * 40) - 20, ((float)(r.NextDouble()) * 20f), ((float)(r.NextDouble()) * 20) - 10) * 0.05f;
                    sparks[k][i].col = noteage;
                    Vector3 rval = Vector3.Transform(new Vector3(0, 0, -Board.zeroZ), Matrix.CreateRotationX(Board.rotate));
                    if (boards[board].GetBoardType() != PERCUSSIONIST)
                        sparks[k][i].loc.X = (-.8f + (noteage * 0.4f)) * Board.width * lefty;
                    else
                        sparks[k][i].loc.X = (-.75f + (Board.drumsToGuitar[noteage] * 0.5f)) * Board.width;
                    sparks[k][i].loc.Y = Board.height + rval.Y;
                    sparks[k][i].loc.Z = rval.Z;
                    sparks[k][i].loc.X += (float)(r.NextDouble() - 0.5) * (Board.width / (boards[board].GetBoardType() == PERCUSSIONIST ? 4 : 5));
                    sparks[k][i].loc.Y += (float)(r.NextDouble() - 0.5) * 0.3f;
                    count++;
                    if (count < sparksToAdd)
                    {
                        while (true)
                        {
                            noteage++;
                            if (noteage >= 5)
                                noteage = 0;
                            if ((note & bits[noteage]) > 0)
                                break;
                        }
                    }
                    else
                        return;
                }
            }
        }
        public void AddSparksFS(byte note, int board)
        {
            int lefty = 1;
            if (FSIsLefty[board])
                lefty = -1;
            int count = 0;
            int noteage = 0;
            for (noteage = 0; noteage < 5; noteage++)
                if ((note & bits[noteage]) > 0)
                    break;
            Random r = new Random((int)(DateTime.Now.Ticks / 1000));
            int k = board;
            for (int i = 0; i < sparks[k].Length; i++)
            {
                if (noteage >= 4 && board == 2)
                    break;
                if (sparks[k][i].scale <= 0)
                {
                    sparks[k][i].scale = 0.25f + (float)r.NextDouble();
                    sparks[k][i].dir = new Vector3(((float)(r.NextDouble()) * 2) - 1, ((float)(r.NextDouble()) * 10f) + 15f, (float)(r.NextDouble() * 15)) * 0.2f;
                    sparks[k][i].col = noteage;
                    Vector3 rval = Vector3.Transform(new Vector3(0, 0, -Board.zeroZ), Matrix.CreateRotationX(Board.rotate));
                    if (board != 2)
                        sparks[k][i].loc.X = (-.8f + (noteage * 0.4f)) * Board.width * lefty - FSxOffset[board];
                    else
                        sparks[k][i].loc.X = (-.75f + (Board.drumsToGuitar[noteage] * 0.5f)) * Board.width - FSxOffset[board];
                    sparks[k][i].loc.Y = Board.height + rval.Y;
                    sparks[k][i].loc.Z = rval.Z;
                    sparks[k][i].loc.X += (float)(r.NextDouble() - 0.5) * (Board.width / (board == 2 ? 4 : 5));
                    sparks[k][i].loc.Y += (float)(r.NextDouble() - 0.5) * 0.3f;
                    count++;
                    if (count >= 16)
                    {
                        count = 0;
                        noteage++;
                        for (; noteage < 5; noteage++)
                            if ((note & bits[noteage]) > 0)
                                break;
                        if (noteage >= 5)
                            return;
                    }
                }
            }
        }
        public void AddLesserSparks(byte note, int board)
        {
            int sparksToAdd = 0;
            for (int i = 0; i < 5; i++)
                if ((note & (1 << i)) != 0)
                    sparksToAdd += 8;
            int lefty = 1;
            if (boards[board].IsLefty())
                lefty = -1;
            int count = 0;
            int noteage = 0;
            for (noteage = 0; noteage < 5; noteage++)
                if ((note & bits[noteage]) > 0)
                    break;
            Random r = new Random((int)(DateTime.Now.Ticks / 1000));
            int k = board;
            for (int i = 0; i < sparks.Length; i++)
            {
                if (noteage >= 4 && boards[board].GetBoardType() == PERCUSSIONIST)
                    break;
                if (sparks[k][i].scale <= 0)
                {
                    sparks[k][i].scale = 0.25f + (float)r.NextDouble();
                    sparks[k][i].dir = new Vector3(((float)(r.NextDouble()) * 4) - 2, ((float)(r.NextDouble()) * 15f) + 5f, (float)(r.NextDouble() * 4)) * 0.2f;
                    sparks[k][i].col = noteage;
                    Vector3 rval = Vector3.Transform(new Vector3(0, 0, -Board.zeroZ), Matrix.CreateRotationX(Board.rotate));
                    if (boards[board].GetBoardType() != PERCUSSIONIST)
                        sparks[k][i].loc.X = (-.8f + (noteage * 0.4f)) * Board.width * lefty - boards[board].GetXOffset();
                    else
                        sparks[k][i].loc.X = (-.75f + (Board.drumsToGuitar[noteage] * 0.5f)) * Board.width - boards[board].GetXOffset();
                    sparks[k][i].loc.Y = Board.height + rval.Y;
                    sparks[k][i].loc.Z = rval.Z;
                    sparks[k][i].loc.X += (float)(r.NextDouble() - 0.5) * (Board.width / (boards[board].GetBoardType() == PERCUSSIONIST ? 4 : 5));
                    sparks[k][i].loc.Y += (float)(r.NextDouble() - 0.5) * 0.3f;
                    count++;
                    if (count < sparksToAdd)
                    {
                        while (true)
                        {
                            noteage++;
                            if (noteage >= 5)
                                noteage = 0;
                            if ((note & bits[noteage]) > 0)
                                break;
                        }
                    }
                    else
                        return;
                }
            }
        }*/

        /// <summary>
        /// Must be called from each board
        /// updates all animations
        /// </summary>
        /// <param name="b">Which board's gibs to update</param>
        /// <param name="gameTime">frame-by-frame timespan</param>
        public void Update(Board b, GameTime gameTime)
        {
            CheckGenerateArray(b);

            ShatterGlass[] glassArr = glass[b];
            ShatterSpark[] sparksArr = sparks[b];
            for (int i = 0; i < glassArr.Length; i++)
            {
                if (glassArr[i].scale > 0)
                {
                    glassArr[i].scale -= (float)gameTime.ElapsedGameTime.Milliseconds / 2000f;
                    glassArr[i].dir.Y -= (float)gameTime.ElapsedGameTime.Milliseconds / 1000f;
                    glassArr[i].loc += glassArr[i].dir * (float)gameTime.ElapsedGameTime.Milliseconds * 0.001f;
                }
            }
            for (int i = 0; i < sparksArr.Length; i++)
            {
                sparksArr[i].scale = Math.Min(sparksArr[i].dir.Y, 1) * 4;
                sparksArr[i].dir.Y -= (float)gameTime.ElapsedGameTime.Milliseconds / 100f;
                if (sparksArr[i].scale > 0)
                    sparksArr[i].loc += sparksArr[i].dir * (float)gameTime.ElapsedGameTime.Milliseconds * 0.001f;
            }
        }
        public void Render(Board b)
        {
            CheckGenerateArray(b);

            BasicEffect effect = RenderMaster.GetSingleton().bEffect;
            GraphicsDeviceManager graphics = RenderMaster.GetSingleton().graphics;
            ShatterGlass[] glassArr = glass[b];
            ShatterSpark[] sparkArr = sparks[b];
            for (int r = 0; r < texShard.Length; r++)
            {
                effect.Texture = texShard[r];
                for (int i = 0; i < glassArr.Length; i++)
                    if (glassArr[i].scale > 0 && glassArr[i].frame == r)
                    {
                        Matrix matIdentity, matTransl, matScale, matOrbit;
                        matIdentity = Matrix.Identity;
                        matTransl = Matrix.CreateTranslation(glassArr[i].loc);
                        matOrbit = Matrix.CreateRotationX(glassArr[i].rot.X) * Matrix.CreateRotationY(glassArr[i].rot.Y) * Matrix.CreateRotationZ(glassArr[i].rot.Z);
                        matScale = Matrix.CreateScale((new Vector3(0.1f, 0.1f, 0.1f)) * glassArr[i].scale);

                        // identity, scale, rotate, orbit(translate & rotate), translate
                        effect.World = matIdentity * matScale * matOrbit * matTransl;

                        effect.DiffuseColor = Global.FretColors[glassArr[i].col].ToVector3();
                        effect.Alpha = Global.FretColors[glassArr[i].col].A / 255.0f;
                        effect.CommitChanges();

                        // 5: draw object - select vertex type, primitive type, # of primitives
                        graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                        graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                        graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                        graphics.GraphicsDevice.Vertices[0].SetSource(Global.square, 0, GBVertexFormat.SizeInBytes);
                        graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
                        graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                    }
            }
            effect.Texture = texSpark;
            for (int i = 0; i < sparkArr.Length; i++)
                if (sparkArr[i].scale > 0)
                {
                    Matrix matRot, matTransl, matScale;
                    matRot = Matrix.CreateRotationX(MathHelper.PiOver2) /* Matrix.CreateRotationY((float)(Math.Atan2(venue.GetCamFor().X, venue.GetCamFor().Z)) + MathHelper.PiOver2)*/;
                    matTransl = Matrix.CreateTranslation(sparkArr[i].loc);
                    matScale = Matrix.CreateScale(new Vector3(0.01f, 0.01f, 0.01f) * sparkArr[i].scale);

                    // identity, scale, rotate, orbit(translate & rotate), translate
                    effect.World = matScale * matRot * matTransl;

                    effect.DiffuseColor = Global.FretColors[sparkArr[i].col].ToVector3();
                    effect.Alpha = Global.FretColors[glassArr[i].col].A / 255.0f;
                    effect.CommitChanges();

                    // 5: draw object - select vertex type, primitive type, # of primitives
                    graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                    graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                    graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                    graphics.GraphicsDevice.Vertices[0].SetSource(Global.square, 0, GBVertexFormat.SizeInBytes);
                    graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
                    graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                }
            effect.DiffuseColor = new Vector3(0.8f, 0.8f, 0.8f);
            effect.Alpha = 1.0f;
        }

        public static void CreateSingleton()
        {
            if (SINGLETON_ParticleMaster == null)
                SINGLETON_ParticleMaster = new ParticleMaster();
            else
                throw new InvalidOperationException("Singleton has already been initialized");
        }

        public static void DestroySingleton()
        {
            if (SINGLETON_ParticleMaster != null)
                SINGLETON_ParticleMaster = null;
            else
                throw new InvalidOperationException("Singleton has already been destroyed");
        }

        public static ParticleMaster GetSingleton()
        {
            return SINGLETON_ParticleMaster;
        }
    }
}

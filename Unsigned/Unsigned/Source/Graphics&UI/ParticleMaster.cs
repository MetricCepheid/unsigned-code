using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using FVProductions.Utility;

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
        private Board owner;

        private ShatterGlass[] glass;
        private ShatterSpark[] sparks;

        private static Texture2D[] texShard;
        private static Texture2D texSpark;

        public ParticleMaster(Board b)
        {
            owner = b;
            if(Configuration.ParticleDetail>=5)
            {
                glass = new ShatterGlass[2048];
                sparks = new ShatterSpark[2048];
            }
            if (Configuration.ParticleDetail > 0)
            {
                glass = new ShatterGlass[64 * Configuration.ParticleDetail];
                sparks = new ShatterSpark[64 * Configuration.ParticleDetail];
            }
        }

        public static void Load(ContentManager Content)
        {
            texShard = new Texture2D[8];
            for (int k = 0; k < 8; k++)
                texShard[k] = Content.Load<Texture2D>("textures\\particles\\glassshard0" + (k + 1));
            texSpark = Content.Load<Texture2D>("textures\\particles\\spark");
        }
        
        /// <summary>
        /// Adds new shards on the trigger
        /// </summary>
        /// <param name="noteIndex">the fret index to shoot shards from</param>
        /// <param name="density">number to add, default=16</param>
        public void AddShards(int noteIndex, int density, bool rockPower)
        {
            if (Configuration.ParticleDetail <= 0)
                return;
            if (Configuration.ParticleDetail >= 5)
                density *= 10;
            else
                density *= Configuration.ParticleDetail;

            int lefty = owner.IsLefty ? 1 : -1;
            int count = 0;

            ShatterGlass[] glassArr = glass;

            for (int i = 0; i < glassArr.Length; i++)
            {
                if (glassArr[i].scale <= 0)
                {
                    glassArr[i].scale = 0.5f;
                    glassArr[i].dir = new Vector3(((float)(Global.Random.NextDouble()) * 4) - 2, ((float)(Global.Random.NextDouble()) * 4f), -(float)(Global.Random.NextDouble() * 10)) * 0.2f;
                    glassArr[i].rot = new Vector3((float)(Global.Random.NextDouble() * Math.PI * 2), (float)(Global.Random.NextDouble() * Math.PI * 2), (float)(Global.Random.NextDouble() * Math.PI * 2));
                    glassArr[i].col = rockPower ? -1 : noteIndex;
                    glassArr[i].frame = Global.Random.Next(5);
                    
                    glassArr[i].loc.X = (((noteIndex * 2 / (float)owner.GetBoardType().NumTracks) - 1) + (1.0f / owner.GetBoardType().NumTracks)) * Board.Width * lefty;
                    glassArr[i].loc.Y = 0;
                    glassArr[i].loc.Z = 0;
                    glassArr[i].loc.X += (float)(Global.Random.NextDouble() - 0.5) * (2 * Board.Width / (float)owner.GetBoardType().NumTracks);
                    glassArr[i].loc.Y += (float)(Global.Random.NextDouble() - 0.5) * 0.3f;
                    count++;
                    if (count >= density)
                    {
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Adds new sparks on the trigger
        /// </summary>
        /// <param name="noteIndex">the fret index to shoot sparks from</param>
        /// <param name="density">number to add, default=16</param>
        public void AddSparks(int noteIndex, int density, bool rockPower)
        {
            AddSparks(noteIndex, density, 1.0f, rockPower);
        }

        /// <summary>
        /// Adds new sparks on the trigger
        /// </summary>
        /// <param name="noteIndex">the fret index to shoot sparks from</param>
        /// <param name="density">number to add, default=16</param>
        /// <param name="power">the power of the launch (initial velocity scale)</param>
        public void AddSparks(int noteIndex, int density, float power, bool rockPower)
        {
            if (Configuration.ParticleDetail <= 0)
                return;
            if (Configuration.ParticleDetail >= 5)
            {
                density *= 10;
                power *= 2;
            }
            else
            {
                density *= Configuration.ParticleDetail;
            }

            int lefty = owner.IsLefty ? 1 : -1;
            int count = 0;
            Random r = Global.Random;

            ShatterSpark[] sparkArr = sparks;

            for (int i = 0; i < sparkArr.Length; i++)
            {
                if (sparkArr[i].scale <= 0)
                {
                    sparkArr[i].scale = 0.25f * (float)Global.Random.NextDouble();
                    sparkArr[i].dir = new Vector3(((float)(Global.Random.NextDouble()) * 2) - 1, ((float)(Global.Random.NextDouble()) * 25f * power), (float)(Global.Random.NextDouble() * 15)) * 0.2f;
                    sparkArr[i].col = rockPower ? -1 : noteIndex;

                    sparkArr[i].loc.X = (((noteIndex * 2 / (float)owner.GetBoardType().NumTracks) - 1) + (1.0f / owner.GetBoardType().NumTracks)) * Board.Width * lefty;
                    sparkArr[i].loc.Y = 0;
                    sparkArr[i].loc.Z = 0;
                    sparkArr[i].loc.X += (float)(Global.Random.NextDouble() - 0.5) * (2 * Board.Width / (float)owner.GetBoardType().NumTracks);
                    sparkArr[i].loc.Y += (float)(Global.Random.NextDouble() - 0.5) * 0.3f;
                    count++;
                    if (count >= density)
                    {
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Must be called from each owner
        /// updates all animations
        /// </summary>
        /// <param name="b">Which owner's gibs to update</param>
        /// <param name="gameTime">frame-by-frame timespan</param>
        public void Update(SongTime gameTime)
        {
            if (Configuration.ParticleDetail <= 0)
                return;
            ShatterGlass[] glassArr = glass;
            ShatterSpark[] sparksArr = sparks;
            for (int i = 0; i < glassArr.Length; i++)
            {
                if (glassArr[i].scale > 0)
                {
                    glassArr[i].scale -= (float)gameTime.ElapsedGameTime.Milliseconds / 2000f;
                    glassArr[i].dir.Y -= (float)gameTime.ElapsedGameTime.Milliseconds / 1000f;
                    glassArr[i].loc += glassArr[i].dir * (float)gameTime.ElapsedGameTime.Milliseconds * 0.001f;
                    if (glassArr[i].loc.Y < 0)
                        glassArr[i].scale = 0;
                }
            }
            for (int i = 0; i < sparksArr.Length; i++)
            {
                sparksArr[i].scale = Math.Min(sparksArr[i].dir.Y/2, 1) * 4;
                sparksArr[i].dir.Y -= (float)gameTime.ElapsedGameTime.Milliseconds / 100f;
                if (sparksArr[i].scale > 0)
                    sparksArr[i].loc += sparksArr[i].dir * (float)gameTime.ElapsedGameTime.Milliseconds * 0.001f;
            }
        }

        /// <summary>
        /// Draws the particle system
        /// </summary>
        /// <param name="effect">expected to be UnsignedEngineEffect</param>
        public void Render(FVShader effect)
        {
            if (Configuration.ParticleDetail <= 0)
                return;
            ShatterGlass[] glassArr = glass;
            ShatterSpark[] sparkArr = sparks;
            effect.DiffuseMaterial = Color.Black;
            effect.SpecularMaterial = Color.Black;
            effect.Alpha = 1.0f;
            Global.Graphics.GraphicsDevice.EnableAlphaBlending();
            for (int r = 0; r < texShard.Length; r++)
            {
                effect.DiffuseTexture = texShard[r];
                for (int i = 0; i < glassArr.Length; i++)
                    if (glassArr[i].scale > 0 && glassArr[i].frame == r)
                    {
                        Matrix matIdentity, matTransl, matScale, matOrbit;
                        matIdentity = Matrix.Identity;
                        matTransl = Matrix.CreateTranslation(glassArr[i].loc);
                        matOrbit = Matrix.CreateRotationX(glassArr[i].rot.X) * Matrix.CreateRotationY(glassArr[i].rot.Y) * Matrix.CreateRotationZ(glassArr[i].rot.Z);
                        matScale = Matrix.CreateScale((new Vector3(0.1f, 0.1f, 0.1f)) * glassArr[i].scale);

                        effect.World = matIdentity * matScale * matOrbit * matTransl;

                        if (glassArr[i].col >= 0)
                            effect.AmbientMaterial = Board.DefaultFretColors[owner.GetBoardType().colorIndices[glassArr[i].col]];
                        else
                            effect.AmbientMaterial = new Color(0, 255, 255);
                        effect.CommitChanges();

                        Global.Graphics.GraphicsDevice.DrawSquare();
                    }
            }
            Global.Graphics.GraphicsDevice.EnableAdditiveBlending();
            effect.DiffuseTexture = texSpark;
            for (int i = 0; i < sparkArr.Length; i++)
                if (sparkArr[i].scale > 0)
                {
                    Matrix matRot, matTransl, matScale;
                    matRot = Matrix.CreateRotationX(MathHelper.PiOver2);
                    matTransl = Matrix.CreateTranslation(sparkArr[i].loc);
                    matScale = Matrix.CreateScale(new Vector3(0.01f, 0.01f, 0.01f) * sparkArr[i].scale);

                    effect.World = matScale * matRot * matTransl;

                    if (sparkArr[i].col >= 0)
                        effect.AmbientMaterial = Board.DefaultFretColors[owner.GetBoardType().colorIndices[sparkArr[i].col]];
                    else
                        effect.AmbientMaterial = new Color(0, 255, 255);
                    effect.CommitChanges();

                    Global.Graphics.GraphicsDevice.DrawSquare();
                }
            effect.DiffuseMaterial = new Color(200, 200, 200);
            effect.Alpha = 1.0f;
            Global.Graphics.GraphicsDevice.EnableAlphaBlending();
        }
    }
}

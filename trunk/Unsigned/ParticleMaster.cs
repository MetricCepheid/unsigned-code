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

        private ShatterGlass[][] glass;
        private ShatterSpark[][] sparks;

        private Texture2D[] texShard;
        private Texture2D texSpark;

        private ParticleMaster()
        {
            glass = new ShatterGlass[4][];
            for (int i = 0; i < 4; i++)
                glass[i] = new ShatterGlass[100];
            sparks = new ShatterSpark[4][];
            for (int i = 0; i < 4; i++)
                sparks[i] = new ShatterSpark[100];
        }

        public void Load(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            texShard = new Texture2D[8];
            for (int k = 0; k < 8; k++)
                texShard[k] = content.Load<Texture2D>("graphics\\glassshard0" + (k + 1));
            texSpark = content.Load<Texture2D>("graphics\\spark");
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

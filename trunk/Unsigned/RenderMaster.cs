using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Unsigned
{
    class RenderMaster
    {
        private static RenderMaster SINGLETON_RenderMaster;

        public Effect engine, ppEngine, fader, current;
        public Matrix Projection, View, World;
        public GraphicsDeviceManager graphics;
        public SpriteBatch spritebatch;
        private RenderTarget2D screenTarget, screenTargetPre, screenTargetFinal;
        public SpriteFont fontHandwritten;

        public SpriteBatch Spritebatch;

        private Texture2D texGlow;


        public RenderState RenderState
        {
            get { return graphics.GraphicsDevice.RenderState; }
        }

        private RenderMaster()
        {
            
        }

        public void Load(ContentManager content)
        {
            engine = content.Load<Effect>("shaders\\HFPS_Shader_XNA");//new Effect(graphics.GraphicsDevice,"shaders\\HFPS_Shader_XNA.fxc",CompilerOptions.None,new EffectPool());
            ppEngine = content.Load<Effect>("shaders\\PP_Shader_XNA");
            fader = content.Load<Effect>("shaders\\BoardFade");

            screenTargetFinal = new RenderTarget2D(graphics.GraphicsDevice, GameSettings.windowwidth, GameSettings.windowheight, 1, SurfaceFormat.Color);
            if (HALF_RENDER)
            {
                screenTarget = new RenderTarget2D(graphics.GraphicsDevice, GameSettings.windowwidth / 2, GameSettings.windowheight / 2, 1, SurfaceFormat.Color);
                screenTargetPre = new RenderTarget2D(graphics.GraphicsDevice, GameSettings.windowwidth / 2, GameSettings.windowheight / 2, 1, SurfaceFormat.Color);
                rtBoard = new RenderTarget2D[4];
                rtWaves = new RenderTarget2D[4];
                for (int i = 0; i < 4; i++)
                    if (instruments[i] && i != 1)
                    {
                        rtBoard[i] = new RenderTarget2D(graphics.GraphicsDevice, GameSettings.windowheight / 4, GameSettings.windowheight / 2, 1, SurfaceFormat.Color);
                        rtWaves[i] = new RenderTarget2D(graphics.GraphicsDevice, GameSettings.windowheight / 4, GameSettings.windowheight / 2, 1, SurfaceFormat.Color);
                    }
            }
            else
            {
                screenTarget = new RenderTarget2D(graphics.GraphicsDevice, GameSettings.windowwidth, GameSettings.windowheight, 1, SurfaceFormat.Color);
                screenTargetPre = new RenderTarget2D(graphics.GraphicsDevice, GameSettings.windowwidth, GameSettings.windowheight, 1, SurfaceFormat.Color);
                rtBoard = new RenderTarget2D[4];
                rtWaves = new RenderTarget2D[4];
                for (int i = 0; i < 4; i++)
                    if (instruments[i] && i != 1)
                    {
                        rtBoard[i] = new RenderTarget2D(graphics.GraphicsDevice, GameSettings.windowheight / 2, GameSettings.windowheight, 1, SurfaceFormat.Color);
                        rtWaves[i] = new RenderTarget2D(graphics.GraphicsDevice, GameSettings.windowheight / 2, GameSettings.windowheight, 1, SurfaceFormat.Color);
                    }
            }
        }

        public void ResetLighting()
        {
            int linum = 0;
            bool[] plo = new bool[16];
            Vector3[] plp = new Vector3[16];
            float[] pln = new float[16];
            float[] plf = new float[16];
            Vector3[] pld = new Vector3[16];
            Vector3[] pls = new Vector3[16];

            engine.Parameters["pLightOn"].SetValue(plo);
            engine.Parameters["pLightPos"].SetValue(plp);
            engine.Parameters["pLightNear"].SetValue(pln);
            engine.Parameters["pLightFar"].SetValue(plf);
            engine.Parameters["pLightDiffuse"].SetValue(pld);
            engine.Parameters["pLightSpecular"].SetValue(pls);

        }

        public void SetViewMatrix(Matrix matView)
        {
            engine.Parameters["view"].SetValue(matView);
            engine.Parameters["viewInverse"].SetValue(Matrix.Invert(matView));
        }

        public static void CreateSingleton()
        {
            SINGLETON_RenderMaster = new RenderMaster();
        }

        public static RenderMaster GetSingleton()
        {
            return SINGLETON_RenderMaster;
        }

        public static void DestroySingleton()
        {
            SINGLETON_RenderMaster = null;
        }
    }
}

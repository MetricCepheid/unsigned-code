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

        public FVShader engine, menuEngine;
        public Effect ppEngine, fader;
        public GraphicsDeviceManager graphics;
        public SpriteBatch spritebatch;
        public RenderTarget2D screenTarget, screenTargetPre, screenTargetFinal;
        public SpriteFont fontHandwritten;
        public FVShader bEffect;

        public Texture2D lastframe;

        public Matrix Projection
        {
            set 
            { 
                engine.Projection = value;
                menuEngine.Projection = value;
            }
        }

        public Matrix View
        {
            set
            {
                engine.View = value;
                menuEngine.View = value;
                bEffect.View = value;
            }
        }


        public RenderState RenderState
        {
            get { return graphics.GraphicsDevice.RenderState; }
        }

        private RenderMaster()
        {
            
        }

        public void Load(ContentManager content)
        {
            GBVertexFormat.VertexDeclaration = new VertexDeclaration(graphics.GraphicsDevice, GBVertexFormat.Elements);
            engine = new FVShader(RenderMaster.GetSingleton().graphics.GraphicsDevice,content.Load<Effect>("shaders\\UnsignedEngineShader"),"maintechnique");
            menuEngine = new FVShader(RenderMaster.GetSingleton().graphics.GraphicsDevice,content.Load<Effect>("shaders\\MenuShader"),"menutechnique");
            ppEngine = content.Load<Effect>("shaders\\PP_Shader_XNA");
            fader = content.Load<Effect>("shaders\\BoardFade");
            bEffect = new FVShader(graphics.GraphicsDevice);

            screenTargetFinal = new RenderTarget2D(graphics.GraphicsDevice, GameSettings.windowwidth, GameSettings.windowheight, 1, SurfaceFormat.Color);
            if (GameSettings.HALF_RENDER)
            {
                screenTarget = new RenderTarget2D(graphics.GraphicsDevice, GameSettings.windowwidth / 2, GameSettings.windowheight / 2, 1, SurfaceFormat.Color);
                screenTargetPre = new RenderTarget2D(graphics.GraphicsDevice, GameSettings.windowwidth / 2, GameSettings.windowheight / 2, 1, SurfaceFormat.Color);
            }
            else
            {
                screenTarget = new RenderTarget2D(graphics.GraphicsDevice, GameSettings.windowwidth, GameSettings.windowheight, 1, SurfaceFormat.Color);
                screenTargetPre = new RenderTarget2D(graphics.GraphicsDevice, GameSettings.windowwidth, GameSettings.windowheight, 1, SurfaceFormat.Color);
            }
        }

        public void ResetLighting()
        {
            engine.PointLight0 = PointLight.DisabledLight;
            engine.PointLight1 = PointLight.DisabledLight;
            engine.PointLight2 = PointLight.DisabledLight;
            engine.PointLight3 = PointLight.DisabledLight;
            engine.SpotLight0 = SpotLight.DisabledLight;
            engine.SpotLight1 = SpotLight.DisabledLight;
            engine.SpotLight2 = SpotLight.DisabledLight;
            engine.SpotLight3 = SpotLight.DisabledLight;

            menuEngine.PointLight0 = PointLight.DisabledLight;
            menuEngine.PointLight1 = PointLight.DisabledLight;
            menuEngine.PointLight2 = PointLight.DisabledLight;
            menuEngine.PointLight3 = PointLight.DisabledLight;
            menuEngine.SpotLight0 = SpotLight.DisabledLight;
            menuEngine.SpotLight1 = SpotLight.DisabledLight;
            menuEngine.SpotLight2 = SpotLight.DisabledLight;
            menuEngine.SpotLight3 = SpotLight.DisabledLight;
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using FVProductions.Utility;

namespace Unsigned
{
    public class PostProcessor
    {
        private RenderTarget2D screenTarget, screenTargetPre, screenTargetFinal;
        private Texture2D lastframe;

        private Effect effect;

        private SpriteBatch spriteBatch;

        public SpecialEffectsSettings currentSettings;

        public PostProcessor()
        {

        }

        public void Load(ContentManager Content)
        {
            spriteBatch = new SpriteBatch(Global.Graphics.GraphicsDevice);

            screenTargetFinal = new RenderTarget2D(Global.Graphics.GraphicsDevice, Global.ScreenWidth, Global.ScreenHeight, 1, SurfaceFormat.Color);
            if (Configuration.HalfRender)
            {
                screenTarget = new RenderTarget2D(Global.Graphics.GraphicsDevice, Global.ScreenWidth / 2, Global.ScreenHeight / 2, 1, SurfaceFormat.Color);
                screenTargetPre = new RenderTarget2D(Global.Graphics.GraphicsDevice, Global.ScreenWidth / 2, Global.ScreenHeight / 2, 1, SurfaceFormat.Color);
            }
            else
            {
                screenTarget = new RenderTarget2D(Global.Graphics.GraphicsDevice, Global.ScreenWidth, Global.ScreenHeight, 1, SurfaceFormat.Color);
                screenTargetPre = new RenderTarget2D(Global.Graphics.GraphicsDevice, Global.ScreenWidth, Global.ScreenHeight, 1, SurfaceFormat.Color);
            }
        }

        public void Update(GameTime gameTime)
        {

        }

        public void SetRenderingTarget()
        {
            if (currentSettings.currentFES == SpecialEffectsSettings.FRAME_EFFECT_STYLE.CREST)
            {
                if (currentSettings.countFES < 1)
                    Global.Graphics.GraphicsDevice.SetRenderTarget(0, screenTarget);
                else
                    Global.Graphics.GraphicsDevice.SetRenderTarget(0, screenTargetPre);
            }
            else
                Global.Graphics.GraphicsDevice.SetRenderTarget(0, screenTarget);
            Global.Graphics.GraphicsDevice.Clear(Color.TransparentBlack);
        }

        public void ApplyPostprocess(SongTime gameTime)
        {
            if (Configuration.RenderVenues)
            {
                if (currentSettings.currentFES == SpecialEffectsSettings.FRAME_EFFECT_STYLE.CREST)
                {
                    Global.Graphics.GraphicsDevice.SetRenderTarget(0, screenTargetFinal);
                    Global.Graphics.GraphicsDevice.Clear(Color.Black);

                    spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.None);
                    if (currentSettings.countFES < 1)
                    {
                        spriteBatch.Draw(lastframe, new Rectangle(0, 0, Global.ScreenWidth, Global.ScreenHeight), Color.White);
                        spriteBatch.Draw(screenTarget.GetTexture(), new Rectangle(0, 0, Global.ScreenWidth, Global.ScreenHeight), new Color(new Vector4(1, 1, 1, ((currentSettings.countFES)))));
                    }
                    else
                    {
                        lastframe = screenTargetPre.GetTexture();
                        spriteBatch.Draw(lastframe, new Rectangle(0, 0, Global.ScreenWidth, Global.ScreenHeight), Color.White);
                        currentSettings.countFES--;
                    }
                    //spriteBatch.Draw(lastframe, new Rectangle(0, 0, Global.ScreenWidth, Global.ScreenHeight), Color.White);
                    spriteBatch.End();
                    currentSettings.countFES += gameTime.ElapsedGameTime.Milliseconds / 500f;

                    Global.Graphics.GraphicsDevice.SetRenderTarget(0, null);
                    Global.Graphics.GraphicsDevice.Clear(Color.Black);


                    effect.Parameters["gradientTex"].SetValue(Global.TexWhite);

                    spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.None);
                    effect.CurrentTechnique = effect.Techniques["Gamma"];

                    //ppEngine.Begin();
                    //ppEngine.CurrentTechnique.Passes[0].Begin();
                    effect.Parameters["dotGrainOn"].SetValue(true);
                    effect.Parameters["ValueShift"].SetValue(0.5f);
                    effect.Parameters["grainStrength"].SetValue(.25f);
                    float time = (float)(DateTime.Now.Ticks / 1000 % 90) + 10;
                    effect.Parameters["time"].SetValue(time);
                    effect.CommitChanges();
                    spriteBatch.Draw(screenTargetFinal.GetTexture(), new Rectangle(0, 0, Global.ScreenWidth, Global.ScreenHeight), Color.White);
                }
                else
                {
                    Global.Graphics.GraphicsDevice.SetRenderTarget(0, null);
                    Global.Graphics.GraphicsDevice.Clear(Color.Black);
                    spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.None);
                    spriteBatch.Draw(screenTarget.GetTexture(), new Rectangle(0, 0, Global.ScreenWidth, Global.ScreenHeight), Color.White);
                    spriteBatch.End();
                }
            }
        }
    }
}

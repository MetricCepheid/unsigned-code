using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Unsigned
{
    class OptionsScreen : BaseState
    {
        private Texture2D whitishTex, whitishBM;
        private Texture2D tamp1, tamp2;
        private Texture2D tknob, ttape;
        private Texture2D tledon, tledoff, tswitchon, tswitchoff;

        private Model mamp1, mamp2;

        private String[] guiStyle = { "Unsigned", "Rock Band" };

        private float optionsOffset = 0;
        private int optionsSelected;
        private bool optionsSelectFull;

        private float intro;

        public OptionsScreen()
        {

        }

        public override void Load(ContentManager content)
        {

        }

        public override void Unload(ContentManager content)
        {

        }

        public override void Update(GameTime gameTime)
        {
        }
        public override void Render(GameTime gameTime)
        {
            UnsignedGame game = UnsignedGame.GetSingleton();
            RenderMaster rm = RenderMaster.GetSingleton();
            Effect effect = rm.engine;

            int linum = 0;
            bool[] plo = new bool[16];
            Vector3[] plp = new Vector3[16];
            float[] pln = new float[16];
            float[] plf = new float[16];
            Vector3[] pld = new Vector3[16];
            Vector3[] pls = new Vector3[16];

#if !DEBUG
                    try
                    {
#endif
            rm.graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
            rm.graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
            //graphics.PreferMultiSampling = true;
            rm.graphics.ApplyChanges();

            VertexDeclaration vd = new VertexDeclaration(rm.graphics.GraphicsDevice, GBVertexFormat.Elements);
            rm.graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
            //graphics.GraphicsDevice.

            effect.Parameters["bumpTexture"].SetValue(Global.texDefaultBM);
            effect.Parameters["ambientColor"].SetValue(new Vector4(0.1f, 0.1f, 0.1f, 1.0f));
            effect.Parameters["diffuseColor"].SetValue(new Vector4(0.8f, 0.8f, 0.8f, 1.0f));
            effect.Parameters["specularColor"].SetValue(new Vector4(1f, 1f, 1f, 1.0f));
            effect.Parameters["dLDiffuseColor"].SetValue(new Vector4(0, 0, 0, 0));
            effect.Parameters["dLSpecularColor"].SetValue(new Vector4(0, 0, 0, 0));
            rm.graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;
            rm.graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
            rm.graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;

            Random r = new Random();

            Version SM = rm.graphics.GraphicsDevice.GraphicsDeviceCapabilities.PixelShaderVersion;
            effect.Parameters["pLightOn"].SetValue(plo);
            effect.Parameters["pLightPos"].SetValue(plp);
            effect.Parameters["pLightNear"].SetValue(pln);
            effect.Parameters["pLightFar"].SetValue(plf);
            effect.Parameters["pLightDiffuse"].SetValue(pld);
            effect.Parameters["pLightSpecular"].SetValue(pls);
            effect.Parameters["dLDiffuseColor"].SetValue(new Vector4(0.8f, 0.8f, 0.8f, 1.0f));
            effect.Parameters["dLSpecularColor"].SetValue(new Vector4(0.0f, 0.0f, 0.0f, 1.0f));
            effect.Parameters["dLightDir"].SetValue(Vector3.Normalize(new Vector3(0.1f, -1f, 0.5f)));
            if (SM.Major >= 3)
                effect.CurrentTechnique = effect.Techniques["menutechnique"];
            else if (SM.Major >= 2)
                effect.CurrentTechnique = effect.Techniques["menutechniquet"];
            else
            {
#if WINDOWS
                System.Windows.Forms.MessageBox.Show("Whoops! Your graphics card only supports Shader Model " + SM.Major + "." + SM.Minor + "\nYou need at least 2.0 to run Unsigned");
#endif
                game.Exit();
            }
            effect.CommitChanges();
#if !DEBUG
                    }
                    catch(Exception e)
                    {
#if WINDOWS
                        System.Windows.Forms.MessageBox.Show("Problem in Draw/CD/Pt1\n"+e.Message+"\n"+e.StackTrace);
#endif
                        Exit();
                        return;
                    }

                    try
                    {
#endif
            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();
                effect.Parameters["fullbright"].SetValue(false);

                intro += (float)gameTime.ElapsedGameTime.TotalSeconds;
                float introlerp = (Math.Min(intro, 0.5f) * 2);
                Vector3 campos = ((1 - introlerp) * (new Vector3(-8, 112, 24))) + ((introlerp) * (new Vector3(-2.2f, 106.3f, 1f)));
                Matrix matView = Matrix.CreateLookAt(campos, new Vector3(-2.2f, 106.3f, 0), new Vector3(0, 1, 0));
                //render the background graphics
                rm.SetViewMatrix(matView);

                Matrix matRot, matScale, matTranslate;
                {//basebottom
                    matTranslate = Matrix.CreateTranslation(0, 100, 0);
                    matRot = Matrix.CreateRotationX((float)Math.PI);
                    matScale = Matrix.CreateScale(40, 0, 32);

                    effect.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                    effect.Parameters["wRot"].SetValue(matRot);
                    effect.Parameters["diffuseTexture"].SetValue(whitishTex);
                    effect.Parameters["bumpTexture"].SetValue(whitishBM);
                    effect.Parameters["shininess"].SetValue(0.25f);
                    effect.Parameters["SpecularEnabled"].SetValue(false);
                    effect.Parameters["vertexAlpha"].SetValue(true);
                    effect.Parameters["BumpMappingEnabled"].SetValue(true);
                    effect.CommitChanges();

                    rm.graphics.GraphicsDevice.VertexDeclaration = vd;
                    rm.graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                    rm.graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                    rm.graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                    rm.graphics.GraphicsDevice.Vertices[0].SetSource(Global.square, 0, GBVertexFormat.SizeInBytes);
                    rm.graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
                    rm.graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                }
                {//basewall
                    matTranslate = Matrix.CreateTranslation(0, 132, -4.6f);
                    matRot = Matrix.CreateRotationX((float)Math.PI / 2);
                    matScale = Matrix.CreateScale(40, 0, 32);

                    effect.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                    effect.Parameters["wRot"].SetValue(matRot);
                    effect.Parameters["diffuseTexture"].SetValue(whitishTex);
                    effect.Parameters["bumpTexture"].SetValue(whitishBM);
                    effect.Parameters["shininess"].SetValue(0.25f);
                    effect.Parameters["SpecularEnabled"].SetValue(false);
                    effect.Parameters["vertexAlpha"].SetValue(true);
                    effect.Parameters["BumpMappingEnabled"].SetValue(true);
                    effect.CommitChanges();

                    rm.graphics.GraphicsDevice.VertexDeclaration = vd;
                    rm.graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                    rm.graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                    rm.graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                    rm.graphics.GraphicsDevice.Vertices[0].SetSource(Global.square, 0, GBVertexFormat.SizeInBytes);
                    rm.graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
                    rm.graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                }
                {//rightwall
                    matTranslate = Matrix.CreateTranslation(6.8f, 132, 0f);
                    matRot = Matrix.CreateRotationX((float)Math.PI / 2) * Matrix.CreateRotationY(MathHelper.PiOver2);
                    matScale = Matrix.CreateScale(40, 0, 32);

                    effect.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                    effect.Parameters["wRot"].SetValue(matRot);
                    effect.Parameters["diffuseTexture"].SetValue(whitishTex);
                    effect.Parameters["bumpTexture"].SetValue(whitishBM);
                    effect.Parameters["shininess"].SetValue(0.25f);
                    effect.Parameters["SpecularEnabled"].SetValue(false);
                    effect.Parameters["vertexAlpha"].SetValue(true);
                    effect.Parameters["BumpMappingEnabled"].SetValue(true);
                    effect.CommitChanges();

                    rm.graphics.GraphicsDevice.VertexDeclaration = vd;
                    rm.graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                    rm.graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                    rm.graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                    rm.graphics.GraphicsDevice.Vertices[0].SetSource(Global.square, 0, GBVertexFormat.SizeInBytes);
                    rm.graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
                    rm.graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                }

                {//amp
                    matTranslate = Matrix.CreateTranslation(0, 103.6f, 0);
                    matRot = Matrix.Identity;
                    matScale = Matrix.CreateScale(1, 1, 1);

                    effect.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                    effect.Parameters["wRot"].SetValue(matRot);

                    effect.Parameters["shininess"].SetValue(0.25f);
                    effect.Parameters["SpecularEnabled"].SetValue(false);
                    effect.Parameters["vertexAlpha"].SetValue(false);
                    effect.Parameters["BumpMappingEnabled"].SetValue(false);
                    effect.Parameters["diffuseTexture"].SetValue(tamp1);
                    effect.CommitChanges();
                    foreach (ModelMesh mesh in mamp1.Meshes)
                    {
                        foreach (ModelMeshPart meshpart in mesh.MeshParts)
                        {
                            rm.graphics.GraphicsDevice.VertexDeclaration = meshpart.VertexDeclaration;
                            rm.graphics.GraphicsDevice.Vertices[0].SetSource(mesh.VertexBuffer, meshpart.StreamOffset, meshpart.VertexStride);
                            rm.graphics.GraphicsDevice.Indices = mesh.IndexBuffer;
                            rm.graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, meshpart.BaseVertex, 0, meshpart.NumVertices, meshpart.StartIndex, meshpart.PrimitiveCount);
                        }
                    }

                    effect.Parameters["diffuseTexture"].SetValue(tamp2);
                    effect.CommitChanges();
                    foreach (ModelMesh mesh in mamp2.Meshes)
                    {
                        foreach (ModelMeshPart meshpart in mesh.MeshParts)
                        {
                            rm.graphics.GraphicsDevice.VertexDeclaration = meshpart.VertexDeclaration;
                            rm.graphics.GraphicsDevice.Vertices[0].SetSource(mesh.VertexBuffer, meshpart.StreamOffset, meshpart.VertexStride);
                            rm.graphics.GraphicsDevice.Indices = mesh.IndexBuffer;
                            rm.graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, meshpart.BaseVertex, 0, meshpart.NumVertices, meshpart.StartIndex, meshpart.PrimitiveCount);
                        }
                    }
                }

                pass.End();
            }
            effect.End();
            float xscale = 1, scale = 1;
            rm.spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);

            {//res
                int resIndex = -1;
                for (int i = 0; i < GameSettings.resY.Length; i++)
                {
                    if (GameSettings.resY[i] == GameSettings.windowheight)
                        for (int k = i; k < GameSettings.resX.Length; k++)
                            if (GameSettings.resX[k] == GameSettings.windowwidth)
                                resIndex = k;
                }
                if (resIndex == -1)
                    resIndex = 0;
                float x = 0.2f, y = 0.3f;
                rm.spritebatch.Draw(ttape, new Rectangle((int)((x + 0.07f) * GameSettings.windowwidth + optionsOffset * xscale), (int)(y * GameSettings.windowheight), (int)(0.15f * GameSettings.windowwidth), (int)(0.1f * GameSettings.windowheight)), Color.White);
                rm.spritebatch.DrawString(Global.DefaultFont, "Resolution", new Vector2(((x + 0.08f) * GameSettings.windowwidth + optionsOffset * xscale), (y + 0.01f) * GameSettings.windowheight), Color.Black, 0, new Vector2(0, 0), GameSettings.windowwidth / 1024f, SpriteEffects.None, 0);
                rm.spritebatch.DrawString(Global.DefaultFont, "" + GameSettings.resX[GameSettings.resIndex] + "x" + GameSettings.resY[GameSettings.resIndex], new Vector2(((x + 0.08f) * GameSettings.windowwidth + optionsOffset * xscale), (y + 0.05f) * GameSettings.windowheight), Color.Black, 0, new Vector2(0, 0), scale * (GameSettings.windowwidth / 1024f), SpriteEffects.None, 0);
                rm.spritebatch.Draw(tknob, new Vector2((x * GameSettings.windowwidth + optionsOffset * xscale), y * GameSettings.windowheight), null, Color.White, ((float)resIndex / (GameSettings.resY.Length - 1)) * -MathHelper.Pi, new Vector2(tknob.Width / 2, tknob.Height / 2), scale * (GameSettings.windowwidth / 1024f), SpriteEffects.None, 0);
            }
            {//gui
                float x = 0.4f, y = 0.6f;
                rm.spritebatch.Draw(ttape, new Rectangle((int)((x + 0.07f) * GameSettings.windowwidth + optionsOffset * xscale), (int)(y * GameSettings.windowheight), (int)(0.15f * GameSettings.windowwidth), (int)(0.1f * GameSettings.windowheight)), Color.White);
                rm.spritebatch.DrawString(Global.DefaultFont, "HUD Style", new Vector2(((x + 0.08f) * GameSettings.windowwidth + optionsOffset * xscale), (y + 0.01f) * GameSettings.windowheight), Color.Black, 0, new Vector2(0, 0), GameSettings.windowwidth / 1024f, SpriteEffects.None, 0);
                rm.spritebatch.DrawString(Global.DefaultFont, guiStyle[(int)GameSettings.cGUIStyle], new Vector2(((x + 0.08f) * GameSettings.windowwidth + optionsOffset * xscale), (y + 0.05f) * GameSettings.windowheight), Color.Black, 0, new Vector2(0, 0), scale * (GameSettings.windowwidth / 1024f), SpriteEffects.None, 0);
                rm.spritebatch.Draw(tknob, new Vector2((x * GameSettings.windowwidth + optionsOffset * xscale), y * GameSettings.windowheight), null, Color.White, ((float)GameSettings.cGUIStyle / (guiStyle.Length - 1)) * -MathHelper.Pi, new Vector2(tknob.Width / 2, tknob.Height / 2), scale * (GameSettings.windowwidth / 1024f), SpriteEffects.None, 0);
            }
            {//gfxqual
                float x = 0.6f, y = 0.3f;
                rm.spritebatch.Draw(ttape, new Rectangle((int)((x + 0.07f) * GameSettings.windowwidth + optionsOffset * xscale), (int)(y * GameSettings.windowheight), (int)(0.2f * GameSettings.windowwidth), (int)(0.1f * GameSettings.windowheight)), Color.White);
                rm.spritebatch.DrawString(Global.DefaultFont, "Graphics Level", new Vector2(((x + 0.08f) * GameSettings.windowwidth + optionsOffset * xscale), (y + 0.01f) * GameSettings.windowheight), Color.Black, 0, new Vector2(0, 0), GameSettings.windowwidth / 1024f, SpriteEffects.None, 0);
                rm.spritebatch.DrawString(Global.DefaultFont, "" + GameSettings.renderLevel, new Vector2(((x + 0.08f) * GameSettings.windowwidth + optionsOffset * xscale), (y + 0.05f) * GameSettings.windowheight), Color.Black, 0, new Vector2(0, 0), scale * (GameSettings.windowwidth / 1024f), SpriteEffects.None, 0);
                rm.spritebatch.Draw(tknob, new Vector2((x * GameSettings.windowwidth + optionsOffset * xscale), y * GameSettings.windowheight), null, Color.White, ((float)GameSettings.renderLevel / (10f - 1)) * -MathHelper.Pi, new Vector2(tknob.Width / 2, tknob.Height / 2), scale * (GameSettings.windowwidth / 1024f), SpriteEffects.None, 0);
            }
            {//3don
                float x = 0.8f, y = 0.6f;
                rm.spritebatch.Draw(ttape, new Rectangle((int)((x + 0.03f) * GameSettings.windowwidth + optionsOffset * xscale), (int)((y - 0.05f) * GameSettings.windowheight), (int)(0.15f * GameSettings.windowwidth), (int)(0.1f * GameSettings.windowheight)), Color.White);
                rm.spritebatch.DrawString(Global.DefaultFont, "3D Mode?", new Vector2(((x + 0.04f) * GameSettings.windowwidth + optionsOffset * xscale), (y - 0.04f) * GameSettings.windowheight), Color.Black, 0, new Vector2(0, 0), GameSettings.windowwidth / 1024f, SpriteEffects.None, 0);
                rm.spritebatch.DrawString(Global.DefaultFont, GameSettings.render3D ? "On" : "Off", new Vector2(((x + 0.04f) * GameSettings.windowwidth + optionsOffset * xscale), (y) * GameSettings.windowheight), Color.Black, 0, new Vector2(0, 0), scale * (GameSettings.windowwidth / 1024f), SpriteEffects.None, 0);
                rm.spritebatch.Draw(GameSettings.render3D ? tswitchon : tswitchoff, new Vector2((x * GameSettings.windowwidth + optionsOffset * xscale), y * GameSettings.windowheight), null, Color.White, 0, new Vector2(tswitchon.Width / 2, tswitchon.Height / 2), scale * (GameSettings.windowwidth / 1024f), SpriteEffects.None, 0);
                rm.spritebatch.Draw(GameSettings.render3D ? tledon : tledoff, new Vector2((x * GameSettings.windowwidth + optionsOffset * xscale), (y - 0.13f) * GameSettings.windowheight), null, Color.White, 0, new Vector2(tledon.Width / 2, tledon.Height / 2), scale * (GameSettings.windowwidth / 1024f), SpriteEffects.None, 0);
            }
            {//fulls
                float x = 1f, y = 0.4f;
                rm.spritebatch.Draw(ttape, new Rectangle((int)((x + 0.03f) * GameSettings.windowwidth + optionsOffset * xscale), (int)((y - 0.05f) * GameSettings.windowheight), (int)(0.15f * GameSettings.windowwidth), (int)(0.1f * GameSettings.windowheight)), Color.White);
                rm.spritebatch.DrawString(Global.DefaultFont, "Full Screen?", new Vector2(((x + 0.04f) * GameSettings.windowwidth + optionsOffset * xscale), (y - 0.04f) * GameSettings.windowheight), Color.Black, 0, new Vector2(0, 0), GameSettings.windowwidth / 1024f, SpriteEffects.None, 0);
                rm.spritebatch.DrawString(Global.DefaultFont, GameSettings.fullScreen ? "On" : "Off", new Vector2(((x + 0.04f) * GameSettings.windowwidth + optionsOffset * xscale), (y) * GameSettings.windowheight), Color.Black, 0, new Vector2(0, 0), scale * (GameSettings.windowwidth / 1024f), SpriteEffects.None, 0);
                rm.spritebatch.Draw(GameSettings.fullScreen ? tswitchon : tswitchoff, new Vector2((x * GameSettings.windowwidth + optionsOffset * xscale), y * GameSettings.windowheight), null, Color.White, 0, new Vector2(tswitchon.Width / 2, tswitchon.Height / 2), scale * (GameSettings.windowwidth / 1024f), SpriteEffects.None, 0);
                rm.spritebatch.Draw(GameSettings.fullScreen ? tledon : tledoff, new Vector2((x * GameSettings.windowwidth + optionsOffset * xscale), (y - 0.13f) * GameSettings.windowheight), null, Color.White, 0, new Vector2(tledon.Width / 2, tledon.Height / 2), scale * (GameSettings.windowwidth / 1024f), SpriteEffects.None, 0);
            }
            {//fps
                float x = 1.2f, y = 0.6f;
                rm.spritebatch.Draw(ttape, new Rectangle((int)((x + 0.03f) * GameSettings.windowwidth + optionsOffset * xscale), (int)((y - 0.05f) * GameSettings.windowheight), (int)(0.15f * GameSettings.windowwidth), (int)(0.1f * GameSettings.windowheight)), Color.White);
                rm.spritebatch.DrawString(Global.DefaultFont, "Show FPS?", new Vector2(((x + 0.04f) * GameSettings.windowwidth + optionsOffset * xscale), (y - 0.04f) * GameSettings.windowheight), Color.Black, 0, new Vector2(0, 0), GameSettings.windowwidth / 1024f, SpriteEffects.None, 0);
                rm.spritebatch.DrawString(Global.DefaultFont, GameSettings.ShowFPS ? "On" : "Off", new Vector2(((x + 0.04f) * GameSettings.windowwidth + optionsOffset * xscale), (y) * GameSettings.windowheight), Color.Black, 0, new Vector2(0, 0), scale * (GameSettings.windowwidth / 1024f), SpriteEffects.None, 0);
                rm.spritebatch.Draw(GameSettings.ShowFPS ? tswitchon : tswitchoff, new Vector2((x * GameSettings.windowwidth + optionsOffset * xscale), y * GameSettings.windowheight), null, Color.White, 0, new Vector2(tswitchon.Width / 2, tswitchon.Height / 2), scale * (GameSettings.windowwidth / 1024f), SpriteEffects.None, 0);
                rm.spritebatch.Draw(GameSettings.ShowFPS ? tledon : tledoff, new Vector2((x * GameSettings.windowwidth + optionsOffset * xscale), (y - 0.13f) * GameSettings.windowheight), null, Color.White, 0, new Vector2(tledon.Width / 2, tledon.Height / 2), scale * (GameSettings.windowwidth / 1024f), SpriteEffects.None, 0);
            }

            rm.spritebatch.Draw(GameUIMaster.GetSingleton().texButtonGreen, new Rectangle((int)(0.1f * GameSettings.windowwidth), (int)(0.80f * GameSettings.windowheight), (int)(0.09f * GameSettings.windowheight), (int)(0.09f * GameSettings.windowheight)), Color.White);
            rm.spritebatch.DrawString(Global.DefaultFont, "Select", new Vector2((0.1f * GameSettings.windowwidth) + (0.10f * GameSettings.windowheight), (0.80f * GameSettings.windowheight) + (0.09f * GameSettings.windowheight) - (Global.DefaultFont.MeasureString("Select").Y)), Color.White);
            rm.spritebatch.Draw(GameUIMaster.GetSingleton().texButtonRed, new Rectangle((int)(0.9f * GameSettings.windowwidth) - (int)(0.09f * GameSettings.windowheight), (int)(0.80f * GameSettings.windowheight), (int)(0.09f * GameSettings.windowheight), (int)(0.09f * GameSettings.windowheight)), Color.White);
            rm.spritebatch.DrawString(Global.DefaultFont, "Back", new Vector2((0.9f * GameSettings.windowwidth) - (0.10f * GameSettings.windowheight) - Global.DefaultFont.MeasureString("Back").X, (0.80f * GameSettings.windowheight) + (0.09f * GameSettings.windowheight) - (Global.DefaultFont.MeasureString("Back").Y)), Color.White);
            if (Global.DemoMode)
            {
                rm.spritebatch.DrawString(Global.BigFont, "Demo Mode", new Vector2((GameSettings.windowwidth / 2) - (Global.BigFont.MeasureString("Demo Mode").X / 2), GameSettings.windowheight * 0.15f), new Color(255, 0, 0, 64));
                rm.spritebatch.DrawString(Global.BigFont, "Demo Mode", new Vector2((GameSettings.windowwidth / 2) - (Global.BigFont.MeasureString("Demo Mode").X / 2), GameSettings.windowheight * 0.4f), new Color(255, 0, 0, 64));
                rm.spritebatch.DrawString(Global.BigFont, "Demo Mode", new Vector2((GameSettings.windowwidth / 2) - (Global.BigFont.MeasureString("Demo Mode").X / 2), GameSettings.windowheight * 0.65f), new Color(255, 0, 0, 64));
            }
            rm.spritebatch.End();
#if !DEBUG
                    }
                    catch(Exception e)
                    {
#if WINDOWS
                        System.Windows.Forms.MessageBox.Show("Problem in Draw/CD/Pt2\n"+e.Message+"\n"+e.StackTrace);
#endif
                        Exit();
                        return;
                    }
#endif
        }
    }
}

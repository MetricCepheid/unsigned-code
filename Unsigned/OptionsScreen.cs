using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UnsignedPeripheralPlugins;

namespace Unsigned
{
    class OptionsScreen : BaseState
    {
        private Texture2D concrTex, concrBM;
        private Texture2D tamp1, tamp2;
        private Texture2D tknob, ttape;
        private Texture2D tledon, tledoff, tswitchon, tswitchoff;

        private Model mamp1, mamp2;

        private String[] guiStyle = { "Unsigned", "Rock Band" };

        private float optionsOffset = 0;
        private OPTIONS optionsSelected;

        private float knobBroken;

        private float intro;

        private int rockLevel;

        private enum OPTIONS
        {
            OPT_ROCKLEVEL=1,
            OPT_RESOLUTION,
            OPT_GUISTYLE,
            OPT_RENDERLEVEL,
            OPT_RENDER3D,
            OPT_FULLSCREEN,
            OPT_SHOWFPS,
            MAX,
        };

        private int resIndex;
        private bool fullScreen;

        public OptionsScreen()
        {

        }

        public override void Load(ContentManager content)
        {
            mamp1 = content.Load<Model>("meshes\\amp1");
            tamp1 = content.Load<Texture2D>("graphics\\amp");
            mamp2 = content.Load<Model>("meshes\\amppanel");
            tamp2 = content.Load<Texture2D>("graphics\\amppanel");
            tknob = content.Load<Texture2D>("graphics\\knob");
            ttape = content.Load<Texture2D>("graphics\\tape");
            tledon = content.Load<Texture2D>("graphics\\ledon");
            tledoff = content.Load<Texture2D>("graphics\\ledoff");
            tswitchon = content.Load<Texture2D>("graphics\\switchon");
            tswitchoff = content.Load<Texture2D>("graphics\\switchoff");
            concrTex = content.Load<Texture2D>("graphics\\concr");
            concrBM = content.Load<Texture2D>("graphics\\concrBM");
            resIndex = GameSettings.resIndex;
            fullScreen = GameSettings.fullScreen;
            optionsSelected = OPTIONS.OPT_RESOLUTION;
            rockLevel = 11;
        }

        public override void Unload()
        {
            GameSettings.resIndex = resIndex;
            GameSettings.fullScreen = fullScreen;
        }

        public override void Update(GameTime gameTime)
        {
            Peripheral[] conts = PeripheralManager.GetSingleton().GetPeripherals();

            int offset = 0;
            bool green = false, red = false;
            for (int i = 0; i < conts.Length; i++)
            {
                if (conts[i].WasPressed(PeripheralButton.DOWN))
                    offset++;
                if (conts[i].WasPressed(PeripheralButton.UP))
                    offset--;
                if (conts[i].WasPressed(PeripheralButton.CONFIRM))
                    green = true;
                if (conts[i].WasPressed(PeripheralButton.BACK))
                    red = true;
            }
            optionsSelected += offset;
            if (optionsSelected < (OPTIONS)1)
                optionsSelected = OPTIONS.MAX - 1;
            if (optionsSelected >= OPTIONS.MAX)
                optionsSelected = (OPTIONS)1;

            if (green)
            {
                switch (optionsSelected)
                {
                    case OPTIONS.OPT_ROCKLEVEL:
                        knobBroken = 5;
                        break;
                    case OPTIONS.OPT_RESOLUTION:
                        resIndex++;
                        if (resIndex >= GameSettings.resX.Length)
                            resIndex = 0;
                        else if (GameSettings.resX[resIndex] > RenderMaster.GetSingleton().graphics.GraphicsDevice.DisplayMode.Width
                                || GameSettings.resY[resIndex] > RenderMaster.GetSingleton().graphics.GraphicsDevice.DisplayMode.Height)
                            resIndex = 0;
                        break;
                    case OPTIONS.OPT_GUISTYLE:
                        if (GameSettings.guiStyle == GameUIMaster.GUIStyle.RB)
                            GameSettings.guiStyle = GameUIMaster.GUIStyle.UN;
                        else if (GameSettings.guiStyle == GameUIMaster.GUIStyle.UN)
                            GameSettings.guiStyle = GameUIMaster.GUIStyle.RB;
                        break;
                    case OPTIONS.OPT_RENDERLEVEL:
                        GameSettings.renderLevel++;
                        if (GameSettings.renderLevel > 10)
                            GameSettings.renderLevel = 1;
                        break;
                    case OPTIONS.OPT_RENDER3D:
                        GameSettings.render3D = !GameSettings.render3D;
                        break;
                    case OPTIONS.OPT_FULLSCREEN:
                        fullScreen = !fullScreen;
                        break;
                    case OPTIONS.OPT_SHOWFPS:
                        GameSettings.ShowFPS = !GameSettings.ShowFPS;
                        break;
                }
            }
            if (red)
                UnsignedGame.GetSingleton().PopState();

            optionsOffset = (((int)optionsSelected-3) * 0.2f) + (optionsOffset * 0.8f);

            if (knobBroken > 0)
                knobBroken -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
        public override void Render(GameTime gameTime)
        {
            UnsignedGame game = UnsignedGame.GetSingleton();
            RenderMaster rm = RenderMaster.GetSingleton();
            Effect effect = rm.engine;


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
                Vector3 campos = ((1 - introlerp) * (new Vector3(-8, 112, 24))) + ((introlerp) * (new Vector3(-0.2f+(optionsOffset*0.2f), 106.3f, 1f)));
                Matrix matView = Matrix.CreateLookAt(campos, new Vector3(-0.2f + (optionsOffset * 0.2f), 106.3f, 0), new Vector3(0, 1, 0));
                rm.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, rm.graphics.GraphicsDevice.Viewport.Height / (float)rm.graphics.GraphicsDevice.Viewport.Height, 1.0f, 50);
                //render the background graphics
                rm.SetViewMatrix(matView);

                Matrix matRot, matScale, matTranslate;
                {//basebottom
                    matTranslate = Matrix.CreateTranslation(0, 100, 0);
                    matRot = Matrix.CreateRotationX((float)Math.PI);
                    matScale = Matrix.CreateScale(40, 0, 32);

                    effect.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                    effect.Parameters["wRot"].SetValue(matRot);
                    effect.Parameters["diffuseTexture"].SetValue(concrTex);
                    effect.Parameters["bumpTexture"].SetValue(concrBM);
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
                    effect.Parameters["diffuseTexture"].SetValue(concrTex);
                    effect.Parameters["bumpTexture"].SetValue(concrBM);
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
                    effect.Parameters["diffuseTexture"].SetValue(concrTex);
                    effect.Parameters["bumpTexture"].SetValue(concrBM);
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
            float xscale = -GameSettings.windowwidth*0.2f, scale = 1;
            rm.spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);

            {//rocklev
                float x = 0.2f * (int)OPTIONS.OPT_ROCKLEVEL, y = 0.6f;
                rm.spritebatch.Draw(ttape, new Rectangle((int)((x + 0.07f) * GameSettings.windowwidth + optionsOffset * xscale), (int)(y * GameSettings.windowheight), (int)(0.15f * GameSettings.windowwidth), (int)(0.1f * GameSettings.windowheight)), Color.White);
                rm.spritebatch.DrawString(Global.DefaultFont, "Rock Level", new Vector2(((x + 0.08f) * GameSettings.windowwidth + optionsOffset * xscale), (y + 0.01f) * GameSettings.windowheight), Color.Black, 0, new Vector2(0, 0), GameSettings.windowwidth / 1024f, SpriteEffects.None, 0);
                rm.spritebatch.DrawString(Global.DefaultFont, "" + rockLevel, new Vector2(((x + 0.1f) * GameSettings.windowwidth + optionsOffset * xscale), (y + 0.05f) * GameSettings.windowheight), Color.Black, 0, new Vector2(0, 0), scale * (GameSettings.windowwidth / 1024f), SpriteEffects.None, 0);
                rm.spritebatch.Draw(tknob, new Vector2((x * GameSettings.windowwidth + optionsOffset * xscale), y * GameSettings.windowheight), null, optionsSelected == OPTIONS.OPT_ROCKLEVEL ? Color.White : Color.Gray, (rockLevel / 10f) * -MathHelper.Pi, new Vector2(tknob.Width / 2, tknob.Height / 2), scale * (GameSettings.windowwidth / 1024f), SpriteEffects.None, 0);
            }
            {//res
                float x = 0.2f*(int)OPTIONS.OPT_RESOLUTION, y = 0.3f;
                rm.spritebatch.Draw(ttape, new Rectangle((int)((x + 0.07f) * GameSettings.windowwidth + optionsOffset * xscale), (int)(y * GameSettings.windowheight), (int)(0.15f * GameSettings.windowwidth), (int)(0.1f * GameSettings.windowheight)), Color.White);
                rm.spritebatch.DrawString(Global.DefaultFont, "Resolution", new Vector2(((x + 0.08f) * GameSettings.windowwidth + optionsOffset * xscale), (y + 0.01f) * GameSettings.windowheight), Color.Black, 0, new Vector2(0, 0), GameSettings.windowwidth / 1024f, SpriteEffects.None, 0);
                rm.spritebatch.DrawString(Global.DefaultFont, "" + GameSettings.resX[resIndex] + "x" + GameSettings.resY[resIndex], new Vector2(((x + 0.08f) * GameSettings.windowwidth + optionsOffset * xscale), (y + 0.05f) * GameSettings.windowheight), Color.Black, 0, new Vector2(0, 0), scale * (GameSettings.windowwidth / 1024f), SpriteEffects.None, 0);
                rm.spritebatch.Draw(tknob, new Vector2((x * GameSettings.windowwidth + optionsOffset * xscale), y * GameSettings.windowheight), null, optionsSelected == OPTIONS.OPT_RESOLUTION ? Color.White : Color.Gray, (resIndex / (float)(GameSettings.resY.Length - 1)) * -MathHelper.Pi, new Vector2(tknob.Width / 2, tknob.Height / 2), scale * (GameSettings.windowwidth / 1024f), SpriteEffects.None, 0);
            }
            {//gui
                float x = 0.2f * (int)OPTIONS.OPT_GUISTYLE, y = 0.6f;
                rm.spritebatch.Draw(ttape, new Rectangle((int)((x + 0.07f) * GameSettings.windowwidth + optionsOffset * xscale), (int)(y * GameSettings.windowheight), (int)(0.15f * GameSettings.windowwidth), (int)(0.1f * GameSettings.windowheight)), Color.White);
                rm.spritebatch.DrawString(Global.DefaultFont, "HUD Style", new Vector2(((x + 0.08f) * GameSettings.windowwidth + optionsOffset * xscale), (y + 0.01f) * GameSettings.windowheight), Color.Black, 0, new Vector2(0, 0), GameSettings.windowwidth / 1024f, SpriteEffects.None, 0);
                rm.spritebatch.DrawString(Global.DefaultFont, guiStyle[(int)GameSettings.guiStyle], new Vector2(((x + 0.08f) * GameSettings.windowwidth + optionsOffset * xscale), (y + 0.05f) * GameSettings.windowheight), Color.Black, 0, new Vector2(0, 0), scale * (GameSettings.windowwidth / 1024f), SpriteEffects.None, 0);
                rm.spritebatch.Draw(tknob, new Vector2((x * GameSettings.windowwidth + optionsOffset * xscale), y * GameSettings.windowheight), null, optionsSelected == OPTIONS.OPT_GUISTYLE ? Color.White : Color.Gray, ((float)GameSettings.guiStyle / (guiStyle.Length - 1)) * -MathHelper.Pi, new Vector2(tknob.Width / 2, tknob.Height / 2), scale * (GameSettings.windowwidth / 1024f), SpriteEffects.None, 0);
            }
            {//gfxqual
                float x = 0.2f * (int)OPTIONS.OPT_RENDERLEVEL, y = 0.3f;
                rm.spritebatch.Draw(ttape, new Rectangle((int)((x + 0.07f) * GameSettings.windowwidth + optionsOffset * xscale), (int)(y * GameSettings.windowheight), (int)(0.2f * GameSettings.windowwidth), (int)(0.1f * GameSettings.windowheight)), Color.White);
                rm.spritebatch.DrawString(Global.DefaultFont, "Graphics Level", new Vector2(((x + 0.08f) * GameSettings.windowwidth + optionsOffset * xscale), (y + 0.01f) * GameSettings.windowheight), Color.Black, 0, new Vector2(0, 0), GameSettings.windowwidth / 1024f, SpriteEffects.None, 0);
                rm.spritebatch.DrawString(Global.DefaultFont, "" + GameSettings.renderLevel, new Vector2(((x + 0.08f) * GameSettings.windowwidth + optionsOffset * xscale), (y + 0.05f) * GameSettings.windowheight), Color.Black, 0, new Vector2(0, 0), scale * (GameSettings.windowwidth / 1024f), SpriteEffects.None, 0);
                rm.spritebatch.Draw(tknob, new Vector2((x * GameSettings.windowwidth + optionsOffset * xscale), y * GameSettings.windowheight), null, optionsSelected == OPTIONS.OPT_RENDERLEVEL ? Color.White : Color.Gray, ((float)(GameSettings.renderLevel-1) / (10f - 1)) * -MathHelper.Pi, new Vector2(tknob.Width / 2, tknob.Height / 2), scale * (GameSettings.windowwidth / 1024f), SpriteEffects.None, 0);
            }
            {//3don
                float x = 0.2f * (int)OPTIONS.OPT_RENDER3D, y = 0.6f;
                rm.spritebatch.Draw(ttape, new Rectangle((int)((x + 0.03f) * GameSettings.windowwidth + optionsOffset * xscale), (int)((y - 0.05f) * GameSettings.windowheight), (int)(0.15f * GameSettings.windowwidth), (int)(0.1f * GameSettings.windowheight)), Color.White);
                rm.spritebatch.DrawString(Global.DefaultFont, "3D Mode?", new Vector2(((x + 0.04f) * GameSettings.windowwidth + optionsOffset * xscale), (y - 0.04f) * GameSettings.windowheight), Color.Black, 0, new Vector2(0, 0), GameSettings.windowwidth / 1024f, SpriteEffects.None, 0);
                rm.spritebatch.DrawString(Global.DefaultFont, GameSettings.render3D ? "On" : "Off", new Vector2(((x + 0.04f) * GameSettings.windowwidth + optionsOffset * xscale), (y) * GameSettings.windowheight), Color.Black, 0, new Vector2(0, 0), scale * (GameSettings.windowwidth / 1024f), SpriteEffects.None, 0);
                rm.spritebatch.Draw(GameSettings.render3D ? tswitchon : tswitchoff, new Vector2((x * GameSettings.windowwidth + optionsOffset * xscale), y * GameSettings.windowheight), null, optionsSelected == OPTIONS.OPT_RENDER3D ? Color.White : Color.Gray, 0, new Vector2(tswitchon.Width / 2, tswitchon.Height / 2), scale * (GameSettings.windowwidth / 1024f), SpriteEffects.None, 0);
                rm.spritebatch.Draw(GameSettings.render3D ? tledon : tledoff, new Vector2((x * GameSettings.windowwidth + optionsOffset * xscale), (y - 0.13f) * GameSettings.windowheight), null, Color.White, 0, new Vector2(tledon.Width / 2, tledon.Height / 2), scale * (GameSettings.windowwidth / 1024f), SpriteEffects.None, 0);
            }
            {//fulls
                float x = 0.2f * (int)OPTIONS.OPT_FULLSCREEN, y = 0.4f;
                rm.spritebatch.Draw(ttape, new Rectangle((int)((x + 0.03f) * GameSettings.windowwidth + optionsOffset * xscale), (int)((y - 0.05f) * GameSettings.windowheight), (int)(0.15f * GameSettings.windowwidth), (int)(0.1f * GameSettings.windowheight)), Color.White);
                rm.spritebatch.DrawString(Global.DefaultFont, "Full Screen?", new Vector2(((x + 0.04f) * GameSettings.windowwidth + optionsOffset * xscale), (y - 0.04f) * GameSettings.windowheight), Color.Black, 0, new Vector2(0, 0), GameSettings.windowwidth / 1024f, SpriteEffects.None, 0);
                rm.spritebatch.DrawString(Global.DefaultFont, fullScreen ? "On" : "Off", new Vector2(((x + 0.04f) * GameSettings.windowwidth + optionsOffset * xscale), (y) * GameSettings.windowheight), Color.Black, 0, new Vector2(0, 0), scale * (GameSettings.windowwidth / 1024f), SpriteEffects.None, 0);
                rm.spritebatch.Draw(fullScreen ? tswitchon : tswitchoff, new Vector2((x * GameSettings.windowwidth + optionsOffset * xscale), y * GameSettings.windowheight), null, optionsSelected == OPTIONS.OPT_FULLSCREEN ? Color.White : Color.Gray, 0, new Vector2(tswitchon.Width / 2, tswitchon.Height / 2), scale * (GameSettings.windowwidth / 1024f), SpriteEffects.None, 0);
                rm.spritebatch.Draw(fullScreen ? tledon : tledoff, new Vector2((x * GameSettings.windowwidth + optionsOffset * xscale), (y - 0.13f) * GameSettings.windowheight), null, Color.White, 0, new Vector2(tledon.Width / 2, tledon.Height / 2), scale * (GameSettings.windowwidth / 1024f), SpriteEffects.None, 0);
            }
            {//fps
                float x = 0.2f * (int)OPTIONS.OPT_SHOWFPS, y = 0.6f;
                rm.spritebatch.Draw(ttape, new Rectangle((int)((x + 0.03f) * GameSettings.windowwidth + optionsOffset * xscale), (int)((y - 0.05f) * GameSettings.windowheight), (int)(0.15f * GameSettings.windowwidth), (int)(0.1f * GameSettings.windowheight)), Color.White);
                rm.spritebatch.DrawString(Global.DefaultFont, "Show FPS?", new Vector2(((x + 0.04f) * GameSettings.windowwidth + optionsOffset * xscale), (y - 0.04f) * GameSettings.windowheight), Color.Black, 0, new Vector2(0, 0), GameSettings.windowwidth / 1024f, SpriteEffects.None, 0);
                rm.spritebatch.DrawString(Global.DefaultFont, GameSettings.ShowFPS ? "On" : "Off", new Vector2(((x + 0.04f) * GameSettings.windowwidth + optionsOffset * xscale), (y) * GameSettings.windowheight), Color.Black, 0, new Vector2(0, 0), scale * (GameSettings.windowwidth / 1024f), SpriteEffects.None, 0);
                rm.spritebatch.Draw(GameSettings.ShowFPS ? tswitchon : tswitchoff, new Vector2((x * GameSettings.windowwidth + optionsOffset * xscale), y * GameSettings.windowheight), null, optionsSelected == OPTIONS.OPT_SHOWFPS ? Color.White : Color.Gray, 0, new Vector2(tswitchon.Width / 2, tswitchon.Height / 2), scale * (GameSettings.windowwidth / 1024f), SpriteEffects.None, 0);
                rm.spritebatch.Draw(GameSettings.ShowFPS ? tledon : tledoff, new Vector2((x * GameSettings.windowwidth + optionsOffset * xscale), (y - 0.13f) * GameSettings.windowheight), null, Color.White, 0, new Vector2(tledon.Width / 2, tledon.Height / 2), scale * (GameSettings.windowwidth / 1024f), SpriteEffects.None, 0);
            }

            if (knobBroken > 4)
            {
                rm.spritebatch.DrawString(Global.DefaultFont, "This knob appears to be broken", new Vector2(GameSettings.windowwidth * 0.1f, GameSettings.windowheight * 0.7f), new Color(Global.Orange1.R,Global.Orange1.G,Global.Orange1.B,(byte)((1-(knobBroken-4))*255)));
            }
            else if (knobBroken > 1)
            {
                rm.spritebatch.DrawString(Global.DefaultFont, "This knob appears to be broken", new Vector2(GameSettings.windowwidth * 0.1f, GameSettings.windowheight * 0.7f), Global.Orange1);
            }
            else if (knobBroken > 0)
            {
                rm.spritebatch.DrawString(Global.DefaultFont, "This knob appears to be broken", new Vector2(GameSettings.windowwidth * 0.1f, GameSettings.windowheight * 0.7f), new Color(Global.Orange1.R, Global.Orange1.G, Global.Orange1.B, (byte)(knobBroken * 255)));
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

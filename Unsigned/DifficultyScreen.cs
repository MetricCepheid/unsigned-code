using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using UnsignedPeripheralPlugins;

namespace Unsigned
{
    class DifficultyScreen : BaseState
    {
        byte[] diff;
        bool[] diffConfirm;
        private int menu_ticker = 0;
        private Texture2D whitishTex, whitishBM;
        private Texture2D[] texMetalTypes;
        private Texture2D glassboxTex, glassboxBM;
        private Dictionary<String, Model> instrumentModels;
        Model[][] trophies;

        public DifficultyScreen()
        {
            diffConfirm = new bool[4];
            for (int i = 0; i < 4; i++)
                diffConfirm[i] = false;
            
        }

        public override void Load(ContentManager content)
        {
            
        }

        public override void Unload(ContentManager content)
        {
            
        }

        public override void Update(GameTime gameTime)
        {
            
#if !DEBUG
                try
                {
#endif
                bool green = false, red = false;
                Peripheral[] controllers = PeripheralManager.GetSingleton().GetPeripherals();
                for (int i = 0; i < 4; i++)
                {
                    Peripheral p = PeripheralManager.GetSingleton().GetPeripheral(i);
                    if (p == null)
                        continue;
                    if (p.IsConnected())
                    {
                        if (p.WasPressed(PeripheralButton.GREEN))
                        {
                            if (diffConfirm[i])
                                green = true;
                            else
                                diffConfirm[i] = true;
                        }
                        if (p.WasPressed(PeripheralButton.RED))
                        {
                            if (diffConfirm[i])
                                diffConfirm[i] = false;
                            else
                                red = true;
                        }
                    }
                }
                for(int i=0;i<4;i++)
                {
                    Peripheral p = PeripheralManager.GetSingleton().GetPeripheral(i);
                    if(p!=null && !diffConfirm[i])
                    {
                        bool up = false, down = false;
                        if (p.WasPressed(PeripheralButton.DOWN))
                            down = true;
                        if (p.WasPressed(PeripheralButton.UP))
                            up = true;
                        if (up && diff[i] > 0)
                            diff[i]--;
                        if (down && diff[i] < 3)
                            diff[i]++;
                    }
                }

                bool allconfirmed = true;
                for (int i = 0; i < 4; i++)
                    if (PeripheralManager.GetSingleton().GetPeripheral(i)!=null && !diffConfirm[i])
                        allconfirmed = false;
                if (green && allconfirmed)
                {
                    if (Global.mode == Global.M_GAME)
                    {
                        GameState.CreateSingleton();
                        UnsignedGame.SINGLETON.PushState(GameState.GetSingleton());
                    }
                    // TODO: Work on freestyle
                    //else if (Global.mode == Global.M_FREESTYLE)
                    //    UnsignedGame.SINGLETON.PushState(new FreestyleState());
                }
                if (red)
                { UnsignedGame.SINGLETON.PopState(); }
#if !DEBUG
                }
                catch(Exception e)
                {
                    
#if WINDOWS
                    System.Windows.Forms.MessageBox.Show("Problem in Update/CD/Pt0\n"+e.Message+"\n"+e.StackTrace);
#endif
                    Exit();
                    return;
                }
            
#endif
            }

        public override void Render(GameTime gameTime)
        {
            

#if !DEBUG
                    try
                    {
#endif
            Effect engine = RenderMaster.GetSingleton().engine;

            RenderMaster.GetSingleton().graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
            RenderMaster.GetSingleton().graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
            //graphics.PreferMultiSampling = true;
            RenderMaster.GetSingleton().graphics.ApplyChanges();

            VertexDeclaration vd = new VertexDeclaration(RenderMaster.GetSingleton().graphics.GraphicsDevice, GBVertexFormat.Elements);
            RenderMaster.GetSingleton().graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
            //graphics.GraphicsDevice.

            engine.Parameters["bumpTexture"].SetValue(Global.texDefaultBM);
            engine.Parameters["ambientColor"].SetValue(new Vector4(0.1f, 0.1f, 0.1f, 1.0f));
            engine.Parameters["diffuseColor"].SetValue(new Vector4(0.8f, 0.8f, 0.8f, 1.0f));
            engine.Parameters["specularColor"].SetValue(new Vector4(1f, 1f, 1f, 1.0f));
            engine.Parameters["dLDiffuseColor"].SetValue(new Vector4(0, 0, 0, 0));
            engine.Parameters["dLSpecularColor"].SetValue(new Vector4(0, 0, 0, 0));
            RenderMaster.GetSingleton().RenderState.CullMode = CullMode.None;
            RenderMaster.GetSingleton().RenderState.DepthBufferEnable = true;
            RenderMaster.GetSingleton().RenderState.DepthBufferWriteEnable = true;

            Random r = new Random();


            Version SM =RenderMaster.GetSingleton().graphics.GraphicsDevice.GraphicsDeviceCapabilities.PixelShaderVersion;

            engine.Parameters["dLDiffuseColor"].SetValue(new Vector4(0.8f, 0.8f, 0.8f, 1.0f));
            engine.Parameters["dLSpecularColor"].SetValue(new Vector4(0.0f, 0.0f, 0.0f, 1.0f));
            engine.Parameters["dLightDir"].SetValue(Vector3.Normalize(new Vector3(0.1f, -1f, 0.5f)));
            if (SM.Major >= 3)
                engine.CurrentTechnique = RenderMaster.GetSingleton().engine.Techniques["menutechnique"];
            else if (SM.Major >= 2)
                engine.CurrentTechnique = RenderMaster.GetSingleton().engine.Techniques["menutechniquet"];
            else
                UnsignedGame.SINGLETON.InvalidShaderVersion();
            engine.CommitChanges();
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
            engine.Begin();
            foreach (EffectPass pass in engine.CurrentTechnique.Passes)
            {
                pass.Begin();
                Matrix matProj = Matrix.CreatePerspectiveFieldOfView((float)Math.PI / 4.0f,
                  GameSettings.windowwidth / (float)GameSettings.windowheight,
                  10f, 80.0f);
                engine.Parameters["fullbright"].SetValue(false);

                Matrix matView = Matrix.CreateLookAt(new Vector3(0, 108, 22), new Vector3(0, 104, 0), new Vector3(0, 1, 0));
                //render the background graphics
                engine.Parameters["view"].SetValue(matView);
                engine.Parameters["proj"].SetValue(matProj);
                engine.Parameters["viewInverse"].SetValue(Matrix.Invert(matView));

                Matrix matRot, matScale, matTranslate;
                              
                RhythmMaster rm = RhythmMaster.GetSingleton();
                for(int k=0;k<4;k++)
                    if (rm.IsInstrumentAvailable(k))
                    {
                        Model model = instrumentModels[rm.GetInstrumentType(k).CodeName];
                        matTranslate = Matrix.CreateTranslation(-12+(8*k), 105, -6);
                        matRot = Matrix.CreateRotationX(MathHelper.PiOver4);
                        matScale = Matrix.CreateScale(1, 1, 1);

                        engine.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                        engine.Parameters["wRot"].SetValue(matRot * Matrix.CreateRotationX(MathHelper.Pi / 2));

                        engine.Parameters["shininess"].SetValue(0.25f);
                        engine.Parameters["SpecularEnabled"].SetValue(false);
                        engine.Parameters["vertexAlpha"].SetValue(false);
                        engine.Parameters["BumpMappingEnabled"].SetValue(false);

                        engine.Parameters["diffuseTexture"].SetValue(texMetalTypes[diff[k]]);
                        engine.CommitChanges();
                        foreach (ModelMesh mesh in model.Meshes)
                        {
                            foreach (ModelMeshPart meshpart in mesh.MeshParts)
                            {
                                RenderMaster.GetSingleton().graphics.GraphicsDevice.VertexDeclaration = meshpart.VertexDeclaration;
                                RenderMaster.GetSingleton().graphics.GraphicsDevice.Vertices[0].SetSource(mesh.VertexBuffer, meshpart.StreamOffset, meshpart.VertexStride);
                                RenderMaster.GetSingleton().graphics.GraphicsDevice.Indices = mesh.IndexBuffer;
                                RenderMaster.GetSingleton().graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, meshpart.BaseVertex, 0, meshpart.NumVertices, meshpart.StartIndex, meshpart.PrimitiveCount);
                            }
                        }
                    }

                {//gctop
                    matTranslate = Matrix.CreateTranslation(0, 112, -16);
                    matRot = Matrix.CreateRotationX(-(float)Math.PI);
                    matScale = Matrix.CreateScale(20, 0, 16);

                    engine.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                    engine.Parameters["wRot"].SetValue(matRot);
                    engine.Parameters["diffuseTexture"].SetValue(glassboxTex);
                    engine.Parameters["bumpTexture"].SetValue(glassboxBM);
                    engine.Parameters["shininess"].SetValue(0.25f);
                    engine.Parameters["SpecularEnabled"].SetValue(false);
                    engine.Parameters["vertexAlpha"].SetValue(true);
                    engine.Parameters["BumpMappingEnabled"].SetValue(true);
                    engine.CommitChanges();

                   RenderMaster.GetSingleton().graphics.GraphicsDevice.VertexDeclaration = vd;
                   RenderMaster.GetSingleton().graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                   RenderMaster.GetSingleton().graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                   RenderMaster.GetSingleton().graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                   RenderMaster.GetSingleton().graphics.GraphicsDevice.Vertices[0].SetSource(Global.square, 0, GBVertexFormat.SizeInBytes);
                   RenderMaster.GetSingleton().graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
                   RenderMaster.GetSingleton().graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                }
                {//gcright
                    matTranslate = Matrix.CreateTranslation(20, 106, -16);
                    matRot = Matrix.CreateRotationX((float)Math.PI / 2) * Matrix.CreateRotationY(-MathHelper.PiOver2);
                    matScale = Matrix.CreateScale(16, 0, 6);

                    engine.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                    engine.Parameters["wRot"].SetValue(matRot);
                    engine.Parameters["diffuseTexture"].SetValue(glassboxTex);
                    engine.Parameters["bumpTexture"].SetValue(glassboxBM);
                    engine.Parameters["shininess"].SetValue(0.25f);
                    engine.Parameters["SpecularEnabled"].SetValue(false);
                    engine.Parameters["vertexAlpha"].SetValue(true);
                    engine.Parameters["BumpMappingEnabled"].SetValue(true);
                    engine.CommitChanges();

                   RenderMaster.GetSingleton().graphics.GraphicsDevice.VertexDeclaration = vd;
                   RenderMaster.GetSingleton().graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                   RenderMaster.GetSingleton().graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                   RenderMaster.GetSingleton().graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                   RenderMaster.GetSingleton().graphics.GraphicsDevice.Vertices[0].SetSource(Global.square, 0, GBVertexFormat.SizeInBytes);
                   RenderMaster.GetSingleton().graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
                   RenderMaster.GetSingleton().graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                }
                {//gcleft
                    matTranslate = Matrix.CreateTranslation(-20, 106, -16);
                    matRot = Matrix.CreateRotationX((float)Math.PI / 2) * Matrix.CreateRotationY(MathHelper.PiOver2);
                    matScale = Matrix.CreateScale(16, 0, 6);

                    engine.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                    engine.Parameters["wRot"].SetValue(matRot);
                    engine.Parameters["diffuseTexture"].SetValue(glassboxTex);
                    engine.Parameters["bumpTexture"].SetValue(glassboxBM);
                    engine.Parameters["shininess"].SetValue(0.25f);
                    engine.Parameters["SpecularEnabled"].SetValue(false);
                    engine.Parameters["vertexAlpha"].SetValue(true);
                    engine.Parameters["BumpMappingEnabled"].SetValue(true);
                    engine.CommitChanges();

                   RenderMaster.GetSingleton().graphics.GraphicsDevice.VertexDeclaration = vd;
                   RenderMaster.GetSingleton().graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                   RenderMaster.GetSingleton().graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                   RenderMaster.GetSingleton().graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                   RenderMaster.GetSingleton().graphics.GraphicsDevice.Vertices[0].SetSource(Global.square, 0, GBVertexFormat.SizeInBytes);
                   RenderMaster.GetSingleton().graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
                   RenderMaster.GetSingleton().graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                }
                {//gcfront
                    matTranslate = Matrix.CreateTranslation(0, 106, 0);
                    matRot = Matrix.CreateRotationX((float)Math.PI / 2);
                    matScale = Matrix.CreateScale(20, 0, 6);

                    engine.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                    engine.Parameters["wRot"].SetValue(matRot);
                    engine.Parameters["diffuseTexture"].SetValue(glassboxTex);
                    engine.Parameters["bumpTexture"].SetValue(glassboxBM);
                    engine.Parameters["shininess"].SetValue(0.25f);
                    engine.Parameters["SpecularEnabled"].SetValue(false);
                    engine.Parameters["vertexAlpha"].SetValue(true);
                    engine.Parameters["BumpMappingEnabled"].SetValue(true);
                    engine.CommitChanges();

                   RenderMaster.GetSingleton().graphics.GraphicsDevice.VertexDeclaration = vd;
                   RenderMaster.GetSingleton().graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                   RenderMaster.GetSingleton().graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                   RenderMaster.GetSingleton().graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                   RenderMaster.GetSingleton().graphics.GraphicsDevice.Vertices[0].SetSource(Global.square, 0, GBVertexFormat.SizeInBytes);
                   RenderMaster.GetSingleton().graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
                   RenderMaster.GetSingleton().graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                }

                pass.End();
            }
            engine.End();

            SpriteBatch spritebatch = RenderMaster.GetSingleton().Spritebatch;

            spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
            spritebatch.Draw(GameUIMaster.GetSingleton().texButtonGreen, new Rectangle((int)(0.1f * GameSettings.windowwidth), (int)(0.80f * GameSettings.windowheight), (int)(0.09f * GameSettings.windowheight), (int)(0.09f * GameSettings.windowheight)), Color.White);
            spritebatch.DrawString(Global.DefaultFont, "Select", new Vector2((0.1f * GameSettings.windowwidth) + (0.10f * GameSettings.windowheight), (0.80f * GameSettings.windowheight) + (0.09f * GameSettings.windowheight) - (Global.DefaultFont.MeasureString("Select").Y)), Color.White);
            spritebatch.Draw(GameUIMaster.GetSingleton().texButtonRed, new Rectangle((int)(0.9f * GameSettings.windowwidth) - (int)(0.09f * GameSettings.windowheight), (int)(0.80f * GameSettings.windowheight), (int)(0.09f * GameSettings.windowheight), (int)(0.09f * GameSettings.windowheight)), Color.White);
            spritebatch.DrawString(Global.DefaultFont, "Back", new Vector2((0.9f * GameSettings.windowwidth) - (0.10f * GameSettings.windowheight) - Global.DefaultFont.MeasureString("Back").X, (0.80f * GameSettings.windowheight) + (0.09f * GameSettings.windowheight) - (Global.DefaultFont.MeasureString("Back").Y)), Color.White);
            if (Global.DemoMode)
            {
                spritebatch.DrawString(Global.BigFont, "Demo Mode", new Vector2((GameSettings.windowwidth / 2) - (Global.BigFont.MeasureString("Demo Mode").X / 2), GameSettings.windowheight * 0.15f), new Color(255, 0, 0, 64));
                spritebatch.DrawString(Global.BigFont, "Demo Mode", new Vector2((GameSettings.windowwidth / 2) - (Global.BigFont.MeasureString("Demo Mode").X / 2), GameSettings.windowheight * 0.4f), new Color(255, 0, 0, 64));
                spritebatch.DrawString(Global.BigFont, "Demo Mode", new Vector2((GameSettings.windowwidth / 2) - (Global.BigFont.MeasureString("Demo Mode").X / 2), GameSettings.windowheight * 0.65f), new Color(255, 0, 0, 64));
            }
            spritebatch.End();
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

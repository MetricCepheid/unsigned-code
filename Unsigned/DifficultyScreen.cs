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
        bool[] diffConfirm;
        private Texture2D whitishTex, whitishBM;
        private Texture2D[] texMetalTypes;
        private Texture2D glassboxTex, glassboxBM;
        private Texture2D[][] texStrings;
        private Dictionary<String, Model> instrumentModels;
        private float instrRot;

        PlayerConfigNugget nugget;

        public DifficultyScreen(PlayerConfigNugget nugget)
        {
            diffConfirm = new bool[4];
            for (int i = 0; i < 4; i++)
                diffConfirm[i] = false;
            this.nugget = nugget;
            instrRot = 0;
        }

        public override void Load(ContentManager content)
        {
            instrumentModels = new Dictionary<string, Model>();
            String dir = System.IO.Directory.GetCurrentDirectory();
            dir += "\\meshes\\instruments\\";
            String[] files = System.IO.Directory.GetFiles(dir,"*.xnb");
            for (int i = 0; i < files.Length; i++)
            {
                files[i] = files[i].Substring(files[i].IndexOf("meshes\\"), files[i].Length - files[i].IndexOf("meshes\\"));
                files[i] = files[i].Substring(0,files[i].LastIndexOf('.'));
                String codename = files[i].Substring(files[i].LastIndexOf('\\') + 1);
                instrumentModels.Add(codename,content.Load<Model>(files[i]));
            }
            texMetalTypes = new Texture2D[4];
            texMetalTypes[0] = content.Load<Texture2D>("graphics\\instr_bronze");
            texMetalTypes[1] = content.Load<Texture2D>("graphics\\instr_silver");
            texMetalTypes[2] = content.Load<Texture2D>("graphics\\instr_gold");
            texMetalTypes[3] = content.Load<Texture2D>("graphics\\instr_platinum");
            glassboxTex = content.Load<Texture2D>("graphics\\glasscase");
            glassboxBM = content.Load<Texture2D>("graphics\\glasscasebm");
            whitishTex = content.Load<Texture2D>("graphics\\whitish");
            whitishBM = content.Load<Texture2D>("graphics\\whitishbm");
            texStrings = new Texture2D[4][];
            for (int i = 0; i < 4; i++)
            {
                texStrings[i] = new Texture2D[2];
                for (int k = 0; k < 2; k++)
                    texStrings[i][k] = content.Load<Texture2D>("graphics\\diff" + i + "" + k);
            }
        }

        public override void Unload()
        {
            
        }

        public override void Update(GameTime gameTime)
        {
            instrRot += (float)gameTime.ElapsedGameTime.TotalSeconds;
#if !DEBUG
                try
                {
#endif
                bool green = false, red = false;
                for (int i = 0; i < 4; i++)
                {
                    if (nugget.peripherals[i] == null)
                        continue;
                    if (nugget.peripherals[i].IsConnected())
                    {
                        if (nugget.peripherals[i].WasPressed(PeripheralButton.CONFIRM))
                        {
                            if (diffConfirm[i])
                                green = true;
                            else
                                diffConfirm[i] = true;
                        }
                        if (nugget.peripherals[i].WasPressed(PeripheralButton.BACK))
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
                    if (nugget.peripherals[i] != null && !diffConfirm[i])
                    {
                        bool up = false, down = false;
                        if (nugget.peripherals[i].WasPressed(PeripheralButton.DOWN))
                            down = true;
                        if (nugget.peripherals[i].WasPressed(PeripheralButton.UP))
                            up = true;
                        if (up && nugget.difficulties[i] > 0)
                            nugget.difficulties[i]--;
                        if (down && nugget.difficulties[i] < 3)
                            nugget.difficulties[i]++;
                    }
                }

                bool allconfirmed = true;
                for (int i = 0; i < 4; i++)
                    if (nugget.peripherals[i]!=null && !diffConfirm[i])
                        allconfirmed = false;
                if (green && allconfirmed)
                {
                    UnsignedGame.SINGLETON.PushState(new GameState(nugget));
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
            engine.Parameters["dLSpecularColor"].SetValue(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
            engine.Parameters["dLightDir"].SetValue(Vector3.Normalize(new Vector3(0.1f, -1f, 0.5f)));
            if (SM.Major >= 3)
                engine.CurrentTechnique = RenderMaster.GetSingleton().engine.Techniques["menutechnique"];
            else if (SM.Major >= 2)
                engine.CurrentTechnique = RenderMaster.GetSingleton().engine.Techniques["menutechniquet"];
            else
            {
#if WINDOWS
                System.Windows.Forms.MessageBox.Show("Whoops! Your graphics card only supports Shader Model " + SM.Major + "." + SM.Minor + "\nYou need at least 2.0 to run Unsigned");
#endif
                UnsignedGame.GetSingleton().Exit();
            }
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
                {//basebottom
                    matTranslate = Matrix.CreateTranslation(0, 100, 0);
                    matRot = Matrix.CreateRotationX((float)Math.PI);
                    matScale = Matrix.CreateScale(40, 0, 32);

                    engine.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                    engine.Parameters["wRot"].SetValue(matRot);
                    engine.Parameters["diffuseTexture"].SetValue(whitishTex);
                    engine.Parameters["bumpTexture"].SetValue(whitishBM);
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
                {//basewall
                    matTranslate = Matrix.CreateTranslation(0, 132, -32);
                    matRot = Matrix.CreateRotationX((float)Math.PI / 2);
                    matScale = Matrix.CreateScale(40, 0, 32);

                    engine.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                    engine.Parameters["wRot"].SetValue(matRot);
                    engine.Parameters["diffuseTexture"].SetValue(whitishTex);
                    engine.Parameters["bumpTexture"].SetValue(whitishBM);
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
                
                for(int k=0;k<4;k++)
                    if (nugget.peripherals[k]!=null)
                    {
                        Model model = instrumentModels[InstrumentMaster.GetSingleton().GetInstrument(nugget.instruments[k]).CodeName];
                        matTranslate = Matrix.CreateTranslation(-10+(5*k), 105, -6);
                        matRot = Matrix.CreateRotationY(instrRot)*Matrix.CreateRotationZ(0.1f)*Matrix.CreateRotationY(-instrRot)*Matrix.CreateRotationX(MathHelper.PiOver4);
                        matScale = Matrix.CreateScale(1.25f, 1.25f, 1.25f);

                        engine.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                        engine.Parameters["wRot"].SetValue(matRot * Matrix.CreateRotationX(MathHelper.Pi / 2));

                        engine.Parameters["shininess"].SetValue(1.0f);
                        engine.Parameters["SpecularEnabled"].SetValue(true);
                        engine.Parameters["specularColor"].SetValue(new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
                        engine.Parameters["vertexAlpha"].SetValue(false);
                        engine.Parameters["BumpMappingEnabled"].SetValue(false);

                        engine.Parameters["diffuseTexture"].SetValue(texMetalTypes[nugget.difficulties[k]]);
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

            SpriteBatch spritebatch = RenderMaster.GetSingleton().spritebatch;

            spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
            for (int i = 0; i < 4; i++)
                if(nugget.peripherals[i]!=null)
                {
                    Texture2D tex = texStrings[nugget.difficulties[i]][diffConfirm[i]?1:0];
                    spritebatch.Draw(tex, new Rectangle((int)((GameSettings.windowwidth * 0.125f) + (GameSettings.windowwidth * 0.25f * i) - (tex.Width / 2)), 400, tex.Width, tex.Height), Color.White);
                }
            /*
            spritebatch.Draw(GameUIMaster.GetSingleton().texButtonGreen, new Rectangle((int)(0.1f * GameSettings.windowwidth), (int)(0.80f * GameSettings.windowheight), (int)(0.09f * GameSettings.windowheight), (int)(0.09f * GameSettings.windowheight)), Color.White);
            spritebatch.DrawString(Global.DefaultFont, "Select", new Vector2((0.1f * GameSettings.windowwidth) + (0.10f * GameSettings.windowheight), (0.80f * GameSettings.windowheight) + (0.09f * GameSettings.windowheight) - (Global.DefaultFont.MeasureString("Select").Y)), Color.White);
            spritebatch.Draw(GameUIMaster.GetSingleton().texButtonRed, new Rectangle((int)(0.9f * GameSettings.windowwidth) - (int)(0.09f * GameSettings.windowheight), (int)(0.80f * GameSettings.windowheight), (int)(0.09f * GameSettings.windowheight), (int)(0.09f * GameSettings.windowheight)), Color.White);
            spritebatch.DrawString(Global.DefaultFont, "Back", new Vector2((0.9f * GameSettings.windowwidth) - (0.10f * GameSettings.windowheight) - Global.DefaultFont.MeasureString("Back").X, (0.80f * GameSettings.windowheight) + (0.09f * GameSettings.windowheight) - (Global.DefaultFont.MeasureString("Back").Y)), Color.White);
            */
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

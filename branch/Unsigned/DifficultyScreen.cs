using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using UnsignedPeripheralPlugins;
using SongDataIO;

namespace Unsigned
{
    class DifficultyScreen : BaseState
    {
        private ContentManager content;
        bool[] diffConfirm;
        private Texture2D whitishTex, whitishBM;
        private Texture2D[] texMetalTypes;
        private TextureCube texLight;
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

        public override void Load()
        {
            content = new ContentManager(UnsignedGame.GetSingleton().Services);
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
            Texture2D light = content.Load<Texture2D>("graphics\\instr_lightingcube");
            {
                texLight = new TextureCube(RenderMaster.GetSingleton().graphics.GraphicsDevice, light.Width, 0, TextureUsage.AutoGenerateMipMap, SurfaceFormat.Color);
                Color[] data = new Color[light.Width*light.Height];
                light.GetData<Color>(data);
                texLight.SetData<Color>(CubeMapFace.NegativeX, data);
                texLight.SetData<Color>(CubeMapFace.PositiveX, data);
                texLight.SetData<Color>(CubeMapFace.NegativeY, data);
                texLight.SetData<Color>(CubeMapFace.PositiveY, data);
                texLight.SetData<Color>(CubeMapFace.NegativeZ, data);
                texLight.SetData<Color>(CubeMapFace.PositiveZ, data);
            }
            light = null;
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
            content.Unload();
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
                    UnsignedGame.GetSingleton().Exit();
                    return;
                }
            
#endif
            }

        public override void Render(GameTime gameTime)
        {

            FVShader engine = RenderMaster.GetSingleton().menuEngine;

#if !DEBUG
                    try
                    {
#endif

            RenderMaster.GetSingleton().graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
            RenderMaster.GetSingleton().graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
            //graphics.PreferMultiSampling = true;
            RenderMaster.GetSingleton().graphics.ApplyChanges();

            RenderMaster.GetSingleton().graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
            //graphics.GraphicsDevice.

            engine.NormalMapTexture =Global.texDefaultBM;
            engine.AmbientMaterial = new Color(24, 24, 24);
            engine.DiffuseMaterial = new Color(200, 200, 200);
            engine.SpecularMaterial = Color.White;

            engine.LightingEnabled = GameSettings.Lighting;
            engine.SpecularEnabled = GameSettings.Specular;
            engine.NormalMapEnabled = GameSettings.NormalMapping;
            
            RenderMaster.GetSingleton().RenderState.CullMode = CullMode.None;
            RenderMaster.GetSingleton().RenderState.DepthBufferEnable = true;
            RenderMaster.GetSingleton().RenderState.DepthBufferWriteEnable = true;

            Random r = new Random();


            Version SM =RenderMaster.GetSingleton().graphics.GraphicsDevice.GraphicsDeviceCapabilities.PixelShaderVersion;

            engine.DirectionalLight = new DirectionalLight(true, new Vector3(0.1f, -1f, 0.5f), new Color(200, 200, 200), Color.White);
            if (SM.Major < 1 || (SM.Major==1 && SM.Minor<1))
            {
#if WINDOWS
                System.Windows.Forms.MessageBox.Show("Whoops! Your graphics card only supports Shader Model " + SM.Major + "." + SM.Minor + "\nYou need at least 1.1 to run Unsigned");
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
                        UnsignedGame.GetSingleton().Exit();
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
                

                Matrix matView = Matrix.CreateLookAt(new Vector3(0, 108, 22), new Vector3(0, 104, 0), new Vector3(0, 1, 0));
                //render the background graphics
                engine.View = matView;
                engine.Projection = matProj;

                Matrix matRot, matScale, matTranslate;
                {//basebottom
                    matTranslate = Matrix.CreateTranslation(0, 100, 0);
                    matRot = Matrix.CreateRotationX((float)Math.PI);
                    matScale = Matrix.CreateScale(40, 0, 32);

                    engine.World = matScale * matRot * matTranslate;
                    engine.DiffuseTexture = whitishTex;
                    engine.NormalMapTexture =whitishBM;
                    engine.Shininess = 0.25f;
                    engine.SpecularMaterial = Color.Black;
                    engine.CommitChanges();

                    RenderMaster.GetSingleton().graphics.GraphicsDevice.VertexDeclaration = GBVertexFormat.VertexDeclaration;
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

                    engine.World = matScale * matRot * matTranslate;
                    engine.DiffuseTexture = whitishTex;
                    engine.NormalMapTexture =whitishBM;
                    engine.Shininess = 0.25f;
                    engine.CommitChanges();

                    RenderMaster.GetSingleton().graphics.GraphicsDevice.VertexDeclaration = GBVertexFormat.VertexDeclaration;
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
            engine.CurrentTechnique = engine.Techniques["spheremappingtechnique"];
            engine.Begin();
            foreach(EffectPass pass in engine.CurrentTechnique.Passes)
            {
                pass.Begin();

                Matrix matRot, matScale, matTranslate;
                for(int k=0;k<4;k++)
                    if (nugget.peripherals[k]!=null)
                    {
                        Model model = instrumentModels[InstrumentMaster.GetSingleton().GetInstrument(nugget.instruments[k]).CodeName];
                        matTranslate = Matrix.CreateTranslation(-10+(5*k), 105, -6);
                        matRot = Matrix.CreateRotationY(instrRot)*Matrix.CreateRotationZ(0.1f)*Matrix.CreateRotationY(-instrRot)*Matrix.CreateRotationX(MathHelper.PiOver4);
                        matScale = Matrix.CreateScale(1.25f, 1.25f, 1.25f);

                        engine.World = matScale * matRot * matTranslate;

                        engine.Shininess = 1.0f;
                        engine.SpecularMaterial = new Color(128, 128, 128);
                        engine.NormalMapTexture = Global.texDefaultBM;

                        engine.DiffuseTexture = texMetalTypes[nugget.difficulties[k]];
                        engine.CubeMapTexture = texLight;
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


                pass.End();
            }
            engine.End();
            engine.UpdateTechnique();

            SpriteBatch spritebatch = RenderMaster.GetSingleton().spritebatch;

            spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
            for (int i = 0; i < 4; i++)
                if(nugget.peripherals[i]!=null)
                {
                    Texture2D tex = texStrings[nugget.difficulties[i]][diffConfirm[i]?1:0];
                    spritebatch.Draw(tex, new Rectangle((int)((GameSettings.windowwidth * 0.125f) + (GameSettings.windowwidth * 0.25f * i) - ((tex.Width * (GameSettings.windowwidth / 800f)) / 2)), (int)(GameSettings.windowheight * 0.7f), (int)(tex.Width * (GameSettings.windowwidth / 800f)), (int)(tex.Height * (GameSettings.windowwidth / 800f))), Color.White);
                }
            /*
            spritebatch.Draw(GameUIMaster.GetSingleton().texButtonGreen, new Rectangle((int)(0.1f * GameSettings.windowwidth), (int)(0.80f * GameSettings.windowheight), (int)(0.09f * GameSettings.windowheight), (int)(0.09f * GameSettings.windowheight)), Color.White);
            spritebatch.DrawString(Global.DefaultFont, "Select", new Vector2((0.1f * GameSettings.windowwidth) + (0.10f * GameSettings.windowheight), (0.80f * GameSettings.windowheight) + (0.09f * GameSettings.windowheight) - (Global.DefaultFont.MeasureString("Select").Y)), Color.White);
            spritebatch.Draw(GameUIMaster.GetSingleton().texButtonRed, new Rectangle((int)(0.9f * GameSettings.windowwidth) - (int)(0.09f * GameSettings.windowheight), (int)(0.80f * GameSettings.windowheight), (int)(0.09f * GameSettings.windowheight), (int)(0.09f * GameSettings.windowheight)), Color.White);
            spritebatch.DrawString(Global.DefaultFont, "Back", new Vector2((0.9f * GameSettings.windowwidth) - (0.10f * GameSettings.windowheight) - Global.DefaultFont.MeasureString("Back").X, (0.80f * GameSettings.windowheight) + (0.09f * GameSettings.windowheight) - (Global.DefaultFont.MeasureString("Back").Y)), Color.White);
            */
            if (Global.DemoMode)
            {
                spritebatch.DrawString(Global.BigFont, Localizer.Get("Demo Mode"), new Vector2((GameSettings.windowwidth / 2) - (Global.BigFont.MeasureString(Localizer.Get("Demo Mode")).X / 2), GameSettings.windowheight * 0.15f), new Color(255, 0, 0, 64));
                spritebatch.DrawString(Global.BigFont, Localizer.Get("Demo Mode"), new Vector2((GameSettings.windowwidth / 2) - (Global.BigFont.MeasureString(Localizer.Get("Demo Mode")).X / 2), GameSettings.windowheight * 0.4f), new Color(255, 0, 0, 64));
                spritebatch.DrawString(Global.BigFont, Localizer.Get("Demo Mode"), new Vector2((GameSettings.windowwidth / 2) - (Global.BigFont.MeasureString(Localizer.Get("Demo Mode")).X / 2), GameSettings.windowheight * 0.65f), new Color(255, 0, 0, 64));
            }
            spritebatch.End();
#if !DEBUG
                    }
                    catch(Exception e)
                    {
#if WINDOWS
                        System.Windows.Forms.MessageBox.Show("Problem in Draw/CD/Pt2\n"+e.Message+"\n"+e.StackTrace);
#endif
                        UnsignedGame.GetSingleton().Exit();
                        return;
                    }
#endif
        }
    }
}

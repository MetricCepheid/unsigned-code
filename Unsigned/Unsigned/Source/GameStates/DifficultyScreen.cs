using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using UnsignedPeripheralPlugins;
using SongDataIO;
using FVProductions.Utility;

namespace Unsigned
{
    class DifficultyScreen : BaseState
    {
        private ContentManager Content;

        private FVShader effect;
        private FVShader cubeMapEffect;
        private SpriteBatch spriteBatch;

        private Texture2D whitishTex, whitishBM;
        private Texture2D[] texMetalTypes;
        private TextureCube texLight;
        private Texture2D glassboxTex, glassboxBM;
        private Texture2D[][] texStrings;
        private Dictionary<String, FVModel> instrumentModels;
        private float instrRot;

        SessionInfo nugget;

        public DifficultyScreen(SessionInfo nugget)
        {
            this.nugget = nugget;
            for (int i = 0; i < nugget.diffConfirm.Length; i++)
                nugget.diffConfirm[i] = false;
            instrRot = 0;
        }

        public override void Load()
        {
            Content = new ContentManager(Global.Services);
            Content.RootDirectory = "Content";

            spriteBatch = new SpriteBatch(Global.Graphics.GraphicsDevice);
            effect = new FVShader(Global.Graphics.GraphicsDevice, Content.Load<Effect>("Shaders\\UnsignedEngineShader"), "maintechnique");
            cubeMapEffect = new FVShader(Global.Graphics.GraphicsDevice, Content.Load<Effect>("Shaders\\CubeMappingShader"), "CubeMapTechnique");

            instrumentModels = new Dictionary<string, FVModel>();
            String dir = System.IO.Directory.GetCurrentDirectory();
            dir += "\\Content\\meshes\\DifficultyScreen\\instruments\\";
            String[] files = System.IO.Directory.GetFiles(dir,"*.xnb");
            for (int i = 0; i < files.Length; i++)
            {
                files[i] = files[i].Substring(files[i].IndexOf("meshes\\"), files[i].Length - files[i].IndexOf("meshes\\"));
                files[i] = files[i].Substring(0,files[i].LastIndexOf('.'));
                String codename = files[i].Substring(files[i].LastIndexOf('\\') + 1);
                instrumentModels.Add(codename,ModelLoader.LoadModel(files[i]));
            }
            texMetalTypes = new Texture2D[4];
            texMetalTypes[0] = Content.Load<Texture2D>("textures\\DifficultyScreen\\instr_bronze");
            texMetalTypes[1] = Content.Load<Texture2D>("textures\\DifficultyScreen\\instr_silver");
            texMetalTypes[2] = Content.Load<Texture2D>("textures\\DifficultyScreen\\instr_gold");
            texMetalTypes[3] = Content.Load<Texture2D>("textures\\DifficultyScreen\\instr_platinum");
            Texture2D light = Content.Load<Texture2D>("textures\\DifficultyScreen\\instr_lightingcube");
            {
                texLight = new TextureCube(Global.Graphics.GraphicsDevice, light.Width, 0, TextureUsage.AutoGenerateMipMap, SurfaceFormat.Color);
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
            glassboxTex = Content.Load<Texture2D>("textures\\DifficultyScreen\\glasscase");
            glassboxBM = Content.Load<Texture2D>("textures\\DifficultyScreen\\glasscasebm");
            whitishTex = Content.Load<Texture2D>("textures\\DifficultyScreen\\concr");
            whitishBM = Content.Load<Texture2D>("textures\\DifficultyScreen\\concrbm");
            texStrings = new Texture2D[4][];
            for (int i = 0; i < 4; i++)
            {
                texStrings[i] = new Texture2D[2];
                for (int k = 0; k < 2; k++)
                    texStrings[i][k] = Content.Load<Texture2D>("textures\\DifficultyScreen\\diff" + i + "" + k);
            }
        }

        public override void Unload()
        {
            Content.Unload();
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
                            if (nugget.diffConfirm[i])
                                green = true;
                            else
                                nugget.diffConfirm[i] = true;
                        }
                        if (nugget.peripherals[i].WasPressed(PeripheralButton.BACK))
                        {
                            if (nugget.diffConfirm[i])
                                nugget.diffConfirm[i] = false;
                            else
                                red = true;
                        }
                    }
                }
                for(int i=0;i<4;i++)
                {
                    if (nugget.peripherals[i] != null && !nugget.diffConfirm[i])
                    {
                        bool up = false, down = false;
                        if (nugget.peripherals[i].WasPressed(PeripheralButton.DOWN))
                            down = true;
                        if (nugget.peripherals[i].WasPressed(PeripheralButton.UP))
                            up = true;
                        if (up && nugget.difficulties[i] > Difficulty.Easy)
                            nugget.difficulties[i]--;
                        if (down && nugget.difficulties[i] < Difficulty.Expert)
                            nugget.difficulties[i]++;
                    }
                }

                bool allconfirmed = true;
                for (int i = 0; i < 4; i++)
                    if (nugget.peripherals[i] != null && !nugget.diffConfirm[i])
                        allconfirmed = false;
                if (green && allconfirmed)
                {
                    UnsignedGame.Singleton.SwitchState(new GameState(nugget));
                }
                if (red)
                {
                    for (int i = 0; i < nugget.diffConfirm.Length; i++)
                        nugget.diffConfirm[i] = false;
                    UnsignedGame.Singleton.SwitchState(new SongSelectScreen(nugget)); 
                }
#if !DEBUG
            }
            catch(Exception e)
            {
                Debug.Error("Problem in DifficultyScreen.Update", e);
                UnsignedGame.Singleton.Exit();
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
                Global.Graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

                effect.TextureEnabled = true;
                effect.LightingEnabled = Configuration.Lighting;
                effect.NormalMapEnabled = Configuration.NormalMapping;
                effect.SpecularEnabled = Configuration.Specular;

                cubeMapEffect.TextureEnabled = true;

                effect.NormalMapTexture =Global.TexDefaultBM;
                effect.AmbientMaterial = new Color(24, 24, 24);
                effect.DiffuseMaterial = Color.Gray;
                effect.SpecularMaterial = Color.White;
                effect.SpecularMapTexture = Global.TexWhite;

                effect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, Global.ScreenWidth / (float)Global.ScreenHeight, 1f, 1000f);
                cubeMapEffect.Projection = effect.Projection;

                effect.View = Matrix.CreateLookAt(new Vector3(0, 108, 22), new Vector3(0, 104, 0), new Vector3(0, 1, 0));
                cubeMapEffect.View = effect.View;

                effect.DirectionalLight = new DirectionalLight(true, new Vector3(-0.1f, -1f, 0.5f), Color.White, Color.White);

                effect.CommitChanges();
#if !DEBUG
            }
            catch(Exception e)
            {
                Debug.Error("Problem in DifficultyScreen.Draw[1]",e);
                UnsignedGame.Singleton.Exit();
                return;
            }

            try
            {
#endif
                effect.Begin();
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Begin();

                    Matrix matRot, matScale, matTranslate;
                    {//basebottom
                        matTranslate = Matrix.CreateTranslation(0, 100, 0);
                        matRot = Matrix.CreateRotationX(0);//MathHelper.Pi);
                        matScale = Matrix.CreateScale(40, 0, 32);

                        effect.World = matScale * matRot * matTranslate;
                        effect.DiffuseTexture = whitishTex;
                        effect.NormalMapTexture =whitishBM;
                        effect.Shininess = 12f;
                        effect.SpecularMaterial = Color.White;
                        effect.CommitChanges();

                        Global.Graphics.GraphicsDevice.DrawSquare();
                    }
                    {//basewall
                        matTranslate = Matrix.CreateTranslation(0, 132, -32);
                        matRot = Matrix.CreateRotationX((float)Math.PI / 2);
                        matScale = Matrix.CreateScale(40, 0, 32);

                        effect.World = matScale * matRot * matTranslate;
                        effect.DiffuseTexture = whitishTex;
                        effect.NormalMapTexture =whitishBM;
                        effect.Shininess = 0.25f;
                        effect.CommitChanges();

                        Global.Graphics.GraphicsDevice.DrawSquare();
                    }
                    

                    pass.End();
                }
                effect.End();
                cubeMapEffect.Begin();
                foreach (EffectPass pass in cubeMapEffect.CurrentTechnique.Passes)
                {
                    pass.Begin();

                    Matrix matRot, matScale, matTranslate;
                    for(int k=0;k<4;k++)
                        if (nugget.peripherals[k]!=null)
                        {
                            FVModel model = instrumentModels[InstrumentMaster.Singleton.GetInstrument(nugget.instruments[k]).CodeName];
                            matTranslate = Matrix.CreateTranslation(-10+(5*k), 105, -6);
                            matRot = Matrix.CreateRotationY(instrRot)*Matrix.CreateRotationZ(0.1f)*Matrix.CreateRotationY(-instrRot)*Matrix.CreateRotationX(MathHelper.PiOver4);
                            matScale = Matrix.CreateScale(1.5f);

                            cubeMapEffect.World = matScale * matRot * matTranslate;

                            cubeMapEffect.NormalMapTexture = Global.TexDefaultBM;

                            cubeMapEffect.DiffuseTexture = texMetalTypes[(int)nugget.difficulties[k]];
                            cubeMapEffect.CubeMapTexture = texLight;
                            cubeMapEffect.CommitChanges();

                            model.Draw();
                        }

                    pass.End();
                }
                cubeMapEffect.End();
                
                spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.None);
                for (int i = 0; i < 4; i++)
                    if(nugget.peripherals[i]!=null)
                    {
                        Texture2D tex = texStrings[(int)nugget.difficulties[i]][nugget.diffConfirm[i] ? 1 : 0];
                        spriteBatch.Draw(tex, new Rectangle((int)((Global.ScreenWidth * 0.125f) + (Global.ScreenWidth * 0.25f * i) - ((tex.Width * (Global.ScreenWidth / 800f)) / 2)), (int)(Global.ScreenHeight * 0.7f), (int)(tex.Width * (Global.ScreenWidth / 800f)), (int)(tex.Height * (Global.ScreenWidth / 800f))), Color.White);
                    }
                /*
                spriteBatch.Draw(GameUIMaster.Singleton.texButtonGreen, new Rectangle((int)(0.1f * Global.ScreenWidth), (int)(0.80f * Global.ScreenHeight), (int)(0.09f * Global.ScreenHeight), (int)(0.09f * Global.ScreenHeight)), Color.White);
                spriteBatch.DrawString(Global.DefaultFont, "Select", new Vector2((0.1f * Global.ScreenWidth) + (0.10f * Global.ScreenHeight), (0.80f * Global.ScreenHeight) + (0.09f * Global.ScreenHeight) - (Global.DefaultFont.MeasureString("Select").Y)), Color.White);
                spriteBatch.Draw(GameUIMaster.Singleton.texButtonRed, new Rectangle((int)(0.9f * Global.ScreenWidth) - (int)(0.09f * Global.ScreenHeight), (int)(0.80f * Global.ScreenHeight), (int)(0.09f * Global.ScreenHeight), (int)(0.09f * Global.ScreenHeight)), Color.White);
                spriteBatch.DrawString(Global.DefaultFont, "Back", new Vector2((0.9f * Global.ScreenWidth) - (0.10f * Global.ScreenHeight) - Global.DefaultFont.MeasureString("Back").X, (0.80f * Global.ScreenHeight) + (0.09f * Global.ScreenHeight) - (Global.DefaultFont.MeasureString("Back").Y)), Color.White);
                */
                spriteBatch.End();
#if !DEBUG
            }
            catch(Exception e)
            {
                Debug.Error("Problem in DifficultyScreen.Draw[1]",e);
                UnsignedGame.Singleton.Exit();
                return;
            }
#endif
        }
    }
}

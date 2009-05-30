using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UnsignedPeripheralPlugins;
using FVProductions.Utility;

namespace Unsigned
{
    class SongSelectScreen : BaseState
    {
        private ContentManager Content;

        private SpriteBatch spriteBatch;

        private FVShader effect;

        private Texture2D SongListTex, songchoosetop;
        private Texture2D SongListBG, SongHiLi, SongListBM;
        private RenderTarget2D SongListRT;
        private Texture2D concrTex, concrBM;

        private int SONGLIST_WAVEQUALITY = 100;
        private VertexTangentBinormal[] songlistGeom;
        private float SONGLIST_WAVE_SPEED = 5, slCurrentWave, slWaveLength = 50f, slWaveStrength = 2, SONGLIST_LENGTH = 170, SONGLIST_WIDTH = 1;

        private SetList setList;

        SessionInfo nugget;

        private int selectedSubSet, selectedSong;

        public SongSelectScreen(SessionInfo nugget)
        {
            this.nugget = nugget;
            setList = new SetList();
            setList.LoadSongFileHeaders();
            selectedSubSet = 0;
            selectedSong = 0;
            if (nugget.songFileName != "")
            {
                List<SubSet> subsets = setList.GetSubsets();
                for (int i = 0; i < subsets.Count; i++)
                {
                    List<SongFileHeader> songs = subsets[i].GetSongList();
                    for (int k = 0; k < songs.Count; k++)
                    {
                        if (songs[k].Filename == nugget.songFileName)
                        {
                            selectedSong = k;
                            selectedSubSet = i;
                        }
                    }
                }
            }
        }

        public override void Load()
        {
            Content = new ContentManager(Global.Services);
            Content.RootDirectory = "Content";

            spriteBatch = new SpriteBatch(Global.Graphics.GraphicsDevice);

            SongListRT = new RenderTarget2D(Global.Graphics.GraphicsDevice, Global.ScreenHeight < 512 ? 256 : 512, Global.ScreenHeight < 512 ? 256 : 512, 1, SurfaceFormat.Color);
            SongListBG = Content.Load<Texture2D>("textures\\SongSelect\\songlist");
            SongListBM = Content.Load<Texture2D>("textures\\SongSelect\\songlistbm");
            songchoosetop = Content.Load<Texture2D>("textures\\SongSelect\\songscreentop");
            concrTex = Content.Load<Texture2D>("textures\\SongSelect\\concr");
            concrBM = Content.Load<Texture2D>("textures\\SongSelect\\concrBM");
            SongHiLi = Content.Load<Texture2D>("textures\\SongSelect\\songhili");

            effect = new FVShader(Global.Graphics.GraphicsDevice, Content.Load<Effect>("shaders\\UnsignedEngineShader"), "maintechnique");
        }

        public override void Unload()
        {
            Content.Unload();
        }

        public override void Update(GameTime gameTime)
        {
            bool chgd;
#if !DEBUG
            try
            {
#endif
                chgd = SongListTex==null;


                Peripheral[] peripherals = PeripheralManager.Singleton.GetPeripherals();

                int collective = 0;
                bool green = false, red = false, yellow = false;

                for (int i = 0; i < 4; i++)
                    if (nugget.peripherals[i]!=null && nugget.peripherals[i].IsConnected())
                    {
                        if (nugget.peripherals[i].WasPressed(PeripheralButton.DOWN))
                            collective--;
                        if (nugget.peripherals[i].WasPressed(PeripheralButton.UP))
                            collective++;
                        if (nugget.peripherals[i].WasPressed(PeripheralButton.BACK))
                            red = true;
                        if (nugget.peripherals[i].WasPressed(PeripheralButton.SWITCH))
                            yellow = true;
                    }
                for(int i=0;i<4;i++)
                    if (nugget.peripherals[i] != null)
                    {
                        if (nugget.peripherals[i].WasPressed(PeripheralButton.CONFIRM))
                            green = true;
                        break;
                    }

                if (yellow)
                {
                    setList.NextSortOrder();
                    selectedSubSet = 0;
                    selectedSong = 0;
                    chgd = true;
                }
                if (red)
                { UnsignedGame.Singleton.SwitchState(new ControllerSetupScreen(nugget)); }
                if (green)
                {
                    if(setList.GetSubsets().Count>0 && selectedSubSet<setList.GetSubsets().Count)
                        if (setList.GetSubsets()[selectedSubSet].GetSongList().Count > 0 && selectedSong < setList.GetSubsets()[selectedSubSet].GetSongList().Count)
                        {
                            nugget.songFileName = setList.GetSubsets()[selectedSubSet].GetSongList()[selectedSong].Filename;
                            UnsignedGame.Singleton.SwitchState(new DifficultyScreen(nugget));
                        }
                }
                if(collective != 0)
                {
                    selectedSong -= collective;
                    List<SubSet> subsets = setList.GetSubsets();
                    while (selectedSong >= subsets[selectedSubSet].GetSongList().Count)
                    {
                        if (selectedSubSet >= subsets.Count - 1)
                            selectedSong = subsets[selectedSubSet].GetSongList().Count - 1;
                        else
                        {
                            selectedSong -= subsets[selectedSubSet].GetSongList().Count;
                            selectedSubSet++;
                        }
                    }
                    while (selectedSong < 0)
                    {
                        if (selectedSubSet <= 0)
                            selectedSong = 0;
                        else
                        {
                            selectedSubSet--;
                            selectedSong += subsets[selectedSubSet].GetSongList().Count;
                        }
                    }
                    chgd = true;
                }
#if !DEBUG
            }
            catch(Exception e)
            {
                Debug.Error("Problem in SongSelectScreen.Update[1]",e);
                UnsignedGame.Singleton.Exit();
                return;
            }
            try
            {
#endif
                if (chgd)
                {
                    float scale = Global.ScreenHeight < 512 ? 0.5f : 1.0f;

                    Global.Graphics.GraphicsDevice.SetRenderTarget(0, SongListRT);
                    spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.None);
                    Global.Graphics.GraphicsDevice.Clear(new Color(0, 0, 0, 0));
                    spriteBatch.Draw(SongListBG, new Rectangle(0, 0, SongListRT.Width, SongListRT.Height), Color.White);
                    List<SubSet> subsets = setList.GetSubsets();

                    if (subsets.Count > 0 && subsets[selectedSubSet].GetSongList().Count > 0)
                    {
                        List<String> drawListName = new List<String>();
                        List<String> drawListArtist = new List<String>();
                        List<String> drawListLength = new List<String>();
                        int index = 0;

                        for (int i = 0; i < subsets.Count; i++)
                        {
                            {
                                drawListName.Add("@@@" + subsets[i].name);
                                drawListArtist.Add("");
                                drawListLength.Add("");
                            }
                            for (int j = 0; j < subsets[i].GetSongList().Count; j++)
                            {
                                drawListName.Add(subsets[i].GetSongList()[j].SongName);
                                drawListArtist.Add(subsets[i].GetSongList()[j].ArtistName);
                                drawListLength.Add(subsets[i].GetSongList()[j].Length.ToString());
                                if (i == selectedSubSet && j == selectedSong)
                                    index = drawListName.Count - 1;
                            }
                        }

                        int low = index - 4;
                        int high = index + 4;
                        if (low < 0)
                        {
                            high += -low;
                            low = 0;
                        }
                        if (high >= drawListName.Count)
                        {
                            low = Math.Max(0, low - (high - drawListName.Count));
                            high = drawListName.Count - 1;
                        }
                        spriteBatch.Draw(SongHiLi, new Rectangle((int)(20 * scale), (int)(((index - low) * 40 + 93) * scale), (SongListRT.Width - (int)(40 * scale)), (int)(60 * scale)), Color.White);
                        for (int i = low; i <= high; i++)
                        {
                            spriteBatch.DrawString(Global.DefaultFont, drawListName[i].StartsWith("@@@") ? drawListName[i].Substring(3) : drawListName[i], new Vector2((10 + (drawListName[i].StartsWith("@@@") ? 20 : 50)) * scale, ((i - low) * 40 + 100) * scale), drawListName[i].StartsWith("@@@") ? new Color(new Vector3(.75f, .375f, 0)) : Color.Black, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
                            if (!drawListName[i].StartsWith("@@@"))
                                spriteBatch.DrawString(Global.DefaultFont, drawListArtist[i], new Vector2(70 * scale, ((i - low) * 40 + 125) * scale), Color.Black, 0, Vector2.Zero, scale * 0.75f, SpriteEffects.None, 0);
                        }
                    }
                    else
                    {
                        spriteBatch.DrawString(Global.DefaultFont, "No Songs in current view", new Vector2((10 +  50) * scale, (40 + 100) * scale), Color.Black, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
                    }
                    spriteBatch.End();
                    Global.Graphics.GraphicsDevice.SetRenderTarget(0, null);
                    SongListTex = SongListRT.GetTexture();
                }
#if !DEBUG
            }
            catch(Exception e)
            {
                Debug.Error("Problem in SongSelectScreen.Update[2]",e);
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
                Global.Graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
                Global.Graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
                Global.Graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;
                Global.Graphics.ApplyChanges();

                Global.Graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

                effect.NormalMapTexture = Global.TexDefaultBM;
                effect.AmbientMaterial =  new Color(24, 24, 24);
                effect.DiffuseMaterial = new Color(255, 255, 255);
                effect.SpecularMaterial = new Color(255, 255, 255);
                effect.SpecularMapTexture = Global.TexWhite;

                effect.TextureEnabled = true;
                effect.LightingEnabled = Configuration.Lighting;
                effect.SpecularEnabled = Configuration.Specular;
                effect.NormalMapEnabled = Configuration.NormalMapping;

                effect.DirectionalLight = new DirectionalLight(true, new Vector3(0, 2, 1), new Color(200, 200, 200), new Color(0, 0, 0));

                effect.CommitChanges();

                effect.Begin();
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Begin();

                    effect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, Global.ScreenWidth / (float)Global.ScreenHeight, 1f, 1000f);
                    effect.View = Matrix.CreateLookAt(new Vector3(0, 128, 128), new Vector3(0, 128, 0), new Vector3(0, 1, 0));

                    Matrix matRot, matScale, matTranslate;

                    {
                        matTranslate = Matrix.CreateTranslation(-32, 120, -128);
                        matRot = Matrix.CreateRotationX((float)Math.PI / 2);
                        matScale = Matrix.CreateScale(200, 1, 128);

                        effect.World = matScale * matRot * matTranslate;
                        effect.DiffuseTexture = concrTex;
                        effect.NormalMapTexture = concrBM;
                        effect.SpecularMaterial = new Color(0.01f, 0.01f, 0.01f);
                        effect.Shininess = 1f;
                        effect.CommitChanges();

                        Global.Graphics.GraphicsDevice.DrawSquare();
                    }

                    if (SongListTex != null)
                    {//list
                        slCurrentWave += (gameTime.ElapsedGameTime.Milliseconds / 1000f) * SONGLIST_WAVE_SPEED;
                        //if (slCurrentWave > Math.PI*2)
                        //    slCurrentWave-=(float)(Math.PI*2);
                        if (songlistGeom == null || songlistGeom.Length < (SONGLIST_WAVEQUALITY - 1) * 6)
                            songlistGeom = new VertexTangentBinormal[(SONGLIST_WAVEQUALITY - 1) * 6];
                        float waveLast = 0, wave = 0;
                        Vector3 normal = new Vector3(0, 0, 1), lastNormal = new Vector3(0, 0, 1);
                        Vector3 tangent = new Vector3(0, 1, 0), lastTangent = new Vector3(0, 1, 0);
                        Vector3 binormal = new Vector3(1, 0, 0), lastBinormal = new Vector3(1, 0, 0);
                        for (int i = 0; i < SONGLIST_WAVEQUALITY - 1; i++)
                        {
                            waveLast = wave;
                            wave = (float)Math.Sin((-slCurrentWave + (i / (float)SONGLIST_WAVEQUALITY * slWaveLength)) / (Math.PI * 2));
                            if (i <= SONGLIST_WAVEQUALITY / 10)
                            {
                                wave *= (float)Math.Pow(i / (float)(SONGLIST_WAVEQUALITY / 10), 0.5f);
                            }
                            lastNormal = normal;
                            lastBinormal = binormal;
                            lastTangent = tangent;
                            float hi = 192 - (i / (float)SONGLIST_WAVEQUALITY * SONGLIST_LENGTH), lo = 192 - ((i + 1) / (float)SONGLIST_WAVEQUALITY * SONGLIST_LENGTH);
                            float inh = -100 + (waveLast * slWaveStrength), inl = -100 + (wave * slWaveStrength);
                            normal = new Vector3(0, (inl - inh) * 1.5f, hi - lo);
                            normal.Normalize();
                            tangent = Vector3.Transform(normal, Matrix.CreateRotationX(-MathHelper.PiOver2));
                            binormal = Vector3.Cross(normal, tangent);
                            songlistGeom[(i * 6)] = new VertexTangentBinormal(new Vector3(-64, hi, inh), new Vector2(0, i / (float)SONGLIST_WAVEQUALITY), lastNormal, lastBinormal, lastTangent);
                            songlistGeom[(i * 6) + 1] = new VertexTangentBinormal(new Vector3(-64, lo, inl), new Vector2(0, (i + 1) / (float)SONGLIST_WAVEQUALITY), normal, binormal, tangent);
                            songlistGeom[(i * 6) + 2] = new VertexTangentBinormal(new Vector3(64, lo, inl), new Vector2(1, (i + 1) / (float)SONGLIST_WAVEQUALITY), normal, binormal, tangent);
                            songlistGeom[(i * 6) + 3] = new VertexTangentBinormal(new Vector3(-64, hi, inh), new Vector2(0, i / (float)SONGLIST_WAVEQUALITY), lastNormal, lastBinormal, lastTangent);
                            songlistGeom[(i * 6) + 4] = new VertexTangentBinormal(new Vector3(64, lo, inl), new Vector2(1, (i + 1) / (float)SONGLIST_WAVEQUALITY), normal, binormal, tangent);
                            songlistGeom[(i * 6) + 5] = new VertexTangentBinormal(new Vector3(64, hi, inh), new Vector2(1, i / (float)SONGLIST_WAVEQUALITY), lastNormal, lastBinormal, lastTangent);
                        }

                        matRot = Matrix.CreateRotationY((float)Math.PI / -12);
                        matTranslate = Matrix.CreateTranslation(20, 20, 0);
                        matScale = Matrix.CreateScale(new Vector3(SONGLIST_WIDTH, 1, 1));
                        effect.World = matScale * matRot * matTranslate;
                        effect.SpecularMaterial = new Color(0.1f, 0.1f, 0.1f);
                        effect.DiffuseTexture = SongListTex;
                        effect.NormalMapTexture = SongListBM;
                        effect.Shininess = 1f;
                        effect.CommitChanges();

                        Global.Graphics.GraphicsDevice.VertexDeclaration = VertexTangentBinormal.VertexDeclaration;
                        Global.Graphics.GraphicsDevice.DrawUserPrimitives<VertexTangentBinormal>(PrimitiveType.TriangleList, songlistGeom, 0, (SONGLIST_WAVEQUALITY - 1) * 2);
                    }

                    pass.End();
                }
                effect.End();
                spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.None);
                spriteBatch.Draw(songchoosetop, new Rectangle(0, 0, Global.ScreenWidth, (int)((Global.ScreenHeight / 768f) * 256)), Color.White);
                /*spriteBatch.Draw(GameUIMaster.Singleton.texButtonGreen, new Rectangle((int)(0.1f * Global.ScreenWidth), (int)(0.80f * Global.ScreenHeight), (int)(0.09f * Global.ScreenHeight), (int)(0.09f * Global.ScreenHeight)), Color.White);
                spriteBatch.DrawString(Global.DefaultFont, Localizer.Get("Select"), new Vector2((0.1f * Global.ScreenWidth) + (0.10f * Global.ScreenHeight), (0.80f * Global.ScreenHeight) + (0.09f * Global.ScreenHeight) - (Global.DefaultFont.MeasureString(Localizer.Get("Select")).Y)), Color.White);
                spriteBatch.Draw(GameUIMaster.Singleton.texButtonRed, new Rectangle((int)(0.9f * Global.ScreenWidth) - (int)(0.09f * Global.ScreenHeight), (int)(0.80f * Global.ScreenHeight), (int)(0.09f * Global.ScreenHeight), (int)(0.09f * Global.ScreenHeight)), Color.White);
                spriteBatch.DrawString(Global.DefaultFont, Localizer.Get("Back"), new Vector2((0.9f * Global.ScreenWidth) - (0.10f * Global.ScreenHeight) - Global.DefaultFont.MeasureString(Localizer.Get("Back")).X, (0.80f * Global.ScreenHeight) + (0.09f * Global.ScreenHeight) - (Global.DefaultFont.MeasureString(Localizer.Get("Back")).Y)), Color.White);*/
                spriteBatch.End();
#if !DEBUG
            }
            catch(Exception e)
            {
                Debug.Error("Problem in SongSelectSceen.Draw[1]", e);
                UnsignedGame.Singleton.Exit();
                return;
            }
#endif
        }
    }
}

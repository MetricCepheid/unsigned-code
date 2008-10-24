using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UnsignedPeripheralPlugins;

namespace Unsigned
{
    class SongSelectScreen : BaseState
    {
        private ContentManager content;
        private Texture2D SongListTex, songchoosetop;
        private Texture2D SongListBG, SongHiLi, SongListBM;
        private RenderTarget2D SongListRT;
        private Texture2D concrTex, concrBM;

        private int SONGLIST_WAVEQUALITY = 100;
        private GBVertexFormat[] songlistGeom;
        private VertexBuffer songlistVB;
        private float SONGLIST_WAVE_SPEED = 5, slCurrentWave, slWaveLength = 50f, slWaveStrength = 2, SONGLIST_LENGTH = 170, SONGLIST_WIDTH = 1;

        private SetList setList;

        PlayerConfigNugget nugget;

        private int selectedSubSet, selectedSong;

        public SongSelectScreen(PlayerConfigNugget nugget)
        {
            this.nugget = nugget;
        }

        public override void Load()
        {
            content = new ContentManager(UnsignedGame.GetSingleton().Services);
            SongListRT = new RenderTarget2D(RenderMaster.GetSingleton().graphics.GraphicsDevice, GameSettings.windowheight < 512 ? 256 : 512, GameSettings.windowheight < 512 ? 256 : 512, 1, SurfaceFormat.Color);
            SongListBG = content.Load<Texture2D>("graphics\\songlist");
            SongListBM = content.Load<Texture2D>("graphics\\songlistbm");
            songchoosetop = content.Load<Texture2D>("graphics\\songscreentop");
            concrTex = content.Load<Texture2D>("graphics\\concr");
            concrBM = content.Load<Texture2D>("graphics\\concrBM");
            SongHiLi = content.Load<Texture2D>("graphics\\songhili");
            setList = new SetList();
            setList.LoadSongFileHeaders();
        }

        public override void Unload()
        {
            content.Unload();
        }

        public override void Update(GameTime gameTime)
        {
            bool chgd;
#if !DEBUG
                try
                {
#endif
            chgd = SongListTex==null;


            Peripheral[] peripherals = PeripheralManager.GetSingleton().GetPeripherals();

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
            { UnsignedGame.GetSingleton().PopState(); }
            if (green)
            {
                nugget.songFileName = setList.GetSubsets()[selectedSubSet].GetSongList()[selectedSong].Filename;
                UnsignedGame.GetSingleton().PushState(new DifficultyScreen(nugget));
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
#if WINDOWS
                System.Windows.Forms.MessageBox.Show("Problem in Update/SS/Pt0\n"+e.Message+"\n"+e.StackTrace);
#endif
                UnsignedGame.GetSingleton().Exit();
                return;
            }
            try
            {
#endif
                if (chgd)
                {
                    RenderMaster rm = RenderMaster.GetSingleton();
                        

                    float scale = GameSettings.windowheight < 512 ? 0.5f : 1.0f;

                    rm.graphics.GraphicsDevice.SetRenderTarget(0, SongListRT);
                    rm.spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
                    rm.graphics.GraphicsDevice.RenderState.AlphaDestinationBlend = Blend.InverseSourceAlpha;
                    rm.graphics.GraphicsDevice.Clear(new Color(0, 0, 0, 0));
                    rm.spritebatch.Draw(SongListBG, new Rectangle(0, 0, SongListRT.Width, SongListRT.Height), Color.White);
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
                        rm.spritebatch.Draw(SongHiLi, new Rectangle((int)(20*scale), (int)(((index-low) * 40 + 93)*scale), (SongListRT.Width - (int)(40*scale)), (int)(60*scale)), Color.White);
                        for (int i = low; i <= high; i++)
                        {
                            rm.spritebatch.DrawString(Global.DefaultFont, drawListName[i].StartsWith("@@@") ? drawListName[i].Substring(3) : drawListName[i], new Vector2((10 + (drawListName[i].StartsWith("@@@") ? 20 : 50))*scale, ((i - low) * 40 + 100)*scale), drawListName[i].StartsWith("@@@") ? new Color(new Vector3(.75f, .375f, 0)) : Color.Black,0,Vector2.Zero,scale,SpriteEffects.None,0);
                            if (!drawListName[i].StartsWith("@@@"))
                                rm.spritebatch.DrawString(Global.SmallFont, drawListArtist[i], new Vector2(70*scale, ((i - low) * 40 + 125)*scale), Color.Black, 0, Vector2.Zero, scale,SpriteEffects.None,0);
                        }
                    }
                    rm.spritebatch.End();
                    rm.graphics.GraphicsDevice.SetRenderTarget(0, null);
                    SongListTex = SongListRT.GetTexture();
                }
#if !DEBUG
                }
                catch(Exception e)
                {
#if WINDOWS
                    System.Windows.Forms.MessageBox.Show("Problem in Update/SS/Pt1\n"+e.Message+"\n"+e.StackTrace);
#endif
                    UnsignedGame.GetSingleton().Exit();
                    return;
                }
#endif
                
            
        }

        public override void Render(GameTime gameTime)
        {
            RenderMaster rm = RenderMaster.GetSingleton();
            FVShader effect = rm.engine;
#if !DEBUG
                    try
                    {
#endif
            rm.graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
            rm.graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
            rm.graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;
            //rm.graphics.PreferMultiSampling = true;
            rm.graphics.ApplyChanges();

            rm.graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
            //rm.graphics.GraphicsDevice.

            effect.NormalMapTexture = Global.texDefaultBM;
            effect.AmbientMaterial =  new Color(100, 100, 100);
            effect.DiffuseMaterial = new Color(255, 255, 255);
            effect.SpecularMaterial = new Color(255, 255, 255);

            effect.LightingEnabled = GameSettings.Lighting;
            effect.SpecularEnabled = GameSettings.Specular;
            effect.NormalMapEnabled = GameSettings.NormalMapping;

            effect.DirectionalLight = new DirectionalLight(true, new Vector3(0, 3, 1), new Color(200, 200, 200), new Color(0, 0, 0));

            Version SM = rm.graphics.GraphicsDevice.GraphicsDeviceCapabilities.PixelShaderVersion;
            if (SM.Major < 1 || (SM.Major==1 && SM.Minor<1))
            {
#if WINDOWS
                System.Windows.Forms.MessageBox.Show("Whoops! Your graphics card only supports Shader Model " + SM.Major + "." + SM.Minor + "\nYou need at least 1.1 to run Unsigned");
#endif
                UnsignedGame.GetSingleton().Exit();
            }
            effect.CommitChanges();

            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();

                Matrix matView = Matrix.CreateLookAt(new Vector3(0, 128, 128), new Vector3(0, 128, 0), new Vector3(0, 1, 0)); 
                UnsignedGame.SetProjMatrix(GameSettings.windowwidth, GameSettings.windowheight);
                //render the background rm.graphics
                rm.View = matView;

                Matrix matRot, matScale, matTranslate;

                {
                    matTranslate = Matrix.CreateTranslation(-32, 120, -128);
                    matRot = Matrix.CreateRotationX((float)Math.PI / 2);
                    matScale = Matrix.CreateScale(300, 1, 192);

                    effect.World = matScale * matRot * matTranslate;
                    effect.DiffuseTexture = concrTex;
                    effect.NormalMapTexture = concrBM;
                    effect.Shininess = 0.25f;
                    effect.CommitChanges();

                    rm.graphics.GraphicsDevice.VertexDeclaration = GBVertexFormat.VertexDeclaration;
                    rm.graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                    rm.graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                    rm.graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                    rm.graphics.GraphicsDevice.Vertices[0].SetSource(Global.square, 0, GBVertexFormat.SizeInBytes);
                    rm.graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
                    rm.graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                }

                if (SongListTex != null)
                {//list
                    slCurrentWave += (gameTime.ElapsedGameTime.Milliseconds / 1000f) * SONGLIST_WAVE_SPEED;
                    //if (slCurrentWave > Math.PI*2)
                    //    slCurrentWave-=(float)(Math.PI*2);
                    if (songlistGeom == null || songlistGeom.Length < (SONGLIST_WAVEQUALITY - 1) * 6)
                        songlistGeom = new GBVertexFormat[(SONGLIST_WAVEQUALITY - 1) * 6];
                    float waveLast = 0, wave = 0;
                    Vector3 normal = new Vector3(0, 0, 1), lastNormal = new Vector3(0, 0, 1);
                    for (int i = 0; i < SONGLIST_WAVEQUALITY - 1; i++)
                    {
                        waveLast = wave;
                        wave = (float)Math.Sin((-slCurrentWave + (i / (float)SONGLIST_WAVEQUALITY * slWaveLength)) / (Math.PI * 2));
                        if (i <= SONGLIST_WAVEQUALITY / 10)
                        {
                            wave *= (float)Math.Pow(i / (float)(SONGLIST_WAVEQUALITY / 10), 0.5f);
                        }
                        lastNormal = normal;
                        float hi = 192 - (i / (float)SONGLIST_WAVEQUALITY * SONGLIST_LENGTH), lo = 192 - ((i + 1) / (float)SONGLIST_WAVEQUALITY * SONGLIST_LENGTH);
                        float inh = -100 + (waveLast * slWaveStrength), inl = -100 + (wave * slWaveStrength);
                        normal = new Vector3(0, (inl - inh) * 1.5f, hi - lo);
                        normal.Normalize();
                        songlistGeom[(i * 6)] = new GBVertexFormat(new Vector3(-64, hi, inh), lastNormal, new Vector2(0, i / (float)SONGLIST_WAVEQUALITY));
                        songlistGeom[(i * 6) + 1] = new GBVertexFormat(new Vector3(-64, lo, inl), normal, new Vector2(0, (i + 1) / (float)SONGLIST_WAVEQUALITY));
                        songlistGeom[(i * 6) + 2] = new GBVertexFormat(new Vector3(64, lo, inl), normal, new Vector2(1, (i + 1) / (float)SONGLIST_WAVEQUALITY));
                        songlistGeom[(i * 6) + 3] = new GBVertexFormat(new Vector3(-64, hi, inh), lastNormal, new Vector2(0, i / (float)SONGLIST_WAVEQUALITY));
                        songlistGeom[(i * 6) + 4] = new GBVertexFormat(new Vector3(64, lo, inl), normal, new Vector2(1, (i + 1) / (float)SONGLIST_WAVEQUALITY));
                        songlistGeom[(i * 6) + 5] = new GBVertexFormat(new Vector3(64, hi, inh), lastNormal, new Vector2(1, i / (float)SONGLIST_WAVEQUALITY));
                    }

                    songlistVB = new VertexBuffer(rm.graphics.GraphicsDevice, GBVertexFormat.SizeInBytes * (SONGLIST_WAVEQUALITY - 1) * 6, BufferUsage.WriteOnly);

                    songlistVB.SetData<GBVertexFormat>(songlistGeom);

                    matRot = Matrix.CreateRotationY((float)Math.PI / -12);
                    matTranslate = Matrix.CreateTranslation(-10, 20, 0);
                    matScale = Matrix.CreateScale(new Vector3(SONGLIST_WIDTH, 1, 1));
                    effect.World = matScale * matRot * matTranslate;
                    effect.DiffuseTexture = SongListTex;
                    effect.NormalMapTexture = SongListBM;
                    effect.Shininess = 0.25f;
                    effect.CommitChanges();

                    rm.graphics.GraphicsDevice.VertexDeclaration = GBVertexFormat.VertexDeclaration;
                    rm.graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                    rm.graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                    rm.graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                    rm.graphics.GraphicsDevice.Vertices[0].SetSource(songlistVB, 0, GBVertexFormat.SizeInBytes);
                    rm.graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, (SONGLIST_WAVEQUALITY - 1) * 2);
                    rm.graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                }

                effect.Alpha = 1.0f;

                pass.End();
            }
            effect.End();
            rm.spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
            rm.spritebatch.Draw(songchoosetop, new Rectangle(0, 0, GameSettings.windowwidth, (int)((GameSettings.windowheight / 768f) * 256)), Color.White);
            rm.spritebatch.Draw(GameUIMaster.GetSingleton().texButtonGreen, new Rectangle((int)(0.1f * GameSettings.windowwidth), (int)(0.80f * GameSettings.windowheight), (int)(0.09f * GameSettings.windowheight), (int)(0.09f * GameSettings.windowheight)), Color.White);
            rm.spritebatch.DrawString(Global.DefaultFont, Localizer.Get("Select"), new Vector2((0.1f * GameSettings.windowwidth) + (0.10f * GameSettings.windowheight), (0.80f * GameSettings.windowheight) + (0.09f * GameSettings.windowheight) - (Global.DefaultFont.MeasureString(Localizer.Get("Select")).Y)), Color.White);
            rm.spritebatch.Draw(GameUIMaster.GetSingleton().texButtonRed, new Rectangle((int)(0.9f * GameSettings.windowwidth) - (int)(0.09f * GameSettings.windowheight), (int)(0.80f * GameSettings.windowheight), (int)(0.09f * GameSettings.windowheight), (int)(0.09f * GameSettings.windowheight)), Color.White);
            rm.spritebatch.DrawString(Global.DefaultFont, Localizer.Get("Back"), new Vector2((0.9f * GameSettings.windowwidth) - (0.10f * GameSettings.windowheight) - Global.DefaultFont.MeasureString(Localizer.Get("Back")).X, (0.80f * GameSettings.windowheight) + (0.09f * GameSettings.windowheight) - (Global.DefaultFont.MeasureString(Localizer.Get("Back")).Y)), Color.White);
            if (Global.DemoMode)
            {
                rm.spritebatch.DrawString(Global.BigFont, Localizer.Get("Demo Mode"), new Vector2((GameSettings.windowwidth / 2) - (Global.BigFont.MeasureString(Localizer.Get("Demo Mode")).X / 2), GameSettings.windowheight * 0.15f), new Color(255, 0, 0, 64));
                rm.spritebatch.DrawString(Global.BigFont, Localizer.Get("Demo Mode"), new Vector2((GameSettings.windowwidth / 2) - (Global.BigFont.MeasureString(Localizer.Get("Demo Mode")).X / 2), GameSettings.windowheight * 0.4f), new Color(255, 0, 0, 64));
                rm.spritebatch.DrawString(Global.BigFont, Localizer.Get("Demo Mode"), new Vector2((GameSettings.windowwidth / 2) - (Global.BigFont.MeasureString(Localizer.Get("Demo Mode")).X / 2), GameSettings.windowheight * 0.65f), new Color(255, 0, 0, 64));
            }
            rm.spritebatch.End();
#if !DEBUG
                    }
                    catch(Exception e)
                    {
#if WINDOWS
                        System.Windows.Forms.MessageBox.Show("Problem in Draw/SS/Pt1\n"+e.Message+"\n"+e.StackTrace);
#endif
                        UnsignedGame.GetSingleton().Exit();
                        return;
                    }
#endif
            
        }
    }
}

#region Using Statements
using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
#if !XBOX
//using Microsoft.DirectX.
#endif
#endregion



namespace Unsigned
{
    #region publicstructs

    public struct WaveNode
    {
        public float X, Y;
        public byte Z;
        public bool White;
    }
#endregion

    public class UnsignedGame : Microsoft.Xna.Framework.Game
    {
        public static byte SONGDATA_VERSION = 18;

        public static UnsignedGame SINGLETON;

        private Stack<BaseState> currentState;

        public static bool TEST_SONG = false;


        GraphicsDeviceManager graphics;
        ContentManager content;
        public static Random r;
        private bool demomodepress = false;

        private Texture2D gradientMask;

        SpriteFont sfMenu;
        int menuSnakeRotOffset;
        bool[][] diffExists;




        public static UnsignedGame GetSingleton() { return SINGLETON; }

        public UnsignedGame()
        {
            graphics = new GraphicsDeviceManager(this);
            content = new ContentManager(Services);

            SINGLETON = this;

            r = new Random((int)DateTime.Now.Ticks);

            content.RootDirectory = "";
        }

        protected override void Initialize()
        {
            Configurate();

            RenderMaster.CreateSingleton();
            RenderMaster.GetSingleton().graphics = graphics;
            RenderMaster.GetSingleton().spritebatch = new SpriteBatch(graphics.GraphicsDevice);
            RenderMaster.GetSingleton().Load(content);

            GameUIMaster.CreateSingleton();
            GameUIMaster.GetSingleton().Load(content);

            KeyboardPeripheral.LoadMapping("keymapping.xml");

            PeripheralManager.CreateSingleton();
            PeripheralManager.GetSingleton().CheckConnections();

            CharacterMaster.CreateSingleton();

            InstrumentMaster.CreateSingleton();

            InitXNAApp();

            currentState = new Stack<BaseState>();
            PushState(new FVLogoScreen());

            Global.random = new Random();
            
            GBVertexFormat[] arr = { new GBVertexFormat(new Vector3(-1f,0f, 1f),new Vector3(0f,1f,0f),new Vector2(0f,0f)),
                                     new GBVertexFormat(new Vector3(-1f,0f,-1f),new Vector3(0f,1f,0f),new Vector2(0f,1f)),
                                     new GBVertexFormat(new Vector3( 1f,0f, 1f),new Vector3(0f,1f,0f),new Vector2(1f,0f)),
                                     new GBVertexFormat(new Vector3( 1f,0f, 1f),new Vector3(0f,1f,0f),new Vector2(1f,0f)),
                                     new GBVertexFormat(new Vector3(-1f,0f,-1f),new Vector3(0f,1f,0f),new Vector2(0f,1f)),
                                     new GBVertexFormat(new Vector3( 1f,0f,-1f),new Vector3(0f,1f,0f),new Vector2(1f,1f))};
            Global.square = new VertexBuffer(graphics.GraphicsDevice, GBVertexFormat.SizeInBytes * 6, BufferUsage.WriteOnly);
            Global.square.SetData<GBVertexFormat>(arr);

            //TEST CODE, takes you right into the action!
            /*songname = "War";
            contInput = new byte[4];
            rockerNames = new String[4];
            for (int k = 0; k < 4; k++)
            { contInput[k] = 255; instruments[k] = false; rockerNames[k] = null; }
            instruments[0] = true;
            instruments[1] = true;
            instruments[2] = true;
            instruments[3] = true;
            contInput[0] = 4;
            rockerNames[0] = "default";
            contInput[2] = 4;
            rockerNames[2] = "default";
            contInput[3] = 0;
            rockerNames[3] = "default";
            contInput[1] = 0;
            rockerNames[1] = "default";
            screen = S_INGAME;
            for (int i = 0; i < 4; i++)
                diff[i] = D_EXPERT;
            rtNote = new RenderTarget2D[4];
            SongListRT = new RenderTarget2D(graphics.GraphicsDevice, 1, 1, 1, SurfaceFormat.Color);
            LoadMenuContent();
            LoadGameContent();
            //InitForSong(instruments[0], instruments[1], instruments[2], instruments[3], diff, venueName);*/
            //END TEST CODE

            base.Initialize();
        }

        private void InitXNAApp()
        {
            Window.Title = "Unsigned";

            graphics.PreferredBackBufferWidth = GameSettings.windowwidth;
            graphics.PreferredBackBufferHeight = GameSettings.windowheight;
            graphics.ApplyChanges();
            if (GameSettings.fullScreen)
                graphics.ToggleFullScreen();

            

            //SetProjMatrix(Window.ClientBounds.Width,Window.ClientBounds.Height);
            graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;
            graphics.SynchronizeWithVerticalRetrace = true;
        }

        private void Configurate()
        {
            System.IO.StreamReader fin;
#if !DEBUG
            try
            {
#endif
                fin = new System.IO.StreamReader("config.cfg");
                
#if !DEBUG
            }
            catch (Exception)
            {
#if WINDOWS
                System.Windows.Forms.MessageBox.Show("config.cfg could not be opened");
#endif
                Exit();
                return;
            }
            try
            {
#endif
                do
                {
                    String str = fin.ReadLine();
                    if (str.Length > 10 && str.Substring(0, 10).ToLower().Equals("3dbkground"))
                    {
                        int val = Int32.Parse(str.Substring(str.IndexOf('=') + 1).Trim());
                        if (val == 0)
                            GameSettings.renderLevel = 0;
                        else
                            GameSettings.renderLevel = 10;
                    }
                    else if (str.Length > 10 && str.Substring(0, 10).ToLower().Equals("wavedetail"))
                    {
                        GameSettings.waveDetail = Int32.Parse(str.Substring(str.IndexOf('=') + 1).Trim());
                    }
                    else if (str.Length > 10 && str.Substring(0, 10).ToLower().Equals("resolution"))
                    {
                        GameSettings.windowwidth = Int32.Parse(str.Substring(str.IndexOf('=') + 1, Math.Max(str.IndexOf('x'), str.IndexOf('X')) - (str.IndexOf('=') + 1)).Trim());
                        GameSettings.windowheight = Int32.Parse(str.Substring(Math.Max(str.IndexOf('x'), str.IndexOf('X')) + 1).Trim());
                    }
                    else if (str.Length > 10 && str.Substring(0, 10).ToLower().Equals("fullscreen"))
                    {
                        GameSettings.fullScreen = Boolean.Parse(str.Substring(str.IndexOf('=') + 1).Trim());
                    }
                    else if (str.Length > 10 && str.Substring(0, 10).ToLower().Equals("halfrender"))
                    {
                        GameSettings.HALF_RENDER = Boolean.Parse(str.Substring(str.IndexOf('=') + 1).Trim());
                    }
                    else if (str.Length > 10 && str.Substring(0, 10).ToLower().Equals("iguihasfps"))
                    {
                        GameSettings.ShowFPS = Boolean.Parse(str.Substring(str.IndexOf('=') + 1).Trim());
                    }
                    else if (str.Length > 10 && str.Substring(0, 10).ToLower().Equals("igguistyle"))
                    {
                        String strn = str.Substring(str.IndexOf('=') + 1).Trim();
                        GameSettings.guiStyle = strn.ToLower().Equals("rockband") ? GameUIMaster.GUIStyle.RB : GameUIMaster.GUIStyle.UN;
                    }

                } while (!fin.EndOfStream);
#if !DEBUG
            }
            catch (FormatException)
            {
#if WINDOWS
                System.Windows.Forms.MessageBox.Show("config.cfg number was not a number!");
#endif
            }
            catch (Exception)
            {
#if WINDOWS
                System.Windows.Forms.MessageBox.Show("config.cfg general error");
#endif
            }
#endif
        }

        public static void SetProjMatrix(int w, int h)
        {
            RenderMaster.GetSingleton().Projection = Matrix.CreatePerspectiveFieldOfView((float)Math.PI / 4.0f,
                              w / (float)h,
                              2f, 750.0f);
        }
        
        protected override void LoadContent()
        {
            /*sfBassist = content.Load<SpriteFont>("fonts\\bassist");
            sfGuitarist = content.Load<SpriteFont>("fonts\\guitarist");
            sfDrummer = content.Load<SpriteFont>("fonts\\drummer");
            sfSinger = content.Load<SpriteFont>("fonts\\singer");
             */
            sfMenu = content.Load<SpriteFont>("fonts\\menu");
            Global.texDefaultBM = content.Load<Texture2D>("graphics\\blankbm");
            Global.DefaultFont = content.Load<SpriteFont>("BasicFont");
            Global.BigFont = content.Load<SpriteFont>("fonts\\bigfont");
            Global.SmallFont = content.Load<SpriteFont>("fonts\\smallfont");
            Global.HandwrittenFont = content.Load<SpriteFont>("fonts\\manager");
            Global.gradient = content.Load<Texture2D>("graphics\\gradient");
            gradientMask = content.Load<Texture2D>("graphics\\gradientMask");
            GameUIMaster.GetSingleton().texButtonGreen = content.Load<Texture2D>("graphics\\large_face_a");
            GameUIMaster.GetSingleton().texButtonRed = content.Load<Texture2D>("graphics\\large_face_b");
            GameUIMaster.GetSingleton().texButtonYellow = content.Load<Texture2D>("graphics\\large_face_y");
            Global.texWhite = content.Load<Texture2D>("graphics\\white");
        }

#region oldload
        /*private void LoadCustomSonglist()
        {

            System.IO.StreamReader sr = new System.IO.StreamReader("SongList.txt");
            String str = sr.ReadLine();
            str = str.Trim();
            int numSets = Int32.Parse(str.Substring(str.IndexOf('(') + 1, str.IndexOf(')') - str.IndexOf('(') - 1));
            cSongNames = new String[numSets][];
            vSongNames = new String[numSets][];
            for (int i = 0; i < numSets; i++)
            {
                int j = 0;
                String setname = "";
                while (true)
                {
                    str = sr.ReadLine().Trim();
                    if (str.Length >= 3 && str.Substring(0, 3).Equals("Set"))
                        j = Int32.Parse(str.Substring(str.IndexOf('(') + 1, str.IndexOf(')') - str.IndexOf('(') - 1));
                    if (str.Length >= 4 && str.Substring(0, 4).Equals("Name"))
                        setname = str.Substring(str.IndexOf('(') + 1, str.IndexOf(')') - str.IndexOf('(') - 1);
                    if (str.Length >= 8 && str.Substring(0, 8).Equals("NumSongs"))
                    {
                        int numsongs = Int32.Parse(str.Substring(str.IndexOf('(') + 1, str.IndexOf(')') - str.IndexOf('(') - 1));
                        cSongNames[j] = new String[Int32.Parse(str.Substring(str.IndexOf('(') + 1, str.IndexOf(')') - str.IndexOf('(') - 1))];
                        vSongNames[j] = new String[cSongNames[j].Length + 1];
                        vSongNames[j][0] = setname;
                        for (int k = 0; k < numsongs; k++)
                        {
                            str = sr.ReadLine().Trim();
                            cSongNames[j][k] = str.Substring(str.IndexOf('(') + 1, str.IndexOf(')') - str.IndexOf('(') - 1);
                            System.IO.BinaryReader tmp = new System.IO.BinaryReader(System.IO.File.OpenRead("songdata\\" + cSongNames[j][k] + ".gba"));
                            tmp.ReadByte();
                            vSongNames[j][k + 1] = tmp.ReadString();
                            tmp.Close();
                        }
                        break;
                    }
                }
            }
            sr.Close();
        }*/

        /*private void LoadGameContent()
        {
            if (GameLoaded || GameLoading)
                return;
            ThreadStart ThreadStarter = delegate
            {
                /*if (mode == M_GAME)
                    InitForSong(instruments[0], instruments[1], instruments[2], instruments[3], diff, venueName, this);
                else
                {
                    boardsTarget = new RenderTarget2D[instruments.Length];
                    for(int i=0;i<instruments.Length;i++)
                        if(instruments[i])
                            boardsTarget[i] = new RenderTarget2D(graphics.GraphicsDevice, GameSettings.windowwidth, GameSettings.windowheight, 1, SurfaceFormat.Color);
                    
                    Board.curveHeight = 0.03f;
                    Board.height = -2.0f;
                    Board.length = 3f;
                    Board.width = 0.7f;
                    Board.rotate = .5f;
                    Board.zeroZ = 2.8f;
                    Board.sFade = 0.8f;
                    Board.eFade = 1.2f;
                }


                


                

                
                

                Board.Load(graphics,content);
                
                
                
                
                
                Board.boardTexPlain = new Texture2D[2][];
                /*for (int i = 0; i < 2; i++)
                {
                    Board.boardTexPlain[i] = new Texture2D[Board.boardValidBPM.Length];
                    for (int k = 0; k < Board.boardValidBPM.Length; k++)
                    {
                        Board.boardTexPlain[i][Board.boardBeatsIndex[Board.boardValidBPM[k]]] = content.Load<Texture2D>("graphics\\board_" + (i + 4) + "" + (Board.boardValidBPM[k]));
                    }
                }
                //Board.boardTexPlain[1][4] = content.Load<Texture2D>("graphics\\test");
                
                GC.Collect();
                GameLoaded = true;
                GameLoading = false;
            };
            GameLoading = true;
            if(mode==M_GAME)
#if WINDOWS
                song = SongLoader.LoadSong(songname,this.Window.Handle);
#else
                song = new Song(songname,audioEngine,audioSoundBank,audioWaveBank);
#endif
            Thread myThread = new Thread(ThreadStarter);
            myThread.Start();
        }*/

        /*private void UnloadGameContent()
        {
            
            failStatus = new byte[4];
            for (int i = 0; i < 4; i++)
                failStatus[i] = FS_GOOD;
            failTime = -2;
            screenTarget.Dispose();
            screenTargetPre.Dispose();
            for(int i=0;i<4;i++)
            if(instruments[i] && i!=1)
            {rtBoard[i].Dispose();rtWaves[i].Dispose();}
            screenTarget = null;
            screenTargetPre = null;
            rtBoard = null;
            rtWaves = null;

            screenTargetFinal.Dispose();
            screenTargetFinal = null;
            texRockstarRed = null;
            texRockstarRing = null;
            texRockstarCover = null;
            texRockMeterOutline = null;
            texWhite = null;
            texRockMeterLogoStem = null;
            texRockMeterGuitarLogo = null;
            texRockMeterBassLogo = null;
            texRockMeterDrumLogo = null;
            texRockMeterSingerLogo = null;
            texScoreBoard = null;
            rmUNbg = null;
            rmUNfg = null;
            rmUNstar = null;
            rmUNstaro = null;
            texLine = null;
            texLineEnd = null;
            texGlow = null;
            for (int i = 0; i <= 12; i++)
                if(multToIndex[i]>=0)
                    texMult[multToIndex[i]] = null;
            for (int k = 0; k < 8; k++)
                texShard[k] = null;
            texSpark = null;
            /*for (int i = 0; i < 2; i++)
            {
                for (int k = 0; k < Board.boardValidBPM.Length; k++)
                {
                    Board.boardTexPlain[i][Board.boardBeatsIndex[Board.boardValidBPM[k]]] = null;
                }
            }* /
            Board.texTriggerBorder = null;
            Board.drumfillTex = null;
            Board.spMeterBG = null;
            Board.spMeterFill = null;
            Board.spMeterLED = null;
            Board.spMeterCurl = null;
            Board.texTriggerBorderLit = null;
            Board.texBlast = null;

            for (int i = 0; i < 4; i++)
                if (instruments[i])
                {
                    boardBackgrounds[i] = null;
                }

            for (int i = 0; i < 5; i++)
                Board.texTriggers[i] = null;
            for (int i = 0; i < 5; i++)
                Board.texTriggersLit[i] = null;
            for (int i = 0; i < 5; i++)
                Board.texNotes[i] = null;
            for (int i = 0; i <= 10; i++)
                texRockstarRingHiLi[i] = null;
            Board.vBar = null;
            Board.vBGExt = null;
            Board.vBGInt = null;
            Board.vFuzz = null;
            Board.vHeadBar = null;
            Board.vGlow = null;

            song = null;
            venue = null;

            GC.Collect();

            
            GameLoaded = false;
            GameLoading = false;
        }

        private void LoadMenuContent()
        {
            if (MenuLoaded || MenuLoading)
                return;
            
            MenuLoading = true;
            ThreadStart ThreadStarter = delegate
            {
                SongHiLi = content.Load<Texture2D>("graphics\\songhili");
                songchoosetop = content.Load<Texture2D>("graphics\\songscreentop");
                rtNote = new RenderTarget2D[4];
                rtNote[0] = new RenderTarget2D(graphics.GraphicsDevice, 256, 256, 1, SurfaceFormat.Color);
                rtNote[1] = new RenderTarget2D(graphics.GraphicsDevice, 256, 256, 1, SurfaceFormat.Color);
                rtNote[2] = new RenderTarget2D(graphics.GraphicsDevice, 256, 256, 1, SurfaceFormat.Color);
                rtNote[3] = new RenderTarget2D(graphics.GraphicsDevice, 256, 256, 1, SurfaceFormat.Color);
                mCurtain = content.Load<Model>("meshes\\curtain");
                texCurtainLeft = content.Load<Texture2D>("graphics\\leftcurtain");
                texCurtainRight = content.Load<Texture2D>("graphics\\rightcurtain");
                coolbg1 = content.Load<Texture2D>("graphics\\coolbg1");
                coolbg2 = content.Load<Texture2D>("graphics\\coolbg2");
                resultsScroller = content.Load<Texture2D>("graphics\\resultscroller");
                failbg = content.Load<Texture2D>("graphics\\faildialog");
                whitishTex = content.Load<Texture2D>("graphics\\whitish");
                greyishTex = new Texture2D[5];
                greyishTex[0]= content.Load<Texture2D>("graphics\\greyish");
                greyishTex[1]= content.Load<Texture2D>("graphics\\greyishE");
                greyishTex[2]= content.Load<Texture2D>("graphics\\greyishM");
                greyishTex[3]= content.Load<Texture2D>("graphics\\greyishH");
                greyishTex[4]= content.Load<Texture2D>("graphics\\greyishX");
                whitishBM = content.Load<Texture2D>("graphics\\whitishbm");
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
                mQuarter = content.Load<Model>("meshes\\quarter");
                mPick = content.Load<Model>("meshes\\pick");
                mPoD = content.Load<Model>("meshes\\pod");
                texQuarter = content.Load<Texture2D>("graphics\\quarter");
                texPickGP = content.Load<Texture2D>("graphics\\gppick");
                texPickTit = content.Load<Texture2D>("graphics\\titanium");
                texPoD = content.Load<Texture2D>("graphics\\pod");
                mDrumsticks = content.Load<Model>("meshes\\drumsticks");
                texDSticks = content.Load<Texture2D>("graphics\\ds1");
                texDTDSticks = content.Load<Texture2D>("graphics\\ds2");
                texSticks = content.Load<Texture2D>("graphics\\sticks");
                texTitDSticks = content.Load<Texture2D>("graphics\\titaniumd");
                mSticks = content.Load<Model>("meshes\\dsticks");
                mCMic = content.Load<Model>("meshes\\pcmic");
                mTube = content.Load<Model>("meshes\\pipemic");
                mMic = content.Load<Model>("meshes\\mic");
                mAMic = content.Load<Model>("meshes\\awesomemic");
                texMic = content.Load<Texture2D>("graphics\\mic");
                texCMic = content.Load<Texture2D>("graphics\\cmic");
                texAMic = content.Load<Texture2D>("graphics\\awesomemic");
                texTube = content.Load<Texture2D>("graphics\\pipemic");
                mStrap = content.Load<Model>("meshes\\strap");
                mString = content.Load<Model>("meshes\\string");
                texString = content.Load<Texture2D>("graphics\\string");
                texStrap1 = content.Load<Texture2D>("graphics\\strap1");
                texStrap2 = content.Load<Texture2D>("graphics\\strap2");
                texStrap3 = content.Load<Texture2D>("graphics\\strap3");
                arrowMdl = content.Load<Model>("meshes\\arrow");
                nailMdl = content.Load<Model>("meshes\\nail");
                arrowTex = content.Load<Texture2D>("graphics\\arrow");
                strat = content.Load<Model>("meshes\\stratcont");
                stand = content.Load<Model>("meshes\\gstand");
                stratTex = content.Load<Texture2D>("graphics\\stratcont");
                rustyTex = content.Load<Texture2D>("graphics\\rusty");
                contguis = new ContGUIData[5];
                for (int i = 0; i < 5; i++)
                    contguis[i] = ContGUIData.INVALID;
                GetContGUIData();
                arrowRot = new float[8];
                arrowTimer = new float[8];
                charNameSelected = new int[4];
                for (int i = 0; i < 4; i++)
                    charNameSelected[i] = -1;
                charNames = new String[3][];
                String[] files = System.IO.Directory.GetFiles("characters\\");
                int[] num = new int[3];
                for (int i = 0; i < files.Length; i++)
                {
                    if (files[i].EndsWith(".ggc"))
                        num[0]++;
                    else if (files[i].EndsWith(".gdc"))
                        num[1]++;
                    else if (files[i].EndsWith(".gvc"))
                        num[2]++;
                }
                charNames[0] = new String[num[0]];
                charNames[1] = new String[num[1]];
                charNames[2] = new String[num[2]];
                for (int i = 0; i < files.Length; i++)
                {
                    if (files[i].EndsWith(".ggc"))
                    { charNames[0][charNames[0].Length - num[0]] = files[i].Substring(files[i].LastIndexOf('\\') + 1, files[i].LastIndexOf('.') - files[i].LastIndexOf('\\') - 1); num[0]--; }
                    else if (files[i].EndsWith(".gdc"))
                    { charNames[1][charNames[1].Length - num[1]] = files[i].Substring(files[i].LastIndexOf('\\') + 1, files[i].LastIndexOf('.') - files[i].LastIndexOf('\\') - 1); num[1]--; }
                    else if (files[i].EndsWith(".gvc"))
                    { charNames[2][charNames[2].Length - num[2]] = files[i].Substring(files[i].LastIndexOf('\\') + 1, files[i].LastIndexOf('.') - files[i].LastIndexOf('\\') - 1); num[2]--; }
                }
                finals = new bool[4];
                ContGUIData.KB_ICO_DRUM = content.Load<Texture2D>("graphics\\keyboard_d");
                ContGUIData.KB_ICO_BLUR = content.Load<Texture2D>("graphics\\keyboard_a");
                ContGUIData.KB_ICO_GUITAR = content.Load<Texture2D>("graphics\\keyboard_g");
                ContGUIData.KB_ICO_VOCAL = content.Load<Texture2D>("graphics\\keyboard_v");
                ContGUIData.GUITAR_ICO = content.Load<Texture2D>("graphics\\guitarlogo");
                ContGUIData.GUITAR_ICO_BLUR = content.Load<Texture2D>("graphics\\guitarlogo_blur");
                ContGUIData.GUITARX_ICO = content.Load<Texture2D>("graphics\\xplorerlogo");
                ContGUIData.GUITARX_ICO_BLUR = content.Load<Texture2D>("graphics\\xplorerlogo_blur");
                ContGUIData.DRUMS_ICO = content.Load<Texture2D>("graphics\\drumslogo");
                ContGUIData.DRUMS_ICO_BLUR = content.Load<Texture2D>("graphics\\drumslogo_blur");
                ContGUIData.MICROPHONE_ICO = content.Load<Texture2D>("graphics\\mphonelogo");
                ContGUIData.MICROPHONE_ICO_BLUR = content.Load<Texture2D>("graphics\\mphonelogo_blur");
                texContinue = content.Load<Texture2D>("graphics\\continue");
                GC.Collect();
                MenuLoaded = true;
                MenuLoading = false;
            };
            Thread myThread = new Thread(ThreadStarter);
            myThread.Start();
                

        }*/
#endregion

        protected override void Update(GameTime gameTime)
        {
            PeripheralManager.GetSingleton().QueryAll();
            //Thread.Sleep(1);
            GameSettings.windowheight = graphics.GraphicsDevice.Viewport.Height;
            GameSettings.windowwidth = graphics.GraphicsDevice.Viewport.Width;

            if (demomodepress != Keyboard.GetState().IsKeyDown(Keys.O))
            {
                demomodepress = Keyboard.GetState().IsKeyDown(Keys.O);
                if (demomodepress)
                    Global.DemoMode = !Global.DemoMode;
            }

            currentState.Peek().Update(gameTime);

            base.Update(gameTime);
        }

        private void FillSongDiffs(string songname)
        {
            
        }

        protected override void Draw(GameTime gameTime)
        {
            currentState.Peek().Render(gameTime);

            base.Draw(gameTime);
        }

        public void InitForSong(bool guitarist, bool vocalist, bool percussionist, bool bassist, byte[] difficulty, String venueStr, UnsignedGame gameRef) 
        {
            /*if (guitarist && bassist && percussionist && vocalist)
            {
                //song = new Song(4, 2, songname,gameRef.Window.Handle);
                boards[0] = new Board(GUITAR, 0, song, difficulty[0]);
                boards[0].xOffset = -(int)(250f/800f*GameSettings.windowwidth);
                boards[1] = new Board(VOCALIST, 0, song, difficulty[1]);
                boards[2] = new Board(DRUMS, 0, song, difficulty[2]);
                boards[2].xOffset = 0;
                boards[3] = new Board(BASS, 0, song, difficulty[3]);
                boards[3].xOffset = (int)(250f/800f*GameSettings.windowwidth);
                Board.height = -1.5f;
                Board.length = 2.0f;
                Board.width = 0.3f;
                Board.rotate = .4f;
                Board.zeroZ = 2.3f;
                Board.sFade = 1.8f;
                Board.eFade = 2.3f;
                Board.spShift = -0.01f;
                Board.vocaly = 10;
                Board.vocalheight = 140;
                Board.vocalzerox = GameSettings.windowwidth / 10;
                Board.vocalwidth = 0.25f;
                boardsTarget[1] = new RenderTarget2D(graphics.GraphicsDevice, GameSettings.windowwidth, GameSettings.windowheight, 1, SurfaceFormat.Color);
                boardsTarget[0] = new RenderTarget2D(graphics.GraphicsDevice, GameSettings.windowwidth, GameSettings.windowheight, 1, SurfaceFormat.Color);
                boardsTarget[2] = new RenderTarget2D(graphics.GraphicsDevice, GameSettings.windowwidth, GameSettings.windowheight, 1, SurfaceFormat.Color);
                boardsTarget[3] = new RenderTarget2D(graphics.GraphicsDevice, GameSettings.windowwidth, GameSettings.windowheight, 1, SurfaceFormat.Color);
            }
            else if (guitarist && bassist && percussionist && !vocalist)
            {
                //song = new Song(4, 2, songname,gameRef.Window.Handle);
                boards[0] = new Board(GUITAR, 0, song, difficulty[0]);
                boards[0].xOffset = -300;
                boards[2] = new Board(DRUMS, 0, song, difficulty[2]);
                boards[2].xOffset = 0;
                boards[3] = new Board(BASS, 0, song, difficulty[3]);
                boards[3].xOffset = 300;
                Board.height = -1.5f;
                Board.length = 2.0f;
                Board.width = 0.3f;
                Board.rotate = .4f;
                Board.zeroZ = 2.3f;
                Board.sFade = 1.8f;
                Board.eFade = 2.3f;
                Board.spShift = -0.01f;
                boardsTarget[0] = new RenderTarget2D(graphics.GraphicsDevice, GameSettings.windowwidth, GameSettings.windowheight, 1, SurfaceFormat.Color);
                boardsTarget[2] = new RenderTarget2D(graphics.GraphicsDevice, GameSettings.windowwidth, GameSettings.windowheight, 1, SurfaceFormat.Color);
                boardsTarget[3] = new RenderTarget2D(graphics.GraphicsDevice, GameSettings.windowwidth, GameSettings.windowheight, 1, SurfaceFormat.Color);
            }
            else if (guitarist && bassist && !percussionist && vocalist)
            {
                //song = new Song(4, 2, songname,gameRef.Window.Handle);
                boards[0] = new Board(GUITAR, 0, song, difficulty[0]);
                boards[0].xOffset = -(int)(175f/800f*GameSettings.windowwidth);
                boards[1] = new Board(VOCALIST, 0, song, difficulty[1]);
                boards[3] = new Board(BASS, 0, song, difficulty[3]);
                boards[3].xOffset = (int)(175f/800f*GameSettings.windowwidth);
                Board.height = -1.5f;
                Board.length = 1.5f;
                Board.width = 0.4f;
                Board.rotate = .4f;
                Board.zeroZ = 2.3f;
                Board.sFade = 1.8f;
                Board.eFade = 2.3f;
                Board.spShift = -0.01f;
                Board.vocaly = 10;
                Board.vocalheight = 140;
                Board.vocalzerox = GameSettings.windowwidth / 10;
                Board.vocalwidth = 0.25f;
                boardsTarget[1] = new RenderTarget2D(graphics.GraphicsDevice, GameSettings.windowwidth, GameSettings.windowheight, 1, SurfaceFormat.Color);
                boardsTarget[0] = new RenderTarget2D(graphics.GraphicsDevice, GameSettings.windowwidth, GameSettings.windowheight, 1, SurfaceFormat.Color);
                boardsTarget[3] = new RenderTarget2D(graphics.GraphicsDevice, GameSettings.windowwidth, GameSettings.windowheight, 1, SurfaceFormat.Color);
            }
            else if (guitarist && !bassist && percussionist && vocalist)
            {
                //song = new Song(4, 2, songname,gameRef.Window.Handle);
                boards[0] = new Board(GUITAR, 0, song, difficulty[0]);
                boards[0].xOffset = -(int)(175f/800f*GameSettings.windowwidth);
                boards[1] = new Board(VOCALIST, 0, song, difficulty[1]);
                boards[2] = new Board(DRUMS, 0, song, difficulty[2]);
                boards[2].xOffset = (int)(175f/800f*GameSettings.windowwidth);
                Board.height = -1.5f;
                Board.length = 1.5f;
                Board.width = 0.4f;
                Board.rotate = .4f;
                Board.zeroZ = 2.3f;
                Board.sFade = 1.8f;
                Board.eFade = 2.3f;
                Board.spShift = -0.01f;
                Board.vocaly = 10;
                Board.vocalheight = 140;
                Board.vocalzerox = GameSettings.windowwidth / 10;
                Board.vocalwidth = 0.25f;
                boardsTarget[1] = new RenderTarget2D(graphics.GraphicsDevice, GameSettings.windowwidth, GameSettings.windowheight, 1, SurfaceFormat.Color);
                boardsTarget[0] = new RenderTarget2D(graphics.GraphicsDevice, GameSettings.windowwidth, GameSettings.windowheight, 1, SurfaceFormat.Color);
                boardsTarget[2] = new RenderTarget2D(graphics.GraphicsDevice, GameSettings.windowwidth, GameSettings.windowheight, 1, SurfaceFormat.Color);
            }
            else if (!guitarist && bassist && percussionist && vocalist)
            {
                //song = new Song(4, 2, songname,gameRef.Window.Handle);
                boards[2] = new Board(DRUMS, 0, song, difficulty[2]);
                boards[2].xOffset = -(int)(175f/800f*GameSettings.windowwidth);
                boards[1] = new Board(VOCALIST, 0, song, difficulty[1]);
                boards[3] = new Board(BASS, 0, song, difficulty[3]);
                boards[3].xOffset = (int)(175f/800f*GameSettings.windowwidth);
                Board.height = -1.5f;
                Board.length = 1.5f;
                Board.width = 0.4f;
                Board.rotate = .4f;
                Board.zeroZ = 2.3f;
                Board.sFade = 1.8f;
                Board.eFade = 2.3f;
                Board.spShift = -0.01f;
                Board.vocaly = 10;
                Board.vocalheight = 140;
                Board.vocalzerox = GameSettings.windowwidth / 10;
                Board.vocalwidth = 0.25f;
                boardsTarget[1] = new RenderTarget2D(graphics.GraphicsDevice, GameSettings.windowwidth, GameSettings.windowheight, 1, SurfaceFormat.Color);
                boardsTarget[2] = new RenderTarget2D(graphics.GraphicsDevice, GameSettings.windowwidth, GameSettings.windowheight, 1, SurfaceFormat.Color);
                boardsTarget[3] = new RenderTarget2D(graphics.GraphicsDevice, GameSettings.windowwidth, GameSettings.windowheight, 1, SurfaceFormat.Color);
            }
            else if (guitarist && bassist && !percussionist && !vocalist)
            {
                //song = new Song(4, 2, songname,gameRef.Window.Handle);
                boards[0] = new Board(GUITAR, 0, song, difficulty[0]);
                boards[0].xOffset = -175;
                boards[0].yRotate = -0.15f;
                boards[3] = new Board(BASS, 0, song, difficulty[3]);
                boards[3].xOffset = 175;
                boards[3].yRotate = 0.15f;
                Board.curveHeight = 0.02f;
                Board.height = -1.5f;
                Board.length = 2.5f;
                Board.width = 0.4f;
                Board.rotate = .4f;
                Board.zeroZ = 2.3f;
                Board.sFade = 0.8f;
                Board.eFade = 1.2f;
                boardsTarget[0] = new RenderTarget2D(graphics.GraphicsDevice, GameSettings.windowwidth, GameSettings.windowheight, 1, SurfaceFormat.Color);
                boardsTarget[3] = new RenderTarget2D(graphics.GraphicsDevice, GameSettings.windowwidth, GameSettings.windowheight, 1, SurfaceFormat.Color);
            }
            else if (!guitarist && !bassist && percussionist && vocalist)
            {
                boards[2] = new Board(DRUMS, 0, song, difficulty[2]);
                Board.curveHeight = 0.03f;
                Board.height = -1.6f;
                Board.length = 3f;
                Board.width = 0.6f;
                Board.rotate = .3f;
                Board.zeroZ = 2.8f;
                Board.sFade = 0.8f;
                Board.eFade = 1.2f;
                boardsTarget[2] = new RenderTarget2D(graphics.GraphicsDevice, GameSettings.windowwidth, GameSettings.windowheight, 1, SurfaceFormat.Color);
                boards[1] = new Board(VOCALIST, 0, song, difficulty[1]);
                Board.vocaly = 10;
                Board.vocalheight = 140;
                Board.vocalzerox = GameSettings.windowwidth / 10;
                Board.vocalwidth = 0.25f;
                boardsTarget[1] = new RenderTarget2D(graphics.GraphicsDevice, GameSettings.windowwidth, GameSettings.windowheight, 1, SurfaceFormat.Color);
            }
            else if (guitarist && !bassist && !percussionist && vocalist)
            {
                boards[0] = new Board(GUITAR, 0, song, difficulty[0]);
                Board.curveHeight = 0.03f;
                Board.height = -2.0f;
                Board.length = 3f;
                Board.width = 0.7f;
                Board.rotate = .5f;
                Board.zeroZ = 2.8f;
                Board.sFade = 0.8f;
                Board.eFade = 1.2f;
                boardsTarget[0] = new RenderTarget2D(graphics.GraphicsDevice, GameSettings.windowwidth, GameSettings.windowheight, 1, SurfaceFormat.Color);
                boards[1] = new Board(VOCALIST, 0, song, difficulty[1]);
                Board.vocaly = 10;
                Board.vocalheight = 140;
                Board.vocalzerox = GameSettings.windowwidth / 10;
                Board.vocalwidth = 0.25f;
                boardsTarget[1] = new RenderTarget2D(graphics.GraphicsDevice, GameSettings.windowwidth, GameSettings.windowheight, 1, SurfaceFormat.Color);
            }
            else if (!guitarist && bassist && percussionist && !vocalist)
            {
                //song = new Song(4, 2, songname,gameRef.Window.Handle);
                boards[3] = new Board(BASS, 0, song, difficulty[3]);
                boards[3].xOffset = -175;
                boards[3].yRotate = -0.15f;
                boards[2] = new Board(DRUMS, 0, song, difficulty[2]);
                boards[2].xOffset = 175;
                boards[2].yRotate = 0.15f;
                Board.curveHeight = 0.02f;
                Board.height = -1.5f;
                Board.length = 2.5f;
                Board.width = 0.4f;
                Board.rotate = .4f;
                Board.zeroZ = 2.3f;
                Board.sFade = 0.8f;
                Board.eFade = 1.2f;
                boardsTarget[3] = new RenderTarget2D(graphics.GraphicsDevice, GameSettings.windowwidth, GameSettings.windowheight, 1, SurfaceFormat.Color);
                boardsTarget[2] = new RenderTarget2D(graphics.GraphicsDevice, GameSettings.windowwidth, GameSettings.windowheight, 1, SurfaceFormat.Color);
            }
            else if (guitarist && !bassist && percussionist && !vocalist)
            {
                //song = new Song(4, 2, songname,gameRef.Window.Handle);
                boards[0] = new Board(GUITAR, 0, song, difficulty[0]);
                boards[0].xOffset = -200;
                boards[0].yRotate = -0.15f;
                boards[2] = new Board(DRUMS, 0, song, difficulty[2]);
                boards[2].xOffset = 200;
                boards[2].yRotate = 0.15f;
                Board.curveHeight = 0.02f;
                Board.height = -1.5f;
                Board.length = 2.5f;
                Board.width = 0.4f;
                Board.rotate = .4f;
                Board.zeroZ = 2.3f;
                Board.sFade = 0.8f;
                Board.eFade = 1.2f;
                boardsTarget[0] = new RenderTarget2D(graphics.GraphicsDevice, GameSettings.windowwidth, GameSettings.windowheight, 1, SurfaceFormat.Color);
                boardsTarget[2] = new RenderTarget2D(graphics.GraphicsDevice, GameSettings.windowwidth, GameSettings.windowheight, 1, SurfaceFormat.Color);
            }
            else if (!guitarist && bassist && !percussionist && vocalist)
            {
                boards[3] = new Board(BASSIST, 0, song, difficulty[3]);
                Board.curveHeight = 0.03f;
                Board.height = -2.0f;
                Board.length = 3f;
                Board.width = 0.7f;
                Board.rotate = .5f;
                Board.zeroZ = 2.8f;
                Board.sFade = 0.8f;
                Board.eFade = 1.2f;
                boardsTarget[3] = new RenderTarget2D(graphics.GraphicsDevice, GameSettings.windowwidth, GameSettings.windowheight, 1, SurfaceFormat.Color);
                boards[1] = new Board(VOCALIST, 0, song, difficulty[1]);
                Board.vocaly = 10;
                Board.vocalheight = 140;
                Board.vocalzerox = GameSettings.windowwidth / 10;
                Board.vocalwidth = 0.25f;
                boardsTarget[1] = new RenderTarget2D(graphics.GraphicsDevice, GameSettings.windowwidth, GameSettings.windowheight, 1, SurfaceFormat.Color);
            }
            else if (guitarist && !bassist && !percussionist && !vocalist)
            {
                //song = new Song(4, 2, songname,gameRef.Window.Handle);
                boards[0] = new Board(GUITAR, 0, song, difficulty[0]);
                Board.curveHeight = 0.03f;
                Board.height = -2.0f;
                Board.length = 3f;
                Board.width = 0.7f;
                Board.rotate = .5f;
                Board.zeroZ = 2.8f;
                Board.sFade = 0.8f;
                Board.eFade = 1.2f;
                boardsTarget[0] = new RenderTarget2D(graphics.GraphicsDevice, GameSettings.windowwidth, GameSettings.windowheight, 1, SurfaceFormat.Color);
            }
            else if (!guitarist && bassist && !percussionist && !vocalist)
            {
                //song = new Song(4, 2, songname,gameRef.Window.Handle);
                boards[3] = new Board(BASS, 0, song, difficulty[3]);
                Board.curveHeight = 0.03f;
                Board.height = -2.0f;
                Board.length = 3f;
                Board.width = 0.7f;
                Board.rotate = .5f;
                Board.zeroZ = 2.8f;
                Board.sFade = 0.8f;
                Board.eFade = 1.2f;
                boardsTarget[3] = new RenderTarget2D(graphics.GraphicsDevice, GameSettings.windowwidth, GameSettings.windowheight, 1, SurfaceFormat.Color);
            }
            else if (!guitarist && !bassist && percussionist && !vocalist)
            {
                //song = new Song(4, 2, songname,gameRef.Window.Handle);
                boards[2] = new Board(DRUMS, 0, song, difficulty[2]);
                Board.curveHeight = 0.03f;
                Board.height = -1.6f;
                Board.length = 3f;
                Board.width = 0.6f;
                Board.rotate = .3f;
                Board.zeroZ = 2.8f;
                Board.sFade = 0.8f;
                Board.eFade = 1.2f;
                boardsTarget[2] = new RenderTarget2D(graphics.GraphicsDevice, GameSettings.windowwidth, GameSettings.windowheight, 1, SurfaceFormat.Color);
            }
            else if (!guitarist && !bassist && !percussionist && vocalist)
            {
                
                boards[1] = new Board(VOCALIST, 0, song, difficulty[1]);
                Board.vocaly = 10;
                Board.vocalheight = 140;
                Board.vocalzerox = GameSettings.windowwidth / 10;
                Board.vocalwidth = 0.25f;
                boardsTarget[1] = new RenderTarget2D(graphics.GraphicsDevice, GameSettings.windowwidth, GameSettings.windowheight, 1, SurfaceFormat.Color);
            }*/

            
        }

        public static double dirdistTOhdist(double dir, double dist)
        {
            if (dist < 0)
            {
                dir -= 180;
                dist = -dist;
            }
            while (dir < 0)
                dir += 360;
            while (dir >= 360)
                dir -= 360;
            if (dir == 0)
                return dist;
            else if (dir == 90)
                return 0;
            else if (dir == 180)
                return -dist;
            else if (dir == 270)
                return 0;
            else if (dir > 0 && dir < 90)
                return Math.Sin((90 - dir) / 180f * Math.PI) * dist;
            else if (dir > 90 && dir < 180)
                return -Math.Sin((dir - 90) / 180f * Math.PI) * dist;
            else if (dir > 180 && dir < 270)
                return -Math.Sin((270 - dir) / 180f * Math.PI) * dist;
            else
                return Math.Sin((dir - 270) / 180f * Math.PI) * dist;
        }

        public static double dirdistTOvdist(double dir, double dist)
        {
            if (dist < 0)
            {
                dir -= 180;
                dist = -dist;
            }
            while (dir < 0)
                dir += 360;
            while (dir >= 360)
                dir -= 360;
            if (dir == 0)
                return 0;
            else if (dir == 90)
                return -dist;
            else if (dir == 180)
                return 0;
            else if (dir == 270)
                return dist;
            else if (dir > 0 && dir < 90)
                return -Math.Cos((90 - dir) / 180f * Math.PI) * dist;
            else if (dir > 90 && dir < 180)
                return -Math.Cos((dir - 90) / 180f * Math.PI) * dist;
            else if (dir > 180 && dir < 270)
                return Math.Cos((270 - dir) / 180f * Math.PI) * dist;
            else
                return Math.Cos((dir - 270) / 180f * Math.PI) * dist;
        }

        public static double hvdistTOdir(double hDist, double vDist)
        {
            if (vDist == 0)
            {
                if (hDist >= 0)
                {
                    return 0;
                }
                else
                {
                    return 180;
                }
            }
            else if (hDist == 0)
            {
                if (vDist > 0)
                {
                    return 270;
                }
                else
                {
                    return 90;
                }
            }
            else if (hDist > 0 && vDist < 0)
            {
                return ((Math.Atan(Math.Abs(vDist) / Math.Abs((double)hDist))) / Math.PI * 180);
            }
            else if (hDist < 0 && vDist < 0)
            {
                return ((Math.Atan(Math.Abs(hDist) / Math.Abs((double)vDist))) / Math.PI * 180 + 90);
            }
            else if (hDist < 0 && vDist > 0)
            {
                return ((Math.Atan(Math.Abs((double)vDist) / Math.Abs(hDist))) / Math.PI * 180 + 180);
            }
            else
            {
                return ((Math.Atan(Math.Abs(hDist) / Math.Abs((double)vDist))) / Math.PI * 180 + 270);
            }
        }

        #region shards
        private static Vector3[] shardmethlist = { new Vector3(-.5f,0f,0f),new Vector3(-.5f,1f,0f),new Vector3(-.5f,.5f,.5f),new Vector3(-.5f,.5f,-.5f),new Vector3(.5f,0f,0f),new Vector3(.5f,1f,0f),new Vector3(.5f,.5f,.5f),new Vector3(.5f,.5f,-.5f)};
        
        #endregion

        public void PushState(BaseState state)
        {
            state.Load(content);
            currentState.Push(state);
        }

        internal void PopState()
        {
            currentState.Peek().Unload();
            currentState.Pop();
        }

        internal void RestartSong()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        internal void EndSong()
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}

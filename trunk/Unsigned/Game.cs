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

        float[] lastframes = new float[60];
        int frameIndex;

        GraphicsDeviceManager graphics;
        ContentManager content;
        AudioEngine audioEngine;
        SoundBank audioSoundBank;
        WaveBank audioWaveBank;

#region misc

        private RenderTarget2D ort;
        public static Random r;
        private bool demomodepress = false;
#endregion

#region PauseMenu
        
        
#endregion

#region boards

        
        private Texture2D gradientMask;
        private Texture2D[] boardBackgrounds;
        private RenderTarget2D[] rtBoard, rtWaves;
#endregion

#region song

        private double CurrentTime, lastChange;
        private SetList[] setLists;

#endregion

#region venue

        private static Venue venue;
        private string venueName = "tikibar";

#endregion

#region input

        GamePadState[] controllers;
        GamePadCapabilities[] contCapabilities;
        byte[] contInput;// ...4==keyboard...? yea!
        private byte guitarStrum=0, bassStrum=0, drumsStrum=0, vocalStrum=0;
        //vocal strum? drum strum? yep, its for the pause menu
        private bool guitarGreen = false, bassGreen = false, drumsGreen = false, vocalGreen = false;

#endregion

#region menudata
        private struct ContGUIData
        {
            public CONT_TYPE type;
            public float loc;
            public int nextLoc;
            public PlayerIndex index;
            public Vector4 info;
            public byte status;//0-none,1-finalized,2-chosen
            int wait;

            public ContGUIData(CONT_TYPE type, PlayerIndex index)
            {
                this.type = type;
                loc = -1f;
                nextLoc = 0;
                this.index = index;
                info = Vector4.Zero;
                wait = 0;
                status = 0;
            }

            public RETURN_VALUE Update(GameTime gameTime, bool[] filled)
            {
                if (this.Equals(INVALID))
                    return RETURN_VALUE.NOTHING;

                //handle movement 
                if (loc != nextLoc)
                    loc += Math.Sign(nextLoc-loc)*(Math.Max(Math.Abs(nextLoc - loc),1) *(gameTime.ElapsedGameTime.Milliseconds/500f));
                if (Math.Abs(loc - nextLoc) < 0.05)
                    loc = nextLoc;

                Vector3[] nfo = { new Vector3(-140, 180, -120), new Vector3(-140, 190, -110), new Vector3(-96, 128, -110), new Vector3(-32, 128, -110), new Vector3(32, 128, -110), new Vector3(96, 128, -110), new Vector3(96, 128, -110) };
                if (loc < 0)
                {
                    info = new Vector4(nfo[0].X + 100 * loc, nfo[0].Y-(int)(index+1)*32, nfo[0].Z, (float)Math.PI / 2);
                }
                else if (loc >= 0)
                {
                    if (type == CONT_TYPE.KEYBOARD)
                    {
                        if (loc < 0.2)
                            info = new Vector4((nfo[0] * ((0.2f - loc) / 0.2f)) + (nfo[1] * (loc % 1 / 0.2f)), (1 - loc) * (float)(Math.PI / 2));
                        else if (loc < 1)
                            info = new Vector4((nfo[1] * ((1f - loc) / 0.8f)) + (nfo[2] * ((loc - 0.2f) / 0.8f)), (1 - loc) * (float)(Math.PI / 2));
                        else
                            info = new Vector4((nfo[(int)loc+1] * (1 - (loc - (int)(loc)))) + (nfo[(int)loc + 2] * (loc - (int)(loc))), 0);
                    }
                    else
                    {
                        if (loc < 1)
                            info = new Vector4((new Vector3(nfo[0].X,nfo[0].Y-32*(int)(index+1),nfo[0].Z) * (1 - (loc - (int)(loc)))) + (nfo[(int)loc + 2] * (loc - (int)(loc))), 0);
                        else
                            info = new Vector4((nfo[(int)loc + 1] * (1 - (loc - (int)(loc)))) + (nfo[(int)loc + 2] * (loc - (int)(loc))), 0);
                    }
                }

                //handle input
                if (wait > 0)
                { wait--; return RETURN_VALUE.NOTHING; }
                else if (loc!=(int)loc)
                { return RETURN_VALUE.NOTHING; }
                else if (nextLoc!=loc)
                { return RETURN_VALUE.NOTHING; }
                
                bool down = false, up = false, green = false, red = false;
                    
                if (type == CONT_TYPE.KEYBOARD)
                {
                    KeyboardState kbs = Keyboard.GetState();
                    if (kbs.IsKeyDown(Keys.Up) || kbs.IsKeyDown(Keys.Left))
                        up = true;
                    if (kbs.IsKeyDown(Keys.Down) || kbs.IsKeyDown(Keys.Right))
                        down = true;
                    if (kbs.IsKeyDown(Keys.Enter) || kbs.IsKeyDown(Keys.Space))
                        green = true;
                    if (kbs.IsKeyDown(Keys.Escape) || kbs.IsKeyDown(Keys.Back))
                        red = true;
                }
                else
                {
                    GamePadState gps = GamePad.GetState(index);
                    if (gps.IsButtonDown(Buttons.A))
                        green = true;
                    if (gps.IsButtonDown(Buttons.B))
                        red = true;
                    if (gps.DPad.Down == ButtonState.Pressed)
                        down = true;
                    if (gps.DPad.Up == ButtonState.Pressed)
                        up = true;
                    if(type==CONT_TYPE.DRUMSET)
                    {
                        if (gps.Buttons.Y == ButtonState.Pressed)
                            up = true;
                        if (gps.Buttons.X == ButtonState.Pressed)
                            down = true;
                    }
                }

                if (!up && !down && !green && !red)
                    return RETURN_VALUE.NOTHING;

                if (type == CONT_TYPE.LESPAUL || type == CONT_TYPE.STRATOCASTER || type == CONT_TYPE.XPLORER)
                {
                    if (up && status==0)
                    {
                        wait = 30;
                        if (loc == 1)
                            nextLoc = 0;
                        else if (loc > 1 && !filled[0])
                            nextLoc = 1;
                        else if (loc > 1)
                            nextLoc = 0;
                    }
                    else if (down && status==0)
                    {
                        wait = 30;
                        if (loc == 0 && !filled[1])
                            nextLoc = 1;
                        else if (loc == 0 && !filled[3])
                            nextLoc = 4;
                        else if (loc >= 1 && !filled[3])
                            nextLoc = 4;
                    }
                    else if (up && status==1)
                    {wait = 15; return RETURN_VALUE.DECREMENT_NAME;}
                    else if (down && status==1)
                    {wait = 15; return RETURN_VALUE.INCREMENT_NAME;}
                    else if (red)
                    {
                        wait = 30;
                        if (status == 2)
                            status = 1;
                        else if (status == 1)
                        { status = 0; return RETURN_VALUE.UPDATE_NOTE; }
                        else if (status == 0)
                            nextLoc = 0;
                    }
                    else if (green)
                    {
                        wait = 30;
                        if(status == 2)
                            return RETURN_VALUE.NEXT_SCREEN;
                        else if(status == 1)
                            status = 2;
                        else if(status==0)
                        {
                            if (loc == 1 || loc == 4)
                            { status = 1; return RETURN_VALUE.UPDATE_NOTE; }
                            else if (loc == 0)
                            {
                                if (!filled[0])
                                    nextLoc = 1;
                                else if (!filled[3])
                                    nextLoc = 4;
                            }
                        }
                    }
                }
                else if (type == CONT_TYPE.DRUMSET)
                {
                    if (up && status==0)
                    {
                        wait = 30;
                        if (loc >= 1)
                            nextLoc = 0;
                    }
                    else if (down && status==0)
                    {
                        wait = 30;
                        if (loc == 0 && !filled[2])
                            nextLoc = 3;
                    }
                    else if (up && status==1)
                    {wait = 15; return RETURN_VALUE.DECREMENT_NAME;}
                    else if (down && status==1)
                    {wait = 15; return RETURN_VALUE.INCREMENT_NAME;}
                    else if (red)
                    {
                        wait = 30;
                        if (status == 2)
                            status = 1;
                        else if (status == 1)
                        { status = 0; return RETURN_VALUE.UPDATE_NOTE; }
                        else if (status == 0)
                            nextLoc = 0;
                    }
                    else if (green)
                    {
                        wait = 30;
                        if (status == 2)
                            return RETURN_VALUE.NEXT_SCREEN;
                        else if (status == 1)
                            status = 2;
                        else if (status == 0 && loc == 3)
                            status = 1;
                        else if (status == 0 && loc == 0)
                            nextLoc = 3;
                    }
                }
                else if (type == CONT_TYPE.KEYBOARD)
                {
                    if (up && status==0)
                    {
                        wait = 30;
                        if (loc == 1)
                            nextLoc = 0;
                        else if (loc == 2 && !filled[0])
                            nextLoc = 1;
                        else if (loc == 2)
                            nextLoc = 0;
                        else if (loc == 3 && !filled[1])
                            nextLoc = 2;
                        else if (loc == 3 && !filled[0])
                            nextLoc = 1;
                        else if (loc == 3)
                            nextLoc = 0;
                        else if (loc == 4 && !filled[2])
                            nextLoc = 3;
                        else if (loc == 4 && !filled[0])
                            nextLoc = 1;
                        else if (loc == 4)
                            nextLoc = 0;
                    }
                    else if (down && status==0)
                    {
                        wait = 30;
                        if (loc == 0 && !filled[0])
                            nextLoc = 1;
                        else if (loc == 0 && !filled[1])
                            nextLoc = 2;
                        else if (loc == 0 && !filled[2])
                            nextLoc = 3;
                        else if (loc == 0 && !filled[3])
                            nextLoc = 4;
                        else if (loc == 1 && !filled[1])
                            nextLoc = 2;
                        else if (loc == 1 && !filled[2])
                            nextLoc = 3;
                        else if (loc == 1 && !filled[3])
                            nextLoc = 4;
                        else if (loc == 2 && !filled[2])
                            nextLoc = 3;
                        else if (loc == 2 && !filled[3])
                            nextLoc = 4;
                        else if (loc == 3 && !filled[3])
                            nextLoc = 4;
                    }
                    else if (up && status==1)
                    {wait = 30; return RETURN_VALUE.DECREMENT_NAME;}
                    else if (down && status==1)
                    {wait = 30; return RETURN_VALUE.INCREMENT_NAME;}
                    else if (red)
                    {
                        wait = 30;
                        if (status == 2)
                            status = 1;
                        else if (status == 1)
                        { status = 0; return RETURN_VALUE.UPDATE_NOTE; }
                        else if (status == 0)
                            nextLoc = 0;
                    }
                    else if (green && loc>0)
                    {
                        wait = 30;
                        if (status == 2)
                            return RETURN_VALUE.NEXT_SCREEN;
                        else if (status == 1)
                            status = 2;
                        else if (status == 0)
                        {status = 1;return RETURN_VALUE.UPDATE_NOTE;}
                    }
                }


                return RETURN_VALUE.NOTHING;
            }

            public override bool Equals(object obj)
            {
                return index == ((ContGUIData)obj).index;
            }

            public override int GetHashCode()
            {
                return (int)index;
            }

            public int GetLeaderVal()
            {
                if (type == CONT_TYPE.KEYBOARD)
                    return 5;
                if (type == CONT_TYPE.DRUMSET)
                    return 2;
                if (type == CONT_TYPE.MICROPHONE)
                    return 4;
                else
                    return 3;
            }

            public enum CONT_TYPE{ KEYBOARD = 0, XPLORER = 1, STRATOCASTER = 2, LESPAUL = 3,
                                          DRUMSET = 4, MICROPHONE = 5};
            public enum RETURN_VALUE { NOTHING = 0, UPDATE_NOTE = 1, NEXT_SCREEN = 2, INCREMENT_NAME=3, DECREMENT_NAME=4 };

            public static ContGUIData INVALID = new ContGUIData((CONT_TYPE) (-1), (PlayerIndex) (-2));
            public static Texture2D KB_ICO_BLUR, KB_ICO_GUITAR, KB_ICO_DRUM, KB_ICO_VOCAL,
                                    GUITAR_ICO_BLUR, GUITAR_ICO, DRUMS_ICO_BLUR, DRUMS_ICO,
                                    MICROPHONE_ICO_BLUR, MICROPHONE_ICO, GUITARX_ICO_BLUR, GUITARX_ICO;
        }
        static String songname;
        Texture2D concrTex, concrBM, arrowTex, rustyTex;
        Texture2D stratTex, glassboxTex, glassboxBM;
        Texture2D[] greyishTex;
        Model nPadMdl, arrowMdl, nailMdl, strat, stand;
        SpriteFont sfManager, sfGuitarist, sfBassist, sfDrummer, sfSinger;
        String[][] charNames;
        ContGUIData[] contguis;
        int setIndex, songIndex;
        Texture2D texContinue;
        int slIndex = 0;
        int leader;
        bool[] finals;
        int mmenu_select = 0, menu_ticker=0;
        int counterer = 0;
        Model mQuarter, mPick, mPoD;
        Texture2D texQuarter, texPickGP, texPickTit, texPoD;
        Model mDrumsticks, mSticks;
        Texture2D texSticks, texDTDSticks, texDSticks, texTitDSticks;
        Model mTube, mCMic, mMic, mAMic;
        Texture2D texTube, texCMic, texMic, texAMic;
        Model mString, mStrap;
        Texture2D texString, texStrap1, texStrap2, texStrap3;
        String[] rockerNames;
        SpriteFont sfMenu;
        int menuSnakeRotOffset;
        bool[][] diffExists;


        Model mCurtain;
        Texture2D texCurtainLeft, texCurtainRight;

        float mmLogoTime;

        bool MenuLoading = false, MenuLoaded = false, GameLoading = false, GameLoaded = false;
#endregion

#region freestyle

        byte[] oldpressed = new byte[4];
        Texture2D[] FStexBoard = new Texture2D[4];
        bool[] FSIsLefty = new bool[4];
        int[] FSxOffset = new int[4];

#endregion

#region optimizers
        Rectangle rect256 = new Rectangle(0, 0, 256, 256);
#endregion

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
            InitXNAApp();

            currentState = new Stack<BaseState>();
            PushState(new FVLogoScreen());

#if WINDOWS

            for (int i = 0; i < lastframes.Length; i++)
                lastframes[i] = 1 / 30f;

            diffExists = new bool[4][];
            for(int i=0;i<4;i++)
                diffExists[i] = new bool[4];
            
#else
            setLists = new SetList[1];
            setLists[0] = new SetList();
            setLists[0].LoadCustom("songlist.txt");
            setLists[0].name = "Custom";
#endif
            



            
            
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

            

            SetProjMatrix(Window.ClientBounds.Width,Window.ClientBounds.Height);
            graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;
            graphics.SynchronizeWithVerticalRetrace = true;

            RenderMaster.CreateSingleton();

            RenderMaster.GetSingleton().spritebatch = new SpriteBatch(graphics.GraphicsDevice);
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
                        GameSettings.cGUIStyle = strn.ToLower().Equals("rockband") ? GameUIMaster.GUIStyle.RB : GameUIMaster.GUIStyle.UN;
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

        void SetProjMatrix(int w, int h)
        {
            RenderMaster.GetSingleton().Projection = Matrix.CreatePerspectiveFieldOfView((float)Math.PI / 4.0f,
                              w / (float)h,
                              2f, 750.0f);
        }
        
        protected override void LoadContent()
        {
            audioEngine = new AudioEngine("audio\\Win\\Unsigned.xgs");
            audioSoundBank = new SoundBank(audioEngine, "audio\\Win\\Sound Bank.xsb");
            audioWaveBank = new WaveBank(audioEngine, "audio\\Win\\Wave Bank.xwb");

            sfBassist = content.Load<SpriteFont>("fonts\\bassist");
            sfGuitarist = content.Load<SpriteFont>("fonts\\guitarist");
            sfDrummer = content.Load<SpriteFont>("fonts\\drummer");
            sfSinger = content.Load<SpriteFont>("fonts\\singer");
            sfManager = content.Load<SpriteFont>("fonts\\manager");
            sfMenu = content.Load<SpriteFont>("fonts\\menu");
            Global.texDefaultBM = content.Load<Texture2D>("graphics\\blankbm");
            Global.DefaultFont = content.Load<SpriteFont>("BasicFont");
            Global.BigFont = content.Load<SpriteFont>("fonts\\bigfont");
            Global.SmallFont = content.Load<SpriteFont>("fonts\\smallfont");
            Global.gradient = content.Load<Texture2D>("graphics\\gradient");
            gradientMask = content.Load<Texture2D>("graphics\\gradientMask");
            GameUIMaster.GetSingleton().texButtonGreen = content.Load<Texture2D>("graphics\\large_face_a");
            GameUIMaster.GetSingleton().texButtonRed = content.Load<Texture2D>("graphics\\large_face_b");
            GameUIMaster.GetSingleton().texButtonYellow = content.Load<Texture2D>("graphics\\large_face_y");
            Global.texWhite = content.Load<Texture2D>("graphics\\white");

            
        }

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
                if (SongListRT == null)
                {
                    SongListRT = new RenderTarget2D(graphics.GraphicsDevice, 512, 512, 1, SurfaceFormat.Color);
                }
                SongListBG = content.Load<Texture2D>("graphics\\songlist");
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
                nPadTex = new Texture2D[4];
                nPadTex[0] = content.Load<Texture2D>("graphics\\paper1");
                nPadTex[1] = content.Load<Texture2D>("graphics\\paper2");
                nPadTex[2] = content.Load<Texture2D>("graphics\\paper3");
                nPadTex[3] = content.Load<Texture2D>("graphics\\paper4");
                coolbg1 = content.Load<Texture2D>("graphics\\coolbg1");
                coolbg2 = content.Load<Texture2D>("graphics\\coolbg2");
                texNote = new Texture2D[4];
                texSnakeSkin = content.Load<Texture2D>("graphics\\snake");
                mSnake = content.Load<Model>("meshes\\snake");
                rtSnake = new RenderTarget2D(graphics.GraphicsDevice, 512, 128, 1, SurfaceFormat.Color);
                resultsScroller = content.Load<Texture2D>("graphics\\resultscroller");
                failbg = content.Load<Texture2D>("graphics\\faildialog");
                concrTex = content.Load<Texture2D>("graphics\\concr");
                concrBM = content.Load<Texture2D>("graphics\\concrBM");
                waves = content.Load<Texture2D>("graphics\\waves");
                wave = content.Load<Texture2D>("graphics\\wave");
                texHeader = content.Load<Texture2D>("graphics\\header");
                glassboxTex = content.Load<Texture2D>("graphics\\glasscase");
                glassboxBM = content.Load<Texture2D>("graphics\\glasscasebm");
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
                nPadMdl = content.Load<Model>("meshes\\paper1");
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
                flames = new Vector3[4][];
                flames[0] = new Vector3[100];
                flames[1] = new Vector3[100];
                flames[2] = new Vector3[100];
                flames[3] = new Vector3[100];
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
                hairl = content.Load<Texture2D>("graphics\\hairl");
                hairr = content.Load<Texture2D>("graphics\\hairr");
                flameTex = content.Load<Texture2D>("graphics\\flame");
                texContinue = content.Load<Texture2D>("graphics\\continue");
                GC.Collect();
                MenuLoaded = true;
                MenuLoading = false;
            };
            Thread myThread = new Thread(ThreadStarter);
            myThread.Start();
                

        }*/

        protected override void Update(GameTime gameTime)
        {
            //Thread.Sleep(1);
            GameSettings.windowheight = graphics.GraphicsDevice.Viewport.Height;
            GameSettings.windowwidth = graphics.GraphicsDevice.Viewport.Width;
            audioEngine.Update();

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

        private void GetGamepadStates(bool hardWork)
        {
            controllers[0] = GamePad.GetState(PlayerIndex.One);
            controllers[1] = GamePad.GetState(PlayerIndex.Two);
            controllers[2] = GamePad.GetState(PlayerIndex.Three);
            controllers[3] = GamePad.GetState(PlayerIndex.Four);
            if (hardWork)
            {
                contCapabilities[0] = GamePad.GetCapabilities(PlayerIndex.One);
                contCapabilities[1] = GamePad.GetCapabilities(PlayerIndex.Two);
                contCapabilities[2] = GamePad.GetCapabilities(PlayerIndex.Three);
                contCapabilities[3] = GamePad.GetCapabilities(PlayerIndex.Four);
            }
        }

        

        public int GetScore()
        {
            int total = 0;
            for (int i = 0; i < 4; i++)
                if (boards[i] != null)
                {
                    total += boards[i].GetScore();
                }
            return total;
        }

        

        public void InitForSong(bool guitarist, bool vocalist, bool percussionist, bool bassist, byte[] difficulty, String venueStr, UnsignedGame gameRef) 
        {
            if (guitarist && bassist && percussionist && vocalist)
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
            }

            venue = new Venue(venueStr + ".gbw", songname, this, content, graphics, engine);
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

        public static int AddBits(byte ind)
        {
            int ret=0;
            for (int i = 1; i <= 128; i *= 2)
                if ((ind & i) > 0)
                    ret++;
            return ret;
        }

        private void TogglePause()
        {
            if(pausetimer>0)
                return;
            if (!IsPaused)
            {
                IsPaused = true;
                pauseTextDisp = new String[4];
                pauseTextDisp[0] = "Continue";
                pauseTextDisp[1] = "Retry";
                pauseTextDisp[2] = "Options";
                pauseTextDisp[3] = "Exit";
                song.pause();
            }
            else
            {
                IsPaused = false;
                song.resume((long)(CurrentTime / (TicksPerSecond / 1000)));
            }
            pausetimer = 200;
        }

        private void ApplyPauseOption()
        {
            if (!IsPaused)
                return;
            if (pauseTextDisp[0].Equals("Continue"))
            {
                if (pauseSelected == 0)
                    TogglePause();
                else if (pauseSelected == 1)
                { TogglePause(); }//RestartSong(); }
                else if (pauseSelected == 2)
                {
                    pauseTextDisp = new string[2];
                    pauseTextDisp[0] = "Lefty: " + (boards[pauseSelectOwner].IsLefty() ? "On" : "Off");
                    pauseTextDisp[1] = "Back";
                }
                else if (pauseSelected == 3)
                { totalresults = new Results[0]; screen = S_MAINMENU; songname = ""; song = null; boards = null; boardsTarget = null; started = 0; menu_ticker = 200; mmenu_select = 0; contguis = new ContGUIData[5]; UnloadGameContent(); MenuLoaded = false; }
            }
            else if (pauseTextDisp[1].Equals("Back"))
            {
                if (pauseSelected == 0)
                { boards[pauseSelected].ToggleLefty(); pauseTextDisp[0] = "Lefty: " + (boards[pauseSelectOwner].IsLefty() ? "On" : "Off"); }
                else if (pauseSelected == 1)
                {
                    pauseTextDisp = new String[4];
                    pauseTextDisp[0] = "Continue";
                    pauseTextDisp[1] = "Retry";
                    pauseTextDisp[2] = "Options";
                    pauseTextDisp[3] = "Exit";
                }
            }
        }

        public void Hurt(int ind)
        {
            if (boards[ind].GetDifficulty() == D_EASY)
            {
                if (rockMeterLevel[ind] > 80)
                    rockMeterLevel[ind] -= 1;//1f;
                else if (rockMeterLevel[ind] > 20)
                    rockMeterLevel[ind] -= 0.75f;//0.75f;
                else
                    rockMeterLevel[ind] -= 0.5f;// 0.5f;
            }
            else if (boards[ind].GetDifficulty() == D_MEDIUM)
            {
                if (rockMeterLevel[ind] > 80)
                    rockMeterLevel[ind] -= 2;//1f;
                else if (rockMeterLevel[ind] > 20)
                    rockMeterLevel[ind] -= 1f;//0.75f;
                else
                    rockMeterLevel[ind] -= 0.5f;// 0.5f;
            }
            else if (boards[ind].GetDifficulty() == D_HARD)
            {
                if (rockMeterLevel[ind] > 80)
                    rockMeterLevel[ind] -= 3;//1f;
                else if (rockMeterLevel[ind] > 20)
                    rockMeterLevel[ind] -= 1.5f;//0.75f;
                else
                    rockMeterLevel[ind] -= 1f;// 0.5f;
            }
            else if (boards[ind].GetDifficulty() == D_EXPERT)
            {
                if (rockMeterLevel[ind] > 80)
                    rockMeterLevel[ind] -= 4;//1f;
                else if (rockMeterLevel[ind] > 20)
                    rockMeterLevel[ind] -= 3f;//0.75f;
                else
                    rockMeterLevel[ind] -= 2f;// 0.5f;
            }
        }

        public void Help(int ind)
        {
            if (boards[ind].GetDifficulty() == D_EASY)
            {
                if (rockMeterLevel[ind] > 80)
                    rockMeterLevel[ind] += 1 * (boards[ind].IsSPActivated() ? 10 : 1);
                else
                    rockMeterLevel[ind] += 4f * (boards[ind].IsSPActivated() ? 10 : 1);
            }
            else if (boards[ind].GetDifficulty() == D_MEDIUM)
            {
                if (rockMeterLevel[ind] > 80)
                    rockMeterLevel[ind] += 1 * (boards[ind].IsSPActivated() ? 10 : 1);
                else
                    rockMeterLevel[ind] += 3f * (boards[ind].IsSPActivated() ? 10 : 1);
            }
            else if (boards[ind].GetDifficulty() == D_HARD)
            {
                if (rockMeterLevel[ind] > 80)
                    rockMeterLevel[ind] += 0.75f * (boards[ind].IsSPActivated() ? 10 : 1);
                else
                    rockMeterLevel[ind] += 2f * (boards[ind].IsSPActivated() ? 10 : 1);
            }
            if (boards[ind].GetDifficulty() == D_EXPERT)
            {
                if (rockMeterLevel[ind] > 80)
                    rockMeterLevel[ind] += 0.25f * (boards[ind].IsSPActivated() ? 10 : 1);
                else
                    rockMeterLevel[ind] += 1f * (boards[ind].IsSPActivated() ? 10 : 1);
            }
        }

        #region shards
        private static Vector3[] shardmethlist = { new Vector3(-.5f,0f,0f),new Vector3(-.5f,1f,0f),new Vector3(-.5f,.5f,.5f),new Vector3(-.5f,.5f,-.5f),new Vector3(.5f,0f,0f),new Vector3(.5f,1f,0f),new Vector3(.5f,.5f,.5f),new Vector3(.5f,.5f,-.5f)};
        public void AddShards(byte note, int board)
        {
            int lefty = 1;
            if (boards[board].IsLefty())
                lefty = -1;
            int count = 0;
            int noteage = 0;
            for(noteage=0;noteage<5;noteage++)
                if((note&bits[noteage])>0)
                    break;
            Random r = new Random((int)(DateTime.Now.Ticks/1000));
            int k=board;
            
                for (int i = 0; i < glass[k].Length; i++)
                {
                    if (noteage >= 4 && boards[board].GetBoardType() == PERCUSSIONIST)
                        break;
                    if (glass[k][i].scale <= 0)
                    {
                        glass[k][i].scale = 0.5f;
                        glass[k][i].dir = new Vector3(((float)(r.NextDouble()) * 2) - 1, ((float)(r.NextDouble()) * 1.5f) - 1, (float)(r.NextDouble() * 15)) * 0.2f;
                        glass[k][i].rot = new Vector3((float)(r.NextDouble() * Math.PI * 2), (float)(r.NextDouble() * Math.PI * 2), (float)(r.NextDouble() * Math.PI * 2));
                        glass[k][i].col = noteage;
                        glass[k][i].frame = r.Next(5);
                        Vector3 rval = Vector3.Transform(new Vector3(0, 0, -Board.zeroZ), Matrix.CreateRotationX(Board.rotate));
                        if (boards[board].GetBoardType() != PERCUSSIONIST)
                            glass[k][i].loc.X = (-.8f + (noteage * 0.4f)) * Board.width * lefty - boards[board].GetXOffset();
                        else
                            glass[k][i].loc.X = (-.75f + (Board.drumsToGuitar[noteage] * 0.5f)) * Board.width - boards[board].GetXOffset();
                        glass[k][i].loc.Y = Board.height + rval.Y;
                        glass[k][i].loc.Z = rval.Z;
                        glass[k][i].loc.X += (float)(r.NextDouble() - 0.5) * (Board.width / (boards[board].GetBoardType() == PERCUSSIONIST ? 4 : 5));
                        glass[k][i].loc.Y += (float)(r.NextDouble() - 0.5) * 0.3f;
                        count++;
                        if (count >= 16)
                        {
                            count = 0;
                            noteage++;
                            for (; noteage < 5; noteage++)
                                if ((note & bits[noteage]) > 0)
                                    break;
                            if (noteage >= 5)
                                return;
                        }
                    }
                }
            
        }
        public void AddSparks(byte note, int board)
        {
            int sparksToAdd = 0;
            for (int i = 0; i < 5; i++)
                if ((note & (1 << i)) != 0)
                    sparksToAdd += 16;
            int lefty = 1;
            if (boards[board].IsLefty())
                lefty = -1;
            int count = 0;
            int noteage = 0;
            for (noteage = 0; noteage < 5; noteage++)
                if ((note & bits[noteage]) > 0)
                    break;
            Random r = new Random((int)(DateTime.Now.Ticks / 1000));
            int k = board;
            for (int i = 0; i < sparks[k].Length; i++)
            {
                if (noteage >= 4 && boards[board].GetBoardType() == PERCUSSIONIST)
                    break;
                if (sparks[k][i].scale <= 0)
                {
                    sparks[k][i].scale = 0.25f + (float)r.NextDouble();
                    sparks[k][i].dir = new Vector3(((float)(r.NextDouble()) * 2) - 1, ((float)(r.NextDouble()) * 10f) + 15f, (float)(r.NextDouble() * 15)) * 0.2f;
                    sparks[k][i].col = noteage;
                    Vector3 rval = Vector3.Transform(new Vector3(0, 0, -Board.zeroZ), Matrix.CreateRotationX(Board.rotate));
                    if (boards[board].GetBoardType() != PERCUSSIONIST)
                        sparks[k][i].loc.X = (-.8f + (noteage * 0.4f)) * Board.width * lefty;
                    else
                        sparks[k][i].loc.X = (-.75f + (Board.drumsToGuitar[noteage] * 0.5f)) * Board.width;
                    sparks[k][i].loc.Y = Board.height + rval.Y;
                    sparks[k][i].loc.Z = rval.Z;
                    sparks[k][i].loc.X += (float)(r.NextDouble() - 0.5) * (Board.width / (boards[board].GetBoardType() == PERCUSSIONIST ? 4 : 5));
                    sparks[k][i].loc.Y += (float)(r.NextDouble() - 0.5) * 0.3f;
                    count++;
                    if (count < sparksToAdd)
                    {
                        while(true)
                        {
                            noteage++;
                            if (noteage >= 5)
                                noteage = 0;
                            if ((note & bits[noteage]) > 0)
                                break;
                        }
                    }
                    else return;
                }
            }
        }
        public void AddShortSparks(byte note, int board)
        {
            int sparksToAdd = 0;
            for(int i=0;i<5;i++)
                if((note&(1<<i))!=0)
                    sparksToAdd+=8;
            int lefty = 1;
            if (boards[board].IsLefty())
                lefty = -1;
            int count = 0;
            int noteage = 0;
            for (noteage = 0; noteage < 5; noteage++)
                if ((note & bits[noteage]) > 0)
                    break;
            Random r = new Random((int)(DateTime.Now.Ticks / 1000));
            int k = board;
            for (int i = 0; i < sparks[k].Length; i++)
            {
                if (noteage >= 4 && boards[board].GetBoardType() == PERCUSSIONIST)
                    break;
                if (sparks[k][i].scale <= 0)
                {
                    sparks[k][i].scale = 0.25f + (float)r.NextDouble();
                    sparks[k][i].dir = new Vector3(((float)(r.NextDouble()) * 40) - 20, ((float)(r.NextDouble()) * 20f), ((float)(r.NextDouble()) * 20) - 10) * 0.05f;
                    sparks[k][i].col = noteage;
                    Vector3 rval = Vector3.Transform(new Vector3(0, 0, -Board.zeroZ), Matrix.CreateRotationX(Board.rotate));
                    if (boards[board].GetBoardType() != PERCUSSIONIST)
                        sparks[k][i].loc.X = (-.8f + (noteage * 0.4f)) * Board.width * lefty;
                    else
                        sparks[k][i].loc.X = (-.75f + (Board.drumsToGuitar[noteage] * 0.5f)) * Board.width;
                    sparks[k][i].loc.Y = Board.height + rval.Y;
                    sparks[k][i].loc.Z = rval.Z;
                    sparks[k][i].loc.X += (float)(r.NextDouble() - 0.5) * (Board.width / (boards[board].GetBoardType() == PERCUSSIONIST ? 4 : 5));
                    sparks[k][i].loc.Y += (float)(r.NextDouble() - 0.5) * 0.3f;
                    count++;
                    if (count < sparksToAdd)
                    {
                        while(true)
                        {
                            noteage++;
                            if (noteage >= 5)
                                noteage = 0;
                            if ((note & bits[noteage]) > 0)
                                break;
                        }
                    }
                    else
                        return;
                }
            }
        }
        public void AddSparksFS(byte note, int board)
        {
            int lefty = 1;
            if (FSIsLefty[board])
                lefty = -1;
            int count = 0;
            int noteage = 0;
            for (noteage = 0; noteage < 5; noteage++)
                if ((note & bits[noteage]) > 0)
                    break;
            Random r = new Random((int)(DateTime.Now.Ticks / 1000));
            int k = board;
            for (int i = 0; i < sparks[k].Length; i++)
            {
                if (noteage >= 4 && board==2)
                    break;
                if (sparks[k][i].scale <= 0)
                {
                    sparks[k][i].scale = 0.25f + (float)r.NextDouble();
                    sparks[k][i].dir = new Vector3(((float)(r.NextDouble()) * 2) - 1, ((float)(r.NextDouble()) * 10f) + 15f, (float)(r.NextDouble() * 15)) * 0.2f;
                    sparks[k][i].col = noteage;
                    Vector3 rval = Vector3.Transform(new Vector3(0, 0, -Board.zeroZ), Matrix.CreateRotationX(Board.rotate));
                    if (board!=2)
                        sparks[k][i].loc.X = (-.8f + (noteage * 0.4f)) * Board.width * lefty - FSxOffset[board];
                    else
                        sparks[k][i].loc.X = (-.75f + (Board.drumsToGuitar[noteage] * 0.5f)) * Board.width - FSxOffset[board];
                    sparks[k][i].loc.Y = Board.height + rval.Y;
                    sparks[k][i].loc.Z = rval.Z;
                    sparks[k][i].loc.X += (float)(r.NextDouble() - 0.5) * (Board.width / (board == 2 ? 4 : 5));
                    sparks[k][i].loc.Y += (float)(r.NextDouble() - 0.5) * 0.3f;
                    count++;
                    if (count >= 16)
                    {
                        count = 0;
                        noteage++;
                        for (; noteage < 5; noteage++)
                            if ((note & bits[noteage]) > 0)
                                break;
                        if (noteage >= 5)
                            return;
                    }
                }
            }
        }
        public void AddLesserSparks(byte note, int board)
        {
            int sparksToAdd = 0;
            for (int i = 0; i < 5; i++)
                if ((note & (1 << i)) != 0)
                    sparksToAdd += 8;
            int lefty = 1;
            if (boards[board].IsLefty())
                lefty = -1;
            int count = 0;
            int noteage = 0;
            for (noteage = 0; noteage < 5; noteage++)
                if ((note & bits[noteage]) > 0)
                    break;
            Random r = new Random((int)(DateTime.Now.Ticks / 1000));
            int k = board;
            for (int i = 0; i < sparks.Length; i++)
            {
                if (noteage >= 4 && boards[board].GetBoardType() == PERCUSSIONIST)
                    break;
                if (sparks[k][i].scale <= 0)
                {
                    sparks[k][i].scale = 0.25f + (float)r.NextDouble();
                    sparks[k][i].dir = new Vector3(((float)(r.NextDouble()) * 4) - 2, ((float)(r.NextDouble()) * 15f) + 5f, (float)(r.NextDouble() * 4)) * 0.2f;
                    sparks[k][i].col = noteage;
                    Vector3 rval = Vector3.Transform(new Vector3(0, 0, -Board.zeroZ), Matrix.CreateRotationX(Board.rotate));
                    if (boards[board].GetBoardType() != PERCUSSIONIST)
                        sparks[k][i].loc.X = (-.8f + (noteage * 0.4f)) * Board.width * lefty - boards[board].GetXOffset();
                    else
                        sparks[k][i].loc.X = (-.75f + (Board.drumsToGuitar[noteage] * 0.5f)) * Board.width - boards[board].GetXOffset();
                    sparks[k][i].loc.Y = Board.height + rval.Y;
                    sparks[k][i].loc.Z = rval.Z;
                    sparks[k][i].loc.X += (float)(r.NextDouble() - 0.5) * (Board.width / (boards[board].GetBoardType() == PERCUSSIONIST ? 4 : 5));
                    sparks[k][i].loc.Y += (float)(r.NextDouble() - 0.5) * 0.3f;
                    count++;
                    if (count < sparksToAdd)
                    {
                        while (true)
                        {
                            noteage++;
                            if (noteage >= 5)
                                noteage = 0;
                            if ((note & bits[noteage]) > 0)
                                break;
                        }
                    }
                    else
                        return;
                }
            }
        }
        #endregion

        private void UpdateGibs(GameTime gameTime)
        {
            for (int k = 0; k < glass.Length; k++)
            {
                for (int i = 0; i < glass[k].Length; i++)
                {
                    if (glass[k][i].scale > 0)
                    {
                        glass[k][i].scale -= (float)gameTime.ElapsedGameTime.Milliseconds / 2000f;
                        glass[k][i].dir.Y -= (float)gameTime.ElapsedGameTime.Milliseconds / 1000f;
                        glass[k][i].loc += glass[k][i].dir * (float)gameTime.ElapsedGameTime.Milliseconds * 0.001f;
                    }
                }
                for (int i = 0; i < sparks[k].Length; i++)
                {
                    sparks[k][i].scale = Math.Min(sparks[k][i].dir.Y, 1) * 4;
                    sparks[k][i].dir.Y -= (float)gameTime.ElapsedGameTime.Milliseconds / 100f;
                    if (sparks[k][i].scale > 0)
                        sparks[k][i].loc += sparks[k][i].dir * (float)gameTime.ElapsedGameTime.Milliseconds * 0.001f;
                }
            }
        }
        private void DrawGibs(int board)
        {
            int k = board;
            for (int r = 0; r < texShard.Length; r++)
            {
                engine.Parameters["diffuseTexture"].SetValue(texShard[r]);
                for (int i = 0; i < glass[k].Length; i++)
                    if (glass[k][i].scale > 0 && glass[k][i].frame==r)
                    {
                        Matrix matIdentity, matTransl, matScale, matOrbit;
                        matIdentity = Matrix.Identity;
                        matTransl = Matrix.CreateTranslation(glass[k][i].loc);
                        matOrbit = Matrix.CreateRotationX(glass[k][i].rot.X) * Matrix.CreateRotationY(glass[k][i].rot.Y) * Matrix.CreateRotationZ(glass[k][i].rot.Z);
                        matScale = Matrix.CreateScale((new Vector3(0.1f, 0.1f, 0.1f)) * glass[k][i].scale);

                        // identity, scale, rotate, orbit(translate & rotate), translate
                        engine.Parameters["world"].SetValue(matIdentity * matScale * matOrbit * matTransl);

                        engine.Parameters["proj"].SetValue(matProj);


                        
                        engine.Parameters["diffuseColor"].SetValue(FretColorsV4[glass[k][i].col]);
                        engine.CommitChanges();

                        // 5: draw object - select vertex type, primitive type, # of primitives
                        graphics.GraphicsDevice.VertexDeclaration = vd;
                        graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                        graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                        graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                        graphics.GraphicsDevice.Vertices[0].SetSource(square, 0, GBVertexFormat.SizeInBytes);
                        graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
                        graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                    }
            }
            engine.Parameters["diffuseTexture"].SetValue(texSpark);
            for (int i = 0; i < sparks[k].Length; i++)
                if (sparks[k][i].scale > 0)
                {
                    Matrix matRot, matTransl, matScale;
                    matRot = Matrix.CreateRotationX(MathHelper.PiOver2) * Matrix.CreateRotationY((float)(hvdistTOdir(venue.GetCamFor().X, venue.GetCamFor().Z) / 180 * Math.PI) + MathHelper.PiOver2);
                    matTransl = Matrix.CreateTranslation(sparks[k][i].loc);
                    matScale = Matrix.CreateScale(new Vector3(0.01f, 0.01f, 0.01f) * sparks[k][i].scale);

                    // identity, scale, rotate, orbit(translate & rotate), translate
                    engine.Parameters["world"].SetValue(matScale * matRot * matTransl);

                    engine.Parameters["diffuseColor"].SetValue(FretColorsV4[sparks[k][i].col]);
                    engine.CommitChanges();

                    // 5: draw object - select vertex type, primitive type, # of primitives
                    graphics.GraphicsDevice.VertexDeclaration = vd;
                    graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                    graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                    graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                    graphics.GraphicsDevice.Vertices[0].SetSource(square, 0, GBVertexFormat.SizeInBytes);
                    graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
                    graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                }
            engine.Parameters["diffuseColor"].SetValue(new Vector4(0.8f, 0.8f, 0.8f, 1.0f));
        }

        private void ProcessInput(GameTime gameTime, long currenttime)
        {
            byte er = 0;
            for (int i = 0; i < 4; i++)
                if (boards[i] != null)
                {
                    if (i == 0 || i==3)
                    {
                        byte pressed = 0;
                        bool up = false, down = false;
                        if (contInput[i] < 4)
                        {
                            if (controllers[contInput[i]].Buttons.A == ButtonState.Pressed)
                                pressed |= bits[boards[i].IsLefty()?4:0];
                            if (controllers[contInput[i]].Buttons.B == ButtonState.Pressed)
                                pressed |= bits[boards[i].IsLefty()?3:1];
                            if (controllers[contInput[i]].Buttons.Y == ButtonState.Pressed)
                                pressed |= bits[2];
                            if (controllers[contInput[i]].Buttons.X == ButtonState.Pressed)
                                pressed |= bits[boards[i].IsLefty()?1:3];
                            if (controllers[contInput[i]].Buttons.LeftShoulder == ButtonState.Pressed)
                                pressed |= bits[boards[i].IsLefty()?0:4];
                            if (controllers[contInput[i]].DPad.Down == ButtonState.Pressed)
                                down = true;
                            if (controllers[contInput[i]].DPad.Up == ButtonState.Pressed)
                                up = true;
                            if (controllers[contInput[i]].ThumbSticks.Right.Y > 0.95f || controllers[contInput[i]].Buttons.Back == ButtonState.Pressed)
                                StarPowerAction(i);
                            if (controllers[contInput[i]].Buttons.Start == ButtonState.Pressed)
                            { TogglePause(); pauseSelectOwner = i; }
                            boards[i].Whammy(controllers[contInput[i]].ThumbSticks.Right.X, currenttime, gameTime, song.GetBeatLength());
                        }
                        else if (contInput[i] == 4)
                        {
                            KeyboardState kbst = Keyboard.GetState();
                            if (kbst.IsKeyDown(Keys.G))
                                pressed |= bits[boards[i].IsLefty()?0:4];
                            if (kbst.IsKeyDown(Keys.F))
                                pressed |= bits[boards[i].IsLefty()?1:3];
                            if (kbst.IsKeyDown(Keys.D))
                                pressed |= bits[2];
                            if (kbst.IsKeyDown(Keys.S))
                                pressed |= bits[boards[i].IsLefty()?3:1];
                            if (kbst.IsKeyDown(Keys.A))
                                pressed |= bits[boards[i].IsLefty()?4:0];
                            if (kbst.IsKeyDown(Keys.Down))
                                down = true;
                            if (kbst.IsKeyDown(Keys.Up))
                                up = true;
                            if (kbst.IsKeyDown(Keys.RightShift) || kbst.IsKeyDown(Keys.NumPad0) || kbst.IsKeyDown(Keys.Insert) || kbst.IsKeyDown(Keys.D0))
                                StarPowerAction(i);
                            if (kbst.IsKeyDown(Keys.Escape) || kbst.IsKeyDown(Keys.Back))
                            { TogglePause(); pauseSelectOwner = i; }
                            boards[i].Whammy(kbst.IsKeyDown(Keys.Left)?1.0f:-1.0f, currenttime, gameTime, song.GetBeatLength());
                        }
                        if ((pressed & 1) == 0)
                        {
                            if (i == 0)
                                guitarGreen = false;
                            else //if i==3
                                bassGreen = false;
                        }
                        if (!IsPaused)
                        {
                            if (i == 0)
                            {
                                if (down && guitarStrum != 1)
                                { boards[i].Strum(0, currenttime, this, i); guitarStrum = 1; }
                                else if (up && guitarStrum != 2)
                                { boards[i].Strum(0, currenttime, this, i); guitarStrum = 2; }
                                else if (!up && !down)
                                    guitarStrum = 0;
                            }
                            else
                            {
                                if (down && bassStrum != 1)
                                { boards[i].Strum(0, currenttime, this, i); bassStrum = 1; }
                                else if (up && bassStrum != 2)
                                { boards[i].Strum(0, currenttime, this, i); bassStrum = 2; }
                                else if (!up && !down)
                                    bassStrum = 0;
                            }
                            er = boards[i].Update(gameTime, currenttime, this, i, pressed);
                        }
                        else if(i==pauseSelectOwner)
                        {
                            if (i == 0)
                            {
                                if (down && guitarStrum != 1)
                                { pauseSelected++; guitarStrum = 1; }
                                else if (up && guitarStrum != 2)
                                { pauseSelected--; guitarStrum = 2; }
                                else if (!up && !down)
                                    guitarStrum = 0;
                            }
                            else
                            {
                                if (down && bassStrum != 1)
                                { pauseSelected++; bassStrum = 1; }
                                else if (up && bassStrum != 2)
                                { pauseSelected--; bassStrum = 2; }
                                else if (!up && !down)
                                    bassStrum = 0;
                            }
                            if (pauseSelected < 0)
                                pauseSelected = 0;
                            else if (pauseSelected >= pauseTextDisp.Length)
                                pauseSelected = pauseTextDisp.Length - 1;
                            if ((pressed & 1) != 0 && !guitarGreen && i == 0)
                            { ApplyPauseOption(); guitarGreen = true; }
                            if ((pressed & 1) != 0 && !bassGreen && i == 3)
                            { ApplyPauseOption(); bassGreen = true; }
                        }
                    }
                    else if (i == 2)
                    {
                        byte pressed = 0;
                        if (contInput[2] < 4)
                        {
                            if (controllers[contInput[i]].Buttons.A == ButtonState.Pressed)
                                pressed |= 1;
                            if (controllers[contInput[i]].Buttons.B == ButtonState.Pressed)
                                pressed |= 2;
                            if (controllers[contInput[i]].Buttons.Y == ButtonState.Pressed)
                                pressed |= 4;
                            if (controllers[contInput[i]].Buttons.X == ButtonState.Pressed)
                                pressed |= 8;
                            if (controllers[contInput[i]].Buttons.LeftShoulder == ButtonState.Pressed)
                                pressed |= 16;
                            if (controllers[contInput[i]].Buttons.Start == ButtonState.Pressed)
                            { TogglePause(); pauseSelectOwner = i; }
                        }
                        else if (contInput[2] == 4)
                        {
                            KeyboardState kbst = Keyboard.GetState();
                            if (kbst.IsKeyDown(Keys.F))
                                pressed |= 1;
                            if (kbst.IsKeyDown(Keys.A))
                                pressed |= 2;
                            if (kbst.IsKeyDown(Keys.S))
                                pressed |= 4;
                            if (kbst.IsKeyDown(Keys.D))
                                pressed |= 8;
                            if (kbst.IsKeyDown(Keys.Space))
                                pressed |= 16;
                            if (kbst.IsKeyDown(Keys.Escape) || kbst.IsKeyDown(Keys.Back))
                            { TogglePause(); pauseSelectOwner = i; }
                        }
                        if (pressed != 0)
                        {
                            byte e = boards[2].Bang(pressed, currenttime, this);
                            if (e > 0)
                            {
                                if ((e & bits[7]) != 0)
                                {
                                    AddSparks(e, 2);
                                    if ((e & bits[0]) != 0)
                                        audioSoundBank.PlayCue("crash");
                                    if ((e & bits[1]) != 0)
                                        audioSoundBank.PlayCue("snare");
                                    if ((e & bits[2]) != 0)
                                        audioSoundBank.PlayCue("tom1");
                                    if ((e & bits[3]) != 0)
                                        audioSoundBank.PlayCue("tom2");
                                    if ((e & bits[4]) != 0)
                                        audioSoundBank.PlayCue("bass");
                                }
                                else
                                    AddShards(e, 2);
                            }
                        }
                        if ((pressed & 2) == 0)
                            drumsGreen = false;

                        if(!IsPaused)
                            er=boards[i].Update(gameTime, currenttime, this, i, pressed);
                        else if (i == pauseSelectOwner)
                        {
                            bool up = (pressed & 4) != 0, down = (pressed & 8) != 0;
                            if (down && drumsStrum != 1)
                            { pauseSelected++; drumsStrum = 1; }
                            else if (up && drumsStrum != 2)
                            { pauseSelected--; drumsStrum = 2; }
                            else if (!up && !down)
                                drumsStrum = 0;
                            if (pauseSelected < 0)
                                pauseSelected = 0;
                            else if (pauseSelected >= pauseTextDisp.Length)
                                pauseSelected = pauseTextDisp.Length - 1;

                            if ((pressed & 1) != 0 && !drumsGreen)
                            { ApplyPauseOption(); drumsGreen = true; }
                        }
                    }
                    else if(!IsPaused)
                        er=boards[i].Update(gameTime,currenttime, this, i, 0);
                    if (er > 0)
                    {
                        if ((er & bits[7]) != 0)
                            AddSparks(er, i);
                        else
                            AddShards(er, i);
                    }
                }

            

            /*if (IsPaused)
                return;

            if (instruments[0])
            {
                if(contInput[0]<4)
                {
                    if (controllers[contInput[0]].DPad.Down == ButtonState.Pressed && guitarStrum != 1)
                    {
                        guitarStrum = 1;
                        byte pressed = 0;
                        if (controllers[contInput[0]].Buttons.A == ButtonState.Pressed)
                            pressed |= 1;
                        if (controllers[contInput[0]].Buttons.B == ButtonState.Pressed)
                            pressed |= 2;
                        if (controllers[contInput[0]].Buttons.Y == ButtonState.Pressed)
                            pressed |= 4;
                        if (controllers[contInput[0]].Buttons.X == ButtonState.Pressed)
                            pressed |= 8;
                        if (controllers[contInput[0]].Buttons.LeftShoulder == ButtonState.Pressed)
                            pressed |= 16;


                        byte e = boards[0].Strum(pressed, currenttime,this,0);
                        if (e > 0)
                        {
                            Help(0);
                            if ((e & bits[7]) != 0)
                                AddSparks(e, 0);
                            else
                                AddShards(e, 0);
                        }
                        else
                            Hurt(0);
                    }
                    if (controllers[contInput[0]].DPad.Up == ButtonState.Pressed && guitarStrum != 2)
                    {
                        guitarStrum = 2;
                        byte pressed = 0;
                        if (controllers[contInput[0]].Buttons.A == ButtonState.Pressed)
                            pressed |= 1;
                        if (controllers[contInput[0]].Buttons.B == ButtonState.Pressed)
                            pressed |= 2;
                        if (controllers[contInput[0]].Buttons.Y == ButtonState.Pressed)
                            pressed |= 4;
                        if (controllers[contInput[0]].Buttons.X == ButtonState.Pressed)
                            pressed |= 8;
                        if (controllers[contInput[0]].Buttons.LeftShoulder == ButtonState.Pressed)
                            pressed |= 16;
                        byte e = boards[0].Strum(pressed, currenttime,this,0);
                        if (e > 0)
                        {
                            Help(0);
                            if ((e & bits[7]) != 0)
                                AddSparks(e, 0);
                            else
                                AddShards(e, 0);
                        }
                        else
                            Hurt(0);
                    }
                    if (controllers[contInput[0]].DPad.Up == ButtonState.Released &&
                       controllers[contInput[0]].DPad.Down == ButtonState.Released)
                        guitarStrum = 0;
                }
                else if (contInput[0] == 4)
                {
                    if (Keyboard.GetState().IsKeyDown(Keys.Down) && guitarStrum == 0)
                    {
                        guitarStrum = 1;
                        byte pressed = 0;
                        if (Keyboard.GetState().IsKeyDown(Keys.G))
                            pressed |= bits[boards[0].IsLefty()?0:4];
                        if (Keyboard.GetState().IsKeyDown(Keys.F))
                            pressed |= bits[boards[0].IsLefty()?1:3];
                        if (Keyboard.GetState().IsKeyDown(Keys.D))
                            pressed |= bits[2];
                        if (Keyboard.GetState().IsKeyDown(Keys.S))
                            pressed |= bits[boards[0].IsLefty()?3:1];
                        if (Keyboard.GetState().IsKeyDown(Keys.A))
                            pressed |= bits[boards[0].IsLefty()?4:0];

                        byte e = boards[0].Strum(pressed, currenttime,this,0);
                        if (e > 0)
                        {
                            Help(0);
                            if ((e & bits[7]) != 0)
                                AddSparks(e, 0);
                            else
                                AddShards(e, 0);
                        }
                        else
                            Hurt(0);
                    }
                    if (Keyboard.GetState().IsKeyDown(Keys.Up) && guitarStrum == 0)
                    {
                        guitarStrum = 2;
                        byte pressed = 0;
                        if (Keyboard.GetState().IsKeyDown(Keys.G))
                            pressed |= bits[boards[0].IsLefty()?0:4];
                        if (Keyboard.GetState().IsKeyDown(Keys.F))
                            pressed |= bits[boards[0].IsLefty()?1:3];
                        if (Keyboard.GetState().IsKeyDown(Keys.D))
                            pressed |= bits[2];
                        if (Keyboard.GetState().IsKeyDown(Keys.S))
                            pressed |= bits[boards[0].IsLefty()?3:1];
                        if (Keyboard.GetState().IsKeyDown(Keys.A))
                            pressed |= bits[boards[0].IsLefty()?4:0];
                        byte e = boards[0].Strum(pressed, currenttime,this,0);
                        if (e > 0)
                        {
                            Help(0);
                            if ((e & bits[7]) != 0)
                                AddSparks(e, 0);
                            else
                                AddShards(e, 0);
                        }
                        else
                            Hurt(0);
                    }
                    if (!Keyboard.GetState().IsKeyDown(Keys.Up) &&
                        !Keyboard.GetState().IsKeyDown(Keys.Down))
                        guitarStrum = 0;
                }
            }
            if (instruments[3])
            {
                if(contInput[3]<4)
                {
                    if (controllers[contInput[3]].DPad.Down == ButtonState.Pressed && bassStrum != 1)
                    {
                        bassStrum = 1;
                        byte pressed = 0;
                        if (controllers[contInput[3]].Buttons.A == ButtonState.Pressed)
                            pressed |= 1;
                        if (controllers[contInput[3]].Buttons.B == ButtonState.Pressed)
                            pressed |= 2;
                        if (controllers[contInput[3]].Buttons.Y == ButtonState.Pressed)
                            pressed |= 4;
                        if (controllers[contInput[3]].Buttons.X == ButtonState.Pressed)
                            pressed |= 8;
                        if (controllers[contInput[3]].Buttons.LeftShoulder == ButtonState.Pressed)
                            pressed |= 16;

                        byte e = boards[3].Strum(pressed, currenttime,this,3);
                        if (e > 0)
                        {
                            Help(3);
                            if ((e & bits[7]) != 0)
                                AddSparks(e, 3);
                            else
                                AddShards(e, 3);
                        }
                        else
                            Hurt(3);
                    }
                    if (controllers[contInput[3]].DPad.Up == ButtonState.Pressed && bassStrum != 2)
                    {
                        bassStrum = 2;
                        byte pressed = 0;
                        if (controllers[contInput[3]].Buttons.A == ButtonState.Pressed)
                            pressed |= 1;
                        if (controllers[contInput[3]].Buttons.B == ButtonState.Pressed)
                            pressed |= 2;
                        if (controllers[contInput[3]].Buttons.Y == ButtonState.Pressed)
                            pressed |= 4;
                        if (controllers[contInput[3]].Buttons.X == ButtonState.Pressed)
                            pressed |= 8;
                        if (controllers[contInput[3]].Buttons.LeftShoulder == ButtonState.Pressed)
                            pressed |= 16;
                        byte e = boards[3].Strum(pressed, currenttime,this,3);
                        if (e > 0)
                        {
                            Help(3);
                            if ((e & bits[7]) != 0)
                                AddSparks(e, 3);
                            else
                                AddShards(e, 3);
                        }
                        else
                            Hurt(3);
                    }
                    if (controllers[contInput[3]].DPad.Up == ButtonState.Released &&
                       controllers[contInput[3]].DPad.Down == ButtonState.Released)
                        bassStrum = 0;
                }
                else if (contInput[3] == 4)
                {
                    if (Keyboard.GetState().IsKeyDown(Keys.Down) && bassStrum != 1)
                    {
                        bassStrum = 1;
                        byte pressed = 0;
                        if (Keyboard.GetState().IsKeyDown(Keys.G))
                            pressed |= bits[boards[3].IsLefty()?0:4];
                        if (Keyboard.GetState().IsKeyDown(Keys.F))
                            pressed |= bits[boards[3].IsLefty()?1:3];
                        if (Keyboard.GetState().IsKeyDown(Keys.D))
                            pressed |= bits[2];
                        if (Keyboard.GetState().IsKeyDown(Keys.S))
                            pressed |= bits[boards[3].IsLefty()?3:1];
                        if (Keyboard.GetState().IsKeyDown(Keys.A))
                            pressed |= bits[boards[3].IsLefty()?4:0];

                        byte e = boards[3].Strum(pressed, currenttime,this,3);
                        if (e > 0)
                        {
                            Help(3);
                            if ((e & bits[7]) != 0)
                                AddSparks(e, 3);
                            else
                                AddShards(e, 3);
                        }
                        else
                            Hurt(3);
                    }
                    if (Keyboard.GetState().IsKeyDown(Keys.Up) && bassStrum != 2)
                    {
                        bassStrum = 2;
                        byte pressed = 0;
                        if (Keyboard.GetState().IsKeyDown(Keys.G))
                            pressed |= bits[boards[3].IsLefty()?0:4];
                        if (Keyboard.GetState().IsKeyDown(Keys.F))
                            pressed |= bits[boards[3].IsLefty()?1:3];
                        if (Keyboard.GetState().IsKeyDown(Keys.D))
                            pressed |= bits[2];
                        if (Keyboard.GetState().IsKeyDown(Keys.S))
                            pressed |= bits[boards[3].IsLefty()?3:1];
                        if (Keyboard.GetState().IsKeyDown(Keys.A))
                            pressed |= bits[boards[3].IsLefty()?4:0];
                        byte e = boards[3].Strum(pressed, currenttime,this,3);
                        if (e > 0)
                        {
                            Help(3);
                            if ((e & bits[7]) != 0)
                                AddSparks(e, 3);
                            else
                                AddShards(e, 3);
                        }
                        else
                            Hurt(3);
                    }
                    if (!Keyboard.GetState().IsKeyDown(Keys.Up) &&
                        !Keyboard.GetState().IsKeyDown(Keys.Down))
                        bassStrum = 0;
                }
            }*/
        }

        private void ProcessInputFS()
        {
            for (int i = 0; i < 4; i++)
                if (instruments[i])
                {
                    /*if (i == 0 || i==3)
                    {
                        byte pressed = 0;
                        if (contInput[i] < 4)
                        {
                            if (controllers[contInput[i]].Buttons.A == ButtonState.Pressed)
                                pressed |= bits[boards[i].IsLefty()?4:0];
                            if (controllers[contInput[i]].Buttons.B == ButtonState.Pressed)
                                pressed |= bits[boards[i].IsLefty()?3:1];
                            if (controllers[contInput[i]].Buttons.Y == ButtonState.Pressed)
                                pressed |= bits[2];
                            if (controllers[contInput[i]].Buttons.X == ButtonState.Pressed)
                                pressed |= bits[boards[i].IsLefty()?1:3];
                            if (controllers[contInput[i]].Buttons.LeftShoulder == ButtonState.Pressed)
                                pressed |= bits[boards[i].IsLefty()?0:4];
                            if (controllers[contInput[i]].ThumbSticks.Right.Y > 0.95f)
                                boards[i].ActivateStarPower();
                        }
                        else if (contInput[i] == 4)
                        {
                            if (Keyboard.GetState().IsKeyDown(Keys.G))
                                pressed |= bits[boards[i].IsLefty()?0:4];
                            if (Keyboard.GetState().IsKeyDown(Keys.F))
                                pressed |= bits[boards[i].IsLefty()?1:3];
                            if (Keyboard.GetState().IsKeyDown(Keys.D))
                                pressed |= bits[2];
                            if (Keyboard.GetState().IsKeyDown(Keys.S))
                                pressed |= bits[boards[i].IsLefty()?3:1];
                            if (Keyboard.GetState().IsKeyDown(Keys.A))
                                pressed |= bits[boards[i].IsLefty()?4:0];
                        }
                        boards[i].Update(gameTime,currenttime, this, i, pressed);
                    }*/
                    if (i == 2)
                    {
                        byte pressed = 0;
                        if (contInput[2] < 4)
                        {
                            if (controllers[contInput[i]].Buttons.A == ButtonState.Pressed)
                                pressed |= 1;
                            if (controllers[contInput[i]].Buttons.B == ButtonState.Pressed)
                                pressed |= 2;
                            if (controllers[contInput[i]].Buttons.Y == ButtonState.Pressed)
                                pressed |= 4;
                            if (controllers[contInput[i]].Buttons.X == ButtonState.Pressed)
                                pressed |= 8;
                            if (controllers[contInput[i]].Buttons.LeftShoulder == ButtonState.Pressed)
                                pressed |= 16;
                        }
                        else if (contInput[2] == 4)
                        {
                            if (Keyboard.GetState().IsKeyDown(Keys.F))
                                pressed |= 1;
                            if (Keyboard.GetState().IsKeyDown(Keys.A))
                                pressed |= 2;
                            if (Keyboard.GetState().IsKeyDown(Keys.S))
                                pressed |= 4;
                            if (Keyboard.GetState().IsKeyDown(Keys.D))
                                pressed |= 8;
                            if (Keyboard.GetState().IsKeyDown(Keys.Space))
                                pressed |= 16;
                        }
                        byte newPressed = (byte)(((int)pressed ^ (int)oldpressed[i]) & (int)pressed);
                        oldpressed[i] = pressed;
                        if ((newPressed & 1) != 0)
                            audioSoundBank.PlayCue("crash");
                        if ((newPressed & 2) != 0)
                            audioSoundBank.PlayCue("snare");
                        if ((newPressed & 4) != 0)
                            audioSoundBank.PlayCue("tom1");
                        if ((newPressed & 8) != 0)
                            audioSoundBank.PlayCue("tom2");
                        if ((newPressed & 16) != 0)
                            audioSoundBank.PlayCue("bass");
                        AddSparksFS(newPressed, 2);
                        /*if (pressed != 0)
                        {
                            byte e = boards[2].Bang(pressed, currenttime, this);
                            if (e > 0)
                            {
                                if ((e & bits[7]) != 0)
                                    AddSparks(e, 2);
                                else
                                    AddShards(e, 2);
                            }
                        }*/
                        //boards[i].Update(gameTime, currenttime, this, i, pressed);
                    }
                }

            /*if (instruments[0])
            {
                if(contInput[0]<4)
                {
                    if (controllers[contInput[0]].DPad.Down == ButtonState.Pressed && guitarStrum != 1)
                    {
                        guitarStrum = 1;
                        byte pressed = 0;
                        if (controllers[contInput[0]].Buttons.A == ButtonState.Pressed)
                            pressed |= 1;
                        if (controllers[contInput[0]].Buttons.B == ButtonState.Pressed)
                            pressed |= 2;
                        if (controllers[contInput[0]].Buttons.Y == ButtonState.Pressed)
                            pressed |= 4;
                        if (controllers[contInput[0]].Buttons.X == ButtonState.Pressed)
                            pressed |= 8;
                        if (controllers[contInput[0]].Buttons.LeftShoulder == ButtonState.Pressed)
                            pressed |= 16;

                        byte e = boards[0].Strum(pressed, currenttime,this,0);
                        if (e > 0)
                        {
                            Help(0);
                            if ((e & bits[7]) != 0)
                                AddSparks(e, 0);
                            else
                                AddShards(e, 0);
                        }
                        else
                            Hurt(0);
                    }
                    if (controllers[contInput[0]].DPad.Up == ButtonState.Pressed && guitarStrum != 2)
                    {
                        guitarStrum = 2;
                        byte pressed = 0;
                        if (controllers[contInput[0]].Buttons.A == ButtonState.Pressed)
                            pressed |= 1;
                        if (controllers[contInput[0]].Buttons.B == ButtonState.Pressed)
                            pressed |= 2;
                        if (controllers[contInput[0]].Buttons.Y == ButtonState.Pressed)
                            pressed |= 4;
                        if (controllers[contInput[0]].Buttons.X == ButtonState.Pressed)
                            pressed |= 8;
                        if (controllers[contInput[0]].Buttons.LeftShoulder == ButtonState.Pressed)
                            pressed |= 16;
                        byte e = boards[0].Strum(pressed, currenttime,this,0);
                        if (e > 0)
                        {
                            Help(0);
                            if ((e & bits[7]) != 0)
                                AddSparks(e, 0);
                            else
                                AddShards(e, 0);
                        }
                        else
                            Hurt(0);
                    }
                    if (controllers[contInput[0]].DPad.Up == ButtonState.Released &&
                       controllers[contInput[0]].DPad.Down == ButtonState.Released)
                        guitarStrum = 0;
                }
                else if (contInput[0] == 4)
                {
                    if (Keyboard.GetState().IsKeyDown(Keys.Down) && guitarStrum != 1)
                    {
                        guitarStrum = 1;
                        byte pressed = 0;
                        if (Keyboard.GetState().IsKeyDown(Keys.G))
                            pressed |= bits[boards[0].IsLefty()?0:4];
                        if (Keyboard.GetState().IsKeyDown(Keys.F))
                            pressed |= bits[boards[0].IsLefty()?1:3];
                        if (Keyboard.GetState().IsKeyDown(Keys.D))
                            pressed |= bits[2];
                        if (Keyboard.GetState().IsKeyDown(Keys.S))
                            pressed |= bits[boards[0].IsLefty()?3:1];
                        if (Keyboard.GetState().IsKeyDown(Keys.A))
                            pressed |= bits[boards[0].IsLefty()?4:0];

                        byte e = boards[0].Strum(pressed, currenttime,this,0);
                        if (e > 0)
                        {
                            Help(0);
                            if ((e & bits[7]) != 0)
                                AddSparks(e, 0);
                            else
                                AddShards(e, 0);
                        }
                        else
                            Hurt(0);
                    }
                    if (Keyboard.GetState().IsKeyDown(Keys.Up) && guitarStrum != 2)
                    {
                        guitarStrum = 2;
                        byte pressed = 0;
                        if (Keyboard.GetState().IsKeyDown(Keys.G))
                            pressed |= bits[boards[0].IsLefty()?0:4];
                        if (Keyboard.GetState().IsKeyDown(Keys.F))
                            pressed |= bits[boards[0].IsLefty()?1:3];
                        if (Keyboard.GetState().IsKeyDown(Keys.D))
                            pressed |= bits[2];
                        if (Keyboard.GetState().IsKeyDown(Keys.S))
                            pressed |= bits[boards[0].IsLefty()?3:1];
                        if (Keyboard.GetState().IsKeyDown(Keys.A))
                            pressed |= bits[boards[0].IsLefty()?4:0];
                        byte e = boards[0].Strum(pressed, currenttime,this,0);
                        if (e > 0)
                        {
                            Help(0);
                            if ((e & bits[7]) != 0)
                                AddSparks(e, 0);
                            else
                                AddShards(e, 0);
                        }
                        else
                            Hurt(0);
                    }
                    if (!Keyboard.GetState().IsKeyDown(Keys.Up) &&
                        !Keyboard.GetState().IsKeyDown(Keys.Down))
                        guitarStrum = 0;
                }
            }
            if (instruments[3])
            {
                if(contInput[3]<4)
                {
                    if (controllers[contInput[3]].DPad.Down == ButtonState.Pressed && bassStrum != 1)
                    {
                        bassStrum = 1;
                        byte pressed = 0;
                        if (controllers[contInput[3]].Buttons.A == ButtonState.Pressed)
                            pressed |= 1;
                        if (controllers[contInput[3]].Buttons.B == ButtonState.Pressed)
                            pressed |= 2;
                        if (controllers[contInput[3]].Buttons.Y == ButtonState.Pressed)
                            pressed |= 4;
                        if (controllers[contInput[3]].Buttons.X == ButtonState.Pressed)
                            pressed |= 8;
                        if (controllers[contInput[3]].Buttons.LeftShoulder == ButtonState.Pressed)
                            pressed |= 16;

                        byte e = boards[3].Strum(pressed, currenttime,this,3);
                        if (e > 0)
                        {
                            Help(3);
                            if ((e & bits[7]) != 0)
                                AddSparks(e, 3);
                            else
                                AddShards(e, 3);
                        }
                        else
                            Hurt(3);
                    }
                    if (controllers[contInput[3]].DPad.Up == ButtonState.Pressed && bassStrum != 2)
                    {
                        bassStrum = 2;
                        byte pressed = 0;
                        if (controllers[contInput[3]].Buttons.A == ButtonState.Pressed)
                            pressed |= 1;
                        if (controllers[contInput[3]].Buttons.B == ButtonState.Pressed)
                            pressed |= 2;
                        if (controllers[contInput[3]].Buttons.Y == ButtonState.Pressed)
                            pressed |= 4;
                        if (controllers[contInput[3]].Buttons.X == ButtonState.Pressed)
                            pressed |= 8;
                        if (controllers[contInput[3]].Buttons.LeftShoulder == ButtonState.Pressed)
                            pressed |= 16;
                        byte e = boards[3].Strum(pressed, currenttime,this,3);
                        if (e > 0)
                        {
                            Help(3);
                            if ((e & bits[7]) != 0)
                                AddSparks(e, 3);
                            else
                                AddShards(e, 3);
                        }
                        else
                            Hurt(3);
                    }
                    if (controllers[contInput[3]].DPad.Up == ButtonState.Released &&
                       controllers[contInput[3]].DPad.Down == ButtonState.Released)
                        bassStrum = 0;
                }
                else if (contInput[3] == 4)
                {
                    if (Keyboard.GetState().IsKeyDown(Keys.Down) && bassStrum != 1)
                    {
                        bassStrum = 1;
                        byte pressed = 0;
                        if (Keyboard.GetState().IsKeyDown(Keys.G))
                            pressed |= bits[boards[3].IsLefty()?0:4];
                        if (Keyboard.GetState().IsKeyDown(Keys.F))
                            pressed |= bits[boards[3].IsLefty()?1:3];
                        if (Keyboard.GetState().IsKeyDown(Keys.D))
                            pressed |= bits[2];
                        if (Keyboard.GetState().IsKeyDown(Keys.S))
                            pressed |= bits[boards[3].IsLefty()?3:1];
                        if (Keyboard.GetState().IsKeyDown(Keys.A))
                            pressed |= bits[boards[3].IsLefty()?4:0];

                        byte e = boards[3].Strum(pressed, currenttime,this,3);
                        if (e > 0)
                        {
                            Help(3);
                            if ((e & bits[7]) != 0)
                                AddSparks(e, 3);
                            else
                                AddShards(e, 3);
                        }
                        else
                            Hurt(3);
                    }
                    if (Keyboard.GetState().IsKeyDown(Keys.Up) && bassStrum != 2)
                    {
                        bassStrum = 2;
                        byte pressed = 0;
                        if (Keyboard.GetState().IsKeyDown(Keys.G))
                            pressed |= bits[boards[3].IsLefty()?0:4];
                        if (Keyboard.GetState().IsKeyDown(Keys.F))
                            pressed |= bits[boards[3].IsLefty()?1:3];
                        if (Keyboard.GetState().IsKeyDown(Keys.D))
                            pressed |= bits[2];
                        if (Keyboard.GetState().IsKeyDown(Keys.S))
                            pressed |= bits[boards[3].IsLefty()?3:1];
                        if (Keyboard.GetState().IsKeyDown(Keys.A))
                            pressed |= bits[boards[3].IsLefty()?4:0];
                        byte e = boards[3].Strum(pressed, currenttime,this,3);
                        if (e > 0)
                        {
                            Help(3);
                            if ((e & bits[7]) != 0)
                                AddSparks(e, 3);
                            else
                                AddShards(e, 3);
                        }
                        else
                            Hurt(3);
                    }
                    if (!Keyboard.GetState().IsKeyDown(Keys.Up) &&
                        !Keyboard.GetState().IsKeyDown(Keys.Down))
                        bassStrum = 0;
                }
            }*/
        }

        float[] noteLanePos = { 29/256f, 79/256f, 127/256f, 176/256f, 227/256f };
        float ratio = 7 / 8f;
        private void DrawBoardTarget(int i, Vector2[] songtimes, bool sp)
        {

            fader.CurrentTechnique = fader.Techniques["Fade"];
            float fh = (Board.eFade - Board.sFade) / (Board.eFade * (1/ratio));
            if (i == 1)
                return;
            graphics.GraphicsDevice.SetRenderTarget(0, rtWaves[i]);
            graphics.GraphicsDevice.Clear(new Color(0, 0, 0, 0));
            spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);
            if (boards[i].wavesLen>0)
            {
                for (int p = 0; p < boards[i].wavesLen; p++)
                {
                    for (int r = 0; r < 5; r++)
                    {
                        if ((boards[i].waves[p][0].Z & bits[r]) > 0)
                        {
                            for (int q = 0; q < boards[i].wavesSubLen[p] - 1; q++)
                            {
                                float loA, hiA;
                                if (boards[i].waves[p][q].Y / 1000f < Board.sFade)
                                    loA = 1;
                                else if (boards[i].waves[p][q].Y / 1000f < Board.eFade)
                                    loA = 1 - (((boards[i].waves[p][q].Y / 1000f) - Board.sFade) / (Board.eFade - Board.sFade));
                                else
                                    loA = 0;
                                if (boards[i].waves[p][q + 1].Y / 1000f < Board.sFade)
                                    hiA = 1;
                                else if (boards[i].waves[p][q + 1].Y / 1000f < Board.eFade)
                                    hiA = 1 - (((boards[i].waves[p][q + 1].Y / 1000f) - Board.sFade) / (Board.eFade - Board.sFade));
                                else
                                    hiA = 0;
                                float xscale = 15f;
                                //spritebatch.Draw(texWhite, new Vector2(boards[i].waves[p][q].X, (rtBoard[i].Height * ratio) - ((rtBoard[i].Height * ratio) * ((boards[i].waves[p][q].Y / 1000f) / Board.eFade))), Color.White);
                                int note = boards[i].waves[p][q].Z;
                                //if ((note & 128) == 0)
                                {
                                    for (int k = 0; k < 5; k++)
                                        if ((note & (1 << k)) != 0)
                                        {
                                            if ((note & 64) != 0)
                                            {
                                                spritebatch.Draw(texLine, new Vector2((boards[i].waves[p][q].X * xscale) + (boards[i].IsLefty() ? noteLanePos[4 - k] * rtBoard[i].Width : noteLanePos[k] * rtBoard[i].Width), (rtBoard[i].Height * ratio) - ((rtBoard[i].Height * ratio) * ((boards[i].waves[p][q].Y / 1000f) / Board.eFade))), null, boards[i].waves[p][q].White?Color.White:FretColors[k], (float)Math.Atan2(((-rtBoard[i].Height * ratio) * (((boards[i].waves[p][q + 1].Y - boards[i].waves[p][q].Y) / 1000f) / Board.eFade)), (boards[i].waves[p][q + 1].X - boards[i].waves[p][q].X) * xscale) + MathHelper.PiOver2, new Vector2(texLine.Width / 2, texLine.Height), new Vector2(0.5f, (new Vector2((boards[i].waves[p][q + 1].X - boards[i].waves[p][q].X) * xscale, ((rtBoard[i].Height * ratio) * (((boards[i].waves[p][q + 1].Y - boards[i].waves[p][q].Y) / 1000f) / Board.eFade)))).Length() * 1.05f / (float)texLine.Height), SpriteEffects.None, 0);
                                                spritebatch.Draw(texLine, new Vector2((boards[i].waves[p][q].X * -xscale) + (boards[i].IsLefty() ? noteLanePos[4 - k] * rtBoard[i].Width : noteLanePos[k] * rtBoard[i].Width), (rtBoard[i].Height * ratio) - ((rtBoard[i].Height * ratio) * ((boards[i].waves[p][q].Y / 1000f) / Board.eFade))), null, boards[i].waves[p][q].White?Color.White:FretColors[k], (float)Math.Atan2(((-rtBoard[i].Height * ratio) * (((boards[i].waves[p][q + 1].Y - boards[i].waves[p][q].Y) / 1000f) / Board.eFade)), (boards[i].waves[p][q + 1].X - boards[i].waves[p][q].X) * -xscale) + MathHelper.PiOver2, new Vector2(texLine.Width / 2, texLine.Height), new Vector2(0.5f, (new Vector2((boards[i].waves[p][q + 1].X - boards[i].waves[p][q].X) * xscale, ((rtBoard[i].Height * ratio) * (((boards[i].waves[p][q + 1].Y - boards[i].waves[p][q].Y) / 1000f) / Board.eFade)))).Length() * 1.05f / (float)texLine.Height), SpriteEffects.None, 0);
                                            }
                                            else
                                            {
                                                spritebatch.Draw(texLine, new Vector2((boards[i].waves[p][q].X * xscale) + (boards[i].IsLefty() ? noteLanePos[4 - k] * rtBoard[i].Width : noteLanePos[k] * rtBoard[i].Width), (rtBoard[i].Height * ratio) - ((rtBoard[i].Height * ratio) * ((boards[i].waves[p][q].Y / 1000f) / Board.eFade))), null, FadedFretColors[k], (float)Math.Atan2(((-rtBoard[i].Height * ratio) * (((boards[i].waves[p][q + 1].Y - boards[i].waves[p][q].Y) / 1000f) / Board.eFade)), (boards[i].waves[p][q + 1].X - boards[i].waves[p][q].X) * xscale) + MathHelper.PiOver2, new Vector2(texLine.Width / 2, texLine.Height), new Vector2(0.5f, (new Vector2((boards[i].waves[p][q + 1].X - boards[i].waves[p][q].X) * xscale, ((rtBoard[i].Height * ratio) * (((boards[i].waves[p][q + 1].Y - boards[i].waves[p][q].Y) / 1000f) / Board.eFade)))).Length() * 1.05f / (float)texLine.Height), SpriteEffects.None, 0);
                                                spritebatch.Draw(texLine, new Vector2((boards[i].waves[p][q].X * -xscale) + (boards[i].IsLefty() ? noteLanePos[4 - k] * rtBoard[i].Width : noteLanePos[k] * rtBoard[i].Width), (rtBoard[i].Height * ratio) - ((rtBoard[i].Height * ratio) * ((boards[i].waves[p][q].Y / 1000f) / Board.eFade))), null, FadedFretColors[k], (float)Math.Atan2(((-rtBoard[i].Height * ratio) * (((boards[i].waves[p][q + 1].Y - boards[i].waves[p][q].Y) / 1000f) / Board.eFade)), (boards[i].waves[p][q + 1].X - boards[i].waves[p][q].X) * -xscale) + MathHelper.PiOver2, new Vector2(texLine.Width / 2, texLine.Height), new Vector2(0.5f, (new Vector2((boards[i].waves[p][q + 1].X - boards[i].waves[p][q].X) * xscale, ((rtBoard[i].Height * ratio) * (((boards[i].waves[p][q + 1].Y - boards[i].waves[p][q].Y) / 1000f) / Board.eFade)))).Length() * 1.05f / (float)texLine.Height), SpriteEffects.None, 0);
                                            }
                                        }
                                }
                                /*else
                                {
                                    int k = 0;
                                    k = 1;
                                }*/

                                /*tmpMdl[0].Position.X = boards[i].waves[p][q].X;
                                tmpMdl[1].Position.X = boards[i].waves[p][q + 1].X;
                                tmpMdl[3].Position.X = boards[i].waves[p][q + 1].X;
                                tmpMdl[2].Position.X = boards[i].waves[p][q].X + w;
                                tmpMdl[4].Position.X = boards[i].waves[p][q].X + w;
                                tmpMdl[5].Position.X = boards[i].waves[p][q + 1].X + w;
                                tmpMdl[0].Alpha = loA;
                                tmpMdl[1].Alpha = hiA;
                                tmpMdl[2].Alpha = loA;
                                tmpMdl[3].Alpha = hiA;
                                tmpMdl[4].Alpha = loA;
                                tmpMdl[5].Alpha = hiA;*/

                                /*if (q >= boards[i].wavesSubLen[p] - 2)
                                    engine.Parameters["diffuseTexture"].SetValue(texLineEnd);
                                else
                                    engine.Parameters["diffuseTexture"].SetValue(texLine);
                                engine.Parameters["diffuseColor"].SetValue(FretColorsV4[r]);
                                if(((byte)(boards[i].waves[p][q].Z)&128)!=0)
                                    engine.Parameters["diffuseColor"].SetValue(new Vector4(.5f,.5f,.5f,1.0f));
                                engine.CommitChanges();*/

                                /*hide = !hide;

                                vb = new VertexBuffer(graphics.GraphicsDevice, tmpMdl.Length * GBVertexFormat.SizeInBytes, BufferUsage.WriteOnly);
                                vb.SetData<GBVertexFormat>(tmpMdl);*/

                                // 5: draw object - select vertex type, primitive type, # of primitives
                                /*graphics.GraphicsDevice.VertexDeclaration = vd;
                                graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                                graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                                graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                                graphics.GraphicsDevice.Vertices[0].SetSource(vb, 0, GBVertexFormat.SizeInBytes);
                                graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, (vb.SizeInBytes / GBVertexFormat.SizeInBytes) / 3);
                                graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;

                                tmpMdl[0].Position.X = -boards[i].waves[p][q].X;
                                tmpMdl[1].Position.X = -boards[i].waves[p][q + 1].X;
                                tmpMdl[3].Position.X = -boards[i].waves[p][q + 1].X;
                                tmpMdl[2].Position.X = -boards[i].waves[p][q].X + w;
                                tmpMdl[4].Position.X = -boards[i].waves[p][q].X + w;
                                tmpMdl[5].Position.X = -boards[i].waves[p][q + 1].X + w;

                                vb = new VertexBuffer(graphics.GraphicsDevice, tmpMdl.Length * GBVertexFormat.SizeInBytes, BufferUsage.WriteOnly);
                                vb.SetData<GBVertexFormat>(tmpMdl);

                                graphics.GraphicsDevice.VertexDeclaration = vd;
                                graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                                graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                                graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                                graphics.GraphicsDevice.Vertices[0].SetSource(vb, 0, GBVertexFormat.SizeInBytes);
                                graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, (vb.SizeInBytes / GBVertexFormat.SizeInBytes) / 3);
                                graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;*/
                            }

                        }
                    }
                }
            }
            spritebatch.End();


            graphics.GraphicsDevice.SetRenderTarget(0, null);
            boards[i].texWaves = rtWaves[i].GetTexture();
            graphics.GraphicsDevice.SetRenderTarget(0, rtWaves[i]);
            graphics.GraphicsDevice.Clear(new Color(0, 0, 0, 0));

            spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);
            fader.Begin();
            fader.CurrentTechnique.Passes[0].Begin();

            fader.Parameters["blend"].SetValue(fh);
            spritebatch.Draw(boards[i].texWaves, new Rectangle(0, 0, rtWaves[i].Width, rtWaves[i].Height), Color.White);


            spritebatch.End();
            fader.CurrentTechnique.Passes[0].End();
            fader.End();
            graphics.GraphicsDevice.SetRenderTarget(0, rtBoard[i]);

            //float scale = (Board.eFade - Board.sFade) / (Board.eFade * 1.5f);

            fader.CommitChanges();
            float scale = rtBoard[i].Height / (Board.eFade * (1/ratio));

            
            graphics.GraphicsDevice.Clear(new Color(0, 0, 0, 0));
            //graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
            spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
            spritebatch.GraphicsDevice.RenderState.AlphaBlendEnable = true;
            spritebatch.GraphicsDevice.RenderState.SourceBlend = Blend.SourceColor;
            spritebatch.GraphicsDevice.RenderState.DestinationBlend = Blend.DestinationColor;
            spritebatch.GraphicsDevice.RenderState.AlphaDestinationBlend = Blend.InverseSourceAlpha;
            spritebatch.GraphicsDevice.RenderState.AlphaSourceBlend = Blend.SourceAlpha;
            float bgyscale = 2.0f;
            for (int k = -4; k < 8; k++)
            {
                if (boards[i].IsSPActivated())
                    spritebatch.Draw(boardBackgrounds[i], new Rectangle(0, (int)(((((started < 2 ? -CurrentTime : CurrentTime) % (float)TicksPerSecond) / (float)TicksPerSecond) + (k / bgyscale)) * (rtBoard[i].Height / (Board.eFade * (1/ratio)))), rtBoard[i].Width, 1+(int)(rtBoard[i].Height / (Board.eFade * (1/ratio) * bgyscale))),null, new Color(128, 128, 0),0,new Vector2(0,0),SpriteEffects.None,1);
                else if(failStatus[i]==FS_FAILING)
                    spritebatch.Draw(boardBackgrounds[i], new Rectangle(0, (int)(((((started < 2 ? -CurrentTime : CurrentTime) % (float)TicksPerSecond) / (float)TicksPerSecond) + (k / bgyscale)) * (rtBoard[i].Height / (Board.eFade * (1 / ratio)))), rtBoard[i].Width, 1 + (int)(rtBoard[i].Height / (Board.eFade * (1 / ratio) * bgyscale))), null, new Color((byte)(255 * failTime), 0, 0), 0, new Vector2(0, 0), SpriteEffects.None, 1);
                else if (rockMeterLevel[i]<20)
                {
                    if(song.percentBeat>0.5)
                        spritebatch.Draw(boardBackgrounds[i], new Rectangle(0, (int)(((((started < 2 ? -CurrentTime : CurrentTime) % (float)TicksPerSecond) / (float)TicksPerSecond) + (k / bgyscale)) * (rtBoard[i].Height / (Board.eFade * (1 / ratio)))), rtBoard[i].Width, 1 + (int)(rtBoard[i].Height / (Board.eFade * (1 / ratio) * bgyscale))), null, new Color((byte)((song.percentBeat - 0.5) * 255), 0, 0), 0, new Vector2(0, 0), SpriteEffects.None, 1);
                    else
                        spritebatch.Draw(boardBackgrounds[i], new Rectangle(0, (int)(((((started < 2 ? -CurrentTime : CurrentTime) % (float)TicksPerSecond) / (float)TicksPerSecond) + (k / bgyscale)) * (rtBoard[i].Height / (Board.eFade * (1 / ratio)))), rtBoard[i].Width, 1 + (int)(rtBoard[i].Height / (Board.eFade * (1 / ratio) * bgyscale))), null, new Color((byte)((0.5 - song.percentBeat) * 255), 0, 0), 0, new Vector2(0, 0), SpriteEffects.None, 1);
                }
                else if(rockMeterLevel[i]>80)
                    spritebatch.Draw(boardBackgrounds[i], new Rectangle(0, (int)(((((started < 2 ? -CurrentTime : CurrentTime) % (float)TicksPerSecond) / (float)TicksPerSecond) + (k / bgyscale)) * (rtBoard[i].Height / (Board.eFade * (1 / ratio)))), rtBoard[i].Width, 1 + (int)(rtBoard[i].Height / (Board.eFade * (1 / ratio) * bgyscale))), null, new Color(30, (byte)(30 + ((rockMeterLevel[i] - 80) / 20f) * 50), 30), 0, new Vector2(0, 0), SpriteEffects.None, 1);
                else if(rockMeterLevel[i]<40)
                    spritebatch.Draw(boardBackgrounds[i], new Rectangle(0, (int)(((((started < 2 ? -CurrentTime : CurrentTime) % (float)TicksPerSecond) / (float)TicksPerSecond) + (k / bgyscale)) * (rtBoard[i].Height / (Board.eFade * (1 / ratio)))), rtBoard[i].Width, 1 + (int)(rtBoard[i].Height / (Board.eFade * (1 / ratio) * bgyscale))), null, new Color((byte)(30 + (1 - ((rockMeterLevel[i] - 20) / 20f)) * 50), 30, 30), 0, new Vector2(0, 0), SpriteEffects.None, 1);
                else
                    spritebatch.Draw(boardBackgrounds[i], new Rectangle(0, (int)(((((started < 2 ? -CurrentTime : CurrentTime) % (float)TicksPerSecond) / (float)TicksPerSecond) + (k / bgyscale)) * (rtBoard[i].Height / (Board.eFade * (1 / ratio)))), rtBoard[i].Width, 1 + (int)(rtBoard[i].Height / (Board.eFade * (1 / ratio) * bgyscale))), null, new Color(30, 30, 30), 0, new Vector2(0, 0), SpriteEffects.None, 1);
            }

            int fiver = i == 0 || i == 3 ? 1 : 0;
            int lowestPoint = 0;
            //spritebatch.Draw(Board.boardTexPlain[fiver][0], new Rectangle(0, 0, rtBoard[i].Width,rtBoard[i].Height),null,Color.White);// (int)y, rtBoard[i].Width, (int)height), null, Color.White, 0, new Vector2(0, 0), SpriteEffects.None, 0.9f);
            for (int k = 0; k < songtimes.Length - 1; k++)
            {
                if (started < 2 && CurrentTime < 30 * TicksPerSecond)
                {
                    if ((int)(rtBoard[i].Height * ratio) - (int)(songtimes[k].X * scale) > lowestPoint)
                        lowestPoint = (int)(rtBoard[i].Height * ratio) - (int)(songtimes[k].X * scale);
                }

                {
                    float height = (songtimes[k + 1].X - songtimes[k].X) * scale;
                    float y = (rtBoard[i].Height * ratio) - (int)(songtimes[k].X * scale) - (int)((songtimes[k + 1].X - songtimes[k].X) * scale);
                    
                    for (int j = 0; j < (int)(songtimes[k].Y + 0.5); j++)
                    {
                        spritebatch.Draw(texWhite, new Rectangle(0, (int)(y + ((j / songtimes[k].Y) * height)) - 1, rtBoard[i].Width, 5), Color.DarkGray);
                        spritebatch.Draw(texWhite, new Rectangle(0, (int)(y + (((j + 0.5) / songtimes[k].Y) * height)), rtBoard[i].Width, 3), Color.DarkGray);
                    }
                    spritebatch.Draw(texWhite, new Rectangle(0, (int)(y + (((0.5) / songtimes[k].Y) * height)), rtBoard[i].Width, 3), Color.DarkGray);
                    spritebatch.Draw(texWhite, new Rectangle(0, (int)y - 2, rtBoard[i].Width, 5), Color.DarkGray);
                }
            }
            if(i==2)
            for (int k = 0; k < boards[i].OutFills.Length; k++)
            {
                float halfMaxWidth = rtBoard[i].Width / 8f;
                if (boards[i].OutFills[k].W > 0.5)
                {
                    float height = (boards[i].OutFills[k].Y - boards[i].OutFills[k].X) * scale;
                    float y = (rtBoard[i].Height * ratio) - (int)(boards[i].OutFills[k].X * scale) - (int)((boards[i].OutFills[k].Y - boards[i].OutFills[k].X) * scale);
                    for (int r = 0; r < 4; r++)
                    {
                        float center = ((r * 2 + 1)/8f)*rtBoard[i].Width;
                        spritebatch.Draw(Board.drumfillTex, new Rectangle((int)(center - (halfMaxWidth * boards[i].OutFills[k].Z)), (int)y, (int)(2 * (halfMaxWidth * boards[i].OutFills[k].Z)), (int)height), FretColors[Board.guitarToDrums[r]]);
                    }
                }
            }
            /*if (started < 2 && CurrentTime < 30 * TicksPerSecond)
                if (lowestPoint < rtBoard[i].Height * .99)
                    spritebatch.Draw(Board.boardTexPlain[fiver][Board.boardBeatsIndex[1]], new Rectangle(0, lowestPoint, rtBoard[i].Width, rtBoard[i].Height - lowestPoint),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.9f);*/
            if (!DemoMode)
            {
                int spheight = (int)((rtBoard[i].Width / (float)Board.spMeterBG.Width) * Board.spMeterBG.Height * Board.spMeterYScale);
                spritebatch.Draw(Board.spMeterBG, new Rectangle((int)(rtBoard[i].Width * 0.0117f + 0.5f), (int)(rtBoard[i].Height * ratio) + (i == 2 ? boards[i].spMeterShiftDrums : boards[i].spMeterShift), (int)(rtBoard[i].Width * 0.97656f + 0.5f), spheight), null, Color.White, 0, new Vector2(0, 0), SpriteEffects.None, 0.1f);
                {
                    int left = (int)(rtBoard[i].Width * 0.03125f + rtBoard[i].Width * 0.0117f + 0.5f);
                    int top = (int)(rtBoard[i].Height * ratio) + (i == 2 ? boards[i].spMeterShiftDrums : boards[i].spMeterShift) + (int)(spheight * 0.03125f + 0.5f);
                    int width = (int)(rtBoard[i].Width * 0.97656f * 0.9375f * boards[i].GetSPAmount() + 0.5f);
                    int height = (int)(spheight * 0.21875f + 0.5f);
                    spritebatch.Draw(Board.spMeterFill, new Rectangle(left, top, width, height), Color.Yellow);
                    width = (int)(rtBoard[i].Width * 0.97656f * 0.9375f + 0.5f);

                    if (boards[i].GetSPAmount() >= 0.4999f)
                    {
                        for (int k = 0; k < spcircles.Length; k++)
                            if (spcircles[k].pos.X < boards[i].GetSPAmount())
                                spritebatch.Draw(Board.spMeterCurl, new Vector2((spcircles[k].pos.X * width) + left, (spcircles[k].pos.Y * height) + top), null, new Color(new Vector4(1, 1, 1, spcircles[k].alpha)), spcircles[k].rotation, new Vector2(Board.spMeterCurl.Width / 2, Board.spMeterCurl.Height / 2), new Vector2(scale / 1500, scale / 1500), SpriteEffects.None, 0);
                    }
                }
                int f = boards[i].GetMultiplierFraction();
                int m = boards[i].GetMultiplier();
                for (int k = 0; k < f; k++)
                    spritebatch.Draw(Board.spMeterLED, new Rectangle((int)(rtBoard[i].Width * 0.109375f + rtBoard[i].Width * 0.0117f + 0.5f) + (int)(rtBoard[i].Width * 0.97656f * .078125f * k + 0.5f), (int)(rtBoard[i].Height * ratio) + (i == 2 ? boards[i].spMeterShiftDrums : boards[i].spMeterShift) + (int)(spheight * 0.3125f + 0.5f), (int)(rtBoard[i].Width * 0.97656f * .078125f + 0.5f) + 1, (int)(spheight * 0.3125f + 0.5f)), null, m <= 1 ? Color.Yellow : m == 2 && f == 10 ? Color.Yellow : m == 2 ? Color.Green : m == 3 && f == 10 ? Color.Green : Color.Purple, 0, new Vector2(0, 0), SpriteEffects.None, 0.8f);
                if (multToIndex[m] >= 0)
                    spritebatch.Draw(texMult[multToIndex[m]], new Rectangle(rtBoard[i].Width / 3, (int)(rtBoard[i].Height * ratio) + (i == 2 ? boards[i].spMeterShiftDrums : boards[i].spMeterShift) + (int)(spheight * 0.3125f + 0.5f), rtBoard[i].Width / 3, (int)(spheight * 0.5f + 0.5f)), null, Color.White, 0, new Vector2(0, 0), SpriteEffects.None, 0);
            }
            spritebatch.End();

            graphics.GraphicsDevice.SetRenderTarget(0, null);
            boards[i].texBoard = rtBoard[i].GetTexture();
            graphics.GraphicsDevice.SetRenderTarget(0, rtBoard[i]);
            graphics.GraphicsDevice.Clear(new Color(0, 0, 0, 0));

            spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);
            
            fader.CommitChanges();
            fader.Begin();
            fader.CurrentTechnique.Passes[0].Begin();

            fader.Parameters["blend"].SetValue(fh);
            spritebatch.Draw(boards[i].texBoard, new Rectangle(0, 0, rtBoard[i].Width, rtBoard[i].Height), Color.White);


            spritebatch.End();
            fader.CurrentTechnique.Passes[0].End();
            fader.End();
            graphics.GraphicsDevice.SetRenderTarget(0, null);
            boards[i].texBoard = rtBoard[i].GetTexture();
        }
        private void DrawBoardTargetFS(int index)
        {

            fader.CurrentTechnique = fader.Techniques["Fade"];
            float fh = (Board.eFade - Board.sFade) / (Board.eFade * (1/ratio));
            if (index == 1)
                return;
            for (int i = 0; i < 4; i++)
            {
                if (instruments[i])
                {
                    fader.Parameters["blend"].SetValue(fh);
                    graphics.GraphicsDevice.SetRenderTarget(0, rtBoard[i]);

                    //float scale = (Board.eFade - Board.sFade) / (Board.eFade * 1.5f);

                    fader.CommitChanges();
                    float scale = rtBoard[i].Height / (Board.eFade * (1/ratio));

                    
                    graphics.GraphicsDevice.Clear(new Color(255, 255, 255, 0));
                    graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                    spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.BackToFront, SaveStateMode.SaveState);

                    int fiver = index == 0 || index == 3 ? 1 : 0;
                    spritebatch.Draw(Board.boardTexPlain[fiver][0], new Rectangle(0, 0, rtBoard[i].Width, rtBoard[i].Height),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.9f);

                    spritebatch.End();

                    graphics.GraphicsDevice.SetRenderTarget(0, null);
                    FStexBoard[i] = rtBoard[i].GetTexture();
                    graphics.GraphicsDevice.SetRenderTarget(0, rtBoard[i]);
                    graphics.GraphicsDevice.Clear(new Color(0, 0, 0, 0));

                    spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);
                    fader.Begin();
                    fader.CurrentTechnique.Passes[0].Begin();

                    fader.Parameters["blend"].SetValue(fh);
                    spritebatch.Draw(FStexBoard[i], new Rectangle(0, 0, rtBoard[i].Width, rtBoard[i].Height), Color.White);


                    spritebatch.End();
                    fader.CurrentTechnique.Passes[0].End();
                    fader.End();
                }
            }
            graphics.GraphicsDevice.SetRenderTarget(0, null);
            for (int i = 0; i < 4; i++)
            {
                if (instruments[i])
                {
                    FStexBoard[i] = rtBoard[i].GetTexture();
                }
            }
        }

        private void DrawBoard(int i, Matrix fling, bool SP, long currenttime, Matrix matTransl)
        {
            //if (i != 1)
            {
                graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;
                Matrix matScale, matOrbit;
                matOrbit = Matrix.CreateTranslation(0f, 0f, /*-(Board.length * Math.Min(lenvals[0].X, Board.sFade))*/ - Board.zeroZ - (Board.eFade*Board.length)) * fling;
                matScale = Matrix.CreateScale(new Vector3(Board.width, Board.curveHeight, Board.length*Board.eFade*(1/ratio)));
                engine.Parameters["world"].SetValue(matScale * matOrbit * matTransl);
                engine.Parameters["diffuseTexture"].SetValue(mode==M_GAME?boards[i].texBoard:FStexBoard[i]);
                engine.Parameters["wAlpha"].SetValue(1.0f);
                engine.CommitChanges();
                graphics.GraphicsDevice.VertexDeclaration = vd;
                graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                graphics.GraphicsDevice.Vertices[0].SetSource(Board.mdlBoard, 0, GBVertexFormat.SizeInBytes);
                graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, (Board.mdlBoard.SizeInBytes / GBVertexFormat.SizeInBytes) / 3);
                graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;

                /*
                VertexBuffer vb;
                //draw pre-song board
                if (lenvals[0].X > -Board.zeroZ)
                {
                    
                    
                    // identity, scale, rotate, orbit(translate & rotate), translate
                    engine.Parameters["world"].SetValue(matScale * matOrbit * matTransl);
                    if (!SP)
                    {
                        engine.Parameters["diffuseTexture"].SetValue(Board.boardTexPlain[(boards[i].GetBoardType() == GUITAR || boards[i].GetBoardType() == BASS) ? 1 : 0][0]);
                        engine.Parameters["wAlpha"].SetValue(1.0f);
                    }
                    else
                    {
                        engine.Parameters["diffuseTexture"].SetValue(Board.SPBoardTex);
                        engine.Parameters["wAlpha"].SetValue(0.5f);
                    }
                    engine.CommitChanges();

                    // 5: draw object - select vertex type, primitive type, # of primitives
                    graphics.GraphicsDevice.VertexDeclaration = vd;
                    graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                    graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                    graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                    graphics.GraphicsDevice.Vertices[0].SetSource(Board.mdlBoard, 0, GBVertexFormat.SizeInBytes);
                    graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, (Board.mdlBoard.SizeInBytes / GBVertexFormat.SizeInBytes) / 3);
                    graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                }
                //Draw song board
                int k;
                for (k = 0; k < 11; k++)
                {
                    if (boards[i].GetBoardType() != VOCALIST)
                    {
                        if (lenvals[k + 1].X >= Board.sFade)
                            break;
                        matOrbit = Matrix.CreateTranslation(0, 0f + (SP ? 0.01f : 0f), -(Board.length * lenvals[k].X) - Board.zeroZ) * fling;
                        matScale = Matrix.CreateScale(new Vector3(Board.width, Board.curveHeight, Board.length * (lenvals[k].X - lenvals[k + 1].X)));

                        // identity, scale, rotate, orbit(translate & rotate), translate
                        engine.Parameters["world"].SetValue(matScale * matOrbit * matTransl);
                        if (!SP)
                        {
                            engine.Parameters["diffuseTexture"].SetValue(Board.boardTexPlain[(boards[i].GetBoardType() == GUITAR || boards[i].GetBoardType() == BASS) ? 1 : 0][Board.boardBeatsIndex[(int)lenvals[i].Y]]);
                            engine.Parameters["wAlpha"].SetValue(1.0f);
                        }
                        else
                        {
                            engine.Parameters["diffuseTexture"].SetValue(Board.SPBoardTex);
                            engine.Parameters["wAlpha"].SetValue(0.5f);
                        }
                        engine.CommitChanges();

                        // 5: draw object - select vertex type, primitive type, # of primitives
                        graphics.GraphicsDevice.VertexDeclaration = vd;
                        graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                        graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                        graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                        graphics.GraphicsDevice.Vertices[0].SetSource(Board.mdlBoard, 0, GBVertexFormat.SizeInBytes);
                        graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, (Board.mdlBoard.SizeInBytes / GBVertexFormat.SizeInBytes) / 3);
                        graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                    }
                }
                //draw tail
                float[] old = new float[Board.arrBoard.Length];
                for (int r = 0; r < Board.arrBoard.Length; r++)
                    old[r] = Board.arrBoard[r].TexCoord.Y;
                float sTexY = (lenvals[k].X - Board.sFade) / (lenvals[k].X - lenvals[k + 1].X);
                float eTexY = (lenvals[k].X - Board.eFade) / (lenvals[k].X - lenvals[k + 1].X);
                if (lenvals[0].X <= Board.sFade && boards[i].GetBoardType() != VOCALIST)
                {
                    matOrbit = Matrix.CreateTranslation(0f, 0f + (SP ? 0.01f : 0f), -(Board.length * lenvals[k].X) - Board.zeroZ) * fling;
                    matScale = Matrix.CreateScale(new Vector3(Board.width, Board.curveHeight, Board.length * (lenvals[k].X - Board.sFade)));

                    // identity, scale, rotate, orbit(translate & rotate), translate
                    engine.Parameters["world"].SetValue(matScale * matOrbit * matTransl);

                    for (int r = 0; r < Board.arrBoard.Length; r++)
                    {
                        if (old[r] >= 1)
                            Board.arrBoard[r].TexCoord.Y = sTexY;
                    }

                    if (!SP)
                    {
                        engine.Parameters["diffuseTexture"].SetValue(Board.boardTexPlain[(boards[i].GetBoardType() == GUITAR || boards[i].GetBoardType() == BASS) ? 1 : 0][Board.boardBeatsIndex[(int)lenvals[i].Y]]);
                        engine.Parameters["wAlpha"].SetValue(1.0f);
                    }
                    else
                    {
                        engine.Parameters["diffuseTexture"].SetValue(Board.SPBoardTex);
                        engine.Parameters["wAlpha"].SetValue(0.5f);
                    }
                    engine.CommitChanges();

                    vb = new VertexBuffer(graphics.GraphicsDevice, Board.arrBoard.Length * GBVertexFormat.SizeInBytes, BufferUsage.WriteOnly);
                    vb.SetData<GBVertexFormat>(Board.arrBoard);

                    // 5: draw object - select vertex type, primitive type, # of primitives
                    graphics.GraphicsDevice.VertexDeclaration = vd;
                    graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                    graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                    graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                    graphics.GraphicsDevice.Vertices[0].SetSource(vb, 0, GBVertexFormat.SizeInBytes);
                    graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, (vb.SizeInBytes / GBVertexFormat.SizeInBytes) / 3);
                    graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                }
                {
                    matOrbit = Matrix.CreateTranslation(0f, 0f + (SP ? 0.01f : 0f), -(Board.length * Board.sFade) - Board.zeroZ) * fling;
                    matScale = Matrix.CreateScale(new Vector3(Board.width, Board.curveHeight, Board.length * (Board.sFade - Board.eFade)));

                    // identity, scale, rotate, orbit(translate & rotate), translate
                    engine.Parameters["world"].SetValue(matScale * matOrbit * matTransl);
                    for (int r = 0; r < Board.arrBoard.Length; r++)
                    {
                        if (old[r] >= 1)
                        {
                            Board.arrBoard[r].TexCoord.Y = eTexY;
                            Board.arrBoard[r].Alpha = 0f;
                        }
                        if (old[r] <= 0)
                            Board.arrBoard[r].TexCoord.Y = sTexY;
                    }

                    if (lenvals[0].X > Board.eFade)
                    {
                        if (!SP)
                            engine.Parameters["diffuseTexture"].SetValue(Board.boardTexPlain[(boards[i].GetBoardType() == GUITAR || boards[i].GetBoardType() == BASS) ? 1 : 0][0]);
                        else
                            engine.Parameters["diffuseTexture"].SetValue(Board.SPBoardTex);
                    }
                    else
                    {
                        if (!SP)
                            engine.Parameters["diffuseTexture"].SetValue(Board.boardTexPlain[(boards[i].GetBoardType() == GUITAR || boards[i].GetBoardType() == BASS) ? 1 : 0][Board.boardBeatsIndex[(int)lenvals[i].Y]]);
                        else
                            engine.Parameters["diffuseTexture"].SetValue(Board.SPBoardTex);
                    }
                    if (SP)
                        engine.Parameters["wAlpha"].SetValue(0.5f);
                    engine.CommitChanges();

                    vb = new VertexBuffer(graphics.GraphicsDevice, Board.arrBoard.Length * GBVertexFormat.SizeInBytes, BufferUsage.WriteOnly);
                    vb.SetData<GBVertexFormat>(Board.arrBoard);

                    // 5: draw object - select vertex type, primitive type, # of primitives
                    graphics.GraphicsDevice.VertexDeclaration = vd;
                    graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                    graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                    graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                    graphics.GraphicsDevice.Vertices[0].SetSource(vb, 0, GBVertexFormat.SizeInBytes);
                    graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, (vb.SizeInBytes / GBVertexFormat.SizeInBytes) / 3);
                    graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;

                    for (int r = 0; r < Board.arrBoard.Length; r++)
                    {
                        Board.arrBoard[r].TexCoord.Y = old[r];
                        Board.arrBoard[r].Alpha = 1f;
                    }
                }*/
            }
        }

        private void DrawWaves(int i, Matrix fling, Matrix matTransl)
        {
            Matrix matScale, matOrbit;
            matOrbit = Matrix.CreateTranslation(0f, 0.01f, /*-(Board.length * Math.Min(lenvals[0].X, Board.sFade))*/ - Board.zeroZ - (Board.eFade*Board.length)) * fling;
            matScale = Matrix.CreateScale(new Vector3(Board.width, Board.curveHeight, Board.length*Board.eFade*(1/ratio)));
            engine.Parameters["world"].SetValue(matScale * matOrbit * matTransl);
            engine.Parameters["diffuseTexture"].SetValue(boards[i].texWaves);
            engine.Parameters["wAlpha"].SetValue(1.0f);
            engine.CommitChanges();
            graphics.GraphicsDevice.VertexDeclaration = vd;
            graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
            graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
            graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
            graphics.GraphicsDevice.Vertices[0].SetSource(Board.mdlBoard, 0, GBVertexFormat.SizeInBytes);
            graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, (Board.mdlBoard.SizeInBytes / GBVertexFormat.SizeInBytes) / 3);
            graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
            
            /*
            bool hide = false;
            engine.Parameters["SpecularEnabled"].SetValue(false);
            engine.Parameters["fullbright"].SetValue(true);
            engine.Parameters["BumpMappingEnabled"].SetValue(false);
            VertexBuffer vb;
            int lefty = 1;
            if (boards[i].IsLefty())
                lefty = -1;
            GBVertexFormat[] tmpMdl = new GBVertexFormat[6];
            if (!boards[i].getWaves(currenttime, 3000))
                return;
            tmpMdl[0] = new GBVertexFormat(new Vector3(0, 0f, 0f), new Vector3(0,1,0), new Vector2(0f, 1f));
            tmpMdl[1] = new GBVertexFormat(new Vector3(0, 0f, 1f), new Vector3(0,1,0), new Vector2(0f, 0f));
            tmpMdl[2] = new GBVertexFormat(new Vector3(1, 0f, 0f), new Vector3(0,1,0), new Vector2(1f, 1f));
            tmpMdl[3] = new GBVertexFormat(new Vector3(0, 0f, 1f), new Vector3(0,1,0), new Vector2(0f, 0f));
            tmpMdl[4] = new GBVertexFormat(new Vector3(1, 0f, 0f), new Vector3(0,1,0), new Vector2(1f, 1f));
            tmpMdl[5] = new GBVertexFormat(new Vector3(1, 0f, 1f), new Vector3(0,1,0), new Vector2(1f, 0f));
            float w = 0.5f;
            for (int p = 0; p < boards[i].wavesLen; p++)
            {
                for (int r = 0; r < 5; r++)
                {
                    if ((boards[i].waves[p][0].Z & bits[r]) > 0)
                    {
                        for (int q = 0; q < boards[i].wavesSubLen[p] - 1; q++)
                        {
                            float loA, hiA;
                            if (boards[i].waves[p][q].Y / 1000f < Board.sFade)
                                loA = 1;
                            else if (boards[i].waves[p][q].Y / 1000f < Board.eFade)
                                loA = 1 - (((boards[i].waves[p][q].Y / 1000f) - Board.sFade) / (Board.eFade - Board.sFade));
                            else
                                loA = 0;
                            if (boards[i].waves[p][q + 1].Y / 1000f < Board.sFade)
                                hiA = 1;
                            else if (boards[i].waves[p][q + 1].Y / 1000f < Board.eFade)
                                hiA = 1 - (((boards[i].waves[p][q + 1].Y / 1000f) - Board.sFade) / (Board.eFade - Board.sFade));
                            else
                                hiA = 0;
                            tmpMdl[0].Position.X = boards[i].waves[p][q].X;
                            tmpMdl[1].Position.X = boards[i].waves[p][q + 1].X;
                            tmpMdl[3].Position.X = boards[i].waves[p][q + 1].X;
                            tmpMdl[2].Position.X = boards[i].waves[p][q].X + w;
                            tmpMdl[4].Position.X = boards[i].waves[p][q].X + w;
                            tmpMdl[5].Position.X = boards[i].waves[p][q + 1].X + w;
                            tmpMdl[0].Alpha = loA;
                            tmpMdl[1].Alpha = hiA;
                            tmpMdl[2].Alpha = loA;
                            tmpMdl[3].Alpha = hiA;
                            tmpMdl[4].Alpha = loA;
                            tmpMdl[5].Alpha = hiA;
                            Matrix matIdentity, matTransl, matScale, matOrbit;
                            matIdentity = Matrix.Identity;
                            matTransl = Matrix.CreateTranslation(0f, Board.height, 0f);
                            matOrbit = Matrix.CreateTranslation(((r / 4f * 2) - 1f) * lefty * Board.width * 0.8f, 0.11f, -(Board.length * (boards[i].waves[p][q].Y / 1000f)) - Board.zeroZ) * fling;
                            matScale = Matrix.CreateScale(new Vector3(0.1f * Board.width, 0.01f, ((boards[i].waves[p][q].Y / 1000f) - (boards[i].waves[p][q + 1].Y / 1000f)) * Board.length));

                            // identity, scale, rotate, orbit(translate & rotate), translate
                            engine.Parameters["world"].SetValue(matIdentity * matScale * matOrbit * matTransl);

                            if (q >= boards[i].wavesSubLen[p] - 2)
                                engine.Parameters["diffuseTexture"].SetValue(texLineEnd);
                            else
                                engine.Parameters["diffuseTexture"].SetValue(texLine);
                            engine.Parameters["diffuseColor"].SetValue(FretColorsV4[r]);
                            if(((byte)(boards[i].waves[p][q].Z)&128)!=0)
                                engine.Parameters["diffuseColor"].SetValue(new Vector4(.5f,.5f,.5f,1.0f));
                            engine.CommitChanges();

                            hide = !hide;

                            vb = new VertexBuffer(graphics.GraphicsDevice, tmpMdl.Length * GBVertexFormat.SizeInBytes, BufferUsage.WriteOnly);
                            vb.SetData<GBVertexFormat>(tmpMdl);

                            // 5: draw object - select vertex type, primitive type, # of primitives
                            graphics.GraphicsDevice.VertexDeclaration = vd;
                            graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                            graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                            graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                            graphics.GraphicsDevice.Vertices[0].SetSource(vb, 0, GBVertexFormat.SizeInBytes);
                            graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, (vb.SizeInBytes / GBVertexFormat.SizeInBytes) / 3);
                            graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;

                            tmpMdl[0].Position.X = -boards[i].waves[p][q].X;
                            tmpMdl[1].Position.X = -boards[i].waves[p][q + 1].X;
                            tmpMdl[3].Position.X = -boards[i].waves[p][q + 1].X;
                            tmpMdl[2].Position.X = -boards[i].waves[p][q].X + w;
                            tmpMdl[4].Position.X = -boards[i].waves[p][q].X + w;
                            tmpMdl[5].Position.X = -boards[i].waves[p][q + 1].X + w;

                            vb = new VertexBuffer(graphics.GraphicsDevice, tmpMdl.Length * GBVertexFormat.SizeInBytes, BufferUsage.WriteOnly);
                            vb.SetData<GBVertexFormat>(tmpMdl);

                            graphics.GraphicsDevice.VertexDeclaration = vd;
                            graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                            graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                            graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                            graphics.GraphicsDevice.Vertices[0].SetSource(vb, 0, GBVertexFormat.SizeInBytes);
                            graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, (vb.SizeInBytes / GBVertexFormat.SizeInBytes) / 3);
                            graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                        }
                        
                    }
                }
            }
            engine.Parameters["SpecularEnabled"].SetValue(false);
            engine.Parameters["fullbright"].SetValue(true);
            engine.Parameters["BumpMappingEnabled"].SetValue(false);
            engine.Parameters["diffuseColor"].SetValue(new Vector4(0.8f, 0.8f, 0.8f, 1));*/
        }

        private void DrawNotes(int i, Matrix fling, long currenttime)
        {
            if (i != 1)
            {
                engine.Parameters["bumpTexture"].SetValue(texDefaultBM);
                engine.Parameters["BumpMappingEnabled"].SetValue(false);
                engine.Parameters["fullbright"].SetValue(true);
                engine.Parameters["vertexAlpha"].SetValue(false);
                int lefty = 1;
                if (boards[i].IsLefty() && boards[i].GetBoardType() != PERCUSSIONIST)
                    lefty = -1;
                
                /*for (int p = 0; p < boards[i].notesLen; p++)
                {
                    if (boards[i].GetBoardType() == PERCUSSIONIST && boards[i].OutNotes[p].Z > 1.5)
                    {

                    }
                    else if (boards[i].GetBoardType() == PERCUSSIONIST && boards[i].OutNotes[p].Z > 0.5)
                    {
                        engine.Parameters["specularColor"].SetValue(new Vector4(0, 0, 0, 0));
                        engine.Parameters["SpecularEnabled"].SetValue(false);
                        engine.Parameters["fullbright"].SetValue(true);
                        Matrix matIdentity, matTransl, matRot, matScale, matOrbit;
                        engine.Parameters["diffuseTexture"].SetValue(Board.drumfillTex);
                        for (int k = 0; k < 4; k++)
                        {
                            float height = 0;
                            matRot = Matrix.Identity;
                            if (k == 0 || k == 3)
                            { matRot = Matrix.CreateRotationZ(0.07f * -Math.Sign(k - 2)); height = 0.01f; }
                            if (k == 1 || k == 2)
                            { matRot = Matrix.CreateRotationZ(0.03f * -Math.Sign(k - 2)); height = 0.03f; }
                            matIdentity = Matrix.Identity;
                            matTransl = Matrix.CreateTranslation(0f, Board.height + (boards[i].GetBoardBump() * Board.BOARD_BUMP_COEF), 0f);
                            matOrbit = Matrix.CreateTranslation(((k / 2f) - .75f) * Board.width, height, -(Board.length * boards[i].OutNotes[p + 1].Y) - Board.zeroZ) * fling;
                            matScale = Matrix.CreateScale(new Vector3(Board.width / 4f * ((boards[i].OutNotes[p].X * 2 + 1) / 3f), Board.curveHeight * 0.1f, Board.length * (boards[i].OutNotes[p + 1].Y - boards[i].OutNotes[p].Y)));

                            engine.Parameters["wAlpha"].SetValue(1);
                            engine.Parameters["world"].SetValue(matIdentity * matScale * matRot * matOrbit * matTransl);
                            engine.Parameters["diffuseColor"].SetValue(FretColorsV4[Board.guitarToDrums[k]]);
                            engine.CommitChanges();

                            // 5: draw object - select vertex type, primitive type, # of primitives
                            graphics.GraphicsDevice.VertexDeclaration = vd;
                            graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                            graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                            graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                            graphics.GraphicsDevice.Vertices[0].SetSource(Board.mdlBoard, 0, GBVertexFormat.SizeInBytes);
                            graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, (Board.mdlBoard.SizeInBytes / GBVertexFormat.SizeInBytes) / 3);
                            graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                        }

                        if (boards[i].OutNotes[p].W < 0.5)
                        {


                            matIdentity = Matrix.Identity;
                            matTransl = Matrix.CreateTranslation(0f, Board.height + (boards[i].GetBoardBump() * Board.BOARD_BUMP_COEF)+0.1f, 0f);
                            matOrbit = Matrix.CreateTranslation(0.75f * Board.width, 0f, -(Board.length * (boards[i].OutNotes[p + 1].Y - 0.1f)) - Board.zeroZ) * fling;
                            matScale = Matrix.CreateScale(new Vector3(0.15f * Board.width * boards[i].OutNotes[p].X, 0.3f * boards[i].OutNotes[p].X, 0.3f * boards[i].OutNotes[p].X));

                            float alpha;
                            if (boards[i].OutNotes[p].Y < Board.sFade)
                                alpha = 1;
                            else if (boards[i].OutNotes[p].Y < Board.eFade)
                                alpha = 1 - ((boards[i].OutNotes[p].Y - Board.sFade) / (Board.eFade - Board.sFade));
                            else
                                alpha = 0;


                            engine.Parameters["wAlpha"].SetValue(alpha);

                            // identity, scale, rotate, orbit(translate & rotate), translate
                            engine.Parameters["world"].SetValue(matIdentity * matScale * matOrbit * matTransl);

                            engine.Parameters["diffuseTexture"].SetValue(Board.texNotes[0]);
                            engine.CommitChanges();

                            // 5: draw object - select vertex type, primitive type, # of primitives
                            graphics.GraphicsDevice.VertexDeclaration = vd;
                            graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                            graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                            graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                            foreach (ModelMesh mesh in Board.mdlNoteInside.Meshes)
                            {
                                foreach (ModelMeshPart part in mesh.MeshParts)
                                {
                                    graphics.GraphicsDevice.VertexDeclaration = part.VertexDeclaration;
                                    graphics.GraphicsDevice.Vertices[0].SetSource(mesh.VertexBuffer, part.StreamOffset, part.VertexStride);
                                    graphics.GraphicsDevice.Indices = mesh.IndexBuffer;
                                    graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.BaseVertex, 0, part.NumVertices, part.StartIndex, part.PrimitiveCount);
                                }
                            }
                            foreach (ModelMesh mesh in Board.mdlNote.Meshes)
                            {
                                foreach (ModelMeshPart part in mesh.MeshParts)
                                {
                                    graphics.GraphicsDevice.VertexDeclaration = part.VertexDeclaration;
                                    graphics.GraphicsDevice.Vertices[0].SetSource(mesh.VertexBuffer, part.StreamOffset, part.VertexStride);
                                    graphics.GraphicsDevice.Indices = mesh.IndexBuffer;
                                    graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.BaseVertex, 0, part.NumVertices, part.StartIndex, part.PrimitiveCount);
                                }
                            }
                            graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                        }

                        engine.Parameters["diffuseColor"].SetValue(new Vector4(0.8f, 0.8f, 0.8f, 1f));
                        engine.Parameters["specularColor"].SetValue(new Vector4(1, 1, 1, 1));
                        engine.Parameters["SpecularEnabled"].SetValue(true);
                    }
                }*/
                bool whited = false;
                for (int r = 0; r < 6; r++)
                {
                    if (r < 5)
                        engine.Parameters["diffuseTexture"].SetValue(Board.texNotes[r]);
                    else
                        engine.Parameters["diffuseTexture"].SetValue(Board.texTriggerBorderLit);
                    whited = false;
                    for (int p = 0; p < boards[i].notesLen; p++)
                    {
                        if (boards[i].OutNotes[p].W > 0.5 && !whited)
                        { engine.Parameters["diffuseTexture"].SetValue(texWhite); whited = true; }
                        else if (boards[i].OutNotes[p].W <= 0.5 && whited)
                        {
                            if (r < 5)
                                engine.Parameters["diffuseTexture"].SetValue(Board.texNotes[r]);
                            else
                                engine.Parameters["diffuseTexture"].SetValue(Board.texTriggerBorderLit);
                            whited = false;
                        }

                        if(boards[i].GetBoardType() != PERCUSSIONIST || boards[i].OutNotes[p].Z<=0.5)
                        {
                            if (boards[i].GetBoardType() != PERCUSSIONIST)
                            {
                                if (Math.Abs(boards[i].OutNotes[p].X - (-1)) < 0.01 && r != 0)
                                    continue;
                                else if (Math.Abs(boards[i].OutNotes[p].X - (-0.5)) < 0.01 && r != 1)
                                    continue;
                                else if (Math.Abs(boards[i].OutNotes[p].X) < 0.01 && r != 2)
                                    continue;
                                else if (Math.Abs(boards[i].OutNotes[p].X - (0.5)) < 0.01 && r != 3)
                                    continue;
                                else if (Math.Abs(boards[i].OutNotes[p].X - (1.0)) < 0.01 && r != 4)
                                    continue;
                            }
                            else
                            {
                                if (Math.Abs(boards[i].OutNotes[p].X - (-1)) < 0.01 && r != 1)
                                    continue;
                                else if (Math.Abs(boards[i].OutNotes[p].X - (-1 / 3f)) < 0.01 && r != 2)
                                    continue;
                                else if (Math.Abs(boards[i].OutNotes[p].X) < 0.01 && r != 5)
                                    continue;
                                else if (Math.Abs(boards[i].OutNotes[p].X - (1 / 3f)) < 0.01 && r != 3)
                                    continue;
                                else if (Math.Abs(boards[i].OutNotes[p].X - (1)) < 0.01 && r != 0)
                                    continue;
                            }

                            Matrix matIdentity, matTransl, matScale, matOrbit;
                            matIdentity = Matrix.Identity;
                            if (boards[i].GetBoardType() == PERCUSSIONIST && Math.Abs(boards[i].OutNotes[p].X) < 0.01f)
                                matTransl = Matrix.CreateTranslation(0f, Board.height + (boards[i].GetBoardBump() * Board.BOARD_BUMP_COEF), 0f);
                            else
                                matTransl = Matrix.CreateTranslation(0f, Board.height + (boards[i].GetBoardBump() * Board.BOARD_BUMP_COEF) + 0.02f, 0f);
                            matOrbit = Matrix.CreateTranslation(boards[i].OutNotes[p].X * lefty * Board.width * 0.8f, 0f, -(Board.length * boards[i].OutNotes[p].Y) - Board.zeroZ) * fling;
                            if (boards[i].GetBoardType() != PERCUSSIONIST || Math.Abs(boards[i].OutNotes[p].X) > 0.01f)
                                matOrbit = Matrix.CreateRotationX(-boards[i].OutNotes[p].Y * MathHelper.Pi) * matOrbit;
                            if (boards[i].GetBoardType() == PERCUSSIONIST && Math.Abs(boards[i].OutNotes[p].X) < 0.01f)
                                matScale = Matrix.CreateScale(new Vector3(Board.width, Board.curveHeight, Board.length * 0.01f));
                            else
                                matScale = Matrix.CreateScale(new Vector3(((boards[i].OutNotes[p].Z > 0) ? 0.5f : 1.0f) * 0.125f * Board.width, 0.05f * Board.length, 0.05f * Board.length));

                            float alpha;
                            if (boards[i].OutNotes[p].Y < Board.sFade)
                                alpha = 1;
                            else if (boards[i].OutNotes[p].Y < Board.eFade)
                                alpha = 1 - ((boards[i].OutNotes[p].Y - Board.sFade) / (Board.eFade - Board.sFade));
                            else
                                alpha = 0;


                            engine.Parameters["wAlpha"].SetValue(alpha);

                            // identity, scale, rotate, orbit(translate & rotate), translate
                            engine.Parameters["world"].SetValue(matIdentity * matScale * matOrbit * matTransl);



                            engine.CommitChanges();

                            // 5: draw object - select vertex type, primitive type, # of primitives
                            graphics.GraphicsDevice.VertexDeclaration = vd;
                            graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                            graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                            graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                            if (boards[i].GetBoardType() == PERCUSSIONIST && Math.Abs(boards[i].OutNotes[p].X) < 0.01f)
                            {
                                graphics.GraphicsDevice.Vertices[0].SetSource(Board.mdlTriggerBorder, 0, GBVertexFormat.SizeInBytes);
                                graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, (Board.mdlTriggerBorder.SizeInBytes / GBVertexFormat.SizeInBytes) / 3);
                            }
                            else
                            {
                                foreach (ModelMesh mesh in Board.mdlNoteInside.Meshes)
                                {
                                    foreach (ModelMeshPart part in mesh.MeshParts)
                                    {
                                        //engine.Parameters["diffuseTexture"].SetValue(texWhite);
                                        engine.CommitChanges();
                                        graphics.GraphicsDevice.VertexDeclaration = part.VertexDeclaration;
                                        graphics.GraphicsDevice.Vertices[0].SetSource(mesh.VertexBuffer, part.StreamOffset, part.VertexStride);
                                        graphics.GraphicsDevice.Indices = mesh.IndexBuffer;
                                        graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.BaseVertex, 0, part.NumVertices, part.StartIndex, part.PrimitiveCount);
                                    }
                                }
                                foreach (ModelMesh mesh in Board.mdlNote.Meshes)
                                {
                                    foreach (ModelMeshPart part in mesh.MeshParts)
                                    {

                                        //engine.Parameters["diffuseTexture"].SetValue(texWhite);
                                        engine.CommitChanges();
                                        graphics.GraphicsDevice.VertexDeclaration = part.VertexDeclaration;
                                        graphics.GraphicsDevice.Vertices[0].SetSource(mesh.VertexBuffer, part.StreamOffset, part.VertexStride);
                                        graphics.GraphicsDevice.Indices = mesh.IndexBuffer;
                                        graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.BaseVertex, 0, part.NumVertices, part.StartIndex, part.PrimitiveCount);
                                    }
                                }
                            }
                            graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                        }
                    }
                }
                engine.Parameters["wAlpha"].SetValue(1.0f);
                engine.Parameters["fullbright"].SetValue(false);
                engine.Parameters["vertexAlpha"].SetValue(true);
            }
        }

        int[] gloworder = { 0, 4, 1, 3, 2 };
        private void DrawBoardDetail(int i, Matrix fling, Matrix matTransl)
        {
            if (i != 1)
            {
                engine.Parameters["fullbright"].SetValue(true);
                int lefty = 1;
                if (boards[i].IsLefty())
                    lefty = -1;

                if (boards[i].GetBoardType() == GUITAR || boards[i].GetBoardType() == BASS)
                {
                    Matrix matIdentity, matScale, matOrbit;

                    bool[] glow = new bool[5];
                    for (int p = 0; p < 5; p++)
                    {
                        float rise = -.01f;

                        matIdentity = Matrix.Identity;
                        //matTransl = Matrix.CreateTranslation(0f, Board.height + (boards[i].GetBoardBump() * Board.BOARD_BUMP_COEF), 0f);
                        matTransl = Matrix.CreateTranslation((-.8f + (p * 0.4f)) * lefty * Board.width, Board.height + (boards[i].GetBoardBump() * Board.BOARD_BUMP_COEF), 0f);
                        matOrbit = Matrix.CreateTranslation(0f, Board.curveHeight * (1 - Math.Abs(-.8f + (p * 0.4f))) + rise + ((boards[i].GetPopups()[p]) * 0.001f), -Board.zeroZ) * fling;
                        matScale = Matrix.CreateScale(new Vector3((Board.width / 5f), 0.05f, .05f));

                        // identity, scale, rotate, orbit(translate & rotate), translate
                        engine.Parameters["world"].SetValue(matIdentity * matScale * matOrbit * matTransl);

                        engine.Parameters["proj"].SetValue(matProj);

                        engine.Parameters["diffuseTexture"].SetValue(Board.texTriggers[p]);
                        if (contInput[boards[i].GetBoardType()] < 4)
                        {
                            if (p == 0 && controllers[contInput[i]].Buttons.A == ButtonState.Pressed)
                            { engine.Parameters["diffuseTexture"].SetValue(Board.texTriggersLit[p]); glow[p] = true; }
                            if (p == 1 && controllers[contInput[i]].Buttons.B == ButtonState.Pressed)
                            { engine.Parameters["diffuseTexture"].SetValue(Board.texTriggersLit[p]); glow[p] = true; }
                            if (p == 2 && controllers[contInput[i]].Buttons.Y == ButtonState.Pressed)
                            { engine.Parameters["diffuseTexture"].SetValue(Board.texTriggersLit[p]); glow[p] = true; }
                            if (p == 3 && controllers[contInput[i]].Buttons.X == ButtonState.Pressed)
                            { engine.Parameters["diffuseTexture"].SetValue(Board.texTriggersLit[p]); glow[p] = true; }
                            if (p == 4 && controllers[contInput[i]].Buttons.LeftShoulder == ButtonState.Pressed)
                            { engine.Parameters["diffuseTexture"].SetValue(Board.texTriggersLit[p]); glow[p] = true; }
                        }
                        else if (contInput[boards[i].GetBoardType()] == 4 && boards[i].IsLefty())
                        {
                            if (p == 0 && Keyboard.GetState().IsKeyDown(Keys.G))
                            { engine.Parameters["diffuseTexture"].SetValue(Board.texTriggersLit[0]); glow[0] = true; }
                            if (p == 1 && Keyboard.GetState().IsKeyDown(Keys.F))
                            { engine.Parameters["diffuseTexture"].SetValue(Board.texTriggersLit[1]); glow[1] = true; }
                            if (p == 2 && Keyboard.GetState().IsKeyDown(Keys.D))
                            { engine.Parameters["diffuseTexture"].SetValue(Board.texTriggersLit[2]); glow[2] = true; }
                            if (p == 3 && Keyboard.GetState().IsKeyDown(Keys.S))
                            { engine.Parameters["diffuseTexture"].SetValue(Board.texTriggersLit[3]); glow[3] = true; }
                            if (p == 4 && Keyboard.GetState().IsKeyDown(Keys.A))
                            { engine.Parameters["diffuseTexture"].SetValue(Board.texTriggersLit[4]); glow[4] = true; }
                        }
                        else if (contInput[boards[i].GetBoardType()] == 4 && !boards[i].IsLefty())
                        {
                            if (p == 4 && Keyboard.GetState().IsKeyDown(Keys.G))
                            { engine.Parameters["diffuseTexture"].SetValue(Board.texTriggersLit[4]); glow[4] = true; }
                            if (p == 3 && Keyboard.GetState().IsKeyDown(Keys.F))
                            { engine.Parameters["diffuseTexture"].SetValue(Board.texTriggersLit[3]); glow[3] = true; }
                            if (p == 2 && Keyboard.GetState().IsKeyDown(Keys.D))
                            { engine.Parameters["diffuseTexture"].SetValue(Board.texTriggersLit[2]); glow[2] = true; }
                            if (p == 1 && Keyboard.GetState().IsKeyDown(Keys.S))
                            { engine.Parameters["diffuseTexture"].SetValue(Board.texTriggersLit[1]); glow[1] = true; }
                            if (p == 0 && Keyboard.GetState().IsKeyDown(Keys.A))
                            { engine.Parameters["diffuseTexture"].SetValue(Board.texTriggersLit[0]); glow[0] = true; }
                        }

                        engine.CommitChanges();

                        // 5: draw object - select vertex type, primitive type, # of primitives
                        graphics.GraphicsDevice.VertexDeclaration = vd;
                        graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                        graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                        graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                        graphics.GraphicsDevice.Vertices[0].SetSource(Board.mdlTrigger, 0, GBVertexFormat.SizeInBytes);
                        graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, (Board.mdlTrigger.SizeInBytes / GBVertexFormat.SizeInBytes) / 3);
                        graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                    }
                    engine.Parameters["fullbright"].SetValue(true);
                    float[] pop = boards[i].GetPopups();
                    /*for (int p = 0; p < 5; p++)
                    {

                        matIdentity = Matrix.Identity;
                        matTransl = Matrix.CreateTranslation((-.8f + (p * 0.4f)) * lefty * Board.width, Board.height + (boards[i].GetBoardBump() * Board.BOARD_BUMP_COEF), 0f);
                        matOrbit = Matrix.CreateTranslation(0f, Board.curveHeight * (1 - Math.Abs(-.8f + (p * 0.4f))) + 0.1f, -Board.zeroZ) * fling;
                        matScale = Matrix.CreateScale(new Vector3((Board.width / 4f), 1f, (Board.width / 6f)));



                        // identity, scale, rotate, orbit(translate & rotate), translate
                        engine.Parameters["world"].SetValue(matIdentity * matScale * matOrbit * matTransl);

                        engine.Parameters["proj"].SetValue(matProj);
                        engine.Parameters["diffuseColor"].SetValue(new Vector4(FretColors[p].ToVector3(), 0.5f));
                        engine.Parameters["wAlpha"].SetValue(pop[i]);
                        engine.Parameters["diffuseTexture"].SetValue(texGlow);
                        engine.CommitChanges();

                        // 5: draw object - select vertex type, primitive type, # of primitives
                        graphics.GraphicsDevice.VertexDeclaration = vd;
                        graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                        graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                        graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                        graphics.GraphicsDevice.Vertices[0].SetSource(square, 0, GBVertexFormat.SizeInBytes);
                        //graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
                        graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                    }*/
                    engine.Parameters["fullbright"].SetValue(false);
                    engine.Parameters["wAlpha"].SetValue(1);

                }
                else if (boards[i].GetBoardType() == PERCUSSIONIST)
                {


                    bool[] glow = new bool[5];
                    float[] pop = boards[i].GetPopups();
                    for (int k = 0; k < 5; k++)
                        glow[k] = pop[k] > 0;
                    {
                        Matrix matIdentity, matScale, matOrbit;
                        matIdentity = Matrix.Identity;
                        matTransl = Matrix.CreateTranslation(0f, Board.height + (boards[i].GetBoardBump() * Board.BOARD_BUMP_COEF), 0f);
                        matOrbit = Matrix.CreateTranslation(0f, 0f, -(0.08f) - Board.zeroZ) * fling;
                        matScale = Matrix.CreateScale(new Vector3(Board.width, Board.curveHeight, Board.length * 0.005f));

                        // identity, scale, rotate, orbit(translate & rotate), translate
                        engine.Parameters["world"].SetValue(matIdentity * matScale * matOrbit * matTransl);
                        engine.Parameters["diffuseTexture"].SetValue(Board.texTriggerBorder);
                        engine.CommitChanges();

                        // 5: draw object - select vertex type, primitive type, # of primitives
                        graphics.GraphicsDevice.VertexDeclaration = vd;
                        graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                        graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                        graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                        graphics.GraphicsDevice.Vertices[0].SetSource(Board.mdlTriggerBorder, 0, GBVertexFormat.SizeInBytes);
                        graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, (Board.mdlTriggerBorder.SizeInBytes / GBVertexFormat.SizeInBytes) / 3);
                        graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                    }
                    for (int p = 0; p < 4; p++)
                    {
                        Matrix matIdentity, matScale, matOrbit;
                        float rise = -.01f;
                            matIdentity = Matrix.Identity;
                            matTransl = Matrix.CreateTranslation((-.75f + (p * .5f)) * (Board.width), Board.height + (boards[i].GetBoardBump() * Board.BOARD_BUMP_COEF), 0f);
                            matOrbit = Matrix.CreateTranslation(0f, Board.curveHeight * (1 - Math.Abs(-.8f + (p * 0.4f))) + rise + ((boards[i].GetPopups()[p]) * 0.001f), -Board.zeroZ) * fling;
                            matScale = Matrix.CreateScale(new Vector3((Board.width / 4f), 0.05f, .05f));

                            // identity, scale, rotate, orbit(translate & rotate), translate
                            engine.Parameters["world"].SetValue(matIdentity * matScale * matOrbit * matTransl);

                            engine.Parameters["diffuseTexture"].SetValue(Board.texTriggers[Board.guitarToDrums[p]]);
                        if (glow[p] && p < 4)
                            engine.Parameters["diffuseTexture"].SetValue(Board.texTriggersLit[Board.guitarToDrums[p]]);

                        engine.CommitChanges();
                        // 5: draw object - select vertex type, primitive type, # of primitives
                        graphics.GraphicsDevice.VertexDeclaration = vd;
                        graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                        graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                        graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                        graphics.GraphicsDevice.Vertices[0].SetSource(Board.mdlTrigger, 0, GBVertexFormat.SizeInBytes);
                        graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, (Board.mdlTrigger.SizeInBytes / GBVertexFormat.SizeInBytes) / 3);
                        graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;

                    }
                    {
                        Matrix matIdentity, matScale, matOrbit;
                        matIdentity = Matrix.Identity;
                        matTransl = Matrix.CreateTranslation(0f, Board.height + (boards[i].GetBoardBump() * Board.BOARD_BUMP_COEF), 0f);
                        matOrbit = Matrix.CreateTranslation(0f, 0f, (0.08f) - Board.zeroZ) * fling;
                        matScale = Matrix.CreateScale(new Vector3(Board.width, Board.curveHeight, Board.length * 0.005f));

                        // identity, scale, rotate, orbit(translate & rotate), translate
                        engine.Parameters["world"].SetValue(matIdentity * matScale * matOrbit * matTransl);
                        engine.Parameters["diffuseTexture"].SetValue(Board.texTriggerBorder);
                        engine.CommitChanges();

                        // 5: draw object - select vertex type, primitive type, # of primitives
                        graphics.GraphicsDevice.VertexDeclaration = vd;
                        graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                        graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                        graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                        graphics.GraphicsDevice.Vertices[0].SetSource(Board.mdlTriggerBorder, 0, GBVertexFormat.SizeInBytes);
                        graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, (Board.mdlTriggerBorder.SizeInBytes / GBVertexFormat.SizeInBytes) / 3);
                        graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                    }
                    engine.Parameters["fullbright"].SetValue(true);
                    for (int p = 0; p < 4; p++)
                    {
                        if (glow[p])
                        {
                            Matrix matIdentity, matScale, matOrbit;

                            matIdentity = Matrix.Identity;
                            matTransl = Matrix.CreateTranslation((-0.75f + (p * .5f)) * Board.width, Board.height + (boards[i].GetBoardBump() * Board.BOARD_BUMP_COEF), 0f);
                            matOrbit = Matrix.CreateTranslation(0f, Board.curveHeight * (1 - Math.Abs(-.8f + (gloworder[p] * 0.4f))) + 0.1f, -Board.zeroZ) * fling;
                            matScale = Matrix.CreateScale(new Vector3((Board.width / 4f), 1f, (Board.width / 6f)));

                            // identity, scale, rotate, orbit(translate & rotate), translate
                            engine.Parameters["world"].SetValue(matIdentity * matScale * matOrbit * matTransl);

                            if (boards[i].GetBoardType() != PERCUSSIONIST)
                                engine.Parameters["diffuseColor"].SetValue(FretColorsV4[p]);
                            else
                                engine.Parameters["diffuseColor"].SetValue(FretColorsV4[Board.guitarToDrums[p]]);
                            engine.Parameters["specularColor"].SetValue(new Vector4(0, 0, 0, 1));

                            engine.Parameters["diffuseTexture"].SetValue(texGlow);
                            engine.CommitChanges();

                            // 5: draw object - select vertex type, primitive type, # of primitives
                            graphics.GraphicsDevice.VertexDeclaration = vd;
                            graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                            graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                            graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                            graphics.GraphicsDevice.Vertices[0].SetSource(square, 0, GBVertexFormat.SizeInBytes);
                            graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
                            graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                        }
                    }

                    engine.Parameters["diffuseColor"].SetValue(new Vector4(0.8f, 0.8f, 0.8f, 1.0f));
                    engine.Parameters["specularColor"].SetValue(new Vector4(1, 1, 1, 1));
                    engine.Parameters["fullbright"].SetValue(false);
                }
                engine.Parameters["wAlpha"].SetValue(1.0f);
                engine.Parameters["fullbright"].SetValue(false);
            }
        }
        private void DrawBoardDetailFS(int i, Matrix fling, Matrix matTransl)
        {
            if (i != 1)
            {
                engine.Parameters["fullbright"].SetValue(true);
                int lefty = 1;
                if (FSIsLefty[i])
                    lefty = -1;

                if (i!=2)
                {
                    Matrix matIdentity, matScale, matOrbit;

                    bool[] glow = new bool[5];
                    for (int p = 0; p < 5; p++)
                    {
                        float rise = -.01f;

                        matIdentity = Matrix.Identity;
                        //matTransl = Matrix.CreateTranslation(0f, Board.height + (boards[i].GetBoardBump() * Board.BOARD_BUMP_COEF), 0f);
                        matTransl = Matrix.CreateTranslation((-.8f + (p * 0.4f)) * lefty * Board.width, Board.height, 0f);
                        matOrbit = Matrix.CreateTranslation(0f, Board.curveHeight * (1 - Math.Abs(-.8f + (p * 0.4f))) + rise, -Board.zeroZ) * fling;
                        matScale = Matrix.CreateScale(new Vector3((Board.width / 5f), 0.05f, .05f));

                        // identity, scale, rotate, orbit(translate & rotate), translate
                        engine.Parameters["world"].SetValue(matIdentity * matScale * matOrbit * matTransl);

                        engine.Parameters["proj"].SetValue(matProj);

                        engine.Parameters["diffuseTexture"].SetValue(Board.texTriggers[p]);
                        if (contInput[i] < 4)
                        {
                            if (p == 0 && controllers[contInput[i]].Buttons.A == ButtonState.Pressed)
                            { engine.Parameters["diffuseTexture"].SetValue(Board.texTriggersLit[p]); glow[p] = true; }
                            if (p == 1 && controllers[contInput[i]].Buttons.B == ButtonState.Pressed)
                            { engine.Parameters["diffuseTexture"].SetValue(Board.texTriggersLit[p]); glow[p] = true; }
                            if (p == 2 && controllers[contInput[i]].Buttons.Y == ButtonState.Pressed)
                            { engine.Parameters["diffuseTexture"].SetValue(Board.texTriggersLit[p]); glow[p] = true; }
                            if (p == 3 && controllers[contInput[i]].Buttons.X == ButtonState.Pressed)
                            { engine.Parameters["diffuseTexture"].SetValue(Board.texTriggersLit[p]); glow[p] = true; }
                            if (p == 4 && controllers[contInput[i]].Buttons.LeftShoulder == ButtonState.Pressed)
                            { engine.Parameters["diffuseTexture"].SetValue(Board.texTriggersLit[p]); glow[p] = true; }
                        }
                        else if (contInput[i] == 4 && FSIsLefty[i])
                        {
                            if (p == 0 && Keyboard.GetState().IsKeyDown(Keys.G))
                            { engine.Parameters["diffuseTexture"].SetValue(Board.texTriggersLit[0]); glow[0] = true; }
                            if (p == 1 && Keyboard.GetState().IsKeyDown(Keys.F))
                            { engine.Parameters["diffuseTexture"].SetValue(Board.texTriggersLit[1]); glow[1] = true; }
                            if (p == 2 && Keyboard.GetState().IsKeyDown(Keys.D))
                            { engine.Parameters["diffuseTexture"].SetValue(Board.texTriggersLit[2]); glow[2] = true; }
                            if (p == 3 && Keyboard.GetState().IsKeyDown(Keys.S))
                            { engine.Parameters["diffuseTexture"].SetValue(Board.texTriggersLit[3]); glow[3] = true; }
                            if (p == 4 && Keyboard.GetState().IsKeyDown(Keys.A))
                            { engine.Parameters["diffuseTexture"].SetValue(Board.texTriggersLit[4]); glow[4] = true; }
                        }
                        else if (contInput[i] == 4 && !FSIsLefty[i])
                        {
                            if (p == 4 && Keyboard.GetState().IsKeyDown(Keys.G))
                            { engine.Parameters["diffuseTexture"].SetValue(Board.texTriggersLit[4]); glow[4] = true; }
                            if (p == 3 && Keyboard.GetState().IsKeyDown(Keys.F))
                            { engine.Parameters["diffuseTexture"].SetValue(Board.texTriggersLit[3]); glow[3] = true; }
                            if (p == 2 && Keyboard.GetState().IsKeyDown(Keys.D))
                            { engine.Parameters["diffuseTexture"].SetValue(Board.texTriggersLit[2]); glow[2] = true; }
                            if (p == 1 && Keyboard.GetState().IsKeyDown(Keys.S))
                            { engine.Parameters["diffuseTexture"].SetValue(Board.texTriggersLit[1]); glow[1] = true; }
                            if (p == 0 && Keyboard.GetState().IsKeyDown(Keys.A))
                            { engine.Parameters["diffuseTexture"].SetValue(Board.texTriggersLit[0]); glow[0] = true; }
                        }

                        engine.CommitChanges();

                        // 5: draw object - select vertex type, primitive type, # of primitives
                        graphics.GraphicsDevice.VertexDeclaration = vd;
                        graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                        graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                        graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                        graphics.GraphicsDevice.Vertices[0].SetSource(Board.mdlTrigger, 0, GBVertexFormat.SizeInBytes);
                        graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, (Board.mdlTrigger.SizeInBytes / GBVertexFormat.SizeInBytes) / 3);
                        graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                    }
                    engine.Parameters["fullbright"].SetValue(true);
                    //float[] pop = boards[i].GetPopups();
                    /*for (int p = 0; p < 5; p++)
                    {

                        matIdentity = Matrix.Identity;
                        matTransl = Matrix.CreateTranslation((-.8f + (p * 0.4f)) * lefty * Board.width, Board.height + (boards[i].GetBoardBump() * Board.BOARD_BUMP_COEF), 0f);
                        matOrbit = Matrix.CreateTranslation(0f, Board.curveHeight * (1 - Math.Abs(-.8f + (p * 0.4f))) + 0.1f, -Board.zeroZ) * fling;
                        matScale = Matrix.CreateScale(new Vector3((Board.width / 4f), 1f, (Board.width / 6f)));



                        // identity, scale, rotate, orbit(translate & rotate), translate
                        engine.Parameters["world"].SetValue(matIdentity * matScale * matOrbit * matTransl);

                        engine.Parameters["proj"].SetValue(matProj);
                        engine.Parameters["diffuseColor"].SetValue(new Vector4(FretColors[p].ToVector3(), 0.5f));
                        engine.Parameters["wAlpha"].SetValue(pop[i]);
                        engine.Parameters["diffuseTexture"].SetValue(texGlow);
                        engine.CommitChanges();

                        // 5: draw object - select vertex type, primitive type, # of primitives
                        graphics.GraphicsDevice.VertexDeclaration = vd;
                        graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                        graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                        graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                        graphics.GraphicsDevice.Vertices[0].SetSource(square, 0, GBVertexFormat.SizeInBytes);
                        //graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
                        graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                    }*/
                    engine.Parameters["fullbright"].SetValue(false);
                    engine.Parameters["wAlpha"].SetValue(1);

                }
                else if (i==2)
                {


                    bool[] glow = new bool[5];
                    //float[] pop = boards[i].GetPopups();
                    /*for (int k = 0; k < 5; k++)
                        glow[k] = pop[k] > 0;*/
                    for (int p = 0; p < 5; p++)
                    {
                        Matrix matIdentity, matScale, matOrbit;
                        float rise = -.01f;
                        if (p < 4)
                        {
                            matIdentity = Matrix.Identity;
                            matTransl = Matrix.CreateTranslation((-.75f + (p * .5f)) * (Board.width), Board.height, 0f);
                            matOrbit = Matrix.CreateTranslation(0f, Board.curveHeight * (1 - Math.Abs(-.8f + (p * 0.4f))) + rise, -Board.zeroZ) * fling;
                            matScale = Matrix.CreateScale(new Vector3((Board.width / 4f), 0.05f, .05f));

                            // identity, scale, rotate, orbit(translate & rotate), translate
                            engine.Parameters["world"].SetValue(matIdentity * matScale * matOrbit * matTransl);

                            engine.Parameters["diffuseTexture"].SetValue(Board.texTriggers[Board.guitarToDrums[p]]);
                        }
                        else
                            engine.Parameters["diffuseTexture"].SetValue(Board.texTriggerBorder);
                        if (glow[p] && p < 4)
                            engine.Parameters["diffuseTexture"].SetValue(Board.texTriggersLit[Board.guitarToDrums[p]]);

                        engine.CommitChanges();

                        if (p < 4)
                        {
                            // 5: draw object - select vertex type, primitive type, # of primitives
                            graphics.GraphicsDevice.VertexDeclaration = vd;
                            graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                            graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                            graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                            graphics.GraphicsDevice.Vertices[0].SetSource(Board.mdlTrigger, 0, GBVertexFormat.SizeInBytes);
                            graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, (Board.mdlTrigger.SizeInBytes / GBVertexFormat.SizeInBytes) / 3);
                            graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                        }
                        else
                        {
                            matIdentity = Matrix.Identity;
                            matTransl = Matrix.CreateTranslation(0f, Board.height, 0f);
                            matOrbit = Matrix.CreateTranslation(0f, 0f, -(0.04f) - Board.zeroZ) * fling;
                            matScale = Matrix.CreateScale(new Vector3(Board.width, Board.curveHeight, Board.length * 0.005f));

                            // identity, scale, rotate, orbit(translate & rotate), translate
                            engine.Parameters["world"].SetValue(matIdentity * matScale * matOrbit * matTransl);
                            engine.CommitChanges();

                            // 5: draw object - select vertex type, primitive type, # of primitives
                            graphics.GraphicsDevice.VertexDeclaration = vd;
                            graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                            graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                            graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                            graphics.GraphicsDevice.Vertices[0].SetSource(Board.mdlTriggerBorder, 0, GBVertexFormat.SizeInBytes);
                            graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, (Board.mdlTriggerBorder.SizeInBytes / GBVertexFormat.SizeInBytes) / 3);
                            graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;

                            matIdentity = Matrix.Identity;
                            matTransl = Matrix.CreateTranslation(0f, Board.height, 0f);
                            matOrbit = Matrix.CreateTranslation(0f, 0f, (0.04f) - Board.zeroZ) * fling;
                            matScale = Matrix.CreateScale(new Vector3(Board.width, Board.curveHeight, Board.length * 0.005f));

                            // identity, scale, rotate, orbit(translate & rotate), translate
                            engine.Parameters["world"].SetValue(matIdentity * matScale * matOrbit * matTransl);
                            engine.CommitChanges();

                            // 5: draw object - select vertex type, primitive type, # of primitives
                            graphics.GraphicsDevice.VertexDeclaration = vd;
                            graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                            graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                            graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                            graphics.GraphicsDevice.Vertices[0].SetSource(Board.mdlTriggerBorder, 0, GBVertexFormat.SizeInBytes);
                            graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, (Board.mdlTriggerBorder.SizeInBytes / GBVertexFormat.SizeInBytes) / 3);
                            graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                        }

                    }
                    engine.Parameters["fullbright"].SetValue(true);
                    for (int p = 0; p < 4; p++)
                    {
                        if (glow[p])
                        {
                            Matrix matIdentity, matScale, matOrbit;

                            matIdentity = Matrix.Identity;
                            matTransl = Matrix.CreateTranslation((-0.75f + (p * .5f)) * Board.width, Board.height, 0f);
                            matOrbit = Matrix.CreateTranslation(0f, Board.curveHeight * (1 - Math.Abs(-.8f + (gloworder[p] * 0.4f))) + 0.1f, -Board.zeroZ) * fling;
                            matScale = Matrix.CreateScale(new Vector3((Board.width / 4f), 1f, (Board.width / 6f)));

                            // identity, scale, rotate, orbit(translate & rotate), translate
                            engine.Parameters["world"].SetValue(matIdentity * matScale * matOrbit * matTransl);

                            if (boards[i].GetBoardType() != PERCUSSIONIST)
                                engine.Parameters["diffuseColor"].SetValue(FretColorsV4[p]);
                            else
                                engine.Parameters["diffuseColor"].SetValue(FretColorsV4[Board.guitarToDrums[p]]);
                            engine.Parameters["specularColor"].SetValue(new Vector4(0, 0, 0, 1));

                            engine.Parameters["diffuseTexture"].SetValue(texGlow);
                            engine.CommitChanges();

                            // 5: draw object - select vertex type, primitive type, # of primitives
                            graphics.GraphicsDevice.VertexDeclaration = vd;
                            graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                            graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                            graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                            graphics.GraphicsDevice.Vertices[0].SetSource(square, 0, GBVertexFormat.SizeInBytes);
                            graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
                            graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                        }
                    }

                    engine.Parameters["diffuseColor"].SetValue(new Vector4(0.8f, 0.8f, 0.8f, 1.0f));
                    engine.Parameters["specularColor"].SetValue(new Vector4(1, 1, 1, 1));
                    engine.Parameters["fullbright"].SetValue(false);
                }
                engine.Parameters["wAlpha"].SetValue(1.0f);
                engine.Parameters["fullbright"].SetValue(false);
            }
        }

        private byte[] logoslots = new byte[11];
        private Color rmColor;

        private void DrawRockMeter(long currenttime)
        {
            

            float rmFill = GetRockMeterFill();
            if (cGUIStyle == GUIStyle.UN)
            {
                rmColor = new Color(rmFill < 0.66 ? (byte)255 : (byte)0,
                                    rmFill > 0.33 ? (byte)255 : (byte)0,
                                    0);
                int height = (int)(GameSettings.windowheight*0.2f);
#if WINDOWS
                spritebatch.Draw(rmUNbg, new Rectangle(0, (GameSettings.windowheight / 2) - (int)(height * 0.75f), (int)(height * 0.5f), (int)(height*1.5f)), Color.White);
                spritebatch.Draw(rmUNfg, new Rectangle(0, (GameSettings.windowheight / 2), height / 2, height), null, rmColor, MathHelper.Pi - (rmFill * MathHelper.Pi), new Vector2(0, rmUNfg.Height / 2), SpriteEffects.None, 0);
                spritebatch.Draw(rmUNbg, new Rectangle(GameSettings.windowwidth, (GameSettings.windowheight / 2), (int)(height * 0.5f), (int)(height*1.5f)), null, Color.White, MathHelper.Pi, new Vector2(0,rmUNbg.Height/2),SpriteEffects.None,0);
                spritebatch.Draw(rmUNfg, new Rectangle(GameSettings.windowwidth, (GameSettings.windowheight / 2), height / 2, height), null, Color.Wheat, (song.PercentSong()*MathHelper.Pi),new Vector2(0,rmUNfg.Height/2),SpriteEffects.None,0);
#else
                spritebatch.Draw(rmUNbg, new Rectangle((int)(GameSettings.windowwidth*0), (GameSettings.windowheight / 2) - (int)(height * 0.75f), (int)(height * 0.5f), (int)(height*1.5f)), Color.White);
                spritebatch.Draw(rmUNfg, new Rectangle((int)(GameSettings.windowwidth * 0), (GameSettings.windowheight / 2), height / 2, height), null, rmColor, MathHelper.Pi - (rmFill * MathHelper.Pi), new Vector2(0, rmUNfg.Height / 2), SpriteEffects.None, 0);
                spritebatch.Draw(rmUNbg, new Rectangle((int)(GameSettings.windowwidth*1f), (GameSettings.windowheight / 2), (int)(height * 0.5f), (int)(height*1.5f)), null, Color.White, MathHelper.Pi, new Vector2(0,rmUNbg.Height/2),SpriteEffects.None,0);
                spritebatch.Draw(rmUNfg, new Rectangle((int)(GameSettings.windowwidth*1f), (GameSettings.windowheight / 2), height / 2, height), null, Color.Wheat, (song.PercentSong()*MathHelper.Pi),new Vector2(0,rmUNfg.Height/2),SpriteEffects.None,0);
#endif
            }
            else if (cGUIStyle == GUIStyle.RB)
            {
                rmColor = Color.Black;
                
                rmColor = new Color(rmFill < 0.66 ? (byte)255 : (byte)128,
                                    rmFill > 0.33 ? (byte)255 : (byte)128,
                                    127);
                float rex;
                if (currenttime / 1000f > -2.5)
                    rex = rockMeterLoc.X;
                else if ((currenttime / 1000f) > -3)
                    rex = -(rockMeterScale.X * 3) + ((((currenttime / 1000f) + 3) * 2) * ((rockMeterLoc.X * 3) + rockMeterScale.X));
                else
                    rex = -rockMeterScale.X * 3;
                if (failTime > 0)
                {
                    if (song.percentBeat > 0.5)
                    {
                        spritebatch.Draw(texWhite, new Rectangle((int)((rockMeterScale.X * 0.2f) + rex), (int)((rockMeterScale.Y * 0.025f) + rockMeterLoc.Y + (rockMeterScale.Y * (1 - (failTime / 3)) * 0.95f)), (int)(rockMeterScale.X * 0.65f), (int)(rockMeterScale.Y * (failTime / 3) * 0.95f)), new Color((byte)((song.percentBeat - 0.5) * 255 + 128), 0, 0));
                    }
                    else
                    {
                        spritebatch.Draw(texWhite, new Rectangle((int)((rockMeterScale.X * 0.2f) + rex), (int)((rockMeterScale.Y * 0.025f) + rockMeterLoc.Y + (rockMeterScale.Y * (1 - (failTime / 3)) * 0.95f)), (int)(rockMeterScale.X * 0.65f), (int)(rockMeterScale.Y * (failTime / 3) * 0.95f)), new Color((byte)((0.5 - song.percentBeat) * 255 + 128), 0, 0));
                    }
                }
                else
                {
                    if (rmFill <= 0.33)
                        spritebatch.Draw(texWhite, new Rectangle((int)((rockMeterScale.X * 0.2f) + rex), (int)((rockMeterScale.Y * 0.025f) + rockMeterLoc.Y + (rockMeterScale.Y * (1 - rmFill) * 0.95f)), (int)(rockMeterScale.X * 0.65f), (int)(rockMeterScale.Y * rmFill * 0.95f)), new Color(new Vector4(1f, 0, 0f, 0.8f)));
                    else if (rmFill <= 0.67)
                        spritebatch.Draw(texWhite, new Rectangle((int)((rockMeterScale.X * 0.2f) + rex), (int)((rockMeterScale.Y * 0.025f) + rockMeterLoc.Y + (rockMeterScale.Y * (1 - rmFill) * 0.95f)), (int)(rockMeterScale.X * 0.65f), (int)(rockMeterScale.Y * rmFill * 0.95f)), new Color(new Vector4(1f, 1f, 0f, 0.8f)));
                    else
                        spritebatch.Draw(texWhite, new Rectangle((int)((rockMeterScale.X * 0.2f) + rex), (int)((rockMeterScale.Y * 0.025f) + rockMeterLoc.Y + (rockMeterScale.Y * (1 - rmFill) * 0.95f)), (int)(rockMeterScale.X * 0.65f), (int)(rockMeterScale.Y * rmFill * 0.95f)), new Color(new Vector4(0f, 1f, 0f, 0.8f)));
                }
                spritebatch.Draw(texRockMeterOutline, new Rectangle((int)rex, (int)rockMeterLoc.Y, (int)rockMeterScale.X, (int)rockMeterScale.Y), rmColor);

                for (int i = 0; i < 10; i++)
                    logoslots[i] = (byte)0;
                for (int i = 0; i < 4; i++)
                    if (instruments[i])
                        logoslots[(int)Math.Min(Math.Round(Math.Max(0, rockMeterLevel[i]) / 10f), 10)] |= bits[i];

                for (int i = 0; i < 11; i++)
                {
                    int logoscale = 4;
                    if (logoslots[i] != 0)
                    {
                        int numhere = 0;
                        for (int k = 0; k < 4; k++)
                            if ((logoslots[i] & bits[k]) != 0)
                                numhere++;
                        rmFill = ((10 - i) / 10f);
                        rmColor = new Color((1 - rmFill) < 0.66 ? (byte)255 : (byte)128,
                                            (1 - rmFill) > 0.33 ? (byte)255 : (byte)128,
                                            127);
                        spritebatch.Draw(texRockMeterLogoStem, new Vector2(rex + (rockMeterScale.X / 2), rockMeterLoc.Y + (rockMeterScale.Y * ((10 - i) / 10f) * 0.92f) + (rockMeterScale.Y * 0.04f)), new Rectangle(0, 0, 256, 256), rmColor, 0, new Vector2(0, 128), rockMeterScale.X / 800f * 3, new SpriteEffects(), 0);
                        int tnum = numhere;
                        for (int k = 3; k >= 0; k--)
                        {

                            if ((logoslots[i] & bits[k]) != 0)
                            {
                                rmFill = rockMeterLevel[k] / 100f;
                                rmColor = new Color(rmFill < 0.66 ? (byte)255 : (byte)128,
                                    rmFill > 0.33 ? (byte)255 : (byte)128,
                                    127);
                                switch (k)
                                {
                                    case 0:
                                        spritebatch.Draw(texRockMeterGuitarLogo, new Vector2(rex + (rockMeterScale.X / 2) + (tnum * (210 * (rockMeterScale.X / 800f * logoscale))) - (128 * (rockMeterScale.X / 800f * logoscale)), rockMeterLoc.Y + (rockMeterScale.Y * ((10 - i) / 10f) * 0.92f) + (rockMeterScale.Y * 0.04f)), rect256, rmColor, 0, new Vector2(0, 128), rockMeterScale.X / 800f * logoscale, SpriteEffects.None, 0);
                                        break;
                                    case 1:
                                        spritebatch.Draw(texRockMeterSingerLogo, new Vector2(rex + (rockMeterScale.X / 2) + (tnum * (210 * (rockMeterScale.X / 800f * logoscale))) - (128 * (rockMeterScale.X / 800f * logoscale)), rockMeterLoc.Y + (rockMeterScale.Y * ((10 - i) / 10f) * 0.92f) + (rockMeterScale.Y * 0.04f)), rect256, rmColor, 0, new Vector2(0, 128), rockMeterScale.X / 800f * logoscale, SpriteEffects.None, 0);
                                        break;
                                    case 2:
                                        spritebatch.Draw(texRockMeterDrumLogo, new Vector2(rex + (rockMeterScale.X / 2) + (tnum * (210 * (rockMeterScale.X / 800f * logoscale))) - (128 * (rockMeterScale.X / 800f * logoscale)), rockMeterLoc.Y + (rockMeterScale.Y * ((10 - i) / 10f) * 0.92f) + (rockMeterScale.Y * 0.04f)), rect256, rmColor, 0, new Vector2(0, 128), rockMeterScale.X / 800f * logoscale, SpriteEffects.None, 0);
                                        break;
                                    case 3:
                                        spritebatch.Draw(texRockMeterBassLogo, new Vector2(rex + (rockMeterScale.X / 2) + (tnum * (210 * (rockMeterScale.X / 800f * logoscale))) - (128 * (rockMeterScale.X / 800f * logoscale)), rockMeterLoc.Y + (rockMeterScale.Y * ((10 - i) / 10f) * 0.92f) + (rockMeterScale.Y * 0.04f)), rect256, rmColor, 0, new Vector2(0, 128), rockMeterScale.X / 800f * logoscale, SpriteEffects.None, 0);
                                        break;
                                }
                                tnum--;
                            }
                        }
                    }
                }
            }
        }

        private void DrawScoreStars(long currenttime)
        {
            if (cGUIStyle == GUIStyle.UN)
            {
                float height=(GameSettings.windowheight*0.15f);
                int ct = 0;
                for (int i = 0; i < 4; i++)
                    if (instruments[i])
                        ct++;
                /*for (int i = 0; i < (int)GetRockstarAmount(); i++)
                {
                    spritebatch.Draw(rmUNstar, new Vector2(0, GameSettings.windowheight / 2 + ((height / rmUNstaro.Height) * 0.5f * i)), null, GetRockstarAmount() < 5 ? FretColors[(int)GetRockstarAmount()] : Color.White, rockstarDir, new Vector2(rmUNstar.Width / 2, rmUNstar.Height / 2), (height / rmUNstaro.Height) * 0.5f, SpriteEffects.None, 0);
                    spritebatch.Draw(rmUNstar, new Vector2(GameSettings.windowwidth, GameSettings.windowheight / 2 + ((height / rmUNstaro.Height) * 0.5f * i)), null, GetRockstarAmount() < 5 ? FretColors[(int)GetRockstarAmount()] : Color.White, rockstarDir, new Vector2(rmUNstar.Width / 2, rmUNstar.Height / 2), (height / rmUNstaro.Height) * 0.5f, SpriteEffects.None, 0);
                }*/
#if WINDOWS
                spritebatch.Draw(rmUNstar, new Vector2(0, GameSettings.windowheight / 2), null, GetRockstarAmount() < 5 ? FretColors[(int)GetRockstarAmount()] : Color.White, rockstarDir, new Vector2(rmUNstar.Width / 2, rmUNstar.Height / 2), GetRockstarAmount() >= 5 ? (height / rmUNstaro.Height) : (GetRockstarAmount() % 1) * (height / rmUNstaro.Height), SpriteEffects.None, 0);
                spritebatch.Draw(rmUNstaro, new Vector2(0, GameSettings.windowheight / 2), null, Color.White, rockstarDir, new Vector2(rmUNstar.Width / 2, rmUNstar.Height / 2), height / rmUNstaro.Height, SpriteEffects.None, 0);
                //if (ct <= 1)
                {
                    spritebatch.Draw(rmUNstar, new Vector2(GameSettings.windowwidth, GameSettings.windowheight / 2), null, GetRockstarAmount()<5?FretColors[(int)GetRockstarAmount()]:Color.White, rockstarDir, new Vector2(rmUNstar.Width / 2, rmUNstar.Height / 2),GetRockstarAmount()>=5?(height/rmUNstaro.Height):(GetRockstarAmount()%1)*(height/rmUNstaro.Height), SpriteEffects.None, 0);
                    spritebatch.Draw(rmUNstaro, new Vector2(GameSettings.windowwidth, GameSettings.windowheight / 2), null, Color.White, rockstarDir, new Vector2(rmUNstar.Width / 2, rmUNstar.Height / 2), height/rmUNstaro.Height, SpriteEffects.None, 0);
                }
#else
                spritebatch.Draw(rmUNstar, new Vector2(GameSettings.windowwidth*0f, GameSettings.windowheight / 2), null, GetRockstarAmount()<5?FretColors[(int)GetRockstarAmount()]:Color.White, rockstarDir, new Vector2(rmUNstar.Width / 2, rmUNstar.Height / 2),(GetRockstarAmount()%1)*(height/rmUNstaro.Height), SpriteEffects.None, 0);
                spritebatch.Draw(rmUNstaro, new Vector2(GameSettings.windowwidth * 0f, GameSettings.windowheight / 2), null, Color.White, rockstarDir, new Vector2(rmUNstar.Width / 2, rmUNstar.Height / 2), height / rmUNstaro.Height, SpriteEffects.None, 0);
                //if (ct <= 1)
                {
                    spritebatch.Draw(rmUNstar, new Vector2(GameSettings.windowwidth*1f, GameSettings.windowheight / 2), null, FretColors[(int)GetRockstarAmount()], rockstarDir, new Vector2(rmUNstar.Width / 2, rmUNstar.Height / 2),(GetRockstarAmount()%1)*(height/rmUNstaro.Height), SpriteEffects.None, 0);
                    spritebatch.Draw(rmUNstaro, new Vector2(GameSettings.windowwidth*1f, GameSettings.windowheight / 2), null, Color.White, rockstarDir, new Vector2(rmUNstar.Width / 2, rmUNstar.Height / 2), height/rmUNstaro.Height, SpriteEffects.None, 0);
                }
#endif
                String scr = GetScore().ToString();
                char[] scrarr = scr.ToCharArray();
                String scr2 = "";
                int count = 0;
                for (int l = scr.Length - 1; l >= 0; l--)
                {
                    scr2 = scrarr[l] + scr2;
                    if (count >= 2)
                    {
                        count = 0;
                        scr2 = "," + scr2;
                    }
                    else
                        count++;
                }
                if (scr2.StartsWith(","))
                    scr2 = scr2.Substring(1);
                if (instruments[1])
                {
                    spritebatch.DrawString(DefaultFont, scr2, new Vector2((GameSettings.windowwidth / 2) - (DefaultFont.MeasureString(scr2).X / 2), Board.vocaly+Board.vocalheight), Color.White);
                }
                else
                    spritebatch.DrawString(DefaultFont, scr2, new Vector2((GameSettings.windowwidth / 2) - (DefaultFont.MeasureString(scr2).X / 2), 20), Color.White);
            }
            else if (cGUIStyle == GUIStyle.RB)
            {
                float xers;
                if (CurrentTime / 1000f > -2.5)
                    xers = rockstarLoc.X;
                else if (currenttime / 1000f > -3)
                    xers = (Window.ClientBounds.Width - ((((currenttime / 1000f) + 3f) * 2) * (Window.ClientBounds.Width - rockstarLoc.X)));
                else
                    xers = Window.ClientBounds.Width;
                for (int i = 0; i < 5; i++)
                {
                    float scale = (GetRockstarAmount() > i + 1) ? 1 : GetRockstarAmount() - i;
                    if (scale < 0)
                        scale = 0;
                    scale *= 0.9f;

                    spritebatch.Draw(texRockstarRed, new Vector2((int)(xers + (rockstarScale.X / 5 * (i + 0.5f))), (int)(rockstarLoc.Y + (rockstarScale.X / 5 * 0.5f))), new Rectangle(0, 0, 256, 256), new Color(new Vector3(0.1f, 0.1f, 0.1f)), rockstarDir, new Vector2(128, 128), ((rockstarScale.X / 5) / 256), new SpriteEffects(), 0);
                    if (GetRockstarAmount() >= i + 1)
                        spritebatch.Draw(texRockstarRed, new Vector2((int)(xers + (rockstarScale.X / 5 * (i + 0.5f))), (int)(rockstarLoc.Y + (rockstarScale.X / 5 * 0.5f))), new Rectangle(0, 0, 256, 256), Color.White, rockstarDir, new Vector2(128, 128), ((rockstarScale.X / 5) / 256) * scale, new SpriteEffects(), 0);
                    else
                        spritebatch.Draw(texRockstarRed, new Vector2((int)(xers + (rockstarScale.X / 5 * (i + 0.5f))), (int)(rockstarLoc.Y + (rockstarScale.X / 5 * 0.5f))), new Rectangle(0, 0, 256, 256), Color.Gray, rockstarDir, new Vector2(128, 128), ((rockstarScale.X / 5) / 256) * scale, new SpriteEffects(), 0);
                    spritebatch.Draw(texRockstarCover, new Vector2((int)(xers + (rockstarScale.X / 5 * (i + 0.5f))), (int)(rockstarLoc.Y + (rockstarScale.X / 5 * 0.5f))), new Rectangle(0, 0, 256, 256), Color.White, rockstarDir, new Vector2(128, 128), (rockstarScale.X / 5) / 256, new SpriteEffects(), 0);
                    spritebatch.Draw(texRockstarRing, new Vector2((int)(xers + (rockstarScale.X / 5 * (i + 0.5f))), (int)(rockstarLoc.Y + (rockstarScale.X / 5 * 0.5f))), new Rectangle(0, 0, 256, 256), Color.White, rockstarDir, new Vector2(128, 128), (rockstarScale.X / 5) / 256, new SpriteEffects(), 0);
                    if (GetRockstarAmount() >= i + 0.25)
                        spritebatch.Draw(texRockstarRingHiLi[10], new Vector2((int)(xers + (rockstarScale.X / 5 * (i + 0.5f))), (int)(rockstarLoc.Y + (rockstarScale.X / 5 * 0.5f))), new Rectangle(0, 0, 128, 128), Color.White, 0, new Vector2(128, 0), ((rockstarScale.X / 5) / 280), new SpriteEffects(), 0);
                    else if (GetRockstarAmount() > i)
                        spritebatch.Draw(texRockstarRingHiLi[(int)Math.Floor((GetRockstarAmount() - i) / 0.25f * 11)], new Vector2((int)(xers + (rockstarScale.X / 5 * (i + 0.5f))), (int)(rockstarLoc.Y + (rockstarScale.X / 5 * 0.5f))), new Rectangle(0, 0, 128, 128), Color.White, 0, new Vector2(128, 0), ((rockstarScale.X / 5) / 280), new SpriteEffects(), 0);
                    if (GetRockstarAmount() >= i + 0.5)
                        spritebatch.Draw(texRockstarRingHiLi[10], new Vector2((int)(xers + (rockstarScale.X / 5 * (i + 0.5f))), (int)(rockstarLoc.Y + (rockstarScale.X / 5 * 0.5f))), new Rectangle(0, 0, 128, 128), Color.White, (float)(Math.PI) / 2, new Vector2(128, 0), ((rockstarScale.X / 5) / 280), new SpriteEffects(), 0);
                    else if (GetRockstarAmount() > i + 0.25)
                        spritebatch.Draw(texRockstarRingHiLi[(int)Math.Floor((GetRockstarAmount() - (i + 0.25f)) / 0.25f * 11)], new Vector2((int)(xers + (rockstarScale.X / 5 * (i + 0.5f))), (int)(rockstarLoc.Y + (rockstarScale.X / 5 * 0.5f))), new Rectangle(0, 0, 128, 128), Color.White, (float)(Math.PI) / 2, new Vector2(128, 0), ((rockstarScale.X / 5) / 280), new SpriteEffects(), 0);
                    if (GetRockstarAmount() >= i + 0.75)
                        spritebatch.Draw(texRockstarRingHiLi[10], new Vector2((int)(xers + (rockstarScale.X / 5 * (i + 0.5f))), (int)(rockstarLoc.Y + (rockstarScale.X / 5 * 0.5f))), new Rectangle(0, 0, 128, 128), Color.White, (float)(Math.PI), new Vector2(128, 0), ((rockstarScale.X / 5) / 280), new SpriteEffects(), 0);
                    else if (GetRockstarAmount() > i + 0.5)
                        spritebatch.Draw(texRockstarRingHiLi[(int)Math.Floor((GetRockstarAmount() - (i + 0.5f)) / 0.25f * 11)], new Vector2((int)(xers + (rockstarScale.X / 5 * (i + 0.5f))), (int)(rockstarLoc.Y + (rockstarScale.X / 5 * 0.5f))), new Rectangle(0, 0, 128, 128), Color.White, (float)(Math.PI), new Vector2(128, 0), ((rockstarScale.X / 5) / 280), new SpriteEffects(), 0);
                    if (GetRockstarAmount() >= i + 1)
                        spritebatch.Draw(texRockstarRingHiLi[10], new Vector2((int)(xers + (rockstarScale.X / 5 * (i + 0.5f))), (int)(rockstarLoc.Y + (rockstarScale.X / 5 * 0.5f))), new Rectangle(0, 0, 128, 128), Color.White, (float)(Math.PI) * 1.5f, new Vector2(128, 0), ((rockstarScale.X / 5) / 280), new SpriteEffects(), 0);
                    else if (GetRockstarAmount() > i + 0.75f)
                        spritebatch.Draw(texRockstarRingHiLi[(int)Math.Floor((GetRockstarAmount() - (i + 0.75f)) / 0.25f * 11)], new Vector2((int)(xers + (rockstarScale.X / 5 * (i + 0.5f))), (int)(rockstarLoc.Y + (rockstarScale.X / 5 * 0.5f))), new Rectangle(0, 0, 128, 128), Color.White, (float)(Math.PI) * 1.5f, new Vector2(128, 0), ((rockstarScale.X / 5) / 280), new SpriteEffects(), 0);
                }
                spritebatch.Draw(texScoreBoard, new Rectangle((int)xers, (int)rockstarLoc.Y - (int)(rockstarScale.Y / 2), (int)rockstarScale.X, (int)(rockstarScale.Y / 2)), Color.White);
                String scr = GetScore().ToString();
                char[] scrarr = scr.ToCharArray();
                String scr2 = "";
                int count = 0;
                for (int l = scr.Length - 1; l >= 0; l--)
                {
                    scr2 = scrarr[l] + scr2;
                    if (count >= 2)
                    {
                        count = 0;
                        scr2 = "," + scr2;
                    }
                    else
                        count++;
                }
                spritebatch.DrawString(DefaultFont, scr2, new Vector2(xers + (rockstarScale.X * 0.95f) - DefaultFont.MeasureString(scr2).X, rockstarLoc.Y - (rockstarScale.Y / 2)), Color.White);
            }
        }

        private void DrawFlashes(int index, Matrix fling)
        {
            int lefty = 1;
            if (boards[index].IsLefty())
                lefty = -1;
            float[] arr = boards[index].GetPopups();
            for (int i = 0; index==2 ? i < 4 : i < 5; i++)
                if (arr[i]>0)
                {
                    Matrix matRot, matTransl, matOrbit, matScale;
                    matRot = Matrix.CreateRotationY(boards[index].flashRot)*Matrix.CreateRotationX(MathHelper.PiOver4);// *Matrix.CreateRotationY((float)(hvdistTOdir(venue.GetCamFor().X, venue.GetCamFor().Z) / 180 * Math.PI) + MathHelper.PiOver2);
                    if (index == 2)
                    {
                        matTransl = Matrix.CreateTranslation((-.75f + (i * .5f)) * (Board.width), 0.1f + Board.height + (boards[index].GetBoardBump() * Board.BOARD_BUMP_COEF), 0f);
                        matOrbit = Matrix.CreateTranslation(0f, Board.curveHeight * (1 - Math.Abs(-.8f + (i * 0.4f))) + ((boards[index].GetPopups()[i]) * 0.001f), -Board.zeroZ) * fling;
                    }
                    else
                    {
                        matTransl = Matrix.CreateTranslation((-.8f + (i * 0.4f)) * lefty * Board.width, 0.1f+Board.height + (boards[index].GetBoardBump() * Board.BOARD_BUMP_COEF), 0f);
                        matOrbit = Matrix.CreateTranslation(0f, Board.curveHeight * (1 - Math.Abs(-.8f + (i * 0.4f))) + ((boards[index].GetPopups()[i]) * 0.001f), -Board.zeroZ) * fling;
                    }
                    matScale = Matrix.CreateScale(new Vector3(0.2f, 0.2f, 0.2f));

                    // identity, scale, rotate, orbit(translate & rotate), translate
                    engine.Parameters["world"].SetValue(matScale * matRot * matOrbit * matTransl);

                    engine.Parameters["diffuseTexture"].SetValue(Board.texBlast);
                    engine.Parameters["diffuseColor"].SetValue(index==2?FretColorsV4[Board.guitarToDrums[i]]:FretColorsV4[i]);
                    engine.CommitChanges();

                    // 5: draw object - select vertex type, primitive type, # of primitives
                    graphics.GraphicsDevice.VertexDeclaration = vd;
                    graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                    graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                    graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                    graphics.GraphicsDevice.Vertices[0].SetSource(square, 0, GBVertexFormat.SizeInBytes);
                    graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
                    graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                }
            engine.Parameters["diffuseColor"].SetValue(new Vector4(1,1,1,1));
        }
        private void DrawGibsFS(int board)
        {
            int k = board;
            for (int i = 0; i < glass[k].Length; i++)
                if (glass[k][i].scale > 0)
                {
                    Matrix matIdentity, matTransl, matScale, matOrbit;
                    matIdentity = Matrix.Identity;
                    matTransl = Matrix.CreateTranslation(glass[k][i].loc);
                    matOrbit = Matrix.CreateRotationX(glass[k][i].rot.X) * Matrix.CreateRotationY(glass[k][i].rot.Y) * Matrix.CreateRotationZ(glass[k][i].rot.Z);
                    matScale = Matrix.CreateScale((new Vector3(0.1f, 0.1f, 0.1f)) * glass[k][i].scale);

                    // identity, scale, rotate, orbit(translate & rotate), translate
                    engine.Parameters["world"].SetValue(matIdentity * matScale * matOrbit * matTransl);

                    engine.Parameters["proj"].SetValue(matProj);


                    engine.Parameters["diffuseTexture"].SetValue(texShard[glass[k][i].frame]);
                    engine.Parameters["diffuseColor"].SetValue(FretColorsV4[glass[k][i].col]);
                    engine.CommitChanges();

                    // 5: draw object - select vertex type, primitive type, # of primitives
                    graphics.GraphicsDevice.VertexDeclaration = vd;
                    graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                    graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                    graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                    graphics.GraphicsDevice.Vertices[0].SetSource(square, 0, GBVertexFormat.SizeInBytes);
                    graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
                    graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                }
            for (int i = 0; i < sparks[k].Length; i++)
                if (sparks[k][i].scale > 0)
                {
                    Matrix matRot, matTransl, matScale;
                    matRot = Matrix.CreateRotationX(MathHelper.PiOver2) * Matrix.CreateRotationY((float)(hvdistTOdir(Vector3.Forward.X, Vector3.Forward.Z) / 180 * Math.PI)+MathHelper.PiOver2);
                    matTransl = Matrix.CreateTranslation(sparks[k][i].loc);
                    matScale = Matrix.CreateScale(new Vector3(0.01f, 0.01f, 0.01f) * sparks[k][i].scale);

                    // identity, scale, rotate, orbit(translate & rotate), translate
                    engine.Parameters["world"].SetValue(matScale * matRot * matTransl);

                    engine.Parameters["diffuseTexture"].SetValue(texSpark);
                    engine.Parameters["diffuseColor"].SetValue(FretColorsV4[sparks[k][i].col]);
                    engine.CommitChanges();

                    // 5: draw object - select vertex type, primitive type, # of primitives
                    graphics.GraphicsDevice.VertexDeclaration = vd;
                    graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                    graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                    graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                    graphics.GraphicsDevice.Vertices[0].SetSource(square, 0, GBVertexFormat.SizeInBytes);
                    graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
                    graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                }
            engine.Parameters["diffuseColor"].SetValue(new Vector4(0.8f,0.8f,0.8f,1.0f));
        }

        private void GetContGUIData()
        {
            if (contguis[0].Equals(ContGUIData.INVALID))
            {
                contguis[0] = new ContGUIData(ContGUIData.CONT_TYPE.KEYBOARD,(PlayerIndex)(-1));
            }
            if (contguis[1].Equals(ContGUIData.INVALID))
            {
                GamePadCapabilities gpc = GamePad.GetCapabilities(PlayerIndex.One);
                if (gpc.IsConnected)
                {
                    if (gpc.GamePadType == GamePadType.Guitar)
                        contguis[1] = new ContGUIData(ContGUIData.CONT_TYPE.STRATOCASTER,PlayerIndex.One);
                    else if (gpc.GamePadType == (GamePadType)7)
                        contguis[1] = new ContGUIData(ContGUIData.CONT_TYPE.XPLORER,PlayerIndex.One);
                    else if (gpc.GamePadType == GamePadType.DrumKit)
                        contguis[1] = new ContGUIData(ContGUIData.CONT_TYPE.DRUMSET,PlayerIndex.One);
                    else if (gpc.GamePadType == GamePadType.Unknown)
                        contguis[1] = new ContGUIData(ContGUIData.CONT_TYPE.MICROPHONE,PlayerIndex.One);
                }
                else
                    contguis[1] = ContGUIData.INVALID;
            }
            if (contguis[2].Equals(ContGUIData.INVALID))
            {
                GamePadCapabilities gpc = GamePad.GetCapabilities(PlayerIndex.Two);
                if (gpc.IsConnected)
                {
                    if (gpc.GamePadType == GamePadType.Guitar)
                        contguis[2] = new ContGUIData(ContGUIData.CONT_TYPE.STRATOCASTER,PlayerIndex.Two);
                    else if (gpc.GamePadType == (GamePadType)7)
                        contguis[2] = new ContGUIData(ContGUIData.CONT_TYPE.XPLORER,PlayerIndex.Two);
                    else if (gpc.GamePadType == GamePadType.DrumKit)
                        contguis[2] = new ContGUIData(ContGUIData.CONT_TYPE.DRUMSET,PlayerIndex.Two);
                    else if (gpc.GamePadType == GamePadType.Unknown)
                        contguis[2] = new ContGUIData(ContGUIData.CONT_TYPE.MICROPHONE,PlayerIndex.Two);
                }
                else
                    contguis[2] = ContGUIData.INVALID;
            }
            if (contguis[3].Equals(ContGUIData.INVALID))
            {
                GamePadCapabilities gpc = GamePad.GetCapabilities(PlayerIndex.Three);
                if (gpc.IsConnected)
                {
                    if (gpc.GamePadType == GamePadType.Guitar)
                        contguis[3] = new ContGUIData(ContGUIData.CONT_TYPE.STRATOCASTER,PlayerIndex.Three);
                    else if (gpc.GamePadType == (GamePadType)7)
                        contguis[3] = new ContGUIData(ContGUIData.CONT_TYPE.XPLORER,PlayerIndex.Three);
                    else if (gpc.GamePadType == GamePadType.DrumKit)
                        contguis[3] = new ContGUIData(ContGUIData.CONT_TYPE.DRUMSET,PlayerIndex.Three);
                    else if (gpc.GamePadType == GamePadType.Unknown)
                        contguis[3] = new ContGUIData(ContGUIData.CONT_TYPE.MICROPHONE,PlayerIndex.Three);
                }
                else
                    contguis[3] = ContGUIData.INVALID;
            }
            if (contguis[4].Equals(ContGUIData.INVALID))
            {
                GamePadCapabilities gpc = GamePad.GetCapabilities(PlayerIndex.Four);
                if (gpc.IsConnected)
                {
                    if (gpc.GamePadType == GamePadType.Guitar)
                        contguis[4] = new ContGUIData(ContGUIData.CONT_TYPE.STRATOCASTER,PlayerIndex.Four);
                    else if (gpc.GamePadType == (GamePadType)7)
                        contguis[4] = new ContGUIData(ContGUIData.CONT_TYPE.XPLORER,PlayerIndex.Four);
                    else if (gpc.GamePadType == GamePadType.DrumKit)
                        contguis[4] = new ContGUIData(ContGUIData.CONT_TYPE.DRUMSET,PlayerIndex.Four);
                    else if (gpc.GamePadType == GamePadType.Unknown)
                        contguis[4] = new ContGUIData(ContGUIData.CONT_TYPE.MICROPHONE,PlayerIndex.Four);
                }
                else
                    contguis[4] = ContGUIData.INVALID;
            }
        }

        public void Burn(GameTime gt, byte note, int i)
        {
            AddLesserSparks(note, i);
            AddShortSparks(note, i);
        }

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

        
        public void InvalidShaderVersion()
        {
#if WINDOWS
            System.Windows.Forms.MessageBox.Show("Error! Shader Model not supported!");
#endif
            Exit();
        }

        internal void RestartSong()
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}

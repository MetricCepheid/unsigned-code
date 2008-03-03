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
#endregion



namespace Unsigned
{
    #region publicstructs
    struct ShatterGlass
    {
        public Vector3 loc, rot, dir;
        public int col, frame;
        public float scale;
    }
    struct ShatterSpark
    {
        public Vector3 loc, dir;
        public int col;
        public float scale;
    }

    public struct WaveNode
    {
        public float X, Y;
        public byte Z;
    }

    public struct SetList
    {
        String[] sets;
        String[][] songs;

        public void Load(String filename)
        {
            if (!System.IO.File.Exists(filename))
                return;
            System.IO.StreamReader reader = new System.IO.StreamReader(filename);
            String z;
            z = reader.ReadLine().Trim();
            sets = new String[Int32.Parse(z.Substring(z.IndexOf('(') + 1).Substring(0, z.IndexOf(')') - (z.IndexOf('(') + 1)))];
            songs = new String[sets.Length][];

            for (int c = 0; c < sets.Length; c++)
            {
                String a;
                a = reader.ReadLine().Trim(); 
                if (a.Substring(0, a.IndexOf('(')).Equals("Set"))
                {
                    int index = Int32.Parse(a.Substring(a.IndexOf('(') + 1).Substring(0, a.IndexOf(')') - (a.IndexOf('(') + 1)));
                    a = reader.ReadLine().Trim();
                    sets[c] = a.Substring(a.IndexOf('(') + 1).Substring(0, a.IndexOf(')') - (a.IndexOf('(') + 1));
                    a = reader.ReadLine().Trim();
                    int len = Int32.Parse(a.Substring(a.IndexOf('(') + 1).Substring(0, a.IndexOf(')') - (a.IndexOf('(') + 1)));
                    songs[index] = new String[len];
                    for (int i = 0; i < len; i++)
                    {
                        a = reader.ReadLine().Trim();
                        songs[index][i] = a.Substring(a.IndexOf('(') + 1).Substring(0, a.IndexOf(')') - (a.IndexOf('(') + 1));
                    }
                }
            }
        }
    }

    public struct GBVertexFormat
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TexCoord;
        public Vector3 Tangent;
        public float Alpha;


        public GBVertexFormat(Vector3 Position, Vector3 Normal, Vector2 TexCoord)
        {
            this.Position = Position;
            this.TexCoord = TexCoord;
            this.Normal = Normal;
            this.Tangent = Vector3.Transform(Normal, Matrix.CreateRotationZ((float)Math.PI / 2));
            Alpha = 1.0f;
        }
        public GBVertexFormat(Vector3 Position, Vector3 Normal, Vector2 TexCoord, Vector3 Tangent)
        {
            this.Position = Position;
            this.TexCoord = TexCoord;
            this.Normal = Normal;
            this.Tangent = Tangent;
            Alpha = 1.0f;
        }

        public static VertexElement[] Elements =
             {
                 new VertexElement(0, 0, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 0),
                 new VertexElement(0, sizeof(float)*3, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Normal, 0),
                 new VertexElement(0, sizeof(float)*6, VertexElementFormat.Vector2, VertexElementMethod.Default, VertexElementUsage.TextureCoordinate, 0),
                 new VertexElement(0, sizeof(float)*8, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Tangent, 0),
                 new VertexElement(0, sizeof(float)*11, VertexElementFormat.Single,VertexElementMethod.Default,VertexElementUsage.Fog,0),
             };
        public static int SizeInBytes = sizeof(float) * (3 + 2 + 3 + 3  + 1);
    }
#endregion

    public class Game1 : Microsoft.Xna.Framework.Game
    {

        public static bool TEST_SONG = false;

#region enginestuff

        GraphicsDeviceManager graphics;
        ContentManager content;
        SpriteBatch spritebatch;
        AudioEngine audioEngine;
        SoundBank audioSoundBank;
        WaveBank audioWaveBank;
        private Effect engine, ppEngine;
        private Matrix matView;
        private Matrix matProj;
        private RenderTarget2D screenTarget, screenTargetPre, screenTargetFinal;
        private RenderTarget2D[] boardsTarget;

        private int renderLevel = 10;

        private Texture2D gradient;
        public static bool HALF_RENDER = false;
#endregion

#region rockstars

        private Vector2 rockstarLoc, rockstarScale;
        private float rockstarDir;
        private Texture2D texRockstarRed, texRockstarRing, texRockstarCover;
        private Texture2D[] texRockstarRingHiLi;
        private Texture2D texScoreBoard;
        byte lastStar; //for ching after star gain

#endregion

#region rockmeter

        public static float[] rockMeterLevel;
        private Texture2D texRockMeterOutline;
        private Texture2D texRockMeterGuitarLogo, texRockMeterBassLogo,
                          texRockMeterDrumLogo, texRockMeterSingerLogo;
        private Texture2D texRockMeterLogoStem;
        private Vector2 rockMeterLoc, rockMeterScale;

#endregion

#region instrumental

        private bool[] instruments;//whether or not someone is playing this
        private String[] instrumentNames = { "Guitar", "Vocals", "Drums", "Bass" };
        private String[] musicianNames = { "Guitarist", "Vocalist", "Drummer", "Bassist" };
        public static int GUITAR = 0, BASS = 3, DRUMS = 2, VOCALS = 1,
                                GUITARIST = 0, BASSIST = 3, PERCUSSIONIST = 2, VOCALIST = 1;

#endregion

#region specialeffects
        Texture2D lastframe;
        private enum FRAME_EFFECT
        {
            CONSTANT = 0, //this is normal
            SLOW = 1,     //framerate 1/2
            VERYSLOW = 2, //framerate 1/4
            DEATHLY = 3,  //1 fps :O
        };
        private FRAME_EFFECT currentFE;
        private int countFE;
        private enum FRAME_EFFECT_STYLE
        {//for FRAME_EFFECT.CONSTANT, use BLINK
            BLINK = 0, //no fades
            CREST = 1, //fade, flash
            XFADE = 2, //Full fade
        }
        private FRAME_EFFECT_STYLE currentFES;
        private float countFES=1;
        const int DESATURATE = 1, //desaturate
                         HUE_SHIFT = 2,  //hue-shift
                         REDUCE = 4,     //reduce to 8-bit color
                         BLUR = 8,       //gaussian blur
                         GRAIN_DOT = 16, //dot grain
                         GRAIN_XHSH = 32;//crosshash grain
        private int postProcessEffects;//bitwise-or together ^
        //following are used ONLY IF ppe is enabled
        private float desaturate_value;//0-1 (0=B&W,1=full color)
        private float hue_shift;//0-1 (0&1=no difference)
        private float blur_strength;//strength of blur
        private float blur_passes;//number of passes to the blur
        private float grain_strength;//how strong the grain is (0=invisible,1=full-static)
#endregion

#region misc

        public static Texture2D texWhite;
        public static byte[] bits = { 1, 1 << 1, 1 << 2, 1 << 3, 1 << 4, 1 << 5, 1 << 6, 1 << 7 };
        public const byte GUITAR_B = 1, BASS_B = 2, DRUMS_B = 4, VOCALS_B = 8;
        public const byte D_EASY = 3, D_MEDIUM = 6, D_HARD = 12, D_EXPERT = 24;
        public static String[] DifficultyStr = { "Easy", "Medium", "Hard", "Expert" };
        public static Color[] FretColors = { Color.Green, Color.Red, Color.Yellow, Color.Blue, Color.Orange };
        private static Vector4[] FretColorsV4 = { new Vector4(0, 1, 0, 1), new Vector4(1, 0, 0, 1), new Vector4(1, 1, 0, 1), new Vector4(0, 0, 1, 1), new Vector4(1, 0.5f, 0, 1) };
        private bool started = false;
        public static SpriteFont DefaultFont;
        public static long TicksPerSecond = 10000000;
        public static VertexBuffer square;
        public static Texture2D texGlow, texDefaultBM;
        public static VertexDeclaration vd;
        private RenderTarget2D ort;
        int windowheight, windowwidth;
#endregion

#region boards

        Board[] boards;
        private Texture2D[] texShard;
        private Texture2D texSpark;
        private ShatterGlass[] glass;
        private ShatterSpark[] sparks;
        private RenderTarget2D[] rtBar, rtPie;
        private int rtPieS;
#endregion

#region song

        private Song song;
        private long SongStartTime;
        private SetList setlist;

#endregion

#region venue

        private Venue venue;
        private string venueName = "tikibar";

#endregion

#region input

        GamePadState[] controllers;
        GamePadCapabilities[] contCapabilities;
        byte[] contInput;// ...4==keyboard...? yea!
        private byte guitarStrum=0, bassStrum=0;

#endregion

#region waves

        private Texture2D texLine, texLineEnd;

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
                        else if (loc == 0 && !filled[2])
                            nextLoc = 3;
                        else if (loc == 0 && !filled[3])
                            nextLoc = 4;
                        else if (loc == 1 && !filled[2])
                            nextLoc = 3;
                        else if (loc == 1 && !filled[3])
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
                    else if (green)
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
            public static Texture2D KB_ICO_BLUR, KB_ICO_GUITAR, KB_ICO_DRUM,
                                    GUITAR_ICO_BLUR, GUITAR_ICO, DRUMS_ICO_BLUR, DRUMS_ICO,
                                    MICROPHONE_ICO_BLUR, MICROPHONE_ICO, GUITARX_ICO_BLUR, GUITARX_ICO;
        }
        private const byte S_INGAME = 1, S_CHOOSECONT=8, S_CHOOSESONG=4, S_CHOOSEDIFF=16, S_MAINMENU=2, S_RESULTS=32;
        private byte screen = S_MAINMENU;
        String songname;
        byte[] diff;
        Texture2D concrTex, concrBM, arrowTex, rustyTex;
        Texture2D stratTex;
        Texture2D[] nPadTex;
        Model nPadMdl, arrowMdl, nailMdl, strat, stand;
        SpriteFont sfManager, sfGuitarist, sfBassist, sfDrummer, sfSinger;
        float[] arrowTimer, arrowRot;
        String[][] charNames;
        int[] charNameSelected;
        ContGUIData[] contguis;
        float idleTime;
        int songoffset=0, songselected=0;
        Texture2D hairr, hairl, flameTex, texContinue;
        Vector3[][] flames;
        int leader;
        bool[] finals;
        Texture2D[] texNote;
        private RenderTarget2D[] rtNote;
        int mmenu_select = 0, mmenu_ticker=0;
        int counterer = 0;
        String[][] cSongNames, vSongNames;
        String[] rockerNames;


        byte loaded = 0, loading = 0;
#endregion

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            content = new ContentManager(Services);
            
            content.RootDirectory = "";
        }

        protected override void Initialize()
        {
            InitXNAApp();
            
            //audioEngine = new AudioEngine("audio\\Win\\unsigned.xgs");
            //audioWaveBank = new WaveBank(audioEngine, "audio\\Win\\Wave Bank.xwb");
            //audioSoundBank = new SoundBank(audioEngine, "audio\\Win\\Sound Bank.xsb");

            Configurate();

            setlist.Load("SongList.gbl");

            instruments = new bool[4];
            glass = new ShatterGlass[100];
            sparks = new ShatterSpark[100];

            contInput = new byte[4];
            controllers = new GamePadState[4];

            contCapabilities = new GamePadCapabilities[4];

            diff = new byte[4];
            
            GBVertexFormat[] arr = { new GBVertexFormat(new Vector3(-1f,0f, 1f),new Vector3(0f,1f,0f),new Vector2(0f,0f)),
                                     new GBVertexFormat(new Vector3(-1f,0f,-1f),new Vector3(0f,1f,0f),new Vector2(0f,1f)),
                                     new GBVertexFormat(new Vector3( 1f,0f, 1f),new Vector3(0f,1f,0f),new Vector2(1f,0f)),
                                     new GBVertexFormat(new Vector3( 1f,0f, 1f),new Vector3(0f,1f,0f),new Vector2(1f,0f)),
                                     new GBVertexFormat(new Vector3(-1f,0f,-1f),new Vector3(0f,1f,0f),new Vector2(0f,1f)),
                                     new GBVertexFormat(new Vector3( 1f,0f,-1f),new Vector3(0f,1f,0f),new Vector2(1f,1f))};
            square = new VertexBuffer(graphics.GraphicsDevice, GBVertexFormat.SizeInBytes * 6, BufferUsage.WriteOnly);
            square.SetData<GBVertexFormat>(arr);

            //TEST CODE, takes you right into the action!
            /*songname = "Highway_to_Hell";
            contInput = new byte[4];
            rockerNames = new String[4];
            for (int k = 0; k < 4; k++)
            { contInput[k] = 255; instruments[k] = false; rockerNames[k] = null; }
            instruments[0] = true;
            //instruments[2] = true;
            //instruments[3] = true;
            contInput[0] = 0;
            rockerNames[0] = "default";
            contInput[2] = 4;
            rockerNames[2] = "default";
            contInput[3] = 0;
            rockerNames[3] = "default";
            screen = S_INGAME;
            for (int i = 0; i < 4; i++)
                    diff[i] = D_EXPERT;
            InitForSong(instruments[0], instruments[1], instruments[2], instruments[3], diff, venueName);*/
            //END TEST CODE

            base.Initialize();
        }

        private void InitXNAApp()
        {
            Window.Title = "Unsigned";

            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 600;
            windowwidth = 800;
            windowheight = 600;
            //graphics.ToggleFullScreen();

            engine = content.Load<Effect>("shaders\\HFPS_Shader_XNA");//new Effect(graphics.GraphicsDevice,"shaders\\HFPS_Shader_XNA.fxc",CompilerOptions.None,new EffectPool());
            ppEngine = content.Load<Effect>("shaders\\PP_Shader_XNA");
            engine.Parameters["ambientColor"].SetValue(new Vector4(1.0f,1.0f,1.0f,1.0f));
            float[] pLightFar = new float[16];
            for (int i = 0; i < 16; i++)
                pLightFar[i] = 10f;
            engine.Parameters["pLightFar"].SetValue(pLightFar);
            

            SetProjMatrix(Window.ClientBounds.Width,Window.ClientBounds.Height);
            graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;
            graphics.SynchronizeWithVerticalRetrace = true;


            spritebatch = new SpriteBatch(graphics.GraphicsDevice);
        }

        private void Configurate()
        {
            System.IO.StreamReader fin = new System.IO.StreamReader("config.cfg");
            do
            {
                String str = fin.ReadLine();
                if (str.Length > 12 && str.Substring(0, 12).ToLower().Equals("3dbackground"))
                {
                    int val = Int32.Parse(str.Substring(str.IndexOf('=') + 1).Trim());
                    if (val == 0)
                        renderLevel = 0;
                    else
                        renderLevel = 10;
                }
            } while (!fin.EndOfStream);
        }

        void SetProjMatrix(int w, int h)
        {
            matProj = Matrix.CreatePerspectiveFieldOfView((float)Math.PI / 4.0f,
                              w / (float)h,
                              2f, 750.0f);
        }
        
        protected override void LoadContent()
        {
            sfBassist = content.Load<SpriteFont>("fonts\\bassist");
            sfGuitarist = content.Load<SpriteFont>("fonts\\guitarist");
            sfDrummer = content.Load<SpriteFont>("fonts\\drummer");
            sfSinger = content.Load<SpriteFont>("fonts\\singer");
            sfManager = content.Load<SpriteFont>("fonts\\manager");
            texDefaultBM = content.Load<Texture2D>("graphics\\blankbm");
            DefaultFont = content.Load<SpriteFont>("BasicFont");
            gradient = content.Load<Texture2D>("graphics\\gradient");
            System.IO.StreamReader sr = new System.IO.StreamReader("SongList.gbl");
            String str = sr.ReadLine();
            str = str.Trim();
            int numSets = Int32.Parse(str.Substring(str.IndexOf('(') + 1, str.IndexOf(')') - str.IndexOf('(') - 1));
            cSongNames = new String[numSets][];
            vSongNames = new String[numSets][];
            for (int i = 0; i < numSets; i++)
            {
                int j=0;
                String setname="";
                while(true)
                {
                    str = sr.ReadLine().Trim();
                    if(str.Length>=3 && str.Substring(0,3).Equals("Set"))
                        j = Int32.Parse(str.Substring(str.IndexOf('(') + 1, str.IndexOf(')') - str.IndexOf('(') - 1));
                    if(str.Length>=4 && str.Substring(0,4).Equals("Name"))
                        setname = str.Substring(str.IndexOf('(') + 1, str.IndexOf(')') - str.IndexOf('(') - 1);
                    if (str.Length >= 8 && str.Substring(0, 8).Equals("NumSongs"))
                    {
                        int numsongs = Int32.Parse(str.Substring(str.IndexOf('(') + 1, str.IndexOf(')') - str.IndexOf('(') - 1));
                        cSongNames[j] = new String[Int32.Parse(str.Substring(str.IndexOf('(') + 1, str.IndexOf(')') - str.IndexOf('(') - 1))];
                        vSongNames[j] = new String[cSongNames[j].Length+1];
                        vSongNames[j][0] = setname;
                        for (int k = 0; k < numsongs; k++)
                        {
                            str = sr.ReadLine().Trim();
                            cSongNames[j][k] = str.Substring(str.IndexOf('(') + 1, str.IndexOf(')') - str.IndexOf('(') - 1);
                            System.IO.BinaryReader tmp = new System.IO.BinaryReader(System.IO.File.OpenRead("songdata\\"+cSongNames[j][k] + ".gba"));
                            tmp.ReadByte();
                            vSongNames[j][k + 1] = tmp.ReadString();
                            tmp.Close();
                        }
                        break;
                    }
                }
                sr.Close();
            }
        }

        private void LoadContent(byte c)
        {
            if ((loaded & c) == 0 && (loading & c) == 0)
            {
                #region MAINMENU
                if ((c & S_MAINMENU) != 0)
                {
                    loaded |=  S_MAINMENU;
                }
                #endregion
                #region RESULTS
                if ((c & S_RESULTS) != 0)
                {
                    loaded |=  S_RESULTS;
                }
                #endregion
                #region SONGMENU
                else if ((c & S_CHOOSESONG) != 0)
                {
                    loaded |=  S_CHOOSESONG;
                }
                #endregion
                #region DIFFMENU
                else if ((c & S_CHOOSEDIFF) != 0)
                {
                    loaded |=  S_CHOOSEDIFF;
                }
                #endregion
                #region CHOOSECONT
                if ((c&S_CHOOSECONT)!=0)
                {
                    ThreadStart ThreadStarter = delegate
                    {
                        rtNote = new RenderTarget2D[4];
                        rtNote[0] = new RenderTarget2D(graphics.GraphicsDevice, 256, 256, 1, SurfaceFormat.Color);
                        rtNote[1] = new RenderTarget2D(graphics.GraphicsDevice, 256, 256, 1, SurfaceFormat.Color);
                        rtNote[2] = new RenderTarget2D(graphics.GraphicsDevice, 256, 256, 1, SurfaceFormat.Color);
                        rtNote[3] = new RenderTarget2D(graphics.GraphicsDevice, 256, 256, 1, SurfaceFormat.Color);
                        nPadTex = new Texture2D[4];
                        nPadTex[0] = content.Load<Texture2D>("graphics\\paper1");
                        nPadTex[1] = content.Load<Texture2D>("graphics\\paper2");
                        nPadTex[2] = content.Load<Texture2D>("graphics\\paper3");
                        nPadTex[3] = content.Load<Texture2D>("graphics\\paper4");
                        texNote = new Texture2D[4];
                        concrTex = content.Load<Texture2D>("graphics\\concr");
                        concrBM = content.Load<Texture2D>("graphics\\concrBM");
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
                        loading &= (byte)(~S_CHOOSECONT & 255);
                        loaded |= S_CHOOSECONT;
                    };
                    
                    loading |= S_CHOOSECONT;
                    Thread myThread = new Thread(ThreadStarter);
                    myThread.Start();
                }
                #endregion
                #region INGAME
                if ((c&S_INGAME)!=0)
                {
                    ThreadStart ThreadStarter = delegate
                    {
                        rtBar = new RenderTarget2D[4];
                        rtBar[0] = new RenderTarget2D(graphics.GraphicsDevice, 256, 256, 1, SurfaceFormat.Color);
                        rtBar[1] = new RenderTarget2D(graphics.GraphicsDevice, 256, 256, 1, SurfaceFormat.Color);
                        rtBar[2] = new RenderTarget2D(graphics.GraphicsDevice, 256, 256, 1, SurfaceFormat.Color);
                        rtBar[3] = new RenderTarget2D(graphics.GraphicsDevice, 256, 256, 1, SurfaceFormat.Color);
                        rtPie = new RenderTarget2D[4];
                        rtPieS = windowheight;
                        rtPie[0] = new RenderTarget2D(graphics.GraphicsDevice, rtPieS, rtPieS, 1, SurfaceFormat.Color);
                        rtPie[1] = new RenderTarget2D(graphics.GraphicsDevice, rtPieS, rtPieS, 1, SurfaceFormat.Color);
                        rtPie[2] = new RenderTarget2D(graphics.GraphicsDevice, rtPieS, rtPieS, 1, SurfaceFormat.Color);
                        rtPie[3] = new RenderTarget2D(graphics.GraphicsDevice, rtPieS, rtPieS, 1, SurfaceFormat.Color);

                        if (HALF_RENDER)
                        {
                            screenTarget = new RenderTarget2D(graphics.GraphicsDevice, windowwidth / 2, windowheight / 2, 1, SurfaceFormat.Color);
                            screenTargetPre = new RenderTarget2D(graphics.GraphicsDevice, windowwidth / 2, windowheight / 2, 1, SurfaceFormat.Color);
                        }
                        else
                        {
                            screenTarget = new RenderTarget2D(graphics.GraphicsDevice, windowwidth, windowheight, 1, SurfaceFormat.Color);
                            screenTargetPre = new RenderTarget2D(graphics.GraphicsDevice, windowwidth, windowheight, 1, SurfaceFormat.Color);
                        }
                        screenTargetFinal = new RenderTarget2D(graphics.GraphicsDevice, windowwidth, windowheight, 1, SurfaceFormat.Color);                        

                        Board.InitModel(graphics,content,engine);
                        texRockstarRed = content.Load<Texture2D>("graphics\\red");
                        texRockstarRing = content.Load<Texture2D>("graphics\\ring");
                        texRockstarCover = content.Load<Texture2D>("graphics\\starcover");
                        texRockMeterOutline = content.Load<Texture2D>("graphics\\rockmeter");
                        texWhite = content.Load<Texture2D>("graphics\\white");
                        texRockMeterLogoStem = content.Load<Texture2D>("graphics\\logo_stem");
                        texRockMeterGuitarLogo = content.Load<Texture2D>("graphics\\guitar_logo");
                        texRockMeterBassLogo = content.Load<Texture2D>("graphics\\bass_logo");
                        texRockMeterDrumLogo = content.Load<Texture2D>("graphics\\drums_logo");
                        texRockMeterSingerLogo = content.Load<Texture2D>("graphics\\vocal_logo");
                        texScoreBoard = content.Load<Texture2D>("graphics\\scoreboard");
                        texLine = content.Load<Texture2D>("graphics\\line");
                        texLineEnd = content.Load<Texture2D>("graphics\\linetaper");
                        texGlow = content.Load<Texture2D>("graphics\\triggerglow");

                        texShard = new Texture2D[8];
                        for (int k = 0; k < 8; k++)
                            texShard[k] = content.Load<Texture2D>("graphics\\glassshard0" + (k + 1));
                        texSpark = content.Load<Texture2D>("graphics\\spark");
                        Board.boardTexPlain = new Texture2D[2][];
                        for (int i = 0; i < 2; i++)
                        {
                            Board.boardTexPlain[i] = new Texture2D[Board.boardValidBPM.Length];
                            for (int k = 0; k < Board.boardValidBPM.Length; k++)
                            {
                                Board.boardTexPlain[i][Board.boardBeatsIndex[Board.boardValidBPM[k]]] = content.Load<Texture2D>("graphics\\board_" + (i + 4) + "" + (Board.boardValidBPM[k]));
                            }
                        }
                        Board.texTriggerBorder = content.Load<Texture2D>("graphics\\triggerborder");
                        Board.SPMBorder = content.Load<Texture2D>("graphics\\SPMBorder");
                        Board.SPBoardTex = content.Load<Texture2D>("graphics\\flames");
                        Board.SPMFill = content.Load<Texture2D>("graphics\\SPMFill");
                        Board.SPMRbg = content.Load<Texture2D>("graphics\\SPMRbg");
                        Board.drumfillTex = content.Load<Texture2D>("graphics\\drumfill");
                        Board.SPMRfg = content.Load<Texture2D>("graphics\\SPMRfg");
                        Board.SPMRbgb = content.Load<Texture2D>("graphics\\SPMRbgb");
                        Board.SPMRbgs = content.Load<Texture2D>("graphics\\SPMRbgs");
                        Board.SPMRslice = content.Load<Texture2D>("graphics\\SPMRslice");
                        Board.SPMRnum = new Texture2D[8];
                        Board.SPMRnum[0] = null;
                        Board.SPMRnum[1] = content.Load<Texture2D>("graphics\\SPMRnum2");
                        Board.SPMRnum[2] = content.Load<Texture2D>("graphics\\SPMRnum3");
                        Board.SPMRnum[3] = content.Load<Texture2D>("graphics\\SPMRnum4");
                        Board.SPMRnum[4] = content.Load<Texture2D>("graphics\\SPMRnum5");
                        Board.SPMRnum[5] = content.Load<Texture2D>("graphics\\SPMRnum6");
                        Board.SPMRnum[6] = null;
                        Board.SPMRnum[7] = content.Load<Texture2D>("graphics\\SPMRnum8");
                        Board.SPMFlashTex = content.Load<Texture2D>("graphics\\SPMFlash");
                        Board.texTriggerBorderLit = content.Load<Texture2D>("graphics\\triggerborderlit");
                        Board.texTriggers = new Texture2D[5];
                        for (int i = 0; i < 5; i++)
                            Board.texTriggers[i] = content.Load<Texture2D>("graphics\\trigger" + i);
                        Board.texTriggersLit = new Texture2D[5];
                        for (int i = 0; i < 5; i++)
                            Board.texTriggersLit[i] = content.Load<Texture2D>("graphics\\triggerlit" + i);
                        Board.texNotes = new Texture2D[5];
                        for (int i = 0; i < 5; i++)
                            Board.texNotes[i] = content.Load<Texture2D>("graphics\\notes" + i);
                        texRockstarRingHiLi = new Texture2D[11];
                        for (int i = 0; i <= 9; i++)
                            texRockstarRingHiLi[i] = content.Load<Texture2D>("graphics\\border0" + i);
                        texRockstarRingHiLi[10] = content.Load<Texture2D>("graphics\\border10");
                        GC.Collect();
                        loading &= (byte)(~S_INGAME & 255);
                        loaded |= S_INGAME;
                    };

                    Thread myThread = new Thread(ThreadStarter);
                    loading |= S_INGAME;
                    myThread.Start();
                }
                #endregion
            }

        }

        //unload all content except for c
        private void UnloadContent(byte c)
        {
            if ((loaded & ((byte)1 << c)) == 0 && (loading & ((byte)1 << c)) == 0)
            {
                //basically:
                //everything that shares all cats listed
                //should be destroyed
                //if((loaded&(S_MAINMENU|S_INGAME|S_CHOOSESONG|S_CHOOSECONT))==0 &&
                //   (c&(S_MAINMENU|S_INGAME|S_CHOOSESONG|S_CHOOSECONT))==0)

                if ((loaded & (S_CHOOSECONT | S_CHOOSESONG | S_INGAME | S_MAINMENU)) != 0 &&
                   (c & (S_CHOOSECONT | S_CHOOSESONG | S_INGAME | S_MAINMENU)) == 0)
                {
                    texWhite = null;
                    texRockMeterGuitarLogo = null;
                    texRockMeterBassLogo = null;
                    texRockMeterDrumLogo = null;
                    texRockMeterSingerLogo = null;
                    GC.Collect();
                }

                //CHOOSECONT Specific content
                if((loaded&(S_CHOOSECONT))!=0 &&
                   (c&(S_CHOOSECONT))==0)
                {
                    rtNote = null;
                    concrTex = null;
                    concrBM = null;
                    nPadMdl = null;
                    arrowMdl = null;
                    nailMdl = null;
                    arrowTex = null;
                    strat = null;
                    stand = null;
                    stratTex = null;
                    nPadTex = null;
                    rustyTex = null;
                    arrowRot = null;
                    arrowTimer = null;
                    charNameSelected = null;
                    charNames = null;
                    flames = null;
                    ContGUIData.KB_ICO_GUITAR = null;
                    ContGUIData.KB_ICO_DRUM = null;
                    ContGUIData.KB_ICO_BLUR = null;
                    ContGUIData.DRUMS_ICO = null;
                    ContGUIData.GUITAR_ICO = null;
                    hairl = null;
                    hairr = null;
                    flameTex = null;
                    texContinue = null;
                    loaded &=  (byte)(~S_CHOOSECONT & 255);//loaded &= ~S_CHOOSECONT;
                    GC.Collect();
                }

                //INGAME Specific content
                if((loaded&(S_INGAME))!=0 &&
                   (c&(S_INGAME))==0)
                {
                    rtBar = null;
                    rtPie = null;

                    texRockstarRed = null;
                    texRockstarRing = null;
                    texRockstarCover = null;
                    texRockMeterOutline = null;
                    texRockMeterLogoStem = null;
                    
                    texScoreBoard = null;
                    texLine = null;
                    texLineEnd = null;
                    texGlow = null;

                    texShard = null;
                    texSpark = null;
                    Board.boardTexPlain = null;
                    Board.texTriggerBorder = null;
                    Board.SPMBorder = null;
                    Board.SPBoardTex = null;
                    Board.SPMFill = null;
                    Board.SPMRbg = null;
                    Board.drumfillTex = null;
                    Board.SPMRfg = null;
                    Board.SPMRbgb = null;
                    Board.SPMRbgs = null;
                    Board.SPMRslice = null;
                    Board.SPMRnum = null;
                    Board.SPMFlashTex = null;
                    Board.texTriggerBorderLit = null;
                    Board.texTriggers = null;
                    Board.texTriggersLit = null;
                    Board.texNotes = null;
                    texRockstarRingHiLi = null;
                    loaded &= (byte)(~S_INGAME & 255);//loaded &= ~S_INGAME;
                    GC.Collect();
                }
            }

        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();

            windowheight = Window.ClientBounds.Height;
            windowwidth = Window.ClientBounds.Width;

            Random r = new Random();
            GetGamepadStates(false);

            if ((loaded & screen) == 0)
            {
                LoadContent((byte)screen);
            }
            else
            {
                UnloadContent(screen);
                #region mainmenu
                if (screen == S_MAINMENU)
                {
                    if (mmenu_ticker <= 0)
                    {
                        int collective = 0;
                        bool green=false, red=false;
                        GamePadState[] conts = { GamePad.GetState(PlayerIndex.One), GamePad.GetState(PlayerIndex.Two), GamePad.GetState(PlayerIndex.Three), GamePad.GetState(PlayerIndex.Four) };
                        for (int i = 0; i < 4; i++)
                            if (conts[i].IsConnected)
                            {
                                if (conts[i].DPad.Down == ButtonState.Pressed)
                                    collective--;
                                if (conts[i].DPad.Up == ButtonState.Pressed)
                                    collective++;
                                if (conts[i].Buttons.A == ButtonState.Pressed)
                                    green = true;
                                if (conts[i].Buttons.B == ButtonState.Pressed)
                                    red = true;
                            }
                        if (Keyboard.GetState().IsKeyDown(Keys.Down))
                            collective--;
                        if (Keyboard.GetState().IsKeyDown(Keys.Up))
                            collective++;
                        if (Keyboard.GetState().IsKeyDown(Keys.Enter) || Keyboard.GetState().IsKeyDown(Keys.Space) || Keyboard.GetState().IsKeyDown(Keys.A))
                            green = true;
                        if (Keyboard.GetState().IsKeyDown(Keys.Back) || Keyboard.GetState().IsKeyDown(Keys.Escape))
                            red = true;
                        if (mmenu_select % 10 == 0)
                        {
                            mmenu_select -= 10 * collective;
                            while (mmenu_select < 10)
                                mmenu_select += 10;
                            while (mmenu_select >= 50)
                                mmenu_select -= 10;

                            if (green && (mmenu_select == 20 || mmenu_select==40))
                                mmenu_select++;
                        }
                        else
                        {
                            if (mmenu_select > 20 && mmenu_select < 30)
                            {
                                mmenu_select -= collective;
                                while (mmenu_select < 21)
                                    mmenu_select += 1;
                                while (mmenu_select > 24)
                                    mmenu_select -= 1;

                                if (green && mmenu_select == 21)
                                    screen = S_CHOOSECONT;
                            }
                            if (mmenu_select == 41)
                                this.Exit();

                                
                            if (red)
                                mmenu_select = mmenu_select / 10 * 10;
                        }
                        if (collective != 0 || green || red)
                            mmenu_ticker = 200;
                    }
                    else
                        mmenu_ticker -= gameTime.ElapsedGameTime.Milliseconds;
                }
                #endregion
                #region diffscreen
                else if (screen == S_CHOOSEDIFF)
                {
                    if (mmenu_ticker <= 0)
                    {
                        bool green = false, red = false;
                        GamePadState[] conts = { GamePad.GetState(PlayerIndex.One), GamePad.GetState(PlayerIndex.Two), GamePad.GetState(PlayerIndex.Three), GamePad.GetState(PlayerIndex.Four) };
                        for (int i = 0; i < 4; i++)
                            if (conts[i].IsConnected)
                            {
                                if (conts[i].Buttons.A == ButtonState.Pressed)
                                    green = true;
                                if (conts[i].Buttons.B == ButtonState.Pressed)
                                    red = true;
                            }
                        for(int i=0;i<4;i++)
                        if(instruments[i])
                        {
                            if (contInput[i] >= 4)
                            {
                                if (Keyboard.GetState().IsKeyDown(Keys.Down) && diff[i] < 3)
                                { diff[i]++; mmenu_ticker = 200; }
                                else if (Keyboard.GetState().IsKeyDown(Keys.Up) && diff[i] > 0)
                                { diff[i]--; mmenu_ticker = 200; }
                            }
                            else
                            {
                                GamePadState gps = GamePad.GetState((PlayerIndex)contInput[i]);
                                GamePadCapabilities gpc = GamePad.GetCapabilities((PlayerIndex)contInput[i]);
                                bool up=false, down = false;
                                if (gps.DPad.Down == ButtonState.Pressed)
                                    down = true;
                                if (gps.DPad.Up == ButtonState.Pressed)
                                    up = true;
                                if (gpc.GamePadType == GamePadType.DrumKit)
                                {
                                    if (gps.Buttons.X == ButtonState.Pressed)
                                        down = true;
                                    if (gps.Buttons.Y == ButtonState.Pressed)
                                        up = true;
                                }
                                if (up && diff[i] > 0)
                                    diff[i]--;
                                if (down && diff[i] < 3)
                                    diff[i]++;
                                if (up || down)
                                    mmenu_ticker = 200;
                            }
                        }
                        if (Keyboard.GetState().IsKeyDown(Keys.Enter) || Keyboard.GetState().IsKeyDown(Keys.Space) || Keyboard.GetState().IsKeyDown(Keys.A))
                            green = true;
                        if (Keyboard.GetState().IsKeyDown(Keys.Back) || Keyboard.GetState().IsKeyDown(Keys.Escape))
                            red = true;
                        
                        if (green)
                        {
                            screen = S_INGAME;
                            for (int i = 0; i < 4; i++)
                                if (diff[i] == 0)
                                    diff[i] = D_EASY;
                                else if (diff[i] == 1)
                                    diff[i] = D_MEDIUM;
                                else if (diff[i] == 2)
                                    diff[i] = D_HARD;
                                else if (diff[i] == 3)
                                    diff[i] = D_EXPERT;
                            InitForSong(instruments[0], instruments[1], instruments[2], instruments[3], diff, venueName);
                        }
                        if (red)
                        { screen = S_CHOOSESONG; mmenu_ticker = 200; }
                    }
                    else
                        mmenu_ticker -= gameTime.ElapsedGameTime.Milliseconds;
                }
                #endregion
                #region songscreen
                else if (screen == S_CHOOSESONG)
                {
                    int numsongs = 0;
                    for (int i = 0; i < cSongNames.Length; i++)
                        numsongs += cSongNames[i].Length;

                    if (mmenu_ticker <= 0)
                    {
                        int collective = 0;
                        bool green = false, red = false;
                        GamePadState[] conts = { GamePad.GetState(PlayerIndex.One), GamePad.GetState(PlayerIndex.Two), GamePad.GetState(PlayerIndex.Three), GamePad.GetState(PlayerIndex.Four) };
                        for (int i = 0; i < 4; i++)
                            if (conts[i].IsConnected)
                            {
                                if (conts[i].DPad.Down == ButtonState.Pressed)
                                    collective--;
                                if (conts[i].DPad.Up == ButtonState.Pressed)
                                    collective++;
                                if (conts[i].Buttons.A == ButtonState.Pressed)
                                    green = true;
                                if (conts[i].Buttons.B == ButtonState.Pressed)
                                    red = true;
                            }
                        if (Keyboard.GetState().IsKeyDown(Keys.Down))
                            collective--;
                        if (Keyboard.GetState().IsKeyDown(Keys.Up))
                            collective++;
                        if (Keyboard.GetState().IsKeyDown(Keys.Enter) || Keyboard.GetState().IsKeyDown(Keys.Space) || Keyboard.GetState().IsKeyDown(Keys.A))
                            green = true;
                        if (Keyboard.GetState().IsKeyDown(Keys.Back) || Keyboard.GetState().IsKeyDown(Keys.Escape))
                            red = true;

                        if (collective > 0 && songselected > 0)
                            songselected--;
                        else if (collective < 0 && songselected < numsongs - 1)
                            songselected++;
                        if (green)
                        {
                            int i = 0;
                            for (int k = 0; k < cSongNames.Length; k++)
                                for (int j = 0; j < cSongNames[k].Length; j++)
                                {
                                    if (i == songselected)
                                        songname = cSongNames[k][j];
                                    i++;
                                }
                            for (int k = 0; k < 4; k++)
                                diff[k] = D_EASY;
                            screen = S_CHOOSEDIFF;
                            mmenu_ticker = 200;
                        }
                        if (red)
                        {screen = S_CHOOSECONT; mmenu_ticker = 200; }
                        if (collective != 0)
                            mmenu_ticker = 200;
                    }
                    else
                        mmenu_ticker -= gameTime.ElapsedGameTime.Milliseconds;
                }
                #endregion
                #region contchoosescreen
                else if (screen == S_CHOOSECONT)
                {
                    if (Keyboard.GetState().GetPressedKeys().Length <= 0)
                        idleTime += gameTime.ElapsedGameTime.Milliseconds / 1000f;
                    else
                        idleTime = 0;

                    //charNameSelected[i] < charNames[i < 3 ? i : 0].Length - 1 ? new Vector4(0f, 1f, 0f, 1f) : new Vector4(1f, 0f, 0f, 1f)
                    if (mmenu_ticker <= 0)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            if (arrowTimer[i] <= 0)
                            {
                                if (i % 2 == 0)
                                    if (charNameSelected[i / 2] <= 0)
                                    {
                                        arrowRot[i] = 0;
                                        arrowTimer[i] = 5000;
                                        continue;
                                    }
                                if (i % 2 == 1)
                                    if (charNameSelected[i / 2] >= charNames[(i / 2) < 3 ? (i / 2) : 0].Length - 1)
                                    {
                                        arrowRot[i] = 0;
                                        arrowTimer[i] = 5000;
                                        continue;
                                    }
                                if (arrowRot[i] < Math.PI * 2)
                                {
                                    arrowRot[i] += gameTime.ElapsedGameTime.Milliseconds / 100f;
                                    if (arrowRot[i] > Math.PI * 2)
                                    {
                                        arrowRot[i] = 0;
                                        arrowTimer[i] = 1000 + (float)(r.NextDouble() * 3000);
                                    }
                                }
                            }
                            else
                                arrowTimer[i] -= gameTime.ElapsedGameTime.Milliseconds;
                        }

                        /* if ((kblev > 0.5 && !finalized[kblev == 1 ? 0 : (int)(kblev + 0.5)]) || kblev <= 0.5)
                         {
                             if (Math.Abs(kbld) < 0.01 && kblev > 0 && Keyboard.GetState().IsKeyDown(Keys.Left))
                                 kbld = -1;
                             if (Math.Abs(kbld) < 0.01 && kblev < 3 && Keyboard.GetState().IsKeyDown(Keys.Right))
                                 kbld = 1;
                         }
                         else if ((kblev > 0.5 && !chosen[kblev == 1 ? 0 : (int)(kblev + 0.5)]) || kblev <= 0.5)
                         {
                             if (charNameSelected[kblev == 1 ? 0 : (int)(kblev + 0.5)] < charNames[kblev == 1 ? 0 : kblev < 3 ? (int)(kblev + 0.5) : 0].Length - 1 && Keyboard.GetState().IsKeyDown(Keys.Right))
                                 charNameSelected[kblev == 1 ? 0 : (int)(kblev + 0.5)]++;
                             if (charNameSelected[kblev == 1 ? 0 : (int)(kblev + 0.5)] > 0 && Keyboard.GetState().IsKeyDown(Keys.Left))
                                 charNameSelected[kblev == 1 ? 0 : (int)(kblev + 0.5)]--;
                         }
                         kblev += kbld * (gameTime.ElapsedGameTime.Milliseconds / 500f);
                         if (kbld > 0)
                             if (kblev >= kblevp + 1)
                             {
                                 kblev = kblevp + 1;
                                 kblevp = kblev;
                                 kbld = 0;
                             }
                         if (kbld < 0)
                             if (kblev <= kblevp - 1)
                             {
                                 kblev = kblevp - 1;
                                 kblevp = kblev;
                                 kbld = 0;
                             }
                         Vector3[] nfo = { new Vector3(-140, 180, -120), new Vector3(-140, 190, -110), new Vector3(-96, 128, -110), new Vector3(32, 128, -110), new Vector3(96, 128, -110) };
                         if (kblev == (int)kblev)
                             kbinfo = new Vector4(nfo[(int)kblev + (kblev == 0 ? 0 : 1)], kblev <= 0 ? (float)Math.PI / 2 : 0);
                         else if (kblev < 1)
                         {
                             if (kblev < 0.2)
                                 kbinfo = new Vector4((nfo[0] * ((0.2f - kblev) / 0.2f)) + (nfo[1] * (kblev % 1 / 0.2f)), (1 - kblev) * (float)(Math.PI / 2));
                             else
                                 kbinfo = new Vector4((nfo[1] * ((1f - kblev) / 0.8f)) + (nfo[2] * ((kblev - 0.2f) / 0.8f)), (1 - kblev) * (float)(Math.PI / 2));
                         }
                         else
                         {
                             if (kbld > 0)
                                 kbinfo = new Vector4((nfo[(int)kblevp + 1] * (1 - (kblev % 1))) + (nfo[(int)(kblevp + kbld) + 1] * (kblev % 1)), 0);
                             else
                                 kbinfo = new Vector4((nfo[(int)kblevp + 1] * (kblev % 1)) + (nfo[(int)(kblevp + kbld) + 1] * (1 - (kblev % 1))), 0);
                         }*/
                        for (int k = 0; k < 4; k++)
                            for (int i = 0; i < flames[k].Length; i++)
                                if (flames[k][i].Z > 0)
                                {
                                    flames[k][i].Z -= gameTime.ElapsedGameTime.Milliseconds / 1000f;
                                    flames[k][i].Y += (k + 1) * 1.5f * gameTime.ElapsedGameTime.Milliseconds / 100f;
                                    flames[k][i].X += (float)(r.NextDouble() - 0.5) * gameTime.ElapsedGameTime.Milliseconds / 50f;
                                }
                        if (gameTime.ElapsedGameTime.Milliseconds < 1000 / 30f)
                            for (int j = 0; j < 5; j++)
                                if (contguis[j].status == 2)
                                    for (int k = 0; k < 4; k++)
                                        for (int i = 0; i < flames[k].Length; i++)
                                            if (flames[k][i].Z <= 0)
                                            {
                                                flames[k][i] = new Vector3((-96 + ((int)(contguis[j].loc - 1) * 64)) + (-24 + (float)(r.NextDouble() * 48)), 180 + (-24 + (float)(r.NextDouble() * 48)), 1);
                                                break;
                                            }
                        /*if (Keyboard.GetState().IsKeyDown(Keys.Space) || Keyboard.GetState().IsKeyDown(Keys.Enter))
                            if ((int)kblev == kblev && kblev > 0)
                                if (wait <= 0)
                                {
                                    if(kblev > 0.5 && finalized[kblev == 1 ? 0 : (int)(kblev + 0.5)] && chosen[kblev == 1 ? 0 : (int)(kblev + 0.5)])
                                    {
                                        screen = S_INGAME;
                                        contInput = new byte[4];
                                        contInput[kblev == 1 ? 0 : (int)(kblev + 0.5)] = 4;
                                        InitForSong(chosen[0], chosen[1], chosen[2], chosen[3], diff, "garage");
                                    }
                                    else if (!finalized[kblev == 1 ? 0 : (int)(kblev + 0.5)])
                                    {
                                        finalized[kblev == 1 ? 0 : (int)kblev] = true;
                                        wait = 30;
                                    }
                                    else if ((kblev > 0.5 && finalized[kblev == 1 ? 0 : (int)(kblev + 0.5)]) || kblev <= 0.5)
                                    {
                                        chosen[kblev == 1 ? 0 : (int)kblev] = true;
                                        leader = kblev == 1 ? 0 : (int)kblev;
                                        wait = 30;
                                    }
                                }
                        if (Keyboard.GetState().IsKeyDown(Keys.Back) || Keyboard.GetState().IsKeyDown(Keys.Escape))
                            if ((int)kblev == kblev && kblev > 0)
                                if (wait <= 0)
                                {
                                    if (chosen[kblev == 1 ? 0 : (int)(kblev + 0.5)])
                                    {
                                        chosen[kblev == 1 ? 0 : (int)kblev] = false;
                                        wait = 30;
                                    }
                                    else if (finalized[kblev == 1 ? 0 : (int)(kblev + 0.5)])
                                    {
                                        finalized[kblev == 1 ? 0 : (int)kblev] = false;
                                    }
                                }

                        if (wait > 0)
                            wait--;*/


                        GetContGUIData();
                        if (counterer > 10)
                        {
                            if (texNote[0] == null)
                                for (int i = 0; i < 4; i++)
                                {
                                    graphics.GraphicsDevice.SetRenderTarget(0, rtNote[i]);
                                    spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
                                    spritebatch.Draw(nPadTex[i], new Rectangle(0, 0, 256, 256), Color.White);
                                    spritebatch.DrawString(sfManager, musicianNames[i], new Vector2(70, 20), Color.Black);
                                    spritebatch.End();
                                    graphics.GraphicsDevice.SetRenderTarget(0, null);
                                    texNote[i] = rtNote[i].GetTexture();
                                }
                        }
                        else
                            counterer++;
                        for (int i = 0; i < 4; i++)
                            finals[i] = false;
                        leader = 0;
                        for (int i = 0; i < 5; i++)
                        {
                            if (contguis[i].nextLoc > 0)
                                finals[contguis[i].nextLoc - 1] = true;
                            if (contguis[i].GetLeaderVal() * ((contguis[i].status == 2) ? 1 : 0) > contguis[leader].GetLeaderVal() * ((contguis[leader].status == 2) ? 1 : 0))
                                leader = i;
                        }
                        for (int i = 0; i < 5; i++)
                        {
                            ContGUIData.RETURN_VALUE ret = contguis[i].Update(gameTime, finals);
                            if (ret != ContGUIData.RETURN_VALUE.NOTHING)
                            {
                                if (ret == ContGUIData.RETURN_VALUE.NEXT_SCREEN)
                                {
                                    int cgcount = 0;
                                    for (int p = 0; p < contguis.Length; p++)
                                        if (contguis[p].loc > 0.5)
                                            cgcount++;
                                    if(cgcount>0)
                                    if (leader == i)
                                    {
                                        screen = S_CHOOSESONG;
                                        contInput = new byte[4];
                                        rockerNames = new String[4];
                                        for (int k = 0; k < 4; k++)
                                        { contInput[k] = 255; instruments[k] = false; rockerNames[k] = null; }
                                        for (int k = 0; k < 5; k++)
                                            if (contguis[k].status == 2)
                                            {
                                                rockerNames[(int)contguis[k].loc - 1] = charNameSelected[(int)contguis[k].loc - 1] > 0 ? charNames[(int)contguis[k].loc - 1 > 2 ? 0 : (int)contguis[k].loc - 1][charNameSelected[(int)contguis[k].loc - 1]] : "Default";
                                                instruments[(int)contguis[k].loc - 1] = true;
                                                contInput[(int)contguis[k].loc - 1] = (byte)((int)contguis[k].index >= 0 ? (int)contguis[k].index : 4);
                                            }
                                        mmenu_ticker = 200;
                                    }
                                    idleTime = 0;
                                }
                                else
                                {
                                    idleTime = 0;
                                    if (ret == ContGUIData.RETURN_VALUE.INCREMENT_NAME && charNameSelected[(int)contguis[i].loc - 1] < charNames[((int)contguis[i].loc - 1) <= 2 ? ((int)contguis[i].loc - 1) : 0].Length - 1)
                                    {
                                        if (contguis[i].loc == 1 && charNameSelected[0] + 1 == charNameSelected[3] && charNameSelected[0] + 2 < charNames[0].Length)
                                            charNameSelected[(int)contguis[i].loc - 1]++;
                                        else if (contguis[i].loc == 4 && charNameSelected[3] + 1 == charNameSelected[0] && charNameSelected[3] + 2 < charNames[0].Length)
                                            charNameSelected[(int)contguis[i].loc - 1]++;
                                        else if ((contguis[i].loc == 1 && charNameSelected[0] + 1 == charNameSelected[3]))
                                            charNameSelected[(int)contguis[i].loc - 1]--;
                                        else if (contguis[i].loc == 4 && charNameSelected[3] + 1 == charNameSelected[0])
                                            charNameSelected[(int)contguis[i].loc - 1]--;
                                        charNameSelected[(int)contguis[i].loc - 1]++;
                                    }
                                    if (ret == ContGUIData.RETURN_VALUE.DECREMENT_NAME && charNameSelected[(int)contguis[i].loc - 1] > -1)
                                    {
                                        if (contguis[i].loc == 1 && charNameSelected[0] - 1 == charNameSelected[3] && charNameSelected[0] - 2 >= -1)
                                            charNameSelected[(int)contguis[i].loc - 1]--;
                                        else if (contguis[i].loc == 4 && charNameSelected[3] - 1 == charNameSelected[0] && charNameSelected[3] - 2 >= -1)
                                            charNameSelected[(int)contguis[i].loc - 1]--;
                                        else if (contguis[i].loc == 1 && charNameSelected[0] - 1 == charNameSelected[3] && charNameSelected[0] - 1 != -1)
                                            charNameSelected[(int)contguis[i].loc - 1]++;
                                        else if (contguis[i].loc == 4 && charNameSelected[3] - 1 == charNameSelected[0] && charNameSelected[3] - 1 != -1)
                                            charNameSelected[(int)contguis[i].loc - 1]++;
                                        charNameSelected[(int)contguis[i].loc - 1]--;
                                    }
                                    ort = (RenderTarget2D)graphics.GraphicsDevice.GetRenderTarget(0);
                                    graphics.GraphicsDevice.SetRenderTarget(0, rtNote[(int)contguis[i].loc - 1]);
                                    spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
                                    spritebatch.Draw(nPadTex[i], new Rectangle(0, 0, 256, 256), Color.White);
                                    spritebatch.DrawString(sfManager, musicianNames[(int)contguis[i].loc - 1], new Vector2(70, 20), Color.Black);
                                    if (contguis[i].status == 1)
                                        spritebatch.DrawString((int)contguis[i].loc - 1 == 0 || (int)contguis[i].loc - 1 == 3 ? sfGuitarist : (int)contguis[i].loc - 1 == 1 ? sfSinger : sfDrummer, (charNameSelected[(int)contguis[i].loc - 1]) >= 0 ? charNames[((int)contguis[i].loc - 1 < 3) ? (int)contguis[i].loc - 1 : 0][charNameSelected[(int)contguis[i].loc - 1]] : "New Rocker", new Vector2(100, 80), Color.Black, (float)Math.PI / 4 - 0.07f, new Vector2(0, 0), 1.4f, SpriteEffects.None, 0);
                                    spritebatch.End();
                                    graphics.GraphicsDevice.SetRenderTarget(0, ort);
                                    texNote[(int)contguis[i].loc - 1] = rtNote[(int)contguis[i].loc - 1].GetTexture();
                                }
                            }
                        }

                        for (int i = 0; i < 5; i++)
                            if (contguis[i].index >= 0)
                                if ((((int)contguis[i].index == 4 && Keyboard.GetState().IsKeyDown(Keys.Back)) || ((int)contguis[i].index != 4 && GamePad.GetState((PlayerIndex)contguis[i].index).IsButtonDown(Buttons.B))) && contguis[i].status == 0)
                                { screen = S_MAINMENU; mmenu_ticker = 200; }
                        /*for (int i = 0; i < 4; i++)
                        contInput[i] = 100;

                    for (byte i = 0; i < 4; i++)
                    {
                        if (contCapabilities[i].GamePadType == GamePadType.Guitar)
                        {
                            if (contInput[0] >=100)
                                contInput[0] = i;
                            else if (contInput[3] >=100)
                                contInput[3] = i;
                        }
                        else if (contCapabilities[i].GamePadType == GamePadType.DrumKit)
                        {
                            if (contInput[2] >= 100)
                                contInput[2] = i;
                        }
                        else if (contCapabilities[i].GamePadType == GamePadType.GamePad)
                        {
                            if (contInput[1] >= 100)
                                contInput[1] = i;
                        }
                    }

                    bool noone = true;
                    for (int i = 0; i < 4; i++)
                        if (contInput[i] < 100)
                            noone = false;
                    if (noone)
                        contInput[2] = 4;*/
                    }
                    else
                        mmenu_ticker -= gameTime.ElapsedGameTime.Milliseconds;
                }
                #endregion
                #region ingame
                else if (screen == S_INGAME)
                {

                    //start song TEMPORARY CODE? probably not...
                    if (started == false)
                    {
                        SongStartTime = DateTime.Now.Ticks +50000000;
                        started = true;
                    }
                    UpdateGibs(gameTime);

                    long currenttime = DateTime.Now.Ticks - SongStartTime;

                    if (song.IsOver(currenttime))
                        screen = S_RESULTS;
                    if(renderLevel>0)
                        venue.Update(gameTime, currenttime, engine, song);
                    matView = venue.GetViewMatrix();
                    //audioEngine.Update();

                    song.Update((long)currenttime);

                    ProcessInput(gameTime);

                    if (rockstarDir > Math.PI * 2)
                        rockstarDir = 0;
                    rockstarDir += 1f / gameTime.ElapsedGameTime.Milliseconds;
                    if (GetRockstarAmount() > lastStar && lastStar <= 5)
                    {
                        lastStar++;
                        //audioSoundBank.PlayCue("starching");
                    }
                    if (instruments[0] && contInput[0] < 4)
                        boards[0].Whammy(controllers[contInput[0]].ThumbSticks.Right.X, currenttime / (TicksPerSecond / 1000));
                    if (instruments[3] && contInput[3] < 4)
                        boards[3].Whammy(controllers[contInput[3]].ThumbSticks.Right.X, currenttime / (TicksPerSecond / 1000));
                }
                #endregion
                #region results
                else if (screen == S_RESULTS)
                {
                    bool green=false, red=false;//what should red be used for?
                        GamePadState[] conts = { GamePad.GetState(PlayerIndex.One), GamePad.GetState(PlayerIndex.Two), GamePad.GetState(PlayerIndex.Three), GamePad.GetState(PlayerIndex.Four) };
                        for (int i = 0; i < 4; i++)
                            if (conts[i].IsConnected)
                            {
                                if (conts[i].Buttons.A == ButtonState.Pressed)
                                    green = true;
                                if (conts[i].Buttons.B == ButtonState.Pressed)
                                    red = true;
                            }
                        if (Keyboard.GetState().IsKeyDown(Keys.Enter) || Keyboard.GetState().IsKeyDown(Keys.Space) || Keyboard.GetState().IsKeyDown(Keys.A))
                            green = true;
                        if (Keyboard.GetState().IsKeyDown(Keys.Back) || Keyboard.GetState().IsKeyDown(Keys.Escape))
                            red = true;
                        if (green)
                        { screen = S_MAINMENU; songname = ""; song = null; boards = null; boardsTarget = null; started = false; mmenu_ticker = 200; mmenu_select = 0; }
                }
                #endregion
                else
                {
                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            if ((loaded & screen) == 0)
            {
                graphics.GraphicsDevice.Clear(Color.Black);
                spritebatch.Begin();
                spritebatch.DrawString(DefaultFont, "LOADING", new Vector2(100, 100), Color.White);
                spritebatch.End();
            }
            else
            {
                #region mainmenu
                if (screen == S_MAINMENU)
                {
                    graphics.GraphicsDevice.Clear(Color.Black);
                    spritebatch.Begin();
                    if(mmenu_select>=10 && mmenu_select <20)
                        spritebatch.DrawString(DefaultFont,"SINGLE PLAYER",new Vector2(100,100),Color.Red);
                    else
                        spritebatch.DrawString(DefaultFont,"SINGLE PLAYER",new Vector2(100,100),Color.DarkRed);
                    if(mmenu_select>=20 && mmenu_select <30)
                        spritebatch.DrawString(DefaultFont,"MULTIPLAYER",new Vector2(100,150),Color.Yellow);
                    else
                        spritebatch.DrawString(DefaultFont,"MULTIPLAYER",new Vector2(100,150),Color.Gray);
                    if (mmenu_select > 20 && mmenu_select < 30)
                    {
                        if(mmenu_select==21)
                            spritebatch.DrawString(DefaultFont,"QUICKPLAY",new Vector2(400,110),Color.Yellow);
                        else
                            spritebatch.DrawString(DefaultFont,"QUICKPLAY",new Vector2(400,110),Color.Gray);
                        if(mmenu_select==22)
                            spritebatch.DrawString(DefaultFont,"BAND WORLD TOUR",new Vector2(400,160),Color.Red);
                        else
                            spritebatch.DrawString(DefaultFont,"BAND WORLD TOUR",new Vector2(400,160),Color.DarkRed);
                        if(mmenu_select==23)
                            spritebatch.DrawString(DefaultFont,"TUG OF WAR",new Vector2(400,210),Color.Red);
                        else
                            spritebatch.DrawString(DefaultFont,"TUG OF WAR",new Vector2(400,210),Color.DarkRed);
                        if(mmenu_select==24)
                            spritebatch.DrawString(DefaultFont,"SCORE BATTLE",new Vector2(400,260),Color.Red);
                        else
                            spritebatch.DrawString(DefaultFont,"SCORE BATTLE",new Vector2(400,260),Color.DarkRed);
                    }
                    if(mmenu_select>=30 && mmenu_select <40)
                        spritebatch.DrawString(DefaultFont,"OPTIONS",new Vector2(100,200),Color.Red);
                    else
                        spritebatch.DrawString(DefaultFont,"OPTIONS",new Vector2(100,200),Color.DarkRed);
                    if(mmenu_select>=40 && mmenu_select <50)
                        spritebatch.DrawString(DefaultFont,"EXIT",new Vector2(100,250),Color.Yellow);
                    else
                        spritebatch.DrawString(DefaultFont,"EXIT",new Vector2(100,250),Color.Gray);

                    
                    spritebatch.End();
                }
                #endregion
                #region diffscreen
                else if (screen == S_CHOOSEDIFF)
                {
                    graphics.GraphicsDevice.Clear(Color.Black);
                    spritebatch.Begin();

                    spritebatch.DrawString(DefaultFont, "Choose Difficulty", new Vector2(10, 10), Color.Green);

                    for(int i=0;i<4;i++)
                    if (instruments[i])
                    {
                        spritebatch.DrawString(DefaultFont, rockerNames[i], new Vector2(i * 150 + 100, 100), Color.Yellow);
                        spritebatch.DrawString(DefaultFont, DifficultyStr[diff[i]], new Vector2(i * 150 + 100, 200), Color.White);
                    }

                    spritebatch.End();
                }
                #endregion
                #region songscreen
                else if (screen == S_CHOOSESONG)
                {
                    graphics.GraphicsDevice.Clear(Color.Black);
                    spritebatch.Begin();
                    spritebatch.DrawString(DefaultFont, "Choose Song", new Vector2(10, 10), Color.Green);
                    int i = 0, ii=0;
                    for (int j = 0; j < vSongNames.Length; j++)
                    {
                        for (int k = 0; k < vSongNames[j].Length; k++)
                        {
                            if(i>=songoffset)
                                spritebatch.DrawString(DefaultFont, vSongNames[j][k], new Vector2(100 + (k == 0 ? 0 : 50), ii * 40 + 100), k==0?Color.Yellow:i==songselected?Color.White:Color.Gray);
                            if(k!=0)
                                i++;
                            ii++;
                            if (ii >= 12+songoffset)
                                break;
                        }
                        if (ii >= 12+songoffset)
                            break;
                    }
                    spritebatch.End();
                }
                #endregion
                #region contchoosescreen
                else if (screen == S_CHOOSECONT)
                {
                    

                    graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
                    graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
                    //graphics.PreferMultiSampling = true;
                    graphics.ApplyChanges();

                    vd = new VertexDeclaration(graphics.GraphicsDevice, GBVertexFormat.Elements);
                    graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
                    //graphics.GraphicsDevice.

                    engine.Parameters["bumpTexture"].SetValue(texDefaultBM);
                    engine.Parameters["ambientColor"].SetValue(new Vector4(0.1f, 0.1f, 0.1f, 1.0f));
                    engine.Parameters["diffuseColor"].SetValue(new Vector4(0.8f, 0.8f, 0.8f, 1.0f));
                    engine.Parameters["specularColor"].SetValue(new Vector4(1f, 1f, 1f, 1.0f));
                    engine.Parameters["dLDiffuseColor"].SetValue(new Vector4(0, 0, 0, 0));
                    engine.Parameters["dLSpecularColor"].SetValue(new Vector4(0, 0, 0, 0));

                    Random r = new Random();

                    int linum = 0;
                        bool[] plo = new bool[16];
                        Vector3[] plp = new Vector3[16];
                        float[] pln = new float[16];
                        float[] plf = new float[16];
                        Vector3[] pld = new Vector3[16];
                        Vector3[] pls = new Vector3[16];
                    for (int i = 0; i < contguis.Length; i++)
                    {
                        if (contguis[i].status == 2)
                        {
                            plo[linum] = true;
                            plp[linum] = new Vector3(-192+(contguis[i].loc*80), 192,-100);
                            pln[linum] = r.Next(64);
                            plf[linum] = r.Next(64) + 64;
                            pld[linum] = new Vector3(.9f + (float)(r.NextDouble() / 10), .5f + (float)(r.NextDouble() / 10), .2f + (float)(r.NextDouble() / 10));
                            pls[linum] = new Vector3(0.2f, 0.1f, 0.0f);
                            linum++;
                        }
                    }
                    engine.Parameters["pLightOn"].SetValue(plo);
                    engine.Parameters["pLightPos"].SetValue(plp);
                    engine.Parameters["pLightNear"].SetValue(pln);
                    engine.Parameters["pLightFar"].SetValue(plf);
                    engine.Parameters["pLightDiffuse"].SetValue(pld);
                    engine.Parameters["pLightSpecular"].SetValue(pls);
                    engine.Parameters["dLDiffuseColor"].SetValue(new Vector4(0.2f, 0.2f, 0.2f, 1.0f));
                    engine.Parameters["dLSpecularColor"].SetValue(new Vector4(0.0f, 0.0f, 0.0f, 1.0f));
                    engine.Parameters["dLightDir"].SetValue(new Vector3(0, 1, 1));
                    float vmul = 2f, hmul = 2f;

                    engine.CurrentTechnique = engine.Techniques["menutechnique"];
                    engine.CommitChanges();

                    engine.Begin();
                    foreach (EffectPass pass in engine.CurrentTechnique.Passes)
                    {
                        pass.Begin();
                        SetProjMatrix(Window.ClientBounds.Width,Window.ClientBounds.Height);
                        engine.Parameters["fullbright"].SetValue(false);

                        if (idleTime < 29.5)
                            matView = Matrix.CreateLookAt(new Vector3(0, 128, 128), new Vector3(0, 128, 0), new Vector3(0, 1, 0));
                        else
                            matView = Matrix.CreateLookAt(new Vector3(0, 128, 128), new Vector3(-16 + ((idleTime * hmul) % 1 < 0.5 ? (idleTime * hmul) % 0.5f * 32 : (1 - ((idleTime * hmul) % .5f * 2)) * 16), 128 - ((idleTime * vmul) % 1 < 0.5 ? (idleTime * vmul) % 0.5f * 32 : (1 - ((idleTime * vmul) % .5f * 2)) * 16), 0), new Vector3(0, 1, 0));
                        //render the background graphics
                        engine.Parameters["view"].SetValue(matView);
                        engine.Parameters["proj"].SetValue(matProj);
                        engine.Parameters["viewInverse"].SetValue(Matrix.Invert(matView));

                        Matrix matRot, matScale, matTranslate;
                        {
                            matTranslate = Matrix.CreateTranslation(-32, 120, -128);
                            matRot = Matrix.CreateRotationX((float)Math.PI / 2);
                            matScale = Matrix.CreateScale(256, 192, 128);

                            engine.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                            engine.Parameters["wRot"].SetValue(matRot);
                            engine.Parameters["diffuseTexture"].SetValue(concrTex);
                            engine.Parameters["bumpTexture"].SetValue(concrBM);
                            engine.Parameters["shininess"].SetValue(0.25f);
                            engine.Parameters["SpecularEnabled"].SetValue(false);
                            engine.Parameters["vertexAlpha"].SetValue(true);
                            engine.Parameters["BumpMappingEnabled"].SetValue(true);
                            engine.CommitChanges();

                            graphics.GraphicsDevice.VertexDeclaration = vd;
                            graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                            graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                            graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                            graphics.GraphicsDevice.Vertices[0].SetSource(square, 0, GBVertexFormat.SizeInBytes);
                            graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
                            graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                        }

                        for (int i = 0; i < 4; i++)
                        {
                            matTranslate = Matrix.CreateTranslation(-96 + (64 * i), 192, -126);
                            matRot = Matrix.Identity;
                            matScale = Matrix.CreateScale(16, 16, 16);

                            engine.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                            engine.Parameters["wRot"].SetValue(matRot);
                            engine.Parameters["diffuseTexture"].SetValue(texNote[i]);
                            engine.Parameters["diffuseColor"].SetValue(new Vector4(0.8f, 0.8f, 0.8f, 1.0f));
                            engine.Parameters["bumpTexture"].SetValue(texDefaultBM);
                            engine.Parameters["shininess"].SetValue(0.25f);
                            engine.Parameters["SpecularEnabled"].SetValue(false);
                            engine.Parameters["vertexAlpha"].SetValue(false);
                            engine.Parameters["BumpMappingEnabled"].SetValue(false);
                            engine.CommitChanges();

                            foreach (ModelMesh mesh in nPadMdl.Meshes)
                            {
                                foreach (ModelMeshPart meshpart in mesh.MeshParts)
                                {
                                    graphics.GraphicsDevice.VertexDeclaration = meshpart.VertexDeclaration;
                                    graphics.GraphicsDevice.Vertices[0].SetSource(mesh.VertexBuffer, meshpart.StreamOffset, meshpart.VertexStride);
                                    graphics.GraphicsDevice.Indices = mesh.IndexBuffer;
                                    graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, meshpart.BaseVertex, 0, meshpart.NumVertices, meshpart.StartIndex, meshpart.PrimitiveCount);
                                }
                            }
                        }

                        for (int i = 0; i < 5; i++)
                        {
                            if (contguis[i].status==1)
                            {
                                matTranslate = Matrix.CreateTranslation(-96 + (64 * (contguis[i].loc-1)) - 24, 180, -120);
                                matRot = Matrix.CreateRotationZ(arrowRot[(int)(contguis[i].loc-1) * 2]) * Matrix.CreateRotationY(-(float)Math.PI / 2) * Matrix.CreateRotationZ(-MathHelper.PiOver2);
                                matScale = Matrix.CreateScale(4, 2, 4);

                                engine.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                                engine.Parameters["wRot"].SetValue(matRot);
                                engine.Parameters["diffuseTexture"].SetValue(arrowTex);
                                engine.Parameters["bumpTexture"].SetValue(texDefaultBM);
                                engine.Parameters["diffuseColor"].SetValue(charNameSelected[(int)(contguis[i].loc-1)] > -1 ? new Vector4(0f, 1f, 0f, 1f) : new Vector4(1f, 0f, 0f, 1f));
                                engine.Parameters["specularColor"].SetValue(charNameSelected[(int)(contguis[i].loc-1)] > -1 ? new Vector4(0f, 1f, 0f, 1f) : new Vector4(1f, 0f, 0f, 1f));
                                engine.Parameters["SpecularEnabled"].SetValue(true);
                                engine.Parameters["shininess"].SetValue(4f);
                                engine.Parameters["vertexAlpha"].SetValue(false);
                                engine.Parameters["BumpMappingEnabled"].SetValue(false);
                                engine.CommitChanges();
                                foreach (ModelMesh mesh in arrowMdl.Meshes)
                                {
                                    foreach (ModelMeshPart meshpart in mesh.MeshParts)
                                    {
                                        graphics.GraphicsDevice.VertexDeclaration = meshpart.VertexDeclaration;
                                        graphics.GraphicsDevice.Vertices[0].SetSource(mesh.VertexBuffer, meshpart.StreamOffset, meshpart.VertexStride);
                                        graphics.GraphicsDevice.Indices = mesh.IndexBuffer;
                                        graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, meshpart.BaseVertex, 0, meshpart.NumVertices, meshpart.StartIndex, meshpart.PrimitiveCount);
                                    }
                                }

                                matTranslate = Matrix.CreateTranslation(-96 + (64 * (int)(contguis[i].loc-1)) + 24, 180, -120);
                                matRot = Matrix.CreateRotationZ(arrowRot[(int)(contguis[i].loc-1) * 2 + 1]) * Matrix.CreateRotationY((float)Math.PI / 2) * Matrix.CreateRotationZ(-MathHelper.PiOver2);
                                matScale = Matrix.CreateScale(4, 2, 4);

                                engine.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                                engine.Parameters["wRot"].SetValue(matRot);
                                engine.Parameters["diffuseColor"].SetValue(charNameSelected[(int)(contguis[i].loc-1)] < charNames[(int)(contguis[i].loc-1) < 3 ? (int)(contguis[i].loc-1) : 0].Length - 1 ? new Vector4(0f, 1f, 0f, 1f) : new Vector4(1f, 0f, 0f, 1f));
                                engine.CommitChanges();
                                foreach (ModelMesh mesh in arrowMdl.Meshes)
                                {
                                    foreach (ModelMeshPart meshpart in mesh.MeshParts)
                                    {
                                        graphics.GraphicsDevice.VertexDeclaration = meshpart.VertexDeclaration;
                                        graphics.GraphicsDevice.Vertices[0].SetSource(mesh.VertexBuffer, meshpart.StreamOffset, meshpart.VertexStride);
                                        graphics.GraphicsDevice.Indices = mesh.IndexBuffer;
                                        graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, meshpart.BaseVertex, 0, meshpart.NumVertices, meshpart.StartIndex, meshpart.PrimitiveCount);
                                    }
                                }
                            }

                        }

                        {
                            matTranslate = Matrix.CreateTranslation(-140, 200, -120);
                            matRot = Matrix.CreateRotationX((float)Math.PI / 4);
                            matScale = Matrix.CreateScale(2);

                            engine.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                            engine.Parameters["wRot"].SetValue(matRot);
                            engine.Parameters["diffuseColor"].SetValue(new Vector4(0.8f, 0.8f, 0.8f, 1f));
                            engine.Parameters["diffuseTexture"].SetValue(rustyTex);
                            engine.CommitChanges();
                            foreach (ModelMesh mesh in nailMdl.Meshes)
                            {
                                foreach (ModelMeshPart meshpart in mesh.MeshParts)
                                {
                                    graphics.GraphicsDevice.VertexDeclaration = meshpart.VertexDeclaration;
                                    graphics.GraphicsDevice.Vertices[0].SetSource(mesh.VertexBuffer, meshpart.StreamOffset, meshpart.VertexStride);
                                    graphics.GraphicsDevice.Indices = mesh.IndexBuffer;
                                    graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, meshpart.BaseVertex, 0, meshpart.NumVertices, meshpart.StartIndex, meshpart.PrimitiveCount);
                                }
                            }
                        }
                        {
                            matTranslate = Matrix.CreateTranslation(-100, 85, -80);
                            matRot = Matrix.CreateRotationY((float)Math.PI / 4);
                            matScale = Matrix.CreateScale(5);

                            engine.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                            engine.Parameters["wRot"].SetValue(matRot);
                            engine.Parameters["diffuseColor"].SetValue(new Vector4(0.2f, 0.2f, 0.2f, 1f));
                            engine.Parameters["diffuseTexture"].SetValue(texWhite);
                            engine.CommitChanges();
                            foreach (ModelMesh mesh in stand.Meshes)
                            {
                                foreach (ModelMeshPart meshpart in mesh.MeshParts)
                                {
                                    graphics.GraphicsDevice.VertexDeclaration = meshpart.VertexDeclaration;
                                    graphics.GraphicsDevice.Vertices[0].SetSource(mesh.VertexBuffer, meshpart.StreamOffset, meshpart.VertexStride);
                                    graphics.GraphicsDevice.Indices = mesh.IndexBuffer;
                                    graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, meshpart.BaseVertex, 0, meshpart.NumVertices, meshpart.StartIndex, meshpart.PrimitiveCount);
                                }
                            }
                        }
                        {
                            matTranslate = Matrix.CreateTranslation(-105, 92, -85);
                            matRot = Matrix.CreateRotationY((float)Math.PI / 2) * Matrix.CreateRotationX((float)Math.PI / 2 - 0.2f) * Matrix.CreateRotationY((float)Math.PI / 4);
                            matScale = Matrix.CreateScale(5);

                            engine.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                            engine.Parameters["wRot"].SetValue(matRot);
                            engine.Parameters["diffuseColor"].SetValue(new Vector4(0.8f, 0.8f, 0.8f, 1f));
                            engine.Parameters["diffuseTexture"].SetValue(stratTex);
                            engine.CommitChanges();
                            foreach (ModelMesh mesh in strat.Meshes)
                            {
                                foreach (ModelMeshPart meshpart in mesh.MeshParts)
                                {
                                    graphics.GraphicsDevice.VertexDeclaration = meshpart.VertexDeclaration;
                                    graphics.GraphicsDevice.Vertices[0].SetSource(mesh.VertexBuffer, meshpart.StreamOffset, meshpart.VertexStride);
                                    graphics.GraphicsDevice.Indices = mesh.IndexBuffer;
                                    graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, meshpart.BaseVertex, 0, meshpart.NumVertices, meshpart.StartIndex, meshpart.PrimitiveCount);
                                }
                            }
                        }
                        for (int k = 0; k < 4; k++)
                            for (int i = 0; i < flames[k].Length; i++)
                                if (flames[k][i].Z > 0)
                                {
                                    matTranslate = Matrix.CreateTranslation(new Vector3(flames[k][i].X, flames[k][i].Y, -117 + (k * 0.5f)));
                                    matRot = Matrix.CreateRotationX((float)Math.PI / 2);
                                    matScale = Matrix.CreateScale(8, 1, Math.Max(24*(1-flames[k][i].Z),4));

                                    engine.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                                    engine.Parameters["wRot"].SetValue(matRot);
                                    engine.Parameters["diffuseColor"].SetValue(new Vector4(0.8f, 0.8f, 0.8f, 1.0f));
                                    engine.Parameters["specularColor"].SetValue(new Vector4(0f, 0f, 0f, 1f));
                                    float alpha = 0;
                                    if(flames[k][i].Z>3/4f)
                                        alpha = 1-((flames[k][i].Z-3/4f) *4);
                                    else 
                                        alpha = flames[k][i].Z;
                                    engine.Parameters["wAlpha"].SetValue(alpha);
                                    engine.Parameters["diffuseTexture"].SetValue(flameTex);
                                    engine.Parameters["fullbright"].SetValue(true);
                                    engine.Parameters["SpecularEnabled"].SetValue(false);
                                    engine.Parameters["vertexAlpha"].SetValue(true);
                                    engine.Parameters["BumpMappingEnabled"].SetValue(false);
                                    engine.CommitChanges();

                                    graphics.GraphicsDevice.VertexDeclaration = vd;
                                    graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                                    graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                                    graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                                    graphics.GraphicsDevice.Vertices[0].SetSource(square, 0, GBVertexFormat.SizeInBytes);
                                    graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
                                    graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                                }
                        
                                    engine.Parameters["fullbright"].SetValue(true);
                        {//keyboard gui
                            {
                                matTranslate = Matrix.CreateTranslation(new Vector3(contguis[0].info.X, contguis[0].info.Y, contguis[0].info.Z));
                                matRot = Matrix.CreateRotationX((float)Math.PI / 2) * Matrix.CreateRotationZ(contguis[0].info.W);
                                matScale = Matrix.CreateScale(32, 32, 32);

                                engine.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                                engine.Parameters["wRot"].SetValue(matRot);
                                engine.Parameters["diffuseTexture"].SetValue(contguis[0].loc <= 0.99 ? ContGUIData.KB_ICO_BLUR : contguis[0].loc <= 1.99 ? ContGUIData.KB_ICO_GUITAR : contguis[0].loc <= 2.99 ? ContGUIData.KB_ICO_DRUM : ContGUIData.KB_ICO_GUITAR);
                                engine.Parameters["bumpTexture"].SetValue(texDefaultBM);
                                engine.Parameters["shininess"].SetValue(0.25f);
                                engine.Parameters["wAlpha"].SetValue(1);
                                engine.Parameters["SpecularEnabled"].SetValue(false);
                                engine.Parameters["vertexAlpha"].SetValue(true);
                                engine.Parameters["BumpMappingEnabled"].SetValue(true);
                                engine.CommitChanges();

                                graphics.GraphicsDevice.VertexDeclaration = vd;
                                graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                                graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                                graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                                graphics.GraphicsDevice.Vertices[0].SetSource(square, 0, GBVertexFormat.SizeInBytes);
                                graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
                                graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                            }
                            if (contguis[0].loc != (int)contguis[0].loc)
                            {
                                matTranslate = Matrix.CreateTranslation(new Vector3(contguis[0].info.X, contguis[0].info.Y, contguis[0].info.Z));
                                matRot = Matrix.CreateRotationX((float)Math.PI / 2) * Matrix.CreateRotationZ(contguis[0].info.W);
                                matScale = Matrix.CreateScale(32, 32, 32);

                                engine.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                                engine.Parameters["wRot"].SetValue(matRot);
                                engine.Parameters["diffuseTexture"].SetValue(contguis[0].loc <= 1 ? ContGUIData.KB_ICO_GUITAR : contguis[0].loc <= 2 ? ContGUIData.KB_ICO_DRUM : ContGUIData.KB_ICO_GUITAR);
                                engine.Parameters["bumpTexture"].SetValue(texDefaultBM);
                                engine.Parameters["shininess"].SetValue(0);
                                engine.Parameters["wAlpha"].SetValue(contguis[0].loc - (int)contguis[0].loc);
                                engine.Parameters["SpecularEnabled"].SetValue(false);
                                engine.Parameters["vertexAlpha"].SetValue(true);
                                engine.Parameters["BumpMappingEnabled"].SetValue(true);
                                engine.CommitChanges();

                                graphics.GraphicsDevice.VertexDeclaration = vd;
                                graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                                graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                                graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                                graphics.GraphicsDevice.Vertices[0].SetSource(square, 0, GBVertexFormat.SizeInBytes);
                                graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
                                graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                            }
                        }
                                    
                        engine.Parameters["wAlpha"].SetValue(1);
                        for(int j=1;j<=4;j++)
                        {//instrument gui
                            {
                                matTranslate = Matrix.CreateTranslation(new Vector3(contguis[j].info.X, contguis[j].info.Y, contguis[j].info.Z-contguis[j].loc));
                                matRot = Matrix.CreateRotationX((float)Math.PI / 2) * Matrix.CreateRotationZ(contguis[j].info.W);
                                matScale = Matrix.CreateScale(32, 32, 32);

                                engine.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                                engine.Parameters["wRot"].SetValue(matRot);
                                if(contguis[j].type==ContGUIData.CONT_TYPE.DRUMSET)
                                    engine.Parameters["diffuseTexture"].SetValue(contguis[j].loc <= 0.99 ? ContGUIData.DRUMS_ICO_BLUR : ContGUIData.DRUMS_ICO);
                                else if(contguis[j].type==ContGUIData.CONT_TYPE.STRATOCASTER)
                                    engine.Parameters["diffuseTexture"].SetValue(contguis[j].loc <= 0.99 ? ContGUIData.GUITAR_ICO_BLUR : ContGUIData.GUITAR_ICO);
                                else if(contguis[j].type==ContGUIData.CONT_TYPE.XPLORER)
                                    engine.Parameters["diffuseTexture"].SetValue(contguis[j].loc <= 0.99 ? ContGUIData.GUITARX_ICO_BLUR : ContGUIData.GUITARX_ICO);
                                else if(contguis[j].type==ContGUIData.CONT_TYPE.MICROPHONE)
                                    engine.Parameters["diffuseTexture"].SetValue(contguis[j].loc <= 0.99 ? ContGUIData.MICROPHONE_ICO_BLUR : ContGUIData.MICROPHONE_ICO);
                                engine.Parameters["bumpTexture"].SetValue(texDefaultBM);
                                engine.Parameters["shininess"].SetValue(0.25f);
                                engine.Parameters["wAlpha"].SetValue(1);
                                engine.Parameters["SpecularEnabled"].SetValue(false);
                                engine.Parameters["vertexAlpha"].SetValue(true);
                                engine.Parameters["BumpMappingEnabled"].SetValue(true);
                                engine.CommitChanges();

                                graphics.GraphicsDevice.VertexDeclaration = vd;
                                graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                                graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                                graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                                graphics.GraphicsDevice.Vertices[0].SetSource(square, 0, GBVertexFormat.SizeInBytes);
                                graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
                                graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                            }
                            if (contguis[j].loc>0 && contguis[j].loc < 1)
                            {
                                matTranslate = Matrix.CreateTranslation(new Vector3(contguis[j].info.X, contguis[j].info.Y, contguis[j].info.Z-contguis[j].loc));
                                matRot = Matrix.CreateRotationX((float)Math.PI / 2) * Matrix.CreateRotationZ(contguis[j].info.W);
                                matScale = Matrix.CreateScale(32, 32, 32);

                                engine.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                                engine.Parameters["wRot"].SetValue(matRot);
                                if(contguis[j].type==ContGUIData.CONT_TYPE.DRUMSET)
                                    engine.Parameters["diffuseTexture"].SetValue(ContGUIData.DRUMS_ICO);
                                else if(contguis[j].type==ContGUIData.CONT_TYPE.STRATOCASTER)
                                    engine.Parameters["diffuseTexture"].SetValue(ContGUIData.GUITAR_ICO);
                                else if(contguis[j].type==ContGUIData.CONT_TYPE.XPLORER)
                                    engine.Parameters["diffuseTexture"].SetValue(ContGUIData.GUITARX_ICO);
                                else if(contguis[j].type==ContGUIData.CONT_TYPE.MICROPHONE)
                                    engine.Parameters["diffuseTexture"].SetValue(ContGUIData.MICROPHONE_ICO);
                                engine.Parameters["bumpTexture"].SetValue(texDefaultBM);
                                engine.Parameters["shininess"].SetValue(0);
                                engine.Parameters["wAlpha"].SetValue(contguis[j].loc);
                                engine.Parameters["SpecularEnabled"].SetValue(false);
                                engine.Parameters["vertexAlpha"].SetValue(true);
                                engine.Parameters["BumpMappingEnabled"].SetValue(true);
                                engine.CommitChanges();

                                graphics.GraphicsDevice.VertexDeclaration = vd;
                                graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                                graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                                graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                                graphics.GraphicsDevice.Vertices[0].SetSource(square, 0, GBVertexFormat.SizeInBytes);
                                graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
                                graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                            }
                        }
                        engine.Parameters["fullbright"].SetValue(false);
                        engine.Parameters["wAlpha"].SetValue(1.0f);

                        pass.End();
                    }
                    engine.End();
                    spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);

                    if (leader >= 0 && contguis[leader].status==2)
                        spritebatch.Draw(texContinue, new Rectangle((int)(Window.ClientBounds.Width * (contguis[leader].loc) / 6), Window.ClientBounds.Height - (Window.ClientBounds.Height / 5), Window.ClientBounds.Width / 6, Window.ClientBounds.Height / 6), Color.Red);

                    if (idleTime > 30)
                    {
                        spritebatch.Draw(hairl, new Rectangle(-20, 0, (int)Window.ClientBounds.Height - (int)((idleTime * hmul) % 1 < 0.5 ? (idleTime * hmul) % 0.5f * (Window.ClientBounds.Height * 2) : (1 - ((idleTime * hmul) % .5f * 2)) * Window.ClientBounds.Height), (int)Window.ClientBounds.Height), Color.White);
                        spritebatch.Draw(hairr, new Rectangle(20 + (int)Window.ClientBounds.Width - (int)((idleTime * hmul) % 1 < 0.5 ? (idleTime * hmul) % 0.5f * (Window.ClientBounds.Height * 2) : (1 - ((idleTime * hmul) % .5f * 2)) * Window.ClientBounds.Height), 0, (int)((idleTime * hmul) % 1 < 0.5 ? (idleTime * hmul) % 0.5f * (Window.ClientBounds.Height * 2) : (1 - ((idleTime * hmul) % .5f * 2)) * Window.ClientBounds.Height), (int)Window.ClientBounds.Height), Color.White);
                    }

                    //spritebatch.DrawString(DefaultFont, "" + contguis[0].type+","+GamePad.GetState(PlayerIndex.One).IsConnected + ","+ GamePad.GetCapabilities(PlayerIndex.One).GamePadType, new Vector2(100, 100), Color.Red);

                    //spritebatch.DrawString(DefaultFont, "" + contguis[0].loc + "::" + contguis[0].info, new Vector2(10, 10), Color.White);

                    spritebatch.End();
                }
                #endregion
                #region ingame
                else if (screen == S_INGAME)
                {
                    ort = (RenderTarget2D)graphics.GraphicsDevice.GetRenderTarget(0);
                    for (int i = 0; i < 4; i++)
                    {
                        if (instruments[i])
                        {
                            graphics.GraphicsDevice.SetRenderTarget(0, rtBar[i]);
                            spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
                            graphics.GraphicsDevice.Clear(new Color(0, 0, 0, 0));
                            spritebatch.Draw(texWhite, new Rectangle(4, 8, 248, 240), Color.Black);
                            spritebatch.Draw(Board.SPMFill, new Rectangle(0, 0, (int)(256 * boards[i].GetSPAmount()), 256), Color.White);
                            spritebatch.Draw(Board.SPMBorder, new Rectangle(0, 0, 256, 256), Color.White);
                            spritebatch.End();
                            graphics.GraphicsDevice.SetRenderTarget(0, rtPie[i]);
                            graphics.GraphicsDevice.Clear(new Color(0, 0, 0, 0));
                            spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
                            if (boards[i].GetMultiplier() >= 4 * (boards[i].IsSPActivated() ? 2 : 1))
                                spritebatch.Draw(Board.SPMRbgb, new Rectangle(0, 0, rtPieS, rtPieS/3), Color.White);
                            else if (boards[i].GetMultiplier() >= 2)
                                spritebatch.Draw(Board.SPMRbgs, new Rectangle(-(rtPieS/4) + (int)(boards[i].multSlide * rtPieS / 4) + (int)((rtPieS/2) * (1 - boards[i].multSlide)), 0, (int)((rtPieS/2) * (boards[i].multSlide + 1)), rtPieS/3), Color.White);
                            if (boards[i].GetMultiplier() != 1)
                                spritebatch.Draw(Board.SPMRnum[boards[i].GetMultiplier() - 1], new Rectangle(-(rtPieS/4) + (int)(boards[i].multSlide * (rtPieS/4)) + (int)((rtPieS/2) * (1 - boards[i].multSlide)), 0, (int)((rtPieS/2) * (boards[i].multSlide + 1)), rtPieS/3), Color.White);
                            spritebatch.Draw(Board.SPMRbg, new Rectangle(0, 0, rtPieS, rtPieS/3), Color.White);
                            for (int k = 0; k < boards[i].GetMultiplierFraction(); k++)
                                spritebatch.Draw(Board.SPMRslice, new Vector2(rtPieS/2, rtPieS/6), new Rectangle(0, 0, rtPieS, rtPieS), Color.White, k * (float)(Math.PI / 5), new Vector2(128, 128),rtPieS/768f, new SpriteEffects(), 0);
                            spritebatch.Draw(Board.SPMRfg, new Rectangle(0, 0, rtPieS, rtPieS/3), Color.White);
                            spritebatch.End();
                        }
                    }
                    graphics.GraphicsDevice.SetRenderTarget(0, ort);
                    for (int i = 0; i < 4; i++)
                    {
                        if (instruments[i])
                        {
                            boards[i].SPMeterTex = rtBar[i].GetTexture();
                            boards[i].SPMRTex = rtPie[i].GetTexture();
                        }
                    }

                    graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;
                    graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
                    graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
                    vd = new VertexDeclaration(graphics.GraphicsDevice, GBVertexFormat.Elements);
                    //graphics.PreferMultiSampling = true;
                    graphics.ApplyChanges();
                    if (renderLevel > 0)
                    {
                        graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
                        if (currentFES == FRAME_EFFECT_STYLE.CREST)
                        {
                            if (countFES < 1)
                                graphics.GraphicsDevice.SetRenderTarget(0, screenTarget);
                            else
                                graphics.GraphicsDevice.SetRenderTarget(0, screenTargetPre);
                        }
                        else
                            graphics.GraphicsDevice.SetRenderTarget(0, screenTarget);

                        engine.Parameters["bumpTexture"].SetValue(texDefaultBM);
                        engine.Parameters["ambientColor"].SetValue(new Vector4(0.1f, 0.1f, 0.1f, 1.0f));
                        engine.Parameters["diffuseColor"].SetValue(new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
                        engine.Parameters["specularColor"].SetValue(new Vector4(1f, 1f, 1f, 1.0f));

                    
                        engine.CurrentTechnique = engine.Techniques["maintechnique"];
                        matProj = venue.GetProjMatrix(windowwidth / (float)windowheight);
                        graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
                        engine.Begin();
                        foreach (EffectPass pass in engine.CurrentTechnique.Passes)
                        {
                            pass.Begin();
                            engine.Parameters["BumpMappingEnabled"].SetValue(false);
                            engine.Parameters["SpecularEnabled"].SetValue(false);

                            engine.Parameters["fullbright"].SetValue(false);
                            matView = venue.GetViewMatrix();
                            //render the background graphics
                            engine.Parameters["view"].SetValue(matView);
                            engine.Parameters["proj"].SetValue(matProj);
                            engine.Parameters["viewInverse"].SetValue(Matrix.Invert(matView));
                            venue.Render(graphics, engine, matProj, vd, gameTime);
                            pass.End();
                        }
                        engine.End();
                    }

                    for (int i = 0; i < boards.Length; i++)
                    {
                        if (!instruments[i])
                            continue;
                        graphics.GraphicsDevice.SetRenderTarget(0, boardsTarget[i]);
                        matProj = Matrix.CreatePerspectiveFieldOfView((float)Math.PI / 4.0f,
                                  boardsTarget[i].Width / (float)boardsTarget[i].Height,
                                  0.1f, 100.0f);
                        graphics.GraphicsDevice.Clear(new Color(new Vector4(0, 0, 0, 0)));
                        engine.CurrentTechnique = engine.Techniques["boardTechnique"];
                        engine.Begin();
                        foreach (EffectPass pass in engine.CurrentTechnique.Passes)
                        {
                            pass.Begin();

                            engine.Parameters["view"].SetValue(Matrix.Identity);
                            engine.Parameters["viewInverse"].SetValue(Matrix.Identity);
                            engine.Parameters["proj"].SetValue(matProj);

                            //get board measure world lengths
                            Vector2[] lenvals = song.GetZVals((DateTime.Now.Ticks - SongStartTime));

                            //determine fling (song start board comes up)
                            Matrix fling;
                            if ((DateTime.Now.Ticks - SongStartTime) / (float)TicksPerSecond > -3)
                                fling = Matrix.CreateRotationX(Board.rotate);
                            else if ((DateTime.Now.Ticks - SongStartTime) / (float)TicksPerSecond > -4)
                                fling = Matrix.CreateRotationX((((((DateTime.Now.Ticks - SongStartTime) / (float)TicksPerSecond) + 4f)) * Board.rotate * 4) - (Board.rotate * 3));
                            else
                                fling = Matrix.CreateRotationX((float)Math.PI / 2);
                            if (Venue.DEBUG_CAM_CONTROL)
                                fling *= Matrix.CreateRotationX(-0.4f);

                            engine.Parameters["fullbright"].SetValue(true);

                            //draw each boards
                            DrawBoard(i, lenvals, fling, false);
                            if (boards[i].IsSPActivated())
                                DrawBoard(i, lenvals, fling, true);
                            DrawNotes(i, fling);
                            DrawBoardDetail(i, fling);
                            DrawWaves(i, fling);
                            //draw the non-world gibs (glass shards sparks)
                            DrawGibs();
                            pass.End();
                        }
                        engine.End();
                    }

                    if (renderLevel > 0)
                    {
                        if (currentFES == FRAME_EFFECT_STYLE.CREST)
                        {
                            graphics.GraphicsDevice.SetRenderTarget(0, screenTargetFinal);
                            graphics.GraphicsDevice.Clear(Color.Black);

                            spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.None);
                            if (countFES < 1)
                            {
                                spritebatch.Draw(lastframe, new Rectangle(0, 0, windowwidth, windowheight), Color.White);
                                spritebatch.Draw(screenTarget.GetTexture(), new Rectangle(0, 0, windowwidth, windowheight), new Color(new Vector4(1, 1, 1, ((countFES)))));
                            }
                            else
                            {
                                lastframe = screenTargetPre.GetTexture();
                                spritebatch.Draw(lastframe, new Rectangle(0, 0, windowwidth, windowheight), Color.White);
                                countFES--;
                            }
                            //spritebatch.Draw(lastframe, new Rectangle(0, 0, windowwidth, windowheight), Color.White);
                            spritebatch.End();
                            countFES += gameTime.ElapsedGameTime.Milliseconds / 500f;

                            graphics.GraphicsDevice.SetRenderTarget(0, null);
                            graphics.GraphicsDevice.Clear(Color.Black);


                            ppEngine.Parameters["gradientTex"].SetValue(gradient);

                            spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
                            ppEngine.CurrentTechnique = ppEngine.Techniques["Gamma"];

                            //ppEngine.Begin();
                            //ppEngine.CurrentTechnique.Passes[0].Begin();
                            ppEngine.Parameters["dotGrainOn"].SetValue(true);
                            ppEngine.Parameters["ValueShift"].SetValue(0.5f);
                            ppEngine.Parameters["grainStrength"].SetValue(.25f);
                            float time = (float)(DateTime.Now.Ticks / 1000 % 90) + 10;
                            ppEngine.Parameters["time"].SetValue(time);
                            ppEngine.CommitChanges();
                            spritebatch.Draw(screenTargetFinal.GetTexture(), new Rectangle(0, 0, windowwidth, windowheight), Color.White);
                        }
                        else
                        {
                            graphics.GraphicsDevice.SetRenderTarget(0, null);
                            graphics.GraphicsDevice.Clear(Color.Black);
                            spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
                            spritebatch.Draw(screenTarget.GetTexture(), new Rectangle(0, 0, windowwidth, windowheight), Color.White);
                        }
                    }
                    else
                    {
                        graphics.GraphicsDevice.SetRenderTarget(0, null);
                        spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
                        graphics.GraphicsDevice.Clear(Color.Black);
                    }
                    //ppEngine.CurrentTechnique.Passes[0].End();
                    //ppEngine.End();
                    //spritebatch.End();

                    for(int i=0;i<4;i++)
                        if(instruments[i])
                            spritebatch.Draw(boardsTarget[i].GetTexture(), new Rectangle(boards[i].xOffset, 0, windowwidth, windowheight), Color.White);
                    if (screen == S_INGAME)
                    {
                        //draw score/stars
                        DrawScoreStars();

                        //draw rock meter
                        DrawRockMeter();

                        //spritebatch.Draw(boards[0].SPMRTex, new Rectangle(0, 0, 256, 128), Color.White);

                        //draw development info
                        {
                            //spritebatch.DrawString(DefaultFont, "" + ((DateTime.Now.Ticks - SongStartTime) / (float)TicksPerSecond), new Vector2(0, 0), Color.Red);
                            //spritebatch.DrawString(DefaultFont, "" + venue.camindex, new Vector2(0,24), Color.Red);
                            //spritebatch.DrawString(DefaultFont, "" + (boards[2].lastPressed & bits[0]) + (boards[2].lastPressed & bits[1]) + (boards[2].lastPressed & bits[2]) + (boards[2].lastPressed & bits[3]) + (boards[2].lastPressed & bits[4]), new Vector2(0, 48), Color.Red);
                            //spritebatch.DrawString(DefaultFont, "" + boards[2].multiplier, new Vector2(0, 48), Color.Red);
                            //spritebatch.DrawString(DefaultFont, "" + controllers[contInput[0]].ThumbSticks.Right.Y, new Vector2(0, 48), Color.Red);
                        }
                    }

                    spritebatch.End();
                }
                #endregion
                #region results
                if (screen == S_RESULTS)
                {
                    graphics.GraphicsDevice.Clear(Color.Black);
                    spritebatch.Begin();
                    spritebatch.DrawString(DefaultFont, "Song Passed", new Vector2(10, 10), Color.Green);
                    spritebatch.End();
                }
                #endregion
            }

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

        private void UpdateGibs(GameTime gameTime)
        {
            for (int i = 0; i < glass.Length; i++)
            {
                if (glass[i].scale > 0)
                {
                    glass[i].scale -= (float)gameTime.ElapsedGameTime.Milliseconds / 2000f;
                    glass[i].dir.Y -= (float)gameTime.ElapsedGameTime.Milliseconds / 1000f;
                    glass[i].loc += glass[i].dir * (float)gameTime.ElapsedGameTime.Milliseconds * 0.001f;
                }
            }
            for (int i = 0; i < sparks.Length; i++)
            {
                sparks[i].scale = Math.Min(sparks[i].dir.Y,1)*4;
                sparks[i].dir.Y -= (float)gameTime.ElapsedGameTime.Milliseconds / 100f;
                if (sparks[i].scale > 0)
                    sparks[i].loc += sparks[i].dir * (float)gameTime.ElapsedGameTime.Milliseconds * 0.001f;
            }
        }

        public float GetRockstarAmount()
        {
            float total=0, count=0;
            for(int i=0;i<4;i++)
                if (boards[i] != null)
                {
                    int adddiff;
                    if (boards[i].GetDifficulty() == D_EASY)
                        adddiff = 1;
                    else if (boards[i].GetDifficulty() == D_MEDIUM)
                        adddiff = 2;
                    else if (boards[i].GetDifficulty() == D_HARD)
                        adddiff = 3;
                    else
                        adddiff = 4;
                    total += boards[i].GetStars() * adddiff;
                    count += adddiff;
                }
            return total / count;
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

        private float GetRockMeterFill()
        {
            int ct=0;
            float add=0;
            for(int i=0; i<4;i++)
                if (instruments[i])
                {
                    if (rockMeterLevel[i] > 100)
                        rockMeterLevel[i] = 100;
                    if(rockMeterLevel[i]<1)
                        rockMeterLevel[i]=1;
                    //if (TEST_SONG)
                    //    rockMeterLevel[i] = 99f;
                    ct++;
                    add += rockMeterLevel[i];
                }
            return (add / ct) / 100f;
        }

        public void InitForSong(bool guitarist, bool vocalist, bool percussionist, bool bassist, byte[] difficulty, String venue)
        {
            rockstarLoc = new Vector2(graphics.PreferredBackBufferWidth* 0.8f, graphics.PreferredBackBufferHeight * ( (vocalist) ? 0.25f : 0.1f));
            rockstarScale = new Vector2(graphics.PreferredBackBufferWidth * 0.2f, graphics.PreferredBackBufferHeight * 0.1f);
            rockMeterLoc = new Vector2(graphics.PreferredBackBufferWidth * 0.02f, graphics.PreferredBackBufferHeight * 0.20f);
            rockMeterScale = new Vector2(graphics.PreferredBackBufferWidth * 0.02f, graphics.PreferredBackBufferHeight * 0.5f);
            rockstarDir = 0;
            lastStar = 1;
            instruments[0] = guitarist;
            instruments[3] = bassist;
            instruments[2] = percussionist;
            instruments[1] = vocalist;
            rockMeterLevel = new float[4];
            rockMeterLevel[0] = 80;
            rockMeterLevel[1] = 80;
            rockMeterLevel[2] = 80;
            rockMeterLevel[3] = 80;
            boards = new Board[4];
            boardsTarget = new RenderTarget2D[4];
            if (guitarist && bassist && percussionist && vocalist)
            {
            }
            else if (guitarist && bassist && percussionist && !vocalist)
            {
                song = new Song(4, 2, songname,this.Window.Handle);
                boards[0] = new Board(GUITAR, 0, song, difficulty[0]);
                boards[0].xOffset = -250;
                boards[2] = new Board(DRUMS, 0, song, difficulty[2]);
                boards[2].xOffset = 0;
                boards[3] = new Board(BASS, 0, song, difficulty[3]);
                boards[3].xOffset = 250;
                Board.height = -1.5f;
                Board.length = 2.0f;
                Board.width = 0.3f;
                Board.rotate = .4f;
                Board.zeroZ = 2.3f;
                Board.sFade = 1.8f;
                Board.eFade = 2.3f;
                Board.spShift = -0.01f;
                boardsTarget[0] = new RenderTarget2D(graphics.GraphicsDevice, windowwidth, windowheight, 1, SurfaceFormat.Color);
                boardsTarget[2] = new RenderTarget2D(graphics.GraphicsDevice, windowwidth, windowheight, 1, SurfaceFormat.Color);
                boardsTarget[3] = new RenderTarget2D(graphics.GraphicsDevice, windowwidth, windowheight, 1, SurfaceFormat.Color);
            }
            else if (guitarist && bassist && !percussionist && vocalist)
            {
            }
            else if (guitarist && !bassist && percussionist && vocalist)
            {
            }
            else if (!guitarist && bassist && percussionist && vocalist)
            {
            }
            else if (guitarist && bassist && !percussionist && !vocalist)
            {
                song = new Song(4, 2, songname,this.Window.Handle);
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
                boardsTarget[0] = new RenderTarget2D(graphics.GraphicsDevice, windowwidth, windowheight, 1, SurfaceFormat.Color);
                boardsTarget[3] = new RenderTarget2D(graphics.GraphicsDevice, windowwidth, windowheight, 1, SurfaceFormat.Color);
            }
            else if (!guitarist && !bassist && percussionist && vocalist)
            {
            }
            else if (guitarist && !bassist && !percussionist && vocalist)
            {
            }
            else if (!guitarist && bassist && percussionist && !vocalist)
            {
                song = new Song(4, 2, songname,this.Window.Handle);
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
                boardsTarget[3] = new RenderTarget2D(graphics.GraphicsDevice, windowwidth, windowheight, 1, SurfaceFormat.Color);
                boardsTarget[2] = new RenderTarget2D(graphics.GraphicsDevice, windowwidth, windowheight, 1, SurfaceFormat.Color);
            }
            else if (guitarist && !bassist && percussionist && !vocalist)
            {
                song = new Song(4, 2, songname,this.Window.Handle);
                boards[0] = new Board(GUITAR, 0, song, difficulty[0]);
                boards[0].xOffset = -175;
                boards[0].yRotate = -0.15f;
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
                boardsTarget[0] = new RenderTarget2D(graphics.GraphicsDevice, windowwidth, windowheight, 1, SurfaceFormat.Color);
                boardsTarget[2] = new RenderTarget2D(graphics.GraphicsDevice, windowwidth, windowheight, 1, SurfaceFormat.Color);
            }
            else if (!guitarist && bassist && !percussionist && vocalist)
            {
            }
            else if (guitarist && !bassist && !percussionist && !vocalist)
            {
                song = new Song(4, 2, songname,this.Window.Handle);
                boards[0] = new Board(GUITAR, 0, song, difficulty[0]);
                Board.curveHeight = 0.03f;
                Board.height = -2.0f;
                Board.length = 3f;
                Board.width = 0.7f;
                Board.rotate = .5f;
                Board.zeroZ = 2.8f;
                Board.sFade = 0.8f;
                Board.eFade = 1.2f;
                boardsTarget[0] = new RenderTarget2D(graphics.GraphicsDevice, windowwidth, windowheight, 1, SurfaceFormat.Color);
            }
            else if (!guitarist && bassist && !percussionist && !vocalist)
            {
                song = new Song(4, 2, songname,this.Window.Handle);
                boards[3] = new Board(BASS, 0, song, difficulty[3]);
                Board.curveHeight = 0.03f;
                Board.height = -2.0f;
                Board.length = 3f;
                Board.width = 0.7f;
                Board.rotate = .5f;
                Board.zeroZ = 2.8f;
                Board.sFade = 0.8f;
                Board.eFade = 1.2f;
                boardsTarget[3] = new RenderTarget2D(graphics.GraphicsDevice, windowwidth, windowheight, 1, SurfaceFormat.Color);
            }
            else if (!guitarist && !bassist && percussionist && !vocalist)
            {
                song = new Song(4, 2, songname,this.Window.Handle);
                boards[2] = new Board(DRUMS, 0, song, difficulty[2]);
                Board.curveHeight = 0.03f;
                Board.height = -1.6f;
                Board.length = 3f;
                Board.width = 0.6f;
                Board.rotate = .3f;
                Board.zeroZ = 2.8f;
                Board.sFade = 0.8f;
                Board.eFade = 1.2f;
                boardsTarget[2] = new RenderTarget2D(graphics.GraphicsDevice, windowwidth, windowheight, 1, SurfaceFormat.Color);
            }
            else if (!guitarist && !bassist && !percussionist && vocalist)
            {
            }

            this.venue = new Venue(venue + ".gbw", songname, this, content, graphics, engine);
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

        public void Hurt(int ind)
        {
            if (rockMeterLevel[ind] > 80)
                rockMeterLevel[ind] -= 1f;
            else if (rockMeterLevel[ind] > 20)
                rockMeterLevel[ind] -= 0.75f;
            else
                rockMeterLevel[ind] -= 0.5f;
        }

        public void Help(int ind)
        {
            if (rockMeterLevel[ind] > 80)
                rockMeterLevel[ind] += .5f * (boards[ind].IsSPActivated()?10:1);
            else
                rockMeterLevel[ind] += 1.5f * (boards[ind].IsSPActivated() ? 10 : 1);
        }

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

            for (int i = 0; i < glass.Length; i++)
            {
                if (noteage >= 4 && boards[board].GetBoardType() == PERCUSSIONIST)
                    break;
                if (glass[i].scale <= 0)
                {
                    glass[i].scale = 0.5f;
                    glass[i].dir = new Vector3(((float)(r.NextDouble()) * 2) - 1, ((float)(r.NextDouble()) * 1.5f) - 1, (float)(r.NextDouble() * 15))*0.2f;
                    glass[i].rot = new Vector3((float)(r.NextDouble() * Math.PI * 2), (float)(r.NextDouble() * Math.PI * 2), (float)(r.NextDouble() * Math.PI * 2));
                    glass[i].col = noteage;
                    glass[i].frame = r.Next(5);
                    Vector3 rval = Vector3.Transform(new Vector3(0, 0, -Board.zeroZ), Matrix.CreateRotationX(Board.rotate));
                    if (boards[board].GetBoardType() != PERCUSSIONIST)
                        glass[i].loc.X = (-.8f + (noteage * 0.4f)) * Board.width * lefty - boards[board].GetXOffset();
                    else
                        glass[i].loc.X = (-.75f + (Board.drumsToGuitar[noteage] * 0.5f)) * Board.width - boards[board].GetXOffset();
                    glass[i].loc.Y = Board.height + rval.Y;
                    glass[i].loc.Z = rval.Z;
                    glass[i].loc.X += (float)(r.NextDouble() - 0.5) * (Board.width / (boards[board].GetBoardType() == PERCUSSIONIST ? 4 : 5));
                    glass[i].loc.Y += (float)(r.NextDouble() - 0.5)*0.3f;
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
            int lefty = 1;
            if (boards[board].IsLefty())
                lefty = -1;
            int count = 0;
            int noteage = 0;
            for (noteage = 0; noteage < 5; noteage++)
                if ((note & bits[noteage]) > 0)
                    break;
            Random r = new Random((int)(DateTime.Now.Ticks / 1000));

            for (int i = 0; i < sparks.Length; i++)
            {
                if (noteage >= 4 && boards[board].GetBoardType() == PERCUSSIONIST)
                    break;
                if (sparks[i].scale <= 0)
                {
                    sparks[i].scale = 0.25f+(float)r.NextDouble();
                    sparks[i].dir = new Vector3(((float)(r.NextDouble()) * 2) - 1, ((float)(r.NextDouble()) * 10f)+15f, (float)(r.NextDouble() * 15)) * 0.2f;
                    sparks[i].col = noteage;
                    Vector3 rval = Vector3.Transform(new Vector3(0, 0, -Board.zeroZ), Matrix.CreateRotationX(Board.rotate));
                    if (boards[board].GetBoardType() != PERCUSSIONIST)
                        sparks[i].loc.X = (-.8f + (noteage * 0.4f)) * Board.width * lefty - boards[board].GetXOffset();
                    else
                        sparks[i].loc.X = (-.75f + (Board.drumsToGuitar[noteage] * 0.5f)) * Board.width - boards[board].GetXOffset();
                    sparks[i].loc.Y = Board.height + rval.Y;
                    sparks[i].loc.Z = rval.Z;
                    sparks[i].loc.X += (float)(r.NextDouble() - 0.5) * (Board.width / (boards[board].GetBoardType() == PERCUSSIONIST ? 4 : 5));
                    sparks[i].loc.Y += (float)(r.NextDouble() - 0.5) * 0.3f;
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
            int lefty = 1;
            if (boards[board].IsLefty())
                lefty = -1;
            int count = 0;
            int noteage = 0;
            for (noteage = 0; noteage < 5; noteage++)
                if ((note & bits[noteage]) > 0)
                    break;
            Random r = new Random((int)(DateTime.Now.Ticks / 1000));

            for (int i = 0; i < sparks.Length; i++)
            {
                if (noteage >= 4 && boards[board].GetBoardType() == PERCUSSIONIST)
                    break;
                if (sparks[i].scale <= 0)
                {
                    sparks[i].scale = 0.25f+(float)r.NextDouble();
                    sparks[i].dir = new Vector3(((float)(r.NextDouble()) * 4) - 2, ((float)(r.NextDouble()) * 15f)+5f, (float)(r.NextDouble()*4)) * 0.2f;
                    sparks[i].col = noteage;
                    Vector3 rval = Vector3.Transform(new Vector3(0, 0, -Board.zeroZ), Matrix.CreateRotationX(Board.rotate));
                    if (boards[board].GetBoardType() != PERCUSSIONIST)
                        sparks[i].loc.X = (-.8f + (noteage * 0.4f)) * Board.width * lefty - boards[board].GetXOffset();
                    else
                        sparks[i].loc.X = (-.75f + (Board.drumsToGuitar[noteage] * 0.5f)) * Board.width - boards[board].GetXOffset();
                    sparks[i].loc.Y = Board.height + rval.Y;
                    sparks[i].loc.Z = rval.Z;
                    sparks[i].loc.X += (float)(r.NextDouble() - 0.5) * (Board.width / (boards[board].GetBoardType() == PERCUSSIONIST ? 4 : 5));
                    sparks[i].loc.Y += (float)(r.NextDouble() - 0.5) * 0.3f;
                    count++;
                    if (count >= 1)
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

        private void ProcessInput(GameTime gameTime)
        {
            for (int i = 0; i < 4; i++)
                if (boards[i] != null)
                {
                    if (i == 0 || i==3)
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
                        boards[i].Update(gameTime,(long)(DateTime.Now.Ticks - SongStartTime), this, i, pressed);
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
                        if (pressed != 0)
                        {
                            byte e = boards[2].Bang(pressed, (long)(DateTime.Now.Ticks - SongStartTime), this);
                            if (e > 0)
                            {
                                //rockMeterLevel[2] += 0.5f * (boards[2].IsSPActivated()?5:1);
                                if ((e & bits[7]) != 0)
                                    AddSparks(e, 2);
                                else
                                    AddShards(e, 2);
                            }
                        }
                        boards[i].Update(gameTime, (long)(DateTime.Now.Ticks - SongStartTime), this, i, pressed);
                    }
                    else
                        boards[i].Update(gameTime,(long)(DateTime.Now.Ticks - SongStartTime), this, i, 0);
                }

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

                        byte e = boards[0].Strum(pressed, (long)(DateTime.Now.Ticks - SongStartTime),this,0);
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
                        byte e = boards[0].Strum(pressed, (long)(DateTime.Now.Ticks - SongStartTime),this,0);
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

                        byte e = boards[0].Strum(pressed, (long)(DateTime.Now.Ticks - SongStartTime),this,0);
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
                        byte e = boards[0].Strum(pressed, (long)(DateTime.Now.Ticks - SongStartTime),this,0);
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

                        byte e = boards[3].Strum(pressed, (long)(DateTime.Now.Ticks - SongStartTime),this,3);
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
                        byte e = boards[3].Strum(pressed, (long)(DateTime.Now.Ticks - SongStartTime),this,3);
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

                        byte e = boards[3].Strum(pressed, (long)(DateTime.Now.Ticks - SongStartTime),this,3);
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
                        byte e = boards[3].Strum(pressed, (long)(DateTime.Now.Ticks - SongStartTime),this,3);
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
            }
        }

        private void DrawBoard(int i, Vector2[] lenvals, Matrix fling, bool SP)
        {
            Matrix matTransl, matScale, matOrbit;
            VertexBuffer vb;
            //draw pre-song board
            if (lenvals[0].X > -Board.zeroZ)
            {
                matTransl = Matrix.CreateTranslation(0f, Board.height+(boards[i].GetBoardBump()*Board.BOARD_BUMP_COEF), 0f);
                matOrbit = Matrix.CreateTranslation(0f, 0f, -(Board.length * Math.Min(lenvals[0].X, Board.sFade)) - Board.zeroZ) * fling;
                matScale = Matrix.CreateScale(new Vector3(Board.width, Board.curveHeight, Board.length * ((SongStartTime / (float)TicksPerSecond) - Math.Min(lenvals[0].X, Board.sFade))));

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
                    matTransl = Matrix.CreateTranslation(0f, Board.height + (boards[i].GetBoardBump() * Board.BOARD_BUMP_COEF), 0f);
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
                matTransl = Matrix.CreateTranslation(0f, Board.height + (boards[i].GetBoardBump() * Board.BOARD_BUMP_COEF), 0f);
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
                matTransl = Matrix.CreateTranslation(0f, Board.height + (boards[i].GetBoardBump() * Board.BOARD_BUMP_COEF), 0f);
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
                    if(!SP)
                        engine.Parameters["diffuseTexture"].SetValue(Board.boardTexPlain[(boards[i].GetBoardType() == GUITAR || boards[i].GetBoardType() == BASS) ? 1 : 0][0]);
                    else
                        engine.Parameters["diffuseTexture"].SetValue(Board.SPBoardTex);
                }
                else
                {
                    if(!SP)
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
            }
        }

        private void DrawWaves(int i, Matrix fling)
        {
            bool hide = false;
            engine.Parameters["SpecularEnabled"].SetValue(false);
            engine.Parameters["fullbright"].SetValue(true);
            engine.Parameters["BumpMappingEnabled"].SetValue(false);
            VertexBuffer vb;
            int lefty = 1;
            if (boards[i].IsLefty())
                lefty = -1;
            GBVertexFormat[] tmpMdl = new GBVertexFormat[6];
            if (!boards[i].getWaves((long)(DateTime.Now.Ticks - SongStartTime), 3 * (long)TicksPerSecond))
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
            engine.Parameters["diffuseColor"].SetValue(new Vector4(0.8f, 0.8f, 0.8f, 1));
        }

        private void DrawNotes(int i, Matrix fling)
        {
            engine.Parameters["fullbright"].SetValue(true);
            engine.Parameters["vertexAlpha"].SetValue(false);
            int lefty = 1;
            if (boards[i].IsLefty() && boards[i].GetBoardType()!=PERCUSSIONIST)
                lefty = -1;
            Vector4[] notespos = boards[i].GetNotes(DateTime.Now.Ticks - SongStartTime, (long)(Board.eFade * TicksPerSecond));
            for (int p = 0; p < notespos.Length; p++)
            {
                if (boards[i].GetBoardType() == PERCUSSIONIST && notespos[p].Z > 1.5)
                {

                }
                else if (boards[i].GetBoardType() == PERCUSSIONIST && notespos[p].Z > 0.5)
                {
                    engine.Parameters["specularColor"].SetValue(new Vector4(0, 0, 0, 0));
                    engine.Parameters["SpecularEnabled"].SetValue(false);
                    engine.Parameters["BumpMappingEnabled"].SetValue(false);
                    engine.Parameters["fullbright"].SetValue(true);
                    Matrix matIdentity, matTransl, matRot, matScale, matOrbit;
                    for (int k = 0; k < 4; k++)
                    {
                        float height=0;
                        matRot = Matrix.Identity;
                        if (k == 0 || k == 3)
                        { matRot = Matrix.CreateRotationZ(0.07f * -Math.Sign(k - 2)); height = 0.01f; }
                        if (k == 1 || k == 2)
                        { matRot = Matrix.CreateRotationZ(0.03f * -Math.Sign(k - 2)); height = 0.03f; }
                        matIdentity = Matrix.Identity;
                        matTransl = Matrix.CreateTranslation(0f, Board.height + (boards[i].GetBoardBump() * Board.BOARD_BUMP_COEF), 0f);
                        matOrbit = Matrix.CreateTranslation(((k / 2f) - .75f) * Board.width, height, -(Board.length * notespos[p+1].Y) - Board.zeroZ) * fling;
                        matScale = Matrix.CreateScale(new Vector3(Board.width/4f*((notespos[p].X*2+1)/3f), Board.curveHeight*0.1f, Board.length * (notespos[p+1].Y-notespos[p].Y)));

                        engine.Parameters["wAlpha"].SetValue(1);
                        engine.Parameters["world"].SetValue(matIdentity * matScale * matRot * matOrbit * matTransl);
                        engine.Parameters["diffuseTexture"].SetValue(Board.drumfillTex);
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

                    if (notespos[p].W<0.5)
                    {


                        matIdentity = Matrix.Identity;
                        matTransl = Matrix.CreateTranslation(0f, Board.height + (boards[i].GetBoardBump() * Board.BOARD_BUMP_COEF), 0f);
                        matOrbit = Matrix.CreateTranslation(0.75f * Board.width, 0f, -(Board.length * (notespos[p + 1].Y-0.1f)) - Board.zeroZ) * fling;
                        matScale = Matrix.CreateScale(new Vector3(0.15f * Board.width, 0.15f*notespos[p].X, 0.3f));

                        float alpha;
                        if (notespos[p].Y < Board.sFade)
                            alpha = 1;
                        else if (notespos[p].Y < Board.eFade)
                            alpha = 1 - ((notespos[p].Y - Board.sFade) / (Board.eFade - Board.sFade));
                        else
                            alpha = 0;


                        engine.Parameters["wAlpha"].SetValue(alpha);

                        // identity, scale, rotate, orbit(translate & rotate), translate
                        engine.Parameters["world"].SetValue(matIdentity * matScale * matOrbit * matTransl);

                        engine.Parameters["diffuseTexture"].SetValue(Board.texNotes[0]);
                        engine.Parameters["bumpTexture"].SetValue(texDefaultBM);
                        engine.CommitChanges();

                        // 5: draw object - select vertex type, primitive type, # of primitives
                        graphics.GraphicsDevice.VertexDeclaration = vd;
                        graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                        graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                        graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
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
                    engine.Parameters["BumpMappingEnabled"].SetValue(true);
                }
                else
                {
                    Matrix matIdentity, matTransl, matScale, matOrbit;
                    matIdentity = Matrix.Identity;
                    matTransl = Matrix.CreateTranslation(0f, Board.height + (boards[i].GetBoardBump() * Board.BOARD_BUMP_COEF), 0f);
                    matOrbit = Matrix.CreateTranslation(notespos[p].X * lefty * Board.width * 0.8f, 0f, -(Board.length * notespos[p].Y) - Board.zeroZ) * fling;
                    if (boards[i].GetBoardType() == PERCUSSIONIST && Math.Abs(notespos[p].X) < 0.01f)
                        matScale = Matrix.CreateScale(new Vector3(Board.width, Board.curveHeight, Board.length * 0.01f));
                    else
                        matScale = Matrix.CreateScale(new Vector3(((notespos[p].Z > 0)?0.5f:1.0f) * 0.125f * Board.width, 0.10f, 0.0333f*Board.length));

                    float alpha;
                    if (notespos[p].Y < Board.sFade)
                        alpha = 1;
                    else if (notespos[p].Y < Board.eFade)
                        alpha = 1 - ((notespos[p].Y - Board.sFade) / (Board.eFade - Board.sFade));
                    else
                        alpha = 0;


                    engine.Parameters["wAlpha"].SetValue(alpha);

                    // identity, scale, rotate, orbit(translate & rotate), translate
                    engine.Parameters["world"].SetValue(matIdentity * matScale * matOrbit * matTransl);

                    if (boards[i].GetBoardType() != PERCUSSIONIST)
                    {
                        if (Math.Abs(notespos[p].X - (-1)) < 0.01)
                            engine.Parameters["diffuseTexture"].SetValue(Board.texNotes[0]);
                        else if (Math.Abs(notespos[p].X - (-0.5)) < 0.01)
                            engine.Parameters["diffuseTexture"].SetValue(Board.texNotes[1]);
                        else if (Math.Abs(notespos[p].X) < 0.01)
                            engine.Parameters["diffuseTexture"].SetValue(Board.texNotes[2]);
                        else if (Math.Abs(notespos[p].X) - (0.5) < 0.01)
                            engine.Parameters["diffuseTexture"].SetValue(Board.texNotes[3]);
                        else
                            engine.Parameters["diffuseTexture"].SetValue(Board.texNotes[4]);
                    }
                    else
                    {
                        if (Math.Abs(notespos[p].X - (-1)) < 0.01)
                            engine.Parameters["diffuseTexture"].SetValue(Board.texNotes[1]);
                        else if (Math.Abs(notespos[p].X - (-1 / 3f)) < 0.01)
                            engine.Parameters["diffuseTexture"].SetValue(Board.texNotes[2]);
                        else if (Math.Abs(notespos[p].X) < 0.01)
                            engine.Parameters["diffuseTexture"].SetValue(Board.texTriggerBorderLit);
                        else if (Math.Abs(notespos[p].X) - (1 / 3f) < 0.01)
                            engine.Parameters["diffuseTexture"].SetValue(Board.texNotes[3]);
                        else
                            engine.Parameters["diffuseTexture"].SetValue(Board.texNotes[0]);
                    }
                    if (notespos[p].W > 0.5)
                        engine.Parameters["diffuseTexture"].SetValue(texWhite);
                    engine.CommitChanges();

                    // 5: draw object - select vertex type, primitive type, # of primitives
                    graphics.GraphicsDevice.VertexDeclaration = vd;
                    graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                    graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                    graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                    if (boards[i].GetBoardType() == PERCUSSIONIST && Math.Abs(notespos[p].X) < 0.01f)
                    {
                        graphics.GraphicsDevice.Vertices[0].SetSource(Board.mdlTriggerBorder, 0, GBVertexFormat.SizeInBytes);
                        graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, (Board.mdlTriggerBorder.SizeInBytes / GBVertexFormat.SizeInBytes) / 3);
                    }
                    else
                        foreach (ModelMesh mesh in Board.mdlNote.Meshes)
                        {
                            foreach (ModelMeshPart part in mesh.MeshParts)
                            {

                                //engine.Parameters["diffuseTexture"].SetValue(texWhite);
                                engine.Parameters["bumpTexture"].SetValue(texDefaultBM);
                                engine.CommitChanges();
                                graphics.GraphicsDevice.VertexDeclaration = part.VertexDeclaration;
                                graphics.GraphicsDevice.Vertices[0].SetSource(mesh.VertexBuffer, part.StreamOffset, part.VertexStride);
                                graphics.GraphicsDevice.Indices = mesh.IndexBuffer;
                                graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.BaseVertex, 0, part.NumVertices, part.StartIndex, part.PrimitiveCount);
                            }
                        }
                    graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                }
            }
            engine.Parameters["wAlpha"].SetValue(1.0f);
            engine.Parameters["fullbright"].SetValue(false);
            engine.Parameters["vertexAlpha"].SetValue(true);
        }

        private void DrawBoardDetail(int i, Matrix fling)
        {
            engine.Parameters["fullbright"].SetValue(true);
            int lefty = 1;
            if (boards[i].IsLefty())
                lefty = -1;

            if (boards[i].GetBoardType() == GUITAR || boards[i].GetBoardType() == BASS)
            {
                Matrix matIdentity, matTransl, matScale, matOrbit;

                bool[] glow = new bool[5];
                for (int p = 0; p < 5; p++)
                {
                    float rise = -.01f;

                    matIdentity = Matrix.Identity;
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
                for (int p = 0; p < 5; p++)
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
                }
                engine.Parameters["fullbright"].SetValue(false);
                engine.Parameters["wAlpha"].SetValue(1);
            }
            else if (boards[i].GetBoardType() == PERCUSSIONIST)
            {


                bool[] glow = new bool[5];
                float[] pop = boards[i].GetPopups();
                for (int k = 0; k < 5; k++)
                    glow[k] = pop[k] > 0;
                for (int p = 0; p < 5; p++)
                {
                    Matrix matIdentity, matTransl, matScale, matOrbit;
                    float rise = -.01f;
                    if (p < 4)
                    {
                        matIdentity = Matrix.Identity;
                        matTransl = Matrix.CreateTranslation((-.75f + (p * .5f)) * (Board.width), Board.height + (boards[i].GetBoardBump() * Board.BOARD_BUMP_COEF), 0f);
                        matOrbit = Matrix.CreateTranslation(0f, Board.curveHeight * (1 - Math.Abs(-.8f + (p * 0.4f))) + rise + ((boards[i].GetPopups()[p]) * 0.001f), -Board.zeroZ) * fling;
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
                        matTransl = Matrix.CreateTranslation(0f, Board.height + (boards[i].GetBoardBump() * Board.BOARD_BUMP_COEF), 0f);
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
                        matTransl = Matrix.CreateTranslation(0f, Board.height + (boards[i].GetBoardBump() * Board.BOARD_BUMP_COEF), 0f);
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
                int[] gloworder = { 0, 4, 1, 3, 2 };
                engine.Parameters["fullbright"].SetValue(true);
                for (int p = 0; p < 4; p++)
                {
                    if (glow[p])
                    {
                        Matrix matIdentity, matTransl, matScale, matOrbit;

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
            engine.Parameters["fullbright"].SetValue(true);
            {
                Matrix matIdentity, matTransl, matScale, matOrbit;
                matIdentity = Matrix.Identity;
                matTransl = Matrix.CreateTranslation(0f, Board.height + (boards[i].GetBoardBump() * Board.BOARD_BUMP_COEF), 0f);
                matOrbit = Matrix.CreateTranslation(0f, 0.09f, (0.25f) - Board.zeroZ) * fling;
                matScale = Matrix.CreateScale(new Vector3(Board.width*1.1f, Board.curveHeight * 0.8f, Board.length * 0.05f));

                // identity, scale, rotate, orbit(translate & rotate), translate
                engine.Parameters["world"].SetValue(matIdentity * matScale * matOrbit * matTransl);
                engine.Parameters["diffuseTexture"].SetValue(Board.SPMFlashTex);
                engine.Parameters["wAlpha"].SetValue(boards[i].SPMFlash);
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
            engine.Parameters["wAlpha"].SetValue(1.0f);
            {
                Matrix matIdentity, matTransl, matScale, matOrbit;
                matIdentity = Matrix.Identity;
                matTransl = Matrix.CreateTranslation(0f, Board.height + (boards[i].GetBoardBump() * Board.BOARD_BUMP_COEF), 0f);
                matOrbit = Matrix.CreateTranslation(0f, 0.1f, (0.2f) - Board.zeroZ) * fling;
                matScale = Matrix.CreateScale(new Vector3(Board.width*0.9f, Board.curveHeight*0.8f, Board.length * 0.02f));

                // identity, scale, rotate, orbit(translate & rotate), translate
                engine.Parameters["world"].SetValue(matIdentity * matScale * matOrbit * matTransl);
                engine.Parameters["diffuseTexture"].SetValue(boards[i].SPMeterTex);
                engine.Parameters["diffuseColor"].SetValue(new Vector4(0.8f, 0.8f, 0.8f, 1.0f));
                engine.CommitChanges();

                // 5: draw object - select vertex type, primitive type, # of primitives
                graphics.GraphicsDevice.VertexDeclaration = vd;
                graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                graphics.GraphicsDevice.Vertices[0].SetSource(Board.mdlSPM, 0, GBVertexFormat.SizeInBytes);
                graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, (Board.mdlSPM.SizeInBytes / GBVertexFormat.SizeInBytes) / 3);
                graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
            }
            {
                Matrix matIdentity, matTransl, matScale, matOrbit;
                matIdentity = Matrix.Identity;
                matTransl = Matrix.CreateTranslation(0f, Board.height + (boards[i].GetBoardBump() * Board.BOARD_BUMP_COEF), 0f);
                matOrbit = Matrix.CreateTranslation(0f, 0.110f, (0.265f) - Board.zeroZ - Board.spShift) * fling;
                matScale = Matrix.CreateScale(new Vector3(Board.width * 0.325f, 0.02f, Board.length * 0.2f));

                // identity, scale, rotate, orbit(translate & rotate), translate
                engine.Parameters["world"].SetValue(matIdentity * matScale * matOrbit * matTransl);
                engine.Parameters["diffuseTexture"].SetValue(boards[i].SPMRTex);
                engine.Parameters["diffuseColor"].SetValue(new Vector4(0.8f, 0.8f, 0.8f, 1.0f));
                engine.CommitChanges();

                // 5: draw object - select vertex type, primitive type, # of primitives
                graphics.GraphicsDevice.VertexDeclaration = vd;
                graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                graphics.GraphicsDevice.Vertices[0].SetSource(Board.mdlBoard, 0, GBVertexFormat.SizeInBytes);
                graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, (Board.mdlSPM.SizeInBytes / GBVertexFormat.SizeInBytes) / 3);
                graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
            }
            engine.Parameters["fullbright"].SetValue(false);
        }

        private void DrawRockMeter()
        {
            float rex;
            if (((DateTime.Now.Ticks - SongStartTime) / (float)TicksPerSecond) > -2.5)
                rex = rockMeterLoc.X;
            else if (((DateTime.Now.Ticks - SongStartTime) / (float)TicksPerSecond) > -3)
                rex = -(rockMeterScale.X * 3) + (((((DateTime.Now.Ticks - SongStartTime) / (float)TicksPerSecond) + 3) * 2) * ((rockMeterLoc.X * 3) + rockMeterScale.X));
            else
                rex = -rockMeterScale.X * 3;
            float rmFill = GetRockMeterFill();
            if (rmFill <= 0.33)
                spritebatch.Draw(texWhite, new Rectangle((int)((rockMeterScale.X * 0.2f) + rex), (int)((rockMeterScale.Y * 0.025f) + rockMeterLoc.Y + (rockMeterScale.Y * (1 - rmFill) * 0.95f)), (int)(rockMeterScale.X * 0.65f), (int)(rockMeterScale.Y * rmFill * 0.95f)), new Color(new Vector4(1f, 0, 0f, 0.8f)));
            else if(rmFill<=0.67)
                spritebatch.Draw(texWhite, new Rectangle((int)((rockMeterScale.X * 0.2f) + rex), (int)((rockMeterScale.Y * 0.025f) + rockMeterLoc.Y + (rockMeterScale.Y * (1 - rmFill) * 0.95f)), (int)(rockMeterScale.X * 0.65f), (int)(rockMeterScale.Y * rmFill * 0.95f)), new Color(new Vector4(1f, 1f, 0f, 0.8f)));
            else
                spritebatch.Draw(texWhite, new Rectangle((int)((rockMeterScale.X * 0.2f) + rex), (int)((rockMeterScale.Y * 0.025f) + rockMeterLoc.Y + (rockMeterScale.Y * (1 - rmFill) * 0.95f)), (int)(rockMeterScale.X * 0.65f), (int)(rockMeterScale.Y * rmFill * 0.95f)), new Color(new Vector4(0f, 1f, 0f, 0.8f)));
            spritebatch.Draw(texRockMeterOutline, new Rectangle((int)rex, (int)rockMeterLoc.Y, (int)rockMeterScale.X, (int)rockMeterScale.Y), Color.White);

            byte[] logoslots = new byte[11];
            for (int i = 0; i < 10; i++)
                logoslots[i] = (byte)0;
            for (int i = 0; i < 4; i++)
                if (instruments[i])
                    logoslots[(int)Math.Min(Math.Round(rockMeterLevel[i] / 10f), 10)] |= bits[i];

            for (int i = 0; i < 11; i++)
            {
                int numhere = 0;
                for (int k = 0; k < 4; k++)
                    if ((logoslots[i] & bits[k]) != 0)
                        numhere++;
                int logoscale = 4;
                if (numhere > 0)
                {
                    spritebatch.Draw(texRockMeterLogoStem, new Vector2(rex + (rockMeterScale.X / 2), rockMeterLoc.Y + (rockMeterScale.Y * ((10 - i) / 10f) * 0.92f) + (rockMeterScale.Y * 0.04f)), new Rectangle(0, 0, 256, 256), Color.White, 0, new Vector2(0, 128), rockMeterScale.X / 800f * 3, new SpriteEffects(), 0);
                    int tnum = numhere;
                    for (int k = 3; k >= 0; k--)
                        if ((logoslots[i] & bits[k]) != 0)
                        {
                            switch (k)
                            {
                                case 0:
                                    spritebatch.Draw(texRockMeterGuitarLogo, new Vector2(rex + (rockMeterScale.X / 2) + (tnum * (210 * (rockMeterScale.X / 800f * logoscale))) - (128 * (rockMeterScale.X / 800f * logoscale)), rockMeterLoc.Y + (rockMeterScale.Y * ((10 - i) / 10f) * 0.92f) + (rockMeterScale.Y * 0.04f)), new Rectangle(0, 0, 256, 256), Color.White, 0, new Vector2(0, 128), rockMeterScale.X / 800f * logoscale, new SpriteEffects(), 0);
                                    break;
                                case 1:
                                    spritebatch.Draw(texRockMeterBassLogo, new Vector2(rex + (rockMeterScale.X / 2) + (tnum * (210 * (rockMeterScale.X / 800f * logoscale))) - (128 * (rockMeterScale.X / 800f * logoscale)), rockMeterLoc.Y + (rockMeterScale.Y * ((10 - i) / 10f) * 0.92f) + (rockMeterScale.Y * 0.04f)), new Rectangle(0, 0, 256, 256), Color.White, 0, new Vector2(0, 128), rockMeterScale.X / 800f * logoscale, new SpriteEffects(), 0);
                                    break;
                                case 2:
                                    spritebatch.Draw(texRockMeterDrumLogo, new Vector2(rex + (rockMeterScale.X / 2) + (tnum * (210 * (rockMeterScale.X / 800f * logoscale))) - (128 * (rockMeterScale.X / 800f * logoscale)), rockMeterLoc.Y + (rockMeterScale.Y * ((10 - i) / 10f) * 0.92f) + (rockMeterScale.Y * 0.04f)), new Rectangle(0, 0, 256, 256), Color.White, 0, new Vector2(0, 128), rockMeterScale.X / 800f * logoscale, new SpriteEffects(), 0);
                                    break;
                                case 3:
                                    spritebatch.Draw(texRockMeterSingerLogo, new Vector2(rex + (rockMeterScale.X / 2) + (tnum * (210 * (rockMeterScale.X / 800f * logoscale))) - (128 * (rockMeterScale.X / 800f * logoscale)), rockMeterLoc.Y + (rockMeterScale.Y * ((10 - i) / 10f) * 0.92f) + (rockMeterScale.Y * 0.04f)), new Rectangle(0, 0, 256, 256), Color.White, 0, new Vector2(0, 128), rockMeterScale.X / 800f * logoscale, new SpriteEffects(), 0);
                                    break;
                            }
                            tnum--;
                        }
                }
            }
        }

        private void DrawScoreStars()
        {
            float xers;
            if (((DateTime.Now.Ticks - SongStartTime) / (float)TicksPerSecond) > -2.5)
                xers = rockstarLoc.X;
            else if (((DateTime.Now.Ticks - SongStartTime) / (float)TicksPerSecond) > -3)
                xers = (Window.ClientBounds.Width - (((((DateTime.Now.Ticks - SongStartTime) / (float)TicksPerSecond) + 3f) * 2) * (Window.ClientBounds.Width - rockstarLoc.X)));
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
            String scr = "" + GetScore();
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
            spritebatch.DrawString(DefaultFont, scr2, new Vector2(xers+(rockstarScale.X*0.95f)-DefaultFont.MeasureString(scr2).X, rockstarLoc.Y - (rockstarScale.Y / 2)), Color.White);
        }

        private void DrawGibs()
        {

            for (int i = 0; i < glass.Length; i++)
                if (glass[i].scale > 0)
                {
                    Matrix matIdentity, matTransl, matScale, matOrbit;
                    matIdentity = Matrix.Identity;
                    matTransl = Matrix.CreateTranslation(glass[i].loc);
                    matOrbit = Matrix.CreateRotationX(glass[i].rot.X) * Matrix.CreateRotationY(glass[i].rot.Y) * Matrix.CreateRotationZ(glass[i].rot.Z);
                    matScale = Matrix.CreateScale((new Vector3(0.1f, 0.1f, 0.1f)) * glass[i].scale);

                    // identity, scale, rotate, orbit(translate & rotate), translate
                    engine.Parameters["world"].SetValue(matIdentity * matScale * matOrbit * matTransl);

                    engine.Parameters["proj"].SetValue(matProj);


                    engine.Parameters["diffuseTexture"].SetValue(texShard[glass[i].frame]);
                    engine.Parameters["diffuseColor"].SetValue(FretColorsV4[glass[i].col]);
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
            for (int i = 0; i < sparks.Length; i++)
                if (sparks[i].scale > 0)
                {
                    Matrix matRot, matTransl, matScale;
                    matRot = Matrix.CreateRotationX(MathHelper.PiOver2) * Matrix.CreateRotationY((float)(hvdistTOdir(venue.GetCamFor().X, venue.GetCamFor().Z) / 180 * Math.PI)+MathHelper.PiOver2);
                    matTransl = Matrix.CreateTranslation(sparks[i].loc);
                    matScale = Matrix.CreateScale(new Vector3(0.01f, 0.01f, 0.01f) * sparks[i].scale);

                    // identity, scale, rotate, orbit(translate & rotate), translate
                    engine.Parameters["world"].SetValue(matScale * matRot * matTransl);

                    engine.Parameters["diffuseTexture"].SetValue(texSpark);
                    engine.Parameters["diffuseColor"].SetValue(FretColorsV4[sparks[i].col]);
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
        }
    }
}

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
        public bool White;
    }

    public struct SongListEntry
    {
        public String displayName, fileName, artistName, len;
        public SongListEntry(String display, String file, String artist, String length)
        {
            displayName = display;
            fileName = file;
            artistName = artist;
            len = length;
        }
    }

    public class SongSet
    {
        public String name;
        public List<SongListEntry> songs;
        public SongSet(String name)
        {
            this.name = name;
            songs = new List<SongListEntry>();
        }
    }

    public class SetList
    {
        public List<SongSet> setlist;
        public String name;

        public void LoadCustom(String filename)
        {
            setlist = new List<SongSet>();
            System.IO.StreamReader reader = new System.IO.StreamReader(filename);
            String z;
            SongSet CurrentSet = null;
            while (!reader.EndOfStream)
            {
                z = reader.ReadLine();
                if (z.Length >= 3 && z.Substring(0, 3).ToLower().Equals("set"))
                {
                    if (CurrentSet != null && CurrentSet.songs.Count > 0 && !CurrentSet.name.Equals(""))
                        setlist.Add(CurrentSet);
                    String name = z.Substring(z.IndexOf("(")+1);
                    name = name.Substring(0, name.LastIndexOf(')'));
                    CurrentSet = new SongSet(name);
                }
                else
                {
                    String songfilename = "songdata\\" + z;
                    if (!System.IO.File.Exists(songfilename + ".uns"))
                    {
                        if (!System.IO.File.Exists(songfilename + ".gba"))
                            CurrentSet.songs.Add(new SongListEntry("DNE-" + z, "DNE", "NULL", "00:00:00")); continue;
                        System.IO.BinaryReader r2 = new System.IO.BinaryReader(System.IO.File.OpenRead(songfilename));
                        byte version = r2.ReadByte();
                        if (version != 12)
                        { CurrentSet.songs.Add(new SongListEntry("INV-" + z, "DNE", "UNKNOWN", "00:00:00")); continue; }
                        String name = r2.ReadString();
                        String artist = r2.ReadString();
                        r2.ReadUInt32();
                        r2.ReadString();
                        String length = r2.ReadString();
                        CurrentSet.songs.Add(new SongListEntry(name, z, artist, length));
                        r2.Close();
                    }
                    else
                    {
                        System.IO.BinaryReader r2 = new System.IO.BinaryReader(System.IO.File.OpenRead(songfilename + ".uns"));
                        byte version = r2.ReadByte();
                        if (version != Game1.SONGDATA_VERSION)
                        { CurrentSet.songs.Add(new SongListEntry("INV-" + z, "DNE", "UNKNOWN", "00:00:00")); continue; }
                        String name = r2.ReadString();
                        String artist = r2.ReadString();
                        r2.ReadUInt32();
                        r2.ReadString();
                        String length = r2.ReadString();
                        CurrentSet.songs.Add(new SongListEntry(name, z, artist, length));
                        r2.Close();
                    }
                }
            }
            if (CurrentSet != null && CurrentSet.songs.Count > 0 && !CurrentSet.name.Equals(""))
                setlist.Add(CurrentSet);
            reader.Close();
        }

        public void LoadByDecade()
        {
            setlist = new List<SongSet>();
            SongSet[] sets = new SongSet[20];
            for (int i = 0; i < 20; i++)
                sets[i] = new SongSet((i<10?"19":"20") + (i%10) + "0s");

            String[] files = System.IO.Directory.GetFiles("songdata\\");
            for(int i=0;i<files.Length;i++)
                if (files[i].Length > 3 && files[i].Substring(files[i].Length - 3).ToLower().Equals("uns"))
                {
                    String songname2 = files[i].Substring(files[i].LastIndexOf('\\') + 1);
                    songname2 = songname2.Substring(0, songname2.LastIndexOf('.'));
                    System.IO.BinaryReader bin = new System.IO.BinaryReader(System.IO.File.OpenRead(files[i]));
                    bin.ReadBytes(3);//UNS
                    bin.ReadBytes(4 * 6);//lengths
                    byte ver = bin.ReadByte();
                    bool inv = false;
                    if (ver != Game1.SONGDATA_VERSION)
                    { inv = true; }
                    String name = bin.ReadString();
                    String arts = bin.ReadString();
                    int yr = bin.ReadInt32();
                    if (yr < 0)
                        { bin.Close(); continue; }
                    else
                    {
                        yr -= 1900;
                        yr /= 10;
                    }
                    bin.ReadString();
                    String len = bin.ReadString();
                    sets[yr].songs.Add(new SongListEntry((inv ? "INV-" : "") + name, inv ? "DNE" : songname2,arts,len));
                    bin.Close();
                }
            for (int i = 0; i < files.Length; i++)
                if (files[i].Length > 3 && files[i].Substring(files[i].Length - 3).ToLower().Equals("gba"))
                {
                    System.IO.BinaryReader bin = new System.IO.BinaryReader(System.IO.File.OpenRead(files[i]));
                    String songname2 = files[i].Substring(files[i].LastIndexOf('\\') + 1);
                    songname2 = songname2.Substring(0, songname2.LastIndexOf('.'));
                    byte ver = bin.ReadByte();
                    bool inv = false;
                    if (ver != 12)
                    { inv = true; }
                    String name = bin.ReadString();
                    String arts = bin.ReadString();
                    int yr = bin.ReadInt32();
                    if (yr < 0)
                    { bin.Close(); continue; }
                    else
                    {
                        yr -= 1900;
                        yr /= 10;
                    }
                    bin.ReadString();
                    String len = bin.ReadString();
                    bool contains = false;
                    for (int r = 0; r < sets[yr].songs.Count; r++)
                        if (sets[yr].songs[r].fileName.ToLower().Equals(songname2.ToLower()))
                            contains = true;
                    if(contains==false)
                        sets[yr].songs.Add(new SongListEntry((inv ? "INV-" : "") + name, inv ? "DNE" : songname2, arts, len));
                    bin.Close();
                }
            for (int i = 0; i < 20; i++)
                if (sets[i].songs.Count > 0)
                    setlist.Add(sets[i]);

        }
        public void LoadByGenre()
        {
            setlist = new List<SongSet>();

            String[] files = System.IO.Directory.GetFiles("songdata\\");
            for(int i=0;i<files.Length;i++)
                if (files[i].Length > 3 && files[i].Substring(files[i].Length - 3).ToLower().Equals("uns"))
                {
                    System.IO.BinaryReader bin = new System.IO.BinaryReader(System.IO.File.OpenRead(files[i]));
                    String songname2 = files[i].Substring(files[i].LastIndexOf('\\') + 1);
                    songname2 = songname2.Substring(0, songname2.LastIndexOf('.'));
                    bin.ReadBytes(3);//UNS
                    bin.ReadBytes(4 * 6);//lengths
                    byte ver = bin.ReadByte();
                    bool inv = false;
                    if (ver != Game1.SONGDATA_VERSION)
                    { bin.Close(); continue; }
                    String name = bin.ReadString();
                    String arts = bin.ReadString();
                    int yr = bin.ReadInt32();
                    String genre = bin.ReadString();
                    String len = bin.ReadString();
                    bool went = false;
                    for (int k = 0; k < setlist.Count; k++)
                        if (setlist[k].name.ToLower().Equals(genre.ToLower().Trim()))
                        {
                            setlist[k].songs.Add(new SongListEntry((inv ? "INV-" : "") + name, inv ? "DNE" : songname2, arts, len));
                            went = true;
                        }
                    if (!went)
                    {
                        SongSet s = new SongSet(genre);
                        s.songs = new List<SongListEntry>();
                        s.songs.Add(new SongListEntry((inv ? "INV-" : "") + name, inv ? "DNE" : songname2, arts, len));
                        setlist.Add(s);
                    }
                    bin.Close();
                }
            for (int i = 0; i < files.Length; i++)
                if (files[i].Length > 3 && files[i].Substring(files[i].Length - 3).ToLower().Equals("gba"))
                {
                    System.IO.BinaryReader bin = new System.IO.BinaryReader(System.IO.File.OpenRead(files[i]));
                    String songname2 = files[i].Substring(files[i].LastIndexOf('\\') + 1);
                    songname2 = songname2.Substring(0, songname2.LastIndexOf('.'));
                    byte ver = bin.ReadByte();
                    bool inv = false;
                    if (ver != 12)
                    { bin.Close(); continue; }
                    String name = bin.ReadString();
                    String arts = bin.ReadString();
                    int yr = bin.ReadInt32();
                    String genre = bin.ReadString();
                    String len = bin.ReadString();
                    bool went = false;
                    bool contains = false;
                    
                    
                        for (int k = 0; k < setlist.Count; k++)
                            if (setlist[k].name.ToLower().Equals(genre.ToLower().Trim()))
                            {
                                for (int r = 0; r < setlist[k].songs.Count; r++)
                                    if (setlist[k].songs[r].fileName.ToLower().Equals(songname2.ToLower()))
                                        contains = true;
                                if (contains == false)
                                setlist[k].songs.Add(new SongListEntry((inv ? "INV-" : "") + name, inv ? "DNE" : songname2, arts, len));
                                went = true;
                            }
                        if (!went)
                        {
                            SongSet s = new SongSet(genre);
                            s.songs = new List<SongListEntry>();
                            s.songs.Add(new SongListEntry((inv ? "INV-" : "") + name, inv ? "DNE" : songname2, arts, len));
                            setlist.Add(s);
                        }
                    
                    bin.Close();
                }

        }
        public void LoadByName()
        {
            setlist = new List<SongSet>();
            SongSet[] sets = new SongSet[26];
            for (int i = 0; i < 26; i++)
                sets[i] = new SongSet(""+((char)(65+i))+"s");

            String[] files = System.IO.Directory.GetFiles("songdata\\");
            for(int i=0;i<files.Length;i++)
                if (files[i].Length > 3 && files[i].Substring(files[i].Length - 3).ToLower().Equals("uns"))
                {
                    System.IO.BinaryReader bin = new System.IO.BinaryReader(System.IO.File.OpenRead(files[i]));
                    String songname2 = files[i].Substring(files[i].LastIndexOf('\\') + 1);
                    songname2 = songname2.Substring(0, songname2.LastIndexOf('.'));
                    bin.ReadBytes(3);//UNS
                    bin.ReadBytes(4 * 6);//lengths
                    byte ver = bin.ReadByte();
                    bool inv = false;
                    if (ver != Game1.SONGDATA_VERSION)
                    { inv = true; }
                    String name = bin.ReadString();
                    String arts = bin.ReadString();
                    int yr = bin.ReadInt32();
                    bin.ReadString();
                    String len = bin.ReadString();
                        sets[(int)(name.ToUpper().ToCharArray()[0]-65)].songs.Add(new SongListEntry((inv ? "INV-" : "") + name, inv ? "DNE" : songname2,arts,len));
                    bin.Close();
                }
            for (int i = 0; i < files.Length; i++)
                if (files[i].Length > 3 && files[i].Substring(files[i].Length - 3).ToLower().Equals("gba"))
                {
                    System.IO.BinaryReader bin = new System.IO.BinaryReader(System.IO.File.OpenRead(files[i]));
                    String songname2 = files[i].Substring(files[i].LastIndexOf('\\') + 1);
                    songname2 = songname2.Substring(0, songname2.LastIndexOf('.'));
                    byte ver = bin.ReadByte();
                    bool inv = false;
                    if (ver != 12)
                    { inv = true; }
                    String name = bin.ReadString();
                    String arts = bin.ReadString();
                    int yr = bin.ReadInt32();
                    bin.ReadString();
                    String len = bin.ReadString();
                    bool contains = false;
                    for (int r = 0; r < sets[(int)(name.ToUpper().ToCharArray()[0] - 65)].songs.Count; r++)
                        if (sets[(int)(name.ToUpper().ToCharArray()[0] - 65)].songs[r].fileName.ToLower().Equals(songname2.ToLower()))
                            contains = true;
                    if (contains == false)
                        sets[(int)(name.ToUpper().ToCharArray()[0] - 65)].songs.Add(new SongListEntry((inv ? "INV-" : "") + name, inv ? "DNE" : songname2, arts, len));
                    bin.Close();
                }
            for (int i = 0; i < 20; i++)
                if (sets[i].songs.Count > 0)
                    setlist.Add(sets[i]);

        }
        public void LoadByArtist()
        {
            setlist = new List<SongSet>();
            SongSet[] sets = new SongSet[27];
            for (int i = 0; i < 26; i++)
                sets[i] = new SongSet(""+((char)(65+i))+"s");
            sets[26] = new SongSet("?s");

            String[] files = System.IO.Directory.GetFiles("songdata\\");
            for(int i=0;i<files.Length;i++)
                if (files[i].Length > 3 && files[i].Substring(files[i].Length - 3).ToLower().Equals("uns"))
                {
                    System.IO.BinaryReader bin = new System.IO.BinaryReader(System.IO.File.OpenRead(files[i]));
                    String songname2 = files[i].Substring(files[i].LastIndexOf('\\') + 1);
                    songname2 = songname2.Substring(0, songname2.LastIndexOf('.'));
                    bin.ReadBytes(3);//UNS
                    bin.ReadBytes(4 * 6);//lengths
                    byte ver = bin.ReadByte();
                    bool inv = false;
                    if (ver != Game1.SONGDATA_VERSION)
                    { inv = true; }
                    String name = bin.ReadString();
                    String arts = bin.ReadString();
                    int yr = bin.ReadInt32();
                    bin.ReadString();
                    String len = bin.ReadString();
                    if (arts.Equals("Unknown Artist"))
                    {
                            sets[26].songs.Add(new SongListEntry((inv ? "INV-" : "") + name, inv ? "DNE" : songname2, arts, len));
                    }
                    else
                    {
                            sets[(int)(arts.ToUpper().ToCharArray()[0] - 65)].songs.Add(new SongListEntry((inv ? "INV-" : "") + name, inv ? "DNE" : songname2, arts, len));
                    }
                    bin.Close();
                }
            for (int i = 0; i < files.Length; i++)
                if (files[i].Length > 3 && files[i].Substring(files[i].Length - 3).ToLower().Equals("gba"))
                {
                    System.IO.BinaryReader bin = new System.IO.BinaryReader(System.IO.File.OpenRead(files[i]));
                    String songname2 = files[i].Substring(files[i].LastIndexOf('\\') + 1);
                    songname2 = songname2.Substring(0, songname2.LastIndexOf('.'));
                    byte ver = bin.ReadByte();
                    bool inv = false;
                    if (ver != 12)
                    { inv = true; }
                    String name = bin.ReadString();
                    String arts = bin.ReadString();
                    int yr = bin.ReadInt32();
                    bin.ReadString();
                    String len = bin.ReadString();
                    if (arts.Equals("Unknown Artist"))
                    {
                        bool contains = false;
                        for (int r = 0; r < sets[26].songs.Count; r++)
                            if (sets[26].songs[r].fileName.ToLower().Equals(songname2.ToLower()))
                                contains = true;
                        if (contains == false)
                            sets[26].songs.Add(new SongListEntry((inv ? "INV-" : "") + name, inv ? "DNE" : songname2, arts, len));
                    }
                    else
                    {
                        bool contains = false;
                        for (int r = 0; r < sets[(int)(arts.ToUpper().ToCharArray()[0] - 65)].songs.Count; r++)
                            if (sets[(int)(arts.ToUpper().ToCharArray()[0] - 65)].songs[r].fileName.ToLower().Equals(songname2.ToLower()))
                                contains = true;
                        if (contains == false)
                            sets[(int)(arts.ToUpper().ToCharArray()[0] - 65)].songs.Add(new SongListEntry((inv ? "INV-" : "") + name, inv ? "DNE" : songname2, arts, len));
                    }
                    bin.Close();
                }
            for (int i = 0; i < 27; i++)
                if (sets[i].songs.Count > 0)
                    setlist.Add(sets[i]);

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
        public static byte SONGDATA_VERSION = 17;

        public static bool TEST_SONG = false;

        float[] lastframes = new float[60];
        int frameIndex;

#region enginestuff

        GraphicsDeviceManager graphics;
        ContentManager content;
        SpriteBatch spritebatch;
        AudioEngine audioEngine;
        SoundBank audioSoundBank;
        WaveBank audioWaveBank;
        private Effect engine, ppEngine, fader;
        private Matrix matView;
        private Matrix matProj;
        private RenderTarget2D screenTarget, screenTargetPre, screenTargetFinal;
        private RenderTarget2D[] boardsTarget;

        bool fullScreen = false;
        bool render3D = true;
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
        private enum GUIStyle { RB = 0, GH = 2, UN = 1 };
        private GUIStyle cGUIStyle = GUIStyle.UN;
        private Texture2D rmUNbg, rmUNfg, rmUNstar, rmUNstaro;

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
        public static Color[] FretColors = { new Color(0,255,0), new Color(255,0,0), new Color(255,255,0), new Color(0,0,255), new Color(255,128,0),};
        public static Color[] FadedFretColors = { new Color(175,207,175), new Color(207,175,175), new Color(207,207,175), new Color(175,175,207), new Color(207,191,175) };
        private static Vector4[] FretColorsV4 = { new Vector4(0, 1, 0, 1), new Vector4(1, 0, 0, 1), new Vector4(1, 1, 0, 1), new Vector4(0, 0, 1, 1), new Vector4(1, 0.5f, 0, 1) };
        private int started = 0;
        public static SpriteFont DefaultFont, BigFont, SmallFont;
        public static long TicksPerSecond = 10000000;
        public static VertexBuffer square;
        public static Texture2D texGlow, texDefaultBM;
        public static VertexDeclaration vd;
        private RenderTarget2D ort;
        int windowheight, windowwidth, windowyoffset, windowxoffset;
        public static Random r;
        static String loadingText = "Loading";
        public static bool DemoMode = false;
        private bool demomodepress = false;
        byte[] failStatus;
        public static byte FS_GOOD = 0, FS_FAILING = 1, FS_DNE=2;
        float failTime;
        bool IsPaused = false;
        int pausetimer;
        float UIHScale, UIVScale;
        bool ShowFPS;
#endregion

#region PauseMenu
        
        Vector2 pauseMenuPos, pauseMenuVel, pauseWingRot;
        float pauseRot;
        int pauseSelected;
        int pauseSelectOwner;
        String[] pauseTextDisp;
        Texture2D texPauseBorder, texPauseWings, texPausePick;
#endregion

#region boards

        private static Board[] boards;
        private Texture2D[] texShard, texMult, boardBackgrounds;
        private int[] multToIndex = { -1, -1, 0, 1, 2, 3, 4, -1, 5, -1, 6, -1, 7 };
        private Texture2D texSpark;
        private Texture2D gradientMask;
        private ShatterGlass[][] glass;
        private ShatterSpark[][] sparks;
        private RenderTarget2D[] rtBoard, rtWaves;
        struct SPCircle
        {
            public Vector2 pos;
            public float rotation;
            public bool rotDir;
            public float alpha;
        }
        SPCircle[] spcircles = new SPCircle[50];
#endregion

#region song

        private static Song song;
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

#region waves

        private int WAVEDETAIL = 0;
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
        private const byte S_INGAME = 1, S_CHOOSECONT=2, S_CHOOSESONG=3, S_CHOOSEDIFF=4, S_MAINMENU=5, S_RESULTS=6, S_FREESTYLE=7, S_FAIL=8, S_FVLOGO=9, S_OPTIONS=10;
        private const byte M_GAME = 1, M_FREESTYLE = 2;
        private byte screen = S_FVLOGO, mode=M_GAME;
        Texture2D tbgGreen, tbgRed, tbgYellow, tbgPedal;
        static String songname;
        byte[] diff;
	    bool[] diffConfirm;
        Texture2D concrTex, concrBM, arrowTex, rustyTex;
        Texture2D stratTex, whitishTex, whitishBM, glassboxTex, glassboxBM;
        Texture2D[] greyishTex;
        Texture2D[] nPadTex;
        Model nPadMdl, arrowMdl, nailMdl, strat, stand;
        SpriteFont sfManager, sfGuitarist, sfBassist, sfDrummer, sfSinger;
        float[] arrowTimer, arrowRot;
        String[][] charNames;
        int[] charNameSelected;
        ContGUIData[] contguis;
        float idleTime;
        int setIndex, songIndex;
        Texture2D hairr, hairl, flameTex, texContinue;
        Vector3[][] flames;
        int slIndex = 0;
        int leader;
        bool[] finals;
        Texture2D[] texNote;
        private RenderTarget2D[] rtNote;
        int mmenu_select = 0, mmenu_ticker=0;
        int counterer = 0;
        Model mQuarter, mPick, mPoD;
        Texture2D texQuarter, texPickGP, texPickTit, texPoD;
        Model mDrumsticks, mSticks;
        Texture2D texSticks, texDTDSticks, texDSticks, texTitDSticks;
        Model mTube, mCMic, mMic, mAMic;
        Texture2D texTube, texCMic, texMic, texAMic;
        Model mString, mStrap;
        Texture2D texString, texStrap1, texStrap2, texStrap3;
        Model mSnake;
        Texture2D texSnake, texSnakeSkin;
        RenderTarget2D rtSnake;
        String[] rockerNames;
        Results[] totalresults;
        RenderTarget2D SongListRT;
        Texture2D SongListTex;
        Texture2D SongListBG, SongHiLi, songchoosetop;
        int SONGLIST_WAVEQUALITY=100;
        GBVertexFormat[] songlistGeom;
        VertexBuffer songlistVB;
        float SONGLIST_WAVE_SPEED=5, slCurrentWave, slWaveLength=50f, slWaveStrength=2, SONGLIST_LENGTH=170, SONGLIST_WIDTH=1;
        String[][] mMenuStr;
        Color[][] mMenuCol;
        Vector2 menuShiftPos;
        SpriteFont sfMenu;
        int menuSnakeRotOffset;
        Texture2D waves, wave, failbg, resultsScroller;
        float dialogscroll;
        Texture2D coolbg1, coolbg2;
        float waveM1, waveM2;
        Texture2D texHeader;
        bool[][] diffExists;

        Texture2D texBarrel, texGoo1, texGoo2, texPresser;
        Model mBarrel, mGoo1, mGoo2, mPresser;
        BasicEffect bEngine;
        float logoTime;

        Model mCurtain;
        Texture2D texCurtainLeft, texCurtainRight;

        float mmLogoTime;

        //options screen
        Model mamp1, mamp2;
        Texture2D tamp1, tamp2;
        Texture2D tknob, ttape;
        Texture2D tledon, tledoff, tswitchon, tswitchoff;
        int[] resX = { 640, 800, 1024, };
        int[] resY = { 480, 600,  768, };
        String[] guiStyle = { "Unsigned", "Rock Band" };
        float optionsOffset = 0;
        int optionsSelected;
        bool optionsSelectFull;
        float intro;

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

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            content = new ContentManager(Services);

            r = new Random((int)DateTime.Now.Ticks);

            content.RootDirectory = "";
        }

        protected override void Initialize()
        {
            Configurate();
            InitXNAApp();

#if WINDOWS
            //this.IsFixedTimeStep = false;
            //graphics.SynchronizeWithVerticalRetrace = false;

            String[] rFiles = System.IO.Directory.GetFiles(System.IO.Directory.GetCurrentDirectory());
            bool gblExists = false;
            for (int i = 0; i < rFiles.Length; i++)
                if (rFiles[i].Substring(rFiles[i].LastIndexOf('\\') + 1).ToLower().Trim().Equals("songlist.txt"))
                    gblExists = true;
            if (gblExists)
            {
                setLists = new SetList[5];
                setLists[2] = new SetList();
                setLists[2].LoadCustom("songlist.txt");
                setLists[2].name = "Custom";
                //LoadCustomSonglist();
            }
            else
                setLists = new SetList[4];
            setLists[0] = new SetList();
            setLists[0].LoadByDecade();
            setLists[0].name = "By Decade";
            setLists[1] = new SetList();
            setLists[1].LoadByGenre();
            setLists[1].name = "By Genre";
            setLists[2] = new SetList();
            setLists[2].LoadByName();
            setLists[2].name = "Alphabetic";
            setLists[3] = new SetList();
            setLists[3].LoadByArtist();
            setLists[3].name = "By Artist";

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
            currentFE = FRAME_EFFECT.CONSTANT;
            countFE = 0;
            currentFES = FRAME_EFFECT_STYLE.BLINK;
            countFES=1;
            postProcessEffects = 0;
            hue_shift=0;
            blur_strength = 0;
            blur_passes = 0;
            grain_strength = 0;


            instruments = new bool[4];
            glass = new ShatterGlass[4][];
            for(int i=0;i<4;i++)
                glass[i] = new ShatterGlass[100];
            sparks = new ShatterSpark[4][];
            for(int i=0;i<4;i++)
                sparks[i] = new ShatterSpark[100];

            contInput = new byte[4];
            controllers = new GamePadState[4];

            contCapabilities = new GamePadCapabilities[4];

            diff = new byte[4];
	        diffConfirm = new bool[4];

            mMenuStr = new string[4][];
            mMenuStr[0] = new string[4];
            mMenuStr[0][0] = "SINGLE PLAYER";
            mMenuStr[0][1] = "MULTIPLAYER";
            mMenuStr[0][2] = "OPTIONS";
            mMenuStr[0][3] = "EXIT";
            mMenuStr[1] = new string[2];
            mMenuStr[1][0] = "CAREER";
            mMenuStr[1][1] = "QUICKPLAY";
            mMenuStr[2] = new string[4];
            mMenuStr[2][0] = "QUICKPLAY";
            mMenuStr[2][1] = "BAND CAREER";
            mMenuStr[2][2] = "ROCK-OFF";
            mMenuStr[2][3] = "SCORE KRIEG";
            mMenuStr[3] = new string[3];
            mMenuStr[3][0] = "AUDIO OPTIONS";
            mMenuStr[3][1] = "VIDEO OPTIONS";
            mMenuStr[3][2] = "DATA OPTIONS";
            mMenuCol = new Color[4][];
            mMenuCol[0] = new Color[4];
            mMenuCol[0][0] = Color.Red;
            mMenuCol[0][1] = Color.White;
            mMenuCol[0][2] = Color.White;
            mMenuCol[0][3] = Color.White;
            mMenuCol[1] = new Color[2];
            mMenuCol[1][0] = Color.Red;
            mMenuCol[1][1] = Color.White;
            mMenuCol[2] = new Color[4];
            mMenuCol[2][0] = Color.White;
            mMenuCol[2][1] = Color.Red;
            mMenuCol[2][2] = Color.Red;
            mMenuCol[2][3] = Color.Red;
            mMenuCol[3] = new Color[3];
            mMenuCol[3][0] = Color.Red;
            mMenuCol[3][1] = Color.Red;
            mMenuCol[3][2] = Color.Red;

            failStatus = new byte[4];
            failTime = -2;
            
            GBVertexFormat[] arr = { new GBVertexFormat(new Vector3(-1f,0f, 1f),new Vector3(0f,1f,0f),new Vector2(0f,0f)),
                                     new GBVertexFormat(new Vector3(-1f,0f,-1f),new Vector3(0f,1f,0f),new Vector2(0f,1f)),
                                     new GBVertexFormat(new Vector3( 1f,0f, 1f),new Vector3(0f,1f,0f),new Vector2(1f,0f)),
                                     new GBVertexFormat(new Vector3( 1f,0f, 1f),new Vector3(0f,1f,0f),new Vector2(1f,0f)),
                                     new GBVertexFormat(new Vector3(-1f,0f,-1f),new Vector3(0f,1f,0f),new Vector2(0f,1f)),
                                     new GBVertexFormat(new Vector3( 1f,0f,-1f),new Vector3(0f,1f,0f),new Vector2(1f,1f))};
            square = new VertexBuffer(graphics.GraphicsDevice, GBVertexFormat.SizeInBytes * 6, BufferUsage.WriteOnly);
            square.SetData<GBVertexFormat>(arr);
            totalresults = new Results[0];

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

            graphics.PreferredBackBufferWidth = windowwidth;
            graphics.PreferredBackBufferHeight = windowheight;
            graphics.ApplyChanges();
            if(fullScreen)
                graphics.ToggleFullScreen();

            engine = content.Load<Effect>("shaders\\HFPS_Shader_XNA");//new Effect(graphics.GraphicsDevice,"shaders\\HFPS_Shader_XNA.fxc",CompilerOptions.None,new EffectPool());
            ppEngine = content.Load<Effect>("shaders\\PP_Shader_XNA");
            fader = content.Load<Effect>("shaders\\BoardFade");

            SetProjMatrix(Window.ClientBounds.Width,Window.ClientBounds.Height);
            graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;
            graphics.SynchronizeWithVerticalRetrace = true;


            spritebatch = new SpriteBatch(graphics.GraphicsDevice);
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
                            renderLevel = 0;
                        else
                            renderLevel = 10;
                    }
                    else if (str.Length > 10 && str.Substring(0, 10).ToLower().Equals("wavedetail"))
                    {
                        WAVEDETAIL = Int32.Parse(str.Substring(str.IndexOf('=') + 1).Trim());
                    }
                    else if (str.Length > 10 && str.Substring(0, 10).ToLower().Equals("resolution"))
                    {
                        windowwidth = Int32.Parse(str.Substring(str.IndexOf('=') + 1,Math.Max(str.IndexOf('x'),str.IndexOf('X'))-(str.IndexOf('=') + 1)).Trim());
                        windowheight = Int32.Parse(str.Substring(Math.Max(str.IndexOf('x'),str.IndexOf('X'))+1).Trim());
                    }
                    else if (str.Length > 10 && str.Substring(0, 10).ToLower().Equals("fullscreen"))
                    {
                        fullScreen = Boolean.Parse(str.Substring(str.IndexOf('=') + 1).Trim());
                    }
                    else if (str.Length > 10 && str.Substring(0, 10).ToLower().Equals("halfrender"))
                    {
                        HALF_RENDER = Boolean.Parse(str.Substring(str.IndexOf('=') + 1).Trim());
                    }
                    else if (str.Length > 10 && str.Substring(0, 10).ToLower().Equals("iguihasfps"))
                    {
                        ShowFPS = Boolean.Parse(str.Substring(str.IndexOf('=') + 1).Trim());
                    }
                    else if (str.Length > 10 && str.Substring(0, 10).ToLower().Equals("igguistyle"))
                    {
                        String strn = str.Substring(str.IndexOf('=') + 1).Trim();
                        cGUIStyle = strn.ToLower().Equals("rockband") ? GUIStyle.RB : GUIStyle.UN;
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
            matProj = Matrix.CreatePerspectiveFieldOfView((float)Math.PI / 4.0f,
                              w / (float)h,
                              2f, 750.0f);
        }
        
        protected override void LoadContent()
        {
#if WINDOWS
            audioEngine = new AudioEngine("audio\\Win\\Unsigned.xgs");
            audioSoundBank = new SoundBank(audioEngine, "audio\\Win\\Sound Bank.xsb");
            audioWaveBank = new WaveBank(audioEngine, "audio\\Win\\Wave Bank.xwb");
#else
            audioEngine = new AudioEngine("audio\\Unsigned.xgs");
            audioSoundBank = new SoundBank(audioEngine, "audio\\Sound Bank.xsb");
            audioWaveBank = new WaveBank(audioEngine, "audio\\Wave Bank.xwb");
#endif
            sfBassist = content.Load<SpriteFont>("fonts\\bassist");
            sfGuitarist = content.Load<SpriteFont>("fonts\\guitarist");
            sfDrummer = content.Load<SpriteFont>("fonts\\drummer");
            sfSinger = content.Load<SpriteFont>("fonts\\singer");
            sfManager = content.Load<SpriteFont>("fonts\\manager");
            sfMenu = content.Load<SpriteFont>("fonts\\menu");
            texDefaultBM = content.Load<Texture2D>("graphics\\blankbm");
            DefaultFont = content.Load<SpriteFont>("BasicFont");
            BigFont = content.Load<SpriteFont>("fonts\\bigfont");
            SmallFont = content.Load<SpriteFont>("fonts\\smallfont");
            gradient = content.Load<Texture2D>("graphics\\gradient");
            gradientMask = content.Load<Texture2D>("graphics\\gradientMask");
            tbgGreen = content.Load<Texture2D>("graphics\\large_face_a");
            tbgRed = content.Load<Texture2D>("graphics\\large_face_b");
            tbgYellow = content.Load<Texture2D>("graphics\\large_face_y");
            texWhite = content.Load<Texture2D>("graphics\\white");

            texBarrel = content.Load<Texture2D>("graphics\\barrel");
            texGoo1 = content.Load<Texture2D>("graphics\\goo1");
            texGoo2 = content.Load<Texture2D>("graphics\\goo2");
            texPresser = content.Load<Texture2D>("graphics\\presser");

            mBarrel = content.Load<Model>("meshes\\barrel");
            mGoo1 = content.Load<Model>("meshes\\goo1");
            mGoo2 = content.Load<Model>("meshes\\goo2");
            mPresser = content.Load<Model>("meshes\\presser");

            bEngine = new BasicEffect(graphics.GraphicsDevice, new EffectPool());
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

        private void LoadGameContent()
        {
            if (GameLoaded || GameLoading)
                return;
            ThreadStart ThreadStarter = delegate
            {
                if(rtNote!=null)
                for (int i = 0; i < rtNote.Length; i++)
                    if (rtNote[i] != null)
                        rtNote[i].Dispose();
                rtNote = null;
                if (SongListRT != null)
                {
                    SongListRT.Dispose();
                    SongListRT = null;
                }
                GC.Collect();
                if (mode == M_GAME)
                    InitForSong(instruments[0], instruments[1], instruments[2], instruments[3], diff, venueName, this);
                else
                {
                    boardsTarget = new RenderTarget2D[instruments.Length];
                    for(int i=0;i<instruments.Length;i++)
                        if(instruments[i])
                            boardsTarget[i] = new RenderTarget2D(graphics.GraphicsDevice, windowwidth, windowheight, 1, SurfaceFormat.Color);
                    
                    Board.curveHeight = 0.03f;
                    Board.height = -2.0f;
                    Board.length = 3f;
                    Board.width = 0.7f;
                    Board.rotate = .5f;
                    Board.zeroZ = 2.8f;
                    Board.sFade = 0.8f;
                    Board.eFade = 1.2f;
                }


                if (HALF_RENDER)
                {
                    screenTarget = new RenderTarget2D(graphics.GraphicsDevice, windowwidth / 2, windowheight / 2, 1, SurfaceFormat.Color);
                    screenTargetPre = new RenderTarget2D(graphics.GraphicsDevice, windowwidth / 2, windowheight / 2, 1, SurfaceFormat.Color);
                    rtBoard = new RenderTarget2D[4];
                    rtWaves = new RenderTarget2D[4];
                    for(int i=0;i<4;i++)
                        if (instruments[i] && i != 1)
                        {
                            rtBoard[i] = new RenderTarget2D(graphics.GraphicsDevice, windowheight/4, windowheight/2, 1, SurfaceFormat.Color);
                            rtWaves[i] = new RenderTarget2D(graphics.GraphicsDevice, windowheight/4, windowheight/2, 1, SurfaceFormat.Color);
                        }
                }
                else
                {
                    screenTarget = new RenderTarget2D(graphics.GraphicsDevice, windowwidth, windowheight, 1, SurfaceFormat.Color);
                    screenTargetPre = new RenderTarget2D(graphics.GraphicsDevice, windowwidth, windowheight, 1, SurfaceFormat.Color);
                    rtBoard = new RenderTarget2D[4];
                    rtWaves = new RenderTarget2D[4];
                    for(int i=0;i<4;i++)
                        if (instruments[i] && i != 1)
                        {
                            rtBoard[i] = new RenderTarget2D(graphics.GraphicsDevice, windowheight/2, windowheight, 1, SurfaceFormat.Color);
                            rtWaves[i] = new RenderTarget2D(graphics.GraphicsDevice, windowheight/2, windowheight, 1, SurfaceFormat.Color);
                        }
                }


                for (int i = 0; i < spcircles.Length; i++)
                {
                    spcircles[i].alpha = (float)r.NextDouble();
                    spcircles[i].rotation = (float)r.NextDouble();
                    spcircles[i].pos = new Vector2((float)r.NextDouble(), (float)r.NextDouble());
                    spcircles[i].rotDir = r.Next() % 2 == 0;
                }

                screenTargetFinal = new RenderTarget2D(graphics.GraphicsDevice, windowwidth, windowheight, 1, SurfaceFormat.Color);                        

                Board.InitModel(graphics,content,engine);
                texRockstarRed = content.Load<Texture2D>("graphics\\red");
                texRockstarRing = content.Load<Texture2D>("graphics\\ring");
                texRockstarCover = content.Load<Texture2D>("graphics\\starcover");
                texRockMeterOutline = content.Load<Texture2D>("graphics\\rockmeter");
                texRockMeterLogoStem = content.Load<Texture2D>("graphics\\logo_stem");
                texRockMeterGuitarLogo = content.Load<Texture2D>("graphics\\guitar_logo");
                texRockMeterBassLogo = content.Load<Texture2D>("graphics\\bass_logo");
                texRockMeterDrumLogo = content.Load<Texture2D>("graphics\\drums_logo");
                texRockMeterSingerLogo = content.Load<Texture2D>("graphics\\vocal_logo");
                texScoreBoard = content.Load<Texture2D>("graphics\\scoreboard");
                texLine = content.Load<Texture2D>("graphics\\line");
                texLineEnd = content.Load<Texture2D>("graphics\\linetaper");
                texGlow = content.Load<Texture2D>("graphics\\triggerglow");
                rmUNbg = content.Load<Texture2D>("graphics\\roundmeterbg");
                rmUNfg = content.Load<Texture2D>("graphics\\roundmeterfg");
                rmUNstar = content.Load<Texture2D>("graphics\\scorestar");
                rmUNstaro = content.Load<Texture2D>("graphics\\scorestaro");
                texPauseBorder = content.Load<Texture2D>("graphics\\pauseborder");
                texPauseWings = content.Load<Texture2D>("graphics\\pausewing");
                texPausePick = content.Load<Texture2D>("graphics\\pickofselect");
                pauseMenuPos = new Vector2(windowwidth / 2, windowheight / 2);
                pauseMenuVel = new Vector2(10,0);
                pauseWingRot.Y = 30;
                texMult = new Texture2D[8];
                for (int i = 0; i <= 12; i++)
                    if(multToIndex[i]>=0)
                        texMult[multToIndex[i]] = content.Load<Texture2D>("graphics\\X" + i);
                texShard = new Texture2D[8];
                for (int k = 0; k < 8; k++)
                    texShard[k] = content.Load<Texture2D>("graphics\\glassshard0" + (k + 1));
                texSpark = content.Load<Texture2D>("graphics\\spark");
                Board.boardTexPlain = new Texture2D[2][];
                /*for (int i = 0; i < 2; i++)
                {
                    Board.boardTexPlain[i] = new Texture2D[Board.boardValidBPM.Length];
                    for (int k = 0; k < Board.boardValidBPM.Length; k++)
                    {
                        Board.boardTexPlain[i][Board.boardBeatsIndex[Board.boardValidBPM[k]]] = content.Load<Texture2D>("graphics\\board_" + (i + 4) + "" + (Board.boardValidBPM[k]));
                    }
                }*/
                //Board.boardTexPlain[1][4] = content.Load<Texture2D>("graphics\\test");
                Board.texTriggerBorder = content.Load<Texture2D>("graphics\\triggerborder");
                Board.drumfillTex = content.Load<Texture2D>("graphics\\drumfill");
                Board.spMeterBG = content.Load<Texture2D>("graphics\\boardmeter");
                Board.spMeterFill = content.Load<Texture2D>("graphics\\white");
                Board.spMeterLED = content.Load<Texture2D>("graphics\\bulb");
                Board.spMeterCurl = content.Load<Texture2D>("graphics\\curl");
                Board.texTriggerBorderLit = content.Load<Texture2D>("graphics\\triggerborderlit");
                Board.texBlast = content.Load<Texture2D>("graphics\\blast");

                String[] strs = System.IO.Directory.GetFiles("boards\\");

                boardBackgrounds = new Texture2D[4];
                for (int i = 0; i < 4; i++)
                    if (instruments[i])
                    {
                        int index = r.Next(strs.Length);
                        boardBackgrounds[i] = content.Load<Texture2D>(strs[index].Substring(0, strs[index].LastIndexOf('.')));
                    }

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
                Board.vBar = content.Load<Texture2D>("graphics\\vocalbar");
                Board.vBGExt = content.Load<Texture2D>("graphics\\vocalbg_ext");
                Board.vBGInt = content.Load<Texture2D>("graphics\\vocalbg_int");
                Board.vFuzz = content.Load<Texture2D>("graphics\\vocalfuzz");
                Board.vHeadBar = content.Load<Texture2D>("graphics\\vocalheadbar");
                Board.vGlow = content.Load<Texture2D>("graphics\\vGlow");
                for (int i = 0; i < 4; i++)
                    rockMeterLevel[i] = 80;
                GC.Collect();
                GameLoaded = true;
                GameLoading = false;
            };
            GameLoading = true;
            if(mode==M_GAME)
#if WINDOWS
                song = new Song(songname,this.Window.Handle);
#else
                song = new Song(songname,audioEngine,audioSoundBank,audioWaveBank);
#endif
            Thread myThread = new Thread(ThreadStarter);
            myThread.Start();
        }

        private void UnloadGameContent()
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
            }*/
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
                

        }

        protected override void Update(GameTime gameTime)
        {
            //Thread.Sleep(1);
            if (pausetimer > 0)
                pausetimer -= gameTime.ElapsedGameTime.Milliseconds;
            windowheight = graphics.GraphicsDevice.Viewport.Height;
            windowwidth = graphics.GraphicsDevice.Viewport.Width;
            audioEngine.Update();

            if (demomodepress != Keyboard.GetState().IsKeyDown(Keys.O))
            {
                demomodepress = Keyboard.GetState().IsKeyDown(Keys.O);
                if (demomodepress)
                    DemoMode = !DemoMode;
            }

            GetGamepadStates(false);
            #region fvlogo
            if (screen == S_FVLOGO)
            {
                if (logoTime >= 35)
                {
                    if(MenuLoaded)
                        screen = S_MAINMENU;
                }
                else if (!MenuLoaded && !MenuLoading)
                    LoadMenuContent();
                if(logoTime<35)
                logoTime += (float)gameTime.ElapsedGameTime.TotalSeconds*8;
            }
            #endregion
            #region mainmenu
            else if (screen == S_MAINMENU)
            {

                
#if !DEBUG
                    try
                    {
#endif
                    waveM1 += gameTime.ElapsedGameTime.Milliseconds / 50f;
                    waveM2 -= gameTime.ElapsedGameTime.Milliseconds / 25f;
                    if (waveM1 >= windowwidth)
                        waveM1 -= windowwidth;
                    if (waveM2 <= 0)
                        waveM2 += windowwidth;
                    if (mmenu_ticker <= 0)
                    {
                        if (mmLogoTime > 2)
                        {
                            int collective = 0;
                            bool green = false, red = false;
                            GetGamepadStates(true);
                            for (int i = 0; i < 4; i++)
                                if (controllers[i].IsConnected)
                                {
                                    if (controllers[i].DPad.Down == ButtonState.Pressed)
                                        collective--;
                                    if (controllers[i].DPad.Up == ButtonState.Pressed)
                                        collective++;
                                    if (controllers[i].DPad.Right == ButtonState.Pressed)
                                        collective--;
                                    if (controllers[i].DPad.Left == ButtonState.Pressed)
                                        collective++;
                                    if (controllers[i].Buttons.A == ButtonState.Pressed)
                                        green = true;
                                    if (controllers[i].Buttons.B == ButtonState.Pressed)
                                        red = true;
                                    if (contCapabilities[i].GamePadType == GamePadType.DrumKit)
                                    {
                                        if (controllers[i].Buttons.Y == ButtonState.Pressed)
                                            collective++;
                                        if (controllers[i].Buttons.X == ButtonState.Pressed)
                                            collective--;
                                    }
                                }
                            if (Keyboard.GetState().IsKeyDown(Keys.Down))
                                collective--;
                            if (Keyboard.GetState().IsKeyDown(Keys.Up))
                                collective++;
                            if (Keyboard.GetState().IsKeyDown(Keys.Right))
                                collective--;
                            if (Keyboard.GetState().IsKeyDown(Keys.Left))
                                collective++;
                            if (Keyboard.GetState().IsKeyDown(Keys.Enter) || Keyboard.GetState().IsKeyDown(Keys.Space) || Keyboard.GetState().IsKeyDown(Keys.A))
                                green = true;
                            if (Keyboard.GetState().IsKeyDown(Keys.Back) || Keyboard.GetState().IsKeyDown(Keys.Escape))
                                red = true;
                            if (mmenu_select % 10 == 0)
                            {
                                mmenu_select -= 10 * collective;
                                if (collective != 0)
                                {
                                    menuShiftPos.Y = -1 * collective;
                                    menuSnakeRotOffset += collective;
                                }
                                while (mmenu_select < 10)
                                { mmenu_select += 10; menuShiftPos.Y = 0; menuSnakeRotOffset -= collective; }
                                while (mmenu_select >= 50)
                                { mmenu_select -= 10; menuShiftPos.Y = 0; menuSnakeRotOffset -= collective; }

                                if (green && mmenu_select == 20)
                                {
                                    mmenu_select++;
                                    menuShiftPos.X = 1;
                                }
                                if (green && mmenu_select == 30)
                                    screen = S_OPTIONS;
                                if (green && mmenu_select == 40)
                                    this.Exit();
                            }
                            else
                            {
                                if (mmenu_select > 20 && mmenu_select < 30)
                                {
                                    mmenu_select -= collective;
                                    if (collective != 0)
                                    {
                                        menuShiftPos.Y = -1 * collective;
                                        menuSnakeRotOffset += collective;
                                    }
                                    while (mmenu_select < 21)
                                    { mmenu_select += 1; menuShiftPos.Y = 0; menuSnakeRotOffset -= collective; }
                                    while (mmenu_select > 24)
                                    { mmenu_select -= 1; menuShiftPos.Y = 0; menuSnakeRotOffset -= collective; }

                                    if (green && mmenu_select == 21)
                                        screen = S_CHOOSECONT;
                                }


                                if (red)
                                {
                                    mmenu_select = mmenu_select / 10 * 10;
                                    menuShiftPos.X = -1;
                                }
                            }
                            if (collective != 0 || green || red)
                                mmenu_ticker = 200;
                        }
                        else
                        {
                            mmLogoTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                            while (mmenu_select < 10)
                            { mmenu_select += 10; menuShiftPos.Y = 0; }
                            while (mmenu_select >= 50)
                            { mmenu_select -= 10; menuShiftPos.Y = 0; }
                        }
                    }
                    else
                        mmenu_ticker -= gameTime.ElapsedGameTime.Milliseconds;
                    menuShiftPos.Y *= 0.9f;
                    menuShiftPos.X *= 0.9f;
		for(int i=0;i<4;i++)
			diffConfirm[i]=false;
                
#if !DEBUG
                }
                catch(Exception e)
                {
#if WINDOWS
                    System.Windows.Forms.MessageBox.Show("Problem in Update/MM/Pt1\n"+e.Message+"\n"+e.StackTrace);
#endif
                    Exit();
                    return;
                }
#endif
            }
            #endregion
            #region optionsscreen
            else if (screen == S_OPTIONS)
            {
                
            }
            #endregion
            #region diffscreen
            else if (screen == S_CHOOSEDIFF)
            {
#if !DEBUG
                try
                {
#endif
                if (mmenu_ticker <= 0)
                {
                    bool green = false, red = false;
                    GetGamepadStates(true);
                    for (int i = 0; i < 4; i++)
                        if (contInput[i]<4 && controllers[contInput[i]].IsConnected)
                        {
                            if (controllers[contInput[i]].Buttons.A == ButtonState.Pressed)
                            {
                                if (diffConfirm[i])
                                    green = true;
                                else
                                    diffConfirm[i] = true;
                            }
                            if (controllers[contInput[i]].Buttons.B == ButtonState.Pressed)
                            {
                                if (diffConfirm[i])
                                    diffConfirm[i] = false;
                                else
                                    red = true;
                            }
                        }
                        else if (contInput[i] == 4)
                        {
                            if (Keyboard.GetState().IsKeyDown(Keys.Enter) || Keyboard.GetState().IsKeyDown(Keys.Space) || Keyboard.GetState().IsKeyDown(Keys.A))
                            {
                                if (diffConfirm[i])
                                    green = true;
                                else
                                    diffConfirm[i] = true;
                            }
                            if (Keyboard.GetState().IsKeyDown(Keys.Back) || Keyboard.GetState().IsKeyDown(Keys.Escape))
                            {
                                if (diffConfirm[i])
                                    diffConfirm[i] = false;
                                else
                                    red = true;
                            }
                        }
                    for(int i=0;i<4;i++)
                    if(instruments[i] && !diffConfirm[i])
                    {
                        if (contInput[i] >= 4)
                        {

                            bool up = false, down = false;
                            if (Keyboard.GetState().IsKeyDown(Keys.Down))
                                down = true;
                            if (Keyboard.GetState().IsKeyDown(Keys.Up))
                                up = true;
                            if (Keyboard.GetState().IsKeyDown(Keys.Right))
                                down = true;
                            if (Keyboard.GetState().IsKeyDown(Keys.Left))
                                up = true;
                            if (up && diff[i] > 0)
                                diff[i]--;
                            if (down && diff[i] < 3)
                                diff[i]++;
                            if (up || down)
                                mmenu_ticker = 200;
                        }
                        else
                        {
                            bool up=false, down = false;
                            if (controllers[contInput[i]].DPad.Down == ButtonState.Pressed)
                                down = true;
                            if (controllers[contInput[i]].DPad.Up == ButtonState.Pressed)
                                up = true;
                            if (controllers[contInput[i]].DPad.Right == ButtonState.Pressed)
                                down = true;
                            if (controllers[contInput[i]].DPad.Left == ButtonState.Pressed)
                                up = true;
                            if (contCapabilities[contInput[i]].GamePadType == GamePadType.DrumKit)
                            {
                                if (controllers[contInput[i]].Buttons.X == ButtonState.Pressed)
                                    down = true;
                                if (controllers[contInput[i]].Buttons.Y == ButtonState.Pressed)
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

                    bool allconfirmed = true;
                    for (int i = 0; i < 4; i++)
                        if (instruments[i] && !diffConfirm[i])
                            allconfirmed = false;
                    if (green && allconfirmed)
                    {
                        if (mode == M_GAME)
                            screen = S_INGAME;
                        else if (mode == M_FREESTYLE)
                            screen = S_FREESTYLE;
                        for (int i = 0; i < 4; i++)
                            if (diff[i] == 0)
                                diff[i] = D_EASY;
                            else if (diff[i] == 1)
                                diff[i] = D_MEDIUM;
                            else if (diff[i] == 2)
                                diff[i] = D_HARD;
                            else if (diff[i] == 3)
                                diff[i] = D_EXPERT;
                        LoadGameContent();
                    }
                    if (red)
                    { screen = S_CHOOSESONG; mmenu_ticker = 200; }
                }
                else
                    mmenu_ticker -= gameTime.ElapsedGameTime.Milliseconds;
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
            #endregion
            #region songscreen
            else if (screen == S_CHOOSESONG)
            {
                bool chgd;
                SetList setlist;
#if !DEBUG
                try
                {
#endif
                chgd = SongListTex==null;

                setlist = setLists[slIndex];

                if (mmenu_ticker <= 0)
                {
                    int collective = 0;
                    bool green = false, red = false, yellow = false;
                    GetGamepadStates(true);

                    for (int i = 0; i < 4; i++)
                        if (controllers[i].IsConnected)
                        {
                            if (contCapabilities[i].GamePadType == GamePadType.DrumKit && controllers[i].Buttons.Y == ButtonState.Pressed)
                                collective--;
                            if (contCapabilities[i].GamePadType == GamePadType.DrumKit && controllers[i].Buttons.X == ButtonState.Pressed)
                                collective++;
                            if (controllers[i].DPad.Down == ButtonState.Pressed)
                                collective--;
                            if (controllers[i].DPad.Up == ButtonState.Pressed)
                                collective++;
                            if (controllers[i].Buttons.A == ButtonState.Pressed)
                                green = true;
                            if (controllers[i].Buttons.B == ButtonState.Pressed)
                                red = true;
                            if (contCapabilities[i].GamePadType == GamePadType.DrumKit && controllers[i].Buttons.LeftShoulder == ButtonState.Pressed)
                                yellow = true;
                            if (contCapabilities[i].GamePadType != GamePadType.DrumKit && controllers[i].Buttons.Y == ButtonState.Pressed)
                                yellow = true;
                        }
                    if (Keyboard.GetState().IsKeyDown(Keys.Down))
                        collective--;
                    if (Keyboard.GetState().IsKeyDown(Keys.Up))
                        collective++;
                    if (Keyboard.GetState().IsKeyDown(Keys.Enter) || Keyboard.GetState().IsKeyDown(Keys.Space) || Keyboard.GetState().IsKeyDown(Keys.A))
                        green = true;
                    if (Keyboard.GetState().IsKeyDown(Keys.Back) || Keyboard.GetState().IsKeyDown(Keys.Escape))
                        red = true;
                    if (Keyboard.GetState().IsKeyDown(Keys.D))
                        yellow = true;

                    if (setlist.setlist.Count > 0)
                    {
                        if (collective > 0 && songIndex > 0)
                        { songIndex--; chgd = true; }
                        else if (collective > 0 && songIndex <= 0 && setIndex > 0)
                        { setIndex--; songIndex = setlist.setlist[setIndex].songs.Count - 1; chgd = true; }
                        else if (collective < 0 && songIndex < setlist.setlist[setIndex].songs.Count - 1)
                        { songIndex++; chgd = true; }
                        else if (collective < 0 && songIndex >= setlist.setlist[setIndex].songs.Count - 1 && setIndex < setlist.setlist.Count - 1)
                        { songIndex = 0; setIndex++; chgd = true; }

                        if (green)
                        {
                            songname = setlist.setlist[setIndex].songs[songIndex].fileName;
                            for (int k = 0; k < 4; k++)
                                diff[k] = 1;
                            FillSongDiffs(songname);
                            screen = S_CHOOSEDIFF;
                            mmenu_ticker = 200;
                        }
                    }
                    if (yellow)
                    {
                        slIndex++;
                        if (slIndex >= setLists.Length)
                            slIndex = 0;
                        chgd = true;
                        mmenu_ticker = 200;
                        songIndex = 0;
                        setIndex = 0;
                    }
                    if (red)
                    {screen = S_CHOOSECONT; mmenu_ticker = 200; }
                    if (collective != 0)
                        mmenu_ticker = 200;
                }
                else
                    mmenu_ticker -= gameTime.ElapsedGameTime.Milliseconds;
#if !DEBUG
                }
                catch(Exception e)
                {
#if WINDOWS
                    System.Windows.Forms.MessageBox.Show("Problem in Update/SS/Pt0\n"+e.Message+"\n"+e.StackTrace);
#endif
                    Exit();
                    return;
                }
                try
                {
#endif
                    if (chgd)
                    {
                        
                        setlist = setLists[slIndex];

                        if(SongListRT==null)
                            SongListRT = new RenderTarget2D(graphics.GraphicsDevice, 512, 512, 1, SurfaceFormat.Color);

                        graphics.GraphicsDevice.SetRenderTarget(0, SongListRT);
                        spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
                        graphics.GraphicsDevice.RenderState.AlphaDestinationBlend = Blend.InverseSourceAlpha;
                        graphics.GraphicsDevice.Clear(new Color(0, 0, 0, 0));
                        spritebatch.Draw(SongListBG, new Rectangle(0, 0, SongListRT.Width, SongListRT.Height), Color.White);
                        if (setlist.setlist.Count > 0)
                        {
                            List<String> drawnS = new List<string>();
                            List<String> drawnA = new List<string>();
                            int popper = -1;//a nice random Kuntz reference
                            for (int i = 0; i < setlist.setlist.Count; i++)
                            {
                                drawnS.Add("@@@" + setlist.setlist[i].name);
                                drawnA.Add("");
                                for (int k = 0; k < setlist.setlist[i].songs.Count; k++)
                                {
                                    String s = "";
                                    s = setlist.setlist[i].songs[k].displayName;
                                    drawnS.Add(s);
                                    drawnA.Add(setlist.setlist[i].songs[k].artistName);
                                    if (setIndex == i && songIndex == k)
                                        popper = drawnS.Count - 1;
                                }
                            }
                            int low = popper;
                            int high = popper;
                            int avail = 8;
                            while (avail > 0)
                            {
                                if (low == 0 && high == drawnS.Count - 1)
                                    break;
                                if (low > 0)
                                { low--; avail--; }
                                if (high < drawnS.Count - 1)
                                { high++; avail--; }
                            }
                            spritebatch.Draw(SongHiLi, new Rectangle(20, (popper-low) * 40 + 95, SongListRT.Width - 40, 50), Color.White);
                            for (int i = low; i <= high; i++)
                            {
                                spritebatch.DrawString(DefaultFont, drawnS[i].StartsWith("@@@") ? drawnS[i].Substring(3) : drawnS[i], new Vector2(10 + (drawnS[i].StartsWith("@@@") ? 20 : 50), (i - low) * 40 + 100), drawnS[i].StartsWith("@@@") ? new Color(new Vector3(.75f, .375f, 0)) : Color.Black);
                                if (!drawnS[i].StartsWith("@@@"))
                                    spritebatch.DrawString(SmallFont, drawnA[i], new Vector2(70, (i - low) * 40 + 125), Color.Black);
                            }
                            //spritebatch.DrawString(DefaultFont, vSongNames[j][k], new Vector2(10 + (k == 0 ? 20 : 50), (ii + songoffset) * 40 + 60), k == 0 ? new Color(new Vector3(.75f, .375f, 0)) : Color.Black);
                        }
                        spritebatch.End();
                        graphics.GraphicsDevice.SetRenderTarget(0, null);
                        SongListTex = SongListRT.GetTexture();
                    }
#if !DEBUG
                }
                catch(Exception e)
                {
#if WINDOWS
                    System.Windows.Forms.MessageBox.Show("Problem in Update/SS/Pt1\n"+e.Message+"\n"+e.StackTrace);
#endif
                    Exit();
                    return;
                }
#endif
                
            }
            #endregion
            #region contchoosescreen
            else if (screen == S_CHOOSECONT)
            {
                if (rtNote == null)
                {
                    rtNote = new RenderTarget2D[4];
                    rtNote[0] = new RenderTarget2D(graphics.GraphicsDevice, 256, 256, 1, SurfaceFormat.Color);
                    rtNote[1] = new RenderTarget2D(graphics.GraphicsDevice, 256, 256, 1, SurfaceFormat.Color);
                    rtNote[2] = new RenderTarget2D(graphics.GraphicsDevice, 256, 256, 1, SurfaceFormat.Color);
                    rtNote[3] = new RenderTarget2D(graphics.GraphicsDevice, 256, 256, 1, SurfaceFormat.Color);

                    for (int i = 0; i < 4; i++)
                    {
                        ort = (RenderTarget2D)graphics.GraphicsDevice.GetRenderTarget(0);
                        graphics.GraphicsDevice.SetRenderTarget(0, rtNote[i]);
                        spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
                        spritebatch.Draw(nPadTex[i], new Rectangle(0, 0, 256, 256), Color.White);
                        spritebatch.DrawString(sfManager, musicianNames[i], new Vector2(70, 20), Color.Black);
                        spritebatch.End();
                        graphics.GraphicsDevice.SetRenderTarget(0, ort);
                        texNote[i] = rtNote[i].GetTexture();
                    }
                }
#if !DEBUG
                try
                {
#endif
                if (Keyboard.GetState().GetPressedKeys().Length <= 0)
                    idleTime += gameTime.ElapsedGameTime.Milliseconds / 1000f;
                else
                    idleTime = 0;

                GetGamepadStates(false);
                
#if !DEBUG
                }
                catch(Exception e)
                {
#if WINDOWS
                    System.Windows.Forms.MessageBox.Show("Problem in Update/CCS/Pt0\n"+e.Message+"\n"+e.StackTrace);
#endif
                    Exit();
                    return;
                }
#endif
                //charNameSelected[i] < charNames[i < 3 ? i : 0].Length - 1 ? new Vector4(0f, 1f, 0f, 1f) : new Vector4(1f, 0f, 0f, 1f)
                if (mmenu_ticker <= 0)
                {
#if !DEBUG
                    try
                    {
#endif
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
                    
#if !DEBUG
                }
                catch(Exception e)
                {
#if WINDOWS
                    System.Windows.Forms.MessageBox.Show("Problem in Update/CCS/Pt1\n"+e.Message+"\n"+e.StackTrace);
#endif
                    Exit();
                    return;
                }
                    try
                    {
#endif


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
#if !DEBUG
                }
                catch(Exception e)
                {
                    
#if WINDOWS
                    System.Windows.Forms.MessageBox.Show("Problem in Update/CCS/Pt2\n"+e.Message+"\n"+e.StackTrace);
#endif
                    Exit();
                    return;
                }
                    try
                    {
#endif
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

                    for (int i = 1; i < contguis.Length; i++)
                        if (contguis[i].type == ContGUIData.CONT_TYPE.KEYBOARD)
                            contguis[i] = ContGUIData.INVALID;
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
                                spritebatch.Draw(nPadTex[(int)contguis[i].loc - 1], new Rectangle(0, 0, 256, 256), Color.White);
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
                            if ((((int)contguis[i].index == 4 && Keyboard.GetState().IsKeyDown(Keys.Back)) || ((int)contguis[i].index != 4 && controllers[(int)contguis[i].index].IsButtonDown(Buttons.B))) && contguis[i].status == 0)
                            { screen = S_MAINMENU; mmenu_ticker = 200; }
#if !DEBUG
                }
                catch(Exception e)
                {
#if WINDOWS
                    System.Windows.Forms.MessageBox.Show("Problem in Update/CCS/Pt3\n"+e.Message+"\n"+e.StackTrace);
#endif
                    Exit();
                    return;
                }
#endif
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
                if (GameLoaded)
                {
                    if (!IsPaused)
                    {
                        
#if !DEBUG
                    try
                    {
#endif
                        //start song TEMPORARY CODE? probably not...
                        if (started == 0)
                        {
                            CurrentTime = 5 * TicksPerSecond;
                            started = 1;
                            return;
                        }
                        else if (started == 1)
                        {
                            if ((long)CurrentTime - (long)(gameTime.ElapsedGameTime.TotalSeconds*TicksPerSecond) < 0)
                            {
                                CurrentTime = 0;
                                song.play();
                                started = 2;
                            }
                            else
                                CurrentTime -= (long)(gameTime.ElapsedGameTime.TotalSeconds * TicksPerSecond);

                            song.GetZVals(started<2?-(long)CurrentTime:(long)CurrentTime);
                            return;
                        }
                        long songt = song.getTime();
                        if (lastChange != songt && Math.Abs((float)(CurrentTime / (long)(TicksPerSecond / 1000)) - songt) > 50 && Math.Abs((float)(CurrentTime / (long)(TicksPerSecond / 1000)) - songt) < 5000)
                        {
                            CurrentTime = songt * (long)(TicksPerSecond / 1000);
                            lastChange = songt;
                        }
                        else if (lastChange != songt)
                            lastChange = songt;
                        CurrentTime += (gameTime.ElapsedGameTime.TotalSeconds * TicksPerSecond);
                        UpdateGibs(gameTime);

                        for (int i = 0; i < spcircles.Length; i++)
                        {
                            spcircles[i].rotation += spcircles[i].rotDir ? gameTime.ElapsedGameTime.Milliseconds / 1000f : -gameTime.ElapsedGameTime.Milliseconds / 1000f;
                            spcircles[i].alpha -= gameTime.ElapsedGameTime.Milliseconds / 2000f;
                            if (spcircles[i].alpha <= 0)
                            {
                                spcircles[i].alpha = (float)r.NextDouble();
                                spcircles[i].rotation = (float)r.NextDouble();
                                spcircles[i].pos = new Vector2((float)r.NextDouble(), (float)r.NextDouble());
                                spcircles[i].rotDir = r.Next() % 2 == 0;
                            }
                        }

                        
#if !DEBUG
                }
                catch(Exception e)
                {
#if WINDOWS
                    System.Windows.Forms.MessageBox.Show("Problem in Update/IG/Pt0\n"+e.Message+"\n"+e.StackTrace);
#endif
                    Exit();
                    return;
                }
                        try
                        {
#endif

                        long currenttime = (long)((CurrentTime) / (TicksPerSecond / 1000));

                        if (song.IsOver(currenttime))
                            screen = S_RESULTS;
                        if (renderLevel > 0)
                            venue.Update(gameTime, currenttime, engine, song);
                        matView = venue.GetViewMatrix();

                        song.Update(currenttime);

                        ProcessInput(gameTime, currenttime);

                        if (rockstarDir > Math.PI * 2)
                            rockstarDir = 0;
                        rockstarDir += 1f / gameTime.ElapsedGameTime.Milliseconds;
                        if (GetRockstarAmount() > lastStar && lastStar <= 5)
                        {
                            lastStar++;
                            audioSoundBank.PlayCue("starching");
                        }
                        bool allFail = true;
                        int anyFail = 0;
                        for (int i = 0; i < 4; i++)
                        {
                            if (failStatus[i] == FS_GOOD && rockMeterLevel[i] <= 0)
                            {
                                failStatus[i] = FS_FAILING;
                                if (failTime < -1)
                                    failTime = 1;
                            }
                            if (failStatus[i] == FS_GOOD)
                                allFail = false;
                            if (failStatus[i] == FS_FAILING)
                                anyFail++;
                        }
                        /*if (allFail)
                            screen = S_FAIL;
                        if (anyFail > 0)
                            failTime -= (gameTime.ElapsedGameTime.Milliseconds / 20000f) * anyFail;
                        else
                            failTime = -2;
                        if (failTime > -1 && failTime <= 0)
                            screen = S_FAIL;*/

                        if (instruments[0] && contInput[0] < 4)
                            boards[0].Whammy(controllers[contInput[0]].ThumbSticks.Right.X, currenttime,gameTime,song.GetBeatLength());
                        else if (instruments[0] && contInput[0] == 4)
                            boards[0].Whammy(Keyboard.GetState().IsKeyDown(Keys.Left) ? 1 : -1, currenttime, gameTime, song.GetBeatLength());
                        if (instruments[3] && contInput[3] < 4)
                            boards[3].Whammy(controllers[contInput[3]].ThumbSticks.Right.X, currenttime, gameTime, song.GetBeatLength());
                        else if (instruments[3] && contInput[3] == 4)
                            boards[3].Whammy(Keyboard.GetState().IsKeyDown(Keys.Left) ? 1 : -1, currenttime, gameTime, song.GetBeatLength());

                        song.GetZVals(started<2?-(long)currenttime:(long)currenttime);
                        for(int i=0;i<4;i++)
                            if (instruments[i] && i != 1)
                            {
                                boards[i].GetNotes((long)CurrentTime, (long)(Board.eFade * Game1.TicksPerSecond));
                                if (1 != 2)
                                    boards[i].getWaves((long)(started < 2 ? -(CurrentTime / (TicksPerSecond / 1000)) : (CurrentTime / (TicksPerSecond / 1000))));
                            }
                        for (int i = 0; i < 4; i++)
                        {
                            if (rockMeterLevel[i] < 0)
                            {
                                
                                rockMeterLevel[i] = 0;
                            }
                            else if (rockMeterLevel[i] > 100)
                                rockMeterLevel[i] = 100;
                        }
                        
#if !DEBUG
                }
                catch(Exception e)
                {
#if WINDOWS
                    System.Windows.Forms.MessageBox.Show("Problem in Update/IG/Pt1\n"+e.Message+"\n"+e.StackTrace);
#endif
                    Exit();
                    return;
                }
#endif
                    }
                    else
                    {
                        long currenttime = (long)(CurrentTime / (long)(TicksPerSecond / 1000));
                        ProcessInput(gameTime, currenttime);
                        pauseMenuPos += pauseMenuVel * (float)gameTime.ElapsedGameTime.TotalSeconds;
                        if (pauseMenuPos.X > (windowwidth * 0.6f))
                            pauseMenuVel.X = -Math.Abs(pauseMenuVel.X);
                        if (pauseMenuPos.X < (windowwidth * 0.4f))
                            pauseMenuVel.X = Math.Abs(pauseMenuVel.X);
                        pauseMenuVel.Y = pauseWingRot.Y;
                        pauseWingRot.X += (float)gameTime.ElapsedGameTime.TotalSeconds * pauseWingRot.Y * 0.1f;
                        if (pauseWingRot.Y < 0)
                            pauseRot += (pauseMenuVel.X * (float)gameTime.ElapsedGameTime.TotalSeconds * 0.01f) * (Math.Sign(pauseMenuVel.X) == Math.Sign(pauseRot) ? 1 : 3);
                        if (pauseWingRot.Y > 0 && pauseWingRot.X > MathHelper.Pi / 4)
                            pauseWingRot.Y = -160;
                        else if (pauseWingRot.Y < 0 && pauseWingRot.X < -MathHelper.Pi / 2)
                            pauseWingRot.Y = 30;
                    }
                }
            }
            #endregion
            #region freestyle
            else if (screen == S_FREESTYLE)
            {
                if (GameLoaded)
                {
#if !DEBUG
                    try
                    {
#endif
                    //CurrentTime += (long)gameTime.ElapsedGameTime.Ticks;
                    UpdateGibs(gameTime);

                    if (renderLevel > 0)
                    {
                        //venue.UpdateFS(gameTime, currenttime, engine, song);
                        matView = venue.GetViewMatrix();
                    }

                    ProcessInputFS();
#if !DEBUG
                }
                catch(Exception e)
                {
#if WINDOWS
                    System.Windows.Forms.MessageBox.Show("Problem in Update/FS/Pt0\n"+e.Message+"\n"+e.StackTrace);
#endif
                    Exit();
                    return;
                }
#endif

                    /*if (instruments[0] && contInput[0] < 4)
                        boards[0].Whammy(controllers[contInput[0]].ThumbSticks.Right.X, currenttime);
                    if (instruments[3] && contInput[3] < 4)
                        boards[3].Whammy(controllers[contInput[3]].ThumbSticks.Right.X, currenttime);*/
                }
            }
            #endregion
            #region results
            else if (screen == S_RESULTS)
            {
#if !DEBUG
                try
                {
#endif
                if (totalresults.Length == 0)
                {
                    totalresults = new Results[5];
                    for (int i = 0; i < 4; i++)
                        if (instruments[i])
                            totalresults[i] = boards[i].getResults();
                    totalresults[4] = new Results();
                    song.pause();
                    for (int i = 0; i < 4; i++)
                        if(instruments[i])
                        {
                            totalresults[4].hitNotes += totalresults[i].hitNotes;
                            totalresults[4].hitSPPH += totalresults[i].hitSPPH;
                            totalresults[4].missedNotes += totalresults[i].missedNotes;
                            totalresults[4].missedSPPH += totalresults[i].missedSPPH;
                            totalresults[4].totalNotes += totalresults[i].totalNotes;
                            totalresults[4].totalSPPH += totalresults[i].totalSPPH;
                        }
                }

                bool green=false;//what should red be used for?
                GetGamepadStates(false);
                    for (int i = 0; i < 4; i++)
                        if (controllers[i].IsConnected)
                        {
                            if (controllers[i].Buttons.A == ButtonState.Pressed)
                                green = true;
                        }
                    if (Keyboard.GetState().IsKeyDown(Keys.Enter) || Keyboard.GetState().IsKeyDown(Keys.Space) || Keyboard.GetState().IsKeyDown(Keys.A))
                        green = true;
                    if (green)
                    { totalresults = new Results[0]; screen = S_MAINMENU; songname = ""; song = null; boards = null; boardsTarget = null; started = 0; mmenu_ticker = 200; mmenu_select = 0; contguis = new ContGUIData[5]; UnloadGameContent(); MenuLoaded = false; }
                
#if !DEBUG
                }
                catch(Exception e)
                {
#if WINDOWS
                    System.Windows.Forms.MessageBox.Show("Problem in Update/R/Pt0\n"+e.Message+"\n"+e.StackTrace);
#endif
                    Exit();
                    return;
                }
#endif
            }
            #endregion
            #region fail
            else if (screen == S_FAIL)
            {
#if !DEBUG
                try
                {
#endif
                if (totalresults.Length == 0)
                {
                    totalresults = new Results[5];
                    for (int i = 0; i < 4; i++)
                        if (instruments[i])
                            totalresults[i] = boards[i].getResults();
                    totalresults[4] = new Results();
                    
                    for (int i = 0; i < 4; i++)
                        if(instruments[i])
                        {
                            totalresults[4].hitNotes += totalresults[i].hitNotes;
                            totalresults[4].hitSPPH += totalresults[i].hitSPPH;
                            totalresults[4].missedNotes += totalresults[i].missedNotes;
                            totalresults[4].missedSPPH += totalresults[i].missedSPPH;
                            totalresults[4].totalNotes += totalresults[i].totalNotes;
                            totalresults[4].totalSPPH += totalresults[i].totalSPPH;
                        }
                    totalresults[4].percentSong = song.PercentSong();
                    song.pause();
                }

                dialogscroll += gameTime.ElapsedGameTime.Milliseconds / 10000f;
                while (dialogscroll > 1)
                    dialogscroll -= 1;

                bool green=false, red=false;//what should red be used for?
                GetGamepadStates(false);
                    for (int i = 0; i < 4; i++)
                        if (controllers[i].IsConnected)
                        {
                            if (controllers[i].Buttons.A == ButtonState.Pressed)
                                green = true;
                            if (controllers[i].Buttons.B == ButtonState.Pressed)
                                red = true;
                        }
                    if (Keyboard.GetState().IsKeyDown(Keys.Enter) || Keyboard.GetState().IsKeyDown(Keys.Space) || Keyboard.GetState().IsKeyDown(Keys.A))
                        green = true;
                    if (Keyboard.GetState().IsKeyDown(Keys.Back) || Keyboard.GetState().IsKeyDown(Keys.Escape))
                        red = true;
                    if (red)
                    { totalresults = new Results[0]; screen = S_MAINMENU; songname = ""; song = null; boards = null; boardsTarget = null; started = 0; mmenu_ticker = 200; mmenu_select = 0; contguis = new ContGUIData[5]; UnloadGameContent(); MenuLoaded = false; }
                    if(green)
                    { totalresults = new Results[0]; screen = S_INGAME; CurrentTime = -5000; song = null; boards = null; boardsTarget = null; started = 0; UnloadGameContent(); LoadGameContent(); }
                
#if !DEBUG
                }
                catch(Exception e)
                {
#if WINDOWS
                    System.Windows.Forms.MessageBox.Show("Problem in Update/F/Pt0\n"+e.Message+"\n"+e.StackTrace);
#endif
                    Exit();
                    return;
                }
#endif
            }
            #endregion
            else
            {
            }

            base.Update(gameTime);
        }

        private void FillSongDiffs(string songname)
        {
            
        }

        protected override void Draw(GameTime gameTime)
        {
                #region loadscreen
            if (GameLoading)
            {
                graphics.GraphicsDevice.Clear(Color.Black);

                graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
                graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
                //graphics.PreferMultiSampling = true;
                graphics.ApplyChanges();

                vd = new VertexDeclaration(graphics.GraphicsDevice, GBVertexFormat.Elements);
                graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;
                graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
                graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;

                bEngine.DiffuseColor = new Vector3(1f, 1f, 1f);
                bEngine.DirectionalLight0.DiffuseColor = new Vector3(0.8f, 0.8f, 0.8f);
                bEngine.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(-1, -3, -1));
                bEngine.DirectionalLight0.Enabled = true;
                bEngine.DirectionalLight0.SpecularColor = new Vector3(1.0f, 1.0f, 1.0f);
                bEngine.LightingEnabled = true;
                bEngine.SpecularColor = new Vector3(0, 0, 0);
                bEngine.SpecularPower = 12.0f;
                bEngine.TextureEnabled = true;
                bEngine.CommitChanges();
                bEngine.Begin();
                foreach (EffectPass pass in bEngine.CurrentTechnique.Passes)
                {
                    pass.Begin();

                    matProj = Matrix.CreatePerspectiveFieldOfView((float)Math.PI / 4.0f,
                          windowwidth / (float)windowheight,
                          1f, 40.0f);

                    matView = Matrix.CreateLookAt(new Vector3(0,0,7), new Vector3(0,0,0), new Vector3(0, 1, 0));
                    //render the background graphics
                    bEngine.View = matView;
                    bEngine.Projection = matProj;

                    Matrix matRot, matScale, matTranslate;
                    matTranslate = Matrix.CreateTranslation(-2, 0, 0);
                    matRot = Matrix.Identity;// Matrix.CreateRotationX((float)Math.PI);
                    matScale = Matrix.CreateScale(1, 2, 1);

                    bEngine.World = matScale * matRot * matTranslate;
                    bEngine.Texture = texCurtainLeft;
                    bEngine.CommitChanges();

                    foreach (ModelMesh mesh in mCurtain.Meshes)
                    {
                        foreach (ModelMeshPart meshpart in mesh.MeshParts)
                        {
                            graphics.GraphicsDevice.VertexDeclaration = meshpart.VertexDeclaration;
                            graphics.GraphicsDevice.Vertices[0].SetSource(mesh.VertexBuffer, meshpart.StreamOffset, meshpart.VertexStride);
                            graphics.GraphicsDevice.Indices = mesh.IndexBuffer;
                            graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, meshpart.BaseVertex, 0, meshpart.NumVertices, meshpart.StartIndex, meshpart.PrimitiveCount);
                        }
                    }

                    matTranslate = Matrix.CreateTranslation(2, 0, 0);
                    matRot = Matrix.Identity;// Matrix.CreateRotationX((float)Math.PI);
                    matScale = Matrix.CreateScale(1, 2, 1);

                    bEngine.World = matScale * matRot * matTranslate;
                    bEngine.Texture = texCurtainRight;
                    bEngine.CommitChanges();

                    foreach (ModelMesh mesh in mCurtain.Meshes)
                    {
                        foreach (ModelMeshPart meshpart in mesh.MeshParts)
                        {
                            graphics.GraphicsDevice.VertexDeclaration = meshpart.VertexDeclaration;
                            graphics.GraphicsDevice.Vertices[0].SetSource(mesh.VertexBuffer, meshpart.StreamOffset, meshpart.VertexStride);
                            graphics.GraphicsDevice.Indices = mesh.IndexBuffer;
                            graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, meshpart.BaseVertex, 0, meshpart.NumVertices, meshpart.StartIndex, meshpart.PrimitiveCount);
                        }
                    }
                    pass.End();
                }
                bEngine.End();
            }
            else
            {
                #endregion
                #region fvlogo
                if (screen == S_FVLOGO)
                {
                    graphics.GraphicsDevice.Clear(Color.Black);

                    
#if !DEBUG

                    try
                    {
#endif
                    

                    graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
                    graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
                    //graphics.PreferMultiSampling = true;
                    graphics.ApplyChanges();

                    vd = new VertexDeclaration(graphics.GraphicsDevice, GBVertexFormat.Elements);
                    graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;
                    graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
                    graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;

                    bEngine.DiffuseColor = new Vector3(1f, 1f, 1f);
                    bEngine.DirectionalLight0.DiffuseColor = new Vector3(0.8f, 0.8f, 0.8f);
                    bEngine.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(-1, -3, -1));
                    bEngine.DirectionalLight0.Enabled = true;
                    bEngine.DirectionalLight0.SpecularColor = new Vector3(1.0f, 1.0f, 1.0f);
                    bEngine.LightingEnabled = true;
                    bEngine.SpecularColor = new Vector3(1.0f, 1.0f, 1.0f);
                    bEngine.SpecularPower = 12.0f;
                    bEngine.TextureEnabled = true;
                    bEngine.CommitChanges();
#if !DEBUG
                    }
                    catch(Exception e)
                    {
#if WINDOWS
                        System.Windows.Forms.MessageBox.Show("Problem in Draw/MM/Pt1\n"+e.Message+"\n"+e.StackTrace);
#endif
                        Exit();
                        return;
                    }

                    try
                    {
#endif
                    bEngine.Begin();
                    foreach (EffectPass pass in bEngine.CurrentTechnique.Passes)
                    {
                        pass.Begin();
                        matProj = Matrix.CreatePerspectiveFieldOfView((float)Math.PI / 4.0f,
                          windowwidth / (float)windowheight,
                          1f, 40.0f);
                        Vector3 camPos = new Vector3(20, 8, 0);
                        if (logoTime < 20)
                            camPos.X = 20;
                        else if (logoTime < 30)
                            camPos.X = ((((logoTime - 20) / 10f)) * 12) + ((1 - ((logoTime - 20) / 10f)) * 20);
                        else
                            camPos.X = 12;
                        if (logoTime < 20)
                            camPos.Y = 8;
                        else if (logoTime < 30)
                            camPos.Y = ((((logoTime - 20) / 10f)) * 12) + ((1 - ((logoTime - 20) / 10f)) * 8);
                        else
                            camPos.Y = 12;

                        camPos = Vector3.Transform(camPos, Matrix.CreateRotationY(logoTime<30?(float)(Math.PI * 3 / 4f) + (float)((logoTime / 30f) * (Math.PI * 3 / 4f)):(float)(Math.PI*1.5f)));

                        Vector3 target = new Vector3(0, 0, 0);

                        if (logoTime < 20)
                            camPos.X -= 0;
                        else if (logoTime < 30)
                        { target.X -= ((((logoTime - 20) / 10f)) * 1); camPos.X -= ((((logoTime - 20) / 10f)) * 1); }
                        else
                        { target.X -= 1; camPos.X -= 1; }

                        matView = Matrix.CreateLookAt(camPos, target, new Vector3(0, 1, 0));
                        //render the background graphics
                        bEngine.View = matView;
                        bEngine.Projection = matProj;

                        Matrix matRot, matScale, matTranslate;
                        {//goo1
                            matTranslate = Matrix.CreateTranslation(0, 0, 0);
                            matRot = Matrix.Identity;// Matrix.CreateRotationX((float)Math.PI);
                            matScale = Matrix.CreateScale(1, 1, 1);

                            bEngine.World = matScale * matRot * matTranslate;
                            if (logoTime < 17)
                                bEngine.Texture = texGoo1;
                            else
                                bEngine.Texture = texGoo2;
                            bEngine.CommitChanges();

                            graphics.GraphicsDevice.VertexDeclaration = vd;
                            if(logoTime<17)
                                foreach (ModelMesh mesh in mGoo1.Meshes)
                                {
                                    foreach (ModelMeshPart meshpart in mesh.MeshParts)
                                    {
                                        graphics.GraphicsDevice.VertexDeclaration = meshpart.VertexDeclaration;
                                        graphics.GraphicsDevice.Vertices[0].SetSource(mesh.VertexBuffer, meshpart.StreamOffset, meshpart.VertexStride);
                                        graphics.GraphicsDevice.Indices = mesh.IndexBuffer;
                                        graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, meshpart.BaseVertex, 0, meshpart.NumVertices, meshpart.StartIndex, meshpart.PrimitiveCount);
                                    }
                                }
                            else
                                foreach (ModelMesh mesh in mGoo2.Meshes)
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
                        {//barrel
                            matTranslate = Matrix.CreateTranslation(-10.2f, 0, -6.8f);
                            matRot = Matrix.CreateRotationY((float)Math.PI*1.25f);
                            matScale = Matrix.CreateScale(1, 1, 1);

                            bEngine.World = matScale * matRot * matTranslate;
                            bEngine.Texture = texBarrel;
                            bEngine.CommitChanges();

                            graphics.GraphicsDevice.VertexDeclaration = vd;
                            foreach (ModelMesh mesh in mBarrel.Meshes)
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
                        {//presser
                            float y = 30;
                            if (logoTime > 5 && logoTime < 15)
                                y = (10 - (logoTime - 5)) * 3;
                            else if (logoTime >= 15 && logoTime < 20)
                                y = 0;
                            else if (logoTime >= 20 && logoTime < 30)
                                y = (logoTime - 20) * 3;
                            matTranslate = Matrix.CreateTranslation(0, y, 0);
                            matRot = Matrix.Identity;//Matrix.CreateRotationY((float)Math.PI * 1.25f);
                            matScale = Matrix.CreateScale(1, 1, 1);

                            bEngine.World = matScale * matRot * matTranslate;
                            bEngine.Texture = texPresser;
                            bEngine.CommitChanges();

                            graphics.GraphicsDevice.VertexDeclaration = vd;
                            foreach (ModelMesh mesh in mPresser.Meshes)
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
                        pass.End();
                    }
                    bEngine.End();

                    spritebatch.Begin();
                    if(logoTime<5)
                        spritebatch.Draw(texWhite, new Rectangle(0, 0, windowwidth, windowheight), new Color(0, 0, 0, (byte)(255 * (1-((logoTime ) / 5f)))));
                    if (logoTime > 30 && logoTime<35)
                        spritebatch.Draw(texWhite, new Rectangle(0, 0, windowwidth, windowheight), new Color(0, 0, 0, (byte)(255 * ( ((logoTime - 30) / 5f)))));
                    else if(logoTime>=35)
                        spritebatch.Draw(texWhite, new Rectangle(0, 0, windowwidth, windowheight), Color.Black);
                    spritebatch.End();
#if !DEBUG
                    }
                    catch(Exception e)
                    {
#if WINDOWS
                        System.Windows.Forms.MessageBox.Show("Problem in Draw/MM/Pt1\n"+e.Message+"\n"+e.StackTrace);
#endif
                        Exit();
                        return;
                    }

#endif
                }
                #endregion
                #region mainmenu
                else if (screen == S_MAINMENU)
                {
                    graphics.GraphicsDevice.Clear(Color.Black);

                    int linum = 0;
                    bool[] plo = new bool[16];
                    Vector3[] plp = new Vector3[16];
                    float[] pln = new float[16];
                    float[] plf = new float[16];
                    Vector3[] pld = new Vector3[16];
                    Vector3[] pls = new Vector3[16];
#if !DEBUG
                    try
                    {
#endif
                    Color txt = Color.Black;
                    
                    graphics.GraphicsDevice.SetRenderTarget(0, rtSnake);
                    graphics.GraphicsDevice.Clear(Color.Red);
                    spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);
                    //29.589 69.727
                    float xscale=128, yscale=48;
                    float rotxscale = 128, rotyscale = 48;
                    for (int i = -1; i < 5; i++)
                    for (int k = -1;k < 4; k++)
                    {
                        spritebatch.Draw(texSnakeSkin,new Rectangle((int)(((menuShiftPos.X*2)*rotxscale)+(i*xscale)),(int)((menuShiftPos.Y*rotyscale)+(k*yscale)),(int)(xscale),(int)(yscale)),Color.White);
                    }
                    /*if (menuShiftPos.X > 0.004)
                    {
                        spritebatch.Draw(texSnakeSkin, new Rectangle((int)(rtSnake.Width*menuShiftPos.X), (int)(rtSnake.Height * (.29589/3) * (menuShiftPos.Y+1+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (.29589))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.2f);
                        spritebatch.Draw(texSnakeSkin, new Rectangle((int)(rtSnake.Width*menuShiftPos.X), (int)(rtSnake.Height * .29589)+(int)(rtSnake.Height * ((.69727 - .29589)/3) * (menuShiftPos.Y+1+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (.69727 - .29589))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.2f);
                        spritebatch.Draw(texSnakeSkin, new Rectangle((int)(rtSnake.Width*menuShiftPos.X), (int)(rtSnake.Height * .69727)+(int)(rtSnake.Height * ((1 - .69727)/3) * (menuShiftPos.Y+1+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (1 - .69727))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.2f);

                        spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * (.29589/3) * (menuShiftPos.Y+1+(menuSnakeRotOffset%3))), (int)(rtSnake.Width*menuShiftPos.X), (int)(rtSnake.Height * (.29589))),new Rectangle((int)(texSnakeSkin.Width*(1-menuShiftPos.X)),0,texSnakeSkin.Width-(int)(texSnakeSkin.Width*(1-menuShiftPos.X)),texSnakeSkin.Height), Color.White,0,new Vector2(0,0),SpriteEffects.None,0.2f);
                        spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * .29589)+(int)(rtSnake.Height * ((.69727 - .29589)/3) * (menuShiftPos.Y+1+(menuSnakeRotOffset%3))), (int)(rtSnake.Width*menuShiftPos.X), (int)(rtSnake.Height * (.69727 - .29589))),new Rectangle((int)(texSnakeSkin.Width*(1-menuShiftPos.X)),0,texSnakeSkin.Width-(int)(texSnakeSkin.Width*(1-menuShiftPos.X)),texSnakeSkin.Height), Color.White,0,new Vector2(0,0),SpriteEffects.None,0.2f);
                        spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * .69727)+(int)(rtSnake.Height * ((1 - .69727)/3) * (menuShiftPos.Y+1+(menuSnakeRotOffset%3))), (int)(rtSnake.Width*menuShiftPos.X), (int)(rtSnake.Height * (1 - .69727))),new Rectangle((int)(texSnakeSkin.Width*(1-menuShiftPos.X)),0,texSnakeSkin.Width-(int)(texSnakeSkin.Width*(1-menuShiftPos.X)),texSnakeSkin.Height), Color.White,0,new Vector2(0,0),SpriteEffects.None,0.2f);
                    }
                    else if (menuShiftPos.X < -0.004)
                    {
                        menuShiftPos.X = -menuShiftPos.X;
                        spritebatch.Draw(texSnakeSkin, new Rectangle((int)(rtSnake.Width*(1-menuShiftPos.X)), (int)(rtSnake.Height * (.29589/3) * (menuShiftPos.Y+2+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (.29589))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.3f);
                        spritebatch.Draw(texSnakeSkin, new Rectangle((int)(rtSnake.Width*(1-menuShiftPos.X)), (int)(rtSnake.Height * .29589)+(int)(rtSnake.Height * ((.69727 - .29589)/3) * (menuShiftPos.Y+2+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (.69727 - .29589))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.3f);
                        spritebatch.Draw(texSnakeSkin, new Rectangle((int)(rtSnake.Width*(1-menuShiftPos.X)), (int)(rtSnake.Height * .69727)+(int)(rtSnake.Height * ((1 - .69727)/3) * (menuShiftPos.Y+2+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (1 - .69727))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.3f);

                        spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * (.29589/3) * (menuShiftPos.Y+2+(menuSnakeRotOffset%3))), (int)(rtSnake.Width*(1-menuShiftPos.X)), (int)(rtSnake.Height * (.29589))),new Rectangle((int)(texSnakeSkin.Width*(menuShiftPos.X)),0,texSnakeSkin.Width-(int)(texSnakeSkin.Width*(menuShiftPos.X)),texSnakeSkin.Height), Color.White,0,new Vector2(0,0),SpriteEffects.None,0.3f);
                        spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * .29589)+(int)(rtSnake.Height * ((.69727 - .29589)/3) * (menuShiftPos.Y+2+(menuSnakeRotOffset%3))), (int)(rtSnake.Width*(1-menuShiftPos.X)), (int)(rtSnake.Height * (.69727 - .29589))),new Rectangle((int)(texSnakeSkin.Width*(menuShiftPos.X)),0,texSnakeSkin.Width-(int)(texSnakeSkin.Width*(menuShiftPos.X)),texSnakeSkin.Height), Color.White,0,new Vector2(0,0),SpriteEffects.None,0.3f);
                        spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * .69727)+(int)(rtSnake.Height * ((1 - .69727)/3) * (menuShiftPos.Y+2+(menuSnakeRotOffset%3))), (int)(rtSnake.Width*(1-menuShiftPos.X)), (int)(rtSnake.Height * (1 - .69727))),new Rectangle((int)(texSnakeSkin.Width*(menuShiftPos.X)),0,texSnakeSkin.Width-(int)(texSnakeSkin.Width*(menuShiftPos.X)),texSnakeSkin.Height), Color.White,0,new Vector2(0,0),SpriteEffects.None,0.3f);

                        spritebatch.Draw(texSnakeSkin, new Rectangle((int)(rtSnake.Width*(1-menuShiftPos.X)), (int)(rtSnake.Height * (.29589/3) * (menuShiftPos.Y+1+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (.29589))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.2f);
                        spritebatch.Draw(texSnakeSkin, new Rectangle((int)(rtSnake.Width*(1-menuShiftPos.X)), (int)(rtSnake.Height * .29589)+(int)(rtSnake.Height * ((.69727 - .29589)/3) * (menuShiftPos.Y+1+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (.69727 - .29589))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.2f);
                        spritebatch.Draw(texSnakeSkin, new Rectangle((int)(rtSnake.Width*(1-menuShiftPos.X)), (int)(rtSnake.Height * .69727)+(int)(rtSnake.Height * ((1 - .69727)/3) * (menuShiftPos.Y+1+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (1 - .69727))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.2f);

                        spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * (.29589/3) * (menuShiftPos.Y+1+(menuSnakeRotOffset%3))), (int)(rtSnake.Width*(1-menuShiftPos.X)), (int)(rtSnake.Height * (.29589))),new Rectangle((int)(texSnakeSkin.Width*(menuShiftPos.X)),0,texSnakeSkin.Width-(int)(texSnakeSkin.Width*(menuShiftPos.X)),texSnakeSkin.Height), Color.White,0,new Vector2(0,0),SpriteEffects.None,0.2f);
                        spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * .29589)+(int)(rtSnake.Height * ((.69727 - .29589)/3) * (menuShiftPos.Y+1+(menuSnakeRotOffset%3))), (int)(rtSnake.Width*(1-menuShiftPos.X)), (int)(rtSnake.Height * (.69727 - .29589))),new Rectangle((int)(texSnakeSkin.Width*(menuShiftPos.X)),0,texSnakeSkin.Width-(int)(texSnakeSkin.Width*(menuShiftPos.X)),texSnakeSkin.Height), Color.White,0,new Vector2(0,0),SpriteEffects.None,0.2f);
                        spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * .69727)+(int)(rtSnake.Height * ((1 - .69727)/3) * (menuShiftPos.Y+1+(menuSnakeRotOffset%3))), (int)(rtSnake.Width*(1-menuShiftPos.X)), (int)(rtSnake.Height * (1 - .69727))),new Rectangle((int)(texSnakeSkin.Width*(menuShiftPos.X)),0,texSnakeSkin.Width-(int)(texSnakeSkin.Width*(menuShiftPos.X)),texSnakeSkin.Height), Color.White,0,new Vector2(0,0),SpriteEffects.None,0.2f);
                        menuShiftPos.X = -menuShiftPos.X;
                    }
                    else if (menuShiftPos.Y >= 0)
                    {
                        spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * (.29589/3) * (menuShiftPos.Y-1+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (.29589))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.2f);
                        spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * (.29589/3) * (menuShiftPos.Y+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (.29589))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.1f);
                        spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * (.29589/3) * (menuShiftPos.Y+1+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (.29589))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.2f);
                        spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * (.29589/3) * (menuShiftPos.Y+2+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (.29589))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.3f);

                        spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * .29589)+(int)(rtSnake.Height * ((.69727 - .29589)/3) * (menuShiftPos.Y-1+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (.69727 - .29589))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.2f);
                        spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * .29589)+(int)(rtSnake.Height * ((.69727 - .29589)/3) * (menuShiftPos.Y+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (.69727 - .29589))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.1f);
                        spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * .29589)+(int)(rtSnake.Height * ((.69727 - .29589)/3) * (menuShiftPos.Y+1+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (.69727 - .29589))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.2f);
                        spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * .29589)+(int)(rtSnake.Height * ((.69727 - .29589)/3) * (menuShiftPos.Y+2+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (.69727 - .29589))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.3f);
                        
                        spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * .69727)+(int)(rtSnake.Height * ((1 - .69727)/3) * (menuShiftPos.Y-1+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (1 - .69727))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.2f);
                        spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * .69727)+(int)(rtSnake.Height * ((1 - .69727)/3) * (menuShiftPos.Y+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (1 - .69727))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.1f);
                        spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * .69727)+(int)(rtSnake.Height * ((1 - .69727)/3) * (menuShiftPos.Y+1+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (1 - .69727))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.2f);
                        spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * .69727)+(int)(rtSnake.Height * ((1 - .69727)/3) * (menuShiftPos.Y+2+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (1 - .69727))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.3f);
                    }
                    else
                    {
                        spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * (.29589/3) * (menuShiftPos.Y-2+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (.29589))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.3f);
                        spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * (.29589/3) * (menuShiftPos.Y-1+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (.29589))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.2f);
                        spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * (.29589/3) * (menuShiftPos.Y+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (.29589))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.1f);
                        spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * (.29589/3) * (menuShiftPos.Y+1+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (.29589))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.2f);
                        spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * (.29589/3) * (menuShiftPos.Y+2+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (.29589))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.3f);

                        spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * .29589)+(int)(rtSnake.Height * ((.69727 - .29589)/3) * (menuShiftPos.Y-2+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (.69727 - .29589))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.3f);
                        spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * .29589)+(int)(rtSnake.Height * ((.69727 - .29589)/3) * (menuShiftPos.Y-1+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (.69727 - .29589))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.2f);
                        spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * .29589)+(int)(rtSnake.Height * ((.69727 - .29589)/3) * (menuShiftPos.Y+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (.69727 - .29589))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.1f);
                        spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * .29589)+(int)(rtSnake.Height * ((.69727 - .29589)/3) * (menuShiftPos.Y+1+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (.69727 - .29589))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.2f);
                        spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * .29589)+(int)(rtSnake.Height * ((.69727 - .29589)/3) * (menuShiftPos.Y+2+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (.69727 - .29589))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.3f);
                        
                        spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * .69727)+(int)(rtSnake.Height * ((1 - .69727)/3) * (menuShiftPos.Y-2+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (1 - .69727))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.3f);
                        spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * .69727)+(int)(rtSnake.Height * ((1 - .69727)/3) * (menuShiftPos.Y-1+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (1 - .69727))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.2f);
                        spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * .69727)+(int)(rtSnake.Height * ((1 - .69727)/3) * (menuShiftPos.Y+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (1 - .69727))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.1f);
                        spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * .69727)+(int)(rtSnake.Height * ((1 - .69727)/3) * (menuShiftPos.Y+1+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (1 - .69727))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.2f);
                        spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * .69727)+(int)(rtSnake.Height * ((1 - .69727)/3) * (menuShiftPos.Y+2+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (1 - .69727))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.3f);
                        spritebatch.Draw(texSnakeSkin, new Rectangle(0, (int)(rtSnake.Height * .69727)+(int)(rtSnake.Height * ((1 - .69727)/3) * (menuShiftPos.Y+3+(menuSnakeRotOffset%3))), rtSnake.Width, (int)(rtSnake.Height * (1 - .69727))),null, Color.White,0,new Vector2(0,0),SpriteEffects.None,0.4f);
                    }*/
                    if (Math.Abs(menuShiftPos.X) <= 0.004)
                    {
                        if (mmenu_select % 10 == 0)
                        {
                            for (int i = 0; i < mMenuStr[0].Length; i++)
                            {
                                Vector2 sz = DefaultFont.MeasureString("" + mMenuStr[0][i]);
                                spritebatch.DrawString(DefaultFont, "" + mMenuStr[0][i], new Vector2((rtSnake.Width / 2)-(sz.X/2), (rtSnake.Height / 2) - (sz.Y/2) + (yscale * (i+menuShiftPos.Y-(mmenu_select/10)+1))), mMenuCol[0][i]);
                            }
                            /*if (menuShiftPos.Y >= 0)
                            {
                                Vector2 meas = sfMenu.MeasureString(mMenuStr[0][Math.Max(0, mmenu_select / 10 - 1)]);
                                spritebatch.DrawString(sfMenu, mMenuStr[0][Math.Max(0, mmenu_select / 10 - 1)], new Vector2(rtSnake.Width / 2 - (meas.X / 2), ((1 - menuShiftPos.Y) * (rtSnake.Height * (((.69727f - .29589f) / 2) + .29589f) - (meas.Y / 2))) + (menuShiftPos.Y * (rtSnake.Height * (.69727f) - (meas.Y)))), mMenuCol[0][Math.Max(0, mmenu_select / 10 - 1)]);
                                if (mmenu_select / 10 - 2 > 0)
                                {
                                    meas = sfMenu.MeasureString(mMenuStr[0][Math.Max(0, mmenu_select / 10 - 3)]);
                                    spritebatch.DrawString(sfMenu, mMenuStr[0][Math.Max(0, mmenu_select / 10 - 3)], new Vector2(rtSnake.Width / 2 - (meas.X / 2), (rtSnake.Height * (.29589f))), mMenuCol[0][Math.Max(0, mmenu_select / 10 - 3)], 0, new Vector2(0, 0), new Vector2(1, menuShiftPos.Y), SpriteEffects.None, 0);
                                }
                                if (mmenu_select / 10 - 1 > 0)
                                {
                                    meas = sfMenu.MeasureString(mMenuStr[0][Math.Max(0, mmenu_select / 10 - 2)]);
                                    spritebatch.DrawString(sfMenu, mMenuStr[0][Math.Max(0, mmenu_select / 10 - 2)], new Vector2(rtSnake.Width / 2 - (meas.X / 2), ((1 - menuShiftPos.Y) * (rtSnake.Height * (.29589f))) + (menuShiftPos.Y * (rtSnake.Height * (((.69727f - .29589f) / 2) + .29589f) - (meas.Y / 2)))), mMenuCol[0][Math.Max(0, mmenu_select / 10 - 2)]);
                                }
                                if (mmenu_select / 10 - 1 < mMenuStr[0].Length - 1)
                                {
                                    meas = sfMenu.MeasureString(mMenuStr[0][Math.Max(0, mmenu_select / 10)]);
                                    spritebatch.DrawString(sfMenu, mMenuStr[0][Math.Max(0, mmenu_select / 10)], new Vector2(rtSnake.Width / 2 - (meas.X / 2), (rtSnake.Height * (.69727f))), mMenuCol[0][Math.Max(0, mmenu_select / 10)], 0, new Vector2(0, meas.Y), new Vector2(1, 1 - menuShiftPos.Y), SpriteEffects.None, 0);
                                }
                            }
                            else
                            {
                                menuShiftPos.Y = -menuShiftPos.Y;
                                Vector2 meas = sfMenu.MeasureString(mMenuStr[0][Math.Max(0, mmenu_select / 10 - 1)]);
                                spritebatch.DrawString(sfMenu, mMenuStr[0][Math.Max(0, mmenu_select / 10 - 1)], new Vector2(rtSnake.Width / 2 - (meas.X / 2), ((1 - menuShiftPos.Y) * (rtSnake.Height * (((.69727f - .29589f) / 2) + .29589f) - (meas.Y / 2))) + (menuShiftPos.Y * (rtSnake.Height * (.29589f)))), mMenuCol[0][Math.Max(0, mmenu_select / 10 - 1)]);
                                if (mmenu_select / 10 - 1 > 0)
                                {
                                    meas = sfMenu.MeasureString(mMenuStr[0][Math.Max(0, mmenu_select / 10 - 2)]);
                                    spritebatch.DrawString(sfMenu, mMenuStr[0][Math.Max(0, mmenu_select / 10 - 2)], new Vector2(rtSnake.Width / 2 - (meas.X / 2), (rtSnake.Height * (.29589f))), mMenuCol[0][Math.Max(0, mmenu_select / 10 - 2)], 0, new Vector2(0, 0), new Vector2(1, 1 - menuShiftPos.Y), SpriteEffects.None, 0);
                                }
                                if (mmenu_select / 10 - 1 < mMenuStr[0].Length - 1)
                                {
                                    meas = sfMenu.MeasureString(mMenuStr[0][Math.Max(0, mmenu_select / 10)]);
                                    spritebatch.DrawString(sfMenu, mMenuStr[0][Math.Max(0, mmenu_select / 10)], new Vector2(rtSnake.Width / 2 - (meas.X / 2), ((1 - menuShiftPos.Y) * (rtSnake.Height * (.69727f) - (meas.Y))) + (menuShiftPos.Y * (rtSnake.Height * (((.69727f - .29589f) / 2) + .29589f) - (meas.Y / 2)))), mMenuCol[0][Math.Max(0, mmenu_select / 10)]);
                                }
                                if (mmenu_select / 10 - 1 < mMenuStr[0].Length - 2)
                                {
                                    meas = sfMenu.MeasureString(mMenuStr[0][Math.Max(0, mmenu_select / 10 + 1)]);
                                    spritebatch.DrawString(sfMenu, mMenuStr[0][Math.Max(0, mmenu_select / 10 + 1)], new Vector2(rtSnake.Width / 2 - (meas.X / 2), (rtSnake.Height * (.69727f))), mMenuCol[0][Math.Max(0, mmenu_select / 10 + 1)], 0, new Vector2(0, meas.Y), new Vector2(1, menuShiftPos.Y), SpriteEffects.None, 0);
                                }
                                menuShiftPos.Y = -menuShiftPos.Y;
                            }*/
                        }
                        else
                        {
                            for (int i = 0; i < mMenuStr[mmenu_select/10].Length; i++)
                            {
                                Vector2 sz = DefaultFont.MeasureString("" + mMenuStr[mmenu_select/10][i]);
                                spritebatch.DrawString(DefaultFont, "" + mMenuStr[mmenu_select / 10][i], new Vector2((rtSnake.Width / 2) - (sz.X / 2), (rtSnake.Height / 2) - (sz.Y / 2) + (yscale * (i + menuShiftPos.Y - (mmenu_select % 10) + 1))), mMenuCol[mmenu_select / 10][i]);
                            }
                            /*if (menuShiftPos.Y >= 0)
                            {
                                Vector2 meas = sfMenu.MeasureString(mMenuStr[mmenu_select / 10][Math.Max(0, mmenu_select % 10 - 1)]);
                                spritebatch.DrawString(sfMenu, mMenuStr[mmenu_select / 10][Math.Max(0, mmenu_select % 10 - 1)], new Vector2(rtSnake.Width / 2 - (meas.X / 2), ((1 - menuShiftPos.Y) * (rtSnake.Height * (((.69727f - .29589f) / 2) + .29589f) - (meas.Y / 2))) + (menuShiftPos.Y * (rtSnake.Height * (.69727f) - (meas.Y)))), mMenuCol[mmenu_select / 10][Math.Max(0, mmenu_select % 10 - 1)]);
                                if (mmenu_select % 10 - 2 > 0)
                                {
                                    meas = sfMenu.MeasureString(mMenuStr[mmenu_select / 10][Math.Max(0, mmenu_select % 10 - 3)]);
                                    spritebatch.DrawString(sfMenu, mMenuStr[mmenu_select / 10][Math.Max(0, mmenu_select / 10 - 3)], new Vector2(rtSnake.Width / 2 - (meas.X / 2), (rtSnake.Height * (.29589f))), mMenuCol[mmenu_select / 10][Math.Max(0, mmenu_select / 10 - 3)], 0, new Vector2(0, 0), new Vector2(1, menuShiftPos.Y), SpriteEffects.None, 0);
                                }
                                if (mmenu_select % 10 - 1 > 0)
                                {
                                    meas = sfMenu.MeasureString(mMenuStr[mmenu_select / 10][Math.Max(0, mmenu_select % 10 - 2)]);
                                    spritebatch.DrawString(sfMenu, mMenuStr[mmenu_select / 10][Math.Max(0, mmenu_select % 10 - 2)], new Vector2(rtSnake.Width / 2 - (meas.X / 2), ((1 - menuShiftPos.Y) * (rtSnake.Height * (.29589f))) + (menuShiftPos.Y * (rtSnake.Height * (((.69727f - .29589f) / 2) + .29589f) - (meas.Y / 2)))), mMenuCol[mmenu_select / 10][Math.Max(0, mmenu_select % 10 - 2)]);
                                }
                                if (mmenu_select % 10 - 1 < mMenuStr[mmenu_select / 10].Length - 1)
                                {
                                    meas = sfMenu.MeasureString(mMenuStr[mmenu_select / 10][Math.Max(0, mmenu_select % 10)]);
                                    spritebatch.DrawString(sfMenu, mMenuStr[mmenu_select / 10][Math.Max(0, mmenu_select % 10)], new Vector2(rtSnake.Width / 2 - (meas.X / 2), (rtSnake.Height * (.69727f))), mMenuCol[mmenu_select / 10][Math.Max(0, mmenu_select % 10)], 0, new Vector2(0, meas.Y), new Vector2(1, 1 - menuShiftPos.Y), SpriteEffects.None, 0);
                                }
                            }
                            else
                            {
                                menuShiftPos.Y = -menuShiftPos.Y;
                                Vector2 meas = sfMenu.MeasureString(mMenuStr[mmenu_select / 10][Math.Max(0, mmenu_select % 10 - 1)]);
                                spritebatch.DrawString(sfMenu, mMenuStr[mmenu_select / 10][Math.Max(0, mmenu_select % 10 - 1)], new Vector2(rtSnake.Width / 2 - (meas.X / 2), ((1 - menuShiftPos.Y) * (rtSnake.Height * (((.69727f - .29589f) / 2) + .29589f) - (meas.Y / 2))) + (menuShiftPos.Y * (rtSnake.Height * (.29589f)))), mMenuCol[mmenu_select / 10][Math.Max(0, mmenu_select % 10 - 1)]);
                                if (mmenu_select % 10 - 1 > 0)
                                {
                                    meas = sfMenu.MeasureString(mMenuStr[mmenu_select / 10][Math.Max(0, mmenu_select % 10 - 2)]);
                                    spritebatch.DrawString(sfMenu, mMenuStr[mmenu_select / 10][Math.Max(0, mmenu_select % 10 - 2)], new Vector2(rtSnake.Width / 2 - (meas.X / 2), (rtSnake.Height * (.29589f))), mMenuCol[mmenu_select / 10][Math.Max(0, mmenu_select % 10 - 2)], 0, new Vector2(0, 0), new Vector2(1, 1 - menuShiftPos.Y), SpriteEffects.None, 0);
                                }
                                if (mmenu_select % 10 - 1 < mMenuStr[mmenu_select / 10].Length - 1)
                                {
                                    meas = sfMenu.MeasureString(mMenuStr[mmenu_select / 10][Math.Max(0, mmenu_select % 10)]);
                                    spritebatch.DrawString(sfMenu, mMenuStr[mmenu_select / 10][Math.Max(0, mmenu_select % 10)], new Vector2(rtSnake.Width / 2 - (meas.X / 2), ((1 - menuShiftPos.Y) * (rtSnake.Height * (.69727f) - (meas.Y))) + (menuShiftPos.Y * (rtSnake.Height * (((.69727f - .29589f) / 2) + .29589f) - (meas.Y / 2)))), mMenuCol[mmenu_select / 10][Math.Max(0, mmenu_select % 10)]);
                                }
                                if (mmenu_select % 10 - 1 < mMenuStr[mmenu_select / 10].Length - 2)
                                {
                                    meas = sfMenu.MeasureString(mMenuStr[mmenu_select / 10][Math.Max(0, mmenu_select % 10 + 1)]);
                                    spritebatch.DrawString(sfMenu, mMenuStr[mmenu_select / 10][Math.Max(0, mmenu_select % 10 + 1)], new Vector2(rtSnake.Width / 2 - (meas.X / 2), (rtSnake.Height * (.69727f))), mMenuCol[mmenu_select / 10][Math.Max(0, mmenu_select % 10 + 1)], 0, new Vector2(0, meas.Y), new Vector2(1, menuShiftPos.Y), SpriteEffects.None, 0);
                                }
                                menuShiftPos.Y = -menuShiftPos.Y;
                            }*/
                        }
                    }
                    else if (menuShiftPos.X > 0)
                    {
                        for (int i = 0; i < mMenuStr[0].Length; i++)
                        {
                            Vector2 sz = DefaultFont.MeasureString("" + mMenuStr[0][i]);
                            spritebatch.DrawString(DefaultFont, "" + mMenuStr[0][i], new Vector2((rtSnake.Width / 2) - (sz.X / 2) + (((menuShiftPos.X*2)-2)*xscale), (rtSnake.Height / 2) - (sz.Y / 2) + (yscale * (i + menuShiftPos.Y - (mmenu_select / 10) + 1))), new Color(mMenuCol[0][i].R,mMenuCol[0][i].G,mMenuCol[0][i].B,(byte)(255*menuShiftPos.X)));
                        }
                        for (int i = 0; i < mMenuStr[mmenu_select / 10].Length; i++)
                        {
                            Vector2 sz = DefaultFont.MeasureString("" + mMenuStr[mmenu_select / 10][i]);
                            spritebatch.DrawString(DefaultFont, "" + mMenuStr[mmenu_select / 10][i], new Vector2((rtSnake.Width / 2) - (sz.X / 2) + ((menuShiftPos.X * 2) * xscale), (rtSnake.Height / 2) - (sz.Y / 2) + (yscale * (i + menuShiftPos.Y - (mmenu_select % 10) + 1))), new Color(mMenuCol[mmenu_select / 10][i].R, mMenuCol[mmenu_select / 10][i].G, mMenuCol[mmenu_select / 10][i].B, (byte)(255 * (1 - menuShiftPos.X))));
                        }
                        /*Vector2 meas = sfMenu.MeasureString(mMenuStr[0][Math.Max(0, mmenu_select / 10 - 1)]);
                        spritebatch.DrawString(sfMenu, mMenuStr[0][Math.Max(0, mmenu_select / 10 - 1)], new Vector2((rtSnake.Width / 2 - (meas.X / 2))-(rtSnake.Width*(1-menuShiftPos.X)), ((1 - menuShiftPos.Y) * (rtSnake.Height * (((.69727f - .29589f) / 2) + .29589f) - (meas.Y / 2)))), new Color(mMenuCol[0][Math.Max(0, mmenu_select / 10 - 1)].R,mMenuCol[0][Math.Max(0, mmenu_select / 10 - 1)].G,mMenuCol[0][Math.Max(0, mmenu_select / 10 - 1)].B,(byte)(255*menuShiftPos.X)));
                        spritebatch.DrawString(sfMenu, mMenuStr[0][Math.Max(0, mmenu_select / 10 - 1)], new Vector2((rtSnake.Width / 2 - (meas.X / 2))-(rtSnake.Width*(1-menuShiftPos.X))+rtSnake.Width, ((1 - menuShiftPos.Y) * (rtSnake.Height * (.29589f / 2)))), new Color(mMenuCol[0][Math.Max(0, mmenu_select / 10 - 1)].R,mMenuCol[0][Math.Max(0, mmenu_select / 10 - 1)].G,mMenuCol[0][Math.Max(0, mmenu_select / 10 - 1)].B,(byte)(255*menuShiftPos.X)), 0, new Vector2(0,meas.Y / 2),new Vector2(1,.29589f/(.69727f - .29589f)),SpriteEffects.None,0);
                        if (mmenu_select / 10 - 1 > 0)
                        {
                            meas = sfMenu.MeasureString(mMenuStr[0][Math.Max(0, mmenu_select / 10 - 2)]);
                            spritebatch.DrawString(sfMenu, mMenuStr[0][Math.Max(0, mmenu_select / 10 - 2)], new Vector2((rtSnake.Width / 2 - (meas.X / 2))-(rtSnake.Width*(1-menuShiftPos.X)), rtSnake.Height*.29589f), new Color(mMenuCol[0][Math.Max(0, mmenu_select / 10 - 2)].R,mMenuCol[0][Math.Max(0, mmenu_select / 10 - 2)].G,mMenuCol[0][Math.Max(0, mmenu_select / 10 - 2)].B,(byte)(255*menuShiftPos.X)));
                            spritebatch.DrawString(sfMenu, mMenuStr[0][Math.Max(0, mmenu_select / 10 - 2)], new Vector2((rtSnake.Width / 2 - (meas.X / 2))-(rtSnake.Width*(1-menuShiftPos.X))+rtSnake.Width, ((meas.Y/2)*(.29589f/(.69727f - .29589f)))),new Color(mMenuCol[0][Math.Max(0, mmenu_select / 10 - 2)].R,mMenuCol[0][Math.Max(0, mmenu_select / 10 - 2)].G,mMenuCol[0][Math.Max(0, mmenu_select / 10 - 2)].B,(byte)(255*menuShiftPos.X)),0,new Vector2(0,meas.Y / 2),new Vector2(1,.29589f/(.69727f - .29589f)),SpriteEffects.None,0);
                        }
                        if (mmenu_select / 10 - 1 < mMenuStr[0].Length - 1)
                        {
                            meas = sfMenu.MeasureString(mMenuStr[0][Math.Max(0, mmenu_select / 10)]);
                            spritebatch.DrawString(sfMenu, mMenuStr[0][Math.Max(0, mmenu_select / 10)], new Vector2((rtSnake.Width / 2 - (meas.X / 2))-(rtSnake.Width*(1-menuShiftPos.X)), (rtSnake.Height * (.69727f))-((meas.Y/2)*(.29589f/(.69727f - .29589f)))), new Color(mMenuCol[0][Math.Max(0, mmenu_select / 10)].R,mMenuCol[0][Math.Max(0, mmenu_select / 10)].G,mMenuCol[0][Math.Max(0, mmenu_select / 10)].B,(byte)(255*menuShiftPos.X)), 0, new Vector2(0, meas.Y/2), new Vector2(1, 1), SpriteEffects.None, 0);
                            spritebatch.DrawString(sfMenu, mMenuStr[0][Math.Max(0, mmenu_select / 10)], new Vector2((rtSnake.Width / 2 - (meas.X / 2))-(rtSnake.Width*(1-menuShiftPos.X))+rtSnake.Width, (rtSnake.Height * (.29589f))-((meas.Y/2)*(.29589f/(.69727f - .29589f)))), new Color(mMenuCol[0][Math.Max(0, mmenu_select / 10)].R,mMenuCol[0][Math.Max(0, mmenu_select / 10)].G,mMenuCol[0][Math.Max(0, mmenu_select / 10)].B,(byte)(255*menuShiftPos.X)), 0, new Vector2(0, meas.Y/2), new Vector2(1, .29589f/(.69727f - .29589f)), SpriteEffects.None, 0);
                        }

                        meas = sfMenu.MeasureString(mMenuStr[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 1)]);
                        spritebatch.DrawString(sfMenu, mMenuStr[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 1)], new Vector2((rtSnake.Width / 2 - (meas.X / 2))-(rtSnake.Width*(1-menuShiftPos.X))+rtSnake.Width, ((rtSnake.Height * (((.69727f - .29589f) / 2) + .29589f) - (meas.Y / 2)))), new Color(mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 1)].R,mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 1)].G,mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 1)].B,(byte)(255*(1-menuShiftPos.X))));
                        spritebatch.DrawString(sfMenu, mMenuStr[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 1)], new Vector2((rtSnake.Width / 2 - (meas.X / 2))-(rtSnake.Width*(1-menuShiftPos.X)), ((rtSnake.Height * ((1-.69727f) / 2 + .69727f)))), new Color(mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 1)].R,mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 1)].G,mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 1)].B,(byte)(255*(1-menuShiftPos.X))), 0, new Vector2(0,meas.Y / 2),new Vector2(1,.29589f/(.69727f - .29589f)),SpriteEffects.None,0);
                        if (mmenu_select % 10 - 1 > 0)
                        {
                            meas = sfMenu.MeasureString(mMenuStr[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 2)]);
                            spritebatch.DrawString(sfMenu, mMenuStr[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 2)], new Vector2((rtSnake.Width / 2 - (meas.X / 2))-(rtSnake.Width*(1-menuShiftPos.X))+rtSnake.Width, rtSnake.Height*.29589f), new Color(mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 2)].R,mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 2)].G,mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 2)].B,(byte)(255*(1-menuShiftPos.X))));
                            spritebatch.DrawString(sfMenu, mMenuStr[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 2)], new Vector2((rtSnake.Width / 2 - (meas.X / 2))-(rtSnake.Width*(1-menuShiftPos.X)), ((meas.Y/2)*(.29589f/(.69727f - .29589f) + .69727f))),new Color(mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 2)].R,mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 2)].G,mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 2)].B,(byte)(255*(1-menuShiftPos.X))),0,new Vector2(0,meas.Y / 2),new Vector2(1,.29589f/(.69727f - .29589f)),SpriteEffects.None,0);
                        }
                        if (mmenu_select % 10 - 1 < mMenuStr[mmenu_select/10].Length - 1)
                        {
                            meas = sfMenu.MeasureString(mMenuStr[mmenu_select/10][Math.Max(0, mmenu_select / 10)]);
                            spritebatch.DrawString(sfMenu, mMenuStr[mmenu_select/10][Math.Max(0, mmenu_select % 10)], new Vector2((rtSnake.Width / 2 - (meas.X / 2))-(rtSnake.Width*(1-menuShiftPos.X))+rtSnake.Width, (rtSnake.Height * (.69727f))-((meas.Y/2)*(.29589f/(.69727f - .29589f)))), new Color(mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10)].R,mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10)].G,mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10)].B,(byte)(255*(1-menuShiftPos.X))), 0, new Vector2(0, meas.Y/2), new Vector2(1, 1), SpriteEffects.None, 0);
                            spritebatch.DrawString(sfMenu, mMenuStr[mmenu_select/10][Math.Max(0, mmenu_select % 10)], new Vector2((rtSnake.Width / 2 - (meas.X / 2))-(rtSnake.Width*(1-menuShiftPos.X)), (rtSnake.Height * (.29589f))-((meas.Y/2)*(.29589f/(.69727f - .29589f) + .69727f))), new Color(mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10)].R,mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10)].G,mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10)].B,(byte)(255*(1-menuShiftPos.X))), 0, new Vector2(0, meas.Y/2), new Vector2(1, .29589f/(.69727f - .29589f)), SpriteEffects.None, 0);
                        }*/
                    }
                    else if (menuShiftPos.X < 0)
                    {
                        for (int i = 0; i < mMenuStr[0].Length; i++)
                        {
                            Vector2 sz = DefaultFont.MeasureString("" + mMenuStr[0][i]);
                            spritebatch.DrawString(DefaultFont, "" + mMenuStr[0][i], new Vector2((rtSnake.Width / 2) - (sz.X / 2) + (((menuShiftPos.X * 2) ) * xscale), (rtSnake.Height / 2) - (sz.Y / 2) + (yscale * (i + menuShiftPos.Y - (mmenu_select / 10) + 1))), new Color(mMenuCol[0][i].R, mMenuCol[0][i].G, mMenuCol[0][i].B, (byte)(255 * menuShiftPos.X)));
                        }
                        for (int i = 0; i < mMenuStr[mmenu_select / 10].Length; i++)
                        {
                            Vector2 sz = DefaultFont.MeasureString("" + mMenuStr[mmenu_select / 10][i]);
                            spritebatch.DrawString(DefaultFont, "" + mMenuStr[mmenu_select / 10][i], new Vector2((rtSnake.Width / 2) - (sz.X / 2) + (((menuShiftPos.X * 2)+2) * xscale), (rtSnake.Height / 2) - (sz.Y / 2) + (yscale * (i + menuShiftPos.Y - (mmenu_select % 10) + 1))), new Color(mMenuCol[mmenu_select / 10][i].R, mMenuCol[mmenu_select / 10][i].G, mMenuCol[mmenu_select / 10][i].B, (byte)(255 * (1 - menuShiftPos.X))));
                        }
                        /*menuShiftPos.X = (1 + menuShiftPos.X);
                        Vector2 meas = sfMenu.MeasureString(mMenuStr[0][Math.Max(0, mmenu_select / 10 - 1)]);
                        spritebatch.DrawString(sfMenu, mMenuStr[0][Math.Max(0, mmenu_select / 10 - 1)], new Vector2((rtSnake.Width / 2 - (meas.X / 2))-(rtSnake.Width*(1-menuShiftPos.X)), ((1 - menuShiftPos.Y) * (rtSnake.Height * (((.69727f - .29589f) / 2) + .29589f) - (meas.Y / 2)))), new Color(mMenuCol[0][Math.Max(0, mmenu_select / 10 - 1)].R,mMenuCol[0][Math.Max(0, mmenu_select / 10 - 1)].G,mMenuCol[0][Math.Max(0, mmenu_select / 10 - 1)].B,(byte)(255*menuShiftPos.X)));
                        spritebatch.DrawString(sfMenu, mMenuStr[0][Math.Max(0, mmenu_select / 10 - 1)], new Vector2((rtSnake.Width / 2 - (meas.X / 2))-(rtSnake.Width*(1-menuShiftPos.X))+rtSnake.Width, ((1 - menuShiftPos.Y) * (rtSnake.Height * (.29589f / 2)))), new Color(mMenuCol[0][Math.Max(0, mmenu_select / 10 - 1)].R,mMenuCol[0][Math.Max(0, mmenu_select / 10 - 1)].G,mMenuCol[0][Math.Max(0, mmenu_select / 10 - 1)].B,(byte)(255*menuShiftPos.X)), 0, new Vector2(0,meas.Y / 2),new Vector2(1,.29589f/(.69727f - .29589f)),SpriteEffects.None,0);
                        if (mmenu_select / 10 - 1 > 0)
                        {
                            meas = sfMenu.MeasureString(mMenuStr[0][Math.Max(0, mmenu_select / 10 - 2)]);
                            spritebatch.DrawString(sfMenu, mMenuStr[0][Math.Max(0, mmenu_select / 10 - 2)], new Vector2((rtSnake.Width / 2 - (meas.X / 2))-(rtSnake.Width*(1-menuShiftPos.X)), rtSnake.Height*.29589f), new Color(mMenuCol[0][Math.Max(0, mmenu_select / 10 - 2)].R,mMenuCol[0][Math.Max(0, mmenu_select / 10 - 2)].G,mMenuCol[0][Math.Max(0, mmenu_select / 10 - 2)].B,(byte)(255*menuShiftPos.X)));
                            spritebatch.DrawString(sfMenu, mMenuStr[0][Math.Max(0, mmenu_select / 10 - 2)], new Vector2((rtSnake.Width / 2 - (meas.X / 2))-(rtSnake.Width*(1-menuShiftPos.X))+rtSnake.Width, ((meas.Y/2)*(.29589f/(.69727f - .29589f)))),new Color(mMenuCol[0][Math.Max(0, mmenu_select / 10 - 2)].R,mMenuCol[0][Math.Max(0, mmenu_select / 10 - 2)].G,mMenuCol[0][Math.Max(0, mmenu_select / 10 - 2)].B,(byte)(255*menuShiftPos.X)),0,new Vector2(0,meas.Y / 2),new Vector2(1,.29589f/(.69727f - .29589f)),SpriteEffects.None,0);
                        }
                        if (mmenu_select / 10 - 1 < mMenuStr[0].Length - 1)
                        {
                            meas = sfMenu.MeasureString(mMenuStr[0][Math.Max(0, mmenu_select / 10)]);
                            spritebatch.DrawString(sfMenu, mMenuStr[0][Math.Max(0, mmenu_select / 10)], new Vector2((rtSnake.Width / 2 - (meas.X / 2))-(rtSnake.Width*(1-menuShiftPos.X)), (rtSnake.Height * (.69727f))-((meas.Y/2)*(.29589f/(.69727f - .29589f)))), new Color(mMenuCol[0][Math.Max(0, mmenu_select / 10)].R,mMenuCol[0][Math.Max(0, mmenu_select / 10)].G,mMenuCol[0][Math.Max(0, mmenu_select / 10)].B,(byte)(255*menuShiftPos.X)), 0, new Vector2(0, meas.Y/2), new Vector2(1, 1), SpriteEffects.None, 0);
                            spritebatch.DrawString(sfMenu, mMenuStr[0][Math.Max(0, mmenu_select / 10)], new Vector2((rtSnake.Width / 2 - (meas.X / 2))-(rtSnake.Width*(1-menuShiftPos.X))+rtSnake.Width, (rtSnake.Height * (.29589f))-((meas.Y/2)*(.29589f/(.69727f - .29589f)))), new Color(mMenuCol[0][Math.Max(0, mmenu_select / 10)].R,mMenuCol[0][Math.Max(0, mmenu_select / 10)].G,mMenuCol[0][Math.Max(0, mmenu_select / 10)].B,(byte)(255*menuShiftPos.X)), 0, new Vector2(0, meas.Y/2), new Vector2(1, .29589f/(.69727f - .29589f)), SpriteEffects.None, 0);
                        }

                        meas = sfMenu.MeasureString(mMenuStr[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 1)]);
                        spritebatch.DrawString(sfMenu, mMenuStr[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 1)], new Vector2((rtSnake.Width / 2 - (meas.X / 2))-(rtSnake.Width*(1-menuShiftPos.X))+rtSnake.Width, ((rtSnake.Height * (((.69727f - .29589f) / 2) + .29589f) - (meas.Y / 2)))), new Color(mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 1)].R,mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 1)].G,mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 1)].B,(byte)(255*(1-menuShiftPos.X))));
                        spritebatch.DrawString(sfMenu, mMenuStr[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 1)], new Vector2((rtSnake.Width / 2 - (meas.X / 2))-(rtSnake.Width*(1-menuShiftPos.X)), ((rtSnake.Height * ((1-.69727f) / 2 + .69727f)))), new Color(mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 1)].R,mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 1)].G,mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 1)].B,(byte)(255*(1-menuShiftPos.X))), 0, new Vector2(0,meas.Y / 2),new Vector2(1,.29589f/(.69727f - .29589f)),SpriteEffects.None,0);
                        if (mmenu_select % 10 - 1 > 0)
                        {
                            meas = sfMenu.MeasureString(mMenuStr[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 2)]);
                            spritebatch.DrawString(sfMenu, mMenuStr[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 2)], new Vector2((rtSnake.Width / 2 - (meas.X / 2))-(rtSnake.Width*(1-menuShiftPos.X))+rtSnake.Width, rtSnake.Height*.29589f), new Color(mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 2)].R,mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 2)].G,mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 2)].B,(byte)(255*(1-menuShiftPos.X))));
                            spritebatch.DrawString(sfMenu, mMenuStr[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 2)], new Vector2((rtSnake.Width / 2 - (meas.X / 2))-(rtSnake.Width*(1-menuShiftPos.X)), ((meas.Y/2)*(.29589f/(.69727f - .29589f) + .69727f))),new Color(mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 2)].R,mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 2)].G,mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10 - 2)].B,(byte)(255*(1-menuShiftPos.X))),0,new Vector2(0,meas.Y / 2),new Vector2(1,.29589f/(.69727f - .29589f)),SpriteEffects.None,0);
                        }
                        if (mmenu_select % 10 - 1 < mMenuStr[mmenu_select/10].Length - 1)
                        {
                            meas = sfMenu.MeasureString(mMenuStr[mmenu_select/10][Math.Max(0, mmenu_select / 10)]);
                            spritebatch.DrawString(sfMenu, mMenuStr[mmenu_select/10][Math.Max(0, mmenu_select % 10)], new Vector2((rtSnake.Width / 2 - (meas.X / 2))-(rtSnake.Width*(1-menuShiftPos.X))+rtSnake.Width, (rtSnake.Height * (.69727f))-((meas.Y/2)*(.29589f/(.69727f - .29589f)))), new Color(mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10)].R,mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10)].G,mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10)].B,(byte)(255*(1-menuShiftPos.X))), 0, new Vector2(0, meas.Y/2), new Vector2(1, 1), SpriteEffects.None, 0);
                            spritebatch.DrawString(sfMenu, mMenuStr[mmenu_select/10][Math.Max(0, mmenu_select % 10)], new Vector2((rtSnake.Width / 2 - (meas.X / 2))-(rtSnake.Width*(1-menuShiftPos.X)), (rtSnake.Height * (.29589f))-((meas.Y/2)*(.29589f/(.69727f - .29589f) + .69727f))), new Color(mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10)].R,mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10)].G,mMenuCol[mmenu_select/10][Math.Max(0, mmenu_select % 10)].B,(byte)(255*(1-menuShiftPos.X))), 0, new Vector2(0, meas.Y/2), new Vector2(1, .29589f/(.69727f - .29589f)), SpriteEffects.None, 0);
                        }
                        menuShiftPos.X = -(1 - menuShiftPos.X);*/
                    }
                    spritebatch.End();
                    graphics.GraphicsDevice.SetRenderTarget(0, null);
                    texSnake = rtSnake.GetTexture();
#if !DEBUG
                    }
                    catch(Exception e)
                    {
#if WINDOWS
                        System.Windows.Forms.MessageBox.Show("Problem in Draw/MM/Pt0\n"+e.Message+"\n"+e.StackTrace);
#endif
                        Exit();
                        return;
                    }
                    try
                    {
#endif
                        graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
                        graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
                        //graphics.PreferMultiSampling = true;
                        graphics.ApplyChanges();

                        vd = new VertexDeclaration(graphics.GraphicsDevice, GBVertexFormat.Elements);
                        graphics.GraphicsDevice.Clear(new Color(0,0,30,255));
                        //graphics.GraphicsDevice.
                        engine.Parameters["ambientColor"].SetValue(new Vector4(0.1f, 0.1f, 0.1f, 1.0f));
                        engine.Parameters["diffuseColor"].SetValue(new Vector4(0.8f, 0.8f, 0.8f, 1.0f));
                        engine.Parameters["specularColor"].SetValue(new Vector4(1f, 1f, 1f, 1.0f));
                    graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;
                            graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
                            graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;

                        Random r = new Random();

                        
                        for (int i = 0; i < contguis.Length; i++)
                        {
                            if (contguis[i].status >= 2)
                            {
                                plo[linum] = true;
                                plp[linum] = new Vector3(-192+(contguis[i].loc*80), 192,-100);
                                pln[linum] = r.Next(64);
                                plf[linum] = r.Next(64)+65;
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
                        engine.Parameters["dLDiffuseColor"].SetValue(new Vector4(0.8f, 0.8f, 0.8f, 1.0f));
                        engine.Parameters["dLSpecularColor"].SetValue(new Vector4(0.0f, 0.0f, 0.0f, 1.0f));
                        engine.Parameters["dLightDir"].SetValue(Vector3.Normalize(new Vector3(0f, 0f, 1f)));

                        Version SM = graphics.GraphicsDevice.GraphicsDeviceCapabilities.PixelShaderVersion;
                        if (SM.Major >= 3)
                            engine.CurrentTechnique = engine.Techniques["menutechnique"];
                        else if (SM.Major >= 2)
                            engine.CurrentTechnique = engine.Techniques["menutechniquet"];
                        else
                            Exit();
                        engine.CommitChanges();
#if !DEBUG
                    }
                    catch(Exception e)
                    {
#if WINDOWS
                        System.Windows.Forms.MessageBox.Show("Problem in Draw/MM/Pt1\n"+e.Message+"\n"+e.StackTrace);
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
                            matProj = Matrix.CreatePerspectiveFieldOfView((float)Math.PI / 4.0f,
                              windowwidth / (float)windowheight,
                              10f, 80.0f);
                            engine.Parameters["fullbright"].SetValue(false);

                            matView = Matrix.CreateLookAt(new Vector3(0, 0, 32), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
                            //render the background graphics
                            engine.Parameters["view"].SetValue(matView);
                            engine.Parameters["proj"].SetValue(matProj);
                            engine.Parameters["viewInverse"].SetValue(Matrix.Invert(matView));

                            Matrix matRot, matScale, matTranslate;
                            {//basebottom
                                matTranslate = Matrix.CreateTranslation(0, 0, 0);
                                matRot = Matrix.Identity;// Matrix.CreateRotationX((float)Math.PI);
                                matScale = Matrix.CreateScale(2, 2, 2);

                                engine.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                                engine.Parameters["wRot"].SetValue(matRot);
                                engine.Parameters["diffuseTexture"].SetValue(texSnake);
                                engine.Parameters["shininess"].SetValue(0.25f);
                                engine.Parameters["SpecularEnabled"].SetValue(true);
                                engine.Parameters["vertexAlpha"].SetValue(false);
                                engine.Parameters["BumpMappingEnabled"].SetValue(false);
                                engine.CommitChanges();

                                graphics.GraphicsDevice.VertexDeclaration = vd;
                                foreach (ModelMesh mesh in mSnake.Meshes)
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
                            pass.End();
                        }
                        engine.End();
                        
                        
                        
                        spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
                        spritebatch.Draw(waves,new Rectangle((int)waveM1,windowheight*6/8,windowwidth,windowheight*2/8),Color.White);
                        spritebatch.Draw(waves,new Rectangle(-windowwidth+(int)waveM1,windowheight*6/8,windowwidth,windowheight*2/8),Color.White);
                        spritebatch.Draw(waves, new Rectangle((int)waveM2, windowheight * 7 / 8, windowwidth, windowheight * 2 / 8), Color.White);
                        spritebatch.Draw(waves, new Rectangle(-windowwidth+(int)waveM2, windowheight * 7 / 8, windowwidth, windowheight * 2 / 8), Color.White);
                        spritebatch.Draw(songchoosetop, new Rectangle(0, (int)(-songchoosetop.Height * 0.26f), windowwidth, songchoosetop.Height), Color.White);
                        spritebatch.Draw(songchoosetop, new Rectangle(0, (int)(windowheight-songchoosetop.Height *.74f), windowwidth, songchoosetop.Height),null, Color.White,0,new Vector2(0,0),SpriteEffects.FlipVertically,0);
                        spritebatch.Draw(tbgGreen, new Rectangle((int)(0.1f * windowwidth), (int)(0.80f * windowheight), (int)(0.09f * windowheight), (int)(0.09f * windowheight)), Color.White);
                        spritebatch.DrawString(DefaultFont, "Select", new Vector2((0.1f * windowwidth) + (0.10f * windowheight), (0.90f * windowheight) - (DefaultFont.MeasureString("Select").Y)), Color.White);
                        if (DemoMode)
                        {
                            spritebatch.DrawString(BigFont, "Demo Mode", new Vector2((windowwidth / 2) - (BigFont.MeasureString("Demo Mode").X / 2), windowheight * 0.15f), new Color(255, 0, 0, 64));
                            spritebatch.DrawString(BigFont, "Demo Mode", new Vector2((windowwidth / 2) - (BigFont.MeasureString("Demo Mode").X / 2), windowheight * 0.4f), new Color(255, 0, 0, 64));
                            spritebatch.DrawString(BigFont, "Demo Mode", new Vector2((windowwidth / 2) - (BigFont.MeasureString("Demo Mode").X / 2), windowheight * 0.65f), new Color(255, 0, 0, 64));
                        }
                        //spritebatch.Draw(texHeader, new Rectangle((int)(windowwidth*0.1f), (int)(windowheight*0.2f), (int)(windowwidth*0.8f), (int)(windowheight*.2f)), Color.White);
                        //spritebatch.DrawString(DefaultFont, "Multiplayer->Quickplay allows for Single Player play", new Vector2((windowwidth / 2) - (DefaultFont.MeasureString("Multiplayer->Quickplay allows for Single Player play").X / 2), windowheight * 0.8f), Color.White);
                        spritebatch.DrawString(DefaultFont, "Menus in Red are not yet implemented.", new Vector2((windowwidth / 2) - (DefaultFont.MeasureString("Menus in Red are not yet implemented.").X / 2), windowheight * 0.75f), Color.White);

                        if (mmLogoTime < 1)
                            spritebatch.Draw(texWhite, new Rectangle(0, 0, windowwidth, windowheight), Color.Black);
                        else if (mmLogoTime < 2)
                            spritebatch.Draw(texWhite, new Rectangle(0, 0, windowwidth, windowheight), new Color(0, 0, 0, (byte)(255*(1 - (mmLogoTime - 1)))));
                        if(mmLogoTime<1)
                            spritebatch.Draw(texHeader, (new Rectangle((int)(windowwidth * -0.2f * (mmLogoTime))+(int)((1 - mmLogoTime) * windowwidth * 0.4f), (int)(windowheight * .1f * (mmLogoTime))+(int)((1 - mmLogoTime) * windowheight * -0.3f), (int)(windowwidth * 1.4f * (mmLogoTime))+(int)((1 - mmLogoTime) * windowwidth * 0.2f), (int)(windowheight * 0.4f * (mmLogoTime)))), Color.White);
                        else if (mmLogoTime < 2)
                            spritebatch.Draw(texHeader, (new Rectangle((int)((mmLogoTime - 1) * windowwidth * 0.1f)+(int)((1 - (mmLogoTime - 1)) * -windowwidth * 0.2f), (int)((mmLogoTime - 1) * windowheight * 0.2f)+(int)((1 - (mmLogoTime - 1)) * windowheight * 0.1f), (int)((mmLogoTime - 1) * windowwidth * 0.8f)+(int)((1 - (mmLogoTime - 1)) * windowwidth * 1.4f), (int)((mmLogoTime - 1) * windowheight * 0.2f)+(int)((1 - (mmLogoTime - 1)) * windowheight * .4f))), Color.White);
                        else
                            spritebatch.Draw(texHeader, new Rectangle((int)(windowwidth * 0.1f), (int)(windowheight * 0.2f), (int)(windowwidth * 0.8f), (int)(windowheight * .2f)), Color.White);

                        spritebatch.End();
#if !DEBUG
                    }
                    catch(Exception e)
                    {
#if WINDOWS
                        System.Windows.Forms.MessageBox.Show("Problem in Draw/MM/Pt2\n"+e.Message+"\n"+e.StackTrace);
#endif
                        Exit();
                        return;
                    }
#endif
                    /*spritebatch.Begin();
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

                    
                    spritebatch.End();*/
                }
                #endregion
                #region optionsscreen
                else if (screen == S_OPTIONS)
                {

                    int linum = 0;
                    bool[] plo = new bool[16];
                    Vector3[] plp = new Vector3[16];
                    float[] pln = new float[16];
                    float[] plf = new float[16];
                    Vector3[] pld = new Vector3[16];
                    Vector3[] pls = new Vector3[16];

#if !DEBUG
                    try
                    {
#endif
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
                    graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;
                    graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
                    graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;

                    Random r = new Random();

                    Version SM = graphics.GraphicsDevice.GraphicsDeviceCapabilities.PixelShaderVersion;
                    engine.Parameters["pLightOn"].SetValue(plo);
                    engine.Parameters["pLightPos"].SetValue(plp);
                    engine.Parameters["pLightNear"].SetValue(pln);
                    engine.Parameters["pLightFar"].SetValue(plf);
                    engine.Parameters["pLightDiffuse"].SetValue(pld);
                    engine.Parameters["pLightSpecular"].SetValue(pls);
                    engine.Parameters["dLDiffuseColor"].SetValue(new Vector4(0.8f, 0.8f, 0.8f, 1.0f));
                    engine.Parameters["dLSpecularColor"].SetValue(new Vector4(0.0f, 0.0f, 0.0f, 1.0f));
                    engine.Parameters["dLightDir"].SetValue(Vector3.Normalize(new Vector3(0.1f, -1f, 0.5f)));
                    if (SM.Major >= 3)
                        engine.CurrentTechnique = engine.Techniques["menutechnique"];
                    else if (SM.Major >= 2)
                        engine.CurrentTechnique = engine.Techniques["menutechniquet"];
                    else
                        Exit();
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
                        matProj = Matrix.CreatePerspectiveFieldOfView((float)Math.PI / 4.0f,
                          windowwidth / (float)windowheight,
                          .1f, 80.0f);
                        engine.Parameters["fullbright"].SetValue(false);

                        intro += (float)gameTime.ElapsedGameTime.TotalSeconds;
                        float introlerp = (Math.Min(intro, 0.5f) * 2);
                        Vector3 campos = ((1 - introlerp) * (new Vector3(-8, 112, 24))) + ((introlerp) * (new Vector3(-2.2f, 106.3f, 1f)));
                        matView = Matrix.CreateLookAt(campos, new Vector3(-2.2f, 106.3f, 0), new Vector3(0, 1, 0));
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

                            graphics.GraphicsDevice.VertexDeclaration = vd;
                            graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                            graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                            graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                            graphics.GraphicsDevice.Vertices[0].SetSource(square, 0, GBVertexFormat.SizeInBytes);
                            graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
                            graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                        }
                        {//basewall
                            matTranslate = Matrix.CreateTranslation(0, 132, -4.6f);
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

                            graphics.GraphicsDevice.VertexDeclaration = vd;
                            graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                            graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                            graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                            graphics.GraphicsDevice.Vertices[0].SetSource(square, 0, GBVertexFormat.SizeInBytes);
                            graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
                            graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                        }
                        {//rightwall
                            matTranslate = Matrix.CreateTranslation(6.8f, 132, 0f);
                            matRot = Matrix.CreateRotationX((float)Math.PI / 2) * Matrix.CreateRotationY(MathHelper.PiOver2);
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

                            graphics.GraphicsDevice.VertexDeclaration = vd;
                            graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                            graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                            graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                            graphics.GraphicsDevice.Vertices[0].SetSource(square, 0, GBVertexFormat.SizeInBytes);
                            graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
                            graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                        }

                        {//amp
                            matTranslate = Matrix.CreateTranslation(0, 103.6f, 0);
                            matRot = Matrix.Identity;
                            matScale = Matrix.CreateScale(1, 1, 1);

                            engine.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                            engine.Parameters["wRot"].SetValue(matRot);

                            engine.Parameters["shininess"].SetValue(0.25f);
                            engine.Parameters["SpecularEnabled"].SetValue(false);
                            engine.Parameters["vertexAlpha"].SetValue(false);
                            engine.Parameters["BumpMappingEnabled"].SetValue(false);
                            engine.Parameters["diffuseTexture"].SetValue(tamp1);
                            engine.CommitChanges();
                            foreach (ModelMesh mesh in mamp1.Meshes)
                            {
                                foreach (ModelMeshPart meshpart in mesh.MeshParts)
                                {
                                    graphics.GraphicsDevice.VertexDeclaration = meshpart.VertexDeclaration;
                                    graphics.GraphicsDevice.Vertices[0].SetSource(mesh.VertexBuffer, meshpart.StreamOffset, meshpart.VertexStride);
                                    graphics.GraphicsDevice.Indices = mesh.IndexBuffer;
                                    graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, meshpart.BaseVertex, 0, meshpart.NumVertices, meshpart.StartIndex, meshpart.PrimitiveCount);
                                }
                            }

                            engine.Parameters["diffuseTexture"].SetValue(tamp2);
                            engine.CommitChanges();
                            foreach (ModelMesh mesh in mamp2.Meshes)
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

                        pass.End();
                    }
                    engine.End();
                    float xscale = 1, scale = 1;
                    spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);

                    {//res
                        int resIndex = -1;
                        for (int i = 0; i < resY.Length; i++)
                        {
                            if (resY[i] == windowheight)
                                for (int k = i; k < resX.Length; k++)
                                    if (resX[k] == windowwidth)
                                        resIndex = k;
                        }
                        if (resIndex == -1)
                            resIndex = 0;
                        float x=0.2f, y=0.3f;
                        spritebatch.Draw(ttape, new Rectangle((int)((x + 0.07f) * windowwidth + optionsOffset * xscale), (int)(y * windowheight), (int)(0.15f*windowwidth), (int)(0.1f*windowheight)), Color.White);
                        spritebatch.DrawString(DefaultFont, "Resolution", new Vector2(((x + 0.08f) * windowwidth + optionsOffset * xscale), (y+0.01f)*windowheight), Color.Black, 0, new Vector2(0,0),windowwidth/1024f,SpriteEffects.None,0);
                        spritebatch.DrawString(DefaultFont, "" + resX[resIndex] + "x" + resY[resIndex], new Vector2(((x + 0.08f) * windowwidth + optionsOffset * xscale), (y + 0.05f) * windowheight), Color.Black, 0, new Vector2(0, 0), scale * (windowwidth / 1024f), SpriteEffects.None, 0);
                        spritebatch.Draw(tknob, new Vector2((x * windowwidth + optionsOffset * xscale), y * windowheight), null, Color.White, ((float)resIndex / (resY.Length - 1)) * -MathHelper.Pi, new Vector2(tknob.Width / 2, tknob.Height / 2), scale * (windowwidth / 1024f), SpriteEffects.None, 0);
                    }
                    {//gui
                        float x = 0.4f, y = 0.6f;
                        spritebatch.Draw(ttape, new Rectangle((int)((x + 0.07f) * windowwidth + optionsOffset * xscale), (int)(y * windowheight), (int)(0.15f * windowwidth), (int)(0.1f * windowheight)), Color.White);
                        spritebatch.DrawString(DefaultFont, "HUD Style", new Vector2(((x + 0.08f) * windowwidth + optionsOffset * xscale), (y + 0.01f) * windowheight), Color.Black, 0, new Vector2(0, 0), windowwidth / 1024f, SpriteEffects.None, 0);
                        spritebatch.DrawString(DefaultFont, guiStyle[(int)cGUIStyle], new Vector2(((x + 0.08f) * windowwidth + optionsOffset * xscale), (y + 0.05f) * windowheight), Color.Black, 0, new Vector2(0, 0), scale * (windowwidth / 1024f), SpriteEffects.None, 0);
                        spritebatch.Draw(tknob, new Vector2((x * windowwidth + optionsOffset * xscale), y * windowheight), null, Color.White, ((float)cGUIStyle / (guiStyle.Length - 1)) * -MathHelper.Pi, new Vector2(tknob.Width / 2, tknob.Height / 2), scale * (windowwidth / 1024f), SpriteEffects.None, 0);
                    }
                    {//gfxqual
                        float x = 0.6f, y = 0.3f;
                        spritebatch.Draw(ttape, new Rectangle((int)((x + 0.07f) * windowwidth + optionsOffset * xscale), (int)(y * windowheight), (int)(0.2f * windowwidth), (int)(0.1f * windowheight)), Color.White);
                        spritebatch.DrawString(DefaultFont, "Graphics Level", new Vector2(((x + 0.08f) * windowwidth + optionsOffset * xscale), (y + 0.01f) * windowheight), Color.Black, 0, new Vector2(0, 0), windowwidth / 1024f, SpriteEffects.None, 0);
                        spritebatch.DrawString(DefaultFont, ""+renderLevel, new Vector2(((x + 0.08f) * windowwidth + optionsOffset * xscale), (y + 0.05f) * windowheight), Color.Black, 0, new Vector2(0, 0), scale * (windowwidth / 1024f), SpriteEffects.None, 0);
                        spritebatch.Draw(tknob, new Vector2((x * windowwidth + optionsOffset * xscale), y * windowheight), null, Color.White, ((float)renderLevel / (10f - 1)) * -MathHelper.Pi, new Vector2(tknob.Width / 2, tknob.Height / 2), scale * (windowwidth / 1024f), SpriteEffects.None, 0);
                    }
                    {//3don
                        float x = 0.8f, y = 0.6f;
                        spritebatch.Draw(ttape, new Rectangle((int)((x + 0.03f) * windowwidth + optionsOffset * xscale), (int)((y-0.05f) * windowheight), (int)(0.15f * windowwidth), (int)(0.1f * windowheight)), Color.White);
                        spritebatch.DrawString(DefaultFont, "3D Mode?", new Vector2(((x + 0.04f) * windowwidth + optionsOffset * xscale), (y - 0.04f) * windowheight), Color.Black, 0, new Vector2(0, 0), windowwidth / 1024f, SpriteEffects.None, 0);
                        spritebatch.DrawString(DefaultFont, render3D?"On":"Off", new Vector2(((x + 0.04f) * windowwidth + optionsOffset * xscale), (y) * windowheight), Color.Black, 0, new Vector2(0, 0), scale * (windowwidth / 1024f), SpriteEffects.None, 0);
                        spritebatch.Draw(render3D?tswitchon:tswitchoff, new Vector2((x * windowwidth + optionsOffset * xscale), y * windowheight), null, Color.White, 0, new Vector2(tswitchon.Width / 2, tswitchon.Height / 2), scale * (windowwidth / 1024f), SpriteEffects.None, 0);
                        spritebatch.Draw(render3D ? tledon : tledoff, new Vector2((x * windowwidth + optionsOffset * xscale), (y-0.13f) * windowheight), null, Color.White, 0, new Vector2(tledon.Width / 2, tledon.Height / 2), scale * (windowwidth / 1024f), SpriteEffects.None, 0);
                    }
                    {//fulls
                        float x = 1f, y = 0.4f;
                        spritebatch.Draw(ttape, new Rectangle((int)((x + 0.03f) * windowwidth + optionsOffset * xscale), (int)((y - 0.05f) * windowheight), (int)(0.15f * windowwidth), (int)(0.1f * windowheight)), Color.White);
                        spritebatch.DrawString(DefaultFont, "Full Screen?", new Vector2(((x + 0.04f) * windowwidth + optionsOffset * xscale), (y - 0.04f) * windowheight), Color.Black, 0, new Vector2(0, 0), windowwidth / 1024f, SpriteEffects.None, 0);
                        spritebatch.DrawString(DefaultFont, fullScreen? "On" : "Off", new Vector2(((x + 0.04f) * windowwidth + optionsOffset * xscale), (y) * windowheight), Color.Black, 0, new Vector2(0, 0), scale * (windowwidth / 1024f), SpriteEffects.None, 0);
                        spritebatch.Draw(fullScreen ? tswitchon : tswitchoff, new Vector2((x * windowwidth + optionsOffset * xscale), y * windowheight), null, Color.White, 0, new Vector2(tswitchon.Width / 2, tswitchon.Height / 2), scale * (windowwidth / 1024f), SpriteEffects.None, 0);
                        spritebatch.Draw(fullScreen ? tledon : tledoff, new Vector2((x * windowwidth + optionsOffset * xscale), (y - 0.13f) * windowheight), null, Color.White, 0, new Vector2(tledon.Width / 2, tledon.Height / 2), scale * (windowwidth / 1024f), SpriteEffects.None, 0);
                    }
                    {//fps
                        float x = 1.2f, y = 0.6f;
                        spritebatch.Draw(ttape, new Rectangle((int)((x + 0.03f) * windowwidth + optionsOffset * xscale), (int)((y - 0.05f) * windowheight), (int)(0.15f * windowwidth), (int)(0.1f * windowheight)), Color.White);
                        spritebatch.DrawString(DefaultFont, "Show FPS?", new Vector2(((x + 0.04f) * windowwidth + optionsOffset * xscale), (y - 0.04f) * windowheight), Color.Black, 0, new Vector2(0, 0), windowwidth / 1024f, SpriteEffects.None, 0);
                        spritebatch.DrawString(DefaultFont, ShowFPS? "On" : "Off", new Vector2(((x + 0.04f) * windowwidth + optionsOffset * xscale), (y) * windowheight), Color.Black, 0, new Vector2(0, 0), scale * (windowwidth / 1024f), SpriteEffects.None, 0);
                        spritebatch.Draw(ShowFPS ? tswitchon : tswitchoff, new Vector2((x * windowwidth + optionsOffset * xscale), y * windowheight), null, Color.White, 0, new Vector2(tswitchon.Width / 2, tswitchon.Height / 2), scale * (windowwidth / 1024f), SpriteEffects.None, 0);
                        spritebatch.Draw(ShowFPS ? tledon : tledoff, new Vector2((x * windowwidth + optionsOffset * xscale), (y - 0.13f) * windowheight), null, Color.White, 0, new Vector2(tledon.Width / 2, tledon.Height / 2), scale * (windowwidth / 1024f), SpriteEffects.None, 0);
                    }

                    spritebatch.Draw(tbgGreen, new Rectangle((int)(0.1f * windowwidth), (int)(0.80f * windowheight), (int)(0.09f * windowheight), (int)(0.09f * windowheight)), Color.White);
                    spritebatch.DrawString(DefaultFont, "Select", new Vector2((0.1f * windowwidth) + (0.10f * windowheight), (0.80f * windowheight) + (0.09f * windowheight) - (DefaultFont.MeasureString("Select").Y)), Color.White);
                    spritebatch.Draw(tbgRed, new Rectangle((int)(0.9f * windowwidth) - (int)(0.09f * windowheight), (int)(0.80f * windowheight), (int)(0.09f * windowheight), (int)(0.09f * windowheight)), Color.White);
                    spritebatch.DrawString(DefaultFont, "Back", new Vector2((0.9f * windowwidth) - (0.10f * windowheight) - DefaultFont.MeasureString("Back").X, (0.80f * windowheight) + (0.09f * windowheight) - (DefaultFont.MeasureString("Back").Y)), Color.White);
                    if (DemoMode)
                    {
                        spritebatch.DrawString(BigFont, "Demo Mode", new Vector2((windowwidth / 2) - (BigFont.MeasureString("Demo Mode").X / 2), windowheight * 0.15f), new Color(255, 0, 0, 64));
                        spritebatch.DrawString(BigFont, "Demo Mode", new Vector2((windowwidth / 2) - (BigFont.MeasureString("Demo Mode").X / 2), windowheight * 0.4f), new Color(255, 0, 0, 64));
                        spritebatch.DrawString(BigFont, "Demo Mode", new Vector2((windowwidth / 2) - (BigFont.MeasureString("Demo Mode").X / 2), windowheight * 0.65f), new Color(255, 0, 0, 64));
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
                #endregion
                #region diffscreen
                else if (screen == S_CHOOSEDIFF)
                {

                    int linum = 0;
                    bool[] plo = new bool[16];
                    Vector3[] plp = new Vector3[16];
                    float[] pln = new float[16];
                    float[] plf = new float[16];
                    Vector3[] pld = new Vector3[16];
                    Vector3[] pls = new Vector3[16];

#if !DEBUG
                    try
                    {
#endif
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
                    graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;
                    graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
                    graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;

                    Random r = new Random();


                    for (int i = 0; i < contguis.Length; i++)
                    {
                        if (contguis[i].status >= 2)
                        {
                            plo[linum] = true;
                            plp[linum] = new Vector3(-192 + (contguis[i].loc * 80), 192, -100);
                            pln[linum] = r.Next(64);
                            plf[linum] = r.Next(64) + 65;
                            pld[linum] = new Vector3(.9f + (float)(r.NextDouble() / 10), .5f + (float)(r.NextDouble() / 10), .2f + (float)(r.NextDouble() / 10));
                            pls[linum] = new Vector3(0.2f, 0.1f, 0.0f);
                            linum++;
                        }
                    }

                    Version SM = graphics.GraphicsDevice.GraphicsDeviceCapabilities.PixelShaderVersion;
                    engine.Parameters["pLightOn"].SetValue(plo);
                    engine.Parameters["pLightPos"].SetValue(plp);
                    engine.Parameters["pLightNear"].SetValue(pln);
                    engine.Parameters["pLightFar"].SetValue(plf);
                    engine.Parameters["pLightDiffuse"].SetValue(pld);
                    engine.Parameters["pLightSpecular"].SetValue(pls);
                    engine.Parameters["dLDiffuseColor"].SetValue(new Vector4(0.8f, 0.8f, 0.8f, 1.0f));
                    engine.Parameters["dLSpecularColor"].SetValue(new Vector4(0.0f, 0.0f, 0.0f, 1.0f));
                    engine.Parameters["dLightDir"].SetValue(Vector3.Normalize(new Vector3(0.1f, -1f, 0.5f)));
                    if (SM.Major >= 3)
                        engine.CurrentTechnique = engine.Techniques["menutechnique"];
                    else if (SM.Major >= 2)
                        engine.CurrentTechnique = engine.Techniques["menutechniquet"];
                    else
                        Exit();
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
                        matProj = Matrix.CreatePerspectiveFieldOfView((float)Math.PI / 4.0f,
                          windowwidth / (float)windowheight,
                          10f, 80.0f);
                        engine.Parameters["fullbright"].SetValue(false);

                        matView = Matrix.CreateLookAt(new Vector3(0, 108, 22), new Vector3(0, 104, 0), new Vector3(0, 1, 0));
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

                            graphics.GraphicsDevice.VertexDeclaration = vd;
                            graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                            graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                            graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                            graphics.GraphicsDevice.Vertices[0].SetSource(square, 0, GBVertexFormat.SizeInBytes);
                            graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
                            graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
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

                            graphics.GraphicsDevice.VertexDeclaration = vd;
                            graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                            graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                            graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                            graphics.GraphicsDevice.Vertices[0].SetSource(square, 0, GBVertexFormat.SizeInBytes);
                            graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
                            graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                        }

                        {//ssfront
                            matTranslate = Matrix.CreateTranslation(12, 101, -6);
                            matRot = Matrix.CreateRotationX((float)Math.PI / 2);
                            matScale = Matrix.CreateScale(3, 0, -1);

                            engine.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                            engine.Parameters["wRot"].SetValue(matRot);
                            engine.Parameters["diffuseTexture"].SetValue(!instruments[3] ? greyishTex[0] : greyishTex[diff[3] + 1]);
                            engine.Parameters["diffuseColor"].SetValue(diffConfirm[3] ? new Vector4(0, 0, 1, 1) : new Vector4(0.8f, 0.8f, 0.8f, 1));
                            engine.Parameters["bumpTexture"].SetValue(whitishBM);
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
                            engine.Parameters["diffuseColor"].SetValue(new Vector4(0.8f, 0.8f, 0.8f, 1));
                        }
                        {//ssright
                            matTranslate = Matrix.CreateTranslation(9, 101, -12);
                            matRot = Matrix.CreateRotationX((float)Math.PI / 2) * Matrix.CreateRotationY(-MathHelper.PiOver2);
                            matScale = Matrix.CreateScale(6, 0, 1);

                            engine.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                            engine.Parameters["wRot"].SetValue(matRot);
                            engine.Parameters["diffuseTexture"].SetValue(greyishTex[0]);
                            engine.Parameters["bumpTexture"].SetValue(whitishBM);
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
                        {//sstop
                            matTranslate = Matrix.CreateTranslation(12, 102, -12);
                            matRot = Matrix.CreateRotationX((float)Math.PI);
                            matScale = Matrix.CreateScale(3, 0, 6);

                            engine.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                            engine.Parameters["wRot"].SetValue(matRot);
                            engine.Parameters["diffuseTexture"].SetValue(greyishTex[0]);
                            engine.Parameters["bumpTexture"].SetValue(whitishBM);
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
                        {//dsfront
                            matTranslate = Matrix.CreateTranslation(5, 101, -6);
                            matRot = Matrix.CreateRotationX((float)Math.PI / 2);
                            matScale = Matrix.CreateScale(3, 0, -1);

                            engine.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                            engine.Parameters["wRot"].SetValue(matRot);
                            engine.Parameters["diffuseTexture"].SetValue(!instruments[2] ? greyishTex[0] : greyishTex[diff[2] + 1]);
                            engine.Parameters["diffuseColor"].SetValue(diffConfirm[2] ? new Vector4(0, 0, 1, 1) : new Vector4(0.8f, 0.8f, 0.8f, 1));
                            engine.Parameters["bumpTexture"].SetValue(whitishBM);
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
                            engine.Parameters["diffuseColor"].SetValue(new Vector4(0.8f, 0.8f, 0.8f, 1));
                        }
                        {//dsright
                            matTranslate = Matrix.CreateTranslation(2, 101, -16);
                            matRot = Matrix.CreateRotationX((float)Math.PI / 2) * Matrix.CreateRotationY(-MathHelper.PiOver2);
                            matScale = Matrix.CreateScale(10, 0, 1);

                            engine.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                            engine.Parameters["wRot"].SetValue(matRot);
                            engine.Parameters["diffuseTexture"].SetValue(greyishTex[0]);
                            engine.Parameters["bumpTexture"].SetValue(whitishBM);
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
                        {//dstop
                            matTranslate = Matrix.CreateTranslation(5, 102, -16);
                            matRot = Matrix.CreateRotationX((float)Math.PI);
                            matScale = Matrix.CreateScale(3, 0, 10);

                            engine.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                            engine.Parameters["wRot"].SetValue(matRot);
                            engine.Parameters["diffuseTexture"].SetValue(greyishTex[0]);
                            engine.Parameters["bumpTexture"].SetValue(whitishBM);
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

                        {//psfront
                            matTranslate = Matrix.CreateTranslation(-12, 101, -4);
                            matRot = Matrix.CreateRotationX((float)Math.PI / 2);
                            matScale = Matrix.CreateScale(3, 0, -1);

                            engine.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                            engine.Parameters["wRot"].SetValue(matRot);
                            engine.Parameters["diffuseTexture"].SetValue(!instruments[0] ? greyishTex[0] : greyishTex[diff[0] + 1]);
                            engine.Parameters["diffuseColor"].SetValue(diffConfirm[0] ? new Vector4(0, 0, 1, 1) : new Vector4(0.8f, 0.8f, 0.8f, 1));
                            engine.Parameters["bumpTexture"].SetValue(whitishBM);
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
                            engine.Parameters["diffuseColor"].SetValue(new Vector4(0.8f, 0.8f, 0.8f, 1));
                        }
                        {//psright
                            matTranslate = Matrix.CreateTranslation(-9, 101, -8);
                            matRot = Matrix.CreateRotationX((float)Math.PI / 2) * Matrix.CreateRotationY(MathHelper.PiOver2);
                            matScale = Matrix.CreateScale(4, 0, 1);

                            engine.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                            engine.Parameters["wRot"].SetValue(matRot);
                            engine.Parameters["diffuseTexture"].SetValue(greyishTex[0]);
                            engine.Parameters["bumpTexture"].SetValue(whitishBM);
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
                        {//pstop
                            matTranslate = Matrix.CreateTranslation(-12, 102, -8);
                            matRot = Matrix.CreateRotationX((float)Math.PI);
                            matScale = Matrix.CreateScale(3, 0, 4);

                            engine.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                            engine.Parameters["wRot"].SetValue(matRot);
                            engine.Parameters["diffuseTexture"].SetValue(greyishTex[0]);
                            engine.Parameters["bumpTexture"].SetValue(whitishBM);
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

                        {//vsfront
                            matTranslate = Matrix.CreateTranslation(-5, 101, -5);
                            matRot = Matrix.CreateRotationX((float)Math.PI / 2);
                            matScale = Matrix.CreateScale(3, 0, -1);

                            engine.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                            engine.Parameters["wRot"].SetValue(matRot);
                            engine.Parameters["diffuseTexture"].SetValue(!instruments[1] ? greyishTex[0] : greyishTex[diff[1] + 1]);
                            engine.Parameters["bumpTexture"].SetValue(whitishBM);
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
                        {//vsright
                            matTranslate = Matrix.CreateTranslation(-2, 101, -10);
                            matRot = Matrix.CreateRotationX((float)Math.PI / 2) * Matrix.CreateRotationY(MathHelper.PiOver2);
                            matScale = Matrix.CreateScale(5, 0, 1);

                            engine.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                            engine.Parameters["wRot"].SetValue(matRot);
                            engine.Parameters["diffuseTexture"].SetValue(greyishTex[0]);
                            engine.Parameters["bumpTexture"].SetValue(whitishBM);
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
                        {//vstop
                            matTranslate = Matrix.CreateTranslation(-5, 102, -10);
                            matRot = Matrix.CreateRotationX((float)Math.PI);
                            matScale = Matrix.CreateScale(3, 0, 5);

                            engine.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                            engine.Parameters["wRot"].SetValue(matRot);
                            engine.Parameters["diffuseTexture"].SetValue(greyishTex[0]);
                            engine.Parameters["bumpTexture"].SetValue(whitishBM);
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


                        if (instruments[0])
                        {//guitar
                            matTranslate = Matrix.CreateTranslation(-12, 105, -6);
                            matRot = Matrix.CreateRotationX(MathHelper.PiOver4);
                            matScale = Matrix.CreateScale(1, 1, 1);

                            engine.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                            engine.Parameters["wRot"].SetValue(matRot * Matrix.CreateRotationX(MathHelper.Pi / 2));

                            engine.Parameters["shininess"].SetValue(0.25f);
                            engine.Parameters["SpecularEnabled"].SetValue(false);
                            engine.Parameters["vertexAlpha"].SetValue(false);
                            engine.Parameters["BumpMappingEnabled"].SetValue(false);


                            if (diff[0] == 0)
                            {
                                engine.Parameters["diffuseTexture"].SetValue(texQuarter);
                                engine.CommitChanges();
                                foreach (ModelMesh mesh in mQuarter.Meshes)
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
                            else if (diff[0] == 1)
                            {
                                engine.Parameters["diffuseTexture"].SetValue(texPickGP);
                                engine.CommitChanges();
                                foreach (ModelMesh mesh in mPick.Meshes)
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
                            else if (diff[0] == 2)
                            {
                                engine.Parameters["diffuseTexture"].SetValue(texPickTit);
                                engine.CommitChanges();
                                foreach (ModelMesh mesh in mPick.Meshes)
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
                            else
                            {
                                engine.Parameters["diffuseTexture"].SetValue(texPoD);
                                engine.CommitChanges();
                                foreach (ModelMesh mesh in mPoD.Meshes)
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
                        if (instruments[2])
                        {//drums
                            matTranslate = Matrix.CreateTranslation(4, 105, -9);
                            matRot = Matrix.CreateRotationZ(MathHelper.PiOver4 / 2) * Matrix.CreateRotationY(MathHelper.PiOver2);
                            matScale = Matrix.CreateScale(2, 2, 2);

                            engine.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                            engine.Parameters["wRot"].SetValue(matRot * Matrix.CreateRotationX(MathHelper.Pi / 2));

                            engine.Parameters["shininess"].SetValue(0.25f);
                            engine.Parameters["SpecularEnabled"].SetValue(false);
                            engine.Parameters["vertexAlpha"].SetValue(false);
                            engine.Parameters["BumpMappingEnabled"].SetValue(false);


                            if (diff[2] == 0)
                            {
                                engine.Parameters["diffuseTexture"].SetValue(texSticks);
                                engine.CommitChanges();
                                foreach (ModelMesh mesh in mSticks.Meshes)
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
                            else if (diff[2] == 1)
                            {
                                engine.Parameters["diffuseTexture"].SetValue(texDTDSticks);
                                engine.CommitChanges();
                                foreach (ModelMesh mesh in mDrumsticks.Meshes)
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
                            else if (diff[2] == 2)
                            {
                                engine.Parameters["diffuseTexture"].SetValue(texDSticks);
                                engine.CommitChanges();
                                foreach (ModelMesh mesh in mDrumsticks.Meshes)
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
                            else
                            {
                                engine.Parameters["diffuseTexture"].SetValue(texTitDSticks);
                                engine.CommitChanges();
                                foreach (ModelMesh mesh in mDrumsticks.Meshes)
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
                        if (instruments[1])
                        {//drums
                            matTranslate = Matrix.CreateTranslation(-5, 105, -9);
                            matRot = Matrix.CreateRotationZ(MathHelper.PiOver4) * Matrix.CreateRotationY(MathHelper.PiOver2);
                            matScale = Matrix.CreateScale(1, 1, 1);

                            engine.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                            engine.Parameters["wRot"].SetValue(matRot * Matrix.CreateRotationX(MathHelper.Pi / 2));

                            engine.Parameters["shininess"].SetValue(0.25f);
                            engine.Parameters["SpecularEnabled"].SetValue(false);
                            engine.Parameters["vertexAlpha"].SetValue(false);
                            engine.Parameters["BumpMappingEnabled"].SetValue(false);


                            if (diff[1] == 0)
                            {
                                engine.Parameters["diffuseTexture"].SetValue(texTube);
                                engine.CommitChanges();
                                foreach (ModelMesh mesh in mTube.Meshes)
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
                            else if (diff[1] == 1)
                            {
                                engine.Parameters["diffuseTexture"].SetValue(texCMic);
                                engine.CommitChanges();
                                foreach (ModelMesh mesh in mCMic.Meshes)
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
                            else if (diff[1] == 2)
                            {
                                engine.Parameters["diffuseTexture"].SetValue(texMic);
                                engine.CommitChanges();
                                foreach (ModelMesh mesh in mMic.Meshes)
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
                            else
                            {
                                engine.Parameters["diffuseTexture"].SetValue(texAMic);
                                engine.CommitChanges();
                                foreach (ModelMesh mesh in mAMic.Meshes)
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
                        if (instruments[3])
                        {//drums
                            matTranslate = Matrix.CreateTranslation(12, 105, -9);
                            matRot = Matrix.CreateRotationZ(MathHelper.PiOver4) * Matrix.CreateRotationY(MathHelper.PiOver2);
                            matScale = Matrix.CreateScale(1, 1, 1);

                            engine.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                            engine.Parameters["wRot"].SetValue(matRot * Matrix.CreateRotationX(MathHelper.Pi / 2));

                            engine.Parameters["shininess"].SetValue(0.25f);
                            engine.Parameters["SpecularEnabled"].SetValue(false);
                            engine.Parameters["vertexAlpha"].SetValue(false);
                            engine.Parameters["BumpMappingEnabled"].SetValue(false);


                            if (diff[3] == 0)
                            {
                                engine.Parameters["diffuseTexture"].SetValue(texString);
                                engine.CommitChanges();
                                foreach (ModelMesh mesh in mString.Meshes)
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
                            else if (diff[3] == 1)
                            {
                                engine.Parameters["diffuseTexture"].SetValue(texStrap1);
                                engine.CommitChanges();
                                foreach (ModelMesh mesh in mStrap.Meshes)
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
                            else if (diff[3] == 2)
                            {
                                engine.Parameters["diffuseTexture"].SetValue(texStrap2);
                                engine.CommitChanges();
                                foreach (ModelMesh mesh in mStrap.Meshes)
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
                            else
                            {
                                engine.Parameters["diffuseTexture"].SetValue(texStrap3);
                                engine.CommitChanges();
                                foreach (ModelMesh mesh in mStrap.Meshes)
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

                            graphics.GraphicsDevice.VertexDeclaration = vd;
                            graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                            graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                            graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                            graphics.GraphicsDevice.Vertices[0].SetSource(square, 0, GBVertexFormat.SizeInBytes);
                            graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
                            graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
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

                            graphics.GraphicsDevice.VertexDeclaration = vd;
                            graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                            graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                            graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                            graphics.GraphicsDevice.Vertices[0].SetSource(square, 0, GBVertexFormat.SizeInBytes);
                            graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
                            graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
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

                            graphics.GraphicsDevice.VertexDeclaration = vd;
                            graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                            graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                            graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                            graphics.GraphicsDevice.Vertices[0].SetSource(square, 0, GBVertexFormat.SizeInBytes);
                            graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
                            graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
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

                            graphics.GraphicsDevice.VertexDeclaration = vd;
                            graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                            graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                            graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                            graphics.GraphicsDevice.Vertices[0].SetSource(square, 0, GBVertexFormat.SizeInBytes);
                            graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
                            graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                        }

                        pass.End();
                    }
                    engine.End();

                    spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
                    spritebatch.Draw(tbgGreen, new Rectangle((int)(0.1f * windowwidth), (int)(0.80f * windowheight), (int)(0.09f * windowheight), (int)(0.09f * windowheight)), Color.White);
                    spritebatch.DrawString(DefaultFont, "Select", new Vector2((0.1f * windowwidth) + (0.10f * windowheight), (0.80f * windowheight) + (0.09f * windowheight) - (DefaultFont.MeasureString("Select").Y)), Color.White);
                    spritebatch.Draw(tbgRed, new Rectangle((int)(0.9f * windowwidth) - (int)(0.09f * windowheight), (int)(0.80f * windowheight), (int)(0.09f * windowheight), (int)(0.09f * windowheight)), Color.White);
                    spritebatch.DrawString(DefaultFont, "Back", new Vector2((0.9f * windowwidth) - (0.10f * windowheight) - DefaultFont.MeasureString("Back").X, (0.80f * windowheight) + (0.09f * windowheight) - (DefaultFont.MeasureString("Back").Y)), Color.White);
                    if (DemoMode)
                    {
                        spritebatch.DrawString(BigFont, "Demo Mode", new Vector2((windowwidth / 2) - (BigFont.MeasureString("Demo Mode").X / 2), windowheight * 0.15f), new Color(255, 0, 0, 64));
                        spritebatch.DrawString(BigFont, "Demo Mode", new Vector2((windowwidth / 2) - (BigFont.MeasureString("Demo Mode").X / 2), windowheight * 0.4f), new Color(255, 0, 0, 64));
                        spritebatch.DrawString(BigFont, "Demo Mode", new Vector2((windowwidth / 2) - (BigFont.MeasureString("Demo Mode").X / 2), windowheight * 0.65f), new Color(255, 0, 0, 64));
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
                #endregion
                #region songscreen
                else if (screen == S_CHOOSESONG)
                {
#if !DEBUG
                    try
                    {
#endif
                    graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
                    graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
                    graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;
                    //graphics.PreferMultiSampling = true;
                    graphics.ApplyChanges();

                    vd = new VertexDeclaration(graphics.GraphicsDevice, GBVertexFormat.Elements);
                    graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
                    //graphics.GraphicsDevice.

                    engine.Parameters["bumpTexture"].SetValue(texDefaultBM);
                    engine.Parameters["ambientColor"].SetValue(new Vector4(0.4f, 0.4f, 0.4f, 1.0f));
                    engine.Parameters["diffuseColor"].SetValue(new Vector4(1f, 1f, 1f, 1.0f));
                    engine.Parameters["specularColor"].SetValue(new Vector4(1f, 1f, 1f, 1.0f));

                    bool[] plo = new bool[16];
                    Vector3[] plp = new Vector3[16];
                    float[] pln = new float[16];
                    float[] plf = new float[16];
                    Vector3[] pld = new Vector3[16];
                    Vector3[] pls = new Vector3[16];
                    engine.Parameters["pLightOn"].SetValue(plo);
                    engine.Parameters["pLightPos"].SetValue(plp);
                    engine.Parameters["pLightNear"].SetValue(pln);
                    engine.Parameters["pLightFar"].SetValue(plf);
                    engine.Parameters["pLightDiffuse"].SetValue(pld);
                    engine.Parameters["pLightSpecular"].SetValue(pls);
                    engine.Parameters["dLDiffuseColor"].SetValue(new Vector4(0.4f, 0.4f, 0.4f, 1.0f));
                    engine.Parameters["dLSpecularColor"].SetValue(new Vector4(0.0f, 0.0f, 0.0f, 1.0f));
                    engine.Parameters["dLightDir"].SetValue(Vector3.Normalize(new Vector3(0, 3, 1)));

                    Version SM = graphics.GraphicsDevice.GraphicsDeviceCapabilities.PixelShaderVersion;
                    if (SM.Major >= 3)
                        engine.CurrentTechnique = engine.Techniques["menutechnique"];
                    else if (SM.Major >= 2)
                        engine.CurrentTechnique = engine.Techniques["menutechniquet"];
                    else
                        Exit();
                    engine.CommitChanges();

                    engine.Begin();
                    foreach (EffectPass pass in engine.CurrentTechnique.Passes)
                    {
                        pass.Begin();
                        SetProjMatrix(Window.ClientBounds.Width, Window.ClientBounds.Height);
                        engine.Parameters["fullbright"].SetValue(false);

                        matView = Matrix.CreateLookAt(new Vector3(0, 128, 128), new Vector3(0, 128, 0), new Vector3(0, 1, 0));
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

                            songlistVB = new VertexBuffer(graphics.GraphicsDevice, GBVertexFormat.SizeInBytes * (SONGLIST_WAVEQUALITY - 1) * 6, BufferUsage.WriteOnly);

                            songlistVB.SetData<GBVertexFormat>(songlistGeom);

                            matRot = Matrix.CreateRotationY((float)Math.PI / -12);
                            matTranslate = Matrix.CreateTranslation(-10, 20, 0);
                            matScale = Matrix.CreateScale(new Vector3(SONGLIST_WIDTH, 1, 1));
                            engine.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                            engine.Parameters["wRot"].SetValue(matRot);
                            engine.Parameters["diffuseTexture"].SetValue(SongListTex);
                            engine.Parameters["bumpTexture"].SetValue(texDefaultBM);
                            engine.Parameters["shininess"].SetValue(0.25f);
                            engine.Parameters["SpecularEnabled"].SetValue(false);
                            engine.Parameters["vertexAlpha"].SetValue(false);
                            engine.Parameters["BumpMappingEnabled"].SetValue(false);
                            engine.CommitChanges();

                            graphics.GraphicsDevice.VertexDeclaration = vd;
                            graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                            graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                            graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                            graphics.GraphicsDevice.Vertices[0].SetSource(songlistVB, 0, GBVertexFormat.SizeInBytes);
                            graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, (SONGLIST_WAVEQUALITY - 1) * 2);
                            graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                        }

                        engine.Parameters["fullbright"].SetValue(false);
                        engine.Parameters["wAlpha"].SetValue(1.0f);

                        pass.End();
                    }
                    engine.End();
                    spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
                    spritebatch.Draw(songchoosetop, new Rectangle(0, 0, windowwidth, (int)((windowheight / 768f) * 256)), Color.White);
                    spritebatch.Draw(tbgGreen, new Rectangle((int)(0.1f * windowwidth), (int)(0.80f * windowheight), (int)(0.09f * windowheight), (int)(0.09f * windowheight)), Color.White);
                    spritebatch.DrawString(DefaultFont, "Select", new Vector2((0.1f * windowwidth) + (0.10f * windowheight), (0.80f * windowheight) + (0.09f * windowheight) - (DefaultFont.MeasureString("Select").Y)), Color.White);
                    spritebatch.Draw(tbgRed, new Rectangle((int)(0.9f * windowwidth) - (int)(0.09f * windowheight), (int)(0.80f * windowheight), (int)(0.09f * windowheight), (int)(0.09f * windowheight)), Color.White);
                    spritebatch.DrawString(DefaultFont, "Back", new Vector2((0.9f * windowwidth) - (0.10f * windowheight) - DefaultFont.MeasureString("Back").X, (0.80f * windowheight) + (0.09f * windowheight) - (DefaultFont.MeasureString("Back").Y)), Color.White);
                    if (DemoMode)
                    {
                        spritebatch.DrawString(BigFont, "Demo Mode", new Vector2((windowwidth / 2) - (BigFont.MeasureString("Demo Mode").X / 2), windowheight * 0.15f), new Color(255, 0, 0, 64));
                        spritebatch.DrawString(BigFont, "Demo Mode", new Vector2((windowwidth / 2) - (BigFont.MeasureString("Demo Mode").X / 2), windowheight * 0.4f), new Color(255, 0, 0, 64));
                        spritebatch.DrawString(BigFont, "Demo Mode", new Vector2((windowwidth / 2) - (BigFont.MeasureString("Demo Mode").X / 2), windowheight * 0.65f), new Color(255, 0, 0, 64));
                    }
                    spritebatch.End();
#if !DEBUG
                    }
                    catch(Exception e)
                    {
#if WINDOWS
                        System.Windows.Forms.MessageBox.Show("Problem in Draw/SS/Pt1\n"+e.Message+"\n"+e.StackTrace);
#endif
                        Exit();
                        return;
                    }
#endif

                }
                #endregion
                #region contchoosescreen
                else if (screen == S_CHOOSECONT)
                {
                    float vmul = 2f, hmul = 2f;
                    int linum = 0;
                    bool[] plo = new bool[16];
                    Vector3[] plp = new Vector3[16];
                    float[] pln = new float[16];
                    float[] plf = new float[16];
                    Vector3[] pld = new Vector3[16];
                    Vector3[] pls = new Vector3[16];
#if !DEBUG
                    try
                    {
#endif
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


                    for (int i = 0; i < contguis.Length; i++)
                    {
                        if (contguis[i].status >= 2)
                        {
                            plo[linum] = true;
                            plp[linum] = new Vector3(-192 + (contguis[i].loc * 80), 192, -100);
                            pln[linum] = r.Next(64);
                            plf[linum] = r.Next(64) + 65;
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

                    Version SM = graphics.GraphicsDevice.GraphicsDeviceCapabilities.PixelShaderVersion;
                    if (SM.Major >= 3)
                        engine.CurrentTechnique = engine.Techniques["menutechnique"];
                    else if (SM.Major >= 2)
                        engine.CurrentTechnique = engine.Techniques["menutechniquet"];
                    else
                        Exit();
                    engine.CommitChanges();
#if !DEBUG
                    }
                    catch(Exception e)
                    {
#if WINDOWS
                        System.Windows.Forms.MessageBox.Show("Problem in Draw/CCS/Pt1\n"+e.Message+"\n"+e.StackTrace);
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
                        SetProjMatrix(Window.ClientBounds.Width, Window.ClientBounds.Height);

                        engine.Parameters["fullbright"].SetValue(false);

                        if (idleTime < 29.5)
                            matView = Matrix.CreateLookAt(new Vector3(-8, 128, 150), new Vector3(-8, 128, 0), new Vector3(0, 1, 0));
                        else
                            matView = Matrix.CreateLookAt(new Vector3(-8, 128, 150), new Vector3(-24 + ((idleTime * hmul) % 1 < 0.5 ? (idleTime * hmul) % 0.5f * 32 : (1 - ((idleTime * hmul) % .5f * 2)) * 16), 128 - ((idleTime * vmul) % 1 < 0.5 ? (idleTime * vmul) % 0.5f * 32 : (1 - ((idleTime * vmul) % .5f * 2)) * 16), 0), new Vector3(0, 1, 0));
                        //render the background graphics
                        engine.Parameters["view"].SetValue(matView);
                        engine.Parameters["proj"].SetValue(matProj);
                        engine.Parameters["viewInverse"].SetValue(Matrix.Invert(matView));

                        Matrix matRot, matScale, matTranslate;
                        {
                            matTranslate = Matrix.CreateTranslation(-32, 120, -128);
                            matRot = Matrix.CreateRotationX((float)Math.PI / 2);
                            matScale = Matrix.CreateScale(300, 1, 192);

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
                            if (contguis[i].status == 1)
                            {
                                matTranslate = Matrix.CreateTranslation(-96 + (64 * (contguis[i].loc - 1)) - 24, 180, -120);
                                matRot = Matrix.CreateRotationZ(arrowRot[(int)(contguis[i].loc - 1) * 2]) * Matrix.CreateRotationY(-(float)Math.PI / 2) * Matrix.CreateRotationZ(-MathHelper.PiOver2);
                                matScale = Matrix.CreateScale(4, 2, 4);

                                engine.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                                engine.Parameters["wRot"].SetValue(matRot);
                                engine.Parameters["diffuseTexture"].SetValue(arrowTex);
                                engine.Parameters["bumpTexture"].SetValue(texDefaultBM);
                                engine.Parameters["diffuseColor"].SetValue(charNameSelected[(int)(contguis[i].loc - 1)] > -1 ? new Vector4(0f, 1f, 0f, 1f) : new Vector4(1f, 0f, 0f, 1f));
                                engine.Parameters["specularColor"].SetValue(charNameSelected[(int)(contguis[i].loc - 1)] > -1 ? new Vector4(0f, 1f, 0f, 1f) : new Vector4(1f, 0f, 0f, 1f));
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

                                matTranslate = Matrix.CreateTranslation(-96 + (64 * (int)(contguis[i].loc - 1)) + 24, 180, -120);
                                matRot = Matrix.CreateRotationZ(arrowRot[(int)(contguis[i].loc - 1) * 2 + 1]) * Matrix.CreateRotationY((float)Math.PI / 2) * Matrix.CreateRotationZ(-MathHelper.PiOver2);
                                matScale = Matrix.CreateScale(4, 2, 4);

                                engine.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                                engine.Parameters["wRot"].SetValue(matRot);
                                engine.Parameters["diffuseColor"].SetValue(charNameSelected[(int)(contguis[i].loc - 1)] < charNames[(int)(contguis[i].loc - 1) < 3 ? (int)(contguis[i].loc - 1) : 0].Length - 1 ? new Vector4(0f, 1f, 0f, 1f) : new Vector4(1f, 0f, 0f, 1f));
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
                            matTranslate = Matrix.CreateTranslation(100, 85, -80);
                            matRot = Matrix.CreateRotationY(-(float)Math.PI / 4);
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
                            matTranslate = Matrix.CreateTranslation(105, 92, -85);
                            matRot = Matrix.CreateRotationY((float)Math.PI / 2) * Matrix.CreateRotationX((float)Math.PI / 2 - 0.2f) * Matrix.CreateRotationY(-(float)Math.PI / 4);
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
                                    matScale = Matrix.CreateScale(8, 1, Math.Max(24 * (1 - flames[k][i].Z), 4));

                                    engine.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                                    engine.Parameters["wRot"].SetValue(matRot);
                                    engine.Parameters["diffuseColor"].SetValue(new Vector4(0.8f, 0.8f, 0.8f, 1.0f));
                                    engine.Parameters["specularColor"].SetValue(new Vector4(0f, 0f, 0f, 1f));
                                    float alpha = 0;
                                    if (flames[k][i].Z > 3 / 4f)
                                        alpha = 1 - ((flames[k][i].Z - 3 / 4f) * 4);
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
                                engine.Parameters["diffuseTexture"].SetValue(contguis[0].loc <= 0.99 ? ContGUIData.KB_ICO_BLUR : contguis[0].loc <= 1.99 ? ContGUIData.KB_ICO_GUITAR : contguis[0].loc <= 2.99 ? ContGUIData.KB_ICO_VOCAL : contguis[0].loc <= 3.99 ? ContGUIData.KB_ICO_DRUM : ContGUIData.KB_ICO_GUITAR);
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
                                engine.Parameters["diffuseTexture"].SetValue(contguis[0].loc <= 1 ? ContGUIData.KB_ICO_GUITAR : contguis[0].loc <= 2 ? ContGUIData.KB_ICO_VOCAL : contguis[0].loc <= 3 ? ContGUIData.KB_ICO_DRUM : ContGUIData.KB_ICO_GUITAR);
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
                        for (int j = 1; j <= 4; j++)
                        {//instrument gui
                            {
                                matTranslate = Matrix.CreateTranslation(new Vector3(contguis[j].info.X, contguis[j].info.Y, contguis[j].info.Z - contguis[j].loc));
                                matRot = Matrix.CreateRotationX((float)Math.PI / 2) * Matrix.CreateRotationZ(contguis[j].info.W);
                                matScale = Matrix.CreateScale(32, 32, 32);

                                engine.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                                engine.Parameters["wRot"].SetValue(matRot);
                                if (contguis[j].type == ContGUIData.CONT_TYPE.DRUMSET)
                                    engine.Parameters["diffuseTexture"].SetValue(contguis[j].loc <= 0.99 ? ContGUIData.DRUMS_ICO_BLUR : ContGUIData.DRUMS_ICO);
                                else if (contguis[j].type == ContGUIData.CONT_TYPE.STRATOCASTER)
                                    engine.Parameters["diffuseTexture"].SetValue(contguis[j].loc <= 0.99 ? ContGUIData.GUITAR_ICO_BLUR : ContGUIData.GUITAR_ICO);
                                else if (contguis[j].type == ContGUIData.CONT_TYPE.XPLORER)
                                    engine.Parameters["diffuseTexture"].SetValue(contguis[j].loc <= 0.99 ? ContGUIData.GUITARX_ICO_BLUR : ContGUIData.GUITARX_ICO);
                                else if (contguis[j].type == ContGUIData.CONT_TYPE.MICROPHONE)
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
                            if (contguis[j].loc > 0 && contguis[j].loc < 1)
                            {
                                matTranslate = Matrix.CreateTranslation(new Vector3(contguis[j].info.X, contguis[j].info.Y, contguis[j].info.Z - contguis[j].loc));
                                matRot = Matrix.CreateRotationX((float)Math.PI / 2) * Matrix.CreateRotationZ(contguis[j].info.W);
                                matScale = Matrix.CreateScale(32, 32, 32);

                                engine.Parameters["world"].SetValue(matScale * matRot * matTranslate);
                                engine.Parameters["wRot"].SetValue(matRot);
                                if (contguis[j].type == ContGUIData.CONT_TYPE.DRUMSET)
                                    engine.Parameters["diffuseTexture"].SetValue(ContGUIData.DRUMS_ICO);
                                else if (contguis[j].type == ContGUIData.CONT_TYPE.STRATOCASTER)
                                    engine.Parameters["diffuseTexture"].SetValue(ContGUIData.GUITAR_ICO);
                                else if (contguis[j].type == ContGUIData.CONT_TYPE.XPLORER)
                                    engine.Parameters["diffuseTexture"].SetValue(ContGUIData.GUITARX_ICO);
                                else if (contguis[j].type == ContGUIData.CONT_TYPE.MICROPHONE)
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
#if !DEBUG
                    }
                    catch (Exception e)
                    {
#if WINDOWS
                        System.Windows.Forms.MessageBox.Show("Problem in Draw/CCS/Pt2\n"+e.Message+"\n"+e.StackTrace);
#endif
                        Exit();
                        return;
                    }
                    try
                    {
#endif
                    spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);

                    if (leader >= 0 && contguis[leader].status == 2)
                        //spritebatch.Draw(texContinue, new Rectangle((int)(Window.ClientBounds.Width * (contguis[leader].loc) / 6), Window.ClientBounds.Height - (Window.ClientBounds.Height / 5), Window.ClientBounds.Width / 6, Window.ClientBounds.Height / 6), Color.Red);

                        if (idleTime > 30)
                        {
                            spritebatch.Draw(hairl, new Rectangle(-20, 0, (int)Window.ClientBounds.Height - (int)((idleTime * hmul) % 1 < 0.5 ? (idleTime * hmul) % 0.5f * (Window.ClientBounds.Height * 2) : (1 - ((idleTime * hmul) % .5f * 2)) * Window.ClientBounds.Height), (int)Window.ClientBounds.Height), Color.White);
                            spritebatch.Draw(hairr, new Rectangle(20 + (int)Window.ClientBounds.Width - (int)((idleTime * hmul) % 1 < 0.5 ? (idleTime * hmul) % 0.5f * (Window.ClientBounds.Height * 2) : (1 - ((idleTime * hmul) % .5f * 2)) * Window.ClientBounds.Height), 0, (int)((idleTime * hmul) % 1 < 0.5 ? (idleTime * hmul) % 0.5f * (Window.ClientBounds.Height * 2) : (1 - ((idleTime * hmul) % .5f * 2)) * Window.ClientBounds.Height), (int)Window.ClientBounds.Height), Color.White);
                        }

                    spritebatch.Draw(tbgGreen, new Rectangle((int)(0.1f * windowwidth), (int)(0.80f * windowheight), (int)(0.09f * windowheight), (int)(0.09f * windowheight)), Color.White);
                    if (leader >= 0 && contguis[leader].status == 2)
                        spritebatch.DrawString(DefaultFont, "Continue", new Vector2((0.10f * windowwidth) + (int)(0.1f * windowheight), (0.90f * windowheight) - (DefaultFont.MeasureString("Continue").Y)), Color.White);
                    else
                        spritebatch.DrawString(DefaultFont, "Select", new Vector2((0.10f * windowwidth) + (int)(0.1f * windowheight), (0.90f * windowheight) - (DefaultFont.MeasureString("Select").Y)), Color.White);
                    spritebatch.Draw(tbgRed, new Rectangle((int)(0.80f * windowwidth), (int)(0.80f * windowheight), (int)(0.09f * windowheight), (int)(0.09f * windowheight)), Color.White);
                    spritebatch.DrawString(DefaultFont, "Back", new Vector2((0.80f * windowwidth) - DefaultFont.MeasureString("Back").X, (0.90f * windowheight) - (DefaultFont.MeasureString("Back").Y)), Color.White);
                    //spritebatch.DrawString(DefaultFont, "" + contguis[0].type+","+GamePad.GetState(PlayerIndex.One).IsConnected + ","+ GamePad.GetCapabilities(PlayerIndex.One).GamePadType, new Vector2(100, 100), Color.Red);

                    //spritebatch.DrawString(DefaultFont, "" + contguis[0].loc + "::" + contguis[0].info, new Vector2(10, 10), Color.White);
                    if (DemoMode)
                    {
                        spritebatch.DrawString(BigFont, "Demo Mode", new Vector2((windowwidth / 2) - (BigFont.MeasureString("Demo Mode").X / 2), windowheight * 0.15f), new Color(255, 0, 0, 64));
                        spritebatch.DrawString(BigFont, "Demo Mode", new Vector2((windowwidth / 2) - (BigFont.MeasureString("Demo Mode").X / 2), windowheight * 0.4f), new Color(255, 0, 0, 64));
                        spritebatch.DrawString(BigFont, "Demo Mode", new Vector2((windowwidth / 2) - (BigFont.MeasureString("Demo Mode").X / 2), windowheight * 0.65f), new Color(255, 0, 0, 64));
                    }
                    spritebatch.End();
#if !DEBUG
                    }
                    catch (Exception e)
                    {
#if WINDOWS
                        System.Windows.Forms.MessageBox.Show("Problem in Draw/CCS/Pt3\n"+e.Message+"\n"+e.StackTrace);
#endif
                        Exit();
                        return;
                    }
#endif
                }
                #endregion
                #region ingame
                else if (screen == S_INGAME)
                {
                    long currenttime = (long)(CurrentTime / (long)(TicksPerSecond / 1000));
#if !DEBUG

                    try
                    {
#endif
                    graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;
                    graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
                    graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
                    vd = new VertexDeclaration(graphics.GraphicsDevice, GBVertexFormat.Elements);
                    //graphics.PreferMultiSampling = true;
                    graphics.ApplyChanges();
                    Version SM = graphics.GraphicsDevice.GraphicsDeviceCapabilities.PixelShaderVersion;
                    if (SM.Major >= 3)
                        engine.CurrentTechnique = engine.Techniques["maintechnique"];
                    else if (SM.Major >= 2)
                        engine.CurrentTechnique = engine.Techniques["maintechniquet"];
                    else
                    {
#if !XBOX
                        System.Windows.Forms.MessageBox.Show("Error. Must have minimum of Shader Model 2.0");
#endif
                        Exit();
                    }
                    if (renderLevel > 0)
                    {
                        graphics.GraphicsDevice.Clear(Color.Black);
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

                        matProj = venue.GetProjMatrix(windowwidth / (float)windowheight);
                        graphics.GraphicsDevice.Clear(Color.Black);
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
#if !DEBUG
                    }
                    catch(Exception e)
                    {
#if WINDOWS
                        System.Windows.Forms.MessageBox.Show("Problem in Draw/IG/Pt2\n"+e.Message+"\n"+e.StackTrace);
#endif
                        Exit();
                        return;
                    }

                    try
                    {
#endif

                    for (int i = 0; i < boards.Length; i++)
                    {
                        if (!instruments[i])
                            continue;

                        if (i != 1)
                            DrawBoardTarget(i, song.zVals, boards[i].IsSPActivated());

                        graphics.GraphicsDevice.SetRenderTarget(0, boardsTarget[i]);
                        graphics.GraphicsDevice.Clear(new Color(new Vector4(0, 0, 0, 0)));

                        if (i == 1)
                        {
                            spritebatch.Begin();
                            boards[i].Draw(spritebatch, (currenttime));
                            spritebatch.End();
                            continue;
                        }
                        graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;
                        graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
                        graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
                        graphics.ApplyChanges();

                        matProj = Matrix.CreatePerspectiveFieldOfView((float)Math.PI / 4.0f,
                                  boardsTarget[i].Width / (float)boardsTarget[i].Height,
                                  1f, 10.0f);
                        engine.CurrentTechnique = engine.Techniques["boardTechnique"];
                        engine.Begin();
                        foreach (EffectPass pass in engine.CurrentTechnique.Passes)
                        {
                            pass.Begin();

                            engine.Parameters["view"].SetValue(Matrix.Identity);
                            engine.Parameters["viewInverse"].SetValue(Matrix.Identity);
                            engine.Parameters["proj"].SetValue(matProj);

                            //get board measure world lengths


                            //determine fling (song start board comes up)
                            Matrix fling;
                            if (started == 1)
                            {
                                if (currenttime / 1000f < 3)
                                    fling = Matrix.CreateRotationX(Board.rotate);
                                else if (currenttime / 1000f < 4)
                                    fling = Matrix.CreateRotationX(((-((currenttime / 1000f) - 4f)) * Board.rotate * 4) - (Board.rotate * 3));
                                else
                                    fling = Matrix.CreateRotationX((float)Math.PI / 2);
                            }
                            else
                                fling = Matrix.CreateRotationX(Board.rotate);
#if DEBUG_CAM_CONTROL
                                    fling *= Matrix.CreateRotationX(-0.4f);
#endif

                            engine.Parameters["fullbright"].SetValue(true);
                            Matrix matTransl = Matrix.CreateTranslation(0f, Board.height + (boards[i].GetBoardBump() * Board.BOARD_BUMP_COEF), 0f);

                            //draw each boards
                            DrawBoard(i, fling, false, (long)CurrentTime, matTransl);
                            if (failStatus[i] == FS_GOOD)
                                DrawNotes(i, fling, (long)(started < 2 ? -CurrentTime : CurrentTime));
                            DrawBoardDetail(i, fling, matTransl);
                            if (failStatus[i] == FS_GOOD)
                            {
                                DrawWaves(i, fling, matTransl);
                                //draw the non-world gibs (glass shards sparks)
                                DrawGibs(i);
                                DrawFlashes(i, fling);
                            }
                            pass.End();
                        }
                        engine.End();
                        //spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.None);

                        //spritebatch.End();
                    }
#if !DEBUG
                    }
                    catch(Exception e)
                    {
#if WINDOWS
                        System.Windows.Forms.MessageBox.Show("Problem in Draw/IG/Pt3\n"+e.Message+"\n"+e.StackTrace);
#endif
                        Exit();
                        return;
                    }

                    try
                    {
#endif
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

                            spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
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
                            spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
                            spritebatch.Draw(screenTarget.GetTexture(), new Rectangle(0, 0, windowwidth, windowheight), Color.White);
                        }
                    }
                    else
                    {
                        graphics.GraphicsDevice.SetRenderTarget(0, null);
                        spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
                        graphics.GraphicsDevice.Clear(Color.Black);
                    }
#if !DEBUG
                    }
                    catch(Exception e)
                    {
#if WINDOWS
                        System.Windows.Forms.MessageBox.Show("Problem in Draw/IG/Pt4\n"+e.Message+"\n"+e.StackTrace);
#endif
                        Exit();
                        return;
                    }
                    //ppEngine.CurrentTechnique.Passes[0].End();
                    //ppEngine.End();
                    //spritebatch.End();
                    try
                    {
#endif
                    for (int i = 0; i < 4; i++)
                        if (instruments[i])
                            spritebatch.Draw(boardsTarget[i].GetTexture(), new Rectangle(boards[i].xOffset, 0, windowwidth, windowheight), Color.White);
                    //draw score/stars
                    DrawRockMeter(currenttime);
                    DrawScoreStars(currenttime);
                    //draw rock meter

                    //spritebatch.Draw(boards[0].SPMRTex, new Rectangle(0, 0, 256, 128), Color.White);

                    //draw development info
                    {
                        float fps = 0;
                        lastframes[frameIndex % lastframes.Length] = (float)gameTime.ElapsedGameTime.TotalSeconds;

                        frameIndex++;
                        for (int i = 0; i < lastframes.Length; i++)
                            fps += lastframes[i];
                        fps /= lastframes.Length;
                        fps = 1 / fps;
                        if (ShowFPS)
                            spritebatch.DrawString(DefaultFont, "" + (int)fps, new Vector2(windowwidth - 40, windowheight - 40), Color.Red);
                        //spritebatch.Draw(boards[0].texBoard, new Rectangle(0, 0, 300, 600), Color.White);
                        //spritebatch.Draw(boards[0].texBoard, new Rectangle(0, 10, 100, 200), Color.White);
                        //spritebatch.DrawString(DefaultFont, "" + venue.camindex, new Vector2(0,24), Color.Red);
                        //spritebatch.DrawString(DefaultFont, "" + (boards[2].lastPressed & bits[0]) + (boards[2].lastPressed & bits[1]) + (boards[2].lastPressed & bits[2]) + (boards[2].lastPressed & bits[3]) + (boards[2].lastPressed & bits[4]), new Vector2(0, 48), Color.Red);
                        //spritebatch.DrawString(DefaultFont, "" + boards[2].multiplier, new Vector2(0, 48), Color.Red);
                        //spritebatch.DrawString(DefaultFont, "" + controllers[contInput[0]].ThumbSticks.Right.Y, new Vector2(0, 48), Color.Red);

                    }

                    if (started < 2)
                    {
                        float alpha;
                        if (CurrentTime > 4 * TicksPerSecond)
                            alpha = 1 - (((float)CurrentTime - (4 * TicksPerSecond)) / (float)TicksPerSecond);
                        else if (CurrentTime < TicksPerSecond)
                            alpha = (float)CurrentTime / (float)TicksPerSecond;
                        else
                            alpha = 1;
                        Color aColor = new Color(new Vector4(1, 1, 1, alpha));
                        for (int i = 0; i < 2; i++)
                            spritebatch.DrawString(DefaultFont, song.songInfo[i], new Vector2((windowwidth / 2) - (DefaultFont.MeasureString(song.songInfo[i]).X / 2), 180 + (40 * i)), aColor);
                        for (int i = 2; i < song.songInfo.Length; i++)
                            spritebatch.DrawString(DefaultFont, song.songInfo[i], new Vector2((windowwidth / 2) - (DefaultFont.MeasureString(song.songInfo[i]).X / 2), 220 + (40 * i)), aColor);
                    }
                    if (IsPaused)
                    {
                        float scale = 1.5f;

                        Vector2 origin = new Vector2(texPauseBorder.Width / 2, texPauseBorder.Height / 2);
                        Vector3 wingPosR = new Vector3(455, 79, 0);
                        Vector3 wingPosL = new Vector3(25, 59, 0);
                        wingPosR -= new Vector3(origin, 0);
                        wingPosL -= new Vector3(origin, 0);
                        wingPosR *= scale;
                        wingPosL *= scale;
                        wingPosR = Vector3.Transform(wingPosR, Matrix.CreateRotationZ(pauseRot));
                        wingPosL = Vector3.Transform(wingPosL, Matrix.CreateRotationZ(pauseRot));

                        Vector3[] textPos = new Vector3[pauseTextDisp.Length];

                        for (int i = 0; i < textPos.Length; i++)
                        {
                            textPos[i] = new Vector3(texPauseBorder.Width / 2, ((350 - 68) * ((i + 1f) / (textPos.Length + 1f))) + 68, 0);
                            textPos[i] -= new Vector3(origin, 0);
                            textPos[i] *= scale;
                            textPos[i] = Vector3.Transform(textPos[i], Matrix.CreateRotationZ(pauseRot));
                        }

                        Vector3 pickPosL, pickPosR;

                        pickPosL = new Vector3(texPauseBorder.Width * 0.25f, ((350 - 68) * ((pauseSelected + 1f) / (textPos.Length + 1f))) + 68, 0);
                        pickPosL -= new Vector3(origin, 0);
                        pickPosL *= scale;
                        pickPosL = Vector3.Transform(pickPosL, Matrix.CreateRotationZ(pauseRot));

                        pickPosR = new Vector3(texPauseBorder.Width * 0.75f, ((350 - 68) * ((pauseSelected + 1f) / (textPos.Length + 1f))) + 68, 0);
                        pickPosR -= new Vector3(origin, 0);
                        pickPosR *= scale;
                        pickPosR = Vector3.Transform(pickPosR, Matrix.CreateRotationZ(pauseRot));

                        spritebatch.Draw(texPauseBorder, pauseMenuPos, null, Color.White, pauseRot, origin, scale, SpriteEffects.None, 0);
                        spritebatch.Draw(texPauseWings, new Vector2(wingPosR.X, wingPosR.Y) + pauseMenuPos, null, Color.White, -pauseWingRot.X / 2, new Vector2(32, 69), scale, SpriteEffects.None, 0);
                        spritebatch.Draw(texPauseWings, new Vector2(wingPosL.X, wingPosL.Y) + pauseMenuPos, null, Color.White, pauseWingRot.X / 2, new Vector2(texPauseWings.Width - 32, 69), scale, SpriteEffects.FlipHorizontally, 0);
                        for (int i = 0; i < textPos.Length; i++)
                            spritebatch.DrawString(DefaultFont, pauseTextDisp[i], new Vector2(textPos[i].X, textPos[i].Y) + pauseMenuPos, pauseSelected == i ? Color.Red : new Color(100, 128, 100), pauseRot, DefaultFont.MeasureString(pauseTextDisp[i]) * 0.5f, scale * 2, SpriteEffects.None, 0);
                        spritebatch.Draw(texPausePick, new Vector2(pickPosL.X, pickPosL.Y) + pauseMenuPos, null, Color.White, pauseRot, new Vector2(texPausePick.Width, texPausePick.Height / 2), scale / 3, SpriteEffects.None, 0);
                        spritebatch.Draw(texPausePick, new Vector2(pickPosR.X, pickPosR.Y) + pauseMenuPos, null, Color.White, pauseRot + MathHelper.Pi, new Vector2(texPausePick.Width, texPausePick.Height / 2), scale / 3, SpriteEffects.None, 0);
                    }
                    /*if (DemoMode)
                    {
                        spritebatch.DrawString(BigFont, "Demo Mode", new Vector2((windowwidth / 2) - (BigFont.MeasureString("Demo Mode").X / 2), windowheight * 0.15f), new Color(255, 0, 0, 64));
                        spritebatch.DrawString(BigFont, "Demo Mode", new Vector2((windowwidth / 2) - (BigFont.MeasureString("Demo Mode").X / 2), windowheight * 0.4f), new Color(255, 0, 0, 64));
                        spritebatch.DrawString(BigFont, "Demo Mode", new Vector2((windowwidth / 2) - (BigFont.MeasureString("Demo Mode").X / 2), windowheight * 0.65f), new Color(255, 0, 0, 64));
                    }*/


                    //spritebatch.DrawString(DefaultFont, "" + ((currenttime / 1000) / 3600) + ":" + ((currenttime / 1000) / 60 % 3600) + ":" + (currenttime / 1000 % 60), new Vector2(0, 0), Color.Wheat);
#if !DEBUG
                    }
                    catch(Exception e)
                    {
#if WINDOWS
                        System.Windows.Forms.MessageBox.Show("Problem in Draw/IG/Pt5\n"+e.Message+"\n"+e.StackTrace);
#endif
                        Exit();
                        return;
                    }
#endif
                    spritebatch.End();
                }
                #endregion
                #region freestyle
                else if (screen == S_FREESTYLE)
                {
#if !DEBUG

                    try
                    {
#endif
                    graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;
                    graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
                    graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
                    vd = new VertexDeclaration(graphics.GraphicsDevice, GBVertexFormat.Elements);
                    //graphics.PreferMultiSampling = true;
                    graphics.ApplyChanges();
                    Version SM = graphics.GraphicsDevice.GraphicsDeviceCapabilities.PixelShaderVersion;
                    if (SM.Major >= 3)
                        engine.CurrentTechnique = engine.Techniques["maintechnique"];
                    else if (SM.Major >= 2)
                        renderLevel = 0;
                    else
                    {
#if WINDOWS
                        System.Windows.Forms.MessageBox.Show("Problem in Draw\nYour graphics card does not support at least SM2.0");
#endif
                        Exit();
                    }
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
#if !DEBUG
                    }
                    catch(Exception e)
                    {
#if WINDOWS
                        System.Windows.Forms.MessageBox.Show("Problem in Draw/IG/Pt2\n"+e.Message);
#endif
                        Exit();
                        return;
                    }

                    try
                    {
#endif

                    for (int i = 0; i < instruments.Length; i++)
                    {
                        if (!instruments[i])
                            continue;

                        /*if (i == 1)
                        {
                            spritebatch.Begin();
                            boards[i].Draw(spritebatch,(currenttime));
                            spritebatch.End();
                            continue;
                        }*/

                        DrawBoardTargetFS(i);
                        graphics.GraphicsDevice.SetRenderTarget(0, boardsTarget[i]);
                        graphics.GraphicsDevice.Clear(new Color(new Vector4(0, 0, 0, 0)));
                        matProj = Matrix.CreatePerspectiveFieldOfView((float)Math.PI / 4.0f,
                                  boardsTarget[i].Width / (float)boardsTarget[i].Height,
                                  1f, 10.0f);
                        engine.CurrentTechnique = engine.Techniques["boardTechnique"];
                        engine.Begin();
                        foreach (EffectPass pass in engine.CurrentTechnique.Passes)
                        {
                            pass.Begin();

                            engine.Parameters["view"].SetValue(Matrix.Identity);
                            engine.Parameters["viewInverse"].SetValue(Matrix.Identity);
                            engine.Parameters["proj"].SetValue(matProj);

                            //get board measure world lengths


                            //determine fling (song start board comes up)
                            /*Matrix fling;
                            if (started == 1)
                            {
                                if (currenttime / 1000f < 3)
                                    fling = Matrix.CreateRotationX(Board.rotate);
                                else if (currenttime / 1000f < 4)
                                    fling = Matrix.CreateRotationX(((-((currenttime / 1000f) - 4f)) * Board.rotate * 4) - (Board.rotate * 3));
                                else
                                    fling = Matrix.CreateRotationX((float)Math.PI / 2);
                            }
                            else
                                fling = Matrix.CreateRotationX(Board.rotate);
#if DEBUG_CAM_CONTROL
                                fling *= Matrix.CreateRotationX(-0.4f);
#endif
                             */

                            engine.Parameters["fullbright"].SetValue(true);
                            Matrix matTransl = Matrix.CreateTranslation(0f, Board.height, 0f);

                            //draw each boards
                            DrawBoard(i, Matrix.CreateRotationX(Board.rotate), false, (long)CurrentTime, matTransl);
                            //DrawNotes(i, fling,started<2?-CurrentTime:CurrentTime);
                            DrawBoardDetailFS(i, Matrix.CreateRotationX(Board.rotate), matTransl);
                            //DrawWaves(i, fling,matTransl);
                            //draw the non-world gibs (glass shards sparks)
                            DrawGibsFS(i);
                            pass.End();
                        }
                        engine.End();
                        //spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.None);

                        //spritebatch.End();
                    }
#if !DEBUG
                    }
                    catch(Exception e)
                    {
#if WINDOWS
                        System.Windows.Forms.MessageBox.Show("Problem in Draw/IG/Pt3\n"+e.Message);
#endif
                        Exit();
                        return;
                    }

                    try
                    {
#endif
                    if (renderLevel > 0)
                    {
                        /*if (currentFES == FRAME_EFFECT_STYLE.CREST)
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
                        {*/
                        graphics.GraphicsDevice.SetRenderTarget(0, null);
                        graphics.GraphicsDevice.Clear(Color.Black);
                        spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
                        spritebatch.Draw(screenTarget.GetTexture(), new Rectangle(0, 0, windowwidth, windowheight), Color.White);
                        //}
                    }
                    else
                    {
                        graphics.GraphicsDevice.SetRenderTarget(0, null);
                        spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
                        graphics.GraphicsDevice.Clear(Color.Black);
                    }
#if !DEBUG
                    }
                    catch(Exception e)
                    {
#if WINDOWS
                        System.Windows.Forms.MessageBox.Show("Problem in Draw/IG/Pt4\n"+e.Message);
#endif
                        Exit();
                        return;
                    }
                    //ppEngine.CurrentTechnique.Passes[0].End();
                    //ppEngine.End();
                    //spritebatch.End();
                    try
                    {
#endif
                    for (int i = 0; i < 4; i++)
                        if (instruments[i])
                            spritebatch.Draw(boardsTarget[i].GetTexture(), new Rectangle(FSxOffset[i], 0, windowwidth, windowheight), Color.White);
                    //draw score/stars
                    //DrawScoreStars(currenttime);

                    //draw rock meter
                    //DrawRockMeter(currenttime);

                    //spritebatch.Draw(boards[0].SPMRTex, new Rectangle(0, 0, 256, 128), Color.White);

                    //draw development info
                    {
                        //spritebatch.DrawString(DefaultFont, "" + (int)(1000f/gameTime.ElapsedRealTime.Milliseconds), new Vector2(windowwidth-40, windowheight-40), Color.Red);
                        //spritebatch.Draw(boards[0].texBoard, new Rectangle(0, 0, 300, 600), Color.White);
                        //spritebatch.Draw(boards[0].texBoard, new Rectangle(0, 10, 100, 200), Color.White);
                        //spritebatch.DrawString(DefaultFont, "" + venue.camindex, new Vector2(0,24), Color.Red);
                        //spritebatch.DrawString(DefaultFont, "" + (boards[2].lastPressed & bits[0]) + (boards[2].lastPressed & bits[1]) + (boards[2].lastPressed & bits[2]) + (boards[2].lastPressed & bits[3]) + (boards[2].lastPressed & bits[4]), new Vector2(0, 48), Color.Red);
                        //spritebatch.DrawString(DefaultFont, "" + boards[2].multiplier, new Vector2(0, 48), Color.Red);
                        //spritebatch.DrawString(DefaultFont, "" + controllers[contInput[0]].ThumbSticks.Right.Y, new Vector2(0, 48), Color.Red);

                    }

                    /*if (started < 2)
                    {
                        float alpha;
                        if (CurrentTime > 4 * TicksPerSecond)
                            alpha = 1 - ((CurrentTime - (4 * TicksPerSecond)) / (float)TicksPerSecond);
                        else if (CurrentTime < TicksPerSecond)
                            alpha = CurrentTime / (float)TicksPerSecond;
                        else
                            alpha = 1;
                        Color aColor = new Color(new Vector4(1, 1, 1, alpha));
                        for (int i = 0; i < 2; i++)
                            spritebatch.DrawString(DefaultFont, song.songInfo[i], new Vector2((windowwidth / 2) - (DefaultFont.MeasureString(song.songInfo[i]).X / 2), 150 + (40 * i)), aColor);
                        for (int i = 2; i < song.songInfo.Length; i++)
                            spritebatch.DrawString(DefaultFont, song.songInfo[i], new Vector2((windowwidth / 2) - (DefaultFont.MeasureString(song.songInfo[i]).X / 2), 190 + (40 * i)), aColor);
                    }*/
#if !DEBUG
                    }
                    catch(Exception e)
                    {
#if WINDOWS
                        System.Windows.Forms.MessageBox.Show("Problem in Draw/IG/Pt5\n"+e.Message);
#endif
                        Exit();
                        return;
                    }
#endif
                    spritebatch.End();
                }
                #endregion
                #region results
                if (screen == S_RESULTS)
                {
                    graphics.GraphicsDevice.Clear(Color.Black);
                    spritebatch.Begin();
                    spritebatch.Draw(coolbg1, new Rectangle((int)(0.1 * windowwidth), (int)(0.1 * windowheight), (int)(0.8 * windowwidth), (int)(0.8 * windowheight)), new Color(30,30,30));
                    spritebatch.Draw(failbg, new Rectangle(0, 0, windowwidth, windowheight), Color.White);
                    int numRS = 4;
                    for (int i = -1; i < numRS; i++)
                        spritebatch.Draw(resultsScroller, new Rectangle((int)((dialogscroll * (windowwidth / (float)numRS)) + (i * (windowwidth / (float)numRS))), (int)((128f / 768) * windowheight), (int)(windowwidth / (float)numRS + 1), (int)((64f / 768) * windowheight)), Color.White);
                    for (int i = 0; i < numRS+1; i++)
                        spritebatch.Draw(resultsScroller, new Rectangle((int)((-dialogscroll * (windowwidth / (float)numRS)) + (i * (windowwidth / (float)numRS))), windowheight-(int)((192f / 768) * windowheight), (int)(windowwidth / (float)numRS + 1), (int)((64f / 768) * windowheight)), Color.White);
                    spritebatch.DrawString(DefaultFont, "Song Passed", new Vector2((windowwidth / 2) - (DefaultFont.MeasureString("Song Passed").X / 2), windowheight * 0.3f), Color.Green);
                    if (totalresults.Length != 0)
                        for (int i = 0; i < 5; i++)
                        {
                            if (i < 4 && instruments[i])
                            {
                                String str1=instrumentNames[i] + " - Notes: " + totalresults[i].hitNotes + "/" + totalresults[i].totalNotes,
                                       str2=instrumentNames[i] + " - Rock Power Phrases: "+totalresults[i].hitSPPH+"/"+totalresults[i].totalSPPH;
                                spritebatch.DrawString(DefaultFont,
                                                       str1,
                                                       new Vector2((windowwidth/2)-(DefaultFont.MeasureString(str1).X/2), (windowheight*0.4f) + (40 * i)), Color.Wheat);
                                spritebatch.DrawString(DefaultFont,
                                                        str2,
                                                       new Vector2((windowwidth/2)-(DefaultFont.MeasureString(str2).X/2), (windowheight*0.4f) + 20 + (40 * i)), Color.Wheat);
                            }
                                /*
                            else if (i == 4)
                                spritebatch.DrawString(DefaultFont,
                                                       "Total - Notes:" + totalresults[i].hitNotes + "/" +
                                                        totalresults[i].missedNotes + "?" + totalresults[i].totalNotes +
                                                        ", SPPH:" + totalresults[i].hitSPPH + "/" + totalresults[i].missedSPPH +
                                                        "?" + totalresults[i].totalSPPH,
                                                       new Vector2(10, 30 + 20 * i), Color.Green);*/

                        }
                    spritebatch.Draw(tbgGreen, new Rectangle((int)(0.01f * windowwidth), (int)(0.90f * windowheight), (int)(0.09f * windowheight), (int)(0.09f * windowheight)), Color.White);
                    spritebatch.DrawString(DefaultFont, "Continue", new Vector2((0.01f * windowwidth) + (0.10f * windowheight), (0.90f * windowheight)+(0.09f * windowheight) - (DefaultFont.MeasureString("Continue").Y)), Color.White);
                    if (DemoMode)
                    {
                        spritebatch.DrawString(BigFont, "Demo Mode", new Vector2((windowwidth / 2) - (BigFont.MeasureString("Demo Mode").X / 2), windowheight * 0.15f), new Color(255, 0, 0, 64));
                        spritebatch.DrawString(BigFont, "Demo Mode", new Vector2((windowwidth / 2) - (BigFont.MeasureString("Demo Mode").X / 2), windowheight * 0.4f), new Color(255, 0, 0, 64));
                        spritebatch.DrawString(BigFont, "Demo Mode", new Vector2((windowwidth / 2) - (BigFont.MeasureString("Demo Mode").X / 2), windowheight * 0.65f), new Color(255, 0, 0, 64));
                    }
                    spritebatch.End();
                }
                #endregion
                #region fail
                if (screen == S_FAIL)
                {
                    graphics.GraphicsDevice.Clear(Color.Black);
                    spritebatch.Begin();
                    spritebatch.Draw(coolbg2, new Rectangle((int)(0.1 * windowwidth), (int)(0.1 * windowheight), (int)(0.8 * windowwidth), (int)(0.8 * windowheight)), new Color(30,30,30));
                    spritebatch.Draw(failbg, new Rectangle(0, 0, windowwidth, windowheight), Color.White);
                    int numRS = 4;
                    for (int i = -1; i < numRS; i++)
                        spritebatch.Draw(resultsScroller, new Rectangle((int)((dialogscroll * (windowwidth / (float)numRS)) + (i * (windowwidth / (float)numRS))), (int)((128f / 768) * windowheight), (int)(windowwidth / (float)numRS + 1), (int)((64f / 768) * windowheight)), Color.White);
                    for (int i = 0; i < numRS+1; i++)
                        spritebatch.Draw(resultsScroller, new Rectangle((int)((-dialogscroll * (windowwidth / (float)numRS)) + (i * (windowwidth / (float)numRS))), windowheight-(int)((192f / 768) * windowheight), (int)(windowwidth / (float)numRS + 1), (int)((64f / 768) * windowheight)), Color.White);
                    if (totalresults.Length > 0)
                    {
                        String percent = "" + (int)(totalresults[4].percentSong * 100 + 0.5f) + "%";
                        spritebatch.DrawString(DefaultFont, "Failed", new Vector2((windowwidth / 2) - (DefaultFont.MeasureString("Failed").X / 2), windowheight * 0.3f), Color.Red);
                        spritebatch.DrawString(DefaultFont, percent, new Vector2((windowwidth / 2) - (DefaultFont.MeasureString(percent).X / 2), windowheight * 0.4f), Color.Red);
                    }
                    spritebatch.Draw(tbgGreen, new Rectangle((int)(0.01f * windowwidth), (int)(0.90f * windowheight), (int)(0.09f * windowheight), (int)(0.09f * windowheight)), Color.White);
                    spritebatch.DrawString(DefaultFont, "Retry", new Vector2((0.01f * windowwidth) + (0.10f * windowheight), (0.90f * windowheight)+(0.09f * windowheight) - (DefaultFont.MeasureString("Retry").Y)), Color.White);
                    spritebatch.Draw(tbgRed, new Rectangle((int)(0.99f * windowwidth)-(int)(0.09f * windowheight), (int)(0.90f * windowheight), (int)(0.09f * windowheight), (int)(0.09f * windowheight)), Color.White);
                    spritebatch.DrawString(DefaultFont, "Main Menu", new Vector2((0.99f * windowwidth) - (0.10f * windowheight) - DefaultFont.MeasureString("Main Menu").X, (0.90f * windowheight)+(0.09f * windowheight) - (DefaultFont.MeasureString("Main Menu").Y)), Color.White);
                    spritebatch.End();
                }
            }
                #endregion

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

        public float GetRockstarAmount()
        {
            float total=0, count=0;
            for(int i=0;i<4;i++)
                if (instruments[i])
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

        private void StarPowerAction(int i)
        {
            if (failStatus[i] != FS_GOOD)
                return;
            if (boards[i].GetSPAmount() < 0.5)
                return;
            if (failTime > 0)
            {
                for(int k=0;k<failStatus.Length;k++)
                    if (failStatus[k] == FS_FAILING)
                    {
                        failStatus[k] = FS_GOOD;
                        rockMeterLevel[k] = 80;
                        boards[i].EatHalfSP();
                        return;
                    }
            }
            boards[i].ActivateStarPower();
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
                    //if (TEST_SONG)
                    //    rockMeterLevel[i] = 99f;
                    ct++;
                    add += rockMeterLevel[i];
                }
            return (add / ct) / 100f;
        }

        public void InitForSong(bool guitarist, bool vocalist, bool percussionist, bool bassist, byte[] difficulty, String venueStr, Game1 gameRef) 
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
            for (int i = 0; i < 4; i++)
                if (instruments[i])
                    failStatus[i] = FS_GOOD;
                else
                    failStatus[i] = FS_DNE;
            boards = new Board[4];
            boardsTarget = new RenderTarget2D[4];
            if (guitarist && bassist && percussionist && vocalist)
            {
                //song = new Song(4, 2, songname,gameRef.Window.Handle);
                boards[0] = new Board(GUITAR, 0, song, difficulty[0]);
                boards[0].xOffset = -(int)(250f/800f*windowwidth);
                boards[1] = new Board(VOCALIST, 0, song, difficulty[1]);
                boards[2] = new Board(DRUMS, 0, song, difficulty[2]);
                boards[2].xOffset = 0;
                boards[3] = new Board(BASS, 0, song, difficulty[3]);
                boards[3].xOffset = (int)(250f/800f*windowwidth);
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
                Board.vocalzerox = windowwidth / 10;
                Board.vocalwidth = 0.25f;
                boardsTarget[1] = new RenderTarget2D(graphics.GraphicsDevice, windowwidth, windowheight, 1, SurfaceFormat.Color);
                boardsTarget[0] = new RenderTarget2D(graphics.GraphicsDevice, windowwidth, windowheight, 1, SurfaceFormat.Color);
                boardsTarget[2] = new RenderTarget2D(graphics.GraphicsDevice, windowwidth, windowheight, 1, SurfaceFormat.Color);
                boardsTarget[3] = new RenderTarget2D(graphics.GraphicsDevice, windowwidth, windowheight, 1, SurfaceFormat.Color);
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
                boardsTarget[0] = new RenderTarget2D(graphics.GraphicsDevice, windowwidth, windowheight, 1, SurfaceFormat.Color);
                boardsTarget[2] = new RenderTarget2D(graphics.GraphicsDevice, windowwidth, windowheight, 1, SurfaceFormat.Color);
                boardsTarget[3] = new RenderTarget2D(graphics.GraphicsDevice, windowwidth, windowheight, 1, SurfaceFormat.Color);
            }
            else if (guitarist && bassist && !percussionist && vocalist)
            {
                //song = new Song(4, 2, songname,gameRef.Window.Handle);
                boards[0] = new Board(GUITAR, 0, song, difficulty[0]);
                boards[0].xOffset = -(int)(175f/800f*windowwidth);
                boards[1] = new Board(VOCALIST, 0, song, difficulty[1]);
                boards[3] = new Board(BASS, 0, song, difficulty[3]);
                boards[3].xOffset = (int)(175f/800f*windowwidth);
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
                Board.vocalzerox = windowwidth / 10;
                Board.vocalwidth = 0.25f;
                boardsTarget[1] = new RenderTarget2D(graphics.GraphicsDevice, windowwidth, windowheight, 1, SurfaceFormat.Color);
                boardsTarget[0] = new RenderTarget2D(graphics.GraphicsDevice, windowwidth, windowheight, 1, SurfaceFormat.Color);
                boardsTarget[3] = new RenderTarget2D(graphics.GraphicsDevice, windowwidth, windowheight, 1, SurfaceFormat.Color);
            }
            else if (guitarist && !bassist && percussionist && vocalist)
            {
                //song = new Song(4, 2, songname,gameRef.Window.Handle);
                boards[0] = new Board(GUITAR, 0, song, difficulty[0]);
                boards[0].xOffset = -(int)(175f/800f*windowwidth);
                boards[1] = new Board(VOCALIST, 0, song, difficulty[1]);
                boards[2] = new Board(DRUMS, 0, song, difficulty[2]);
                boards[2].xOffset = (int)(175f/800f*windowwidth);
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
                Board.vocalzerox = windowwidth / 10;
                Board.vocalwidth = 0.25f;
                boardsTarget[1] = new RenderTarget2D(graphics.GraphicsDevice, windowwidth, windowheight, 1, SurfaceFormat.Color);
                boardsTarget[0] = new RenderTarget2D(graphics.GraphicsDevice, windowwidth, windowheight, 1, SurfaceFormat.Color);
                boardsTarget[2] = new RenderTarget2D(graphics.GraphicsDevice, windowwidth, windowheight, 1, SurfaceFormat.Color);
            }
            else if (!guitarist && bassist && percussionist && vocalist)
            {
                //song = new Song(4, 2, songname,gameRef.Window.Handle);
                boards[2] = new Board(DRUMS, 0, song, difficulty[2]);
                boards[2].xOffset = -(int)(175f/800f*windowwidth);
                boards[1] = new Board(VOCALIST, 0, song, difficulty[1]);
                boards[3] = new Board(BASS, 0, song, difficulty[3]);
                boards[3].xOffset = (int)(175f/800f*windowwidth);
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
                Board.vocalzerox = windowwidth / 10;
                Board.vocalwidth = 0.25f;
                boardsTarget[1] = new RenderTarget2D(graphics.GraphicsDevice, windowwidth, windowheight, 1, SurfaceFormat.Color);
                boardsTarget[2] = new RenderTarget2D(graphics.GraphicsDevice, windowwidth, windowheight, 1, SurfaceFormat.Color);
                boardsTarget[3] = new RenderTarget2D(graphics.GraphicsDevice, windowwidth, windowheight, 1, SurfaceFormat.Color);
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
                boardsTarget[0] = new RenderTarget2D(graphics.GraphicsDevice, windowwidth, windowheight, 1, SurfaceFormat.Color);
                boardsTarget[3] = new RenderTarget2D(graphics.GraphicsDevice, windowwidth, windowheight, 1, SurfaceFormat.Color);
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
                boardsTarget[2] = new RenderTarget2D(graphics.GraphicsDevice, windowwidth, windowheight, 1, SurfaceFormat.Color);
                boards[1] = new Board(VOCALIST, 0, song, difficulty[1]);
                Board.vocaly = 10;
                Board.vocalheight = 140;
                Board.vocalzerox = windowwidth / 10;
                Board.vocalwidth = 0.25f;
                boardsTarget[1] = new RenderTarget2D(graphics.GraphicsDevice, windowwidth, windowheight, 1, SurfaceFormat.Color);
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
                boardsTarget[0] = new RenderTarget2D(graphics.GraphicsDevice, windowwidth, windowheight, 1, SurfaceFormat.Color);
                boards[1] = new Board(VOCALIST, 0, song, difficulty[1]);
                Board.vocaly = 10;
                Board.vocalheight = 140;
                Board.vocalzerox = windowwidth / 10;
                Board.vocalwidth = 0.25f;
                boardsTarget[1] = new RenderTarget2D(graphics.GraphicsDevice, windowwidth, windowheight, 1, SurfaceFormat.Color);
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
                boardsTarget[3] = new RenderTarget2D(graphics.GraphicsDevice, windowwidth, windowheight, 1, SurfaceFormat.Color);
                boardsTarget[2] = new RenderTarget2D(graphics.GraphicsDevice, windowwidth, windowheight, 1, SurfaceFormat.Color);
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
                boardsTarget[0] = new RenderTarget2D(graphics.GraphicsDevice, windowwidth, windowheight, 1, SurfaceFormat.Color);
                boardsTarget[2] = new RenderTarget2D(graphics.GraphicsDevice, windowwidth, windowheight, 1, SurfaceFormat.Color);
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
                boardsTarget[3] = new RenderTarget2D(graphics.GraphicsDevice, windowwidth, windowheight, 1, SurfaceFormat.Color);
                boards[1] = new Board(VOCALIST, 0, song, difficulty[1]);
                Board.vocaly = 10;
                Board.vocalheight = 140;
                Board.vocalzerox = windowwidth / 10;
                Board.vocalwidth = 0.25f;
                boardsTarget[1] = new RenderTarget2D(graphics.GraphicsDevice, windowwidth, windowheight, 1, SurfaceFormat.Color);
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
                boardsTarget[0] = new RenderTarget2D(graphics.GraphicsDevice, windowwidth, windowheight, 1, SurfaceFormat.Color);
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
                boardsTarget[3] = new RenderTarget2D(graphics.GraphicsDevice, windowwidth, windowheight, 1, SurfaceFormat.Color);
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
                boardsTarget[2] = new RenderTarget2D(graphics.GraphicsDevice, windowwidth, windowheight, 1, SurfaceFormat.Color);
            }
            else if (!guitarist && !bassist && !percussionist && vocalist)
            {
                
                boards[1] = new Board(VOCALIST, 0, song, difficulty[1]);
                Board.vocaly = 10;
                Board.vocalheight = 140;
                Board.vocalzerox = windowwidth / 10;
                Board.vocalwidth = 0.25f;
                boardsTarget[1] = new RenderTarget2D(graphics.GraphicsDevice, windowwidth, windowheight, 1, SurfaceFormat.Color);
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
                { totalresults = new Results[0]; screen = S_MAINMENU; songname = ""; song = null; boards = null; boardsTarget = null; started = 0; mmenu_ticker = 200; mmenu_select = 0; contguis = new ContGUIData[5]; UnloadGameContent(); MenuLoaded = false; }
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
            for (int k = 0; k < boards[i].OutDFs.Length; k++)
            {
                float halfMaxWidth = rtBoard[i].Width / 8f;
                if (boards[i].OutDFs[k].W > 0.5)
                {
                    float height = (boards[i].OutDFs[k].Y - boards[i].OutDFs[k].X) * scale;
                    float y = (rtBoard[i].Height * ratio) - (int)(boards[i].OutDFs[k].X * scale) - (int)((boards[i].OutDFs[k].Y - boards[i].OutDFs[k].X) * scale);
                    for (int r = 0; r < 4; r++)
                    {
                        float center = ((r * 2 + 1)/8f)*rtBoard[i].Width;
                        spritebatch.Draw(Board.drumfillTex, new Rectangle((int)(center - (halfMaxWidth * boards[i].OutDFs[k].Z)), (int)y, (int)(2 * (halfMaxWidth * boards[i].OutDFs[k].Z)), (int)height), FretColors[Board.guitarToDrums[r]]);
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
                int height = (int)(windowheight*0.2f);
#if WINDOWS
                spritebatch.Draw(rmUNbg, new Rectangle(0, (windowheight / 2) - (int)(height * 0.75f), (int)(height * 0.5f), (int)(height*1.5f)), Color.White);
                spritebatch.Draw(rmUNfg, new Rectangle(0, (windowheight / 2), height / 2, height), null, rmColor, MathHelper.Pi - (rmFill * MathHelper.Pi), new Vector2(0, rmUNfg.Height / 2), SpriteEffects.None, 0);
                spritebatch.Draw(rmUNbg, new Rectangle(windowwidth, (windowheight / 2), (int)(height * 0.5f), (int)(height*1.5f)), null, Color.White, MathHelper.Pi, new Vector2(0,rmUNbg.Height/2),SpriteEffects.None,0);
                spritebatch.Draw(rmUNfg, new Rectangle(windowwidth, (windowheight / 2), height / 2, height), null, Color.Wheat, (song.PercentSong()*MathHelper.Pi),new Vector2(0,rmUNfg.Height/2),SpriteEffects.None,0);
#else
                spritebatch.Draw(rmUNbg, new Rectangle((int)(windowwidth*0), (windowheight / 2) - (int)(height * 0.75f), (int)(height * 0.5f), (int)(height*1.5f)), Color.White);
                spritebatch.Draw(rmUNfg, new Rectangle((int)(windowwidth * 0), (windowheight / 2), height / 2, height), null, rmColor, MathHelper.Pi - (rmFill * MathHelper.Pi), new Vector2(0, rmUNfg.Height / 2), SpriteEffects.None, 0);
                spritebatch.Draw(rmUNbg, new Rectangle((int)(windowwidth*1f), (windowheight / 2), (int)(height * 0.5f), (int)(height*1.5f)), null, Color.White, MathHelper.Pi, new Vector2(0,rmUNbg.Height/2),SpriteEffects.None,0);
                spritebatch.Draw(rmUNfg, new Rectangle((int)(windowwidth*1f), (windowheight / 2), height / 2, height), null, Color.Wheat, (song.PercentSong()*MathHelper.Pi),new Vector2(0,rmUNfg.Height/2),SpriteEffects.None,0);
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
                float height=(windowheight*0.15f);
                int ct = 0;
                for (int i = 0; i < 4; i++)
                    if (instruments[i])
                        ct++;
                /*for (int i = 0; i < (int)GetRockstarAmount(); i++)
                {
                    spritebatch.Draw(rmUNstar, new Vector2(0, windowheight / 2 + ((height / rmUNstaro.Height) * 0.5f * i)), null, GetRockstarAmount() < 5 ? FretColors[(int)GetRockstarAmount()] : Color.White, rockstarDir, new Vector2(rmUNstar.Width / 2, rmUNstar.Height / 2), (height / rmUNstaro.Height) * 0.5f, SpriteEffects.None, 0);
                    spritebatch.Draw(rmUNstar, new Vector2(windowwidth, windowheight / 2 + ((height / rmUNstaro.Height) * 0.5f * i)), null, GetRockstarAmount() < 5 ? FretColors[(int)GetRockstarAmount()] : Color.White, rockstarDir, new Vector2(rmUNstar.Width / 2, rmUNstar.Height / 2), (height / rmUNstaro.Height) * 0.5f, SpriteEffects.None, 0);
                }*/
#if WINDOWS
                spritebatch.Draw(rmUNstar, new Vector2(0, windowheight / 2), null, GetRockstarAmount() < 5 ? FretColors[(int)GetRockstarAmount()] : Color.White, rockstarDir, new Vector2(rmUNstar.Width / 2, rmUNstar.Height / 2), GetRockstarAmount() >= 5 ? (height / rmUNstaro.Height) : (GetRockstarAmount() % 1) * (height / rmUNstaro.Height), SpriteEffects.None, 0);
                spritebatch.Draw(rmUNstaro, new Vector2(0, windowheight / 2), null, Color.White, rockstarDir, new Vector2(rmUNstar.Width / 2, rmUNstar.Height / 2), height / rmUNstaro.Height, SpriteEffects.None, 0);
                //if (ct <= 1)
                {
                    spritebatch.Draw(rmUNstar, new Vector2(windowwidth, windowheight / 2), null, GetRockstarAmount()<5?FretColors[(int)GetRockstarAmount()]:Color.White, rockstarDir, new Vector2(rmUNstar.Width / 2, rmUNstar.Height / 2),GetRockstarAmount()>=5?(height/rmUNstaro.Height):(GetRockstarAmount()%1)*(height/rmUNstaro.Height), SpriteEffects.None, 0);
                    spritebatch.Draw(rmUNstaro, new Vector2(windowwidth, windowheight / 2), null, Color.White, rockstarDir, new Vector2(rmUNstar.Width / 2, rmUNstar.Height / 2), height/rmUNstaro.Height, SpriteEffects.None, 0);
                }
#else
                spritebatch.Draw(rmUNstar, new Vector2(windowwidth*0f, windowheight / 2), null, GetRockstarAmount()<5?FretColors[(int)GetRockstarAmount()]:Color.White, rockstarDir, new Vector2(rmUNstar.Width / 2, rmUNstar.Height / 2),(GetRockstarAmount()%1)*(height/rmUNstaro.Height), SpriteEffects.None, 0);
                spritebatch.Draw(rmUNstaro, new Vector2(windowwidth * 0f, windowheight / 2), null, Color.White, rockstarDir, new Vector2(rmUNstar.Width / 2, rmUNstar.Height / 2), height / rmUNstaro.Height, SpriteEffects.None, 0);
                //if (ct <= 1)
                {
                    spritebatch.Draw(rmUNstar, new Vector2(windowwidth*1f, windowheight / 2), null, FretColors[(int)GetRockstarAmount()], rockstarDir, new Vector2(rmUNstar.Width / 2, rmUNstar.Height / 2),(GetRockstarAmount()%1)*(height/rmUNstaro.Height), SpriteEffects.None, 0);
                    spritebatch.Draw(rmUNstaro, new Vector2(windowwidth*1f, windowheight / 2), null, Color.White, rockstarDir, new Vector2(rmUNstar.Width / 2, rmUNstar.Height / 2), height/rmUNstaro.Height, SpriteEffects.None, 0);
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
                    spritebatch.DrawString(DefaultFont, scr2, new Vector2((windowwidth / 2) - (DefaultFont.MeasureString(scr2).X / 2), Board.vocaly+Board.vocalheight), Color.White);
                }
                else
                    spritebatch.DrawString(DefaultFont, scr2, new Vector2((windowwidth / 2) - (DefaultFont.MeasureString(scr2).X / 2), 20), Color.White);
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
    }
}

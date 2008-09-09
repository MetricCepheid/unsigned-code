using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using UnsignedPeripheralPlugins;

namespace Unsigned
{
    public class NoteSet : IComparable
    {
        public uint time, length;
        public ulong type, endtype;
        public String text;
        public uint end
        {
            get { return time + length; }
        }

        public VIS_STATE[] visible;//0=visible,1=greyedout,2=invisible,3=invisibleButAvailable(HOPO)
        public bool burning;//for held Notes

        public uint late;

        public enum VIS_STATE { VISIBLE = 0, GREYED_OUT = 1, INVISIBLE = 2, HOPOED = 3, OVERDONE=4/*drums*/, };

        private bool strummed;
        private ulong pressed;
        private bool good;

        public void Strum()
        {
            strummed = true;
        }

        public void AddPressedGuitar(Instrument instr, ulong pressed)
        {
            if(Board.IsValidFrettage(type,pressed, instr))
                 good = true;
        }

        public ulong AddPressedDrums(Instrument instr, ulong pressed)
        {
            ulong ret = 0;
            for (int i = 0; i < instr.NumTracks; i++)
            {
                if ((pressed & (((ulong)1) << i)) != 0 && (this.pressed & (((ulong)1) << i)) == 0)
                { ret |= (byte)(1 << i); visible[i] = VIS_STATE.INVISIBLE; }
            }
            this.pressed |= pressed;
            ret &= this.type;
            return ret;
        }


        public bool IsGood(Instrument instr, bool HOPOable)
        {
            if(!instr.NeedsStrum)
                return (pressed) == (type);
            if ((instr.CanHOPO && HOPOable && (type&(((ulong)1)<<instr.NumTracks))!=0) || strummed)
                return good;
            return false;
        }

        public int CompareTo(object other)
        {
            return time.CompareTo(((NoteSet)other).time);
        }

        public void Kill()
        {
            for(int i=0;i<visible.Length;i++)
                visible[i] = VIS_STATE.INVISIBLE;
        }

        public bool IsHOPO(Instrument instrument)
        {
            return (type & (((ulong)1) << instrument.NumTracks)) != 0;
        }
    }

    public struct Phrase
    {
        public uint time;
        public SongData.TYPE type;
        public bool rockpower;
        public SongData.RTYPE rType;
        public NoteSet[] notes;
    }

    public struct Fill
    {
        public uint time, len;
        public bool hitGreen;
        public float amount;
        public uint end
        {
            get { return time + len; }
        }

        public Fill(uint time, uint len)
        {
            this.time = time;
            this.len = len;
            hitGreen = false;
            amount = 0f;
        }
    }

    public struct RockPowerPhrase
    {
        public uint time, len;
        public bool okay;//defaulted to true, falsed when note missed
        public uint end
        {
            get { return time + len; }
        }
        public RockPowerPhrase(uint time, uint len)
        {
            this.time = time;
            this.len = len;
            okay = true;
        }
    }

    public struct Solo
    {
        public uint time, len;
        public bool okay;//defaulted to true, falsed when note missed
        public uint end
        {
            get { return time + len; }
        }
        public Solo(uint time, uint len)
        {
            this.time = time;
            this.len = len;
            okay = true;
        }
    }

    class WaveVector2
    {
        public float X;
        public long Y;
        public bool White;
        public WaveVector2(float X, long Y)
        {
            this.X = X;
            this.Y = Y;
            White = false;
        }

        public WaveVector2(float X, long Y, bool w)
        {
            this.X = X;
            this.Y = Y;
            White = w;
        }
    }

    class Board
    {
        private struct SPCircle
        {
            public Vector2 pos;
            public float rotation;
            public bool rotDir;
            public float alpha;
        }
        public static float vocalheight, vocaly, vocalzerox, vocalwidth;
        public static float width, length, curveHeight, height, rotate, zeroZ, sFade, eFade, spShift;
        public static Texture2D[][] boardTexPlain;
        public static float WaveDetail = 50f;
        public static Texture2D drumfillTex, spMeterBG, spMeterLED, spMeterFill, spMeterCurl;
        public static Texture2D vBar, vBGExt, vBGInt, vFuzz, vHeadBar, vGlow, texBlast;
        public static int[] boardValidBPM = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, };
        public static int[] boardBeatsIndex = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, };
        public static GBVertexFormat[] arrBoard;
        public static VertexBuffer mdlBoard, mdlSPM;
        public static VertexBuffer mdlTrigger, mdlTriggerBorder;
        public static Model mdlNote, mdlNoteInside;
        public static Texture2D[] texNotes, texBarNotes, texTriggers, texTriggersLit;
        public static Texture2D texTriggerBorder, texTriggerBorderLit;
        private static Texture2D texLine, texLineEnd;
        public static float BOARD_BUMP_COEF = 0.002f;
        public static float spMeterYScale = 0.4f;
        public static int[] guitarToDrums = { 1, 2, 3, 0, 4 };
        public static int[] drumsToGuitar = { 3, 0, 1, 2, 4 };
        private static Texture2D texGlow;
        private static SPCircle[] spcircles = new SPCircle[50];
        private static uint PILLOW = 100;//padding in front of and behind note
        private static Texture2D[] boardBackgrounds, texMult;

        private const float BOARD_FADE_RATIO =  7 / 8f;

        private Instrument type;
        private int xOffset;
        private float yRotate=0;
        private Texture2D texBoard, texWaves;
        private float multSlide=0;
        private float SPMFlash=0, SPMFVel=0;
        private int spMeterShift = 7, spMeterShiftDrums=10;
        private float flashRot;
        private byte difficulty;
        private int index;
        private float multiplier=1;
        private byte lastPressed=0;
        private int[] starPts;
        private int score;
        private bool LeftySwitch = false;
        private float[] popup, popupSpeed;
        private float StarPowerAmount = 0, SPADisplay = 0;
        private int RPIndex, FillIndex;
        private bool SPGood = true;
        private bool SPActivated = false;
        private LinkedList<WaveVector2> whammyage;
        float waveoffset=0;
        Results myResults;
        private float rockMeterLevel;
        private int boardBackground;
        private byte numFails;
        Peripheral controller; 
        private RenderTarget2D boardTarget;        


        private WaveNode[][] waves;
        private int wavesLen;
        private int[] wavesSubLen;
        bool SPDelayStart;
        public RenderTarget2D rtBoard, rtWaves;

        //public const byte NS_GREEN = 1, NS_RED = 2, NS_YELLOW = 4, NS_BLUE = 8, NS_ORANGE = 16, NS_HOPO = 32;

        //Legacy
        private static string[] SETTINGS_EXT = { ".gbg", ".gbv", ".gbd", ".gbb", }; public static int OFFSET_TO_GBA = 0, OFFSET_TO_GBG = 1, OFFSET_TO_GBB = 2, OFFSET_TO_GBD = 3, OFFSET_TO_GBV = 4, OFFSET_TO_GBE = 5;

        private NoteSet[] Notes
        {
            get
            {
                for (int i = 0; i < RhythmMaster.GetSingleton().GetSongData().instruments.Length; i++)
                    if (RhythmMaster.GetSingleton().GetSongData().instruments[i].instrumentType.Equals(GetBoardType().CodeName))
                        return RhythmMaster.GetSingleton().GetSongData().instruments[i].diffSets[difficulty].phrases[0].notes;
                return null;
            }
        }

        private Phrase[] Phrases
        {
            get
            {
                for (int i = 0; i < RhythmMaster.GetSingleton().GetSongData().instruments.Length; i++)
                    if (RhythmMaster.GetSingleton().GetSongData().instruments[i].instrumentType.Equals(GetBoardType().CodeName))
                        return RhythmMaster.GetSingleton().GetSongData().instruments[i].diffSets[difficulty].phrases;
                return null;
            }
        }

        private Fill[] Fills
        {
            get
            {
                for (int i = 0; i < RhythmMaster.GetSingleton().GetSongData().instruments.Length; i++)
                    if (RhythmMaster.GetSingleton().GetSongData().instruments[i].instrumentType.Equals(GetBoardType().CodeName))
                        return RhythmMaster.GetSingleton().GetSongData().instruments[i].fills;
                return null;
            }
        }

        private RockPowerPhrase[] RPPhrases
        {
            get
            {
                for (int i = 0; i < RhythmMaster.GetSingleton().GetSongData().instruments.Length; i++)
                    if (RhythmMaster.GetSingleton().GetSongData().instruments[i].instrumentType.Equals(GetBoardType().CodeName))
                        return RhythmMaster.GetSingleton().GetSongData().instruments[i].rpPhrases;
                return null;
            }
        }

        private Solo[] Solos
        {
            get
            {
                for (int i = 0; i < RhythmMaster.GetSingleton().GetSongData().instruments.Length; i++)
                    if (RhythmMaster.GetSingleton().GetSongData().instruments[i].instrumentType.Equals(GetBoardType().CodeName))
                        return RhythmMaster.GetSingleton().GetSongData().instruments[i].solos;
                return null;
            }
        }

        public Peripheral Peripheral
        {
            get { return controller; }
            set { controller = value; }
        }

#region Accessors

        public bool IsFailing
        {
            get { return rockMeterLevel<=0; }
        }

        public bool IsLefty
        {
            get { return LeftySwitch; }
        }

#endregion

        public Board(Instrument type, int xOffset, SongData song, byte difficulty)
        {
            this.type = type;
            this.xOffset = xOffset;
            this.difficulty = difficulty;
            myResults.instr = type;
            score = 0;
            FillIndex = 0;
            RPIndex = 0;
            popup = new float[type.NumTracks];
            popupSpeed = new float[type.NumTracks];
            if (type.CanWhammy)
            {
                whammyage = new LinkedList<WaveVector2>();
                waves = new WaveNode[5][];
                for (int i = 0; i < 5; i++)
                    waves[i] = new WaveNode[50];
            }
            rockMeterLevel = 80.0f;
        }

        public Instrument GetBoardType()
        {
            return type;
        }

        public int GetXOffset()
        {
            return xOffset;
        }

        public void LoadInstance(ContentManager content)
        {
            GraphicsDeviceManager graphics = RenderMaster.GetSingleton().graphics;
            if (GameSettings.HALF_RENDER)
            {
                rtBoard = new RenderTarget2D(graphics.GraphicsDevice, GameSettings.windowheight / 4, GameSettings.windowheight / 2, 1, SurfaceFormat.Color);
                rtWaves = new RenderTarget2D(graphics.GraphicsDevice, GameSettings.windowheight / 4, GameSettings.windowheight / 2, 1, SurfaceFormat.Color);
            }
            else
            {
                rtBoard = new RenderTarget2D(graphics.GraphicsDevice, GameSettings.windowheight / 2, GameSettings.windowheight, 1, SurfaceFormat.Color);
                rtWaves = new RenderTarget2D(graphics.GraphicsDevice, GameSettings.windowheight / 2, GameSettings.windowheight, 1, SurfaceFormat.Color);
            }
            boardTarget = new RenderTarget2D(graphics.GraphicsDevice, GameSettings.windowwidth, GameSettings.windowheight, 1, SurfaceFormat.Color);
            NoteSet[] notes = Notes;
            for (int i = 0; i < notes.Length - 1; i++)
            {
                uint dist = notes[i + 1].time - notes[i].time;
                if (dist > PILLOW)
                    dist = PILLOW;
                notes[i].late = notes[i].time + dist;
            }
        }

        public static void Load(ContentManager content)
        {
            GraphicsDeviceManager graphics = RenderMaster.GetSingleton().graphics;
            Random r = Global.random;
            for (int i = 0; i < spcircles.Length; i++)
            {
                spcircles[i].alpha = (float)r.NextDouble();
                spcircles[i].rotation = (float)r.NextDouble();
                spcircles[i].pos = new Vector2((float)r.NextDouble(), (float)r.NextDouble());
                spcircles[i].rotDir = r.Next() % 2 == 0;
            }
            texGlow = content.Load<Texture2D>("graphics\\triggerglow");
            texLine = content.Load<Texture2D>("graphics\\line");
            texLineEnd = content.Load<Texture2D>("graphics\\linetaper");
            texTriggerBorder = content.Load<Texture2D>("graphics\\triggerborder");
            drumfillTex = content.Load<Texture2D>("graphics\\drumfill");
            spMeterBG = content.Load<Texture2D>("graphics\\boardmeter");
            spMeterFill = content.Load<Texture2D>("graphics\\white");
            spMeterLED = content.Load<Texture2D>("graphics\\bulb");
            spMeterCurl = content.Load<Texture2D>("graphics\\curl");
            texTriggerBorderLit = content.Load<Texture2D>("graphics\\triggerborderlit");
            texBlast = content.Load<Texture2D>("graphics\\blast");
            texTriggers = new Texture2D[5];
            for (int i = 0; i < 5; i++)
                texTriggers[i] = content.Load<Texture2D>("graphics\\trigger" + i);
            texTriggersLit = new Texture2D[5];
            for (int i = 0; i < 5; i++)
                texTriggersLit[i] = content.Load<Texture2D>("graphics\\triggerlit" + i);
            texNotes = new Texture2D[5];
            for (int i = 0; i < 5; i++)
                texNotes[i] = content.Load<Texture2D>("graphics\\Notes" + i);
            vBar = content.Load<Texture2D>("graphics\\vocalbar");
            vBGExt = content.Load<Texture2D>("graphics\\vocalbg_ext");
            vBGInt = content.Load<Texture2D>("graphics\\vocalbg_int");
            vFuzz = content.Load<Texture2D>("graphics\\vocalfuzz");
            vHeadBar = content.Load<Texture2D>("graphics\\vocalheadbar");
            vGlow = content.Load<Texture2D>("graphics\\vGlow");
            List<Texture2D> texBGs = new List<Texture2D>();
            String[] files = System.IO.Directory.GetFiles(System.IO.Directory.GetCurrentDirectory() + "\\boards\\");
            for (int i = 0; i < files.Length; i++)
            {
                if(files[i].ToLower().EndsWith(".png") || files[i].ToLower().EndsWith(".bmp") || files[i].ToLower().EndsWith(".jpg"))
                    texBGs.Add(Texture2D.FromFile(RenderMaster.GetSingleton().graphics.GraphicsDevice,files[i]));
            }
            boardBackgrounds = texBGs.ToArray();
            texMult = new Texture2D[8];
            for (int i = 0; i < Global.multToIndex.Length; i++)
                if (Global.multToIndex[i] >= 0)
                    texMult[Global.multToIndex[i]] = content.Load<Texture2D>("graphics\\X" + i);
            {
                float[] xs = { -1f, -1f, -.65f, -.65f, -.65f, -1f,     -.65f, -.65f, -.35f, -.65f, -.35f, -.35f,     -.35f, -.35f,    0f, -.35f,    0f,    0f,};
                float[] ys = {  0f,  0f,  .56f,  .56f,  .56f,  0f,      .56f,  .56f,  .88f,  .56f,  .88f,  .88f,      .88f,  .88f,    1f,  .88f,    1f,    1f,};
                float[] zs = {  1f, -1f,    1f,   -1f,    1f, -1f,        1f,   -1f,    1f,   -1f,    1f,   -1f,        1f,   -1f,    1f,   -1f,    1f,   -1f,};

                GBVertexFormat[] zmdlBoard = new GBVertexFormat[xs.Length * 2];

                for (int i = 0; i < xs.Length; i++)
                {
                    zmdlBoard[i] = new GBVertexFormat(new Vector3(xs[i], ys[i], (zs[i] + 1) / 2), new Vector3(0f, 0f, 0f), new Vector2((xs[i] + 1) / 2, (1 + zs[i]) / 2), new Vector3(0f, 0f, 0f));
                    zmdlBoard[i + xs.Length] = new GBVertexFormat(new Vector3(-xs[i], ys[i], (zs[i] + 1) / 2), new Vector3(0f, 0f, 0f), new Vector2(((-xs[i]) + 1) / 2, (1 + zs[i]) / 2), new Vector3(0f, 0f, 0f));
                }

                arrBoard = zmdlBoard;
                mdlBoard = new VertexBuffer(graphics.GraphicsDevice, 2 * xs.Length * GBVertexFormat.SizeInBytes, BufferUsage.WriteOnly);
                mdlBoard.SetData<GBVertexFormat>(zmdlBoard);
            }
            {
                float[] xs = { -1f, -1f, -.65f, -.65f, -.65f, -1f, -.65f, -.65f, -.35f, -.65f, -.35f, -.35f, -.35f, -.35f, 0f, -.35f, 0f, 0f, };
                float[] ys = { 0f, 0f, .5f, .5f, .5f, 0f, .5f, .5f, .85f, .5f, .85f, .85f, .85f, .85f, 1f, .85f, 1f, 1f, };
                float[] zs = { 1f, -1f, 1f, -1f, 1f, -1f, 1f, -1f, 1f, -1f, 1f, -1f, 1f, -1f, 1f, -1f, 1f, -1f, };

                GBVertexFormat[] zmdlBoard = new GBVertexFormat[xs.Length * 2];

                for (int i = 0; i < xs.Length; i++)
                {
                    zmdlBoard[i] = new GBVertexFormat(new Vector3((zs[i]<0?xs[i]:xs[i]*0.9f), ys[i], (zs[i] + 1) / 2), new Vector3(0f, 0f, 0f), new Vector2((xs[i] + 1) / 2, (1 + zs[i]) / 2), new Vector3(0f, 0f, 0f));
                    zmdlBoard[i + xs.Length] = new GBVertexFormat(new Vector3(-(zs[i] < 0 ? xs[i] : xs[i] * 0.9f), ys[i], (zs[i] + 1) / 2), new Vector3(0f, 0f, 0f), new Vector2(((-xs[i]) + 1) / 2, (1 + zs[i]) / 2), new Vector3(0f, 0f, 0f));
                }

                mdlSPM = new VertexBuffer(graphics.GraphicsDevice, 2 * xs.Length * GBVertexFormat.SizeInBytes, BufferUsage.WriteOnly);
                mdlSPM.SetData<GBVertexFormat>(zmdlBoard);
            }
            {
                mdlNoteInside = content.Load<Model>("meshes\\noteinside");
                foreach (ModelMesh mesh in mdlNoteInside.Meshes)
                    foreach(ModelMeshPart part in mesh.MeshParts)
                        part.Effect = RenderMaster.GetSingleton().engine;
                mdlNote = content.Load<Model>("meshes\\note");
                foreach (ModelMesh mesh in mdlNote.Meshes)
                    foreach(ModelMeshPart part in mesh.MeshParts)
                        part.Effect = RenderMaster.GetSingleton().engine;      
            }
            {
                float[] xs = { -1f, -1f, -.65f, -.65f, -.65f, -1f,     -.65f, -.65f, -.35f, -.65f, -.35f, -.35f,     -.35f, -.35f,    0f, -.35f,    0f,    0f,     -1f, -1f, -.65f, -.65f, -.65f, -1f,     -.65f, -.65f, -.35f, -.65f, -.35f, -.35f,     -.35f, -.35f,    0f, -.35f,    0f,    0f,     -1f, -1f, -.65f, -.65f, -.65f, -1f,     -.65f, -.65f, -.35f, -.65f, -.35f, -.35f,     -.35f, -.35f,    0f, -.35f,    0f,    0f,     };
                float[] ys = {  0f, .5f,   .5f,  1.5f,   .5f, .5f,       .5f,  1.5f,  .85f,  1.5f,  .85f, 1.85f,      .85f, 1.85f,    1f, 1.85f,    1f,    2f,     .5f,  1f,  .75f,  1.5f,  .75f,  1f,      1.5f,  1.5f, 1.85f,  1.5f, 1.85f, 1.85f,     1.85f, 1.85f,    2f, 1.85f,    2f,    2f,     .5f,  0f,  1.5f,   .5f,  1.5f,  0f,      1.5f,   .5f, 1.85f,   .5f, 1.85f,  .85f,     1.85f,  .85f,    2f,  .85f,    2f,    1f,     };
                float[] zs = {  1f,  0f,    1f,    0f,    1f,  0f,        1f,    0f,    1f,    0f,    1f,    0f,        1f,    0f,    1f,    0f,    1f,    0f,      0f,-.5f,    0f,  -.5f,    0f,-.5f,        0f,  -.5f,    0f,  -.5f,    0f,  -.5f,        0f,  -.5f,    0f,  -.5f,    0f,  -.5f,    -.5f, -1f,  -.5f,   -1f,  -.5f, -1f,      -.5f,   -1f,  -.5f,   -1f,  -.5f,   -1f,      -.5f,   -1f,  -.5f,   -1f,  -.5f,   -1f,     };
    
                GBVertexFormat[] zmdlTriggerBorder = new GBVertexFormat[xs.Length*2];
                for (int i = 0; i < xs.Length; i++)
                {
                    zmdlTriggerBorder[i] = new GBVertexFormat(new Vector3(xs[i], ys[i], (zs[i] + 1) / 2), new Vector3(0f, 0f, 0f), new Vector2((xs[i] + 1) / 2, (1 + zs[i]) / 2),new Vector3(0f,0f,0f));
                    zmdlTriggerBorder[i + xs.Length] = new GBVertexFormat(new Vector3(-xs[i], ys[i], (zs[i] + 1) / 2), new Vector3(0f, 0f, 0f), new Vector2(((-xs[i]) + 1) / 2, (1 + zs[i]) / 2),new Vector3(0f,0f,0f));
                }

                mdlTriggerBorder = new VertexBuffer(graphics.GraphicsDevice, 2 * xs.Length * GBVertexFormat.SizeInBytes, BufferUsage.WriteOnly);
                mdlTriggerBorder.SetData<GBVertexFormat>(zmdlTriggerBorder);
            }
            {
                float[] xs = { -1f, -1f,  1f, -1f,  1f,  1f,      -1f, -1f,  1f, -1f,  1f,  1f,     -1f, -1f, -1f, -1f, -1f, -1f,      1f,  1f,  1f,  1f,  1f,  1f,     };
                float[] ys = {  1f,  1f,  1f,  1f,  1f,  1f,       0f,  1f,  0f,  1f,  0f,  1f,      0f,  1f,  0f,  0f,  1f,  1f,      0f,  1f,  0f,  0f,  1f,  1f,     };
                float[] zs = { .5f,  0f, .5f,  0f, .5f,  0f,       1f, .5f,  1f, .5f,  1f, .5f,      1f, .5f,  0f,  0f,  0f, .5f,      1f, .5f,  0f,  0f,  0f, .5f,     };
                float[] us = {  0f,  0f,  1f,  0f,  1f,  1f,       0f,  0f,  1f,  0f,  1f,  1f,      0f,  0f,  0f,  0f,  0f,  0f,      0f,  0f,  0f,  0f,  0f,  0f,     };
                float[] vs = {.25f,  0f,.25f,  0f,.25f,  0f,       1f,.25f,  1f,.25f,  1f,.25f,      1f, .5f,  0f,  0f,  0f, .5f,      1f, .5f,  0f,  0f,  0f, .5f,     };

                GBVertexFormat[] zmdlTrigger = new GBVertexFormat[xs.Length * 2];
                for (int i = 0; i < xs.Length; i++)
                {
                    zmdlTrigger[i] = new GBVertexFormat(new Vector3(xs[i], ys[i], zs[i]), new Vector3(0f, 0f, 0f), new Vector2(us[i], vs[i]), new Vector3(0f, 0f, 0f));
                    zmdlTrigger[i + xs.Length] = new GBVertexFormat(new Vector3(xs[i], ys[i], -zs[i]), new Vector3(0f, 0f, 0f), new Vector2(us[i], vs[i]), new Vector3(0f, 0f, 0f));
                }

                mdlTrigger = new VertexBuffer(graphics.GraphicsDevice, 2 * xs.Length * GBVertexFormat.SizeInBytes, BufferUsage.WriteOnly);
                mdlTrigger.SetData<GBVertexFormat>(zmdlTrigger);
            }
        }

        //float[] drumxvals = { 1f, -1f, -1 / 3f, 1 / 3f, 0f };
        /*public void GetNotes(long currenttime, long viewdistance)
        {
            int k;
            for (k = 0; k < Notes.Length; k++)
                if ((long)Notes[k].time*(Global.TicksPerSecond/1000) > (currenttime) - (viewdistance / 3))
                    break;
            int count = 0;
            for (int i = k; i<Notes.Length && (long)Notes[i].time*(Global.TicksPerSecond/1000) < currenttime + viewdistance; i++)
            {
                if (Notes[i].visible[0]==NoteSet.VIS_STATE.INVISIBLE)
                    continue;
                if (type.CanHOPO && Notes[i].visible[0] == NoteSet.VIS_STATE.HOPOED)
                    continue;
                if ((type.RPEnableType & Instrument.RockPowerEnableTypes.FILL) != 0 && (SPActivated || StarPowerAmount < 0.5 || FillIndex >= FillStart.Length || Notes[i].time < FillStart[FillIndex] || Notes[i].time > FillEnd[FillIndex]))
                    continue;
                for (int r = 0; r < type.NumTracks;r++ )
                    if ((Notes[i].type & (1 << r)) != 0)
                        count++;
            }
            if ((type.RPEnableType & Instrument.RockPowerEnableTypes.FILL) != 0)
            {
                if (!SPActivated && StarPowerAmount>=0.5 && FillIndex < FillStart.Length && (currenttime / (Global.TicksPerSecond / 1000)) > FillStart[FillIndex] && (currenttime / (Global.TicksPerSecond / 1000)) < FillEnd[FillIndex] && FillAmount[FillIndex] >= 1)
                    count++;
            }

            if(OutNotes.Length < count)
                OutNotes = new Vector4[count];
            notesLen = count;
            int indx = 0;
            int plus = 0;
            for (int i = k; i < Notes.Length && (long)Notes[i].time*(Global.TicksPerSecond/1000) < currenttime + viewdistance; i++)
            {
                float sp = 0f;
                if (RPIndex<SPStart.Length && i >= SPStart[RPIndex] && i <= SPEnd[RPIndex] && SPGood)
                    sp = 1f;

                if (Notes[i].visible[0] == NoteSet.VIS_STATE.INVISIBLE)
                    continue;
                if (type.CanHOPO && Notes[i].visible[0] == NoteSet.VIS_STATE.HOPOED)
                    continue;
                if ((type.RPEnableType & Instrument.RockPowerEnableTypes.FILL) != 0 && (SPActivated || StarPowerAmount < 0.5 || FillIndex >= FillStart.Length || Notes[i].time < FillStart[FillIndex] || Notes[i].time > FillEnd[FillIndex]))
                    continue;
                for (int r = 0; r < type.NumTracks; r++)
                    if ((Notes[i].type & (1 << r)) != 0)
                    {
                        OutNotes[indx].X = -1f + ((2.0f/(type.NumTracks-1)) * r);
                        OutNotes[indx].Y = (long)Notes[i].time * (Global.TicksPerSecond / 1000);
                        OutNotes[indx].Z = ((Notes[i].type & NS_HOPO) != 0 && type.CanHOPO) ? 1f : -1f;
                        OutNotes[indx].W = sp;
                        indx++;
                    }
            }

            if ((type.RPEnableType & Instrument.RockPowerEnableTypes.FILL) != 0)
            {
                if (!SPActivated && StarPowerAmount>=0.5 && FillIndex < FillStart.Length && (currenttime / (Global.TicksPerSecond / 1000)) >= FillStart[FillIndex] && (currenttime / (Global.TicksPerSecond / 1000)) <= FillEnd[FillIndex] && FillAmount[FillIndex] >= 0.95)
                {
                    OutNotes[indx].X = 1;
                    OutNotes[indx].Y = (long)(FillEnd[FillIndex] - 50) * (Global.TicksPerSecond / 1000);
                    OutNotes[indx].Z = 0;
                    OutNotes[indx].W = 0;
                }
            }
            for (int i = 0; i < notesLen; i++)
                OutNotes[i].Y = (OutNotes[i].Y - currenttime) / (float)Global.TicksPerSecond;

            for (int i = 0; i < 4; i++)
                OutFills[i] = new Vector4(0, 0, 0, 0);
            if ((type.RPEnableType & Instrument.RockPowerEnableTypes.FILL) != 0)
            if(!SPActivated && StarPowerAmount>=0.5)
            for (int i = 0; i < 4; i++)
            {
                if (FillIndex + i >= FillStart.Length)
                    return;
                OutFills[i].X = (FillStart[FillIndex + i] - (currenttime/(Global.TicksPerSecond/1000)))/1000f;
                OutFills[i].Y = (FillEnd[FillIndex + i] - (currenttime / (Global.TicksPerSecond / 1000))) / 1000f;
                OutFills[i].Z = FillAmount[FillIndex + i] * 0.75f + 0.25f;
                OutFills[i].W = 100;
            }
        }*/

        public static bool IsValidFrettage(ulong note, ulong pressed, Instrument instr)
        {
            ulong NOT_HOPO_VALUE = ~(((ulong)1) << instr.NumTracks);
            note &= NOT_HOPO_VALUE;
            int numNotes = 0;
            for (int i = 0; i < instr.NumTracks; i++)
                if ((note & (((ulong)1)<<((int)i))) != 0)
                    numNotes++;
            if (numNotes > 1 && pressed == note)
                return true;
            else if (numNotes > 1)
                return false;
            if ((note & pressed) != note)
                return false;
            for (int i = 4; i >= 0; i--)
            {
                if ((note & (((ulong)1)<<i)) != 0)
                    return true;
                if ((pressed & (((ulong)1)<<i)) != 0)
                    return false;
            }
            return false;
        }

        public ulong Update(GameTime gameTime)
        {
            for (int i = 0; i < spcircles.Length; i++)
            {
                Random r = Global.random;
                spcircles[i].rotation += spcircles[i].rotDir ? (float)gameTime.ElapsedGameTime.TotalSeconds : -(float)gameTime.ElapsedGameTime.TotalSeconds;
                spcircles[i].alpha -= gameTime.ElapsedGameTime.Milliseconds / 2000f;
                if (spcircles[i].alpha <= 0)
                {
                    spcircles[i].alpha = (float)r.NextDouble();
                    spcircles[i].rotation = (float)r.NextDouble();
                    spcircles[i].pos = new Vector2((float)r.NextDouble(), (float)r.NextDouble());
                    spcircles[i].rotDir = r.Next() % 2 == 0;
                }
            }

            ulong pressed = controller.GetFrets();
            ulong newPressed = controller.GetBufferedFrets();
            RhythmMaster rm = RhythmMaster.GetSingleton();
                for (int i = 0; i < popup.Length; i++)
                {
                    if (popup[i] > 20 && popupSpeed[i] > 0)
                        popupSpeed[i] = 0;
                    if (popup[i] <= 0 && popupSpeed[i] < 0)
                    {
                        popup[i] = 0;
                        popupSpeed[i] = 0;
                    }
                    else if (popupSpeed[i] > 0 || popup[i] > 0)
                    {
                        popupSpeed[i] -= (gameTime.ElapsedGameTime.Milliseconds);
                    }
                    popup[i] += popupSpeed[i] * (gameTime.ElapsedGameTime.Milliseconds / 100f);
                }
                if (Global.DemoMode)
                {
                    if(index<Notes.Length)
                        if (Notes[index].time - (rm.GetCurrentTime()*1000) < 0)
                        {
                            if(!Notes[index].burning)
                            {
                                if (GetBoardType().ContainsHeldNotes && Notes[index].length > 0)
                                {
                                    for(int i=0;i<Notes[index].visible.Length;i++)
                                        Notes[index].visible[i] = NoteSet.VIS_STATE.INVISIBLE;
                                    Notes[index].burning = true;
                                    for (int i = 0; i < 5; i++)
                                        if ((Notes[index].type & (((ulong)1) << i)) != 0)
                                            popupSpeed[i] += 100;
                                    flashRot = (float)(UnsignedGame.r.Next() * Math.PI * 2);
                                    return Notes[index].type;
                                }
                                else
                                {
                                    for (int i = 0; i < Notes[index].visible.Length; i++)
                                        Notes[index].visible[i] = NoteSet.VIS_STATE.INVISIBLE;
                                    for (int i = 0; i < 5; i++)
                                        if ((Notes[index].type & (((ulong)1) << i)) != 0)
                                               popupSpeed[i] += 100;
                                    index++;
                                    flashRot = (float)(UnsignedGame.r.Next() * Math.PI * 2);
                                    return Notes[index - 1].type;
                                }
                            }
                            else //well, i'm just gonna assume we support held Notes
                            {
                                if ((Notes[index].end) - (rm.GetCurrentTime()*1000) < 0)
                                {
                                    index++;
                                    return 0;
                                }
                                else
                                {
                                    RhythmMaster.GetSingleton().Burn(Notes[index].type, this);
                                    //Burn(gameTime, Notes[index].type);
                                    return 0;// (byte)(Game1.bits[7] | Notes[index].type);
                                }
                            }
                        }
                    return 0;
                }
                waveoffset -= gameTime.ElapsedGameTime.Milliseconds / 100f;

                if (GetBoardType().RPEnableType == Instrument.RockPowerEnableTypes.FILL)
                {
                    if (FillIndex < Fills.Length && rm.GetCurrentTime() > Fills[FillIndex].end)
                    {
                        if (SPDelayStart)
                        {
                            SPActivated = true;
                            SPDelayStart = false;
                        }
                        FillIndex++;
                    }
                    if (StarPowerAmount >= 0.5 && !SPActivated && FillIndex < Fills.Length && rm.GetCurrentTime()*1000 >= Fills[FillIndex].time && rm.GetCurrentTime()*1000 <= Fills[FillIndex].end)
                    {
                        rm.AddSparks(newPressed, this);
                        for (int i = 0; i < 5; i++)
                            if ((newPressed & (((ulong)1) << i)) != 0)
                            {
                                popupSpeed[drumsToGuitar[i]] += 100f;
                                Fills[FillIndex].amount += 1f / (((Fills[FillIndex].len) / 1000f) * 7);
                                Fills[FillIndex].amount = Math.Min(Fills[FillIndex].amount, 1);
                            }
                        while (Notes[index].time <= Fills[FillIndex].end+100)
                        { index++; myResults.hitNotes++; }
                        if (rm.GetCurrentTime()*1000 >= (Fills[FillIndex].end) - 100 && rm.GetCurrentTime()*1000 <= Fills[FillIndex].end)
                            if ((newPressed | 1) != 0)
                                if (Fills[FillIndex].amount >= 1)
                                    SPDelayStart = true;
                    }
                }
                if (index< Notes.Length && Notes[index].time - PILLOW < rm.GetCurrentTime()*1000)
                {

                    if (!GetBoardType().NeedsStrum)
                    {
                        ulong flash = Notes[index].AddPressedDrums(GetBoardType(),newPressed);
                        for (int i = 0; i < 5; i++)
                            if ((flash & (((ulong)1) << i)) != 0)
                            {
                                multiplier += 0.1f;
                                popupSpeed[drumsToGuitar[i]] += 100f;
                            }
                        for (int i = 0; i < 5; i++)
                            if ((flash & (((ulong)1) << i)) != 0)
                            { score += 100 * (int)multiplier; }
                        if (Notes[index].IsGood(GetBoardType(),false))
                        {
                            Notes[index].Kill();
                            Help();
                            index++;
                            myResults.hitNotes++;
                            return Notes[index - 1].type;
                        }
                        if (Notes[index].late <= rm.GetCurrentTime()*1000)
                        {
                            if (RPIndex < RPPhrases.Length && index >= RPPhrases[RPIndex].time && index <= RPPhrases[RPIndex].end)
                                SPGood = false;
                            index++;
                            multiplier = 1;
                            Hurt();
                            myResults.missedNotes++;
                        }
                    }
                    else
                    {
                        if (controller.WasPressed(PeripheralButton.UP) || controller.WasPressed(PeripheralButton.DOWN))
                            Strum();

                        if (GetBoardType().ContainsHeldNotes && Notes[index].burning)
                        {
                            LinkedListNode<WaveVector2> temp = whammyage.First;
                            while (temp != null)
                            {
                                temp.Value.Y += gameTime.ElapsedGameTime.Milliseconds;
                                temp = temp.Next;
                            }
                            if (Notes[index].end <= rm.GetCurrentTime()*1000)
                                index++;
                            else if (!IsValidFrettage(Notes[index].type, pressed,GetBoardType()))
                                index++;
                            else
                            {
                                rm.Burn(Notes[index].type, this);
                                Burn(gameTime, Notes[index].type);
                            }
                        }
                        else
                        {
                            Notes[index].AddPressedGuitar(GetBoardType(),pressed);
                            if (Notes[index].IsGood(GetBoardType(), index > 0 && Notes[index - 1].visible[0] == NoteSet.VIS_STATE.INVISIBLE))
                            {
                                Notes[index].Kill();
                                for (int i = 0; i < GetBoardType().NumTracks; i++)
                                    if ((Notes[index].type & (((ulong)1) << i)) != 0)
                                        popupSpeed[i] += 100f;
                                multiplier += 0.1f;
                                for (int i = 0; i < GetBoardType().NumTracks; i++)
                                    if ((Notes[index].type & (((ulong)1) << i)) != 0)
                                    { score += 100 * (int)multiplier; }
                                myResults.hitNotes++;
                                Help();
                                if (Notes[index].length > 0)
                                {
                                    Notes[index].burning = true;
                                    return Notes[index].type;
                                }
                                else
                                    index++;
                                return Notes[index - 1].type;
                            }
                            if (Notes[index].late <= rm.GetCurrentTime()*1000)
                            {
                                if (RPIndex < RPPhrases.Length && index >= RPPhrases[RPIndex].time && index <= RPPhrases[RPIndex].end)
                                    SPGood = false;
                                index++;
                                multiplier = 1;
                                Hurt();
                                myResults.missedNotes++;
                            }
                        }
                    }

                    
                }
                else if (newPressed != 0 && !GetBoardType().NeedsStrum)
                {
                    Hurt();
                    multiplier = 1;
                }

                if (RPIndex < RPPhrases.Length && (index > RPPhrases[RPIndex].end || (index == RPPhrases[RPIndex].end && Notes[index].burning)))
                {
                    if (SPGood)
                    {
                        StarPowerAmount += 0.25f;
                        myResults.hitSPPH++;
                    }
                    else 
                        myResults.missedSPPH++;
                    SPGood = true;
                    RPIndex++;
                }

                if (multiplier > GetBoardType().MaxMultiplier)
                    multiplier = GetBoardType().MaxMultiplier;

                

            if (StarPowerAmount < 0)
                StarPowerAmount = 0;
            else if (StarPowerAmount > 1)
                StarPowerAmount = 1;
            if (Math.Abs(StarPowerAmount - SPADisplay) > 0.001f)
            {
                SPADisplay = (StarPowerAmount*0.25f)+(SPADisplay*0.75f);
            }
            else
                SPADisplay = StarPowerAmount;
            if (SPActivated)
            {
                StarPowerAmount -= gameTime.ElapsedGameTime.Milliseconds / 20000f;
                if (StarPowerAmount < 0)
                {
                    SPActivated = false;
                    StarPowerAmount = 0;
                }
            }

            if (rockMeterLevel > 100)
                rockMeterLevel = 100;

            return 0;
        }

        public float GetBoardBump()//bass bump
        {
            float ret = 0;
            for(int i=0;i<GetBoardType().NumTracks;i++)
                if((GetBoardType().BumpNotes & (((ulong)1) << i)) != 0)
                    ret += popup[i];
            return ret;
        }

        public byte Strum()
        {
            if (index < Notes.Length && Notes[index].time - PILLOW < RhythmMaster.GetSingleton().GetCurrentTime()*1000)
                Notes[index].Strum();
            else
            { multiplier = 1; Hurt(); }
            return 0;
            /*if (index >= Notes.Length)
                return 0;
            if (Math.Abs(Notes[index].time - (long)currenttime) < 100)
            {
                if (!(Notes[index].burning && index + 1 < Notes.Length && Math.Abs(Notes[index + 1].time - (long)currenttime) < 100))
                {
                    if (IsValidFrettage(Notes[index].type, pressed))
                    {
                        if (Notes[index].visible[0] != NoteSet.VIS_STATE.HOPOED && !Notes[index].burning)
                        {
                            int scre = 0;
                            for (int i = 0; i < 5; i++)
                                if ((Notes[index].type & Game1.bits[i]) > 0)
                                {
                                    scre++;
                                    popupSpeed[i] += 100f;
                                }
                            scre *= 100 * (int)(multiplier);
                            score += scre;
                            multiplier += .1f;
                        }
                        Notes[index].visible[0] = NoteSet.VIS_STATE.INVISIBLE;
                        if (Notes[index].burning)
                        {
                            Notes[index].burning = false;
                            game.Hurt(ind);
                            if (RPIndex < SPStart.Length && Notes[index].time >= SPStart[RPIndex] && Notes[index].time < SPEnd[RPIndex])
                                SPGood = false;
                            multiplier = 1;
                            return 0;
                        }
                        if (Notes[index].length > 0)
                        {
                            Notes[index].burning = true;
                            myResults.hitNotes++;
                            return Notes[index].type;
                        }
                        //else
                        myResults.hitNotes++;
                        index++;
                        return Notes[index - 1].type;
                    }
                }
                else
                {
                    index++;
                    if (IsValidFrettage(Notes[index].type, pressed))
                    {
                        if (Notes[index].visible[0] != NoteSet.VIS_STATE.HOPOED && !Notes[index].burning)
                        {
                            int scre = 0;
                            for (int i = 0; i < 5; i++)
                                if ((Notes[index].type & Game1.bits[i]) > 0)
                                {
                                    scre++;
                                    popupSpeed[i] += 100f;
                                }
                            scre *= 100 * (int)(multiplier);
                            score += scre;
                            multiplier += .1f;
                        }
                        Notes[index].visible[0] = NoteSet.VIS_STATE.INVISIBLE;
                        if (Notes[index].length > 0)
                        {
                            Notes[index].burning = true;
                            myResults.hitNotes++;
                            return Notes[index].type;
                        }
                        //else
                        myResults.hitNotes++;
                        index++;
                        return Notes[index - 1].type;
                    }
                }
            }
            else if (Notes[index].burning && index + 1 < Notes.Length && Math.Abs(Notes[index + 1].time - (long)currenttime) < 100)
            {
                index++;
                    if (IsValidFrettage(Notes[index].type, pressed))
                    {
                        if (Notes[index].visible[0] != NoteSet.VIS_STATE.HOPOED && !Notes[index].burning)
                        {
                            int scre = 0;
                            for (int i = 0; i < 5; i++)
                                if ((Notes[index].type & Game1.bits[i]) > 0)
                                {
                                    scre++;
                                    popupSpeed[i] += 100f;
                                }
                            scre *= 100 * (int)(multiplier);
                            score += scre;
                            multiplier += .1f;
                        }
                        Notes[index].visible[0] = NoteSet.VIS_STATE.INVISIBLE;
                        if (Notes[index].length > 0)
                        {
                            Notes[index].burning = true;
                            myResults.hitNotes++;
                            return Notes[index].type;
                        }
                        //else
                        myResults.hitNotes++;
                        index++;
                        return Notes[index - 1].type;
                    }
            }
            else if (currenttime > (long)Notes[index].time && currenttime < ((long)Notes[index].time + (long)Notes[index].length))
            {
                Notes[index].burning = false; multiplier = 1; game.Hurt(ind);
                if (RPIndex < SPStart.Length && Notes[index].time >= SPStart[RPIndex] && Notes[index].time < SPEnd[RPIndex])
                    SPGood = false; return 0;
            }
            
            if (RPIndex < SPStart.Length && Notes[index].time >= SPStart[RPIndex] && Notes[index].time < SPEnd[RPIndex])
                            SPGood = false;
            game.Hurt(ind);
            multiplier = 1;
            return 0;*/
        }

        public byte Bang(byte pressed, long currenttime, UnsignedGame game)
        {
            return 0;
            /*byte newPressed = 0;
            for (int i = 0; i < 5; i++)
            {
                if ((lastPressed & Game1.bits[i]) == 0 && (pressed & Game1.bits[i]) != 0)
                    newPressed |= Game1.bits[i];
            }
            if (newPressed == 0)
                return 0;
            if (index >= Notes.Length)
                return 0;
            if (StarPowerAmount>=0.5 && !SPActivated && FillIndex<FillEnd.Length && currenttime >= (long)FillStart[FillIndex] && currenttime <= (long)FillEnd[FillIndex])
            {
                if ((newPressed & Game1.bits[0]) != 0)
                    if (Math.Abs((FillEnd[FillIndex] - 100) - (long)currenttime) < 100)
                    { SPActivated = true; DFHitGreen[FillIndex] = true; }
                float numperfill = 4f * (FillEnd[FillIndex] - FillStart[FillIndex]) / 1000f;
                int numhit = 0;
                for (int i = 0; i < 5; i++)
                    if ((newPressed & Game1.bits[i]) != 0)
                        numhit++;
                FillAmount[FillIndex] += numhit / numperfill;
                if (FillAmount[FillIndex] > 1)
                    FillAmount[FillIndex] = 1f;
                return (byte)(newPressed | Game1.bits[7]);
            }
            else if ((Notes[index].time - (long)currenttime) < 100)
            {
                byte ret = 0;
                for (int i = 0; i < 5; i++)
                    if ((Notes[index].type & Game1.bits[i]) != 0 && (newPressed & Game1.bits[i]) != 0 && Notes[index].visible[i]==0)
                    {
                        ret |= Game1.bits[i];
                        Notes[index].visible[i] = NoteSet.VIS_STATE.INVISIBLE;
                        popupSpeed[drumsToGuitar[i]] += 100f;
                    }
                    else if ((Notes[index].type & Game1.bits[i]) == 0 && (newPressed & Game1.bits[i]) != 0)
                    {
                        Notes[index].visible[i] = NoteSet.VIS_STATE.OVERDONE;
                    }
                return ret;
            }
            game.Hurt(2);
            multiplier = 1;
            return 0;*/
        }

        public float GetStars()
        {
            int i;
            for (i = 0; i < starPts.Length; i++)
                if (score < starPts[i])
                    break;
            if (i > 5)
                return 5;
            float dec = (float)(score - starPts[i-1]) / (starPts[i] - starPts[i-1]);
            return dec + i-1;
        }

        public byte GetDifficulty()
        {
            return difficulty;
        }

        public int GetScore()
        {
            return score;
        }

        public int GetMultiplierFraction()
        {
            if (multiplier >= GetBoardType().MaxMultiplier)
                return 10;
            int i= (int)((multiplier % 1) * 10);
            if (i == 0 && multiplier > 1)
                i = 10;
            return i;
        }

        public int GetMultiplier()
        {
            return (int)multiplier*(SPActivated?2:1);
        }

        /*public bool GetWaves(long currenttime)
        {
            int count = 0;
            for (int i = index; i<Notes.Length && Notes[i].time < currenttime + (eFade*1000) && Notes[i].time+Notes[i].length>(long)currenttime; i++)
            {
                if (Notes[i].length > 0)
                    count++;
            }
            if (wavesLen < count)
            { waves = new WaveNode[count][]; wavesSubLen = new int[count]; }
            wavesLen = count;
            if (count == 0)
                return false;
            count = 0;
            for (int i = index; i < Notes.Length && Notes[i].time < currenttime + (eFade*1000) && Notes[i].time+Notes[i].length>(long)currenttime; i++)
            {
                if (Notes[i].length > 0)
                {
                    bool white = false;
                    if (RPIndex < RPPhrases.Length && SPGood && i >= SPStart[RPIndex] && i <= SPEnd[RPIndex])
                        white = true;
                    if (Notes[i].burning)
                    {
                        
                        int num = (int)((Math.Min(Notes[i].length, eFade * 1000f - (Notes[i].time - (long)currenttime)) - ((Notes[i].time < (long)currenttime) ? (long)currenttime - Notes[i].time : 0) - WaveDetail) / WaveDetail);
                        num += 2;
                        if (wavesSubLen[count] < num)
                        { waves[count] = new WaveNode[num];  }
                        wavesSubLen[count] = num;
                        waves[count][0].X = 0f;
                        waves[count][0].Y = (Notes[i].time < (long)currenttime) ? 0 : (Notes[i].time - (long)currenttime);
                        waves[count][0].Z = (byte)(Notes[i].type|(Notes[i].burning||Notes[i].time>(long)currenttime?0:128)|64);
                        waves[count][0].White = white;
                        float varyPower;
                        for (int n = 0; n < num - 1; n++)
                        {
                            waves[count][n].White = white;
                            waves[count][n].Y = ((Notes[i].time < (long)currenttime) ? 0 : (Notes[i].time - (long)currenttime)) + (WaveDetail * n);
                            varyPower = GetWhammy(waves[count][n].Y,currenttime);
                            if(varyPower<0.1)
                                waves[count][n].X = 0.2f-((float)(Math.Sin(waveoffset + (waves[count][n].Y / 100f))+1)*0.1f*(varyPower*10));
                            else
                                waves[count][n].X = (float)Math.Sin(waveoffset + (waves[count][n].Y / 100f)) * (varyPower);
                            waves[count][n].Z = (byte)(Notes[i].type|(Notes[i].burning||Notes[i].time>(long)currenttime?0:128)|64);
                        }
                        waves[count][num-1].White = white;
                        waves[count][num - 1].Y = Math.Min(((Notes[i].time + Notes[i].length) - (long)currenttime), ((eFade * 1000f)));
                        //waves[count][num - 1].X = (float)Math.Sin(waveoffset + (waves[count][num - 1].Y / 100f));
                        waves[count][num - 1].X = (float)Math.Sign(waves[count][num - 1].X) * 0.1f;
                        waves[count][num - 1].Z = (byte)(Notes[i].type|(Notes[i].burning||Notes[i].time>(long)currenttime?0:128)|64);
                        count++;
                    }
                    else
                    {
                        if (wavesSubLen[count] < 2)
                        { waves[count] = new WaveNode[2]; }
                         wavesSubLen[count] = 2;
                        waves[count][0].White = white;
                        waves[count][0].X = 0.2f;
                        waves[count][0].Y = (Notes[i].time < (long)currenttime) ? 0 : (Notes[i].time - (long)currenttime);
                        waves[count][0].Z = (byte)(Notes[i].type|(Notes[i].burning||Notes[i].time>(long)currenttime-100?0:128)|(Notes[i].time >= (long)currenttime?64:0));

                        waves[count][1].White = white;
                        waves[count][1].Y = Math.Min(((Notes[i].time + Notes[i].length) - (long)currenttime), (eFade * 1000f));
                        waves[count][1].X = 0.2f;
                        waves[count][1].Z = (byte)(Notes[i].type|(Notes[i].burning||Notes[i].time>(long)currenttime-100?0:128)|(Notes[i].time >= (long)currenttime?64:0));
                        count++;
                    }
                }
            }
            return true;
        }*/

        public float GetSPAmount()
        {
            return SPADisplay;
        }

        public bool IsSPActivated()
        {
            return SPActivated;
        }

        public void Whammy(float p,long currenttime,GameTime gametime, int beatLength)
        {
            p = (p + 1) / 2f;
            if (index >= Notes.Length)
                return;
            if (Notes[index].burning && Notes[index].time < currenttime && Notes[index].end > currenttime)
            {
                whammyage.AddFirst(new WaveVector2(p, 0));
                int end = (int)Math.Min(eFade * 1000, (Notes[index].end) - (long)currenttime);
                while (whammyage.Last.Value.Y >= end)
                    whammyage.RemoveLast();
                score += (int)(10 * ((gametime.ElapsedGameTime.TotalSeconds*1000) / beatLength));
                if(RPIndex<RPPhrases.Length && index>=RPPhrases[RPIndex].time && index<=RPPhrases[RPIndex].end)
                StarPowerAmount += (float)(((gametime.ElapsedGameTime.TotalSeconds * 1000) / beatLength));
                if(RPIndex<RPPhrases.Length && index>=RPPhrases[RPIndex].time && index<=RPPhrases[RPIndex].end)
                    StarPowerAmount += (float)(0.05f * ((gametime.ElapsedGameTime.TotalSeconds * 1000) / beatLength));
            }
            else if (whammyage.Count > 0)
                whammyage.Clear();
        }

        private float GetWhammy(float y,long currenttime)
        {
            if (whammyage.Count < 2)
                return 0f;
            LinkedListNode<WaveVector2> temp = whammyage.First;
            while (temp!=null && temp.Value.Y < y)
            {
                temp = temp.Next;
            }
            if (temp == null)
            {
                int end = (int)Math.Min(eFade * 1000, (Notes[index].end) - (long)currenttime);
                float p = (y - whammyage.Last.Value.Y) / (end - whammyage.Last.Value.Y);
                return (0.1f * p) + (whammyage.Last.Value.X*(1 - p));
            }
            else if (temp.Equals(whammyage.First))
            {
                float p = y / whammyage.First.Value.Y;
                return (0.1f * (1 - p)) + (whammyage.First.Value.X * p);
            }
            else
            {
                float p = (y - temp.Previous.Value.Y) / (temp.Value.Y - temp.Previous.Value.Y);
                return (temp.Value.X * p) + (temp.Previous.Value.X * (1 - p));
            }
        }

        public void Burn(GameTime gt, ulong note)
        {
            int num = Global.AddBits(note & ~(((ulong)1) << GetBoardType().NumTracks));
            score += (int)(num * 100 * gt.ElapsedGameTime.TotalSeconds);
        }

        public Results GetResults()
        {
            return myResults;
        }

        public void ActivateStarPower()
        {
            if (StarPowerAmount>=0.5)
                SPActivated = true;
        }

        public void Draw(Matrix fling)
        {
            RenderMaster rm = RenderMaster.GetSingleton();
            SpriteBatch spritebatch = rm.spritebatch;
            BasicEffect effect = rm.bEffect;
            GraphicsDeviceManager graphics = rm.graphics;

            if (GetBoardType().Dimensions == Instrument.BoardDimensions.TWO_DIMENSIONAL)
            {
                long currenttime = (long)(RhythmMaster.GetSingleton().GetCurrentTime() * 1000);
                Vector2 center = new Vector2(vFuzz.Width / 2, vFuzz.Height / 2);
                float vScale = 0.2f;
                float height = vFuzz.Height * vScale;
                Color glow = new Color(150, 255, 150, 255);
                spritebatch.Draw(vBGInt, new Rectangle(0, (int)vocaly, 1024, (int)vocalheight), Color.White);
                for (int i = index; i < Phrases.Length; i++)
                {
                    spritebatch.Draw(vBar, new Rectangle((int)(vocalzerox + (vocalwidth * (Phrases[i].time - (long)currenttime))), (int)vocaly, 8, (int)vocalheight), Color.White);

                    for (int k = 0; k < Phrases[i].notes.Length; k++)
                    {
                        if (Phrases[i].notes[k].type < 0)
                            continue;
                        float minx = (vocalzerox + (vocalwidth * (Phrases[i].notes[k].time - currenttime))), maxx = (vocalzerox + (vocalwidth * ((Phrases[i].notes[k].length) - currenttime))), y = (1 - ((Phrases[i].notes[k].type % 12) / 12f)) * vocalheight * .69f + vocaly;
                        spritebatch.Draw(vGlow, new Rectangle((int)minx, (int)(y - height / 2), (int)Math.Min(128 * vScale, (maxx - minx) * vScale), (int)(height)), new Rectangle(0, 0, 128, 256), glow);
                        spritebatch.Draw(vGlow, new Rectangle((int)(minx + Math.Min(128 * vScale, (maxx - minx) * vScale)), (int)(y - height / 2), (int)((maxx - minx) - (Math.Min(128 * vScale, (maxx - minx) * vScale) * 2)), (int)(height)), new Rectangle(128, 0, 128, 256), glow);
                        spritebatch.Draw(vGlow, new Rectangle((int)(maxx - Math.Min(128 * vScale, (maxx - minx) * vScale)), (int)(y - height / 2), (int)Math.Min(128 * vScale, (maxx - minx) * vScale), (int)(height)), new Rectangle(128, 0, -128, 256), glow);
                    }
                }

                spritebatch.Draw(vBGExt, new Rectangle(0, (int)vocaly, 1024, (int)vocalheight), Color.White);

                for (int i = index; i < Phrases.Length; i++)
                    for (int k = 0; k < Phrases[i].notes.Length; k++)
                        spritebatch.DrawString(Global.DefaultFont, Phrases[i].notes[k].text, new Vector2((vocalzerox + (vocalwidth * (Phrases[i].notes[k].time - currenttime))), vocaly + (0.75f * vocalheight)), Color.White);
                spritebatch.Draw(vHeadBar, new Rectangle((int)vocalzerox, (int)vocaly, 8, (int)vocalheight), Color.White);
            }
            else //if (GetBoardType().Dimensions == Instrument.BoardDimensions.THREE_DIMENSIONAL)
            {
                DrawBoardTarget();

                graphics.GraphicsDevice.SetRenderTarget(0, boardTarget);
                graphics.GraphicsDevice.Clear(new Color(new Vector4(0, 0, 0, 0)));

                graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;
                graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
                graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
                graphics.ApplyChanges();

                Matrix matProj = Matrix.CreatePerspectiveFieldOfView((float)Math.PI / 4.0f,
                          boardTarget.Width / (float)boardTarget.Height,
                          0.01f, 1000.0f);

                rm.SetViewMatrix(Matrix.Identity);
                effect.Projection = matProj;

                //get board measure world lengths

#if DEBUG_CAM_CONTROL
                                    fling *= Matrix.CreateRotationX(-0.4f);
#endif

                Matrix matTransl = Matrix.CreateTranslation(0f, Board.height + (GetBoardBump() * Board.BOARD_BUMP_COEF), 0f);
                effect.AmbientLightColor = new Vector3(1, 1, 1);
                effect.Begin();
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Begin();

                    //draw each boards
                    DrawBoard(fling, matTransl);
                    if (!IsFailing)
                        DrawNotes(fling);
                    DrawBoardDetail(fling, matTransl);
                    if (!IsFailing)
                    {   
                        DrawWaves(fling, matTransl);

                        //ParticleMaster.GetSingleton().Render(i?);
                        DrawFlashes(fling);
                    }
                    pass.End();
                }
                effect.End();
                graphics.GraphicsDevice.SetRenderTarget(0, null);
            }
        }

        internal void EatHalfSP()
        {
            StarPowerAmount -= 0.5f;
        }

        internal void ToggleLefty()
        {
            LeftySwitch = !LeftySwitch;
        }

        /// <summary>
        /// Turns a "time" into a position on the board
        /// </summary>
        /// <param name="position">the absolute time value of something (e.g. a note)</param>
        /// <param name="ratio">the position of the frets. 0 is at the base of the board, 1 is at the far end</param>
        /// <returns>the position on the board to draw (0-1 is visible range)</returns>
        private static float GetBoardPos(double position, float ratio)
        {
            //relative position to current time
            double relPos = position - RhythmMaster.GetSingleton().GetCurrentTime();

            //total length of the board
            double len = (Board.eFade * (1 / (1 - ratio)));

            // in correct units
            double retPos = relPos / len;

            //shift for ratio offset
            retPos += ratio;

            //fix for tip of board being 0
            retPos = 1 - retPos;

            return (float)retPos;
        }

        private void DrawBoardTarget()
        {
            Effect fader = RenderMaster.GetSingleton().fader;
            SpriteBatch spritebatch = RenderMaster.GetSingleton().spritebatch;
            GraphicsDeviceManager graphics = RenderMaster.GetSingleton().graphics;

            float[] noteLanePos = { 29 / 256f, 79 / 256f, 127 / 256f, 176 / 256f, 227 / 256f };
            float ratio = BOARD_FADE_RATIO;
            fader.CurrentTechnique = fader.Techniques["Fade"];
            float fh = (Board.eFade - Board.sFade) / (Board.eFade * (1 / ratio));
            if (GetBoardType().Dimensions == Instrument.BoardDimensions.TWO_DIMENSIONAL)
                return;
            graphics.GraphicsDevice.SetRenderTarget(0, rtWaves);
            graphics.GraphicsDevice.Clear(new Color(0, 0, 0, 0));
            spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);
            if (wavesLen > 0)
            {
                for (int p = 0; p < wavesLen; p++)
                {
                    for (int r = 0; r < 5; r++)
                    {
                        if ((waves[p][0].Z & (((ulong)1)<<r)) > 0)
                        {
                            for (int q = 0; q < wavesSubLen[p] - 1; q++)
                            {
                                float loA, hiA;
                                if (waves[p][q].Y / 1000f < Board.sFade)
                                    loA = 1;
                                else if (waves[p][q].Y / 1000f < Board.eFade)
                                    loA = 1 - (((waves[p][q].Y / 1000f) - Board.sFade) / (Board.eFade - Board.sFade));
                                else
                                    loA = 0;
                                if (waves[p][q + 1].Y / 1000f < Board.sFade)
                                    hiA = 1;
                                else if (waves[p][q + 1].Y / 1000f < Board.eFade)
                                    hiA = 1 - (((waves[p][q + 1].Y / 1000f) - Board.sFade) / (Board.eFade - Board.sFade));
                                else
                                    hiA = 0;
                                float xscale = 15f;
                                //spritebatch.Draw(texWhite, new Vector2(waves[p][q].X, (rtBoard.Height * ratio) - ((rtBoard.Height * ratio) * ((waves[p][q].Y / 1000f) / Board.eFade))), Color.White);
                                int note = waves[p][q].Z;
                                //if ((note & 128) == 0)
                                {
                                    for (int k = 0; k < 5; k++)
                                        if ((note & (1 << k)) != 0)
                                        {
                                            if ((note & 64) != 0)
                                            {
                                                spritebatch.Draw(texLine, new Vector2((waves[p][q].X * xscale) + (IsLefty ? noteLanePos[4 - k] * rtBoard.Width : noteLanePos[k] * rtBoard.Width), (rtBoard.Height * ratio) - ((rtBoard.Height * ratio) * ((waves[p][q].Y / 1000f) / Board.eFade))), null, waves[p][q].White ? Color.White : Global.FretColors[k], (float)Math.Atan2(((-rtBoard.Height * ratio) * (((waves[p][q + 1].Y - waves[p][q].Y) / 1000f) / Board.eFade)), (waves[p][q + 1].X - waves[p][q].X) * xscale) + MathHelper.PiOver2, new Vector2(texLine.Width / 2, texLine.Height), new Vector2(0.5f, (new Vector2((waves[p][q + 1].X - waves[p][q].X) * xscale, ((rtBoard.Height * ratio) * (((waves[p][q + 1].Y - waves[p][q].Y) / 1000f) / Board.eFade)))).Length() * 1.05f / (float)texLine.Height), SpriteEffects.None, 0);
                                                spritebatch.Draw(texLine, new Vector2((waves[p][q].X * -xscale) + (IsLefty ? noteLanePos[4 - k] * rtBoard.Width : noteLanePos[k] * rtBoard.Width), (rtBoard.Height * ratio) - ((rtBoard.Height * ratio) * ((waves[p][q].Y / 1000f) / Board.eFade))), null, waves[p][q].White ? Color.White : Global.FretColors[k], (float)Math.Atan2(((-rtBoard.Height * ratio) * (((waves[p][q + 1].Y - waves[p][q].Y) / 1000f) / Board.eFade)), (waves[p][q + 1].X - waves[p][q].X) * -xscale) + MathHelper.PiOver2, new Vector2(texLine.Width / 2, texLine.Height), new Vector2(0.5f, (new Vector2((waves[p][q + 1].X - waves[p][q].X) * xscale, ((rtBoard.Height * ratio) * (((waves[p][q + 1].Y - waves[p][q].Y) / 1000f) / Board.eFade)))).Length() * 1.05f / (float)texLine.Height), SpriteEffects.None, 0);
                                            }
                                            else
                                            {
                                                spritebatch.Draw(texLine, new Vector2((waves[p][q].X * xscale) + (IsLefty ? noteLanePos[4 - k] * rtBoard.Width : noteLanePos[k] * rtBoard.Width), (rtBoard.Height * ratio) - ((rtBoard.Height * ratio) * ((waves[p][q].Y / 1000f) / Board.eFade))), null, Global.FadedFretColors(k), (float)Math.Atan2(((-rtBoard.Height * ratio) * (((waves[p][q + 1].Y - waves[p][q].Y) / 1000f) / Board.eFade)), (waves[p][q + 1].X - waves[p][q].X) * xscale) + MathHelper.PiOver2, new Vector2(texLine.Width / 2, texLine.Height), new Vector2(0.5f, (new Vector2((waves[p][q + 1].X - waves[p][q].X) * xscale, ((rtBoard.Height * ratio) * (((waves[p][q + 1].Y - waves[p][q].Y) / 1000f) / Board.eFade)))).Length() * 1.05f / (float)texLine.Height), SpriteEffects.None, 0);
                                                spritebatch.Draw(texLine, new Vector2((waves[p][q].X * -xscale) + (IsLefty ? noteLanePos[4 - k] * rtBoard.Width : noteLanePos[k] * rtBoard.Width), (rtBoard.Height * ratio) - ((rtBoard.Height * ratio) * ((waves[p][q].Y / 1000f) / Board.eFade))), null, Global.FadedFretColors(k), (float)Math.Atan2(((-rtBoard.Height * ratio) * (((waves[p][q + 1].Y - waves[p][q].Y) / 1000f) / Board.eFade)), (waves[p][q + 1].X - waves[p][q].X) * -xscale) + MathHelper.PiOver2, new Vector2(texLine.Width / 2, texLine.Height), new Vector2(0.5f, (new Vector2((waves[p][q + 1].X - waves[p][q].X) * xscale, ((rtBoard.Height * ratio) * (((waves[p][q + 1].Y - waves[p][q].Y) / 1000f) / Board.eFade)))).Length() * 1.05f / (float)texLine.Height), SpriteEffects.None, 0);
                                            }
                                        }
                                }
                            }

                        }
                    }
                }
            }
            spritebatch.End();


            graphics.GraphicsDevice.SetRenderTarget(0, null);
            texWaves = rtWaves.GetTexture();
            graphics.GraphicsDevice.SetRenderTarget(0, rtWaves);
            graphics.GraphicsDevice.Clear(new Color(0, 0, 0, 0));

            spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);
            fader.Begin();
            fader.CurrentTechnique.Passes[0].Begin();

            fader.Parameters["blend"].SetValue(fh);
            spritebatch.Draw(texWaves, new Rectangle(0, 0, rtWaves.Width, rtWaves.Height), Color.White);


            spritebatch.End();
            fader.CurrentTechnique.Passes[0].End();
            fader.End();
            graphics.GraphicsDevice.SetRenderTarget(0, rtBoard);

            //float scale = (Board.eFade - Board.sFade) / (Board.eFade * 1.5f);

            fader.CommitChanges();


            graphics.GraphicsDevice.Clear(new Color(0, 0, 0, 255));
            //graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
            spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
            spritebatch.GraphicsDevice.RenderState.AlphaBlendEnable = true;
            spritebatch.GraphicsDevice.RenderState.SourceBlend = Blend.SourceColor;
            spritebatch.GraphicsDevice.RenderState.DestinationBlend = Blend.DestinationColor;
            spritebatch.GraphicsDevice.RenderState.AlphaDestinationBlend = Blend.InverseSourceAlpha;
            spritebatch.GraphicsDevice.RenderState.AlphaSourceBlend = Blend.SourceAlpha;
            float bgyscale = 2.0f;

            Color bgColor = Color.Gray;
            if (IsSPActivated())
                bgColor = new Color(200, 200, 0);
            else if (IsFailing)
                bgColor = new Color((byte)(255 * RhythmMaster.GetSingleton().GetFailTime()), 0, 0);
            else if (rockMeterLevel < 20)
            {
                if (RhythmMaster.GetSingleton().GetPercentBeat() > 0.5)
                    bgColor = new Color((byte)((RhythmMaster.GetSingleton().GetPercentBeat() - 0.5) * 255), 0, 0);
                else
                    bgColor = new Color((byte)((0.5 - RhythmMaster.GetSingleton().GetPercentBeat()) * 255), 0, 0);
            }
            else if (rockMeterLevel > 80)
                bgColor = new Color(30, (byte)(30 + ((rockMeterLevel - 80) / 20f) * 50), 30);
            else if (rockMeterLevel > 40)
                bgColor = new Color((byte)(30 + (1 - ((rockMeterLevel - 20) / 20f)) * 50), 30, 30);

            for (int i = 0; i < RhythmMaster.GetSingleton().GetSongData().info.barlines.Length; i++)
            {
                SongData.Barline[] barlines = RhythmMaster.GetSingleton().GetSongData().info.barlines;
                float Y = GetBoardPos(barlines[i].time / 1000.0, 1 - ratio) * rtBoard.Height;
                if (Y < 0)
                    break;
                if (i < RhythmMaster.GetSingleton().GetSongData().info.barlines.Length - 1)
                {
                    float Y2 = GetBoardPos(barlines[i + 1].time / 1000.0, 1 - ratio) * rtBoard.Height;
                    spritebatch.Draw(boardBackgrounds[boardBackground],new Rectangle(0,(int)Y2,rtBoard.Width,(int)(Y-Y2)),bgColor);
                }
            }
            for (int i = 0; i < RhythmMaster.GetSingleton().GetSongData().info.barlines.Length; i++)
            {
                SongData.Barline[] barlines = RhythmMaster.GetSingleton().GetSongData().info.barlines;
                float Y = GetBoardPos(barlines[i].time / 1000.0, 1 - ratio) * rtBoard.Height;
                if (Y < 0)
                    break;
                spritebatch.Draw(Global.texWhite, new Rectangle(0, (int)Y-3, rtBoard.Width, 7), Color.White);
                if (i < RhythmMaster.GetSingleton().GetSongData().info.barlines.Length - 1)
                {
                    float Y2 = GetBoardPos(barlines[i + 1].time / 1000.0, 1 - ratio) * rtBoard.Height;
                    spritebatch.Draw(Global.texWhite, new Rectangle(0, (int)(((Y2 - Y) * (1 / (float)(RhythmMaster.GetSingleton().GetSongData().info.barlines[i].numBeats * 2))) + Y) - 1, rtBoard.Width, 2), Color.White);
                    for (int k = 1; k <= RhythmMaster.GetSingleton().GetSongData().info.barlines[i].numBeats - 1; k++)
                    {
                        spritebatch.Draw(Global.texWhite, new Rectangle(0, (int)(((Y2 - Y) * (k / (float)RhythmMaster.GetSingleton().GetSongData().info.barlines[i].numBeats)) + Y) - 1, rtBoard.Width, 3), Color.White);
                        spritebatch.Draw(Global.texWhite, new Rectangle(0, (int)(((Y2 - Y) * (((k * 2) + 1) / (float)(RhythmMaster.GetSingleton().GetSongData().info.barlines[i].numBeats * 2))) + Y) - 1, rtBoard.Width, 2), Color.White);
                    }
                }
            }

            /*for (int k = -4; k < 8; k++)
            {
                if (IsSPActivated())
                    spritebatch.Draw(boardBackgrounds[boardBackground], new Rectangle(0, (int)(((((started < 2 ? -CurrentTime : CurrentTime) % (float)TicksPerSecond) / (float)TicksPerSecond) + (k / bgyscale)) * (rtBoard.Height / (Board.eFade * (1 / ratio)))), rtBoard.Width, 1 + (int)(rtBoard.Height / (Board.eFade * (1 / ratio) * bgyscale))), null, new Color(128, 128, 0), 0, new Vector2(0, 0), SpriteEffects.None, 1);
                else if (isFailing)
                    spritebatch.Draw(boardBackgrounds[boardBackground], new Rectangle(0, (int)(((((started < 2 ? -CurrentTime : CurrentTime) % (float)TicksPerSecond) / (float)TicksPerSecond) + (k / bgyscale)) * (rtBoard.Height / (Board.eFade * (1 / ratio)))), rtBoard.Width, 1 + (int)(rtBoard.Height / (Board.eFade * (1 / ratio) * bgyscale))), null, new Color((byte)(255 * failTime), 0, 0), 0, new Vector2(0, 0), SpriteEffects.None, 1);
                else if (rockMeterLevel < 20)
                {
                    if (RhythmMaster.GetSingleton().GetPercentBeat() > 0.5)
                        spritebatch.Draw(boardBackgrounds[boardBackground], new Rectangle(0, (int)(((((started < 2 ? -CurrentTime : CurrentTime) % (float)TicksPerSecond) / (float)TicksPerSecond) + (k / bgyscale)) * (rtBoard.Height / (Board.eFade * (1 / ratio)))), rtBoard.Width, 1 + (int)(rtBoard.Height / (Board.eFade * (1 / ratio) * bgyscale))), null, new Color((byte)((song.percentBeat - 0.5) * 255), 0, 0), 0, new Vector2(0, 0), SpriteEffects.None, 1);
                    else
                        spritebatch.Draw(boardBackgrounds[boardBackground], new Rectangle(0, (int)(((((started < 2 ? -CurrentTime : CurrentTime) % (float)TicksPerSecond) / (float)TicksPerSecond) + (k / bgyscale)) * (rtBoard.Height / (Board.eFade * (1 / ratio)))), rtBoard.Width, 1 + (int)(rtBoard.Height / (Board.eFade * (1 / ratio) * bgyscale))), null, new Color((byte)((0.5 - song.percentBeat) * 255), 0, 0), 0, new Vector2(0, 0), SpriteEffects.None, 1);
                }
                else if (rockMeterLevel > 80)
                    spritebatch.Draw(boardBackgrounds[boardBackground], new Rectangle(0, (int)(((((started < 2 ? -CurrentTime : CurrentTime) % (float)TicksPerSecond) / (float)TicksPerSecond) + (k / bgyscale)) * (rtBoard.Height / (Board.eFade * (1 / ratio)))), rtBoard.Width, 1 + (int)(rtBoard.Height / (Board.eFade * (1 / ratio) * bgyscale))), null, new Color(30, (byte)(30 + ((rockMeterLevel[i] - 80) / 20f) * 50), 30), 0, new Vector2(0, 0), SpriteEffects.None, 1);
                else if (rockMeterLevel < 40)
                    spritebatch.Draw(boardBackgrounds[boardBackground], new Rectangle(0, (int)(((((started < 2 ? -CurrentTime : CurrentTime) % (float)TicksPerSecond) / (float)TicksPerSecond) + (k / bgyscale)) * (rtBoard.Height / (Board.eFade * (1 / ratio)))), rtBoard.Width, 1 + (int)(rtBoard.Height / (Board.eFade * (1 / ratio) * bgyscale))), null, new Color((byte)(30 + (1 - ((rockMeterLevel[i] - 20) / 20f)) * 50), 30, 30), 0, new Vector2(0, 0), SpriteEffects.None, 1);
                else
                    spritebatch.Draw(boardBackgrounds[boardBackground], new Rectangle(0, (int)(((((started < 2 ? -CurrentTime : CurrentTime) % (float)TicksPerSecond) / (float)TicksPerSecond) + (k / bgyscale)) * (rtBoard.Height / (Board.eFade * (1 / ratio)))), rtBoard.Width, 1 + (int)(rtBoard.Height / (Board.eFade * (1 / ratio) * bgyscale))), null, new Color(30, 30, 30), 0, new Vector2(0, 0), SpriteEffects.None, 1);
            }

            int fiver = i == 0 || i == 3 ? 1 : 0;
            int lowestPoint = 0;
            //spritebatch.Draw(Board.boardTexPlain[fiver][0], new Rectangle(0, 0, rtBoard.Width,rtBoard.Height),null,Color.White);// (int)y, rtBoard.Width, (int)height), null, Color.White, 0, new Vector2(0, 0), SpriteEffects.None, 0.9f);
            for (int k = 0; k < songtimes.Length - 1; k++)
            {
                if (started < 2 && CurrentTime < 30 * TicksPerSecond)
                {
                    if ((int)(rtBoard.Height * ratio) - (int)(songtimes[k].X * scale) > lowestPoint)
                        lowestPoint = (int)(rtBoard.Height * ratio) - (int)(songtimes[k].X * scale);
                }

                {
                    float height = (songtimes[k + 1].X - songtimes[k].X) * scale;
                    float y = (rtBoard.Height * ratio) - (int)(songtimes[k].X * scale) - (int)((songtimes[k + 1].X - songtimes[k].X) * scale);

                    for (int j = 0; j < (int)(songtimes[k].Y + 0.5); j++)
                    {
                        spritebatch.Draw(texWhite, new Rectangle(0, (int)(y + ((j / songtimes[k].Y) * height)) - 1, rtBoard.Width, 5), Color.DarkGray);
                        spritebatch.Draw(texWhite, new Rectangle(0, (int)(y + (((j + 0.5) / songtimes[k].Y) * height)), rtBoard.Width, 3), Color.DarkGray);
                    }
                    spritebatch.Draw(texWhite, new Rectangle(0, (int)(y + (((0.5) / songtimes[k].Y) * height)), rtBoard.Width, 3), Color.DarkGray);
                    spritebatch.Draw(texWhite, new Rectangle(0, (int)y - 2, rtBoard.Width, 5), Color.DarkGray);
                }
            }*/

            // Draw Board Fills
            if ((GetBoardType().RPEnableType&Instrument.RockPowerEnableTypes.FILL)!=0)
                for (int k = 0; k < Fills.Length; k++)
                {
                    float halfMaxWidth = rtBoard.Width / (float)(GetBoardType().NumDrawnTracks*2);
                    if (!Fills[k].hitGreen)
                    {
                        float y1 = GetBoardPos(Fills[k].end, 1/ratio);
                        float y2 = GetBoardPos(Fills[k].time, 1/ratio);
                        for (int r = 0; r < GetBoardType().NumDrawnTracks; r++)
                        {
                            float center = ((r * 2 + 1) / (float)(GetBoardType().NumDrawnTracks*2)) * rtBoard.Width;
                            spritebatch.Draw(Board.drumfillTex, new Rectangle((int)(center - (halfMaxWidth * Fills[k].amount)), (int)y1, (int)(2 * (halfMaxWidth * Fills[k].amount)), (int)((y2-y1)+0.5f)), Global.FretColors[r]);
                        }
                    }
                }

            // Draw metainfo under frets
            if (!Global.DemoMode)
            {
                // just trust me on this one...
                float rpWidth = 0.97656f;
                // this should? be compounded on rpWidth
                float rpMeterWidth = 0.9375f;
                // the height of the overal console thingy
                int rpHeight = (int)((rtBoard.Width / (float)Board.spMeterBG.Width) * Board.spMeterBG.Height * Board.spMeterYScale);
                // this should be compounded on rpHeight
                float rpMeterHeight = 0.21875f;
                // draw the console thingy background
                spritebatch.Draw(Board.spMeterBG, 
                                 new Rectangle((int)(rtBoard.Width * ((1-rpWidth)/2) + 0.5f), 
                                               (int)(rtBoard.Height * ratio) + spMeterShift, 
                                               (int)(rtBoard.Width * rpWidth + 0.5f), 
                                               rpHeight), 
                                 null, Color.White, 0, new Vector2(0, 0), SpriteEffects.None, 0.1f);

                // draw the rock power meter
                {
                    // get the draw rect for the meter
                    // all the extraneous 0.5fs are for rounding... damn Rectangle
                    Rectangle meterRect = new Rectangle((int)(rtBoard.Width * ((1 - rpWidth) / 2) + rtBoard.Width * ((1 - rpMeterWidth) / 2) + 0.5f),
                                                        (int)(rtBoard.Height * ratio) + spMeterShift + (int)(rpHeight * 0.03125f + 0.5f),
                                                        (int)(rtBoard.Width * rpWidth * rpMeterWidth * GetSPAmount() + 0.5f),
                                                        (int)(rpHeight * rpMeterHeight + 0.5f));
                    spritebatch.Draw(Board.spMeterFill, meterRect, Color.Yellow);
                    meterRect.Width = (int)(rtBoard.Width * rpWidth * rpMeterWidth + 0.5f);

                    // TODO: find some way to do this that doesnt look retarded
                    if (GetSPAmount() >= 0.4999f)
                    {
                        float ringScale = (rtBoard.Width / (float)spMeterCurl.Width) * 0.05f;
                        for (int k = 0; k < spcircles.Length; k++)
                            if (spcircles[k].pos.X < GetSPAmount())
                                spritebatch.Draw(Board.spMeterCurl, 
                                                 new Vector2((spcircles[k].pos.X * meterRect.Width) + meterRect.Left, 
                                                             (spcircles[k].pos.Y * meterRect.Height) + meterRect.Top), 
                                                 null, new Color(new Vector4(1, 1, 1, spcircles[k].alpha)), 
                                                 spcircles[k].rotation, 
                                                 new Vector2(Board.spMeterCurl.Width / 2, Board.spMeterCurl.Height / 2), 
                                                 new Vector2(ringScale, ringScale), 
                                                 SpriteEffects.None, 0);
                    }
                }

                // TODO: jesus christ! wtf...
                int f = GetMultiplierFraction();
                int m = GetMultiplier();
                // draw the multiplier bar
                for (int k = 0; k < f; k++)
                    spritebatch.Draw(Board.spMeterLED, 
                                     new Rectangle((int)(rtBoard.Width * 0.109375f + rtBoard.Width * 0.0117f + 0.5f) + (int)(rtBoard.Width * 0.97656f * .078125f * k + 0.5f), 
                                                   (int)(rtBoard.Height * ratio) + spMeterShift + (int)(rpHeight * 0.3125f + 0.5f), 
                                                   (int)(rtBoard.Width * 0.97656f * .078125f + 0.5f) + 1, 
                                                   (int)(rpHeight * 0.3125f + 0.5f)), 
                                     null, 
                                     m <= 1 ? Color.Yellow : m == 2 && f == 10 ? Color.Yellow : m == 2 ? Color.Green : m == 3 && f == 10 ? Color.Green : Color.Purple, 
                                     0, new Vector2(0, 0), SpriteEffects.None, 0.8f);
                // draw the multiplier number
                if (Global.multToIndex[m] >= 0)
                    spritebatch.Draw(texMult[Global.multToIndex[m]], 
                                     new Rectangle(rtBoard.Width / 3, 
                                                   (int)(rtBoard.Height * ratio) + spMeterShift + (int)(rpHeight * 0.3125f + 0.5f), 
                                                   rtBoard.Width / 3, 
                                                   (int)(rpHeight * 0.5f + 0.5f)), 
                                     null, Color.White, 0, new Vector2(0, 0), SpriteEffects.None, 0);
            }
            spritebatch.End();

            graphics.GraphicsDevice.SetRenderTarget(0, null);
            texBoard = rtBoard.GetTexture();
            graphics.GraphicsDevice.SetRenderTarget(0, rtBoard);
            graphics.GraphicsDevice.Clear(new Color(0, 0, 0, 0));

            spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);

            fader.CommitChanges();
            fader.Begin();
            fader.CurrentTechnique.Passes[0].Begin();

            fader.Parameters["blend"].SetValue(fh);
            spritebatch.Draw(texBoard, new Rectangle(0, 0, rtBoard.Width, rtBoard.Height), Color.White);


            spritebatch.End();
            fader.CurrentTechnique.Passes[0].End();
            fader.End();
            graphics.GraphicsDevice.SetRenderTarget(0, null);
            texBoard = rtBoard.GetTexture();
        }

        private void DrawNotes(Matrix fling)
        {
            RenderMaster rm = RenderMaster.GetSingleton();
            RhythmMaster rtm = RhythmMaster.GetSingleton();
            BasicEffect effect = rm.bEffect;

            effect.DirectionalLight0.SpecularColor = new Vector3(0.6f, 0.6f, 0.6f);
            effect.DirectionalLight0.Enabled = true;
            effect.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(0, -2, -1));
            effect.DirectionalLight0.DiffuseColor = new Vector3(0.8f, 0.8f, 0.8f);
            effect.DirectionalLight1.Enabled = false;
            effect.DirectionalLight2.Enabled = false;
            effect.DiffuseColor = new Vector3(0.8f, 0.8f, 0.8f);

            if (GetBoardType().Dimensions == Instrument.BoardDimensions.THREE_DIMENSIONAL)
            {
                int lefty = 1;
                if (IsLefty)
                    lefty = -1;

                bool whited = false;
                for (int r = 0; r < GetBoardType().NumTracks; r++)
                {
                    if (r < GetBoardType().NumDrawnTracks)
                        effect.Texture = Board.texNotes[r];
                    else
                        effect.Texture = Board.texBarNotes[r-GetBoardType().NumDrawnTracks];
                    whited = false;
                    for (int p = Math.Max(index-32,0); p < Notes.Length; p++)
                    {
                        bool IsWhite = false;
                        for(int i=0;i<RPPhrases.Length;i++)
                            if(Notes[p].time>=RPPhrases[i].time && Notes[p].time<=RPPhrases[i].end)
                                IsWhite = true;
                        if (IsWhite && !whited)
                        { effect.Texture = Global.texWhite; whited = true; }
                        else if (!IsWhite && whited)
                        {
                            if (r < GetBoardType().NumDrawnTracks)
                                effect.Texture = Board.texNotes[r];
                            else
                                effect.Texture = Board.texBarNotes[r - GetBoardType().NumDrawnTracks];
                            whited = false;
                        }

                        /*if (GetBoardType() != PERCUSSIONIST)
                        {
                            if (Math.Abs(OutNotes[p].X - (-1)) < 0.01 && r != 0)
                                continue;
                            else if (Math.Abs(OutNotes[p].X - (-0.5)) < 0.01 && r != 1)
                                continue;
                            else if (Math.Abs(OutNotes[p].X) < 0.01 && r != 2)
                                continue;
                            else if (Math.Abs(OutNotes[p].X - (0.5)) < 0.01 && r != 3)
                                continue;
                            else if (Math.Abs(OutNotes[p].X - (1.0)) < 0.01 && r != 4)
                                continue;
                        }
                        else
                        {
                            if (Math.Abs(OutNotes[p].X - (-1)) < 0.01 && r != 1)
                                continue;
                            else if (Math.Abs(OutNotes[p].X - (-1 / 3f)) < 0.01 && r != 2)
                                continue;
                            else if (Math.Abs(OutNotes[p].X) < 0.01 && r != 5)
                                continue;
                            else if (Math.Abs(OutNotes[p].X - (1 / 3f)) < 0.01 && r != 3)
                                continue;
                            else if (Math.Abs(OutNotes[p].X - (1)) < 0.01 && r != 0)
                                continue;
                        }*/

                        // how far along the board... should be called Z probly
                        float ct = (float)rtm.GetCurrentTime();
                        float Y = ((Notes[p].time / 1000f) - ct);
                        if (Y > eFade)
                            continue;

                        if ((Notes[p].type & (((ulong)1) << r)) == 0)
                            continue;
                        if (Notes[p].visible[r] == NoteSet.VIS_STATE.INVISIBLE)
                            continue;

                        Matrix matIdentity, matTransl, matScale, matOrbit;
                        matIdentity = Matrix.Identity;
                        if( r >= GetBoardType().NumDrawnTracks )
                            matTransl = Matrix.CreateTranslation(0f, Board.height + (GetBoardBump() * Board.BOARD_BUMP_COEF), 0f);
                        else
                            matTransl = Matrix.CreateTranslation(0f, Board.height + (GetBoardBump() * Board.BOARD_BUMP_COEF) + 0.02f, 0f);

                        matOrbit = Matrix.CreateTranslation(((((r*2)+1)/(float)(GetBoardType().NumDrawnTracks*2))-0.5f) * lefty * Board.width * 2.0f, 0f, -(Board.length * Y) - Board.zeroZ) * fling;

                        if (r<GetBoardType().NumDrawnTracks)
                            matOrbit = Matrix.CreateRotationX(-Y * MathHelper.Pi) * matOrbit;

                        if (r>=GetBoardType().NumDrawnTracks)
                            matScale = Matrix.CreateScale(new Vector3(Board.width, Board.curveHeight, Board.length * 0.01f));
                        else
                            matScale = Matrix.CreateScale(new Vector3((Notes[p].IsHOPO(GetBoardType()) ? 0.5f : 1.0f) * 0.125f * Board.width, 0.05f * Board.length, 0.05f * Board.length));

                        float alpha;
                        if (Y < Board.sFade)
                            alpha = 1;
                        else if (Y < Board.eFade)
                            alpha = 1 - ((Y - Board.sFade) / (Board.eFade - Board.sFade));
                        else
                            alpha = 0;


                        effect.Alpha = alpha;

                        // identity, scale, rotate, orbit(translate & rotate), translate
                        effect.World = matIdentity * matScale * matOrbit * matTransl;



                        effect.CommitChanges();

                        GraphicsDeviceManager graphics = rm.graphics;
                        // 5: draw object - select vertex type, primitive type, # of primitives
                        graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                        graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                        graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                        if (r>=GetBoardType().NumDrawnTracks)
                        {
                            graphics.GraphicsDevice.Vertices[0].SetSource(Board.mdlTriggerBorder, 0, GBVertexFormat.SizeInBytes);
                            graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, (Board.mdlTriggerBorder.SizeInBytes / GBVertexFormat.SizeInBytes) / 3);
                        }
                        else
                        {
                            foreach (ModelMesh mesh in mdlNoteInside.Meshes)
                            {
                                foreach (ModelMeshPart part in mesh.MeshParts)
                                {
                                    //effect.Parameters["diffuseTexture"].SetValue(texWhite);
                                    effect.CommitChanges();
                                    graphics.GraphicsDevice.VertexDeclaration = part.VertexDeclaration;
                                    graphics.GraphicsDevice.Vertices[0].SetSource(mesh.VertexBuffer, part.StreamOffset, part.VertexStride);
                                    graphics.GraphicsDevice.Indices = mesh.IndexBuffer;
                                    graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.BaseVertex, 0, part.NumVertices, part.StartIndex, part.PrimitiveCount);
                                }
                            }
                            effect.SpecularPower = 32f;
                            effect.SpecularColor = new Vector3(1.0f, 1.0f, 1.0f);
                            effect.AmbientLightColor = new Vector3(0.2f, 0.2f, 0.2f);
                            foreach (ModelMesh mesh in mdlNote.Meshes)
                            {
                                foreach (ModelMeshPart part in mesh.MeshParts)
                                {

                                    //effect.Parameters["diffuseTexture"].SetValue(texWhite);
                                    effect.CommitChanges();
                                    graphics.GraphicsDevice.VertexDeclaration = part.VertexDeclaration;
                                    graphics.GraphicsDevice.Vertices[0].SetSource(mesh.VertexBuffer, part.StreamOffset, part.VertexStride);
                                    graphics.GraphicsDevice.Indices = mesh.IndexBuffer;
                                    graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.BaseVertex, 0, part.NumVertices, part.StartIndex, part.PrimitiveCount);
                                }
                            }
                            effect.AmbientLightColor = new Vector3(1.0f, 1.0f, 1.0f);
                        }
                        graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                        
                    }
                }
            }
        }

        private void DrawBoardDetail(Matrix fling, Matrix matTransl)
        {
            if (GetBoardType().Dimensions==Instrument.BoardDimensions.THREE_DIMENSIONAL)
            {
                RenderMaster rm = RenderMaster.GetSingleton();
                BasicEffect effect = rm.bEffect;
                SpriteBatch spritebatch = rm.spritebatch;
                GraphicsDeviceManager graphics = rm.graphics;

                VertexDeclaration vd = new VertexDeclaration(rm.graphics.GraphicsDevice, GBVertexFormat.Elements);
                graphics.GraphicsDevice.VertexDeclaration = vd;

                effect.AmbientLightColor = new Vector3(1.0f, 1.0f, 1.0f);
                effect.DiffuseColor = new Vector3(0.5f, 0.5f, 0.5f);
                effect.SpecularColor = new Vector3(0.0f, 0.0f, 0.0f);

                int lefty = 1;
                if (IsLefty)
                    lefty = -1;

                //if (GetBoardType() == GUITAR || GetBoardType() == BASS)
                {
                    Matrix matIdentity, matScale, matOrbit;

                    bool[] glow = new bool[5];
                    for (int p = 0; p < GetBoardType().NumDrawnTracks; p++)
                    {
                        float rise = -.01f;

                        matIdentity = Matrix.Identity;
                        //matTransl = Matrix.CreateTranslation(0f, Board.height + (GetBoardBump() * Board.BOARD_BUMP_COEF), 0f);
                        float lanePos = (((p * 2 + 1) / (float)(GetBoardType().NumDrawnTracks * 2)) - 0.5f) * 2;
                        float height = -(lanePos * lanePos) + 1;
                        matTransl = Matrix.CreateTranslation(lanePos * lefty * Board.width, Board.height + (GetBoardBump() * Board.BOARD_BUMP_COEF), 0f);
                        matOrbit = Matrix.CreateTranslation(0f, Board.curveHeight * height + rise + ((popup[p]) * 0.001f), -Board.zeroZ) * fling;
                        matScale = Matrix.CreateScale(new Vector3((Board.width / 5f), 0.05f, .05f));

                        // identity, scale, rotate, orbit(translate & rotate), translate
                        effect.World = matIdentity * matScale * matOrbit * matTransl;

                        //effect.Parameters["proj"].SetValue(matProj);

                        
                        if((controller.GetFrets()&(((ulong)1)<<p))!=0)
                        { effect.Texture = Board.texTriggersLit[p]; glow[p] = true; }
                        else
                            effect.Texture = Board.texTriggers[p];

                        effect.CommitChanges();

                        // 5: draw object - select vertex type, primitive type, # of primitives
                        graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                        graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                        graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                        graphics.GraphicsDevice.Vertices[0].SetSource(Board.mdlTrigger, 0, GBVertexFormat.SizeInBytes);
                        graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, (Board.mdlTrigger.SizeInBytes / GBVertexFormat.SizeInBytes) / 3);
                        graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                    }
                }
            }
        }

        private void DrawFlashes(Matrix fling)
        {
            RenderMaster rm = RenderMaster.GetSingleton();
            BasicEffect effect = rm.bEffect;
            GraphicsDeviceManager graphics = rm.graphics;

            int lefty = 1;
            if (IsLefty)
                lefty = -1;

            effect.Texture = Board.texBlast;
            for (int i = 0; i < GetBoardType().NumDrawnTracks; i++)
                if (popup[i] > 0)
                {
                    Matrix matRot, matTransl, matOrbit, matScale;
                    matRot = Matrix.CreateRotationY(flashRot) * Matrix.CreateRotationX(MathHelper.PiOver4);// *Matrix.CreateRotationY((float)(hvdistTOdir(venue.GetCamFor().X, venue.GetCamFor().Z) / 180 * Math.PI) + MathHelper.PiOver2);
                    float lanePos = (((i * 2 + 1) / (float)(GetBoardType().NumDrawnTracks * 2)) - 0.5f) * 2;
                    float height = -(lanePos * lanePos) + 1;
                    matTransl = Matrix.CreateTranslation(lanePos * (Board.width), 0.1f + Board.height + (GetBoardBump() * Board.BOARD_BUMP_COEF), 0f);
                    matOrbit = Matrix.CreateTranslation(0f, Board.curveHeight * height + ((popup[i]) * 0.001f), -Board.zeroZ) * fling;
                    matScale = Matrix.CreateScale(new Vector3(0.2f, 0.2f, 0.2f));

                    // identity, scale, rotate, orbit(translate & rotate), translate
                    effect.World = matScale * matRot * matOrbit * matTransl;

                    effect.DiffuseColor = Global.FretColors[i].ToVector3();
                    effect.CommitChanges();

                    // 5: draw object - select vertex type, primitive type, # of primitives
                    graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                    graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                    graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                    graphics.GraphicsDevice.Vertices[0].SetSource(Global.square, 0, GBVertexFormat.SizeInBytes);
                    graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
                    graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                }
            effect.DiffuseColor = new Vector3(1, 1, 1);
        }

        private void DrawBoard(Matrix fling, Matrix matTransl)
        {
            RenderMaster rm = RenderMaster.GetSingleton();
            GraphicsDeviceManager graphics = rm.graphics;
            BasicEffect effect = rm.bEffect;


            effect.AmbientLightColor = new Vector3(1.0f, 1.0f, 1.0f);
            effect.DiffuseColor = new Vector3(0.5f, 0.5f, 0.5f);
            effect.SpecularColor = new Vector3(0.0f, 0.0f, 0.0f);
            effect.TextureEnabled = true;

            VertexDeclaration vd = new VertexDeclaration(rm.graphics.GraphicsDevice, GBVertexFormat.Elements);
            graphics.GraphicsDevice.VertexDeclaration = vd;

            graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;
            Matrix matScale, matOrbit;
            matOrbit = Matrix.CreateTranslation(0f, 0f, -Board.zeroZ - (Board.eFade * Board.length)) * fling;
            matScale = Matrix.CreateScale(new Vector3(Board.width, Board.curveHeight, Board.length * Board.eFade * (1 / BOARD_FADE_RATIO)));
            effect.World = matScale * matOrbit * matTransl;
            effect.Texture = rtBoard.GetTexture();
            effect.Alpha = 1.0f;
            effect.CommitChanges();
            graphics.ApplyChanges();
            graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
            graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
            graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
            graphics.GraphicsDevice.Vertices[0].SetSource(Board.mdlBoard, 0, GBVertexFormat.SizeInBytes);
            graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, (Board.mdlBoard.SizeInBytes / GBVertexFormat.SizeInBytes) / 3);
            graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
        }

        private void DrawWaves(Matrix fling, Matrix matTransl)
        {
            RenderMaster rm = RenderMaster.GetSingleton();
            GraphicsDeviceManager graphics = rm.graphics;
            BasicEffect effect = rm.bEffect;

            effect.AmbientLightColor = new Vector3(1.0f, 1.0f, 1.0f);
            effect.DiffuseColor = new Vector3(0.5f, 0.5f, 0.5f);
            effect.SpecularColor = new Vector3(0.0f, 0.0f, 0.0f);
            effect.TextureEnabled = true;

            VertexDeclaration vd = new VertexDeclaration(rm.graphics.GraphicsDevice, GBVertexFormat.Elements);
            graphics.GraphicsDevice.VertexDeclaration = vd;

            Matrix matScale, matOrbit;
            matOrbit = Matrix.CreateTranslation(0f, 0.01f,  -Board.zeroZ - (Board.eFade * Board.length)) * fling;
            matScale = Matrix.CreateScale(new Vector3(Board.width, Board.curveHeight, Board.length * Board.eFade * (1 / BOARD_FADE_RATIO)));
            effect.World = matScale * matOrbit * matTransl;
            effect.Texture = rtWaves.GetTexture();
            effect.Alpha = 1.0f;
            effect.CommitChanges();
            graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
            graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
            graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
            graphics.GraphicsDevice.Vertices[0].SetSource(Board.mdlBoard, 0, GBVertexFormat.SizeInBytes);
            graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, (Board.mdlBoard.SizeInBytes / GBVertexFormat.SizeInBytes) / 3);
            graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
        }

        private void Help()
        {
            if (difficulty == Global.D_EASY)
            {
                if (rockMeterLevel > 80)
                    rockMeterLevel += 1 * (SPActivated ? 10 : 1);
                else
                    rockMeterLevel += 4f * (SPActivated ? 10 : 1);
            }
            else if (difficulty == Global.D_MEDIUM)
            {
                if (rockMeterLevel > 80)
                    rockMeterLevel += 1 * (SPActivated ? 10 : 1);
                else
                    rockMeterLevel += 3f * (SPActivated ? 10 : 1);
            }
            else if (difficulty == Global.D_HARD)
            {
                if (rockMeterLevel > 80)
                    rockMeterLevel += 0.75f * (SPActivated ? 10 : 1);
                else
                    rockMeterLevel += 2f * (SPActivated ? 10 : 1);
            }
            if (difficulty == Global.D_EXPERT)
            {
                if (rockMeterLevel > 80)
                    rockMeterLevel += 0.25f * (SPActivated ? 10 : 1);
                else
                    rockMeterLevel += 1f * (SPActivated ? 10 : 1);
            }
        }

        private void Hurt()
        {
            if (difficulty == Global.D_EASY)
            {
                if (rockMeterLevel > 80)
                    rockMeterLevel -= 1;//1f;
                else if (rockMeterLevel > 20)
                    rockMeterLevel -= 0.75f;//0.75f;
                else
                    rockMeterLevel -= 0.5f;// 0.5f;
            }
            else if (difficulty == Global.D_MEDIUM)
            {
                if (rockMeterLevel > 80)
                    rockMeterLevel -= 2;//1f;
                else if (rockMeterLevel > 20)
                    rockMeterLevel -= 1f;//0.75f;
                else
                    rockMeterLevel -= 0.5f;// 0.5f;
            }
            else if (difficulty == Global.D_HARD)
            {
                if (rockMeterLevel > 80)
                    rockMeterLevel -= 3;//1f;
                else if (rockMeterLevel > 20)
                    rockMeterLevel -= 1.5f;//0.75f;
                else
                    rockMeterLevel -= 1f;// 0.5f;
            }
            else if (difficulty == Global.D_EXPERT)
            {
                if (rockMeterLevel > 80)
                    rockMeterLevel -= 4;//1f;
                else if (rockMeterLevel > 20)
                    rockMeterLevel -= 3f;//0.75f;
                else
                    rockMeterLevel -= 2f;// 0.5f;
            }
        }

        /// <summary>
        /// Saves a board from failing
        /// e.g. someone saves using rock power
        /// </summary>
        public void Save()
        {
            rockMeterLevel = 80;
        }

        public float GetRockMeterLevel()
        {
            return rockMeterLevel;
        }

        public Texture2D GetRender()
        {
            return boardTarget.GetTexture();
        }
    }
}

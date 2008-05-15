using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace Unsigned
{
    class NoteSet : IComparable
    {
        public uint time;
        public byte type;
        public int length;
        public VIS_STATE[] visible;//0=visible,1=greyedout,2=invisible,3=invisibleButAvailable(HOPO)
        public bool burning;//for held notes

        public uint late;

        public enum VIS_STATE { VISIBLE = 0, GREYED_OUT = 1, INVISIBLE = 2, HOPOED = 3, OVERDONE=4/*drums*/, };

        private bool strummed;
        private byte pressed;

        public void Strum()
        {
            strummed = true;
        }

        public void addPressedGuitar(byte pressed)
        {
            if(Board.IsValidFrettage(type,pressed))
                 this.pressed = type;
        }

        public void addPressedDrums(byte pressed)
        {
            this.pressed |= pressed;
        }


        public bool IsGood(bool HOPOable)
        {
            if ((HOPOable && (type&(1<<5))!=0) || strummed)
                return pressed == type;
            return false;
        }

        public bool IsGood()
        {
            return (pressed & 0x1F) == (type & 0x1F);
        }

        public int CompareTo(object other)
        {
            return time.CompareTo(((NoteSet)other).time);
        }

        internal void Kill()
        {
            for(int i=0;i<visible.Length;i++)
                visible[i] = VIS_STATE.INVISIBLE;
        }
    }

    struct Results
    {
        public int hitNotes, missedNotes;
        public int totalNotes, totalSPPH;//temp
        public int hitSPPH, missedSPPH;
        public float percentSong;
    }

    struct VocalWord
    {
        public uint time,len;
        public short note;
        public string value;
    }

    struct VocalPhrase
    {
        public enum TYPE {REGULAR=0, BLANK=1, RHYTHM=2};
        public enum RTYPE { TAMBOURINE = 0, COWBELL = 1, CLAP = 2 };
        public TYPE type;
        public RTYPE rType;
        public bool SP;
        public VocalWord[] words;
        public uint time;
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
        private static Random random;
        private int type;
        public int xOffset;
        public static float vocalheight, vocaly, vocalzerox, vocalwidth;
        public static float width, length, curveHeight, height, rotate, zeroZ, sFade, eFade, spShift;
        public float yRotate=0;
        public static Texture2D[][] boardTexPlain;
        public Texture2D texBoard, texWaves;
        public float multSlide=0;
        public static float WaveDetail = 50f;
        public static Texture2D drumfillTex, spMeterBG, spMeterLED, spMeterFill, spMeterCurl;
        public static Texture2D vBar, vBGExt, vBGInt, vFuzz, vHeadBar, vGlow, texBlast;
        public float SPMFlash=0, SPMFVel=0;
        public static int[] boardValidBPM = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, };
        public static int[] boardBeatsIndex = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, };
        public static GBVertexFormat[] arrBoard;
        public static VertexBuffer mdlBoard, mdlSPM;
        public static VertexBuffer mdlTrigger, mdlTriggerBorder;
        public static Model mdlNote, mdlNoteInside;
        public static Texture2D[] texNotes, texTriggers, texTriggersLit;
        public static Texture2D texTriggerBorder, texTriggerBorderLit;
        public static float BOARD_BUMP_COEF = 0.002f;
        public static float spMeterYScale = 0.4f;
        public int spMeterShift = 7, spMeterShiftDrums=10;
        private Song song;
        public float flashRot;
        private byte difficulty;
        private int index;
        private float multiplier=1;
        public byte lastPressed=0;
        private int[] starPts;
        private int score;
        private bool LeftySwitch = false;
        private float[] popup, popupSpeed;
        private float StarPowerAmount = 0, SPADisplay = 0;
        private int SPIndex, DFIndex;
        private int[] SPStart, SPEnd;
        private int[] DFStart, DFEnd;
        private bool[] DFHitGreen;
        private float[] dfA;
        private bool SPGood = true;
        private bool SPActivated = false;
        public static int[] guitarToDrums = { 1, 2, 3, 0, 4 };
        public static int[] drumsToGuitar = { 3, 0, 1, 2, 4 };
        public LinkedList<WaveVector2> whammyage;
        float waveoffset=0;
        Results myResults;

        private static uint PILLOW = 100;//padding in front of and behind note

        public Vector4[] OutNotes;
        public int notesLen;
        public WaveNode[][] waves;
        public int wavesLen;
        public int[] wavesSubLen;

        private NoteSet[] notes;
        private VocalPhrase[] vNotes;
        public const byte NS_GREEN = 1, NS_RED = 2, NS_YELLOW = 4, NS_BLUE = 8, NS_ORANGE = 16, NS_HOPO = 32;
        private static string[] SETTINGS_EXT = { ".gbg", ".gbv", ".gbd", ".gbb", };

        public Board(int type, int xOffset, Song song, byte difficulty)
        {
            this.type = type;
            this.xOffset = xOffset;
            this.song = song;
            this.difficulty = difficulty;
            LoadNotes(song.GetFilename() + SETTINGS_EXT[type],difficulty,song);
            score = 0;
            DFIndex = 0;
            SPIndex = 0;
            popup = new float[5];
            popupSpeed = new float[5];
            if (type == 0 || type == 3)
            {
                whammyage = new LinkedList<WaveVector2>();
                waves = new WaveNode[5][];
                for (int i = 0; i < 5; i++)
                    waves[i] = new WaveNode[50];
            }
            OutNotes = new Vector4[0];
            random = new Random();
            
        }

        public int GetBoardType()
        {
            return type;
        }

        public int GetXOffset()
        {
            return xOffset;
        }

        public static void InitModel(GraphicsDeviceManager graphics, ContentManager content, Effect engine)
        {
            {
                float[] xs = { -1f, -1f, -.65f, -.65f, -.65f, -1f,     -.65f, -.65f, -.35f, -.65f, -.35f, -.35f,     -.35f, -.35f,    0f, -.35f,    0f,    0f,};
                float[] ys = {  0f,  0f,   .5f,   .5f,   .5f,  0f,       .5f,   .5f,  .85f,   .5f,  .85f,  .85f,      .85f,  .85f,    1f,  .85f,    1f,    1f,};
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
                        part.Effect = engine;
                mdlNote = content.Load<Model>("meshes\\note");
                foreach (ModelMesh mesh in mdlNote.Meshes)
                    foreach(ModelMeshPart part in mesh.MeshParts)
                        part.Effect = engine;      
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

        float[] drumxvals = { 1f, -1f, -1 / 3f, 1 / 3f, 0f };
        public void GetNotes(long currenttime, long viewdistance)
        {
            int k;
            for (k = 0; k < notes.Length; k++)
                if ((long)notes[k].time*(Game1.TicksPerSecond/1000) > (currenttime) - (viewdistance / 4))
                    break;
            int count = 0;
            bool fill = false;
            for (int i = k; i<notes.Length && (long)notes[i].time*(Game1.TicksPerSecond/1000) < currenttime + viewdistance; i++)
            {
                if (GetBoardType() != Game1.PERCUSSIONIST)
                {
                    if (notes[i].visible[0]==NoteSet.VIS_STATE.INVISIBLE || notes[i].visible[0] == NoteSet.VIS_STATE.HOPOED)
                        continue;
                    if ((notes[i].type & NS_GREEN) != 0)
                        count++;
                    if ((notes[i].type & NS_RED) != 0)
                        count++;
                    if ((notes[i].type & NS_YELLOW) != 0)
                        count++;
                    if ((notes[i].type & NS_BLUE) != 0)
                        count++;
                    if ((notes[i].type & NS_ORANGE) != 0)
                        count++;
                }
                else
                {

                    if (StarPowerAmount >= 0.5 && !SPActivated && DFIndex<DFStart.Length && notes[i].time >= DFStart[DFIndex] && notes[i].time <= DFEnd[DFIndex])
                        fill=true;
                    else if (StarPowerAmount >= 0.5 && !SPActivated && DFIndex+1<DFStart.Length && notes[i].time >= DFStart[DFIndex+1] && notes[i].time <= DFEnd[DFIndex+1])
                        fill=true;
                    else
                    {
                        if (fill)
                        { count+=2; fill = false; }
                        if ((notes[i].type & NS_GREEN) != 0 && notes[i].visible[0] == 0)
                            count++;
                        if ((notes[i].type & NS_RED) != 0 && notes[i].visible[1] == 0)
                            count++;
                        if ((notes[i].type & NS_YELLOW) != 0 && notes[i].visible[2] == 0)
                            count++;
                        if ((notes[i].type & NS_BLUE) != 0 && notes[i].visible[3] == 0)
                            count++;
                        if ((notes[i].type & NS_ORANGE) != 0 && notes[i].visible[4] == 0)
                            count++;
                    }
                }
            }

            if (fill)
               { count+=2; fill = false; }

            if(OutNotes.Length < count)
                OutNotes = new Vector4[count];
            notesLen = count;
            int indx = 0;
            int plus = 0;
            for (int i = k; i < notes.Length && (long)notes[i].time*(Game1.TicksPerSecond/1000) < currenttime + viewdistance; i++)
            {
                float sp = 0f;
                if (SPIndex<SPStart.Length && i >= SPStart[SPIndex] && i <= SPEnd[SPIndex] && SPGood)
                    sp = 1f;

                if (GetBoardType() != Game1.PERCUSSIONIST)
                {
                    if (notes[i].visible[0] == NoteSet.VIS_STATE.INVISIBLE || notes[i].visible[0] == NoteSet.VIS_STATE.HOPOED)
                        continue;
                    for(int r=0;r<5;r++)
                    if ((notes[i].type & (1<<r)) != 0)
                    {
                        OutNotes[indx].X=-1f+(0.5f*r);
                        OutNotes[indx].Y=(long)notes[i].time*(Game1.TicksPerSecond/1000);
                        OutNotes[indx].Z=((notes[i].type & NS_HOPO) != 0) ? 1f : -1f;
                        OutNotes[indx].W=sp;
                        indx++;
                    }
                }
                else
                {
                    if (StarPowerAmount >= 0.5 && !SPActivated && DFIndex < DFStart.Length && notes[i].time >= DFStart[DFIndex] && notes[i].time <= DFEnd[DFIndex])
                        fill = true;
                    else if (StarPowerAmount >= 0.5 && !SPActivated && DFIndex + 1 < DFStart.Length && notes[i].time >= DFStart[DFIndex+1] && notes[i].time <= DFEnd[DFIndex+1])
                        fill = true;
                    else
                    {
                        if (fill)
                        {
                            OutNotes[indx].X=dfA[DFIndex + plus];
                            OutNotes[indx].Y=(long)Math.Max(DFStart[DFIndex + plus]*(Game1.TicksPerSecond/1000),currenttime);
                            OutNotes[indx].Z=1;
                            OutNotes[indx].W=DFHitGreen[DFIndex + plus] ? 1 : 0;
                            OutNotes[indx + 1].X=dfA[DFIndex + plus];
                            OutNotes[indx + 1].Y=(long)DFEnd[DFIndex + plus]*(Game1.TicksPerSecond/1000);
                            OutNotes[indx + 1].Z=2;
                            OutNotes[indx + 1].W=0;
                            indx+=2;
                            fill = false;
                            plus++;
                        }
                        for(int r=0;r<5;r++)
                            if ((notes[i].type & (1 << r)) != 0 && notes[i].visible[r] == 0)
                            {
                                OutNotes[indx].X=drumxvals[r];
                                OutNotes[indx].Y=(long)notes[i].time*(Game1.TicksPerSecond/1000);
                                OutNotes[indx].Z=0;
                                OutNotes[indx].W=sp;
                                indx++;
                            }
                    }
                }
            }
            if (fill)
            {
                OutNotes[indx].X=dfA[DFIndex+plus];
                OutNotes[indx].Y=(long)Math.Max(DFStart[DFIndex + plus]*(Game1.TicksPerSecond/1000),currenttime);
                OutNotes[indx].Z=1;
                OutNotes[indx].W=0;
                OutNotes[indx + 1].X=dfA[DFIndex + plus];
                OutNotes[indx+1].Y=(long)DFEnd[DFIndex + plus]*(Game1.TicksPerSecond/1000);
                OutNotes[indx+1].Z = 2;
                OutNotes[indx+1].W=0;
                indx += 2;
                fill = false;
                plus++;
            }
            for (int i = 0; i < notesLen; i++)
                OutNotes[i].Y = (OutNotes[i].Y - currenttime) / (float)Game1.TicksPerSecond;
        }

        private void LoadNotes(String filename, byte diff, Song song)
        {
            Vector2[] bars = song.GetAllBars();
            System.IO.BinaryReader reader = new System.IO.BinaryReader(System.IO.File.OpenRead("songdata\\" + filename));

            byte version = reader.ReadByte();

            if (GetBoardType() != Game1.VOCALIST)
            {
                int numSPP = reader.ReadInt32();
                myResults.totalSPPH = numSPP;
                SPStart = new int[numSPP];
                SPEnd = new int[numSPP];
                for (int i = 0; i < numSPP; i++)
                {
                    SPStart[i] = reader.ReadInt32();
                    SPEnd[i] = reader.ReadInt32() + SPStart[i];
                }

                if (GetBoardType() == Game1.PERCUSSIONIST)
                {
                    int numDFP = reader.ReadInt32();
                    DFStart = new int[numDFP];
                    DFEnd = new int[numDFP];
                    dfA = new float[numDFP];
                    DFHitGreen = new bool[numDFP];
                    for (int i = 0; i < numDFP; i++)
                    {
                        DFStart[i] = reader.ReadInt32();
                        DFEnd[i] = reader.ReadInt32();
                        dfA[i] = 0;
                        DFHitGreen[i] = false;
                    }
                }
                for (int dfrep = 3; dfrep >= 0; dfrep--)
                {
                    bool skip = true;
                    int difr = reader.ReadInt32();
                    if (diff == Game1.D_EXPERT && difr == 3)
                        skip = false;
                    if (diff == Game1.D_HARD && difr == 2)
                        skip = false;
                    if (diff == Game1.D_MEDIUM && difr == 1)
                        skip = false;
                    if (diff == Game1.D_EASY && difr == 0)
                        skip = false;

                    if (skip)
                    {
                        int notesn = reader.ReadInt32();
                        for (int i = 0; i < notesn; i++)
                        {
                            reader.ReadByte();
                            if (GetBoardType() != Game1.DRUMS)
                            {
                                reader.ReadInt32();
                                reader.ReadInt32();
                            }
                            else
                            {
                                reader.ReadInt32();
                            }
                        }
                        for (int i = 1; i <= 5; i++)
                            reader.ReadInt32();
                    }
                    else
                    {
                        notes = new NoteSet[reader.ReadInt32()];
                        myResults.totalNotes = notes.Length;
                        for (int i = 0; i < notes.Length; i++)
                        {
                            notes[i] = new NoteSet();
                            notes[i].type = reader.ReadByte();
                            if (GetBoardType() != Game1.DRUMS)
                            {
                                notes[i].time = reader.ReadUInt32();
                                notes[i].length = reader.ReadInt32();
                                notes[i].visible = new NoteSet.VIS_STATE[1];
                            }
                            else
                            {
                                notes[i].visible = new NoteSet.VIS_STATE[5];
                                notes[i].time = reader.ReadUInt32();
                                notes[i].length = 0;
                            }
                        }
                        starPts = new int[6];
                        starPts[0] = 0;
                        for (int i = 1; i <= 5; i++)
                            starPts[i] = reader.ReadInt32();
                        int numrep = 0;
                        uint curtm = 0;
                        for (int i = 0; i < notes.Length; i++)
                        {
                            if (notes[i].time < curtm)
                                numrep++;
                            curtm = notes[i].time;
                        }
                        break;
                    }
                }
            }
            else
            {
                vNotes = new VocalPhrase[reader.ReadInt32()];
                for (int i = 0; i < vNotes.Length; i++)
                {
                    vNotes[i] = new VocalPhrase();
                    vNotes[i].time = reader.ReadUInt32();
                    vNotes[i].type = (VocalPhrase.TYPE)reader.ReadByte();
                    vNotes[i].words = new VocalWord[reader.ReadInt32()];
                    switch (vNotes[i].type)
                    {
                        case VocalPhrase.TYPE.REGULAR:
                            for (int k = 0; k < vNotes[i].words.Length; k++)
                            {
                                vNotes[i].words[k] = new VocalWord();
                                vNotes[i].words[k].time = reader.ReadUInt32();
                                vNotes[i].words[k].len = reader.ReadUInt32();
                                vNotes[i].words[k].note = reader.ReadInt16();
                                vNotes[i].words[k].value = reader.ReadString();
                            }
                            break;
                        case VocalPhrase.TYPE.BLANK:
                            break;
                        case VocalPhrase.TYPE.RHYTHM:
                            vNotes[i].rType = (VocalPhrase.RTYPE)reader.ReadByte();
                            for (int k = 0; k < vNotes[i].words.Length; k++)
                            {
                                vNotes[i].words[k].time = reader.ReadUInt32();
                            }
                            break;
                    }
                }
            }

            reader.Close();

            if (GetBoardType() == Game1.VOCALIST)
            {

                return;
            }

            //post processing
            for (int i = 0; i < notes.Length; i++)
            {
                if (i == notes.Length - 1)
                    notes[i].late = notes[i].time + PILLOW;
                else
                {
                    if (notes[i + 1].time - notes[i].time < PILLOW * 2)
                        notes[i].late = notes[i].time+((notes[i + 1].time - notes[i].time) / 2);
                    else
                        notes[i].late = notes[i].time + PILLOW;
                }
            }
            for (int i = 0; i < SPStart.Length; i++)
            {
                int k;
                for (k = 0; k < notes.Length; k++)
                {
                    if (notes[k].time >= SPStart[i] && notes[k].time <= SPEnd[i])
                    {
                        SPStart[i] = k;
                        break;
                    }
                }
                for (k = 0; k < notes.Length; k++)
                {
                    if (notes[k].time > SPEnd[i])
                    {
                        SPEnd[i] = k;
                        break;
                    }
                }
            }
        }

        public static bool IsValidFrettage(byte note, byte pressed)
        {
            note &= (byte)(~NS_HOPO & 255);
            int numNotes = 0;
            for (int i = 0; i < 5; i++)
                if ((note & Game1.bits[i]) != 0)
                    numNotes++;
            if (numNotes > 1 && pressed == (note & (~Game1.bits[5])))
                return true;
            else if (numNotes > 1)
                return false;
            if ((note & pressed) != note)
                return false;
            for (int i = 4; i >= 0; i--)
            {
                if ((note & Game1.bits[i]) != 0)
                    return true;
                if ((pressed & Game1.bits[i]) != 0)
                    return false;
            }
            return false;
            /*OLD VERSION:
             * bool fair = false;
            if ((note & NS_ORANGE) != 0)
            {
                if (((pressed & NS_ORANGE) != 0))
                {
                    if (((note & NS_GREEN) != 0) == false && ((note & NS_RED) != 0) == false && ((note & NS_YELLOW) != 0) == false && ((note & NS_BLUE) != 0) == false)
                        fair = true;
                    if (((((note & NS_GREEN) != 0) == false && !(((pressed & NS_GREEN) != 0))) || (((note & NS_GREEN) != 0) == true && ((pressed & NS_GREEN) != 0))) &&
                        ((((note & NS_RED) != 0) == false && !(((pressed & NS_RED) != 0))) || (((note & NS_RED) != 0) == true && ((pressed & NS_RED) != 0))) &&
                        ((((note & NS_YELLOW) != 0) == false && !(((pressed & NS_YELLOW) != 0))) || (((note & NS_YELLOW) != 0) == true && ((pressed & NS_YELLOW) != 0))) &&
                        ((((note & NS_BLUE) != 0) == false && !(((pressed & NS_BLUE) != 0))) || (((note & NS_BLUE) != 0) == true && ((pressed & NS_BLUE) != 0))))
                        fair = true;
                }
            }
            else if (((note & NS_BLUE) != 0) == true)
            {
                if (((pressed & NS_BLUE) != 0) && !((pressed & NS_ORANGE) != 0))
                {
                    if (((note & NS_GREEN) != 0) == false && ((note & NS_RED) != 0) == false && ((note & NS_YELLOW) != 0) == false)
                        fair = true;
                    if (((((note & NS_GREEN) != 0) == false && !(((pressed & NS_GREEN) != 0))) || (((note & NS_GREEN) != 0) == true && ((pressed & NS_GREEN) != 0))) &&
                        ((((note & NS_RED) != 0) == false && !(((pressed & NS_RED) != 0))) || (((note & NS_RED) != 0) == true && ((pressed & NS_RED) != 0))) &&
                        ((((note & NS_YELLOW) != 0) == false && !(((pressed & NS_YELLOW) != 0))) || (((note & NS_YELLOW) != 0) == true && ((pressed & NS_YELLOW) != 0))))
                        fair = true;
                }
            }
            else if (((note & NS_YELLOW) != 0) == true)
            {
                if (((pressed & NS_YELLOW) != 0) && !((pressed & NS_ORANGE) != 0) && !((pressed & NS_BLUE) != 0))
                {
                    if (((note & NS_GREEN) != 0) == false && ((note & NS_RED) != 0) == false)
                        fair = true;
                    if (((((note & NS_GREEN) != 0) == false && !((pressed & NS_GREEN) != 0)) || (((note & NS_GREEN) != 0) == true && ((pressed & NS_GREEN) != 0))) &&
                        ((((note & NS_RED) != 0) == false && !(((pressed & NS_RED) != 0))) || (((note & NS_RED) != 0) == true && ((pressed & NS_RED) != 0))))
                        fair = true;
                }
            }
            else if (((note & NS_RED) != 0) == true)
            {
                if (((pressed & NS_RED) != 0) && !((pressed & NS_ORANGE) != 0) && !((pressed & NS_BLUE) != 0) && !((pressed & NS_YELLOW) != 0))
                {
                    if (((note & NS_GREEN) != 0) == false || ((pressed & NS_GREEN) != 0))
                        fair = true;
                }
            }
            else if (((note & NS_GREEN) != 0) == true)
            {
                if (((pressed & NS_GREEN) != 0) && !((pressed & NS_ORANGE) != 0) && !((pressed & NS_BLUE) != 0) && !((pressed & NS_YELLOW) != 0) && !((pressed & NS_RED) != 0))
                {
                    fair = true;
                }
            }
            return fair;*/
        }

        public byte Update(GameTime gameTime, long currenttime,Game1 reff, int ind, byte pressed)
        {
            if (GetBoardType() != Game1.VOCALS)
            {

                for (int i = 0; i < 5; i++)
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
                if (Game1.DemoMode)
                {
                    if(index<notes.Length)
                        if (notes[index].time - currenttime < 0)
                        {
                            if(!notes[index].burning)
                            {
                                if (notes[index].length > 0)
                                {
                                    for(int i=0;i<notes[index].visible.Length;i++)
                                        notes[index].visible[i] = NoteSet.VIS_STATE.INVISIBLE;
                                    notes[index].burning = true;
                                    for (int i = 0; i < 5; i++)
                                        if ((notes[index].type & (1 << i)) != 0)
                                            if (ind == 0 || ind == 3)
                                                popupSpeed[i] += 100;
                                            else
                                                popupSpeed[guitarToDrums[i]] += 100;
                                    flashRot = (float)(Game1.r.Next() * Math.PI * 2);
                                    return notes[index].type;

                                }
                                else
                                {
                                    for (int i = 0; i < notes[index].visible.Length; i++)
                                        notes[index].visible[i] = NoteSet.VIS_STATE.INVISIBLE;
                                    for (int i = 0; i < 5; i++)
                                        if ((notes[index].type & (1 << i)) != 0)
                                            if (ind == 0 || ind == 3)
                                                popupSpeed[i] += 100;
                                            else
                                                popupSpeed[guitarToDrums[i]] += 100;
                                    index++;
                                    flashRot = (float)(Game1.r.Next() * Math.PI * 2);
                                    return notes[index - 1].type;
                                }
                            }
                            else
                            {
                                if ((notes[index].time + notes[index].length) - currenttime < 0)
                                {
                                    index++;
                                    return 0;
                                }
                                else
                                {
                                    reff.Burn(gameTime, notes[index].type, ind);
                                    //Burn(gameTime, notes[index].type);
                                    return 0;// (byte)(Game1.bits[7] | notes[index].type);
                                }
                            }
                        }
                    return 0;
                }
                waveoffset -= gameTime.ElapsedGameTime.Milliseconds / 100f;

                if (index< notes.Length && notes[index].time - PILLOW < currenttime)
                {
                    byte newPressed = (byte)((pressed^lastPressed)&pressed);
                    lastPressed = pressed;

                    if (GetBoardType() == Game1.DRUMS)
                    {
                        notes[index].addPressedDrums(newPressed);
                        if (notes[index].IsGood())
                        {
                            notes[index].Kill();
                            for (int i = 0; i < 5; i++)
                                if ((notes[index].type & (1 << i)) != 0)
                                    popupSpeed[i] += 100f;
                            for (int i = 0; i < 5; i++)
                                if ((notes[index].type & (1 << i)) != 0)
                                { multiplier += 0.1f; }
                            for (int i = 0; i < 5; i++)
                                if ((notes[index].type & (1 << i)) != 0)
                                { score += 100 * (int)multiplier; }
                            reff.Help(ind);
                            myResults.hitNotes++;
                            index++;
                            return notes[index-1].type;
                        }
                    }
                    else
                    {
                        if (notes[index].burning)
                        {
                            if (notes[index].time + notes[index].length <= currenttime)
                                index++;
                            else
                            {
                                Burn(gameTime, notes[index].type);
                                reff.Burn(gameTime, notes[index].type, ind);
                            }
                        }
                        else
                        {
                            notes[index].addPressedGuitar(pressed);
                            if (notes[index].IsGood(index > 0 && notes[index - 1].visible[0] == NoteSet.VIS_STATE.INVISIBLE))
                            {
                                notes[index].Kill();
                                for (int i = 0; i < 5; i++)
                                    if ((notes[index].type & (1 << i)) != 0)
                                        popupSpeed[i] += 100f;
                                multiplier += 0.1f;
                                for (int i = 0; i < 5; i++)
                                    if ((notes[index].type & (1 << i)) != 0)
                                    { score += 100 * (int)multiplier; }
                                myResults.hitNotes++;
                                reff.Help(ind);
                                index++;
                                return notes[index - 1].type;
                            }
                        }
                    }

                    if (notes[index].late <= currenttime)
                    {
                        if (SPIndex < SPStart.Length && index >= SPStart[SPIndex] && index <= SPEnd[SPIndex])
                            SPGood = false;
                        index++; 
                        multiplier = 1; 
                        reff.Hurt(ind); 
                        myResults.missedNotes++;
                    }
                }

                if (SPIndex < SPStart.Length && index > SPEnd[SPIndex])
                {
                    if (SPGood)
                    {
                        StarPowerAmount += 0.25f;
                        myResults.hitSPPH++;
                    }
                    else 
                        myResults.missedSPPH++;
                    SPGood = true;
                    SPIndex++;
                }

                if (GetBoardType() != Game1.BASS && multiplier > 4)
                    multiplier = 4;
                if (GetBoardType() == Game1.BASS && multiplier > 6)
                    multiplier = 6;

                /*if (GetBoardType() != Game1.PERCUSSIONIST)
                    if (index < notes.Length)
                        if (multiplier > 1)
                            if ((notes[index].type & NS_HOPO) != 0)
                                if (Math.Abs((long)notes[index].time - (long)currenttime) < 100)
                                    if (notes[index].visible[0] != NoteSet.VIS_STATE.HOPOED)
                                        if (IsValidFrettage(notes[index].type, pressed))
                                        {
                                            int scre = Game1.AddBits(notes[index].type);
                                            scre -= 1;
                                            scre *= (int)(100 * multiplier);
                                            score += scre;
                                            for(int i=0;i<5;i++)
                                                if((notes[index].type & (1<<i)) != 0)
                                                    popupSpeed[i] += 100f;
                                            multiplier += 0.1f;
                                            notes[index].visible[0] = NoteSet.VIS_STATE.HOPOED;
                                            reff.AddShards(notes[index].type, ind);
                                            if (notes[index].length > 0)
                                                notes[index].burning = true;
                                        }
                while (index < notes.Length - 1 && Math.Abs((notes[index].time + notes[index].length) - (long)currenttime) > Math.Abs((notes[index + 1].time) - (long)currenttime))
                {
                    if (GetBoardType() != Game1.PERCUSSIONIST)
                    {
                        if (notes[index].visible[0] != NoteSet.VIS_STATE.HOPOED && !notes[index].burning)
                        {
                            reff.Hurt(ind);
                            multiplier = 1;
                            notes[index].visible[0] = NoteSet.VIS_STATE.GREYED_OUT;
                            myResults.missedNotes++;
                            if (SPIndex < SPStart.Length && notes[index].time >= SPStart[SPIndex] && notes[index].time < SPEnd[SPIndex])
                                SPGood = false;
                        }
                    }
                    else
                    {
                        bool shouldhurt = false;
                        for (int i = 0; i < 5; i++)
                            if ((notes[index].type & Game1.bits[i]) != 0 && notes[index].visible[i] == 0)
                            {
                                shouldhurt = true;
                                notes[index].visible[i] = NoteSet.VIS_STATE.GREYED_OUT;
                            }
                        bool indf = false;
                        if (index<notes.Length && DFIndex<DFStart.Length && !SPActivated && StarPowerAmount>=0.5f && notes[index].time >= DFStart[DFIndex] && notes[index].time <= DFEnd[DFIndex])
                            indf = true;
                        if (indf)
                        {
                            myResults.hitNotes++;
                        }
                        else if (shouldhurt)
                        {
                            reff.Hurt(ind);
                            multiplier = 1;
                            if (SPIndex < SPStart.Length && notes[index].time >= SPStart[SPIndex] && notes[index].time < SPEnd[SPIndex])
                            {
                                SPGood = false;

                            }
                            myResults.missedNotes++;
                            //reff.Hurt(2);
                        }
                        else
                        {
                            reff.Help(2);
                            myResults.hitNotes++;
                            int scre = 0;
                            for (int i = 0; i < 5; i++)
                                if ((notes[index].type & Game1.bits[i]) != 0)
                                    scre++;
                            score += scre * (int)multiplier * 100;
                            multiplier = Math.Min(multiplier + (scre*0.1f), 4);
                        }
                    }
                    index++;
                }
                if (index < notes.Length && (notes[index].time + notes[index].length) - (long)currenttime < -100)
                {
                    if (GetBoardType() != Game1.PERCUSSIONIST)
                    {
                        if (notes[index].visible[0] != NoteSet.VIS_STATE.HOPOED && !notes[index].burning)
                        {
                            reff.Hurt(ind);
                            multiplier = 1;
                            notes[index].visible[0] = NoteSet.VIS_STATE.GREYED_OUT;
                            myResults.missedNotes++;
                            if (SPIndex < SPStart.Length && notes[index].time >= SPStart[SPIndex] && notes[index].time < SPEnd[SPIndex])
                                SPGood = false;
                        }
                    }
                    else
                    {
                        bool shouldhurt = false;
                        for (int i = 0; i < 5; i++)
                            if ((notes[index].type & Game1.bits[i]) != 0 && notes[index].visible[i] == NoteSet.VIS_STATE.VISIBLE)
                            {
                                shouldhurt = true;
                                notes[index].visible[i] = NoteSet.VIS_STATE.GREYED_OUT;
                            }
                            else if ((notes[index].type & Game1.bits[i]) == 0 && notes[index].visible[i] == NoteSet.VIS_STATE.OVERDONE)
                            {
                                shouldhurt = true;
                            }
                        

                        
                        bool indf = false;
                        if (index<notes.Length && DFIndex<DFStart.Length && !SPActivated && StarPowerAmount>=0.5f && notes[index].time > DFStart[DFIndex] && notes[index].time < DFEnd[DFIndex])
                            indf = true;
                        if (indf)
                        {
                            myResults.hitNotes++;

                        }
                        else if (shouldhurt)
                        {
                            reff.Hurt(ind);
                            multiplier = 1;
                            if (SPIndex < SPStart.Length && notes[index].time >= SPStart[SPIndex] && notes[index].time < SPEnd[SPIndex])
                            {
                                SPGood = false;
                            }
                            myResults.missedNotes++;
                            //reff.Hurt(2);
                        }
                        else
                        {
                            
                            reff.Help(2);
                            myResults.hitNotes++;
                            int scre = 0;
                            for (int i = 0; i < 5; i++)
                                if ((notes[index].type & Game1.bits[i]) != 0)
                                    scre++;
                            multiplier = Math.Min(multiplier + (scre*0.1f), 4);
                            score += scre * (int)multiplier * 100;
                        }
                    }
                    index++;
                }
                if ((GetBoardType() == Game1.GUITARIST || GetBoardType() == Game1.BASSIST) && index < notes.Length && notes[index].burning)
                {
                    LinkedListNode<WaveVector2> temp = whammyage.First;
                    while (temp != null)
                    {
                        temp.Value.Y += gameTime.ElapsedGameTime.Milliseconds;
                        temp = temp.Next;
                    }
                    if (IsValidFrettage(notes[index].type, pressed))
                    {
                        reff.Burn(gameTime, notes[index].type, ind);
                        Burn(gameTime, notes[index].type);
                    }
                    else
                    {
                        notes[index].burning = false;
                        index++;
                    }
                }
                

                if (SPIndex < SPStart.Length && (long)currenttime > SPEnd[SPIndex])
                {
                    if (SPGood)
                    {
                        StarPowerAmount += 0.25f;
                        myResults.hitSPPH++;
                    }
                    SPGood = true;
                    SPIndex++;
                }

                
                /*if (StarPowerAmount >= 0.5)
                {
                    if (SPMFVel == 0)
                        SPMFVel = 4f;
                    SPMFlash += SPMFVel * (gameTime.ElapsedGameTime.Milliseconds / 1000f);
                    if (SPMFlash > 1)
                        SPMFVel = -4;
                    if (SPMFlash < 0)
                        SPMFVel = 4;
                }
                else
                    SPMFlash = 0;
                if (GetMultiplier() >= 2 && multSlide < 1)
                    multSlide += gameTime.ElapsedGameTime.Milliseconds / 1000f;
                if (GetMultiplier() < 2 && multSlide > 0)
                    multSlide -= gameTime.ElapsedGameTime.Milliseconds / 1000f;


                if (GetBoardType() == Game1.PERCUSSIONIST)
                    if (DFIndex < DFEnd.Length && (long)currenttime > DFEnd[DFIndex])
                        DFIndex++;
                lastPressed = pressed;

                if (multiplier > 6 && type == Game1.BASS)
                    multiplier = 6;
                else if (multiplier > 4 && type != Game1.BASS)
                    multiplier = 4;*/
            }
            else
            {

            }

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
            return 0;
        }

        public float GetBoardBump()//bass bump
        {
            if (GetBoardType() == Game1.PERCUSSIONIST)
            {
                return popup[4];
            }
            return 0;
        }

        public byte Strum(byte pressed, long currenttime, Game1 game, int ind)
        {
            if (notes[index].time - PILLOW < currenttime)
                notes[index].Strum();
            return 0;
            /*if (index >= notes.Length)
                return 0;
            if (Math.Abs(notes[index].time - (long)currenttime) < 100)
            {
                if (!(notes[index].burning && index + 1 < notes.Length && Math.Abs(notes[index + 1].time - (long)currenttime) < 100))
                {
                    if (IsValidFrettage(notes[index].type, pressed))
                    {
                        if (notes[index].visible[0] != NoteSet.VIS_STATE.HOPOED && !notes[index].burning)
                        {
                            int scre = 0;
                            for (int i = 0; i < 5; i++)
                                if ((notes[index].type & Game1.bits[i]) > 0)
                                {
                                    scre++;
                                    popupSpeed[i] += 100f;
                                }
                            scre *= 100 * (int)(multiplier);
                            score += scre;
                            multiplier += .1f;
                        }
                        notes[index].visible[0] = NoteSet.VIS_STATE.INVISIBLE;
                        if (notes[index].burning)
                        {
                            notes[index].burning = false;
                            game.Hurt(ind);
                            if (SPIndex < SPStart.Length && notes[index].time >= SPStart[SPIndex] && notes[index].time < SPEnd[SPIndex])
                                SPGood = false;
                            multiplier = 1;
                            return 0;
                        }
                        if (notes[index].length > 0)
                        {
                            notes[index].burning = true;
                            myResults.hitNotes++;
                            return notes[index].type;
                        }
                        //else
                        myResults.hitNotes++;
                        index++;
                        return notes[index - 1].type;
                    }
                }
                else
                {
                    index++;
                    if (IsValidFrettage(notes[index].type, pressed))
                    {
                        if (notes[index].visible[0] != NoteSet.VIS_STATE.HOPOED && !notes[index].burning)
                        {
                            int scre = 0;
                            for (int i = 0; i < 5; i++)
                                if ((notes[index].type & Game1.bits[i]) > 0)
                                {
                                    scre++;
                                    popupSpeed[i] += 100f;
                                }
                            scre *= 100 * (int)(multiplier);
                            score += scre;
                            multiplier += .1f;
                        }
                        notes[index].visible[0] = NoteSet.VIS_STATE.INVISIBLE;
                        if (notes[index].length > 0)
                        {
                            notes[index].burning = true;
                            myResults.hitNotes++;
                            return notes[index].type;
                        }
                        //else
                        myResults.hitNotes++;
                        index++;
                        return notes[index - 1].type;
                    }
                }
            }
            else if (notes[index].burning && index + 1 < notes.Length && Math.Abs(notes[index + 1].time - (long)currenttime) < 100)
            {
                index++;
                    if (IsValidFrettage(notes[index].type, pressed))
                    {
                        if (notes[index].visible[0] != NoteSet.VIS_STATE.HOPOED && !notes[index].burning)
                        {
                            int scre = 0;
                            for (int i = 0; i < 5; i++)
                                if ((notes[index].type & Game1.bits[i]) > 0)
                                {
                                    scre++;
                                    popupSpeed[i] += 100f;
                                }
                            scre *= 100 * (int)(multiplier);
                            score += scre;
                            multiplier += .1f;
                        }
                        notes[index].visible[0] = NoteSet.VIS_STATE.INVISIBLE;
                        if (notes[index].length > 0)
                        {
                            notes[index].burning = true;
                            myResults.hitNotes++;
                            return notes[index].type;
                        }
                        //else
                        myResults.hitNotes++;
                        index++;
                        return notes[index - 1].type;
                    }
            }
            else if (currenttime > (long)notes[index].time && currenttime < ((long)notes[index].time + (long)notes[index].length))
            {
                notes[index].burning = false; multiplier = 1; game.Hurt(ind);
                if (SPIndex < SPStart.Length && notes[index].time >= SPStart[SPIndex] && notes[index].time < SPEnd[SPIndex])
                    SPGood = false; return 0;
            }
            
            if (SPIndex < SPStart.Length && notes[index].time >= SPStart[SPIndex] && notes[index].time < SPEnd[SPIndex])
                            SPGood = false;
            game.Hurt(ind);
            multiplier = 1;
            return 0;*/
        }

        public byte Bang(byte pressed, long currenttime, Game1 game)
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
            if (index >= notes.Length)
                return 0;
            if (StarPowerAmount>=0.5 && !SPActivated && DFIndex<DFEnd.Length && currenttime >= (long)DFStart[DFIndex] && currenttime <= (long)DFEnd[DFIndex])
            {
                if ((newPressed & Game1.bits[0]) != 0)
                    if (Math.Abs((DFEnd[DFIndex] - 100) - (long)currenttime) < 100)
                    { SPActivated = true; DFHitGreen[DFIndex] = true; }
                float numperfill = 4f * (DFEnd[DFIndex] - DFStart[DFIndex]) / 1000f;
                int numhit = 0;
                for (int i = 0; i < 5; i++)
                    if ((newPressed & Game1.bits[i]) != 0)
                        numhit++;
                dfA[DFIndex] += numhit / numperfill;
                if (dfA[DFIndex] > 1)
                    dfA[DFIndex] = 1f;
                return (byte)(newPressed | Game1.bits[7]);
            }
            else if ((notes[index].time - (long)currenttime) < 100)
            {
                byte ret = 0;
                for (int i = 0; i < 5; i++)
                    if ((notes[index].type & Game1.bits[i]) != 0 && (newPressed & Game1.bits[i]) != 0 && notes[index].visible[i]==0)
                    {
                        ret |= Game1.bits[i];
                        notes[index].visible[i] = NoteSet.VIS_STATE.INVISIBLE;
                        popupSpeed[drumsToGuitar[i]] += 100f;
                    }
                    else if ((notes[index].type & Game1.bits[i]) == 0 && (newPressed & Game1.bits[i]) != 0)
                    {
                        notes[index].visible[i] = NoteSet.VIS_STATE.OVERDONE;
                    }
                return ret;
            }
            game.Hurt(2);
            multiplier = 1;
            return 0;*/
        }

        public float GetStars()
        {
            if (GetBoardType() == Game1.VOCALIST)
                return 5f;
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
            if (multiplier >= 4 && GetBoardType() != Game1.BASSIST)
                return 10;
            else if (multiplier >= 6)
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

        public bool IsLefty()
        {
            return LeftySwitch;
        }

        public bool getWaves(long currenttime)
        {
            int count = 0;
            for (int i = index; i<notes.Length && notes[i].time < currenttime + (eFade*1000) && notes[i].time+notes[i].length>(long)currenttime; i++)
            {
                if (notes[i].length > 0)
                    count++;
            }
            if (wavesLen < count)
            { waves = new WaveNode[count][]; wavesSubLen = new int[count]; }
            wavesLen = count;
            if (count == 0)
                return false;
            count = 0;
            for (int i = index; i < notes.Length && notes[i].time < currenttime + (eFade*1000) && notes[i].time+notes[i].length>(long)currenttime; i++)
            {
                if (notes[i].length > 0)
                {
                    bool white = false;
                    if (SPIndex < SPStart.Length && SPGood && i >= SPStart[SPIndex] && i <= SPEnd[SPIndex])
                        white = true;
                    if (notes[i].burning)
                    {
                        
                        int num = (int)((Math.Min(notes[i].length, eFade * 1000f - (notes[i].time - (long)currenttime)) - ((notes[i].time < (long)currenttime) ? (long)currenttime - notes[i].time : 0) - WaveDetail) / WaveDetail);
                        num += 2;
                        if (wavesSubLen[count] < num)
                        { waves[count] = new WaveNode[num];  }
                        wavesSubLen[count] = num;
                        waves[count][0].X = 0f;
                        waves[count][0].Y = (notes[i].time < (long)currenttime) ? 0 : (notes[i].time - (long)currenttime);
                        waves[count][0].Z = (byte)(notes[i].type|(notes[i].burning||notes[i].time>(long)currenttime?0:128)|64);
                        waves[count][0].White = white;
                        float varyPower;
                        for (int n = 0; n < num - 1; n++)
                        {
                            waves[count][n].White = white;
                            waves[count][n].Y = ((notes[i].time < (long)currenttime) ? 0 : (notes[i].time - (long)currenttime)) + (WaveDetail * n);
                            varyPower = GetWhammy(waves[count][n].Y,currenttime);
                            if(varyPower<0.1)
                                waves[count][n].X = 0.2f-((float)(Math.Sin(waveoffset + (waves[count][n].Y / 100f))+1)*0.1f*(varyPower*10));
                            else
                                waves[count][n].X = (float)Math.Sin(waveoffset + (waves[count][n].Y / 100f)) * (varyPower);
                            waves[count][n].Z = (byte)(notes[i].type|(notes[i].burning||notes[i].time>(long)currenttime?0:128)|64);
                        }
                        waves[count][num-1].White = white;
                        waves[count][num - 1].Y = Math.Min(((notes[i].time + notes[i].length) - (long)currenttime), ((eFade * 1000f)));
                        //waves[count][num - 1].X = (float)Math.Sin(waveoffset + (waves[count][num - 1].Y / 100f));
                        waves[count][num - 1].X = (float)Math.Sign(waves[count][num - 1].X) * 0.1f;
                        waves[count][num - 1].Z = (byte)(notes[i].type|(notes[i].burning||notes[i].time>(long)currenttime?0:128)|64);
                        count++;
                    }
                    else
                    {
                        if (wavesSubLen[count] < 2)
                        { waves[count] = new WaveNode[2]; }
                         wavesSubLen[count] = 2;
                        waves[count][0].White = white;
                        waves[count][0].X = 0.2f;
                        waves[count][0].Y = (notes[i].time < (long)currenttime) ? 0 : (notes[i].time - (long)currenttime);
                        waves[count][0].Z = (byte)(notes[i].type|(notes[i].burning||notes[i].time>(long)currenttime-100?0:128)|(notes[i].time >= (long)currenttime?64:0));

                        waves[count][1].White = white;
                        waves[count][1].Y = Math.Min(((notes[i].time + notes[i].length) - (long)currenttime), (eFade * 1000f));
                        waves[count][1].X = 0.2f;
                        waves[count][1].Z = (byte)(notes[i].type|(notes[i].burning||notes[i].time>(long)currenttime-100?0:128)|(notes[i].time >= (long)currenttime?64:0));
                        count++;
                    }
                }
            }
            return true;
        }

        public float[] GetPopups()
        {
            return popup;
        }

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
            if (index >= notes.Length)
                return;
            if (notes[index].burning && notes[index].time < (long)currenttime && notes[index].time + notes[index].length > (long)currenttime)
            {
                whammyage.AddFirst(new WaveVector2(p, 0));
                int end = (int)Math.Min(eFade * 1000, (notes[index].time + notes[index].length) - (long)currenttime);
                while (whammyage.Last.Value.Y >= end)
                    whammyage.RemoveLast();
                score += (int)(50 * ((gametime.ElapsedGameTime.TotalSeconds*1000) / beatLength));
                if(SPIndex<SPStart.Length && index>=SPStart[SPIndex] && index<=SPEnd[SPIndex])
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
                int end = (int)Math.Min(eFade * 1000, (notes[index].time + notes[index].length) - (long)currenttime);
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

        public void Burn(GameTime gt, byte note)
        {
            int num = Game1.AddBits((byte)(note & 31));
            float dt = gt.ElapsedGameTime.Milliseconds / 1000f;
            score += (int)(num * 100 * dt);
        }

        public Results getResults()
        {
            return myResults;
        }

        public void ActivateStarPower()
        {
            if (StarPowerAmount>=0.5)
                SPActivated = true;
        }

        public void Draw(SpriteBatch spritebatch, long currenttime)
        {
            Vector2 center = new Vector2(vFuzz.Width/2,vFuzz.Height/2);
            float vScale=0.2f;
            float height = vFuzz.Height*vScale;
            Color glow = new Color(150, 255, 150, 255);
            spritebatch.Draw(vBGInt, new Rectangle(0, (int)vocaly, 1024, (int)vocalheight), Color.White);
            for (int i = index; i < vNotes.Length; i++)
            {
                spritebatch.Draw(vBar, new Rectangle((int)(vocalzerox+(vocalwidth*(vNotes[i].time-(long)currenttime))), (int)vocaly, 8, (int)vocalheight), Color.White);
                
                for (int k = 0; k < vNotes[i].words.Length; k++)
                {
                    if (vNotes[i].words[k].note < 0)
                        continue;
                    float minx=(vocalzerox+(vocalwidth*(vNotes[i].words[k].time-currenttime))), maxx=(vocalzerox+(vocalwidth*((vNotes[i].words[k].len)-currenttime))), y=(1-((vNotes[i].words[k].note%12)/12f))*vocalheight*.69f+vocaly;
                    spritebatch.Draw(vGlow, new Rectangle((int)minx, (int)(y - height/2), (int)Math.Min(128 * vScale, (maxx - minx) * vScale), (int)(height)), new Rectangle(0, 0, 128, 256), glow);
                    spritebatch.Draw(vGlow, new Rectangle((int)(minx+Math.Min(128 * vScale, (maxx - minx) * vScale)), (int)(y - height/2), (int)((maxx-minx)-(Math.Min(128 * vScale, (maxx - minx) * vScale)*2)), (int)(height)), new Rectangle(128, 0, 128, 256), glow);
                    spritebatch.Draw(vGlow, new Rectangle((int)(maxx-Math.Min(128 * vScale, (maxx - minx) * vScale)), (int)(y - height/2), (int)Math.Min(128 * vScale, (maxx - minx) * vScale), (int)(height)), new Rectangle(128, 0, -128, 256), glow);
                }
            }

            spritebatch.Draw(vBGExt, new Rectangle(0, (int)vocaly, 1024, (int)vocalheight), Color.White);
            
            for (int i = index; i < vNotes.Length; i++)
            for (int k = 0; k < vNotes[i].words.Length; k++)
                    spritebatch.DrawString(Game1.DefaultFont, vNotes[i].words[k].value, new Vector2((vocalzerox + (vocalwidth * (vNotes[i].words[k].time - currenttime))), vocaly + (0.75f * vocalheight)), Color.White);
            spritebatch.Draw(vHeadBar, new Rectangle((int)vocalzerox, (int)vocaly, 8, (int)vocalheight), Color.White);
        }

        internal void EatHalfSP()
        {
            StarPowerAmount -= 0.5f;
        }

        
    }
}

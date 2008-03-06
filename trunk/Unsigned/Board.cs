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
        public int time;
        public byte type;
        public int length;
        public VIS_STATE[] visible;//0=visible,1=greyedout,2=invisible,3=invisibleButAvailable(HOPO)
        public bool burning;//for held notes

        public enum VIS_STATE { VISIBLE = 0, GREYED_OUT = 1, INVISIBLE = 2, HOPOED = 3, };

        public int CompareTo(object other)
        {
            return time.CompareTo(((NoteSet)other).time);
        }
    }

    struct Results
    {
        public int hitNotes, missedNotes;
        public int hitSPPH, missedSPPH;
    }

    class Board
    {
        private int type;
        public int xOffset;

        public static float width, length, curveHeight, height, rotate, zeroZ, sFade, eFade, spShift;
        public float yRotate=0;
        public static Texture2D[][] boardTexPlain;
        public Texture2D SPMeterTex, SPMRTex;
        public float multSlide=0;
        public static Texture2D SPMBorder, SPMFill, SPMFlashTex, drumfillTex, SPBoardTex;
        public static Texture2D SPMRbg, SPMRslice, SPMRfg, SPMRbgb, SPMRbgs;
        public static Texture2D[] SPMRnum;
        public float SPMFlash=0, SPMFVel=0;
        public static int[] boardValidBPM = { 0, 1, 2, 3, 4, 6, 8, };
        public static int[] boardBeatsIndex = { 0, 1, 2, 3, 4, -1, 5, -1, 6, };
        public static GBVertexFormat[] arrBoard;
        public static VertexBuffer mdlBoard, mdlSPM;
        public static VertexBuffer mdlTrigger, mdlTriggerBorder;
        public static Model mdlNote;
        public static Texture2D[] texNotes, texTriggers, texTriggersLit;
        public static Texture2D texTriggerBorder, texTriggerBorderLit;
        public static float BOARD_BUMP_COEF = 0.002f;
        private Song song;
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
        public static int[] guitarToDrums = { 1, 2, 3, 0, -1 };
        public static int[] drumsToGuitar = { 3, 0, 1, 2, 4 };
        public LinkedList<Vector2> whammyage;
        float waveoffset=0;
        Results myResults;

        public WaveNode[][] waves;
        public int wavesLen;
        public int[] wavesSubLen;

        private NoteSet[] notes;
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
                whammyage = new LinkedList<Vector2>();
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

        public Vector4[] GetNotes(long currenttime, long viewdistance)
        {
            currenttime /= (long)Game1.TicksPerSecond / 1000;
            viewdistance /= (long)Game1.TicksPerSecond / 1000;
            int k;
            for (k = 0; k < notes.Length; k++)
                if (notes[k].time > currenttime - (viewdistance / 4))
                    break;
            int count = 0;
            bool fill = false;
            for (int i = k; i<notes.Length && notes[i].time < currenttime + viewdistance; i++)
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

                    if (StarPowerAmount >= 0.5 && DFIndex<DFStart.Length && notes[i].time >= DFStart[DFIndex] && notes[i].time <= DFEnd[DFIndex])
                        fill=true;
                    else if (StarPowerAmount >= 0.5 && DFIndex+1<DFStart.Length && notes[i].time >= DFStart[DFIndex+1] && notes[i].time <= DFEnd[DFIndex+1])
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

            Vector4[] ret = new Vector4[count];
            int indx = 0;
            int plus = 0;
            for (int i = k; i < notes.Length && notes[i].time < currenttime + viewdistance; i++)
            {
                float sp = 0f;
                if (SPIndex<SPStart.Length && notes[i].time >= SPStart[SPIndex] && notes[i].time < SPEnd[SPIndex] && SPGood)
                    sp = 1f;

                if (GetBoardType() != Game1.PERCUSSIONIST)
                {
                    if (notes[i].visible[0] == NoteSet.VIS_STATE.INVISIBLE || notes[i].visible[0] == NoteSet.VIS_STATE.HOPOED)
                        continue;
                    if ((notes[i].type & NS_GREEN) != 0)
                    {
                            ret[indx] = new Vector4(-1f, notes[i].time, ((notes[i].type & NS_HOPO) != 0) ? 1f : -1f, sp);
                        indx++;
                    }
                    if ((notes[i].type & NS_RED) != 0)
                    {
                        ret[indx] = new Vector4(-0.5f, notes[i].time, ((notes[i].type & NS_HOPO) != 0) ? 1f : -1f, sp);
                        indx++;
                    }
                    if ((notes[i].type & NS_YELLOW) != 0)
                    {
                        ret[indx] = new Vector4(0f, notes[i].time, ((notes[i].type & NS_HOPO) != 0) ? 1f : -1f, sp);
                        indx++;
                    }
                    if ((notes[i].type & NS_BLUE) != 0)
                    {
                        ret[indx] = new Vector4(0.5f, notes[i].time, ((notes[i].type & NS_HOPO) != 0) ? 1f : -1f, sp);
                        indx++;
                    }
                    if ((notes[i].type & NS_ORANGE) != 0)
                    {
                        ret[indx] = new Vector4(1f, notes[i].time, ((notes[i].type & NS_HOPO) != 0) ? 1f : -1f, sp);
                        indx++;
                    }
                }
                else
                {
                    if (StarPowerAmount >= 0.5 && DFIndex < DFStart.Length && notes[i].time >= DFStart[DFIndex] && notes[i].time <= DFEnd[DFIndex])
                        fill = true;
                    else if (StarPowerAmount >= 0.5 && DFIndex + 1 < DFStart.Length && notes[i].time >= DFStart[DFIndex+1] && notes[i].time <= DFEnd[DFIndex+1])
                        fill = true;
                    else
                    {
                        if (fill)
                        {
                            ret[indx] = new Vector4(dfA[DFIndex + plus], DFStart[DFIndex + plus], 1, (DFHitGreen[DFIndex + plus] ? 1 : 0));
                            ret[indx + 1] = new Vector4(dfA[DFIndex + plus], DFEnd[DFIndex + plus], 2, 0);
                            indx+=2;
                            fill = false;
                            plus++;
                        }
                        if ((notes[i].type & NS_GREEN) != 0 && notes[i].visible[0] == 0)
                        {
                            ret[indx] = new Vector4(1f, notes[i].time, 0, sp);
                            indx++;
                        }
                        if ((notes[i].type & NS_RED) != 0 && notes[i].visible[1] == 0)
                        {
                            ret[indx] = new Vector4(-1f, notes[i].time, 0, sp);
                            indx++;
                        }
                        if ((notes[i].type & NS_YELLOW) != 0 && notes[i].visible[2] == 0)
                        {
                            ret[indx] = new Vector4(-1 / 3f, notes[i].time, 0, sp);
                            indx++;
                        }
                        if ((notes[i].type & NS_BLUE) != 0 && notes[i].visible[3] == 0)
                        {
                            ret[indx] = new Vector4(1 / 3f, notes[i].time, 0, sp);
                            indx++;
                        }
                        if ((notes[i].type & NS_ORANGE) != 0 && notes[i].visible[4] == 0)
                        {
                            ret[indx] = new Vector4(0f, notes[i].time, 0, sp);
                            indx++;
                        }
                    }
                }
            }
            if (fill)
            {
                ret[indx] = new Vector4(dfA[DFIndex+plus], DFStart[DFIndex + plus], 1, 0);
                ret[indx + 1] = new Vector4(dfA[DFIndex + plus], DFEnd[DFIndex + plus], 2, 0);
                indx += 2;
                fill = false;
                plus++;
            }
            for (int i = 0; i < ret.Length; i++)
                ret[i].Y = (ret[i].Y - currenttime) / 1000f;
            return ret;
        }

        private void LoadNotes(String filename, byte diff, Song song)
        {
            Vector2[] bars = song.GetAllBars();
            System.IO.BinaryReader reader = new System.IO.BinaryReader(System.IO.File.OpenRead("songdata\\" + filename));

            byte version = reader.ReadByte();

            int numSPP = reader.ReadInt32();
            SPStart = new int[numSPP];
            SPEnd = new int[numSPP];
            for (int i = 0; i < numSPP; i++)
            {
                SPStart[i] = reader.ReadInt32();
                SPEnd[i] = reader.ReadInt32() + SPStart[i];
            }

            if (GetBoardType()==Game1.PERCUSSIONIST)
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
            for(int dfrep = 3; dfrep>=0; dfrep--)
            {
                bool skip = true;
                int difr = reader.ReadInt32();
                if (diff == Game1.D_EXPERT && difr==3)
                    skip = false;
                if (diff == Game1.D_MEDIUM && difr==2)
                    skip = false;
                if (diff == Game1.D_EASY && difr==1)
                    skip = false;
                if (diff == Game1.D_HARD && difr==0)
                    skip = false;

                if (skip)
                {
                    int notesn = reader.ReadInt32();
                    for (int i = 0; i < notesn; i++)
                    {
                        reader.ReadByte();
                        if (difr!=Game1.DRUMS)
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
                    for (int i = 0; i < notes.Length; i++)
                    {
                        notes[i] = new NoteSet();
                        notes[i].type = reader.ReadByte();
                        if (GetBoardType()!=Game1.DRUMS)
                        {
                            notes[i].time = reader.ReadInt32();
                            notes[i].length = reader.ReadInt32();
                            notes[i].visible = new NoteSet.VIS_STATE[1];
                        }
                        else
                        {
                            notes[i].visible = new NoteSet.VIS_STATE[5];
                            notes[i].time = reader.ReadInt32();
                            notes[i].length = 0;
                        }
                    }
                    starPts = new int[6];
                    starPts[0] = 0;
                    for (int i = 1; i <= 5; i++)
                        starPts[i] = reader.ReadInt32();
                    int numrep = 0;
                    int curtm = 0;
                    for (int i = 0; i < notes.Length; i++)
                    {
                        if (notes[i].time < curtm)
                            numrep++;
                        curtm = notes[i].time;
                    }
                    Console.WriteLine("Numerrors(old):" + numrep);
                    Array.Sort<NoteSet>(notes);
                    numrep = 0;
                    curtm = 0;
                    for (int i = 0; i < notes.Length; i++)
                    {
                        if (notes[i].time < curtm)
                            numrep++;
                        curtm = notes[i].time;
                    }
                    Console.WriteLine("Numerrors(new):" + numrep);
                    break;
                }
            }
        }

        public bool IsValidFrettage(byte note, byte pressed)
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

        public void Update(GameTime gameTime, long currenttime,Game1 reff, int ind, byte pressed)
        {
            waveoffset -= gameTime.ElapsedGameTime.Milliseconds / 100f;

            currenttime/=(long)Game1.TicksPerSecond/1000;
            if (GetBoardType()!=Game1.PERCUSSIONIST)
            if (index<notes.Length)
            if (multiplier>1)
            if ((notes[index].type&NS_HOPO)!=0)
            if (Math.Abs(notes[index].time - currenttime) < 100)
            if (notes[index].visible[0] != NoteSet.VIS_STATE.HOPOED)
            if (IsValidFrettage(notes[index].type, pressed))
            {
                int scre = Game1.AddBits(notes[index].type);
                scre -= 1;
                scre *= (int)(100 * multiplier);
                score += scre;
                multiplier += 0.1f;
                notes[index].visible[0] = NoteSet.VIS_STATE.HOPOED;
                reff.AddShards(notes[index].type, ind);
                if (notes[index].length > 0)
                    notes[index].burning = true;
            }
            while (index < notes.Length - 1 && Math.Abs((notes[index].time+notes[index].length) - currenttime) > Math.Abs((notes[index + 1].time) - currenttime))
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
                    if (shouldhurt)
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
                    { multiplier = Math.Min(multiplier+0.1f,4); reff.Help(2); myResults.hitNotes++; }
                }
                index++;
            }
            if (index < notes.Length && (notes[index].time+notes[index].length) - currenttime < -100)
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
                    if (shouldhurt)
                    {
                        reff.Hurt(ind);
                        multiplier = 1;
                        if (SPIndex<SPStart.Length && notes[index].time >= SPStart[SPIndex] && notes[index].time<SPEnd[SPIndex])
                        {
                            SPGood = false;
                            
                        }
                        myResults.missedNotes++;
                        //reff.Hurt(2);
                    }
                    else
                    { multiplier = Math.Min(multiplier+0.1f,4); reff.Help(2); myResults.hitNotes++; }
                }
                index++;
            }
            if ((GetBoardType()==Game1.GUITARIST || GetBoardType()==Game1.BASSIST) && index<notes.Length && notes[index].burning)
            {
                LinkedListNode<Vector2> temp = whammyage.First;
                while (temp!=null)
                {
                    temp.Value = new Vector2(temp.Value.X,temp.Value.Y + gameTime.ElapsedGameTime.Milliseconds);
                    temp = temp.Next;
                }
                if (IsValidFrettage(notes[index].type, pressed))
                {
                    reff.Burn(gameTime,notes[index].type,ind);
                    Burn(gameTime, notes[index].type);
                }
                else
                {
                    notes[index].burning = false;
                    index++;
                }
            }
            for (int i = 0; i < 5; i++)
            {
                if (popup[i] > 20 && popupSpeed[i]>0)
                    popupSpeed[i] = 0;
                if (popup[i] <= 0 && popupSpeed[i]<0)
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

            if (SPIndex < SPStart.Length && currenttime > SPEnd[SPIndex])
            {
                if (SPGood)
                {
                    StarPowerAmount += 0.25f;
                }
                SPGood = true;
                SPIndex++;
            }

            if (Math.Abs(StarPowerAmount - SPADisplay) > 0.0001f)
            {
                if (StarPowerAmount > SPADisplay)
                    SPADisplay += gameTime.ElapsedGameTime.Milliseconds / 5000f;
                else if (StarPowerAmount < SPADisplay)
                    SPADisplay -= gameTime.ElapsedGameTime.Milliseconds / 5000f;
            }
            if (StarPowerAmount >= 0.5)
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

            if (SPActivated)
            {
                StarPowerAmount -= gameTime.ElapsedGameTime.Milliseconds / 20000f;
                if (StarPowerAmount < 0)
                {
                    SPActivated = false;
                    StarPowerAmount = 0;
                }
            }
            if (GetBoardType() == Game1.PERCUSSIONIST)
            if (DFIndex<DFEnd.Length && currenttime > DFEnd[DFIndex])
                DFIndex++;
            lastPressed = pressed;

            if (multiplier > 6 && type == Game1.BASS)
                multiplier = 6;
            else if (multiplier > 4 && type != Game1.BASS)
                multiplier = 4;
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
            currenttime/=(long)Game1.TicksPerSecond/1000;
            if (index >= notes.Length)
                return 0;
            if (Math.Abs(notes[index].time - currenttime) < 100)
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
            else if (currenttime > notes[index].time && currenttime < notes[index].time+notes[index].length)
            {
                notes[index].burning = false; multiplier = 1; game.Hurt(ind);
            if (SPIndex < SPStart.Length && notes[index].time >= SPStart[SPIndex] && notes[index].time < SPEnd[SPIndex])
                            SPGood = false; return 0; }
            
            if (SPIndex < SPStart.Length && notes[index].time >= SPStart[SPIndex] && notes[index].time < SPEnd[SPIndex])
                            SPGood = false;
            game.Hurt(ind);
            multiplier = 1;
            return 0;
        }

        public byte Bang(byte pressed, long currenttime, Game1 game)
        {
            byte newPressed = 0;
            for (int i = 0; i < 5; i++)
            {
                if ((lastPressed & Game1.bits[i]) == 0 && (pressed & Game1.bits[i]) != 0)
                    newPressed |= Game1.bits[i];
            }
            if (newPressed == 0)
                return 0;
            currenttime /= (long)Game1.TicksPerSecond / 1000;
            if (index >= notes.Length)
                return 0;
            if (StarPowerAmount>0.5 && DFIndex<DFEnd.Length && currenttime >= DFStart[DFIndex] && currenttime <= DFEnd[DFIndex])
            {
                if ((newPressed & Game1.bits[0]) != 0)
                    if (Math.Abs((DFEnd[DFIndex] - 100) - currenttime) < 100)
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
            else if ((notes[index].time - currenttime) < 100)
            {
                
                int scre = 0;
                byte ret = 0;
                for (int i = 0; i < 5; i++)
                    if ((notes[index].type & Game1.bits[i]) != 0 && (newPressed & Game1.bits[i]) != 0 && notes[index].visible[i]==0)
                    {
                        ret |= Game1.bits[i];
                        scre++;
                        notes[index].visible[i] = NoteSet.VIS_STATE.INVISIBLE;
                        popupSpeed[drumsToGuitar[i]] += 100f;
                    }
                scre *= 100 * (int)(multiplier);
                score += scre;
                return ret;
            }
            game.Hurt(2);
            multiplier = 1;
            return 0;
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
            if (multiplier >= 4 && GetBoardType() != Game1.BASSIST)
                return 10;
            else if (multiplier >= 6)
                return 10;
            return (int)((multiplier % 1) * 10);
        }

        public int GetMultiplier()
        {
            return (int)multiplier*(SPActivated?2:1);
        }

        public bool IsLefty()
        {
            return LeftySwitch;
        }

        public bool getWaves(long currenttime, long viewdistance)
        {
            currenttime/=(long)(Game1.TicksPerSecond/1000);
            viewdistance/=(long)(Game1.TicksPerSecond/1000);
            int count = 0;
            for (int i = index; i<notes.Length && notes[i].time < currenttime + (eFade*1000) && notes[i].time+notes[i].length>currenttime; i++)
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
            for (int i = index; i < notes.Length && notes[i].time < currenttime + (eFade*1000) && notes[i].time+notes[i].length>currenttime; i++)
            {
                if (notes[i].length > 0)
                {
                    if (notes[i].burning)
                    {
                        
                        int num = (int)((Math.Min(notes[i].length, eFade * 1000f - (notes[i].time - currenttime)) - ((notes[i].time < currenttime) ? currenttime - notes[i].time : 0) - 50f) / 50f);
                        num += 2;
                        if (wavesSubLen[count] < num)
                        { waves[count] = new WaveNode[num];  }
                        wavesSubLen[count] = num;
                        waves[count][0].X = 0f;
                        waves[count][0].Y = (notes[i].time < currenttime) ? 0 : (notes[i].time - currenttime);
                        waves[count][0].Z = (byte)(notes[i].type|(notes[i].burning||notes[i].time>currenttime?0:128));
                        float varyPower;
                        for (int n = 0; n < num - 1; n++)
                        {
                            waves[count][n].Y = ((notes[i].time < currenttime) ? 0 : (notes[i].time - currenttime)) + (50f * n);
                            varyPower = GetWhammy(waves[count][n].Y,currenttime);
                            if(varyPower<0.1)
                                waves[count][n].X = 0.2f-((float)(Math.Sin(waveoffset + (waves[count][n].Y / 100f))+1)*0.1f*(varyPower*10));
                            else
                                waves[count][n].X = (float)Math.Sin(waveoffset + (waves[count][n].Y / 100f)) * (varyPower);
                            waves[count][n].Z = (byte)(notes[i].type|(notes[i].burning||notes[i].time>currenttime?0:128));
                        }

                        waves[count][num - 1].Y = Math.Min(((notes[i].time + notes[i].length) - currenttime), ((eFade * 1000f)));
                        waves[count][num - 1].X = (float)Math.Sin(waveoffset + (waves[count][num - 1].Y / 100f));
                        waves[count][num - 1].X = (float)Math.Sign(waves[count][num - 1].X) * 0.1f;
                        waves[count][num - 1].Z = (byte)(notes[i].type|(notes[i].burning||notes[i].time>currenttime?0:128));
                        count++;
                    }
                    else
                    {
                        if (wavesSubLen[count] < 2)
                        { waves[count] = new WaveNode[2]; }
                         wavesSubLen[count] = 2;
                        waves[count][0].X = 0.2f;
                        waves[count][0].Y = (notes[i].time < currenttime) ? 0 : (notes[i].time - currenttime);
                        waves[count][0].Z = (byte)(notes[i].type|(notes[i].burning||notes[i].time>currenttime-100?0:128));

                        waves[count][1].Y = Math.Min(((notes[i].time + notes[i].length) - currenttime), ((eFade * 1000f)));
                        waves[count][1].X = 0.2f;
                        waves[count][1].Z = (byte)(notes[i].type|(notes[i].burning||notes[i].time>currenttime-100?0:128));
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

        public void Whammy(float p,long currenttime)
        {
            p = (p + 1) / 2f;
            if (index >= notes.Length)
                return;
            if (notes[index].burning && notes[index].time < currenttime && notes[index].time + notes[index].length > currenttime)
            {
                whammyage.AddFirst(new Vector2(p, 0));
                int end = (int)Math.Min(eFade * 1000, (notes[index].time + notes[index].length) - currenttime);
                while (whammyage.Last.Value.Y >= end)
                    whammyage.RemoveLast();
            }
            else if (whammyage.Count > 0)
                whammyage.Clear();
        }

        private float GetWhammy(float y,long currenttime)
        {
            if (whammyage.Count < 2)
                return 0f;
            LinkedListNode<Vector2> temp = whammyage.First;
            while (temp!=null && temp.Value.Y < y)
            {
                temp = temp.Next;
            }
            if (temp == null)
            {
                int end = (int)Math.Min(eFade * 1000, (notes[index].time + notes[index].length) - currenttime);
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
    }
}

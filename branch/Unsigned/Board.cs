using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using UnsignedPeripheralPlugins;
using SongDataIO;

namespace Unsigned
{

    class Wave
    {
        public float Value;
        public float Time;

        public Wave(float val, long time)
        {
            Value = val;
            Time = time;
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
        // scaling values
        private static float vocalheight, vocaly, vocalzerox, vocalwidth;
        private static float width, length, curveHeight, height, rotate, zeroZ, sFade, eFade, spShift;
        public static float Width { get { return width; } }
        public static float Length { get { return length; } }
        public static float Height { get { return height; } }
        public static float Rotate { get { return rotate; } }
        public static float ZeroZOffset { get { return zeroZ; } }

        // textures, models
        private static Texture2D drumfillTex, spMeterBG, spMeterLED, spMeterFill, spMeterCurl;
        private static Texture2D vBar, vBGExt, vBGInt, vFuzz, vHeadBar, vGlow, texBlast;
        private static VertexBuffer mdlBoard, mdlSPM;
        private static VertexBuffer triggerVB, noteVB, noteInsideVB, mdlTriggerBorder;
        private static IndexBuffer triggerIB, noteIB, noteInsideIB;
        private static Texture2D[] texNotes, texBarNotes, texTriggers, texTriggersLit;
        private static Texture2D texTriggerBorder, texTriggerBorderLit;
        private static Texture2D texLine, texLineEnd;
        private static Texture2D texGlow;
        private static Texture2D[] texMult;

        // texture for board paths
        private static String[] boardBGs;

        // const values
        private const float BOARD_FADE_RATIO = 7 / 8f;
        private static float BOARD_BUMP_COEF = 0.002f;
        private static float RP_METER_Y_SCALE = 0.4f;
        private static uint PILLOW = 100;//padding in front of and behind note

        // sp circles (yes, they are actually static!)
        private static SPCircle[] spcircles = new SPCircle[50];

        // generic info
        private Instrument type;
        private Peripheral controller;
        private Results myResults;

        // gfx
        // what is used to draw onscreen
        private RenderTarget2D rtBoard, rtWaves;
        // the current background
        private Texture2D boardBackground;
        // the final render
        private RenderTarget2D boardTarget;

        // look & feel info (gui)
        private int xOffset;
        // the rotate of the flashes so they dont get stale
        private float flashRot;
        // which difficulty is being played
        private byte difficulty;
        // the multiplier before RP x2
        private float multiplier;
        // a float for more accurate wave addition
        private float score;
        // for fret boards popping up on strum/bang
        private float[] popup, popupSpeed;
        // StarPowerAmount is the actual value, SPADisplay follows for "filling up" and "draining"
        private float StarPowerAmount = 0, SPADisplay = 0;
        // which phrase is next/current
        private int RPIndex, FillIndex;
        // is false when player misses a note in the current RP phrase
        private bool RPGood = true;
        // pretty self explanatory...
        private bool SPActivated = false;
        // the values relative to currenttime
        private List<Wave> whammyValues;
        // this is just pushed along so the sin waves moves
        private double sinwavestart=0;
        // level 0-100
        private float rockMeterLevel;
        // if its 3, they can't be saved
        private byte numFails;
        // if true, activates rock power after current fill
        private bool SPDelayStart;
        // the next note
        private int currentNoteIndex;

        //public const byte NS_GREEN = 1, NS_RED = 2, NS_YELLOW = 4, NS_BLUE = 8, NS_ORANGE = 16, NS_HOPO = 32;

        //Legacy
        private static string[] SETTINGS_EXT = { ".gbg", ".gbv", ".gbd", ".gbb", }; public static int OFFSET_TO_GBA = 0, OFFSET_TO_GBG = 1, OFFSET_TO_GBB = 2, OFFSET_TO_GBD = 3, OFFSET_TO_GBV = 4, OFFSET_TO_GBE = 5;

        private SongData.NoteSet[] _notes;
        private SongData.NoteSet[] Notes
        {
            get
            {
                if (_notes == null)
                {
                    for (int i = 0; i < RhythmMaster.GetSingleton().GetSongData().instruments.Length; i++)
                        if (RhythmMaster.GetSingleton().GetSongData().instruments[i].instrumentType.Equals(GetBoardType().CodeName))
                        {
                            _notes = RhythmMaster.GetSingleton().GetSongData().instruments[i].diffSets[difficulty].phrases[0].notes;
                            return _notes;
                        }
                    return null;
                }
                return _notes;
            }
        }

        private SongData.Phrase[] _phrases;
        private SongData.Phrase[] Phrases
        {
            get
            {
                if (_phrases == null)
                {
                    for (int i = 0; i < RhythmMaster.GetSingleton().GetSongData().instruments.Length; i++)
                        if (RhythmMaster.GetSingleton().GetSongData().instruments[i].instrumentType.Equals(GetBoardType().CodeName))
                        {
                            _phrases = RhythmMaster.GetSingleton().GetSongData().instruments[i].diffSets[difficulty].phrases;
                            return _phrases;
                        }
                    return null;
                }
                return _phrases;
            }
        }

        private SongData.Fill[] _fills;
        private SongData.Fill[] Fills
        {
            get
            {
                if (_fills == null)
                {
                    for (int i = 0; i < RhythmMaster.GetSingleton().GetSongData().instruments.Length; i++)
                        if (RhythmMaster.GetSingleton().GetSongData().instruments[i].instrumentType.Equals(GetBoardType().CodeName))
                        {
                            _fills = RhythmMaster.GetSingleton().GetSongData().instruments[i].fills;
                            return _fills;
                        }
                    return null;
                }
                return _fills;
            }
        }

        private SongData.RockPowerPhrase[] _rpphrases;
        private SongData.RockPowerPhrase[] RPPhrases
        {
            get
            {
                if (_rpphrases == null)
                {
                    for (int i = 0; i < RhythmMaster.GetSingleton().GetSongData().instruments.Length; i++)
                        if (RhythmMaster.GetSingleton().GetSongData().instruments[i].instrumentType.Equals(GetBoardType().CodeName))
                        {
                            _rpphrases = RhythmMaster.GetSingleton().GetSongData().instruments[i].rpPhrases;
                            return _rpphrases;
                        }
                    return null;
                }
                return _rpphrases;
            }
        }

        private SongData.Solo[] _solos;
        private SongData.Solo[] Solos
        {
            get
            {
                if (_solos == null)
                {
                    for (int i = 0; i < RhythmMaster.GetSingleton().GetSongData().instruments.Length; i++)
                        if (RhythmMaster.GetSingleton().GetSongData().instruments[i].instrumentType.Equals(GetBoardType().CodeName))
                        {
                            _solos = RhythmMaster.GetSingleton().GetSongData().instruments[i].solos;
                            return _solos;
                        }
                    return null;
                }
                return _solos;
            }
        }

        public Peripheral Peripheral
        {
            get { return controller; }
            set { controller = value; }
        }

        private uint[] StarPoints
        {
            get
            {
                for (int i = 0; i < RhythmMaster.GetSingleton().GetSongData().instruments.Length; i++)
                    if (RhythmMaster.GetSingleton().GetSongData().instruments[i].instrumentType.Equals(GetBoardType().CodeName))
                        return RhythmMaster.GetSingleton().GetSongData().instruments[i].diffSets[difficulty].starScoreLevels;
                return null;
            }
        }

#region Accessors

        public bool IsFailing
        {
            get { return rockMeterLevel<=0; }
        }

        public bool IsLefty
        {
            get { return controller.LeftySwitch; }
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
                whammyValues = new List<Wave>();
            }
            rockMeterLevel = 80.0f;

            flashRot = 0;

            Board.curveHeight = 0.03f;
            Board.height = -2.0f;
            Board.length = 3f;
            Board.width = 0.7f;
            Board.rotate = .5f;
            Board.zeroZ = 2.8f;
            Board.sFade = 0.8f;
            Board.eFade = 1.2f;
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
            SongData.NoteSet[] notes = Notes;
            for (int i = 0; i < notes.Length - 1; i++)
            {
                uint dist = notes[i + 1].time - notes[i].time;
                if (dist > PILLOW)
                    dist = PILLOW;
                notes[i].late = notes[i].time + dist;
            }
            boardBackground = Texture2D.FromFile(RenderMaster.GetSingleton().graphics.GraphicsDevice, boardBGs[Global.random.Next(boardBGs.Length)]);
            for (int i = 0; i < Notes.Length; i++)
            {
                Notes[i].visible = new SongData.NoteSet.VIS_STATE[GetBoardType().NumTracks];
                for (int r = 0; r < Notes[i].visible.Length; r++)
                    Notes[i].visible[r] = SongData.NoteSet.VIS_STATE.VISIBLE;
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
            int maxColors = 5;
            texTriggers = new Texture2D[maxColors];
            for (int i = 0; i < maxColors; i++)
                texTriggers[i] = content.Load<Texture2D>("graphics\\trigger" + i);
            texTriggersLit = new Texture2D[maxColors];
            for (int i = 0; i < maxColors; i++)
                texTriggersLit[i] = content.Load<Texture2D>("graphics\\triggerlit" + i);
            texNotes = new Texture2D[maxColors];
            for (int i = 0; i < maxColors; i++)
                texNotes[i] = content.Load<Texture2D>("graphics\\Notes" + i);
            texBarNotes = new Texture2D[maxColors];
            for (int i = 0; i < maxColors; i++)
                texBarNotes[i] = content.Load<Texture2D>("graphics\\BarNote" + i);
            vBar = content.Load<Texture2D>("graphics\\vocalbar");
            vBGExt = content.Load<Texture2D>("graphics\\vocalbg_ext");
            vBGInt = content.Load<Texture2D>("graphics\\vocalbg_int");
            vFuzz = content.Load<Texture2D>("graphics\\vocalfuzz");
            vHeadBar = content.Load<Texture2D>("graphics\\vocalheadbar");
            vGlow = content.Load<Texture2D>("graphics\\vGlow");
            List<String> texBGs = new List<String>();
            String[] files = System.IO.Directory.GetFiles(System.IO.Directory.GetCurrentDirectory() + "\\boards\\");
            for (int i = 0; i < files.Length; i++)
            {
                if(files[i].ToLower().EndsWith(".png") || files[i].ToLower().EndsWith(".bmp") || files[i].ToLower().EndsWith(".jpg"))
                    texBGs.Add(files[i]);
            }
            boardBGs = texBGs.ToArray();
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
                    zmdlBoard[i] = new GBVertexFormat(new Vector3(xs[i], ys[i], (zs[i] + 1) / 2), new Vector3(0f, 1f, 0f), new Vector2((xs[i] + 1) / 2, (1 + zs[i]) / 2), new Vector3(0f, 0f, 1f));
                    zmdlBoard[i + xs.Length] = new GBVertexFormat(new Vector3(-xs[i], ys[i], (zs[i] + 1) / 2), new Vector3(0f, 1f, 0f), new Vector2(((-xs[i]) + 1) / 2, (1 + zs[i]) / 2), new Vector3(0f, 0f, 1f));
                }

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
                Model mdlNoteInside = content.Load<Model>("meshes\\noteinside");
                noteInsideVB = ModelConverter.Convert(mdlNoteInside.Meshes[0].VertexBuffer, mdlNoteInside.Meshes[0].MeshParts[0].VertexDeclaration);
                noteInsideIB = mdlNoteInside.Meshes[0].IndexBuffer;
                Model mdlNote = content.Load<Model>("meshes\\note");
                noteVB = ModelConverter.Convert(mdlNote.Meshes[0].VertexBuffer, mdlNote.Meshes[0].MeshParts[0].VertexDeclaration);
                noteIB = mdlNote.Meshes[0].IndexBuffer;
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
                Model mdl = content.Load<Model>("meshes\\triggerdown");
                triggerVB =  ModelConverter.Convert(mdl.Meshes[0].VertexBuffer, mdl.Meshes[0].MeshParts[0].VertexDeclaration);
                triggerIB = mdl.Meshes[0].IndexBuffer;
            }
            
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

            ParticleMaster.GetSingleton().Update(this,gameTime);

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
                /*if (Global.DemoMode)
                {
                    if(currentNoteIndex<Notes.Length)
                        if (Notes[currentNoteIndex].time - (rm.GetCurrentTime()*1000) < 0)
                        {
                            if(!Notes[currentNoteIndex].burning)
                            {
                                if (GetBoardType().ContainsHeldNotes && Notes[currentNoteIndex].length > 0)
                                {
                                    for(int i=0;i<Notes[currentNoteIndex].visible.Length;i++)
                                        Notes[currentNoteIndex].visible[i] = NoteSet.VIS_STATE.INVISIBLE;
                                    Notes[currentNoteIndex].burning = true;
                                    for (int i = 0; i < 5; i++)
                                        if ((Notes[currentNoteIndex].type & (((ulong)1) << i)) != 0)
                                            popupSpeed[i] += 100;
                                    flashRot = (float)(UnsignedGame.r.Next() * Math.PI * 2);
                                    return Notes[currentNoteIndex].type;
                                }
                                else
                                {
                                    for (int i = 0; i < Notes[currentNoteIndex].visible.Length; i++)
                                        Notes[currentNoteIndex].visible[i] = NoteSet.VIS_STATE.INVISIBLE;
                                    for (int i = 0; i < 5; i++)
                                        if ((Notes[currentNoteIndex].type & (((ulong)1) << i)) != 0)
                                               popupSpeed[i] += 100;
                                    currentNoteIndex++;
                                    flashRot = (float)(UnsignedGame.r.Next() * Math.PI * 2);
                                    return Notes[currentNoteIndex - 1].type;
                                }
                            }
                            else //well, i'm just gonna assume we support held Notes
                            {
                                if ((Notes[currentNoteIndex].end) - (rm.GetCurrentTime()*1000) < 0)
                                {
                                    currentNoteIndex++;
                                    return 0;
                                }
                                else
                                {
                                    Burn(gameTime,Notes[currentNoteIndex].type);
                                    return 0;// (byte)(Game1.bits[7] | Notes[currentNoteIndex].type);
                                }
                            }
                        }
                    return 0;
                }*/
                sinwavestart -= gameTime.ElapsedGameTime.TotalSeconds*0.2;

                if (GetBoardType().CanWhammy)
                    Whammy(controller.GetAnalogValue(PeripheralAnalog.WHAMMY_BAR), gameTime);

                if (GetBoardType().RPEnableType == Instrument.RockPowerEnableTypes.FILL)
                {
                    if ((StarPowerAmount < 0.5 || SPActivated) &&
                        (FillIndex < Fills.Length && rm.GetCurrentTime()*1000 > Fills[FillIndex].time))
                    {
                        FillIndex++;
                    }
                    if (FillIndex < Fills.Length && rm.GetCurrentTime()*1000 > Fills[FillIndex].end)
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
                        ParticleMaster.GetSingleton().AddSparks(newPressed, this, 10);
                        for (int i = 0; i < 5; i++)
                            if ((newPressed & (((ulong)1) << i)) != 0)
                            {
                                popupSpeed[i] += 100f;
                                Fills[FillIndex].amount += 1f / (((Fills[FillIndex].len) / 1000f) * 6);
                                Fills[FillIndex].amount = Math.Min(Fills[FillIndex].amount, 1);
                            }
                        while (Notes[currentNoteIndex].time <= Fills[FillIndex].end + 100)
                        { myResults.hitNotes++; for (int q = 0; q < GetBoardType().NumTracks; q++) Notes[currentNoteIndex].visible[q] = SongData.NoteSet.VIS_STATE.INVISIBLE; currentNoteIndex++; }
                        if (rm.GetCurrentTime()*1000 >= (Fills[FillIndex].end) - 100 && rm.GetCurrentTime()*1000 <= Fills[FillIndex].end)
                            if ((newPressed & 0x8) != 0)
                                if (Fills[FillIndex].amount >= 1)
                                    SPDelayStart = true;
                    }
                }
                if (currentNoteIndex< Notes.Length && Notes[currentNoteIndex].time - PILLOW < rm.GetCurrentTime()*1000)
                {
                    if (!GetBoardType().NeedsStrum)
                    {
                        ulong flash = Notes[currentNoteIndex].AddPressedDrums(GetBoardType(),newPressed);
                        for (int i = 0; i < 5; i++)
                            if ((flash & (((ulong)1) << i)) != 0)
                            {
                                multiplier += 0.1f;
                                popupSpeed[i] += 100f;
                            }
                        for (int i = 0; i < 5; i++)
                            if ((flash & (((ulong)1) << i)) != 0)
                            { score += 100 * (int)multiplier; }
                        if (Notes[currentNoteIndex].IsGood(GetBoardType(),false))
                        {
                            Notes[currentNoteIndex].Kill();
                            Help();
                            currentNoteIndex++;
                            myResults.hitNotes++;
                            ParticleMaster.GetSingleton().AddShards(Notes[currentNoteIndex - 1].type, this, 10);
                            return Notes[currentNoteIndex - 1].type;
                        }
                        if (Notes[currentNoteIndex].late <= rm.GetCurrentTime()*1000)
                        {
                            if (RPIndex < RPPhrases.Length && Notes[currentNoteIndex].time >= RPPhrases[RPIndex].time && Notes[currentNoteIndex].time <= RPPhrases[RPIndex].end)
                                RPGood = false;
                            currentNoteIndex++;
                            multiplier = 1;
                            Hurt();
                            myResults.missedNotes++;
                        }
                    }
                    else
                    {
                        if (controller.WasPressed(PeripheralButton.UP) || controller.WasPressed(PeripheralButton.DOWN))
                            Strum();

                        if (GetBoardType().ContainsHeldNotes && Notes[currentNoteIndex].burning)
                        {
                            for(int k=0;k<whammyValues.Count;k++)
                            {
                                whammyValues[k].Time += (float)gameTime.ElapsedGameTime.TotalSeconds;
                            }
                            if (Notes[currentNoteIndex].end <= rm.GetCurrentTime()*1000)
                                currentNoteIndex++;
                            else if (!SongData.NoteSet.IsValidFrettage(Notes[currentNoteIndex].type, pressed,GetBoardType()))
                                currentNoteIndex++;
                            else
                            {
                                Burn(gameTime, Notes[currentNoteIndex].type);
                            }
                        }
                        else
                        {
                            Notes[currentNoteIndex].AddPressedGuitar(GetBoardType(),pressed);
                            if (Notes[currentNoteIndex].IsGood(GetBoardType(), currentNoteIndex > 0 && Notes[currentNoteIndex - 1].visible[0] == SongData.NoteSet.VIS_STATE.INVISIBLE))
                            {
                                Notes[currentNoteIndex].Kill();
                                for (int i = 0; i < GetBoardType().NumTracks; i++)
                                    if ((Notes[currentNoteIndex].type & (((ulong)1) << i)) != 0)
                                        popupSpeed[i] += 100f;
                                multiplier += 0.1f;
                                for (int i = 0; i < GetBoardType().NumTracks; i++)
                                    if ((Notes[currentNoteIndex].type & (((ulong)1) << i)) != 0)
                                    { score += 100 * (int)multiplier; }
                                myResults.hitNotes++;
                                ParticleMaster.GetSingleton().AddShards(Notes[currentNoteIndex].type, this, 10);
                                Help();
                                if (Notes[currentNoteIndex].length > 0)
                                {
                                    Notes[currentNoteIndex].burning = true;
                                    return Notes[currentNoteIndex].type;
                                }
                                else
                                    currentNoteIndex++;
                                return Notes[currentNoteIndex - 1].type;
                            }
                            if (Notes[currentNoteIndex].late <= rm.GetCurrentTime()*1000)
                            {
                                if (RPIndex < RPPhrases.Length && Notes[currentNoteIndex].time >= RPPhrases[RPIndex].time && Notes[currentNoteIndex].time <= RPPhrases[RPIndex].end)
                                    RPGood = false;
                                currentNoteIndex++;
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

                if (RPIndex < RPPhrases.Length && Notes[currentNoteIndex].end > RPPhrases[RPIndex].end)
                {
                    if (RPGood)
                    {
                        StarPowerAmount += 0.25f;
                        myResults.hitSPPH++;
                    }
                    else 
                        myResults.missedSPPH++;
                    RPGood = true;
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
            if (!GetBoardType().NeedsStrum)
                return 0;
            if (currentNoteIndex < Notes.Length && Notes[currentNoteIndex].time - PILLOW < RhythmMaster.GetSingleton().GetCurrentTime()*1000 && !Notes[currentNoteIndex].HasStrummed())
                Notes[currentNoteIndex].Strum();
            else
            { multiplier = 1; Hurt(); }
            return 0;
        }

        public byte Bang(byte pressed, long currenttime, UnsignedGame game)
        {
            return 0;
        }

        public float GetStars()
        {
            int i;
            for (i = 0; i < StarPoints.Length; i++)
                if (score >= StarPoints[i])
                    break;
            if (i > 5)
                return 5;
            if (score < StarPoints[0])
                return score / StarPoints[0];
            float dec = (float)(score - StarPoints[i]) / (StarPoints[i+1] - StarPoints[i]);
            return dec + i+1;
        }

        public byte GetDifficulty()
        {
            return difficulty;
        }

        public int GetScore()
        {
            return (int)score;
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

        public float GetSPAmount()
        {
            return SPADisplay;
        }

        public bool IsSPActivated()
        {
            return SPActivated;
        }

        public void Whammy(float whammyAmount,GameTime gametime)
        {
            if (currentNoteIndex >= Notes.Length)
                return;
            float bpm = RhythmMaster.GetSingleton().GetBPM();
            long currenttime = (long)(RhythmMaster.GetSingleton().GetCurrentTime()*1000);
            if (Notes[currentNoteIndex].burning && Notes[currentNoteIndex].time < currenttime && Notes[currentNoteIndex].end > currenttime)
            {
                if(whammyValues.Count<=0 || whammyValues[0].Value!=whammyAmount)
                    whammyValues.Insert(0, new Wave(whammyAmount, 0));
                score += (int)(gametime.ElapsedGameTime.TotalSeconds * bpm);
                if (RPIndex < RPPhrases.Length && currentNoteIndex >= RPPhrases[RPIndex].time && currentNoteIndex <= RPPhrases[RPIndex].end)
                    StarPowerAmount += (float)(((gametime.ElapsedGameTime.TotalSeconds * 1000) * bpm));
                if (RPIndex < RPPhrases.Length && currentNoteIndex >= RPPhrases[RPIndex].time && currentNoteIndex <= RPPhrases[RPIndex].end)
                    StarPowerAmount += (float)(0.05f * ((gametime.ElapsedGameTime.TotalSeconds * 1000) * bpm));
            }
            else
                whammyValues.Clear();
            while (whammyValues.Count > 0 && whammyValues[whammyValues.Count - 1].Time > eFade)
                whammyValues.RemoveAt(whammyValues.Count - 1);
        }

        private float GetWaveWidth(double time, bool timesWidth)
        {
            float sinRet, whammyRet=0;

            //part one: get the scale value based on user input
            if (whammyValues.Count < 1)
                whammyRet = 0.1f;
            else
            {
                int i;
                for (i = 0; i < whammyValues.Count; i++)
                {
                    if (time < whammyValues[i].Time)
                        break;
                }
                i--;
                if (i < 0)
                {
                    //lerp between start and first
                    float timeDifference = whammyValues[0].Time;

                    whammyRet = FVMath.Lerp(0, whammyValues[0].Value, (((float)time - 0) / timeDifference));
                }
                else if (i >= whammyValues.Count - 1)
                {
                    //lerp bertween last and end
                    float timeDifference = eFade-whammyValues[whammyValues.Count-1].Time;

                    whammyRet = FVMath.Lerp(whammyValues[whammyValues.Count-1].Value, 0, (((float)time - whammyValues[whammyValues.Count-1].Time) / timeDifference));
                }
                else
                {
                    //lerp between two nodes
                    float timeDifference = whammyValues[i+1].Time-whammyValues[i].Time;

                    whammyRet = FVMath.Lerp(whammyValues[i].Value, whammyValues[i + 1].Value, (((float)time - whammyValues[i].Time) / timeDifference));
                }
            }

            // part two: get the sin for curviness
            sinRet = (float)Math.Sin((sinwavestart+time)*20f);

            // part three: multiply <-- this is the hard one
            return sinRet*(timesWidth?whammyRet:0.1f);
        }

        public void Burn(GameTime gt, ulong note)
        {
            int num = Global.AddBits(note & ~(((ulong)1) << GetBoardType().NumTracks));
            score += (int)(num * 100 * gt.ElapsedGameTime.TotalSeconds);
            ParticleMaster.GetSingleton().AddSparks(note, this, 0.05f);
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
            FVShader effect = rm.engine;
            GraphicsDeviceManager graphics = rm.graphics;

            if (GetBoardType().Dimensions == Instrument.BoardDimensions.TWO_DIMENSIONAL)
            {
                long currenttime = (long)(RhythmMaster.GetSingleton().GetCurrentTime() * 1000);
                Vector2 center = new Vector2(vFuzz.Width / 2, vFuzz.Height / 2);
                float vScale = 0.2f;
                float height = vFuzz.Height * vScale;
                Color glow = new Color(150, 255, 150, 255);
                spritebatch.Draw(vBGInt, new Rectangle(0, (int)vocaly, 1024, (int)vocalheight), Color.White);
                for (int i = currentNoteIndex; i < Phrases.Length; i++)
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

                for (int i = currentNoteIndex; i < Phrases.Length; i++)
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

                rm.View = Matrix.Identity;
                effect.Projection = matProj;

                //get board measure world lengths

#if DEBUG_CAM_CONTROL
                                    fling *= Matrix.CreateRotationX(-0.4f);
#endif

                Matrix matTransl = Matrix.CreateTranslation(0f, Board.height + (GetBoardBump() * Board.BOARD_BUMP_COEF), 0f);
                effect.AmbientMaterial = Color.White;

                effect.LightingEnabled = GameSettings.Lighting;
                effect.SpecularEnabled = GameSettings.Specular;
                effect.NormalMapEnabled = GameSettings.NormalMapping;

                effect.CommitChanges();

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
                        DrawFlashes(fling);
                        ParticleMaster.GetSingleton().Render(this);
                    }
                    pass.End();
                }
                effect.End();
                spritebatch.Begin();
                spritebatch.DrawString(Global.DefaultFont, "" + rockMeterLevel, new Vector2(20, 120), Color.Red);
                spritebatch.End();
                graphics.GraphicsDevice.SetRenderTarget(0, null);
            }
        }

        internal void EatHalfSP()
        {
            StarPowerAmount -= 0.5f;
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
            spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
            double currentTime = RhythmMaster.GetSingleton().GetCurrentTime();
            for (int i = Math.Max(0,currentNoteIndex-1); i < Notes.Length; i++)
            {
                if (Notes[i].time / 1000f > currentTime + eFade)
                    break;
                if (Notes[i].length <= 0)
                    continue;
                bool isGray = false;
                bool isSP = false;
                for (int spi = 0; spi < RPPhrases.Length; spi++)
                {
                    if (Notes[i].time >= RPPhrases[spi].time && Notes[i].time <= RPPhrases[spi].end)
                    {
                        if (spi != RPIndex)
                            isSP = true;
                        else if (RPGood)
                            isSP = true;
                    }
                }
                if (i<currentNoteIndex || (Notes[i].time < currentTime * 1000 && Notes[i].end > currentTime * 1000 && !Notes[i].burning))
                    isGray = true;

                float LineWidth = 0.8f;

                // how far between nodes (curve resolution) in seconds
                float spaceBetween = 0.01f;

                float k;
                for(k=(float)Math.Max(0,Notes[i].time/1000.0f-currentTime);k+spaceBetween<Math.Min(eFade,Notes[i].end/1000.0f-currentTime);k+=spaceBetween)
                {
                    float Z1 = GetBoardPos(k + currentTime, 1 - ratio) * rtWaves.Height;
                    float Z2 = GetBoardPos(k + spaceBetween + currentTime, 1 - ratio) * rtWaves.Height;
                    float X1base = ((GetWaveWidth(k,Notes[i].burning) + 1) / 2.0f);
                    float X2base = ((GetWaveWidth(k + spaceBetween, Notes[i].burning) + 1) / 2.0f);
                    float X3base = (((((X1base * 2) - 1) * -1) + 1) / 2.0f);
                    float X4base = (((((X2base * 2) - 1) * -1) + 1) / 2.0f);

                    float Length;
                    float Angle1, Angle2;

                    {
                        float X1 = ((X1base) / (float)GetBoardType().NumDrawnTracks) * rtWaves.Width;
                        float X2 = ((X2base) / (float)GetBoardType().NumDrawnTracks) * rtWaves.Width;
                        float X3 = ((X3base) / (float)GetBoardType().NumDrawnTracks) * rtWaves.Width;
                        float X4 = ((X4base) / (float)GetBoardType().NumDrawnTracks) * rtWaves.Width;

                        Length = (new Vector2(X2 - X1, Z2 - Z1)).Length();
                        Angle1 = (float)Math.Atan2(Z2 - Z1, X2 - X1);
                        Angle2 = (float)Math.Atan2(Z2 - Z1, X4 - X3);
                    }

                    for (int r = 0; r < GetBoardType().NumDrawnTracks; r++)
                    {
                        if ((Notes[i].type & (((ulong)1) << r)) == 0)
                            continue;

                        float X1a = ((X1base + r) / (float)GetBoardType().NumDrawnTracks) * rtWaves.Width;
                        float X1b = ((X3base + r) / (float)GetBoardType().NumDrawnTracks) * rtWaves.Width;

                        spritebatch.Draw(texLine, new Vector2(X1a, Z1), null, isGray? Color.Gray : isSP ? Color.White : Global.FretColors[GetBoardType().colorIndices[r]], Angle1, new Vector2(0, texLine.Height / 2), new Vector2(Length / texLine.Width, LineWidth), SpriteEffects.None, 0);
                        spritebatch.Draw(texLine, new Vector2(X1b, Z1), null, isGray ? Color.Gray : isSP ? Color.White : Global.FretColors[GetBoardType().colorIndices[r]], Angle2, new Vector2(0, texLine.Height / 2), new Vector2(Length / texLine.Width, LineWidth), SpriteEffects.None, 0);
                    }
                }
                //last one :P
                if (Notes[i].end > 1000 * currentTime)
                {
                    float Z1 = GetBoardPos(k + currentTime, 1 - ratio) * rtWaves.Height;
                    float Z2 = GetBoardPos(Notes[i].end/1000.0f, 1 - ratio) * rtWaves.Height;
                    float X1base = ((GetWaveWidth(k, Notes[i].burning) + 1) / 2.0f);
                    float X2base = ((GetWaveWidth(Math.Min(Notes[i].end / 1000.0f - (float)currentTime, eFade), Notes[i].burning) + 1) / 2.0f);


                    for (int r = 0; r < GetBoardType().NumDrawnTracks; r++)
                    {
                        if ((Notes[i].type & (((ulong)1) << r)) == 0)
                            continue;

                        float X1 = ((X1base + r) / (float)GetBoardType().NumDrawnTracks) * rtWaves.Width;
                        float X2 = ((X2base + r) / (float)GetBoardType().NumDrawnTracks) * rtWaves.Width;

                        float Length = (new Vector2(X2 - X1, Z2 - Z1)).Length();
                        float Angle = (float)Math.Atan2(Z2 - Z1, X2 - X1);

                        spritebatch.Draw(texLine, new Vector2(X1, Z1), null, isGray ? Color.Gray : (isSP) ? Color.White : Global.FretColors[GetBoardType().colorIndices[r]], Angle, new Vector2(0, texLine.Height / 2), new Vector2(Length / texLine.Width, LineWidth), SpriteEffects.None, 0);
                    }
                }
            }
            spritebatch.End();


            
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
                bgColor = new Color(30, 30, 30);

            for (int i = 0; i < RhythmMaster.GetSingleton().GetSongData().info.barlines.Length; i++)
            {
                SongData.Barline[] barlines = RhythmMaster.GetSingleton().GetSongData().info.barlines;
                float Y = GetBoardPos(barlines[i].time / 1000.0, 1 - ratio) * rtBoard.Height;
                if (Y < 0)
                    break;
                if (i < RhythmMaster.GetSingleton().GetSongData().info.barlines.Length - 1)
                {
                    float Y2 = GetBoardPos(barlines[i + 1].time / 1000.0, 1 - ratio) * rtBoard.Height;
                    spritebatch.Draw(boardBackground,new Rectangle(0,(int)Y2,rtBoard.Width,(int)(Y-Y2)),bgColor);
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
            for (int i = 1; i < GetBoardType().NumDrawnTracks; i++)
            {
                spritebatch.Draw(Global.texWhite, new Rectangle((int)(((float)i / (float)GetBoardType().NumDrawnTracks) * rtBoard.Width), 0, 1, rtBoard.Height), Color.White);
            }

            // Draw Board Fills
            if ((GetBoardType().RPEnableType&Instrument.RockPowerEnableTypes.FILL)!=0 && StarPowerAmount>=0.5f && !SPActivated)
            {
                int k = FillIndex;
                float halfMaxWidth = rtBoard.Width / (float)(GetBoardType().NumDrawnTracks*2);
                if (!Fills[k].hitGreen)
                {
                    float y1 = GetBoardPos(Fills[k].end / 1000.0f, 1 - ratio) * rtBoard.Height;
                    float y2 = GetBoardPos(Fills[k].time / 1000.0f, 1 - ratio) * rtBoard.Height;
                    for (int r = 0; r < GetBoardType().NumDrawnTracks; r++)
                    {
                        float center = ((r * 2 + 1) / (float)(GetBoardType().NumDrawnTracks*2)) * rtBoard.Width;
                        spritebatch.Draw(Board.drumfillTex, new Rectangle((int)(center - (halfMaxWidth * Fills[k].amount)), (int)y1, (int)(2 * (halfMaxWidth * ((Fills[k].amount*0.8f)+0.2f))), (int)((y2 - y1) + 0.5f)), Global.FretColors[GetBoardType().colorIndices[r]]);
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
                int rpHeight = (int)((rtBoard.Width / (float)Board.spMeterBG.Width) * Board.spMeterBG.Height * Board.RP_METER_Y_SCALE);
                // this should be compounded on rpHeight
                float rpMeterHeight = 0.21875f;
                // the shift so it isnt covered by the fretboard
                int spMeterShift = 7;
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

            Texture2D texWaves = rtWaves.GetTexture();
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

            Texture2D texBoard = rtBoard.GetTexture();
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
        }

        private void DrawNotes(Matrix fling)
        {
            RenderMaster rm = RenderMaster.GetSingleton();
            RhythmMaster rtm = RhythmMaster.GetSingleton();
            FVShader effect = rm.engine;

            GraphicsDeviceManager graphics = rm.graphics;

            effect.DirectionalLight = new DirectionalLight(true,new Vector3(0, 2, 1),new Color(200, 200, 200),new Color(150, 150, 150));
            effect.DiffuseMaterial = new Color(200, 200, 200);

            graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
            graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
            graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;


            if (GetBoardType().Dimensions == Instrument.BoardDimensions.THREE_DIMENSIONAL)
            {
                int lefty = 1;
                if (IsLefty)
                    lefty = -1;

                bool whited = false;
                for (int r = 0; r < GetBoardType().NumTracks; r++)
                {
                    if (r < GetBoardType().NumDrawnTracks)
                        effect.DiffuseTexture = Board.texNotes[GetBoardType().colorIndices[r]];
                    else
                        effect.DiffuseTexture = Board.texBarNotes[GetBoardType().colorIndices[r - GetBoardType().NumDrawnTracks]];
                    whited = false;
                    for (int p = Math.Max(currentNoteIndex-32,0); p < Notes.Length; p++)
                    {
                        if((GetBoardType().RPEnableType & Instrument.RockPowerEnableTypes.FILL)!=0)
                            if (StarPowerAmount >= 0.5f && !SPActivated)
                                if (FillIndex < Fills.Length)
                                    if(Notes[p].time >= Fills[FillIndex].time && Notes[p].time <= Fills[FillIndex].end)
                                        continue;
                        bool IsWhite = false;
                        for(int i=0;i<RPPhrases.Length;i++)
                            if (Notes[p].time >= RPPhrases[i].time && Notes[p].time <= RPPhrases[i].end)
                            {
                                if (i != RPIndex)
                                    IsWhite = true;
                                else if (RPGood)
                                    IsWhite = true;
                            }
                        if (IsWhite && !whited)
                        { effect.DiffuseTexture = Global.texWhite; whited = true; }
                        else if (!IsWhite && whited)
                        {
                            if (r < GetBoardType().NumDrawnTracks)
                                effect.DiffuseTexture = Board.texNotes[GetBoardType().colorIndices[r]];
                            else
                                effect.DiffuseTexture = Board.texBarNotes[GetBoardType().colorIndices[r - GetBoardType().NumDrawnTracks]];
                            whited = false;
                        }

                        // how far along the board... should be called Z probly
                        float ct = (float)rtm.GetCurrentTime();
                        float Y = ((Notes[p].time / 1000f) - ct);
                        if (Y > eFade)
                            break;

                        if ((Notes[p].type & (((ulong)1) << r)) == 0)
                            continue;
                        if (Notes[p].visible[r] == SongData.NoteSet.VIS_STATE.INVISIBLE)
                            continue;

                        Matrix matIdentity, matTransl, matScale, matOrbit;
                        matIdentity = Matrix.Identity;
                        if( r >= GetBoardType().NumDrawnTracks )
                            matTransl = Matrix.CreateTranslation(0f, Board.height + (GetBoardBump() * Board.BOARD_BUMP_COEF), 0f);
                        else
                            matTransl = Matrix.CreateTranslation(0f, Board.height + (GetBoardBump() * Board.BOARD_BUMP_COEF) + 0.02f, 0f);

                        if(r>=GetBoardType().NumDrawnTracks)
                            matOrbit = Matrix.CreateTranslation(0, 0f, -(Board.length * Y) - Board.zeroZ) * fling;
                        else
                            matOrbit = Matrix.CreateTranslation(((((r * 2) + 1) / (float)(GetBoardType().NumDrawnTracks * 2)) - 0.5f) * lefty * Board.width * 2.0f, 0f, -(Board.length * Y) - Board.zeroZ) * fling;

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

                        effect.DiffuseMaterial = Color.White;

                        effect.Alpha = alpha;

                        // identity, scale, rotate, orbit(translate & rotate), translate
                        effect.World = matIdentity * matScale * matOrbit * matTransl;

                        graphics.GraphicsDevice.VertexDeclaration = GBVertexFormat.VertexDeclaration;

                        effect.CommitChanges();
                        if (r>=GetBoardType().NumDrawnTracks)
                        {
                            graphics.GraphicsDevice.Vertices[0].SetSource(Board.mdlTriggerBorder, 0, GBVertexFormat.SizeInBytes);
                            graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, (Board.mdlTriggerBorder.SizeInBytes / GBVertexFormat.SizeInBytes) / 3);
                        }
                        else
                        {
                            graphics.GraphicsDevice.Vertices[0].SetSource(noteInsideVB, 0, GBVertexFormat.SizeInBytes);
                            graphics.GraphicsDevice.Indices = noteInsideIB;
                            graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, noteInsideIB.SizeInBytes / (noteInsideIB.IndexElementSize == IndexElementSize.ThirtyTwoBits ? 4 : 2), 0, noteInsideIB.SizeInBytes / (noteInsideIB.IndexElementSize == IndexElementSize.ThirtyTwoBits ? 12 : 6));

                            effect.Shininess = 32f;
                            effect.SpecularMaterial = Color.White;
                            effect.AmbientMaterial = new Color(50, 50, 50);
                            effect.CommitChanges();

                            graphics.GraphicsDevice.Vertices[0].SetSource(noteVB, 0, GBVertexFormat.SizeInBytes);
                            graphics.GraphicsDevice.Indices = noteIB;
                            graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, noteIB.SizeInBytes / (noteIB.IndexElementSize == IndexElementSize.ThirtyTwoBits ? 4 : 2), 0, noteIB.SizeInBytes / (noteIB.IndexElementSize == IndexElementSize.ThirtyTwoBits ? 12 : 6));

                            effect.AmbientMaterial = Color.White;
                        }
                        
                    }
                }
                if((GetBoardType().RPEnableType& Instrument.RockPowerEnableTypes.FILL)!=0)
                if(FillIndex<Fills.Length && Fills[FillIndex].time/1000f<rtm.GetCurrentTime()+eFade)
                for(int r=3;r<4;r++)
                {
                    if (r < GetBoardType().NumDrawnTracks)
                        effect.DiffuseTexture = Board.texNotes[GetBoardType().colorIndices[r]];
                    
                    {
                        // how far along the board... should be called Z probly
                        float ct = (float)rtm.GetCurrentTime();
                        float Y = (((Fills[FillIndex].end-50) / 1000f) - ct);
                        if (Y > eFade)
                            break;

                        Matrix matIdentity, matTransl, matScale, matOrbit;
                        matIdentity = Matrix.Identity;
                        matTransl = Matrix.CreateTranslation(0f, Board.height + (GetBoardBump() * Board.BOARD_BUMP_COEF) + 0.02f, 0f);

                        matOrbit = Matrix.CreateTranslation((((((GetBoardType().NumDrawnTracks-1) * 2) + 1) / (float)(GetBoardType().NumDrawnTracks * 2)) - 0.5f) * lefty * Board.width * 2.0f, 0f, -(Board.length * Y) - Board.zeroZ) * fling;

                        matOrbit = Matrix.CreateRotationX(-Y * MathHelper.Pi) * matOrbit;
                        float sc = (Fills[FillIndex].amount - 0.8f) * 5;
                        if (sc > 1)
                            sc = 1;
                        if (sc < 0)
                            break;
                        matScale = Matrix.CreateScale(new Vector3(sc * 0.125f * Board.width, 2 * sc * 0.05f * Board.length, 2 * sc * 0.05f * Board.length));

                        float alpha;
                        if (Y < Board.sFade)
                            alpha = 1;
                        else if (Y < Board.eFade)
                            alpha = 1 - ((Y - Board.sFade) / (Board.eFade - Board.sFade));
                        else
                            alpha = 0;


                        effect.Alpha = alpha;
                        effect.DiffuseMaterial = Color.White;

                        // identity, scale, rotate, orbit(translate & rotate), translate
                        effect.World = matIdentity * matScale * matOrbit * matTransl;

                        effect.CommitChanges();
                        // 5: draw object - select vertex type, primitive type, # of primitives

                        graphics.GraphicsDevice.Vertices[0].SetSource(noteInsideVB, 0, GBVertexFormat.SizeInBytes);
                        graphics.GraphicsDevice.Indices = noteInsideIB;
                        graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, noteInsideIB.SizeInBytes / (noteInsideIB.IndexElementSize == IndexElementSize.ThirtyTwoBits ? 4 : 2), 0, noteInsideIB.SizeInBytes / (noteInsideIB.IndexElementSize == IndexElementSize.ThirtyTwoBits ? 12 : 6));

                        effect.Shininess = 32f;
                        effect.SpecularMaterial = Color.White;
                        effect.AmbientMaterial = new Color(50, 50, 50);
                        effect.CommitChanges();

                        graphics.GraphicsDevice.Vertices[0].SetSource(noteVB, 0, GBVertexFormat.SizeInBytes);
                        graphics.GraphicsDevice.Indices = noteIB;
                        graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, noteIB.SizeInBytes / (noteIB.IndexElementSize == IndexElementSize.ThirtyTwoBits ? 4 : 2), 0, noteIB.SizeInBytes / (noteIB.IndexElementSize == IndexElementSize.ThirtyTwoBits ? 12 : 6));

                        effect.AmbientMaterial = Color.White;
                    }
                }
            }
        }

        private void DrawBoardDetail(Matrix fling, Matrix matTransl)
        {
            if (GetBoardType().Dimensions==Instrument.BoardDimensions.THREE_DIMENSIONAL)
            {
                RenderMaster rm = RenderMaster.GetSingleton();
                FVShader effect = rm.engine;
                SpriteBatch spritebatch = rm.spritebatch;
                GraphicsDeviceManager graphics = rm.graphics;

                graphics.GraphicsDevice.VertexDeclaration = GBVertexFormat.VertexDeclaration;

                effect.Alpha = 1.0f;
                effect.AmbientMaterial = Color.White;
                effect.DiffuseMaterial = new Color(128, 128, 128);
                effect.SpecularMaterial = Color.Black;

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
                        matScale = Matrix.CreateScale(new Vector3((Board.width / (float)GetBoardType().NumDrawnTracks), 0.05f, .05f));

                        // identity, scale, rotate, orbit(translate & rotate), translate
                        effect.World = matIdentity * matScale * matOrbit * matTransl;

                        //effect.Parameters["proj"].SetValue(matProj);

                        
                        if((controller.GetFrets()&(((ulong)1)<<p))!=0)
                        { effect.DiffuseTexture = Board.texTriggersLit[GetBoardType().colorIndices[p]]; glow[p] = true; }
                        else
                            effect.DiffuseTexture = Board.texTriggers[GetBoardType().colorIndices[p]];

                        effect.CommitChanges();

                        // 5: draw object - select vertex type, primitive type, # of primitives
                        graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                        graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                        graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                        graphics.GraphicsDevice.Vertices[0].SetSource(triggerVB, 0, GBVertexFormat.SizeInBytes);
                        graphics.GraphicsDevice.Indices = triggerIB;
                        graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, (triggerIB.SizeInBytes / (triggerIB.IndexElementSize == IndexElementSize.ThirtyTwoBits ? 4 : 2)), 0, (triggerIB.SizeInBytes / (triggerIB.IndexElementSize == IndexElementSize.ThirtyTwoBits ? 12 : 6)));
                        graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                    }
                }
            }
        }

        private void DrawFlashes(Matrix fling)
        {
            RenderMaster rm = RenderMaster.GetSingleton();
            FVShader effect = rm.engine;
            GraphicsDeviceManager graphics = rm.graphics;

            effect.DiffuseTexture = Board.texBlast;
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

                    effect.DiffuseMaterial = Global.FretColors[GetBoardType().colorIndices[i]];
                    effect.CommitChanges();

                    // 5: draw object - select vertex type, primitive type, # of primitives
                    graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                    graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                    graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                    graphics.GraphicsDevice.Vertices[0].SetSource(Global.square, 0, GBVertexFormat.SizeInBytes);
                    graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
                    graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                }
            effect.DiffuseMaterial = Color.White;
        }

        private void DrawBoard(Matrix fling, Matrix matTransl)
        {
            RenderMaster rm = RenderMaster.GetSingleton();
            GraphicsDeviceManager graphics = rm.graphics;
            FVShader effect = rm.engine;

            effect.AmbientMaterial = Color.White;
            effect.DiffuseMaterial = new Color(128, 128, 128);
            effect.SpecularMaterial = Color.Black;
            effect.TextureEnabled = true;

            graphics.GraphicsDevice.VertexDeclaration = GBVertexFormat.VertexDeclaration;

            graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;
            Matrix matScale, matOrbit;
            matOrbit = Matrix.CreateTranslation(0f, 0f, -Board.zeroZ - (Board.eFade * Board.length)) * fling;
            matScale = Matrix.CreateScale(new Vector3(Board.width, Board.curveHeight, Board.length * Board.eFade * (1 / BOARD_FADE_RATIO)));
            effect.World = matScale * matOrbit * matTransl;
            effect.DiffuseTexture = rtBoard.GetTexture();
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
            FVShader effect = rm.engine;

            effect.AmbientMaterial = Color.White;
            effect.DiffuseMaterial = new Color(128, 128, 128);
            effect.SpecularMaterial = Color.Black;
            effect.TextureEnabled = true;

            graphics.GraphicsDevice.VertexDeclaration = GBVertexFormat.VertexDeclaration;

            Matrix matScale, matOrbit;
            matOrbit = Matrix.CreateTranslation(0f, 0.01f,  -Board.zeroZ - (Board.eFade * Board.length)) * fling;
            matScale = Matrix.CreateScale(new Vector3(Board.width, Board.curveHeight, Board.length * Board.eFade * (1 / BOARD_FADE_RATIO)));
            effect.World = matScale * matOrbit * matTransl;
            effect.DiffuseTexture = rtWaves.GetTexture();
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
            if (difficulty == 0)
            {
                if (rockMeterLevel > 80)
                    rockMeterLevel += 1 * (SPActivated ? 10 : 1);
                else
                    rockMeterLevel += 4f * (SPActivated ? 10 : 1);
            }
            else if (difficulty == 1)
            {
                if (rockMeterLevel > 80)
                    rockMeterLevel += 1 * (SPActivated ? 10 : 1);
                else
                    rockMeterLevel += 3f * (SPActivated ? 10 : 1);
            }
            else if (difficulty == 2)
            {
                if (rockMeterLevel > 80)
                    rockMeterLevel += 0.75f * (SPActivated ? 10 : 1);
                else
                    rockMeterLevel += 2f * (SPActivated ? 10 : 1);
            }
            if (difficulty == 3)
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

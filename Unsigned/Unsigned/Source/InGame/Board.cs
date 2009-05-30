using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using UnsignedPeripheralPlugins;
using SongDataIO;
using FVProductions.Utility;

namespace Unsigned
{

    class Board
    {
        private class SoloEndMessage
        {
            private const float FADE_TIME = 0.5f;
            private const float HOLD_TIME = 1.0f;

            public String Message { get; private set; }
            public float Time { get; private set; }
            public float Alpha { get; private set; }
            public bool ShouldDestroy { get; private set; }

            public SoloEndMessage(String msg)
            {
                Message = msg;
                Time = 0;
                Alpha = 0;
                ShouldDestroy = false;
            }

            public void Update(SongTime songTime)
            {
                Time += (float)songTime.ElapsedGameTime.TotalSeconds;
                float x;
                if (Time < FADE_TIME)
                    x = Time / FADE_TIME;
                else if (Time < FADE_TIME + HOLD_TIME)
                    x = 1;
                else
                    x = 1 - ((Time - (FADE_TIME + HOLD_TIME)) / FADE_TIME);

                Alpha = -(((x * 1.2f) - 1) * ((x * 1.2f) - 1)) + 1;

                if (Time >= FADE_TIME + HOLD_TIME + FADE_TIME)
                    ShouldDestroy = true;
            }
        }

        private struct Wave
        {
            public float Value;
            public float Time;

            public Wave(float val, float time)
            {
                Value = val;
                Time = time;
            }
        }

        // scaling values
        private static float vocalheight, vocaly, vocalzerox, vocalwidth;
        private static float width, curveHeight, height, rotate, sFade, eFade;
        public static float Width { get { return width; } }
        public static float Length { get; private set; }
        public static float Height { get { return height; } }
        public static float Rotate { get { return rotate; } }
        public static float LengthRPRatio { get; private set; }

        // textures, models
        public static FVShader effect;
        private static Effect faderEffect;
        private static SpriteBatch spriteBatch;
        private static Texture2D drumfillTex, spMeterBG, spMeterLED, spMeterFill, spMeterCurl, texBoardBorderFade;
        private static Texture2D vBar, vBGExt, vBGInt, vFuzz, vHeadBar, vGlow, texBlast, texNotesWhite;
        private static FVModel mdlBoard, mdlBoardBorder, mdlSPM;
        private static FVModel mdlTrigger, mdlNote, mdlNoteInside, mdlTriggerBorder;
        private static Texture2D[] texNotes, texBarNotes, texTriggers, texTriggersLit;
        private static Texture2D texTriggerBorder, texTriggerBorderLit;
        private static Texture2D texLine, texLineEnd, breEndTex, breMiddleTex;
        private static Texture2D texGlow, texRPSheen, texSoloPercentBG;
        private static Texture2D[] texMult, texLightning;
        public  static Color[] DefaultFretColors = { 
                                                        new Color(0f,1f,0f), //green
                                                        new Color(1f,0f,0f), //red
                                                        new Color(1f,1f,0f), //yellow
                                                        new Color(0f,0f,1f), //blue
                                                        new Color(1f,0.5f,0f), //orange
                                                   };

        // texture for board paths
        private static String[] boardBGs;

        // const values
        private const float BOARD_FADE_RATIO = 7 / 8f;
        private const float BOARD_BUMP_COEF = 0.002f;
        private const float RP_METER_Y_SCALE = 0.4f;
        private const int ScorePerNote = 100;
        private const float PILLOW = 0.1f;//padding in front of and behind note

        // rock power global multiplier
        public static int RPMultiplier { get; private set; }

        // generic info
        private Instrument instrumentType;
        private Peripheral controller;
        private Results myResults;

        // gfx
        // what is used to draw onscreen
        private RenderTarget2D rtBoard, rtWaves;
        // the current background
        private Texture2D boardBackground;
        // the final render
        private RenderTarget2D boardTarget;
        // the particle handler
        private ParticleMaster myParticles;

        // where the board is drawn after being rendered
        private int xOffset;
        // the rotate of the flashes so they dont get stale
        private float flashRot;
        // which difficulty is being played
        private Difficulty difficulty;
        // the multiplier before RP x2
        private float multiplier;
        // a float for more accurate wave addition
        private float score;
        public int Score { get { return (int)(score); } }
        // for fret boards popping up on strum/bang
        private float[] popup, popupSpeed;
        // RockPowerAmount is the actual value, RockPowerDisplayAmount follows for "filling up" and "draining"
        private float RockPowerAmount = 0, RockPowerDisplayAmount = 0;
        // pretty self explanatory...
        public bool RockPowerActivated { get; private set; }
        // the values relative to currenttime
        private List<Wave> whammyValues;
        // this is just pushed along so the sin waves moves
        private double sinwavestart=0;
        // level 0-1
        public float RockMeterLevel { get; private set; }
        // if its 3, they can't be saved
        private byte numFails;
        // the next note
        private int currentNoteIndex;
        // the position of the sheen as it travels along the board (0-1)
        private float rpSheen;
        // the streak the user has running now
        private int streak;
        // buffers the fret lights so they can fade for drums instead
        // of being on for one frame
        private float[] fretLights;
        // the solo percent sign alpha
        private float soloPercentAlpha;
        // the lit up values of the BRE lanes
        private float[] breFades;
        // set to true once we start the BRE. any notes after this cancel BRE score
        private bool breStarted;
        // if true, breScore is not added
        private bool breFailed;
        // a float for more accurate wave addition
        private float breScore;
        public int BREScore { get { return (int)(breScore); } }

        private Queue<SoloEndMessage> soloEndMessages;

        public String InstrumentCode { get { return instrumentType.CodeName; } }

        // our reference to the songdata
        private SongData songData;

        private GameNote[] _notes;
        private GameNote[] Notes
        {
            get
            {
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
                    for (int i = 0; i < songData.instruments.Length; i++)
                        if (songData.instruments[i].instrumentType.Equals(GetBoardType().CodeName))
                        {
                            _phrases = songData.instruments[i].diffSets[(int)difficulty].phrases;
                            return _phrases;
                        }
                    return null;
                }
                return _phrases;
            }
        }

        private GameFill[] _fills;
        private GameFill[] Fills
        {
            get
            {
                return _fills;
            }
        }

        private GameRockPowerPhrase[] _rpphrases;
        private GameRockPowerPhrase[] RPPhrases
        {
            get
            {
                return _rpphrases;
            }
        }

        private GameSolo[] _solos;
        private GameSolo[] Solos
        {
            get
            {
                return _solos;
            }
        }

        public Peripheral Peripheral
        {
            get { return controller; }
            set { controller = value; }
        }

        private uint[] _starPoints;
        private uint[] StarPoints
        {
            get
            {
                return _starPoints;
            }
        }

#region Accessors

        public bool IsFailing
        {
            get { return RockMeterLevel<=0; }
        }

        public bool IsLefty
        {
            get { return controller.LeftySwitch; }
        }

#endregion

        public static void Load(ContentManager Content)
        {
            Random r = Global.Random;

            faderEffect = Content.Load<Effect>("shaders\\BoardFade");
            effect = new FVShader(Global.Graphics.GraphicsDevice, Content.Load<Effect>("shaders\\UnsignedEngineShader"), "maintechnique");

            spriteBatch = new SpriteBatch(Global.Graphics.GraphicsDevice);

            texGlow = Content.Load<Texture2D>("textures\\Game\\triggerglow");
            texLine = Content.Load<Texture2D>("textures\\Game\\line");
            texLineEnd = Content.Load<Texture2D>("textures\\Game\\linetaper");
            texTriggerBorder = Content.Load<Texture2D>("textures\\Game\\triggerborder");
            drumfillTex = Content.Load<Texture2D>("textures\\Game\\drumfill");
            spMeterBG = Content.Load<Texture2D>("textures\\Game\\boardmeter");
            spMeterFill = Content.Load<Texture2D>("textures\\Game\\rpMeterFill");
            spMeterLED = Content.Load<Texture2D>("textures\\Game\\bulb");
            spMeterCurl = Content.Load<Texture2D>("textures\\Game\\curl");
            texTriggerBorderLit = Content.Load<Texture2D>("textures\\Game\\triggerborderlit");
            texBlast = Content.Load<Texture2D>("textures\\Game\\blast");
            texNotesWhite = Content.Load<Texture2D>("textures\\Game\\NotesWhite");
            texBoardBorderFade = Content.Load<Texture2D>("textures\\Game\\boardFade");
            texRPSheen = Content.Load<Texture2D>("textures\\Game\\rpSheen");
            texSoloPercentBG = Content.Load<Texture2D>("textures\\Game\\solopercentbg");
            breEndTex = Content.Load<Texture2D>("textures\\Game\\breend");
            breMiddleTex = Content.Load<Texture2D>("textures\\Game\\bremiddle");
            texLightning = new Texture2D[3];
            for (int i = 0; i < texLightning.Length; i++)
                texLightning[i] = Content.Load<Texture2D>("textures\\Game\\lightning" + (i + 1));
            int maxColors = 5;
            texTriggers = new Texture2D[maxColors];
            for (int i = 0; i < maxColors; i++)
                texTriggers[i] = Content.Load<Texture2D>("textures\\Game\\trigger" + i);
            texTriggersLit = new Texture2D[maxColors];
            for (int i = 0; i < maxColors; i++)
                texTriggersLit[i] = Content.Load<Texture2D>("textures\\Game\\triggerlit" + i);
            texNotes = new Texture2D[maxColors];
            for (int i = 0; i < maxColors; i++)
                texNotes[i] = Content.Load<Texture2D>("textures\\Game\\Notes" + i);
            texBarNotes = new Texture2D[maxColors];
            for (int i = 0; i < maxColors; i++)
                texBarNotes[i] = Content.Load<Texture2D>("textures\\Game\\BarNote" + i);
            vBar = Content.Load<Texture2D>("textures\\Game\\vocalbar");
            vBGExt = Content.Load<Texture2D>("textures\\Game\\vocalbg_ext");
            vBGInt = Content.Load<Texture2D>("textures\\Game\\vocalbg_int");
            vFuzz = Content.Load<Texture2D>("textures\\Game\\vocalfuzz");
            vHeadBar = Content.Load<Texture2D>("textures\\Game\\vocalheadbar");
            vGlow = Content.Load<Texture2D>("textures\\Game\\vGlow");
            List<String> texBGs = new List<String>();
            if (!System.IO.Directory.Exists("Content\\boards\\"))
                System.IO.Directory.CreateDirectory("Content\\boards\\");
            String[] files = System.IO.Directory.GetFiles("Content\\boards\\");
            for (int i = 0; i < files.Length; i++)
            {
                if (files[i].ToLower().EndsWith(".png") || files[i].ToLower().EndsWith(".bmp") || files[i].ToLower().EndsWith(".jpg"))
                    texBGs.Add(files[i]);
            }
            boardBGs = texBGs.ToArray();
            texMult = new Texture2D[8];
            for (int i = 0; i < texMult.Length; i++)
                texMult[i] = Content.Load<Texture2D>("textures\\Game\\X" + i);
            mdlBoard = ModelLoader.LoadModel("meshes\\Game\\board");
            mdlBoardBorder = ModelLoader.LoadModel("meshes\\Game\\boardborder");
            mdlSPM = ModelLoader.LoadModel("meshes\\Game\\board");
            mdlNote = ModelLoader.LoadModel("meshes\\Game\\note");
            mdlNoteInside = ModelLoader.LoadModel("meshes\\Game\\noteinside");
            mdlTrigger = ModelLoader.LoadModel("meshes\\Game\\triggerdown");
            mdlTriggerBorder = ModelLoader.LoadModel("meshes\\Game\\bassline");
        }

        public static void UpdateRPMultiplier(Board[] boards)
        {
            RPMultiplier = 1;
            for (int i = 0; i < boards.Length; i++)
            {
                if (boards[i].RockPowerActivated)
                    RPMultiplier *= 2;
            }
        }

        public Board(Instrument type, int xOffset, SongData song, Difficulty difficulty)
        {
            this.songData = song;
            this.instrumentType = type;
            this.xOffset = xOffset;
            this.difficulty = difficulty;

            GenerateNoteArray();

            myResults.instr = type;
            myResults.totalNotes = Notes.Length;
            myResults.hitNotes = 0;
            myResults.missedNotes = 0;
            myResults.totalSPPH = RPPhrases.Length;
            myResults.percentSong = 1.0f;
            myResults.hitSPPH = 0;
            myResults.missedSPPH = 0;
            score = 0;
            popup = new float[type.NumTracks];
            popupSpeed = new float[type.NumTracks];
            if (type.CanWhammy)
            {
                whammyValues = new List<Wave>();
            }
            RockMeterLevel = 0.8f;

            fretLights = new float[instrumentType.NumDrawnTracks];

            flashRot = 0;

            breFades = new float[instrumentType.NumTracks];

            curveHeight = 0.03f;
            height = -2.0f;
            Length = 6f;
            width = 0.7f;
            rotate = .5f;
            sFade = 1.4f;
            eFade = 1.8f;
            LengthRPRatio = 1f / 8f;

            RockPowerDisplayAmount = 0.0f;

            RockPowerAmount = 0.0f;

            rpSheen = 0;

            soloEndMessages = new Queue<SoloEndMessage>();
        }

        public Instrument GetBoardType()
        {
            return instrumentType;
        }

        public int GetXOffset()
        {
            return xOffset;
        }

        public void LoadInstance(ContentManager Content)
        {
            if (Configuration.HalfRender)
            {
                rtBoard = new RenderTarget2D(Global.Graphics.GraphicsDevice, Global.ScreenHeight / 4, Global.ScreenHeight / 2, 1, SurfaceFormat.Color);
                rtWaves = new RenderTarget2D(Global.Graphics.GraphicsDevice, Global.ScreenHeight / 4, Global.ScreenHeight / 2, 1, SurfaceFormat.Color);
            }
            else
            {
                rtBoard = new RenderTarget2D(Global.Graphics.GraphicsDevice, Global.ScreenHeight / 2, Global.ScreenHeight, 1, SurfaceFormat.Color);
                rtWaves = new RenderTarget2D(Global.Graphics.GraphicsDevice, Global.ScreenHeight / 2, Global.ScreenHeight, 1, SurfaceFormat.Color);
            }
            boardTarget = new RenderTarget2D(Global.Graphics.GraphicsDevice, Global.ScreenWidth, Global.ScreenHeight, 1, SurfaceFormat.Color);

            if (boardBGs.Length <= 0)
                boardBackground = Content.Load<Texture2D>("textures\\global\\black");
            else
                boardBackground = Texture2D.FromFile(Global.Graphics.GraphicsDevice, boardBGs[Global.Random.Next(boardBGs.Length)]); 
            myParticles = new ParticleMaster(this);
        }

        public void Update(SongTime songTime)
        {
            ulong pressed = controller.GetFrets();
            ulong newPressed = controller.GetBufferedFrets();

            for (int i = 0; i < fretLights.Length; i++)
                if (fretLights[i] > 0)
                    fretLights[i] -= (float)songTime.ElapsedGameTime.TotalSeconds * 10;
            if (!instrumentType.NeedsStrum)
                for (int i = 0; i < instrumentType.NumDrawnTracks; i++)
                    if ((newPressed & ((ulong)1 << i)) != 0)
                        fretLights[i] = 1.0f;

            if ((GetBoardType().RPEnableType & Instrument.RockPowerEnableTypes.FILL) != 0)
            {
                for (int i = 0; i < Fills.Length; i++)
                {
                    if (Fills[i].Start >= songTime.TotalSongTime.TotalSeconds && Fills[i].Start <= songTime.TotalSongTime.TotalSeconds + eFade)
                    {
                        if (RockPowerAmount < 0.49999f || RockPowerActivated)
                            Fills[i].Hide();
                        else
                        {
                            for (int n = 0; n < Notes.Length; n++)
                                if (Notes[n].Start >= Fills[i].Start && Notes[n].Start <= Fills[i].HideNotesEnd)
                                    Notes[n].Hide();
                        }
                    }
                }
            }

            for (int i = 0; i < RPPhrases.Length; i++)
            {
                if (RPPhrases[i].Start > songTime.TotalSongTime.TotalSeconds)
                    break;
                if (RPPhrases[i].End < songTime.TotalSongTime.TotalSeconds)
                    if (!RPPhrases[i].Used && RPPhrases[i].Okay)
                    {
                        RockPowerAmount += 0.25f;
                        RPPhrases[i].Use();
                        myResults.hitSPPH++;
                    }
            }

            bool strummed = Peripheral.WasPressed(PeripheralButton.UP) || Peripheral.WasPressed(PeripheralButton.DOWN);

            if (rpSheen < 1.5f && RockPowerActivated)
                rpSheen += (float)songTime.ElapsedGameTime.TotalSeconds;
            
            if (RockPowerActivated)
            {
                RockPowerAmount -= GetBeatsPerMinute(songTime) * (float)songTime.ElapsedGameTime.TotalSeconds * 0.0005f;
                if (RockPowerAmount <= 0)
                {
                    RockPowerAmount = 0;
                    RockPowerActivated = false;
                }
            }

            if (songData.info.bre.enabled && songTime.TotalSongTime.TotalSeconds >= songData.info.bre.start/1000f && songTime.TotalSongTime.TotalSeconds <= songData.info.bre.end/1000f)
            {
                for(int i=0;i<instrumentType.NumTracks;i++)
                    if (breFades[i] > 0)
                    {
                        breFades[i] -= (float)songTime.ElapsedGameTime.TotalSeconds;
                        if (breFades[i] < 0)
                            breFades[i] = 0;
                    }
                breStarted = true;
                if (instrumentType.NeedsStrum)
                {
                    if (strummed)
                    {
                        for (int i = 0; i < instrumentType.NumTracks; i++)
                        {
                            if ((pressed & ((ulong)1 << i)) != 0)
                            {
                                breScore += (1.5f - breFades[i]) * 100;
                                myParticles.AddSparks(i, 4, 1.0f, false);
                                breFades[i] = 1.5f;
                                popup[i] = 1;
                            }
                        }
                        flashRot = (float)Global.Random.NextDouble() * MathHelper.TwoPi;
                    }
                }
            }
            else
            {

                if (RockPowerAmount >= 0.49999f)
                    if (!RockPowerActivated)
                        if (Peripheral.IsPressed(PeripheralButton.SELECT))
                        {
                            RockPowerActivated = true;
                            rpSheen = 0;
                        }

                Whammy(Peripheral.GetAnalogValue(PeripheralAnalog.WHAMMY_BAR), songTime);

                //pass up notes
                if (currentNoteIndex < Notes.Length && Notes[currentNoteIndex].Burning)
                {
                    if (Notes[currentNoteIndex].End < songTime.TotalSongTime.TotalSeconds || strummed)
                        currentNoteIndex++;
                }
                else
                {
                    while (currentNoteIndex < Notes.Length && ((Notes[currentNoteIndex].Start) - (float)songTime.TotalSongTime.TotalSeconds) < -PILLOW)
                    {
                        ResetMultiplier();
                        score += ScorePerNote * Notes[currentNoteIndex].Kill() * GetScoreMultiplier();
                        FailRockPowerPhraseForNote(Notes[currentNoteIndex]);
                        for (int i = 0; i < Notes[currentNoteIndex].NumNotes; i++)
                            Hurt();
                        currentNoteIndex++;
                        myResults.missedNotes++;
                        if (streak > myResults.streak)
                            myResults.streak = streak;
                        if (breStarted)
                            breFailed = true;
                        streak = 0;
                    }
                }
                if (currentNoteIndex >= Notes.Length || !Notes[currentNoteIndex].Burning)
                    if (currentNoteIndex + 1 < Notes.Length && (Math.Abs(Notes[currentNoteIndex].Start - (float)songTime.TotalSongTime.TotalSeconds) > Math.Abs(Notes[currentNoteIndex + 1].Start - (float)songTime.TotalSongTime.TotalSeconds)))
                    {
                        ResetMultiplier();
                        score += ScorePerNote * Notes[currentNoteIndex].Kill() * GetScoreMultiplier();
                        FailRockPowerPhraseForNote(Notes[currentNoteIndex]);
                        for (int i = 0; i < Notes[currentNoteIndex].NumNotes; i++)
                            Hurt();
                        currentNoteIndex++;
                        myResults.missedNotes++;
                        if (streak > myResults.streak)
                            myResults.streak = streak;
                        if (breStarted)
                            breFailed = true;
                        streak = 0;
                    }

                while (currentNoteIndex < Notes.Length && Notes[currentNoteIndex].IsHidden)
                {
                    currentNoteIndex++;
                }

                if (currentNoteIndex < Notes.Length)
                {
                    int currentFillIndex = -1;
                    if ((GetBoardType().RPEnableType & Instrument.RockPowerEnableTypes.FILL) != 0)
                        for (int i = 0; i < Fills.Length; i++)
                            if (Fills[i].Visible)
                                if (songTime.TotalSongTime.TotalSeconds >= Fills[i].Start && songTime.TotalSongTime.TotalSeconds <= Fills[i].End)
                                    currentFillIndex = i;
                    if (currentFillIndex >= 0)
                    {
                        for (int i = 0; i < GetBoardType().NumTracks; i++)
                            if ((newPressed & ((ulong)1 << i)) != 0)
                            {
                                Fills[currentFillIndex].Hit();
                                myParticles.AddSparks(i, 4, false);
                                popup[i] = 1;
                                flashRot = (float)Global.Random.NextDouble() * MathHelper.TwoPi;
                            }
                        if ((newPressed & ((ulong)8)) != 0)
                            if (Math.Abs(songTime.TotalSongTime.TotalSeconds - Fills[currentFillIndex].GreenNotePos) < PILLOW)
                            {
                                if (Fills[currentFillIndex].Use())
                                {
                                    RockPowerActivated = true;
                                    rpSheen = 0;
                                }
                            }
                    }
                    else
                    {
                        float diff = (Notes[currentNoteIndex].Start) - (float)songTime.TotalSongTime.TotalSeconds;
                        if (Math.Abs(diff) < PILLOW)
                        {
                            if (instrumentType.NeedsStrum)
                            {
                                if (strummed)
                                {
                                    if (!Notes[currentNoteIndex].Strummed)
                                        Notes[currentNoteIndex].Strum();
                                    else
                                    {
                                        ResetMultiplier();
                                        FailRockPowerPhraseForNow(songTime);
                                        Hurt();
                                        if (streak > myResults.streak)
                                            myResults.streak = streak;
                                        streak = 0;
                                        if (breStarted)
                                            breFailed = true;
                                    }
                                }
                                Notes[currentNoteIndex].AddHeld(pressed);
                            }
                            else
                            {
                                bool rp = false;
                                for (int i = 0; i < RPPhrases.Length; i++)
                                    if (RPPhrases[i].Okay && Notes[currentNoteIndex].Start >= RPPhrases[i].Start && Notes[currentNoteIndex].Start <= RPPhrases[i].End)
                                        rp = true;
                                ulong hit = Notes[currentNoteIndex].AddPressed(newPressed);
                                for (int i = 0; i < instrumentType.NumTracks; i++)
                                    if ((((ulong)1 << i) & newPressed) != 0 && (((ulong)1 << i) & hit) == 0)
                                    {
                                        ResetMultiplier();
                                        FailRockPowerPhraseForNow(songTime);
                                        Hurt();
                                        if (streak > myResults.streak)
                                            myResults.streak = streak;
                                        streak = 0;
                                        if (breStarted)
                                            breFailed = true;
                                    }
                                for (int i = 0; i < instrumentType.NumTracks; i++)
                                    if ((((ulong)1 << i) & hit) != 0)
                                    {
                                        popup[i] = 1;
                                        myParticles.AddSparks(i, 2, rp ? 0.5f : 0.25f, rp);
                                        myParticles.AddShards(i, 5, rp);
                                        IncreaseMultiplier();
                                        Help();
                                    }
                            }
                            if (!Notes[currentNoteIndex].Burning && Notes[currentNoteIndex].IsGood(currentNoteIndex == 0 ? false : Notes[currentNoteIndex - 1].IsDead))
                            {
                                score += ScorePerNote * Notes[currentNoteIndex].Kill() * GetScoreMultiplier();
                                myResults.hitNotes++;
                                streak++;
                                bool rp = false;
                                for (int i = 0; i < RPPhrases.Length; i++)
                                    if (RPPhrases[i].Okay && Notes[currentNoteIndex].Start >= RPPhrases[i].Start && Notes[currentNoteIndex].Start <= RPPhrases[i].End)
                                        rp = true;
                                if (GetBoardType().NeedsStrum)
                                    for (int i = 0; i < instrumentType.NumDrawnTracks; i++)
                                        if (Notes[currentNoteIndex].HasFret(i))
                                        {
                                            popup[i] = 1;
                                            myParticles.AddSparks(i, 5, rp ? 0.7f : 0.5f, rp);
                                            myParticles.AddShards(i, 6, rp);
                                        }
                                if (GetBoardType().HasSolos)
                                    for (int i = 0; i < Solos.Length; i++)
                                        if (Solos[i].Visible && Notes[currentNoteIndex].Start >= Solos[i].Start && Notes[currentNoteIndex].Start <= Solos[i].End)
                                            Solos[i].HitNotes++;
                                flashRot = (float)Global.Random.NextDouble() * MathHelper.TwoPi;
                                if (GetBoardType().NeedsStrum)
                                {
                                    IncreaseMultiplier();
                                    for (int i = 0; i < Notes[currentNoteIndex].NumNotes; i++)
                                        Help();
                                }
                                if (Notes[currentNoteIndex].Length > 0)
                                    Notes[currentNoteIndex].Burning = true;
                                else
                                    currentNoteIndex++;
                            }
                        }
                        else
                        {
                            if (instrumentType.NeedsStrum && strummed)
                            {
                                FailRockPowerPhraseForNow(songTime);
                                ResetMultiplier();
                                Hurt();
                                if (streak > myResults.streak)
                                    myResults.streak = streak;
                                streak = 0;
                                if (breStarted)
                                    breFailed = true;
                            }
                            if (!instrumentType.NeedsStrum && newPressed != 0)
                            {
                                for (int i = 0; i < instrumentType.NumTracks; i++)
                                    if ((((ulong)1 << i) & newPressed) != 0)
                                    {
                                        ResetMultiplier();
                                        FailRockPowerPhraseForNow(songTime);
                                        Hurt();
                                        if (streak > myResults.streak)
                                            myResults.streak = streak;
                                        streak = 0;
                                        if (breStarted)
                                            breFailed = true;
                                    }
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < instrumentType.NumDrawnTracks; i++)
            {
                if (popup[i] > 0)
                {
                    popup[i] -= (float)songTime.ElapsedGameTime.TotalSeconds * 20f;
                    if (popup[i] < 0)
                        popup[i] = 0;
                }
            }

            myParticles.Update(songTime);

            if (RockPowerAmount < 0)
                RockPowerAmount = 0;
            else if (RockPowerAmount > 1)
                RockPowerAmount = 1;
            if (Math.Abs(RockPowerAmount - RockPowerDisplayAmount) > 1.0f)
            {
                RockPowerDisplayAmount = RockPowerAmount;
            }
            else if (Math.Abs(RockPowerAmount - RockPowerDisplayAmount) > 0.001f)
            {
                RockPowerDisplayAmount = (RockPowerAmount * (float)(songTime.ElapsedGameTime.TotalSeconds*10)) + (RockPowerDisplayAmount * (float)(1-(songTime.ElapsedGameTime.TotalSeconds*10)));
            }
            else
                RockPowerDisplayAmount = RockPowerAmount;

            if (RockMeterLevel > 1.0f)
                RockMeterLevel = 1.0f;
            if (RockMeterLevel < 0.0f)
                RockMeterLevel = 0.0f;

            if (multiplier < 1)
                multiplier = 1;
            if (multiplier > instrumentType.MaxMultiplier)
                multiplier = instrumentType.MaxMultiplier;

            bool inSolo = false;
            if (GetBoardType().HasSolos)
            {
                for (int i = 0; i < Solos.Length; i++)
                {
                    if (songTime.TotalSongTime.TotalSeconds >= Solos[i].Start && songTime.TotalSongTime.TotalSeconds <= Solos[i].End-0.5f)
                    {
                        if (Solos[i].Visible)
                        {
                            inSolo = true;
                        }
                    }
                    else if (songTime.TotalSongTime.TotalSeconds >= Solos[i].End && !Solos[i].Used)
                    {
                        // http://rockband.scorehero.com/forum/viewtopic.php?t=3238

                        // n = number of notes in the solo. Hold points mean nothing. Chords count as single notes. 

                        // Perfect Solo: 100%, 100n points 
                        // Awesome Solo: 95%-99%, 50n points (Obviously, there's a huge cut for one missed note. Imagine the bonus on a GGaHT FC...) 
                        // Great Solo: 90% - 94%, 30n points 
                        // Good Solo: 80% - 89%, 20n points 
                        // Solid Solo: 70% - 79%, 10n points 
                        // Okay Solo: 60% - 69%, 5n points 
                        // Messy Solo: 0% - 59%, 0 points

                        Solos[i].Use();
                        int bonus = 0;
                        int pc = Solos[i].Percentage;
                        if (pc >= 100)
                        {
                            soloEndMessages.Enqueue(new SoloEndMessage(Localizer.Get("Perfect Solo")));
                            bonus = 100 * Solos[i].NumNotes;
                        }
                        else if (pc >= 95)
                        {
                            soloEndMessages.Enqueue(new SoloEndMessage(Localizer.Get("Awesome Solo")));
                            bonus = 50 * Solos[i].NumNotes;
                        }
                        else if (pc >= 90)
                        {
                            soloEndMessages.Enqueue(new SoloEndMessage(Localizer.Get("Great Solo")));
                            bonus = 30 * Solos[i].NumNotes;
                        }
                        else if (pc >= 80)
                        {
                            soloEndMessages.Enqueue(new SoloEndMessage(Localizer.Get("Good Solo")));
                            bonus = 20 * Solos[i].NumNotes;
                        }
                        else if (pc >= 70)
                        {
                            soloEndMessages.Enqueue(new SoloEndMessage(Localizer.Get("Solid Solo")));
                            bonus = 10 * Solos[i].NumNotes;
                        }
                        else if (pc >= 60)
                        {
                            soloEndMessages.Enqueue(new SoloEndMessage(Localizer.Get("Okay Solo")));
                            bonus = 5 * Solos[i].NumNotes;
                        }
                        else
                            soloEndMessages.Enqueue(new SoloEndMessage(Localizer.Get("Poor Solo")));

                        soloEndMessages.Enqueue(new SoloEndMessage("Bonus: " + bonus));
                        score += bonus;
                    }
                }
            }
            if (inSolo && soloPercentAlpha < 1)
            {
                soloPercentAlpha += (float)songTime.ElapsedGameTime.TotalSeconds * 2;
                if (soloPercentAlpha > 1)
                    soloPercentAlpha = 1;
            }
            if (!inSolo && soloPercentAlpha > 0)
            {
                soloPercentAlpha -= (float)songTime.ElapsedGameTime.TotalSeconds * 2;
                if (soloPercentAlpha < 0)
                    soloPercentAlpha = 0;
            }

            if (soloEndMessages.Count > 0)
            {
                soloEndMessages.Peek().Update(songTime);
                if (soloEndMessages.Peek().ShouldDestroy)
                    soloEndMessages.Dequeue();
            }
        }

        public void Draw(SongTime songTime)
        {
            RenderTarget2D oldRT = (RenderTarget2D)Global.Graphics.GraphicsDevice.GetRenderTarget(0);
            if (GetBoardType().Dimensions == Instrument.BoardDimensions.TWO_DIMENSIONAL)
            {
                long currenttime = (long)(songTime.ElapsedGameTime.TotalSeconds * 1000);
                Vector2 center = new Vector2(vFuzz.Width / 2, vFuzz.Height / 2);
                float vScale = 0.2f;
                float height = vFuzz.Height * vScale;
                Color glow = new Color(150, 255, 150, 255);
                spriteBatch.Draw(vBGInt, new Rectangle(0, (int)vocaly, 1024, (int)vocalheight), Color.White);
                for (int i = currentNoteIndex; i < Phrases.Length; i++)
                {
                    spriteBatch.Draw(vBar, new Rectangle((int)(vocalzerox + (vocalwidth * (Phrases[i].time - (long)currenttime))), (int)vocaly, 8, (int)vocalheight), Color.White);

                    for (int k = 0; k < Phrases[i].notes.Length; k++)
                    {
                        if (Phrases[i].notes[k].type < 0)
                            continue;
                        float minx = (vocalzerox + (vocalwidth * (Phrases[i].notes[k].time - currenttime))), maxx = (vocalzerox + (vocalwidth * ((Phrases[i].notes[k].length) - currenttime))), y = (1 - ((Phrases[i].notes[k].type % 12) / 12f)) * vocalheight * .69f + vocaly;
                        spriteBatch.Draw(vGlow, new Rectangle((int)minx, (int)(y - height / 2), (int)Math.Min(128 * vScale, (maxx - minx) * vScale), (int)(height)), new Rectangle(0, 0, 128, 256), glow);
                        spriteBatch.Draw(vGlow, new Rectangle((int)(minx + Math.Min(128 * vScale, (maxx - minx) * vScale)), (int)(y - height / 2), (int)((maxx - minx) - (Math.Min(128 * vScale, (maxx - minx) * vScale) * 2)), (int)(height)), new Rectangle(128, 0, 128, 256), glow);
                        spriteBatch.Draw(vGlow, new Rectangle((int)(maxx - Math.Min(128 * vScale, (maxx - minx) * vScale)), (int)(y - height / 2), (int)Math.Min(128 * vScale, (maxx - minx) * vScale), (int)(height)), new Rectangle(128, 0, -128, 256), glow);
                    }
                }

                spriteBatch.Draw(vBGExt, new Rectangle(0, (int)vocaly, 1024, (int)vocalheight), Color.White);

                for (int i = currentNoteIndex; i < Phrases.Length; i++)
                    for (int k = 0; k < Phrases[i].notes.Length; k++)
                        spriteBatch.DrawString(Global.DefaultFont, Phrases[i].notes[k].text, new Vector2((vocalzerox + (vocalwidth * (Phrases[i].notes[k].time - currenttime))), vocaly + (0.75f * vocalheight)), Color.White);
                spriteBatch.Draw(vHeadBar, new Rectangle((int)vocalzerox, (int)vocaly, 8, (int)vocalheight), Color.White);
            }
            else //if (GetBoardType().Dimensions == Instrument.BoardDimensions.THREE_DIMENSIONAL)
            {
                DrawBoardTarget(songTime);

                Global.Graphics.GraphicsDevice.SetRenderTarget(0, boardTarget);
                Global.Graphics.GraphicsDevice.Clear(new Color(new Vector4(0, 0, 0, 0)));

                Global.Graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;
                Global.Graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
                Global.Graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
                Global.Graphics.ApplyChanges();

                effect.View = Matrix.CreateLookAt(new Vector3(0, 2f, -2.1f), new Vector3(0, 0, 1.5f), Vector3.Up);
                effect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, boardTarget.Width / (float)boardTarget.Height, 0.1f, 100.0f);

                effect.AmbientMaterial = Color.White;

                effect.LightingEnabled = Configuration.Lighting;
                effect.SpecularEnabled = Configuration.Specular;
                effect.NormalMapEnabled = Configuration.NormalMapping;

                effect.CommitChanges();

                effect.Begin();
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Begin();

                    DrawBoard(songTime);
                    //if (!IsFailing)
                    {
                        Global.Graphics.GraphicsDevice.RenderState.CullMode = CullMode.CullClockwiseFace;
                        DrawNotes(songTime);
                        Global.Graphics.GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
                        DrawNotes(songTime);
                    }
                    DrawBoardDetail();
                    if (!IsFailing)
                        DrawWaves();
                    DrawFlashes();
                    myParticles.Render(effect);
                    pass.End();
                }
                effect.End();
#if DEBUG
                spriteBatch.Begin();
                //spriteBatch.DrawString(Global.DefaultFont, "" + ((int)(RockMeterLevel*10000)/100f), new Vector2(20, 120), Color.Red);
                for (int i = 0; i < Solos.Length; i++)
                {
                    if (songTime.TotalSongTime.TotalSeconds >= Solos[i].Start && songTime.TotalSongTime.TotalSeconds <= Solos[i].End)
                    {
                        if (Solos[i].Visible)
                        {
                            float scale = -(((soloPercentAlpha * 1.2f) - 1) * ((soloPercentAlpha * 1.2f) - 1)) + 1;
                            spriteBatch.Draw(texSoloPercentBG, new Vector2(Global.ScreenWidth * 0.5f, Global.ScreenHeight * 0.3f), null, new Color(0f,0f,0.5f,0.5f*scale), 0, new Vector2(texSoloPercentBG.Width / 2, texSoloPercentBG.Height/2), Global.ScreenHeight / 1600f * scale, SpriteEffects.None, 0);
                            String str = Solos[i].Percentage + "%";
                            Vector2 meas = Global.DefaultFont.MeasureString(str);
                            spriteBatch.DrawString(Global.DefaultFont, str, new Vector2(Global.ScreenWidth * 0.5f, Global.ScreenHeight * 0.3f), new Color(Color.White,scale), 0, meas * 0.5f, Global.ScreenHeight / 500f * scale, SpriteEffects.None, 0); 
                        }
                    }
                }
                if (soloEndMessages.Count > 0)
                {
                    String str = soloEndMessages.Peek().Message;
                    Vector2 meas = Global.DefaultFont.MeasureString(str);
                    spriteBatch.DrawString(Global.DefaultFont, str, new Vector2(Global.ScreenWidth * 0.5f, Global.ScreenHeight * 0.3f), new Color(Color.White,soloEndMessages.Peek().Alpha), 0, meas * 0.5f, Global.ScreenHeight / 400f * soloEndMessages.Peek().Alpha, SpriteEffects.None, 0);
                }
                spriteBatch.End();
#endif
            }
            Global.Graphics.GraphicsDevice.SetRenderTarget(0, oldRT);
        }

        public Texture2D GetRender()
        {
            return boardTarget.GetTexture();
        }

        public Results GetResults()
        {
            if (streak > myResults.streak)
                myResults.streak = streak;
            return myResults;
        }

        public float GetNumStars()
        {
            int i;
            for (i = StarPoints.Length-1; i > 0; i--)
                if (score >= StarPoints[i])
                    break;
            if (score >= StarPoints[StarPoints.Length - 1])
                return 6;
            if (score < StarPoints[0])
                return score / (float)StarPoints[0];
            float dec = (score - StarPoints[i]) / (float)(StarPoints[i + 1] - StarPoints[i]);
            return dec + i + 1;
        }

        public void Reset()
        {
            currentNoteIndex = 0;
            for (int i = 0; i < Notes.Length; i++)
            {
                Notes[i].Reset();
            }
            score = 0;
            RockMeterLevel = 0.8f;
            multiplier = 1;

        }



        private float GetBeatTime(SongTime songTime)
        {
            float currentTime = (float)songTime.TotalSongTime.TotalSeconds;
            for (int i = 0; i < songData.info.barlines.Length-1; i++)
            {
                if (currentTime * 1000 < songData.info.barlines[i].time)
                    continue;
                if (currentTime * 1000 > songData.info.barlines[i + 1].time)
                    continue;
                float start = songData.info.barlines[i].time / 1000f;
                float end = songData.info.barlines[i+1].time / 1000f;
                float val = (currentTime - start) / (end - start);
                val *= songData.info.barlines[i].numBeats;
                val %= 1.0f;
                return val;
            }
            return 0;
        }

        private float GetBeatsPerMinute(SongTime songTime)
        {
            float currentTime = (float)songTime.TotalSongTime.TotalSeconds;
            for (int i = 0; i < songData.info.barlines.Length - 1; i++)
            {
                if (currentTime * 1000 < songData.info.barlines[i].time)
                    continue;
                if (currentTime * 1000 > songData.info.barlines[i + 1].time)
                    continue;
                float start = songData.info.barlines[i].time / 1000f;
                float end = songData.info.barlines[i + 1].time / 1000f;
                float length = (end - start) / songData.info.barlines[i].numBeats;
                return 60f / length;
            }
            return 0;
        }

        private int GetScoreMultiplier()
        {
            return (int)multiplier * RPMultiplier;
        }

        private void IncreaseMultiplier()
        {
            multiplier += 0.1f;
            if (multiplier >= instrumentType.MaxMultiplier)
                multiplier = instrumentType.MaxMultiplier;
        }

        private void ResetMultiplier()
        {
            multiplier = 1f;
        }

        private int GetMultiplierFraction()
        {
            if (multiplier >= GetBoardType().MaxMultiplier)
                return 10;
            int i= (int)((multiplier % 1) * 10);
            if (i == 0 && multiplier > 1)
                i = 10;
            return i;
        }

        private int GetMultiplier()
        {
            return (int)multiplier;
        }

        private bool IsSPActivated()
        {
            return RockPowerActivated;
        }

        private void Whammy(float whammyAmount, SongTime songTime)
        {
            if (currentNoteIndex >= Notes.Length)
                return;
            if (!instrumentType.CanWhammy)
                return;
            float bpm = GetBeatsPerMinute(songTime);
            float currenttime = (float)(songTime.TotalSongTime.TotalSeconds);
            if (Notes[currentNoteIndex].Burning && Notes[currentNoteIndex].Start < currenttime && Notes[currentNoteIndex].End > currenttime)
            {
                float diff = 0;
                if (whammyValues.Count <= 0 || whammyValues[0].Value != whammyAmount)
                {
                    if(whammyValues.Count>0)
                        diff = Math.Abs(whammyValues[0].Value - whammyAmount);
                    whammyValues.Insert(0, new Wave(whammyAmount, 0));
                    if (Global.Random.Next(10) == 0)
                        for (int i = 0; i < GetBoardType().NumDrawnTracks; i++)
                            if (Notes[currentNoteIndex].HasFret(i))
                                myParticles.AddSparks(i, 1, false);
                }
                else
                    if (Global.Random.Next(15) == 0)
                        for (int i = 0; i < GetBoardType().NumDrawnTracks; i++)
                            if (Notes[currentNoteIndex].HasFret(i))
                                myParticles.AddSparks(i, 1, false);
                score += (float)(songTime.ElapsedGameTime.TotalSeconds * bpm * (0.1f+diff) * Notes[currentNoteIndex].NumNotes * GetScoreMultiplier());

                bool rp = false;
                for (int i = 0; i < RPPhrases.Length; i++)
                    if (RPPhrases[i].Okay && Notes[currentNoteIndex].Start >= RPPhrases[i].Start && Notes[currentNoteIndex].Start <= RPPhrases[i].End)
                        rp = true;
                if (rp)
                    RockPowerAmount += (float)(songTime.ElapsedGameTime.TotalSeconds * bpm * Notes[currentNoteIndex].NumNotes * (0.0001f + (0.0002f * diff)));
            }
            else
                whammyValues.Clear();
            for (int i = 0; i < whammyValues.Count; i++)
                whammyValues[i] = new Wave(whammyValues[i].Value, whammyValues[i].Time + (float)songTime.ElapsedGameTime.TotalSeconds);
            while (whammyValues.Count > 0 && whammyValues[whammyValues.Count - 1].Time > eFade)
                whammyValues.RemoveAt(whammyValues.Count - 1);
        }

        private float GetWaveWidth(SongTime songTime, double time, bool timesWidth)
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
            sinRet = (float)Math.Sin((sinwavestart + time) * 20f);

            // part three: multiply <-- this is the hard one
            return sinRet*(timesWidth?whammyRet:0.1f);
        }

        private void ActivateStarPower()
        {
            if (RockPowerAmount>=0.5)
                RockPowerActivated = true;
        }

        private void EatHalfSP()
        {
            RockPowerAmount -= 0.5f;
        }

        /// <summary>
        /// Turns a "time" into a position on the board
        /// </summary>
        /// <param name="position">the absolute time value of something (e.g. a note)</param>
        /// <param name="ratio">the position of the frets. 0 is at the base of the board, 1 is at the far end</param>
        /// <returns>the position on the board to draw (0-1 is visible range)</returns>
        private static float GetBoardPos(SongTime songTime, double position, float ratio)
        {
            //relative position to current time
            double relPos = position - songTime.TotalSongTime.TotalSeconds;

            //total length of the board
            double len = (eFade * (1 / (1 - ratio)));

            // in correct units
            double retPos = relPos / len;

            //shift for ratio offset
            retPos += ratio;

            //fix for tip of board being 0
            retPos = 1 - retPos;

            return (float)retPos;
        }

        private void DrawBoardTarget(SongTime songTime)
        {
            float[] noteLanePos = { 29 / 256f, 79 / 256f, 127 / 256f, 176 / 256f, 227 / 256f };
            float ratio = BOARD_FADE_RATIO;
            faderEffect.CurrentTechnique = faderEffect.Techniques["Fade"];
            float fh = (eFade - sFade) / (eFade * (1 / ratio));
            if (GetBoardType().Dimensions == Instrument.BoardDimensions.TWO_DIMENSIONAL)
                return;
            Global.Graphics.GraphicsDevice.SetRenderTarget(0, rtWaves);
            Global.Graphics.GraphicsDevice.Clear(new Color(0, 0, 0, 0));
            Color RPColor = new Color(0f, 1f, 1f);
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.None);
            double currentTime = songTime.TotalSongTime.TotalSeconds;
            for (int i = Math.Max(0,currentNoteIndex-1); i < Notes.Length; i++)
            {
                if (Notes[i].Start > currentTime + eFade)
                    break;
                if (Notes[i].Length <= 0)
                    continue;
                if (Notes[i].Start < currentTime && !Notes[i].Burning)
                    continue;
                bool isGray = false;
                bool isSP = false;
                for (int spi = 0; spi < RPPhrases.Length; spi++)
                {
                    if (Notes[i].Start >= RPPhrases[spi].Start && Notes[i].Start <= RPPhrases[spi].End)
                    {
                        if (RPPhrases[spi].Okay)
                            isSP = true;
                    }
                }
                if (i < currentNoteIndex || (Notes[i].Start < currentTime && Notes[i].End > currentTime && !Notes[i].Burning))
                    isGray = true;

                float LineWidth = 0.8f;

                // how far between nodes (curve resolution) in seconds
                float spaceBetween = 0.01f;

                float k;
                for (k = (float)Math.Max(0, Notes[i].Start - currentTime); k + spaceBetween < Math.Min(eFade, Notes[i].End - currentTime); k += spaceBetween)
                {
                    float Z1 = GetBoardPos(songTime,k + currentTime, 1 - ratio) * rtWaves.Height;
                    float Z2 = GetBoardPos(songTime,k + spaceBetween + currentTime, 1 - ratio) * rtWaves.Height;
                    float X1base = ((GetWaveWidth(songTime, k,Notes[i].Burning) + 1) / 2.0f);
                    float X2base = ((GetWaveWidth(songTime, k + spaceBetween, Notes[i].Burning) + 1) / 2.0f);
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
                        if (!Notes[i].HasFret(r))
                            continue;

                        float X1a = ((X1base + (IsLefty ? (GetBoardType().NumDrawnTracks - 1) - r : r)) / (float)GetBoardType().NumDrawnTracks) * rtWaves.Width;
                        float X1b = ((X3base + (IsLefty ? (GetBoardType().NumDrawnTracks - 1) - r : r)) / (float)GetBoardType().NumDrawnTracks) * rtWaves.Width;

                        spriteBatch.Draw(texLine, new Vector2(X1a, Z1), null, isGray ? Color.Gray : isSP ? RPColor : DefaultFretColors[instrumentType.colorIndices[r]], Angle1, new Vector2(0, texLine.Height / 2), new Vector2(Length / texLine.Width, LineWidth), SpriteEffects.None, 0);
                        spriteBatch.Draw(texLine, new Vector2(X1b, Z1), null, isGray ? Color.Gray : isSP ? RPColor : DefaultFretColors[instrumentType.colorIndices[r]], Angle2, new Vector2(0, texLine.Height / 2), new Vector2(Length / texLine.Width, LineWidth), SpriteEffects.None, 0);
                    }
                }
                //last one :P
                if (Notes[i].End > currentTime)
                {
                    float Z1 = GetBoardPos(songTime,k + currentTime, 1 - ratio) * rtWaves.Height;
                    float Z2 = GetBoardPos(songTime,Notes[i].End/1000.0f, 1 - ratio) * rtWaves.Height;
                    float X1base = ((GetWaveWidth(songTime, k, Notes[i].Burning) + 1) / 2.0f);
                    float X2base = ((GetWaveWidth(songTime, Math.Min(Notes[i].End - (float)currentTime, eFade), Notes[i].Burning) + 1) / 2.0f);


                    for (int r = 0; r < GetBoardType().NumDrawnTracks; r++)
                    {
                        if (!Notes[i].HasFret(r))
                            continue;

                        float X1 = ((X1base + (IsLefty ? (GetBoardType().NumDrawnTracks - 1) - r : r)) / (float)GetBoardType().NumDrawnTracks) * rtWaves.Width;
                        float X2 = ((X2base + (IsLefty ? (GetBoardType().NumDrawnTracks - 1) - r : r)) / (float)GetBoardType().NumDrawnTracks) * rtWaves.Width;

                        float Length = (new Vector2(X2 - X1, Z2 - Z1)).Length();
                        float Angle = (float)Math.Atan2(Z2 - Z1, X2 - X1);

                        //spriteBatch.Draw(texLine, new Vector2(X1, Z1), null, isGray ? Color.Gray : (isSP) ? Color.White : DefaultFretColors[instrumentType.colorIndices[r]], Angle, new Vector2(0, texLine.Height / 2), new Vector2(Length / texLine.Width, LineWidth), SpriteEffects.None, 0);
                    }
                }
            }
            spriteBatch.End();


            
            Global.Graphics.GraphicsDevice.SetRenderTarget(0, rtBoard);

            //float scale = (eFade - sFade) / (eFade * 1.5f);

            faderEffect.CommitChanges();


            Global.Graphics.GraphicsDevice.Clear(new Color(0, 0, 0, 0.85f));
            Global.Graphics.GraphicsDevice.EnableAlphaBlending();
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.None);

            

            Color bgColor;
            if (IsSPActivated())
                bgColor = new Color(0, 255, 255);
            else if (IsFailing)
                // TODO: fix this
                bgColor = new Color((byte)(255 * 1f/*RhythmMaster.Singleton.GetFailTime()*/), 0, 0);
            else if (RockMeterLevel < 0.2f)
            {
                float percentBeat = GetBeatTime(songTime);
                if (percentBeat > 0.5)
                    bgColor = new Color((byte)((percentBeat - 0.5) * 255), 0, 0);
                else
                    bgColor = new Color((byte)((0.5 - percentBeat) * 255), 0, 0);
            }
            else if (RockMeterLevel > 0.8f)
                bgColor = new Color(30, (byte)(30 + ((RockMeterLevel - 0.8f) / 0.2f) * 225), 30);
            else
                bgColor = new Color(30, 30, 30);

            SongData.Barline[] barlines = songData.info.barlines;
            for (int i = 0; i < barlines.Length; i++)
            {
                float Y = GetBoardPos(songTime, barlines[i].time / 1000.0, 1 - ratio) * rtBoard.Height;
                if (Y < 0)
                    break;
                if (i < barlines.Length - 1)
                {
                    float Y2 = GetBoardPos(songTime, barlines[i + 1].time / 1000.0, 1 - ratio) * rtBoard.Height;
                    spriteBatch.Draw(boardBackground,new Rectangle(0,(int)Y2,rtBoard.Width,(int)(Y-Y2)),new Color(bgColor,0.85f));
                }
            }

            // Draw Solo Blues
            if (GetBoardType().HasSolos)
            {
                for (int k = 0; k < Solos.Length; k++)
                {
                    if (Solos[k].Visible)
                    {
                        float y1 = GetBoardPos(songTime, Solos[k].End, 1 - ratio) * rtBoard.Height;
                        float y2 = GetBoardPos(songTime, Solos[k].Start, 1 - ratio) * rtBoard.Height;
                        spriteBatch.Draw(Global.TexWhite, new Rectangle(0, (int)y1, rtBoard.Width, (int)((y2 - y1) + 0.5f)), new Color(0f, 0f, 1f, 0.85f));
                    }
                }
            }

            for (int i = 1; i < GetBoardType().NumDrawnTracks; i++)
            {
                spriteBatch.Draw(Global.TexWhite, new Rectangle((int)(((float)i / (float)GetBoardType().NumDrawnTracks) * rtBoard.Width) - 1, 0, 3, rtBoard.Height), new Color(Color.White, 0.85f));
            }
            for (int i = 0; i < barlines.Length; i++)
            {
                float Y = GetBoardPos(songTime, barlines[i].time / 1000.0, 1 - ratio) * rtBoard.Height;
                if (Y < 0)
                    break;
                spriteBatch.Draw(Global.TexWhite, new Rectangle(0, (int)Y-3, rtBoard.Width, 7), Color.White);
                if (i < barlines.Length - 1)
                {
                    float Y2 = GetBoardPos(songTime, barlines[i + 1].time / 1000.0, 1 - ratio) * rtBoard.Height;
                    spriteBatch.Draw(Global.TexWhite, new Rectangle(0, (int)(((Y2 - Y) * (1 / (float)(barlines[i].numBeats * 2))) + Y) - 1, rtBoard.Width, 2), Color.White);
                    for (int k = 1; k <= barlines[i].numBeats - 1; k++)
                    {
                        spriteBatch.Draw(Global.TexWhite, new Rectangle(0, (int)(((Y2 - Y) * (k / (float)barlines[i].numBeats)) + Y) - 1, rtBoard.Width, 3), Color.White);
                        spriteBatch.Draw(Global.TexWhite, new Rectangle(0, (int)(((Y2 - Y) * (((k * 2) + 1) / (float)(barlines[i].numBeats * 2))) + Y) - 1, rtBoard.Width, 2), Color.White);
                    }
                }
            }

            // draw RP sheen
            if (RockPowerActivated && rpSheen < 1.4f)
            {
                int y = (int)(rtBoard.Height*ratio*(-rpSheen+1));
                spriteBatch.Draw(texRPSheen, new Rectangle(0, y - (int)(rtBoard.Height * 0.1f), rtBoard.Width, (int)(rtBoard.Height * 0.2f)), new Color(0, 255, 255, 255));
            }

            // Draw Board Fills
            if ((GetBoardType().RPEnableType&Instrument.RockPowerEnableTypes.FILL)!=0)// && RockPowerAmount>=0.5f && !RockPowerActivated)
            {
                float halfMaxWidth = rtBoard.Width / (float)(GetBoardType().NumDrawnTracks * 2);
                for (int k = 0; k < Fills.Length; k++)
                {
                    if (Fills[k].Visible)
                    {
                        float y1 = GetBoardPos(songTime, Fills[k].End, 1 - ratio) * rtBoard.Height;
                        float y2 = GetBoardPos(songTime, Fills[k].Start, 1 - ratio) * rtBoard.Height;
                        for (int r = 0; r < GetBoardType().NumDrawnTracks; r++)
                        {
                            float center = ((r * 2 + 1) / (float)(GetBoardType().NumDrawnTracks * 2)) * rtBoard.Width;
                            spriteBatch.Draw(drumfillTex, new Rectangle((int)(center - (halfMaxWidth * Fills[k].Width)), (int)y1, (int)(2 * (halfMaxWidth * ((Fills[k].Width * 0.8f) + 0.2f))), (int)((y2 - y1) + 0.5f)), DefaultFretColors[instrumentType.colorIndices[r]]);
                        }
                    }
                }
            }

            // Draw Big Rock Ending
            if (songData.info.bre.enabled)
            {
                float halfMaxWidth = rtBoard.Width / (float)(GetBoardType().NumDrawnTracks * 2);
                float y1 = GetBoardPos(songTime, songData.info.bre.end/1000f, 1 - ratio) * rtBoard.Height;
                float y2 = GetBoardPos(songTime, songData.info.bre.start/1000f, 1 - ratio) * rtBoard.Height;
                for (int r = 0; r < GetBoardType().NumDrawnTracks; r++)
                {
                    float center = ((r * 2 + 1) / (float)(GetBoardType().NumDrawnTracks * 2)) * rtBoard.Width;
                    Rectangle rect = new Rectangle((int)(center - halfMaxWidth), (int)y1, (int)(2 * halfMaxWidth), (int)((y2 - y1) + 0.5f));
                    Rectangle top = new Rectangle(rect.X, rect.Y, rect.Width, rect.Width / 2);
                    Rectangle bottom = new Rectangle(rect.X, rect.Bottom-(rect.Width/2), rect.Width, rect.Width / 2);
                    Rectangle middle = new Rectangle(rect.X, rect.Y+(rect.Width/2), rect.Width, rect.Height-rect.Width);
                    float colscale = ((((breFades[r] / 0.5f)) * 0.5f) + 0.5f);
                    if (colscale > 1.0f)
                        colscale = 1.0f;
                    Vector3 colBase = (DefaultFretColors[instrumentType.colorIndices[r]].ToVector3()) * colscale;
                    Color col = new Color(colBase.X, colBase.Y, colBase.Z, 1.0f);
                    spriteBatch.Draw(breEndTex, top, col);
                    spriteBatch.Draw(breEndTex, new Rectangle(bottom.Center.X,bottom.Center.Y-(bottom.Height/2),bottom.Width,bottom.Height), null, col, MathHelper.Pi, new Vector2(breEndTex.Width/2,breEndTex.Height), SpriteEffects.None, 0);
                    spriteBatch.Draw(breMiddleTex, middle, col);
                }
            }

            // Draw metainfo under frets
            //if (!Global.DemoMode)
            {
                // just trust me on this one...
                float rpWidth = 0.97656f;
                // this should? be compounded on rpWidth
                float rpMeterWidth = 0.9375f;
                // the height of the overal console thingy
                int rpHeight = (int)((rtBoard.Width / (float)spMeterBG.Width) * spMeterBG.Height * RP_METER_Y_SCALE);
                // this should be compounded on rpHeight
                float rpMeterHeight = 0.21875f;
                // the shift so it isnt covered by the fretboard
                int spMeterShift = 4;
                // draw the console thingy background
                spriteBatch.Draw(spMeterBG, 
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
                                                        (int)(rtBoard.Width * rpWidth * rpMeterWidth * RockPowerDisplayAmount + 0.5f),
                                                        (int)(rpHeight * rpMeterHeight + 0.5f));
                    Color spMeterFillCol = new Color(0,128,128);
                    if (RockPowerAmount >= 0.499999f)
                    {
                        float lVal = GetBeatTime(songTime);
                        if (lVal > 0.5f)
                            lVal = 0.5f - (lVal - 0.5f);
                        lVal = 0.5f - lVal;
                        spMeterFillCol = new Color(0, 0.5f + lVal, 0.5f + lVal);
                    }
                    spriteBatch.Draw(spMeterFill, meterRect, spMeterFillCol);
                    meterRect.Width = (int)(rtBoard.Width * rpWidth * rpMeterWidth + 0.5f);
                }

                int f = GetMultiplierFraction();
                int m = GetMultiplier();
                int mIndex = -1;
                if (m == 2)
                    mIndex = 0;
                else if (m == 3)
                    mIndex = 1;
                else if (m == 4)
                    mIndex = 2;
                else if (m == 5)
                    mIndex = 3;
                else if (m == 6)
                    mIndex = 4;
                else if (m == 8)
                    mIndex = 5;
                else if (m == 10)
                    mIndex = 6;
                else if (m == 12)
                    mIndex = 7;
                // draw the multiplier bar
                for (int k = 0; k < f; k++)
                    spriteBatch.Draw(spMeterLED, 
                                     new Rectangle((int)(rtBoard.Width * 0.109375f + rtBoard.Width * 0.0117f + 0.5f) + (int)(rtBoard.Width * 0.97656f * .078125f * k + 0.5f), 
                                                   (int)(rtBoard.Height * ratio) + spMeterShift + (int)(rpHeight * 0.3125f + 0.5f), 
                                                   (int)(rtBoard.Width * 0.97656f * .078125f + 0.5f) + 1, 
                                                   (int)(rpHeight * 0.3125f + 0.5f)), 
                                     null, 
                                     m <= 1 ? Color.Yellow : m == 2 && f == 10 ? Color.Yellow : m == 2 ? Color.Green : m == 3 && f == 10 ? Color.Green : Color.Purple, 
                                     0, new Vector2(0, 0), SpriteEffects.None, 0.8f);
                // draw the multiplier number
                if (mIndex >= 0)
                    spriteBatch.Draw(texMult[mIndex], 
                                     new Rectangle(rtBoard.Width / 3, 
                                                   (int)(rtBoard.Height * ratio) + spMeterShift + (int)(rpHeight * 0.3125f + 0.5f), 
                                                   rtBoard.Width / 3, 
                                                   (int)(rpHeight * 0.5f + 0.5f)), 
                                     null, Color.White, 0, new Vector2(0, 0), SpriteEffects.None, 0);
            }
            Global.Graphics.GraphicsDevice.EnableAlphaBlending();
            spriteBatch.End();

            Texture2D texWaves = rtWaves.GetTexture();
            Global.Graphics.GraphicsDevice.SetRenderTarget(0, rtWaves);
            Global.Graphics.GraphicsDevice.Clear(new Color(0, 0, 0, 0));

            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
            faderEffect.Begin();
            faderEffect.CurrentTechnique.Passes[0].Begin();

            faderEffect.Parameters["blend"].SetValue(fh);
            faderEffect.CommitChanges();
            spriteBatch.Draw(texWaves, new Rectangle(0, 0, rtWaves.Width, rtWaves.Height), Color.White);

            spriteBatch.End();
            faderEffect.CurrentTechnique.Passes[0].End();
            faderEffect.End();

            Texture2D texBoard = rtBoard.GetTexture();
            Global.Graphics.GraphicsDevice.SetRenderTarget(0, rtBoard);
            Global.Graphics.GraphicsDevice.Clear(new Color(0, 0, 0, 0));

            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
            faderEffect.Begin();
            faderEffect.CurrentTechnique.Passes[0].Begin();

            faderEffect.Parameters["blend"].SetValue(fh);
            faderEffect.CommitChanges();
            spriteBatch.Draw(texBoard, new Rectangle(0, 0, rtBoard.Width, rtBoard.Height), Color.White);

            spriteBatch.End();
            faderEffect.CurrentTechnique.Passes[0].End();
            faderEffect.End();
            Global.Graphics.GraphicsDevice.SetRenderTarget(0, null);
        }

        private void DrawNotes(SongTime songTime)
        {
            effect.DirectionalLight = new DirectionalLight(true,new Vector3(0, 2, 1),new Color(200, 200, 200),new Color(150, 150, 150));
            effect.DiffuseMaterial = new Color(200, 200, 200);

            Global.Graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
            Global.Graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
            Global.Graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;

            if (GetBoardType().Dimensions == Instrument.BoardDimensions.THREE_DIMENSIONAL)
            {
                int lefty = 1;
                if (IsLefty)
                    lefty = -1;

                bool whited = false;
                for (int r = 0; r < GetBoardType().NumTracks; r++)
                {
                    if (r < GetBoardType().NumDrawnTracks)
                        effect.DiffuseTexture = texNotes[GetBoardType().colorIndices[r]];
                    else
                        effect.DiffuseTexture = texBarNotes[GetBoardType().colorIndices[r - GetBoardType().NumDrawnTracks]];
                    whited = false;
                    for (int p = Math.Max(currentNoteIndex,0); p < Notes.Length; p++)
                    {
                        bool IsWhite = false;
                        for(int i=0;i<RPPhrases.Length;i++)
                            if (RPPhrases[i].Okay && Notes[p].Start >= RPPhrases[i].Start && Notes[p].Start <= RPPhrases[i].End)
                            {
                                IsWhite = true;
                            }
                        if (IsWhite && !whited)
                        { effect.DiffuseTexture = texNotesWhite; whited = true; }
                        else if (!IsWhite && whited)
                        {
                            if (r < GetBoardType().NumDrawnTracks)
                                effect.DiffuseTexture = texNotes[GetBoardType().colorIndices[r]];
                            else
                                effect.DiffuseTexture = texBarNotes[GetBoardType().colorIndices[r - GetBoardType().NumDrawnTracks]];
                            whited = false;
                        }

                        // how far along the board... should be called Z probly
                        float Y = ((Notes[p].Start) - (float)songTime.TotalSongTime.TotalSeconds);
                        if (Y > eFade)
                            break;
                        Y /= eFade;
                        Y *= 1f - LengthRPRatio;

                        if (!Notes[p].IsVisible(r))
                            continue;

                        Matrix matTransl, matScale, matRotate;

                        if( r >= GetBoardType().NumDrawnTracks )
                            matTransl = Matrix.CreateTranslation(0f, 0.005f, (Length * Y));
                        else
                            matTransl = Matrix.CreateTranslation(((((r * 2) + 1) / (float)(GetBoardType().NumDrawnTracks * 2)) - 0.5f) * -lefty * width * 2.0f, 0.02f, (Length * Y));

                        if (r < GetBoardType().NumDrawnTracks)
                            matRotate = Matrix.CreateRotationX(Y * 10);
                        else
                            matRotate = Matrix.Identity;

                        if (r>=GetBoardType().NumDrawnTracks)
                            matScale = Matrix.CreateScale(new Vector3(width, 1, Length * 0.01f));
                        else
                            matScale = Matrix.CreateScale(new Vector3((Notes[p].IsHOPO ? 0.5f : 1.0f) * 0.125f * width, 0.25f * Width, 0.25f * Width));

                        float alpha;
                        if ((Notes[p].Start - (float)songTime.TotalSongTime.TotalSeconds) < sFade)
                            alpha = 1;
                        else if ((Notes[p].Start - (float)songTime.TotalSongTime.TotalSeconds) < eFade)
                            alpha = 1 - (((Notes[p].Start - (float)songTime.TotalSongTime.TotalSeconds) - sFade) / (eFade - sFade));
                        else
                            alpha = 0;

                        effect.DiffuseMaterial = Color.White;

                        effect.Alpha = alpha;

                        // identity, scale, rotate, orbit(translate & rotate), translate
                        effect.World = matScale * matRotate * matTransl;

                        effect.CommitChanges();
                        if (r>=GetBoardType().NumDrawnTracks)
                        {
                            mdlTriggerBorder.Draw();
                        }
                        else
                        {
                            //mdlNoteInside.Draw();

                            effect.Shininess = 32f;
                            effect.SpecularMaterial = Color.White;
                            effect.AmbientMaterial = new Color(50, 50, 50);
                            effect.CommitChanges();

                            mdlNote.Draw();

                            effect.AmbientMaterial = Color.White;
                        }
                        
                    }
                }
                // draw fill green note
                if ((GetBoardType().RPEnableType & Instrument.RockPowerEnableTypes.FILL) != 0)
                {
                    for (int i = 0; i < Fills.Length; i++)
                    {
                        if (Fills[i].End < songTime.TotalSongTime.TotalSeconds)
                            continue;
                        if (Fills[i].Start > songTime.TotalSongTime.TotalSeconds + eFade)
                            break;
                        if (!Fills[i].Visible)
                            continue;
                        int r = 3;
                        if (r < GetBoardType().NumDrawnTracks)
                            effect.DiffuseTexture = texNotes[GetBoardType().colorIndices[r]];

                        float Y = (Fills[i].GreenNotePos - (float)songTime.TotalSongTime.TotalSeconds);
                        if (Y > eFade)
                            break;
                        Y /= eFade;
                        Y *= 1f - LengthRPRatio;

                        Matrix matTransl, matScale, matRotate;

                        if (r >= GetBoardType().NumDrawnTracks)
                            matTransl = Matrix.CreateTranslation(0f, height, (Length * Y));
                        else
                            matTransl = Matrix.CreateTranslation(((((r * 2) + 1) / (float)(GetBoardType().NumDrawnTracks * 2)) - 0.5f) * -lefty * width * 2.0f, 0.02f, (Length * Y));

                        if (r < GetBoardType().NumDrawnTracks)
                            matRotate = Matrix.CreateRotationX(Y * 10);
                        else
                            matRotate = Matrix.Identity;

                        if (r >= GetBoardType().NumDrawnTracks)
                            matScale = Matrix.CreateScale(new Vector3(width, curveHeight, Length * 0.01f));
                        else
                            matScale = Matrix.CreateScale(new Vector3(0.125f * width, 0.4f * Width, 0.4f * Width));

                        float alpha;
                        if (Y < sFade)
                            alpha = 1;
                        else if (Y < eFade)
                            alpha = 1 - ((Y - sFade) / (eFade - sFade));
                        else
                            alpha = 0;

                        effect.DiffuseMaterial = Color.White;

                        effect.Alpha = alpha;

                        // identity, scale, rotate, orbit(translate & rotate), translate
                        effect.World = matScale * matRotate * matTransl;

                        effect.CommitChanges();
                        if (r >= GetBoardType().NumDrawnTracks)
                        {
                            mdlTriggerBorder.Draw();
                        }
                        else
                        {
                            //mdlNoteInside.Draw();

                            effect.Shininess = 32f;
                            effect.SpecularMaterial = Color.White;
                            effect.AmbientMaterial = new Color(50, 50, 50);
                            effect.CommitChanges();

                            mdlNote.Draw();

                            effect.AmbientMaterial = Color.White;
                        }
                    }
                }
            }
        }

        private void DrawBoardDetail()
        {
            if (GetBoardType().Dimensions==Instrument.BoardDimensions.THREE_DIMENSIONAL)
            {
                Global.Graphics.GraphicsDevice.VertexDeclaration = VertexTangentBinormal.VertexDeclaration;

                effect.Alpha = 1.0f;
                effect.AmbientMaterial = Color.White;
                effect.DiffuseMaterial = new Color(128, 128, 128);
                effect.SpecularMaterial = Color.Black;

                int lefty = 1;
                if (IsLefty)
                    lefty = -1;

                //if (GetBoardType() == GUITAR || GetBoardType() == BASS)
                {
                    Matrix matScale, matTransl;

                    bool[] glow = new bool[5];
                    for (int p = 0; p < GetBoardType().NumDrawnTracks; p++)
                    {

                        //matTransl = Matrix.CreateTranslation(0f, height + (GetBoardBump() * BOARD_BUMP_COEF), 0f);
                        float lanePos = (((p * 2 + 1) / (float)(GetBoardType().NumDrawnTracks * 2)) - 0.5f) * 2;
                        matTransl = Matrix.CreateTranslation(lanePos * -lefty * width, 0, 0);
                        matScale = Matrix.CreateScale((width / (float)GetBoardType().NumDrawnTracks), 0.05f, .05f);

                        effect.World = matScale * matTransl;

                        if(GetBoardType().NeedsStrum && (controller.GetFrets()&(((ulong)1)<<p))!=0)
                        { effect.DiffuseTexture = texTriggersLit[GetBoardType().colorIndices[p]]; glow[p] = true; }
                        else if (!GetBoardType().NeedsStrum)
                        { effect.DiffuseTexture = (fretLights[p]>0?texTriggersLit:texTriggers)[GetBoardType().colorIndices[p]]; glow[p] = true; }
                        else
                            effect.DiffuseTexture = texTriggers[GetBoardType().colorIndices[p]];

                        effect.CommitChanges();

                        mdlTrigger.Draw();
                    }
                }
            }
        }

        private void DrawFlashes()
        {
            Global.Graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;
            effect.DiffuseTexture = texBlast;
            effect.SpecularMaterial = Color.Black;
            effect.DiffuseMaterial = Color.Black;
            int lefty = IsLefty ? 1 : -1;
            for (int i = 0; i < GetBoardType().NumDrawnTracks; i++)
                if (popup[i] > 0)
                {
                    Matrix matRot, matTransl, matScale;
                    matRot = Matrix.CreateRotationY(flashRot) *Matrix.CreateRotationX(-MathHelper.PiOver4);
                    float lanePos = (((i * 2 + 1) / (float)(GetBoardType().NumDrawnTracks * 2)) - 0.5f) * 2 * lefty;
                    matTransl = Matrix.CreateTranslation(lanePos * width, 0.1f, 0f);
                    matScale = Matrix.CreateScale(new Vector3(0.2f, 0.2f, 0.2f));

                    effect.World = matScale * matRot * matTransl;

                    effect.AmbientMaterial = DefaultFretColors[instrumentType.colorIndices[i]];
                    effect.CommitChanges();

                    Global.Graphics.GraphicsDevice.DrawSquare();
                }
            effect.DiffuseMaterial = Color.White;
        }

        private void DrawBoard(SongTime songTime)
        {
            effect.AmbientMaterial = Color.White;
            effect.DiffuseMaterial = new Color(128, 128, 128);
            effect.SpecularMaterial = Color.Black;
            effect.TextureEnabled = true;

            Global.Graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;
            //effect.World = matScale * matOrbit * matTransl;
            effect.World = Matrix.CreateRotationY(MathHelper.Pi) * Matrix.CreateTranslation(0, 0, 1f) * Matrix.CreateScale(Width, curveHeight, 0.5f*Length) * Matrix.CreateTranslation(0,0,-Length*(1/8f));
            effect.DiffuseTexture = rtBoard.GetTexture();
            effect.LightingEnabled = false;
            effect.Alpha = 1.0f;
            effect.CommitChanges();

            mdlBoard.Draw();

            // TODO: change to beat
            float ebb = GetBeatTime(songTime);
            if (ebb < 0.5f)
                ebb *= 2;
            else
                ebb = (0.5f - (ebb - 0.5f)) * 2;
            if (IsFailing)
            {
                effect.AmbientMaterial = new Color(0.25f, 0f, 0f);
                effect.DiffuseMaterial = new Color(0.5f, 0f, 0f);
            }
            else if (RockPowerActivated)
            {
                effect.AmbientMaterial = new Color(0.0f, 0.25f, 0.25f);
                effect.DiffuseMaterial = new Color(0.0f, 0.5f, 0.5f);
            }
            else if (RockMeterLevel < 0.2f)
            {
                float ch = RockMeterLevel * 5;
                if (RockPowerAmount > 0.49999f)
                {
                    effect.AmbientMaterial = new Color(0.25f * ebb, 0.25f * Math.Max(ch, 1 - ebb), 0.25f * Math.Max(ch, 1 - ebb));
                    effect.DiffuseMaterial = new Color(0.5f * ebb, 0.5f * Math.Max(ch, 1 - ebb), 0.5f * Math.Max(ch, 1 - ebb));
                }
                else
                {
                    effect.AmbientMaterial = new Color(0.25f, 0.25f * ch, 0.25f * ch);
                    effect.DiffuseMaterial = new Color(0.5f, 0.5f * ch, 0.5f * ch);
                }
            }
            else if (RockPowerAmount > 0.49999f)
            {
                effect.AmbientMaterial = new Color(0.25f*ebb, 0.25f, 0.25f);
                effect.DiffuseMaterial = new Color(0.5f*ebb, 0.5f, 0.5f);
            }
            else
            {
                effect.AmbientMaterial = new Color(0.25f, 0.25f, 0.25f);
                effect.DiffuseMaterial = new Color(0.5f, 0.5f, 0.5f);
            }
            effect.World = Matrix.CreateTranslation(0, 0, 1f) * Matrix.CreateScale(0.1f, 0.1f, 0.5f * Length * (sFade / eFade)) * Matrix.CreateTranslation(-Width, 0, -Length * (1 / 8f));
            effect.SpecularMaterial = Color.White;
            effect.DiffuseTexture = Global.TexWhite;
            effect.Alpha = 1.0f;
            effect.Shininess = 16.0f;
            effect.CommitChanges();

            mdlBoardBorder.Draw();

            effect.World = Matrix.CreateTranslation(0, 0, 1f) * Matrix.CreateScale(0.1f, 0.1f, 0.5f * Length * (sFade/eFade)) * Matrix.CreateTranslation(Width, 0, -Length * (1 / 8f));
            effect.CommitChanges();

            mdlBoardBorder.Draw(); 

            effect.World = Matrix.CreateRotationY(MathHelper.Pi) * Matrix.CreateTranslation(0, 0, 1f) * Matrix.CreateScale(0.1f, 0.1f, 0.5f * Length * (1 - (sFade / eFade))) * Matrix.CreateTranslation(-Width, 0, (-Length * (1 / 8f)) + (Length * (sFade / eFade)));
            effect.DiffuseTexture = texBoardBorderFade;
            effect.CommitChanges();

            mdlBoardBorder.Draw();

            effect.World = Matrix.CreateRotationY(MathHelper.Pi) * Matrix.CreateTranslation(0, 0, 1f) * Matrix.CreateScale(0.1f, 0.1f, 0.5f * Length * (1 - (sFade / eFade))) * Matrix.CreateTranslation(Width, 0, (-Length * (1 / 8f)) + (Length * (sFade / eFade)));
            effect.CommitChanges();

            mdlBoardBorder.Draw();

            if (RockPowerActivated)
            {
                effect.World = Matrix.CreateRotationZ((float)Global.Random.NextDouble() * MathHelper.TwoPi) * Matrix.CreateTranslation(0, 0, 1f) * Matrix.CreateScale(0.3f, 0.3f, 0.5f * Length) * Matrix.CreateTranslation(-Width, 0, -Length * (1 / 8f));
                effect.AmbientMaterial = new Color(1f, 1f, 1f);
                effect.DiffuseMaterial = new Color(0.0f, 0.0f, 0.0f);
                effect.SpecularMaterial = Color.Black;
                effect.DiffuseTexture = texLightning[Global.Random.Next(texLightning.Length)];
                effect.Alpha = Math.Min(1.0f,rpSheen*10);
                effect.Shininess = 16.0f;
                effect.CommitChanges();

                Global.Graphics.GraphicsDevice.RenderState.CullMode = CullMode.CullClockwiseFace;
                mdlBoardBorder.Draw();
                Global.Graphics.GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
                mdlBoardBorder.Draw();

                effect.World = Matrix.CreateRotationZ((float)Global.Random.NextDouble() * MathHelper.TwoPi) * Matrix.CreateTranslation(0, 0, 1f) * Matrix.CreateScale(0.3f, 0.3f, 0.5f * Length) * Matrix.CreateTranslation(Width, 0, -Length * (1 / 8f));
                effect.CommitChanges();

                Global.Graphics.GraphicsDevice.RenderState.CullMode = CullMode.CullClockwiseFace;
                mdlBoardBorder.Draw();
                Global.Graphics.GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
                mdlBoardBorder.Draw();
            }
        }

        private void DrawWaves()
        {
            effect.AmbientMaterial = Color.White;
            effect.DiffuseMaterial = new Color(128, 128, 128);
            effect.SpecularMaterial = Color.Black;
            effect.TextureEnabled = true;
            
            Global.Graphics.GraphicsDevice.VertexDeclaration = VertexTangentBinormal.VertexDeclaration;
            Global.Graphics.GraphicsDevice.RenderState.DepthBias = -0.0001f;
            Global.Graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;
            //effect.World = matScale * matOrbit * matTransl;
            effect.World = Matrix.CreateRotationY(MathHelper.Pi) * Matrix.CreateTranslation(0, 0, 1f) * Matrix.CreateScale(Width, curveHeight, 0.5f * Length) * Matrix.CreateTranslation(0, 0, -Length * (1 / 8f));
            effect.DiffuseTexture = rtWaves.GetTexture();
            effect.LightingEnabled = false;
            effect.Alpha = 1.0f;
            effect.CommitChanges();
            Global.Graphics.ApplyChanges();

            mdlBoard.Draw();

            Global.Graphics.GraphicsDevice.RenderState.DepthBias = 0f;
        }

        private void Help()
        {
            if (difficulty == Difficulty.Easy)
            {
                if (RockMeterLevel > 0.8f)
                    RockMeterLevel += 0.01f * (RockPowerActivated ? 10 : 1);
                else
                    RockMeterLevel += 0.04f * (RockPowerActivated ? 10 : 1);
            }
            else if (difficulty == Difficulty.Medium)
            {
                if (RockMeterLevel > 0.8f)
                    RockMeterLevel += 0.01f * (RockPowerActivated ? 10 : 1);
                else
                    RockMeterLevel += 0.03f * (RockPowerActivated ? 10 : 1);
            }
            else if (difficulty == Difficulty.Hard)
            {
                if (RockMeterLevel > 0.8f)
                    RockMeterLevel += 0.0075f * (RockPowerActivated ? 10 : 1);
                else
                    RockMeterLevel += 0.02f * (RockPowerActivated ? 10 : 1);
            }
            if (difficulty == Difficulty.Expert)
            {
                if (RockMeterLevel > 0.8f)
                    RockMeterLevel += 0.0025f * (RockPowerActivated ? 10 : 1);
                else
                    RockMeterLevel += 0.01f * (RockPowerActivated ? 10 : 1);
            }
        }

        private void Hurt()
        {
            if (difficulty == Difficulty.Easy)
            {
                if (RockMeterLevel > 0.8f)
                    RockMeterLevel -= 0.01f;//1f;
                else if (RockMeterLevel > 0.2f)
                    RockMeterLevel -= 0.0075f;//0.75f;
                else
                    RockMeterLevel -= 0.005f;// 0.5f;
            }
            else if (difficulty == Difficulty.Medium)
            {
                if (RockMeterLevel > 0.8f)
                    RockMeterLevel -= 0.02f;//1f;
                else if (RockMeterLevel > 0.2f)
                    RockMeterLevel -= 0.01f;//0.75f;
                else
                    RockMeterLevel -= 0.005f;// 0.5f;
            }
            else if (difficulty == Difficulty.Hard)
            {
                if (RockMeterLevel > 0.8f)
                    RockMeterLevel -= 0.03f;//1f;
                else if (RockMeterLevel > 0.2f)
                    RockMeterLevel -= 0.015f;//0.75f;
                else
                    RockMeterLevel -= 0.01f;// 0.5f;
            }
            else if (difficulty == Difficulty.Expert)
            {
                if (RockMeterLevel > 0.8f)
                    RockMeterLevel -= 0.04f;//1f;
                else if (RockMeterLevel > 0.2f)
                    RockMeterLevel -= 0.03f;//0.75f;
                else
                    RockMeterLevel -= 0.02f;// 0.5f;
            }
        }

        /// <summary>
        /// Saves a board from failing
        /// e.g. someone saves using rock power
        /// </summary>
        private void Save()
        {
            RockMeterLevel = 0.8f;
        }

        private void GenerateNoteArray()
        {
            List<SongData.NoteSet> list = new List<SongData.NoteSet>();
            for (int i = 0; i < songData.instruments.Length; i++)
            {
                if (songData.instruments[i].instrumentType == instrumentType.CodeName)
                {
                    for (int p = 0; p < songData.instruments[i].diffSets[(int)difficulty].phrases.Length; p++)
                    {
                        for (int k = 0; k < songData.instruments[i].diffSets[(int)difficulty].phrases[p].notes.Length; k++)
                        {
                            if(!songData.info.bre.enabled || songData.instruments[i].diffSets[(int)difficulty].phrases[p].notes[k].time<=songData.info.bre.start ||
                               songData.instruments[i].diffSets[(int)difficulty].phrases[p].notes[k].time>=songData.info.bre.end)
                                list.Add(songData.instruments[i].diffSets[(int)difficulty].phrases[p].notes[k]);
                        }
                    }
                    break;
                }
            }
            _notes = new GameNote[list.Count];
            for (int i = 0; i < _notes.Length; i++)
                _notes[i] = new GameNote(instrumentType, list[i]);

            for (int i = 0; i < songData.instruments.Length; i++)
                if (songData.instruments[i].instrumentType.Equals(GetBoardType().CodeName))
                {
                    _starPoints = new uint[songData.instruments[i].diffSets[(int)difficulty].starScoreLevels.Length];
                    for (int k = 0; k < songData.instruments[i].diffSets[(int)difficulty].starScoreLevels.Length; k++)
                        _starPoints[k] = songData.instruments[i].diffSets[(int)difficulty].starScoreLevels[k];
                }
            if (_starPoints[_starPoints.Length - 1] <= 0)//gold star val needs to be filled in
                _starPoints[_starPoints.Length - 1] = _starPoints[_starPoints.Length - 2] * 3 / 2;

            for (int i = 0; i < songData.instruments.Length; i++)
                if (songData.instruments[i].instrumentType.Equals(GetBoardType().CodeName))
                {
                    _rpphrases = new GameRockPowerPhrase[songData.instruments[i].rpPhrases.Length];
                    for (int k = 0; k < songData.instruments[i].rpPhrases.Length; k++)
                        _rpphrases[k] = new GameRockPowerPhrase(songData.instruments[i].rpPhrases[k]);
                    break;
                }

            for (int i = 0; i < songData.instruments.Length; i++)
                if (songData.instruments[i].instrumentType.Equals(GetBoardType().CodeName))
                {
                    if (songData.instruments[i].fills != null)
                    {
                        _fills = new GameFill[songData.instruments[i].fills.Length];
                        for (int k = 0; k < songData.instruments[i].fills.Length; k++)
                        {
                            _fills[k] = new GameFill(songData.instruments[i].fills[k]);
                            Point mb = TimeToMeasureBeat(_fills[k].End);
                            mb = FixMeasureBeat(new Point(mb.X, mb.Y+2));
                            _fills[k].HideNotesEnd = MeasureBeatToTime(mb.X, mb.Y);
                        }
                    }
                    break;
                }

            for (int i = 0; i < songData.instruments.Length; i++)
                if (songData.instruments[i].instrumentType.Equals(GetBoardType().CodeName))
                {
                    if (songData.instruments[i].solos != null)
                    {
                        _solos = new GameSolo[songData.instruments[i].solos.Length];
                        for (int k = 0; k < songData.instruments[i].solos.Length; k++)
                        {
                            _solos[k] = new GameSolo(songData.instruments[i].solos[k]);
                            for (int n = 0; n < Notes.Length; n++)
                            {
                                if (Notes[n].Start >= _solos[k].Start && Notes[n].Start <= _solos[k].End)
                                    _solos[k].NumNotes++;
                            }
                        }
                    }
                    break;
                }
        }

        private void FailRockPowerPhraseForNote(GameNote note)
        {
            for (int i = 0; i < RPPhrases.Length; i++)
            {
                if (note.Start < RPPhrases[i].Start)
                    continue;
                if (note.Start > RPPhrases[i].End)
                    continue;
                if (RPPhrases[i].Okay)
                {
                    RPPhrases[i].Fail();
                    myResults.missedSPPH++;
                }
                break;
            }
        }

        private void FailRockPowerPhraseForNow(SongTime songTime)
        {
            for (int i = 0; i < RPPhrases.Length; i++)
            {
                if (songTime.TotalSongTime.TotalSeconds < RPPhrases[i].Start)
                    continue;
                if (songTime.TotalSongTime.TotalSeconds > RPPhrases[i].End)
                    continue;
                if (RPPhrases[i].Okay)
                {
                    RPPhrases[i].Fail();
                    myResults.missedSPPH++;
                }
                break;
            }
        }

        private Point TimeToMeasureBeat(float time)
        {
            if(time<songData.info.barlines[0].time)
                return new Point(-1,-1);
            for(int i=0;i<songData.info.barlines.Length-1;i++)
                if (time >= songData.info.barlines[i].time / 1000f && time <= songData.info.barlines[i + 1].time / 1000f)
                {
                    float start = songData.info.barlines[i].time / 1000f;
                    float end = songData.info.barlines[i+1].time / 1000f;
                    int beat = (int)(((time - start) / (end - start)) * songData.info.barlines[i].numBeats);
                    return new Point(i, beat);
                }
            return new Point(songData.info.barlines.Length, 0);
        }

        private Point FixMeasureBeat(Point mb)
        {
            while (mb.Y < 0 && mb.X > 0)
            {
                mb.X--;
                mb.Y += (int)songData.info.barlines[mb.X].numBeats;
            }
            while (mb.X < songData.info.barlines.Length && mb.Y >= songData.info.barlines[mb.X].numBeats)
            {
                mb.Y -= (int)songData.info.barlines[mb.X].numBeats;
                mb.X++;
            }
            if (mb.X < 0)
                return new Point(-1, -1);
            if (mb.X >= songData.info.barlines.Length)
                return new Point(songData.info.barlines.Length, 0);
            return mb;
        }

        private float MeasureBeatToTime(int measure, int beat)
        {
            if (measure < 0)
                return 0;
            if (measure >= songData.info.barlines.Length-1)
                return songData.info.barlines[songData.info.barlines.Length - 1].time / 1000f;
            return (songData.info.barlines[measure].time + ((songData.info.barlines[measure + 1].time - songData.info.barlines[measure].time) * (beat / (float)songData.info.barlines[measure].numBeats))) / 1000f;
        }
    }
}

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
#endregion

namespace Unsigned
{
    public struct StaticWorldObject
    {
        public int ModelIndex;
        public char ModelType;
        public float x, y, z;
        public float Orientation;
        public int TextureIndex, BMIndex;
        public float Shininess;
    }

    public struct VenueGeometry
    {
        public VertexBuffer vb;
        public int texIndex;
    }

    public struct CamBlendPos
    {
        public Vector3[] pos;
        public Vector3[] target;
        public Vector3[] up;
        public float[] marks;
        public TYPE_LEN TYPE;
        public enum TYPE_LEN
        {
            FLASH=0,    //F
            SHORT=1,    //S
            NORMAL=2,   //N
            LENGTHY=3,  //L
            EXTENDED=4, //E
        }
    }

    public struct LightData
    {
        public Vector3 Pos;
        public bool On;
        public float Near, Far;
        public Vector3 Diffuse;
        public Vector3 Specular;
    }

    public struct Material
    {
        public Texture2D tex, bm;
        public Vector4 diffuse, specular;
        public float shininess;
        public Material(Texture2D t, Texture2D bm)
        {
            tex = t;
            this.bm = bm;
            diffuse = new Vector4(0.8f, 0.8f, 0.8f, 1);
            specular = new Vector4(1f, 1f, 1f, 1f);
            shininess = 24f;
        }
    }

    public struct LightTarget
    {
        public Vector3 dir;
        public byte type; 
        public float ct;
    }

    public struct DLight
    {
        public float on;
        public uint index;
        public Vector3 pos;
        public float innerAngle, outerAngle;
        public LIGHT_TYPE type;
        public LightTarget[] targs;
        public enum LIGHT_TYPE 
        {
            SWEEP=0,
            NORMAL=1, 
            STROBE=2,
            SPOT=3,
        };
    }

    public struct SEffect
    {
        public uint begin, end;
        public EFFECT_TYPE type;
        public int data;
        public enum EFFECT_TYPE 
        { 
            LIGHTING_NORMAL = 0, 
            LIGHTING_STROBE = 1, 
            LIGHTING_SLOWSTROBE = 2, 
            LIGHTING_BLACKOUT = 3, 
            LIGHTING_CHASE_G = 4, 
            LIGHTING_CHASE_B = 5, 
            LIGHTING_CHASE_D = 6, 
            LIGHTING_CHASE_V = 7, 
            LIGHTING_SWEEP = 8, 
            EFFECT_SMOKE = 9, 
            EFFECT_FLARE = 10 
        };
        public static String[] EF_TP_STR = 
        {
            "nr", "sb", "ss", "bo", "cg", "cb", "cd", "cv", "sw", "sk", "fl",
        };
    }

    class Venue
    {
        public static VenueGeometry[] StaticGeom;
        public static Material[] StaticTexture;
        private int lastTexApplied;
        private List<Entity> Entities;
        public static Model[] Models;
        private CamBlendPos[] CamBlends;
        private Vector3 camPos, camUp, camFor;

        private uint cNear, cFar;

        private float fast_strobe_on = 0f;

        private SEffect[] effects;

        #region DEBUG_VAR
        public static bool DEBUG_CAM_CONTROL = false;
        private Vector3 DEBUG_cp;
        private Vector2 DEBUG_rot;
        #endregion

        private String Filename;

        private DLight[] lights;

        public int camindex;
        private long camtime = -1;
        private float camblendvalue;
        private static Random rand=new Random();
        private int[] camtimes;

        private Model guitarM;
        private Texture guitarT;
        private Model bassM;
        private Texture bassT;
        private Model microphoneM;
        private Texture microphoneT;
        private Model[] drumsetM;
        private Texture[] drumsetT;
        private static int DS_BASSDRUM = 0, DS_CRASHCYMBAL = 1, DS_RIDECYMBAL = 2, DS_HIHATCYMBAL = 3, DS_FLOORTOM = 4, DS_TOMTOMS = 5, DS_SNARE = 6;
        private Rocker guitarist,bassist,drummer,vocalist;

        public static float SCALE = 1f;

        public Venue(String Filename, String Songname, Game1 game, ContentManager content, GraphicsDeviceManager graphics, Effect e)
        {
            this.Filename = Filename;
            LoadWorld("venues\\"+Filename, "songdata\\"+Songname,game,content,graphics,"Random","Random","Random","Random",e);
        }

        public void Update(GameTime gameTime, ulong songtime, Effect engine, Song song)
        {
            if (DEBUG_CAM_CONTROL)
            {/*
                KeyboardState kbs = Keyboard.GetState();
                if (kbs.IsKeyDown(Keys.H))
                    DEBUG_cp += new Vector3((float)Game1.dirdistTOhdist((DEBUG_rot.X*180/Math.PI) + 90, 32), 0, (float)Game1.dirdistTOvdist((DEBUG_rot.X*180/Math.PI) + 90, 32))*(gameTime.ElapsedGameTime.Milliseconds*0.001f);
                if (kbs.IsKeyDown(Keys.K))
                    DEBUG_cp += new Vector3((float)Game1.dirdistTOhdist((DEBUG_rot.X*180/Math.PI) - 90, 32), 0, (float)Game1.dirdistTOvdist((DEBUG_rot.X*180/Math.PI) - 90, 32))*(gameTime.ElapsedGameTime.Milliseconds*0.001f);
                if (kbs.IsKeyDown(Keys.U))
                    DEBUG_cp += new Vector3((float)Game1.dirdistTOhdist((DEBUG_rot.X*180/Math.PI), 32), 0, (float)Game1.dirdistTOvdist((DEBUG_rot.X*180/Math.PI), 32))*(gameTime.ElapsedGameTime.Milliseconds*0.001f);
                if (kbs.IsKeyDown(Keys.J))
                    DEBUG_cp += new Vector3((float)Game1.dirdistTOhdist((DEBUG_rot.X*180/Math.PI) + 180, 32), 0, (float)Game1.dirdistTOvdist((DEBUG_rot.X*180/Math.PI) + 180, 32))*(gameTime.ElapsedGameTime.Milliseconds*0.001f);
                if (kbs.IsKeyDown(Keys.O))
                    DEBUG_cp += new Vector3(0,32,0)*(gameTime.ElapsedGameTime.Milliseconds*0.001f);
                if (kbs.IsKeyDown(Keys.L))
                    DEBUG_cp -= new Vector3(0,32,0)*(gameTime.ElapsedGameTime.Milliseconds*0.001f);
                if(kbs.IsKeyDown(Keys.NumPad8))
                    DEBUG_rot.Y+=MathHelper.PiOver4*(gameTime.ElapsedGameTime.Milliseconds*0.001f);
                if(kbs.IsKeyDown(Keys.NumPad2))
                    DEBUG_rot.Y-=MathHelper.PiOver4*(gameTime.ElapsedGameTime.Milliseconds*0.001f);
                if(kbs.IsKeyDown(Keys.NumPad4))
                    DEBUG_rot.X+=MathHelper.PiOver4*(gameTime.ElapsedGameTime.Milliseconds*0.001f);
                if(kbs.IsKeyDown(Keys.NumPad6))
                    DEBUG_rot.X-=MathHelper.PiOver4*(gameTime.ElapsedGameTime.Milliseconds*0.001f);
            */}

            if (camtime==-1)
            {//sets the next camera view once the previous one is finished
                int len = camtimes[1] - camtimes[0];
                CamBlendPos.TYPE_LEN tlen = (CamBlendPos.TYPE_LEN)(-1);
                if (len < 500)
                    tlen = CamBlendPos.TYPE_LEN.FLASH;
                else if (len < 2000)
                    tlen = CamBlendPos.TYPE_LEN.SHORT;
                else if (len < 5000)
                    tlen = CamBlendPos.TYPE_LEN.NORMAL;
                else if (len < 10000)
                    tlen = CamBlendPos.TYPE_LEN.LENGTHY;
                else
                    tlen = CamBlendPos.TYPE_LEN.EXTENDED;
                List<int> list = new List<int>();
                for (int i = 0; i < CamBlends.Length; i++)
                    if (CamBlends[i].TYPE == tlen)
                        list.Add(i);
                camindex = list[rand.Next(list.Count)];
                camtime++;
            }
            else if (camtime < camtimes.Length - 1 && (long)songtime > camtimes[camtime + 1])
            {//sets up camera movement interpolation
                int k;
                do 
                {
                    int len = camtimes[camtime+1] - camtimes[camtime];
                    CamBlendPos.TYPE_LEN tlen = (CamBlendPos.TYPE_LEN)(-1);
                    if (len < 1000)
                        tlen = CamBlendPos.TYPE_LEN.FLASH;
                    else if (len < 2000)
                        tlen = CamBlendPos.TYPE_LEN.SHORT;
                    else if (len < 6000)
                        tlen = CamBlendPos.TYPE_LEN.NORMAL;
                    else if (len < 12000)
                        tlen = CamBlendPos.TYPE_LEN.LENGTHY;
                    else
                        tlen = CamBlendPos.TYPE_LEN.EXTENDED;
                    List<int> list = new List<int>();
                    for (int i = 0; i < CamBlends.Length; i++)
                        if (CamBlends[i].TYPE == tlen)
                            list.Add(i);
                    k = list[rand.Next(list.Count)];
                }
                while (k == camindex && CamBlends.Length>1);
                camindex = k;
                camtime++;
            }
            if (camtime >= camtimes.Length-1)//sets flag for new camera
                camblendvalue = -1;
            else//interpolation math:
                camblendvalue = ((long)songtime - camtimes[camtime]) / (float)(camtimes[camtime + 1] - camtimes[camtime]);

            //Dynamic light init
            Vector3[] plPos = new Vector3[16];
            bool[] plOn = new bool[16];
            float[] plNear = new float[16];
            float[] plFar = new float[16];
            Vector3[] plDif = new Vector3[16];
            Vector3[] plSpc = new Vector3[16];
            int pl = 0;

            for (int i = 0; i < Entities.Count; i++)
            {//updates the entities
                Entities[i].Update(gameTime);
            }

            int lt = 0;

            float[] fars = new float[16], nears = new float[16];
            float[] powers = new float[16];
            bool[] ons = new bool[16];
            Vector3[] poss = new Vector3[16], dirs = new Vector3[16];
            int numLights=6;

            for (int i = 0; i < effects.Length; i++)
            {
                if (effects[i].begin <= songtime && effects[i].end > songtime)
                {
                    switch (effects[i].type)
                    {
                        case SEffect.EFFECT_TYPE.LIGHTING_NORMAL:
                            for (int j = 0; j < lights.Length; j++)
                            {
                                if (lights[j].type == DLight.LIGHT_TYPE.NORMAL)
                                {
                                    ons[lt] = true;
                                    fars[lt] = lights[j].outerAngle;
                                    nears[lt] = lights[j].innerAngle;
                                    powers[lt] = effects[i].data/100f;
                                    poss[lt] = lights[j].pos;
                                    dirs[lt] = lights[j].targs[0].dir;
                                    lt++;
                                    if (lt >= numLights)
                                        break;
                                }
                            }
                            break;
                        case SEffect.EFFECT_TYPE.LIGHTING_STROBE:
                            for (int j = 0; j < lights.Length; j++)
                            {
                                if (lights[j].type == DLight.LIGHT_TYPE.STROBE)
                                {
                                    ons[lt] = true;
                                    fars[lt] = lights[j].outerAngle;
                                    nears[lt] = lights[j].innerAngle;
                                    powers[lt] = fast_strobe_on;
                                    poss[lt] = lights[j].pos;
                                    dirs[lt] = lights[j].targs[0].dir;
                                    lt++;
                                    if (lt >= numLights)
                                        break;
                                }
                            }
                            fast_strobe_on -= 0.5f;
                            if (fast_strobe_on < 0)
                                fast_strobe_on = 1f;
                            break;
                        case SEffect.EFFECT_TYPE.LIGHTING_SLOWSTROBE:
                            for (int j = 0; j < lights.Length; j++)
                            {
                                if (lights[j].type == DLight.LIGHT_TYPE.STROBE)
                                {
                                    ons[lt] = true;
                                    fars[lt] = lights[j].outerAngle;
                                    nears[lt] = lights[j].innerAngle;
                                    powers[lt] = GetStrobe(effects[i].data, song,songtime);
                                    poss[lt] = lights[j].pos;
                                    dirs[lt] = lights[j].targs[0].dir;
                                    lt++;
                                    if (lt >= numLights)
                                        break;
                                }
                            }
                            break;
                        case SEffect.EFFECT_TYPE.LIGHTING_CHASE_G:

                            break;
                        case SEffect.EFFECT_TYPE.LIGHTING_CHASE_B:
                            break;
                        case SEffect.EFFECT_TYPE.LIGHTING_CHASE_D:
                            break;
                        case SEffect.EFFECT_TYPE.LIGHTING_CHASE_V:
                            break;
                        case SEffect.EFFECT_TYPE.LIGHTING_SWEEP:
                            for (int j = 0; j < lights.Length; j++)
                            {
                                if (lights[j].type == DLight.LIGHT_TYPE.SWEEP)
                                {
                                    ons[lt] = true;
                                    fars[lt] = lights[j].outerAngle;
                                    nears[lt] = lights[j].innerAngle;
                                    powers[lt] = 1f;
                                    poss[lt] = lights[j].pos;
                                    float val = (((songtime) - effects[i].begin) / (float)(effects[i].end - effects[i].begin));
                                    dirs[lt] = (lights[j].targs[0].dir*(1-val))+(lights[j].targs[1].dir*val);
                                    lt++;
                                    if (lt >= numLights)
                                        break;
                                }
                            }
                            break;
                        case SEffect.EFFECT_TYPE.EFFECT_SMOKE:
                            break;
                        case SEffect.EFFECT_TYPE.EFFECT_FLARE:
                            break;
                        default:
                            break;
                    }
                    if (lt >= numLights)
                        break;                    
                }
            }

            engine.Parameters["pLightOn"].SetValue(ons);
            engine.Parameters["pLightPos"].SetValue(poss);
            engine.Parameters["pLightPower"].SetValue(powers);
            engine.Parameters["pLightDir"].SetValue(dirs);
            engine.Parameters["pLightNear"].SetValue(nears);
            engine.Parameters["pLightFar"].SetValue(fars);
            engine.CommitChanges();
        }

        private float GetStrobe(int spb, Song song, ulong currenttime)
        {
            float measure = song.GetMeasureProgress(currenttime);
            int bpm = song.GetBPMeasure(currenttime);
            float beat = (measure * bpm) % 1;
            return ((beat * spb)) % 1;
        }

        public Matrix GetViewMatrix()
        {
            if (DEBUG_CAM_CONTROL)
            {
                Vector3 cu = new Vector3(0, 1, 0);
                Vector3 ct = Vector3.Transform(new Vector3(1, 0, 0), Matrix.CreateRotationZ(DEBUG_rot.Y) * Matrix.CreateRotationY(DEBUG_rot.X))+DEBUG_cp;
                camPos = DEBUG_cp;
                camUp = cu;
                camFor = ct;
                return Matrix.CreateLookAt(DEBUG_cp,ct,cu);
            }
            else
            {
                Vector3 cp = new Vector3(0f, 0f, 0f), ct = new Vector3(0f, 0f, 0f), cu = new Vector3(0f, 1f, 0f);
                if (CamBlends.Length < 1)
                    return Matrix.CreateLookAt(cp, ct, cu);//No cam blends... problem!
                if (camblendvalue < 0)
                {//not sure...default before/after song?
                    cp = new Vector3(0, 100, -200);
                    ct = new Vector3(0, 75, 0);
                }
                else
                {
                    int i=0;
                    for (; i < CamBlends[camindex].marks.Length; i++)
                        if (CamBlends[camindex].marks[i] <= camblendvalue)
                            break;
                    cp = new Vector3((CamBlends[camindex].pos[i].X * camblendvalue) + (CamBlends[camindex].pos[i+1].X * (1 - camblendvalue)), (CamBlends[camindex].pos[i].Y * camblendvalue) + (CamBlends[camindex].pos[i+1].Y * (1 - camblendvalue)), (CamBlends[camindex].pos[i].Z * camblendvalue) + (CamBlends[camindex].pos[i+1].Z * (1 - camblendvalue)));
                    ct = new Vector3((CamBlends[camindex].target[i].X * camblendvalue) + (CamBlends[camindex].target[i+1].X * (1 - camblendvalue)), (CamBlends[camindex].target[i].Y * camblendvalue) + (CamBlends[camindex].target[i+1].Y * (1 - camblendvalue)), (CamBlends[camindex].target[i].Z * camblendvalue) + (CamBlends[camindex].target[i+1].Z * (1 - camblendvalue)));
                    cu = new Vector3((CamBlends[camindex].up[i].X * camblendvalue) + (CamBlends[camindex].up[i+1].X * (1 - camblendvalue)), (CamBlends[camindex].up[i].Y * camblendvalue) + (CamBlends[camindex].up[i+1].Y * (1 - camblendvalue)), (CamBlends[camindex].up[i].Z * camblendvalue) + (CamBlends[camindex].up[i+1].Z * (1 - camblendvalue)));
                }
                
                camPos = cp;
                camUp = cu;
                camFor = ct;
                return Matrix.CreateLookAt(cp, ct, cu);
            }
        }

        public Matrix GetProjMatrix(float aspect)
        {
            return Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver2,aspect,
                                                       cNear, cFar);
        }

        private void LoadWorld(String Filename, String Songname, Game1 game, ContentManager content, GraphicsDeviceManager graphics, String g, String b, String d, String v, Effect e) 
        {
            
            guitarist = new Rocker("rockers\\"+g,game,content,e);
            bassist = new Rocker("rockers\\"+b,game,content,e);
            drummer = new Rocker("rockers\\"+d,game,content,e);
            vocalist = new Rocker("rockers\\"+v,game,content,e);

            //TODO: fix for customized content
            guitarM = content.Load<Model>("meshes\\gibsonsg");
            drumsetM = new Model[7];
            drumsetT = new Texture2D[7];
            drumsetM[DS_BASSDRUM] = content.Load<Model>("meshes\\bassdrum01");
            drumsetT[DS_BASSDRUM] = content.Load<Texture2D>("graphics\\bassdrum01");
            drumsetM[DS_CRASHCYMBAL] = content.Load<Model>("meshes\\crashcymbal01");
            drumsetT[DS_CRASHCYMBAL] = content.Load<Texture2D>("graphics\\crashcymbal01");
            drumsetM[DS_FLOORTOM] = content.Load<Model>("meshes\\floortom01");
            drumsetT[DS_FLOORTOM] = content.Load<Texture2D>("graphics\\floortom01");
            drumsetM[DS_HIHATCYMBAL] = content.Load<Model>("meshes\\hihatcymbal01");
            drumsetT[DS_HIHATCYMBAL] = content.Load<Texture2D>("graphics\\hihatcymbal01");
            drumsetM[DS_RIDECYMBAL] = content.Load<Model>("meshes\\ridecymbal01");
            drumsetT[DS_RIDECYMBAL] = content.Load<Texture2D>("graphics\\ridecymbal01");
            drumsetM[DS_SNARE] = content.Load<Model>("meshes\\snaredrum01");
            drumsetT[DS_SNARE] = content.Load<Texture2D>("graphics\\snaredrum01");
            drumsetM[DS_TOMTOMS] = content.Load<Model>("meshes\\tomtoms01");
            drumsetT[DS_TOMTOMS] = content.Load<Texture2D>("graphics\\tomtoms01");

            System.IO.BinaryReader fin = new System.IO.BinaryReader(System.IO.File.Open(Filename,System.IO.FileMode.Open));

            char[] header = fin.ReadChars(8);

            ulong filesize = fin.ReadUInt64();

            cNear = fin.ReadUInt32();
            cFar = fin.ReadUInt32();

            guitarist.SetPosition(new Vector3(fin.ReadSingle(), fin.ReadSingle(), fin.ReadSingle()));
            vocalist.SetPosition(new Vector3(fin.ReadSingle(), fin.ReadSingle(), fin.ReadSingle()));
            drummer.SetPosition(new Vector3(fin.ReadSingle(), fin.ReadSingle(), fin.ReadSingle()));
            bassist.SetPosition(new Vector3(fin.ReadSingle(), fin.ReadSingle(), fin.ReadSingle()));

            fin.ReadChars(2);// T{

            StaticTexture = new Material[fin.ReadUInt32()];

            for (int i = 0; i < StaticTexture.Length; i++)
            {
                String n = fin.ReadString();
                Texture2D a = content.Load<Texture2D>("graphics\\"+n+"Tex");
                Texture2D c = content.Load<Texture2D>("graphics\\"+n+"BM");
                StaticTexture[i] = new Material(a, c);
            }

            fin.ReadChars(3);// }G{

            StaticGeom = new VenueGeometry[fin.ReadUInt32()];

            for ( int i = 0; i < StaticGeom.Length; i++)
            {
                StaticGeom[i] = new VenueGeometry();
                Vector3 normal = new Vector3(fin.ReadSingle(), fin.ReadSingle(), fin.ReadSingle());
                Vector3 tangent = new Vector3(fin.ReadSingle(), fin.ReadSingle(), fin.ReadSingle());
                StaticGeom[i].texIndex = (int)fin.ReadUInt32();
                GBVertexFormat[] buffer = new GBVertexFormat[fin.ReadUInt32()];
                for (int k = 0; k < buffer.Length; k++)
                    buffer[k] = new GBVertexFormat(new Vector3(fin.ReadSingle(), fin.ReadSingle(), fin.ReadSingle()), normal, new Vector2(fin.ReadSingle(), fin.ReadSingle()), tangent);
                StaticGeom[i].vb = new VertexBuffer(graphics.GraphicsDevice, buffer.Length * GBVertexFormat.SizeInBytes, BufferUsage.WriteOnly);
                StaticGeom[i].vb.SetData<GBVertexFormat>(buffer);
            }

            fin.ReadChars(3);// }E{

            uint numEs = fin.ReadUInt32();
            Entities = new List<Entity>();

            for (int i = 0; i < numEs; i++)
            {
                String type = fin.ReadString();
                if (type.Equals("cam"))
                {
                    uint num = fin.ReadUInt32();
                    Vector3[] pos = new Vector3[num], angle = new Vector3[num];
                    uint[] index = new uint[num], part = new uint[num], tpe = new uint[num];
                    uint numcams = 0;
                    for (int j = 0; j < num; j++)
                    {
                        index[j] = fin.ReadUInt32();
                        part[j] = fin.ReadUInt32();
                        tpe[j] = fin.ReadUInt32();
                        pos[j] = new Vector3(fin.ReadSingle(), fin.ReadSingle(), fin.ReadSingle());
                        angle[j] = new Vector3(fin.ReadSingle(), fin.ReadSingle(), fin.ReadSingle());
                        if (index[j] > numcams)
                            numcams = index[j];
                    }
                    uint[] lens = new uint[numcams];
                    uint[] amts = new uint[numcams];
                    for (int j = 0; j < num; j++)
                    {
                        if (lens[index[j] - 1] < part[j])
                            lens[index[j] - 1] = part[j];
                        amts[index[j]-1]++;
                    }
                    CamBlends = new CamBlendPos[numcams];
                    for (int k = 0; k < numcams; k++)
                    {
                        CamBlends[k].marks = new float[amts[k]];
                        CamBlends[k].pos = new Vector3[amts[k]];
                        CamBlends[k].target = new Vector3[amts[k]];
                        CamBlends[k].up = new Vector3[amts[k]];
                    }
                    //fill it up
                    for ( int k = 0; k < num; k++)
                    {
                        CamBlends[index[k]-1].TYPE = (CamBlendPos.TYPE_LEN)tpe[k];
                        CamBlends[index[k]-1].marks[amts[index[k]-1]-1] = part[k] / (float)lens[index[k]-1];
                        CamBlends[index[k]-1].pos[amts[index[k]-1] - 1] = pos[k];
                        CamBlends[index[k]-1].target[amts[index[k]-1] - 1] = angle[k];
                        CamBlends[index[k]-1].up[amts[index[k]-1] - 1] = Vector3.Up;
                        amts[index[k]-1]--;
                    }
                    //sort it
                    for (int k = 0; k < CamBlends.Length; k++)
                    {
                        for (int m = 0; m < CamBlends[k].marks.Length; m++)
                        {
                            for(int n=m;n>0;n--)
                            if(CamBlends[k].marks[n]<CamBlends[k].marks[n-1])
                            {
                                float tmp = CamBlends[k].marks[n];
                                CamBlends[k].marks[n] = CamBlends[k].marks[n - 1];
                                CamBlends[k].marks[n - 1] = tmp;
                                Vector3 temp = CamBlends[k].pos[n];
                                CamBlends[k].pos[n] = CamBlends[k].pos[n - 1];
                                CamBlends[k].pos[n - 1] = temp;
                                temp = CamBlends[k].target[n];
                                CamBlends[k].target[n] = CamBlends[k].target[n - 1];
                                CamBlends[k].target[n - 1] = temp;
                                temp = CamBlends[k].up[n];
                                CamBlends[k].up[n] = CamBlends[k].up[n - 1];
                                CamBlends[k].up[n - 1] = temp;
                            }
                            else
                                break;
                        }
                    }
                }
                if (type.Equals("lit"))
                {
                    int num = fin.ReadInt32();
                    lights = new DLight[num];
                    for (int j = 0; j < num; j++)
                    {
                        lights[j].on = 0f;
                        lights[j].index = fin.ReadUInt32();
                        lights[j].type = (DLight.LIGHT_TYPE)fin.ReadInt32();
                        lights[j].innerAngle = fin.ReadSingle();
                        lights[j].outerAngle = fin.ReadSingle();
                        lights[j].pos = new Vector3(fin.ReadSingle(), fin.ReadSingle(), fin.ReadSingle());
                        lights[j].targs = new LightTarget[fin.ReadInt32()];
                        for (int k = 0; k < lights[j].targs.Length; k++)
                        {
                            lights[j].targs[k].type = fin.ReadByte();
                            lights[j].targs[k].ct = (fin.ReadChar()-61)/(26f);
                            lights[j].targs[k].dir = new Vector3(fin.ReadSingle(), fin.ReadSingle(), fin.ReadSingle());
                        }
                    }
                }
            }

            fin.ReadChars(1);// }
            fin.Close();

            System.IO.BinaryReader sr = new System.IO.BinaryReader(System.IO.File.OpenRead(Songname + ".gbe"));

            int nTransitions = sr.ReadInt32();
            camtimes = new int[nTransitions];
            for (int i = 0; i < nTransitions; i++)
                camtimes[i] = sr.ReadInt32();
            int nEffects = sr.ReadInt32();
            effects = new SEffect[nEffects];
            for (int i = 0; i < nEffects; i++)
            {
                effects[i] = new SEffect();
                effects[i].begin = sr.ReadUInt32();
                String eftp = ""+sr.ReadChar()+sr.ReadChar();
                effects[i].end = sr.ReadUInt32();
                effects[i].data = sr.ReadInt32();
                for (int k = 0; k < SEffect.EF_TP_STR.Length; k++)
                    if (eftp.Equals(SEffect.EF_TP_STR[k]))
                        effects[i].type = (SEffect.EFFECT_TYPE)k;
            }

            sr.Close();
        }

        public void Render(GraphicsDeviceManager graphics, Effect engine, Matrix matProj,
                           VertexDeclaration vd, GameTime gameTime)
        {
            lastTexApplied=-1;
            Matrix matIdentity, matTransl, matScale, matRot, matOrbit, mMatWorld;

            engine.Parameters["ambientColor"].SetValue(new Vector4(.2f, .2f, .2f, 1f));
            engine.Parameters["fullbright"].SetValue(false);

            for (int i = 0; i < StaticGeom.Length; i++)
            {
                matIdentity = Matrix.Identity;
                matScale = Matrix.CreateScale(SCALE);

                // identity, scale, rotate, orbit(translate & rotate), translate
                mMatWorld = matIdentity * matScale;
                    engine.Parameters["world"].SetValue(mMatWorld);
                    engine.Parameters["wRot"].SetValue(Matrix.Identity);
                    engine.Parameters["shininess"].SetValue(StaticTexture[StaticGeom[i].texIndex].shininess);
                    engine.Parameters["diffuseColor"].SetValue(new Vector4(.8f, .8f, .8f, 1f));
                    //engine.Parameters["specularColor"].SetValue(new Vector4(.8f, .8f, .8f, 1f));
                    if (lastTexApplied != StaticGeom[i].texIndex)
                    {
                        engine.Parameters["diffuseTexture"].SetValue(StaticTexture[StaticGeom[i].texIndex].tex);
                        engine.Parameters["bumpTexture"].SetValue(StaticTexture[StaticGeom[i].texIndex].bm);
                        lastTexApplied = StaticGeom[i].texIndex;
                    }
                    engine.CommitChanges();

                    // 5: draw object - select vertex type, primitive type, # of primitives
                    graphics.GraphicsDevice.VertexDeclaration = vd;
                    graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                    graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                    graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                    graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;
                    graphics.ApplyChanges();
                    graphics.GraphicsDevice.Vertices[0].SetSource(StaticGeom[i].vb, 0, GBVertexFormat.SizeInBytes);
                    graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleFan, 0, (StaticGeom[i].vb.SizeInBytes/GBVertexFormat.SizeInBytes)-2);
                    graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
            }
            for (int i = 0; i < Entities.Count; i++)
            {
                Entities[i].Draw(engine,graphics,camPos);
            }
            engine.Parameters["vertexAlpha"].SetValue(false);
            engine.Parameters["BumpMappingEnabled"].SetValue(false);
            //engine.Parameters["SpecularEnabled"].SetValue(false);
            graphics.GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
            {//guitarist

                guitarist.Draw(gameTime, graphics);
                
            }//guitarist
            {//Bassist
                bassist.Draw(gameTime, graphics);
            }//Bassist
            {//Drummer

                drummer.Draw(gameTime, graphics);
                engine.Parameters["vertexAlpha"].SetValue(false);

                //BASS DRUM
                matIdentity = Matrix.Identity;
                matTransl = Matrix.CreateTranslation(drummer.GetPosition());
                matOrbit = Matrix.CreateTranslation(0,0,-50*SCALE)*Matrix.CreateRotationY(drummer.Rot);
                matScale = Matrix.CreateScale(SCALE*128f);


                // identity, scale, rotate, orbit(translate & rotate), translate
                mMatWorld = matIdentity * matScale * matOrbit * matTransl;

                //engine.Parameters["diffuseColor"].SetValue(new Vector4(0.8f, 0.8f, 0.8f, 1.0f));
                //engine.Parameters["fullbright"].SetValue(true);
                engine.Parameters["world"].SetValue(mMatWorld);
                engine.Parameters["wRot"].SetValue(Matrix.Identity);
                engine.Parameters["diffuseTexture"].SetValue(drumsetT[DS_BASSDRUM]);
                engine.Parameters["bumpTexture"].SetValue(Game1.texDefaultBM);
                engine.CommitChanges();

                foreach (ModelMesh mesh in drumsetM[DS_BASSDRUM].Meshes)
                {
                    foreach (ModelMeshPart meshpart in mesh.MeshParts)
                    {
                        //engine.Parameters["diffuseTexture"].SetValue(Game1.texWhite);
                        engine.CommitChanges();
                        graphics.GraphicsDevice.VertexDeclaration = meshpart.VertexDeclaration;
                        graphics.GraphicsDevice.Vertices[0].SetSource(mesh.VertexBuffer, meshpart.StreamOffset, meshpart.VertexStride);
                        graphics.GraphicsDevice.Indices = mesh.IndexBuffer;
                        graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, meshpart.BaseVertex, 0, meshpart.NumVertices, meshpart.StartIndex, meshpart.PrimitiveCount);
                    }
                }
                //engine.Parameters["fullbright"].SetValue(false);

                //CRASH CYMBAL
                matIdentity = Matrix.Identity;
                matRot = Matrix.CreateRotationY(-(float)(Math.PI * 3.5 / 8));
                matOrbit = Matrix.CreateTranslation(0, 0, -65 * SCALE) * Matrix.CreateRotationY(drummer.Rot-(float)(Math.PI/5.5));
                matScale = Matrix.CreateScale(SCALE * 128);

                // identity, scale, rotate, orbit(translate & rotate), translate
                mMatWorld = matIdentity * matScale * matRot * matOrbit * matTransl;

                engine.Parameters["world"].SetValue(mMatWorld);
                engine.Parameters["wRot"].SetValue(Matrix.Identity);
                engine.Parameters["diffuseTexture"].SetValue(drumsetT[DS_CRASHCYMBAL]);
                engine.Parameters["bumpTexture"].SetValue(Game1.texDefaultBM);
                engine.CommitChanges();

                foreach (ModelMesh mesh in drumsetM[DS_CRASHCYMBAL].Meshes)
                {
                    foreach (ModelMeshPart meshpart in mesh.MeshParts)
                    {
                        engine.CommitChanges();
                        graphics.GraphicsDevice.VertexDeclaration = meshpart.VertexDeclaration;
                        graphics.GraphicsDevice.Vertices[0].SetSource(mesh.VertexBuffer, meshpart.StreamOffset, meshpart.VertexStride);
                        graphics.GraphicsDevice.Indices = mesh.IndexBuffer;
                        graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, meshpart.BaseVertex, 0, meshpart.NumVertices, meshpart.StartIndex, meshpart.PrimitiveCount);
                    }
                }

                //FLOOR TOM
                matIdentity = Matrix.Identity;
                matRot = Matrix.CreateRotationY(-(float)(Math.PI * 3.5 / 8));
                matOrbit = Matrix.CreateTranslation(0, 0, -40 * SCALE) * Matrix.CreateRotationY(drummer.Rot - (float)(Math.PI / 5.5));
                matScale = Matrix.CreateScale(SCALE * 128);

                // identity, scale, rotate, orbit(translate & rotate), translate
                mMatWorld = matIdentity * matScale * matRot * matOrbit * matTransl;

                engine.Parameters["world"].SetValue(mMatWorld);
                engine.Parameters["wRot"].SetValue(Matrix.Identity);
                engine.Parameters["diffuseTexture"].SetValue(drumsetT[DS_FLOORTOM]);
                engine.Parameters["bumpTexture"].SetValue(Game1.texDefaultBM);
                engine.CommitChanges();

                foreach (ModelMesh mesh in drumsetM[DS_FLOORTOM].Meshes)
                {
                    foreach (ModelMeshPart meshpart in mesh.MeshParts)
                    {
                        engine.CommitChanges();
                        graphics.GraphicsDevice.VertexDeclaration = meshpart.VertexDeclaration;
                        graphics.GraphicsDevice.Vertices[0].SetSource(mesh.VertexBuffer, meshpart.StreamOffset, meshpart.VertexStride);
                        graphics.GraphicsDevice.Indices = mesh.IndexBuffer;
                        graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, meshpart.BaseVertex, 0, meshpart.NumVertices, meshpart.StartIndex, meshpart.PrimitiveCount);
                    }
                }
                //TOM TOMS
                matIdentity = Matrix.Identity;
                matOrbit = Matrix.CreateTranslation(0, 32 * SCALE, -50 * SCALE) * Matrix.CreateRotationY(drummer.Rot+0.04f);
                matScale = Matrix.CreateScale(SCALE * 128);

                // identity, scale, rotate, orbit(translate & rotate), translate
                mMatWorld = matIdentity * matScale * matOrbit * matTransl;

                engine.Parameters["world"].SetValue(mMatWorld);
                engine.Parameters["wRot"].SetValue(Matrix.Identity);
                engine.Parameters["diffuseTexture"].SetValue(drumsetT[DS_TOMTOMS]);
                engine.Parameters["bumpTexture"].SetValue(Game1.texDefaultBM);
                engine.CommitChanges();

                foreach (ModelMesh mesh in drumsetM[DS_TOMTOMS].Meshes)
                {
                    foreach (ModelMeshPart meshpart in mesh.MeshParts)
                    {
                        engine.CommitChanges();
                        graphics.GraphicsDevice.VertexDeclaration = meshpart.VertexDeclaration;
                        graphics.GraphicsDevice.Vertices[0].SetSource(mesh.VertexBuffer, meshpart.StreamOffset, meshpart.VertexStride);
                        graphics.GraphicsDevice.Indices = mesh.IndexBuffer;
                        graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, meshpart.BaseVertex, 0, meshpart.NumVertices, meshpart.StartIndex, meshpart.PrimitiveCount);
                    }
                }
                //SNARE DRUM
                matIdentity = Matrix.Identity;
                matRot = Matrix.CreateRotationY(-(float)Math.PI / 2f);
                matOrbit = Matrix.CreateTranslation(0, 0, -50 * SCALE) * Matrix.CreateRotationY(drummer.Rot + (float)(Math.PI / 5.5));
                matScale = Matrix.CreateScale(SCALE * 128);

                // identity, scale, rotate, orbit(translate & rotate), translate
                mMatWorld = matIdentity * matScale * matRot * matOrbit * matTransl;

                engine.Parameters["world"].SetValue(mMatWorld);
                engine.Parameters["wRot"].SetValue(Matrix.Identity);
                engine.Parameters["diffuseTexture"].SetValue(drumsetT[DS_SNARE]);
                engine.Parameters["bumpTexture"].SetValue(Game1.texDefaultBM);
                engine.CommitChanges();

                foreach (ModelMesh mesh in drumsetM[DS_SNARE].Meshes)
                {
                    foreach (ModelMeshPart meshpart in mesh.MeshParts)
                    {
                        engine.CommitChanges();
                        graphics.GraphicsDevice.VertexDeclaration = meshpart.VertexDeclaration;
                        graphics.GraphicsDevice.Vertices[0].SetSource(mesh.VertexBuffer, meshpart.StreamOffset, meshpart.VertexStride);
                        graphics.GraphicsDevice.Indices = mesh.IndexBuffer;
                        graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, meshpart.BaseVertex, 0, meshpart.NumVertices, meshpart.StartIndex, meshpart.PrimitiveCount);
                    }
                }
                //RIDE CYMBAL
                matIdentity = Matrix.Identity;
                matRot = Matrix.CreateRotationY(-(float)Math.PI / 2.5f);
                matOrbit = Matrix.CreateTranslation(0, 0, -65 * SCALE) * Matrix.CreateRotationY(drummer.Rot + (float)(Math.PI / 6.2));
                matScale = Matrix.CreateScale(SCALE * 128);

                // identity, scale, rotate, orbit(translate & rotate), translate
                mMatWorld = matIdentity * matScale * matRot * matOrbit * matTransl;

                engine.Parameters["world"].SetValue(mMatWorld);
                engine.Parameters["wRot"].SetValue(Matrix.Identity);
                engine.Parameters["diffuseTexture"].SetValue(drumsetT[DS_RIDECYMBAL]);
                engine.Parameters["bumpTexture"].SetValue(Game1.texDefaultBM);
                engine.CommitChanges();

                foreach (ModelMesh mesh in drumsetM[DS_RIDECYMBAL].Meshes)
                {
                    foreach (ModelMeshPart meshpart in mesh.MeshParts)
                    {
                        engine.CommitChanges();
                        graphics.GraphicsDevice.VertexDeclaration = meshpart.VertexDeclaration;
                        graphics.GraphicsDevice.Vertices[0].SetSource(mesh.VertexBuffer, meshpart.StreamOffset, meshpart.VertexStride);
                        graphics.GraphicsDevice.Indices = mesh.IndexBuffer;
                        graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, meshpart.BaseVertex, 0, meshpart.NumVertices, meshpart.StartIndex, meshpart.PrimitiveCount);
                    }
                }
                //HIHAT
                matIdentity = Matrix.Identity;
                matRot = Matrix.CreateRotationY(-(float)Math.PI / 2.3f);
                matOrbit = Matrix.CreateTranslation(0, 0, -40 * SCALE) * Matrix.CreateRotationY(drummer.Rot + (float)(Math.PI / 3.5));
                matScale = Matrix.CreateScale(SCALE * 128);

                // identity, scale, rotate, orbit(translate & rotate), translate
                mMatWorld = matIdentity * matScale * matRot * matOrbit * matTransl;

                engine.Parameters["world"].SetValue(mMatWorld);
                engine.Parameters["wRot"].SetValue(Matrix.Identity);
                engine.Parameters["diffuseTexture"].SetValue(drumsetT[DS_HIHATCYMBAL]);
                engine.Parameters["bumpTexture"].SetValue(Game1.texDefaultBM);
                engine.CommitChanges();

                foreach (ModelMesh mesh in drumsetM[DS_HIHATCYMBAL].Meshes)
                {
                    foreach (ModelMeshPart meshpart in mesh.MeshParts)
                    {
                        engine.CommitChanges();
                        graphics.GraphicsDevice.VertexDeclaration = meshpart.VertexDeclaration;
                        graphics.GraphicsDevice.Vertices[0].SetSource(mesh.VertexBuffer, meshpart.StreamOffset, meshpart.VertexStride);
                        graphics.GraphicsDevice.Indices = mesh.IndexBuffer;
                        graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, meshpart.BaseVertex, 0, meshpart.NumVertices, meshpart.StartIndex, meshpart.PrimitiveCount);
                    }
                }

            }//drummer
            {//singer
                vocalist.Draw(gameTime, graphics);
            }//singer
            engine.Parameters["vertexAlpha"].SetValue(true);
            engine.Parameters["BumpMappingEnabled"].SetValue(true);
            engine.Parameters["SpecularEnabled"].SetValue(true);
            graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;
        }

        internal Vector3 GetCamPos()
        {
            return camPos;
        }

        internal Vector3 GetCamUp()
        {
            return Vector3.Normalize(camUp);
        }

        internal Vector3 GetCamFor()
        {
            return Vector3.Normalize(camFor - camPos);
        }
    }
}



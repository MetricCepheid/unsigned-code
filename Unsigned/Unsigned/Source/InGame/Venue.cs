//#define DEBUG_CAM_CONTROL

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using SongDataIO;
using FVProductions.Utility;

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

    class Venue
    {
        private ContentManager Content;

        private FVShader effect;

        private static VenueGeometry[] StaticGeom;
        private static Material[] StaticTexture;
        private int lastTexApplied;
        private List<Entity> Entities;
        private static FVModel[] Models;
        private CamBlendPos[] CamBlends;
        private Vector3 camPos, camUp, camFor;

        private uint cNear, cFar;

        private SongData songData;

        private SongData.SpecialEffect[] effects { get { return songData.effects.effects; } }

        private Fan[] Fans;

#if DEBUG_CAM_CONTROL
        private Vector3 DEBUG_cp;
        private Vector2 DEBUG_rot;
#endif

        private String Filename;

        private DLight[] lights;

        public int camindex;
        private long camtime = -1;
        private float camblendvalue;
        private uint[] camtimes { get { return songData.effects.cameraSwitches; } }

        private const int DS_BASSDRUM = 0, DS_CRASHCYMBAL = 1, DS_RIDECYMBAL = 2, DS_HIHATCYMBAL = 3, DS_FLOORTOM = 4, DS_TOMTOMS = 5, DS_SNARE = 6;
        private Rocker[] rockers;

        public static float SCALE = 1f;

        public Venue(String Filename, SongData songData, SessionInfo nugget)
        {
            this.Filename = Filename;
            this.songData = songData;
            LoadWorld(Filename,nugget);
        }

        public void Update(SongTime songTime)
        {
#if DEBUG_CAM_CONTROL
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
#endif
            if (camtime == -1)
            {//sets the next camera view once the previous one is finished
                uint len = camtimes[1] - camtimes[0];
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
                camindex = list[Global.Random.Next(list.Count)];
                camtime++;
            }
            else if (camtime < camtimes.Length - 1 && songTime.TotalSongTime.TotalMilliseconds > camtimes[camtime + 1])
            {//sets up camera movement interpolation
                int k;
                do
                {
                    uint len = camtimes[camtime + 1] - camtimes[camtime];
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
                    k = list[Global.Random.Next(list.Count)];
                }
                while (k == camindex && CamBlends.Length > 1);
                camindex = k;
                camtime++;
            }
            if (camtime >= camtimes.Length - 1)//sets flag for new camera
                camblendvalue = -1;
            else//interpolation math:
                camblendvalue = ((long)(songTime.TotalSongTime.TotalMilliseconds) - camtimes[camtime]) / (float)(camtimes[camtime + 1] - camtimes[camtime]);

            for (int i = 0; i < Entities.Count; i++)
            {//updates the entities
                Entities[i].Update(songTime);
            }

            Fan.Update(GetBeatTime(songTime));
            Vector3 vocalistPos = Vector3.Zero;
            for(int i=0;i<rockers.Length;i++)
                if(rockers[i].GetInstrument().CodeName=="LVX")
                    vocalistPos = rockers[i].Position;
            for (int i = 0; i < Fans.Length; i++)
                Fans[i].Update(songTime, vocalistPos);
        }

        public void SetLights(Effect engine, uint songtime)
        {
            //Dynamic light init
            Vector3[] plPos = new Vector3[16];
            bool[] plOn = new bool[16];
            float[] plNear = new float[16];
            float[] plFar = new float[16];
            Vector3[] plDif = new Vector3[16];
            Vector3[] plSpc = new Vector3[16];

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
                    if(effects[i] is SongData.NormalLightingSpecialEffect)
                    {
                        for (int j = 0; j < lights.Length; j++)
                        {
                            if (lights[j].type == DLight.LIGHT_TYPE.NORMAL)
                            {
                                ons[lt] = true;
                                fars[lt] = lights[j].outerAngle;
                                nears[lt] = lights[j].innerAngle;
                                powers[lt] = ((SongData.NormalLightingSpecialEffect)effects[i]).color.R/100f;
                                poss[lt] = lights[j].pos;
                                dirs[lt] = lights[j].targs[0].dir;
                                lt++;
                                if (lt >= numLights)
                                    break;
                            }
                        }
                    }
                    /*
                        case LightingEffect.EFFECT_TYPE.LIGHTING_STROBE:
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
                        case LightingEffect.EFFECT_TYPE.LIGHTING_SLOWSTROBE:
                            for (int j = 0; j < lights.Length; j++)
                            {
                                if (lights[j].type == DLight.LIGHT_TYPE.STROBE)
                                {
                                    ons[lt] = true;
                                    fars[lt] = lights[j].outerAngle;
                                    nears[lt] = lights[j].innerAngle;
                                    powers[lt] = GetStrobe(effects[i].data);
                                    poss[lt] = lights[j].pos;
                                    dirs[lt] = lights[j].targs[0].dir;
                                    lt++;
                                    if (lt >= numLights)
                                        break;
                                }
                            }
                            break;
                        case LightingEffect.EFFECT_TYPE.LIGHTING_CHASE_G:

                            break;
                        case LightingEffect.EFFECT_TYPE.LIGHTING_CHASE_B:
                            break;
                        case LightingEffect.EFFECT_TYPE.LIGHTING_CHASE_D:
                            break;
                        case LightingEffect.EFFECT_TYPE.LIGHTING_CHASE_V:
                            break;
                        case LightingEffect.EFFECT_TYPE.LIGHTING_SWEEP:
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
                        case LightingEffect.EFFECT_TYPE.EFFECT_SMOKE:
                            break;
                        case LightingEffect.EFFECT_TYPE.EFFECT_FLARE:
                            break;
                        default:
                            break;
                    }
                     */
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

        private float GetStrobe(int spb)
        {
            // TODO: fix this
            float measure = 0;// RhythmMaster.Singleton.GetMeasureProgress();
            int bpm = 0;// RhythmMaster.Singleton.GetBPMeasure();
            float beat = (measure * bpm) % 1;
            return ((beat * spb)) % 1;
        }

        public Matrix GetViewMatrix()
        {
#if DEBUG_CAM_CONTROL
                Vector3 cu = new Vector3(0, 1, 0);
                Vector3 ct = Vector3.Transform(new Vector3(1, 0, 0), Matrix.CreateRotationZ(DEBUG_rot.Y) * Matrix.CreateRotationY(DEBUG_rot.X))+DEBUG_cp;
                camPos = DEBUG_cp;
                camUp = cu;
                camFor = ct;
                return Matrix.CreateLookAt(DEBUG_cp,ct,cu);
#else
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
#endif
        }

        public Matrix GetProjMatrix()
        {
            return Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                                                       Global.ScreenWidth/(float)Global.ScreenHeight,
                                                       1f, 1000f);
        }

        private void LoadWorld(String Filename, SessionInfo nugget) 
        {
            Content = new ContentManager(Global.Services);
            Content.RootDirectory = "Content";

            Filename = "Content\\Venues\\" + Filename;

            effect = new FVShader(Global.Graphics.GraphicsDevice, Content.Load<Effect>("shaders\\UnsignedEngineShader"), "maintechnique");

            rockers = new Rocker[4];
            rockers[0] = new Rocker(nugget.characterIndices[0], InstrumentMaster.Singleton.GetInstrument("LGT"));
            rockers[1] = new Rocker(nugget.characterIndices[1], InstrumentMaster.Singleton.GetInstrument("LVX"));
            rockers[2] = new Rocker(nugget.characterIndices[2], InstrumentMaster.Singleton.GetInstrument("SET"));
            rockers[3] = new Rocker(nugget.characterIndices[3], InstrumentMaster.Singleton.GetInstrument("BAS"));

            //TODO: fix for customized Content
            //String BaseModelDirectory = "meshes\\instruments\\";
            //String BaseTextureDirectory = "textures\\instruments\\";
            //String GuitarDirectory = "GibsonSG\\";
            //String DrumsDirectory = "Drums01\\";
            System.IO.BinaryReader fin = new System.IO.BinaryReader(System.IO.File.Open(Filename,System.IO.FileMode.Open,System.IO.FileAccess.Read));

            char[] header = fin.ReadChars(7);

            byte version = fin.ReadByte();
            if (version != 3)
                return;

            cNear = fin.ReadUInt32();
            cFar = fin.ReadUInt32();

            rockers[0].Position = new Vector3(fin.ReadSingle(), fin.ReadSingle(), fin.ReadSingle());
            rockers[1].Position = new Vector3(fin.ReadSingle(), fin.ReadSingle(), fin.ReadSingle());
            rockers[2].Position = new Vector3(fin.ReadSingle(), fin.ReadSingle(), fin.ReadSingle());
            rockers[3].Position = new Vector3(fin.ReadSingle(), fin.ReadSingle(), fin.ReadSingle());

            StaticTexture = new Material[fin.ReadUInt32()];

            for (int i = 0; i < StaticTexture.Length; i++)
            {
                String n = fin.ReadString();
                Texture2D a = Texture2D.FromFile(Global.Graphics.GraphicsDevice,"Content\\textures\\Venues\\"+n+"Tex.png");
                Texture2D c = Texture2D.FromFile(Global.Graphics.GraphicsDevice,"Content\\textures\\Venues\\"+n+"BM.png");
                a.GenerateMipMaps(TextureFilter.Anisotropic);
                c.GenerateMipMaps(TextureFilter.Anisotropic);
                StaticTexture[i] = new Material(a, c);
            }

            StaticGeom = new VenueGeometry[fin.ReadUInt32()];

            for ( int i = 0; i < StaticGeom.Length; i++)
            {
                StaticGeom[i] = new VenueGeometry();
                Vector3 normal = new Vector3(fin.ReadSingle(), fin.ReadSingle(), fin.ReadSingle());
                Vector3 tangent = new Vector3(fin.ReadSingle(), fin.ReadSingle(), fin.ReadSingle());
                Vector3 binormal = Vector3.Cross(normal, tangent);
                StaticGeom[i].texIndex = (int)fin.ReadUInt32();
                VertexTangentBinormal[] buffer = new VertexTangentBinormal[fin.ReadUInt32()];
                for (int k = 0; k < buffer.Length; k++)
                {
                    Vector3 pos = new Vector3(fin.ReadSingle(), fin.ReadSingle(), fin.ReadSingle());
                    Vector2 texCoords = new Vector2(fin.ReadSingle(), fin.ReadSingle());
                    buffer[k] = new VertexTangentBinormal(pos, texCoords, normal, binormal, tangent);
                }
                StaticGeom[i].vb = new VertexBuffer(Global.Graphics.GraphicsDevice, buffer.Length * VertexTangentBinormal.SizeInBytes, BufferUsage.WriteOnly);
                StaticGeom[i].vb.SetData<VertexTangentBinormal>(buffer);
            }

            uint numFans = fin.ReadUInt32();

            Fan.Load(Content);

            Fans = new Fan[numFans];
            for (int i = 0; i < numFans; i++)
            {
                Fans[i] = new Fan(new Vector3(fin.ReadSingle(), fin.ReadSingle(), fin.ReadSingle()));
            }

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

            fin.Close();

        }

        public void Render(GameTime gameTime)
        {
            lastTexApplied=-1;
            Matrix matIdentity, matScale, mMatWorld;

            effect.TextureEnabled = true;
            effect.AmbientMaterial = new Color(50, 50, 50);
            effect.View = GetViewMatrix();
            effect.Projection = GetProjMatrix();
            effect.DirectionalLight = new DirectionalLight(true, new Vector3(1, 3, -1), new Color(0.5f,0.5f,0.5f), Color.White);
            effect.LightingEnabled = Configuration.Lighting;
            effect.NormalMapEnabled = Configuration.NormalMapping;
            effect.SpecularEnabled = Configuration.Specular;

            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();

                for (int i = 0; i < StaticGeom.Length; i++)
                {
                    matIdentity = Matrix.Identity;
                    matScale = Matrix.CreateScale(SCALE);

                    // identity, scale, rotate, orbit(translate & rotate), translate
                    mMatWorld = matIdentity * matScale;
                    effect.World = mMatWorld;
                    effect.Shininess = StaticTexture[StaticGeom[i].texIndex].shininess;
                    effect.DiffuseMaterial = new Color(200, 200, 200);
                    effect.SpecularMaterial = new Color(200, 200, 200);
                    if (lastTexApplied != StaticGeom[i].texIndex)
                    {
                        effect.DiffuseTexture = StaticTexture[StaticGeom[i].texIndex].tex;
                        effect.NormalMapTexture = StaticTexture[StaticGeom[i].texIndex].bm;
                        lastTexApplied = StaticGeom[i].texIndex;
                    }
                    effect.CommitChanges();

                    // 5: draw object - select vertex type, primitive type, # of primitives
                    Global.Graphics.GraphicsDevice.VertexDeclaration = VertexTangentBinormal.VertexDeclaration;
                    Global.Graphics.GraphicsDevice.Vertices[0].SetSource(StaticGeom[i].vb, 0, VertexTangentBinormal.SizeInBytes);
                    Global.Graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleFan, 0, (StaticGeom[i].vb.SizeInBytes / VertexTangentBinormal.SizeInBytes) - 2);
                }
                for (int i = 0; i < Fans.Length; i++)
                {
                    Fans[i].Draw(effect);
                }
                for (int i = 0; i < Entities.Count; i++)
                {
                    Entities[i].Draw(effect, camPos);
                }

                pass.End();
            }
            effect.End();
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

        private float GetBeatTime(SongTime songTime)
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
                float val = (currentTime - start) / (end - start);
                val *= songData.info.barlines[i].numBeats;
                val %= 1.0f;
                return val;
            }
            return 0;
        }
    }
}



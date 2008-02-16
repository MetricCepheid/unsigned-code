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

namespace GarageBand
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

        #region DEBUG_VAR
        public static bool DEBUG_CAM_CONTROL = false;
        private Vector3 DEBUG_cp;
        private Vector2 DEBUG_rot;
        #endregion

        private String Filename;

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

        public Venue(String Filename, Game1 game, ContentManager content, GraphicsDeviceManager graphics, int[] camtimes, Effect e)
        {
            this.Filename = Filename;
            LoadWorld("venues\\"+Filename,game,content,graphics,"Random","Random","Random","Random",e);
            this.camtimes = camtimes;
        }

        public void Update(GameTime gameTime, long songtime, Effect engine)
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
                //TODO: needs work for camtime len
                camindex = rand.Next(CamBlends.Length);
                camtime++;
            }
            else if (camtime < camtimes.Length - 1 && (songtime / (Game1.TicksPerSecond / 1000)) > camtimes[camtime + 1])
            {//sets up camera movement interpolation
                int k;
                do { k = rand.Next(CamBlends.Length); }
                while (k == camindex && CamBlends.Length>1);
                camindex = k;
                camtime++;
            }
            if (camtime >= camtimes.Length-1)//sets flag for new camera
                camblendvalue = -1;
            else//interpolation math:
                camblendvalue = ((songtime / (Game1.TicksPerSecond / 1000)) - camtimes[camtime]) / (float)(camtimes[camtime + 1] - camtimes[camtime]);

            //Dynamic light init
            Vector3[] plPos = new Vector3[16];
            bool[] plOn = new bool[16];
            float[] plNear = new float[16];
            float[] plFar = new float[16];
            Vector3[] plDif = new Vector3[16];
            Vector3[] plSpc = new Vector3[16];
            int pl = 0;

            for (int i = 0; i < Entities.Count; i++)
            {//updates the entities, gets dynamic lighting info
                Entities[i].Update(gameTime);
                if (Entities[i] is LightEntity)
                {
                    LightData l = (Entities[i] as LightEntity).GetLight();
                    if (l.On)
                    {
                        plOn[pl] = l.On;
                        plPos[pl] = l.Pos;
                        plNear[pl] = l.Near;
                        plFar[pl] = l.Far;
                        plDif[pl] = l.Diffuse;
                        plSpc[pl] = l.Specular;
                        pl++;
                    }
                }
            }

            engine.Parameters["pLightPos"].SetValue(plPos);
            engine.Parameters["pLightOn"].SetValue(plOn);
            engine.Parameters["pLightNear"].SetValue(plNear);
            engine.Parameters["pLightFar"].SetValue(plFar);
            engine.Parameters["pLightDiffuse"].SetValue(plDif);
            engine.Parameters["pLightSpecular"].SetValue(plSpc);
            engine.Parameters["dLDiffuseColor"].SetValue(new Vector4(0, 0, 0, 0));
            engine.Parameters["dLSpecularColor"].SetValue(new Vector4(0, 0, 0, 0));
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

        private void LoadWorld(String Filename, Game1 game, ContentManager content, GraphicsDeviceManager graphics, String g, String b, String d, String v, Effect e) 
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
                        CamBlends[index[k]-1].TYPE = (CamBlendPos.TYPE_LEN)tpe[index[k]-1];
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
            }

            fin.ReadChars(1);// }

            /*OLD CODE FOLLOWS... pre-rewrite
            while (z.Length>=2 && z.Substring(0, 2).Equals("//"));
            StaticWorld = new VertexBuffer[Int32.Parse(z)];

            for(int c=0;c<StaticWorld.Length;c++)
            {
                GBVertexFormat[] buffer;
                String a;
                do{a = reader.ReadLine();}
                while (a.Length >= 2 && a.Substring(0, 2).Equals("//"));
                buffer = new GBVertexFormat[Int32.Parse(a)*3];
                String[] r = { reader.ReadLine(), reader.ReadLine(), reader.ReadLine(), reader.ReadLine(), reader.ReadLine(), reader.ReadLine(), reader.ReadLine(), reader.ReadLine(), reader.ReadLine(), reader.ReadLine(), reader.ReadLine()};
                for (int k = 0; k < 5; k++)
                    r[k] = r[k].Trim();
                for (int i = 0; i < buffer.Length; i++)
                {
                    buffer[i] = new GBVertexFormat(new Vector3((float)Double.Parse(r[0].Substring(0, r[0].IndexOf(','))),
                                                               (float)Double.Parse(r[1].Substring(0, r[1].IndexOf(','))),
                                                               (float)Double.Parse(r[2].Substring(0, r[2].IndexOf(',')))),
                                                   new Vector3((float)Double.Parse(r[5].Substring(0, r[5].IndexOf(','))),
                                                               (float)Double.Parse(r[6].Substring(0, r[6].IndexOf(','))),
                                                               (float)Double.Parse(r[7].Substring(0, r[7].IndexOf(',')))),
                                                   new Vector2((float)Double.Parse(r[3].Substring(0, r[3].IndexOf(','))),
                                                               (float)Double.Parse(r[4].Substring(0, r[4].IndexOf(',')))),
                                                   new Vector3((float)Double.Parse(r[8].Substring(0, r[8].IndexOf(','))),
                                                               (float)Double.Parse(r[9].Substring(0, r[9].IndexOf(','))),
                                                               (float)Double.Parse(r[10].Substring(0, r[10].IndexOf(',')))));
                    for (int k = 0; k <= 10; k++)
                        r[k] = r[k].Substring(r[k].IndexOf(',')+1).Trim();
                }
                StaticWorld[c] = new VertexBuffer(graphics.GraphicsDevice, GBVertexFormat.SizeInBytes * buffer.Length, BufferUsage.WriteOnly);
                StaticWorld[c].SetData<GBVertexFormat>(buffer);
            }

            do { z = reader.ReadLine(); }
            while (z.Length >= 2 && z.Substring(0, 2).Equals("//"));
            ModelArray = new Model[Int32.Parse(z)];

            for (int c = 0; c < ModelArray.Length; c++)
            {
                String a;
                do { a = reader.ReadLine(); }
                while (a.Length >= 2 && a.Substring(0, 2).Equals("//"));
                ModelArray[c] = content.Load<Model>(a);
                ModelArray[c].Tag = a.Substring(a.LastIndexOf('\\') + 1).Trim();
            }

            do { z = reader.ReadLine(); }
            while (z.Length >= 2 && z.Substring(0, 2).Equals("//"));
            StaticWorldArray = new StaticWorldObject[Int32.Parse(z)];
            for (int i = 0; i < StaticWorldArray.Length; i++)
            {
                String a;
                do { a = reader.ReadLine(); }
                while (a.Length >= 2 && a.Substring(0, 2).Equals("//"));
                StaticWorldArray[i].ModelType = a.ToCharArray()[0];
                StaticWorldArray[i].ModelIndex = Int32.Parse(a.Substring(1));
                do { a = reader.ReadLine(); }
                while (a.Length >= 2 && a.Substring(0, 2).Equals("//"));
                a = a.Trim();
                StaticWorldArray[i].x = Int32.Parse(a.Substring(0, a.IndexOf(',')));
                a = a.Substring(a.IndexOf(',')+1).Trim();
                StaticWorldArray[i].y = Int32.Parse(a.Substring(0, a.IndexOf(',')));
                a = a.Substring(a.IndexOf(',') + 1).Trim();
                StaticWorldArray[i].z = Int32.Parse(a.Substring(0, a.IndexOf(',')));
                a = a.Substring(a.IndexOf(',') + 1).Trim();
                StaticWorldArray[i].Orientation = Int32.Parse(a.Substring(0, a.IndexOf(',')));
                a = a.Substring(a.IndexOf(',') + 1).Trim();
                StaticWorldArray[i].Shininess = (float)Double.Parse(a);
                do { a = reader.ReadLine(); }
                while (a.Length >= 2 && a.Substring(0, 2).Equals("//"));
                StaticWorldArray[i].TextureIndex = Int32.Parse(a);
                do { a = reader.ReadLine(); }
                while (a.Length >= 2 && a.Substring(0, 2).Equals("//"));
                StaticWorldArray[i].BMIndex = Int32.Parse(a);
                StaticWorldArray[i].Shininess = 2f;
            }

            do { z = reader.ReadLine(); }
            while (z.Length >= 2 && z.Substring(0, 2).Equals("//"));
            StaticTexture = new Texture2D[Int32.Parse(z)];
            for (int i = 0; i < StaticTexture.Length; i++)
            {
                String a;
                do { a = reader.ReadLine(); }
                while (a.Length >= 2 && a.Substring(0, 2).Equals("//"));
                StaticTexture[i] = content.Load<Texture2D>(a);
                StaticTexture[i].Tag = a.Substring(a.LastIndexOf('\\') + 1).Trim();
            }

            do { z = reader.ReadLine(); }
            while (z.Length >= 2 && z.Substring(0, 2).Equals("//"));
            CamBlends = new CamBlendPos[Int32.Parse(z)];
            for (int i = 0; i < CamBlends.Length; i++)
            {
                CamBlends[i].pos1 = new Vector3();
                CamBlends[i].pos2 = new Vector3();
                CamBlends[i].focus1 = new Vector3();
                CamBlends[i].focus2 = new Vector3();
                String a;
                do { a = reader.ReadLine(); }
                while (a.Length >= 2 && a.Substring(0, 2).Equals("//"));
                a = a.Trim();
                CamBlends[i].pos2.X = Int32.Parse(a.Substring(0, a.IndexOf(',')));
                a = a.Substring(a.IndexOf(',') + 1).Trim();
                CamBlends[i].pos2.Y = Int32.Parse(a.Substring(0, a.IndexOf(',')));
                a = a.Substring(a.IndexOf(',') + 1).Trim();
                CamBlends[i].pos2.Z = Int32.Parse(a.Substring(0, a.IndexOf(',')));
                a = a.Substring(a.IndexOf(',') + 1).Trim();
                CamBlends[i].focus2.X = Int32.Parse(a.Substring(0, a.IndexOf(',')));
                a = a.Substring(a.IndexOf(',') + 1).Trim();
                CamBlends[i].focus2.Y = Int32.Parse(a.Substring(0, a.IndexOf(',')));
                a = a.Substring(a.IndexOf(',') + 1).Trim();
                CamBlends[i].focus2.Z = Int32.Parse(a.Substring(0, a.IndexOf(',')));
                do { a = reader.ReadLine(); }
                while (a.Length >= 2 && a.Substring(0, 2).Equals("//"));
                a = a.Trim();
                CamBlends[i].pos1.X = Int32.Parse(a.Substring(0, a.IndexOf(',')));
                a = a.Substring(a.IndexOf(',') + 1).Trim();
                CamBlends[i].pos1.Y = Int32.Parse(a.Substring(0, a.IndexOf(',')));
                a = a.Substring(a.IndexOf(',') + 1).Trim();
                CamBlends[i].pos1.Z = Int32.Parse(a.Substring(0, a.IndexOf(',')));
                a = a.Substring(a.IndexOf(',') + 1).Trim();
                CamBlends[i].focus1.X = Int32.Parse(a.Substring(0, a.IndexOf(',')));
                a = a.Substring(a.IndexOf(',') + 1).Trim();
                CamBlends[i].focus1.Y = Int32.Parse(a.Substring(0, a.IndexOf(',')));
                a = a.Substring(a.IndexOf(',') + 1).Trim();
                CamBlends[i].focus1.Z = Int32.Parse(a.Substring(0, a.IndexOf(',')));
            }
            do { z = reader.ReadLine(); }
            while (z.Length >= 2 && z.Substring(0, 2).Equals("//"));
            int num = Int32.Parse(z);
            CharLocs = new Vector3[4][];
            for (int i = 0; i < 4; i++)
                CharLocs[i] = new Vector3[TYPE_NUM];
            for (int i = 0; i < num; i++)
            {
                String a;
                do { a = reader.ReadLine(); }
                while (a.Length >= 2 && a.Substring(0, 2).Equals("//"));
                a = a.Trim();
                int index = -1;
                for (int k = 0; k < TYPE_NUM; k++)
                    if (TYPES_S[k].Equals(a))
                        index = k;
                do { a = reader.ReadLine(); }
                while (a.Length >= 2 && a.Substring(0, 2).Equals("//"));
                a = a.Trim();
                CharLocs[0][index].X = Int32.Parse(a.Substring(0, a.IndexOf(',')));
                a = a.Substring(a.IndexOf(',') + 1).Trim();
                CharLocs[0][index].Y = Int32.Parse(a.Substring(0, a.IndexOf(',')));
                a = a.Substring(a.IndexOf(',') + 1).Trim();
                CharLocs[0][index].Z = Int32.Parse(a.Substring(0, a.IndexOf(',')));
                do { a = reader.ReadLine(); }
                while (a.Length >= 2 && a.Substring(0, 2).Equals("//"));
                a = a.Trim();
                CharLocs[1][index].X = Int32.Parse(a.Substring(0, a.IndexOf(',')));
                a = a.Substring(a.IndexOf(',') + 1).Trim();
                CharLocs[1][index].Y = Int32.Parse(a.Substring(0, a.IndexOf(',')));
                a = a.Substring(a.IndexOf(',') + 1).Trim();
                CharLocs[1][index].Z = Int32.Parse(a.Substring(0, a.IndexOf(',')));
                do { a = reader.ReadLine(); }
                while (a.Length >= 2 && a.Substring(0, 2).Equals("//"));
                a = a.Trim();
                CharLocs[2][index].X = Int32.Parse(a.Substring(0, a.IndexOf(',')));
                a = a.Substring(a.IndexOf(',') + 1).Trim();
                CharLocs[2][index].Y = Int32.Parse(a.Substring(0, a.IndexOf(',')));
                a = a.Substring(a.IndexOf(',') + 1).Trim();
                CharLocs[2][index].Z = Int32.Parse(a.Substring(0, a.IndexOf(',')));
                do { a = reader.ReadLine(); }
                while (a.Length >= 2 && a.Substring(0, 2).Equals("//"));
                a = a.Trim();
                CharLocs[3][index].X = Int32.Parse(a.Substring(0, a.IndexOf(',')));
                a = a.Substring(a.IndexOf(',') + 1).Trim();
                CharLocs[3][index].Y = Int32.Parse(a.Substring(0, a.IndexOf(',')));
                a = a.Substring(a.IndexOf(',') + 1).Trim();
                CharLocs[3][index].Z = Int32.Parse(a.Substring(0, a.IndexOf(',')));
            }
            do { z = reader.ReadLine(); }
            while (z.Length >= 2 && z.Substring(0, 2).Equals("//"));
            int numDO = Int32.Parse(z);
            Entities = new Entity[numDO];
            for (int i = 0; i < numDO; i++)
            {
                String a;
                do { a = reader.ReadLine(); }
                while (a.Length >= 2 && a.Substring(0, 2).Equals("//"));
                a = a.Trim();
                String tp = a;
                if (tp.Equals("Swinger"))
                {
                    SwingingEntity obj;
                    do { a = reader.ReadLine(); }
                    while (a.Length >= 2 && a.Substring(0, 2).Equals("//"));
                    a = a.Trim();
                    String mdl = a;
                    do { a = reader.ReadLine(); }
                    while (a.Length >= 2 && a.Substring(0, 2).Equals("//"));
                    a = a.Trim();
                    String tex = a;
                    do { a = reader.ReadLine(); }
                    while (a.Length >= 2 && a.Substring(0, 2).Equals("//"));
                    a = a.Trim();
                    float x, y, zz;
                    x = (float)Double.Parse(a.Substring(0, a.IndexOf(',')));
                    a = a.Substring(a.IndexOf(',') + 1).Trim();
                    y = (float)Double.Parse(a.Substring(0, a.IndexOf(',')));
                    a = a.Substring(a.IndexOf(',') + 1).Trim();
                    zz = (float)Double.Parse(a.Substring(0, a.IndexOf(',')));
                    obj = new SwingingEntity(mdl,tex, new Vector3(x, y, zz));
                    do { a = reader.ReadLine(); }
                    while (a.Length >= 2 && a.Substring(0, 2).Equals("//"));
                    a = a.Trim();
                    x = (float)Double.Parse(a.Substring(0, a.IndexOf(',')));
                    a = a.Substring(a.IndexOf(',') + 1).Trim();
                    y = (float)Double.Parse(a.Substring(0, a.IndexOf(',')));
                    a = a.Substring(a.IndexOf(',') + 1).Trim();
                    zz = (float)Double.Parse(a.Substring(0, a.IndexOf(',')));
                    obj.SetSwing(x, y, zz);
                    bool fb, em;
                    do { a = reader.ReadLine(); }
                    while (a.Length >= 2 && a.Substring(0, 2).Equals("//"));
                    a = a.Trim();
                    fb = Boolean.Parse(a.Substring(0, a.IndexOf(',')));
                    a = a.Substring(a.IndexOf(',') + 1).Trim();
                    em = Boolean.Parse(a.Substring(0, a.IndexOf(',')));
                    do { a = reader.ReadLine(); }
                    while (a.Length >= 2 && a.Substring(0, 2).Equals("//"));
                    a = a.Trim();
                    x = (float)Double.Parse(a.Substring(0, a.IndexOf(',')));
                    a = a.Substring(a.IndexOf(',') + 1).Trim();
                    y = (float)Double.Parse(a.Substring(0, a.IndexOf(',')));
                    a = a.Substring(a.IndexOf(',') + 1).Trim();
                    zz = (float)Double.Parse(a.Substring(0, a.IndexOf(',')));
                    obj.SetLightData(fb, em, new Vector3(x, y, zz));
                    Entities[i] = obj;
                }
            }*/
        }

        public void Render(GraphicsDeviceManager graphics, Effect engine, Matrix matProj,
                           VertexDeclaration vd, GameTime gameTime)
        {
            lastTexApplied=-1;
            Matrix matIdentity, matTransl, matScale, matRot, matOrbit, mMatWorld;

            engine.Parameters["ambientColor"].SetValue(new Vector4(.5f, .5f, .5f, 1f));
            engine.Parameters["fullbright"].SetValue(true);

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
                    engine.Parameters["specularColor"].SetValue(new Vector4(.8f, .8f, .8f, 1f));
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



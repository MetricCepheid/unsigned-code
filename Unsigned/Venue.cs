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

    public interface DynamicWorldObject
    {
        void Update(GameTime gameTime);
        void Draw(Effect engine, GraphicsDeviceManager graphics, Vector3 CamPos);
        Light GetLight();
    }

    

    public struct CamBlendPos
    {
        public Vector3 pos1, pos2;
        public Vector3 focus1, focus2;
    }

    public struct Light
    {
        public Vector3 Pos;
        public bool On;
        public float Near, Far;
        public Vector3 Diffuse;
        public Vector3 Specular;
    }

    class Venue
    {
        protected struct Swinger : DynamicWorldObject
        {
            private int model, texture;
            private float swingAmt, rotSpd, ovalish;
            private Vector3 loc;
            private bool fullBright, emmissive;
            private Vector3 relLightLoc;
            private Matrix staticRot;
            private float rotVal;
            private bool glow;

            public Swinger(int model,int tex, Vector3 loc)
            {
                this.model = model;
                texture = tex;
                this.loc = loc;
                staticRot = Matrix.Identity;
                swingAmt = 0;
                rotSpd = 0;
                ovalish = 1;
                fullBright=false;
                emmissive=false;
                relLightLoc=Vector3.Zero;
                rotVal = 0;
                glow = false;
            }
            public Swinger(String model, String tex, Vector3 loc)
            {
                this.model = -1;
                for (int i = 0; i < ModelArray.Length; i++)
                {
                    if (((String)(ModelArray[i].Tag)).Equals(model))
                    {
                        this.model = i;
                        break;
                    }
                }
                texture = -1;
                for (int i = 0; i < StaticTexture.Length; i++)
                {
                    if (((String)StaticTexture[i].Tag).Equals(tex))
                    {
                        texture = i;
                        break;
                    }
                }
                this.loc = loc;
                staticRot = Matrix.Identity;
                swingAmt = 0;
                rotSpd = 0;
                ovalish = 1;
                fullBright=false;
                emmissive=false;
                relLightLoc=Vector3.Zero;
                rotVal = 0;
                glow = false;
            }

            public void SetSwing(float amt, float rSpd, float oval)
            {
                swingAmt = amt;
                rotSpd = rSpd;
                ovalish = oval;
            }

            public void Update(GameTime gameTime)
            {
                rotVal += rotSpd * (gameTime.ElapsedGameTime.Milliseconds / 1000f);
            }

            public Light GetLight()
            {
                if (emmissive)
                {
                    Matrix matRot = Matrix.CreateRotationX(swingAmt) * Matrix.CreateRotationY(rotVal);
                    Light l = new Light();
                    l.On = true;
                    l.Near = 128;
                    l.Far = 512;
                    l.Diffuse = new Vector3(1, 1, 0.9f);
                    l.Specular = new Vector3(1, 1, 0.8f);
                    l.Pos = relLightLoc;
                    l.Pos = Vector3.Transform(l.Pos, matRot);
                    l.Pos += loc;
                    return l;
                }
                return new Light();
            }

            public void Draw(Effect engine, GraphicsDeviceManager graphics, Vector3 CamPos)
            {
                Matrix matIdentity = Matrix.Identity;
                float xval = (float)Game1.dirdistTOhdist(rotVal * 180 / Math.PI,swingAmt);
                float yval = (float)Game1.dirdistTOvdist(rotVal * 180 / Math.PI, swingAmt);
                xval *= ovalish;
                Matrix matRot = Matrix.CreateRotationX(xval) * Matrix.CreateRotationZ(yval);
                Matrix matTransl = Matrix.CreateTranslation(loc);
                Matrix matScale = Matrix.CreateScale(SCALE);

                // identity, scale, rotate, orbit(translate & rotate), translate
                Matrix matWorld = matIdentity * matScale * matRot * matTransl;
                if (fullBright)
                    engine.Parameters["fullbright"].SetValue(true);
                engine.Parameters["world"].SetValue(matWorld);
                engine.Parameters["wRot"].SetValue(matRot);
                engine.Parameters["diffuseTexture"].SetValue(StaticTexture[texture]);
                engine.Parameters["bumpTexture"].SetValue(Game1.texDefaultBM);
                engine.CommitChanges();

                foreach (ModelMesh mesh in ModelArray[model].Meshes)
                {
                    foreach (ModelMeshPart meshpart in mesh.MeshParts)
                    {
                        graphics.GraphicsDevice.VertexDeclaration = meshpart.VertexDeclaration;
                        graphics.GraphicsDevice.Vertices[0].SetSource(mesh.VertexBuffer, meshpart.StreamOffset, meshpart.VertexStride);
                        graphics.GraphicsDevice.Indices = mesh.IndexBuffer;
                        graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, meshpart.BaseVertex, 0, meshpart.NumVertices, meshpart.StartIndex, meshpart.PrimitiveCount);
                    }
                }

                if (glow)
                {
                    Matrix matSubTransl = Matrix.CreateTranslation(relLightLoc);
                    matRot = Matrix.CreateRotationZ(MathHelper.PiOver2);
                    Matrix matRot2 = Matrix.CreateRotationX(xval) * Matrix.CreateRotationZ(yval);
                    Vector3 diff =(Vector3.Transform(relLightLoc, matRot2) + loc)-CamPos;
                    Matrix matOrbitB = Matrix.CreateRotationX((float)Math.Atan2(-diff.Y, Math.Sqrt(diff.X * diff.X + diff.Z * diff.Z))) * Matrix.CreateRotationY(-(float)Math.Atan2(diff.Z, diff.X));
                    Matrix matOrbitA = Matrix.CreateTranslation(new Vector3(0, 0, 8));
                    matTransl = Matrix.CreateTranslation(loc);
                    matScale = Matrix.CreateScale(32);

                    // identity, scale, rotate, orbit(translate & rotate), translate
                    matWorld = matIdentity * matScale * matRot * (matOrbitA*matOrbitB) * (matSubTransl * matRot2) * matTransl;
                    engine.Parameters["fullbright"].SetValue(true);
                    engine.Parameters["world"].SetValue(matWorld);
                    engine.Parameters["wRot"].SetValue(matRot*matOrbitB);
                    engine.Parameters["diffuseTexture"].SetValue(Game1.texGlow);
                    engine.Parameters["bumpTexture"].SetValue(Game1.texDefaultBM);
                    engine.CommitChanges();

                    // 5: draw object - select vertex type, primitive type, # of primitives
                    graphics.GraphicsDevice.VertexDeclaration = Game1.vd;
                    graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                    graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                    graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                    graphics.GraphicsDevice.Vertices[0].SetSource(Game1.square, 0, GBVertexFormat.SizeInBytes);
                    graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
                    graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                }
                engine.Parameters["fullbright"].SetValue(false);
            }

            internal void SetLightData(bool fb, bool em, Vector3 pos)
            {
                fullBright = fb;
                emmissive = em;
                relLightLoc = pos;
                glow = true;
            }
        }

        private VertexBuffer[] StaticWorld;
        private static Texture2D[] StaticTexture;
        private StaticWorldObject[] StaticWorldArray; 
        private DynamicWorldObject[] DynamicWorldArray;
        private static Model[] ModelArray;
        private CamBlendPos[] CamBlends;
        private Vector3 camPos, camUp, camFor;
        public static bool DEBUG_CAM_CONTROL = false;
        private Vector3 DEBUG_cp;
        private Vector2 DEBUG_rot;
        private String Filename;
        public int camindex;
        private long camtime = -1;
        private float camblendvalue;
        private static Random rand = null;
        private int[] camtimes;
        private Model guitar;
        private Model[] drumset;
        private Texture[] drumsetT;
        private static int DS_BASSDRUM = 0, DS_CRASHCYMBAL = 1, DS_RIDECYMBAL = 2, DS_HIHATCYMBAL = 3, DS_FLOORTOM = 4, DS_TOMTOMS = 5, DS_SNARE = 6;
        private Rocker guitarist,bassist,drummer,vocalist;
        private Light[] lights;

        #region dynamicmodels

        private VertexBuffer mdlGuitarist, mdlBassist, mdlDrummer, mdlSinger;
        private Texture2D texGuitarist, texBassist, texDrummer, texSinger;

        #endregion

        //insongtypes
        public static byte TYPE_NUM = 1, TYPE_NORMAL=0;
        public static String[] TYPES_S = { "NORMAL" };

        private Vector3[][] CharLocs;

        private Vector3 locGuitarist, locBassist, locDrummer, locSinger;

        public static float SCALE = 1f;

        public Venue(String Filename, Game1 game, ContentManager content, GraphicsDeviceManager graphics, int[] camtimes, Effect e)
        {
            this.Filename = Filename;
            LoadWorld("venues\\"+Filename,game,content,graphics,"Louis","Random","Random","Random",e);
            LoadDynamicModels(content,graphics);
            if (rand == null)
                rand = new Random((int)DateTime.Now.Ticks);
            this.camtimes = camtimes;
            lights = new Light[4];
            lights[0] = new Light();
            lights[0].Far = 500f;
            lights[0].Near = 256f;
            lights[0].On = true;
            lights[0].Pos = new Vector3(0, 150, 100);
            DEBUG_rot = new Vector2(0, 0);
        }

        public void Reload(Game1 game,ContentManager content, GraphicsDeviceManager graphics, Effect e)
        {
            LoadWorld(Filename, game, content, graphics, guitarist.GetName(), bassist.GetName(), drummer.GetName(), vocalist.GetName(),e);
        }

        public void Update(GameTime gameTime, long songtime, Effect engine)
        {
            if (DEBUG_CAM_CONTROL)
            {
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
            }

            if (camtime==-1)
            {
                camindex = rand.Next(CamBlends.Length);
                camtime++;
            }
            else if (camtime < camtimes.Length - 1 && (songtime / (Game1.TicksPerSecond / 1000)) > camtimes[camtime + 1])
            {
                int k;
                do { k = rand.Next(CamBlends.Length); }
                while (k == camindex && CamBlends.Length>1);
                camindex = k;
                camtime++;
            }
            if (camtime >= camtimes.Length-1)
                camblendvalue = -1;
            else
                camblendvalue = ((songtime / (Game1.TicksPerSecond / 1000)) - camtimes[camtime]) / (float)(camtimes[camtime + 1] - camtimes[camtime]);
            locGuitarist = CharLocs[0][0];
            locBassist = CharLocs[1][0];
            locDrummer = CharLocs[2][0];
            locSinger = CharLocs[3][0];

            Vector3[] plPos = new Vector3[16];
            bool[] plOn = new bool[16];
            float[] plNear = new float[16];
            float[] plFar = new float[16];
            Vector3[] plDif = new Vector3[16];
            Vector3[] plSpc = new Vector3[16];
            int pl = 0;
            for (int i = 0; i < DynamicWorldArray.Length; i++)
            {
                DynamicWorldArray[i].Update(gameTime);
                Light l = DynamicWorldArray[i].GetLight();
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
                Vector3 cp = new Vector3(0f, 0f, 0f), ct = new Vector3(0f, 1f, 0f), cu = new Vector3(0f, 1f, 0f);
                if (CamBlends.Length < 1)
                    return Matrix.CreateLookAt(cp, ct, cu);
                if (camblendvalue < 0)
                {
                    cp = new Vector3(0, 100, -200);
                    ct = new Vector3(0, 75, 0);
                }
                else
                {
                    cp = new Vector3((CamBlends[camindex].pos1.X * camblendvalue) + (CamBlends[camindex].pos2.X * (1 - camblendvalue)), (CamBlends[camindex].pos1.Y * camblendvalue) + (CamBlends[camindex].pos2.Y * (1 - camblendvalue)), (CamBlends[camindex].pos1.Z * camblendvalue) + (CamBlends[camindex].pos2.Z * (1 - camblendvalue)));
                    ct = new Vector3((CamBlends[camindex].focus1.X * camblendvalue) + (CamBlends[camindex].focus2.X * (1 - camblendvalue)), (CamBlends[camindex].focus1.Y * camblendvalue) + (CamBlends[camindex].focus2.Y * (1 - camblendvalue)), (CamBlends[camindex].focus1.Z * camblendvalue) + (CamBlends[camindex].focus2.Z * (1 - camblendvalue)));
                }
                cu = new Vector3(0f, 1f, 0f);
                camPos = cp;
                camUp = cu;
                camFor = ct;
                //cp = new Vector3(0f, 400f, 99f);
                //ct = new Vector3(0f, 180f, 100f);
                return Matrix.CreateLookAt(cp, ct, cu);
            }
        }

        private void LoadWorld(String Filename, Game1 game, ContentManager content, GraphicsDeviceManager graphics, String g, String b, String d, String v, Effect e) 
        {
            guitar = content.Load<Model>("meshes\\gibsonsg");
            guitarist = new Rocker("rockers\\"+g,game,content,e);
            bassist = new Rocker("rockers\\"+b,game,content,e);
            drummer = new Rocker("rockers\\"+d,game,content,e);
            vocalist = new Rocker("rockers\\"+v,game,content,e);
            drumset = new Model[7];
            drumsetT = new Texture2D[7];
            drumset[DS_BASSDRUM] = content.Load<Model>("meshes\\bassdrum01");
            drumsetT[DS_BASSDRUM] = content.Load<Texture2D>("graphics\\bassdrum01");
            drumset[DS_CRASHCYMBAL] = content.Load<Model>("meshes\\crashcymbal01");
            drumsetT[DS_CRASHCYMBAL] = content.Load<Texture2D>("graphics\\crashcymbal01");
            drumset[DS_FLOORTOM] = content.Load<Model>("meshes\\floortom01");
            drumsetT[DS_FLOORTOM] = content.Load<Texture2D>("graphics\\floortom01");
            drumset[DS_HIHATCYMBAL] = content.Load<Model>("meshes\\hihatcymbal01");
            drumsetT[DS_HIHATCYMBAL] = content.Load<Texture2D>("graphics\\hihatcymbal01");
            drumset[DS_RIDECYMBAL] = content.Load<Model>("meshes\\ridecymbal01");
            drumsetT[DS_RIDECYMBAL] = content.Load<Texture2D>("graphics\\ridecymbal01");
            drumset[DS_SNARE] = content.Load<Model>("meshes\\snaredrum01");
            drumsetT[DS_SNARE] = content.Load<Texture2D>("graphics\\snaredrum01");
            drumset[DS_TOMTOMS] = content.Load<Model>("meshes\\tomtoms01");
            drumsetT[DS_TOMTOMS] = content.Load<Texture2D>("graphics\\tomtoms01");

            if (!System.IO.File.Exists(Filename))
                return;
            System.IO.StreamReader reader = new System.IO.StreamReader(Filename);
            String z;
            do { z = reader.ReadLine(); }
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
            DynamicWorldArray = new DynamicWorldObject[numDO];
            for (int i = 0; i < numDO; i++)
            {
                String a;
                do { a = reader.ReadLine(); }
                while (a.Length >= 2 && a.Substring(0, 2).Equals("//"));
                a = a.Trim();
                String tp = a;
                if (tp.Equals("Swinger"))
                {
                    Swinger obj;
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
                    obj = new Swinger(mdl,tex, new Vector3(x, y, zz));
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
                    DynamicWorldArray[i] = obj;
                }
            }
        }

        public void Render(GraphicsDeviceManager graphics, Effect engine, Matrix matProj,
                           VertexDeclaration vd, GameTime gameTime)
        {

            Matrix matIdentity, matTransl, matScale, matRot, matOrbit, mMatWorld;

            for (int i = 0; i < StaticWorldArray.Length; i++)
            {
                matIdentity = Matrix.Identity;
                matTransl = Matrix.CreateTranslation(StaticWorldArray[i].x, StaticWorldArray[i].y, StaticWorldArray[i].z);
                matScale = Matrix.CreateScale(SCALE);
                matRot = Matrix.CreateRotationY((float)(StaticWorldArray[i].Orientation / 180f * Math.PI));

                // identity, scale, rotate, orbit(translate & rotate), translate
                mMatWorld = matIdentity * matScale * matRot * matTransl;
                if (StaticWorldArray[i].ModelType == 'c')
                {
                    engine.Parameters["world"].SetValue(mMatWorld);
                    engine.Parameters["wRot"].SetValue(matRot);
                    engine.Parameters["shininess"].SetValue(StaticWorldArray[i].Shininess);
                    engine.Parameters["diffuseTexture"].SetValue(StaticTexture[StaticWorldArray[i].TextureIndex]);
                    engine.Parameters["bumpTexture"].SetValue(StaticTexture[StaticWorldArray[i].BMIndex]);
                    engine.CommitChanges();

                    // 5: draw object - select vertex type, primitive type, # of primitives
                    graphics.GraphicsDevice.VertexDeclaration = vd;
                    graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                    graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                    graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                    graphics.GraphicsDevice.Vertices[0].SetSource(StaticWorld[StaticWorldArray[i].ModelIndex], 0, GBVertexFormat.SizeInBytes);
                    graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, (StaticWorld[StaticWorldArray[i].ModelIndex].SizeInBytes/GBVertexFormat.SizeInBytes)/3);
                    graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                }
                else
                {
                    /*foreach (ModelMesh mesh in ModelArray[StaticWorldArray[i].ModelIndex].Meshes)
                    {
                        foreach (BasicEffect effect in mesh.Effects)
                        {
                            effect.EnableDefaultLighting();

                            effect.View = mMatView;
                            effect.Projection = mMatProj;
                            effect.World = Matrix.CreateScale(10f)*mMatWorld;
                        }
                        mesh.Draw(SaveStateMode.SaveState);
                    }*/
                }
            }
            for (int i = 0; i < DynamicWorldArray.Length; i++)
            {
                DynamicWorldArray[i].Draw(engine,graphics,camPos);
            }
            engine.Parameters["vertexAlpha"].SetValue(false);
            engine.Parameters["BumpMappingEnabled"].SetValue(false);
            //engine.Parameters["SpecularEnabled"].SetValue(false);
            graphics.GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
            {//guitarist
                matIdentity = Matrix.Identity;
                matTransl = Matrix.CreateTranslation(locGuitarist.X, locGuitarist.Y, locGuitarist.Z);
                matScale = Matrix.CreateScale(SCALE*16);

                // identity, scale, rotate, orbit(translate & rotate), translate
                mMatWorld = matIdentity * matScale * matTransl;

                engine.Parameters["world"].SetValue(mMatWorld);
                engine.Parameters["wRot"].SetValue(Matrix.Identity);
                engine.Parameters["diffuseTexture"].SetValue(texGuitarist);
                engine.CommitChanges();

                matIdentity = Matrix.Identity;
                matTransl = Matrix.CreateTranslation(locGuitarist.X, locGuitarist.Y + 50, locGuitarist.Z-20);
                matRot = Matrix.CreateRotationX(-(float)Math.PI / 2) * Matrix.CreateRotationZ(-(float)Math.PI / 8 * 5);
                matScale = Matrix.CreateScale(SCALE * 4);

                guitarist.Draw(gameTime, mMatWorld,graphics);

                // identity, scale, rotate, orbit(translate & rotate), translate
                mMatWorld = matIdentity * matScale * matRot * matTransl;

                /*foreach (ModelMesh mesh in guitar.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.EnableDefaultLighting();

                        effect.View = mMatView;
                        effect.Projection = mMatProj;
                        effect.World = mMatWorld;
                    }
                    mesh.Draw(SaveStateMode.SaveState);
                }*/
                
            }//guitarist
            {//Bassist
                matIdentity = Matrix.Identity;
                matTransl = Matrix.CreateTranslation(locBassist.X, locBassist.Y, locBassist.Z);
                matScale = Matrix.CreateScale(SCALE * 16);

                // identity, scale, rotate, orbit(translate & rotate), translate
                mMatWorld = matIdentity * matScale * matTransl;

                engine.Parameters["world"].SetValue(mMatWorld);
                engine.Parameters["wRot"].SetValue(Matrix.Identity);
                engine.Parameters["diffuseTexture"].SetValue(texBassist);
                engine.CommitChanges();

                // 5: draw object - select vertex type, primitive type, # of primitives
                graphics.GraphicsDevice.VertexDeclaration = vd;
                graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;

                bassist.Draw(gameTime, mMatWorld,graphics);
            }//Bassist
            {//Drummer
                matIdentity = Matrix.Identity;
                matTransl = Matrix.CreateTranslation(locDrummer.X, locDrummer.Y, locDrummer.Z);
                matScale = Matrix.CreateScale(SCALE * 16);

                // identity, scale, rotate, orbit(translate & rotate), translate
                mMatWorld = matIdentity * matScale * matTransl;

                drummer.Draw(gameTime, mMatWorld, graphics);
                engine.Parameters["vertexAlpha"].SetValue(false);

                //BASS DRUM
                matIdentity = Matrix.Identity;
                matTransl = Matrix.CreateTranslation(locDrummer.X, locDrummer.Y, locDrummer.Z);
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

                foreach (ModelMesh mesh in drumset[DS_BASSDRUM].Meshes)
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
                matTransl = Matrix.CreateTranslation(locDrummer.X, locDrummer.Y, locDrummer.Z);
                matOrbit = Matrix.CreateTranslation(0, 0, -65 * SCALE) * Matrix.CreateRotationY(drummer.Rot-(float)(Math.PI/5.5));
                matScale = Matrix.CreateScale(SCALE * 128);

                // identity, scale, rotate, orbit(translate & rotate), translate
                mMatWorld = matIdentity * matScale * matRot * matOrbit * matTransl;

                engine.Parameters["world"].SetValue(mMatWorld);
                engine.Parameters["wRot"].SetValue(Matrix.Identity);
                engine.Parameters["diffuseTexture"].SetValue(drumsetT[DS_CRASHCYMBAL]);
                engine.Parameters["bumpTexture"].SetValue(Game1.texDefaultBM);
                engine.CommitChanges();

                foreach (ModelMesh mesh in drumset[DS_CRASHCYMBAL].Meshes)
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
                matTransl = Matrix.CreateTranslation(locDrummer.X, locDrummer.Y, locDrummer.Z);
                matOrbit = Matrix.CreateTranslation(0, 0, -40 * SCALE) * Matrix.CreateRotationY(drummer.Rot - (float)(Math.PI / 5.5));
                matScale = Matrix.CreateScale(SCALE * 128);

                // identity, scale, rotate, orbit(translate & rotate), translate
                mMatWorld = matIdentity * matScale * matRot * matOrbit * matTransl;

                engine.Parameters["world"].SetValue(mMatWorld);
                engine.Parameters["wRot"].SetValue(Matrix.Identity);
                engine.Parameters["diffuseTexture"].SetValue(drumsetT[DS_FLOORTOM]);
                engine.Parameters["bumpTexture"].SetValue(Game1.texDefaultBM);
                engine.CommitChanges();

                foreach (ModelMesh mesh in drumset[DS_FLOORTOM].Meshes)
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
                matTransl = Matrix.CreateTranslation(locDrummer.X, locDrummer.Y, locDrummer.Z);
                matOrbit = Matrix.CreateTranslation(0, 32 * SCALE, -50 * SCALE) * Matrix.CreateRotationY(drummer.Rot+0.04f);
                matScale = Matrix.CreateScale(SCALE * 128);

                // identity, scale, rotate, orbit(translate & rotate), translate
                mMatWorld = matIdentity * matScale * matOrbit * matTransl;

                engine.Parameters["world"].SetValue(mMatWorld);
                engine.Parameters["wRot"].SetValue(Matrix.Identity);
                engine.Parameters["diffuseTexture"].SetValue(drumsetT[DS_TOMTOMS]);
                engine.Parameters["bumpTexture"].SetValue(Game1.texDefaultBM);
                engine.CommitChanges();

                foreach (ModelMesh mesh in drumset[DS_TOMTOMS].Meshes)
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
                matTransl = Matrix.CreateTranslation(locDrummer.X, locDrummer.Y, locDrummer.Z);
                matOrbit = Matrix.CreateTranslation(0, 0, -50 * SCALE) * Matrix.CreateRotationY(drummer.Rot + (float)(Math.PI / 5.5));
                matScale = Matrix.CreateScale(SCALE * 128);

                // identity, scale, rotate, orbit(translate & rotate), translate
                mMatWorld = matIdentity * matScale * matRot * matOrbit * matTransl;

                engine.Parameters["world"].SetValue(mMatWorld);
                engine.Parameters["wRot"].SetValue(Matrix.Identity);
                engine.Parameters["diffuseTexture"].SetValue(drumsetT[DS_SNARE]);
                engine.Parameters["bumpTexture"].SetValue(Game1.texDefaultBM);
                engine.CommitChanges();

                foreach (ModelMesh mesh in drumset[DS_SNARE].Meshes)
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
                matTransl = Matrix.CreateTranslation(locDrummer.X, locDrummer.Y, locDrummer.Z);
                matOrbit = Matrix.CreateTranslation(0, 0, -65 * SCALE) * Matrix.CreateRotationY(drummer.Rot + (float)(Math.PI / 6.2));
                matScale = Matrix.CreateScale(SCALE * 128);

                // identity, scale, rotate, orbit(translate & rotate), translate
                mMatWorld = matIdentity * matScale * matRot * matOrbit * matTransl;

                engine.Parameters["world"].SetValue(mMatWorld);
                engine.Parameters["wRot"].SetValue(Matrix.Identity);
                engine.Parameters["diffuseTexture"].SetValue(drumsetT[DS_RIDECYMBAL]);
                engine.Parameters["bumpTexture"].SetValue(Game1.texDefaultBM);
                engine.CommitChanges();

                foreach (ModelMesh mesh in drumset[DS_RIDECYMBAL].Meshes)
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
                matTransl = Matrix.CreateTranslation(locDrummer.X, locDrummer.Y, locDrummer.Z);
                matOrbit = Matrix.CreateTranslation(0, 0, -40 * SCALE) * Matrix.CreateRotationY(drummer.Rot + (float)(Math.PI / 3.5));
                matScale = Matrix.CreateScale(SCALE * 128);

                // identity, scale, rotate, orbit(translate & rotate), translate
                mMatWorld = matIdentity * matScale * matRot * matOrbit * matTransl;

                engine.Parameters["world"].SetValue(mMatWorld);
                engine.Parameters["wRot"].SetValue(Matrix.Identity);
                engine.Parameters["diffuseTexture"].SetValue(drumsetT[DS_HIHATCYMBAL]);
                engine.Parameters["bumpTexture"].SetValue(Game1.texDefaultBM);
                engine.CommitChanges();

                foreach (ModelMesh mesh in drumset[DS_HIHATCYMBAL].Meshes)
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
                matIdentity = Matrix.Identity;
                matTransl = Matrix.CreateTranslation(locSinger.X, locSinger.Y, locSinger.Z);
                matScale = Matrix.CreateScale(SCALE * 16);

                // identity, scale, rotate, orbit(translate & rotate), translate
                mMatWorld = matIdentity * matScale * matTransl;

                engine.Parameters["world"].SetValue(mMatWorld);
                engine.Parameters["wRot"].SetValue(Matrix.Identity);
                engine.Parameters["diffuseTexture"].SetValue(texSinger);
                engine.CommitChanges();

                vocalist.Draw(gameTime, mMatWorld, graphics);
            }//singer
            engine.Parameters["vertexAlpha"].SetValue(true);
            engine.Parameters["BumpMappingEnabled"].SetValue(true);
            engine.Parameters["SpecularEnabled"].SetValue(true);
            graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;
        }

        /*
         * this should go the fuck away
         */
        public void LoadDynamicModels(ContentManager content, GraphicsDeviceManager graphics)
        {
            float[] xs = { -1f, -1f,  1f, -1f,  1f,  1f,     -1f, -1f,  1f, -1f,  1f,  1f,      1f,  1f,  1f,  1f,  1f,  1f,     -1f, -1f, -1f, -1f, -1f, -1f,     -1f, -1f,  1f, -1f,  1f,  1f,};
            float[] ys = {  0f,  1f,  0f,  1f,  0f,  1f,      0f,  1f,  0f,  1f,  0f,  1f,      0f,  1f,  0f,  1f,  0f,  1f,      0f,  1f,  0f,  1f,  0f,  1f,      1f,  1f,  1f,  1f,  1f,  1f,};
            float[] zs = {  1f,  1f,  1f,  1f,  1f,  1f,     -1f, -1f, -1f, -1f, -1f, -1f,     -1f, -1f,  1f, -1f,  1f,  1f,     -1f, -1f,  1f, -1f,  1f,  1f,     -1f,  1f, -1f,  1f, -1f,  1f,};
            float[] us = {  0f,  0f,  1f,  0f,  1f,  1f,      0f,  0f,  1f,  0f,  1f,  1f,      0f,  0f,  1f,  0f,  1f,  1f,      0f,  0f,  1f,  0f,  1f,  1f,      0f,  0f,  1f,  0f,  1f,  1f,};
            float[] vs = { .5f,  0f, .5f,  0f, .5f,  0f,     .5f,  0f, .5f,  0f, .5f,  0f,     .5f,  0f, .5f,  0f, .5f,  0f,     .5f,  0f, .5f,  0f, .5f,  0f,      1f, .5f,  1f, .5f,  1f, .5f,};
            float[] ms = {  0f,  0f,  0f,  0f,  0f,  0f,      0f,  0f,  0f,  0f,  0f,  0f,      1f,  1f,  1f,  1f,  1f,  1f,     -1f, -1f, -1f, -1f, -1f, -1f,      0f,  0f,  0f,  0f,  0f,  0f,};
            float[] ns = {  0f,  0f,  0f,  0f,  0f,  0f,      0f,  0f,  0f,  0f,  0f,  0f,      0f,  0f,  0f,  0f,  0f,  0f,      0f,  0f,  0f,  0f,  0f,  0f,      1f,  1f,  1f,  1f,  1f,  1f,};
            float[] os = {  1f,  1f,  1f,  1f,  1f,  1f,     -1f, -1f, -1f, -1f, -1f, -1f,      0f,  0f,  0f,  0f,  0f,  0f,      0f,  0f,  0f,  0f,  0f,  0f,      0f,  0f,  0f,  0f,  0f,  0f,};
            float[] ps = {  0f,  0f,  0f,  0f,  0f,  0f,      0f,  0f,  0f,  0f,  0f,  0f,      0f,  0f,  0f,  0f,  0f,  0f,      0f,  0f,  0f,  0f,  0f,  0f,      0f,  0f,  0f,  0f,  0f,  0f,};
            float[] qs = {  1f,  1f,  1f,  1f,  1f,  1f,      1f,  1f,  1f,  1f,  1f,  1f,      1f,  1f,  1f,  1f,  1f,  1f,      1f,  1f,  1f,  1f,  1f,  1f,      0f,  0f,  0f,  0f,  0f,  0f,};
            float[] rs = {  0f,  0f,  0f,  0f,  0f,  0f,      0f,  0f,  0f,  0f,  0f,  0f,      0f,  0f,  0f,  0f,  0f,  0f,      0f,  0f,  0f,  0f,  0f,  0f,      1f,  1f,  1f,  1f,  1f,  1f,};

            GBVertexFormat[] vmdlGuitarist    = new GBVertexFormat[30];
            GBVertexFormat[] vmdlBassist = new GBVertexFormat[30];
            GBVertexFormat[] vmdlDrummer = new GBVertexFormat[30];
            GBVertexFormat[] vmdlSinger = new GBVertexFormat[30];
            for (int i = 0; i < 30; i++)
            {
                vmdlGuitarist[i] = new GBVertexFormat(new Vector3(xs[i], ys[i] * 4, zs[i]),new Vector3(ms[i],ns[i],os[i]), new Vector2(us[i], vs[i]),new Vector3(ps[i],qs[i],rs[i]));
                vmdlBassist[i]   = new GBVertexFormat(new Vector3(xs[i], ys[i] * 4, zs[i]),new Vector3(ms[i],ns[i],os[i]), new Vector2(us[i], vs[i]),new Vector3(ps[i],qs[i],rs[i]));
                vmdlDrummer[i]   = new GBVertexFormat(new Vector3(xs[i], ys[i] * 4, zs[i]),new Vector3(ms[i],ns[i],os[i]), new Vector2(us[i], vs[i]),new Vector3(ps[i],qs[i],rs[i]));
                vmdlSinger[i]    = new GBVertexFormat(new Vector3(xs[i], ys[i] * 4, zs[i]),new Vector3(ms[i],ns[i],os[i]), new Vector2(us[i], vs[i]),new Vector3(ps[i],qs[i],rs[i]));
            }

            mdlGuitarist = new VertexBuffer(graphics.GraphicsDevice, GBVertexFormat.SizeInBytes * xs.Length, BufferUsage.WriteOnly);
            mdlBassist   = new VertexBuffer(graphics.GraphicsDevice, GBVertexFormat.SizeInBytes * xs.Length, BufferUsage.WriteOnly);
            mdlDrummer   = new VertexBuffer(graphics.GraphicsDevice, GBVertexFormat.SizeInBytes * xs.Length, BufferUsage.WriteOnly);
            mdlSinger    = new VertexBuffer(graphics.GraphicsDevice, GBVertexFormat.SizeInBytes * xs.Length, BufferUsage.WriteOnly);
            mdlGuitarist.SetData<GBVertexFormat>(vmdlGuitarist);
            mdlBassist.SetData<GBVertexFormat>(vmdlBassist);
            mdlDrummer.SetData<GBVertexFormat>(vmdlDrummer);
            mdlSinger.SetData<GBVertexFormat>(vmdlSinger); 

            texGuitarist = content.Load<Texture2D>("graphics\\guitarist_tex"); 
            texBassist = content.Load<Texture2D>("graphics\\bassist_tex");
            texDrummer = content.Load<Texture2D>("graphics\\drummer_tex");
            texSinger = content.Load<Texture2D>("graphics\\singer_tex");
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


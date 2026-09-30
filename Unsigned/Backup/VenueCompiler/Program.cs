using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using System.IO;

namespace HFPS_LEVEL_COMPILER
{

    static class Program
    {
        private struct TempTempFiller
        {
            public Plane plane;
            public Vector4 UAxis, VAxis;
            public float UScale, VScale;
            public String tex;
            public Vector3[] threepoints;
        }

        private struct LightTarget
        {
            public Vector3 dir;
            public byte type; 
            public char ct;
        }

        private struct ModelTemplate
        {
            public String name;
            public String[] modelNames;
            public String[] textureNames;
            public String[] modelFlags;
            public Vector3 scale;
        }

        private struct Model
        {
            public String name;
            public Vector3 pos, rot, scale;
        }

        private struct DLight
        {
            public Vector3 pos;
            public float innerAngle, outerAngle;
            public LIGHT_TYPE type;
            public uint index;
            public List<LightTarget> targs;
            public enum LIGHT_TYPE 
            {
                SWEEP=0,
                NORMAL=1, 
                STROBE=2,
                SPOT=3,
            };
            public static string[] TYPES = { "sweep", "normal", "strobe", "spot"};
        }

        private struct TempDLight
        {
            public int index;
            public Vector3 pos;
            public DLight.LIGHT_TYPE type;
            public float innerAngle, outerAngle;
        }

        private struct TempTarget
        {
            public String dat;
            public int val;
            public char which;
            public Vector3 spot;
        }

        private struct TempFiller
        {
            public Vector3[] D3Points, UVPoints;
            public Vector3 Normal, Tangent;
            public String Texture;
        }

        private struct TriggerOnce
        {
            public Vector3 bbt, bbb;
            public TriggerOutput[] outputs;
            public String name;
        }

        private struct TriggerOutput
        {
            public String target, action, trigger, parameters;
            public float delay;
            public bool onlyonce;
        }

        private struct MovingGeometry
        {
            public String name;
            public TempFiller[] VisData, BSPData;
            public Vector3 MoveDirection;
            public float Dist, Speed;
        }

        private struct TempCamera
        {
            public Vector3 pos;
            public Vector3 dir;
            public int index, part;
            
            public TYPE_LEN TYPE;
            public enum TYPE_LEN
            {
                FLASH=0,    //F
                SHORT=1,    //S
                NORMAL=2,   //N
                LENGTHY=3,  //L
                EXTENDED=4, //E
            }
            public static char[] TYPE_char = { 'F', 'S', 'N', 'L', 'E', };
        }

        private struct TempFan
        {
            public Vector3 pos;

            public TempFan(Vector3 p)
            {
                pos = p;
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(String[] args)
        {
            if (args.Length <= 0)
            {
                return;
            }

            Vector3 playerpos = Vector3.Zero;
            Vector2 playerrot = Vector2.Zero;
            Vector3[] lightpos = new Vector3[0];
            Vector4[] lightcol = new Vector4[0];
            TriggerOnce[] trigonces = new TriggerOnce[0];
            MovingGeometry[] movegeometrys = new MovingGeometry[0];
            List<TempCamera> cameras = new List<TempCamera>();
            List<TempTarget> targetnodes = new List<TempTarget>();
            List<TempDLight> dlights = new List<TempDLight>();
            List<TempFan> fans = new List<TempFan>();
            List<Model> models = new List<Model>();
            ModelTemplate[] mdlTemplates = new ModelTemplate[0];
            Vector3 guitarist = Vector3.Zero, 
                    bassist = Vector3.Zero, 
                    drummer = Vector3.Zero, 
                    vocalist = Vector3.Zero;

            uint cNear=0, cFar=0;

            StreamReader fss = new StreamReader(args[0].Substring(0, args[0].LastIndexOf('.')) + ".txt");
            String[] texOld = new String[0], texNew = new String[0];
            float[] texScale = new float[0];
            float scale = 1f;
            while (!fss.EndOfStream)
            {
                String now;
                do { now = fss.ReadLine().Trim(); }
                while (now.Length <= 0 || (now.Length >= 2 && now.Substring(0, 2).Equals("//")));
                if (now.Equals("tex"))
                {
                    fss.ReadLine();// "{"
                    String a, b;
                    float c;
                    while (true)
                    {
                        a = fss.ReadLine().Trim();
                        if (a.Equals("}"))
                            break;
                        b = fss.ReadLine().Trim();
                        c = (float)ParseDouble(fss.ReadLine().Trim());
                        String[] texOldt = new String[texOld.Length + 1];
                        String[] texNewt = new String[texNew.Length + 1];
                        float[] texScalet = new float[texScale.Length + 1];
                        for (int i = 0; i < texOld.Length; i++)
                            texOldt[i] = texOld[i];
                        for (int i = 0; i < texNew.Length; i++)
                            texNewt[i] = texNew[i];
                        for (int i = 0; i < texScale.Length; i++)
                            texScalet[i] = texScale[i];
                        texOldt[texOld.Length] = a;
                        texNewt[texNew.Length] = b;
                        texScalet[texScale.Length] = c;
                        texOld = texOldt;
                        texNew = texNewt;
                        texScale = texScalet;
                    }
                }
                else if (now.Equals("scale"))
                {
                    fss.ReadLine();// "{"
                    scale = (float)ParseDouble(fss.ReadLine().Trim());
                    fss.ReadLine();// "}"
                }
                else if (now.Equals("clip"))
                {
                    fss.ReadLine();// "{"
                    int Near = Int32.Parse(fss.ReadLine().Trim());
                    int Far = Int32.Parse(fss.ReadLine().Trim());
                    if (Near < 0 || Far < 0)
                    {
                        Console.Error.WriteLine("ERROR! Clipping planes must not be negative in " + args[0].Substring(0, args[0].LastIndexOf('.')) + ".txt");
                        throw new InvalidDataException("Clipping planes must not be negative in " + args[0].Substring(0, args[0].LastIndexOf('.')) + ".txt");
                    }
                    cNear = (uint)Near;
                    cFar = (uint)Far;
                    
                    fss.ReadLine();// "}"
                }
                else if (now.Equals("models"))
                {
                    fss.ReadLine();// "{"

                    mdlTemplates = new ModelTemplate[Int32.Parse(fss.ReadLine().Trim())];

                    for (int i = 0; i < mdlTemplates.Length; i++)
                    {
                        String name = fss.ReadLine().Trim();
                        Vector3 sc = new Vector3();
                        {
                            String str = fss.ReadLine().Trim();
                            sc.X = Single.Parse(str.Substring(0, str.IndexOf(',')));
                            str = str.Substring(str.IndexOf(',') + 1);
                            sc.Y = Single.Parse(str.Substring(0, str.IndexOf(',')));
                            str = str.Substring(str.IndexOf(',') + 1);
                            sc.Z = Single.Parse(str);
                        }
                        String[] mdlNames = new String[Int32.Parse(fss.ReadLine().Trim())];
                        String[] mdlFlags = new String[mdlNames.Length];
                        String[] mdlTextures = new String[mdlNames.Length];
                        for (int k = 0; k < mdlNames.Length; k++)
                        {
                            mdlNames[k] = fss.ReadLine().Trim();
                            if (mdlNames[k].IndexOf(':') >= 0)
                            {
                                mdlFlags[k] = mdlNames[k].Substring(mdlNames[k].IndexOf(':') + 1);
                                mdlNames[k] = mdlNames[k].Substring(0, mdlNames[k].IndexOf(':'));
                            }
                            else
                                mdlFlags[k] = "";
                            mdlTextures[k] = fss.ReadLine().Trim();
                        }
                        ModelTemplate t = new ModelTemplate();
                        t.name = name;
                        t.modelNames = mdlNames;
                        t.modelFlags = mdlFlags;
                        t.textureNames = mdlTextures;
                        t.scale = sc;
                        mdlTemplates[i] = t;
                    }

                    fss.ReadLine();// "}"
                }
            }
            fss.Close();


            System.IO.StreamReader fs = new System.IO.StreamReader(args[0]);
            String mainStr = fs.ReadLine().Trim();

            TempFiller[] polygons = new TempFiller[0];
            TempFiller[] polygonsW = new TempFiller[0];

            while (!fs.EndOfStream)
            {
                if (mainStr.Equals("versioninfo"))
                {

                    String str = fs.ReadLine().Trim();// this should be "{"
                    while (!str.Equals("}"))
                    {

                        str = fs.ReadLine().Trim();
                    }
                }
                else if (mainStr.Equals("visgroups"))
                {
                    String str = fs.ReadLine().Trim();// this should be "{"
                    while (!str.Equals("}"))
                    {

                        str = fs.ReadLine().Trim();
                    }
                }
                else if (mainStr.Equals("viewsettings"))
                {
                    String str = fs.ReadLine().Trim();// this should be "{"
                    while (!str.Equals("}"))
                    {

                        str = fs.ReadLine().Trim();
                    }
                }
                else if (mainStr.Equals("world"))
                {

                    String str = fs.ReadLine().Trim();// this should be "{"
                    while (!str.Equals("}"))
                    {
                        if (str.Equals("solid"))
                        {
                            TempTempFiller[] actives = new TempTempFiller[0];
                            TempTempFiller[] activesW = new TempTempFiller[0];
                            Vector3[] points = new Vector3[0];

                            String substr = fs.ReadLine().Trim();// this should be "{"
                            while (!substr.Equals("}"))
                            {
                                if (substr.Equals("side"))
                                {
                                    bool yesdraw = true;
                                    TempTempFiller add = new TempTempFiller();

                                    Vector3[] pts = new Vector3[3];
                                    String subsubstr = fs.ReadLine().Trim();// this should be "{"
                                    subsubstr = fs.ReadLine().Trim();// this should be the first var

                                    while (!subsubstr.Equals("}"))
                                    {
                                        if (yesdraw)
                                        {
                                            subsubstr = subsubstr.Substring(1);
                                            String var = subsubstr.Substring(0, subsubstr.IndexOf('\"'));
                                            subsubstr = subsubstr.Substring(subsubstr.IndexOf('\"') + 1);
                                            subsubstr = subsubstr.Substring(subsubstr.IndexOf('\"') + 1);
                                            String val = subsubstr.Substring(0, subsubstr.IndexOf('\"'));
                                            if (var.Equals("plane"))
                                            {
                                                pts[0] = new Vector3(0, 0, 0);
                                                pts[1] = new Vector3(0, 0, 0);
                                                pts[2] = new Vector3(0, 0, 0);
                                                pts[0].X = (float)ParseDouble(val.Substring(1, val.IndexOf(' ')));
                                                val = val.Substring(val.IndexOf(' ') + 1);

                                                pts[0].Z = (float)ParseDouble(val.Substring(0, val.IndexOf(' ')));
                                                val = val.Substring(val.IndexOf(' ') + 1);

                                                pts[0].Y = (float)ParseDouble(val.Substring(0, val.IndexOf(')')));
                                                val = val.Substring(val.IndexOf('(') + 1);

                                                pts[1].X = (float)ParseDouble(val.Substring(0, val.IndexOf(' ')));
                                                val = val.Substring(val.IndexOf(' ') + 1);

                                                pts[1].Z = (float)ParseDouble(val.Substring(0, val.IndexOf(' ')));
                                                val = val.Substring(val.IndexOf(' ') + 1);

                                                pts[1].Y = (float)ParseDouble(val.Substring(0, val.IndexOf(')')));
                                                val = val.Substring(val.IndexOf('(') + 1);

                                                pts[2].X = (float)ParseDouble(val.Substring(0, val.IndexOf(' ')));
                                                val = val.Substring(val.IndexOf(' ') + 1);

                                                pts[2].Z = (float)ParseDouble(val.Substring(0, val.IndexOf(' ')));
                                                val = val.Substring(val.IndexOf(' ') + 1);

                                                pts[2].Y = (float)ParseDouble(val.Substring(0, val.IndexOf(')')));

                                                add.threepoints = pts;
                                            }
                                            else if (var.Equals("material"))
                                            {
                                                add.tex = val;
                                            }
                                            else if (var.Equals("uaxis"))
                                            {
                                                add.UAxis.X = (float)ParseDouble(val.Substring(1, val.IndexOf(' ')));
                                                val = val.Substring(val.IndexOf(' ') + 1);
                                                add.UAxis.Z = (float)ParseDouble(val.Substring(0, val.IndexOf(' ')));
                                                val = val.Substring(val.IndexOf(' ') + 1);
                                                add.UAxis.Y = (float)ParseDouble(val.Substring(0, val.IndexOf(' ')));
                                                val = val.Substring(val.IndexOf(' ') + 1);
                                                add.UAxis.W = (float)ParseDouble(val.Substring(0, val.IndexOf(']')));
                                                val = val.Substring(val.IndexOf(']') + 1).Trim();
                                                add.UScale = (float)ParseDouble(val);
                                            }
                                            else if (var.Equals("vaxis"))
                                            {
                                                add.VAxis.X = (float)ParseDouble(val.Substring(1, val.IndexOf(' ')));
                                                val = val.Substring(val.IndexOf(' ') + 1);
                                                add.VAxis.Z = (float)ParseDouble(val.Substring(0, val.IndexOf(' ')));
                                                val = val.Substring(val.IndexOf(' ') + 1);
                                                add.VAxis.Y = (float)ParseDouble(val.Substring(0, val.IndexOf(' ')));
                                                val = val.Substring(val.IndexOf(' ') + 1);
                                                add.VAxis.W = (float)ParseDouble(val.Substring(0, val.IndexOf(']')));
                                                val = val.Substring(val.IndexOf(']') + 1).Trim();
                                                add.VScale = (float)ParseDouble(val);
                                            }
                                        }
                                        subsubstr = fs.ReadLine().Trim();
                                    }
                                    {
                                        Vector3 normal = Vector3.Cross(new Vector3(add.VAxis.X, add.VAxis.Y, add.VAxis.Z), new Vector3(add.UAxis.X, add.UAxis.Y, add.UAxis.Z));
                                        normal = Vector3.Normalize(normal);
                                        Matrix tangentmatrix = ComputeTangentMatrix(Vector3.Normalize(new Vector3(add.VAxis.X, add.VAxis.Y, add.VAxis.Z)), normal);
                                        Vector3[] ptsT = new Vector3[3];
                                        for (int i = 0; i < 3; i++)
                                            ptsT[i] = Vector3.Transform(pts[i], tangentmatrix);
                                        if (IsCCW(ptsT))
                                            normal = -normal;

                                        Vector3 negNormal = new Vector3(-normal.X, -normal.Y, -normal.Z);
                                        float D = -Vector3.Dot((negNormal), pts[0]);

                                        add.plane = new Plane(normal, D);
                                    }
                                    for (int k = 0; k < 3; k++)
                                    {
                                        bool toAdd = true;
                                        for (int i = 0; i < points.Length; i++)
                                            if (points[i].X == pts[k].X && points[i].Y == pts[k].Y && points[i].Z == pts[k].Z)
                                                toAdd = false;
                                        if (toAdd)
                                        {
                                            Vector3[] more = new Vector3[points.Length + 1];
                                            for (int i = 0; i < points.Length; i++)
                                                more[i] = points[i];
                                            more[points.Length] = pts[k];
                                            points = more;
                                        }
                                    }
                                    if (!add.tex.Equals("TOOLS/TOOLSNODRAW"))
                                    {
                                        TempTempFiller[] addarr = new TempTempFiller[actives.Length + 1];
                                        for (int i = 0; i < actives.Length; i++)
                                            addarr[i] = actives[i];
                                        addarr[actives.Length] = add;
                                        actives = addarr;

                                        TempTempFiller[] addWarr = new TempTempFiller[activesW.Length + 1];
                                        for (int i = 0; i < activesW.Length; i++)
                                            addWarr[i] = activesW[i];
                                        addWarr[activesW.Length] = add;
                                        activesW = addWarr;
                                    }
                                }
                                else if (substr.Equals("editor"))
                                {
                                    String subsubstr = fs.ReadLine().Trim();// this should be "{"
                                    while (!subsubstr.Equals("}"))
                                    {
                                        subsubstr = fs.ReadLine().Trim();
                                    }
                                }
                                substr = fs.ReadLine().Trim();
                            }
                            {//VISpolys
                                TempFiller[] tempPoly = new TempFiller[polygons.Length + actives.Length];
                                for (int i = 0; i < polygons.Length; i++)
                                    tempPoly[i] = polygons[i];
                                for (int i = 0; i < actives.Length; i++)
                                {
                                        Vector3 negNormal = -actives[i].plane.Normal;
                                        Vector3[] pOnLine = new Vector3[0];

                                        //find all points on selected plane
                                        for (int k = 0; k < points.Length; k++)
                                        {
                                            float D = -Vector3.Dot((negNormal), points[k]);
                                            if (Math.Abs(D - actives[i].plane.D) < 0.1f)
                                            {
                                                Vector3[] pOnLineMore = new Vector3[pOnLine.Length + 1];
                                                for (int p = 0; p < pOnLine.Length; p++)
                                                    pOnLineMore[p] = pOnLine[p];
                                                pOnLine = pOnLineMore;

                                                pOnLine[pOnLine.Length - 1] = points[k];
                                            }
                                        }

                                        //find good point config
                                        Vector3[][] combos = GetCombos(pOnLine);
                                        Vector3[][] combosTransformed = new Vector3[combos.Length][];
                                        Matrix tng = ComputeTangentMatrix(Vector3.Normalize(new Vector3(actives[i].VAxis.X, actives[i].VAxis.Y, actives[i].VAxis.Z)), Vector3.Normalize(actives[i].plane.Normal));
                                        for (int o = 0; o < combos.Length; o++)
                                        {
                                            combosTransformed[o] = new Vector3[combos[o].Length];
                                            for (int n = 0; n < combos[o].Length; n++)
                                            {
                                                combosTransformed[o][n] = Vector3.Transform(combos[o][n], tng);
                                            }
                                        }
                                        int r;
                                        for (r = 0; r < combos.Length; r++)
                                        {
                                            if (!LineCollisions(combosTransformed[r]))
                                                if (!IsCCW(combosTransformed[r]))
                                                    break;
                                        }


                                        if (r >= combos.Length)
                                            r = 0;

                                        Vector3[] pointlist = new Vector3[combos[r].Length];
                                        Vector3[] pointlistT = new Vector3[combosTransformed[r].Length];
                                        for (int k = 0; k < pointlist.Length; k++)
                                        {
                                            pointlist[k] = combos[r][combos[r].Length - 1 - k];
                                            pointlistT[k] = combosTransformed[r][combosTransformed[r].Length - 1 - k];
                                        }

                                        tempPoly[i + polygons.Length] = new TempFiller();
                                        tempPoly[i + polygons.Length].Texture = actives[i].tex;
                                        tempPoly[i + polygons.Length].D3Points = pointlist;
                                        tempPoly[i + polygons.Length].Normal = actives[i].plane.Normal;
                                        Vector3 tempaxis = new Vector3(actives[i].UAxis.X, actives[i].UAxis.Y, actives[i].UAxis.Z);
                                        Vector4 tempuaxis = new Vector4(Vector3.Transform(tempaxis, tng), actives[i].UAxis.W);
                                        tempaxis = new Vector3(actives[i].VAxis.X, actives[i].VAxis.Y, actives[i].VAxis.Z);
                                        Vector4 tempvaxis = new Vector4(Vector3.Transform(tempaxis, tng), actives[i].VAxis.W);
                                        float xtras = 1;
                                        for (int k = 0; k < texOld.Length; k++)
                                            if (texOld[k].Equals(actives[i].tex))
                                            {
                                                xtras = texScale[k];
                                                break;
                                            }
                                        tempPoly[i + polygons.Length].UVPoints = GetUV(pointlistT, tempuaxis, actives[i].UScale * xtras, tempvaxis, actives[i].VScale * xtras);
                                        tempPoly[i + polygons.Length].Tangent = new Vector3(actives[i].VAxis.X, actives[i].VAxis.Y, actives[i].VAxis.Z);
                                    
                                }
                                polygons = tempPoly;
                            }
                        }
                        str = fs.ReadLine().Trim();
                    }
                }
                else if (mainStr.Equals("entity"))
                {
                    String classname="";
                    Vector3 pos=Vector3.Zero;
                    Vector3 angles=Vector3.Zero;
                    Vector4 lightdata = Vector4.Zero;
                    Vector3[] boxpoints = new Vector3[0];
                    float innercone=0, outercone=0;
                    float distance = 0;
                    float speed = 0;
                    float position = 0;//0-1 for doors etc
                    TriggerOutput[] targets = new TriggerOutput[0];
                    String name = "", modelName = "";
                    String str = fs.ReadLine().Trim();// this should be "{"
                    str = fs.ReadLine().Trim();
                    TempFiller[] VisBuffer = new TempFiller[0], 
                                 BSPBuffer = new TempFiller[0];
                    while (!str.Equals("}"))
                    {
                        if (str.Equals("editor"))
                        {
                            String substr = fs.ReadLine().Trim();// this should be "{"
                            while (!substr.Equals("}"))
                            {

                                substr = fs.ReadLine().Trim();
                            }
                        }
                        else if (str.Equals("connections"))
                        {
                            String substr = fs.ReadLine().Trim();// this should be "{"
                            substr = fs.ReadLine().Trim();
                            while (!substr.Equals("}"))
                            {
                                TriggerOutput add = new TriggerOutput();
                                substr = substr.Substring(1);
                                add.trigger = substr.Substring(0, substr.IndexOf('\"'));
                                substr = substr.Substring(substr.IndexOf('\"') + 1).Trim();
                                substr = substr.Substring(substr.IndexOf('\"') + 1).Trim();
                                add.target = substr.Substring(0, substr.IndexOf(','));
                                substr = substr.Substring(substr.IndexOf(',') + 1).Trim();
                                add.action = substr.Substring(0, substr.IndexOf(','));
                                substr = substr.Substring(substr.IndexOf(',') + 1).Trim();
                                add.parameters = substr.Substring(0, substr.IndexOf(','));
                                substr = substr.Substring(substr.IndexOf(',') + 1).Trim();
                                add.delay = (float)Double.Parse(substr.Substring(0, substr.IndexOf(',')));
                                substr = substr.Substring(substr.IndexOf(',') + 1).Trim();
                                String onon = substr.Substring(0, substr.IndexOf('\"'));
                                if (onon.Equals("1"))
                                    add.onlyonce = true;
                                else
                                    add.onlyonce = false;
                                TriggerOutput[] nt = new TriggerOutput[targets.Length + 1];
                                for (int i = 0; i < targets.Length; i++)
                                    nt[i] = targets[i];
                                nt[targets.Length] = add;
                                targets = nt;
                                substr = fs.ReadLine().Trim();
                            }
                        }
                        else if (str.Equals("solid"))
                        {
                            TempTempFiller[] actives = new TempTempFiller[0];
                            TempTempFiller[] activesW = new TempTempFiller[0];
                            Vector3[] points = new Vector3[0];

                            String substr = fs.ReadLine().Trim();// this should be "{"
                            while (!substr.Equals("}"))
                            {
                                if (substr.Equals("side"))
                                {
                                    bool yesdraw = true;
                                    TempTempFiller add = new TempTempFiller();

                                    Vector3[] pts = new Vector3[3];
                                    String subsubstr = fs.ReadLine().Trim();// this should be "{"
                                    subsubstr = fs.ReadLine().Trim();// this should be the first var

                                    while (!subsubstr.Equals("}"))
                                    {
                                        if (yesdraw)
                                        {
                                            subsubstr = subsubstr.Substring(1);
                                            String var = subsubstr.Substring(0, subsubstr.IndexOf('\"'));
                                            subsubstr = subsubstr.Substring(subsubstr.IndexOf('\"') + 1);
                                            subsubstr = subsubstr.Substring(subsubstr.IndexOf('\"') + 1);
                                            String val = subsubstr.Substring(0, subsubstr.IndexOf('\"'));
                                            if (var.Equals("plane"))
                                            {
                                                pts[0] = new Vector3(0, 0, 0);
                                                pts[1] = new Vector3(0, 0, 0);
                                                pts[2] = new Vector3(0, 0, 0);
                                                pts[0].X = (float)ParseDouble(val.Substring(1, val.IndexOf(' ')));
                                                val = val.Substring(val.IndexOf(' ') + 1);

                                                pts[0].Z = (float)ParseDouble(val.Substring(0, val.IndexOf(' ')));
                                                val = val.Substring(val.IndexOf(' ') + 1);

                                                pts[0].Y = (float)ParseDouble(val.Substring(0, val.IndexOf(')')));
                                                val = val.Substring(val.IndexOf('(') + 1);

                                                pts[1].X = (float)ParseDouble(val.Substring(0, val.IndexOf(' ')));
                                                val = val.Substring(val.IndexOf(' ') + 1);

                                                pts[1].Z = (float)ParseDouble(val.Substring(0, val.IndexOf(' ')));
                                                val = val.Substring(val.IndexOf(' ') + 1);

                                                pts[1].Y = (float)ParseDouble(val.Substring(0, val.IndexOf(')')));
                                                val = val.Substring(val.IndexOf('(') + 1);

                                                pts[2].X = (float)ParseDouble(val.Substring(0, val.IndexOf(' ')));
                                                val = val.Substring(val.IndexOf(' ') + 1);

                                                pts[2].Z = (float)ParseDouble(val.Substring(0, val.IndexOf(' ')));
                                                val = val.Substring(val.IndexOf(' ') + 1);

                                                pts[2].Y = (float)ParseDouble(val.Substring(0, val.IndexOf(')')));

                                                add.threepoints = pts;
                                            }
                                            else if (var.Equals("material"))
                                            {
                                                add.tex = val;
                                            }
                                            else if (var.Equals("uaxis"))
                                            {
                                                add.UAxis.X = (float)ParseDouble(val.Substring(1, val.IndexOf(' ')));
                                                val = val.Substring(val.IndexOf(' ') + 1);
                                                add.UAxis.Z = (float)ParseDouble(val.Substring(0, val.IndexOf(' ')));
                                                val = val.Substring(val.IndexOf(' ') + 1);
                                                add.UAxis.Y = (float)ParseDouble(val.Substring(0, val.IndexOf(' ')));
                                                val = val.Substring(val.IndexOf(' ') + 1);
                                                add.UAxis.W = (float)ParseDouble(val.Substring(0, val.IndexOf(']')));
                                                val = val.Substring(val.IndexOf(']') + 1).Trim();
                                                add.UScale = (float)ParseDouble(val);
                                            }
                                            else if (var.Equals("vaxis"))
                                            {
                                                add.VAxis.X = (float)ParseDouble(val.Substring(1, val.IndexOf(' ')));
                                                val = val.Substring(val.IndexOf(' ') + 1);
                                                add.VAxis.Z = (float)ParseDouble(val.Substring(0, val.IndexOf(' ')));
                                                val = val.Substring(val.IndexOf(' ') + 1);
                                                add.VAxis.Y = (float)ParseDouble(val.Substring(0, val.IndexOf(' ')));
                                                val = val.Substring(val.IndexOf(' ') + 1);
                                                add.VAxis.W = (float)ParseDouble(val.Substring(0, val.IndexOf(']')));
                                                val = val.Substring(val.IndexOf(']') + 1).Trim();
                                                add.VScale = (float)ParseDouble(val);
                                            }
                                        }
                                        subsubstr = fs.ReadLine().Trim();
                                    }
                                    {
                                        Vector3 normal = Vector3.Cross(new Vector3(add.VAxis.X, add.VAxis.Y, add.VAxis.Z), new Vector3(add.UAxis.X, add.UAxis.Y, add.UAxis.Z));
                                        normal = Vector3.Normalize(normal);
                                        Matrix tangentmatrix = ComputeTangentMatrix(Vector3.Normalize(new Vector3(add.VAxis.X, add.VAxis.Y, add.VAxis.Z)), normal);
                                        Vector3[] ptsT = new Vector3[3];
                                        for (int i = 0; i < 3; i++)
                                            ptsT[i] = Vector3.Transform(pts[i], tangentmatrix);
                                        if (IsCCW(ptsT))
                                            normal = -normal;

                                        Vector3 negNormal = new Vector3(-normal.X, -normal.Y, -normal.Z);
                                        float D = -Vector3.Dot((negNormal), pts[0]);

                                        add.plane = new Plane(normal, D);
                                    }
                                    for (int k = 0; k < 3; k++)
                                    {
                                        bool toAdd = true;
                                        for (int i = 0; i < points.Length; i++)
                                            if (points[i].X == pts[k].X && points[i].Y == pts[k].Y && points[i].Z == pts[k].Z)
                                                toAdd = false;
                                        if (toAdd)
                                        {
                                            Vector3[] more = new Vector3[points.Length + 1];
                                            for (int i = 0; i < points.Length; i++)
                                                more[i] = points[i];
                                            more[points.Length] = pts[k];
                                            points = more;
                                        }

                                        bool toAdd2 = true;
                                        for (int i = 0; i < boxpoints.Length; i++)
                                            if (boxpoints[i].X == pts[k].X && boxpoints[i].Y == pts[k].Y && boxpoints[i].Z == pts[k].Z)
                                                toAdd2 = false;
                                        if (toAdd2)
                                        {
                                            Vector3[] more = new Vector3[boxpoints.Length + 1];
                                            for (int i = 0; i < boxpoints.Length; i++)
                                                more[i] = boxpoints[i];
                                            more[boxpoints.Length] = pts[k];
                                            boxpoints = more;
                                        }
                                    }

                                    if (!add.tex.Equals("TOOLS/TOOLSNODRAW"))
                                    {
                                        TempTempFiller[] addarr = new TempTempFiller[actives.Length + 1];
                                        for (int i = 0; i < actives.Length; i++)
                                            addarr[i] = actives[i];
                                        addarr[actives.Length] = add;
                                        actives = addarr;
                                    }
                                }
                                else if (substr.Equals("editor"))
                                {
                                    String subsubstr = fs.ReadLine().Trim();// this should be "{"
                                    while (!subsubstr.Equals("}"))
                                    {

                                        subsubstr = fs.ReadLine().Trim();
                                    }
                                }
                                substr = fs.ReadLine().Trim();
                            }
                            {//VISpolys
                                TempFiller[] tempPoly = new TempFiller[VisBuffer.Length + actives.Length];
                                for (int i = 0; i < VisBuffer.Length; i++)
                                    tempPoly[i] = VisBuffer[i];
                                for (int i = 0; i < actives.Length; i++)
                                {
                                    Vector3 negNormal = -actives[i].plane.Normal;
                                    Vector3[] pOnLine = new Vector3[0];

                                    //find all points on selected plane
                                    for (int k = 0; k < points.Length; k++)
                                    {
                                        float D = -Vector3.Dot((negNormal), points[k]);
                                        if (Math.Abs(D - actives[i].plane.D) < 0.1f)
                                        {
                                            Vector3[] pOnLineMore = new Vector3[pOnLine.Length + 1];
                                            for (int p = 0; p < pOnLine.Length; p++)
                                                pOnLineMore[p] = pOnLine[p];
                                            pOnLine = pOnLineMore;

                                            pOnLine[pOnLine.Length - 1] = points[k];
                                        }
                                    }

                                    //find good point config
                                    Vector3[][] combos = GetCombos(pOnLine);
                                    Vector3[][] combosTransformed = new Vector3[combos.Length][];
                                    Matrix tng = ComputeTangentMatrix(Vector3.Normalize(new Vector3(actives[i].VAxis.X, actives[i].VAxis.Y, actives[i].VAxis.Z)), Vector3.Normalize(actives[i].plane.Normal));
                                    for (int o = 0; o < combos.Length; o++)
                                    {
                                        combosTransformed[o] = new Vector3[combos[o].Length];
                                        for (int n = 0; n < combos[o].Length; n++)
                                        {
                                            combosTransformed[o][n] = Vector3.Transform(combos[o][n], tng);
                                        }
                                    }
                                    int r;
                                    for (r = 0; r < combos.Length; r++)
                                    {
                                        if (!LineCollisions(combosTransformed[r]))
                                            if (!IsCCW(combosTransformed[r]))
                                                break;
                                    }

                                    if (r >= combos.Length)
                                        r = 0;

                                    Vector3[] pointlist = new Vector3[combos[r].Length];
                                    Vector3[] pointlistT = new Vector3[combosTransformed[r].Length];
                                    for (int k = 0; k < pointlist.Length; k++)
                                    {
                                        pointlist[k] = combos[r][combos[r].Length - 1 - k];
                                        pointlistT[k] = combosTransformed[r][combosTransformed[r].Length - 1 - k];
                                    }

                                    tempPoly[i + VisBuffer.Length] = new TempFiller();
                                    tempPoly[i + VisBuffer.Length].Texture = actives[i].tex;
                                    tempPoly[i + VisBuffer.Length].D3Points = pointlist;
                                    tempPoly[i + VisBuffer.Length].Normal = actives[i].plane.Normal;
                                    Vector3 tempaxis = new Vector3(actives[i].UAxis.X, actives[i].UAxis.Y, actives[i].UAxis.Z);
                                    Vector4 tempuaxis = new Vector4(Vector3.Transform(tempaxis, tng), actives[i].UAxis.W);
                                    tempaxis = new Vector3(actives[i].VAxis.X, actives[i].VAxis.Y, actives[i].VAxis.Z);
                                    Vector4 tempvaxis = new Vector4(Vector3.Transform(tempaxis, tng), actives[i].VAxis.W);
                                    float xtras = 1;
                                    for (int k = 0; k < texOld.Length; k++)
                                        if (texOld[k].Equals(actives[i].tex))
                                        {
                                            xtras = texScale[k];
                                            break;
                                        }
                                    tempPoly[i + VisBuffer.Length].UVPoints = GetUV(pointlistT, tempuaxis, actives[i].UScale * xtras, tempvaxis, actives[i].VScale * xtras);
                                    tempPoly[i + VisBuffer.Length].Tangent = new Vector3(actives[i].VAxis.X, actives[i].VAxis.Y, actives[i].VAxis.Z);
                                }
                                VisBuffer = tempPoly;
                            }
                        }
                        else
                        {
                            String var, val;
                            str = str.Substring(1);
                            var = str.Substring(0, str.IndexOf('\"'));
                            str = str.Substring(str.IndexOf('\"') + 1);
                            str = str.Substring(str.IndexOf('\"') + 1);
                            val = str.Substring(0, str.IndexOf('\"'));

                            if (var.Equals("classname"))
                            {
                                classname = val;
                            }
                            else if (var.Equals("angles"))
                            {
                                angles.X = (float)Double.Parse(val.Substring(0, val.IndexOf(' ')));
                                val = val.Substring(val.IndexOf(' ')).Trim();
                                angles.Y = (float)Double.Parse(val.Substring(0, val.IndexOf(' ')));
                                val = val.Substring(val.IndexOf(' ')).Trim();
                                angles.Z = (float)Double.Parse(val);
                            }
                            else if (var.Equals("origin"))
                            {
                                pos.X = (float)Double.Parse(val.Substring(0, val.IndexOf(' ')));
                                val = val.Substring(val.IndexOf(' ')).Trim();
                                pos.Z = (float)Double.Parse(val.Substring(0, val.IndexOf(' ')));
                                val = val.Substring(val.IndexOf(' ')).Trim();
                                pos.Y = (float)Double.Parse(val);
                            }
                            else if (var.Equals("_light"))
                            {
                                lightdata.X = (float)Double.Parse(val.Substring(0, val.IndexOf(' ')));
                                val = val.Substring(val.IndexOf(' ')).Trim();
                                lightdata.Y = (float)Double.Parse(val.Substring(0, val.IndexOf(' ')));
                                val = val.Substring(val.IndexOf(' ')).Trim();
                                lightdata.Z = (float)Double.Parse(val.Substring(0, val.IndexOf(' ')));
                                val = val.Substring(val.IndexOf(' ')).Trim();
                                lightdata.W = (float)Double.Parse(val);
                            }
                            else if (var.Equals("movedir"))
                            {
                                angles.X = (float)Double.Parse(val.Substring(0, val.IndexOf(' ')));
                                val = val.Substring(val.IndexOf(' ')).Trim();
                                angles.Y = (float)Double.Parse(val.Substring(0, val.IndexOf(' ')));
                                val = val.Substring(val.IndexOf(' ')).Trim();
                                angles.Z = (float)Double.Parse(val);
                            }
                            else if (var.Equals("movedistance"))
                            {
                                distance = (float)Double.Parse(val);
                            }
                            else if (var.Equals("speed"))
                            {
                                speed = (float)Double.Parse(val);
                            }
                            else if (var.Equals("startposition"))
                            {
                                position = (float)Double.Parse(val);
                            }
                            else if (var.Equals("targetname"))
                            {
                                name = val;
                            }
                            else if (var.Equals("model"))
                            {
                                modelName = val;
                            }
                            else if (var.Equals("_cone"))
                            {
                                outercone = (float)Double.Parse(val);
                            }
                            else if (var.Equals("_inner_cone"))
                            {
                                innercone = (float)Double.Parse(val);
                            }
                        }

                        str = fs.ReadLine().Trim();
                    }
                    if (classname.Equals("light"))
                    {
                        Vector3[] lp = new Vector3[lightpos.Length+1];
                        for (int i = 0; i < lightpos.Length; i++)
                            lp[i] = lightpos[i];
                        lp[lightpos.Length] = pos*scale;
                        lightpos = lp;
                        
                        Vector4[] lc = new Vector4[lightcol.Length+1];
                        for (int i = 0; i < lightcol.Length; i++)
                            lc[i] = lightcol[i];
                        lc[lightcol.Length] = lightdata/255f;
                        lightcol = lc;
                    }
                    else if (classname.Equals("func_movelinear"))
                    {
                        MovingGeometry nOne = new MovingGeometry();
                        nOne.BSPData = BSPBuffer;
                        nOne.VisData = VisBuffer;
                        nOne.Dist = distance;
                        nOne.name = name;
                        nOne.MoveDirection = angles;
                        nOne.Speed = speed/distance;

                        MovingGeometry[] nArr = new MovingGeometry[movegeometrys.Length+1];
                        for (int i = 0; i < movegeometrys.Length; i++)
                            nArr[i] = movegeometrys[i];
                        nArr[movegeometrys.Length] = nOne;
                        movegeometrys = nArr;
                    }
                    else if (classname.Equals("point_camera"))
                    {
                        TempCamera nO = new TempCamera();
                        nO.dir = angles;
                        nO.pos = pos;
                        char tC = name.ToCharArray()[7];
                        name = name.Substring(8).Trim();
                        nO.index = Int32.Parse(name.Substring(0, name.IndexOf('_')));
                        name = name.Substring(name.IndexOf('_') + 1);
                        nO.part = Int32.Parse(name);
                        for (int i = 0; i < TempCamera.TYPE_char.Length; i++)
                            if (tC == TempCamera.TYPE_char[i])
                                nO.TYPE = (TempCamera.TYPE_LEN)i;
                        
                        cameras.Add(nO);
                    }
                    else if (classname.Equals("light_spot"))
                    {
                        TempDLight nO = new TempDLight();
                        nO.pos = pos;
                        nO.innerAngle = innercone;
                        nO.outerAngle = outercone;
                        for (int i = 0; i < DLight.TYPES.Length; i++)
                            if (name.Substring(0, name.IndexOf('_')).Equals(DLight.TYPES[i]))
                                nO.type = (DLight.LIGHT_TYPE)i;
                        nO.index = Int32.Parse(name.Substring(name.IndexOf('_')+1));

                        dlights.Add(nO);
                    }
                    else if (classname.Equals("info_target"))
                    {
                        TempTarget nO = new TempTarget();
                        nO.spot = pos;
                        int one = name.IndexOf('_') + 1;
                        int two = 1;// name.Length - 1;
                        nO.val = Int32.Parse(name.Substring(one, two));
                        nO.which = name.ToCharArray()[name.Length - 1];
                        nO.dat = name.Substring(0, name.IndexOf('_'));

                        targetnodes.Add(nO);
                    }
                    else if (classname.Equals("npc_combine_s"))
                    {
                        if (name.Equals("guitarist"))
                            guitarist = pos*scale;
                        else if (name.Equals("bassist"))
                            bassist = pos*scale;
                        else if (name.Equals("drummer"))
                            drummer = pos*scale;
                        else if (name.Equals("vocalist"))
                            vocalist = pos*scale;
                    }
                    else if (classname.Equals("npc_citizen"))
                    {
                        fans.Add(new TempFan(pos * scale));
                    }
                    else if(classname == "prop_static")
                    {
                        Model m = new Model();
                        m.name = modelName;
                        m.pos = pos * scale;
                        m.rot = angles;
                        m.scale = new Vector3(1, 1, 1);
                        models.Add(m);
                    }
                }
                else if (mainStr.Equals("cameras"))
                {
                    String str = fs.ReadLine().Trim();// this should be "{"
                    while (!str.Equals("}"))
                    {
                        if (str.Equals("camera"))
                        {
                            String substr = fs.ReadLine().Trim();// this should be "{"
                            while (!substr.Equals("}"))
                            {
                                substr = fs.ReadLine().Trim();
                            }
                        }
                        str = fs.ReadLine().Trim();
                    }
                }
                else if (mainStr.Equals("cordon"))
                {
                    String str = fs.ReadLine().Trim();// this should be "{"
                    while (!str.Equals("}"))
                    {
                        str = fs.ReadLine().Trim();
                    }
                }
                if(!fs.EndOfStream)
                    mainStr = fs.ReadLine().Trim();
            }
            fs.Close();

            DLight[] fdlights = new DLight[dlights.Count];
            for (int i = 0; i < fdlights.Length; i++)
            {
                fdlights[i] = new DLight();
                fdlights[i].innerAngle = dlights[i].innerAngle;
                fdlights[i].outerAngle = dlights[i].outerAngle;
                fdlights[i].pos = dlights[i].pos;
                fdlights[i].targs = new List<LightTarget>();
                fdlights[i].type = dlights[i].type;
                fdlights[i].index = (uint)dlights[i].index;
            }

            for (int i = 0; i < dlights.Count; i++)
            {
                for (int j = 0; j < targetnodes.Count; j++)
                {
                    String val1 = targetnodes[j].dat;
                    String val2 = DLight.TYPES[(int)dlights[i].type];
                    if (val1.Equals(val2))
                    if(targetnodes[j].val==dlights[i].index)
                    {
                        LightTarget l = new LightTarget();
                        l.ct = targetnodes[j].which;
                        l.dir = Vector3.Normalize(targetnodes[j].spot-dlights[i].pos);
                        l.type = (byte)(targetnodes[j].val);
                        fdlights[i].targs.Add(l);
                    }
                }
            }
            
            BinaryWriter fsw = new BinaryWriter(File.Open(args[0].Substring(0, args[0].LastIndexOf('.')) + ".gbw",FileMode.Create));
            char[] header = { 'U', 'n', 's', 'd', 'V', 'n', 'u'};
            fsw.Write(header);
            fsw.Write((byte)3);
            fsw.Write((int)(cNear*scale));
            fsw.Write((int)((cFar*scale)+1));
            fsw.Write(guitarist.X);
            fsw.Write(guitarist.Y);
            fsw.Write(guitarist.Z);
            fsw.Write(vocalist.X);
            fsw.Write(vocalist.Y);
            fsw.Write(vocalist.Z);
            fsw.Write(drummer.X);
            fsw.Write(drummer.Y);
            fsw.Write(drummer.Z);
            fsw.Write(bassist.X);
            fsw.Write(bassist.Y);
            fsw.Write(bassist.Z);
            fsw.Write((uint)texNew.Length);
            for (int i = 0; i < texNew.Length; i++)
                fsw.Write(texNew[i]);
            fsw.Write((uint)polygons.Length);
            for (int i = 0; i < polygons.Length; i++)
            {
                Vector3 tangent = Vector3.Normalize(polygons[i].Tangent);
                String tex = polygons[i].Texture;
                uint t = (uint)(((long)Int32.MaxValue)*2-1);
                for (uint k = 0; k < texOld.Length; k++)
                    if (tex.Equals(texOld[k]))
                        t = k;
                polygons[i].Normal = Vector3.Normalize(polygons[i].Normal);
                fsw.Write(polygons[i].Normal.X);
                fsw.Write(polygons[i].Normal.Y);
                fsw.Write(polygons[i].Normal.Z);
                fsw.Write(tangent.X);
                fsw.Write(tangent.Y);
                fsw.Write(tangent.Z);
                fsw.Write(t);
                fsw.Write((uint)polygons[i].D3Points.Length);
                for (int k = 0; k < polygons[i].D3Points.Length; k++)
                {
                    fsw.Write(scale * polygons[i].D3Points[k].X);
                    fsw.Write(scale * polygons[i].D3Points[k].Y);
                    fsw.Write(scale * polygons[i].D3Points[k].Z);
                    fsw.Write(polygons[i].UVPoints[k].X);
                    fsw.Write(polygons[i].UVPoints[k].Y);
                }
            }

            fsw.Write((uint)fans.Count);

            for (int i = 0; i < fans.Count; i++)
            {
                fsw.Write(fans[i].pos.X);
                fsw.Write(fans[i].pos.Y);
                fsw.Write(fans[i].pos.Z);
            }

            int numEs = 0;
            if (cameras.Count > 0)
                numEs++;
            if (fdlights.Length > 0)
                numEs++;
            if (models.Count > 0)
                numEs++;

            fsw.Write(numEs);

            if(cameras.Count>0)
            {
                fsw.Write("cam");
                fsw.Write(cameras.Count);
                for (int i = 0; i < cameras.Count; i++)
                {
                    fsw.Write(cameras[i].index);
                    fsw.Write(cameras[i].part);
                    fsw.Write((uint)cameras[i].TYPE);
                    fsw.Write(scale*cameras[i].pos.X);
                    fsw.Write(scale*cameras[i].pos.Y);
                    fsw.Write(scale*cameras[i].pos.Z);
                    fsw.Write(cameras[i].dir.X);
                    fsw.Write(cameras[i].dir.Y);
                    fsw.Write(cameras[i].dir.Z);
                }
            }

            if (fdlights.Length > 0)
            {
                fsw.Write("lit");
                fsw.Write(fdlights.Length);
                for (int i = 0; i < fdlights.Length; i++)
                {
                    fsw.Write(fdlights[i].index-1);
                    fsw.Write((uint)fdlights[i].type);
                    fsw.Write(fdlights[i].innerAngle/360f*(float)(Math.PI));
                    fsw.Write(fdlights[i].outerAngle/360f*(float)(Math.PI));
                    fsw.Write(fdlights[i].pos.X);
                    fsw.Write(fdlights[i].pos.Y);
                    fsw.Write(fdlights[i].pos.Z);
                    fsw.Write(fdlights[i].targs.Count);
                    for (int k = 0; k < fdlights[i].targs.Count; k++)
                    {
                        fsw.Write(fdlights[i].targs[k].type);
                        fsw.Write(fdlights[i].targs[k].ct);
                        fsw.Write(fdlights[i].targs[k].dir.X);
                        fsw.Write(fdlights[i].targs[k].dir.Y);
                        fsw.Write(fdlights[i].targs[k].dir.Z);
                    }
                }
            }

            if (models.Count > 0)
            {
                fsw.Write("mdl");
                fsw.Write(models.Count);
                for (int i = 0; i < models.Count; i++)
                {
                    for (int k = 0; k < mdlTemplates.Length; k++)
                    {
                        if (mdlTemplates[k].name == models[i].name)
                        {
                            fsw.Write(mdlTemplates[k].modelNames.Length);
                            for (int j = 0; j < mdlTemplates[k].modelNames.Length; j++)
                            {
                                fsw.Write(mdlTemplates[k].modelNames[j]);
                                fsw.Write(mdlTemplates[k].textureNames[j]);
                                fsw.Write(models[i].pos.X);
                                fsw.Write(models[i].pos.Y);
                                fsw.Write(models[i].pos.Z);
                                fsw.Write(models[i].rot.X);
                                fsw.Write(models[i].rot.Y);
                                fsw.Write(models[i].rot.Z);
                                fsw.Write(mdlTemplates[k].scale.X);
                                fsw.Write(mdlTemplates[k].scale.Y);
                                fsw.Write(mdlTemplates[k].scale.Z);
                            }
                        }
                    }
                }
            }
            fsw.Close(); 
        }

        // finds all orders of specified Vector3s
        public static Vector3[][] GetCombos(Vector3[] Orig)
        {
            return GetCombosHelper(new Vector3[0], Orig);
        }
        public static Vector3[][] GetCombosHelper(Vector3[] Header, Vector3[] Orig)
        {
            Vector3[][] ret = new Vector3[0][];
            if (Orig.Length == 1)
            {
                ret = new Vector3[1][];
                ret[0] = new Vector3[Header.Length + Orig.Length];
                for (int i = 0; i < Header.Length; i++)
                    ret[0][i] = Header[i];
                for (int i = 0; i < Orig.Length; i++)
                    ret[0][i + Header.Length] = Orig[i];
                return ret;
            }
            for (int i = 0; i < Orig.Length; i++)
            {
                Vector3[] newHeader = new Vector3[Header.Length+1];
                for (int k = 0; k < Header.Length; k++)
                    newHeader[k] = Header[k];
                newHeader[newHeader.Length - 1] = Orig[i];
                Vector3[] clone = new Vector3[Orig.Length];
                for (int k = 0; k < clone.Length; k++)
                    clone[k] = Orig[k];
                Vector3 temp = clone[0];
                clone[0] = clone[i];
                clone[i] = temp;
                Vector3[] notOrig = new Vector3[Orig.Length - 1];
                for (int k = 0; k < notOrig.Length; k++)
                    notOrig[k] = clone[k + 1];
                Vector3[][] add = GetCombosHelper(newHeader, notOrig);
                Vector3[][] newRet = new Vector3[ret.Length + add.Length][];
                for (int k = 0; k < ret.Length; k++)
                    newRet[k] = ret[k];
                for (int k = 0; k < add.Length; k++)
                    newRet[k + ret.Length] = add[k];
                ret = newRet;
            }
            return ret;
        }

        //determines if these points form a polygon without any lines crossing
        public static bool LineCollisions(Vector3[] points)
        {
            if (points[0].X != points[points.Length - 1].X || points[0].Y != points[points.Length - 1].Y || points[0].Z != points[points.Length - 1].Z)
            {
                Vector3[] nPoints = new Vector3[points.Length + 1];
                for (int i = 0; i < points.Length; i++)
                    nPoints[i] = points[i];
                nPoints[nPoints.Length - 1] = nPoints[0];
                points = nPoints;
            }
            for (int i = 0; i < points.Length - 2; i++)
            {
                Vector3 line1S = points[i], line1E = points[i + 1],
                        line2S, line2E;
                for (int k = i + 1; k < points.Length - 1; k++)
                {
                    line2S = points[k];
                    line2E = points[k + 1];
                    // y1=m1x1+b1
                    // y2=m2x2+b2
                    // m1x+b1=m2x+b2
                    // m1x = m2x+b2-b1
                    // m1x-m2x = b2-b1
                    // (m1-m2)x = b2-b1
                    // x = (b2-b1)/(m1-m2)
                    float m1 = (line1E.Y - line1S.Y) / (line1E.X - line1S.X);
                    float m2 = (line2E.Y - line2S.Y) / (line2E.X - line2S.X);
                    if (m1 == m2)
                        continue;
                    float b1 = line1S.Y - ((line1S.X) * m1);
                    float b2 = line2S.Y - ((line2S.X) * m2);
                    float iX = (b2 - b1) / (m1 - m2);
                    float iY = (m1 * iX) + b1;
                    float iY2 = (m2 * iX) + b2;
                    if( ( (line1E.X>line1S.X && (iX>=line1S.X && iX<=line1E.X)) || (line1E.X<=line1S.X && (iX<=line1S.X && iX>=line1E.X)) ) &&
                        ( (line2E.X>line2S.X && (iX>=line2S.X && iX<=line2E.X)) || (line2E.X<=line2S.X && (iX<=line2S.X && iX>=line2E.X)) ) &&
                        ( (line1E.Y>line1S.Y && (iY>=line1S.Y && iY<=line1E.Y)) || (line1E.Y<=line1S.Y && (iY<=line1S.Y && iY>=line1E.Y)) ) &&
                        ( (line2E.Y>line2S.Y && (iY>=line2S.Y && iY<=line2E.Y)) || (line2E.Y<=line2S.Y && (iY<=line2S.Y && iY>=line2E.Y)) ) )
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static Matrix ComputeTangentMatrix(Vector3 tangent, Vector3 normal)
        {
            Matrix worldToTangentSpace = Matrix.Identity;
            Vector3 one = Vector3.Cross(normal, tangent);
            worldToTangentSpace.M11 = one.X;
            worldToTangentSpace.M21 = one.Y;
            worldToTangentSpace.M31 = one.Z;
            worldToTangentSpace.M12 = tangent.X;
            worldToTangentSpace.M22 = tangent.Y;
            worldToTangentSpace.M32 = tangent.Z;
            worldToTangentSpace.M13 = normal.X;
            worldToTangentSpace.M23 = normal.Y;
            worldToTangentSpace.M33 = normal.Z;
            return worldToTangentSpace;
        }

        public static void UnitTestIsCCW(String path)
        {
            Vector3[] t1 = { new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector3(-1, 0, 0), new Vector3(0, -1, 0), };
            Vector3[] t2 = { new Vector3(1, 1, 0), new Vector3(-1, 1, 0), new Vector3(-1, -1, 0), new Vector3(1, -1, 0), };
            Vector3[] t3 = { new Vector3(5, -5, 0), new Vector3(4, -4, 0), new Vector3(3, -5, 0), new Vector3(4, -6, 0), };
            Vector3[] t4 = { new Vector3(5, -4, 0), new Vector3(3, -4, 0), new Vector3(3, -6, 0), new Vector3(5, -6, 0), };
            StreamWriter testerfile = new StreamWriter(path + "testresults.txt");
            testerfile.WriteLine("t1: " + IsCCW(t1));
            testerfile.WriteLine("t2: " + IsCCW(t2));
            testerfile.WriteLine("t3: " + IsCCW(t3));
            testerfile.WriteLine("t4: " + IsCCW(t4));
            testerfile.Close();
        }

        public static bool IsCCW(Vector3[] pts)
        {
            Vector3 center = Vector3.Zero;
            for (int i = 0; i < pts.Length; i++)
                center += pts[i];
            center /= pts.Length;
            for (int i = 0; i < pts.Length; i++)
                pts[i] -= center;
            for (int i = 0; i < pts.Length - 1; i++)
            {
                float angleTheta = (float)((Math.Atan2(pts[i + 1].Y, pts[i + 1].X) - Math.Atan2(pts[i].Y, pts[i].X))*180/Math.PI);
                while(angleTheta>=180)
                    angleTheta-=360;
                while(angleTheta<-180)
                    angleTheta+=360;
                if(angleTheta<0)
                    return false;
            }
            return true;
        }

        public static Vector3[] GetUV(Vector3[] TPoints, Vector4 uvec, float uscale, Vector4 vvec, float vscale)
        {
            Vector3[] UVPoints = new Vector3[TPoints.Length];

            for (int i = 0; i < TPoints.Length; i++)
            {
                UVPoints[i] = Vector3.Zero;
                float ustep = uscale * 512;
                float vstep = vscale * 512;
                UVPoints[i].X = ((TPoints[i].X+uvec.W) / ustep)+0.5f;
                UVPoints[i].Y = ((TPoints[i].Y+vvec.W) / vstep)+0.5f;
            }

            return UVPoints;
        }

        public static double ParseDouble(String a)
        {
            if (a.IndexOf('e') < 0)
                return Double.Parse(a);
            else
                return 0;
        }
    }
}
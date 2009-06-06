using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using SongDataIO;
using FVProductions.Utility;

namespace Unsigned
{
    class Rocker
    {
        public static SongData SongData;
        public Vector3 Position { get; set; }
        public float Yaw { get; set; }
        private Dictionary<String,FVModel> models;
        private Texture2D tex;
        private Instrument instrument;
        private Dictionary<String, Texture2D> texInstr;
        private Dictionary<String,FVModel> mdlInstr;
        private CharacterIdol idol;
        private AnimationInfo Animation;
        private float Scale;
                                     
        public Rocker(CharacterIdol idol, Instrument instr)
        {
            instrument = instr;
            Yaw = 0;
            this.idol = idol;
            Scale = 10;
        }

        public void Load(ContentManager Content)
        {
            {
                models = new Dictionary<String, FVModel>();
                String[] bodyParts = System.IO.Directory.GetFiles("Content\\meshes\\avatars\\");
                for (int i = 0; i < bodyParts.Length; i++)
                {
                    String fn = bodyParts[i].Substring(bodyParts[i].IndexOf("meshes\\"));
                    fn = fn.Substring(0, fn.LastIndexOf('.'));
                    String name = fn.Substring(fn.LastIndexOf('\\') + 1);
                    FVModel m = ModelLoader.LoadModel(fn);
                    models.Add(name.ToUpper(), m);
                }
            }
            if (idol.InstrumentBrand != "[none]")
            {
                {
                    mdlInstr = new Dictionary<String, FVModel>();
                    String path = "meshes\\instruments\\" + instrument.CodeName + "\\" + idol.InstrumentBrand + "\\";
                    String contentPath = "Content\\"+path;
                    String[] mdlFiles = System.IO.Directory.GetFiles(contentPath);
                    for(int i=0;i<mdlFiles.Length;i++)
                    {
                        String str = mdlFiles[i].Substring(mdlFiles[i].LastIndexOf('\\') + 1);
                        str = str.Substring(0, str.LastIndexOf('.'));
                        if (str.StartsWith("mdl" + idol.InstrumentIndex))
                        {
                            String loadStr = mdlFiles[i].Substring(mdlFiles[i].IndexOf("meshes\\"));
                            loadStr = loadStr.Substring(0, loadStr.IndexOf('.'));
                            mdlInstr.Add(str.Substring(str.IndexOf('_')+1).ToUpper(), ModelLoader.LoadModel(loadStr));
                        }
                    }
                }
                {
                    texInstr = new Dictionary<String, Texture2D>();
                    String path = "textures\\instruments\\" + instrument.CodeName + "\\" + idol.InstrumentBrand + "\\";
                    String contentPath = "Content\\" + path;
                    String[] mdlFiles = System.IO.Directory.GetFiles(contentPath);
                    for (int i = 0; i < mdlFiles.Length; i++)
                    {
                        String str = mdlFiles[i].Substring(mdlFiles[i].LastIndexOf('\\') + 1);
                        str = str.Substring(0, str.LastIndexOf('.'));
                        if (str.StartsWith("tex" + idol.InstrumentIndex))
                        {
                            String loadStr = mdlFiles[i].Substring(mdlFiles[i].IndexOf("textures\\"));
                            loadStr = loadStr.Substring(0, loadStr.IndexOf('.'));
                            texInstr.Add(str.Substring(str.IndexOf('_') + 1).ToUpper(), Content.Load<Texture2D>(loadStr));
                        }
                    }
                }
            }
            tex = Content.Load<Texture2D>("textures\\avatars\\fanTex");
            String skeleFilename = "Content\\animations\\"+instrument.CodeName+"Hierarchy.txt";
            String animFilename = "Content\\animations\\"+instrument.CodeName+".una";
            if (System.IO.File.Exists(skeleFilename) && System.IO.File.Exists(animFilename))
            {
                Animation = AnimationInfo.Load(skeleFilename, animFilename);
                Animation.AnimationIndex = 0;
                Animation.CurrentAnimationTimeValue = 0;
            }
        }

        public void Draw(SongTime songTime, FVShader effect)
        {
            Yaw = (float)Math.Atan2(Position.X, Position.Z+100)+MathHelper.Pi;

            Matrix w = Matrix.CreateRotationY(Yaw);

            if (Animation != null)
            {
                Joint j = Animation.Skeleton.RootJoint;

                DrawJoint(j, w, effect);
            }
        }

        private void DrawJoint(Joint j, Matrix world, FVShader effect)
        {
            Matrix JointMatrix = (Animation == null || Animation.CurrentAnimation == null) ?
                                 Matrix.Identity :
                                 Animation.CurrentAnimation.GetJointMatrix(Animation.CurrentAnimationTimeValue, j.JointMatrix);
            Matrix postWorld = JointMatrix * Matrix.CreateTranslation(j.postOffset) * world;
            
            if (models.ContainsKey(j.Name.ToUpper()))
            {
                effect.World = Matrix.CreateTranslation(-j.preOffset) * postWorld * Matrix.CreateScale(Scale) * Matrix.CreateTranslation(Position);
                effect.DiffuseTexture = tex;
                effect.CommitChanges();
                models[j.Name.ToUpper()].Draw();
            }
            else if (mdlInstr != null && mdlInstr.ContainsKey(j.Name.ToUpper()))
            {
                effect.World = Matrix.CreateTranslation(-j.preOffset) * postWorld * Matrix.CreateScale(Scale) * Matrix.CreateTranslation(Position);
                effect.DiffuseTexture = texInstr[j.Name.ToUpper()];
                effect.CommitChanges();
                mdlInstr[j.Name.ToUpper()].Draw();
            }
            for (int i = 0; i < j.Children.Count; i++)
                DrawJoint(j.Children[i], postWorld,effect);
        }

        public CharacterIdol GetCharacter()
        {
            return idol;
        }

        public Instrument GetInstrument()
        {
            return instrument;
        }
    }
}

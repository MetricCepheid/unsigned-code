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
    public class Rocker
    {
        public static SongData SongData;
        public Vector3 Position { get; set; }
        private float _yaw = 0;
        private bool setYaw = false;
        public float Yaw 
        { 
            get 
            { 
                return _yaw; 
            } 
            set 
            { 
                _yaw = value; 
                setYaw = true; 
            } 
        }
        private Dictionary<String,FVModel> models;
        private Texture2D tex;
        public Instrument Instrument;
        private Dictionary<String, Texture2D> texInstr;
        private Dictionary<String,FVModel> mdlInstr;
        private CharacterIdol idol;
        public AnimationWrapper Animation;
        private float Scale;
                                     
        public Rocker(CharacterIdol idol, Instrument instr)
        {
            Instrument = instr;
            _yaw = 0;
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
                    String path = "meshes\\instruments\\" + Instrument.CodeName + "\\" + idol.InstrumentBrand + "\\";
                    String contentPath = "Content\\" + path;
                    String[] mdlFiles = System.IO.Directory.GetFiles(contentPath);
                    for (int i = 0; i < mdlFiles.Length; i++)
                    {
                        String str = mdlFiles[i].Substring(mdlFiles[i].LastIndexOf('\\') + 1);
                        str = str.Substring(0, str.LastIndexOf('.'));
                        if (str.StartsWith("mdl" + idol.InstrumentIndex))
                        {
                            String loadStr = mdlFiles[i].Substring(mdlFiles[i].IndexOf("meshes\\"));
                            loadStr = loadStr.Substring(0, loadStr.IndexOf('.'));
                            mdlInstr.Add(str.Substring(str.IndexOf('_') + 1).ToUpper(), ModelLoader.LoadModel(loadStr));
                        }
                    }
                }
                {
                    texInstr = new Dictionary<String, Texture2D>();
                    String path = "textures\\instruments\\" + Instrument.CodeName + "\\" + idol.InstrumentBrand + "\\";
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
            {
                ContentManager cntnt = new ContentManager(Global.Services);
                cntnt.RootDirectory = "Content\\textures\\avatars";
                RenderTarget2D rt = new RenderTarget2D(Global.Graphics.GraphicsDevice, 512, 512, 1, SurfaceFormat.Color);
                DepthStencilBuffer dst = new DepthStencilBuffer(Global.Graphics.GraphicsDevice, 512, 512, Global.Graphics.GraphicsDevice.DepthStencilBuffer.Format);
                RenderTarget2D lastRT = (RenderTarget2D)Global.Graphics.GraphicsDevice.GetRenderTarget(0);
                DepthStencilBuffer lastDST = Global.Graphics.GraphicsDevice.DepthStencilBuffer;
                Global.Graphics.GraphicsDevice.SetRenderTarget(0, rt);
                Global.Graphics.GraphicsDevice.DepthStencilBuffer = dst;
                Global.Graphics.GraphicsDevice.Clear(Color.Black);
                SpriteBatch sb = new SpriteBatch(Global.Graphics.GraphicsDevice);
                sb.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
                sb.Draw(cntnt.Load<Texture2D>("skin_" + idol.SkinTexIndex), new Rectangle(0, 0, 512, 512), Color.White);
                sb.Draw(cntnt.Load<Texture2D>("hair" + idol.HairTexIndex), new Rectangle(0, 0, 512, 512), Color.White);
                sb.Draw(cntnt.Load<Texture2D>("shirt_base"), new Rectangle(0, 0, 512, 512), CharacterIdol.IdolColors[(int)idol.ShirtBGColor]);
                sb.Draw(cntnt.Load<Texture2D>("pants_"+idol.PantsTexIndex), new Rectangle(0, 0, 512, 512), Color.White);
                sb.Draw(cntnt.Load<Texture2D>("shoe"+idol.ShoeTexIndex), new Rectangle(0, 0, 512, 512), Color.White);
                sb.Draw(cntnt.Load<Texture2D>("shirt_overlay"+idol.ShirtFGTexIndex), new Rectangle(0, 0, 512, 512), CharacterIdol.IdolColors[(int)idol.ShirtFGColor]);
                sb.Draw(cntnt.Load<Texture2D>("necklace"+idol.NecklaceTexIndex), new Rectangle(0, 0, 512, 512), CharacterIdol.IdolColors[(int)idol.NecklaceColor]);
                sb.End();
                Global.Graphics.GraphicsDevice.SetRenderTarget(0, lastRT);
                Global.Graphics.GraphicsDevice.DepthStencilBuffer = lastDST;
                tex = rt.GetTexture();
                rt.Dispose();
                dst.Dispose();
                cntnt.Unload();
            }
            String skeleFilename = "Content\\animations\\" + Instrument.CodeName + "Hierarchy.txt";
            String animFilename = "Content\\animations\\" + Instrument.CodeName + ".una";
            if (System.IO.File.Exists(skeleFilename) && System.IO.File.Exists(animFilename))
            {
                Animation = new AnimationWrapper(AnimationInfo.Load(skeleFilename, animFilename));
                Animation.RunAnimation(AnimationWrapper.ARIType.RunToPoint, "MoveHand", 0.1f, 1.0f);
            }
        }

        public void Draw(SongTime songTime, FVShader effect)
        {
            Animation.Update(songTime);

            if(!setYaw)
                _yaw = (float)Math.Atan2(Position.X, Position.Z+100)+MathHelper.Pi;

            Matrix w = Matrix.CreateRotationY(Yaw);

            if (Animation != null)
            {
                DrawJoint(Animation.RootJoint, w, effect);
                DrawJoint(Animation.InstrRootJoint, w, effect);
            }
        }

        private void DrawJoint(Joint j, Matrix world, FVShader effect)
        {
            Vector3 offset = j.Root.Name == "Root" ? Animation.GetOffset() : Vector3.Zero;
            offset.Z = -offset.Z;
            offset.X = -offset.X;
            Matrix JointMatrix = Animation.GetJointMatrix(j.JointMatrix);
            Matrix postWorld = JointMatrix * Matrix.CreateTranslation(j.postOffset) * world;
            
            if (models.ContainsKey(j.Name.ToUpper()))
            {
                effect.World = Matrix.CreateTranslation(-j.preOffset) * postWorld * Matrix.CreateTranslation(offset) * Matrix.CreateScale(Scale) * Matrix.CreateTranslation(Position);
                effect.DiffuseTexture = tex;
                effect.CommitChanges();
                models[j.Name.ToUpper()].Draw();
            }
            else if (mdlInstr != null && mdlInstr.ContainsKey(j.Name.ToUpper()))
            {
                effect.World = Matrix.CreateTranslation(-j.preOffset) * postWorld * Matrix.CreateTranslation(offset) * Matrix.CreateScale(Scale) * Matrix.CreateTranslation(Position);
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
            return Instrument;
        }

        internal void Reset()
        {
            
        }
    }
}

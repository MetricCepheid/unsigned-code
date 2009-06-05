using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace UnsignedAnimationEditor
{
    public partial class Form1 : Form
    {
        private String animationFilename = null;

        public Game1 Game;

        private AnimationInfo _ai;
        private AnimationInfo AnimInfo
        {
            get { return _ai; }
            set
            {
                _ai = value;
                if (_ai != null)
                    controlPanel1.Enabled = true;
                else
                    controlPanel1.Enabled = false;
                controlPanel1.AnimationInfo = _ai;
                Game.AnimationInfo = _ai;
            }
        }

        public Form1()
        {
            InitializeComponent();
            this.Text = "Unsigned Character Animation Editor";
        }

        public IntPtr GetDrawSurface()
        {
            return pictureBox1.Handle;
        }

        protected override void OnClosed(EventArgs e)
        {
            Application.Exit();
            base.OnClosed(e);
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "Skeleton ASCII file (TXT)|*.txt";
            d.Title = "Load Skeleton";
            DialogResult dr = d.ShowDialog();
            if(dr == DialogResult.OK)
            {
                AnimInfo = new AnimationInfo();
                AnimInfo.Skeleton = new Skeleton();
                AnimInfo.Skeleton.Load(System.IO.File.OpenRead(d.FileName));
                controlPanel1.AnimationInfo = AnimInfo;
                Game.AnimationInfo = AnimInfo;
                controlPanel1.AnimationInfoChanged();
            }
        }

        private void pictureBox1_MouseEnter(object sender, EventArgs e)
        {
            Game.CanUseMouse = true;
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            Game.CanUseMouse = false;
        }

        private void Form1_MouseLeave(object sender, EventArgs e)
        {
            Game.CanUseMouse = false;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "Skeleton ASCII file (TXT)|*.txt";
            d.Title = "Load Skeleton";
            DialogResult dr = d.ShowDialog();
            if (dr == DialogResult.OK)
            {
                AnimInfo = new AnimationInfo();
                AnimInfo.Skeleton = new Skeleton();
                AnimInfo.Skeleton.Load(System.IO.File.OpenRead(d.FileName));
            }
            else return;
            d = new OpenFileDialog();
            d.Filter = "Unsigned Character Animation File (UNA)|*.una";
            d.Title = "Load Animation";
            dr = d.ShowDialog();
            if (dr == DialogResult.OK)
            {
                animationFilename = d.FileName;
            }
            else return;
            LoadHelper();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (animationFilename == null)
                saveAsToolStripMenuItem_Click(this, new EventArgs());
            else
                SaveHelper();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog d = new SaveFileDialog();
            d.Filter = "Unsigned Character Animation File (UNA)|*.una";
            d.Title = "Save Animation";
            DialogResult dr = d.ShowDialog();
            if (dr == DialogResult.OK)
            {
                animationFilename = d.FileName;
                SaveHelper();
            }
        }

        private void SaveHelper()
        {
            this.Text = "Unsigned Character Animation Editor - " + animationFilename;
            BinaryWriter bw = new BinaryWriter(File.OpenWrite(animationFilename));
            bw.Write((uint)AnimInfo.Animations.Count);
            for (int i = 0; i < AnimInfo.Animations.Count; i++)
            {
                bw.Write(AnimInfo.Animations[i].Name);
                bw.Write(AnimInfo.Animations[i].Length);
                bw.Write((uint)AnimInfo.Animations[i].KeyframeCount);
                for (int k = 0; k < AnimInfo.Animations[i].KeyframeCount; k++)
                {
                    Frame f = AnimInfo.Animations[i].Keyframe(k);
                    bw.Write(f.Time);
                    bw.Write((uint)f.Matrices.Length);
                    for (int j = 0; j < f.Matrices.Length; j++)
                    {
                        bw.Write(f.Matrices[j].M11);
                        bw.Write(f.Matrices[j].M12);
                        bw.Write(f.Matrices[j].M13);
                        bw.Write(f.Matrices[j].M14);

                        bw.Write(f.Matrices[j].M21);
                        bw.Write(f.Matrices[j].M22);
                        bw.Write(f.Matrices[j].M23);
                        bw.Write(f.Matrices[j].M24);

                        bw.Write(f.Matrices[j].M31);
                        bw.Write(f.Matrices[j].M32);
                        bw.Write(f.Matrices[j].M33);
                        bw.Write(f.Matrices[j].M34);

                        bw.Write(f.Matrices[j].M41);
                        bw.Write(f.Matrices[j].M42);
                        bw.Write(f.Matrices[j].M43);
                        bw.Write(f.Matrices[j].M44);
                    }
                }
            }
            bw.Close();
        }

        private void LoadHelper()
        {
            this.Text = "Unsigned Character Animation Editor - " + animationFilename;
            BinaryReader br = new BinaryReader(File.OpenRead(animationFilename));
            AnimInfo.Animations = new List<Animation>();
            uint numAnims = br.ReadUInt32();
            for (int i = 0; i < numAnims; i++)
            {
                Animation anim = new Animation(br.ReadString(), AnimInfo.Skeleton.GetMatrixLength());
                anim.Frames.Clear();
                anim.Length = br.ReadSingle();
                uint numKeyframes = br.ReadUInt32();
                for (int k = 0; k < numKeyframes; k++)
                {
                    Frame f = new Frame(br.ReadInt32(), (int)br.ReadUInt32());
                    if (f.Matrices.Length != AnimInfo.Skeleton.GetMatrixLength())
                    {
                        MessageBox.Show("Error: Skeleton does not match animation");
                        AnimInfo.Animations = null;
                        return;
                    }
                    for (int j = 0; j < f.Matrices.Length; j++)
                    {
                        f.Matrices[j] = new Microsoft.Xna.Framework.Matrix(
                            br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle(),
                            br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle(),
                            br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle(),
                            br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                    }
                    anim.Frames.Add(f);
                }
                AnimInfo.Animations.Add(anim);
            }
            br.Close();
            controlPanel1.AnimationInfo = AnimInfo;
            controlPanel1.AnimationInfoChanged();
            Game.AnimationInfo = AnimInfo;
        }
    }
}

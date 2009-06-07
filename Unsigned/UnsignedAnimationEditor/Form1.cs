using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using Unsigned;

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
            AnimInfo.SaveAnimations(animationFilename);
        }

        private void LoadHelper()
        {
            this.Text = "Unsigned Character Animation Editor - " + animationFilename;
            AnimInfo.LoadAnimation(animationFilename);
            controlPanel1.AnimationInfo = AnimInfo;
            controlPanel1.AnimationInfoChanged();
            Game.AnimationInfo = AnimInfo;
        }
    }
}

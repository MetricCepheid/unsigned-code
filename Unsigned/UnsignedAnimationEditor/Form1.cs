using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace UnsignedAnimationEditor
{
    public partial class Form1 : Form
    {
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
            AnimInfo = new AnimationInfo();
            AnimInfo.Skeleton = new Skeleton();
            AnimInfo.Skeleton.Load(System.IO.File.OpenRead("GuitaristHierarchy.txt"));
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
    }
}

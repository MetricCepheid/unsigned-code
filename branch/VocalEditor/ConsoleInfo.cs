using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace VocalEditor
{
    public partial class ConsoleInfo : Form
    {
        MainForm mf;
        public ConsoleInfo(MainForm m)
        {
            InitializeComponent();
            
            mf = m;
        }

        public void Update(VoxSong s, VocalPreviewer vp)
        {
            String str = "";
            str += "Vocal Previewer:\r\n";
            str += "Size: " + vp.Size.Width + "x" + vp.Size.Height + "\r\n";
            str += "Bounds: " + mf.vocalPane.Bounds.ToString() + "\r\n";
            str += "ClientRect: " + mf.vocalPane.ClientRectangle.ToString() + "\r\n";
            str += "\r\nConsole Window:\r\n";
            str += "Bounds: " + this.Bounds.ToString() + "\r\n";
            textBox1.Text = str;
        }

        protected override void  OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            base.OnClosing(e);
            Hide();
            mf.UncheckConsole();
        }
    }
}
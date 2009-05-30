using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SongDataIO;

namespace SongdataVersionChecker
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            InstrumentMaster.Singleton.Load(System.IO.Directory.GetCurrentDirectory() + "\\Instruments\\");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "UNS Songdata Files (*.uns)|*.uns|GB* Songdata Files (*.gba)|*.gba";
            d.AddExtension = true;
            d.AutoUpgradeEnabled = true;
            d.CheckFileExists = true;
            d.CheckPathExists = true;
            d.Multiselect = false;
            d.RestoreDirectory = false;
            d.ShowHelp = false;
            d.ShowReadOnly = false;
            d.SupportMultiDottedExtensions = false;
            d.Title = "Open SongData File";
            d.ValidateNames = true;
            DialogResult dr = d.ShowDialog(this);
            if (dr == DialogResult.OK)
            {
                SongData SongData = SongDataLoader.LoadSongData(d.FileName);
                textBox1.Text = "Songdata Version: " + SongData.info.version;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SongDataIO;

namespace SongdataConverter
{
    public partial class Form1 : Form
    {
        private SongData _songData = null;
        private SongData SongData
        {
            get { return _songData; }
            set { _songData = value; NewSongDataLoaded(); }
        }
        private String songDataFilename = null;

        public static Cursor DeleteCursor;
        public static bool Ctrl { get { return Keyboard.IsKeyPressed(Keyboard.VirtualKeyStates.VK_CONTROL); } }

        public Form1()
        {
            InstrumentMaster.Singleton.Load(System.IO.Directory.GetCurrentDirectory() + "\\Instruments\\");
            InitializeComponent();
            NewSongDataLoaded();
            leadGuitarDataViewer.InstrumentType = InstrumentMaster.Singleton.GetInstrument("LGT");
            leadGuitarDataViewer.ViewType = InstrumentDataViewer.InstrumentViewType.Guitar;
            bassDataViewer.InstrumentType = InstrumentMaster.Singleton.GetInstrument("BAS");
            bassDataViewer.ViewType = InstrumentDataViewer.InstrumentViewType.Guitar;
            drumsDataViewer.InstrumentType = InstrumentMaster.Singleton.GetInstrument("SET");
            drumsDataViewer.ViewType = InstrumentDataViewer.InstrumentViewType.Drums;

            Bitmap b = (Bitmap)Bitmap.FromFile("Content\\trash.png");
            DeleteCursor = new Cursor(b.GetHicon());
        }

        private void NewSongDataLoaded()
        {
            tabControl.Enabled = SongData != null;
            saveAsToolStripMenuItem.Enabled = SongData != null;
            saveToolStripMenuItem.Enabled = SongData != null;
            songInfoViewer.SongData = SongData;
            leadGuitarDataViewer.SongData = SongData;
            bassDataViewer.SongData = SongData;
            drumsDataViewer.SongData = SongData;
            bandBonusesDataViewer1.SongData = SongData;

            if (SongData == null)
                this.Text = "Unsigned 2.0 SongData Conversion Utility";
            else
                this.Text = "Unsigned 2.0 SongData Conversion Utility - " + SongData.info.name;
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutForm f = new AboutForm();
            f.ShowDialog(this);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "UNS Songdata Files (*.uns)|*.uns|GB* Songdata Files (*.gba)|*.gba|Feedback Chart Files (*.chart)|*.chart";
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
                if (d.FileName.EndsWith(".chart"))
                {
                    SongData = ChartLoader.LoadChart(d.FileName);
                }
                else
                {
                    SongData = SongDataLoader.LoadSongData(d.FileName);
                }
                songDataFilename = d.FileName;
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SongData != null)
            {
                if (songDataFilename == null || !songDataFilename.EndsWith(".uns"))
                {
                    saveAsToolStripMenuItem_Click(sender, e);
                }
                else
                    Save(songDataFilename);
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SongData != null)
            {
                SaveFileDialog d = new SaveFileDialog();
                d.Filter = "UNS Songdata Files (*.uns)|*.uns";
                d.AddExtension = true;
                d.AutoUpgradeEnabled = true;
                d.CheckPathExists = true;
                d.RestoreDirectory = false;
                d.ShowHelp = false;
                d.SupportMultiDottedExtensions = false;
                d.Title = "Open SongData File";
                d.ValidateNames = true;
                DialogResult dr = d.ShowDialog(this);
                if (dr == DialogResult.OK)
                {
                    Save(d.FileName);
                }
            }
        }

        private void Save(String filename)
        {
            try
            {
                songInfoViewer.ApplyChanges();
                leadGuitarDataViewer.ApplyChanges();
                bassDataViewer.ApplyChanges();
                drumsDataViewer.ApplyChanges();
            }
            catch (Exception e)
            {
                MessageBox.Show(this, "Error Saving:\n" + e.Message, "Save Failed", MessageBoxButtons.OK);
                return;
            }

            songDataFilename = filename;
            SongDataLoader.SaveSongData(SongData, filename);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (SongData != null)
            {
                DialogResult dr = MessageBox.Show(this, "Are you sure you want to exit?\nYou will lose any unsaved changes", "Confirm Exit", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                if (dr == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
            base.OnClosing(e);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

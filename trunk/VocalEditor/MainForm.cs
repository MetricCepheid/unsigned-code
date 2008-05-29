using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace VocalEditor
{
    public partial class MainForm : Form
    {
        
        Song song;
        ConsoleInfo consoleWindow;
        private bool changed;
        Timer t;

        public static int RES_ONE = 0, RES_TWO = 1, RES_THREE = 2, RES_FOUR = 3, RES_SIX = 4, RES_EIGHT = 5, RES_TWELVE = 6, RES_SIXTEEN = 7, RES_THIRTYTWO = 8, RES_SIXTYFOUR = 9;

        public MainForm()
        {
            InitializeComponent();
            consoleWindow = new ConsoleInfo(this);
            vocalPane.SetParentForm(this);
            UpdateActivations();
            changed = false;
            t = new Timer();
            t.Tick += new EventHandler(Update);
            t.Interval = 40;
            t.Start();
        }

        public void Update(Object stateInfo, EventArgs e)
        {
            vocalPane.Invalidate();
        }

        private void UpdateActivations()
        {
            
            nodeTypeTalkyRadio.Enabled = false;
            nodeTypeVocalRadio.Enabled = false;
            nodeTextBox.Enabled = false;
            nodeTimeNumUpDown.Enabled = false;
            nodeLengthNumUpDown.Enabled = false;
            nodeConnectedCheckbox.Enabled = false;
            phraseTypeEmptyRadio.Enabled = false;
            phraseTypeRhythmRadio.Enabled = false;
            phraseTypeVocalsRadio.Enabled = false;
            resTrackbar.Enabled = false;
            xScaleTrackBar.Enabled = false;
            controlModeAddRadio.Enabled = false;
            controlModeEditRadio.Enabled = false;
            controlModeRemoveRadio.Enabled = false;
            editingNodesRadio.Enabled = false;
            editingPhrasesRadio.Enabled = false;
            vocalPane.Cursor = Cursors.Default;

            if(song!=null)
            if (song.valid)
            {
                resTrackbar.Enabled = true;
                xScaleTrackBar.Enabled = true;
                controlModeAddRadio.Enabled = true;
                controlModeEditRadio.Enabled = true;
                controlModeRemoveRadio.Enabled = true;
                editingNodesRadio.Enabled = true;
                editingPhrasesRadio.Enabled = true;
                if (controlModeAddRadio.Checked)
                    vocalPane.Cursor = Cursors.UpArrow;
                else if (controlModeEditRadio.Checked)
                    vocalPane.Cursor = Cursors.NoMove2D;
                else if (controlModeRemoveRadio.Checked)
                    vocalPane.Cursor = Cursors.No;
                else
                    vocalPane.Cursor = Cursors.Default;

                if (editingPhrasesRadio.Checked)
                {
                    phraseTypeEmptyRadio.Enabled = true;
                    phraseTypeRhythmRadio.Enabled = true;
                    phraseTypeVocalsRadio.Enabled = true;
                }
                else if (editingNodesRadio.Checked)
                {
                    nodeTypeTalkyRadio.Enabled = true;
                    nodeTypeVocalRadio.Enabled = true;
                    nodeTextBox.Enabled = true;
                    nodeTimeNumUpDown.Enabled = true;
                    nodeLengthNumUpDown.Enabled = true;
                    nodeConnectedCheckbox.Enabled = true;
                }
            }
            consoleWindow.Update(song, vocalPane);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.AddExtension = true;
            d.Multiselect = false;
            d.Filter = "Unsigned SongData files (*.uns)|*.uns";
            DialogResult dr = d.ShowDialog();
            if (dr == DialogResult.OK)
            {
                song = new Song(d.FileName);
                vocalPane.SetSong(song);
            }
            if(song!=null)
            if (song.valid)
            {
                displayOnToolStripMenuItem.Checked = true;
                vocalPane.display = true;
            }
            consoleWindow.Update(song, vocalPane);
            UpdateActivations();
        }

        private void displayOnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (displayOnToolStripMenuItem.Checked)
            {
                displayOnToolStripMenuItem.Checked = false;
                vocalPane.display = false;
            }
            else if(song!=null && song.valid)
            {
                displayOnToolStripMenuItem.Checked = true;
                vocalPane.display = true;
            }
            consoleWindow.Update(song, vocalPane);
            vocalPane.Invalidate();
        }

        private void consoleOnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (consoleOnToolStripMenuItem.Checked)
            {
                consoleOnToolStripMenuItem.Checked = false;
                consoleWindow.Hide();
            }
            else
            {
                consoleOnToolStripMenuItem.Checked = true;
                consoleWindow.Show();
            }
            consoleWindow.Update(song, vocalPane);
        }

        public void UncheckConsole()
        {
            consoleOnToolStripMenuItem.Checked = false;
        }

        private void resTrackbar_Scroll(object sender, EventArgs e)
        {
            vocalPane.Refresh();
        }

        private void xScaleTrackBar_Scroll(object sender, EventArgs e)
        {
            vocalPane.Refresh();
        }

        private void controlModeAddRadio_CheckedChanged(object sender, EventArgs e)
        {
            UpdateActivations();
        }

        private void controlModeEditRadio_CheckedChanged(object sender, EventArgs e)
        {
            UpdateActivations();
        }

        private void controlModeRemoveRadio_CheckedChanged(object sender, EventArgs e)
        {
            UpdateActivations();
        }

        private void editingPhrasesRadio_CheckedChanged(object sender, EventArgs e)
        {
            UpdateActivations();
        }

        private void editingNodesRadio_CheckedChanged(object sender, EventArgs e)
        {
            UpdateActivations();
        }

        private void smallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            smallToolStripMenuItem.Checked = true;
            mediumToolStripMenuItem.Checked = false;
            largeToolStripMenuItem.Checked = false;
            hugeToolStripMenuItem.Checked = false;
            vocalPane.NodeSize = 4;
            consoleWindow.Update(song, vocalPane);
            vocalPane.Refresh();
        }

        private void mediumToolStripMenuItem_Click(object sender, EventArgs e)
        {
            smallToolStripMenuItem.Checked = false;
            mediumToolStripMenuItem.Checked = true;
            largeToolStripMenuItem.Checked = false;
            hugeToolStripMenuItem.Checked = false;
            vocalPane.NodeSize = 8;
            consoleWindow.Update(song, vocalPane);
            vocalPane.Refresh();
        }

        private void largeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            smallToolStripMenuItem.Checked = false;
            mediumToolStripMenuItem.Checked = false;
            largeToolStripMenuItem.Checked = true;
            hugeToolStripMenuItem.Checked = false;
            vocalPane.NodeSize = 12;
            consoleWindow.Update(song, vocalPane);
            vocalPane.Refresh();
        }

        private void hugeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            smallToolStripMenuItem.Checked = false;
            mediumToolStripMenuItem.Checked = false;
            largeToolStripMenuItem.Checked = false;
            hugeToolStripMenuItem.Checked = true;
            vocalPane.NodeSize = 16;
            consoleWindow.Update(song, vocalPane);
            vocalPane.Refresh();
        }

        private void previewScroll_Scroll(object sender, ScrollEventArgs e)
        {
            vocalPane.offset = (float)previewScroll.Value / previewScroll.Maximum;
        }
    }
}
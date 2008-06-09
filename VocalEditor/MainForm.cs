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

        public static String[] noteNames = {"C","Db","D","Eb","E","F","Gb","G","Ab","Bb","B","C","Db","D","Eb","E","F","Gb","G","Ab","A","Bb","B","C","Db","D","Eb","E","F","Gb","G","Ab","A","Bb","B"};
        public Stack<UndoCommand> undoList;
        public Stack<UndoCommand> redoList;
        public Stack<UndoCommand> undos
        {
            get { return undoList; }
        }
        public Stack<UndoCommand> redos
        {
            get { return redoList; }
        }
        public bool undoDisabled = false;

        public enum MODE
        {
            ADD=0,
            EDIT,
            REMOVE
        };
        public MODE Mode
        {
            get
            {
                if (controlModeAddRadio.Checked)
                    return MODE.ADD;
                else if (controlModeEditRadio.Checked)
                    return MODE.EDIT;
                else
                    return MODE.REMOVE;
            }
        }

        public const int RES_ONE = 0, RES_TWO = 1, RES_THREE = 2, RES_FOUR = 3, RES_SIX = 4, RES_EIGHT = 5, RES_TWELVE = 6, RES_SIXTEEN = 7, RES_THIRTYTWO = 8, RES_SIXTYFOUR = 9;

        public MainForm()
        {
            InitializeComponent();
            consoleWindow = new ConsoleInfo(this);
            vocalPane.SetParentForm(this);
            UpdateActivations();
            changed = false;
            undoList = new Stack<UndoCommand>();
            redoList = new Stack<UndoCommand>();
            t = new Timer();
            t.Tick += new EventHandler(Update);
            t.Interval = 40;
            t.Start();
        }

        public void Update(Object stateInfo, EventArgs e)
        {
            vocalPane.Invalidate();
        }

        public void UpdateActivations()
        {
            
            nodeTypeTalkyRadio.Enabled = false;
            nodeTypeVocalRadio.Enabled = false;
            nodeTextBox.Enabled = false;
            timeTextBox.Enabled = false;
            lengthTextBox.Enabled = false;
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
            noteTextBox.Enabled = false;
            vocalPane.Cursor = Cursors.Default;

            if (undos==null || undos.Count <= 0)
                undoToolStripMenuItem.Enabled = false;
            else
                undoToolStripMenuItem.Enabled = true;

            if (redos == null || redos.Count <= 0)
                redoToolStripMenuItem.Enabled = false;
            else
                redoToolStripMenuItem.Enabled = true;


            if(song!=null)
            if (song.valid)
            {
                resTrackbar.Enabled = true;
                xScaleTrackBar.Enabled = true;
                controlModeAddRadio.Enabled = true;
                controlModeEditRadio.Enabled = true;
                controlModeRemoveRadio.Enabled = true;
                editingNodesRadio.Enabled = true;
                //editingPhrasesRadio.Enabled = true;
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
                    if (vocalPane.SelectedNode >= 0)
                    {
                        //nodeTypeTalkyRadio.Enabled = true;
                        //nodeTypeVocalRadio.Enabled = true;
                        //nodeTextBox.Enabled = true;
                        //timeTextBox.Enabled = true;
                        //lengthTextBox.Enabled = true;
                        nodeConnectedCheckbox.Enabled = true;
                        VocalWord w = song.notes[vocalPane.SelectedNodePhrase].words[vocalPane.SelectedNode];
                        if (w.startNote < VocalPreviewer.numNotes)
                            nodeTypeVocalRadio.Checked = true;
                        else
                            nodeTypeTalkyRadio.Checked = true;
                        int timeint = 0, lenint = 0;
                        float timefloat = 0, lenfloat = 0;
                        for (int i = 1; i < song.bars.Length; i++)
                            if (w.time < song.bars[i].time)
                            {
                                float f = ((w.time - song.bars[i - 1].time) / (float)(song.bars[i].time - song.bars[i - 1].time));
                                timefloat = f;
                                f *= song.bars[i - 1].numBeats;
                                f *= 16;
                                f = ((int)(f + 0.5f)) / 16f;
                                timeTextBox.Text = "" + i + " : " + (f + 1);
                                timeint = i;
                                break;
                            }
                        for (int i = 1; i < song.bars.Length; i++)
                        {
                            if (w.connected)
                            {
                                if(song.notes[vocalPane.SelectedNodePhrase].words[vocalPane.SelectedNode + 1].time < song.bars[i].time)
                                {
                                    float f = (((w.connected ? song.notes[vocalPane.SelectedNodePhrase].words[vocalPane.SelectedNode + 1].time : w.len) - song.bars[i - 1].time) / (float)(song.bars[i].time - song.bars[i - 1].time));
                                    lenfloat = f;
                                    f *= song.bars[i - 1].numBeats;
                                    f *= 16;
                                    f = ((int)(f + 0.5f)) / 16f;
                                    lenint = i;
                                    break;
                                }
                            }
                            else
                            {
                                if (w.len < song.bars[i].time)
                                {
                                    float f = (((w.connected ? song.notes[vocalPane.SelectedNodePhrase].words[vocalPane.SelectedNode + 1].time : w.len) - song.bars[i - 1].time) / (float)(song.bars[i].time - song.bars[i - 1].time));
                                    lenfloat = f;
                                    f *= song.bars[i - 1].numBeats;
                                    f *= 16;
                                    f = ((int)(f + 0.5f)) / 16f;
                                    lenint = i;
                                    break;

                                }
                            }
                        }
                        float lenpos = 0;
                        if (lenint != timeint)
                        {
                            lenpos += (1 - timefloat) * song.bars[timeint].numBeats;
                            for (int i = timeint + 1; i < lenint; i++)
                            {
                                lenpos += song.bars[i].numBeats;
                            }
                            lenpos += lenfloat * song.bars[lenint].numBeats;
                        }
                        else
                        {
                            lenpos += (lenfloat - timefloat) * song.bars[lenint].numBeats;
                        }
                        lenpos *= 16;
                        lenpos = ((int)(lenpos + 0.5f)) / 16f;
                        lengthTextBox.Text = "" + lenpos;
                        noteTextBox.Text = noteNames[w.startNote];
                        nodeConnectedCheckbox.Checked = w.connected;
                        undoDisabled = true;
                        nodeTextBox.Text = w.value;
                        undoDisabled = false;
                    }
                    else
                    {
                        nodeTypeTalkyRadio.Checked = false;
                        nodeTypeVocalRadio.Checked = false;
                        undoDisabled = true;
                        nodeTextBox.Text = "";
                        undoDisabled = false;
                        timeTextBox.Text= "";
                        lengthTextBox.Text = "";
                        nodeConnectedCheckbox.Checked = false;
                        phraseTypeEmptyRadio.Checked = false;
                        phraseTypeRhythmRadio.Checked = false;
                        phraseTypeVocalsRadio.Checked = false;
                    }
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

        private void nodeConnectedCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (vocalPane.SelectedNode >= 0)
            {
                VocalWord w = song.notes[vocalPane.SelectedNodePhrase].words[vocalPane.SelectedNode];
                w.connected = nodeConnectedCheckbox.Checked;
                if (vocalPane.SelectedNode<song.notes[vocalPane.SelectedNodePhrase].words.Count)
                {
                    w.connected = false;
                    nodeConnectedCheckbox.Checked = false;
                }
                UpdateActivations();
            }
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            undoDisabled = true;
            if (undos.Count > 0)
            {
                UndoCommand redo = undos.Pop().Execute();
                redos.Push(redo);
                UpdateActivations();
            }
            undoDisabled = false;
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            undoDisabled = true;
            if (redos.Count > 0)
            {
                UndoCommand undo = redos.Pop().Execute();
                undos.Push(undo);
                UpdateActivations();
            }
            undoDisabled = false;
        }

        private String nodeTextBoxLastText="";
        private void nodeTextBox_TextChanged(object sender, EventArgs e)
        {
            if (!undoDisabled)
            {
                undos.Push(new UndoTextEdit(nodeTextBox, nodeTextBoxLastText));
                redos.Clear();
            }
            nodeTextBoxLastText = nodeTextBox.Text;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}
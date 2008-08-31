using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Unsigned
{
    public partial class SongAssembler : Form
    {
        List<Instrument> instruments;
        bool ProjectLoaded, ProjectSaved;

        List<TrackFile> leftInfo;
        TrackFile rightInfo;

        Track selectedTrack;

        public SongAssembler()
        {
            InitializeComponent();
            instruments = new List<Instrument>();
            leftInfo = null;
            rightInfo = null;
            selectedTrack = null;
            propertyGrid.SelectedObject = null;
            reloadToolStripMenuItem_Click(this, new EventArgs());
            ReloadProjectTree();
            ProjectLoaded = false;
            ProjectSaved = false;

            UpdateLoadMenus();
            UpdateTrackInfo(null);
        }

        public void ReloadProjectTree()
        {
            projectTree.Nodes.Clear();
            if (rightInfo == null)
                return;
            TreeNode mainNode = new TreeNode();
            mainNode.Text = rightInfo.Filename;
            mainNode.Name = "mainNode";
            TreeNode infoNode = new TreeNode();
            infoNode.Text = "Chart Info";
            infoNode.Name = "infoNode";
            mainNode.Nodes.Add(infoNode);
            for (int i = 0; i < rightInfo.tracks.Count; i++)
            {
                TreeNode trackNode = new TreeNode();
                trackNode.Name = "trackNode" + i;
                trackNode.Text = "Invalid Instrument Track";
                for (int k = 0; k < instruments.Count; k++)
                    if (rightInfo.tracks[i].InstrumentType.Equals(instruments[k].CodeName))
                        trackNode.Text = instruments[k].FullName + " Track";
                mainNode.Nodes.Add(trackNode);
            }
            projectTree.Nodes.Add(mainNode);
        }

        public void ReloadLoadedTree()
        {
            loadedTree.Nodes.Clear();
            if (rightInfo == null)
                return;
            for (int r = 0; r < leftInfo.Count; r++)
            {
                TreeNode mainNode = new TreeNode();
                mainNode.Text = leftInfo[r].Filename;
                mainNode.Name = "mainNode"+r;
                TreeNode infoNode = new TreeNode();
                infoNode.Text = "Chart Info";
                infoNode.Name = "infoNode";
                mainNode.Nodes.Add(infoNode);
                for (int i = 0; i < leftInfo[r].tracks.Count; i++)
                {
                    TreeNode trackNode = new TreeNode();
                    trackNode.Name = "trackNode" + i;
                    trackNode.Text = "Invalid Instrument Track";
                    for (int k = 0; k < instruments.Count; k++)
                        if (leftInfo[r].tracks[i].InstrumentType.Equals(instruments[k].CodeName))
                            trackNode.Text = instruments[k].FullName + " Track";
                    mainNode.Nodes.Add(trackNode);
                }
                loadedTree.Nodes.Add(mainNode);
            }
        }

        public void RefreshTrees()
        {

        }

        private void UpdateTrackInfo(Track track)
        {
            selectedTrack = track;
            if (track != null)
            {
                for (int i = 0; i < instruments.Count; i++)
                    if (instruments[i].CodeName.Equals(track.InstrumentType))
                        instrumentComboBox.SelectedIndex = i;
                bool check = false;
                for (int i = 0; i < track.diffSets[0].phrases.Count; i++)
                    if (track.diffSets[0].phrases[i].SongNotes.Count > 0)
                        check = true;
                easyCheckBox.Checked = check;
                check = false;
                for (int i = 0; i < track.diffSets[1].phrases.Count; i++)
                    if (track.diffSets[1].phrases[i].SongNotes.Count > 0)
                        check = true;
                mediumCheckBox.Checked = check;
                check = false;
                for (int i = 0; i < track.diffSets[2].phrases.Count; i++)
                    if (track.diffSets[2].phrases[i].SongNotes.Count > 0)
                        check = true;
                hardCheckBox.Checked = check;
                check = false;
                for (int i = 0; i < track.diffSets[3].phrases.Count; i++)
                    if (track.diffSets[3].phrases[i].SongNotes.Count > 0)
                        check = true;
                expertCheckBox.Checked = check;

                if (loadedTree.SelectedNode == null && projectTree.SelectedNode != null)
                    instrumentComboBox.Enabled = true;
                else
                    instrumentComboBox.Enabled = false;

            }
            else
            {
                easyCheckBox.Checked = false;
                mediumCheckBox.Checked = false;
                hardCheckBox.Checked = false;
                expertCheckBox.Checked = false;
                instrumentComboBox.SelectedIndex=-1;
                instrumentComboBox.Enabled = false;
            }

            if (loadedTree.SelectedNode == null || selectedTrack == null)
                copyButton.Enabled = false;
            else
                copyButton.Enabled = true;
            if (loadedTree.SelectedNode == null || selectedTrack == null)
                convertButton.Enabled = false;
            else
                convertButton.Enabled = true;
            if (projectTree.SelectedNode == null || selectedTrack == null)
                removeButton.Enabled = false;
            else
                removeButton.Enabled = true;
        }

        private void loadedTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            projectTree.SelectedNode = null;
            if (loadedTree.SelectedNode.Name.Length > 7 && loadedTree.SelectedNode.Name.Substring(0, 8).Equals("mainNode"))
            {
                int index = Int32.Parse(loadedTree.SelectedNode.Name.Substring(8).Trim());
                propertyGrid.SelectedObject = leftInfo[index];
            }
            else if (loadedTree.SelectedNode.Name.Length > 7 && loadedTree.SelectedNode.Name.Equals("infoNode"))
            {
                int index = Int32.Parse(loadedTree.SelectedNode.Parent.Name.Substring(8).Trim());
                propertyGrid.SelectedObject = leftInfo[index].info;
            }
            if (loadedTree.SelectedNode.Name.Length>8 && loadedTree.SelectedNode.Name.Substring(0, 9).Equals("trackNode"))
            {
                int index = Int32.Parse(loadedTree.SelectedNode.Parent.Name.Substring(8).Trim());
                propertyGrid.SelectedObject = leftInfo[index].tracks[Int32.Parse(loadedTree.SelectedNode.Name.Substring(9))];
                UpdateTrackInfo(leftInfo[index].tracks[Int32.Parse(loadedTree.SelectedNode.Name.Substring(9))]);
            }
            else
                UpdateTrackInfo(null);
        }

        private void projectTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            loadedTree.SelectedNode = null;
            if (projectTree.SelectedNode.Name.Length > 7 && projectTree.SelectedNode.Name.Equals("mainNode"))
                propertyGrid.SelectedObject = rightInfo;
            else if (projectTree.SelectedNode.Name.Length > 7 && projectTree.SelectedNode.Name.Equals("infoNode"))
                propertyGrid.SelectedObject = rightInfo.info;
            if (projectTree.SelectedNode.Name.Length>8 && projectTree.SelectedNode.Name.Substring(0, 9).Equals("trackNode"))
            {
                propertyGrid.SelectedObject = rightInfo.tracks[Int32.Parse(projectTree.SelectedNode.Name.Substring(9))];
                UpdateTrackInfo(rightInfo.tracks[Int32.Parse(projectTree.SelectedNode.Name.Substring(9))]);
            }
            else
                UpdateTrackInfo(null);
        }

        private void instrumentComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void loadInChartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.AddExtension = true;
            dialog.Filter = "Feedback Chart files (*.chart)|*.chart";
            dialog.InitialDirectory = System.IO.Directory.GetCurrentDirectory();
            DialogResult dr = dialog.ShowDialog();
            if (dr == DialogResult.OK)
            {
                TrackFile trfl = ChartHandler.LoadChart(dialog.FileName);
                leftInfo.Add(trfl);
                ReloadLoadedTree();
            }
        }

        private void reloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            instruments.Clear();
            instrumentComboBox.Items.Clear();
            String[] files = System.IO.Directory.GetFiles("instruments");
            for (int fileI = 0; fileI < files.Length; fileI++)
            {
                Instrument instr = new Instrument();
                System.IO.StreamReader fin = new System.IO.StreamReader(files[fileI]);
                while (!fin.EndOfStream)
                {
                    String str = fin.ReadLine();
                    instr.SetValue(str.Substring(0, str.IndexOf('=')).Trim(), str.Substring(str.IndexOf('=') + 1).Trim());
                }
                instruments.Add(instr);
                instrumentComboBox.Items.Add(instr.FullName);
            }
        }

        private void UpdateLoadMenus()
        {
            if (!ProjectLoaded)
                loadInChartToolStripMenuItem.Enabled = false;
            else
                loadInChartToolStripMenuItem.Enabled = true;
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            rightInfo = new TrackFile();
            leftInfo = new List<TrackFile>();
            ProjectLoaded = true;
            ProjectSaved = true;
            UpdateLoadMenus();
            ReloadProjectTree();
            propertyGrid.SelectedObject = null;
        }

        private void copyButton_Click(object sender, EventArgs e)
        {
            if (selectedTrack != null && loadedTree.SelectedNode != null)
            {
                rightInfo.tracks.Add(selectedTrack);
                ReloadProjectTree();
            }
        }

        private void removeButton_Click(object sender, EventArgs e)
        {
            if (selectedTrack != null && projectTree.SelectedNode != null)
            {
                rightInfo.tracks.Remove(selectedTrack);
                ReloadProjectTree();
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SongDataIO;

namespace SongdataConverter
{
    public partial class InstrumentDataViewer : UserControl
    {
        public enum InstrumentViewType
        {
            Guitar,
            Drums,
            Vocals,
        }

        public InstrumentViewType ViewType;
        private Instrument _instrType;
        public Instrument InstrumentType { get { return _instrType; } set { _instrType = value; notesViewerControl1.InstrumentType = value; } }

        private static int[] StepNumerators =   {  1,  1,  1, 1, 1, 1, 1, };
        private static int[] StepDenomerators = { 64, 32, 16, 8, 4, 2, 1, };

        private SongData _songData = null;
        public SongData SongData
        {
            get { return _songData; }
            set { _songData = value; RefreshAllInfo(); }
        }

        public InstrumentDataViewer()
        {
            InitializeComponent();
            ChangeDifficulty();
            trackBar1_Scroll(this, new EventArgs());
        }

        private void RefreshAllInfo()
        {
            notesViewerControl1.SongData = SongData;
            if (SongData != null)
            {
                hScrollBar1.Maximum = (int)(SongData.info.barlines[SongData.info.barlines.Length - 1].time * notesViewerControl1.xScale);
            }
        }

        private void difficultyTrackBar_Scroll(object sender, EventArgs e)
        {
            difficultyNumeric.Value = difficultyTrackBar.Value;
        }

        private void difficultyNumeric_ValueChanged(object sender, EventArgs e)
        {
            difficultyTrackBar.Value = (int)difficultyNumeric.Value;
        }

        public void ApplyChanges()
        {
            if (SongData != null)
            {
                for (int inst = 0; inst < SongData.instruments.Length; inst++)
                {
                    if (SongData.instruments[inst].instrumentType == InstrumentType.CodeName)
                    {
                        SongData.instruments[inst].difficulty = (byte)difficultyNumeric.Value;
                    }
                }
                notesViewerControl1.CheckValidity();
            }
        }

        private void easyRadio_CheckedChanged(object sender, EventArgs e)
        {
            ChangeDifficulty();
        }

        private void mediumRadio_CheckedChanged(object sender, EventArgs e)
        {
            ChangeDifficulty();
        }

        private void hardRadio_CheckedChanged(object sender, EventArgs e)
        {
            ChangeDifficulty();
        }

        private void expertRadio_CheckedChanged(object sender, EventArgs e)
        {
            ChangeDifficulty();
        }

        private void ChangeDifficulty()
        {
            int diff = -1;
            if (easyRadio.Checked)
                diff = 0;
            if (mediumRadio.Checked)
                diff = 1;
            if (hardRadio.Checked)
                diff = 2;
            if (expertRadio.Checked)
                diff = 3;
            notesViewerControl1.Difficulty = diff;
            notesViewerControl1.Invalidate();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            stepLabel.Text = "Step: " + StepNumerators[trackBar1.Value] + "/" + StepDenomerators[trackBar1.Value];
            notesViewerControl1.StepDenominator = StepDenomerators[trackBar1.Value];
        }

        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            notesViewerControl1.TextXOffset = -hScrollBar1.Value;
            notesViewerControl1.Invalidate();
        }
    }
}

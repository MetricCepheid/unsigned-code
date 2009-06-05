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
    public partial class RoadieDataViewer : UserControl
    {
        public enum InstrumentViewType
        {
            Guitar,
            Drums,
            Vocals,
        }

        public InstrumentViewType ViewType;

        private static int[] StepNumerators =   {  1,  1,  1, 1, 1, 1, 1, };
        private static int[] StepDenomerators = { 64, 32, 16, 8, 4, 2, 1, };

        private SongData _songData = null;
        public SongData SongData
        {
            get { return _songData; }
            set { _songData = value; RefreshAllInfo(); }
        }

        public RoadieDataViewer()
        {
            InitializeComponent();
            trackBar1_Scroll(this, new EventArgs());
        }

        private void RefreshAllInfo()
        {
            roadieViewerControl1.SongData = SongData;
            if (SongData != null)
            {
                hScrollBar1.Maximum = (int)(SongData.info.barlines[SongData.info.barlines.Length - 1].time * roadieViewerControl1.xScale);
            }
        }

        public void ApplyChanges()
        {
            if (SongData != null)
            {
                roadieViewerControl1.CheckValidity();
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            stepLabel.Text = "Step: " + StepNumerators[trackBar1.Value] + "/" + StepDenomerators[trackBar1.Value];
            roadieViewerControl1.StepDenominator = StepDenomerators[trackBar1.Value];
        }

        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            roadieViewerControl1.TextXOffset = -hScrollBar1.Value;
            roadieViewerControl1.Invalidate();
        }
    }
}

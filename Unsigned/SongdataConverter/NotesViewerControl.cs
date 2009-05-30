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
    public partial class NotesViewerControl : UserControl
    {
        private enum SelectionPart
        {
            None = 0,
            Beginning,
            Center,
            End,
        }

        private SongData _songData = null;
        public SongData SongData
        {
            get { return _songData; }
            set { _songData = value; RefreshAllInfo(); }
        }
        private Instrument _instrType;
        public Instrument InstrumentType { get { return _instrType; } set { _instrType = value; GenerateInstrInfo(); } }

        private static Brush[] colors = { Brushes.Green, Brushes.Red, Brushes.Yellow, Brushes.Blue, Brushes.Orange };

        private int selectedLane, selectedIndex, selectedOriginalX;
        private SelectionPart selectedPart;

        public int StepDenominator;

        public int numLanes;
        public float xScale;

        public int TextXOffset;

        public int Difficulty;

        public NotesViewerControl()
        {
            InitializeComponent();
            xScale = 0.1f;
            selectedLane = -1;
        }

        public void RefreshAllInfo()
        {
            if (SongData != null)
            {
                //Bounds = new Rectangle(Bounds.X, Bounds.Y, (int)(SongData.info.barlines[SongData.info.barlines.Length - 1].time * xScale), Bounds.Height);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(Color.Black);
            if (SongData != null && InstrumentType != null)
            {
                //special lane color divideby amount
                int divideby = 4;
                {
                    int y = (int)(Bounds.Height * (InstrumentType.NumDrawnTracks / (float)(numLanes)));
                    int h = (int)(Bounds.Height * (1 / (float)(numLanes)));
                    Color srcCol = Color.Blue;
                    Color b = Color.FromArgb(srcCol.R / divideby, srcCol.G / divideby, srcCol.B / divideby);
                    e.Graphics.FillRectangle(new SolidBrush(b), new Rectangle(0, y, Bounds.Width, h));
                }
                {
                    int y = (int)(Bounds.Height * ((InstrumentType.NumDrawnTracks + 1) / (float)(numLanes)));
                    int h = (int)(Bounds.Height * (1 / (float)(numLanes)));
                    Color srcCol = Color.Red;
                    Color b = Color.FromArgb(srcCol.R / divideby, srcCol.G / divideby, srcCol.B / divideby);
                    e.Graphics.FillRectangle(new SolidBrush(b), new Rectangle(0, y, Bounds.Width, h));
                }
                {
                    int y = (int)(Bounds.Height * ((InstrumentType.NumDrawnTracks + 2) / (float)(numLanes)));
                    int h = (int)(Bounds.Height * (1 / (float)(numLanes)));
                    Color srcCol = Color.Yellow;
                    Color b = Color.FromArgb(srcCol.R / divideby, srcCol.G / divideby, srcCol.B / divideby);
                    e.Graphics.FillRectangle(new SolidBrush(b), new Rectangle(0, y, Bounds.Width, h));
                }
                {
                    int y = (int)(Bounds.Height * ((InstrumentType.NumDrawnTracks + 3) / (float)(numLanes)));
                    int h = (int)(Bounds.Height * (1 / (float)(numLanes)));
                    Color srcCol = Color.Green;
                    Color b = Color.FromArgb(srcCol.R / divideby, srcCol.G / divideby, srcCol.B / divideby);
                    e.Graphics.FillRectangle(new SolidBrush(b), new Rectangle(0, y, Bounds.Width, h));
                }
                {
                    int y = (int)(Bounds.Height * ((InstrumentType.NumDrawnTracks + 4) / (float)(numLanes)));
                    int h = (int)(Bounds.Height * (1 / (float)(numLanes)));
                    Color srcCol = Color.FromArgb(0, 255, 255);
                    Color b = Color.FromArgb(srcCol.R / divideby, srcCol.G / divideby, srcCol.B / divideby);
                    e.Graphics.FillRectangle(new SolidBrush(b), new Rectangle(0, y, Bounds.Width, h));
                }
                for (int instr = 0; instr < SongData.instruments.Length; instr++)
                    if (SongData.instruments[instr].instrumentType == InstrumentType.CodeName)
                    {
                        for (int i = 0; i < SongData.instruments[instr].rpPhrases.Length; i++)
                        {
                            int x = (int)(SongData.instruments[instr].rpPhrases[i].time * xScale) + TextXOffset;
                            int w = (int)(SongData.instruments[instr].rpPhrases[i].len * xScale);
                            int y = (int)(Height * ((InstrumentType.NumDrawnTracks + 4) / (float)(numLanes)));
                            int h = (int)(Height * (1 / (float)(numLanes)));
                            e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(0, 255, 255)), new Rectangle(x, y, w, h));
                        }
                        if(SongData.instruments[instr].fills!=null)
                        for (int i = 0; i < SongData.instruments[instr].fills.Length; i++)
                        {
                            int x = (int)(SongData.instruments[instr].fills[i].time * xScale) + TextXOffset;
                            int w = (int)(SongData.instruments[instr].fills[i].len * xScale);
                            int y = (int)(Height * ((InstrumentType.NumDrawnTracks + 3) / (float)(numLanes)));
                            int h = (int)(Height * (1 / (float)(numLanes)));
                            e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(0, 255, 0)), new Rectangle(x, y, w, h));
                        }
                        if (SongData.instruments[instr].solos != null)
                            for (int i = 0; i < SongData.instruments[instr].solos.Length; i++)
                            {
                                int x = (int)(SongData.instruments[instr].solos[i].time * xScale) + TextXOffset;
                                int w = (int)(SongData.instruments[instr].solos[i].len * xScale);
                                int y = (int)(Height * ((InstrumentType.NumDrawnTracks) / (float)(numLanes)));
                                int h = (int)(Height * (1 / (float)(numLanes)));
                                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(0, 0, 255)), new Rectangle(x, y, w, h));
                            }
                        for (int p = 0; p < SongData.instruments[instr].diffSets[Difficulty].phrases.Length; p++)
                        {
                            for (int n = 0; n < SongData.instruments[instr].diffSets[Difficulty].phrases[p].notes.Length; n++)
                            {
                                SongData.NoteSet note = SongData.instruments[instr].diffSets[Difficulty].phrases[p].notes[n];
                                for (int i = 0; i < InstrumentType.NumDrawnTracks; i++)
                                {
                                    if ((note.type & ((ulong)1 << i)) != 0)
                                    {
                                        int R = ((SolidBrush)colors[InstrumentType.colorIndices[i]]).Color.R / 2;
                                        int G = ((SolidBrush)colors[InstrumentType.colorIndices[i]]).Color.G / 2;
                                        int B = ((SolidBrush)colors[InstrumentType.colorIndices[i]]).Color.B / 2;
                                        SolidBrush b = new SolidBrush(Color.FromArgb(R, G, B));
                                        int x = (int)(note.time * xScale) + TextXOffset;
                                        int w = (int)(note.length * xScale);
                                        int h = (int)(Bounds.Height * (1 / (float)(numLanes)));
                                        Rectangle rect = new Rectangle(x, (int)(Bounds.Height * (i / (float)(numLanes))) + (int)(h * 0.3f), w, (int)(h * 0.4f));
                                        e.Graphics.FillRectangle(b, rect);
                                    }
                                }
                            }
                        }
                    }
                e.Graphics.DrawString("Solos:", new Font(FontFamily.GenericSansSerif, 14), Brushes.White, new PointF(10, Bounds.Height * ((InstrumentType.NumDrawnTracks) / (float)numLanes)));
                e.Graphics.DrawString("PS1:", new Font(FontFamily.GenericSansSerif, 14), Brushes.White, new PointF(10, Bounds.Height * ((InstrumentType.NumDrawnTracks + 1) / (float)numLanes)));
                e.Graphics.DrawString("PS2:", new Font(FontFamily.GenericSansSerif, 14), Brushes.White, new PointF(10, Bounds.Height * ((InstrumentType.NumDrawnTracks + 2) / (float)numLanes)));
                e.Graphics.DrawString("Fills:", new Font(FontFamily.GenericSansSerif, 14), Brushes.White, new PointF(10, Bounds.Height * ((InstrumentType.NumDrawnTracks + 3) / (float)numLanes)));
                e.Graphics.DrawString("RP:", new Font(FontFamily.GenericSansSerif, 14), Brushes.White, new PointF(10, Bounds.Height * ((InstrumentType.NumDrawnTracks + 4) / (float)numLanes)));

                for (int i = 0; i < SongData.info.barlines.Length - 1; i++)
                {
                    e.Graphics.FillRectangle(Brushes.White, new Rectangle((int)(SongData.info.barlines[i].time * xScale) - 1 + TextXOffset, 0, 3, Bounds.Height));
                    for (int k = 1; k < SongData.info.barlines[i].numBeats; k++)
                    {
                        int x = (int)(((SongData.info.barlines[i + 1].time - SongData.info.barlines[i].time) * xScale) * (k / (float)SongData.info.barlines[i].numBeats)) + (int)(SongData.info.barlines[i].time * xScale) + TextXOffset;
                        e.Graphics.FillRectangle(Brushes.White, new Rectangle(x, 0, 1, Bounds.Height));
                    }
                }
                for (int i = 0; i <= numLanes; i++)
                {
                    int h1 = 0;
                    int h2 = 1;
                    if (i == 0 || i == InstrumentType.NumDrawnTracks)
                    {
                        h1 = 1;
                        h2 = 3;
                    }
                    int y = (int)(Bounds.Height * (i / (float)(numLanes)));
                    e.Graphics.FillRectangle(Brushes.White, new Rectangle(0, y - h1, Bounds.Width, h2));
                }
                for (int instr = 0; instr < SongData.instruments.Length; instr++)
                    if (SongData.instruments[instr].instrumentType == InstrumentType.CodeName)
                    {
                        for (int p = 0; p < SongData.instruments[instr].diffSets[Difficulty].phrases.Length; p++)
                        {
                            for (int n = 0; n < SongData.instruments[instr].diffSets[Difficulty].phrases[p].notes.Length; n++)
                            {
                                SongData.NoteSet note = SongData.instruments[instr].diffSets[Difficulty].phrases[p].notes[n];

                                for (int i = InstrumentType.NumDrawnTracks; i < InstrumentType.NumTracks; i++)
                                {
                                    if ((note.type & ((ulong)1 << i)) != 0)
                                    {
                                        int x = (int)(note.time * xScale) + TextXOffset;
                                        Rectangle rect = new Rectangle(x - 2, 0, 4, (int)(Bounds.Height * (InstrumentType.NumDrawnTracks / (float)(numLanes))));
                                        e.Graphics.FillRectangle(colors[InstrumentType.colorIndices[i]], rect);
                                    }
                                }
                                for (int i = 0; i < InstrumentType.NumDrawnTracks; i++)
                                {
                                    if ((note.type & ((ulong)1 << i)) != 0)
                                    {
                                        int x = (int)(note.time * xScale) + TextXOffset;
                                        Rectangle rect = new Rectangle(x - 5, (int)(Bounds.Height * (i / (float)(numLanes))), 10, (int)(Bounds.Height * (1 / (float)(numLanes))));
                                        e.Graphics.FillRectangle(colors[InstrumentType.colorIndices[i]], rect);
                                        if (InstrumentType.CanHOPO && (note.type & ((ulong)1 << InstrumentType.NumTracks)) != 0)
                                            e.Graphics.DrawRectangle(Pens.White, new Rectangle(rect.X + 2, rect.Y + 2, rect.Width - 5, rect.Height - 5));
                                        else
                                            e.Graphics.DrawRectangle(Pens.Black, new Rectangle(rect.X + 2, rect.Y + 2, rect.Width - 5, rect.Height - 5));
                                    }
                                }
                            }
                        }
                    }
            }
            if (!this.Focused)
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(128,0,0,0)), new Rectangle(0,0,Bounds.Width,Bounds.Height));
            }
            //base.OnPaint(e);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            //base.OnPaintBackground(e);
        }

        private void GenerateInstrInfo()
        {
            if (InstrumentType != null)
            {
                numLanes = InstrumentType.NumDrawnTracks + 5;
            }
        }

        private void NotesViewerControl_MouseLeave(object sender, EventArgs e)
        {
            Cursor = Cursors.Default;
            selectedLane = -1;
        }

        private void NotesViewerControl_MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = new Point(e.X-TextXOffset, e.Y);
            int h = (int)(Bounds.Height * (InstrumentType.NumDrawnTracks / (float)(numLanes)));
            if (mousePos.Y >= h)
            {
                if (selectedLane >= 0)
                {
                    for (int i = InstrumentType.NumDrawnTracks; i < numLanes; i++)
                    {
                        int y1 = (int)(Bounds.Height * (i / (float)(numLanes)));
                        int y2 = (int)(Bounds.Height * ((i + 1) / (float)(numLanes)));
                        if (mousePos.Y >= y1 && mousePos.Y < y2)
                        {
                            if (i - InstrumentType.NumDrawnTracks == selectedLane)
                            {
                                switch (selectedLane)
                                {
                                    case 0:
                                        if (!InstrumentType.HasSolos)
                                            break;
                                        for (int instr = 0; instr < SongData.instruments.Length; instr++)
                                        {
                                            if (SongData.instruments[instr].instrumentType == InstrumentType.CodeName)
                                            {
                                                if (selectedPart == SelectionPart.Beginning)
                                                {
                                                    uint end = SongData.instruments[instr].solos[selectedIndex].time + SongData.instruments[instr].solos[selectedIndex].len;
                                                    uint time = (uint)GetNearestTimeStepFromX(mousePos.X);
                                                    int len = (int)((long)(end) - (long)(time));
                                                    if (len > 10)
                                                    {
                                                        SongData.instruments[instr].solos[selectedIndex].time = time;
                                                        SongData.instruments[instr].solos[selectedIndex].len = (uint)len;
                                                    }
                                                }
                                                else if (selectedPart == SelectionPart.End)
                                                {
                                                    int len = (int)(GetNearestTimeStepFromX(mousePos.X) - SongData.instruments[instr].solos[selectedIndex].time);
                                                    if (len > 10)
                                                    {
                                                        SongData.instruments[instr].solos[selectedIndex].len = (uint)len;
                                                    }
                                                }
                                                else if (selectedPart == SelectionPart.Center)
                                                {
                                                    long t1 = GetNearestTimeStepFromX(selectedOriginalX);
                                                    long t2 = GetNearestTimeStepFromX(mousePos.X);
                                                    if (t1 != t2)
                                                    {
                                                        SongData.instruments[instr].solos[selectedIndex].time = (uint)(SongData.instruments[instr].solos[selectedIndex].time + (t2 - t1));
                                                        selectedOriginalX = mousePos.X;
                                                    }
                                                }
                                                Invalidate();
                                            }
                                        }
                                        break;
                                    case 3:
                                        if ((InstrumentType.RPEnableType & Instrument.RockPowerEnableTypes.FILL) == 0)
                                            break;
                                        for (int instr = 0; instr < SongData.instruments.Length; instr++)
                                        {
                                            if (SongData.instruments[instr].instrumentType == InstrumentType.CodeName)
                                            {
                                                if (selectedPart == SelectionPart.Beginning)
                                                {
                                                    uint end = SongData.instruments[instr].fills[selectedIndex].time + SongData.instruments[instr].fills[selectedIndex].len;
                                                    uint time = (uint)GetNearestTimeStepFromX(mousePos.X);
                                                    int len = (int)((long)(end) - (long)(time));
                                                    if (len > 10)
                                                    {
                                                        SongData.instruments[instr].fills[selectedIndex].time = time;
                                                        SongData.instruments[instr].fills[selectedIndex].len = (uint)len;
                                                    }
                                                }
                                                else if (selectedPart == SelectionPart.End)
                                                {
                                                    int len = (int)(GetNearestTimeStepFromX(mousePos.X) - SongData.instruments[instr].fills[selectedIndex].time);
                                                    if (len > 10)
                                                    {
                                                        SongData.instruments[instr].fills[selectedIndex].len = (uint)len;
                                                    }
                                                }
                                                else if (selectedPart == SelectionPart.Center)
                                                {
                                                    long t1 = GetNearestTimeStepFromX(selectedOriginalX);
                                                    long t2 = GetNearestTimeStepFromX(mousePos.X);
                                                    if (t1 != t2)
                                                    {
                                                        SongData.instruments[instr].fills[selectedIndex].time = (uint)(SongData.instruments[instr].fills[selectedIndex].time + (t2 - t1));
                                                        selectedOriginalX = mousePos.X;
                                                    }
                                                }
                                                Invalidate();
                                            }
                                        }
                                        break;
                                    case 4:
                                        for (int instr = 0; instr < SongData.instruments.Length; instr++)
                                        {
                                            if (SongData.instruments[instr].instrumentType == InstrumentType.CodeName)
                                            {
                                                if (selectedPart == SelectionPart.Beginning)
                                                {
                                                    uint end = SongData.instruments[instr].rpPhrases[selectedIndex].time + SongData.instruments[instr].rpPhrases[selectedIndex].len;
                                                    uint time = (uint)GetNearestTimeStepFromX(mousePos.X);
                                                    int len = (int)((long)(end) - (long)(time));
                                                    if (len > 10)
                                                    {
                                                        SongData.instruments[instr].rpPhrases[selectedIndex].time = time;
                                                        SongData.instruments[instr].rpPhrases[selectedIndex].len = (uint)len;
                                                    }
                                                }
                                                else if (selectedPart == SelectionPart.End)
                                                {
                                                    int len = (int)(GetNearestTimeStepFromX(mousePos.X) - SongData.instruments[instr].rpPhrases[selectedIndex].time);
                                                    if (len > 10)
                                                    {
                                                        SongData.instruments[instr].rpPhrases[selectedIndex].len = (uint)len;
                                                    }
                                                }
                                                else if (selectedPart == SelectionPart.Center)
                                                {
                                                    long t1 = GetNearestTimeStepFromX(selectedOriginalX);
                                                    long t2 = GetNearestTimeStepFromX(mousePos.X);
                                                    if (t1 != t2)
                                                    {
                                                        SongData.instruments[instr].rpPhrases[selectedIndex].time = (uint)(SongData.instruments[instr].rpPhrases[selectedIndex].time+(t2 - t1));
                                                        selectedOriginalX = mousePos.X;
                                                    }
                                                }
                                                Invalidate();
                                            }
                                        }
                                        break;
                                }
                            }
                            else
                            {
                                selectedLane = -1;
                                selectedIndex = -1;
                                selectedPart = SelectionPart.None;
                            }
                        }
                    }
                }
                else
                {
                    Cursor = Cursors.Default;
                    for (int i = InstrumentType.NumDrawnTracks; i < numLanes; i++)
                    {
                        int y1 = (int)(Bounds.Height * (i / (float)(numLanes)));
                        int y2 = (int)(Bounds.Height * ((i + 1) / (float)(numLanes)));
                        if (mousePos.Y >= y1 && mousePos.Y < y2)
                        {
                            switch (i - InstrumentType.NumDrawnTracks)
                            {
                                case 0:
                                    if (!InstrumentType.HasSolos)
                                        break;
                                    for (int instr = 0; instr < SongData.instruments.Length; instr++)
                                    {
                                        if (SongData.instruments[instr].instrumentType == InstrumentType.CodeName)
                                            for (int rp = 0; rp < SongData.instruments[instr].solos.Length; rp++)
                                            {
                                                int x1 = (int)(SongData.instruments[instr].solos[rp].time * xScale);
                                                int x2 = (int)((SongData.instruments[instr].solos[rp].time + SongData.instruments[instr].solos[rp].len) * xScale);
                                                if (Form1.Ctrl)
                                                {
                                                    if (mousePos.X >= x1 && mousePos.X <= x2)
                                                        Cursor = Form1.DeleteCursor;
                                                }
                                                else
                                                {
                                                    if (Math.Abs(mousePos.X - x1) < 10)
                                                        Cursor = Cursors.SizeWE;
                                                    else if (Math.Abs(mousePos.X - x2) < 10)
                                                        Cursor = Cursors.SizeWE;
                                                    else if (mousePos.X >= x1 && mousePos.X <= x2)
                                                        Cursor = Cursors.SizeAll;
                                                }
                                            }
                                    }
                                    if (Cursor == Cursors.Default)
                                        Cursor = Cursors.Cross;
                                    break;
                                case 3:
                                    if ((InstrumentType.RPEnableType & Instrument.RockPowerEnableTypes.FILL) == 0)
                                        break;
                                    for (int instr = 0; instr < SongData.instruments.Length; instr++)
                                    {
                                        if (SongData.instruments[instr].instrumentType == InstrumentType.CodeName)
                                            for (int rp = 0; rp < SongData.instruments[instr].fills.Length; rp++)
                                            {
                                                int x1 = (int)(SongData.instruments[instr].fills[rp].time * xScale);
                                                int x2 = (int)((SongData.instruments[instr].fills[rp].time + SongData.instruments[instr].fills[rp].len) * xScale);
                                                if (Form1.Ctrl)
                                                {
                                                    if (mousePos.X >= x1 && mousePos.X <= x2)
                                                        Cursor = Form1.DeleteCursor;
                                                }
                                                else
                                                {
                                                    if (Math.Abs(mousePos.X - x1) < 10)
                                                        Cursor = Cursors.SizeWE;
                                                    else if (Math.Abs(mousePos.X - x2) < 10)
                                                        Cursor = Cursors.SizeWE;
                                                    else if (mousePos.X >= x1 && mousePos.X <= x2)
                                                        Cursor = Cursors.SizeAll;
                                                }
                                            }
                                    }
                                    if (Cursor == Cursors.Default)
                                        Cursor = Cursors.Cross;
                                    break;
                                case 4:
                                    for (int instr = 0; instr < SongData.instruments.Length; instr++)
                                    {
                                        if (SongData.instruments[instr].instrumentType == InstrumentType.CodeName)
                                            for (int rp = 0; rp < SongData.instruments[instr].rpPhrases.Length; rp++)
                                            {
                                                int x1 = (int)(SongData.instruments[instr].rpPhrases[rp].time * xScale);
                                                int x2 = (int)((SongData.instruments[instr].rpPhrases[rp].time + SongData.instruments[instr].rpPhrases[rp].len) * xScale);
                                                if (Form1.Ctrl)
                                                {
                                                    if (mousePos.X >= x1 && mousePos.X <= x2)
                                                        Cursor = Form1.DeleteCursor;
                                                }
                                                else
                                                {
                                                    if (Math.Abs(mousePos.X - x1) < 10)
                                                        Cursor = Cursors.SizeWE;
                                                    else if (Math.Abs(mousePos.X - x2) < 10)
                                                        Cursor = Cursors.SizeWE;
                                                    else if (mousePos.X >= x1 && mousePos.X <= x2)
                                                        Cursor = Cursors.SizeAll;
                                                }
                                            }
                                    }
                                    if (Cursor == Cursors.Default)
                                        Cursor = Cursors.Cross;
                                    break;
                            }
                        }
                    }
                }
            }
            else
                Cursor = Cursors.Default;
        }

        private void NotesViewerControl_MouseDown(object sender, MouseEventArgs e)
        {
            if (selectedLane >= 0)
                return;
            Point mousePos = new Point(e.X-TextXOffset, e.Y);
            int h = (int)(Bounds.Height * (InstrumentType.NumDrawnTracks / (float)(numLanes)));
            if (mousePos.Y >= h)
            {
                for (int i = InstrumentType.NumDrawnTracks; i < numLanes; i++)
                {
                    int y1 = (int)(Bounds.Height * (i / (float)(numLanes)));
                    int y2 = (int)(Bounds.Height * ((i + 1) / (float)(numLanes)));
                    if (mousePos.Y >= y1 && mousePos.Y < y2)
                    {
                        switch (i - InstrumentType.NumDrawnTracks)
                        {
                            case 0:
                                if (!InstrumentType.HasSolos)
                                    break;
                                for (int instr = 0; instr < SongData.instruments.Length; instr++)
                                {
                                    if (SongData.instruments[instr].instrumentType == InstrumentType.CodeName)
                                    {
                                        for (int rp = 0; rp < SongData.instruments[instr].solos.Length; rp++)
                                        {
                                            int x1 = (int)(SongData.instruments[instr].solos[rp].time * xScale);
                                            int x2 = (int)((SongData.instruments[instr].solos[rp].time + SongData.instruments[instr].solos[rp].len) * xScale);
                                            if (e.Button == MouseButtons.Left)
                                            {
                                                if (Form1.Ctrl)
                                                {
                                                    if (mousePos.X >= x1 && mousePos.X <= x2)
                                                    {
                                                        ConfirmDialog d = new ConfirmDialog();
                                                        d.Title = "Delete Rock Power Section?";
                                                        d.Question = "Are you sure you want to delete this Rock Power Section?";
                                                        DialogResult res = d.ShowDialog();
                                                        if (res == DialogResult.Yes)
                                                        {
                                                            List<SongData.Solo> rpp = new List<SongData.Solo>();
                                                            for (int p = 0; p < SongData.instruments[instr].solos.Length; p++)
                                                            {
                                                                if (p != rp)
                                                                    rpp.Add(SongData.instruments[instr].solos[p]);
                                                            }
                                                            SongData.instruments[instr].solos = rpp.ToArray();
                                                            Invalidate();
                                                            return;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (Math.Abs(mousePos.X - x1) < 10)
                                                    {
                                                        selectedLane = i - InstrumentType.NumDrawnTracks;
                                                        selectedIndex = rp;
                                                        selectedPart = SelectionPart.Beginning;
                                                        return;
                                                    }
                                                    else if (Math.Abs(mousePos.X - x2) < 10)
                                                    {
                                                        selectedLane = i - InstrumentType.NumDrawnTracks;
                                                        selectedIndex = rp;
                                                        selectedPart = SelectionPart.End;
                                                        return;
                                                    }
                                                    else if (mousePos.X >= x1 && mousePos.X < x2)
                                                    {
                                                        selectedLane = i - InstrumentType.NumDrawnTracks;
                                                        selectedIndex = rp;
                                                        selectedPart = SelectionPart.Center;
                                                        selectedOriginalX = mousePos.X;
                                                        return;
                                                    }
                                                }
                                            }
                                        }
                                        if (selectedLane < 0)
                                        {
                                            List<SongData.Solo> rpp = new List<SongData.Solo>();
                                            long newStartTime = GetNearestTimeStepFromX(mousePos.X);
                                            int p = 0;
                                            for (; p < SongData.instruments[instr].solos.Length; p++)
                                            {
                                                if (SongData.instruments[instr].solos[p].time < newStartTime)
                                                    rpp.Add(SongData.instruments[instr].solos[p]);
                                                else
                                                    break;
                                            }
                                            selectedIndex = rpp.Count;
                                            rpp.Add(new SongData.Solo((uint)newStartTime, 10000));
                                            for (; p < SongData.instruments[instr].solos.Length; p++)
                                            {
                                                rpp.Add(SongData.instruments[instr].solos[p]);
                                            }
                                            SongData.instruments[instr].solos = rpp.ToArray();
                                            selectedLane = i - InstrumentType.NumDrawnTracks;
                                            selectedPart = SelectionPart.End;
                                            return;
                                        }
                                    }
                                }
                                break;
                            case 3:
                                if ((InstrumentType.RPEnableType & Instrument.RockPowerEnableTypes.FILL) == 0)
                                    break;
                                for (int instr = 0; instr < SongData.instruments.Length; instr++)
                                {
                                    if (SongData.instruments[instr].instrumentType == InstrumentType.CodeName)
                                    {
                                        for (int rp = 0; rp < SongData.instruments[instr].fills.Length; rp++)
                                        {
                                            int x1 = (int)(SongData.instruments[instr].fills[rp].time * xScale);
                                            int x2 = (int)((SongData.instruments[instr].fills[rp].time + SongData.instruments[instr].fills[rp].len) * xScale);
                                            if (e.Button == MouseButtons.Left)
                                            {
                                                if (Form1.Ctrl)
                                                {
                                                    if (mousePos.X >= x1 && mousePos.X <= x2)
                                                    {
                                                        ConfirmDialog d = new ConfirmDialog();
                                                        d.Title = "Delete Rock Power Section?";
                                                        d.Question = "Are you sure you want to delete this Rock Power Section?";
                                                        DialogResult res = d.ShowDialog();
                                                        if (res == DialogResult.Yes)
                                                        {
                                                            List<SongData.Fill> rpp = new List<SongData.Fill>();
                                                            for (int p = 0; p < SongData.instruments[instr].fills.Length; p++)
                                                            {
                                                                if (p != rp)
                                                                    rpp.Add(SongData.instruments[instr].fills[p]);
                                                            }
                                                            SongData.instruments[instr].fills = rpp.ToArray();
                                                            Invalidate();
                                                            return;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (Math.Abs(mousePos.X - x1) < 10)
                                                    {
                                                        selectedLane = i - InstrumentType.NumDrawnTracks;
                                                        selectedIndex = rp;
                                                        selectedPart = SelectionPart.Beginning;
                                                        return;
                                                    }
                                                    else if (Math.Abs(mousePos.X - x2) < 10)
                                                    {
                                                        selectedLane = i - InstrumentType.NumDrawnTracks;
                                                        selectedIndex = rp;
                                                        selectedPart = SelectionPart.End;
                                                        return;
                                                    }
                                                    else if (mousePos.X >= x1 && mousePos.X < x2)
                                                    {
                                                        selectedLane = i - InstrumentType.NumDrawnTracks;
                                                        selectedIndex = rp;
                                                        selectedPart = SelectionPart.Center;
                                                        selectedOriginalX = mousePos.X;
                                                        return;
                                                    }
                                                }
                                            }
                                        }
                                        if (selectedLane < 0)
                                        {
                                            List<SongData.Fill> rpp = new List<SongData.Fill>();
                                            long newStartTime = GetNearestTimeStepFromX(mousePos.X);
                                            int p = 0;
                                            for (; p < SongData.instruments[instr].fills.Length; p++)
                                            {
                                                if (SongData.instruments[instr].fills[p].time < newStartTime)
                                                    rpp.Add(SongData.instruments[instr].fills[p]);
                                                else
                                                    break;
                                            }
                                            selectedIndex = rpp.Count;
                                            rpp.Add(new SongData.Fill((uint)newStartTime, 10000));
                                            for (; p < SongData.instruments[instr].fills.Length; p++)
                                            {
                                                rpp.Add(SongData.instruments[instr].fills[p]);
                                            }
                                            SongData.instruments[instr].fills = rpp.ToArray();
                                            selectedLane = i - InstrumentType.NumDrawnTracks;
                                            selectedPart = SelectionPart.End;
                                            return;
                                        }
                                    }
                                }
                                break;
                            case 4:
                                for (int instr = 0; instr < SongData.instruments.Length; instr++)
                                {
                                    if (SongData.instruments[instr].instrumentType == InstrumentType.CodeName)
                                    {
                                        for (int rp = 0; rp < SongData.instruments[instr].rpPhrases.Length; rp++)
                                        {
                                            int x1 = (int)(SongData.instruments[instr].rpPhrases[rp].time * xScale);
                                            int x2 = (int)((SongData.instruments[instr].rpPhrases[rp].time + SongData.instruments[instr].rpPhrases[rp].len) * xScale);
                                            if (e.Button == MouseButtons.Left)
                                            {
                                                if (Form1.Ctrl)
                                                {
                                                    if (mousePos.X >= x1 && mousePos.X <= x2)
                                                    {
                                                        ConfirmDialog d = new ConfirmDialog();
                                                        d.Title = "Delete Rock Power Section?";
                                                        d.Question = "Are you sure you want to delete this Rock Power Section?";
                                                        DialogResult res = d.ShowDialog();
                                                        if (res == DialogResult.Yes)
                                                        {
                                                            List<SongData.RockPowerPhrase> rpp = new List<SongData.RockPowerPhrase>();
                                                            for (int p = 0; p < SongData.instruments[instr].rpPhrases.Length; p++)
                                                            {
                                                                if (p != rp)
                                                                    rpp.Add(SongData.instruments[instr].rpPhrases[p]);
                                                            }
                                                            SongData.instruments[instr].rpPhrases = rpp.ToArray();
                                                            Invalidate();
                                                            return;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (Math.Abs(mousePos.X - x1) < 10)
                                                    {
                                                        selectedLane = i - InstrumentType.NumDrawnTracks;
                                                        selectedIndex = rp;
                                                        selectedPart = SelectionPart.Beginning;
                                                        return;
                                                    }
                                                    else if (Math.Abs(mousePos.X - x2) < 10)
                                                    {
                                                        selectedLane = i - InstrumentType.NumDrawnTracks;
                                                        selectedIndex = rp;
                                                        selectedPart = SelectionPart.End;
                                                        return;
                                                    }
                                                    else if (mousePos.X >= x1 && mousePos.X < x2)
                                                    {
                                                        selectedLane = i - InstrumentType.NumDrawnTracks;
                                                        selectedIndex = rp;
                                                        selectedPart = SelectionPart.Center;
                                                        selectedOriginalX = mousePos.X;
                                                        return;
                                                    }
                                                }
                                            }
                                        }
                                        if(selectedLane<0)
                                        {
                                            List<SongData.RockPowerPhrase> rpp = new List<SongData.RockPowerPhrase>();
                                            long newStartTime = GetNearestTimeStepFromX(mousePos.X);
                                            int p = 0;
                                            for (; p < SongData.instruments[instr].rpPhrases.Length; p++)
                                            {
                                                if (SongData.instruments[instr].rpPhrases[p].time < newStartTime)
                                                    rpp.Add(SongData.instruments[instr].rpPhrases[p]);
                                                else
                                                    break;
                                            }
                                            selectedIndex = rpp.Count;
                                            rpp.Add(new SongData.RockPowerPhrase((uint)newStartTime, 10000));
                                            for (; p < SongData.instruments[instr].rpPhrases.Length; p++)
                                            {
                                                rpp.Add(SongData.instruments[instr].rpPhrases[p]);
                                            }
                                            SongData.instruments[instr].rpPhrases = rpp.ToArray();
                                            selectedLane = i - InstrumentType.NumDrawnTracks;
                                            selectedPart = SelectionPart.End;
                                            return;
                                        }
                                    }
                                }
                                break;
                        }
                    }
                }
            }
        }

        private void NotesViewerControl_MouseUp(object sender, MouseEventArgs e)
        {
            selectedLane = -1;
        }

        private long GetNearestTimeStepFromX(int x)
        {
            long resultantX = -1;
            for (int i = 0; i < SongData.info.barlines.Length - 1; i++)
            {
                long x1 = (long)(SongData.info.barlines[i].time * xScale);
                long x2 = (long)(SongData.info.barlines[i+1].time * xScale);
                if (x >= x1 && x < x2)
                {
                    if (StepDenominator == 1)
                    {
                        if (Math.Abs(x1 - x) < Math.Abs(x2 - x))
                            resultantX = x1;
                        else
                            resultantX = x2;
                    }
                    else if (StepDenominator == 2 || StepDenominator == 4)
                    {
                        int ind = 0;
                        int numBeats = (int)SongData.info.barlines[i].numBeats;
                        for (int k = 1; k <= numBeats; k++)
                        {
                            long x3 = (long)(x1 + ((ind / (float)numBeats) * (x2 - x1)));
                            long x4 = (long)(x1 + ((k / (float)numBeats) * (x2 - x1)));
                            if (Math.Abs(x4 - x) < Math.Abs(x3 - x))
                                ind = k;
                        }
                        resultantX = (long)(x1 + ((ind / (float)numBeats) * (x2 - x1)));
                    }
                    else
                    {
                        int numDivs = StepDenominator / 4;

                        int ind1 = 0, ind2 = 0;
                        int numBeats = (int)SongData.info.barlines[i].numBeats;
                        for (int k = 0; k < numBeats; k++)
                        {
                            for (int j = 0; j < numDivs; j++)
                            {
                                long x3b1 = (long)(x1 + ((ind1 / (float)numBeats) * (x2 - x1)));
                                long x3b2 = (long)(x1 + (((ind1 + 1) / (float)numBeats) * (x2 - x1)));
                                long x4b1 = (long)(x1 + ((k / (float)numBeats) * (x2 - x1)));
                                long x4b2 = (long)(x1 + (((k + 1) / (float)numBeats) * (x2 - x1)));
                                long x3 = (long)(x3b1 + ((ind2 / (float)numDivs) * (x3b2 - x3b1)));
                                long x4 = (long)(x4b1 + ((j / (float)numDivs) * (x4b2 - x4b1)));
                                if (Math.Abs(x4 - x) < Math.Abs(x3 - x))
                                {
                                    ind1 = k;
                                    ind2 = j;
                                }
                            }
                        }
                        {
                            long x3b1 = (long)(x1 + ((ind1 / (float)numBeats) * (x2 - x1)));
                            long x3b2 = (long)(x1 + (((ind1 + 1) / (float)numBeats) * (x2 - x1)));
                            resultantX = (long)(x3b1 + ((ind2 / (float)numDivs) * (x3b2 - x3b1)));
                        }
                    }
                }
            }
            return (int)(resultantX/xScale);
        }

        private void NotesViewerControl_KeyDown(object sender, KeyEventArgs e)
        {
        }

        private void NotesViewerControl_KeyUp(object sender, KeyEventArgs e)
        {
        }

        private void NotesViewerControl_Leave(object sender, EventArgs e)
        {
        }

        private void NotesViewerControl_Enter(object sender, EventArgs e)
        {
            Invalidate();
        }

        internal void CheckValidity()
        {
            for (int instr = 0; instr < SongData.instruments.Length; instr++)
            {
                if (SongData.instruments[instr].instrumentType == InstrumentType.CodeName)
                {
                    for(int rp1=0;rp1<SongData.instruments[instr].rpPhrases.Length;rp1++)
                        for (int rp2 = 0; rp2 < SongData.instruments[instr].rpPhrases.Length; rp2++)
                        {
                            if (rp1 != rp2)
                            {
                                SongData.RockPowerPhrase r1 = SongData.instruments[instr].rpPhrases[rp1];
                                SongData.RockPowerPhrase r2 = SongData.instruments[instr].rpPhrases[rp2];
                                if ((r1.time >= r2.time && r1.time <= r2.end) || (r1.end >= r2.time && r1.end <= r2.end) ||
                                   (r2.time >= r1.time && r2.time <= r1.end) || (r2.end >= r1.time && r2.end <= r1.end))
                                {
                                    throw new Exception("Overlapping Rock Power Phrases for " + InstrumentType.FullName);
                                }
                            }
                        }
                    if((InstrumentType.RPEnableType&Instrument.RockPowerEnableTypes.FILL)!=0)
                    for (int df1 = 0; df1 < SongData.instruments[instr].fills.Length; df1++)
                        for (int df2 = 0; df2 < SongData.instruments[instr].fills.Length; df2++)
                        {
                            if (df1 != df2)
                            {
                                SongData.Fill r1 = SongData.instruments[instr].fills[df1];
                                SongData.Fill r2 = SongData.instruments[instr].fills[df2];
                                if ((r1.time >= r2.time && r1.time <= r2.end) || (r1.end >= r2.time && r1.end <= r2.end) ||
                                   (r2.time >= r1.time && r2.time <= r1.end) || (r2.end >= r1.time && r2.end <= r1.end))
                                {
                                    throw new Exception("Overlapping Fills for " + InstrumentType.FullName);
                                }
                            }
                        }
                    if(InstrumentType.HasSolos)
                    for (int sl1 = 0; sl1 < SongData.instruments[instr].solos.Length; sl1++)
                        for (int sl2 = 0; sl2 < SongData.instruments[instr].solos.Length; sl2++)
                        {
                            if (sl1 != sl2)
                            {
                                SongData.Solo r1 = SongData.instruments[instr].solos[sl1];
                                SongData.Solo r2 = SongData.instruments[instr].solos[sl2];
                                if ((r1.time >= r2.time && r1.time <= r2.end) || (r1.end >= r2.time && r1.end <= r2.end) ||
                                   (r2.time >= r1.time && r2.time <= r1.end) || (r2.end >= r1.time && r2.end <= r1.end))
                                {
                                    throw new Exception("Overlapping Solos for " + InstrumentType.FullName);
                                }
                            }
                        }
                }
            }
        }
    }
}

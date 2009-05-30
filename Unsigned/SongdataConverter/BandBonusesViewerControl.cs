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
    public partial class BandBonusesViewerControl : UserControl
    {
        private enum SelectionPart
        {
            None = 0,
            Beginning,
            Center,
            End,
            ChangeInstruments,
        }

        private SongData _songData = null;
        public SongData SongData
        {
            get { return _songData; }
            set { _songData = value; RefreshAllInfo(); }
        }
        private static Brush[] colors = { Brushes.Green, Brushes.Red, Brushes.Yellow, Brushes.Blue, Brushes.Orange };

        private int selectedLane, selectedIndex, selectedOriginalX;
        private SelectionPart selectedPart;

        private Dictionary<String, Image> instrumentIcons;

        private String[] DrawnInstruments = { "LGT", "BAS", "SET" };

        private int NumDrawnTracks { get { return DrawnInstruments.Length; } }

        public int StepDenominator;

        public int numLanes;
        public float xScale;

        public int TextXOffset;

        private const int Difficulty = 3;

        public BandBonusesViewerControl()
        {
            InitializeComponent();
            xScale = 0.1f;
            selectedLane = -1;
            numLanes = NumDrawnTracks + 2;
            instrumentIcons = new Dictionary<string, Image>();
            if (InstrumentMaster.Singleton.IsLoaded)
            {
                for (int i = 0; i < InstrumentMaster.Singleton.GetNumInstruments(); i++)
                {
                    instrumentIcons.Add(InstrumentMaster.Singleton.GetInstrument(i).CodeName, Bitmap.FromFile("Content\\"+InstrumentMaster.Singleton.GetInstrument(i).CodeName + "icon.png"));
                    rightClickMenu.Items.Add(InstrumentMaster.Singleton.GetInstrument(i).FullName);
                    rightClickMenu.Items[rightClickMenu.Items.Count - 1].Click += new EventHandler(RightClickMenuItem_click);
                }
            }
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
            if (SongData != null)
            {
                //special lane color divideby amount
                int divideby = 4;
                {
                    int y = (int)(Bounds.Height * (NumDrawnTracks / (float)(numLanes)));
                    int h = (int)(Bounds.Height * (1 / (float)(numLanes)));
                    Color srcCol = Color.Lime;
                    Color b = Color.FromArgb(srcCol.R / divideby, srcCol.G / divideby, srcCol.B / divideby);
                    e.Graphics.FillRectangle(new SolidBrush(b), new Rectangle(0, y, Bounds.Width, h));
                }
                {
                    int y = (int)(Bounds.Height * ((NumDrawnTracks + 1) / (float)(numLanes)));
                    int h = (int)(Bounds.Height * (1 / (float)(numLanes)));
                    Color srcCol = Color.Yellow;
                    Color b = Color.FromArgb(srcCol.R / divideby, srcCol.G / divideby, srcCol.B / divideby);
                    e.Graphics.FillRectangle(new SolidBrush(b), new Rectangle(0, y, Bounds.Width, h));
                }
                if (SongData.info.bre.enabled)
                {
                    int x = (int)(SongData.info.bre.start * xScale) + TextXOffset;
                    int w = (int)(SongData.info.bre.end * xScale) + TextXOffset;
                    int y = (int)(Height * (NumDrawnTracks / (float)(numLanes)));
                    int h = (int)(Height * (1 / (float)(numLanes)));
                    e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(0, 255, 0)), new Rectangle(x, y, w-x, h));
                }
                for(int i=0;i<SongData.info.harmonies.Length;i++)
                {
                    int x = (int)(SongData.info.harmonies[i].start * xScale) + TextXOffset;
                    int w = (int)(SongData.info.harmonies[i].end * xScale) + TextXOffset;
                    int y = (int)(Height * ((NumDrawnTracks+1) / (float)(numLanes)));
                    int h = (int)(Height * (1 / (float)(numLanes)));
                    e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, 255, 0)), new Rectangle(x, y, w - x, h));
                    int height = h / 2;
                    for (int k = 0; k < SongData.info.harmonies[i].instruments.Length; k += 3)
                    {
                        String str = SongData.info.harmonies[i].instruments.Substring(k, 3);
                        int index = k / 3;
                        e.Graphics.DrawImage(instrumentIcons[str], new Rectangle(x + (height * (index / 2)), y + (height * (index % 2)), height, height));
                    }
                }
                for (int track = 0; track < DrawnInstruments.Length; track++)
                {
                    Instrument InstrumentType = InstrumentMaster.Singleton.GetInstrument(DrawnInstruments[track]);
                    for (int instr = 0; instr < SongData.instruments.Length; instr++)
                        if (SongData.instruments[instr].instrumentType == InstrumentType.CodeName)
                        {
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
                                            int h = (int)(Bounds.Height * ((1 / (float)InstrumentType.NumDrawnTracks) / (float)(numLanes)));
                                            Rectangle rect = new Rectangle(x, (int)(Bounds.Height * ((track / (float)(numLanes)) + ((i / (float)InstrumentType.NumDrawnTracks) / (float)numLanes))) + (int)(h * 0.3f), w, (int)(h * 0.4f));
                                            e.Graphics.FillRectangle(b, rect);
                                        }
                                    }
                                }
                            }
                        }
                }
                e.Graphics.DrawString("Guitar:", new Font(FontFamily.GenericSansSerif, 14), Brushes.White, new PointF(10, Bounds.Height * ((0) / (float)numLanes)));
                e.Graphics.DrawString("Bass:", new Font(FontFamily.GenericSansSerif, 14), Brushes.White, new PointF(10, Bounds.Height * ((1) / (float)numLanes)));
                e.Graphics.DrawString("Drums:", new Font(FontFamily.GenericSansSerif, 14), Brushes.White, new PointF(10, Bounds.Height * ((2) / (float)numLanes)));
                e.Graphics.DrawString("BRE:", new Font(FontFamily.GenericSansSerif, 14), Brushes.White, new PointF(10, Bounds.Height * ((NumDrawnTracks) / (float)numLanes)));
                e.Graphics.DrawString("Harmonies:", new Font(FontFamily.GenericSansSerif, 14), Brushes.White, new PointF(10, Bounds.Height * ((NumDrawnTracks + 1) / (float)numLanes)));
                //e.Graphics.DrawString("PS2:", new Font(FontFamily.GenericSansSerif, 14), Brushes.White, new PointF(10, Bounds.Height * ((NumDrawnTracks + 2) / (float)numLanes)));
                //e.Graphics.DrawString("Fills:", new Font(FontFamily.GenericSansSerif, 14), Brushes.White, new PointF(10, Bounds.Height * ((NumDrawnTracks + 3) / (float)numLanes)));
                //e.Graphics.DrawString("RP:", new Font(FontFamily.GenericSansSerif, 14), Brushes.White, new PointF(10, Bounds.Height * ((NumDrawnTracks + 4) / (float)numLanes)));

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
                    if (i == 0 || i == NumDrawnTracks)
                    {
                        h1 = 1;
                        h2 = 3;
                    }
                    int y = (int)(Bounds.Height * (i / (float)(numLanes)));
                    e.Graphics.FillRectangle(Brushes.White, new Rectangle(0, y - h1, Bounds.Width, h2));
                }
                for (int track = 0; track < DrawnInstruments.Length; track++)
                {
                    Instrument InstrumentType = InstrumentMaster.Singleton.GetInstrument(DrawnInstruments[track]);
                    for (int instr = 0; instr < SongData.instruments.Length; instr++)
                        if (SongData.instruments[instr].instrumentType == DrawnInstruments[track])
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
                                            Rectangle rect = new Rectangle(x - 2, (int)(Bounds.Height * (track / (float)(numLanes))), 4, (int)(Bounds.Height * (1 / (float)(numLanes))));
                                            e.Graphics.FillRectangle(colors[InstrumentType.colorIndices[i]], rect);
                                        }
                                    }
                                    for (int i = 0; i < InstrumentType.NumDrawnTracks; i++)
                                    {
                                        if ((note.type & ((ulong)1 << i)) != 0)
                                        {
                                            int x = (int)(note.time * xScale) + TextXOffset;
                                            Rectangle rect = new Rectangle(x - 5, (int)(Bounds.Height * ((track / (float)(numLanes))+((i/(float)InstrumentType.NumDrawnTracks)/(float)numLanes))), 10, (int)(Bounds.Height * ((1/(float)InstrumentType.NumDrawnTracks) / (float)(numLanes))));
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
            }
            if (!this.Focused)
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(128, 0, 0, 0)), new Rectangle(0, 0, Bounds.Width, Bounds.Height));
            }
            //base.OnPaint(e);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            //base.OnPaintBackground(e);
        }

        private void NotesViewerControl_MouseLeave(object sender, EventArgs e)
        {
            Cursor = Cursors.Default;
            selectedLane = -1;
        }

        private void NotesViewerControl_MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = new Point(e.X - TextXOffset, e.Y);
            int h = (int)(Bounds.Height * (NumDrawnTracks / (float)(numLanes)));
            if (mousePos.Y >= h)
            {
                if (selectedLane >= 0)
                {
                    for (int i = NumDrawnTracks; i < numLanes; i++)
                    {
                        int y1 = (int)(Bounds.Height * (i / (float)(numLanes)));
                        int y2 = (int)(Bounds.Height * ((i + 1) / (float)(numLanes)));
                        if (mousePos.Y >= y1 && mousePos.Y < y2)
                        {
                            if (i - NumDrawnTracks == selectedLane)
                            {
                                switch (selectedLane)
                                {
                                    case 0:
                                        if (selectedPart == SelectionPart.Beginning)
                                        {
                                            uint time = (uint)GetNearestTimeStepFromX(mousePos.X);
                                            long len = ((long)(SongData.info.bre.end) - (long)(time));
                                            if (len > 10)
                                            {
                                                SongData.info.bre.start = time;
                                            }
                                        }
                                        else if (selectedPart == SelectionPart.End)
                                        {
                                            long end = GetNearestTimeStepFromX(mousePos.X);
                                            long len = (end - SongData.info.bre.start);
                                            if (len > 10)
                                            {
                                                SongData.info.bre.end = (uint)end;
                                            }
                                        }
                                        else if (selectedPart == SelectionPart.Center)
                                        {
                                            long t1 = GetNearestTimeStepFromX(selectedOriginalX);
                                            long t2 = GetNearestTimeStepFromX(mousePos.X);
                                            if (t1 != t2)
                                            {
                                                SongData.info.bre.start = (uint)(SongData.info.bre.start + (t2 - t1));
                                                SongData.info.bre.end = (uint)(SongData.info.bre.end + (t2 - t1));
                                                selectedOriginalX = mousePos.X;
                                            }
                                        }
                                        Invalidate();
                                        break;
                                    case 1:
                                        if (selectedPart == SelectionPart.Beginning)
                                        {
                                            uint time = (uint)GetNearestTimeStepFromX(mousePos.X);
                                            long len = ((long)(SongData.info.harmonies[selectedIndex].end) - (long)(time));
                                            if (len > 10)
                                            {
                                                SongData.info.harmonies[selectedIndex].start = time;
                                            }
                                        }
                                        else if (selectedPart == SelectionPart.End)
                                        {
                                            long end = GetNearestTimeStepFromX(mousePos.X);
                                            long len = (end - SongData.info.harmonies[selectedIndex].start);
                                            if (len > 10)
                                            {
                                                SongData.info.harmonies[selectedIndex].end = (uint)end;
                                            }
                                        }
                                        else if (selectedPart == SelectionPart.Center)
                                        {
                                            long t1 = GetNearestTimeStepFromX(selectedOriginalX);
                                            long t2 = GetNearestTimeStepFromX(mousePos.X);
                                            if (t1 != t2)
                                            {
                                                SongData.info.harmonies[selectedIndex].start = (uint)(SongData.info.harmonies[selectedIndex].start + (t2 - t1));
                                                SongData.info.harmonies[selectedIndex].end = (uint)(SongData.info.harmonies[selectedIndex].end + (t2 - t1));
                                                selectedOriginalX = mousePos.X;
                                            }
                                        }
                                        Invalidate();
                                        break;
                                }
                            }
                            else
                            {
                                selectedLane = -1;
                                selectedPart = SelectionPart.None;
                            }
                        }
                    }
                }
                else
                {
                    Cursor = Cursors.Default;
                    for (int i = NumDrawnTracks; i < numLanes; i++)
                    {
                        int y1 = (int)(Bounds.Height * (i / (float)(numLanes)));
                        int y2 = (int)(Bounds.Height * ((i + 1) / (float)(numLanes)));
                        if (mousePos.Y >= y1 && mousePos.Y < y2)
                        {
                            switch (i - NumDrawnTracks)
                            {
                                case 0:
                                    {
                                        int x1 = (int)(SongData.info.bre.start * xScale);
                                        int x2 = (int)(SongData.info.bre.end * xScale);
                                        if (Form1.Ctrl)
                                        {
                                            if(SongData.info.bre.enabled)
                                            if (mousePos.X >= x1 && mousePos.X <= x2)
                                                Cursor = Form1.DeleteCursor;
                                        }
                                        else
                                        {
                                            if (SongData.info.bre.enabled)
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
                                    if (Cursor == Cursors.Default && !SongData.info.bre.enabled)
                                        Cursor = Cursors.Cross;
                                    break;
                                case 1:
                                    for (int rp = 0; rp < SongData.info.harmonies.Length; rp++)
                                    {
                                        int x1 = (int)(SongData.info.harmonies[rp].start * xScale);
                                        int x2 = (int)(SongData.info.harmonies[rp].end * xScale);
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
            Point mousePos = new Point(e.X - TextXOffset, e.Y);
            int h = (int)(Bounds.Height * (NumDrawnTracks / (float)(numLanes)));
            if (mousePos.Y >= h)
            {
                for (int i = NumDrawnTracks; i < numLanes; i++)
                {
                    int y1 = (int)(Bounds.Height * (i / (float)(numLanes)));
                    int y2 = (int)(Bounds.Height * ((i + 1) / (float)(numLanes)));
                    if (mousePos.Y >= y1 && mousePos.Y < y2)
                    {
                        switch (i - NumDrawnTracks)
                        {
                            case 0:
                                {
                                    int x1 = (int)(SongData.info.bre.start * xScale);
                                    int x2 = (int)(SongData.info.bre.end * xScale);
                                    if (e.Button == MouseButtons.Left)
                                    {
                                        if (Form1.Ctrl)
                                        {
                                            if (SongData.info.bre.enabled)
                                            {
                                                if (mousePos.X >= x1 && mousePos.X <= x2)
                                                {
                                                    ConfirmDialog d = new ConfirmDialog();
                                                    d.Title = "Delete Big Rock Ending?";
                                                    d.Question = "Are you sure you want to delete the Big Rock Ending?";
                                                    DialogResult res = d.ShowDialog();
                                                    if (res == DialogResult.Yes)
                                                    {
                                                        SongData.info.bre.enabled = false;
                                                        Invalidate();
                                                        return;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (SongData.info.bre.enabled)
                                            {
                                                if (Math.Abs(mousePos.X - x1) < 10)
                                                {
                                                    selectedLane = i - NumDrawnTracks;
                                                    selectedIndex = 0;
                                                    selectedPart = SelectionPart.Beginning;
                                                    return;
                                                }
                                                else if (Math.Abs(mousePos.X - x2) < 10)
                                                {
                                                    selectedLane = i - NumDrawnTracks;
                                                    selectedIndex = 0;
                                                    selectedPart = SelectionPart.End;
                                                    return;
                                                }
                                                else if (mousePos.X >= x1 && mousePos.X < x2)
                                                {
                                                    selectedLane = i - NumDrawnTracks;
                                                    selectedIndex = 0;
                                                    selectedPart = SelectionPart.Center;
                                                    selectedOriginalX = mousePos.X;
                                                    return;
                                                }
                                            }
                                        }
                                        if (selectedLane < 0 && !SongData.info.bre.enabled)
                                        {
                                            List<SongData.Solo> rpp = new List<SongData.Solo>();
                                            long newStartTime = GetNearestTimeStepFromX(mousePos.X);
                                            selectedIndex = 0;
                                            SongData.info.bre.enabled = true;
                                            SongData.info.bre.start = (uint)newStartTime;
                                            SongData.info.bre.end = (uint)newStartTime + 10000;
                                            selectedLane = i - NumDrawnTracks;
                                            selectedPart = SelectionPart.End;
                                            return;
                                        }
                                    }
                                }
                                break;
                            case 1:
                                for (int rp = 0; rp < SongData.info.harmonies.Length; rp++)
                                {
                                    int x1 = (int)(SongData.info.harmonies[rp].start * xScale);
                                    int x2 = (int)(SongData.info.harmonies[rp].end * xScale);
                                    if (e.Button == MouseButtons.Left)
                                    {
                                        if (Form1.Ctrl)
                                        {
                                            if (mousePos.X >= x1 && mousePos.X <= x2)
                                            {
                                                ConfirmDialog d = new ConfirmDialog();
                                                d.Title = "Delete Harmony Section?";
                                                d.Question = "Are you sure you want to delete this Harmony Section?";
                                                DialogResult res = d.ShowDialog();
                                                if (res == DialogResult.Yes)
                                                {
                                                    List<SongData.Harmony> rpp = new List<SongData.Harmony>();
                                                    for (int p = 0; p < SongData.info.harmonies.Length; p++)
                                                    {
                                                        if (p != rp)
                                                            rpp.Add(SongData.info.harmonies[p]);
                                                    }
                                                    SongData.info.harmonies = rpp.ToArray();
                                                    Invalidate();
                                                    return;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (Math.Abs(mousePos.X - x1) < 10)
                                            {
                                                selectedLane = i - NumDrawnTracks;
                                                selectedIndex = rp;
                                                selectedPart = SelectionPart.Beginning;
                                                return;
                                            }
                                            else if (Math.Abs(mousePos.X - x2) < 10)
                                            {
                                                selectedLane = i - NumDrawnTracks;
                                                selectedIndex = rp;
                                                selectedPart = SelectionPart.End;
                                                return;
                                            }
                                            else if (mousePos.X >= x1 && mousePos.X < x2)
                                            {
                                                selectedLane = i - NumDrawnTracks;
                                                selectedIndex = rp;
                                                selectedPart = SelectionPart.Center;
                                                selectedOriginalX = mousePos.X;
                                                return;
                                            }
                                        }
                                    }
                                    else if (e.Button == MouseButtons.Right)
                                    {
                                        if (mousePos.X >= x1 && mousePos.X < x2)
                                        {
                                            selectedLane = i - NumDrawnTracks;
                                            selectedIndex = rp;
                                            selectedPart = SelectionPart.ChangeInstruments;
                                            rightClickMenu.Show();
                                            rightClickMenu.Location = MousePosition;
                                            return;
                                        }
                                    }
                                }
                                if (selectedLane < 0)
                                {
                                    List<SongData.Harmony> rpp = new List<SongData.Harmony>();
                                    long newStartTime = GetNearestTimeStepFromX(mousePos.X);
                                    int p = 0;
                                    for (; p < SongData.info.harmonies.Length; p++)
                                    {
                                        if (SongData.info.harmonies[p].start < newStartTime)
                                            rpp.Add(SongData.info.harmonies[p]);
                                        else
                                            break;
                                    }
                                    selectedIndex = rpp.Count;
                                    String str = "";
                                    for (int u = 0; u < InstrumentMaster.Singleton.GetNumInstruments(); u++)
                                        str += InstrumentMaster.Singleton.GetInstrument(u).CodeName;
                                    rpp.Add(new SongData.Harmony((uint)newStartTime, (uint)newStartTime+10000, str));
                                    for (; p < SongData.info.harmonies.Length; p++)
                                    {
                                        rpp.Add(SongData.info.harmonies[p]);
                                    }
                                    SongData.info.harmonies = rpp.ToArray();
                                    selectedLane = i - NumDrawnTracks;
                                    selectedPart = SelectionPart.End;
                                    return;
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
                long x2 = (long)(SongData.info.barlines[i + 1].time * xScale);
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
            return (int)(resultantX / xScale);
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
        }

        internal void RightClickMenuItem_click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem)
            {
                ToolStripMenuItem item = (ToolStripMenuItem)sender;
                //if (selectedLane == 1)
                {
                    String str = SongData.info.harmonies[selectedIndex].instruments;
                    Instrument instr = null;
                    for (int i = 0; i < InstrumentMaster.Singleton.GetNumInstruments(); i++)
                        if (InstrumentMaster.Singleton.GetInstrument(i).FullName == item.Text)
                            instr = InstrumentMaster.Singleton.GetInstrument(i);
                    if (instr != null)
                    {
                        if (SongData.info.harmonies[selectedIndex].ContainsInstrument(instr))
                            SongData.info.harmonies[selectedIndex].RemoveInstrument(instr);
                        else
                            SongData.info.harmonies[selectedIndex].AddInstrument(instr);
                    }
                }
            }
            Invalidate();
        }
    }
}

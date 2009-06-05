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
    public partial class RoadieViewerControl : UserControl
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

        public RoadieViewerControl()
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
                    Color srcCol = Color.Blue;
                    Color b = Color.FromArgb(srcCol.R / divideby, srcCol.G / divideby, srcCol.B / divideby);
                    e.Graphics.FillRectangle(new SolidBrush(b), new Rectangle(0, y, Bounds.Width, h));
                }
                {
                    int y = (int)(Bounds.Height * ((NumDrawnTracks + 1) / (float)(numLanes)));
                    int h = (int)(Bounds.Height * (1 / (float)(numLanes)));
                    Color srcCol = Color.White;
                    Color b = Color.FromArgb(srcCol.R / divideby, srcCol.G / divideby, srcCol.B / divideby);
                    e.Graphics.FillRectangle(new SolidBrush(b), new Rectangle(0, y, Bounds.Width, h));
                }
                for (int i = 0; i < SongData.effects.effects.Length; i++)
                {
                    if (SongData.effects.effects[i] is SongData.NormalLightingSpecialEffect)
                    {
                        SongData.NormalLightingSpecialEffect ef = (SongData.NormalLightingSpecialEffect)SongData.effects.effects[i];
                        int x = (int)(ef.begin * xScale) + TextXOffset;
                        int x2 = (int)(ef.end * xScale) + TextXOffset;
                        int y = (int)(Height * ((NumDrawnTracks+1) / (float)(numLanes)));
                        int h = (int)(Height * (1 / (float)(numLanes)));
                        e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(ef.color.R, ef.color.G, ef.color.B)), new Rectangle(x, y, x2-x, h));
                    }
                    else if (SongData.effects.effects[i] is SongData.GradientLightingSpecialEffect)
                    {
                        SongData.GradientLightingSpecialEffect ef = (SongData.GradientLightingSpecialEffect)SongData.effects.effects[i];
                        int x = (int)(ef.begin * xScale) + TextXOffset;
                        int w = (int)(((ef.end * xScale) + TextXOffset)-x);
                        int y = (int)(Height * ((NumDrawnTracks + 1) / (float)(numLanes)));
                        int h = (int)(Height * (1 / (float)(numLanes)));
                        for (int k = 0; k < w; k += 10)
                        {
                            Color col = Color.FromArgb((byte)(((k / (float)w) * ef.color2.R) + ((1 - (k / (float)w)) * ef.color1.R)), (byte)(((k / (float)w) * ef.color2.G) + ((1 - (k / (float)w)) * ef.color1.G)), (byte)(((k / (float)w) * ef.color2.B) + ((1 - (k / (float)w)) * ef.color1.B)));
                            e.Graphics.FillRectangle(new SolidBrush(col), new Rectangle(x+k, y, Math.Min(10,w - k), h));
                        }
                    }
                    else if (SongData.effects.effects[i] is SongData.StrobeLightingSpecialEffect)
                    {
                        SongData.StrobeLightingSpecialEffect ef = (SongData.StrobeLightingSpecialEffect)SongData.effects.effects[i];
                        int x = (int)(ef.begin * xScale) + TextXOffset;
                        int w = (int)(((ef.end * xScale) + TextXOffset) - x);
                        int y = (int)(Height * ((NumDrawnTracks + 1) / (float)(numLanes)));
                        int h = (int)(Height * (1 / (float)(numLanes)));
                        for (int k = 0; k < w; k += 1)
                        {
                            float lerp = GetBeatTime((((x+k) - TextXOffset) / xScale)/1000f);
                            lerp *= ef.frequency;
                            lerp %= 1.0f;
                            if (lerp < 0.5f)
                                lerp *= 2;
                            else
                                lerp = 1-((lerp-0.5f)*2);
                            Color col = Color.FromArgb((byte)((lerp * ef.color2.R) + ((1 - lerp) * ef.color1.R)), (byte)((lerp * ef.color2.G) + ((1 - lerp) * ef.color1.G)), (byte)((lerp * ef.color2.B) + ((1 - lerp) * ef.color1.B)));
                            e.Graphics.FillRectangle(new SolidBrush(col), new Rectangle(x + k, y, Math.Min(1, w - k), h));
                        }
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
                e.Graphics.DrawString("Camera:", new Font(FontFamily.GenericSansSerif, 14), Brushes.White, new PointF(10, Bounds.Height * ((NumDrawnTracks) / (float)numLanes)));
                e.Graphics.DrawString("Main Lights:", new Font(FontFamily.GenericSansSerif, 14), Brushes.White, new PointF(10, Bounds.Height * ((NumDrawnTracks + 1) / (float)numLanes)));

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

                for (int i = 0; i < SongData.effects.cameraSwitches.Length; i++)
                {
                    int x = (int)(SongData.effects.cameraSwitches[i] * xScale) + TextXOffset - 2;
                    int w = 4;
                    int y = (int)(Height * ((NumDrawnTracks) / (float)(numLanes)));
                    int h = (int)(Height * (1 / (float)(numLanes)));
                    e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(0, 0, 255)), new Rectangle(x, y, w, h));
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
                                        if (selectedPart == SelectionPart.Center)
                                        {
                                            SongData.effects.cameraSwitches[selectedIndex] = (uint)GetNearestTimeStepFromX(mousePos.X);
                                        }
                                        Invalidate();
                                        break;
                                    case 1:
                                        if (selectedPart == SelectionPart.Beginning)
                                        {
                                            uint time = (uint)GetNearestTimeStepFromX(mousePos.X);
                                            long len = ((long)(SongData.effects.effects[selectedIndex].end) - (long)(time));
                                            if (len > 10)
                                            {
                                                SongData.effects.effects[selectedIndex].time = time;
                                                SongData.effects.effects[selectedIndex].length = (uint)len;
                                            }
                                        }
                                        else if (selectedPart == SelectionPart.End)
                                        {
                                            long end = GetNearestTimeStepFromX(mousePos.X);
                                            long len = (end - SongData.effects.effects[selectedIndex].begin);
                                            if (len > 10)
                                            {
                                                SongData.effects.effects[selectedIndex].length = (uint)len;
                                            }
                                        }
                                        else if (selectedPart == SelectionPart.Center)
                                        {
                                            long t1 = GetNearestTimeStepFromX(selectedOriginalX);
                                            long t2 = GetNearestTimeStepFromX(mousePos.X);
                                            if (t1 != t2)
                                            {
                                                SongData.effects.effects[selectedIndex].time = (uint)(SongData.effects.effects[selectedIndex].begin + (t2 - t1));
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
                                        for (int rp = 0; rp < SongData.effects.cameraSwitches.Length; rp++)
                                        {
                                            int x1 = (int)(SongData.effects.cameraSwitches[rp] * xScale);
                                            if (Form1.Ctrl)
                                            {
                                                if (Math.Abs(mousePos.X- x1)<10)
                                                    Cursor = Form1.DeleteCursor;
                                            }
                                            else
                                            {
                                                if (Math.Abs(mousePos.X - x1) < 10)
                                                    Cursor = Cursors.SizeWE;
                                            }
                                        }
                                        if (Cursor == Cursors.Default)
                                            Cursor = Cursors.Cross;
                                    }
                                    break;
                                case 1:
                                    for (int rp = 0; rp < SongData.effects.effects.Length; rp++)
                                    {
                                        int x1 = (int)(SongData.effects.effects[rp].begin * xScale);
                                        int x2 = (int)(SongData.effects.effects[rp].end * xScale);
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
                                for (int rp = 0; rp < SongData.effects.cameraSwitches.Length; rp++)
                                {
                                    int x1 = (int)(SongData.effects.cameraSwitches[rp] * xScale);
                                    if (e.Button == MouseButtons.Left)
                                    {
                                        if (Form1.Ctrl)
                                        {
                                            if (Math.Abs(mousePos.X - x1) < 10)
                                            {
                                                ConfirmDialog d = new ConfirmDialog();
                                                d.Title = "Delete Camera Switch Point?";
                                                d.Question = "Are you sure you want to delete the Camera Switch Point?";
                                                DialogResult res = d.ShowDialog();
                                                if (res == DialogResult.Yes)
                                                {
                                                    List<uint> l = new List<uint>();
                                                    for (int y = 0; y < SongData.effects.cameraSwitches.Length; y++)
                                                        if (y != rp)
                                                            l.Add(SongData.effects.cameraSwitches[y]);
                                                    SongData.effects.cameraSwitches = l.ToArray();
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
                                                selectedPart = SelectionPart.Center;
                                                return;
                                            }
                                        }
                                    }
                                }
                                if (selectedLane < 0)
                                {
                                    List<uint> rpp = new List<uint>();
                                    long newStartTime = GetNearestTimeStepFromX(mousePos.X);
                                    int k = 0;
                                    for (; k < SongData.effects.cameraSwitches.Length; k++)
                                        if (SongData.effects.cameraSwitches[k] < newStartTime)
                                            rpp.Add(SongData.effects.cameraSwitches[k]);
                                        else
                                            break;
                                    selectedIndex = k;
                                    rpp.Add((uint)newStartTime);
                                    for (; k < SongData.effects.cameraSwitches.Length; k++)
                                        rpp.Add(SongData.effects.cameraSwitches[k]);
                                    selectedLane = i - NumDrawnTracks;
                                    selectedPart = SelectionPart.Center;
                                    SongData.effects.cameraSwitches = rpp.ToArray();
                                    return;
                                }
                                break;
                            case 1:
                                for (int rp = 0; rp < SongData.effects.effects.Length; rp++)
                                {
                                    int x1 = (int)(SongData.effects.effects[rp].begin * xScale);
                                    int x2 = (int)(SongData.effects.effects[rp].end * xScale);
                                    if (e.Button == MouseButtons.Left)
                                    {
                                        if (Form1.Ctrl)
                                        {
                                            if (mousePos.X >= x1 && mousePos.X <= x2)
                                            {
                                                ConfirmDialog d = new ConfirmDialog();
                                                d.Title = "Delete Normal Lighting Effect?";
                                                d.Question = "Are you sure you want to delete this Normal Lighting Effect?";
                                                DialogResult res = d.ShowDialog();
                                                if (res == DialogResult.Yes)
                                                {
                                                    List<SongData.SpecialEffect> rpp = new List<SongData.SpecialEffect>();
                                                    for (int p = 0; p < SongData.effects.effects.Length; p++)
                                                    {
                                                        if (p != rp)
                                                            rpp.Add(SongData.effects.effects[p]);
                                                    }
                                                    SongData.effects.effects = rpp.ToArray();
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
                                            LightingEffectDialog d = new LightingEffectDialog();
                                            d.Effect = SongData.effects.effects[selectedIndex];
                                            DialogResult dr = d.ShowDialog();
                                            if (dr == DialogResult.OK)
                                                SongData.effects.effects[selectedIndex] = d.Effect;
                                            Invalidate();
                                            return;
                                        }
                                    }
                                }
                                if (selectedLane < 0)
                                {
                                    List<SongData.SpecialEffect> rpp = new List<SongData.SpecialEffect>();
                                    long newStartTime = GetNearestTimeStepFromX(mousePos.X);
                                    int p = 0;
                                    for (; p < SongData.effects.effects.Length; p++)
                                    {
                                        if (SongData.effects.effects[p].begin < newStartTime)
                                            rpp.Add(SongData.effects.effects[p]);
                                        else
                                            break;
                                    }
                                    selectedIndex = rpp.Count;
                                    rpp.Add(new SongData.NormalLightingSpecialEffect((uint)newStartTime, (uint)10000, new SongData.Color(255,255,255)));
                                    for (; p < SongData.effects.effects.Length; p++)
                                    {
                                        rpp.Add(SongData.effects.effects[p]);
                                    }
                                    SongData.effects.effects = rpp.ToArray();
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

        private float GetBeatTime(float time)
        {
            if (SongData == null)
                return 0;
            float currentTime = time;
            for (int i = 0; i < SongData.info.barlines.Length - 1; i++)
            {
                if (currentTime * 1000 < SongData.info.barlines[i].time)
                    continue;
                if (currentTime * 1000 > SongData.info.barlines[i + 1].time)
                    continue;
                float start = SongData.info.barlines[i].time / 1000f;
                float end = SongData.info.barlines[i + 1].time / 1000f;
                float val = (currentTime - start) / (end - start);
                val *= SongData.info.barlines[i].numBeats;
                val %= 1.0f;
                return val;
            }
            return 0;
        }
    }
}

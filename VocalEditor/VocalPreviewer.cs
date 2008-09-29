using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace VocalEditor
{
    public partial class VocalPreviewer : UserControl
    {
        public bool display;
        VoxSong song;
        public static MainForm mf;
        public int NodeSize;//4-*, multiple of 2
        public Color NodeSelectColor, NodeColor;
        public int SelectedNode, SelectedPhrase;
        public bool SelectedNodeBegin;
        float HSCALE = 0;
        public static int numNotes = 36;
        long mouseClickTime;
        Point mouseClickSpot;
        public int LineThickness=1;
        public float offset;
        private short oldUndoNote;
        private uint oldUndoTime;
        byte mouseDown = 0;

        public VocalPreviewer()
        {
            InitializeComponent();
            display = false;
            NodeColor = Color.Aqua;
            NodeSelectColor = Color.Lime;
            NodeSize = 8;
            SelectedNode = -1;
            SelectedPhrase = -1;
        }

        public void SetParentForm(MainForm m)
        {
            mf = m;
        }

        public void SetDisplay(bool d)
        {
            display = d;
        }
        public bool GetDisplay()
        {
            return display;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g;
            Bitmap drawing;

            drawing = new Bitmap(this.Width, this.Height, e.Graphics);
            g = Graphics.FromImage(drawing);

            g.FillRectangle(Brushes.Black, ClientRectangle);
            SolidBrush hLineBrush = new SolidBrush(Color.FromArgb(255, 30, 30, 30));
            SolidBrush vLineBrush = new SolidBrush(Color.FromArgb(255, 60, 60, 60));
            for (int i = 1; i < numNotes; i++)
                g.FillRectangle(hLineBrush, new Rectangle(0, (int)(ClientRectangle.Height * (i / (float)numNotes) + 0.5f), ClientRectangle.Width, 1));
            if (display)
            {
                float mfxValue = 100-(mf.xScaleTrackBar.Value / 100f);
                if (mfxValue <= 50)
                    HSCALE = (mfxValue / 100f + 0.5f) * (mfxValue / 100f + 0.5f);
                else
                    HSCALE = (mfxValue - 49)/2f+0.5f;
                int barsub = 0;
                if (mfxValue > 90)
                    barsub = 2;
                else if (mfxValue > 80)
                    barsub = 1;
                if(song==null)
                    return;
                if(!song.valid)
                    return;
                int width = (int)(song.bars[song.bars.Length - 1].time / HSCALE);
                int xoffset = (int)(width * offset);
                int startPhr=-1, endPhr=-1;
                for (int i = 0; i < song.notes.Count; i++)
                    if ((song.notes[i].time / HSCALE) > xoffset)
                    { startPhr = i - 1; break; }
                for (int i = 0; i < song.notes.Count; i++)
                    if ((song.notes[i].time / HSCALE) > xoffset + ClientRectangle.Width)
                    { endPhr = i; break; }
                if (startPhr < 0)
                    startPhr = 0;
                Brush bgBrush1 = new SolidBrush(Color.FromArgb(60,0,0)), bgBrush2 = new SolidBrush(Color.FromArgb(60,0,60));
                for (int i = startPhr; i < endPhr; i++)
                {
                    g.FillRectangle(i % 2 == 0 ? bgBrush1 : bgBrush2, new Rectangle((int)(song.notes[i].time / HSCALE) - xoffset, 0, (int)((song.notes[i + 1].time / HSCALE) - (song.notes[i].time / HSCALE)), ClientRectangle.Height));
                }
                for (int i = 1; i < numNotes; i++)
                    g.FillRectangle(hLineBrush, new Rectangle(0, (int)(ClientRectangle.Height * (i / (float)numNotes) + 0.5f), ClientRectangle.Width, 1));
                //Size = new Size((int)(song.bars[song.bars.Length - 1].time * HSCALE),239);
                for (int i = 0; i < song.bars.Length-1; i++)
                {
                    int k = 0;
                    if (mfxValue <= 80)
                        g.FillRectangle(vLineBrush, new Rectangle((int)((song.bars[i].time + (((k + 0.5f) / (float)song.bars[i].numBeats) * (song.bars[i + 1].time - song.bars[i].time))) / HSCALE) - xoffset - 1 + barsub, 0, 2 - (barsub), ClientRectangle.Height));
                    for (k=1; k < song.bars[i].numBeats; k++)
                    {
                        if (mfxValue > 90)
                            continue;
                        g.FillRectangle(vLineBrush, new Rectangle((int)((song.bars[i].time + ((k / (float)song.bars[i].numBeats) * (song.bars[i + 1].time - song.bars[i].time))) / HSCALE) - xoffset - 2 + barsub, 0, 4 - (barsub * 2), ClientRectangle.Height));
                        if (mfxValue > 80)
                            continue;
                        g.FillRectangle(vLineBrush, new Rectangle((int)((song.bars[i].time + (((k + 0.5f) / (float)song.bars[i].numBeats) * (song.bars[i + 1].time - song.bars[i].time))) / HSCALE) - xoffset - 1 + barsub, 0, 2 - (barsub), ClientRectangle.Height));
                    }
                }
                SolidBrush dYellow = new SolidBrush(Color.FromArgb(128,128,0));
                g.FillRectangle(Brushes.Yellow, new Rectangle(-xoffset, 0, 2, ClientRectangle.Height));//first bar
                for (int i = 0; i < song.bars.Length - 1; i++)
                {
                    for (int k = 0; k < song.bars[i].numBeats; k++)
                    {
                        
                        if (mf.resTrackbar.Value >= MainForm.RES_TWO && k%2==0 && song.bars[i].numBeats%2==0)
                            g.FillRectangle(Brushes.Yellow, new Rectangle((int)((song.bars[i].time + ((k / (float)song.bars[i].numBeats) * (song.bars[i + 1].time - song.bars[i].time))) / HSCALE) - xoffset - 2 + barsub, 0, 4 - (int)(Math.Min(barsub, 1) * 2), ClientRectangle.Height));
                        if (mfxValue > 90)
                            continue;
                        if (mf.resTrackbar.Value >= MainForm.RES_FOUR)
                            g.FillRectangle(Brushes.Yellow, new Rectangle((int)((song.bars[i].time + ((k / (float)song.bars[i].numBeats) * (song.bars[i + 1].time - song.bars[i].time))) / HSCALE) - xoffset - 2 + barsub, 0, 4 - (barsub * 2), ClientRectangle.Height));
                        if (mfxValue > 80)
                            continue;
                        if (mf.resTrackbar.Value >= MainForm.RES_EIGHT)
                            g.FillRectangle(dYellow, new Rectangle((int)((song.bars[i].time + (((k + 0.5f) / (float)song.bars[i].numBeats) * (song.bars[i + 1].time - song.bars[i].time))) / HSCALE) - xoffset - 1 + barsub, 0, 2 - (barsub), ClientRectangle.Height));
                    }
                    //bar line
                    g.FillRectangle(Brushes.Orange, new Rectangle((int)(song.bars[i].time / HSCALE) - xoffset - 3 + barsub, 0, 6 - (barsub * 2), ClientRectangle.Height));
                }
                Pen[] lineGreen = {new Pen(Color.FromArgb(255,0,255,0),LineThickness), 
                                   new Pen(Color.FromArgb(255,0,192,0),LineThickness+2),
                                   new Pen(Color.FromArgb(255,0,128,0),LineThickness+4)};
                Pen[] lineYellow = {new Pen(Color.FromArgb(255,255,255,0),LineThickness), 
                                   new Pen(Color.FromArgb(255,192,192,0),LineThickness+2),
                                   new Pen(Color.FromArgb(255,128,128,0),LineThickness+4)};
                    for (int k = 0; k < song.words.Count; k++)
                    {
                        Point pos1 = new Point((int)(song.words[k].time / HSCALE) - xoffset, (int)((1 - ((song.words[k].startNote+1) / (float)numNotes - (0.5f / (float)numNotes))) * (ClientRectangle.Height)));
                        bool overdrive = false;
                        for (int i = 0; i < song.notes.Count - 1; i++)
                            if (song.words[k].time >= song.notes[i].time && song.words[k].time <= song.notes[i + 1].time)
                                if (song.notes[i].overdrive)
                                    overdrive = true;
                        if (song.words[k].connected)
                        {
                            Point pos2 = new Point((int)(song.words[k + 1].time / HSCALE) - xoffset, (int)((1 - ((song.words[k + 1].startNote+1) / (float)numNotes - (0.5f / (float)numNotes))) * (ClientRectangle.Height)));
                            for (int j = 2; j >= 0; j--)
                                g.DrawLine(overdrive ? lineYellow[j] : lineGreen[j], pos1, pos2);
                        }
                        else
                        {
                            Point pos2 = new Point((int)((song.words[k].len) / HSCALE) - xoffset, (int)((1 - ((song.words[k].endNote+1) / (float)numNotes - (0.5f / (float)numNotes))) * (ClientRectangle.Height)));
                            for (int j = 2; j >= 0; j--)
                                g.DrawLine(overdrive ? lineYellow[j] : lineGreen[j], pos1, pos2);
                        }
                    }
                    for(int k=0;k<song.words.Count;k++)
                    {
                        Rectangle rect = new Rectangle((int)(song.words[k].time / HSCALE - (NodeSize / 2)) - xoffset, (int)((1 - ((song.words[k].startNote+1) / (float)numNotes - (0.5f / (float)numNotes))) * (ClientRectangle.Height)) - (NodeSize / 2), NodeSize, NodeSize);
                        if (k == SelectedNode && SelectedNodeBegin)
                            g.FillRectangle(new SolidBrush(NodeSelectColor), rect);
                        else
                            g.FillRectangle(new SolidBrush(NodeColor), rect);
                    }
                    for (int k = 0; k < song.words.Count; k++)
                    {
                        bool sel = k == SelectedNode && !SelectedNodeBegin;
                        if (song.words[k].connected)
                            continue;
                        Rectangle rect = new Rectangle((int)(song.words[k].len / HSCALE - (NodeSize / 2)) - xoffset, (int)((1 - ((song.words[k].endNote+1) / (float)numNotes - (0.5f / (float)numNotes))) * (ClientRectangle.Height)) - (NodeSize / 2), NodeSize, NodeSize);
                        g.DrawRectangle(sel?new Pen(NodeSelectColor):new Pen(NodeColor), rect);
                        if (NodeSize < 8)
                            continue;
                        rect.X++;
                        rect.Y++;
                        rect.Width -= 2;
                        rect.Height -= 2;
                        g.DrawRectangle(sel ? new Pen(NodeSelectColor) : new Pen(NodeColor), rect);
                        if (NodeSize < 12)
                            continue;
                        rect.X++;
                        rect.Y++;
                        rect.Width -= 2;
                        rect.Height -= 2;
                        g.DrawRectangle(sel ? new Pen(NodeSelectColor) : new Pen(NodeColor), rect);
                        if (NodeSize < 16)
                            continue;
                        rect.X++;
                        rect.Y++;
                        rect.Width -= 2;
                        rect.Height -= 2;
                        g.DrawRectangle(sel ? new Pen(NodeSelectColor) : new Pen(NodeColor), rect);
                    }
                }
            

            e.Graphics.DrawImageUnscaled(drawing, 0, 0);
            g.Dispose();
            //base.OnPaint(e);
        }

        public void SetSong(VoxSong s)
        {
            song = s;
            Invalidate();
        }

        private void VocalPreviewer_MouseDown(object sender, MouseEventArgs e)
        {
            float mfxValue = 100 - (mf.xScaleTrackBar.Value / 100f);
            if (mfxValue <= 50)
                HSCALE = (mfxValue / 100f + 0.5f) * (mfxValue / 100f + 0.5f);
            else
                HSCALE = (mfxValue - 49) / 2f + 0.5f;
            mouseClickTime = DateTime.Now.Ticks;
            mouseClickSpot = e.Location;
            int oldSN = SelectedNode;
            int oldSP = SelectedPhrase;
            bool oldSNB = SelectedNodeBegin;
            SelectedNode = -1;
            SelectedPhrase = -1;
            if (mouseDown != 0)
                return;
            if(e.Button== MouseButtons.Left)
                mouseDown = 1;
            if (e.Button == MouseButtons.Middle)
                mouseDown = 2;
            if (e.Button == MouseButtons.Right)
                mouseDown = 4;
            if (song == null)
                return;
            if (!song.valid)
                return;
            if (mf.editingNodesRadio.Checked)
            {
                if (mf.Mode == MainForm.MODE.EDIT)
                {
                    int width = (int)(song.bars[song.bars.Length - 1].time / HSCALE);
                    int xoffset = (int)(width * offset);
                        for (int k = 0; k < song.words.Count; k++)
                        {
                            Rectangle rect = new Rectangle((int)(song.words[k].time / HSCALE - (NodeSize / 2)) - xoffset, (int)((1 - ((song.words[k].startNote + 1) / (float)numNotes - (0.5f / (float)numNotes))) * (ClientRectangle.Height)) - (NodeSize / 2), NodeSize, NodeSize);
                            if (rect.Contains(e.Location))
                            {
                                SelectedNode = k;
                                SelectedNodeBegin = true;
                                oldUndoNote = song.words[SelectedNode].startNote;
                                oldUndoTime = song.words[SelectedNode].time;
                            }
                        }
                    
                        for (int k = 0; k < song.words.Count; k++)
                        {
                            if (song.words[k].connected)
                                continue;
                            Rectangle rect = new Rectangle((int)(song.words[k].len / HSCALE - (NodeSize / 2)) - xoffset, (int)((1 - ((song.words[k].endNote + 1) / (float)numNotes - (0.5f / (float)numNotes))) * (ClientRectangle.Height)) - (NodeSize / 2), NodeSize, NodeSize);
                            if (rect.Contains(e.Location))
                            {
                                SelectedNode = k;
                                SelectedNodeBegin = false;
                                oldUndoNote = song.words[SelectedNode].endNote;
                                oldUndoTime = song.words[SelectedNode].time + song.words[SelectedNode].len;
                            }
                        }
                    if (oldSN != SelectedNode || oldSNB != SelectedNodeBegin || oldSP != SelectedPhrase)
                    {
                        mf.undos.Push(new UndoNoteSelect(this, oldSN, oldSNB, oldSP));
                        mf.redos.Clear();
                    }
                }
                else if (mf.Mode == MainForm.MODE.ADD)
                {
                    int y = (int)(e.Y / (Height / (float)numNotes));
                    y = (numNotes) - y;
                    y--;
                    if (y >= numNotes)
                        y = numNotes - 1;
                    if (y < 0)
                        y = 0;

                    float x = (e.X + ((song.bars[song.bars.Length - 1].time / HSCALE) * offset)) * HSCALE;

                    float div = GetResDiv();
                    int selBar = -1;
                    for (int i = 0; i < song.bars.Length; i++)
                    {
                        if (song.bars[i].time > x)
                        { selBar = i - 1; break; }
                    }
                    div = (div * 4) / song.bars[selBar].numBeats;
                    uint time = 0;
                    for (int i = 0; i <= (1 / div); i++)
                    {
                        if (((song.bars[selBar + 1].time - song.bars[selBar].time) * div * i) + song.bars[selBar].time > x)
                        {
                            if (Math.Abs((((song.bars[selBar + 1].time - song.bars[selBar].time) * div * i) + song.bars[selBar].time) - x) <
                               Math.Abs((((song.bars[selBar + 1].time - song.bars[selBar].time) * div * (i - 1)) + song.bars[selBar].time) - x))
                                time = (uint)(((song.bars[selBar + 1].time - song.bars[selBar].time) * div * i) + song.bars[selBar].time);
                            else
                                time = (uint)(((song.bars[selBar + 1].time - song.bars[selBar].time) * div * (i - 1)) + song.bars[selBar].time);
                            break;
                        }
                    }

                    VoxSong.VocalWord w = new VoxSong.VocalWord();
                    w.connected = false;
                    w.endNote = (short)y;
                    w.len = time;
                    w.startNote = (short)y;
                    w.time = time;
                    w.value = "";
                    song.words.Add(w);
                    SelectedNode = song.words.IndexOf(w);
                    SelectedNodeBegin = false;
                    SelectedPhrase = -1;
                }
                else if (mf.Mode == MainForm.MODE.REMOVE)
                {
                    int width = (int)(song.bars[song.bars.Length - 1].time / HSCALE);
                    int xoffset = (int)(width * offset);
                    for (int k = 0; k < song.words.Count; k++)
                    {
                        Rectangle rect = new Rectangle((int)(song.words[k].time / HSCALE - (NodeSize / 2)) - xoffset, (int)((1 - ((song.words[k].startNote + 1) / (float)numNotes - (0.5f / (float)numNotes))) * (ClientRectangle.Height)) - (NodeSize / 2), NodeSize, NodeSize);
                        if (rect.Contains(e.Location))
                        {
                            mf.undos.Push(new UndoNoteRemove(song, song.words[k]));
                            mf.redos.Clear();
                            song.words.RemoveAt(k);
                        }
                    }
                }
            }
            mf.UpdateActivations();
            Refresh();
        }

        private void VocalPreviewer_MouseUp(object sender, MouseEventArgs e)
        {
            if (mouseDown == 1 && e.Button != MouseButtons.Left)
                return;
            if (mouseDown == 2 && e.Button != MouseButtons.Middle)
                return;
            if (mouseDown == 4 && e.Button != MouseButtons.Right)
                return;

            if (mf.editingNodesRadio.Checked)
            {
                if (SelectedNode >= 0)
                {
                    if (mf.Mode == MainForm.MODE.EDIT)
                    {
                        if (song.words[SelectedNode].time >= song.words[SelectedNode].len)
                        {
                            if (SelectedNodeBegin)
                            {
                                song.words[SelectedNode].time = oldUndoTime;
                                song.words[SelectedNode].startNote = oldUndoNote;
                            }
                            else
                            {
                                song.words[SelectedNode].len = oldUndoTime;
                                song.words[SelectedNode].endNote = oldUndoNote;
                            }
                        }
                        else if (SelectedNodeBegin && song.words[SelectedNode].startNote != oldUndoNote)
                        { mf.undos.Push(new UndoNoteMove(song, SelectedNode, SelectedNodeBegin, oldUndoNote, oldUndoTime)); mf.redos.Clear(); }
                        else if (!SelectedNodeBegin && song.words[SelectedNode].endNote != oldUndoNote)
                        { mf.undos.Push(new UndoNoteMove(song, SelectedNode, SelectedNodeBegin, oldUndoNote, oldUndoTime)); mf.redos.Clear(); }
                    }
                    else if (mf.Mode == MainForm.MODE.ADD)
                    {
                        if (song.words[SelectedNode].time >= song.words[SelectedNode].len)
                        {
                            song.words.RemoveAt(SelectedNode);
                        }
                        else
                        { mf.undos.Push(new UndoNoteCreate(song, song.words[SelectedNode])); mf.redos.Clear(); }
                    }
                }
            }
            mouseDown = 0;
            Refresh();
        }

        private void VocalPreviewer_MouseLeave(object sender, EventArgs e)
        {
            if (mf.editingNodesRadio.Checked)
            {
                if (mouseDown != 0)
                    if (SelectedNode >= 0)
                    {
                        if (SelectedNodeBegin)
                        {
                            song.words[SelectedNode].time = oldUndoTime;
                            song.words[SelectedNode].startNote = oldUndoNote;
                        }
                        else
                        {
                            song.words[SelectedNode].len = oldUndoTime;
                            song.words[SelectedNode].endNote = oldUndoNote;
                        }
                        SelectedNode = -1;
                        SelectedPhrase = -1;
                    }
            }
            mouseDown = 0;
        }

        private void VocalPreviewer_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown!=0)
            {
                if (mf.editingNodesRadio.Checked)
                {
                    if (SelectedNode >= 0)
                    {
                        if (mouseDown == 1 || mouseDown == 2)
                        {
                            int y = (int)(e.Y / (Height / (float)numNotes));
                            y = (numNotes) - y;
                            y--;
                            if (y >= numNotes)
                                y = numNotes - 1;
                            if (y < 0)
                                y = 0;
                            if (SelectedNodeBegin)
                                song.words[SelectedNode].startNote = (short)y;
                            else
                                song.words[SelectedNode].endNote = (short)y;
                        }

                        if (mouseDown == 1 || mouseDown == 4)
                        {
                            float x = (e.X + ((song.bars[song.bars.Length - 1].time / HSCALE) * offset)) * HSCALE;
                            float div = GetResDiv();
                            int selBar = -1;
                            for (int i = 0; i < song.bars.Length; i++)
                            {
                                if (song.bars[i].time > x)
                                { selBar = i - 1; break; }
                            }
                            div = (div * 4) / song.bars[selBar].numBeats;
                            for (int i = 0; i <= (1 / div); i++)
                            {
                                if (((song.bars[selBar + 1].time - song.bars[selBar].time) * div * i) + song.bars[selBar].time > x)
                                {
                                    uint val;
                                    if (Math.Abs((((song.bars[selBar + 1].time - song.bars[selBar].time) * div * i) + song.bars[selBar].time) - x) <
                                       Math.Abs((((song.bars[selBar + 1].time - song.bars[selBar].time) * div * (i - 1)) + song.bars[selBar].time) - x))
                                        val = (uint)(((song.bars[selBar + 1].time - song.bars[selBar].time) * div * i) + song.bars[selBar].time);
                                    else
                                        val = (uint)(((song.bars[selBar + 1].time - song.bars[selBar].time) * div * (i - 1)) + song.bars[selBar].time);

                                    if (SelectedNodeBegin)
                                        song.words[SelectedNode].time = val;
                                    else
                                        song.words[SelectedNode].len = val;
                                    break;
                                }
                            }
                        }
                    }
                }
                else if (mf.editingPhrasesRadio.Checked)
                {
                    //if(mouseDow
                }
            }
        }

        public float GetResDiv()
        {
            switch (mf.resTrackbar.Value)
            {
                case MainForm.RES_ONE:
                    return 1f / 1;
                case MainForm.RES_TWO:
                    return 1f / 2;
                case MainForm.RES_THREE:
                    return 1f / 3;
                case MainForm.RES_FOUR:
                    return 1f / 4;
                case MainForm.RES_SIX:
                    return 1f / 6;
                case MainForm.RES_EIGHT:
                    return 1f / 8;
                case MainForm.RES_TWELVE:
                    return 1f / 12;
                case MainForm.RES_SIXTEEN:
                    return 1f / 16;
                case MainForm.RES_THIRTYTWO:
                    return 1f / 32;
                case MainForm.RES_SIXTYFOUR:
                    return 1f / 64;
            }
            return 0;
        }
    }
}

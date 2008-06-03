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
        Song song;
        MainForm mf;
        public int NodeSize;//4-*, multiple of 2
        public Color NodeSelectColor, NodeColor;
        public int SelectedNode, SelectedNodePhrase, SelectedPhrase;
        public bool SelectedNodeBegin;
        float HSCALE = 0;
        public static int numNotes = 36;
        long mouseClickTime;
        Point mouseClickSpot;
        public int LineThickness=1;
        public float offset;
        bool mouseDown = false;

        public VocalPreviewer()
        {
            InitializeComponent();
            display = false;
            NodeColor = Color.Aqua;
            NodeSelectColor = Color.Lime;
            NodeSize = 8;
            SelectedNodePhrase = -1;
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
                        if (mfxValue > 70)
                            continue;
                        if (mf.resTrackbar.Value >= MainForm.RES_SIXTEEN)
                        {
                            g.FillRectangle(dYellow, new Rectangle((int)((song.bars[i].time + (((k + 0.75f) / (float)song.bars[i].numBeats) * (song.bars[i + 1].time - song.bars[i].time))) / HSCALE) - xoffset - 1 + barsub, 0, 2 - (barsub), ClientRectangle.Height));
                            g.FillRectangle(dYellow, new Rectangle((int)((song.bars[i].time + (((k + 0.25f) / (float)song.bars[i].numBeats) * (song.bars[i + 1].time - song.bars[i].time))) / HSCALE) - xoffset - 1 + barsub, 0, 2 - (barsub), ClientRectangle.Height));
                        }
                        if (mfxValue > 65)
                            continue;
                        if (mf.resTrackbar.Value >= MainForm.RES_THIRTYTWO)
                        {
                            g.FillRectangle(dYellow, new Rectangle((int)((song.bars[i].time + (((k + 0.125f) / (float)song.bars[i].numBeats) * (song.bars[i + 1].time - song.bars[i].time))) / HSCALE) - xoffset, 0, 1, ClientRectangle.Height));
                            g.FillRectangle(dYellow, new Rectangle((int)((song.bars[i].time + (((k + 0.375f) / (float)song.bars[i].numBeats) * (song.bars[i + 1].time - song.bars[i].time))) / HSCALE) - xoffset, 0, 1, ClientRectangle.Height));
                            g.FillRectangle(dYellow, new Rectangle((int)((song.bars[i].time + (((k + 0.625f) / (float)song.bars[i].numBeats) * (song.bars[i + 1].time - song.bars[i].time))) / HSCALE) - xoffset, 0, 1, ClientRectangle.Height));
                            g.FillRectangle(dYellow, new Rectangle((int)((song.bars[i].time + (((k + 0.875f) / (float)song.bars[i].numBeats) * (song.bars[i + 1].time - song.bars[i].time))) / HSCALE) - xoffset, 0, 1, ClientRectangle.Height));
                        }
                        if (mfxValue > 60)
                            continue;
                        if (mf.resTrackbar.Value >= MainForm.RES_SIXTYFOUR)
                        {
                            g.FillRectangle(dYellow, new Rectangle((int)((song.bars[i].time + (((k + 0.125f + 0.0625f) / (float)song.bars[i].numBeats) * (song.bars[i + 1].time - song.bars[i].time))) / HSCALE) - xoffset, 0, 1, ClientRectangle.Height));
                            g.FillRectangle(dYellow, new Rectangle((int)((song.bars[i].time + (((k + 0.375f + 0.0625f) / (float)song.bars[i].numBeats) * (song.bars[i + 1].time - song.bars[i].time))) / HSCALE) - xoffset, 0, 1, ClientRectangle.Height));
                            g.FillRectangle(dYellow, new Rectangle((int)((song.bars[i].time + (((k + 0.625f + 0.0625f) / (float)song.bars[i].numBeats) * (song.bars[i + 1].time - song.bars[i].time))) / HSCALE) - xoffset, 0, 1, ClientRectangle.Height));
                            g.FillRectangle(dYellow, new Rectangle((int)((song.bars[i].time + (((k + 0.875f + 0.0625f) / (float)song.bars[i].numBeats) * (song.bars[i + 1].time - song.bars[i].time))) / HSCALE) - xoffset, 0, 1, ClientRectangle.Height));
                            g.FillRectangle(dYellow, new Rectangle((int)((song.bars[i].time + (((k + 0.125f - 0.0625f) / (float)song.bars[i].numBeats) * (song.bars[i + 1].time - song.bars[i].time))) / HSCALE) - xoffset, 0, 1, ClientRectangle.Height));
                            g.FillRectangle(dYellow, new Rectangle((int)((song.bars[i].time + (((k + 0.375f - 0.0625f) / (float)song.bars[i].numBeats) * (song.bars[i + 1].time - song.bars[i].time))) / HSCALE) - xoffset, 0, 1, ClientRectangle.Height));
                            g.FillRectangle(dYellow, new Rectangle((int)((song.bars[i].time + (((k + 0.625f - 0.0625f) / (float)song.bars[i].numBeats) * (song.bars[i + 1].time - song.bars[i].time))) / HSCALE) - xoffset, 0, 1, ClientRectangle.Height));
                            g.FillRectangle(dYellow, new Rectangle((int)((song.bars[i].time + (((k + 0.875f - 0.0625f) / (float)song.bars[i].numBeats) * (song.bars[i + 1].time - song.bars[i].time))) / HSCALE) - xoffset, 0, 1, ClientRectangle.Height));
                        }
                    }
                    g.FillRectangle(Brushes.Yellow, new Rectangle((int)(song.bars[i].time / HSCALE) - xoffset - 3 + barsub, 0, 6 - (barsub * 2), ClientRectangle.Height));
                }
                Pen[] lineGreen = {new Pen(Color.FromArgb(255,0,255,0),LineThickness), 
                                   new Pen(Color.FromArgb(255,0,192,0),LineThickness+2),
                                   new Pen(Color.FromArgb(255,0,128,0),LineThickness+4)};
                Pen[] lineYellow = {new Pen(Color.FromArgb(255,255,255,0),LineThickness), 
                                   new Pen(Color.FromArgb(255,192,192,0),LineThickness+2),
                                   new Pen(Color.FromArgb(255,128,128,0),LineThickness+4)};
                for (int i = 0; i < song.notes.Count; i++)
                {
                    for (int k = 0; k < song.notes[i].words.Count; k++)
                    {
                        Point pos1 = new Point((int)(song.notes[i].words[k].time / HSCALE) - xoffset, (int)((1 - ((song.notes[i].words[k].startNote+1) / (float)numNotes - (0.5f / (float)numNotes))) * (ClientRectangle.Height)));
                        if (song.notes[i].words[k].connected)
                        {
                            Point pos2 = new Point((int)(song.notes[i].words[k + 1].time / HSCALE) - xoffset, (int)((1 - ((song.notes[i].words[k + 1].startNote+1) / (float)numNotes - (0.5f / (float)numNotes))) * (ClientRectangle.Height)));
                            for (int j = 2; j >= 0; j--)
                                g.DrawLine(song.notes[i].overdrive ? lineYellow[j] : lineGreen[j], pos1, pos2);
                        }
                        else
                        {
                            Point pos2 = new Point((int)((song.notes[i].words[k].len) / HSCALE) - xoffset, (int)((1 - ((song.notes[i].words[k].endNote+1) / (float)numNotes - (0.5f / (float)numNotes))) * (ClientRectangle.Height)));
                            for (int j = 2; j >= 0; j--)
                                g.DrawLine(song.notes[i].overdrive ? lineYellow[j] : lineGreen[j], pos1, pos2);
                        }
                    }
                }
                for (int i = 0; i < song.notes.Count; i++)
                {
                    for(int k=0;k<song.notes[i].words.Count;k++)
                    {
                        Rectangle rect = new Rectangle((int)(song.notes[i].words[k].time / HSCALE - (NodeSize / 2)) - xoffset, (int)((1 - ((song.notes[i].words[k].startNote+1) / (float)numNotes - (0.5f / (float)numNotes))) * (ClientRectangle.Height)) - (NodeSize / 2), NodeSize, NodeSize);
                        if (i == SelectedNodePhrase && k == SelectedNode && SelectedNodeBegin)
                            g.FillRectangle(new SolidBrush(NodeSelectColor), rect);
                        else
                            g.FillRectangle(new SolidBrush(NodeColor), rect);
                    }
                }
                for (int i = 0; i < song.notes.Count; i++)
                {
                    for (int k = 0; k < song.notes[i].words.Count; k++)
                    {
                        bool sel = i == SelectedNodePhrase && k == SelectedNode && !SelectedNodeBegin;
                        if (song.notes[i].words[k].connected)
                            continue;
                        Rectangle rect = new Rectangle((int)(song.notes[i].words[k].len / HSCALE - (NodeSize / 2)) - xoffset, (int)((1 - ((song.notes[i].words[k].endNote+1) / (float)numNotes - (0.5f / (float)numNotes))) * (ClientRectangle.Height)) - (NodeSize / 2), NodeSize, NodeSize);
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
            }

            e.Graphics.DrawImageUnscaled(drawing, 0, 0);
            g.Dispose();
            //base.OnPaint(e);
        }

        public void SetSong(Song s)
        {
            song = s;
            Invalidate();
        }

        private void VocalPreviewer_MouseDown(object sender, MouseEventArgs e)
        {
            mouseClickTime = DateTime.Now.Ticks;
            mouseClickSpot = e.Location;
            SelectedNodePhrase = -1;
            SelectedNode = -1;
            SelectedPhrase = -1;
            mouseDown = true;
            int width = (int)(song.bars[song.bars.Length - 1].time / HSCALE);
            int xoffset = (int)(width * offset);
            if (song != null && song.valid)
            for (int i = 0; i < song.notes.Count; i++)
            {
                for (int k = 0; k < song.notes[i].words.Count; k++)
                {
                    Rectangle rect = new Rectangle((int)(song.notes[i].words[k].time / HSCALE - (NodeSize / 2))-xoffset, (int)((1 - ((song.notes[i].words[k].startNote+1) / (float)numNotes - (0.5f / (float)numNotes))) * (ClientRectangle.Height)) - (NodeSize / 2), NodeSize, NodeSize);
                    if (rect.Contains(e.Location))
                    {
                        SelectedNodePhrase = i;
                        SelectedNode = k;
                        SelectedNodeBegin = true;
                    }
                }
            }
            for (int i = 0; i < song.notes.Count; i++)
            {
                for (int k = 0; k < song.notes[i].words.Count; k++)
                {
                    if (song.notes[i].words[k].connected)
                        continue;
                    Rectangle rect = new Rectangle((int)(song.notes[i].words[k].len / HSCALE - (NodeSize / 2)) - xoffset, (int)((1 - ((song.notes[i].words[k].endNote + 1) / (float)numNotes - (0.5f / (float)numNotes))) * (ClientRectangle.Height)) - (NodeSize / 2), NodeSize, NodeSize);
                    if (rect.Contains(e.Location))
                    {
                        SelectedNodePhrase = i;
                        SelectedNode = k;
                        SelectedNodeBegin = false;
                    }
                }
            }
            mf.UpdateActivations();
            Refresh();
        }

        private void VocalPreviewer_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
            Refresh();
        }

        private void VocalPreviewer_MouseLeave(object sender, EventArgs e)
        {
            mouseDown = false;
        }

        private void VocalPreviewer_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                if (SelectedNode >= 0)
                {
                    int y = (int)(e.Y / (Height / (float)numNotes));
                    y = (numNotes) - y;
                    y--;
                    if (y >= numNotes)
                        y = numNotes - 1;
                    if (y < 0)
                        y = 0;
                    if(SelectedNodeBegin)
                        song.notes[SelectedNodePhrase].words[SelectedNode].startNote = (short)y;
                    else
                        song.notes[SelectedNodePhrase].words[SelectedNode].endNote = (short)y;
                }
            }
        }
    }
}

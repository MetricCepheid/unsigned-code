using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace UnsignedAnimationEditor
{
    public partial class KeyframeTrackBar : UserControl
    {
        public event EventHandler ValueChanged;

        private int _value=0, _min=0, _max=100;
        private int _tickF = 1;
        public int Minimum 
        {
            get
            {
                return _min;
            }
            set
            {
                if (value < _max)
                    _min = value;
                else
                    throw new Exception("Minimum value must be less than Maximum");
            }
        }
        public int Maximum
        {
            get
            {
                return _max;
            }
            set
            {
                if (value > _min)
                    _max = value;
                else
                    throw new Exception("Maximum value must be more than Minimum");
            }
        }
        public int Value 
        {
            get
            {
                return _value;
            }
            set
            {
                if (value >= _min && value <= _max)
                    _value = value;
                else
                    throw new Exception("Value must be between Minimum and Minimum");
            }
        }

        private bool grabbed;

        public int TickFrequency 
        {
            get 
            {
                return _tickF;
            }
            set
            {
                if(_tickF>0)
                    _tickF = value;
            }
        }

        public bool OnKeyframe { get { return Keyframes.Contains(Value); } }

        public List<int> Keyframes { get; set; }

        public KeyframeTrackBar()
        {
            grabbed = false;
            Keyframes = new List<int>();
            InitializeComponent();
        }

        public void AddKeyframe()
        {
            if (!Keyframes.Contains(Value))
                Keyframes.Add(Value);
            Invalidate();
        }

        public void RemoveKeyframe()
        {
            if (Keyframes.Contains(Value))
                Keyframes.Remove(Value);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (Enabled)
            {
                e.Graphics.DrawLine(Pens.Black, new Point(0, Bounds.Height / 2), new Point(Bounds.Width, Bounds.Height / 2));
                int w = (Bounds.Width / 20) - ((Bounds.Width / 20) % 2);
                int h = (Bounds.Height * 2 / 3) - ((Bounds.Height * 2 / 3) % 2);
                int x1 = w / 2;
                int x2 = Bounds.Width - (w / 2);
                for (int i = Minimum; i < Maximum; i += TickFrequency)
                {
                    int xx = (int)(x1 + ((x2 - x1) * ((i - Minimum) / (float)(Maximum - Minimum))));
                    e.Graphics.DrawLine(Pens.Black, new Point(xx, Bounds.Height / 2 - 1), new Point(xx, Bounds.Height / 2 + 1));
                }
                for (int i = 0; i < Keyframes.Count; i++)
                {
                    int xx = (int)(x1 + ((x2 - x1) * ((Keyframes[i] - Minimum) / (float)(Maximum - Minimum))));
                    e.Graphics.FillRectangle(Brushes.Red, new Rectangle(xx - 2, Bounds.Height / 2 - 2, 5, 5));
                }
                int x = (int)(x1 + ((x2 - x1) * ((Value - Minimum) / (float)(Maximum - Minimum))));
                e.Graphics.FillRectangle(Keyframes.Contains(Value) ? Brushes.Red : Brushes.DimGray, new Rectangle(x - (w / 2), (Bounds.Height - h) / 2, w, h));
                e.Graphics.DrawRectangle(Pens.Black, new Rectangle(x - (w / 2), (Bounds.Height - h) / 2, w, h));
            }
            else
            {
                e.Graphics.DrawLine(Pens.DimGray, new Point(0, Bounds.Height / 2), new Point(Bounds.Width, Bounds.Height / 2));
                int w = (Bounds.Width / 20) - ((Bounds.Width / 20) % 2);
                int h = (Bounds.Height * 2 / 3) - ((Bounds.Height * 2 / 3) % 2);
                int x1 = w / 2;
                int x2 = Bounds.Width - (w / 2);
                for (int i = Minimum; i < Maximum; i += TickFrequency)
                {
                    int xx = (int)(x1 + ((x2 - x1) * ((i - Minimum) / (float)(Maximum - Minimum))));
                    e.Graphics.DrawLine(Pens.DimGray, new Point(xx, Bounds.Height / 2 - 1), new Point(xx, Bounds.Height / 2 + 1));
                }
                for (int i = 0; i < Keyframes.Count; i++)
                {
                    int xx = (int)(x1 + ((x2 - x1) * ((Keyframes[i] - Minimum) / (float)(Maximum - Minimum))));
                    e.Graphics.FillRectangle(Brushes.DarkRed, new Rectangle(xx - 2, Bounds.Height / 2 - 2, 5, 5));
                }
                int x = (int)(x1 + ((x2 - x1) * ((Value - Minimum) / (float)(Maximum - Minimum))));
                e.Graphics.FillRectangle(Keyframes.Contains(Value) ? Brushes.DarkRed : Brushes.Gray, new Rectangle(x - (w / 2), (Bounds.Height - h) / 2, w, h));
                e.Graphics.DrawRectangle(Pens.DimGray, new Rectangle(x - (w / 2), (Bounds.Height - h) / 2, w, h));
            }
            base.OnPaint(e);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
        }

        private void KeyframeTrackBar_MouseDown(object sender, MouseEventArgs e)
        {
            int w = (Bounds.Width / 20) - ((Bounds.Width / 20) % 2);
            int h = (Bounds.Height * 2 / 3) - ((Bounds.Height * 2 / 3) % 2);
            int x1 = w / 2;
            int x2 = Bounds.Width - (w / 2);
            int x = (int)(x1 + ((x2 - x1) * ((Value - Minimum) / (float)(Maximum - Minimum))));
            Rectangle r = new Rectangle(x - (w / 2), (Bounds.Height - h) / 2, w, h);
            if (r.Contains(e.X, e.Y))
            {
                grabbed = true;
            }
        }

        private void KeyframeTrackBar_MouseUp(object sender, MouseEventArgs e)
        {
            grabbed = false;
        }

        private void KeyframeTrackBar_MouseLeave(object sender, EventArgs e)
        {
            grabbed = false;
        }

        private void KeyframeTrackBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (grabbed)
            {
                int w = (Bounds.Width / 20) - ((Bounds.Width / 20) % 2);
                int x1 = w / 2;
                int x2 = Bounds.Width - (w / 2);
                float val = (e.X - x1) / (float)(x2 - x1);
                if (val < 0)
                    val = 0;
                if (val > 1)
                    val = 1;
                int oldVal = Value;
                Value = Minimum + (int)((Maximum - Minimum) * val);
                if (oldVal != Value)
                {
                    Invalidate();
                    if (ValueChanged != null)
                        ValueChanged.Invoke(this, new EventArgs());
                }
            }
        }
    }
}

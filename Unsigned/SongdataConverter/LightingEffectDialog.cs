using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SongDataIO;

namespace SongdataConverter
{
    public partial class LightingEffectDialog : Form
    {
        private const int NORMAL = 0, GRADIENT = 1, STROBE = 2;

        private SongData.SpecialEffect _ef;
        public SongData.SpecialEffect Effect { get { return _ef; } set { _ef = value; EffectChanged(); } }

        public LightingEffectDialog()
        {
            InitializeComponent();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void colorButton1_Click(object sender, EventArgs e)
        {
            ColorDialog d = new ColorDialog();
            if (Effect is SongData.NormalLightingSpecialEffect)
            {
                SongData.Color c1 = ((SongData.NormalLightingSpecialEffect)Effect).color;
                d.Color = Color.FromArgb(c1.R, c1.G, c1.B);
            }
            else if (Effect is SongData.GradientLightingSpecialEffect)
            {
                SongData.Color c1 = ((SongData.GradientLightingSpecialEffect)Effect).color1;
                SongData.Color c2 = ((SongData.GradientLightingSpecialEffect)Effect).color2;
                d.Color = Color.FromArgb(c1.R, c1.G, c1.B);
            }
            else if (Effect is SongData.StrobeLightingSpecialEffect)
            {
                SongData.Color c1 = ((SongData.StrobeLightingSpecialEffect)Effect).color1;
                SongData.Color c2 = ((SongData.StrobeLightingSpecialEffect)Effect).color2;
                d.Color = Color.FromArgb(c1.R, c1.G, c1.B);
            }
            if (d.ShowDialog() == DialogResult.OK)
            {
                if (Effect is SongData.NormalLightingSpecialEffect)
                {
                    ((SongData.NormalLightingSpecialEffect)Effect).color = new SongData.Color(d.Color.R,d.Color.G,d.Color.B);
                }
                else if (Effect is SongData.GradientLightingSpecialEffect)
                {
                    ((SongData.GradientLightingSpecialEffect)Effect).color1 = new SongData.Color(d.Color.R, d.Color.G, d.Color.B);
                }
                else if (Effect is SongData.StrobeLightingSpecialEffect)
                {
                    ((SongData.StrobeLightingSpecialEffect)Effect).color1 = new SongData.Color(d.Color.R, d.Color.G, d.Color.B);
                }
                colorPanel1.BackColor = d.Color;
            }
        }

        private void colorButton2_Click(object sender, EventArgs e)
        {
            ColorDialog d = new ColorDialog();
            if (Effect is SongData.GradientLightingSpecialEffect)
            {
                SongData.Color c1 = ((SongData.GradientLightingSpecialEffect)Effect).color1;
                SongData.Color c2 = ((SongData.GradientLightingSpecialEffect)Effect).color2;
                d.Color = Color.FromArgb(c2.R, c2.G, c2.B);
            }
            else if (Effect is SongData.StrobeLightingSpecialEffect)
            {
                SongData.Color c1 = ((SongData.StrobeLightingSpecialEffect)Effect).color1;
                SongData.Color c2 = ((SongData.StrobeLightingSpecialEffect)Effect).color2;
                d.Color = Color.FromArgb(c2.R, c2.G, c2.B);
            }
            if (d.ShowDialog() == DialogResult.OK)
            {
                if (Effect is SongData.GradientLightingSpecialEffect)
                {
                    ((SongData.GradientLightingSpecialEffect)Effect).color2 = new SongData.Color(d.Color.R, d.Color.G, d.Color.B);
                }
                else if (Effect is SongData.StrobeLightingSpecialEffect)
                {
                    ((SongData.StrobeLightingSpecialEffect)Effect).color2 = new SongData.Color(d.Color.R, d.Color.G, d.Color.B);
                }
                colorPanel2.BackColor = d.Color;
            }
        }

        private bool cbInUse;
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!cbInUse)
            {
                cbInUse = true;
                if (Effect is SongData.NormalLightingSpecialEffect)
                {
                    if (comboBox1.SelectedIndex == GRADIENT)
                        Effect = new SongData.GradientLightingSpecialEffect(Effect.time, Effect.length, ((SongData.NormalLightingSpecialEffect)Effect).color, ((SongData.NormalLightingSpecialEffect)Effect).color);
                    else if (comboBox1.SelectedIndex == STROBE)
                        Effect = new SongData.StrobeLightingSpecialEffect(Effect.time, Effect.length, ((SongData.NormalLightingSpecialEffect)Effect).color, ((SongData.NormalLightingSpecialEffect)Effect).color, 1);
                }
                else if (Effect is SongData.GradientLightingSpecialEffect)
                {
                    if (comboBox1.SelectedIndex == NORMAL)
                        Effect = new SongData.NormalLightingSpecialEffect(Effect.time, Effect.length, ((SongData.GradientLightingSpecialEffect)Effect).color1);
                    else if (comboBox1.SelectedIndex == STROBE)
                        Effect = new SongData.StrobeLightingSpecialEffect(Effect.time, Effect.length, ((SongData.GradientLightingSpecialEffect)Effect).color1, ((SongData.GradientLightingSpecialEffect)Effect).color2, 1);
                }
                else if (Effect is SongData.StrobeLightingSpecialEffect)
                {
                    if (comboBox1.SelectedIndex == NORMAL)
                        Effect = new SongData.NormalLightingSpecialEffect(Effect.time, Effect.length, ((SongData.StrobeLightingSpecialEffect)Effect).color1);
                    else if (comboBox1.SelectedIndex == GRADIENT)
                        Effect = new SongData.GradientLightingSpecialEffect(Effect.time, Effect.length, ((SongData.StrobeLightingSpecialEffect)Effect).color1, ((SongData.StrobeLightingSpecialEffect)Effect).color2);
                }
            }
            cbInUse = false;
        }

        private void EffectChanged()
        {
            cbInUse = true;
            if (Effect is SongData.NormalLightingSpecialEffect)
            {
                comboBox1.SelectedIndex = 0;
                colorButton2.Enabled = false;
                frequencyNumeric.Enabled = false;
                SongData.Color c1 = ((SongData.NormalLightingSpecialEffect)Effect).color;
                colorPanel1.BackColor = Color.FromArgb(c1.R,c1.G,c1.B);
                colorPanel2.BackColor = Color.WhiteSmoke;
            }
            else if (Effect is SongData.GradientLightingSpecialEffect)
            {
                comboBox1.SelectedIndex = 1;
                colorButton2.Enabled = true;
                frequencyNumeric.Enabled = false;
                SongData.Color c1 = ((SongData.GradientLightingSpecialEffect)Effect).color1;
                SongData.Color c2 = ((SongData.GradientLightingSpecialEffect)Effect).color2;
                colorPanel1.BackColor = Color.FromArgb(c1.R, c1.G, c1.B);
                colorPanel2.BackColor = Color.FromArgb(c2.R, c2.G, c2.B);
            }
            else if (Effect is SongData.StrobeLightingSpecialEffect)
            {
                comboBox1.SelectedIndex = 2;
                colorButton2.Enabled = true;
                frequencyNumeric.Enabled = true;
                frequencyNumeric.Value = (decimal)((SongData.StrobeLightingSpecialEffect)Effect).frequency;
                SongData.Color c1 = ((SongData.StrobeLightingSpecialEffect)Effect).color1;
                SongData.Color c2 = ((SongData.StrobeLightingSpecialEffect)Effect).color2;
                colorPanel1.BackColor = Color.FromArgb(c1.R, c1.G, c1.B);
                colorPanel2.BackColor = Color.FromArgb(c2.R, c2.G, c2.B);
            }
            cbInUse = false;
        }

        private void frequencyNumeric_ValueChanged(object sender, EventArgs e)
        {
            if (Effect is SongData.StrobeLightingSpecialEffect)
            {
                (Effect as SongData.StrobeLightingSpecialEffect).frequency = (float)frequencyNumeric.Value;
            }
        }
    }
}

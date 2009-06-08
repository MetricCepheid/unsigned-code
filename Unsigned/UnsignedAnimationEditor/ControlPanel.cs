using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FVProductions.Utility;
using Unsigned;
using Microsoft.Xna.Framework;

namespace UnsignedAnimationEditor
{
    public partial class ControlPanel : UserControl
    {
        public AnimationInfo AnimationInfo;

        public ControlPanel()
        {
            InitializeComponent();
            comboBox1_SelectedIndexChanged(null, null);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            keyframeTrackBar1.AddKeyframe();
            AnimationInfo.CurrentAnimation.AddKeyframe(keyframeTrackBar1.Value*2);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            keyframeTrackBar1.RemoveKeyframe();
            AnimationInfo.CurrentAnimation.RemoveKeyframe(keyframeTrackBar1.Value * 2);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            InputDialog d = new InputDialog();
            d.Question = "New Animation: Name?";
            d.Text = "New Animation";
            if (d.ShowDialog() == DialogResult.OK)
            {
                AnimationInfo.Animations.Add(new Animation(d.Answer, AnimationInfo.Skeleton.GetMatrixLength()));
                ResetComboBox();
            }
        }

        private void ResetComboBox()
        {
            animComboBox.SelectedIndex = -1;
            animComboBox.Items.Clear();
            for (int i = 0; i < AnimationInfo.Animations.Count; i++)
            {
                animComboBox.Items.Add(AnimationInfo.Animations[i]);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (AnimationInfo != null)
            {
                if (animComboBox.SelectedIndex < 0)
                    AnimationInfo.AnimationIndex = -1;
                else
                    for (int i = 0; i < AnimationInfo.Animations.Count; i++)
                        if (AnimationInfo.Animations[i] == animComboBox.Items[animComboBox.SelectedIndex])
                            AnimationInfo.AnimationIndex = i;
            }
            frameComboBox.Items.Clear();
            frameComboBox.SelectedIndex = -1;
            if (AnimationInfo!=null && AnimationInfo.CurrentAnimation != null)
            {
                String[] arr = AnimationInfo.Skeleton.GetMatrixNames();
                for (int i = 0; i < arr.Length; i++)
                    frameComboBox.Items.Add(arr[i]);
                keyframeTrackBar1.Keyframes.Clear();
                for (int i = 0; i < AnimationInfo.CurrentAnimation.KeyframeCount; i++)
                    keyframeTrackBar1.Keyframes.Add(AnimationInfo.CurrentAnimation.Keyframe(i).Time/2);
                frameComboBox.Enabled = true;
                keyframeTrackBar1.Enabled = true;
            }
            else
            {
                frameComboBox.Enabled = false;
                keyframeTrackBar1.Enabled = false;
            }
        }

        private void keyframeTrackBar1_ValueChanged(object sender, EventArgs e)
        {
            AnimationInfo.CurrentAnimationTimeValue = keyframeTrackBar1.Value * 2;
            if (AnimationInfo.CurrentKeyframe != null)
            {
                xOffsetNumeric.Value = (decimal)AnimationInfo.CurrentKeyframe.Offset.X;
                yOffsetNumeric.Value = (decimal)AnimationInfo.CurrentKeyframe.Offset.Y;
                zOffsetNumeric.Value = (decimal)AnimationInfo.CurrentKeyframe.Offset.Z;
            }
            else
            {
                xOffsetNumeric.Value = 0;
                yOffsetNumeric.Value = 0;
                zOffsetNumeric.Value = 0;
            }
        }

        private void frameComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            AnimationInfo.SelectedJointIndex = frameComboBox.SelectedIndex;
        }

        public void AnimationInfoChanged()
        {
            ResetComboBox();
            keyframeTrackBar1.Value = AnimationInfo.CurrentAnimationTimeValue / 2;
            comboBox1_SelectedIndexChanged(this, new EventArgs());
        }

        private void xOffsetNumeric_ValueChanged(object sender, EventArgs e)
        {
            if (AnimationInfo != null)
            {
                if (AnimationInfo.CurrentKeyframe != null)
                {
                    AnimationInfo.CurrentKeyframe.Offset.X = (float)xOffsetNumeric.Value;
                }
            }
        }

        private void yOffsetNumeric_ValueChanged(object sender, EventArgs e)
        {
            if (AnimationInfo != null)
            {
                if (AnimationInfo.CurrentKeyframe != null)
                {
                    AnimationInfo.CurrentKeyframe.Offset.Y = (float)yOffsetNumeric.Value;
                }
            }
        }

        private void zOffsetNumeric_ValueChanged(object sender, EventArgs e)
        {
            if (AnimationInfo != null)
            {
                if (AnimationInfo.CurrentKeyframe != null)
                {
                    AnimationInfo.CurrentKeyframe.Offset.Z = (float)zOffsetNumeric.Value;
                }
            }
        }

        private void copyKeyframeButton_Click(object sender, EventArgs e)
        {
            if (AnimationInfo != null)
            {
                if (AnimationInfo.CurrentAnimation != null)
                {
                    CopyKeyframeForm f = new CopyKeyframeForm();
                    DialogResult dr = f.ShowDialog();
                    if (dr == DialogResult.OK)
                    {
                        Frame fr = AnimationInfo.CurrentAnimation.GenerateNewFrame(AnimationInfo.CurrentAnimationTimeValue);
                        fr.Time = f.TimeValue;
                        keyframeTrackBar1.AddKeyframe(fr.Time/2);
                        AnimationInfo.CurrentAnimation.AddKeyframe(fr);
                    }
                }
            }
        }

        private void resetJointButton_Click_1(object sender, EventArgs e)
        {
            if (AnimationInfo != null)
            {
                if (AnimationInfo.CurrentKeyframe != null)
                {
                    AnimationInfo.CurrentKeyframe.Matrices[AnimationInfo.SelectedJointIndex] = Matrix.Identity;
                }
            }
        }
    }
}

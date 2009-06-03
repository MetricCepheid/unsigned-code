using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FVProductions.Utility;

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
        }

        private void button3_Click(object sender, EventArgs e)
        {
            InputDialog d = new InputDialog();
            d.Question = "New Keyframe: Name?";
            d.Text = "New Keyframe";
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
            if(AnimationInfo!=null)
                AnimationInfo.AnimationIndex = animComboBox.SelectedIndex;
            frameComboBox.Items.Clear();
            frameComboBox.SelectedIndex = -1;
            if (AnimationInfo!=null && AnimationInfo.CurrentAnimation != null)
            {
                String[] arr = AnimationInfo.Skeleton.GetMatrixNames();
                for (int i = 0; i < arr.Length; i++)
                    frameComboBox.Items.Add(arr[i]);
                keyframeTrackBar1.Keyframes.Clear();
                for (int i = 0; i < AnimationInfo.CurrentAnimation.KeyframeCount; i++)
                    keyframeTrackBar1.Keyframes.Add((int)(AnimationInfo.CurrentAnimation.Keyframe(i).Time*(keyframeTrackBar1.Maximum-keyframeTrackBar1.Minimum))+keyframeTrackBar1.Minimum);
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
        }

        private void frameComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            AnimationInfo.SelectedJointIndex = frameComboBox.SelectedIndex;
        }
    }
}

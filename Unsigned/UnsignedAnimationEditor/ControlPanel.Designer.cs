namespace UnsignedAnimationEditor
{
    partial class ControlPanel
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ControlPanel));
            this.animComboBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.addKeyframeButton = new System.Windows.Forms.Button();
            this.rmvKeyframeButton = new System.Windows.Forms.Button();
            this.newAnimButton = new System.Windows.Forms.Button();
            this.frameComboBox = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.xOffsetNumeric = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.yOffsetNumeric = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.zOffsetNumeric = new System.Windows.Forms.NumericUpDown();
            this.keyframeTrackBar1 = new UnsignedAnimationEditor.KeyframeTrackBar();
            ((System.ComponentModel.ISupportInitialize)(this.xOffsetNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.yOffsetNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.zOffsetNumeric)).BeginInit();
            this.SuspendLayout();
            // 
            // animComboBox
            // 
            this.animComboBox.FormattingEnabled = true;
            this.animComboBox.Location = new System.Drawing.Point(3, 24);
            this.animComboBox.MaxDropDownItems = 64;
            this.animComboBox.Name = "animComboBox";
            this.animComboBox.Size = new System.Drawing.Size(168, 24);
            this.animComboBox.Sorted = true;
            this.animComboBox.TabIndex = 0;
            this.animComboBox.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(133, 17);
            this.label1.TabIndex = 1;
            this.label1.Text = "Selected Animation:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 135);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(128, 17);
            this.label2.TabIndex = 2;
            this.label2.Text = "Animation Position:";
            // 
            // addKeyframeButton
            // 
            this.addKeyframeButton.Location = new System.Drawing.Point(4, 189);
            this.addKeyframeButton.Name = "addKeyframeButton";
            this.addKeyframeButton.Size = new System.Drawing.Size(111, 23);
            this.addKeyframeButton.TabIndex = 4;
            this.addKeyframeButton.Text = "Add Keyframe";
            this.addKeyframeButton.UseVisualStyleBackColor = true;
            this.addKeyframeButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // rmvKeyframeButton
            // 
            this.rmvKeyframeButton.Location = new System.Drawing.Point(122, 188);
            this.rmvKeyframeButton.Name = "rmvKeyframeButton";
            this.rmvKeyframeButton.Size = new System.Drawing.Size(49, 23);
            this.rmvKeyframeButton.TabIndex = 5;
            this.rmvKeyframeButton.Text = "Rmv";
            this.rmvKeyframeButton.UseVisualStyleBackColor = true;
            this.rmvKeyframeButton.Click += new System.EventHandler(this.button2_Click);
            // 
            // newAnimButton
            // 
            this.newAnimButton.Location = new System.Drawing.Point(0, 54);
            this.newAnimButton.Name = "newAnimButton";
            this.newAnimButton.Size = new System.Drawing.Size(171, 23);
            this.newAnimButton.TabIndex = 6;
            this.newAnimButton.Text = "Add New Animation";
            this.newAnimButton.UseVisualStyleBackColor = true;
            this.newAnimButton.Click += new System.EventHandler(this.button3_Click);
            // 
            // frameComboBox
            // 
            this.frameComboBox.FormattingEnabled = true;
            this.frameComboBox.Location = new System.Drawing.Point(4, 238);
            this.frameComboBox.Name = "frameComboBox";
            this.frameComboBox.Size = new System.Drawing.Size(164, 24);
            this.frameComboBox.TabIndex = 7;
            this.frameComboBox.SelectedIndexChanged += new System.EventHandler(this.frameComboBox_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 215);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(42, 17);
            this.label3.TabIndex = 8;
            this.label3.Text = "Joint:";
            // 
            // xOffsetNumeric
            // 
            this.xOffsetNumeric.DecimalPlaces = 2;
            this.xOffsetNumeric.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.xOffsetNumeric.Location = new System.Drawing.Point(4, 289);
            this.xOffsetNumeric.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.xOffsetNumeric.Name = "xOffsetNumeric";
            this.xOffsetNumeric.Size = new System.Drawing.Size(120, 22);
            this.xOffsetNumeric.TabIndex = 9;
            this.xOffsetNumeric.ValueChanged += new System.EventHandler(this.xOffsetNumeric_ValueChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(4, 269);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(64, 17);
            this.label4.TabIndex = 10;
            this.label4.Text = "X-Offset:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(4, 314);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(64, 17);
            this.label5.TabIndex = 12;
            this.label5.Text = "Y-Offset:";
            // 
            // yOffsetNumeric
            // 
            this.yOffsetNumeric.DecimalPlaces = 2;
            this.yOffsetNumeric.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.yOffsetNumeric.Location = new System.Drawing.Point(4, 334);
            this.yOffsetNumeric.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.yOffsetNumeric.Name = "yOffsetNumeric";
            this.yOffsetNumeric.Size = new System.Drawing.Size(120, 22);
            this.yOffsetNumeric.TabIndex = 11;
            this.yOffsetNumeric.ValueChanged += new System.EventHandler(this.yOffsetNumeric_ValueChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(4, 359);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(64, 17);
            this.label6.TabIndex = 14;
            this.label6.Text = "Z-Offset:";
            // 
            // zOffsetNumeric
            // 
            this.zOffsetNumeric.DecimalPlaces = 2;
            this.zOffsetNumeric.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.zOffsetNumeric.Location = new System.Drawing.Point(4, 379);
            this.zOffsetNumeric.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.zOffsetNumeric.Name = "zOffsetNumeric";
            this.zOffsetNumeric.Size = new System.Drawing.Size(120, 22);
            this.zOffsetNumeric.TabIndex = 13;
            this.zOffsetNumeric.ValueChanged += new System.EventHandler(this.zOffsetNumeric_ValueChanged);
            // 
            // keyframeTrackBar1
            // 
            this.keyframeTrackBar1.BackColor = System.Drawing.Color.Transparent;
            this.keyframeTrackBar1.Keyframes = ((System.Collections.Generic.List<int>)(resources.GetObject("keyframeTrackBar1.Keyframes")));
            this.keyframeTrackBar1.Location = new System.Drawing.Point(4, 156);
            this.keyframeTrackBar1.Maximum = 50;
            this.keyframeTrackBar1.Minimum = 0;
            this.keyframeTrackBar1.Name = "keyframeTrackBar1";
            this.keyframeTrackBar1.Size = new System.Drawing.Size(167, 27);
            this.keyframeTrackBar1.TabIndex = 3;
            this.keyframeTrackBar1.TickFrequency = 5;
            this.keyframeTrackBar1.Value = 0;
            this.keyframeTrackBar1.ValueChanged += new System.EventHandler(this.keyframeTrackBar1_ValueChanged);
            // 
            // ControlPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label6);
            this.Controls.Add(this.zOffsetNumeric);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.yOffsetNumeric);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.xOffsetNumeric);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.frameComboBox);
            this.Controls.Add(this.newAnimButton);
            this.Controls.Add(this.rmvKeyframeButton);
            this.Controls.Add(this.addKeyframeButton);
            this.Controls.Add(this.keyframeTrackBar1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.animComboBox);
            this.Name = "ControlPanel";
            this.Size = new System.Drawing.Size(175, 517);
            ((System.ComponentModel.ISupportInitialize)(this.xOffsetNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.yOffsetNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.zOffsetNumeric)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox animComboBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private KeyframeTrackBar keyframeTrackBar1;
        private System.Windows.Forms.Button addKeyframeButton;
        private System.Windows.Forms.Button rmvKeyframeButton;
        private System.Windows.Forms.Button newAnimButton;
        private System.Windows.Forms.ComboBox frameComboBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown xOffsetNumeric;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown yOffsetNumeric;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown zOffsetNumeric;
    }
}

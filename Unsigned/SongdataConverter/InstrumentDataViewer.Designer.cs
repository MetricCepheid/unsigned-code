namespace SongdataConverter
{
    partial class InstrumentDataViewer
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
            this.difficultyTrackBar = new System.Windows.Forms.TrackBar();
            this.label1 = new System.Windows.Forms.Label();
            this.difficultyNumeric = new System.Windows.Forms.NumericUpDown();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.expertRadio = new System.Windows.Forms.RadioButton();
            this.hardRadio = new System.Windows.Forms.RadioButton();
            this.mediumRadio = new System.Windows.Forms.RadioButton();
            this.easyRadio = new System.Windows.Forms.RadioButton();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            this.stepLabel = new System.Windows.Forms.Label();
            this.notesViewerControl1 = new SongdataConverter.NotesViewerControl();
            this.hScrollBar1 = new System.Windows.Forms.HScrollBar();
            ((System.ComponentModel.ISupportInitialize)(this.difficultyTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.difficultyNumeric)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            this.SuspendLayout();
            // 
            // difficultyTrackBar
            // 
            this.difficultyTrackBar.LargeChange = 2;
            this.difficultyTrackBar.Location = new System.Drawing.Point(222, 414);
            this.difficultyTrackBar.Minimum = 1;
            this.difficultyTrackBar.Name = "difficultyTrackBar";
            this.difficultyTrackBar.Size = new System.Drawing.Size(513, 56);
            this.difficultyTrackBar.TabIndex = 1;
            this.difficultyTrackBar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.difficultyTrackBar.Value = 1;
            this.difficultyTrackBar.Scroll += new System.EventHandler(this.difficultyTrackBar_Scroll);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(223, 391);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(132, 17);
            this.label1.TabIndex = 2;
            this.label1.Text = "Perceived Difficulty:";
            // 
            // difficultyNumeric
            // 
            this.difficultyNumeric.Location = new System.Drawing.Point(695, 389);
            this.difficultyNumeric.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.difficultyNumeric.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.difficultyNumeric.Name = "difficultyNumeric";
            this.difficultyNumeric.Size = new System.Drawing.Size(40, 22);
            this.difficultyNumeric.TabIndex = 3;
            this.difficultyNumeric.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.difficultyNumeric.ValueChanged += new System.EventHandler(this.difficultyNumeric_ValueChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.expertRadio);
            this.groupBox1.Controls.Add(this.hardRadio);
            this.groupBox1.Controls.Add(this.mediumRadio);
            this.groupBox1.Controls.Add(this.easyRadio);
            this.groupBox1.Location = new System.Drawing.Point(6, 342);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(85, 128);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Difficulty";
            // 
            // expertRadio
            // 
            this.expertRadio.AutoSize = true;
            this.expertRadio.Checked = true;
            this.expertRadio.Location = new System.Drawing.Point(6, 102);
            this.expertRadio.Name = "expertRadio";
            this.expertRadio.Size = new System.Drawing.Size(69, 21);
            this.expertRadio.TabIndex = 3;
            this.expertRadio.TabStop = true;
            this.expertRadio.Text = "Expert";
            this.expertRadio.UseVisualStyleBackColor = true;
            this.expertRadio.CheckedChanged += new System.EventHandler(this.expertRadio_CheckedChanged);
            // 
            // hardRadio
            // 
            this.hardRadio.AutoSize = true;
            this.hardRadio.Location = new System.Drawing.Point(6, 75);
            this.hardRadio.Name = "hardRadio";
            this.hardRadio.Size = new System.Drawing.Size(60, 21);
            this.hardRadio.TabIndex = 2;
            this.hardRadio.Text = "Hard";
            this.hardRadio.UseVisualStyleBackColor = true;
            this.hardRadio.CheckedChanged += new System.EventHandler(this.hardRadio_CheckedChanged);
            // 
            // mediumRadio
            // 
            this.mediumRadio.AutoSize = true;
            this.mediumRadio.Location = new System.Drawing.Point(6, 48);
            this.mediumRadio.Name = "mediumRadio";
            this.mediumRadio.Size = new System.Drawing.Size(78, 21);
            this.mediumRadio.TabIndex = 1;
            this.mediumRadio.Text = "Medium";
            this.mediumRadio.UseVisualStyleBackColor = true;
            this.mediumRadio.CheckedChanged += new System.EventHandler(this.mediumRadio_CheckedChanged);
            // 
            // easyRadio
            // 
            this.easyRadio.AutoSize = true;
            this.easyRadio.Location = new System.Drawing.Point(6, 21);
            this.easyRadio.Name = "easyRadio";
            this.easyRadio.Size = new System.Drawing.Size(60, 21);
            this.easyRadio.TabIndex = 0;
            this.easyRadio.Text = "Easy";
            this.easyRadio.UseVisualStyleBackColor = true;
            this.easyRadio.CheckedChanged += new System.EventHandler(this.easyRadio_CheckedChanged);
            // 
            // trackBar1
            // 
            this.trackBar1.LargeChange = 2;
            this.trackBar1.Location = new System.Drawing.Point(98, 359);
            this.trackBar1.Maximum = 6;
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.trackBar1.Size = new System.Drawing.Size(56, 106);
            this.trackBar1.TabIndex = 6;
            this.trackBar1.Value = 4;
            this.trackBar1.Scroll += new System.EventHandler(this.trackBar1_Scroll);
            // 
            // stepLabel
            // 
            this.stepLabel.AutoSize = true;
            this.stepLabel.Location = new System.Drawing.Point(97, 339);
            this.stepLabel.Name = "stepLabel";
            this.stepLabel.Size = new System.Drawing.Size(45, 17);
            this.stepLabel.TabIndex = 7;
            this.stepLabel.Text = "Step: ";
            // 
            // notesViewerControl1
            // 
            this.notesViewerControl1.InstrumentType = null;
            this.notesViewerControl1.Location = new System.Drawing.Point(6, 3);
            this.notesViewerControl1.Name = "notesViewerControl1";
            this.notesViewerControl1.Size = new System.Drawing.Size(729, 306);
            this.notesViewerControl1.SongData = null;
            this.notesViewerControl1.TabIndex = 0;
            // 
            // hScrollBar1
            // 
            this.hScrollBar1.Location = new System.Drawing.Point(6, 312);
            this.hScrollBar1.Name = "hScrollBar1";
            this.hScrollBar1.Size = new System.Drawing.Size(729, 21);
            this.hScrollBar1.TabIndex = 8;
            this.hScrollBar1.Scroll += new System.Windows.Forms.ScrollEventHandler(this.hScrollBar1_Scroll);
            // 
            // InstrumentDataViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.hScrollBar1);
            this.Controls.Add(this.notesViewerControl1);
            this.Controls.Add(this.stepLabel);
            this.Controls.Add(this.trackBar1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.difficultyNumeric);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.difficultyTrackBar);
            this.Name = "InstrumentDataViewer";
            this.Size = new System.Drawing.Size(738, 473);
            ((System.ComponentModel.ISupportInitialize)(this.difficultyTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.difficultyNumeric)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TrackBar difficultyTrackBar;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown difficultyNumeric;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton expertRadio;
        private System.Windows.Forms.RadioButton hardRadio;
        private System.Windows.Forms.RadioButton mediumRadio;
        private System.Windows.Forms.RadioButton easyRadio;
        private System.Windows.Forms.TrackBar trackBar1;
        private System.Windows.Forms.Label stepLabel;
        private NotesViewerControl notesViewerControl1;
        private System.Windows.Forms.HScrollBar hScrollBar1;
    }
}

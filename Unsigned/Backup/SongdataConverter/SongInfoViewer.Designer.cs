namespace SongdataConverter
{
    partial class SongInfoViewer
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.songLengthTextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.genreComboBox = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.yearNumeric = new System.Windows.Forms.NumericUpDown();
            this.artistTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.nameTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.quotesListBox = new System.Windows.Forms.ListBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.addQuoteButton = new System.Windows.Forms.Button();
            this.editQuoteButton = new System.Windows.Forms.Button();
            this.removeQuoteButton = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.chartersListBox = new System.Windows.Forms.ListBox();
            this.removeCharterButton = new System.Windows.Forms.Button();
            this.editCharterButton = new System.Windows.Forms.Button();
            this.addCharterButton = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.yearNumeric)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.songLengthTextBox);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.genreComboBox);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.yearNumeric);
            this.groupBox1.Controls.Add(this.artistTextBox);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.nameTextBox);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(185, 255);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Metainfo";
            // 
            // songLengthTextBox
            // 
            this.songLengthTextBox.Location = new System.Drawing.Point(9, 223);
            this.songLengthTextBox.MaxLength = 8;
            this.songLengthTextBox.Name = "songLengthTextBox";
            this.songLengthTextBox.Size = new System.Drawing.Size(162, 22);
            this.songLengthTextBox.TabIndex = 8;
            this.songLengthTextBox.Text = "00:00:00";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 202);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(93, 17);
            this.label5.TabIndex = 1;
            this.label5.Text = "Song Length:";
            // 
            // genreComboBox
            // 
            this.genreComboBox.FormattingEnabled = true;
            this.genreComboBox.Items.AddRange(new object[] {
            "Classic",
            "Arena",
            "Punk",
            "Grunge",
            "Glam",
            "Alternative",
            "Progressive",
            "Acoustic",
            "Heavy Metal",
            "Speed Metal",
            "Death Metal",
            "Doom Metal",
            "Power Metal",
            "Thrash Metal",
            "Industrial",
            "Emo",
            "Screamo",
            "Rap",
            "Gothic",
            "Southern",
            "Swing/Jazz",
            "Soft Rock",
            "Ska",
            "Acid",
            "Stoner",
            "Surf",
            "Folk",
            "Rockabilly",
            "Funk",
            "Pop"});
            this.genreComboBox.Location = new System.Drawing.Point(9, 175);
            this.genreComboBox.Name = "genreComboBox";
            this.genreComboBox.Size = new System.Drawing.Size(162, 24);
            this.genreComboBox.TabIndex = 7;
            this.genreComboBox.Text = "Genre";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 154);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(52, 17);
            this.label4.TabIndex = 6;
            this.label4.Text = "Genre:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 109);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(136, 17);
            this.label3.TabIndex = 5;
            this.label3.Text = "Year song released:";
            // 
            // yearNumeric
            // 
            this.yearNumeric.Location = new System.Drawing.Point(9, 129);
            this.yearNumeric.Maximum = new decimal(new int[] {
            2100,
            0,
            0,
            0});
            this.yearNumeric.Minimum = new decimal(new int[] {
            1900,
            0,
            0,
            0});
            this.yearNumeric.Name = "yearNumeric";
            this.yearNumeric.Size = new System.Drawing.Size(162, 22);
            this.yearNumeric.TabIndex = 4;
            this.yearNumeric.Value = new decimal(new int[] {
            2009,
            0,
            0,
            0});
            // 
            // artistTextBox
            // 
            this.artistTextBox.Location = new System.Drawing.Point(9, 84);
            this.artistTextBox.MaxLength = 512;
            this.artistTextBox.Name = "artistTextBox";
            this.artistTextBox.Size = new System.Drawing.Size(162, 22);
            this.artistTextBox.TabIndex = 3;
            this.artistTextBox.Text = "Artist Name";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 64);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 17);
            this.label2.TabIndex = 2;
            this.label2.Text = "Artist:";
            // 
            // nameTextBox
            // 
            this.nameTextBox.Location = new System.Drawing.Point(6, 39);
            this.nameTextBox.MaxLength = 512;
            this.nameTextBox.Name = "nameTextBox";
            this.nameTextBox.Size = new System.Drawing.Size(165, 22);
            this.nameTextBox.TabIndex = 1;
            this.nameTextBox.Text = "Song Name";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(86, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Song Name:";
            // 
            // quotesListBox
            // 
            this.quotesListBox.FormattingEnabled = true;
            this.quotesListBox.ItemHeight = 16;
            this.quotesListBox.Location = new System.Drawing.Point(6, 18);
            this.quotesListBox.Name = "quotesListBox";
            this.quotesListBox.Size = new System.Drawing.Size(448, 228);
            this.quotesListBox.TabIndex = 1;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.removeQuoteButton);
            this.groupBox2.Controls.Add(this.editQuoteButton);
            this.groupBox2.Controls.Add(this.addQuoteButton);
            this.groupBox2.Controls.Add(this.quotesListBox);
            this.groupBox2.Location = new System.Drawing.Point(194, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(541, 255);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Quotes";
            // 
            // addQuoteButton
            // 
            this.addQuoteButton.Location = new System.Drawing.Point(460, 12);
            this.addQuoteButton.Name = "addQuoteButton";
            this.addQuoteButton.Size = new System.Drawing.Size(75, 23);
            this.addQuoteButton.TabIndex = 2;
            this.addQuoteButton.Text = "Add";
            this.addQuoteButton.UseVisualStyleBackColor = true;
            this.addQuoteButton.Click += new System.EventHandler(this.addQuoteButton_Click);
            // 
            // editQuoteButton
            // 
            this.editQuoteButton.Location = new System.Drawing.Point(460, 41);
            this.editQuoteButton.Name = "editQuoteButton";
            this.editQuoteButton.Size = new System.Drawing.Size(75, 23);
            this.editQuoteButton.TabIndex = 3;
            this.editQuoteButton.Text = "Edit";
            this.editQuoteButton.UseVisualStyleBackColor = true;
            this.editQuoteButton.Click += new System.EventHandler(this.editQuoteButton_Click);
            // 
            // removeQuoteButton
            // 
            this.removeQuoteButton.Location = new System.Drawing.Point(460, 70);
            this.removeQuoteButton.Name = "removeQuoteButton";
            this.removeQuoteButton.Size = new System.Drawing.Size(75, 23);
            this.removeQuoteButton.TabIndex = 4;
            this.removeQuoteButton.Text = "Remove";
            this.removeQuoteButton.UseVisualStyleBackColor = true;
            this.removeQuoteButton.Click += new System.EventHandler(this.removeQuoteButton_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.removeCharterButton);
            this.groupBox3.Controls.Add(this.chartersListBox);
            this.groupBox3.Controls.Add(this.editCharterButton);
            this.groupBox3.Controls.Add(this.addCharterButton);
            this.groupBox3.Location = new System.Drawing.Point(3, 264);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(217, 206);
            this.groupBox3.TabIndex = 3;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Charter(s)";
            // 
            // chartersListBox
            // 
            this.chartersListBox.FormattingEnabled = true;
            this.chartersListBox.ItemHeight = 16;
            this.chartersListBox.Location = new System.Drawing.Point(7, 22);
            this.chartersListBox.Name = "chartersListBox";
            this.chartersListBox.Size = new System.Drawing.Size(120, 180);
            this.chartersListBox.TabIndex = 0;
            // 
            // removeCharterButton
            // 
            this.removeCharterButton.Location = new System.Drawing.Point(133, 80);
            this.removeCharterButton.Name = "removeCharterButton";
            this.removeCharterButton.Size = new System.Drawing.Size(75, 23);
            this.removeCharterButton.TabIndex = 7;
            this.removeCharterButton.Text = "Remove";
            this.removeCharterButton.UseVisualStyleBackColor = true;
            this.removeCharterButton.Click += new System.EventHandler(this.removeCharterButton_Click);
            // 
            // editCharterButton
            // 
            this.editCharterButton.Location = new System.Drawing.Point(133, 51);
            this.editCharterButton.Name = "editCharterButton";
            this.editCharterButton.Size = new System.Drawing.Size(75, 23);
            this.editCharterButton.TabIndex = 6;
            this.editCharterButton.Text = "Edit";
            this.editCharterButton.UseVisualStyleBackColor = true;
            this.editCharterButton.Click += new System.EventHandler(this.editCharterButton_Click);
            // 
            // addCharterButton
            // 
            this.addCharterButton.Location = new System.Drawing.Point(133, 22);
            this.addCharterButton.Name = "addCharterButton";
            this.addCharterButton.Size = new System.Drawing.Size(75, 23);
            this.addCharterButton.TabIndex = 5;
            this.addCharterButton.Text = "Add";
            this.addCharterButton.UseVisualStyleBackColor = true;
            this.addCharterButton.Click += new System.EventHandler(this.addCharterButton_Click);
            // 
            // SongInfoViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "SongInfoViewer";
            this.Size = new System.Drawing.Size(738, 473);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.yearNumeric)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox nameTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox artistTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox genreComboBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown yearNumeric;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox songLengthTextBox;
        private System.Windows.Forms.ListBox quotesListBox;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button removeQuoteButton;
        private System.Windows.Forms.Button editQuoteButton;
        private System.Windows.Forms.Button addQuoteButton;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button removeCharterButton;
        private System.Windows.Forms.ListBox chartersListBox;
        private System.Windows.Forms.Button editCharterButton;
        private System.Windows.Forms.Button addCharterButton;
    }
}

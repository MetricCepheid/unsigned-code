namespace SongdataConverter
{
    partial class Form1
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.songTab = new System.Windows.Forms.TabPage();
            this.songInfoViewer = new SongdataConverter.SongInfoViewer();
            this.lGuitarTab = new System.Windows.Forms.TabPage();
            this.leadGuitarDataViewer = new SongdataConverter.InstrumentDataViewer();
            this.rGuitarTab = new System.Windows.Forms.TabPage();
            this.label1 = new System.Windows.Forms.Label();
            this.bassTab = new System.Windows.Forms.TabPage();
            this.bassDataViewer = new SongdataConverter.InstrumentDataViewer();
            this.drumsTab = new System.Windows.Forms.TabPage();
            this.drumsDataViewer = new SongdataConverter.InstrumentDataViewer();
            this.vocalsTab = new System.Windows.Forms.TabPage();
            this.bandBonusesTab = new System.Windows.Forms.TabPage();
            this.bandBonusesDataViewer1 = new SongdataConverter.BandBonusesDataViewer();
            this.effectsTab = new System.Windows.Forms.TabPage();
            this.roadieDataViewer1 = new SongdataConverter.RoadieDataViewer();
            this.menuStrip1.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.songTab.SuspendLayout();
            this.lGuitarTab.SuspendLayout();
            this.rGuitarTab.SuspendLayout();
            this.bassTab.SuspendLayout();
            this.drumsTab.SuspendLayout();
            this.bandBonusesTab.SuspendLayout();
            this.effectsTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(782, 28);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.toolStripSeparator,
            this.saveToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.toolStripSeparator1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(44, 24);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("openToolStripMenuItem.Image")));
            this.openToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openToolStripMenuItem.Size = new System.Drawing.Size(167, 24);
            this.openToolStripMenuItem.Text = "&Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // toolStripSeparator
            // 
            this.toolStripSeparator.Name = "toolStripSeparator";
            this.toolStripSeparator.Size = new System.Drawing.Size(164, 6);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("saveToolStripMenuItem.Image")));
            this.saveToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(167, 24);
            this.saveToolStripMenuItem.Text = "&Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(167, 24);
            this.saveAsToolStripMenuItem.Text = "Save &As";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(164, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(167, 24);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(53, 24);
            this.helpToolStripMenuItem.Text = "&Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(128, 24);
            this.aboutToolStripMenuItem.Text = "&About...";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.songTab);
            this.tabControl.Controls.Add(this.lGuitarTab);
            this.tabControl.Controls.Add(this.rGuitarTab);
            this.tabControl.Controls.Add(this.bassTab);
            this.tabControl.Controls.Add(this.drumsTab);
            this.tabControl.Controls.Add(this.vocalsTab);
            this.tabControl.Controls.Add(this.bandBonusesTab);
            this.tabControl.Controls.Add(this.effectsTab);
            this.tabControl.Location = new System.Drawing.Point(12, 31);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(758, 514);
            this.tabControl.TabIndex = 1;
            // 
            // songTab
            // 
            this.songTab.BackColor = System.Drawing.Color.WhiteSmoke;
            this.songTab.Controls.Add(this.songInfoViewer);
            this.songTab.Location = new System.Drawing.Point(4, 25);
            this.songTab.Name = "songTab";
            this.songTab.Padding = new System.Windows.Forms.Padding(3);
            this.songTab.Size = new System.Drawing.Size(750, 485);
            this.songTab.TabIndex = 2;
            this.songTab.Text = "Song Info";
            // 
            // songInfoViewer
            // 
            this.songInfoViewer.BackColor = System.Drawing.SystemColors.Control;
            this.songInfoViewer.Location = new System.Drawing.Point(6, 6);
            this.songInfoViewer.Name = "songInfoViewer";
            this.songInfoViewer.Size = new System.Drawing.Size(738, 473);
            this.songInfoViewer.SongData = null;
            this.songInfoViewer.TabIndex = 0;
            // 
            // lGuitarTab
            // 
            this.lGuitarTab.BackColor = System.Drawing.Color.WhiteSmoke;
            this.lGuitarTab.Controls.Add(this.leadGuitarDataViewer);
            this.lGuitarTab.Location = new System.Drawing.Point(4, 25);
            this.lGuitarTab.Name = "lGuitarTab";
            this.lGuitarTab.Padding = new System.Windows.Forms.Padding(3);
            this.lGuitarTab.Size = new System.Drawing.Size(750, 485);
            this.lGuitarTab.TabIndex = 3;
            this.lGuitarTab.Text = "Lead Guitar";
            // 
            // leadGuitarDataViewer
            // 
            this.leadGuitarDataViewer.InstrumentType = null;
            this.leadGuitarDataViewer.Location = new System.Drawing.Point(6, 6);
            this.leadGuitarDataViewer.Name = "leadGuitarDataViewer";
            this.leadGuitarDataViewer.Size = new System.Drawing.Size(738, 473);
            this.leadGuitarDataViewer.SongData = null;
            this.leadGuitarDataViewer.TabIndex = 0;
            // 
            // rGuitarTab
            // 
            this.rGuitarTab.BackColor = System.Drawing.Color.WhiteSmoke;
            this.rGuitarTab.Controls.Add(this.label1);
            this.rGuitarTab.Location = new System.Drawing.Point(4, 25);
            this.rGuitarTab.Name = "rGuitarTab";
            this.rGuitarTab.Padding = new System.Windows.Forms.Padding(3);
            this.rGuitarTab.Size = new System.Drawing.Size(750, 485);
            this.rGuitarTab.TabIndex = 4;
            this.rGuitarTab.Text = "Rhythm Guitar";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(133, 223);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(495, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "This space is reserved for a possible feature in an future version of Unsigned";
            // 
            // bassTab
            // 
            this.bassTab.BackColor = System.Drawing.Color.WhiteSmoke;
            this.bassTab.Controls.Add(this.bassDataViewer);
            this.bassTab.Location = new System.Drawing.Point(4, 25);
            this.bassTab.Name = "bassTab";
            this.bassTab.Padding = new System.Windows.Forms.Padding(3);
            this.bassTab.Size = new System.Drawing.Size(750, 485);
            this.bassTab.TabIndex = 5;
            this.bassTab.Text = "Bass";
            // 
            // bassDataViewer
            // 
            this.bassDataViewer.InstrumentType = null;
            this.bassDataViewer.Location = new System.Drawing.Point(6, 6);
            this.bassDataViewer.Name = "bassDataViewer";
            this.bassDataViewer.Size = new System.Drawing.Size(738, 473);
            this.bassDataViewer.SongData = null;
            this.bassDataViewer.TabIndex = 0;
            // 
            // drumsTab
            // 
            this.drumsTab.BackColor = System.Drawing.Color.WhiteSmoke;
            this.drumsTab.Controls.Add(this.drumsDataViewer);
            this.drumsTab.Location = new System.Drawing.Point(4, 25);
            this.drumsTab.Name = "drumsTab";
            this.drumsTab.Padding = new System.Windows.Forms.Padding(3);
            this.drumsTab.Size = new System.Drawing.Size(750, 485);
            this.drumsTab.TabIndex = 6;
            this.drumsTab.Text = "Drums";
            // 
            // drumsDataViewer
            // 
            this.drumsDataViewer.InstrumentType = null;
            this.drumsDataViewer.Location = new System.Drawing.Point(6, 6);
            this.drumsDataViewer.Name = "drumsDataViewer";
            this.drumsDataViewer.Size = new System.Drawing.Size(738, 473);
            this.drumsDataViewer.SongData = null;
            this.drumsDataViewer.TabIndex = 0;
            // 
            // vocalsTab
            // 
            this.vocalsTab.BackColor = System.Drawing.Color.WhiteSmoke;
            this.vocalsTab.Location = new System.Drawing.Point(4, 25);
            this.vocalsTab.Name = "vocalsTab";
            this.vocalsTab.Padding = new System.Windows.Forms.Padding(3);
            this.vocalsTab.Size = new System.Drawing.Size(750, 485);
            this.vocalsTab.TabIndex = 7;
            this.vocalsTab.Text = "Vocals";
            // 
            // bandBonusesTab
            // 
            this.bandBonusesTab.BackColor = System.Drawing.Color.WhiteSmoke;
            this.bandBonusesTab.Controls.Add(this.bandBonusesDataViewer1);
            this.bandBonusesTab.Location = new System.Drawing.Point(4, 25);
            this.bandBonusesTab.Name = "bandBonusesTab";
            this.bandBonusesTab.Padding = new System.Windows.Forms.Padding(3);
            this.bandBonusesTab.Size = new System.Drawing.Size(750, 485);
            this.bandBonusesTab.TabIndex = 9;
            this.bandBonusesTab.Text = "Band Bonuses";
            // 
            // bandBonusesDataViewer1
            // 
            this.bandBonusesDataViewer1.Location = new System.Drawing.Point(6, 6);
            this.bandBonusesDataViewer1.Name = "bandBonusesDataViewer1";
            this.bandBonusesDataViewer1.Size = new System.Drawing.Size(738, 473);
            this.bandBonusesDataViewer1.SongData = null;
            this.bandBonusesDataViewer1.TabIndex = 0;
            // 
            // effectsTab
            // 
            this.effectsTab.BackColor = System.Drawing.Color.WhiteSmoke;
            this.effectsTab.Controls.Add(this.roadieDataViewer1);
            this.effectsTab.Location = new System.Drawing.Point(4, 25);
            this.effectsTab.Name = "effectsTab";
            this.effectsTab.Padding = new System.Windows.Forms.Padding(3);
            this.effectsTab.Size = new System.Drawing.Size(750, 485);
            this.effectsTab.TabIndex = 8;
            this.effectsTab.Text = "Roadie Work";
            // 
            // roadieDataViewer1
            // 
            this.roadieDataViewer1.Location = new System.Drawing.Point(6, 6);
            this.roadieDataViewer1.Name = "roadieDataViewer1";
            this.roadieDataViewer1.Size = new System.Drawing.Size(738, 473);
            this.roadieDataViewer1.SongData = null;
            this.roadieDataViewer1.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(782, 557);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "Unsigned 2.0 SongData Conversion Utility";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.tabControl.ResumeLayout(false);
            this.songTab.ResumeLayout(false);
            this.lGuitarTab.ResumeLayout(false);
            this.rGuitarTab.ResumeLayout(false);
            this.rGuitarTab.PerformLayout();
            this.bassTab.ResumeLayout(false);
            this.drumsTab.ResumeLayout(false);
            this.bandBonusesTab.ResumeLayout(false);
            this.effectsTab.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage songTab;
        private System.Windows.Forms.TabPage lGuitarTab;
        private System.Windows.Forms.TabPage rGuitarTab;
        private System.Windows.Forms.TabPage bassTab;
        private System.Windows.Forms.TabPage drumsTab;
        private System.Windows.Forms.TabPage vocalsTab;
        private System.Windows.Forms.TabPage effectsTab;
        private SongInfoViewer songInfoViewer;
        private InstrumentDataViewer leadGuitarDataViewer;
        private System.Windows.Forms.Label label1;
        private InstrumentDataViewer bassDataViewer;
        private InstrumentDataViewer drumsDataViewer;
        private System.Windows.Forms.TabPage bandBonusesTab;
        private BandBonusesDataViewer bandBonusesDataViewer1;
        private RoadieDataViewer roadieDataViewer1;
    }
}


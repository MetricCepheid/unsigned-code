namespace SongdataConverter
{
    partial class RoadieViewerControl
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
            this.components = new System.ComponentModel.Container();
            this.rightClickMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.SuspendLayout();
            // 
            // rightClickMenu
            // 
            this.rightClickMenu.Name = "rightClickMenu";
            this.rightClickMenu.Size = new System.Drawing.Size(153, 26);
            // 
            // BandBonusesViewerControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.DoubleBuffered = true;
            this.Name = "BandBonusesViewerControl";
            this.Size = new System.Drawing.Size(466, 114);
            this.MouseLeave += new System.EventHandler(this.NotesViewerControl_MouseLeave);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.NotesViewerControl_MouseMove);
            this.Leave += new System.EventHandler(this.NotesViewerControl_Leave);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.NotesViewerControl_KeyUp);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.NotesViewerControl_MouseDown);
            this.Enter += new System.EventHandler(this.NotesViewerControl_Enter);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.NotesViewerControl_MouseUp);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.NotesViewerControl_KeyDown);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip rightClickMenu;

    }
}

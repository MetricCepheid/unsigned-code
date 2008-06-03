namespace VocalEditor
{
    partial class VocalPreviewer
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
            this.SuspendLayout();
            // 
            // VocalPreviewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "VocalPreviewer";
            this.Size = new System.Drawing.Size(1000, 256);
            this.MouseLeave += new System.EventHandler(this.VocalPreviewer_MouseLeave);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.VocalPreviewer_MouseMove);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.VocalPreviewer_MouseDown);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.VocalPreviewer_MouseUp);
            this.ResumeLayout(false);

        }

        #endregion
    }
}

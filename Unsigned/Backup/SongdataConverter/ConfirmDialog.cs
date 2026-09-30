using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SongdataConverter
{
    public partial class ConfirmDialog : Form
    {
        public String Title { get { return Text; } set { Text = value; } }
        public String Question { get { return label1.Text; } set { label1.Text = value; } }

        public ConfirmDialog()
        {
            InitializeComponent();
        }

        private void yesButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Yes;
            this.Close();
        }

        private void noButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.No;
            this.Close();
        }
    }
}

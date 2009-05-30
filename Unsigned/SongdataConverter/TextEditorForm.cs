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
    public partial class TextEditorForm : Form
    {
        public String Value
        {
            get { return textBox1.Text; }
            set { textBox1.Text = value; }
        }

        public TextEditorForm()
        {
            InitializeComponent();
        }

        private void okayButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}

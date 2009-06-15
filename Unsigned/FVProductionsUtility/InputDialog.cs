using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FVProductions.Utility
{
    public partial class InputDialog : Form
    {
        public String Question
        {
            get { return label1.Text; }
            set { label1.Text = value; }
        }
        public String Answer
        {
            get { return textBox1.Text; }
            set { textBox1.Text = value; }
        }

        public InputDialog()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
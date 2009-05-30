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
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();
            this.Bounds = new Rectangle((Screen.GetBounds(this).Width / 2) - (Bounds.Width / 2),
                                        (Screen.GetBounds(this).Height / 2) - (Bounds.Height / 2),
                                        Bounds.Width, Bounds.Height);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

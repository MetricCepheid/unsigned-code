using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SongDataIO;

namespace SongdataConverter
{
    public partial class SongInfoViewer : UserControl
    {
        private SongData _songData = null;
        public SongData SongData
        {
            get { return _songData; }
            set { _songData = value; RefreshAllInfo(); }
        }

        public SongInfoViewer()
        {
            InitializeComponent();
        }

        private void RefreshAllInfo()
        {
            nameTextBox.Text = "Song Name";
            artistTextBox.Text = "Artist Name";
            yearNumeric.Value = 2009;
            songLengthTextBox.Text = "00:00:00";
            genreComboBox.SelectedIndex = -1;
            quotesListBox.SelectedIndex = -1;
            quotesListBox.Items.Clear();
            chartersListBox.SelectedIndex = -1;
            chartersListBox.Items.Clear();

            if(SongData != null)
            {
                nameTextBox.Text = SongData.info.name;
                artistTextBox.Text = SongData.info.artist;
                if (SongData.info.year < yearNumeric.Minimum || SongData.info.year > yearNumeric.Maximum)
                    yearNumeric.Value = DateTime.Now.Year;
                else
                    yearNumeric.Value = SongData.info.year;
                songLengthTextBox.Text = (SongData.info.length.Hours>0?((SongData.info.length.Hours<10?"0":"")+SongData.info.length.Hours+":"):"")+
                                         (SongData.info.length.Minutes<10?"0":"")+SongData.info.length.Minutes+":"+
                                         (SongData.info.length.Seconds<10?"0":"")+SongData.info.length.Seconds;
                for (int i = 0; i < genreComboBox.Items.Count; i++)
                {
                    String option = genreComboBox.Items[i].ToString().ToLower().Trim();
                    if (option == SongData.info.genre.ToLower().Trim())
                        genreComboBox.SelectedIndex = i;
                    if ((option+" rock") == SongData.info.genre.ToLower().Trim())
                        genreComboBox.SelectedIndex = i;
                }
                for (int i = 0; i < SongData.info.quotes.Length; i++)
                    if(SongData.info.quotes[i] != null && SongData.info.quotes[i].Length>0)
                        quotesListBox.Items.Add(SongData.info.quotes[i].Replace("\"", ""));
                for (int i = 0; i < SongData.info.charters.Length; i++)
                    if (SongData.info.charters[i] != null && SongData.info.charters[i].Length > 0)
                        chartersListBox.Items.Add(SongData.info.charters[i]);
            }
        }

        public void ApplyChanges()
        {
            if (nameTextBox.Text == null)
                throw new Exception("Song Name is not valid");
            if (nameTextBox.Text.Length <= 0)
                throw new Exception("Song Name \"" + nameTextBox.Text + "\" is not valid");
            if (artistTextBox.Text == null)
                throw new Exception("Artist Name is not valid");
            if (artistTextBox.Text.Length <= 0)
                throw new Exception("Artist Name \"" + artistTextBox.Text + "\" is not valid");
            if (genreComboBox.SelectedIndex < 0)
                throw new Exception("No Genre Selected");
            TimeSpan length = ParseLength();
            SongData.info.name = nameTextBox.Text;
            SongData.info.artist = artistTextBox.Text;
            SongData.info.length = length;
            SongData.info.genre = genreComboBox.Items[genreComboBox.SelectedIndex].ToString();
            SongData.info.year = (uint)yearNumeric.Value;
            SongData.info.quotes = new String[8];
            for (int i = 0; i < quotesListBox.Items.Count; i++)
                SongData.info.quotes[i] = quotesListBox.Items[i].ToString().Replace("\"", "");
            for (int i = quotesListBox.Items.Count; i < 8; i++)
                SongData.info.quotes[i] = "";
        }

        private TimeSpan ParseLength()
        {
            String lenStr = songLengthTextBox.Text.Trim();
            int numColons = 0;
            for (int i = 0; i < lenStr.Length; i++)
            {
                if (!Char.IsDigit(lenStr[i]) && lenStr[i] != ':')
                    throw new Exception("Song Length field contains invalid character \'" + lenStr[i] + "\'");
                if (lenStr[i] == ':')
                    numColons++;
            }
            if (numColons < 1 || numColons > 2)
                throw new Exception("Song Length field has invalid format. Try \"MM:SS\" or \"HH:MM:SS\"");

            String hour, minute, second;

            if (numColons == 2)
            {
                hour = lenStr.Substring(0, lenStr.IndexOf(':'));
                lenStr = lenStr.Substring(lenStr.IndexOf(':') + 1);
            }
            else
                hour = "00";

            minute = lenStr.Substring(0, lenStr.IndexOf(':'));
            lenStr = lenStr.Substring(lenStr.IndexOf(':') + 1);
            second = lenStr;
            if (minute.Length <= 0)
                minute = "00";
            if (second.Length <= 0)
                second = "00";
            int h = Int32.Parse(hour);
            int m = Int32.Parse(minute);
            int s = Int32.Parse(second);
            while (s >= 60)
            {
                m++;
                s -= 60;
            }
            while (m >= 60)
            {
                h++;
                m -= 60;
            }
            return new TimeSpan(h, m, s);
        }

        private void addQuoteButton_Click(object sender, EventArgs e)
        {
            if (quotesListBox.Items.Count >= 8)
            {
                MessageBox.Show(this, "Only 8 quotes are allowed", "Add New Quote Failed", MessageBoxButtons.OK);
                return;
            }
            quotesListBox.SelectedIndex = -1;
            TextEditorForm f = new TextEditorForm();
            f.Value = "New Quote";
            DialogResult r = f.ShowDialog();
            if (r == DialogResult.OK)
            {
                if (f.Value != null && f.Value.Length > 0)
                {
                    quotesListBox.Items.Add(f.Value);
                    quotesListBox.SelectedIndex = quotesListBox.Items.Count - 1;
                }
            }
        }

        private void editQuoteButton_Click(object sender, EventArgs e)
        {
            if(quotesListBox.SelectedIndex>=0 && quotesListBox.SelectedIndex<quotesListBox.Items.Count)
            {
                TextEditorForm f = new TextEditorForm();
                f.Value = quotesListBox.Items[quotesListBox.SelectedIndex].ToString();
                DialogResult r = f.ShowDialog();
                if (r == DialogResult.OK)
                {
                    quotesListBox.Items[quotesListBox.SelectedIndex] = f.Value;
                }
            }
        }

        private void removeQuoteButton_Click(object sender, EventArgs e)
        {
            if (quotesListBox.SelectedIndex >= 0 && quotesListBox.SelectedIndex < quotesListBox.Items.Count)
            {
                quotesListBox.Items.RemoveAt(quotesListBox.SelectedIndex);
                quotesListBox.SelectedIndex = -1;
            }
        }

        private void addCharterButton_Click(object sender, EventArgs e)
        {
            chartersListBox.SelectedIndex = -1;
            TextEditorForm f = new TextEditorForm();
            f.Value = "New Charter";
            DialogResult r = f.ShowDialog();
            if (r == DialogResult.OK)
            {
                if (f.Value != null && f.Value.Length > 0)
                {
                    chartersListBox.Items.Add(f.Value);
                    chartersListBox.SelectedIndex = chartersListBox.Items.Count - 1;
                }
            }
        }

        private void editCharterButton_Click(object sender, EventArgs e)
        {
            if (chartersListBox.SelectedIndex >= 0 && chartersListBox.SelectedIndex < chartersListBox.Items.Count)
            {
                TextEditorForm f = new TextEditorForm();
                f.Value = chartersListBox.Items[chartersListBox.SelectedIndex].ToString();
                DialogResult r = f.ShowDialog();
                if (r == DialogResult.OK)
                {
                    chartersListBox.Items[chartersListBox.SelectedIndex] = f.Value;
                }
            }
        }

        private void removeCharterButton_Click(object sender, EventArgs e)
        {
            if (chartersListBox.SelectedIndex >= 0 && chartersListBox.SelectedIndex < chartersListBox.Items.Count)
            {
                chartersListBox.Items.RemoveAt(chartersListBox.SelectedIndex);
                chartersListBox.SelectedIndex = -1;
            }
        }
    }
}

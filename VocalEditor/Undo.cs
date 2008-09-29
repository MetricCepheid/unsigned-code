using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace VocalEditor
{
    public abstract class UndoCommand
    {
        public abstract UndoCommand Execute();
    }

    public class UndoNoteSelect : UndoCommand
    {
        private VocalPreviewer voxorz;
        private int oldSP, oldSN;
        private bool oldSNB;

        public UndoNoteSelect(VocalPreviewer vp, int oldSN, bool oldSNB, int oldSP)
        {
            this.voxorz = vp;
            this.oldSN = oldSN;
            this.oldSNB = oldSNB;
            this.oldSP = oldSP;
        }

        public override UndoCommand Execute()
        {
            UndoNoteSelect ret = new UndoNoteSelect(voxorz, voxorz.SelectedNode, voxorz.SelectedNodeBegin, voxorz.SelectedPhrase);
            voxorz.SelectedNode = oldSN;
            voxorz.SelectedNodeBegin = oldSNB;
            voxorz.SelectedPhrase = oldSP;
            return ret;
        }
    }

    public class UndoNoteRemove : UndoCommand
    {
        public Song song;
        public VocalWord word;

        public UndoNoteRemove(Song s, VocalWord vw)
        {
            song = s;
            word = vw;
        }

        public override UndoCommand Execute()
        {
            UndoNoteCreate ret = new UndoNoteCreate(song, word);
            song.words.Add(word);
            song.words.Sort();
            return ret;
        }
    }

    public class UndoNoteMove : UndoCommand
    {
        Song song;
        int selNote;
        bool selBegin;
        uint oldTime;
        short oldVal;

        public UndoNoteMove(Song s, int selectedNote, bool selectedBegin, short noteVal, uint oldTime)
        {
            song = s;
            selNote = selectedNote;
            selBegin = selectedBegin;
            this.oldTime = oldTime;
            oldVal = noteVal;
        }

        public override UndoCommand Execute()
        {
            UndoNoteMove ret = new UndoNoteMove(song, selNote, selBegin, selBegin ? song.words[selNote].startNote : song.words[selNote].endNote, selBegin ? song.words[selNote].time : song.words[selNote].end);
            if (selBegin)
            {
                song.words[selNote].time = oldTime;
                song.words[selNote].startNote = oldVal;
            }
            else
            {
                song.words[selNote].len = oldTime - song.words[selNote].time;
                song.words[selNote].endNote = oldVal;
            }
            return ret;
        }
    }

    public class UndoNoteCreate : UndoCommand
    {
        Song song;
        VocalWord word;

        public UndoNoteCreate(Song s, VocalWord vw)
        {
            song = s;
            word = vw;
        }

        public override UndoCommand Execute()
        {
            UndoNoteRemove ret = new UndoNoteRemove(song, word);
            song.words.Remove(word);
            return ret;
        }
    }

    public class UndoTextEdit : UndoCommand
    {
        TextBox textBox;
        String text;

        public UndoTextEdit(TextBox tb, String text)
        {
            textBox = tb;
            this.text = text;
        }

        public override UndoCommand Execute()
        {
            UndoTextEdit ret = new UndoTextEdit(textBox, textBox.Text);
            textBox.Text = text;
            return ret;
        }
    }
}
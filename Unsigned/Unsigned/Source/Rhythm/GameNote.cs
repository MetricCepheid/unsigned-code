using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SongDataIO;

namespace Unsigned
{
    internal class GameNote
    {
        // time and length (in seconds)
        // for non-held notes, length = 0
        private float time, length;
        // for vocal notes, the word associated with this note
        private String text;
        // which colors this note is
        private bool[] frets;
        // which notes have been hit (drums only)
        private bool[] pressed;
        // true if the note was hit
        private bool dead;
        // true if note is inside a visible fill
        private bool hidden;
        // the visibility state of the notes
        // 0 = invisible
        // 1 = visible, yet to be hit
        private byte[] isVisible;
        // if the user has hit a valid pressed config for this note (guitar/bass only)
        private bool hitGoodFrettage;

        /// <summary>
        /// When this note starts
        /// </summary>
        public float Start { get { return time; } }
        /// <summary>
        /// When this note ends
        /// For non-held notes, should be same as Start
        /// </summary>
        public float End { get { return time + length; } }
        /// <summary>
        /// How long this note is
        /// For non-held notes, should be 0
        /// </summary>
        public float Length { get { return length; } }

        /// <summary>
        /// Whether the user has strummed this note yet
        /// </summary>
        public bool Strummed { get; private set; }

        /// <summary>
        /// Whether this note has been hit and killed
        /// </summary>
        public bool IsDead { get { return dead; } }

        /// <summary>
        /// If this note can be HOPO'd
        /// </summary>
        public bool IsHOPO
        {
            get;
            private set;
        }

        public bool IsHidden { get { return hidden; } }

        /// <summary>
        /// If this note is currently being held
        /// </summary>
        public bool Burning { get; set; }

        public int NumNotes { get; private set; }

        public GameNote(Instrument instr, SongData.NoteSet ns)
        {
            time = ns.time / 1000f;
            length = ns.length / 1000f;
            text = ns.text;
            frets = new bool[instr.NumTracks];
            if (!instr.NeedsStrum)
                pressed = new bool[instr.NumTracks];
            else
                pressed = null;
            isVisible = new byte[instr.NumTracks];
            for (int i = 0; i < instr.NumTracks; i++)
            {
                if ((((ulong)1 << i) & ns.type) != 0)
                    frets[i] = true;
                if (!instr.NeedsStrum)
                    pressed[i] = false;
                isVisible[i] = 1;
            }
            hitGoodFrettage = false;
            Strummed = false;
            if (instr.CanHOPO && (((ulong)1 << instr.NumTracks) & ns.type) != 0)
                IsHOPO = true;
            Burning = false;
            NumNotes = 0;
            for (int i = 0; i < instr.NumTracks; i++)
                if (frets[i])
                    NumNotes++;
            dead = false;
            hidden = false;
        }

        private void CheckValidFrettage(ulong press)
        {
            if (NumNotes > 1)
            {
                for (int i = 0; i < frets.Length; i++)
                    if (frets[i] != ((((ulong)1 << i) & press) != 0))
                        return;
                hitGoodFrettage = true;
            }
            else
            {
                bool foundNote = false;
                for (int i = 0; i < frets.Length; i++)
                {
                    if (!foundNote)
                    {
                        if (frets[i])
                            if ((((ulong)1 << i)&press) == 0)
                                return;
                            else
                                foundNote = true;
                    }
                    else if ((((ulong)1 << i)&press) != 0)
                        return;
                }
                hitGoodFrettage = true;
            }
        }

        public void Strum()
        {
            Strummed = true;
        }

        public void AddHeld(ulong pressed)
        {
            CheckValidFrettage(pressed);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="press"></param>
        /// <returns>the notes hit by this press</returns>
        public ulong AddPressed(ulong press)
        {
            ulong ret = 0;
            for (int i = 0; i < pressed.Length; i++)
            {
                if ((((ulong)1 << i) & press) != 0)
                {
                    if (frets[i] && !pressed[i])
                    {
                        isVisible[i] = 0;
                        pressed[i] = true;
                        ret |= ((ulong)1 << i);
                    }
                }
            }
            return ret;
        }

        public bool IsGood(bool HOPOable)
        {
            if (pressed != null)
            {
                bool good = true;
                for (int i = 0; i < frets.Length; i++)
                    if (frets[i] != pressed[i])
                        good = false;
                return good;
            }
            //needs strum
            if (Strummed || (IsHOPO && HOPOable))
                return hitGoodFrettage;
            return false;
        }

        public int Kill(bool HOPOable)
        {
            for (int i = 0; i < isVisible.Length; i++)
                isVisible[i] = 0;
            if (pressed != null)
            {
                int count = 0;
                for (int i = 0; i < frets.Length; i++)
                    if (frets[i] && pressed[i])
                        count++;
                if (count >= NumNotes)
                    dead = true;
                return count;
            }
            else
            {
                if (hitGoodFrettage && (Strummed || (IsHOPO && HOPOable)))
                    dead = true;
                return (hitGoodFrettage && (Strummed || (IsHOPO && HOPOable))) ? NumNotes : 0;
            }
        }

        public void Reset()
        {
            for (int i = 0; i < isVisible.Length; i++)
            {
                if (pressed!=null)
                    pressed[i] = false;
                isVisible[i] = 1;
            }
            hitGoodFrettage = false;
            Strummed = false;
            Burning = false;
            NumNotes = 0;
            for (int i = 0; i < frets.Length; i++)
                if (frets[i])
                    NumNotes++;
            dead = false;
            hidden = false;
        }

        public bool IsVisible(int r)
        {
            if (hidden)
                return false;
            if (r >= frets.Length)
                return false;
            if (!frets[r])
                return false;
            return isVisible[r] == 1;
        }

        public bool HasFret(int r)
        {
            if (r >= frets.Length)
                return false;
            if (!frets[r])
                return false;
            return true;
        }

        internal void Hide()
        {
            hidden = true;
        }

        public static bool IsValidFrettage(GameNote note, ulong press)
        {
            if (note.NumNotes > 1)
            {
                for (int i = 0; i < note.frets.Length; i++)
                    if (note.frets[i] != ((((ulong)1 << i) & press) != 0))
                        return false;
                return true;
            }
            else
            {
                bool foundNote = false;
                for (int i = 0; i < note.frets.Length; i++)
                {
                    if (!foundNote)
                    {
                        if (note.frets[i])
                            if ((((ulong)1 << i) & press) == 0)
                                return false;
                            else
                                foundNote = true;
                    }
                    else if ((((ulong)1 << i) & press) != 0)
                        return false;
                }
                return true;
            }
        }
    }
}

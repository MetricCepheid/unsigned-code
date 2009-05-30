using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SongDataIO;

namespace Unsigned
{
    /// <summary>
    /// Represents a "fill" (think drum fills) that
    /// can be used for some instruments to activate
    /// rock power
    /// </summary>
    public class GameFill
    {
        private float time, len;
        private float amount;
        private bool used, visible;

        public float GreenNotePos { get; private set; }
        public float Start { get { return time; } }
        public float Length { get { return len; } }
        public float End
        {
            get { return time + len; }
        }
        public float HideNotesEnd { get; set; }
        public float Width { get { return Math.Max(0, Math.Min(1, amount)); } }

        public bool Visible { get { return visible; } }
        public bool Used { get { return used; } }

        public GameFill(SongData.Fill fill)
        {
            this.time = fill.time/1000f;
            this.len = fill.len/1000f;
            GreenNotePos = End - 0.05f;
            used = false;
            visible = true;
            amount = 0f;
        }

        // called when rp is gained in the middle of one
        // which should be never, but there are idiot charters
        public void Hide()
        {
            visible = false;
        }

        public bool Use()
        {
            if (amount >= 1.0f)
            {
                used = true;
                return true;
            }
            return false;
        }

        internal void Hit()
        {
            amount += 1 / (4 * Length);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SongDataIO;

namespace Unsigned
{
    public class GameSolo
    {
        private float time, len;
        private bool visible, used;

        public float Start { get { return time; } }
        public float Length { get { return len; } }
        public float End
        {
            get { return time + len; }
        }

        public int NumNotes { get; set; }
        public int HitNotes { get; set; }

        /// <summary>
        /// Returns the percentage of notes hit (1-100)
        /// </summary>
        public int Percentage
        {
            get
            {
                if (NumNotes <= 0)//protect agains divide by 0 errors
                    return 0;
                if (HitNotes >= NumNotes)
                    return 100;
                int p = (int)((HitNotes / (float)NumNotes) * 100f);
                if (p > 99)
                    p = 99;
                return p;
            }
        }


        public bool Visible { get { return visible; } }
        public bool Used { get { return used; } }

        public GameSolo(SongData.Solo solo)
        {
            this.time = solo.time / 1000f;
            this.len = solo.len / 1000f;
            visible = true;
            used = false;
            NumNotes = 0;
            HitNotes = 0;
        }

        // called when user fails in solo
        public void Hide()
        {
            visible = false;
        }

        public void Use()
        {
            used = true;
        }
    }
}

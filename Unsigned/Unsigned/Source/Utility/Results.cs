using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SongDataIO;

namespace Unsigned
{
    public struct Results
    {
        public int hitNotes, missedNotes;
        public int totalNotes, totalSPPH;//temp
        public int hitSPPH, missedSPPH;
        public int streak;
        public float percentSong;
        public Instrument instr;
    }
}

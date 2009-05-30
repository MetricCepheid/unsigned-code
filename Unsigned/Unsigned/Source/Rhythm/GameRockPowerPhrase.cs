using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SongDataIO;

namespace Unsigned
{
    /// <summary>
    /// Represents a section of time in which
    /// all encompassed notes are white and, if
    /// hit, will give the user 25% rock power
    /// </summary>
    public class GameRockPowerPhrase
    {
        private float time, len;
        public bool Okay { get; private set; }//defaulted to true, falsed when note missed
        public bool Used { get; private set; }

        public float Start { get { return time; } }
        public float Length { get { return len; } }
        public float End
        {
            get { return time + len; }
        }

        public GameRockPowerPhrase(SongData.RockPowerPhrase rpp)
        {
            this.time = rpp.time/1000f;
            this.len = rpp.len/1000f;
            Okay = true;
            Used = false;
        }

        public void Fail()
        {
            Okay = false;
        }

        public void Use()
        {
            Used = true;
        }
    }
}

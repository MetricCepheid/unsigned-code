using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unsigned
{
    public struct SongTime
    {
        private TimeSpan _egt, _tst;
        public TimeSpan ElapsedGameTime { get { return _egt; } }
        public TimeSpan TotalSongTime { get { return _tst; } }

        public SongTime(TimeSpan elapsed, TimeSpan songTime)
        {
            _egt = elapsed;
            _tst = songTime;
        }
    }
}

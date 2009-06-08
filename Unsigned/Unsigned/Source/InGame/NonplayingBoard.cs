using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SongDataIO;

namespace Unsigned
{
    /// <summary>
    /// For nonplaying animations
    /// </summary>
    public class NonplayingBoard
    {
        private SongData songData;
        public Instrument instrumentType;
        public Rocker rocker;

        private int currentNoteIndex;

        public SongData.NoteSet Note
        {
            get
            {
                for(int i=0;i<songData.instruments.Length;i++)
                    if (songData.instruments[i].instrumentType == instrumentType.CodeName)
                    {
                        int index = 0;
                        for (int k = 0; k < songData.instruments[i].diffSets[instrumentType.DiffSame?0:3].phrases.Length; k++)
                        {
                            for (int j = 0; j < songData.instruments[i].diffSets[instrumentType.DiffSame ? 0 : 3].phrases[k].notes.Length; j++)
                            {
                                if (index == currentNoteIndex)
                                {
                                    return songData.instruments[i].diffSets[instrumentType.DiffSame ? 0 : 3].phrases[k].notes[j];
                                }
                                index++;
                            }
                        }
                    }
                return null;
            }
        }

        public NonplayingBoard(SongData sng, Instrument instr)
        {
            songData = sng;
            instrumentType = instr;
            currentNoteIndex = 0;
        }

        public void Update(SongTime songTime)
        {
            SongData.NoteSet currentNote = Note;
            if (currentNote != null)
            {
                if (instrumentType.NeedsStrum)
                {
                    int highNote = -1;
                    for (int i = 0; i < instrumentType.NumTracks; i++)
                        if ((currentNote.type & ((ulong)1 << i)) != 0)
                            highNote = i;
                    rocker.Animation.RunAnimation(AnimationWrapper.ARIType.RunToPoint, "MoveHand", 10f, highNote / (float)(instrumentType.NumTracks - 1));
                }
                if (songTime.TotalSongTime.TotalMilliseconds >= currentNote.time)
                {
                    if (instrumentType.NeedsStrum)
                        rocker.Animation.RunAnimation(AnimationWrapper.ARIType.RunOnce, "StrumGuitar", 4.0f);
                    else
                    {
                        for (int i = 0; i < instrumentType.NumTracks; i++)
                        {
                            if ((currentNote.type & ((ulong)1 << i)) != 0)
                            {
                                if(i>=instrumentType.NumDrawnTracks)
                                    rocker.Animation.RunAnimation(AnimationWrapper.ARIType.RunOnce, "BassHit", 5f);
                                else if(i<instrumentType.NumDrawnTracks/2)
                                    rocker.Animation.RunAnimation(AnimationWrapper.ARIType.RunOnce, "HitLeft", 5f);
                                else
                                    rocker.Animation.RunAnimation(AnimationWrapper.ARIType.RunOnce, "HitRight", 5f);
                            }
                        }
                    }
                    currentNoteIndex++;
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using SongDataIO;

namespace VocalEditor
{
    public class VoxSong
    {
        public struct Harmony
        {
            public uint start, end;
            public byte instruments;
        }
        public class VocalWord : IComparable
        {
            public uint time;
            public uint len;
            public short startNote, endNote;
            public String value;
            public bool connected;
            public uint end
            {
                get { return time + len; }
            }

            public int CompareTo(Object other)
            {
                return time.CompareTo(((VocalWord)other).time);
            }
        }
        public class VocalPhrase : IComparable
        {
            public static byte REGULAR = 0, BLANK = 1, RHYTHM = 2;
            public uint time;
            public byte typ;
            public bool overdrive;
            public byte rhythmType;

            public int CompareTo(Object other)
            {
                return time.CompareTo(((VocalPhrase)other).time);
            }
            public VocalPhrase()
            {
            }
        }

        public bool valid;
        public SongData.Barline[] bars;
        public List<VocalPhrase> notes;
        public List<VocalWord> words;

        private VoxSong()
        {
            notes = new List<VocalPhrase>();
            words = new List<VocalWord>();
            bars = new SongData.Barline[0];
        }

        public static VoxSong FromSongData(SongData songdata)
        {
            VoxSong song = new VoxSong();
            song.bars = (SongData.Barline[])songdata.info.barlines.Clone();
            for (int instr = 0; instr < songdata.instruments.Length; instr++)
            {
                if (songdata.instruments[instr].instrumentType.Equals("LVX"))
                {
                    for (int diff = 0; diff < songdata.instruments[instr].diffSets.Length; diff++)
                    {
                        if (songdata.instruments[instr].diffSets[diff]!=null)
                        {
                            for (int i = 0; i < songdata.instruments[instr].diffSets[diff].phrases.Length; i++)
                            {
                                VocalPhrase phrase = new VocalPhrase();
                                phrase.time = songdata.instruments[instr].diffSets[diff].phrases[i].time;
                                phrase.typ = (byte)songdata.instruments[instr].diffSets[diff].phrases[i].type;
                                phrase.rhythmType = (byte)songdata.instruments[instr].diffSets[diff].phrases[i].rType;
                                phrase.overdrive = songdata.instruments[instr].diffSets[diff].phrases[i].rockpower;
                                song.notes.Add(phrase);

                                for (int k = 0; k < songdata.instruments[instr].diffSets[diff].phrases[i].notes.Length; k++)
                                {
                                    VocalWord word = new VocalWord();
                                    word.time = songdata.instruments[instr].diffSets[diff].phrases[i].notes[k].time;
                                    word.len = songdata.instruments[instr].diffSets[diff].phrases[i].notes[k].length;
                                    word.startNote = (short)songdata.instruments[instr].diffSets[diff].phrases[i].notes[k].type;
                                    word.endNote = (short)songdata.instruments[instr].diffSets[diff].phrases[i].notes[k].endtype;
                                    word.value = songdata.instruments[instr].diffSets[diff].phrases[i].notes[k].text;
                                    word.connected = false;
                                    if (k < songdata.instruments[instr].diffSets[diff].phrases[i].notes.Length - 1)
                                        word.connected = songdata.instruments[instr].diffSets[diff].phrases[i].notes[k].end == songdata.instruments[instr].diffSets[diff].phrases[i].notes[k + 1].time &&
                                                         songdata.instruments[instr].diffSets[diff].phrases[i].notes[k].endtype == songdata.instruments[instr].diffSets[diff].phrases[i].notes[k + 1].type;
                                    song.words.Add(word);
                                }
                            }
                        }
                    }
                }
            }
            song.valid = true;
            return song;
        }

        public bool SaveToSongData(SongData songdata)
        {
            //first, make sure data is valid:
            // 1:sort
            // 2:check for overlaps
            words.Sort();
            notes.Sort();

            //check for one note overlapping another
            for (int i = 0; i < words.Count-1; i++)
            {
                if (words[i].end >= words[i + 1].time)
                {
                    System.Windows.Forms.MessageBox.Show("Invalid Data!\nNote at " + (words[i].time / 60000) + ":" + ((words[i].time / 1000) % 60) + "." + (words[i].time % 1000) + " (\'" + words[i].value + "\') overlaps the next note\nPlease fix before proceeding.");
                    return false;
                }
            }

            // check for notes going outside phrases
            for (int i = 0; i < notes.Count - 1; i++)
            {
                for (int k = 0; k < words.Count; k++)
                {
                    if (words[k].time >= notes[i].time && words[k].time < notes[i + 1].time)
                    {
                        if (words[k].end > notes[i + 1].time)
                        {
                            System.Windows.Forms.MessageBox.Show("Invalid Data!\nNote at " + (words[i].time / 60000) + ":" + ((words[i].time / 1000) % 60) + "." + (words[i].time % 1000) + " (\'" + words[i].value + "\') extends beyond the phrase!\nPlease fix before proceeding.");
                            return false;
                        }
                    }
                }
            }

            SongData.Phrase[] phrases = new SongData.Phrase[notes.Count];

            for (int i = 0; i < notes.Count-1; i++)
            {
                phrases[i] = new SongData.Phrase();
                phrases[i].rockpower = notes[i].overdrive;
                phrases[i].type = (SongData.TYPE)notes[i].typ;
                phrases[i].rType = (SongData.RTYPE)notes[i].rhythmType;
                phrases[i].time = notes[i].time;

                List<SongData.NoteSet> pnotes = new List<SongData.NoteSet>();
                for (int k = 0; k < words.Count; k++)
                {
                    if (words[k].time >= notes[i].time && words[k].time < notes[i + 1].time)
                    {
                        SongData.NoteSet toAdd = new SongData.NoteSet();
                        toAdd.time = words[k].time;
                        toAdd.length = words[k].len;
                        toAdd.type = (ulong)words[k].startNote;
                        toAdd.endtype = (ulong)words[k].endNote;
                        toAdd.text = words[k].value;
                        pnotes.Add(toAdd);
                    }
                }
                pnotes.Sort();

                phrases[i].notes = pnotes.ToArray();
            }

            for (int instr = 0; instr < songdata.instruments.Length; instr++)
            {
                if (songdata.instruments[instr].instrumentType.Equals("LVX"))
                {
                    for (int diff = 0; diff < songdata.instruments[instr].diffSets.Length; diff++)
                    {
                        if (songdata.instruments[instr].diffSets[diff] != null)
                        {
                            songdata.instruments[instr].diffSets[diff].phrases = phrases;
                        }
                    }
                }
            }

            return true;
        }
    }
}

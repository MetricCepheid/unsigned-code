using System;
using System.Collections.Generic;
using System.Text;

namespace Unsigned
{
    class Instrument
    {
        //Rock Power Enable Types
        public enum RockPowerEnableTypes
        {
            SELECT = 1, // select button or guitar tilt
            FILL = 2, // same as HMX Rock Band drum fills

            // mix types.  users can use either type
            SELECT_FILL = 3,
        };

        public enum BoardDimensions
        {
            TWO_DIMENSIONAL = 2,
            THREE_DIMENSIONAL = 3,
        };

        public enum PhraseType
        {
            NONE=0,

            BLANK=1,
            REGULAR=2,
            RHYTHM=4,

            BLANK_REGULAR=3,
            REGULAR_RHYTHM=6,
            BLANK_RHYTHM=5,

            BLANK_REGULAR_RHYTHM=7,
        };

        // number of chord/note tracks (in RB, 5 for G/B and 4 for drums)
        public int NumTracks;

        // how this instrument activates star power
        public RockPowerEnableTypes RPEnableType;

        // whether the board is 2d or 3d
        public BoardDimensions Dimensions;

        // Code name is 3-length string
        // full name is the name of the instrument in plain english
        public String CodeName, FullName;

        // whether notes have a length property
        public bool ContainsHeldNotes;

        // whether held notes can be pitch modulated by a whammy bar
        // irrelevant if !ContainsHeldNotes
        public bool CanWhammy;

        // whether <8th notes can be hit based on GH rules
        public bool CanHOPO;

        // whether the instrument supports solos
        public bool HasSolos;

        // whether a note can end on a different pitch than it started on
        public bool PitchShifts;

        // whether or not the note contains text
        public bool ContainsText;

        // what kinds of phrases are available
        public PhraseType TypesOfPhrases;

        void SetValue(String variable, String value)
        {
            if (variable.ToLower().Trim().Equals("numtracks"))
                NumTracks = Int32.Parse(value);
            else if (variable.ToLower().Trim().Equals("rpenabletype"))
                RPEnableType = (RockPowerEnableTypes)Enum.Parse(Type.GetType("RockPowerEnableTypes"), value.Trim().ToUpper());
            else if (variable.ToLower().Trim().Equals("dimensions"))
                Dimensions = (BoardDimensions)Enum.Parse(Type.GetType("BoardDimensions"), value.Trim().ToUpper());
            else if (variable.ToLower().Trim().Equals("codename"))
                CodeName = value;
            else if (variable.ToLower().Trim().Equals("fullname"))
                FullName = value;
            else if (variable.ToLower().Trim().Equals("containsheldnotes"))
                ContainsHeldNotes = Boolean.Parse(value);
            else if (variable.ToLower().Trim().Equals("canwhammy"))
                CanWhammy = Boolean.Parse(value);
            else if (variable.ToLower().Trim().Equals("canhopo"))
                CanHOPO = Boolean.Parse(value);
            else if (variable.ToLower().Trim().Equals("hassolos"))
                HasSolos = Boolean.Parse(value);
            else if (variable.ToLower().Trim().Equals("pitchshifts"))
                PitchShifts = Boolean.Parse(value);
            else if (variable.ToLower().Trim().Equals("containstext"))
                ContainsText = Boolean.Parse(value);
            else if (variable.ToLower().Trim().Equals("typesofphrase"))
                TypesOfPhrases = (BoardDimensions)Enum.Parse(Type.GetType("PhraseType"), value.Trim().ToUpper());
            else
                System.Windows.Forms.MessageBox.Show("Invalid Instrument Variable Name: " + variable);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace UnsignedPeripheralPlugins
{
    /// <summary>
    /// Handles a peripheral button
    /// Never use hard-coded casted ints,
    /// as this enum is especially volatile
    /// </summary>
    public enum PeripheralButton
    {
        NONE = 0,
        FRET0,
        FRET1,
        FRET2,
        FRET3,
        FRET4,

        UP,
        DOWN,

        START,
        SELECT,

        CONFIRM,    //usually green
        BACK,       //usually red
        SWITCH,     //yellow or foot pedal (switches song list sorting, etc)

        TOTAL,
    };

    /// <summary>
    /// Handles a peripheral analog control
    /// Never use hard-coded casted ints,
    /// as this enum is especially volatile
    /// </summary>
    public enum PeripheralAnalog
    {
        WHAMMY_BAR=0,

        TOTAL,
    };

    /// <summary>
    /// Defines the type 
    /// </summary>
    public enum PeripheralType
    {
        UNKNOWN = 0,
        GAMEPAD,// regular gamepad (std controller)
        GUITAR,//RB Guitar + all backwards compatible guitars
        DRUMS4,//RB Drumset
        DRUMS3x2,// GH:WT Drumset
        DRUMS4x3,// RB(2) Drumset + RB2 Cymbals
        MICROPHONE_GAMEPAD,//Microphone with gamepad
        DIGITAL,//MIDI instrument - Mic pitch recognition with rhythm
    };
}

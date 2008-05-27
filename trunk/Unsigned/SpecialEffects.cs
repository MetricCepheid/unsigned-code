using System;
using System.Collections.Generic;
using System.Text;

namespace Unsigned
{
    public struct SEffect
    {
        public uint begin, end;
        public EFFECT_TYPE type;
        public int data;
        public enum EFFECT_TYPE 
        { 
            LIGHTING_NORMAL = 0, 
            LIGHTING_STROBE = 1, 
            LIGHTING_SLOWSTROBE = 2, 
            LIGHTING_BLACKOUT = 3, 
            LIGHTING_CHASE_G = 4, 
            LIGHTING_CHASE_B = 5, 
            LIGHTING_CHASE_D = 6, 
            LIGHTING_CHASE_V = 7, 
            LIGHTING_SWEEP = 8, 
            EFFECT_SMOKE = 9, 
            EFFECT_FLARE = 10 
        };
        public static String[] EF_TP_STR = 
        {
            "nr", "sb", "ss", "bo", "cg", "cb", "cd", "cv", "sw", "sk", "fl",
        };
    }
}

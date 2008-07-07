using System;
using System.Collections.Generic;
using System.Text;

namespace Unsigned
{
    public struct LightingEffect
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

    public struct SpecialEffectsSettings
    {
        private enum FRAME_EFFECT
        {
            CONSTANT = 0, //this is normal
            SLOW = 1,     //framerate 1/2
            VERYSLOW = 2, //framerate 1/4
            DEATHLY = 3,  //1 fps :O
        };
        private FRAME_EFFECT currentFE;

        private enum FRAME_EFFECT_STYLE
        {//for FRAME_EFFECT.CONSTANT, use BLINK
            BLINK = 0, //no fades
            CREST = 1, //fade, flash
            XFADE = 2, //Full fade
        }
        private FRAME_EFFECT_STYLE currentFES;

        const int DESATURATE = 1, //desaturate
                  HUE_SHIFT = 2,  //hue-shift
                  REDUCE = 4,     //reduce to 8-bit color
                  BLUR = 8,       //gaussian blur
                  GRAIN_DOT = 16, //dot grain
                  GRAIN_XHSH = 32;//crosshash grain

        private int postProcessEffects;//bitwise-or together ^
        //following are used ONLY IF ppe is enabled
        private float desaturate_value;//0-1 (0=B&W,1=full color)
        private float hue_shift;//0-1 (0&1=no difference)
        private float blur_strength;//strength of blur
        private float blur_passes;//number of passes to the blur
        private float grain_strength;//how strong the grain is (0=invisible,1=full-static)
    }
}

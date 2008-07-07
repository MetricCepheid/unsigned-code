using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Unsigned
{
    class RhythmMaster
    {
        private static RhythmMaster SINGLETON_RhythmMaster = null;
        
        byte lastStar; //for ching after star gain

        public static float[] rockMeterLevel;

        private bool[] instruments;//whether or not someone is playing this
        private String[] instrumentNames;//guitar, etc
        private String[] musicianNames;//guitarist, etc
        Instrument[] instrumentTypes;

        private int started = 0;

        private RhythmMaster()
        {
            instrumentTypes = new Instrument[4];
            instrumentTypes[0].CodeName = "LGT";
            instrumentTypes[0].FullName = "Lead Guitar";
            instrumentTypes[0].Dimensions = Instrument.BoardDimensions.THREE_DIMENSIONAL;
            instrumentTypes[0].NumTracks = 5;
            instrumentTypes[0].RPEnableType = Instrument.RockPowerEnableTypes.SELECT;
            instrumentTypes[0].ContainsHeldNotes = true;
            instrumentTypes[0].CanWhammy = true;
            instrumentTypes[0].CanHOPO = true;
            instrumentTypes[0].HasSolos = true;
            instrumentTypes[1].CodeName = "LVX";
            instrumentTypes[1].FullName = "Lead Vocals";
            instrumentTypes[1].Dimensions = Instrument.BoardDimensions.TWO_DIMENSIONAL;
            instrumentTypes[1].NumTracks = 12 * 3;// 3 octaves
            instrumentTypes[1].RPEnableType = Instrument.RockPowerEnableTypes.FILLS;
            instrumentTypes[1].ContainsHeldNotes = true;
            instrumentTypes[1].CanWhammy = false;
            instrumentTypes[1].CanHOPO = false;
            instrumentTypes[1].HasSolos = false;
            instrumentTypes[2].CodeName = "SET";
            instrumentTypes[2].FullName = "Drum Set";
            instrumentTypes[2].Dimensions = Instrument.BoardDimensions.THREE_DIMENSIONAL;
            instrumentTypes[2].NumTracks = 4;
            instrumentTypes[2].RPEnableType = Instrument.RockPowerEnableTypes.FILLS;
            instrumentTypes[2].ContainsHeldNotes = false;
            instrumentTypes[2].CanWhammy = false;
            instrumentTypes[2].CanHOPO = false;
            instrumentTypes[2].HasSolos = false;
            instrumentTypes[3].CodeName = "BAS";
            instrumentTypes[3].FullName = "Bass Guitar";
            instrumentTypes[3].Dimensions = Instrument.BoardDimensions.THREE_DIMENSIONAL;
            instrumentTypes[3].NumTracks = 5;
            instrumentTypes[3].RPEnableType = Instrument.RockPowerEnableTypes.SELECT;
            instrumentTypes[3].ContainsHeldNotes = true;
            instrumentTypes[3].CanWhammy = true;
            instrumentTypes[3].CanHOPO = true;
            instrumentTypes[3].HasSolos = false;
        }

        public static void CreateSingleton()
        {
            if (SINGLETON_RhythmMaster == null)
                SINGLETON_RhythmMaster = new RhythmMaster();
            else
                throw new InvalidOperationException("Singleton has already been initialized");
        }

        public static void DestroySingleton()
        {
            if (SINGLETON_RhythmMaster != null)
                SINGLETON_RhythmMaster = null;
            else
                throw new InvalidOperationException("Singleton has already been destroyed");
        }

        public static RhythmMaster GetSingleton()
        {
            return SINGLETON_RhythmMaster;
        }
    }

    class GameRenderMaster
    {
        private static GameRenderMaster SINGLETON_GameRenderMaster = null;

        private Texture2D texGlow;

        private GameRenderMaster()
        {

        }

        public static void CreateSingleton()
        {
            if (SINGLETON_GameRenderMaster == null)
                SINGLETON_GameRenderMaster = new GameRenderMaster();
            else
                throw new InvalidOperationException("Singleton has already been initialized");
        }

        public static void DestroySingleton()
        {
            if (SINGLETON_GameRenderMaster != null)
                SINGLETON_GameRenderMaster = null;
            else
                throw new InvalidOperationException("Singleton has already been destroyed");
        }

        public static GameRenderMaster GetSingleton()
        {
            return SINGLETON_GameRenderMaster;
        }
    }
}

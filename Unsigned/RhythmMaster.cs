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
        
        private byte lastStar; //for ching after star gain

        public static float[] rockMeterLevel;

        private int[] selectedInstruments;
        private Instrument[] instrumentTypes;
        private Board[] boards;
        private bool[] isFailing;
        private float failTime;
        private byte[] numFails;

        private int started = 0;

        private RhythmMaster()
        {
            selectedInstruments = new int[4];
            selectedInstruments[0] = -1;
            selectedInstruments[1] = -1;
            selectedInstruments[2] = -1;
            selectedInstruments[3] = -1;
            isFailing = new bool[4];
            isFailing[0] = false;
            isFailing[1] = false;
            isFailing[2] = false;
            isFailing[3] = false;
            numFails = new byte[4];
            numFails[0] = 0;
            numFails[1] = 0;
            numFails[2] = 0;
            numFails[3] = 0;
            rockMeterLevel = new float[4];
            rockMeterLevel[0] = 80;
            rockMeterLevel[1] = 80;
            rockMeterLevel[2] = 80;
            rockMeterLevel[3] = 80;
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
            instrumentTypes[1].RPEnableType = Instrument.RockPowerEnableTypes.FILL;
            instrumentTypes[1].ContainsHeldNotes = true;
            instrumentTypes[1].CanWhammy = false;
            instrumentTypes[1].CanHOPO = false;
            instrumentTypes[1].HasSolos = false;
            instrumentTypes[2].CodeName = "SET";
            instrumentTypes[2].FullName = "Drum Set";
            instrumentTypes[2].Dimensions = Instrument.BoardDimensions.THREE_DIMENSIONAL;
            instrumentTypes[2].NumTracks = 4;
            instrumentTypes[2].RPEnableType = Instrument.RockPowerEnableTypes.FILL;
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

            lastStar = 0;
        }

        public Instrument GetInstrumentType(int index)
        {
            if (IsInstrumentAvailable(index))
                return instrumentTypes[index];
            else
                return null;
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

        /// <summary>
        /// Returns whether or not a player is using this instrument slot (0-3)
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool IsInstrumentAvailable(int index)
        {
            return selectedInstruments[index] >= 0;
        }

        public float GetRockstarAmount()
        {
            float total = 0, count = 0;
            for (int i = 0; i < 4; i++)
                if (selectedInstruments[i]>=0)
                {
                    int adddiff;
                    if (boards[i].GetDifficulty() == Global.D_EASY)
                        adddiff = 1;
                    else if (boards[i].GetDifficulty() == Global.D_MEDIUM)
                        adddiff = 2;
                    else if (boards[i].GetDifficulty() == Global.D_HARD)
                        adddiff = 3;
                    else
                        adddiff = 4;
                    total += boards[i].GetStars() * adddiff;
                    count += adddiff;
                }
            return total / count;
        }

        private void StarPowerAction(int i)
        {
            if (isFailing[i])
                return;
            if (boards[i].GetSPAmount() < 0.5)
                return;
            if (failTime > 0)
            {
                for (int k = 0; k < isFailing.Length; k++)
                    if (selectedInstruments[i]>=0 && isFailing[k])
                    {
                        isFailing[k] = false;
                        rockMeterLevel[k] = 80;
                        boards[i].EatHalfSP();
                        return;
                    }
            }
            boards[i].ActivateStarPower();
        }

        public float GetRockMeterFill()
        {
            int ct = 0;
            float add = 0;
            for (int i = 0; i < 4; i++)
                if (selectedInstruments[i]>=0)
                {
                    if (rockMeterLevel[i] > 100)
                        rockMeterLevel[i] = 100;
                    //if (TEST_SONG)
                    //    rockMeterLevel[i] = 99f;
                    ct++;
                    add += rockMeterLevel[i];
                }
            return (add / ct) / 100f;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnsignedPeripheralPlugins;

namespace Unsigned
{
    public class SessionInfo
    {
        public enum ConfirmState
        {
            EMPTY_SLOT = 0,
            CHOOSING_NAME,
            CHOOSING_INSTRUMENT,
            FULLY_CONFIRMED
        };
        // for controllersetupscreen
        public ConfirmState[] confirmStates;
        public Peripheral[] peripherals;
        public int[] instruments;
        public int[] characterIndices;
        public Difficulty[] difficulties;
        public bool[] diffConfirm;

        public String songFileName;

        public SessionInfo()
        {
            confirmStates = new ConfirmState[4];
            for (int i = 0; i < confirmStates.Length; i++)
                confirmStates[i] = ConfirmState.EMPTY_SLOT;
            peripherals = new Peripheral[4];
            instruments = new int[4];
            diffConfirm = new bool[4];
            characterIndices = new int[4];
            for (int i = 0; i < characterIndices.Length; i++)
                characterIndices[i] = -1;
            difficulties = new Difficulty[4];
            for (int i = 0; i < difficulties.Length; i++)
                difficulties[i] = Difficulty.Medium;
            songFileName = "";
        }
    }
}

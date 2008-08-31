using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Input;
using UnsignedPeripheralPlugins;

namespace Unsigned
{
    class KeyboardPeripheral : Peripheral
    {
        private static KeyboardPeripheral SINGLETON_KeyboardPeripheral = null;

        private Keys[] keymap;
        private bool[] bufferedMap;
        private KeyboardState previousState;

        private KeyboardPeripheral()
        {
            keymap = new Keys[(int)PeripheralButton.TOTAL];
            bufferedMap = new bool[(int)PeripheralButton.TOTAL];
        }

        public override void Query()
        {
            KeyboardState currentState = Keyboard.GetState();

            for(int i=1;i<(int)PeripheralButton.TOTAL;i++)
            {
                if (currentState.IsKeyDown(keymap[i]) && previousState.IsKeyUp(keymap[i]))
                    bufferedMap[i] = true;
                else
                    bufferedMap[i] = false;
            }

            previousState = currentState;
        }

        public override bool IsConnected()
        {
            return true;
        }

        public override bool WasPressed(PeripheralButton button)
        {
            return bufferedMap[(int)button];
        }

        public override Peripheral[] GetControllers()
        {
            KeyboardPeripheral[] ret = new KeyboardPeripheral[1];
            ret[0] = new KeyboardPeripheral();
            ret[0].keymap[PeripheralButton.BLUE] = Keys.F;
            ret[0].keymap[PeripheralButton.DOWN] = Keys.Down;
            ret[0].keymap[PeripheralButton.GREEN] = Keys.A;
            ret[0].keymap[PeripheralButton.ORANGE] = Keys.G;
            ret[0].keymap[PeripheralButton.RED] = Keys.S;
            ret[0].keymap[PeripheralButton.SELECT] = Keys.RightShift;
            ret[0].keymap[PeripheralButton.START] = Keys.Escape;
            ret[0].keymap[PeripheralButton.UP] = Keys.Up;
            ret[0].keymap[PeripheralButton.YELLOW] = Keys.D;
            return ret;
        }

        public static void CreateSingleton()
        {
            if (SINGLETON_KeyboardPeripheral == null)
                SINGLETON_KeyboardPeripheral = new KeyboardPeripheral();
            else
                throw new InvalidOperationException("KeyboardPeripheral has already been instantiated");
        }

        public static KeyboardPeripheral GetSingleton()
        {
            if (SINGLETON_KeyboardPeripheral == null)
                throw new InvalidOperationException("Attempt to access KeyboardPeripheral singleton before creation");
            return SINGLETON_KeyboardPeripheral;
        }

        public static void DestroySingleton()
        {
            if (SINGLETON_KeyboardPeripheral != null)
                SINGLETON_KeyboardPeripheral = null;
            else
                throw new InvalidOperationException("KeyboardPeripheral has already been destroyed");
        }
    }
}

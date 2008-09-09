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

        public KeyboardPeripheral()
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

        public override bool IsPressed(PeripheralButton peripheralButton)
        {
            return Keyboard.GetState().IsKeyDown(keymap[(int)peripheralButton]);
        }

        public override Peripheral[] GetControllers()
        {
            KeyboardPeripheral[] ret = new KeyboardPeripheral[1];
            ret[0] = new KeyboardPeripheral();
            ret[0].keymap[(int)PeripheralButton.BLUE] = Keys.F;
            ret[0].keymap[(int)PeripheralButton.DOWN] = Keys.Down;
            ret[0].keymap[(int)PeripheralButton.GREEN] = Keys.A;
            ret[0].keymap[(int)PeripheralButton.ORANGE] = Keys.G;
            ret[0].keymap[(int)PeripheralButton.RED] = Keys.S;
            ret[0].keymap[(int)PeripheralButton.SELECT] = Keys.RightShift;
            ret[0].keymap[(int)PeripheralButton.START] = Keys.Escape;
            ret[0].keymap[(int)PeripheralButton.UP] = Keys.Up;
            ret[0].keymap[(int)PeripheralButton.YELLOW] = Keys.D;
            ret[0].keymap[(int)PeripheralButton.CONFIRM] = Keys.Enter;
            ret[0].keymap[(int)PeripheralButton.BACK] = Keys.Back;
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

        public override ulong GetFrets()
        {
            ulong ret = 0;
            if (IsPressed(PeripheralButton.GREEN))
                ret |= (((ulong)1) << 0);
            if (IsPressed(PeripheralButton.RED))
                ret |= (((ulong)1) << 1);
            if (IsPressed(PeripheralButton.YELLOW))
                ret |= (((ulong)1) << 2);
            if (IsPressed(PeripheralButton.BLUE))
                ret |= (((ulong)1) << 3);
            if (IsPressed(PeripheralButton.ORANGE))
                ret |= (((ulong)1) << 4);
            return ret;
        }

        public override ulong GetBufferedFrets()
        {
            ulong ret = 0;
            if (WasPressed(PeripheralButton.GREEN))
                ret |= (((ulong)1) << 0);
            if (WasPressed(PeripheralButton.RED))
                ret |= (((ulong)1) << 1);
            if (WasPressed(PeripheralButton.YELLOW))
                ret |= (((ulong)1) << 2);
            if (WasPressed(PeripheralButton.BLUE))
                ret |= (((ulong)1) << 3);
            if (WasPressed(PeripheralButton.ORANGE))
                ret |= (((ulong)1) << 4);
            return ret;
        }
    }
}

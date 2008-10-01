using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using UnsignedPeripheralPlugins;

namespace Unsigned
{
    class Xbox360Peripheral : Peripheral
    {
        /// <summary>
        /// The XNA PlayerIndex of this controller 
        /// (which green light in the xbox360 ring is on)
        /// </summary>
        private PlayerIndex index;

        /// <summary>
        /// Stores the previous state for buffered input
        /// </summary>
        private GamePadState previousState;

        /// <summary>
        /// Stores the current state for non-buffered input
        /// And so it doesn't have to be queried as much
        /// </summary>
        private GamePadState currentState;

        /// <summary>
        /// Stored which buttons are buffered pressed
        /// </summary>
        private bool[] bufferedMap;

        public Xbox360Peripheral()
        {
            bufferedMap = new bool[(int)PeripheralButton.TOTAL];
        }

        public override bool WasPressed(PeripheralButton button)
        {
            return bufferedMap[(int)button];
        }

        public override bool IsPressed(PeripheralButton button)
        {
            switch (button)
            {
            case PeripheralButton.FRET0:
                {
                    if (currentState.Buttons.A == ButtonState.Pressed)
                        return true;
                    return false;
                }
            case PeripheralButton.FRET1:
                {
                    if (currentState.Buttons.B == ButtonState.Pressed)
                        return true;
                    return false;
                }
            case PeripheralButton.FRET2:
                {
                    if (currentState.Buttons.Y == ButtonState.Pressed)
                        return true;
                    return false;
                }
            case PeripheralButton.FRET3:
                {
                    if (currentState.Buttons.X == ButtonState.Pressed)
                        return true;
                    return false;
                }
            case PeripheralButton.FRET4:
                {
                    if (currentState.Buttons.LeftShoulder == ButtonState.Pressed)
                        return true;
                    return false;
                }
            case PeripheralButton.DOWN:
                {
                    if (currentState.DPad.Down == ButtonState.Pressed)
                        return true;
                    return false;
                }
            case PeripheralButton.UP:
                {
                    if (currentState.DPad.Up == ButtonState.Pressed)
                        return true;
                    return false;
                }
            case PeripheralButton.CONFIRM:
                {
                    if (currentState.Buttons.A == ButtonState.Pressed)
                        return true;
                    return false;
                }
            case PeripheralButton.BACK:
                {
                    if (currentState.Buttons.B == ButtonState.Pressed)
                        return true;
                    return false;
                }
            case PeripheralButton.SWITCH:
                {
                    if (currentState.Buttons.Y == ButtonState.Pressed)
                        return true;
                    return false;
                }
            }
            throw new ArgumentException("Invalid Button");
        }

        public override bool IsConnected()
        {
            return GamePad.GetCapabilities(index).IsConnected;
        }

        public override Peripheral[] GetControllers()
        {
            int len = 0;
            for (int i = 0; i < 4; i++)
                if (GamePad.GetCapabilities((PlayerIndex)i).IsConnected)
                    len++;
            Xbox360Peripheral[] ret = new Xbox360Peripheral[len];
            int k = 0;
            for (int i = 0; i < len; i++)
            {
                GamePadCapabilities c = GamePad.GetCapabilities((PlayerIndex)i);
                if (c.IsConnected)
                {
                    ret[k] = new Xbox360Peripheral();
                    ret[k].index = (PlayerIndex)i;
                    if (c.GamePadType == GamePadType.Guitar || c.GamePadType == (GamePadType)7)
                        ret[k].pType = PeripheralType.GUITAR;
                    else if (c.GamePadType == GamePadType.DrumKit)
                        ret[k].pType = PeripheralType.DRUMS4;
                    else if (c.GamePadType == GamePadType.GamePad)
                        ret[k].pType = PeripheralType.GAMEPAD;
                    else
                        ret[k].pType = PeripheralType.UNKNOWN;
                    k++;
                }
            }
            return ret;
        }

        public override void Query()
        {
            currentState = GamePad.GetState(index);

            if (currentState.Buttons.A == ButtonState.Pressed && previousState.Buttons.A == ButtonState.Released)
                bufferedMap[(int)PeripheralButton.CONFIRM] = true;
            else
                bufferedMap[(int)PeripheralButton.CONFIRM] = false;
            if (currentState.Buttons.A == ButtonState.Pressed && previousState.Buttons.A == ButtonState.Released)
                bufferedMap[(int)PeripheralButton.FRET0] = true;
            else
                bufferedMap[(int)PeripheralButton.FRET0] = false;
            if (currentState.Buttons.B == ButtonState.Pressed && previousState.Buttons.B == ButtonState.Released)
                bufferedMap[(int)PeripheralButton.FRET1] = true;
            else
                bufferedMap[(int)PeripheralButton.FRET1] = false;
            if (currentState.DPad.Up == ButtonState.Pressed && previousState.DPad.Up == ButtonState.Released)
                bufferedMap[(int)PeripheralButton.UP] = true;
            else
                bufferedMap[(int)PeripheralButton.UP] = false;
            if (currentState.DPad.Down == ButtonState.Pressed && previousState.DPad.Down == ButtonState.Released)
                bufferedMap[(int)PeripheralButton.DOWN] = true;
            else
                bufferedMap[(int)PeripheralButton.DOWN] = false;

            previousState = currentState;
        }

        public override ulong GetFrets()
        {
            ulong ret = 0;
            if (IsPressed(PeripheralButton.FRET0))
                ret |= (((ulong)1) << 0);
            if (IsPressed(PeripheralButton.FRET1))
                ret |= (((ulong)1) << 1);
            if (IsPressed(PeripheralButton.FRET2))
                ret |= (((ulong)1) << 2);
            if (IsPressed(PeripheralButton.FRET3))
                ret |= (((ulong)1) << 3);
            if (IsPressed(PeripheralButton.FRET4))
                ret |= (((ulong)1) << 4);
            return ret;
        }

        public override ulong GetBufferedFrets()
        {
            return 0;
        }

        public override float GetAnalogValue(PeripheralAnalog analogControl)
        {
            switch (analogControl)
            {
                case PeripheralAnalog.WHAMMY_BAR:
                    return (currentState.ThumbSticks.Right.X + 1) / 2.0f;
            }
            return 0.0f;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using UnsignedPeripheralPlugins;

namespace Unsigned
{
    /// <summary>
    /// Represents a plugged in Xbox360 Controller
    /// </summary>
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

        /// <summary>
        /// Creates a new Xbox360Peripheral
        /// Invalid until currentPlayerIndex is set
        /// </summary>
        public Xbox360Peripheral()
        {
            bufferedMap = new bool[(int)PeripheralButton.TOTAL];
        }

        /// <summary>
        /// Returns whether or not the button is pressed since the last query
        /// </summary>
        /// <param name="button">Which button to query</param>
        /// <returns>whether the button was pressed since the last frame</returns>
        public override bool WasPressed(PeripheralButton button)
        {
            return bufferedMap[(int)button];
        }

        /// <summary>
        /// Returns whether or not the button is currently pressed
        /// </summary>
        /// <param name="button">Which button to query</param>
        /// <returns>whether the button is currently pressed</returns>
        public override bool IsPressed(PeripheralButton button)
        {
            switch (button)
            {
            case PeripheralButton.FRET0:
                {
                    if (pType == PeripheralType.DRUMS4)
                    {
                        if(currentState.Buttons.B==ButtonState.Pressed)
                            return true;
                    }
                    else if(currentState.Buttons.A == ButtonState.Pressed)
                        return true;
                    return false;
                }
            case PeripheralButton.FRET1:
                {
                    if (pType == PeripheralType.DRUMS4)
                    {
                        if (currentState.Buttons.Y == ButtonState.Pressed)
                            return true;
                    }
                    else if (currentState.Buttons.B == ButtonState.Pressed)
                        return true;
                    return false;
                }
            case PeripheralButton.FRET2:
                {
                    if (pType == PeripheralType.DRUMS4)
                    {
                        if (currentState.Buttons.X == ButtonState.Pressed)
                            return true;
                    }
                    else if (currentState.Buttons.Y == ButtonState.Pressed)
                        return true;
                    return false;
                }
            case PeripheralButton.FRET3:
                {
                    if (pType == PeripheralType.DRUMS4)
                    {
                        if (currentState.Buttons.A == ButtonState.Pressed)
                            return true;
                    }
                    else if (currentState.Buttons.X == ButtonState.Pressed)
                        return true;
                    return false;
                }
            case PeripheralButton.FRET4:
                {
                    if (pType == PeripheralType.DRUMS4)
                    {
                        if (currentState.Buttons.LeftShoulder == ButtonState.Pressed)
                            return true;
                    }
                    else if (currentState.Buttons.LeftShoulder == ButtonState.Pressed)
                        return true;
                    return false;
                }
            case PeripheralButton.DOWN:
                {
                    if (pType == PeripheralType.DRUMS4)
                    {
                        if (currentState.Buttons.X == ButtonState.Pressed)
                            return true;
                    }
                    if (currentState.DPad.Down == ButtonState.Pressed)
                        return true;
                    return false;
                }
            case PeripheralButton.UP:
                {
                    if (pType == PeripheralType.DRUMS4)
                    {
                        if (currentState.Buttons.Y == ButtonState.Pressed)
                            return true;
                    }
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
            case PeripheralButton.SELECT:
                {
                    if (currentState.Buttons.Back == ButtonState.Pressed)
                        return true;
                    else if (currentState.ThumbSticks.Right.Y > 0.75f)
                        return true;
                    return false;
                }
            }
            throw new ArgumentException("Invalid Button");
        }

        /// <summary>
        /// Determines whether the hardware represented by this peripheral is connected
        /// </summary>
        /// <returns></returns>
        public override bool IsConnected()
        {
            return GamePad.GetCapabilities(index).IsConnected;
        }

        /// <summary>
        /// Returns an array of available connected peripherals
        /// Must be treated as if static
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Allows the Peripheral to query the hardware state and update buffered input
        /// Called every frame
        /// </summary>
        public override void Query()
        {
            currentState = GamePad.GetState(index);
            if (currentState.Buttons.A == ButtonState.Pressed && previousState.Buttons.A == ButtonState.Released)
                bufferedMap[(int)PeripheralButton.CONFIRM] = true;
            else
                bufferedMap[(int)PeripheralButton.CONFIRM] = false;
            if (currentState.Buttons.B == ButtonState.Pressed && previousState.Buttons.B == ButtonState.Released)
                bufferedMap[(int)PeripheralButton.BACK] = true;
            else
                bufferedMap[(int)PeripheralButton.BACK] = false;
            if (pType == PeripheralType.DRUMS4)
            {
                if (currentState.Buttons.B == ButtonState.Pressed && previousState.Buttons.B == ButtonState.Released)
                    bufferedMap[(int)PeripheralButton.FRET0] = true;
                else
                    bufferedMap[(int)PeripheralButton.FRET0] = false;
            }
            else if (currentState.Buttons.A == ButtonState.Pressed && previousState.Buttons.A == ButtonState.Released)
                bufferedMap[(int)PeripheralButton.FRET0] = true;
            else
                bufferedMap[(int)PeripheralButton.FRET0] = false;
            if (pType == PeripheralType.DRUMS4)
            {
                if (currentState.Buttons.Y == ButtonState.Pressed && previousState.Buttons.Y == ButtonState.Released)
                    bufferedMap[(int)PeripheralButton.FRET1] = true;
                else
                    bufferedMap[(int)PeripheralButton.FRET1] = false;
            }
            else if (currentState.Buttons.B == ButtonState.Pressed && previousState.Buttons.B == ButtonState.Released)
                bufferedMap[(int)PeripheralButton.FRET1] = true;
            else
                bufferedMap[(int)PeripheralButton.FRET1] = false;
            if (pType == PeripheralType.DRUMS4)
            {
                if (currentState.Buttons.X == ButtonState.Pressed && previousState.Buttons.X == ButtonState.Released)
                    bufferedMap[(int)PeripheralButton.FRET2] = true;
                else
                    bufferedMap[(int)PeripheralButton.FRET2] = false;
            }
            else if (currentState.Buttons.Y == ButtonState.Pressed && previousState.Buttons.Y == ButtonState.Released)
                bufferedMap[(int)PeripheralButton.FRET2] = true;
            else
                bufferedMap[(int)PeripheralButton.FRET2] = false;
            if (pType == PeripheralType.DRUMS4)
            {
                if (currentState.Buttons.A == ButtonState.Pressed && previousState.Buttons.A == ButtonState.Released)
                    bufferedMap[(int)PeripheralButton.FRET3] = true;
                else
                    bufferedMap[(int)PeripheralButton.FRET3] = false;
            }
            else if (currentState.Buttons.X == ButtonState.Pressed && previousState.Buttons.X == ButtonState.Released)
                bufferedMap[(int)PeripheralButton.FRET3] = true;
            else
                bufferedMap[(int)PeripheralButton.FRET3] = false;
            if (currentState.Buttons.LeftShoulder == ButtonState.Pressed && previousState.Buttons.LeftShoulder == ButtonState.Released)
                bufferedMap[(int)PeripheralButton.FRET4] = true;
            else
                bufferedMap[(int)PeripheralButton.FRET4] = false;
            if (pType == PeripheralType.DRUMS4 && currentState.Buttons.Y == ButtonState.Pressed && previousState.Buttons.Y == ButtonState.Released)
                bufferedMap[(int)PeripheralButton.UP] = true;
            else if (currentState.DPad.Up == ButtonState.Pressed && previousState.DPad.Up == ButtonState.Released)
                bufferedMap[(int)PeripheralButton.UP] = true;
            else
                bufferedMap[(int)PeripheralButton.UP] = false;
            if (pType == PeripheralType.DRUMS4 && currentState.Buttons.X == ButtonState.Pressed && previousState.Buttons.X == ButtonState.Released)
                bufferedMap[(int)PeripheralButton.DOWN] = true;
            else if (currentState.DPad.Down == ButtonState.Pressed && previousState.DPad.Down == ButtonState.Released)
                bufferedMap[(int)PeripheralButton.DOWN] = true;
            else
                bufferedMap[(int)PeripheralButton.DOWN] = false;
            if (currentState.Buttons.Back == ButtonState.Pressed && previousState.Buttons.Back == ButtonState.Released)
                bufferedMap[(int)PeripheralButton.SELECT] = true;
            else
                bufferedMap[(int)PeripheralButton.SELECT] = false;
            if (currentState.Buttons.Start == ButtonState.Pressed && previousState.Buttons.Start == ButtonState.Released)
                bufferedMap[(int)PeripheralButton.START] = true;
            else
                bufferedMap[(int)PeripheralButton.START] = false;
            if (pType == PeripheralType.DRUMS4)
            {
                if (currentState.Buttons.LeftShoulder == ButtonState.Pressed && previousState.Buttons.LeftShoulder == ButtonState.Released)
                    bufferedMap[(int)PeripheralButton.SWITCH] = true;
                else
                    bufferedMap[(int)PeripheralButton.SWITCH] = false;
            }
            else if (currentState.Buttons.Y == ButtonState.Pressed && previousState.Buttons.Y == ButtonState.Released)
                bufferedMap[(int)PeripheralButton.SWITCH] = true;
            else
                bufferedMap[(int)PeripheralButton.SWITCH] = false;

            previousState = currentState;
        }

        /// <summary>
        /// Returns a bitwise or-ed value indicating which frets are currently down
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Returns a bitwise or-ed value indicating which frets were pressed last frame
        /// </summary>
        /// <returns></returns>
        public override ulong GetBufferedFrets()
        {
            ulong ret = 0;
            if (WasPressed(PeripheralButton.FRET0))
                ret |= (((ulong)1) << 0);
            if (WasPressed(PeripheralButton.FRET1))
                ret |= (((ulong)1) << 1);
            if (WasPressed(PeripheralButton.FRET2))
                ret |= (((ulong)1) << 2);
            if (WasPressed(PeripheralButton.FRET3))
                ret |= (((ulong)1) << 3);
            if (WasPressed(PeripheralButton.FRET4))
                ret |= (((ulong)1) << 4);
            return ret;
        }

        /// <summary>
        /// Gets an analog value from the controller (0-1)
        /// </summary>
        /// <param name="analogControl">Which control to query</param>
        /// <returns>The analog value, clamped from 0-1</returns>
        public override float GetAnalogValue(PeripheralAnalog analogControl)
        {
            switch (analogControl)
            {
                case PeripheralAnalog.WHAMMY_BAR:
                    return (currentState.ThumbSticks.Right.X + 1) / 2.0f;
            }
            return 0.0f;
        }

        //I made these class variables so I didn't allocate them every time
        private String[] gArr = { "LGT", "RGT", "BAS" };
        private String[] dArr = { "SET" };
        private String[] vArr = { "LVX", "BVX", "BVA", "BVB" };
        /// <summary>
        /// Returns an array of 3-letter instrument code-names supported
        /// by this specific peripheral
        /// </summary>
        /// <returns>An array of 3-letter strings</returns>
        public override string[] GetSupportedInstruments()
        {
            GamePadCapabilities gpc = GamePad.GetCapabilities(index);
            if (pType == PeripheralType.GUITAR)
                return gArr;
            else if (pType == PeripheralType.DRUMS4)
                return dArr;
            else if (pType == PeripheralType.MICROPHONE_GAMEPAD)
                return vArr;
            else
                return new String[0];
        }
    }
}

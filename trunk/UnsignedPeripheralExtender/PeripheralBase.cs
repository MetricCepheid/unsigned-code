using System;
using System.Collections.Generic;
using System.Text;

namespace UnsignedPeripheralPlugins
{
    public abstract class Peripheral
    {
        protected static List<PeripheralListener> listeners;
        protected PeripheralType pType;
        protected String mode = "MNU";

        public static void AddListener(PeripheralListener listener)
        {
            listeners.Add(listener);
        }

        public void SetMode(String newMode)
        {
            if (newMode.Length == 3)
                mode = newMode.ToUpper();
        }

        // *** MENU FUNCTIONS ***

        /// <summary>
        /// Returns whether or not the button is pressed since the last query
        /// </summary>
        /// <param name="button">Which button to query</param>
        /// <returns>whether the button was pressed since the last frame</returns>
        public abstract bool WasPressed(PeripheralButton button);

        /// <summary>
        /// Returns whether or not the button is currently pressed
        /// </summary>
        /// <param name="button">Which button to query</param>
        /// <returns>whether the button is currently pressed</returns>
        public abstract bool IsPressed(PeripheralButton button);

        // *** HARDWARE FUNCTIONS ***

        /// <summary>
        /// Determines whether the hardware represented by this peripheral is connected
        /// </summary>
        /// <returns></returns>
        public abstract bool IsConnected();

        /// <summary>
        /// Allows the Peripheral to query the hardware state and update buffered input
        /// Called every frame
        /// </summary>
        public abstract void Query();

        /// <summary>
        /// Returns an array of available connected peripherals
        /// Must be treated as if static
        /// </summary>
        /// <returns></returns>
        public abstract Peripheral[] GetControllers();

        /// <summary>
        /// Returns a bitwise or-ed value indicating which frets are currently down
        /// </summary>
        /// <returns></returns>
        public abstract ulong GetFrets();

        /// <summary>
        /// Returns a bitwise or-ed value indicating which frets were pressed last frame
        /// </summary>
        /// <returns></returns>
        public abstract ulong GetBufferedFrets();

        /// <summary>
        /// Gets an analog value from the controller (0-1)
        /// </summary>
        /// <param name="analogControl">Which control to query</param>
        /// <returns>The analog value, clamped from 0-1</returns>
        public abstract float GetAnalogValue(PeripheralAnalog analogControl);

        /// <summary>
        /// Simple parsing function for the scripting
        /// </summary>
        /// <param name="buttonStr">A string corresponding to a PeripheralButton value</param>
        /// <returns></returns>
        public static PeripheralButton GetButtonFromString(String buttonStr)
        {
            for (PeripheralButton i = (PeripheralButton)0; i < PeripheralButton.TOTAL; i++)
                if (i.ToString().ToLower().Equals(buttonStr.ToLower()))
                    return i;
            return PeripheralButton.NONE;
        }
    }
}

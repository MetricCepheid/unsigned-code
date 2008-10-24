using System;
using System.Collections.Generic;
using System.Text;

namespace UnsignedPeripheralPlugins
{
    /// <summary>
    /// Represents a single piece of plugged in input-hardware
    /// </summary>
    public abstract class Peripheral
    {
        /// <summary>
        /// We may at some time have event-based input
        /// </summary>
        protected static List<PeripheralListener> listeners;
        /// <summary>
        /// The type of peripheral (guitar, drums, etc)
        /// </summary>
        protected PeripheralType pType;
        /// <summary>
        /// This will be set by the game.
        /// Valid values are "MNU" for menu
        /// and three letter instrument code names
        /// </summary>
        protected String mode = "MNU";

        // false is righty, true is lefty
        // should not be modified. Only stored
        // in this class so the option holds
        // between songs
        public bool LeftySwitch = false;

        /// <summary>
        /// Adds a listener for all peripherals
        /// to report to.
        /// </summary>
        /// <param name="listener">The listener to be added</param>
        public static void AddListener(PeripheralListener listener)
        {
            listeners.Add(listener);
        }

        /// <summary>
        /// Sets the mode of this instrument.
        /// Usually sets to either "MNU" for menu
        /// or a three-letter instrument code name
        /// </summary>
        /// <param name="newMode">The new mode</param>
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
        /// Returns an array of 3-letter instrument code-names supported
        /// by this specific peripheral
        /// </summary>
        /// <returns>An array of 3-letter strings</returns>
        public abstract String[] GetSupportedInstruments();

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

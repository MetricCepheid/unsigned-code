using System;
using System.Collections.Generic;
using System.Text;

namespace UnsignedPeripheralPlugins
{
    /// <summary>
    /// Any class implementing the PeripheralListener will handle 
    /// all input from any type of peripheral
    /// </summary>
    public interface PeripheralListener
    {
        /// <summary>
        /// Called when a button is pressed (buffered)
        /// </summary>
        /// <param name="button">The button pressed</param>
        /// <param name="flags">Various Bit Flags (lower frets, cymbals, etc)</param>
        void ButtonPressed(PeripheralButton button, byte flags);

        /// <summary>
        /// Called when a button is released (buffered)
        /// </summary>
        /// <param name="button">The button pressed</param>
        /// <param name="flags">Various Bit Flags (lower frets, cymbals, etc)</param>
        void ButtonReleased(PeripheralButton button, byte flags);
    }
}

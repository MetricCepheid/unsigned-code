using System;
using System.Collections.Generic;
using System.Text;

namespace Unsigned
{
    /// <summary>
    /// Handles math that C# just can't (or more likely won't) do on its own.
    /// Or maybe I am just an "Extract Method" whore
    /// </summary>
    static class FVMath
    {
        /// <summary>
        /// Linearly Interpolates between two values
        /// </summary>
        /// <param name="val1">The first value</param>
        /// <param name="val2">The second value</param>
        /// <param name="weight">The weight of val2 (0=full val1, 1=full val2)</param>
        /// <returns>The interpolated value</returns>
        public static float Lerp(float val1, float val2, float weight)
        {
            if (weight < 0 || weight > 1)
                throw new ArgumentException("Weight value was out of (0-1) range", "weight");
            return (val1 * (1 - weight)) + (val2 * weight);
        }
    }
}

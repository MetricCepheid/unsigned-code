using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;

namespace Unsigned
{
    abstract class InputPerphBase
    {
        /// <summary>
        ///  Returns whether or not the "confirm" button is pressed (usually Green/A)
        /// </summary>
        public abstract bool IsConfirmPressed();

        /// <summary>
        ///  Returns whether or not the "back" button is pressed (usually Red/B)
        /// </summary>
        public abstract bool IsBackPressed();

        /// <summary>
        ///  Returns whether or not the "Up" button is pressed
        /// </summary>
        public abstract bool IsUpPressed();

        /// <summary>
        ///  Returns whether or not the "down" button is pressed
        /// </summary>
        public abstract bool IsDownPressed();
    }
}